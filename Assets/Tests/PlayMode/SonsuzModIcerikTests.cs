using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 5: yakın geçiş, combo, dash, olaylar, fırlatıcı uyarısı.</summary>
public class SonsuzModIcerikTests
{
    GameObject prefab;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        KakTestUtil.ResetWorld();
        yield return KakTestUtil.LoadGameWithLevel();
#if UNITY_EDITOR
        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
#endif
    }

    [UnityTearDown]
    public IEnumerator TearDown() { PlayerHealth.DevGodMode = false; KakTestUtil.ResetWorld(); yield return null; }

    void StopShooters()
    {
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
    }

    [UnityTest]
    public IEnumerator YakinGecis_BonusVeSayacVerir()
    {
        StopShooters();
        KakTestUtil.MakePlayerSafe();
        var sm = GameManager.Instance.scoreManager;
        Vector3 pl = Projectile.PlayerTarget.position;
        int before = sm.NearMissCount;
        // Oyuncunun 0.5 birim yanından geçen taş
        Projectile.Launch(prefab, null, pl + new Vector3(-3f, 0.5f, 0f), Vector2.right, 2f);
        yield return KakTestUtil.WaitUntil(() => sm.NearMissCount > before, 5f, "yakın geçiş sayılmadı");
        Assert.AreEqual(before + 1, sm.NearMissCount, "Tek taş bir kez sayılmalı");
    }

    [UnityTest]
    public IEnumerator Combo_Artar_HasarlaSifirlanir()
    {
        StopShooters();
        var sm = GameManager.Instance.scoreManager;
        sm.comboStepSeconds = 0.3f;
        yield return KakTestUtil.WaitReal(1f);
        Assert.Greater(sm.ComboMultiplier, 1.15f, "Combo artmadı");
        Object.FindAnyObjectByType<PlayerHealth>().TakeDamage(1);
        yield return null;
        Assert.AreEqual(1f, sm.ComboMultiplier, 0.001f, "Hasarda combo sıfırlanmadı");
    }

    [UnityTest]
    public IEnumerator Dash_AtilirOlumsuzBeklemeliDir()
    {
        StopShooters();
        var dash = Object.FindAnyObjectByType<PlayerDash>();
        Assert.IsNotNull(dash, "PlayerDash yok");
        var move = dash.GetComponent<PlayerMovement2D>();
        var ph = dash.GetComponent<PlayerHealth>();
        move.InputOverride = Vector2.right;
        yield return KakTestUtil.WaitReal(0.1f);
        Vector3 p0 = move.transform.position;
        Assert.IsTrue(dash.TryDash(), "Dash başlamadı");
        Assert.IsTrue(ph.IsInvulnerable, "Dash sırasında ölümsüz değil");
        Assert.IsFalse(dash.TryDash(), "Bekleme süresi çalışmıyor");
        yield return KakTestUtil.WaitReal(0.25f);
        Assert.Greater(move.transform.position.x - p0.x, 1.2f, "Dash yeterince uzağa atmadı");
        move.InputOverride = null;
    }

    [UnityTest]
    public IEnumerator Olaylar_HataVermedenBiter_FirlaticilarGeriGelir()
    {
        PlayerHealth.DevGodMode = true;
        var em = Object.FindAnyObjectByType<EndlessEventManager>();
        Assert.IsNotNull(em, "EndlessEventManager yok");
        var shooters = LevelManager.Instance.spawners;
        var before = new bool[shooters.Length];
        for (int i = 0; i < shooters.Length; i++) before[i] = shooters[i].gameObject.activeSelf;

        for (int id = 0; id < 4; id++)
        {
            em.StartEvent(id);
            Assert.IsTrue(em.Running, "Olay başlamadı: " + id);
            yield return KakTestUtil.WaitUntil(() => !em.Running, 15f, "olay bitmedi: " + id);
        }
        for (int i = 0; i < shooters.Length; i++)
        {
            Assert.AreEqual(before[i], shooters[i].gameObject.activeSelf, "Olay sonrası fırlatıcı aktifliği bozuldu: " + shooters[i].name);
            Assert.IsTrue(shooters[i].enabled, "Olay sonrası fırlatıcı kapalı kaldı");
        }
    }

    [UnityTest]
    public IEnumerator Firlatici_AtistanOnceUyarir()
    {
        var shooter = LevelManager.Instance.spawners[0];
        var sr = shooter.spawnerVisual.GetComponent<SpriteRenderer>();
        Color baseColor = sr.color;
        bool warned = false;
        float end = Time.realtimeSinceStartup + shooter.shootInterval + 1f;
        while (Time.realtimeSinceStartup < end && !warned)
        {
            if (sr.color != baseColor) warned = true;
            yield return null;
        }
        Assert.IsTrue(warned, "Fırlatıcı atıştan önce renk değiştirmedi");
    }
}
