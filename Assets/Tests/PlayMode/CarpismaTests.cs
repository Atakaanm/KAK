using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Denetim D1: adil çarpışma. Taş, görünen gövdeye değmeden vurmamalı; değince vurmalı.
/// Kalkan çarpışma alanını büyütmemeli.
/// </summary>
public class CarpismaTests
{
    // Oyuncu karakterinin görünen gövdesi (idle karelerin opak sınırları, 1 px = 0.024): ~17 x 40 px
    const float VisualBodyWidth = 0.41f;
    const float VisualBodyHeight = 0.96f;

    GameObject prefab;
    PlayerHealth ph;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        KakTestUtil.ResetWorld();
        yield return KakTestUtil.LoadGameWithLevel();
#if UNITY_EDITOR
        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
#endif
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        var spawner = Object.FindAnyObjectByType<PowerupSpawner>();
        if (spawner != null) spawner.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        ph = Object.FindAnyObjectByType<PlayerHealth>();
        Assert.IsNotNull(ph);
        yield return KakTestUtil.WaitReal(0.5f); // başlangıç korumaları bitsin
    }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [Test]
    public void Govde_GorseldenKucuk_AmaCokKucukDegil()
    {
        var hb = ph.GetComponent<PlayerHitbox>();
        Assert.IsNotNull(hb, "Oyuncuda PlayerHitbox yok (KacAtaKac/Oyuncu Çarpışmasını Kur)");
        Assert.AreSame(hb.HurtCollider, PlayerHitbox.Hurt);
        Vector3 size = hb.HurtCollider.bounds.size;
        Assert.LessOrEqual(size.x, VisualBodyWidth, "Gövde collider'ı görünen gövdeden geniş");
        Assert.LessOrEqual(size.y, VisualBodyHeight, "Gövde collider'ı görünen gövdeden uzun");
        Assert.GreaterOrEqual(size.x, VisualBodyWidth * 0.7f, "Gövde collider'ı fazla dar (taşlar içinden geçer gibi görünür)");
        Assert.GreaterOrEqual(size.y, VisualBodyHeight * 0.7f, "Gövde collider'ı fazla kısa");
        Assert.IsTrue(hb.HurtCollider.isTrigger);
        Assert.IsTrue(hb.HurtCollider.CompareTag("Player"));
    }

    [UnityTest]
    public IEnumerator Tas_GovdeyeCarparsaHasarVerir()
    {
        int hp0 = ph.currentHealth;
        Vector3 pl = ph.transform.position;
        Projectile.Launch(prefab, null, pl + new Vector3(-3f, 0f, 0f), Vector2.right, 1f);
        yield return KakTestUtil.WaitUntil(() => ph.currentHealth < hp0, 4f, "Gövdeye gelen taş hasar vermedi");
        Assert.AreEqual(hp0 - 1, ph.currentHealth);
    }

    [UnityTest]
    public IEnumerator Tas_OmuzuSiyirinca_HasarVermez()
    {
        // Eski daire (yarıçap 0.40, temas 0.66) bu taşı yakalardı: taş görünen gövdeye değmeden vuruyordu
        const float offset = 0.58f;
        int hp0 = ph.currentHealth;
        Vector3 pl = ph.transform.position;
        var p = Projectile.Launch(prefab, null, pl + new Vector3(offset, -3f, 0f), Vector2.up, 1f);
        float rockVisualR = 0.336f * Mathf.Abs(p.transform.lossyScale.x); // 28 px taş
        Assert.Greater(offset, VisualBodyWidth * 0.5f + rockVisualR, "Test kurulumu: taş gövdeye görsel olarak değiyor");
        yield return KakTestUtil.WaitReal(2.5f);
        Assert.AreEqual(hp0, ph.currentHealth, "Gövdeye değmeyen taş hasar verdi");
    }

    [UnityTest]
    public IEnumerator Kalkan_OlcekDegistirmez_BalonGosterir_BirVurusuEmer()
    {
        Vector3 scale0 = ph.transform.localScale;
        Color color0 = ph.playerSpriteRenderer.color;
        ph.ActivateShield();
        yield return null;
        Assert.AreEqual(scale0, ph.transform.localScale, "Kalkan oyuncuyu (ve çarpışmasını) büyütmemeli");
        Assert.AreEqual(color0, ph.playerSpriteRenderer.color, "Kalkan karakteri boyamamalı");
        var bubble = ph.GetComponentInChildren<ShieldBubble>(true);
        Assert.IsNotNull(bubble);
        Assert.IsTrue(bubble.Visible, "Kalkan balonu görünmüyor");

        int hp0 = ph.currentHealth;
        ph.TakeDamage(1);
        Assert.AreEqual(hp0, ph.currentHealth, "Kalkan vuruşu emmedi");
        Assert.IsFalse(ph.HasShield);
        yield return KakTestUtil.WaitReal(0.4f);
        Assert.IsFalse(bubble.gameObject.activeSelf, "Kalkan kırılınca balon kaybolmadı");
    }
}
