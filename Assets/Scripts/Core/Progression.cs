using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IdLevel
{
    public string id;
    public int level;
}

/// <summary>
/// Faz 15 K3: ortak gelişim (tüm karakterler paylaşır; kullanıcı kararı 2026-10-04). İzler UpgradeCatalog'da,
/// seviyeler SaveData.upgrades'te. İzler toplam seviyeyle adım adım açılır. Eski karakter başına yükseltmeler
/// (CharLevels) kayıt yüklenirken altın iadesiyle taşınır (LegacyRefund).
/// </summary>
public static class Progression
{
    public static event Action Changed;

    static List<IdLevel> List
    {
        get
        {
            var d = SaveSystem.Data;
            if (d.upgrades == null) d.upgrades = new List<IdLevel>();
            return d.upgrades;
        }
    }

    static UpgradeTrackData[] Tracks
    {
        get
        {
            var c = UpgradeCatalog.Load();
            return c != null && c.tracks != null ? c.tracks : Array.Empty<UpgradeTrackData>();
        }
    }

    public static UpgradeTrackData Track(string id)
    {
        var c = UpgradeCatalog.Load();
        return c != null ? c.Find(id) : null;
    }

    public static int Level(string id)
    {
        foreach (var l in List) if (l != null && l.id == id) return Mathf.Max(0, l.level);
        return 0;
    }

    public static int Level(UpgradeTrackData t) => t == null ? 0 : Mathf.Clamp(Level(t.id), 0, t.maxLevel);

    public static int TotalLevels()
    {
        int n = 0;
        foreach (var t in Tracks) n += Level(t);
        return n;
    }

    public static int MaxTotal()
    {
        int n = 0;
        foreach (var t in Tracks) if (t != null) n += t.maxLevel;
        return n;
    }

    /// <summary>0 = hiç gelişim yok, 1 = her şey en üstte (tempo bununla ölçeklenir).</summary>
    public static float PowerNormalized()
    {
        int max = MaxTotal();
        return max > 0 ? Mathf.Clamp01(TotalLevels() / (float)max) : 0f;
    }

    public static bool Unlocked(UpgradeTrackData t) => t != null && TotalLevels() >= t.unlockAtTotal;

    /// <summary>Sonraki seviyenin fiyatı; en üstteyse -1.</summary>
    public static int Cost(UpgradeTrackData t) => t == null ? -1 : t.Cost(Level(t));

    /// <summary>Faz 15 K5: eşya izlerinin üst seviyeleri sandıktan çıkan parşömen de ister.</summary>
    public static int ScrollCost(UpgradeTrackData t) => t is ItemTrackData it ? it.ScrollCost(Level(t)) : 0;

    public static bool CanUpgrade(UpgradeTrackData t)
    {
        int c = Cost(t);
        var d = SaveSystem.Data;
        return c >= 0 && Unlocked(t) && d.coins >= c && d.scrolls >= ScrollCost(t);
    }

    public static bool TryUpgrade(UpgradeTrackData t)
    {
        if (!CanUpgrade(t)) return false;
        var d = SaveSystem.Data;
        d.scrolls -= ScrollCost(t);
        d.coins -= Cost(t);
        SetLevel(t.id, Level(t) + 1);
        SaveSystem.Save();
        Changed?.Invoke();
        return true;
    }

    /// <summary>Testler ve geliştirici menüsü: seviyeyi doğrudan yazar (kaydetmez).</summary>
    public static void SetLevel(string id, int level)
    {
        foreach (var l in List)
            if (l != null && l.id == id) { l.level = Mathf.Max(0, level); return; }
        List.Add(new IdLevel { id = id, level = Mathf.Max(0, level) });
    }

    public static bool AnyAffordable()
    {
        foreach (var t in Tracks) if (CanUpgrade(t)) return true;
        return false;
    }

    /// <summary>Henüz açılmamış en yakın iz ("?" merak kartı); yoksa null.</summary>
    public static UpgradeTrackData NextLocked()
    {
        UpgradeTrackData best = null;
        int total = TotalLevels();
        foreach (var t in Tracks)
            if (t != null && t.unlockAtTotal > total && (best == null || t.unlockAtTotal < best.unlockAtTotal)) best = t;
        return best;
    }

    /// <summary>Gelişim seviyelerinin özelliklerini sayfaya ekler.</summary>
    public static void AddModifiers(StatSheet sheet)
    {
        foreach (var t in Tracks)
        {
            if (t is ItemTrackData) continue; // eşya izleri ItemProgress'te
            int lvl = Level(t);
            if (lvl > 0) sheet.Add(t.ModifierAt(lvl));
        }
    }

    // ── Eski karakter başına yükseltmeler (Faz 11 G3) → altın iadesi ──
    static readonly int[] OldHealth = { 60, 250, 600, 1000 };
    static readonly int[] OldSpeed = { 80, 200, 450 };
    static readonly int[] OldPower = { 80, 200, 450 };

    public static int LegacyRefund(List<CharLevels> old)
    {
        if (old == null) return 0;
        int sum = 0;
        foreach (var l in old)
        {
            if (l == null) continue;
            sum += Paid(OldHealth, l.health) + Paid(OldSpeed, l.speed) + Paid(OldPower, l.power);
        }
        return sum;
    }

    static int Paid(int[] costs, int level)
    {
        int s = 0;
        for (int i = 0; i < Mathf.Min(level, costs.Length); i++) s += costs[i];
        return s;
    }

    public static void RaiseChanged() => Changed?.Invoke();
}
