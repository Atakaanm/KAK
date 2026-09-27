using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 11 G7: Buz Gölü — kayma, büyüyen kartopu, eldiven (yakala-fırlat), buz ayakkabısı, soğuk ve ateş, dünya seçimi.</summary>
public class BuzTests
{
    const string IceLevel = "Assets/Data/Worlds/Ice/Endless_Ice_LevelData.asset";

    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { PlayerHealth.DevGodMode = false; KakTestUtil.ResetWorld(); yield return null; }

    static IEnumerator LoadIce()
    {
        GameSettings.SelectedLevel = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(IceLevel);
        Assert.IsNotNull(GameSettings.SelectedLevel, "Buz seviyesi yok");
        yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        yield return null;
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
    }

    static ProjectileData Data(string n) => UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Worlds/Ice/" + n + ".asset");
    static PowerupData Pu(string n) => UnityEditor.AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/" + n + ".asset");

    [UnityTest]
    public IEnumerator BuzGolu_TemaFiziKardanAdamSoguk()
    {
        yield return LoadIce();
        var lm = LevelManager.Instance;
        Assert.AreEqual("ice", lm.currentLevel.theme.themeId);
        Assert.AreEqual(lm.currentLevel.theme.arenaSprite, Object.FindAnyObjectByType<ArenaAutoLayout>().arenaSpriteRenderer.sprite, "Buz arenası uygulanmadı");
        var mv = Object.FindAnyObjectByType<PlayerMovement2D>();
        Assert.Less(mv.arenaFriction, 0.5f, "Buzda kayma yok");
        Assert.Greater(mv.verticalSpeedMultiplier, 1f, "Dikey hız çarpanı yok");
        Assert.IsNotNull(mv.GetComponent<ColdMeter>(), "Soğuk göstergesi yok");
        Assert.IsNotNull(Object.FindAnyObjectByType<ColdHud>(), "Soğuk çubuğu yok");
        var vis = Object.FindAnyObjectByType<CornerShooter>().spawnerVisual;
        var snowman = UnityEditor.AssetDatabase.LoadAssetAtPath<SpawnerData>("Assets/Data/Worlds/Ice/Snowman_SpawnerData.asset");
        Assert.AreSame(snowman.idleSouth, vis.south.idle, "Fırlatıcılar kardan adam değil");
    }

    [UnityTest]
    public IEnumerator Kayma_BirakincaHemenDurmaz()
    {
        yield return LoadIce();
        KakTestUtil.MakePlayerSafe();
        var mv = Object.FindAnyObjectByType<PlayerMovement2D>();
        var rb = mv.GetComponent<Rigidbody2D>();
        mv.InputOverride = Vector2.right;
        yield return KakTestUtil.WaitReal(0.8f);
        mv.InputOverride = Vector2.zero;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.Greater(rb.linearVelocity.magnitude, 0.5f, "Buzda bırakınca anında durdu (kayma yok)");
        mv.Grip(5f); // buz ayakkabısı
        yield return new WaitForFixedUpdate();
        Assert.Less(rb.linearVelocity.magnitude, 0.01f, "Buz ayakkabısıyla durmadı");
        mv.InputOverride = null;
    }

    [UnityTest]
    public IEnumerator Kartopu_YolAldikcaBuyur()
    {
        yield return LoadIce();
        KakTestUtil.MakePlayerSafe();
        var d = Data("Snowball_Kartopu");
        Assert.Greater(d.growPerSecond, 0f);
        var p = Projectile.Launch(d.projectilePrefab, d, new Vector3(-2f, 2f, 0f), Vector2.right, 0.2f);
        float s0 = p.transform.localScale.x;
        yield return KakTestUtil.WaitReal(1f);
        Assert.Greater(p.transform.localScale.x, s0 * 1.15f, "Kartopu büyümedi");
        Assert.LessOrEqual(p.transform.localScale.x, s0 * d.maxGrowScale + 0.001f, "Büyüme sınırı aşıldı");
    }

