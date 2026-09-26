using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Görev türü. "Tek oyunda" türleri bir oyunun değeriyle, "Toplam" türleri oyunlar boyunca birikerek ilerler.</summary>
public enum MissionType { SurviveSeconds, NearMisses, Dashes, CoinsInRun, ReachStage, ShieldBlocks, PlayGames }

[Serializable]
public class MissionState
{
    public MissionType type;
    public int target;
    public int progress;
    public int reward;
    public bool done;
}

/// <summary>Bir oyunun görevlerle ilgili istatistikleri (GameManager oyun sonunda doldurur).</summary>
public struct RunStats
{
    public float seconds;
    public int nearMisses, dashes, coins, shieldBlocks, maxStage;
}

/// <summary>
/// Görevler (Faz 3c.4): her zaman 3 aktif görev; oyun sonunda değerlendirilir, tamamlananın altın ödülü verilir,
/// yerine bir sonraki oyunda yenisi gelir. Her 3 tamamlanan görevde seviye (tier) artar: hedef ve ödül büyür.
/// Subway Surfers'ın 3'lü görev setlerinden uyarlandı (3c.md). Açılış: FeatureGate.Missions (3. oyun).
/// </summary>
public static class MissionSystem
{
    public const int ActiveCount = 3;

    static readonly MissionType[] Pool =
    {
        MissionType.SurviveSeconds, MissionType.NearMisses, MissionType.Dashes,
        MissionType.CoinsInRun, MissionType.ReachStage, MissionType.ShieldBlocks, MissionType.PlayGames
    };

    static readonly string[] StageKeys = { "stage_Baslangic", "stage_Kolay", "stage_Orta", "stage_Zor", "stage_Cehennem", "stage_Imkansiz" };

    public static bool IsCumulative(MissionType t) => t == MissionType.ShieldBlocks || t == MissionType.PlayGames;

    public static int Tier => SaveSystem.Data.missionsCompleted / ActiveCount;

    /// <summary>Tamamlananları çıkarır, eksikleri havuzdan (aynı türden iki tane olmadan) doldurur.</summary>
    public static List<MissionState> Ensure()
    {
        var d = SaveSystem.Data;
        if (d.missions == null) d.missions = new List<MissionState>();
        d.missions.RemoveAll(m => m == null || m.done);
        int guard = 0;
        while (d.missions.Count < ActiveCount && guard++ < 50)
        {
            var t = Pool[UnityEngine.Random.Range(0, Pool.Length)];
            if (d.missions.Exists(m => m.type == t)) continue;
            d.missions.Add(Create(t, Tier));
        }
        return d.missions;
    }

    public static MissionState Create(MissionType t, int tier)
    {
        var m = new MissionState { type = t };
        switch (t)
        {
            case MissionType.SurviveSeconds: m.target = 45 + 30 * tier; m.reward = 40 + 20 * tier; break;
            case MissionType.NearMisses: m.target = 3 + 2 * tier; m.reward = 40 + 20 * tier; break;
            case MissionType.Dashes: m.target = 3 + 2 * tier; m.reward = 30 + 15 * tier; break;
            case MissionType.CoinsInRun: m.target = 8 + 6 * tier; m.reward = 50 + 25 * tier; break;
            case MissionType.ReachStage: m.target = Mathf.Min(1 + tier, 5); m.reward = 60 + 30 * tier; break;
            case MissionType.ShieldBlocks: m.target = 1 + tier; m.reward = 40 + 20 * tier; break;
            case MissionType.PlayGames: m.target = 3 + tier; m.reward = 30 + 10 * tier; break;
        }
        return m;
    }

    static int RunValue(MissionType t, RunStats r)
    {
        switch (t)
        {
            case MissionType.SurviveSeconds: return Mathf.FloorToInt(r.seconds);
            case MissionType.NearMisses: return r.nearMisses;
            case MissionType.Dashes: return r.dashes;
            case MissionType.CoinsInRun: return r.coins;
            case MissionType.ReachStage: return r.maxStage;
            case MissionType.ShieldBlocks: return r.shieldBlocks;
            case MissionType.PlayGames: return 1;
            default: return 0;
        }
    }

    /// <summary>
    /// Oyunu görevlere işler. Tamamlananları işaretler, ödülü cüzdana ekler (kaydetmez: GameManager kaydeder).
    /// Döner: bu oyunla tamamlanan görevlerin toplam ödülü.
    /// </summary>
    public static int EvaluateRun(RunStats r, List<MissionState> justCompleted = null)
    {
        var d = SaveSystem.Data;
        if (d.missions == null) return 0;
        int total = 0;
        foreach (var m in d.missions)
        {
            if (m == null || m.done) continue;
            int v = RunValue(m.type, r);
            m.progress = IsCumulative(m.type) ? m.progress + v : Mathf.Max(m.progress, v);
            if (m.progress >= m.target)
            {
                m.progress = m.target;
                m.done = true;
                total += m.reward;
                d.missionsCompleted++;
                justCompleted?.Add(m);
            }
        }
        d.coins += total;
        d.totalCoins += total;
        return total;
    }

    /// <summary>Görev metni ("Tek oyunda 8 yakın geçiş yap").</summary>
    public static string Describe(MissionState m)
    {
        string key = "mission_" + m.type;
        if (m.type == MissionType.ReachStage)
        {
            string stage = Loc.T(StageKeys[Mathf.Clamp(m.target, 0, StageKeys.Length - 1)]).TrimEnd('!');
            return string.Format(Loc.T(key), stage);
        }
        // Tekil biçim (EN "Block 1 rocks" hatası): hedef 1 ise varsa "_1" anahtarı
        if (m.target == 1 && Loc.Has(key + "_1")) key += "_1";
        return string.Format(Loc.T(key), m.target);
    }
}
