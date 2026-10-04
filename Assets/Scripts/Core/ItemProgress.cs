using UnityEngine;

/// <summary>
/// Faz 15 K5: eşya geliştirmelerinin oyundaki karşılıkları (UpgradeCatalog'daki ItemTrackData izleri).
/// Oyun başında Snapshot() bir kez okur; oyunda dizi okuması (tahsis yok). Menü de aynı hesabı kullanır.
/// </summary>
public static class ItemProgress
{
    const int TypeCount = 16; // PowerupType sınırı (sona eklenir)
    static readonly float[] duration = new float[TypeCount];
    static readonly float[] frequency = new float[TypeCount];
    static readonly float[] ground = new float[TypeCount];
    static readonly int[] power = new int[TypeCount];

    static void Reset()
    {
        for (int i = 0; i < TypeCount; i++) { duration[i] = 1f; frequency[i] = 1f; ground[i] = 1f; power[i] = 0; }
    }

    /// <summary>Kayıttaki seviyelerden eşya çarpanlarını hesaplar (oyun başında, menüde gösterimden önce).</summary>
    public static void Snapshot()
    {
        Reset();
        var cat = UpgradeCatalog.Load();
        if (cat == null || cat.tracks == null) return;
        foreach (var t in cat.tracks)
        {
            if (!(t is ItemTrackData it)) continue;
            int i = (int)it.item;
            if (i < 0 || i >= TypeCount) continue;
            int lvl = Progression.Level(t);
            if (lvl <= 0) continue;
            switch (it.kind)
            {
                case ItemStat.Duration: duration[i] += it.percentPerLevel * lvl; break;
                case ItemStat.Frequency: frequency[i] += it.percentPerLevel * lvl; break;
                case ItemStat.GroundTime: ground[i] += it.percentPerLevel * lvl; break;
                case ItemStat.Power: power[i] += lvl; break;
            }
        }
        for (int i = 0; i < TypeCount; i++)
        {
            duration[i] = Mathf.Max(0.2f, duration[i]);
            frequency[i] = Mathf.Max(0.1f, frequency[i]);
            ground[i] = Mathf.Max(0.3f, ground[i]);
        }
    }

    public static float DurationMult(PowerupType t) => Get(duration, t, 1f);
    public static float FrequencyMult(PowerupType t) => Get(frequency, t, 1f);
    public static float GroundMult(PowerupType t) => Get(ground, t, 1f);
    public static int Power(PowerupType t) { int i = (int)t; return i >= 0 && i < TypeCount ? power[i] : 0; }

    static float Get(float[] a, PowerupType t, float fallback) { int i = (int)t; return i >= 0 && i < TypeCount ? a[i] : fallback; }

    /// <summary>Genel özellik veren eşya izleri (Pranga direnci) sayfaya eklenir.</summary>
    public static void AddModifiers(StatSheet sheet)
    {
        var cat = UpgradeCatalog.Load();
        if (cat == null || cat.tracks == null) return;
        foreach (var t in cat.tracks)
        {
            if (!(t is ItemTrackData it) || it.kind != ItemStat.Resist) continue;
            int lvl = Progression.Level(t);
            if (lvl > 0) sheet.Add(StatId.ShackleResist, it.flatPerLevel * lvl, 0f);
        }
    }

    static ItemProgress() => Reset();
}
