using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>Faz 11 G3: adım adım güçlenme — kalıcı karakter ve pet yükseltmeleri.</summary>
public class IlerlemeTests
{
    string path;
    PlayerData ata;

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Application.temporaryCachePath, "kak_ilerleme_test.json");
        if (File.Exists(path)) File.Delete(path);
        SaveSystem.OverridePath = path;
        SaveSystem.Unload();
        ata = ScriptableObject.CreateInstance<PlayerData>();
        ata.id = "Boy"; ata.maxHealth = 3; ata.startHealth = 1; ata.unlockPrice = 0;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(ata);
        SaveSystem.OverridePath = null; SaveSystem.Unload();
        if (File.Exists(path)) File.Delete(path);
    }

    [Test]
    public void TekCanlaBaslar_YukselttikceArtar_UstSinirdaDurur()
    {
        Assert.AreEqual(1, CharacterProgress.Hearts(ata));
        SaveSystem.Data.coins = 10000;
        Assert.IsTrue(CharacterProgress.TryUpgrade(ata, CharStat.Health));
        Assert.AreEqual(2, CharacterProgress.Hearts(ata));
        Assert.IsTrue(CharacterProgress.TryUpgrade(ata, CharStat.Health));
        Assert.AreEqual(3, CharacterProgress.Hearts(ata));
        Assert.AreEqual(-1, CharacterProgress.Cost(ata, CharStat.Health), "üst sınırda fiyat -1 olmalı");
        Assert.IsFalse(CharacterProgress.TryUpgrade(ata, CharStat.Health));
        Assert.AreEqual(3, CharacterProgress.Hearts(ata));
    }

    [Test]
    public void Yukseltme_AltinDuser_YetmezseOlmaz_Kaydedilir()
    {
        int c1 = CharacterProgress.Cost(ata, CharStat.Speed);
        SaveSystem.Data.coins = c1 - 1;
        Assert.IsFalse(CharacterProgress.TryUpgrade(ata, CharStat.Speed), "altın yetmezken yükseldi");
        SaveSystem.Data.coins = c1 + 5;
        Assert.IsTrue(CharacterProgress.TryUpgrade(ata, CharStat.Speed));
        Assert.AreEqual(5, SaveSystem.Data.coins);
        Assert.AreEqual(1f + CharacterProgress.SpeedPerLevel, CharacterProgress.SpeedMult(ata), 1e-5f);
        Assert.Greater(CharacterProgress.Cost(ata, CharStat.Speed), c1, "sonraki seviye daha pahalı olmalı");
        SaveSystem.Unload(); // diskten tekrar oku
        Assert.AreEqual(1, CharacterProgress.Level(ata, CharStat.Speed), "yükseltme kaydedilmedi");
    }

    [Test]
    public void SahipOlunmayanKarakter_Yukseltilemez()
    {
        var tank = ScriptableObject.CreateInstance<PlayerData>();
        tank.id = "Tank"; tank.maxHealth = 5; tank.startHealth = 2; tank.unlockPrice = 800; tank.isLocked = true;
        SaveSystem.Data.coins = 10000;
        Assert.AreEqual(2, CharacterProgress.Hearts(tank));
        Assert.IsFalse(CharacterProgress.TryUpgrade(tank, CharStat.Health));
        Object.DestroyImmediate(tank);
    }

    [Test]
    public void Pet_SahipOlununca_SeviyeAtlar()
    {
        var pet = ScriptableObject.CreateInstance<PetData>();
        pet.id = "Turtle"; pet.price = 100;
        SaveSystem.Data.coins = 10000;
        Assert.IsFalse(CharacterProgress.TryUpgradePet(pet), "sahip olunmayan pet yükseldi");
        SaveSystem.Data.unlockedPets.Add("Turtle");
        Assert.IsTrue(CharacterProgress.TryUpgradePet(pet));
        Assert.AreEqual(1, CharacterProgress.PetLevelOf("Turtle"));
        for (int i = 0; i < 5; i++) CharacterProgress.TryUpgradePet(pet);
        Assert.AreEqual(CharacterProgress.PetLevels, CharacterProgress.PetLevelOf("Turtle"), "üst sınırı aştı");
        Object.DestroyImmediate(pet);
    }
}
