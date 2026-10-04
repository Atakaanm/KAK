using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>Faz 15 K3: özellik sayfası, ortak gelişim (adım adım açılma, fiyat, kayıt), karakter pasifi, eski yükseltmelerin altın iadesi.</summary>
public class GelisimTests
{
    string path;
    readonly List<Object> made = new List<Object>();

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Application.temporaryCachePath, "kak_gelisim_test.json");
        if (File.Exists(path)) File.Delete(path);
        SaveSystem.OverridePath = path;
        SaveSystem.Unload();
        Assert.IsNotNull(UpgradeCatalog.Load(), "UpgradeCatalog yok: KacAtaKac/Gelişim İzlerini Kur");
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var o in made) Object.DestroyImmediate(o);
        made.Clear();
        SaveSystem.OverridePath = null; SaveSystem.Unload();
        if (File.Exists(path)) File.Delete(path);
    }

    PlayerData Char(string id, int start, int max, params StatModifier[] passive)
    {
        var p = ScriptableObject.CreateInstance<PlayerData>();
        p.id = id; p.startHealth = start; p.maxHealth = max; p.moveSpeed = 4f; p.passive = passive;
        made.Add(p);
        return p;
    }

    [Test]
    public void Sayfa_TabanArtiSabitCarpiYuzde()
    {
        var s = new StatSheet();
        s.Add(StatId.MoveSpeed, 0f, 0.1f);
        s.Add(StatId.MoveSpeed, 0f, 0.05f);
        s.Add(StatId.Magnet, 0.5f, 0f);
        Assert.AreEqual(4f * 1.15f, s.Apply(StatId.MoveSpeed, 4f), 1e-4f);
        Assert.AreEqual(0.5f, s.Apply(StatId.Magnet, 0f), 1e-4f);
        s.Clear();
        Assert.AreEqual(4f, s.Apply(StatId.MoveSpeed, 4f), 1e-4f);
    }

    [Test]
    public void CokOzellik_AdimAdimAcilir()
    {
        var cat = UpgradeCatalog.Load();
        Assert.GreaterOrEqual(cat.tracks.Length, 12, "Kullanıcı birçok özellik istedi (3 az)");
        var coin = Progression.Track("coin");
        Assert.Greater(coin.unlockAtTotal, 0);
        SaveSystem.Data.coins = 100000;
        Assert.IsFalse(Progression.Unlocked(coin), "Kilitli iz baştan açık");
        Assert.IsFalse(Progression.TryUpgrade(coin), "Kilitli iz yükseldi");
        Assert.IsNotNull(Progression.NextLocked(), "Merak kartı için sıradaki kilitli iz yok");
        var hearts = Progression.Track("hearts");
        for (int i = 0; i < coin.unlockAtTotal; i++)
        {
            var t = Progression.Unlocked(hearts) && Progression.Cost(hearts) >= 0 ? hearts : Progression.Track("speed");
            Assert.IsTrue(Progression.TryUpgrade(t));
        }
        Assert.IsTrue(Progression.Unlocked(coin), "Toplam seviye yetince iz açılmadı");
        Assert.IsTrue(Progression.TryUpgrade(coin));
    }

    [Test]
    public void Yukseltme_AltinDuser_Kaydedilir_GucArtar()
    {
        var speed = Progression.Track("speed");
        int c = Progression.Cost(speed);
        SaveSystem.Data.coins = c - 1;
        Assert.IsFalse(Progression.TryUpgrade(speed));
        SaveSystem.Data.coins = c + 3;
        Assert.AreEqual(0f, Progression.PowerNormalized(), 1e-5f);
        Assert.IsTrue(Progression.TryUpgrade(speed));
        Assert.AreEqual(3, SaveSystem.Data.coins);
        Assert.Greater(Progression.PowerNormalized(), 0f, "Güç (tempo) artmadı");
        Assert.Greater(Progression.Cost(speed), c, "Sonraki seviye daha pahalı olmalı");
        SaveSystem.Unload();
        Assert.AreEqual(1, Progression.Level("speed"), "Seviye kaydedilmedi");
    }

    [Test]
    public void Pasif_ve_Gelisim_Toplanir_CanKarakterSinirinda()
    {
        var ada = Char("Ada", 1, 2, new StatModifier(StatId.NearMissBonus, 0f, 0.25f));
        Progression.SetLevel("nearbonus", 2);
        Progression.SetLevel("hearts", 4);
        var s = StatBuilder.Build(ada);
        float per = Progression.Track("nearbonus").percentPerLevel;
        Assert.AreEqual(1f + 0.25f + 2 * per, StatBuilder.NearBonusMult(s), 1e-4f);
        Assert.AreEqual(2, StatBuilder.Hearts(ada, s), "Can karakterin sınırını aştı");
        var tank = Char("Tank", 2, 5);
        Assert.AreEqual(5, StatBuilder.Hearts(tank, StatBuilder.Build(tank)));
    }

    [Test]
    public void OzelIzler_KalkanYenilenmesi_IkinciSans_Miknatis()
    {
        var p = Char("Boy", 1, 3);
        var s = StatBuilder.Build(p);
        Assert.AreEqual(0f, StatBuilder.ShieldRegenInterval(s));
        Assert.AreEqual(0, StatBuilder.Revives(s));
        Assert.AreEqual(0f, StatBuilder.Magnet(s));
        Progression.SetLevel("shieldregen", 2);
        Progression.SetLevel("revive", 1);
        Progression.SetLevel("magnet", 3);
        s = StatBuilder.Build(p);
        Assert.AreEqual(StatBuilder.ShieldRegenSeconds[2], StatBuilder.ShieldRegenInterval(s));
        Assert.AreEqual(1, StatBuilder.Revives(s));
        Assert.AreEqual(3 * Progression.Track("magnet").flatPerLevel, StatBuilder.Magnet(s), 1e-4f);
    }

    [Test]
    public void EskiKarakterYukseltmeleri_BirKezAltinIadesi()
    {
        var d = new SaveData { coins = 10 };
        d.charLevels = new List<CharLevels> { new CharLevels { id = "Boy", health = 2, speed = 1, power = 0 }, new CharLevels { id = "Tank", power = 2 } };
        d.Upgrade();
        int expected = 60 + 250 + 80 + 80 + 200;
        Assert.AreEqual(expected, d.legacyRefund);
        Assert.AreEqual(10 + expected, d.coins);
        d.Upgrade();
        Assert.AreEqual(10 + expected, d.coins, "İade iki kez yapıldı");
        Assert.AreEqual(2, d.charLevels.Count, "Eski liste silinmemeli");
    }
}
