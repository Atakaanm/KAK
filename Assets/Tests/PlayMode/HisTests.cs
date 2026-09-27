using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 11 G1: telefonda bulunan his hataları (ölümde kayma, çapraz koşu titremesi, göktaşı uyarısı, altın dağılımı, dash, kontrol boyutu).</summary>
public class HisTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { PlayerHealth.DevGodMode = false; KakTestUtil.ResetWorld(); yield return null; }

    static void StopShooters()
    {
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
    }

    [UnityTest]
    public IEnumerator Olunce_KarakterKipirdamaz()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        StopShooters();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        var move = ph.GetComponent<PlayerMovement2D>();
        move.InputOverride = Vector2.right;
        yield return KakTestUtil.WaitReal(0.3f);
        KakTestUtil.KillPlayer();
        Vector3 p0 = ph.transform.position;
        yield return KakTestUtil.WaitReal(0.6f); // ölüm yavaş çekimi sırasında
        Assert.Less(Vector3.Distance(p0, ph.transform.position), 0.02f, "Ölünce karakter hâlâ kayıyor");
        Assert.IsTrue(move.Frozen);
        move.InputOverride = null;
    }

    [UnityTest]
    public IEnumerator Capraz_HerYondeAyniHiz()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        StopShooters();
        KakTestUtil.MakePlayerSafe();
        var move = Object.FindAnyObjectByType<PlayerMovement2D>();
        var rb = move.GetComponent<Rigidbody2D>();
        move.InputOverride = new Vector2(1f, 0f);
        yield return KakTestUtil.WaitReal(0.2f);
        float straight = rb.linearVelocity.magnitude;
        move.InputOverride = new Vector2(1f, 1f).normalized;
        yield return KakTestUtil.WaitReal(0.2f);
        float diag = rb.linearVelocity.magnitude;
        move.InputOverride = null;
        Assert.Greater(straight, 0.5f);
        Assert.AreEqual(straight, diag, straight * 0.02f, "Çapraz koşu düz koşudan hızlı");
    }

    [UnityTest]
    public IEnumerator Goktasi_UyariIsaretiGosterir_InisteGizlenir()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        StopShooters();
        KakTestUtil.MakePlayerSafe();
        var meteor = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Projectiles/Meteor_Goktasi.asset");
        Assert.IsNotNull(meteor.warningSprite, "Göktaşına uyarı işareti bağlı değil");
        var prefab = meteor.projectilePrefab != null ? meteor.projectilePrefab
            : UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        var p = Projectile.LaunchMeteor(prefab, meteor, new Vector3(1f, 1f, 0f));
        Assert.IsNotNull(p);
        yield return null;
        var warn = p.transform.Find("Warning");
        Assert.IsNotNull(warn, "Uyarı işareti oluşmadı");
        var sr = warn.GetComponent<SpriteRenderer>();
        Assert.IsTrue(sr.enabled);
        Assert.AreEqual(meteor.warningSprite, sr.sprite);
        yield return KakTestUtil.WaitUntil(() => !p.gameObject.activeSelf, meteor.meteorFallTime + 2f, "göktaşı inmedi");
        Assert.IsFalse(sr.enabled, "Havuza dönen göktaşında uyarı açık kaldı");
    }

    [UnityTest]
    public IEnumerator Altinlar_TekTek_VeAraliKli()
    {
        SaveSystem.Data.gamesPlayed = 1;
        yield return KakTestUtil.LoadGameWithLevel();
        StopShooters();
        KakTestUtil.MakePlayerSafe();
        var cs = Object.FindAnyObjectByType<CoinSpawner>();
        cs.enabled = false;
        int n = 0;
        for (int i = 0; i < 4; i++) n += cs.SpawnCoin();
        yield return null;
        Assert.GreaterOrEqual(n, 2, "yer bulunamadı");
        Assert.AreEqual(n, Coin.Active.Count, "Tek çağrıda birden fazla altın doğdu");
        for (int i = 0; i < Coin.Active.Count; i++)
            for (int j = i + 1; j < Coin.Active.Count; j++)
                Assert.GreaterOrEqual(Vector2.Distance(Coin.Active[i].transform.position, Coin.Active[j].transform.position),
                                      cs.minCoinSpacing - 0.001f, "Altınlar birbirine çok yakın");
    }

    [UnityTest]
    public IEnumerator Dash_Rafta_DugmeGizli()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var dash = Object.FindAnyObjectByType<PlayerDash>();
        Assert.IsFalse(dash.available);
        var btn = Object.FindAnyObjectByType<DashButton>(FindObjectsInactive.Include);
        Assert.IsNotNull(btn);
        Assert.IsFalse(btn.gameObject.activeInHierarchy, "Dash düğmesi görünüyor");
    }

    [UnityTest]
    public IEnumerator KontrolBoyutu_JoystickeUygulanir()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var joy = Object.FindAnyObjectByType<VirtualJoystick>();
        Assert.IsNotNull(joy);
        ControlSettings.Set(2);
        Assert.AreEqual(1.25f, joy.background.localScale.x, 0.001f);
        ControlSettings.Set(0);
        Assert.AreEqual(0.8f, joy.background.localScale.x, 0.001f);
        ControlSettings.Set(1);
        Assert.AreEqual(1f, joy.background.localScale.x, 0.001f);
    }

    [UnityTest]
    public IEnumerator Zorluk_SureyeBagli_SkoraDegil()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        StopShooters();
        KakTestUtil.MakePlayerSafe();
        var sm = GameManager.Instance.scoreManager;
        var dm = DifficultyManager.Instance;
        sm.AddScore(50000); // G2: skor (yakın geçiş çarpanı) zorluğu hızlandırmamalı
        yield return null; yield return null;
        Assert.AreEqual(0, dm.CurrentStageIndex, "Skor kademeyi ilerletti");
        sm.AdvanceTime(80f); // 75 sn = Orta
        yield return null; yield return null;
        Assert.AreEqual(2, dm.CurrentStageIndex, "Süre kademeyi ilerletmedi");
    }

    [UnityTest]
    public IEnumerator TekCanlaBaslar_DoluykenKalp_BuOyunlukBuyutur()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        StopShooters();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        Assert.AreEqual(1, ph.MaxHealth, "G3: yükseltmesiz Ata tek canla başlamalı");
        Assert.AreEqual(3, ph.HealthCap);
        var heart = UnityEditor.AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/HeartData.asset");
        PowerupPickup.Apply(heart, ph.gameObject, ph.transform.position);
        Assert.AreEqual(2, ph.MaxHealth, "Dolu canla kalp +1 kalp vermedi");
        Assert.AreEqual(2, ph.CurrentHealth);
        PowerupPickup.Apply(heart, ph.gameObject, ph.transform.position);
        PowerupPickup.Apply(heart, ph.gameObject, ph.transform.position);
        Assert.AreEqual(3, ph.MaxHealth, "Üst sınırı aştı");
    }

    [UnityTest]
    public IEnumerator Pranga_Yavaslatir_HiziIptalEder_KirmiziGosterilir()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        StopShooters();
        KakTestUtil.MakePlayerSafe();
        var move = Object.FindAnyObjectByType<PlayerMovement2D>();
        var speed = UnityEditor.AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/SpeedData.asset");
        var shackle = UnityEditor.AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/ShackleData.asset");
        Assert.IsTrue(shackle.harmful);
        Assert.GreaterOrEqual(shackle.minStage, 1, "Pranga ısınma kademesinde çıkmamalı");
        float normal = move.CurrentSpeed;
        PowerupPickup.Apply(speed, move.gameObject, move.transform.position);
        PowerupPickup.Apply(shackle, move.gameObject, move.transform.position); // hız iptal, yavaşlar
        yield return null;
        Assert.AreEqual(normal * shackle.powerMultiplier, move.CurrentSpeed, 0.01f, "Pranga yavaşlatmadı / hızı iptal etmedi");
        var hud = Object.FindAnyObjectByType<PowerupHud>();
        Assert.AreEqual(1, hud.ActiveCount, "Hız ve Pranga çipi aynı anda görünüyor");
    }
}
