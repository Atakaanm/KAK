using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 3c.6: günlük ödül paneli menüde kendiliğinden açılır, ödül verilir.</summary>
public class GunlukOdulTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { DailyReward.TodayOverride = -1; KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator KilitliyseAcilmaz()
    {
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        yield return KakTestUtil.WaitReal(1f);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        Assert.IsFalse(menu.dailyPanel.gameObject.activeSelf, "Günlük ödül kilitliyken açıldı");
    }

    [UnityTest]
    public IEnumerator IkinciGun_KendiligindenAcilir_OdulVerilir()
    {
        SaveSystem.Data.playDays = FeatureGate.DailyDays;
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        yield return KakTestUtil.WaitUntil(() => menu.dailyPanel.gameObject.activeSelf, 3f, "Günlük ödül paneli açılmadı");
        int before = SaveSystem.Data.coins;
        menu.dailyPanel.OnClaim();
        Assert.AreEqual(before + DailyReward.Rewards[0], SaveSystem.Data.coins);
        Assert.IsFalse(DailyReward.CanClaim());
        menu.dailyPanel.OnClaim(); // KAPAT
        Assert.IsFalse(menu.dailyPanel.gameObject.activeSelf);
    }
}
