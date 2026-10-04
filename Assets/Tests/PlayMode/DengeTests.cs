using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Denge ölçümü (uzun): usta ve acemi bot birkaç tam oyun oynar, 2x hızda.
/// Sadece istenince çalışır: python3 tools/kak_bridge.py denge
/// Hedef (Faz 15 K1, tempo): güç 0 acemi 60-120 sn, usta 150-300 sn; tam güç (g=1) usta 240-420 sn.
/// Seçenekler: denge dungeon g=1 (güç) · denge dungeon eski (kademe sistemi, kıyas) · denge dungeon cirak (ilk oyun)
/// </summary>
public class DengeTests
{
    const float Speed = 2f;
    const float CapSeconds = 420f; // oyun süresi üst sınırı (G2: İmkansız 6. dakikada)

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        if (!File.Exists(".claude-bridge/run_denge")) Assert.Ignore("Denge ölçümü istenmedi (python3 tools/kak_bridge.py denge)");
        LogAssert.ignoreFailingMessages = true;
        KakTestUtil.ResetWorld();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        KakTime.SetTestSpeed(1f);
        KakTestUtil.ResetWorld();
        yield return null;
    }

    IEnumerator PlayOne(float skill, System.Collections.Generic.List<string> results, bool useDash = true)
    {
        var hits = new System.Collections.Generic.Dictionary<string, int>();
        System.Action<int, Vector3> onHit = (hp, pos) =>
        {
            string k = string.IsNullOrEmpty(PlayerHealth.LastHitSource) ? "?" : PlayerHealth.LastHitSource;
            hits[k] = hits.TryGetValue(k, out int n) ? n + 1 : 1;
            PlayerHealth.LastHitSource = "";
        };
        GameEvents.PlayerDamaged += onHit;
        KakTestUtil.ResetWorld();
        // G7: bayrak dosyasındaki dünya (denge ice → Buz Gölü). Faz 15 K1 seçenekleri: "g=0.5" güç, "eski" tempo profili kapalı, "cirak" ilk oyun
        string flag = File.Exists(".claude-bridge/run_denge") ? File.ReadAllText(".claude-bridge/run_denge").Trim() : "dungeon";
        string[] opts = flag.Split(' ');
        string world = opts[0];
        bool oldStages = false;
        foreach (var o in opts)
        {
            // Faz 15: g=0.5 → bütün gelişim izleri yarıda (gerçek yükseltmeler; güç ve tempo bundan doğar)
            if (o.StartsWith("g=") && float.TryParse(o.Substring(2), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float g))
            {
                var cat = UpgradeCatalog.Load();
                if (cat != null) foreach (var t in cat.tracks) if (t != null) Progression.SetLevel(t.id, Mathf.RoundToInt(t.maxLevel * Mathf.Clamp01(g)));
            }
            if (o == "eski") oldStages = true;
            if (o == "cirak") DifficultyManager.ApprenticeOff = false;
        }
        var catWorld = world != "dungeon" ? EndlessWorlds.Load()?.Find(world) : null; // G7 / Faz 13: herhangi bir dünya (ice, cave...)
        if (catWorld != null && catWorld.level != null)
        {
            GameSettings.SelectedLevel = catWorld.level;
            yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        }
        else if (oldStages)
        {
#if UNITY_EDITOR
            var lvl = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(KakTestUtil.EndlessLevelPath));
            lvl.tempoProfile = null;
            GameSettings.SelectedLevel = lvl;
#endif
            yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        }
        else yield return KakTestUtil.LoadGameWithLevel();
        KakTime.SetTestSpeed(Speed);
        var player = Object.FindAnyObjectByType<PlayerMovement2D>();
        var bot = player.gameObject.AddComponent<KakAutoPilot>();
        bot.skill = skill;
        if (!useDash) bot.dashThreshold = 9999f;
        var sm = GameManager.Instance.scoreManager;
        while (!bot.Finished && sm.ElapsedSeconds < CapSeconds) yield return null;
        var dmEnd = DifficultyManager.Instance;
        int stage = dmEnd != null ? dmEnd.CurrentStageIndex + 1 : 0;
        string tempoInfo = dmEnd != null && dmEnd.TempoMode ? $" τ={dmEnd.Tempo:F2} güç={dmEnd.RunPower:F2}" : " (kademe)";
        GameEvents.PlayerDamaged -= onHit;
        var hitList = new System.Collections.Generic.List<string>();
        foreach (var kv in hits) hitList.Add(kv.Key + ":" + kv.Value);
        string r = $"{(useDash ? "dash" : "dashsiz")} {(GameSettings.SelectedLevel != null ? GameSettings.SelectedLevel.levelName : "?")} skill={skill:F2} vuran=[{string.Join(",", hitList)}] süre={sm.ElapsedSeconds:F0}sn skor={sm.ScoreInt} kademe={stage} vuruş={bot.HitsTaken} yakın={sm.NearMissCount} combo={sm.ComboMultiplier:F1}{tempoInfo}{(bot.Finished ? "" : " (üst sınır)")}";
        results.Add(r);
        Debug.Log("[DengeTests] " + r);
        KakTime.SetTestSpeed(1f);
    }

    [UnityTest]
    [Timeout(1500000)]
    public IEnumerator Usta_ve_Acemi_Bot()
    {
        var results = new System.Collections.Generic.List<string>();
        // G1: dash rafta → yalnız dash'siz oyun
        for (int i = 0; i < 3; i++) yield return PlayOne(1f, results, false);
        for (int i = 0; i < 3; i++) yield return PlayOne(0.25f, results, false);
        Debug.Log("[DengeTests] ÖZET\n" + string.Join("\n", results));
        Assert.Pass(string.Join("\n", results));
    }
}
