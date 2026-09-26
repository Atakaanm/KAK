using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>Faz 3c.4: görev ilerlemesi, ödül, yenilenme, seviye.</summary>
public class MissionTests
{
    string path;

    [SetUp]
    public void SetUp()
    {
        path = Path.Combine(Application.temporaryCachePath, "kak_mission_test.json");
        if (File.Exists(path)) File.Delete(path);
        SaveSystem.OverridePath = path;
        SaveSystem.Unload();
    }

    [TearDown]
    public void TearDown() { SaveSystem.OverridePath = null; SaveSystem.Unload(); if (File.Exists(path)) File.Delete(path); }

    [Test]
    public void Ensure_UcFarkliGorev()
    {
        var list = MissionSystem.Ensure();
        Assert.AreEqual(MissionSystem.ActiveCount, list.Count);
        Assert.AreEqual(list.Count, new HashSet<MissionType>(list.ConvertAll(m => m.type)).Count, "Aynı türden iki görev");
        foreach (var m in list) Assert.IsFalse(string.IsNullOrEmpty(MissionSystem.Describe(m)));
    }

    [Test]
    public void TekOyunGorevi_EnIyiDegeriTutar_Toplam_Birikir()
    {
        var d = SaveSystem.Data;
        d.missions = new List<MissionState>
        {
            MissionSystem.Create(MissionType.NearMisses, 0),   // tek oyunda 3
            MissionSystem.Create(MissionType.PlayGames, 0),    // toplam 3 oyun
        };
        MissionSystem.EvaluateRun(new RunStats { nearMisses = 2 });
        MissionSystem.EvaluateRun(new RunStats { nearMisses = 1 });
        Assert.AreEqual(2, d.missions[0].progress, "Tek oyun görevi toplanmamalı, en iyi değer tutulmalı");
        Assert.AreEqual(2, d.missions[1].progress, "Oyun sayısı birikmeli");
        Assert.IsFalse(d.missions[0].done);
    }

    [Test]
    public void Tamamlaninca_OdulVerilir_SonraYenisiGelir_SeviyeArtar()
    {
        var d = SaveSystem.Data;
        d.missions = new List<MissionState>
        {
            MissionSystem.Create(MissionType.SurviveSeconds, 0),
            MissionSystem.Create(MissionType.Dashes, 0),
            MissionSystem.Create(MissionType.CoinsInRun, 0),
        };
        int expected = d.missions[0].reward + d.missions[1].reward + d.missions[2].reward;
        var done = new List<MissionState>();
        int reward = MissionSystem.EvaluateRun(new RunStats { seconds = 999, dashes = 99, coins = 99 }, done);
        Assert.AreEqual(expected, reward);
        Assert.AreEqual(expected, d.coins, "Ödül cüzdana eklenmedi");
        Assert.AreEqual(3, done.Count);
        Assert.AreEqual(1, MissionSystem.Tier, "3 görevden sonra seviye artmalı");
        var fresh = MissionSystem.Ensure();
        Assert.AreEqual(3, fresh.Count);
        foreach (var m in fresh) Assert.IsFalse(m.done, "Tamamlananlar yenilenmedi");
        // Seviye 1 hedefleri seviye 0'dan büyük
        var s1 = MissionSystem.Create(MissionType.NearMisses, 1);
        Assert.Greater(s1.target, MissionSystem.Create(MissionType.NearMisses, 0).target);
    }
}