    [UnityTest]
    public IEnumerator Eldiven_Yakalar_Firlatir_DusmanKartopunuParcalar()
    {
        yield return LoadIce();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        ph.SetMaxHealth(3);
        PowerupPickup.Apply(Pu("GloveData"), ph.gameObject, ph.transform.position);
        var catcher = ph.GetComponent<SnowballCatcher>();
        Assert.IsNotNull(catcher);
        var d = Data("Snowball_Kartopu");
        Projectile.Launch(d.projectilePrefab, d, ph.transform.position + new Vector3(0f, 2f, 0f), Vector2.down, 2f);
        yield return KakTestUtil.WaitUntil(() => catcher.Holding, 3f, "kartopu yakalanmadı");
        Assert.AreEqual(3, ph.CurrentHealth, "Yakalanan kartopu hasar verdi");

        // Düşman kartopu yukarıda, oyuncuya doğru; fırlatılan kartopu onu parçalamalı
        var mv = ph.GetComponent<PlayerMovement2D>();
        mv.InputOverride = Vector2.up;
        yield return null;
        int smashed = 0;
        System.Action<Vector3> onSmash = _ => smashed++;
        GameEvents.SnowballSmashed += onSmash;
        var enemy = Projectile.Launch(d.projectilePrefab, d, ph.transform.position + new Vector3(0f, 2.6f, 0f), Vector2.down, 0.05f);
        catcher.Throw();
        mv.InputOverride = null;
        Assert.IsFalse(catcher.Holding);
        yield return KakTestUtil.WaitUntil(() => smashed > 0, 3f, "fırlatılan kartopu düşmanı parçalamadı");
        GameEvents.SnowballSmashed -= onSmash;
        Assert.IsFalse(enemy.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator Soguk_Dondurur_AtesIsitir()
    {
        yield return LoadIce();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        ph.SetMaxHealth(3);
        var cm = ph.GetComponent<ColdMeter>();
        var mv = ph.GetComponent<PlayerMovement2D>();
        cm.coldSeconds = 1.5f;
        yield return KakTestUtil.WaitReal(0.95f);
        Assert.Less(mv.coldSpeedMultiplier, 1f, "Üşüyünce yavaşlamadı");
        yield return KakTestUtil.WaitUntil(() => mv.IceFrozen, 3f, "Tam soğukta donmadı");
        Assert.AreEqual(3, ph.CurrentHealth, "Donma doğrudan can götürmemeli (taşlara açık kalmak yeterli ceza)");
        float v = cm.Value;
        PowerupPickup.Apply(Pu("FireData"), ph.gameObject, ph.transform.position);
        Assert.Less(cm.Value, v - 0.3f, "Ateş ısıtmadı");
    }

    [UnityTest]
    public IEnumerator Dunyalar_Kilit_Secim_AyriRekor()
    {
        var cat = EndlessWorlds.Load();
        var ice = cat.Find("ice");
        Assert.IsNotNull(ice);
        Assert.IsFalse(EndlessWorlds.Select(ice), "Buz ilk oyunda seçilebildi");
        SaveSystem.Data.gamesPlayed = FeatureGate.WorldsGames;
        Assert.IsTrue(EndlessWorlds.Select(ice));
        Assert.AreSame(ice, EndlessWorlds.Selected());
        SaveSystem.Data.bestScoreEndless = 5000;
        yield return LoadIce();
        yield return KakTestUtil.WaitReal(1.2f);
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance.IsGameOver, 3f, "oyun bitmedi");
        Assert.Greater(SaveSystem.Data.bestScoreIce, 0, "Buz rekoru kaydedilmedi");
        Assert.AreEqual(5000, SaveSystem.Data.bestScoreEndless, "Zindan rekoru değişmemeli");
    }

    [UnityTest]
    public IEnumerator BuzGolu_8Saniye_HataLoguOlmadanCalisir()
    {
        var problems = new System.Collections.Generic.List<string>();
        Application.LogCallback onLog = (msg, stack, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) problems.Add(type + ": " + msg);
        };
        Application.logMessageReceived += onLog;
        GameSettings.SelectedLevel = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(IceLevel);
        yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        KakTestUtil.MakePlayerSafe();
        yield return KakTestUtil.WaitReal(8f);
        Application.logMessageReceived -= onLog;
        Assert.IsEmpty(problems, "Buzda hata logu:\n" + string.Join("\n", problems));
    }
}
