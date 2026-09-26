using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 3c.3: karakter satın alma/seçme ve istatistiklerin oyunda uygulanması.</summary>
public class KarakterTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { PlayerHealth.DevGodMode = false; KakTestUtil.ResetWorld(); yield return null; }

    static PlayerData Char(string id) => CharacterCatalog.Load().Find(id);

    static string HeartNames()
    {
        var ui = Object.FindAnyObjectByType<HealthUI>();
        return ui.name + ": " + string.Join(", ", ui.GetComponentsInChildren<UnityEngine.UI.Image>(false).Select(i => i.name + (i.transform.parent != null ? "<" + i.transform.parent.name : "")));
    }

    static int VisibleHearts()
    {
        var ui = Object.FindAnyObjectByType<HealthUI>();
        return ui.GetComponentsInChildren<UnityEngine.UI.Image>(false).Count(i => i.name.Contains("Heart"));
    }

    [Test]
    public void Katalog_DortKarakter_IlkBedava()
    {
        var cat = CharacterCatalog.Load();
        Assert.IsNotNull(cat, "Resources/CharacterCatalog yok");
        Assert.AreEqual(4, cat.characters.Length);
        Assert.AreEqual(0, cat.characters[0].unlockPrice, "Başlangıç karakteri bedava olmalı");
        foreach (var c in cat.characters) Assert.IsNotNull(c.Portrait, c.id + " portresi yok");
    }

    [UnityTest]
    public IEnumerator SatinAl_Sec_OyundaIstatistiklerUygulanir()
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = FeatureGate.CharactersGames;
        d.coins = 400;
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        menu.OnCharactersClicked();
        yield return KakTestUtil.WaitReal(0.3f);
        var panel = Object.FindAnyObjectByType<CharacterPanel>();
        Assert.IsNotNull(panel, "Karakter paneli açılmadı");
        panel.OnCardAction(1); // Çevik 300
        Assert.AreEqual(100, d.coins, "Altın düşülmedi");
        Assert.AreEqual("Swift", d.selectedCharacter);
        Assert.IsTrue(CharacterCatalog.Owned(Char("Swift")));

        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        var sw = Char("Swift");
        Assert.AreEqual(sw.maxHealth, ph.maxHealth);
        Assert.AreEqual(sw.maxHealth, ph.currentHealth);
        Assert.AreEqual(sw.maxHealth, VisibleHearts(), "HUD'da karakterin can sayısı kadar kalp olmalı");
        Assert.AreEqual(sw.dashCooldown, ph.GetComponent<PlayerDash>().cooldown, 0.001f);
        var hb = ph.GetComponent<PlayerHitbox>();
        Assert.AreEqual(0.36f * sw.hurtboxScale, hb.hurtSize.x, 0.001f, "Gövde küçülmedi");
        Assert.AreEqual(sw.moveSpeed, ph.GetComponent<PlayerMovement2D>().playerData.moveSpeed, 0.001f);
        var vis = Object.FindAnyObjectByType<PlayerDirectionSprite>();
        Assert.AreSame(sw.south.idle, vis.south.idle, "Görsel değişmedi");
    }

    [UnityTest]
    public IEnumerator AltinYetmezse_SatinAlinmaz()
    {
        SaveSystem.Data.coins = 100;
        Assert.IsFalse(CharacterCatalog.TryBuy(Char("Tank")));
        Assert.AreEqual(100, SaveSystem.Data.coins);
        Assert.IsFalse(CharacterCatalog.Owned(Char("Tank")));
        // Sahip olunmayan karakter seçilemez, seçim bozulmaz
        Assert.IsFalse(CharacterCatalog.Select(Char("Tank")));
        Assert.AreEqual("Boy", CharacterCatalog.Selected().id);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Tank_DortCan_KalkanlaBaslar()
    {
        SaveSystem.Data.unlockedCharacters.Add("Tank");
        SaveSystem.Data.selectedCharacter = "Tank";
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        Assert.AreEqual(4, ph.currentHealth);
        Assert.AreEqual(4, VisibleHearts(), HeartNames());
        Assert.IsTrue(ph.HasShield, "Tank kalkanla başlamadı");
    }

    [UnityTest]
    public IEnumerator Sansli_AltinCarpani()
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = 1;
        d.unlockedCharacters.Add("Lucky");
        d.selectedCharacter = "Lucky";
        yield return KakTestUtil.LoadGameWithLevel();
        var sm = GameManager.Instance.scoreManager;
        sm.AddCoins(4, Vector3.zero);
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance.IsGameOver, 3f, "oyun bitmedi");
        Assert.AreEqual(5, GameManager.Instance.RunCoins, "4 altın × 1,25 = 5 olmalı");
    }

    [UnityTest]
    public IEnumerator HasarAlinca_GorunenKalpAzalir()
    {
        // Eski hata: HealthUI yalnızca Heart1'i yönetiyordu; klonlar Heart2/3'ün üstüne biniyor,
        // can kaybında alttaki kalpler görünür kalıyordu.
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        Assert.AreEqual(3, ShownHearts(), HeartNames());
        ph.TakeDamage(1);
        yield return KakTestUtil.WaitReal(1f);
        Assert.AreEqual(2, ShownHearts(), "Hasardan sonra görünen kalp 2 olmalı: " + HeartNames());
    }

    /// <summary>Gerçekten görünen kalpler (aktif, ölçeği ve saydamlığı yerinde).</summary>
    static int ShownHearts()
    {
        var ui = Object.FindAnyObjectByType<HealthUI>();
        return ui.GetComponentsInChildren<UnityEngine.UI.Image>(false)
                 .Count(i => i.name.Contains("Heart") && i.transform.localScale.x > 0.1f && i.color.a > 0.1f);
    }
}
