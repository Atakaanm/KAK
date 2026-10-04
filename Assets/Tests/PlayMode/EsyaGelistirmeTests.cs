using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 15 K4/K5: GELİŞİM/EŞYA ekranı (yükselt, yetmezse olmaz, kilitli kalabalık yok) ve eşya geliştirmelerinin oyundaki etkisi.</summary>
public class EsyaGelistirmeTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    static PowerupData Pu(string n) => UnityEditor.AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/" + n + ".asset");

    static void Quiet()
    {
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        if (EndlessEventManager.Instance != null) EndlessEventManager.Instance.enabled = false;
    }

    static ProgressPanel Page(MainMenuController menu, int tab)
    {
        menu.hubTabs.Show(tab);
        return menu.hubTabs.pages[tab].GetComponent<ProgressPanel>();
    }

    [UnityTest]
    public IEnumerator Gelisim_Yukselt_YetmezseOlmaz_KilitliKalabalikYok()
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = FeatureGate.CharactersGames;
        d.coins = 100;
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        Assert.IsNotNull(menu.hubTabs, "GELİŞİM merkezi kurulmamış");
        menu.OnCharactersClicked();
        yield return null;
        var pp = Page(menu, 1);
        yield return null;
        var hearts = pp.RowOf("hearts");
        Assert.IsNotNull(hearts, "Can satırı yok");
        Assert.IsTrue(hearts.gameObject.activeSelf);

        int locked = 0;
        var cat = UpgradeCatalog.Load();
        foreach (var t in cat.tracks)
        {
            if (t is ItemTrackData) continue;
            var r = pp.RowOf(t.id);
            if (r != null && r.gameObject.activeSelf && !Progression.Unlocked(t)) locked++;
        }
        Assert.LessOrEqual(locked, pp.lockedPreview, "Kilitli satırlar ekranı dolduruyor");

        pp.OnRowClicked(hearts);
        Assert.AreEqual(1, Progression.Level("hearts"), "Yükseltme olmadı");
        Assert.AreEqual(100 - 60, d.coins, "Altın düşülmedi");
        pp.OnRowClicked(hearts); // 250 altın: yetmez
        Assert.AreEqual(1, Progression.Level("hearts"), "Altın yetmezken yükseldi");
        yield return KakTestUtil.WaitReal(0.4f);
    }

    [UnityTest]
    public IEnumerator Esya_ParsomenOlmadanUstSeviyeOlmaz()
    {
        var d = SaveSystem.Data;
        d.coins = 100000;
        d.gamesPlayed = FeatureGate.CharactersGames;
        Progression.SetLevel("hearts", 3); // eşya izleri açılsın (toplam gelişim)
        var t = Progression.Track("it_shield_freq") as ItemTrackData;
        Assert.IsNotNull(t);
        Progression.SetLevel(t.id, 3); // sıradaki seviye parşömen ister
        Assert.Greater(Progression.ScrollCost(t), 0);
        d.scrolls = 0;
        Assert.IsFalse(Progression.TryUpgrade(t), "Parşömensiz üst seviye alındı");
        d.scrolls = 5;
        int need = Progression.ScrollCost(t);
        Assert.IsTrue(Progression.TryUpgrade(t));
        Assert.AreEqual(5 - need, d.scrolls, "Parşömen düşülmedi");
        yield return null;
    }

    [UnityTest]
    public IEnumerator GucluKalkan_IkiVurusEmer_SiklikVeYerdeKalmaArtar()
    {
        Progression.SetLevel("it_shield_pow", 1);
        Progression.SetLevel("it_shield_freq", 2);
        Progression.SetLevel("it_shield_ground", 2);
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var ph = PlayerStats.Primary.GetComponent<PlayerHealth>();
        int hp = ph.CurrentHealth;
        PowerupPickup.Apply(Pu("ShieldData"), ph.gameObject, ph.transform.position);
        Assert.IsTrue(ph.HasShield);
        ph.SetInvulnerable(0f);
        typeof(PlayerHealth).GetField("invulnerableUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, -1f);
        typeof(PlayerHealth).GetField("isInvincible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, false);
        ph.TakeDamage(1);
        Assert.IsTrue(ph.HasShield, "Güçlü kalkan ilk vuruşta gitti");
        typeof(PlayerHealth).GetField("isInvincible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, false);
        ph.TakeDamage(1);
        Assert.IsFalse(ph.HasShield, "Kalkan iki vuruştan sonra da duruyor");
        Assert.AreEqual(hp, ph.CurrentHealth, "Kalkan varken can gitti");

        Assert.AreEqual(1f + 2 * 0.15f, ItemProgress.FrequencyMult(PowerupType.Shield), 0.001f);
        Assert.AreEqual(1f + 2 * 0.20f, ItemProgress.GroundMult(PowerupType.Shield), 0.001f);
        Assert.AreEqual(1f, ItemProgress.FrequencyMult(PowerupType.Heal), 0.001f, "Başka eşyaya sızdı");
    }

    [UnityTest]
    public IEnumerator PrangaDirenci_SureyiKisaltir_SuresiGelisenYavasCekimUzar()
    {
        Progression.SetLevel("it_shackle_res", 2);
        Progression.SetLevel("it_slow_dur", 3);
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        var st = PlayerStats.Primary;
        Assert.AreEqual(0.3f, st.ShackleResist, 0.001f, "Pranga direnci özellik sayfasına gelmedi");
        float got = -1f;
        System.Action<PowerupData, float> h = (pd, sec) => { if (pd.type == PowerupType.TimeSlow) got = sec; };
        GameEvents.PowerupActivated += h;
        var slow = Pu("SloMoData");
        PowerupPickup.Apply(slow, st.gameObject, st.transform.position);
        GameEvents.PowerupActivated -= h;
        Assert.AreEqual(slow.duration * st.PowerDuration * (1f + 3 * 0.12f), got, 0.01f, "Yavaş çekim süresi geliştirmeyle uzamadı");
    }
}
