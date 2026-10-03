using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Faz 11 G7: Buz Gölü dünyası kurulumu (tekrar çalıştırılabilir). Önce görseller: python3 tools/kak_gen_ice.py all
///  - içe aktarma ayarları (arena, kardan adam, eşya ikonları, Resources sprite'ları)
///  - kartopu/sarkıt mermi türleri, buz kademeleri (zindan kademelerinin süre ve çarpanlarıyla), kardan adam fırlatıcısı
///  - Eldiven / Buz ayakkabısı / Ateş eşyaları + prefab'ları, tema, Endless_Ice seviyesi, Resources/EndlessWorlds
/// Menü: KacAtaKac/Dünyalar/Buz Gölünü Kur. Köprü: invoke KakIceSetup Setup
/// </summary>
public static class KakIceSetup
{
    const string Dir = "Assets/Data/Worlds/Ice/";
    const string ArenaSprite = "Assets/Sprites/Arena/Ice_arena_01.png";
    const string SnowmanDir = "Assets/Sprites/Spawner_Ice/";
    static readonly string[] Dirs = { "North", "South", "East", "West", "North_East", "North_West", "South_East", "South_West" };

    [MenuItem("KacAtaKac/Dünyalar/Buz Gölünü Kur")]
    public static string Setup()
    {
        EnsureFolder("Assets/Data", "Worlds");
        EnsureFolder("Assets/Data/Worlds", "Ice");
        EnsureFolder("Assets/Data/Worlds/Ice", "Stages");
        ImportSettings();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        var snowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Projectiles/snowball.png");
        var icicleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Projectiles/icicle.png");
        var warn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Projectiles/warn_mark.png");

        // ── Mermiler: kartopları yol aldıkça büyür, eldivenle yakalanır ──
        // Faz 12 H2: kartopları çarpana kadar büyür (eski üst sınır 1,25-1,6 çabuk doluyordu), büyüdükçe yavaşlar (growSlowdown)
        var snow = Proj("Snowball_Kartopu", "Kartopu", ProjectileMotion.Straight, 1.35f, 0.9f, prefab, snowSprite, 0.28f, 2.0f, 0.85f);
        var small = Proj("SmallSnowball_KucukKartopu", "Küçük Kartopu", ProjectileMotion.Straight, 1.9f, 0.65f, prefab, snowSprite, 0.22f, 1.6f, 0.88f);
        var big = Proj("BigSnowball_DevKartopu", "Dev Kartopu", ProjectileMotion.Straight, 0.95f, 1.05f, prefab, snowSprite, 0.26f, 2.1f, 0.82f);
        var bounce = Proj("BounceSnowball_SekenKartopu", "Seken Kartopu", ProjectileMotion.Bounce, 1.55f, 0.8f, prefab, snowSprite, 0.12f, 1.6f, 0.9f);
        // Yuvarlanan olayı: şeridi tarayan dev kartopu (eldivenle yakalanmaz, az büyür)
        var rolling = Proj("RollingSnowball_YuvarlananKartopu", "Yuvarlanan Kartopu", ProjectileMotion.Straight, 1.05f, 1.1f, prefab, snowSprite, 0.1f, 1.3f, 1f);
        rolling.catchable = false;
        bounce.bounces = 2; bounce.lifeTime = 9f;
        var icicle = Proj("Icicle_BuzSarkiti", "Buz Sarkıtı", ProjectileMotion.Meteor, 0f, 1.1f, prefab, icicleSprite, 0f, 1f);
        icicle.catchable = false; icicle.meteorFallTime = 1.2f; icicle.meteorStartHeight = 7f; icicle.meteorRadius = 0.5f; icicle.warningSprite = warn;

        // ── Kademeler: zindanla aynı süre eşikleri ve çarpanlar, buz mermileri ──
        string[] src = { "Stage1_Baslangic", "Stage2_Kolay", "Stage3_Orta", "Stage4_Zor", "Stage5_Cehennem", "Stage6_Imkansiz" };
        var lists = new (ProjectileData d, float w)[][]
        {
            new[] { (snow, 1f) },
            new[] { (snow, 3f), (small, 1f) },
            new[] { (snow, 3f), (small, 1.3f), (big, 0.5f), (bounce, 0.6f) },
            new[] { (snow, 2.6f), (small, 1.5f), (big, 0.7f), (bounce, 0.9f), (icicle, 0.6f) },
            new[] { (snow, 2.2f), (small, 1.6f), (big, 0.9f), (bounce, 1.1f), (icicle, 0.9f) },
            new[] { (snow, 1.8f), (small, 1.8f), (big, 1f), (bounce, 1.3f), (icicle, 1.2f) },
        };
        var stages = new DifficultyStageData[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            var from = AssetDatabase.LoadAssetAtPath<DifficultyStageData>("Assets/Data/" + src[i] + ".asset");
            string path = Dir + "Stages/Ice_" + src[i] + ".asset";
            var st = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(path);
            if (st == null) { st = ScriptableObject.CreateInstance<DifficultyStageData>(); AssetDatabase.CreateAsset(st, path); }
            if (from != null) EditorUtility.CopySerialized(from, st);
            st.name = "Ice_" + src[i];
            st.projectileSpeedMultiplier *= 0.93f; // kayarken kaçmak daha zor: mermiler biraz yavaş (bot ölçümü)
            st.minSeconds *= 1.5f; // kaymaya alışma payı: ısınma 45 sn, İmkansız 9. dakika
            st.availableProjectiles = new ProjectileData[lists[i].Length];
            st.projectileWeights = new float[lists[i].Length];
            for (int k = 0; k < lists[i].Length; k++) { st.availableProjectiles[k] = lists[i][k].d; st.projectileWeights[k] = lists[i][k].w; }
            EditorUtility.SetDirty(st);
            stages[i] = st;
        }

        // ── Kardan adam fırlatıcısı ──
        var rockThrower = AssetDatabase.LoadAssetAtPath<SpawnerData>("Assets/Data/RockThrower_SpawnerData.asset");
        var snowman = LoadOrCreate<SpawnerData>(Dir + "Snowman_SpawnerData.asset");
        if (rockThrower != null) EditorUtility.CopySerialized(rockThrower, snowman);
        snowman.name = "Snowman_SpawnerData";
        snowman.spawnerName = "Snowman";
        snowman.projectileData = snow;
        snowman.idleNorth = Idle("North"); snowman.idleSouth = Idle("South"); snowman.idleEast = Idle("East"); snowman.idleWest = Idle("West");
        snowman.idleNorthEast = Idle("North_East"); snowman.idleNorthWest = Idle("North_West");
        snowman.idleSouthEast = Idle("South_East"); snowman.idleSouthWest = Idle("South_West");
        snowman.attackNorth = Attack("North"); snowman.attackSouth = Attack("South"); snowman.attackEast = Attack("East"); snowman.attackWest = Attack("West");
        snowman.attackNorthEast = Attack("North_East"); snowman.attackNorthWest = Attack("North_West");
        snowman.attackSouthEast = Attack("South_East"); snowman.attackSouthWest = Attack("South_West");
        EditorUtility.SetDirty(snowman);

        // ── Eşyalar ──
        var glove = Powerup("GloveData", "Glove", PowerupType.Glove, 10f, 1f, 0.8f, "GloveIcon", 1);
        var boots = Powerup("IceBootsData", "IceBoots", PowerupType.IceBoots, 8f, 1f, 0.8f, "IceBootsIcon", 0);
        var fire = Powerup("FireData", "Fire", PowerupType.Fire, 0f, 0.7f, 1.7f, "FireIcon", 0); // powerMultiplier = ısınma miktarı

        // ── Tema ──
        var theme = LoadOrCreate<WorldTheme>(Dir + "Ice_Theme.asset");
        theme.themeId = "ice";
        theme.arenaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArenaSprite);
        theme.playableAreaNormalized = new Rect(0.0795f, 0.0795f, 0.841f, 0.841f);
        theme.useSpawnerAnchors = true;
        theme.floorFriction = 0.33f;          // hafif patinaj: ~0,3 sn'de hıza ulaşır / durur (bot ölçümüyle)
        theme.floorSpeedMultiplier = 1f;
        theme.verticalSpeedMultiplier = 1.2f; // aşağı-yukarı daha hızlı
        theme.backdropTile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Ice/backdrop_tile.png");
        theme.corridorTile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Ice/corridor_tile.png");
        theme.corridorWall = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Ice/corridor_wall.png");
        theme.ledgeTile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Ice/ledge_tile.png");
        theme.cameraBackground = new Color(0.07f, 0.08f, 0.13f, 1f);
        theme.arenaTint = Color.white;
        theme.torches = false;
        theme.coldEnabled = true;
        theme.coldSeconds = 35f;
        theme.snowfall = true;
        // Faz 12 H2: olaylar dünyaya göre — taş yağmuru yerine sarkıt yağmuru, yuvarlanan kaya yerine dev kartopu
        theme.eventRainData = icicle;
        theme.eventRainTitleKey = "ev_icicle";
        theme.eventRollingData = rolling;
        theme.eventRollingTitleKey = "ev_snowroll";
        EditorUtility.SetDirty(theme);

