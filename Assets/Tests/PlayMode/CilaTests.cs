using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 3c.7: yeni rekor konfetisi, sıradaki hedef çubuğu, menüde "alınabilir" işareti.</summary>
public class CilaTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator YeniRekor_KonfetiVe_HedefCubugu()
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = FeatureGate.CharactersGames; // karakterler açık → hedef: en ucuz karakter
        d.coins = 100;
        d.bestScoreEndless = 0;
        yield return KakTestUtil.LoadGameWithLevel();
        yield return KakTestUtil.WaitReal(1.2f); // skor > 0 → yeni rekor
        KakTestUtil.KillPlayer();
        var gos = Object.FindAnyObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        yield return KakTestUtil.WaitUntil(() => gos.confetti != null && gos.confetti.Running, 5f, "Yeni rekorda konfeti yok");
        yield return KakTestUtil.WaitUntil(() => gos.goalRow.gameObject.activeSelf, 5f, "Hedef satırı görünmedi");
        var goal = NextGoal.Find();
        Assert.IsTrue(goal.HasValue);
        StringAssert.Contains(goal.Value.price.ToString(), gos.goalText.text);
        Assert.Greater(gos.goalFill.fillAmount, 0f);
    }

    [UnityTest]
    public IEnumerator Menu_AlinabilirIsareti()
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = FeatureGate.CharactersGames;
        FeatureGate.MarkIntroduced(Feature.Characters); // YENİ! rozeti yerine nokta görünsün
        d.coins = 1000;
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        var fb = Object.FindAnyObjectByType<MainMenuController>().charactersFeature;
        Assert.IsTrue(fb.buyDot.gameObject.activeSelf, "Altın yetince alınabilir işareti görünmeli");
        d.coins = 0;
        fb.Refresh();
        Assert.IsFalse(fb.buyDot.gameObject.activeSelf);
    }
}
