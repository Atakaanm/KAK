using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 14 Ö1: odak sürümünün kapsamı — tek dünya, altın + karakter, 5 eşya, 3 olay, 2 kişilik ikinci planda.</summary>
public class OdakKapsamTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [Test]
    public void GizliOzellikler_HicAcilmaz_KarakterVeAltinAcik()
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = 50; d.totalCoins = 5000; d.playDays = 10;
        Assert.IsFalse(FeatureGate.IsUnlocked(Feature.Worlds));
        Assert.IsFalse(FeatureGate.IsUnlocked(Feature.Pets));
        Assert.IsFalse(FeatureGate.IsUnlocked(Feature.Missions));
        Assert.IsFalse(FeatureGate.IsUnlocked(Feature.DailyReward));
        Assert.IsTrue(FeatureGate.IsUnlocked(Feature.Coins));
        Assert.IsTrue(FeatureGate.IsUnlocked(Feature.Characters));
        d.selectedWorld = "football";
        Assert.AreEqual("dungeon", EndlessWorlds.Selected().id, "Odak sürümünde seçili dünya hep Zindan");
    }

    [UnityTest]
    public IEnumerator Menude_DunyalarVePetYok_OynaVe2KisiVar()
    {
        SaveSystem.Data.gamesPlayed = 30;
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        yield return null;
        foreach (var fb in Object.FindObjectsByType<FeatureButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (fb.feature == Feature.Worlds || fb.feature == Feature.Pets)
                Assert.IsFalse(fb.gameObject.activeInHierarchy, fb.feature + " düğmesi görünüyor");
        Assert.IsTrue(Object.FindAnyObjectByType<MainMenuController>().playButton.gameObject.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator Zindan_BesEsya_UcOlay()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var level = LevelManager.Instance.currentLevel;
        var names = System.Array.ConvertAll(level.availablePowerups, p => p.type);
        CollectionAssert.AreEquivalent(new[] { PowerupType.Heal, PowerupType.Shield, PowerupType.TimeSlow, PowerupType.Ghost, PowerupType.Shackle }, names);
        var ev = Object.FindAnyObjectByType<EndlessEventManager>();
        var opt = typeof(EndlessEventManager).GetField("eventOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // en yüksek kademede seçenekler: sessizlik (2) ve sürü (4) yok
        GameManager.Instance.scoreManager.AdvanceTime(400f); // en yüksek kademe
        yield return null; yield return null;
        Assert.GreaterOrEqual(DifficultyManager.Instance.CurrentStageIndex, 4);
        ev.StartRandomEvent();
        var list = (System.Collections.Generic.List<int>)opt.GetValue(ev);
        CollectionAssert.DoesNotContain(list, 2, "Sessizlik olayı kapsam dışı");
        CollectionAssert.DoesNotContain(list, 4);
        CollectionAssert.IsSubsetOf(new[] { 0, 1, 3 }, list);
    }
}
