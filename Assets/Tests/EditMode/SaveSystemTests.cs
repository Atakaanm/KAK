using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>Kayıt sistemi: gidiş-dönüş, bozuk dosya, PlayerPrefs taşıma.</summary>
public class SaveSystemTests
{
    string path;

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Application.temporaryCachePath, "kak_test_save.json");
        if (File.Exists(path)) File.Delete(path);
        SaveSystem.OverridePath = path;
        SaveSystem.Unload();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(path)) File.Delete(path);
        SaveSystem.OverridePath = null;
        SaveSystem.Unload();
    }

    [Test]
    public void KaydetYukle_DegerlerKorunur()
    {
        SaveSystem.Data.bestScoreEndless = 1234;
        SaveSystem.Data.coins = 55;
        SaveSystem.Data.settings.music = false;
        SaveSystem.Data.unlockedCharacters.Add("Girl");
        SaveSystem.Save();
        SaveSystem.Unload();

        Assert.AreEqual(1234, SaveSystem.Data.bestScoreEndless);
        Assert.AreEqual(55, SaveSystem.Data.coins);
        Assert.IsFalse(SaveSystem.Data.settings.music);
        CollectionAssert.Contains(SaveSystem.Data.unlockedCharacters, "Girl");
    }

    [Test]
    public void BozukDosya_YeniKayitAcar()
    {
        File.WriteAllText(path, "{ bu json değil");
        LogAssertIgnore();
        Assert.IsNotNull(SaveSystem.Data);
        Assert.AreEqual(SaveSystem.CurrentVersion, SaveSystem.Data.version);
    }

    [Test]
    public void IlkAcilis_PlayerPrefsTasinir()
    {
        int oldBest = PlayerPrefs.GetInt("BestScore", 0);
        int oldMusic = PlayerPrefs.GetInt("MusicOn", 1);
        try
        {
            PlayerPrefs.SetInt("BestScore", 4775);
            PlayerPrefs.SetInt("MusicOn", 0);
            Assert.AreEqual(4775, SaveSystem.Data.bestScoreEndless, "Eski rekor taşınmadı");
            Assert.IsFalse(SaveSystem.Data.settings.music, "Eski müzik ayarı taşınmadı");
            Assert.IsTrue(File.Exists(path), "Taşıma sonrası kayıt dosyası yazılmadı");
        }
        finally
        {
            PlayerPrefs.SetInt("BestScore", oldBest);
            PlayerPrefs.SetInt("MusicOn", oldMusic);
        }
    }

    static void LogAssertIgnore() => UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
}
