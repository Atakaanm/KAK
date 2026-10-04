using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 15 K1: sürekli tempo — kademe afişi yok, yeni fırlatıcı payını yavaş alır, çırak, güç, merhamet, nefes payı, geri uyum.</summary>
public class TempoTests
{
    int stageEvents;
    void OnStage(string n, int i) => stageEvents++;

    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); stageEvents = 0; yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        GameEvents.StageChanged -= OnStage;
        KakTestUtil.ResetWorld();
        yield return null;
    }

    /// <summary>Taşlar ve olaylar dursun: yalnız tempo değerleri ölçülür.</summary>
    static void Quiet()
    {
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        if (EndlessEventManager.Instance != null) EndlessEventManager.Instance.enabled = false;
        KakTestUtil.MakePlayerSafe();
    }

    static IEnumerator Frames() { yield return null; yield return null; }

    /// <summary>Uyanık fırlatıcıların toplam atış hızı (atış/sn).</summary>
    static float TotalRate(DifficultyManager dm)
    {
        float r = 0f;
        for (int i = 0; i < dm.ActiveThrowers; i++) r += 1f / dm.allSpawners[i].shootInterval;
        return r;
    }

    [UnityTest]
    public IEnumerator Tempo_SureyleArtar_KademeAfisiYok()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        GameEvents.StageChanged += OnStage;
        var dm = DifficultyManager.Instance;
        var sm = GameManager.Instance.scoreManager;
        Assert.IsTrue(dm.TempoMode, "Zindan sonsuz bölümünde tempo profili yok");
        float t0 = dm.Tempo;
        Assert.Less(t0, 0.03f, "Güçsüz oyuncu sıfıra yakın tempoyla başlamalı");
        sm.AdvanceTime(80f);
        yield return Frames();
        Assert.Greater(dm.Tempo, t0 + 0.2f, "Tempo süreyle artmadı");
        Assert.AreEqual(2, dm.CurrentStageIndex, "Uyumluluk kademe dizini τ'dan türemedi");
        Assert.AreEqual(0, stageEvents, "Tempo modunda kademe afişi/olayı çıkmamalı");
        Assert.Greater(dm.GetScoreSpeedMultiplier(), 1.1f, "Hızlı oyun hızlı skor vermiyor");
    }

    [UnityTest]
    public IEnumerator YeniFirlatici_PayiniYavasAlir_ToplamSicramaz()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var dm = DifficultyManager.Instance;
        var sm = GameManager.Instance.scoreManager;
        Assert.AreEqual(1, dm.ActiveThrowers, "Oyun tek fırlatıcıyla başlamalı");
        sm.AdvanceTime(29.5f);
        yield return Frames();
        float before = TotalRate(dm);
        sm.AdvanceTime(1f); // ≈30,5 sn: 2. fırlatıcı uyanır
        yield return Frames();
        Assert.AreEqual(2, dm.ActiveThrowers, "2. fırlatıcı uyanmadı");
        var s = dm.allSpawners;
        Assert.Greater(s[1].shootInterval, s[0].shootInterval * 5f, "Yeni fırlatıcı hemen tam payla başladı");
        Assert.AreEqual(before, TotalRate(dm), before * 0.08f, "Fırlatıcı eklenince toplam atış hızı sıçradı");
        sm.AdvanceTime(11f);
        yield return Frames();
        Assert.AreEqual(s[0].shootInterval, s[1].shootInterval, 0.02f, "Yeni fırlatıcı ~10 sn'de tam paya gelmedi");
    }

    [UnityTest]
    public IEnumerator Cirak_IlkOyunYavas_SonraNormal()
    {
        DifficultyManager.ApprenticeOff = false; // taze kayıt: 0 oyun
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var dm = DifficultyManager.Instance;
        var sm = GameManager.Instance.scoreManager;
        Assert.AreEqual(0, dm.RunGames);
        float curve = dm.tempo.projectileSpeed.Evaluate(dm.Tempo);
        Assert.AreEqual(curve * 0.85f, dm.GetProjectileSpeedMultiplier(), 0.02f, "İlk oyunda taşlar yavaş değil");
        sm.AdvanceTime(70f);
        yield return Frames();
        Assert.AreEqual(70f / (360f * 1.35f), dm.Tempo, 0.01f, "İlk oyunda rampa uzun değil");
        Assert.AreEqual(dm.tempo.projectileSpeed.Evaluate(dm.Tempo), dm.GetProjectileSpeedMultiplier(), 0.01f, "Çırak yavaşlığı 60 sn'de sönmedi");
    }

    [UnityTest]
    public IEnumerator Guc_HizliIsinir_AmaSakinBaslar()
    {
        PlayerPower.TestOverride = 1f;
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var dm = DifficultyManager.Instance;
        Assert.AreEqual(1f, dm.RunPower, 0.001f);
        Assert.Less(dm.Tempo, 0.05f, "Güçlü oyuncu da sakin başlamalı (ani sıçrama yok)");
        Assert.AreEqual(1, dm.ActiveThrowers, "Başta tek fırlatıcı olmalı");
        GameManager.Instance.scoreManager.AdvanceTime(20f);
        yield return Frames();
        Assert.Greater(dm.Tempo, 0.33f, "Güçlü oyuncu ~20 sn'de hızlı ısınmalı");
        Assert.AreEqual(3, dm.ActiveThrowers, "Isınınca 3 fırlatıcı uyanık olmalı");
    }

    [UnityTest]
    public IEnumerator Merhamet_HasarSonrasiTempoBekler()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var dm = DifficultyManager.Instance;
        var sm = GameManager.Instance.scoreManager;
        sm.AdvanceTime(100f);
        yield return Frames();
        GameEvents.RaisePlayerDamaged(1, Vector3.zero);
        yield return null;
        float a = dm.Tempo;
        yield return KakTestUtil.WaitReal(1.5f);
        Assert.AreEqual(a, dm.Tempo, 0.0005f, "Hasardan sonra tempo artmaya devam etti");
    }

    [UnityTest]
    public IEnumerator NefesPayi_OlayBitinceAtisSeyrelir()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var dm = DifficultyManager.Instance;
        var sm = GameManager.Instance.scoreManager;
        sm.AdvanceTime(300f);
        yield return Frames();
        float r0 = dm.FireRate;
        dm.Breathe();
        yield return KakTestUtil.WaitReal(1.3f);
        Assert.Greater(dm.Relax, 0.95f, "Nefes payı ~1 sn'de gelmedi");
        Assert.Less(dm.FireRate, r0 * 0.9f, "Nefes payında atış seyrekleşmedi");
        Assert.AreEqual(0f, dm.DoubleShotChance, "Nefes payında çift atış olmamalı");
    }

    [UnityTest]
    public IEnumerator ProfilYoksa_EskiKademeSistemi()
    {
#if UNITY_EDITOR
        var src = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(KakTestUtil.EndlessLevelPath);
        var lvl = Object.Instantiate(src);
        lvl.tempoProfile = null;
        GameSettings.SelectedLevel = lvl;
#endif
        yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        Quiet();
        GameEvents.StageChanged += OnStage;
        var dm = DifficultyManager.Instance;
        Assert.IsFalse(dm.TempoMode);
        GameManager.Instance.scoreManager.AdvanceTime(80f);
        yield return Frames();
        Assert.AreEqual(2, dm.CurrentStageIndex);
        Assert.Greater(stageEvents, 0, "Eski kademe sistemi kademe olayını bildirmeli");
    }
}
