using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static KakIceSetup;

/// <summary>
/// Faz 13 F1: Futbol Arenası dünyası. Önce `python3 tools/kak_gen_football.py all`.
/// Kurar: mermiler (top: seker, sarı kart: yavaşlatır + sayaç, kırmızı kart: can, düşen top, dev top), 6 kademe
/// (zindan süreleri, ×0,95 hız), hakem fırlatıcısı (Ata kareleri siyah formaya boyalı), tema (olaylar: TOP YAĞMURU!,
/// DEV TOP!, KART YAĞMURU!, HAKEM DÜDÜĞÜ!), seviye, katalog (10. oyunda açılır). Tekrar çalıştırılabilir.
/// </summary>
public static class KakFootballSetup
{
    const string Dir = "Assets/Data/Worlds/Football/";
    const string ArenaSprite = "Assets/Sprites/Arena/Football_arena_01.png";
    const string PreviewSprite = "Assets/Sprites/Arena/Football_preview.png";
    const string RefDir = "Assets/Sprites/Spawner_Football/";
    static readonly string[] Dirs = { "North", "South", "East", "West", "North_East", "North_West", "South_East", "South_West" };

    [MenuItem("KacAtaKac/Dünyalar/Futbol Arenasını Kur")]
    public static string Setup()
    {
        EnsureFolder("Assets/Data/Worlds", "Football");
        EnsureFolder("Assets/Data/Worlds/Football", "Stages");
        Imports();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        Sprite Art(string n) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Projectiles/" + n + ".png");

        // ── Mermiler ──
        var ball = Proj("Ball_Top", "Top", ProjectileMotion.Bounce, 1.55f, 0.95f, prefab, Art("ball"));
        ball.bounces = 2;
        // Kullanıcı: "sarı kartlar karakteri takip etsin bir yere kadar… kırmızılar daha yavaş ama tek atsın"
        var yellow = Proj("YellowCard_SariKart", "Sarı Kart", ProjectileMotion.Homing, 1.35f, 1f, prefab, Art("card_yellow"));
        yellow.homingTurnRate = 75f; yellow.homingDuration = 1.4f; // bir süre takip eder, sonra düz gider (kaçılabilir)
        yellow.effect = ProjectileEffect.YellowCard; yellow.effectStrength = 0.6f; yellow.effectDuration = 1.8f;
        var red = Proj("RedCard_KirmiziKart", "Kırmızı Kart", ProjectileMotion.Straight, 1.05f, 1.05f, prefab, Art("card_red"));
        red.effect = ProjectileEffect.RedCard; // yavaş, düz; değerse oyundan atar
        // Kullanıcı: "tehlike durumu yukarıdan şişe gelsin" → tribünden şişeler ("!" uyarılı)
        var falling = Proj("Bottle_Sise", "Şişe", ProjectileMotion.Meteor, 0f, 1.1f, prefab, Art("bottle"));
        falling.meteorFallTime = 1.2f; falling.meteorStartHeight = 7f; falling.meteorRadius = 0.45f;
        falling.warningSprite = Art("warn_mark");
        var big = Proj("BigBall_DevTop", "Dev Top", ProjectileMotion.Straight, 1.05f, 1.6f, prefab, Art("ball"));
        foreach (var d in new[] { ball, yellow, red, falling, big }) EditorUtility.SetDirty(d);

        // ── Kademeler: zindanın süreleri, mermiler ×0,95; kırmızı kart seyrek ve geç (yumuşak: 2 sarı ile gelir) ──
        string[] src = { "Stage1_Baslangic", "Stage2_Kolay", "Stage3_Orta", "Stage4_Zor", "Stage5_Cehennem", "Stage6_Imkansiz" };
        var lists = new (ProjectileData d, float w)[][]
        {
            new[] { (ball, 1f) },
            new[] { (ball, 2f), (yellow, 1f) },
            new[] { (ball, 1.6f), (yellow, 1.2f), (falling, 0.4f) },
            new[] { (ball, 1.4f), (yellow, 1.2f), (falling, 0.6f), (red, 0.15f) },
            new[] { (ball, 1.3f), (yellow, 1.3f), (falling, 0.8f), (red, 0.25f), (big, 0.35f) },
            new[] { (ball, 1.2f), (yellow, 1.4f), (falling, 1f), (red, 0.35f), (big, 0.5f) },
        };
        var stages = new DifficultyStageData[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            var from = AssetDatabase.LoadAssetAtPath<DifficultyStageData>("Assets/Data/" + src[i] + ".asset");
            string path = Dir + "Stages/Football_" + src[i] + ".asset";
            var st = LoadOrCreate<DifficultyStageData>(path);
            if (from != null) EditorUtility.CopySerialized(from, st);
            st.name = "Football_" + src[i];
            st.projectileSpeedMultiplier *= 0.95f;
            st.availableProjectiles = new ProjectileData[lists[i].Length];
            st.projectileWeights = new float[lists[i].Length];
            for (int k = 0; k < lists[i].Length; k++) { st.availableProjectiles[k] = lists[i][k].d; st.projectileWeights[k] = lists[i][k].w; }
            EditorUtility.SetDirty(st);
            stages[i] = st;
        }

        // ── Hakem ──
        var rockThrower = AssetDatabase.LoadAssetAtPath<SpawnerData>("Assets/Data/RockThrower_SpawnerData.asset");
        var refData = LoadOrCreate<SpawnerData>(Dir + "Referee_SpawnerData.asset");
        if (rockThrower != null) EditorUtility.CopySerialized(rockThrower, refData);
        refData.name = "Referee_SpawnerData";
        refData.spawnerName = "Referee";
        refData.projectileData = ball;
        refData.idleNorth = Idle("North"); refData.idleSouth = Idle("South"); refData.idleEast = Idle("East"); refData.idleWest = Idle("West");
        refData.idleNorthEast = Idle("North_East"); refData.idleNorthWest = Idle("North_West");
        refData.idleSouthEast = Idle("South_East"); refData.idleSouthWest = Idle("South_West");
        refData.attackNorth = Attack("North"); refData.attackSouth = Attack("South"); refData.attackEast = Attack("East"); refData.attackWest = Attack("West");
        refData.attackNorthEast = Attack("North_East"); refData.attackNorthWest = Attack("North_West");
        refData.attackSouthEast = Attack("South_East"); refData.attackSouthWest = Attack("South_West");
        EditorUtility.SetDirty(refData);

        // ── Tema ──
        var theme = LoadOrCreate<WorldTheme>(Dir + "Football_Theme.asset");
        theme.themeId = "football";
        theme.arenaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArenaSprite);
        theme.playableAreaNormalized = new Rect(0.0795f, 0.0795f, 0.841f, 0.841f);
        theme.useSpawnerAnchors = true;
        theme.floorFriction = 1f; theme.floorSpeedMultiplier = 1f; theme.verticalSpeedMultiplier = 1f;
        theme.backdropTile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Football/backdrop_tile.png");
        theme.corridorTile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Football/corridor_tile.png");
        theme.corridorWall = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Football/corridor_wall.png");
        theme.ledgeTile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Football/ledge_tile.png");
        theme.cameraBackground = new Color(0.08f, 0.09f, 0.15f, 1f);
        theme.arenaTint = Color.white;
        theme.torches = false;   // projektörler arenanın çiziminde
        theme.darkness = 0f;
        theme.coldEnabled = false; theme.snowfall = false;
        theme.eventRainData = falling; theme.eventRainTitleKey = "ev_bottles";
        theme.eventRollingData = big; theme.eventRollingTitleKey = "ev_bigball";
        theme.eventSwarmData = ball; theme.eventSwarmTitleKey = "ev_counter"; // takip eden sarı kart sürüsü kaçılmaz olurdu
        theme.eventCalmTitleKey = "ev_whistle";
        theme.redCardEliminates = true;
        theme.goalChance = true;          // F2: ara sıra serbest top → üst kaleye GOL!
        theme.goalBallSprite = Art("ball");
        EditorUtility.SetDirty(theme);

