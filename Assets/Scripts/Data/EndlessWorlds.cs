using System;
using UnityEngine;

/// <summary>
/// G7 (Faz 11): Sonsuz modun dünyaları/arenaları (Taş Zindanı, Buz Gölü...). Menüdeki DÜNYALAR paneli buradan okur; OYNA seçili
/// dünyanın seviyesini başlatır. Kayıt: SaveData.selectedWorld, dünya başına rekor. Kurulum: KakIceSetup (Resources/EndlessWorlds).
/// (Faz 2B'nin yıldızlı bölüm dünyaları ayrı: WorldCatalog / WorldData.)
/// </summary>
[CreateAssetMenu(fileName = "EndlessWorlds", menuName = "KacAtaKac/Endless Worlds")]
public class EndlessWorlds : ScriptableObject
{
    [Serializable]
    public class World
    {
        public string id = "dungeon";
        public string nameKey = "world_dungeon";
        public string descKey = "worlddesc_dungeon";
        public LevelData level;
        public Sprite preview;
        [Tooltip("Açılması için gereken oyun sayısı")]
        public int unlockGames = 0;
    }

    public World[] worlds;

    public static EndlessWorlds Load() => Resources.Load<EndlessWorlds>("EndlessWorlds");

    public World Find(string id)
    {
        if (worlds == null) return null;
        foreach (var w in worlds) if (w != null && w.id == id) return w;
        return null;
    }

    public static bool Unlocked(World w) => w != null && SaveSystem.Data.gamesPlayed >= w.unlockGames;

    /// <summary>Seçili (ve açık) dünya; yoksa ilk dünya.</summary>
    public static World Selected()
    {
        var c = Load();
        if (c == null || c.worlds == null || c.worlds.Length == 0) return null;
        if (!KakScope.Worlds) return c.Find("dungeon") ?? c.worlds[0]; // Faz 14: odak sürümünde tek dünya
        var w = c.Find(SaveSystem.Data.selectedWorld);
        return w != null && Unlocked(w) ? w : c.worlds[0];
    }

    public static bool Select(World w)
    {
        if (w == null || !Unlocked(w)) return false;
        SaveSystem.Data.selectedWorld = w.id;
        SaveSystem.Save();
        return true;
    }

    /// <summary>Dünyanın rekoru (tek kişilik).</summary>
    public static int Best(string worldId)
    {
        var d = SaveSystem.Data;
        if (worldId == "ice") return d.bestScoreIce;
        if (string.IsNullOrEmpty(worldId) || worldId == "dungeon") return d.bestScoreEndless;
        if (d.worldBests != null) foreach (var w in d.worldBests) if (w != null && w.id == worldId) return w.best;
        return 0;
    }

    /// <summary>Faz 13 K2: dünyanın rekorunu yazar (zindan/buz eski alanlarına, diğerleri worldBests listesine).</summary>
    public static void SetBest(string worldId, int score)
    {
        var d = SaveSystem.Data;
        if (worldId == "ice") { d.bestScoreIce = score; return; }
        if (string.IsNullOrEmpty(worldId) || worldId == "dungeon") { d.bestScoreEndless = score; return; }
        if (d.worldBests == null) d.worldBests = new System.Collections.Generic.List<WorldBest>();
        foreach (var w in d.worldBests) if (w != null && w.id == worldId) { w.best = score; return; }
        d.worldBests.Add(new WorldBest { id = worldId, best = score });
    }

    /// <summary>Bir seviyenin ait olduğu dünya (tema kimliğinden; teması olmayan = zindan).</summary>
    public static string WorldIdOf(LevelData level) => level != null && level.theme != null ? level.theme.themeId : "dungeon";
}
