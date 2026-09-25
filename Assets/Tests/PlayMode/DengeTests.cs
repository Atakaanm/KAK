using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Denge ölçümü (uzun): usta ve acemi bot birkaç tam oyun oynar, 2x hızda.
/// Sadece istenince çalışır: python3 tools/kak_bridge.py denge
/// Hedef (progress.md): acemi 60-90 sn, usta 180-300 sn.
/// </summary>
public class DengeTests
{
    const float Speed = 2f;
    const float CapSeconds = 300f; // oyun süresi üst sınırı

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
        yield return KakTestUtil.LoadGameWithLevel();
        KakTime.SetTestSpeed(Speed);
        var player = Object.FindAnyObjectByType<PlayerMovement2D>();
        var bot = player.gameObject.AddComponent<KakAutoPilot>();
        bot.skill = skill;
        if (!useDash) bot.dashThreshold = 9999f;
        var sm = GameManager.Instance.scoreManager;
        while (!bot.Finished && sm.ElapsedSeconds < CapSeconds) yield return null;
        int stage = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentStageIndex + 1 : 0;
        GameEvents.PlayerDamaged -= onHit;
        var hitList = new System.Collections.Generic.List<string>();
        foreach (var kv in hits) hitList.Add(kv.Key + ":" + kv.Value);
        string r = $"{(useDash ? "dash" : "dashsiz")} skill={skill:F2} vuran=[{string.Join(",", hitList)}] süre={sm.ElapsedSeconds:F0}sn skor={sm.ScoreInt} kademe={stage} vuruş={bot.HitsTaken} yakın={sm.NearMissCount} combo={sm.ComboMultiplier:F1}{(bot.Finished ? "" : " (üst sınır)")}";
        results.Add(r);
        Debug.Log("[DengeTests] " + r);
        KakTime.SetTestSpeed(1f);
    }

    [UnityTest]
    [Timeout(1500000)]
    public IEnumerator Usta_ve_Acemi_Bot()
    {
        var results = new System.Collections.Generic.List<string>();
        for (int i = 0; i < 3; i++) yield return PlayOne(1f, results, true);
        for (int i = 0; i < 3; i++) yield return PlayOne(1f, results, false);
        for (int i = 0; i < 2; i++) yield return PlayOne(0.25f, results, false);
        Debug.Log("[DengeTests] ÖZET\n" + string.Join("\n", results));
        Assert.Pass(string.Join("\n", results));
    }
}
