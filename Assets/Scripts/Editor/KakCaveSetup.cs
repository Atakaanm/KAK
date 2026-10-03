using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static KakIceSetup;

/// <summary>
/// Faz 13 K2: Karanlık Mağara dünyası. Önce `python3 tools/kak_gen_cave.py all` ve `tools/kak_gen_dark.py`.
/// Kurar: mermiler (yarasa, sarkıt, spor, ağ + zindan taşları), 6 kademe (zindan ×1,5 süre, ×0,9 hız), Taş Muhafız
/// (mağara) fırlatıcısı, tema (karanlık 0,95, meşalesiz duvarlar), olaylar (GÖÇÜK!, YARASA SÜRÜSÜ!), seviye, katalog
/// (6. oyunda açılır). Tekrar çalıştırılabilir.
/// </summary>
public static class KakCaveSetup
{
    const string Dir = "Assets/Data/Worlds/Cave/";
    const string ArenaSprite = "Assets/Sprites/Arena/Cave_arena_01.png";
    const string PreviewSprite = "Assets/Sprites/Arena/Cave_preview.png";
    const string GuardDir = "Assets/Sprites/Spawner_Cave/";
    static readonly string[] Dirs = { "North", "South", "East", "West", "North_East", "North_West", "South_East", "South_West" };

