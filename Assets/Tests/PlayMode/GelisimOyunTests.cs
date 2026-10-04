using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 15 K3: ortak gelişim oyunda uygulanır — can, hız, kıl payı, skor, uyarı, yerde kalma; mıknatıs; ikinci şans.</summary>
public class GelisimOyunTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { Projectile.WarningMult = 1f; KakTestUtil.ResetWorld(); yield return null; }

    static void Quiet()
    {
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        if (EndlessEventManager.Instance != null) EndlessEventManager.Instance.enabled = false;
    }

    [UnityTest]
    public IEnumerator Gelisim_OyunBasindaUygulanir()
    {
        Progression.SetLevel("hearts", 2);
        Progression.SetLevel("speed", 5);
        Progression.SetLevel("nearradius", 4);
        Progression.SetLevel("score", 5);
        Progression.SetLevel("sense", 5);
        Progression.SetLevel("combomax", 4);
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var stats = PlayerStats.Primary;
        Assert.IsNotNull(stats, "Oyuncuda PlayerStats yok");
        var ph = stats.GetComponent<PlayerHealth>();
        var mv = stats.GetComponent<PlayerMovement2D>();
        var data = stats.Data;
        Assert.AreEqual(Mathf.Min(data.maxHealth, StatBuilder.StartHealth(data) + 2), ph.MaxHealth, "Can gelişimi uygulanmadı");
        Assert.AreEqual(StatBuilder.MoveSpeed(data, stats.Sheet), mv.moveSpeed, 1e-3f);
        Assert.Greater(mv.moveSpeed, data.moveSpeed * 1.14f, "Hız gelişimi uygulanmadı");
        Assert.AreEqual(0.75f * (1f + 4 * Progression.Track("nearradius").percentPerLevel), mv.GetComponent<NearMissTracker>().radius, 0.01f);
        var sm = GameManager.Instance.scoreManager;
        Assert.Greater(sm.scoreGain, 1.2f, "Skor ustası uygulanmadı");
        Assert.AreEqual(3f + 1f, sm.multMax, 0.01f, "Çarpan tavanı uygulanmadı");
        Assert.Greater(Projectile.WarningMult, 1.35f, "Sezgi uygulanmadı");
    }

    [UnityTest]
    public IEnumerator Miknatis_AltiniCeker()
    {
        Progression.SetLevel("magnet", 5);
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        KakTestUtil.MakePlayerSafe();
        var stats = PlayerStats.Primary;
        Assert.Greater(stats.MagnetRadius, 1.5f);
        var cs = Object.FindAnyObjectByType<CoinSpawner>();
        Assert.IsNotNull(cs, "CoinSpawner yok");
        var mv = stats.GetComponent<PlayerMovement2D>();
        mv.InputOverride = Vector2.zero;
        Vector3 p = stats.transform.position;
        int before = GameManager.Instance.scoreManager.Coins;
        var coin = cs.SpawnCoinAt(p + new Vector3(1.2f, 0f, 0f));
        Assert.IsNotNull(coin, "Altın doğmadı");
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance.scoreManager.Coins > before, 2f, "Mıknatıs altını çekmedi");
    }

    [UnityTest]
    public IEnumerator IkinciSans_BirKezKaldirir()
    {
        Progression.SetLevel("revive", 1);
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var gm = GameManager.Instance;
        KakTestUtil.KillPlayer();
        yield return null;
        Assert.IsFalse(gm.IsGameOver, "İkinci şans oyunu kurtarmadı");
        var ph = PlayerStats.Primary.GetComponent<PlayerHealth>();
        Assert.IsFalse(ph.IsDead, "Oyuncu kalkmadı");
        Assert.AreEqual(1, gm.SecondChancesUsed);
        yield return KakTestUtil.WaitReal(0.2f);
        KakTestUtil.KillPlayer();
        yield return null;
        Assert.IsTrue(gm.IsGameOver, "İkinci şans iki kez kullanıldı");
    }
}
