using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>Faz 3c.2: adım adım açılma kuralları.</summary>
public class FeatureGateTests
{
    string path;

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Application.temporaryCachePath, "kak_featuregate_test.json");
        if (File.Exists(path)) File.Delete(path);
        SaveSystem.OverridePath = path;
        SaveSystem.Unload();
    }

    [TearDown]
    public void TearDown() { SaveSystem.OverridePath = null; SaveSystem.Unload(); if (File.Exists(path)) File.Delete(path); }

    [Test]
    public void YeniOyuncu_HerSeyKilitli()
    {
        foreach (var f in FeatureGate.All) Assert.IsFalse(FeatureGate.IsUnlocked(f), f + " yeni oyuncuda açık");
        Assert.AreEqual(0, FeatureGate.UnlockedMask());
    }

    [Test]
    public void Takvim_OyunSayisiVeAltinlaAcilir()
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = 1;
        Assert.IsTrue(FeatureGate.IsUnlocked(Feature.Coins));
        Assert.IsFalse(FeatureGate.IsUnlocked(Feature.Missions));
        d.gamesPlayed = FeatureGate.MissionsGames;
        Assert.IsTrue(FeatureGate.IsUnlocked(Feature.Missions));
        Assert.IsFalse(FeatureGate.IsUnlocked(Feature.Characters));
        d.totalCoins = FeatureGate.CharactersCoins;          // altınla erken açılır
        Assert.IsTrue(FeatureGate.IsUnlocked(Feature.Characters));
        Assert.IsFalse(FeatureGate.IsUnlocked(Feature.Pets));
        d.unlockedCharacters.Add("Cevik");                    // ikinci karakter → pet'ler
        Assert.IsTrue(FeatureGate.IsUnlocked(Feature.Pets));
        d.playDays = FeatureGate.DailyDays;
        Assert.IsTrue(FeatureGate.IsUnlocked(Feature.DailyReward));
    }

    [Test]
    public void YeniRozeti_BirKezGosterilir()
    {
        SaveSystem.Data.gamesPlayed = 5;
        Assert.IsTrue(FeatureGate.IsNew(Feature.Characters));
        FeatureGate.MarkIntroduced(Feature.Characters);
        Assert.IsFalse(FeatureGate.IsNew(Feature.Characters));
        Assert.IsTrue(FeatureGate.IsUnlocked(Feature.Characters));
    }

    [Test]
    public void KilitIpucu_KalanOyunuSoyler()
    {
        SaveSystem.Data.gamesPlayed = 2;
        StringAssert.Contains("3", FeatureGate.LockedHint(Feature.Characters));
        Assert.AreEqual("", FeatureGate.LockedHint(Feature.Coins));
    }
}
