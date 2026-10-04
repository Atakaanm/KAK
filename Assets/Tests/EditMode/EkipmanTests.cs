using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>Faz 15 K7: ekipman altyapısı — 3 aynı parça bir üst nadirlik, takma yuvası, özellikler yalnız kapsam açıkken.</summary>
public class EkipmanTests
{
    string path;

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Application.temporaryCachePath, "kak_ekipman_test.json");
        if (File.Exists(path)) File.Delete(path);
        SaveSystem.OverridePath = path;
        SaveSystem.Unload();
        KakScope.ResetToProduct();
        Assert.IsNotNull(EquipmentCatalog.Load(), "EquipmentCatalog yok: KacAtaKac/Ekipman Altyapısını Kur");
    }

    [TearDown]
    public void TearDown()
    {
        KakScope.ResetToProduct();
        SaveSystem.OverridePath = null; SaveSystem.Unload();
        if (File.Exists(path)) File.Delete(path);
    }

    [Test]
    public void UcAyniParca_BirUstNadirlik_TakiliysaYenisiTakilir()
    {
        var a = Equipment.Add("leather_hat");
        Equipment.Add("leather_hat");
        Assert.IsFalse(Equipment.CanMerge(a), "2 parçayla birleşti");
        var c = Equipment.Add("leather_hat");
        c.level = 3;
        Assert.IsTrue(Equipment.Equip(a.uid));
        Assert.IsTrue(Equipment.CanMerge(a));
        var m = Equipment.Merge(a.uid);
        Assert.IsNotNull(m);
        Assert.AreEqual((int)EquipmentRarity.Good, m.rarity);
        Assert.AreEqual(3, m.level, "En yüksek seviye korunmadı");
        Assert.AreEqual(1, Equipment.All.Count, "Birleşen parçalar silinmedi");
        Assert.AreEqual(m, Equipment.Equipped(EquipmentSlot.Head), "Takılı parça birleşince yenisi takılmadı");
    }

    [Test]
    public void Yuva_TekParca_OzellikYalnizKapsamAcikken()
    {
        var hat = Equipment.Add("leather_hat");
        var hat2 = Equipment.Add("leather_hat", 2);
        Equipment.Equip(hat.uid);
        Equipment.Equip(hat2.uid);
        Assert.AreEqual(hat2, Equipment.Equipped(EquipmentSlot.Head), "Aynı yuvaya ikinci parça eskisinin yerine geçmedi");

        var s = new StatSheet();
        Equipment.AddModifiers(s);
        Assert.AreEqual(0f, s.Percent(StatId.CoinGain), 1e-5f, "Kapsam kapalıyken ekipman etki etti");
        KakScope.Equipment = true;
        s.Clear();
        Equipment.AddModifiers(s);
        float baseP = Equipment.Def("leather_hat").modifiers[0].percent;
        Assert.AreEqual(baseP * (1f + 2 * Equipment.RarityStep), s.Percent(StatId.CoinGain), 1e-4f, "Nadirlik çarpanı uygulanmadı");
    }
}
