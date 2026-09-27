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
    public static int Best(string worldId) => worldId == "ice" ? SaveSystem.Data.bestScoreIce : SaveSystem.Data.bestScoreEndless;

    /// <summary>Bir seviyenin ait olduğu dünya (tema kimliğinden; teması olmayan = zindan).</summary>
    public static string WorldIdOf(LevelData level) => level != null && level.theme != null ? level.theme.themeId : "dungeon";
}
