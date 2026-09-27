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
/// G3 (Faz 11): oyuncu adım adım güçlenir. Karakter başına kalıcı yükseltmeler (altınla, seviye seviye):
/// Can (+1 kalp, karakterin üst sınırına kadar), Hız (+%4/seviye), Güçlendirme süresi (+%12/seviye).
/// Petler: seviye başına daha güçlü etki. Kayıt: SaveData.charLevels / petLevels.
/// </summary>
public static class CharacterProgress
{
    public const float SpeedPerLevel = 0.04f;
    public const float PowerPerLevel = 0.12f;
    public const int SpeedLevels = 3, PowerLevels = 3, PetLevels = 3;

    // İlk yükseltme 1-2 oyunda, sonrakiler giderek pahalı (denge: tools/kak_bridge.py denge + oyuncu geri bildirimi)
    static readonly int[] HealthCosts = { 60, 250, 600, 1000 };
    static readonly int[] SpeedCosts = { 80, 200, 450 };
    static readonly int[] PowerCosts = { 80, 200, 450 };
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

    public static int Level(PlayerData p, CharStat s)
    {
        if (p == null) return 0;
        var l = Get(p.id);
        if (l == null) return 0;
        int v = s == CharStat.Health ? l.health : s == CharStat.Speed ? l.speed : l.power;
        return Mathf.Clamp(v, 0, MaxLevel(p, s));
    }

    public static int MaxLevel(PlayerData p, CharStat s)
    {
        if (p == null) return 0;
        switch (s)
        {
            case CharStat.Health: return Mathf.Clamp(p.maxHealth - StartHealth(p), 0, HealthCosts.Length);
            case CharStat.Speed: return SpeedLevels;
            default: return PowerLevels;
        }
    }

    /// <summary>Sonraki seviyenin fiyatı; en üst seviyedeyse -1.</summary>
    public static int Cost(PlayerData p, CharStat s)
    {
        int lvl = Level(p, s);
        if (lvl >= MaxLevel(p, s)) return -1;
        var t = s == CharStat.Health ? HealthCosts : s == CharStat.Speed ? SpeedCosts : PowerCosts;
        return t[Mathf.Clamp(lvl, 0, t.Length - 1)];
    }

    public static bool CanUpgrade(PlayerData p, CharStat s)
    {
        int c = Cost(p, s);
        return c >= 0 && CharacterCatalog.Owned(p) && SaveSystem.Data.coins >= c;
    }

    public static bool TryUpgrade(PlayerData p, CharStat s)
    {
        if (!CanUpgrade(p, s)) return false;
        var d = SaveSystem.Data;
        d.coins -= Cost(p, s);
        var l = Get(p.id, true);
        if (s == CharStat.Health) l.health++;
        else if (s == CharStat.Speed) l.speed++;
        else l.power++;
        SaveSystem.Save();
        Changed?.Invoke();
        return true;
    }

    public static bool AnyUpgradeAffordable(PlayerData p)
    {
        return CanUpgrade(p, CharStat.Health) || CanUpgrade(p, CharStat.Speed) || CanUpgrade(p, CharStat.Power);
    }

    public static int StartHealth(PlayerData p) => p == null ? 1 : Mathf.Clamp(p.startHealth, 1, Mathf.Max(1, p.maxHealth));
    /// <summary>Oyuna başlarken can (başlangıç + yükseltme).</summary>
    public static int Hearts(PlayerData p) => p == null ? 1 : Mathf.Min(p.maxHealth, StartHealth(p) + Level(p, CharStat.Health));
    public static float SpeedMult(PlayerData p) => 1f + SpeedPerLevel * Level(p, CharStat.Speed);
    public static float PowerMult(PlayerData p) => 1f + PowerPerLevel * Level(p, CharStat.Power);

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
