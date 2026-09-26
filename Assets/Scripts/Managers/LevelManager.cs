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

    [Header("Bölüm modu")]
    [Tooltip("Düşman gölgesi ve varsayılan mermi prefab'ı")]
    public Sprite enemyShadow;
    public GameObject projectilePrefabDefault;
    public DungeonFrame dungeonFrame;
    public ScreenComposer screenComposer;
    [Tooltip("Pet gölgesi ve ışığı (Faz 3c.5)")]
    public Sprite petShadowSprite;
    public Sprite petGlowSprite;

    /// <summary>Tema uygulandığında (karanlık, ışık vb. sistemler dinler).</summary>
    public static event System.Action<WorldTheme> ThemeApplied;
    /// <summary>Bu oyunda oynanan karakter (altın çarpanı vb. için).</summary>
    public PlayerData CurrentCharacter { get; private set; }
    public WorldTheme CurrentTheme { get; private set; }

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
        // Karakter: oyuncunun seçtiği (katalog), yoksa bölümün verisi
        CurrentCharacter = CharacterCatalog.Selected(level.playerData);
        ApplyPlayerData(CurrentCharacter);

        // Pet (Faz 3c.5): özellik açık ve pet seçiliyse oyuncunun yanında
        var pet = FeatureGate.IsUnlocked(Feature.Pets) ? PetCatalog.Selected() : null;
        if (pet != null && playerMovement != null) PetFollower.Spawn(pet, playerMovement.transform, petShadowSprite, petGlowSprite);

        // --- ARENA AYARLARI ---
        ApplyArenaData(level.arenaData);
        if (level.theme != null) ApplyTheme(level.theme);

        // --- SPAWNER AYARLARI ---
        ApplySpawnerData(level.spawnerDataList);

        // --- ZORLUK SISTEMI (Endless mod) ---
        if (level.levelType == LevelType.Endless && level.enableDifficulty)
        {
            System.Array.Sort(level.difficultyStages, (a, b) => a.minScore.CompareTo(b.minScore));
            SetupDifficulty(level.difficultyStages);
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

        var composer = screenComposer != null ? screenComposer : Object.FindAnyObjectByType<ScreenComposer>();
        if (composer != null) composer.ForceRecalculate();

        SetupProjectileBounds();

        if (powerupSpawner != null) powerupSpawner.RefreshArenaBounds();

        // --- BÖLÜM (Stage) ---
        bool stage = level.levelType == LevelType.Stage;
        if (stage) SetupStage(level);
        var lmc = LevelModeController.Instance != null ? LevelModeController.Instance : FindAnyObjectByType<LevelModeController>();
        if (lmc != null) lmc.Begin(level);
        var events = FindAnyObjectByType<EndlessEventManager>();
        if (events != null) events.enabled = !stage;

        // Oyun sırasında Instantiate olmasın (parçalanan taşlar ve taş yağmuru için pay)
        if (ProjectilePool.Instance != null && spawners != null && spawners.Length > 0 && spawners[0] != null && spawners[0].projectilePrefab != null)
            ProjectilePool.Instance.Prewarm(spawners[0].projectilePrefab, 40);

        KakLog.Info("[LevelManager] Level hazirlandi: " + level.levelName);
    }

    void ApplyPlayerData(PlayerData data)
    {
        if (data == null) return;

        if (playerMovement != null)
        {
            playerMovement.SetMoveSpeed(data.moveSpeed);
        }

        if (playerMovement != null) playerMovement.playerData = data; // powerup süre çarpanı buradan okunur

        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(data.maxHealth);
            playerHealth.invincibilityDuration = data.invincibilityDuration;
            if (data.startWithShield) playerHealth.ActivateShield();
        }

        // Karakter istatistikleri (Faz 3c.3): dash, gövde, powerup sıklığı
        var player = playerMovement != null ? playerMovement.gameObject : null;
        if (player != null)
        {
            var dash = player.GetComponent<PlayerDash>();
            if (dash != null && data.dashCooldown > 0f) dash.cooldown = data.dashCooldown;
            var hb = player.GetComponent<PlayerHitbox>();
            if (hb != null && !Mathf.Approximately(data.hurtboxScale, 1f))
            {
                hb.hurtSize *= data.hurtboxScale;
                hb.Apply();
            }
        }
        if (powerupSpawner != null && data.powerupSpawnRateMultiplier > 0f && !Mathf.Approximately(data.powerupSpawnRateMultiplier, 1f))
            powerupSpawner.SetSpawnInterval(powerupSpawner.spawnIntervalMin / data.powerupSpawnRateMultiplier,
                                            powerupSpawner.spawnIntervalMax / data.powerupSpawnRateMultiplier);

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

    /// <summary>Dünya teması: arena görseli, oynanabilir alan, zemin fiziği, çerçeve karoları, ortam.</summary>
    public void ApplyTheme(WorldTheme t)
    {
        CurrentTheme = t;
        if (arenaLayout != null)
        {
            if (t.arenaSprite != null && arenaLayout.arenaSpriteRenderer != null) arenaLayout.arenaSpriteRenderer.sprite = t.arenaSprite;
            if (arenaLayout.arenaSpriteRenderer != null) arenaLayout.arenaSpriteRenderer.color = t.arenaTint;
            arenaLayout.playableAreaNormalized = t.playableAreaNormalized;
            arenaLayout.useSpawnerAnchors = t.useSpawnerAnchors;
        }
        if (playerMovement != null)
        {
            playerMovement.arenaFriction = t.floorFriction;
            playerMovement.arenaSpeedMultiplier = t.floorSpeedMultiplier;
        }
        var frame = dungeonFrame != null ? dungeonFrame : FindAnyObjectByType<DungeonFrame>();
        if (frame != null)
        {
            if (t.backdropTile != null && frame.backdrop != null) frame.backdrop.sprite = t.backdropTile;
            if (t.corridorTile != null && frame.corridorFloor != null) frame.corridorFloor.sprite = t.corridorTile;
            if (t.corridorWall != null)
            {
                if (frame.corridorWallLeft != null) frame.corridorWallLeft.sprite = t.corridorWall;
                if (frame.corridorWallRight != null) frame.corridorWallRight.sprite = t.corridorWall;
            }
            if (t.ledgeTile != null && frame.ledge != null) frame.ledge.sprite = t.ledgeTile;
            frame.torchesEnabled = t.torches;
        }
        var composer = screenComposer != null ? screenComposer : FindAnyObjectByType<ScreenComposer>();
        if (composer != null) composer.backgroundColor = t.cameraBackground;
        ThemeApplied?.Invoke(t);
        KakLog.Info("[LevelManager] Tema: " + t.themeId);
    }

    /// <summary>Bölüm kurulumu: köşe fırlatıcıları (sayı/aralık/taş) ve bölüm düşmanları.</summary>
    void SetupStage(LevelData level)
    {
        if (spawners != null)
        {
            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] == null) continue;
                bool on = i < level.cornerShooters;
                spawners[i].gameObject.SetActive(on);
                spawners[i].shootInterval = level.cornerInterval;
                spawners[i].overrideProjectile = level.cornerProjectile;
            }
        }

        var parent = transform.Find("Enemies");
        if (parent == null) { parent = new GameObject("Enemies").transform; parent.SetParent(transform, false); }
        for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
        if (level.enemies == null || arenaLayout == null) return;
        Rect play = arenaLayout.PlayableWorldRect;
        GameObject prefab = projectilePrefabDefault;
        if (prefab == null && spawners != null && spawners.Length > 0 && spawners[0] != null) prefab = spawners[0].projectilePrefab;
        foreach (var e in level.enemies)
            if (e != null && e.data != null) EnemyFactory.Create(e, play, prefab, enemyShadow, parent);
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
}