        // ── Seviye ──
        var dungeonLevel = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Endless_Level1_LevelData.asset");
        var level = LoadOrCreate<LevelData>(Dir + "Endless_Ice_LevelData.asset");
        if (dungeonLevel != null) EditorUtility.CopySerialized(dungeonLevel, level);
        level.name = "Endless_Ice_LevelData";
        level.levelName = "Endless Ice";
        level.levelId = "endless_ice";
        level.theme = theme;
        level.spawnerDataList = new[] { snowman, snowman, snowman, snowman };
        level.difficultyStages = stages;
        var pu = new List<PowerupData>();
        foreach (var n in new[] { "HeartData", "ShieldData", "GhostData", "SloMoData", "SpeedData", "ShackleData", "InvisibleData" })
        {
            var d = AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/" + n + ".asset");
            if (d != null) pu.Add(d);
        }
        pu.Add(glove); pu.Add(boots); pu.Add(fire);
        level.availablePowerups = pu.ToArray();
        EditorUtility.SetDirty(level);

        // ── Dünya kataloğu ──
        var cat = LoadOrCreate<EndlessWorlds>("Assets/Resources/EndlessWorlds.asset");
        cat.worlds = new[]
        {
            new EndlessWorlds.World { id = "dungeon", nameKey = "world_dungeon", descKey = "worlddesc_dungeon", level = dungeonLevel,
                                     preview = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Arena/Dungeon_arena_01.png"), unlockGames = 0 },
            new EndlessWorlds.World { id = "ice", nameKey = "world_ice", descKey = "worlddesc_ice", level = level,
                                     preview = theme.arenaSprite, unlockGames = 3 },
        };
        EditorUtility.SetDirty(cat);
        AssetDatabase.SaveAssets();
        return "[KakIceSetup] Buz Gölü: 5 mermi, 6 kademe, kardan adam, 3 eşya, tema, seviye, dünya kataloğu";
    }

