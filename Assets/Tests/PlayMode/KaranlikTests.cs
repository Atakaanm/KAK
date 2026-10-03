using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 13 K1: karanlık dünya + meşale (yarıçap seviyeyle, satın alma), karanlıkta ipuçları.</summary>
public class KaranlikTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { GameSettings.TwoPlayer = false; DarkWorld.Reset(); KakTestUtil.ResetWorld(); yield return null; }

    static WorldTheme DarkTheme(float d = 0.95f)
    {
        var t = ScriptableObject.CreateInstance<WorldTheme>();
        t.darkness = d;
        return t;
    }

    static float WorldDiameter(Transform t)
    {
        var r = t.GetComponent<Renderer>();
        return r.bounds.size.x;
    }

    [UnityTest]
    public IEnumerator Karanlik_Katman_Mesale_Ipuclari()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        Assert.IsFalse(DarkWorld.Active, "Zindan karanlık olmamalı");
        Assert.IsNull(Object.FindAnyObjectByType<DarknessOverlay>());

        DarkWorld.Apply(DarkTheme());
        yield return null;
        Assert.IsTrue(DarkWorld.Active);
        var overlay = Object.FindAnyObjectByType<DarknessOverlay>();
        Assert.IsNotNull(overlay, "Karanlık katmanı yok");
        var osr = overlay.GetComponent<SpriteRenderer>();
        Assert.AreEqual(SpriteMaskInteraction.VisibleOutsideMask, osr.maskInteraction);
        Assert.Greater(osr.color.a, 0.9f);
        var cam = Object.FindAnyObjectByType<ScreenComposer>().GetComponent<Camera>();
        Assert.Greater(osr.bounds.size.y, cam.orthographicSize * 2f, "Karanlık ekranı kaplamıyor");

        // Meşale: oyuncuda delik, yarıçap seviyeye göre
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        var light = ph.GetComponent<PlayerLight>();
        Assert.IsNotNull(light, "Oyuncuda meşale yok");
        var mask = ph.transform.Find("LightMask").GetComponent<SpriteMask>();
        Assert.IsNotNull(mask.sprite, "Işık maskesi sprite'ı yüklenmedi (Resources içe aktarımı)");
        Assert.AreEqual(TorchProgress.Radius[0] * 2f, mask.bounds.size.x, TorchProgress.Radius[0] * 2f * 0.05f, "Işık yarıçapı Sv1 değil"); // ±%4 titreme
        SaveSystem.Data.torchLevel = 3;
        yield return null;
        Assert.AreEqual(TorchProgress.Radius[3] * 2f, mask.bounds.size.x, TorchProgress.Radius[3] * 2f * 0.05f, "Meşale seviyesi ışığı büyütmüyor");

        // İpuçları karanlığın üstünde: taş parıltısı, fırlatıcı gözleri, eşya
        var rockData = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Rock_ProjectileData.asset");
        var p = Projectile.Launch(rockData.projectilePrefab != null ? rockData.projectilePrefab : UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab"),
                                  rockData, ph.transform.position + new Vector3(2f, 2f, 0f), Vector2.left, 0.1f);
        var glint = p.transform.Find("Glint").GetComponent<SpriteRenderer>();
        Assert.IsTrue(glint.enabled);
        Assert.Greater(glint.sortingOrder, DarkWorld.DarkOrder, "Taş parıltısı karanlığın altında");
        var eyes = Object.FindAnyObjectByType<ShooterEyes>();
        Assert.IsNotNull(eyes, "Fırlatıcı gözleri yok");
        Assert.Greater(Object.FindAnyObjectByType<PowerupSpawner>().powerupSortingOrder, DarkWorld.DarkOrder, "Eşyalar karanlıkta görünmez");
    }

    [UnityTest]
    public IEnumerator Karanlik_SonrakiBolumeTasinmaz()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        DarkWorld.Apply(DarkTheme());
        Assert.IsTrue(Projectile.DarkMode);
        yield return KakTestUtil.LoadGameWithLevel(); // normal bölüm yeniden
        Assert.IsFalse(DarkWorld.Active, "Karanlık bayrağı sonraki bölümde kaldı");
        Assert.IsFalse(Projectile.DarkMode);
        Assert.IsNull(Object.FindAnyObjectByType<DarknessOverlay>());
    }

    [UnityTest]
    public IEnumerator IkiKisilik_IkiMesale()
    {
        GameSettings.TwoPlayer = true;
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        DarkWorld.Apply(DarkTheme());
        yield return null;
        foreach (var ph in PlayerRegistry.All) Assert.IsNotNull(ph.GetComponent<PlayerLight>(), ph.name + " meşalesiz");
    }

    [Test]
    public void Mesale_AltinlaYukselir()
    {
        KakTestUtil.ResetWorld();
        SaveSystem.Data.torchLevel = 0;
        SaveSystem.Data.coins = 100;
        Assert.IsFalse(TorchProgress.TryUpgrade(), "Altın yetmezken yükseldi");
        SaveSystem.Data.coins = TorchProgress.Costs[0] + 10;
        Assert.IsTrue(TorchProgress.TryUpgrade());
        Assert.AreEqual(1, TorchProgress.Level);
        Assert.AreEqual(10, SaveSystem.Data.coins);
        Assert.Greater(TorchProgress.Radius[1], TorchProgress.Radius[0]);
        SaveSystem.Data.torchLevel = TorchProgress.MaxLevel;
        SaveSystem.Data.coins = 99999;
        Assert.AreEqual(-1, TorchProgress.NextCost);
        Assert.IsFalse(TorchProgress.TryUpgrade(), "En üst seviyeden yükseldi");
    }
}
