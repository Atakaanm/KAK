using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 3c.1: altın — ilk oyunda kapalı, sonra çıkar, toplanır, oyun sonunda cüzdana eklenir.</summary>
public class AltinTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { PlayerHealth.DevGodMode = false; KakTestUtil.ResetWorld(); yield return null; }

    static void Quiet()
    {
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        var ps = Object.FindAnyObjectByType<PowerupSpawner>();
        if (ps != null) ps.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
    }

    [UnityTest]
    public IEnumerator IlkOyunda_AltinKapali()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var cs = Object.FindAnyObjectByType<CoinSpawner>();
        Assert.IsNotNull(cs, "CoinSpawner yok (Bağlılık Katmanını Kur)");
        Assert.IsFalse(cs.ActiveThisRun, "İlk oyunda altın çıkmamalı (saf kaçış)");
        var hud = Object.FindAnyObjectByType<CoinHud>();
        Assert.IsNotNull(hud);
        Assert.IsFalse(hud.content.gameObject.activeSelf, "Altın kapalıyken HUD sayacı görünmemeli");
    }

    [UnityTest]
    public IEnumerator IkinciOyundan_AltinCikar_Toplanir_CuzdanaEklenir()
    {
        SaveSystem.Data.gamesPlayed = 1;
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var cs = Object.FindAnyObjectByType<CoinSpawner>();
        Assert.IsTrue(cs.ActiveThisRun, "İkinci oyunda altın açık olmalı");
        var hud = Object.FindAnyObjectByType<CoinHud>();
        Assert.IsTrue(hud.content.gameObject.activeSelf);

        int n = cs.SpawnCluster();
        Assert.Greater(n, 0, "küme doğmadı");
        yield return null;
        Assert.AreEqual(n, Coin.Active.Count);

        // Oyuncuyu bir altının üstüne taşı
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        var coin = Coin.Active[0];
        var rb = ph.GetComponent<Rigidbody2D>();
        rb.position = coin.transform.position;
        ph.transform.position = coin.transform.position;
        var sm = GameManager.Instance.scoreManager;
        yield return KakTestUtil.WaitUntil(() => sm.Coins >= 1, 2f, "altın toplanmadı");
        Assert.AreEqual(sm.Coins.ToString(), hud.countText.text, "HUD sayacı güncellenmedi");

        int walletBefore = SaveSystem.Data.coins;
        int run = sm.Coins;
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance.IsGameOver, 3f, "oyun bitmedi");
        int expected = run + sm.ScoreInt / 100;
        Assert.AreEqual(walletBefore + expected, SaveSystem.Data.coins, "Altın cüzdana eklenmedi");
        Assert.AreEqual(expected, GameManager.Instance.RunCoins + GameManager.Instance.RunCoinBonus);

        var gos = Object.FindAnyObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        yield return KakTestUtil.WaitUntil(() => gos.coinsRow.gameObject.activeSelf, 5f, "oyun sonunda altın satırı görünmedi");
    }

    [UnityTest]
    public IEnumerator Altin_SuresiDolunca_HavuzaDoner()
    {
        SaveSystem.Data.gamesPlayed = 1;
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var cs = Object.FindAnyObjectByType<CoinSpawner>();
        cs.enabled = false;
        cs.SpawnCluster();
        yield return null;
        var coin = Coin.Active[0];
        var go = coin.gameObject;
        coin.lifetime = 0.3f;
        yield return KakTestUtil.WaitUntil(() => !go.activeSelf, 2f, "süresi dolan altın kaybolmadı");
        Assert.IsTrue(go != null, "altın yok edildi (havuza dönmeliydi)");
    }
}