    // ─────────────────────────────── yardımcılar ───────────────────────────────
    static void ImportSettings()
    {
        var ti = AssetImporter.GetAtPath(ArenaSprite) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100f;       // zindan arenasıyla aynı (ölçek 0,4 → piksel 0,024 dünya birimi)
            ti.filterMode = FilterMode.Point;
            ti.maxTextureSize = 2048;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
        }
        var spawnerRef = AssetImporter.GetAtPath("Assets/Sprites/Spawner/South/south.png") as TextureImporter;
        foreach (var d in Dirs)
            foreach (var n in new[] { "idle", "attack_1", "attack_2", "attack_3", "attack_4" })
                CopyImport(spawnerRef, SnowmanDir + d + "/" + n + ".png");
        var heartRef = AssetImporter.GetAtPath("Assets/Sprites/Powerups/HeartIcon.png") as TextureImporter;
        foreach (var n in new[] { "GloveIcon", "IceBootsIcon", "FireIcon" }) CopyImport(heartRef, "Assets/Sprites/Powerups/" + n + ".png");
        SpriteImport("Assets/Resources/IceBlock.png", KakArtImportRules.ArtPixelPPU, true);
        SpriteImport("Assets/Resources/Snowflake.png", 100f, true);
        SpriteImport("Assets/Resources/UiWhite.png", 100f, false);
    }

    static void CopyImport(TextureImporter from, string path)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null || from == null) return;
        var s = new TextureImporterSettings();
        from.ReadTextureSettings(s);
        ti.SetTextureSettings(s);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.SaveAndReimport();
    }

    static void SpriteImport(string path, float ppu, bool point)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.filterMode = point ? FilterMode.Point : FilterMode.Bilinear;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();
    }

    static Sprite Idle(string dir) => AssetDatabase.LoadAssetAtPath<Sprite>(SnowmanDir + dir + "/idle.png");
    static Sprite[] Attack(string dir)
    {
        var a = new Sprite[4];
        for (int i = 0; i < 4; i++) a[i] = AssetDatabase.LoadAssetAtPath<Sprite>(SnowmanDir + dir + "/attack_" + (i + 1) + ".png");
        return a;
    }

    static ProjectileData Proj(string file, string name, ProjectileMotion motion, float speed, float scale, GameObject prefab, Sprite sprite,
                               float grow, float maxGrow, float slowdown = 1f)
    {
        var d = LoadOrCreate<ProjectileData>(Dir + file + ".asset");
        d.projectileName = name;
        d.motion = motion;
        d.speed = speed;
        d.visualScale = scale;
        d.tint = Color.white;
        d.damage = 1;
        d.lifeTime = 6f;
        d.noSpin = true;            // yuvarlak kartopunda dönen parlama tuhaf görünür
        d.projectilePrefab = prefab;
        d.projectileSprite = sprite;
        d.growPerSecond = grow;
        d.maxGrowScale = maxGrow;
        d.growSlowdown = slowdown;
        d.growHitShare = grow > 0f ? 0.5f : 1f; // görüntü tam büyür, çarpışma alanı %50 (bot ölçümü: tam büyüme usta süresini 92 → 67 sn düşürdü)
        d.catchable = motion != ProjectileMotion.Meteor;
        EditorUtility.SetDirty(d);
        return d;
    }

    static PowerupData Powerup(string file, string name, PowerupType type, float duration, float power, float weight, string icon, int minStage)
    {
        string dataPath = "Assets/Data/Powerups/" + file + ".asset";
        string prefabPath = "Assets/Prefabs/Powerups/" + name + "Pickup.prefab";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Powerups/" + icon + ".png");
        var d = LoadOrCreate<PowerupData>(dataPath);
        d.powerupName = name; d.type = type; d.duration = duration; d.powerMultiplier = power; d.healthAmount = 0;
        d.spawnChanceWeight = weight; d.minStage = minStage; d.harmful = false; d.icon = sprite;
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            AssetDatabase.CopyAsset("Assets/Prefabs/Powerups/HeartPickup.prefab", prefabPath);
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        root.name = name + "Pickup";
        foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true)) sr.sprite = sprite;
        var pick = root.GetComponentInChildren<PowerupPickup>(true);
        if (pick != null) pick.powerupData = d;
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        d.visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        EditorUtility.SetDirty(d);
        return d;
    }

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
        return a;
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }
}
