using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Oyun sahnesinin (Game) yönetici yapısını kalıcı ve tekrarlanabilir şekilde kurar.
/// Runtime'daki "yoksa yarat" yedeklerine gerek kalmaz; referanslar Inspector'da görünür.
///
/// Menü: KacAtaKac/Sahne Yöneticilerini Kur
/// Köprü: python3 tools/kak_bridge.py invoke KakSceneSetup SetupManagers
/// </summary>
public static class KakSceneSetup
{
    const string GameScenePath = KakEditorUtil.GameScenePath;
    const string DefaultLevelPath = "Assets/Data/Endless_Level1_LevelData.asset";

    [MenuItem("KacAtaKac/Sahne Yöneticilerini Kur")]
    public static void SetupManagersMenu()
    {
        Debug.Log(SetupManagers());
    }

    [MenuItem("KacAtaKac/Oyuncu Çarpışmasını Kur")]
    public static void SetupPlayerHitboxMenu() => Debug.Log(SetupPlayerHitbox());

    /// <summary>
    /// Oyuncuya PlayerHitbox ekler: ayak izi (duvar) + gövde (taş) collider'ları. Sahneyi kaydeder.
    /// Köprü: python3 tools/kak_bridge.py invoke KakSceneSetup SetupPlayerHitbox
    /// </summary>
    public static string SetupPlayerHitbox()
    {
        if (Application.isPlaying) return "HATA: Play modunda çalıştırılamaz.";
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != GameScenePath)
        {
            KakEditorUtil.SaveNamedScenes();
            scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        }
        var health = Object.FindAnyObjectByType<PlayerHealth>();
        if (health == null) return "HATA: PlayerHealth bulunamadı.";
        var go = health.gameObject;
        var hb = go.GetComponent<PlayerHitbox>();
        if (hb == null) hb = Undo.AddComponent<PlayerHitbox>(go);
        Undo.RegisterFullObjectHierarchyUndo(go, "Oyuncu çarpışması");
        hb.Apply();
        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        var foot = go.GetComponent<CircleCollider2D>();
        return $"[KakSceneSetup] PlayerHitbox kuruldu: ayak r={foot.radius:F3} (yerel) ofset={foot.offset}, gövde={hb.HurtCollider.size} (yerel)";
    }

    public static string SetupManagers()
    {
        if (Application.isPlaying) return "HATA: Play modunda çalıştırılamaz.";

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != GameScenePath)
        {
            KakEditorUtil.SaveNamedScenes();
            scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        }

        var log = new StringBuilder("[KakSceneSetup] ");

        // Kök obje
        var root = GameObject.Find("Managers");
        if (root == null)
        {
            root = new GameObject("Managers");
            Undo.RegisterCreatedObjectUndo(root, "Managers");
            log.Append("Managers oluşturuldu. ");
        }

        var lm = GetOrCreate<LevelManager>(root, "LevelManager", log);
        var dm = GetOrCreate<DifficultyManager>(root, "DifficultyManager", log);
        var pool = GetOrCreate<ProjectilePool>(root, "ProjectilePool", log);
        var ps = GetOrCreate<PowerupSpawner>(root, "PowerupSpawner", log);

        // LevelManager referansları
        Undo.RecordObject(lm, "LevelManager referansları");
        lm.defaultLevel = AssetDatabase.LoadAssetAtPath<LevelData>(DefaultLevelPath);
        lm.playerMovement = Object.FindAnyObjectByType<PlayerMovement2D>();
        lm.playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
        lm.playerVisual = Object.FindAnyObjectByType<PlayerDirectionSprite>();
        lm.arenaLayout = Object.FindAnyObjectByType<ArenaAutoLayout>();
        lm.spawners = LevelManager.SortByCorner(Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None));
        lm.spawnerVisuals = new SpawnerDirectionAnimator[lm.spawners.Length];
        for (int i = 0; i < lm.spawners.Length; i++)
            lm.spawnerVisuals[i] = lm.spawners[i].spawnerVisual;
        lm.difficultyManager = dm;
        lm.powerupSpawner = ps;
        // Bölüm modu/tema referansları (Faz 2B): sahnede hazır olanlar
        lm.screenComposer = Object.FindAnyObjectByType<ScreenComposer>();
        lm.dungeonFrame = Object.FindAnyObjectByType<DungeonFrame>();
        lm.projectilePrefabDefault = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        lm.enemyShadow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/Black.png");
        EditorUtility.SetDirty(lm);

        // GameManager referansları
        var gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            Undo.RecordObject(gm, "GameManager referansları");
            gm.levelManager = lm;
            gm.difficultyManager = dm;
            if (gm.scoreManager == null) gm.scoreManager = gm.GetComponent<ScoreManager>();
            EditorUtility.SetDirty(gm);
        }
        else
        {
            log.Append("UYARI: GameManager bulunamadı! ");
        }

        // Zorluk yöneticisi: skor bağlantısı (aşamalar ve spawner'lar LevelData'dan gelir)
        Undo.RecordObject(dm, "DifficultyManager");
        if (gm != null) dm.scoreManager = gm.scoreManager;
        EditorUtility.SetDirty(dm);

        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);

        log.Append("defaultLevel=" + (lm.defaultLevel != null ? lm.defaultLevel.name : "YOK"));
        log.Append(", spawner sırası=");
        foreach (var s in lm.spawners) log.Append(s.name + " ");
        log.Append(saved ? "| sahne kaydedildi." : "| SAHNE KAYDEDİLEMEDİ!");
        return log.ToString();
    }

    static T GetOrCreate<T>(GameObject root, string name, StringBuilder log) where T : Component
    {
        var existing = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
        if (existing != null)
        {
            if (existing.transform.parent != root.transform && existing.transform.root != root.transform)
            {
                Undo.SetTransformParent(existing.transform, root.transform, "Yöneticiyi taşı");
                log.Append(name + " Managers altına taşındı. ");
            }
            return existing;
        }

        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(root.transform, false);
        log.Append(name + " oluşturuldu. ");
        return go.AddComponent<T>();
    }
}
