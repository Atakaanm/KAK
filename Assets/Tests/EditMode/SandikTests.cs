using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>Faz 15 K6: sandık açma — anahtar düşer, ödül aralıkta, garanti (her 10. açılış bir üst sandık), altın rekorla ölçeklenir.</summary>
public class SandikTests
{
    string path;

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Application.temporaryCachePath, "kak_sandik_test.json");
        if (File.Exists(path)) File.Delete(path);
        SaveSystem.OverridePath = path;
        SaveSystem.Unload();
        Assert.IsNotNull(EconomyData.Load(), "EconomyData yok: KacAtaKac/Sandıkları Kur");
    }

    [TearDown]
    public void TearDown()
    {
        SaveSystem.OverridePath = null; SaveSystem.Unload();
        if (File.Exists(path)) File.Delete(path);
    }

    [Test]
    public void AnahtarYetmezse_Acilmaz()
    {
        SaveSystem.Data.keys = 0;
        Assert.IsFalse(ChestSystem.CanOpen(0));
        Assert.IsFalse(ChestSystem.TryOpen(0, out _));
        Assert.AreEqual(0, SaveSystem.Data.chestsOpened);
    }

    [Test]
    public void AhsapSandik_AnahtarDuser_OdulAralikta()
    {
        var d = SaveSystem.Data;
        d.keys = 2; d.coins = 0; d.scrolls = 0;
        var def = ChestSystem.Def(0);
        Assert.IsTrue(ChestSystem.TryOpen(0, out var r, new System.Random(7)));
        Assert.AreEqual(2 - def.keyCost, d.keys - r.keys);
        Assert.IsFalse(r.pity);
        Assert.That(r.gold, Is.InRange(def.goldMin - 5, def.goldMax + 5), "Altın aralık dışında");
        Assert.AreEqual(r.gold, d.coins);
        Assert.That(r.scrolls, Is.InRange(0, def.scrollMax));
        Assert.AreEqual(r.scrolls, d.scrolls);
        Assert.AreEqual(1, d.chestsOpened);
    }

    [Test]
    public void OnuncuAcilis_BirUstSandiginOdulu()
    {
        var e = EconomyData.Load();
        var d = SaveSystem.Data;
        d.keys = 5;
        d.chestsOpened = e.pityEvery - 1;
        Assert.AreEqual(1, ChestSystem.UntilPity);
        Assert.IsTrue(ChestSystem.TryOpen(0, out var r, new System.Random(1)));
        Assert.IsTrue(r.pity, "Garanti açılışı olmadı");
        Assert.AreEqual(1, r.defIndex, "Bir üst sandığın ödülü verilmedi");
        Assert.GreaterOrEqual(r.gold, ChestSystem.Def(1).goldMin - 5);
        Assert.AreEqual(e.pityEvery, ChestSystem.UntilPity, "Sayaç yeniden başlamadı");
    }

    [Test]
    public void Altin_RekorlaOlceklenir()
    {
        var e = EconomyData.Load();
        SaveSystem.Data.bestScoreEndless = 0;
        Assert.AreEqual(1f, ChestSystem.GoldScale(), 1e-4f);
        SaveSystem.Data.bestScoreEndless = Mathf.RoundToInt(e.bestScoreForDouble * 0.5f);
        Assert.AreEqual(1.5f, ChestSystem.GoldScale(), 0.01f);
        SaveSystem.Data.bestScoreEndless = 999999;
        Assert.AreEqual(1f + e.bestScoreCap, ChestSystem.GoldScale(), 1e-4f, "Ölçek tavanı aşıldı");
    }
}
