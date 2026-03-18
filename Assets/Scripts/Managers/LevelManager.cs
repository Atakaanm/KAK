using UnityEngine;

/// <summary>
/// Sahne basinda LevelData'yi okuyarak oyunu kuran manager.
/// Player'a uygun PlayerData'yi, Arena'ya uygun ArenaData'yi,
/// Spawner'lara uygun SpawnerData'lari uygular.
/// Endless modda DifficultyManager'i aktiflestirir.
///
/// Kullanim: Sahneye bos bir GameObject koy, bu scripti ekle,
/// LevelData referansini ata, sahne objelerini baglat.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Aktif Level")]
    public LevelData currentLevel; // Inspector'dan veya menu'den atanir

    [Header("Sahne Referanslari")]
    public PlayerMovement2D playerMovement;
    public PlayerHealth playerHealth;
    public PlayerDirectionSprite playerVisual;
    public ArenaAutoLayout arenaLayout;
    public CornerShooter[] spawners;
    public SpawnerDirectionAnimator[] spawnerVisuals;
    public DifficultyManager difficultyManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (currentLevel != null)
        {
            ApplyLevelData(currentLevel);
        }
    }

    /// <summary>
    /// Verilen LevelData'yi sahneye uygular.
    /// Butun oyun elemanlari bu data'ya gore ayarlanir.
    /// </summary>
    public void ApplyLevelData(LevelData level)
    {
        if (level == null) return;

        currentLevel = level;

        Debug.Log("[LevelManager] Level yukleniyor: " + level.levelName);

        // --- PLAYER AYARLARI ---
        ApplyPlayerData(level.playerData);

        // --- ARENA AYARLARI ---
        ApplyArenaData(level.arenaData);

        // --- SPAWNER AYARLARI ---
        ApplySpawnerData(level.spawnerDataList);

        // --- ZORLUK SISTEMI ---
        if (level.levelType == LevelType.Endless && level.enableDifficulty)
        {
            SetupDifficulty(level.difficultyStages);
        }

        Debug.Log("[LevelManager] Level hazirlandi: " + level.levelName);
    }

    /// <summary>
    /// PlayerData'yi sahneye uygular.
    /// Hareket hizi, can, gorsel sprite'lar buradan alinir.
    /// </summary>
    void ApplyPlayerData(PlayerData data)
    {
        if (data == null) return;

        // Hareket hizi
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = data.moveSpeed;
        }

        // Can
        if (playerHealth != null)
        {
            playerHealth.maxHealth = data.maxHealth;
            playerHealth.invincibilityDuration = data.invincibilityDuration;
        }

        // Gorsel — DirectionData yapisini PlayerDirectionSprite'a aktar
        if (playerVisual != null)
        {
            playerVisual.north.idle      = data.north.idle;
            playerVisual.south.idle      = data.south.idle;
            playerVisual.east.idle       = data.east.idle;
            playerVisual.west.idle       = data.west.idle;
            playerVisual.northEast.idle  = data.northEast.idle;
            playerVisual.northWest.idle  = data.northWest.idle;
            playerVisual.southEast.idle  = data.southEast.idle;
            playerVisual.southWest.idle  = data.southWest.idle;

            playerVisual.north.runFrames     = data.north.runFrames;
            playerVisual.south.runFrames     = data.south.runFrames;
            playerVisual.east.runFrames      = data.east.runFrames;
            playerVisual.west.runFrames      = data.west.runFrames;
            playerVisual.northEast.runFrames = data.northEast.runFrames;
            playerVisual.northWest.runFrames = data.northWest.runFrames;
            playerVisual.southEast.runFrames = data.southEast.runFrames;
            playerVisual.southWest.runFrames = data.southWest.runFrames;
        }

        Debug.Log("[LevelManager] Player ayarlandi: " + data.playerName);
    }

    /// <summary>
    /// ArenaData'yi sahneye uygular.
    /// Duvar kalinliklari, spawner pozisyonlari, arena sprite'i burada ayarlanir.
    /// </summary>
    void ApplyArenaData(ArenaData data)
    {
        if (data == null || arenaLayout == null) return;

        arenaLayout.wallThickness = data.wallThickness;
        arenaLayout.wallInset = data.wallInset;
        arenaLayout.spawnerInsetX = data.spawnerInsetX;
        arenaLayout.spawnerInsetY = data.spawnerInsetY;
        arenaLayout.spawnerTopDepthOffset = data.spawnerTopDepthOffset;

        // Arena sprite'ini degistir
        if (data.arenaSprite != null && arenaLayout.arenaSpriteRenderer != null)
        {
            arenaLayout.arenaSpriteRenderer.sprite = data.arenaSprite;
        }

        Debug.Log("[LevelManager] Arena ayarlandi: " + data.arenaName);
    }

    /// <summary>
    /// SpawnerData listesini spawner'lara uygular.
    /// Ates araligi, mermi tipi ve gorsel sprite setleri burada kurulur.
    /// </summary>
    void ApplySpawnerData(SpawnerData[] dataList)
    {
        if (dataList == null || spawners == null) return;

        int count = Mathf.Min(dataList.Length, spawners.Length);

        for (int i = 0; i < count; i++)
        {
            SpawnerData data = dataList[i];
            if (data == null) continue;

            // Ates ayarlari
            if (spawners[i] != null)
            {
                spawners[i].shootInterval = data.shootInterval;

                // Mermi prefabini ProjectileData'dan al
                if (data.projectileData != null && data.projectileData.projectilePrefab != null)
                {
                    spawners[i].projectilePrefab = data.projectileData.projectilePrefab;
                }
            }

            // Gorsel sprite'lar
            if (i < spawnerVisuals.Length && spawnerVisuals[i] != null)
            {
                SpawnerDirectionAnimator vis = spawnerVisuals[i];

                // Idle sprite'lar
                vis.north.idle = data.idleNorth;
                vis.south.idle = data.idleSouth;
                vis.east.idle = data.idleEast;
                vis.west.idle = data.idleWest;
                vis.northEast.idle = data.idleNorthEast;
                vis.northWest.idle = data.idleNorthWest;
                vis.southEast.idle = data.idleSouthEast;
                vis.southWest.idle = data.idleSouthWest;

                // Attack frame'ler
                vis.north.attackFrames = data.attackNorth;
                vis.south.attackFrames = data.attackSouth;
                vis.east.attackFrames = data.attackEast;
                vis.west.attackFrames = data.attackWest;
                vis.northEast.attackFrames = data.attackNorthEast;
                vis.northWest.attackFrames = data.attackNorthWest;
                vis.southEast.attackFrames = data.attackSouthEast;
                vis.southWest.attackFrames = data.attackSouthWest;
            }
        }

        Debug.Log("[LevelManager] " + count + " spawner ayarlandi.");
    }

    /// <summary>
    /// Zorluk asamalarini DifficultyManager'a aktarir.
    /// </summary>
    void SetupDifficulty(DifficultyStageData[] stages)
    {
        if (difficultyManager == null || stages == null || stages.Length == 0) return;

        difficultyManager.stages = stages;
        Debug.Log("[LevelManager] Zorluk sistemi kuruldu: " + stages.Length + " asama.");
    }
}
