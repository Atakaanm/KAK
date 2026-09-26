using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>Faz 3c.6: günlük ödül serisi.</summary>
public class DailyRewardTests
{
    string path;

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Application.temporaryCachePath, "kak_daily_test.json");
        if (File.Exists(path)) File.Delete(path);
        SaveSystem.OverridePath = path;
        SaveSystem.Unload();
        SaveSystem.Data.playDays = FeatureGate.DailyDays; // açık
    }

    [TearDown]
    public void TearDown() { DailyReward.TodayOverride = -1; SaveSystem.OverridePath = null; SaveSystem.Unload(); if (File.Exists(path)) File.Delete(path); }

    [Test]
    public void KilitliyseAlinamaz()
    {
        SaveSystem.Data.playDays = 1;
        DailyReward.TodayOverride = 100;
        Assert.IsFalse(DailyReward.CanClaim());
        Assert.AreEqual(0, DailyReward.Claim());
    }

    [Test]
    public void ArdisikGunler_SeriIlerler_GunlukBirKez()
    {
        DailyReward.TodayOverride = 100;
        Assert.AreEqual(DailyReward.Rewards[0], DailyReward.Claim());
        Assert.IsFalse(DailyReward.CanClaim(), "Aynı gün ikinci kez alındı");
        DailyReward.TodayOverride = 101;
        Assert.AreEqual(2, DailyReward.NextDay);
        Assert.AreEqual(DailyReward.Rewards[1], DailyReward.Claim());
        Assert.AreEqual(DailyReward.Rewards[0] + DailyReward.Rewards[1], SaveSystem.Data.coins);
    }

    [Test]
    public void GunKacarsa_BasaDoner_YediGundenSonraDongu()
    {
        DailyReward.TodayOverride = 100; DailyReward.Claim();
        DailyReward.TodayOverride = 101; DailyReward.Claim();
        DailyReward.TodayOverride = 103; // 102 kaçtı
        Assert.AreEqual(1, DailyReward.NextDay, "Kaçırılan gün sonrası seri sıfırlanmadı");
        for (int day = 0; day < 7; day++) { DailyReward.TodayOverride = 200 + day; DailyReward.Claim(); }
        Assert.AreEqual(7, SaveSystem.Data.dailyStreak);
        DailyReward.TodayOverride = 207;
        Assert.AreEqual(1, DailyReward.NextDay, "7. günden sonra döngü başa dönmeli");
    }
}
