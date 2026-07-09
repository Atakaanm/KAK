using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Unity Editorde ust menuye "KacAtaKac" secenegi ekler.
/// Tek tikla tum default data assetleri ve klasoru olusturur.
/// Bu dosya Editor/ klasoründe olmali — oyun build'ine dahil olmaz.
/// </summary>
public class KacAtaKacSetup : EditorWindow
{
    private const string DATA_FOLDER = "Assets/Data";

    // ─── Menu Giris Noktalari ──────────────────────────────────────────────

    [MenuItem("KacAtaKac/Setup - Tum Default Dataları Olustur")]
    public static void CreateAllDefaults()
    {
        EnsureFolder(DATA_FOLDER);
        EnsureFolder(DATA_FOLDER + "/Powerups");

        CreateDefaultProjectileData();
        CreateDefaultSpawnerData();
        CreateDefaultPlayerData();
        CreateDefaultArenaData();
        CreateDefaultDifficultyStages();
        CreateDefaultLevelData();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[KacAtaKac] Tum default datalar olusturuldu: " + DATA_FOLDER);
        EditorUtility.DisplayDialog(
            "KacAtaKac Setup Tamamlandi!",
            "Tum default data objeleri '" + DATA_FOLDER + "' klasorune olusturuldu.\n\n" +
            "Simdi her assetin Inspector'unda sprite ve prefab referanslarini doldurun.",
            "Tamam"
        );
    }

