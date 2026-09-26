using UnityEngine;

/// <summary>Tüm bölüm dünyaları (sıralı). Bölüm seçimi ve "sonraki bölüm" bunu kullanır.</summary>
[CreateAssetMenu(fileName = "WorldCatalog", menuName = "KacAtaKac/World Catalog")]
public class WorldCatalog : ScriptableObject
{
    public WorldData[] worlds;

    static WorldCatalog cached;
    public static WorldCatalog Load()
    {
        if (cached == null) cached = Resources.Load<WorldCatalog>("WorldCatalog");
        return cached;
    }

    public bool IsWorldUnlocked(WorldData w) => w != null && SaveSystem.Data.TotalStars >= w.starsToUnlock;

    public bool IsLevelUnlocked(WorldData w, int index)
    {
        if (!IsWorldUnlocked(w)) return false;
        if (index == 0) return true;
        var prev = w.levels[index - 1];
        return prev != null && SaveSystem.Data.Stars(prev.levelId) > 0;
    }

    /// <summary>Verilen bölümden sonraki bölüm (aynı dünyada), yoksa null.</summary>
    public LevelData Next(LevelData level)
    {
        if (worlds == null || level == null) return null;
        foreach (var w in worlds)
        {
            if (w == null || w.levels == null) continue;
            for (int i = 0; i < w.levels.Length; i++)
                if (w.levels[i] == level) return i + 1 < w.levels.Length ? w.levels[i + 1] : null;
        }
        return null;
    }

    public WorldData WorldOf(LevelData level)
    {
        if (worlds == null) return null;
        foreach (var w in worlds) if (w != null && System.Array.IndexOf(w.levels, level) >= 0) return w;
        return null;
    }
}
