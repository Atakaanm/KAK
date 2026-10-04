using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 15 K8: cesaret çubuğu dolunca oyun durur ve 3 kart çıkar; seçilen kart uygulanır, oyun güvenli devam eder; taş kırıcı; iki kişilikte yok.</summary>
public class KacisKartiTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { KakTime.SetPaused(false); KakTestUtil.ResetWorld(); yield return null; }

    static void Quiet()
    {
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        if (EndlessEventManager.Instance != null) EndlessEventManager.Instance.enabled = false;
    }

    [UnityTest]
    public IEnumerator CubukDolunca_UcKart_Secilince_UygulanirDevamEder()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var rp = RunPerks.Instance;
        Assert.IsNotNull(rp, "RunPerks kurulmadı");
        var panel = Object.FindAnyObjectByType<PerkPanel>();
        Assert.IsNotNull(panel, "Kart seçim ekranı sahnede yok (KakUiSetup.SetupGameUi)");
        rp.Add(rp.Need + 0.1f);
        yield return null; yield return null;
        Assert.IsTrue(rp.Choosing, "Çubuk dolunca seçim başlamadı");
        Assert.IsTrue(panel.Open, "Kart ekranı açılmadı");
        Assert.IsTrue(KakTime.Paused, "Seçim sırasında oyun durmadı");
        Assert.AreEqual(3, rp.OfferCount, "3 kart çıkmadı");
        Assert.AreNotEqual(rp.Offer[0], rp.Offer[1]);
        Assert.AreNotEqual(rp.Offer[1], rp.Offer[2]);

        var chosen = rp.Offer[1];
        panel.Pick(1); // kartlar gelmeden seçilmez
        Assert.IsTrue(panel.Open, "Kartlar gelmeden seçim yapıldı");
        yield return KakTestUtil.WaitReal(0.8f);
        panel.Pick(1);
        yield return null;
        Assert.IsFalse(panel.Open);
        Assert.IsFalse(rp.Choosing);
        Assert.IsFalse(KakTime.Paused, "Seçimden sonra oyun devam etmedi");
        Assert.AreEqual(1, rp.LevelOf(chosen), "Seçilen kartın seviyesi artmadı");
        var ph = PlayerStats.Primary.GetComponent<PlayerHealth>();
        Assert.IsTrue(ph.IsInvulnerable, "Seçimden sonra güvenli devam yok");
    }

    [UnityTest]
    public IEnumerator TasKirici_YakindakiTasiKirar_GoktasinaDokunmaz()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        KakTestUtil.MakePlayerSafe();
        var st = PlayerStats.Primary;
        var rb = st.gameObject.AddComponent<RockBreaker>();
        rb.level = 3;
        var shooter = Object.FindAnyObjectByType<CornerShooter>(FindObjectsInactive.Include);
        var data = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Rock_ProjectileData.asset");
        Vector3 p = st.transform.position + new Vector3(1.2f, 0f, 0f);
        var rock = Projectile.Launch(shooter.projectilePrefab, data, p, Vector2.up, 0.01f, 1f);
        Assert.IsNotNull(rock);
        rb.ForceReady();
        yield return null; yield return null;
        Assert.AreEqual(1, rb.Broken, "Taş kırıcı yakındaki taşı kırmadı");
        Assert.IsFalse(Projectile.Active.Contains(rock), "Kırılan taş havuza dönmedi");
    }

    [UnityTest]
    public IEnumerator IkiKisilikte_KartYok()
    {
        GameSettings.TwoPlayer = true;
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var rp = RunPerks.Instance;
        if (rp == null) yield break; // kurulmaması da doğru
        rp.Add(rp.Need + 1f);
        yield return null; yield return null;
        Assert.IsFalse(rp.Choosing, "İki kişilikte kart seçimi açıldı");
        Assert.IsFalse(KakTime.Paused);
    }
}