    // ─── Yardimci: Klasor Olusturma ───────────────────────────────────────

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folderName);
            Debug.Log("[KacAtaKac] Klasor olusturuldu: " + path);
        }
    }

    // ─── Default Asset Olusturuculari ─────────────────────────────────────

    static void CreateDefaultProjectileData()
    {
        string path = DATA_FOLDER + "/Rock_ProjectileData.asset";
        if (AssetExists(path)) return;

        ProjectileData data = ScriptableObject.CreateInstance<ProjectileData>();
        data.projectileName = "Rock";
        data.speed = 1.5f;
        data.damage = 1;
        data.lifeTime = 5f;
        data.rotationSpeed = 0f;

        AssetDatabase.CreateAsset(data, path);
        Debug.Log("[KacAtaKac] Olusturuldu: " + path);
    }

    static void CreateDefaultSpawnerData()
    {
        string path = DATA_FOLDER + "/RockThrower_SpawnerData.asset";
        if (AssetExists(path)) return;

        SpawnerData data = ScriptableObject.CreateInstance<SpawnerData>();
        data.spawnerName = "RockThrower";
        data.shootInterval = 1.5f;

        ProjectileData proj = AssetDatabase.LoadAssetAtPath<ProjectileData>(DATA_FOLDER + "/Rock_ProjectileData.asset");
        data.projectileData = proj;

        AssetDatabase.CreateAsset(data, path);
        Debug.Log("[KacAtaKac] Olusturuldu: " + path);
    }

    static void CreateDefaultPlayerData()
    {
        CreatePlayerDataAsset("Boy", PlayerType.Boy, 3, 5f, DATA_FOLDER + "/Boy_PlayerData.asset");
        CreatePlayerDataAsset("Girl", PlayerType.Girl, 3, 6f, DATA_FOLDER + "/Girl_PlayerData.asset");
    }

    static void CreatePlayerDataAsset(string pName, PlayerType type, int health, float speed, string path)
    {
        if (AssetExists(path)) return;

        PlayerData data = ScriptableObject.CreateInstance<PlayerData>();
        data.playerName = pName;
        data.playerType = type;
        data.maxHealth = health;
        data.moveSpeed = speed;
        data.invincibilityDuration = 0.35f;
        data.isLocked = (type != PlayerType.Boy);

        AssetDatabase.CreateAsset(data, path);
        Debug.Log("[KacAtaKac] Olusturuldu: " + path);
    }

    static void CreateDefaultArenaData()
    {
        string path = DATA_FOLDER + "/Dungeon_ArenaData.asset";
        if (AssetExists(path)) return;

        ArenaData data = ScriptableObject.CreateInstance<ArenaData>();
        data.arenaName = "Dungeon";
        data.wallThickness = 0.5f;
        data.wallInset = 0.15f;
        data.spawnerInsetX = 1.0f;
        data.spawnerInsetY = 1.0f;
        data.spawnerTopDepthOffset = 0.35f;

        AssetDatabase.CreateAsset(data, path);
        Debug.Log("[KacAtaKac] Olusturuldu: " + path);
    }

    static void CreateDefaultDifficultyStages()
    {
        // 6 asamali yeni zorluk sistemi
        //                    isim              min   max   spawner  interval  projSpd  projScale  plrSpd  scoreMult
        CreateStage6("Stage1_Baslangic",    0,    49,  2,   1.0f,    1.0f,    1.0f,     1.0f,   1.0f,  DATA_FOLDER + "/Stage1_Baslangic.asset");
        CreateStage6("Stage2_Kolay",       50,   149,  2,   0.85f,   1.15f,   1.0f,     1.05f,  1.1f,  DATA_FOLDER + "/Stage2_Kolay.asset");
        CreateStage6("Stage3_Orta",       150,   349,  3,   0.7f,    1.3f,    1.15f,    1.1f,   1.2f,  DATA_FOLDER + "/Stage3_Orta.asset");
        CreateStage6("Stage4_Zor",        350,   599,  3,   0.55f,   1.5f,    1.3f,     1.15f,  1.3f,  DATA_FOLDER + "/Stage4_Zor.asset");
        CreateStage6("Stage5_Cehennem",   600,   999,  4,   0.4f,    1.75f,   1.45f,    1.2f,   1.5f,  DATA_FOLDER + "/Stage5_Cehennem.asset");
        CreateStage6("Stage6_Imkansiz",  1000,    -1,  4,   0.3f,    2.0f,    1.6f,     1.25f,  1.8f,  DATA_FOLDER + "/Stage6_Imkansiz.asset");
    }

    static void CreateStage6(string name, int min, int max, int spawnerCount,
        float intervalMult, float speedMult, float scaleMult,
        float playerSpeedMult, float scoreMult, string path)
    {
        if (AssetExists(path)) return;

        DifficultyStageData data = ScriptableObject.CreateInstance<DifficultyStageData>();
        data.stageName = name;
        data.minScore = min;
        data.maxScore = max;
        data.activeSpawnerCount = spawnerCount;
        data.shootIntervalMultiplier = intervalMult;
        data.projectileSpeedMultiplier = speedMult;
        data.projectileScaleMultiplier = scaleMult;
        data.playerSpeedMultiplier = playerSpeedMult;
        data.scoreSpeedMultiplier = scoreMult;

        AssetDatabase.CreateAsset(data, path);
        Debug.Log("[KacAtaKac] Olusturuldu: " + path);
    }

    static void CreateDefaultLevelData()
    {
        string path = DATA_FOLDER + "/Endless_Level1_LevelData.asset";
        if (AssetExists(path)) return;

        LevelData data = ScriptableObject.CreateInstance<LevelData>();
        data.levelName = "Endless Level 1";
        data.levelIndex = 0;
        data.levelType = LevelType.Endless;
        data.enableEndlessScore = true;
        data.enableDifficulty = true;

        data.arenaData  = AssetDatabase.LoadAssetAtPath<ArenaData>(DATA_FOLDER + "/Dungeon_ArenaData.asset");
        data.playerData = AssetDatabase.LoadAssetAtPath<PlayerData>(DATA_FOLDER + "/Boy_PlayerData.asset");

        SpawnerData spawner = AssetDatabase.LoadAssetAtPath<SpawnerData>(DATA_FOLDER + "/RockThrower_SpawnerData.asset");
        if (spawner != null)
            data.spawnerDataList = new SpawnerData[] { spawner, spawner, spawner, spawner };

        // 6 asamali zorluk sistemi
        DifficultyStageData s1 = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage1_Baslangic.asset");
        DifficultyStageData s2 = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage2_Kolay.asset");
        DifficultyStageData s3 = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage3_Orta.asset");
        DifficultyStageData s4 = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage4_Zor.asset");
        DifficultyStageData s5 = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage5_Cehennem.asset");
        DifficultyStageData s6 = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage6_Imkansiz.asset");

        data.difficultyStages = new DifficultyStageData[] { s1, s2, s3, s4, s5, s6 };

        AssetDatabase.CreateAsset(data, path);
        Debug.Log("[KacAtaKac] Olusturuldu: " + path);
    }

    static bool AssetExists(string path)
    {
        if (File.Exists(Path.Combine(Application.dataPath, "..", path)))
        {
            Debug.Log("[KacAtaKac] Zaten mevcut, atlaniyor: " + path);
            return true;
        }
        return false;
    }
}