    [MenuItem("KacAtaKac/Dünyalar/Karanlık Mağarayı Kur")]
    public static string Setup()
    {
        EnsureFolder("Assets/Data/Worlds", "Cave");
        EnsureFolder("Assets/Data/Worlds/Cave", "Stages");
        Imports();
        KakDarkSetup.ImportSprites();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        Sprite Art(string n) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Projectiles/" + n + ".png");
        var warn = Art("warn_mark");

        // ── Mermiler ──
        // Bot ölçümü: en çok yarasa vurdu; karanlıkta insan için daha da zor → biraz yavaş ve dar dalga (yumuşak)
        var bat = Proj("Bat_Yarasa", "Yarasa", ProjectileMotion.Wave, 1.3f, 1f, prefab, Art("bat"));
        bat.waveAmplitude = 0.35f; bat.waveFrequency = 1.3f; bat.noSpin = true;
        bat.glowColor = new Color(1f, 0.25f, 0.3f, 0.9f); // kırmızı gözler
        var spore = Proj("Spore_Spor", "Spor", ProjectileMotion.Straight, 0.95f, 0.9f, prefab, Art("spore"));
        spore.effect = ProjectileEffect.Slow; spore.effectStrength = 0.65f; spore.effectDuration = 2.2f; spore.damage = 0;
        spore.glowInDark = true; spore.glowColor = new Color(0.4f, 1f, 0.7f, 0.9f);
        var web = Proj("Web_Ag", "Örümcek Ağı", ProjectileMotion.Straight, 1.25f, 1f, prefab, Art("web"));
        web.effect = ProjectileEffect.Slow; web.effectStrength = 0.4f; web.effectDuration = 1.4f; web.damage = 0;
        web.glowColor = new Color(0.85f, 0.9f, 1f, 0.8f);
        var stal = Proj("Stalactite_Sarkit", "Sarkıt", ProjectileMotion.Meteor, 0f, 1.1f, prefab, Art("stalactite"));
        stal.meteorFallTime = 1.25f; stal.meteorStartHeight = 7f; stal.meteorRadius = 0.45f; stal.warningSprite = warn;
        foreach (var d in new[] { bat, spore, web, stal }) EditorUtility.SetDirty(d);
        var rock = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Rock_ProjectileData.asset");
        var pebble = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Projectiles/Pebble_Cakil.asset");
        var boulder = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Projectiles/Boulder_Kaya.asset");

        // ── Kademeler: zindanın eşikleri ×1,5 (karanlığa alışma payı), mermiler ×0,9 ──
        string[] src = { "Stage1_Baslangic", "Stage2_Kolay", "Stage3_Orta", "Stage4_Zor", "Stage5_Cehennem", "Stage6_Imkansiz" };
        var lists = new (ProjectileData d, float w)[][]
        {
            new[] { (rock, 1f) },
            new[] { (rock, 2f), (bat, 1f) },
            new[] { (rock, 1.6f), (bat, 1.2f), (web, 0.6f), (stal, 0.5f) },
            new[] { (rock, 1.3f), (bat, 1.4f), (web, 0.8f), (spore, 0.7f), (stal, 0.8f), (pebble, 0.6f) },
            new[] { (rock, 1.2f), (bat, 1.6f), (web, 0.9f), (spore, 0.9f), (stal, 1f), (pebble, 0.8f), (boulder, 0.5f) },
            new[] { (rock, 1.1f), (bat, 1.8f), (web, 1f), (spore, 1f), (stal, 1.2f), (pebble, 1f), (boulder, 0.7f) },
        };
        var stages = new DifficultyStageData[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            var from = AssetDatabase.LoadAssetAtPath<DifficultyStageData>("Assets/Data/" + src[i] + ".asset");
            string path = Dir + "Stages/Cave_" + src[i] + ".asset";
            var st = LoadOrCreate<DifficultyStageData>(path);
            if (from != null) EditorUtility.CopySerialized(from, st);
            st.name = "Cave_" + src[i];
            st.projectileSpeedMultiplier *= 0.9f;
            st.minSeconds *= 1.5f;
            var l = new List<(ProjectileData d, float w)>();
            foreach (var e in lists[i]) if (e.d != null) l.Add(e);
            st.availableProjectiles = new ProjectileData[l.Count];
            st.projectileWeights = new float[l.Count];
            for (int k = 0; k < l.Count; k++) { st.availableProjectiles[k] = l[k].d; st.projectileWeights[k] = l[k].w; }
            EditorUtility.SetDirty(st);
            stages[i] = st;
        }

        // ── Taş Muhafız (mağara) ──
        var rockThrower = AssetDatabase.LoadAssetAtPath<SpawnerData>("Assets/Data/RockThrower_SpawnerData.asset");
        var guard = LoadOrCreate<SpawnerData>(Dir + "CaveGuardian_SpawnerData.asset");
        if (rockThrower != null) EditorUtility.CopySerialized(rockThrower, guard);
        guard.name = "CaveGuardian_SpawnerData";
        guard.spawnerName = "CaveGuardian";
        guard.projectileData = rock;
        guard.idleNorth = Idle("North"); guard.idleSouth = Idle("South"); guard.idleEast = Idle("East"); guard.idleWest = Idle("West");
        guard.idleNorthEast = Idle("North_East"); guard.idleNorthWest = Idle("North_West");
        guard.idleSouthEast = Idle("South_East"); guard.idleSouthWest = Idle("South_West");
        guard.attackNorth = Attack("North"); guard.attackSouth = Attack("South"); guard.attackEast = Attack("East"); guard.attackWest = Attack("West");
        guard.attackNorthEast = Attack("North_East"); guard.attackNorthWest = Attack("North_West");
        guard.attackSouthEast = Attack("South_East"); guard.attackSouthWest = Attack("South_West");
        EditorUtility.SetDirty(guard);

        // ── Tema ──
        var theme = LoadOrCreate<WorldTheme>(Dir + "Cave_Theme.asset");
        theme.themeId = "cave";
        theme.arenaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArenaSprite);
        theme.playableAreaNormalized = new Rect(0.0795f, 0.0795f, 0.841f, 0.841f);
        theme.useSpawnerAnchors = true;
        theme.floorFriction = 1f; theme.floorSpeedMultiplier = 1f; theme.verticalSpeedMultiplier = 1f;
        theme.backdropTile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Cave/backdrop_tile.png");
        theme.corridorTile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Cave/corridor_tile.png");
        theme.corridorWall = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Cave/corridor_wall.png");
        theme.ledgeTile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Cave/ledge_tile.png");
        theme.cameraBackground = new Color(0.02f, 0.02f, 0.04f, 1f);
        theme.arenaTint = Color.white;
        theme.torches = false;          // karanlık: tek ışık oyuncunun meşalesi
        theme.darkness = 0.95f;
        theme.coldEnabled = false; theme.snowfall = false;
        theme.eventRainData = stal; theme.eventRainTitleKey = "ev_cavein";
        theme.eventRollingData = boulder; theme.eventRollingTitleKey = "";
        theme.eventSwarmData = bat; theme.eventSwarmTitleKey = "ev_swarm";
        EditorUtility.SetDirty(theme);

