using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz Y3: reklamla devam et ve 2× altın akışları (sahte sağlayıcıyla). Varsayılan: reklam kapalı.</summary>
public class ReklamTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    static SimulatedAdProvider EnableAds(bool succeed = true)
    {
        var c = ScriptableObject.CreateInstance<AdConfig>();
        c.enabled = true;
        c.provider = AdProviderKind.Simulated;
        c.continueSeconds = 30f;
        var p = new SimulatedAdProvider { duration = 0f, succeed = succeed };
        AdService.OverrideConfig = c;
        AdService.OverrideProvider = p;
        return p;
    }

    [UnityTest]
    public IEnumerator ReklamKapaliyken_DevamTeklifiYok_EskisiGibiOyunSonu()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.KillPlayer();
        Assert.AreEqual(1, SaveSystem.Data.gamesPlayed, "Reklam kapalıyken kayıt hemen yapılmalı");
        yield return KakTestUtil.WaitReal(1.2f);
        Assert.IsFalse(GameManager.Instance.continuePanel.Visible);
        Assert.IsTrue(GameManager.Instance.gameOverPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator DevamEt_Kabul_Canlanir_KayitIkinciOlumde_BirKez()
    {
        EnableAds();
        yield return KakTestUtil.LoadGameWithLevel();
        var gm = GameManager.Instance;
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => gm.continuePanel.Visible, 3f, "Devam et paneli çıkmadı");
        Assert.AreEqual(0, SaveSystem.Data.gamesPlayed, "Devam teklifi beklerken oyun kaydedilmemeli");
        gm.continuePanel.Accept();
        yield return null;
        Assert.IsFalse(gm.IsGameOver, "Canlanma olmadı");
        Assert.AreEqual(1, gm.Revives);
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        Assert.IsFalse(ph.IsDead);
        Assert.AreEqual(1, ph.currentHealth);
        Assert.IsTrue(ph.IsInvulnerable, "Canlanınca kısa dokunulmazlık olmalı");
        Assert.AreEqual(1f, Time.timeScale, 0.01f);
        Assert.IsFalse(gm.continuePanel.Visible);

        yield return KakTestUtil.WaitReal(0.3f);
        KakTestUtil.KillPlayer(); // ikinci ölüm: oyun başına 1 teklif
        yield return KakTestUtil.WaitReal(1.2f);
        Assert.IsFalse(gm.continuePanel.Visible, "İkinci kez devam teklif edilmemeli");
        Assert.AreEqual(1, SaveSystem.Data.gamesPlayed, "Oyun bir kez kaydedilmeli");
        Assert.IsTrue(gm.gameOverPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator DevamEt_Ret_OyunSonuVeKayit()
    {
        EnableAds();
        yield return KakTestUtil.LoadGameWithLevel();
        var gm = GameManager.Instance;
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => gm.continuePanel.Visible, 3f, "Devam et paneli çıkmadı");
        gm.continuePanel.Decline();
        yield return null;
        Assert.IsFalse(gm.continuePanel.Visible);
        Assert.AreEqual(1, SaveSystem.Data.gamesPlayed);
        Assert.IsTrue(gm.gameOverPanel.activeSelf);
        Assert.IsTrue(gm.IsGameOver);
    }

    [UnityTest]
    public IEnumerator ReklamYarimKalirsa_Canlanmaz_OyunSonu()
    {
        EnableAds(succeed: false);
        yield return KakTestUtil.LoadGameWithLevel();
        var gm = GameManager.Instance;
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => gm.continuePanel.Visible, 3f, "Devam et paneli çıkmadı");
        gm.continuePanel.Accept();
        yield return null;
        Assert.IsTrue(gm.IsGameOver, "Reklam izlenmeden canlanıldı");
        Assert.AreEqual(0, gm.Revives);
        Assert.IsTrue(gm.gameOverPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator IkiKatAltin_ReklamlaBirKez()
    {
        SaveSystem.Data.gamesPlayed = 1; // altın açık
        EnableAds();
        yield return KakTestUtil.LoadGameWithLevel();
        var gm = GameManager.Instance;
        gm.scoreManager.AddCoins(6, Vector3.zero);
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => gm.continuePanel.Visible, 3f, "Devam et paneli çıkmadı");
        gm.continuePanel.Decline();
        var gos = Object.FindAnyObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        yield return KakTestUtil.WaitUntil(() => gos.doubleCoinsButton.gameObject.activeSelf, 5f, "2× altın butonu çıkmadı");
        int before = SaveSystem.Data.coins;
        int earned = gm.RunCoins + gm.RunCoinBonus;
        gos.OnDoubleCoins();
        Assert.AreEqual(before + earned, SaveSystem.Data.coins, "Altın ikiye katlanmadı");
        Assert.IsFalse(gos.doubleCoinsButton.gameObject.activeSelf);
        Assert.IsFalse(AdService.CanShow(AdPlacement.DoubleCoins), "Oyun başına bir kez");
    }
}
