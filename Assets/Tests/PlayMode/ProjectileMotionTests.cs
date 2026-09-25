using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Taş hareket türleri: seken, parçalanan, güdümlü, göktaşı.</summary>
public class ProjectileMotionTests
{
    GameObject prefab;
    Rect play;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        KakTestUtil.ResetWorld();
        yield return KakTestUtil.LoadGameWithLevel();
        // Fırlatıcıları kapat: sadece testin attığı taşlar olsun
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        var ps = Object.FindAnyObjectByType<PowerupSpawner>();
        if (ps != null) ps.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        play = Object.FindAnyObjectByType<ArenaAutoLayout>().PlayableWorldRect;
#if UNITY_EDITOR
        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
#endif
        Assert.IsNotNull(prefab);
    }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    static ProjectileData Data(string name)
    {
#if UNITY_EDITOR
        var d = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Projectiles/" + name + ".asset");
        Assert.IsNotNull(d, "Veri yok: " + name + " (KacAtaKac/Taş Türlerini Kur)");
        return d;
#else
        return null;
#endif
    }

    [UnityTest]
    public IEnumerator Seken_DuvardanSeker_SonraKirilir()
    {
        KakTestUtil.MakePlayerSafe();
        var d = Data("Bouncer_Seken");
        var p = Projectile.Launch(prefab, d, play.center, Vector2.right, 3f);
        float end = Time.realtimeSinceStartup + 4f;
        bool reversed = false;
        while (Time.realtimeSinceStartup < end && p.gameObject.activeSelf)
        {
            if (p.moveDirection.x < 0f) reversed = true;
            yield return null;
        }
        Assert.IsTrue(reversed, "Taş duvardan sekmedi");
    }

    [UnityTest]
    public IEnumerator Parcalanan_UcParcayaBolunur()
    {
        KakTestUtil.MakePlayerSafe();
        var d = Data("Splitter_Parcalanan");
        var p = Projectile.Launch(prefab, d, play.center + new Vector2(0f, -1.5f), Vector2.up, 1f);
        Assert.AreEqual(1, Projectile.Active.Count);
        yield return KakTestUtil.WaitUntil(() => !p.gameObject.activeSelf, 3f, "taş bölünmedi");
        Assert.AreEqual(d.splitCount, Projectile.Active.Count, "Parça sayısı yanlış");
        foreach (var c in Projectile.Active) Assert.AreEqual(ProjectileMotion.Straight, c.Motion, "Parçalar çakıl olmalı");
    }

    [UnityTest]
    public IEnumerator Gudumlu_OyuncuyaDoner()
    {
        KakTestUtil.MakePlayerSafe();
        var d = Data("Homing_Gudumlu");
        var player = Projectile.PlayerTarget;
        Assert.IsNotNull(player, "PlayerTarget yok");
        Vector2 start = (Vector2)player.position + new Vector2(-2.5f, 2f);
        var p = Projectile.Launch(prefab, d, start, Vector2.up, 1f); // oyuncudan uzağa doğru
        float angle0 = Vector2.Angle(p.moveDirection, (Vector2)player.position - start);
        yield return KakTestUtil.WaitReal(0.8f);
        float angle1 = Vector2.Angle(p.moveDirection, (Vector2)player.position - (Vector2)p.transform.position);
        Assert.Less(angle1, angle0 - 30f, $"Güdümlü taş dönmedi ({angle0:F0}° → {angle1:F0}°)");
    }

    [UnityTest]
    public IEnumerator Goktasi_GolgeUyarisi_InisteHasarVerir()
    {
        var d = Data("Meteor_Goktasi");
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        int hp0 = ph.currentHealth;
        var p = Projectile.LaunchMeteor(prefab, d, ph.transform.position);
        yield return null;
        var col = p.GetComponent<Collider2D>();
        Assert.IsFalse(col.enabled, "Düşerken çarpışma kapalı olmalı");
        Assert.Greater(p.FallRemaining01, 0.5f);
        yield return KakTestUtil.WaitUntil(() => !p.gameObject.activeSelf, 3f, "göktaşı inmedi");
        Assert.AreEqual(hp0 - 1, ph.currentHealth, "İnişte oyuncu hasar almadı");
    }
}
