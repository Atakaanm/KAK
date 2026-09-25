using UnityEngine;

/// <summary>
/// Sahne basinda LevelData'yi okuyarak oyunu kuran manager.
/// Player'a uygun PlayerData'yi, Arena'ya uygun ArenaData'yi,
/// Spawner'lara uygun SpawnerData'lari uygular.
/// Endless modda DifficultyManager'i aktiflestirir.
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
    public PowerupSpawner powerupSpawner;
    public WaveManager waveManager;

    [Header("Varsayılan Level (Build için)")]
    public LevelData defaultLevel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Önceki sahneden kalan statik mermi sınırlarını sıfırla
        Projectile.ResetBounds();

        // --- REFERANSLAR EKSİKSE OTOMATİK BUL ---
        if (playerMovement == null) playerMovement = FindAnyObjectByType<PlayerMovement2D>();
        if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerVisual == null) playerVisual = FindAnyObjectByType<PlayerDirectionSprite>();
        if (arenaLayout == null) arenaLayout = FindAnyObjectByType<ArenaAutoLayout>();
        if (spawners == null || spawners.Length == 0) spawners = FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
        // Deterministik sıra: zorluk kademeleri spawner'ları bu sırayla açar (önce çaprazlar)
        spawners = SortByCorner(spawners);
        // Görseller her zaman kendi spawner'ıyla eşleşsin
        spawnerVisuals = new SpawnerDirectionAnimator[spawners.Length];
        for (int i = 0; i < spawners.Length; i++)
            spawnerVisuals[i] = spawners[i] != null ? spawners[i].spawnerVisual : null;

        // PowerupSpawner otomatik bul
        if (powerupSpawner == null)
        {
            powerupSpawner = FindAnyObjectByType<PowerupSpawner>();
            if (powerupSpawner == null)
            {
                GameObject spawnerObj = new GameObject("PowerupSpawner");
                powerupSpawner = spawnerObj.AddComponent<PowerupSpawner>();
                spawnerObj.transform.parent = this.transform;
            }
        }

        // DifficultyManager otomatik bul
        if (difficultyManager == null)
        {
            difficultyManager = FindAnyObjectByType<DifficultyManager>();
        }
        if (difficultyManager == null)
        {
            difficultyManager = new GameObject("DifficultyManager").AddComponent<DifficultyManager>();
            difficultyManager.transform.parent = this.transform;
            Debug.LogWarning("[LevelManager] DifficultyManager sahnede yoktu, yedek olarak oluşturuldu.");
        }

        // WaveManager otomatik bul
        if (waveManager == null)
        {
            waveManager = FindAnyObjectByType<WaveManager>();
        }

        // ProjectilePool — sahnede yoksa yedek olarak yarat
        if (ProjectilePool.Instance == null && FindAnyObjectByType<ProjectilePool>() == null)
        {
            GameObject poolObj = new GameObject("ProjectilePool");
            poolObj.AddComponent<ProjectilePool>();
            poolObj.transform.parent = this.transform;
            Debug.LogWarning("[LevelManager] ProjectilePool sahnede yoktu, yedek olarak oluşturuldu.");
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Spawner'ları köşe önceliğine göre sıralar: SolAlt, SağÜst, SolÜst, SağAlt.
    /// 2 aktif spawner'lı kademede oyuncu çapraz ateş alır; sıra her oyunda aynıdır.
    /// </summary>
    public static CornerShooter[] SortByCorner(CornerShooter[] list)
    {
        if (list == null || list.Length < 2) return list;
        Vector3 center = Vector3.zero;
        int n = 0;
        foreach (var s in list) if (s != null) { center += s.transform.position; n++; }
        if (n > 0) center /= n;

        var sorted = (CornerShooter[])list.Clone();
        System.Array.Sort(sorted, (a, b) => CornerRank(a, center).CompareTo(CornerRank(b, center)));
        return sorted;
    }

    static int CornerRank(CornerShooter s, Vector3 center)
    {
        if (s == null) return 99;
        Vector3 d = s.transform.position - center;
        bool left = d.x < 0f, bottom = d.y < 0f;
        if (left && bottom) return 0;   // SolAlt
        if (!left && !bottom) return 1; // SağÜst
        if (left) return 2;             // SolÜst
        return 3;                       // SağAlt
    }

    void Start()
    {
        // Menueden gelen level datasi varsa onu kullan
        if (GameSettings.SelectedLevel != null)
        {
            currentLevel = GameSettings.SelectedLevel;
        }

        // Build'de menü atlandıysa defaultLevel'i kullan
        if (currentLevel == null && defaultLevel != null)
        {
            currentLevel = defaultLevel;
            KakLog.Info("[LevelManager] defaultLevel kullanılıyor: " + defaultLevel.levelName);
        }

        if (currentLevel != null)
        {
            ApplyLevelData(currentLevel);
        }
        else
        {
            Debug.LogError("🚨 [LevelManager] DİKKAT: HİÇBİR LEVEL DATA DOSYASI BULUNAMADI!");
        }

        SetupProjectileBounds();
    }



    /// <summary>
    /// Arena sınırlarını projectile sistemine bildirir.
    /// </summary>
    void SetupProjectileBounds()
    {
        if (arenaLayout != null && arenaLayout.arenaSpriteRenderer != null)
        {
            // Mermiler duvarın iç yüzüne ulaşınca havuza döner (duvarın/HUD'ın üstünden uçmaz)
            Rect play = arenaLayout.PlayableWorldRect;
            Projectile.SetArenaBounds(new Bounds(play.center, new Vector3(play.width, play.height, 1f)));
            KakLog.Info("[LevelManager] Arena sınırları Projectile sistemine bildirildi.");
        }
    }

    /// <summary>
    /// Verilen LevelData'yi sahneye uygular.
    /// </summary>
    public void ApplyLevelData(LevelData level)
    {
        if (level == null) return;

        currentLevel = level;

        KakLog.Info("[LevelManager] Level yukleniyor: " + level.levelName);

        // --- PLAYER AYARLARI ---
        ApplyPlayerData(level.playerData);

        // --- ARENA AYARLARI ---
        ApplyArenaData(level.arenaData);

        // --- SPAWNER AYARLARI ---
        ApplySpawnerData(level.spawnerDataList);

        // --- ZORLUK SISTEMI (Endless mod) ---
        if (level.levelType == LevelType.Endless && level.enableDifficulty)
        {
            System.Array.Sort(level.difficultyStages, (a, b) => a.minScore.CompareTo(b.minScore));
            SetupDifficulty(level.difficultyStages);
        }

        // --- DALGA SISTEMI (Stage mod) ---
        if (level.levelType == LevelType.Stage && level.waves != null && level.waves.Length > 0)
        {
            SetupWaves(level.waves);
        }

        // --- GÜÇLENDİRME (POWERUP) SİSTEMİ ---
        if (powerupSpawner != null && level.availablePowerups != null && level.availablePowerups.Length > 0)
        {
            powerupSpawner.Init(level.availablePowerups);
            // RefreshArenaBounds() Init içinde çağrılıyor — gerçek sprite bounds kullanılıyor
        }

        // ── KURULUM SIRASI GARANTİSİ ──
        if (arenaLayout != null)
            arenaLayout.ApplyLayout();

        var composer = Object.FindAnyObjectByType<ScreenComposer>();
        if (composer != null) composer.ForceRecalculate();
        else
        {
            var camFit = Object.FindAnyObjectByType<CameraFitWidth>();
            if (camFit != null) camFit.ForceRecalculate();
        }

        SetupProjectileBounds();

        if (powerupSpawner != null) powerupSpawner.RefreshArenaBounds();

        KakLog.Info("[LevelManager] Level hazirlandi: " + level.levelName);
    }

    void ApplyPlayerData(PlayerData data)
    {
        if (data == null) return;

        if (playerMovement != null)
        {
            playerMovement.SetMoveSpeed(data.moveSpeed);
        }

        if (playerHealth != null)
        {
            playerHealth.maxHealth = data.maxHealth;
            playerHealth.invincibilityDuration = data.invincibilityDuration;
        }

        if (playerVisual != null)
        {
            // SADECE Data içinde bir görsel (en azından South yönü için) atanmışsa üzerine yaz.
            // Atanmamışsa sahnedeki Player objesinin mevcut animasyonlarını bozma.
            if (data.south != null && data.south.idle != null)
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
                
                KakLog.Info("[LevelManager] Player görselleri Data'dan yüklendi: " + data.playerName);
            }
            else
            {
                KakLog.Info("[LevelManager] PlayerData'da görsel yok. Sahnedeki Player görseli kullanılıyor.");
            }
        }

        KakLog.Info("[LevelManager] Player istatistikleri ayarlandi: " + data.playerName);
    }

    void ApplyArenaData(ArenaData data)
    {
        if (data == null || arenaLayout == null) return;

        arenaLayout.wallThickness = data.wallThickness;
        arenaLayout.wallInset = data.wallInset;
        arenaLayout.playableAreaNormalized = data.playableAreaNormalized;
        arenaLayout.useSpawnerAnchors = data.useSpawnerAnchors;
        arenaLayout.anchorTopLeft = data.anchorTopLeft;
        arenaLayout.anchorTopRight = data.anchorTopRight;
        arenaLayout.anchorBottomLeft = data.anchorBottomLeft;
        arenaLayout.anchorBottomRight = data.anchorBottomRight;
        arenaLayout.spawnerInsetX = data.spawnerInsetX;
        arenaLayout.spawnerInsetY = data.spawnerInsetY;
        arenaLayout.spawnerTopDepthOffset = data.spawnerTopDepthOffset;

        if (data.arenaSprite != null && arenaLayout.arenaSpriteRenderer != null)
        {
            arenaLayout.arenaSpriteRenderer.sprite = data.arenaSprite;
        }

        if (playerMovement != null)
        {
            playerMovement.arenaFriction = data.floorFriction;
            playerMovement.arenaSpeedMultiplier = data.movementSpeedMultiplier;
        }

        KakLog.Info("[LevelManager] Arena ayarlandi: " + data.arenaName);
    }

    void ApplySpawnerData(SpawnerData[] dataList)
    {
        if (dataList == null || spawners == null) return;

        int count = Mathf.Min(dataList.Length, spawners.Length);

        for (int i = 0; i < count; i++)
        {
            SpawnerData data = dataList[i];
            if (data == null) continue;

            if (spawners[i] != null)
            {
                spawners[i].shootInterval = data.shootInterval;

                if (data.projectileData != null && data.projectileData.projectilePrefab != null)
                {
                    spawners[i].projectilePrefab = data.projectileData.projectilePrefab;
                }
            }

            if (i < spawnerVisuals.Length && spawnerVisuals[i] != null)
            {
                SpawnerDirectionAnimator vis = spawnerVisuals[i];

                vis.north.idle = data.idleNorth;
                vis.south.idle = data.idleSouth;
                vis.east.idle = data.idleEast;
                vis.west.idle = data.idleWest;
                vis.northEast.idle = data.idleNorthEast;
                vis.northWest.idle = data.idleNorthWest;
                vis.southEast.idle = data.idleSouthEast;
                vis.southWest.idle = data.idleSouthWest;

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

        KakLog.Info("[LevelManager] " + count + " spawner ayarlandi.");
    }

    /// <summary>
    /// Zorluk asamalarini DifficultyManager'a aktarir.
    /// Tüm gerekli referansları da bağlar.
    /// </summary>
    void SetupDifficulty(DifficultyStageData[] stages)
    {
        if (difficultyManager == null || stages == null || stages.Length == 0) return;

        // Spawner'lar LevelData ile ayarlandıktan SONRA başlat: orijinal aralıklar doğru kaydedilir
        ScoreManager score = GameManager.Instance != null ? GameManager.Instance.scoreManager : null;
        difficultyManager.Init(stages, spawners, score);

        KakLog.Info("[LevelManager] Zorluk sistemi kuruldu: " + stages.Length + " asama.");
    }

    /// <summary>
    /// WaveData listesini WaveManager'a aktarır ve dalga sistemini başlatır.
    /// levelType == Stage olduğunda ApplyLevelData tarafından çağrılır.
    /// </summary>
    void SetupWaves(WaveData[] waveList)
    {
        // WaveManager yoksa sahnede yarat
        if (waveManager == null)
        {
            waveManager = FindAnyObjectByType<WaveManager>();
            if (waveManager == null)
            {
                GameObject wm = new GameObject("WaveManager");
                waveManager = wm.AddComponent<WaveManager>();
                wm.transform.parent = this.transform;
            }
        }

        // Spawner referanslarını bağla
        if (spawners != null && spawners.Length > 0)
        {
            waveManager.spawners = spawners;
        }

        // Dalga listesini ver ve başlat
        waveManager.Init(waveList);

        KakLog.Info("[LevelManager] Dalga sistemi başlatıldı: " + waveList.Length + " dalga.");
    }
}