        // ── Seviye ──
        var dungeonLevel = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Endless_Level1_LevelData.asset");
        var level = LoadOrCreate<LevelData>(Dir + "Endless_Cave_LevelData.asset");
        if (dungeonLevel != null) EditorUtility.CopySerialized(dungeonLevel, level);
        level.name = "Endless_Cave_LevelData";
        level.levelName = "Endless Cave";
        level.theme = theme;
        level.spawnerDataList = new[] { guard, guard, guard, guard };
        level.difficultyStages = stages;
        var pu = new List<PowerupData>();
        foreach (var n in new[] { "HeartData", "ShieldData", "GhostData", "SloMoData", "SpeedData", "ShackleData", "InvisibleData" })
        {
            var d = AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/" + n + ".asset");
            if (d != null) pu.Add(d);
        }
        level.availablePowerups = pu.ToArray();
        EditorUtility.SetDirty(level);

        RegisterWorld(new EndlessWorlds.World { id = "cave", nameKey = "world_cave", descKey = "worlddesc_cave", level = level,
                                               preview = AssetDatabase.LoadAssetAtPath<Sprite>(PreviewSprite), unlockGames = 6 });
        AssetDatabase.SaveAssets();
        return "[KakCaveSetup] Karanlık Mağara: 4 yeni mermi, 6 kademe, Taş Muhafız, tema, olaylar, seviye, katalog";
    }

    static ProjectileData Proj(string file, string name, ProjectileMotion motion, float speed, float scale, GameObject prefab, Sprite sprite)
    {
        var d = LoadOrCreate<ProjectileData>(Dir + file + ".asset");
        d.projectileName = name;
        d.motion = motion;
        d.speed = speed;
        d.visualScale = scale;
        d.tint = Color.white;
        d.damage = 1;
        d.lifeTime = 7f;
        d.projectilePrefab = prefab;
        d.projectileSprite = sprite;
        d.growPerSecond = 0f; d.maxGrowScale = 1f; d.growSlowdown = 1f; d.growHitShare = 1f;
        d.catchable = false;
        d.effect = ProjectileEffect.Damage;
        d.glowInDark = false;
        return d;
    }

    static void Imports()
    {
        SpriteImport(ArenaSprite, 100f, true);
        SpriteImport(PreviewSprite, 100f, false);
        SpriteImport("Assets/Resources/StatusSlow.png", 100f, true);
        var ti = AssetImporter.GetAtPath(ArenaSprite) as TextureImporter;
        if (ti != null) { ti.maxTextureSize = 2048; ti.SaveAndReimport(); }
        var spawnerRef = AssetImporter.GetAtPath("Assets/Sprites/Spawner/South/south.png") as TextureImporter;
        foreach (var d in Dirs)
        {
            CopyImport(spawnerRef, GuardDir + d + "/idle.png");
            for (int i = 1; i <= 6; i++) CopyImport(spawnerRef, GuardDir + d + "/attack_" + i + ".png");
        }
        var tileRef = AssetImporter.GetAtPath("Assets/Art/Tiles/Dungeon/backdrop_tile.png") as TextureImporter;
        foreach (var n in new[] { "backdrop_tile", "corridor_tile", "corridor_wall", "ledge_tile" })
            CopyImport(tileRef, "Assets/Art/Tiles/Cave/" + n + ".png");
        AssetDatabase.Refresh();
    }

    static Sprite Idle(string dir) => AssetDatabase.LoadAssetAtPath<Sprite>(GuardDir + dir + "/idle.png");
    static Sprite[] Attack(string dir)
    {
        var a = new Sprite[6];
        for (int i = 0; i < 6; i++) a[i] = AssetDatabase.LoadAssetAtPath<Sprite>(GuardDir + dir + "/attack_" + (i + 1) + ".png");
        return a;
    }
}
