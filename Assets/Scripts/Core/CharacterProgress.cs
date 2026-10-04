using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharStat { Health, Speed, Power }

[Serializable]
public class CharLevels
{
    public string id;
    public int health, speed, power;
}

[Serializable]
public class PetLevel
{
    public string id;
    public int level;
}

/// <summary>
/// G3 (Faz 11): oyuncu adım adım güçlenir. Faz 15 K3'ten beri Can/Hız/Güç ortak gelişim izleridir (Progression);
/// bu sınıf eski ekranlar için yönlendirir ve pet seviyelerini tutar. Eski karakter başına seviyeler (SaveData.charLevels)
/// yalnız altın iadesi için okunur.
/// </summary>
public static class CharacterProgress
{
    public const float SpeedPerLevel = 0.04f;
    public const float PowerPerLevel = 0.12f;
    public const int SpeedLevels = 3, PowerLevels = 3, PetLevels = 3;

    // Pet fiyatları (karakter Can/Hız/Güç fiyatları Faz 15 K3'te gelişim izlerine taşındı: Assets/Data/Upgrades)
    static readonly int[] PetCosts = { 150, 400, 800 };

    public static event Action Changed;

    static List<CharLevels> List
    {
        get
        {
            var d = SaveSystem.Data;
            if (d.charLevels == null) d.charLevels = new List<CharLevels>();
            return d.charLevels;
        }
    }

    public static CharLevels Get(string id, bool create = false)
    {
        foreach (var l in List) if (l != null && l.id == id) return l;
        if (!create) return null;
        var n = new CharLevels { id = id };
        List.Add(n);
        return n;
    }

    // ── Faz 15 K3: Can/Hız/Güç artık ortak gelişim izleri (Progression); bu API eski ekranlar için yönlendirir ──
    public static string TrackId(CharStat s) => s == CharStat.Health ? "hearts" : s == CharStat.Speed ? "speed" : "power";
    static UpgradeTrackData Track(CharStat s) => Progression.Track(TrackId(s));

    public static int Level(PlayerData p, CharStat s) => Mathf.Min(Progression.Level(Track(s)), MaxLevel(p, s));

    public static int MaxLevel(PlayerData p, CharStat s)
    {
        var t = Track(s);
        if (t == null) return 0;
        if (s == CharStat.Health && p != null) return Mathf.Clamp(p.maxHealth - StartHealth(p), 0, t.maxLevel);
        return t.maxLevel;
    }

    /// <summary>Sonraki seviyenin fiyatı; en üst seviyedeyse -1.</summary>
    public static int Cost(PlayerData p, CharStat s) => Level(p, s) >= MaxLevel(p, s) ? -1 : Progression.Cost(Track(s));

    public static bool CanUpgrade(PlayerData p, CharStat s) => Cost(p, s) >= 0 && CharacterCatalog.Owned(p) && Progression.CanUpgrade(Track(s));

    public static bool TryUpgrade(PlayerData p, CharStat s)
    {
        if (!CanUpgrade(p, s) || !Progression.TryUpgrade(Track(s))) return false;
        Changed?.Invoke();
        return true;
    }

    public static bool AnyUpgradeAffordable(PlayerData p)
    {
        return CanUpgrade(p, CharStat.Health) || CanUpgrade(p, CharStat.Speed) || CanUpgrade(p, CharStat.Power);
    }

    public static int StartHealth(PlayerData p) => StatBuilder.StartHealth(p);
    /// <summary>Oyuna başlarken can (taban + pasif + gelişim).</summary>
    public static int Hearts(PlayerData p) => p == null ? 1 : StatBuilder.Hearts(p, StatBuilder.Build(p));
    public static float SpeedMult(PlayerData p) => p == null || p.moveSpeed <= 0f ? 1f : StatBuilder.MoveSpeed(p, StatBuilder.Build(p)) / p.moveSpeed;
    public static float PowerMult(PlayerData p) => p == null || p.powerupDurationMultiplier <= 0f ? 1f : StatBuilder.PowerDuration(p, StatBuilder.Build(p)) / p.powerupDurationMultiplier;

    // ── Petler ──
    public static int PetLevelOf(string petId)
    {
        var d = SaveSystem.Data;
        if (d.petLevels == null) return 0;
        foreach (var l in d.petLevels) if (l != null && l.id == petId) return Mathf.Clamp(l.level, 0, PetLevels);
        return 0;
    }

    public static int PetCost(PetData p) => p == null || PetLevelOf(p.id) >= PetLevels ? -1 : PetCosts[PetLevelOf(p.id)];

    public static bool TryUpgradePet(PetData p)
    {
        int c = PetCost(p);
        var d = SaveSystem.Data;
        if (c < 0 || !PetCatalog.Owned(p) || d.coins < c) return false;
        d.coins -= c;
        if (d.petLevels == null) d.petLevels = new List<PetLevel>();
        var l = d.petLevels.Find(x => x != null && x.id == p.id);
        if (l == null) { l = new PetLevel { id = p.id }; d.petLevels.Add(l); }
        l.level++;
        SaveSystem.Save();
        Changed?.Invoke();
        return true;
    }
}