        // ── Seviye ──
        var dungeonLevel = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Endless_Level1_LevelData.asset");
        var level = LoadOrCreate<LevelData>(Dir + "Endless_Football_LevelData.asset");
        if (dungeonLevel != null) EditorUtility.CopySerialized(dungeonLevel, level);
        level.name = "Endless_Football_LevelData";
        level.levelName = "Endless Football";
        level.theme = theme;
        level.spawnerDataList = new[] { refData, refData, refData, refData };
        level.difficultyStages = stages;
        var pu = new List<PowerupData>();
        foreach (var n in new[] { "HeartData", "ShieldData", "GhostData", "SloMoData", "SpeedData", "ShackleData", "InvisibleData" })
        {
            var d = AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/" + n + ".asset");
            if (d != null) pu.Add(d);
        }
        level.availablePowerups = pu.ToArray();
        EditorUtility.SetDirty(level);

        RegisterWorld(new EndlessWorlds.World { id = "football", nameKey = "world_football", descKey = "worlddesc_football", level = level,
                                               preview = AssetDatabase.LoadAssetAtPath<Sprite>(PreviewSprite), unlockGames = 10 });
        AssetDatabase.SaveAssets();
        return "[KakFootballSetup] Futbol Arenası: 5 mermi, 6 kademe, hakem, tema, olaylar, seviye, katalog";
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
        d.noSpin = false;
        d.growPerSecond = 0f; d.maxGrowScale = 1f; d.growSlowdown = 1f; d.growHitShare = 1f;
        d.catchable = false;
        d.effect = ProjectileEffect.Damage;
        d.glowInDark = false;
        return d;
    }

    static void Imports()
    {
        SpriteImport(ArenaSprite, 100f, true);
        var ti = AssetImporter.GetAtPath(ArenaSprite) as TextureImporter;
        if (ti != null) { ti.maxTextureSize = 2048; ti.SaveAndReimport(); }
        SpriteImport(PreviewSprite, 100f, false);
        SpriteImport("Assets/Resources/StatusYellow.png", 100f, true);
        SpriteImport("Assets/Resources/StatusRed.png", 100f, true);
        SpriteImport("Assets/Resources/BallMarker.png", KakArtImportRules.ArtPixelPPU, true);
        var playerRef = AssetImporter.GetAtPath("Assets/Sprites/Player/South/south.png") as TextureImporter;
        foreach (var d in Dirs)
        {
            CopyImport(playerRef, RefDir + d + "/idle.png");
            for (int i = 1; i <= 4; i++) CopyImport(playerRef, RefDir + d + "/attack_" + i + ".png");
        }
        AssetDatabase.Refresh();
    }

    static Sprite Idle(string dir) => AssetDatabase.LoadAssetAtPath<Sprite>(RefDir + dir + "/idle.png");
    static Sprite[] Attack(string dir)
    {
        var a = new Sprite[4];
        for (int i = 0; i < 4; i++) a[i] = AssetDatabase.LoadAssetAtPath<Sprite>(RefDir + dir + "/attack_" + (i + 1) + ".png");
        return a;
    }
}
