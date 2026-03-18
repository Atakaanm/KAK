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
    private const string SPAWNER_SPRITE_ROOT = "Assets/Sprites/Spawner";

    // ─── Menu Giris Noktalari ──────────────────────────────────────────────

    [MenuItem("KacAtaKac/Setup - Tum Default Dataları Olustur")]
    public static void CreateAllDefaults()
    {
        EnsureFolder(DATA_FOLDER);

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

    [MenuItem("KacAtaKac/Spawner Spritelarini Yeniden Adlandir")]
    public static void RenameSpawnerSprites()
    {
        string[] directions = { "North", "South", "East", "West", "North_East", "North_West", "South_East", "South_West" };
        int totalRenamed = 0;

        foreach (string dir in directions)
        {
            string dirPath = Path.Combine(SPAWNER_SPRITE_ROOT, dir);
            if (!Directory.Exists(dirPath)) continue;

            string[] files = Directory.GetFiles(dirPath, "frame_*_delay-*.gif");
            System.Array.Sort(files); // Siralı işle

            int frameIndex = 1;
            foreach (string oldPath in files)
            {
                string newName = dir + "_Attack_" + frameIndex + ".gif";
                string newPath = Path.Combine(dirPath, newName);

                if (File.Exists(newPath))
                {
                    frameIndex++;
                    continue;
                }

                File.Move(oldPath, newPath);

                // Eski meta dosyasını sil (Unity yeni isimle üretecek)
                string oldMeta = oldPath + ".meta";
                if (File.Exists(oldMeta))
                    File.Delete(oldMeta);

                frameIndex++;
                totalRenamed++;
            }
        }

        AssetDatabase.Refresh();

        string msg = totalRenamed > 0
            ? totalRenamed + " sprite yeniden adlandirildi!\nReimport tamamlandi."
            : "Yeniden adlandirilacak dosya bulunamadi.\n(Ya daha once yapildi ya da dosyalar baska formatta.)";

        Debug.Log("[KacAtaKac] Sprite rename: " + msg);
        EditorUtility.DisplayDialog("Sprite Rename", msg, "Tamam");
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

        // ProjectileData referansi — eger daha once olusturulduysa bagla
        ProjectileData proj = AssetDatabase.LoadAssetAtPath<ProjectileData>(DATA_FOLDER + "/Rock_ProjectileData.asset");
        data.projectileData = proj;

        AssetDatabase.CreateAsset(data, path);
        Debug.Log("[KacAtaKac] Olusturuldu: " + path);
    }

    static void CreateDefaultPlayerData()
    {
        // Boy
        CreatePlayerDataAsset("Boy", PlayerType.Boy, 3, 5f, DATA_FOLDER + "/Boy_PlayerData.asset");
        // Girl (ileride farkli degerlerle)
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
        data.isLocked = (type != PlayerType.Boy); // Boy baslangicta acik, diger karakterler kilitli

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
        ProjectileData proj = AssetDatabase.LoadAssetAtPath<ProjectileData>(DATA_FOLDER + "/Rock_ProjectileData.asset");

        CreateStage("Stage_Kolay",   0,   200,  2, 1.0f, 1.0f,  proj, DATA_FOLDER + "/Stage_Kolay.asset");
        CreateStage("Stage_Orta",  200,   500,  3, 0.8f, 1.2f,  proj, DATA_FOLDER + "/Stage_Orta.asset");
        CreateStage("Stage_Zor",   500,  1000,  4, 0.65f, 1.5f, proj, DATA_FOLDER + "/Stage_Zor.asset");
        CreateStage("Stage_Cehennem", 1000, -1, 4, 0.5f, 2.0f,  proj, DATA_FOLDER + "/Stage_Cehennem.asset");
    }

    static void CreateStage(string name, int min, int max, int spawnerCount,
        float intervalMult, float speedMult, ProjectileData proj, string path)
    {
        if (AssetExists(path)) return;

        DifficultyStageData data = ScriptableObject.CreateInstance<DifficultyStageData>();
        data.stageName = name;
        data.minScore = min;
        data.maxScore = max;
        data.activeSpawnerCount = spawnerCount;
        data.shootIntervalMultiplier = intervalMult;
        data.projectileSpeedMultiplier = speedMult;
        if (proj != null) data.availableProjectiles = new ProjectileData[] { proj };

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

        // Referanslari bagla
        data.arenaData  = AssetDatabase.LoadAssetAtPath<ArenaData>(DATA_FOLDER + "/Dungeon_ArenaData.asset");
        data.playerData = AssetDatabase.LoadAssetAtPath<PlayerData>(DATA_FOLDER + "/Boy_PlayerData.asset");

        SpawnerData spawner = AssetDatabase.LoadAssetAtPath<SpawnerData>(DATA_FOLDER + "/RockThrower_SpawnerData.asset");
        if (spawner != null)
            data.spawnerDataList = new SpawnerData[] { spawner, spawner, spawner, spawner };

        DifficultyStageData kolay   = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage_Kolay.asset");
        DifficultyStageData orta    = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage_Orta.asset");
        DifficultyStageData zor     = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage_Zor.asset");
        DifficultyStageData cehennem = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(DATA_FOLDER + "/Stage_Cehennem.asset");

        data.difficultyStages = new DifficultyStageData[] { kolay, orta, zor, cehennem };

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
