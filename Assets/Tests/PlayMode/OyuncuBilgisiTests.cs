using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Denetim D4: oyuncu bilgisi — aktif güçlendirme göstergesi.</summary>
public class OyuncuBilgisiTests
{
    PlayerHealth ph;
    PowerupHud hud;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        KakTestUtil.ResetWorld();
        yield return KakTestUtil.LoadGameWithLevel();
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        var spawner = Object.FindAnyObjectByType<PowerupSpawner>();
        if (spawner != null) spawner.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        ph = Object.FindAnyObjectByType<PlayerHealth>();
        hud = Object.FindAnyObjectByType<PowerupHud>();
        Assert.IsNotNull(hud, "PowerupHud sahnede yok (Sonsuz Mod İçeriğini Kur)");
        yield return KakTestUtil.WaitReal(0.3f);
    }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    static PowerupData Data(string name)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/" + name + ".asset");
#else
        return null;
#endif
    }

    [UnityTest]
    public IEnumerator SureliGuclendirme_Gosterilir_SuresiBitinceKaybolur()
    {
        var d = Data("SpeedData");
        Assert.IsNotNull(d);
        PowerupPickup.Apply(d, ph.gameObject, ph.transform.position);
        yield return null;
        Assert.AreEqual(1, hud.ActiveCount, "Hız göstergesi çıkmadı");
        float r = hud.Remaining(PowerupType.SpeedBoost);
        Assert.Greater(r, d.duration * 0.5f, "Kalan süre yanlış");
        yield return KakTestUtil.WaitUntil(() => hud.ActiveCount == 0, d.duration * 2f + 1f, "Süre bitince gösterge kaybolmadı");
    }

    [UnityTest]
    public IEnumerator Kalkan_VurulanaKadarGosterilir()
    {
        PowerupPickup.Apply(Data("ShieldData"), ph.gameObject, ph.transform.position);
        yield return KakTestUtil.WaitReal(1f);
        Assert.AreEqual(1, hud.ActiveCount, "Kalkan göstergesi yok ya da süre dolunca kayboldu");
        ph.TakeDamage(1);
        yield return null;
        Assert.AreEqual(0, hud.ActiveCount, "Kalkan kırılınca gösterge kaybolmadı");
    }

    [UnityTest]
    public IEnumerator AyniGuclendirme_TekrarAlininca_SureYenilenir_CipCogalmaz()
    {
        var d = Data("GhostData");
        PowerupPickup.Apply(d, ph.gameObject, ph.transform.position);
        yield return KakTestUtil.WaitReal(1f);
        float before = hud.Remaining(PowerupType.Ghost);
        PowerupPickup.Apply(d, ph.gameObject, ph.transform.position);
        yield return null;
        Assert.AreEqual(1, hud.ActiveCount);
        Assert.Greater(hud.Remaining(PowerupType.Ghost), before + 0.5f, "Süre yenilenmedi");
    }

    [UnityTest]
    public IEnumerator IlkOyun_Ipuclari_HareketSonraDash_BirKezGosterilir()
    {
        var hints = Object.FindAnyObjectByType<OnboardingHints>(FindObjectsInactive.Include);
        Assert.IsNotNull(hints, "OnboardingHints sahnede yok");
        Assert.AreEqual(OnboardingHints.Move, hints.Current, "İlk oyunda hareket ipucu gösterilmedi");
        Assert.IsTrue(hints.hintText.enabled);

        var move = ph.GetComponent<PlayerMovement2D>();
        move.InputOverride = Vector2.left;
        yield return KakTestUtil.WaitUntil(() => SaveSystem.Data.HasSeen(OnboardingHints.Move), 4f, "hareket edince ipucu bitmedi");
        move.InputOverride = null;

        hints.dashHintAfter = 0.5f;
        float t0 = Time.realtimeSinceStartup;
        while (hints.Current != OnboardingHints.Dash && Time.realtimeSinceStartup - t0 < 4f) yield return null;
        Assert.AreEqual(OnboardingHints.Dash, hints.Current,
            $"dash ipucu çıkmadı: current={hints.Current ?? "null"} seenDash={SaveSystem.Data.HasSeen(OnboardingHints.Dash)} seenMove={SaveSystem.Data.HasSeen(OnboardingHints.Move)} btn={(hints.dashButton != null)} dash={(hints.dashButton != null && hints.dashButton.dash != null)} ready={(hints.dashButton != null && hints.dashButton.dash != null && hints.dashButton.dash.Ready)} gameOver={GameManager.Instance.IsGameOver} enabled={hints.isActiveAndEnabled}");
        Assert.IsTrue(hints.dashButton.highlight, "dash butonu vurgulanmadı");
        Assert.IsTrue(ph.GetComponent<PlayerDash>().TryDash());
        yield return null;
        Assert.IsTrue(SaveSystem.Data.HasSeen(OnboardingHints.Dash));
        Assert.IsNull(hints.Current);
        Assert.IsFalse(hints.dashButton.highlight);
    }

    [UnityTest]
    public IEnumerator YuksekRekorluOyuncuya_TemelIpucuGosterilmez()
    {
        SaveSystem.Data.gamesPlayed = 2;
        SaveSystem.Data.bestScoreEndless = 4775;
        yield return KakTestUtil.LoadGameWithLevel();
        var hints = Object.FindAnyObjectByType<OnboardingHints>(FindObjectsInactive.Include);
        yield return null;
        Assert.IsNull(hints.Current, "Rekoru yüksek oyuncuya hareket ipucu gösterildi");
    }

    [UnityTest]
    public IEnumerator DeneyimliOyuncuya_IpucuGosterilmez()
    {
        SaveSystem.Data.gamesPlayed = 10;
        yield return KakTestUtil.LoadGameWithLevel();
        var hints = Object.FindAnyObjectByType<OnboardingHints>(FindObjectsInactive.Include);
        yield return null;
        Assert.IsNull(hints.Current);
        Assert.IsFalse(hints.hintText.enabled);
    }
}
