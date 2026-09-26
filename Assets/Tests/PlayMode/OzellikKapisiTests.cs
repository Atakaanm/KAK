using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 3c.2: menüde kilitli/yeni özellik butonu, cüzdan, oyun sonunda "yeni açıldı" afişi.</summary>
public class OzellikKapisiTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator Menu_KarakterButonu_KilitliSonraYeni()
    {
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        var fb = menu.charactersFeature;
        Assert.IsNotNull(fb, "Karakter butonunda FeatureButton yok");
        Assert.IsTrue(fb.Locked);
        Assert.IsTrue(fb.lockHint.gameObject.activeSelf, "kilit ipucu görünmüyor");
        StringAssert.Contains(FeatureGate.CharactersGames.ToString(), fb.lockHint.text);
        menu.OnCharactersClicked();
        yield return null;
        Assert.IsFalse(menu.charactersPanel.activeSelf, "Kilitliyken karakter paneli açıldı");

        SaveSystem.Data.gamesPlayed = FeatureGate.CharactersGames;
        fb.Refresh();
        Assert.IsFalse(fb.Locked);
        Assert.IsTrue(fb.newBadge.gameObject.activeSelf, "YENİ! rozeti görünmüyor");
        menu.OnCharactersClicked();
        yield return KakTestUtil.WaitReal(0.3f);
        Assert.IsTrue(menu.charactersPanel.activeSelf, "Açıkken karakter paneli açılmadı");
        Assert.IsFalse(fb.newBadge.gameObject.activeSelf, "Tanıtıldıktan sonra rozet kalktı mı");
        Assert.IsTrue(FeatureGate.Introduced(Feature.Characters));
    }

    [UnityTest]
    public IEnumerator Menu_Cuzdan_AltinAcilincaGorunur()
    {
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        var w = Object.FindAnyObjectByType<WalletHud>();
        Assert.IsNotNull(w, "Menüde cüzdan yok");
        Assert.IsFalse(w.content.gameObject.activeSelf, "Altın kilitliyken cüzdan görünmemeli");
        SaveSystem.Data.gamesPlayed = 1;
        SaveSystem.Data.coins = 42;
        w.Refresh();
        Assert.IsTrue(w.content.gameObject.activeSelf);
        Assert.AreEqual("42", w.amount.text);
    }

    [UnityTest]
    public IEnumerator OyunSonu_YeniAcilanOzellik_AfisGosterilir()
    {
        SaveSystem.Data.gamesPlayed = FeatureGate.CharactersGames - 1; // bu oyunla karakterler açılacak
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance.IsGameOver, 3f, "oyun bitmedi");
        Assert.AreEqual((int)Feature.Characters, GameManager.Instance.NewlyUnlocked);
        var gos = Object.FindAnyObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        yield return KakTestUtil.WaitUntil(() => gos.unlockBanner.gameObject.activeSelf, 6f, "yeni açılan özellik afişi görünmedi");
        StringAssert.Contains(FeatureGate.Name(Feature.Characters), gos.unlockText.text);
    }
}
