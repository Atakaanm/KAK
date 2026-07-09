using UnityEngine;

/// <summary>
/// Arenada rastgele aralıklarla rastgele PowerUp çıkaran sistem.
/// Item'lar karakter boyutunda küre gibi gözükür.
///
/// Arena sınırları için öncelik sırası:
///   1. Init() ile dışarıdan verilen değerler (LevelManager)
///   2. Start()'ta ArenaAutoLayout'tan otomatik algılanan değerler
///   3. Yedek sabit değerler (-4, +4)
/// </summary>
public class PowerupSpawner : MonoBehaviour
{
    [Header("Spawn Aralığı (Rastgele)")]
    public float spawnIntervalMin = 5f;
    public float spawnIntervalMax = 10f;

    [Header("Item Boyutu")]
    public float itemWorldSize = 0.65f;

    [Header("Arena Sınırları (otomatik doldurulur)")]
    public Vector2 spawnAreaMin = new Vector2(-4f, -4f);
    public Vector2 spawnAreaMax = new Vector2(4f,  4f);

    [Header("Sorting")]
    [Tooltip("Powerup sprite'ının arena ve karakter üzerinde çıkması için sorting order")]
    public int powerupSortingOrder = 20;

    private PowerupData[] availablePowerups;
    private float timer = 0f;
    private float currentInterval;
    private bool boundsReady = false;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    void Start()
    {
        // Bounds daha önce Init() ile set edilmediyse arena'dan otomatik al
        if (!boundsReady)
        {
            RefreshArenaBounds();
        }
        currentInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// LevelManager tarafından çağrılır — powerup listesini ve opsiyonel sınırları verir.
    /// </summary>
    public void Init(PowerupData[] powerups)
    {
        availablePowerups = powerups;
        timer = 0f;
        currentInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);

        // Arena bounds'u her Init'te taze al (sahne boyutu değişmiş olabilir)
        RefreshArenaBounds();
    }

    /// <summary>
    /// ArenaAutoLayout'tan arena sınırlarını okuyarak spawnAreaMin/Max'ı günceller.
    /// LevelManager veya SceneAutoWire tarafından da çağrılabilir.
    /// </summary>
    public void RefreshArenaBounds()
    {
        var arena = FindAnyObjectByType<ArenaAutoLayout>();
        
        // 1. ÖNCELİK: Fiziksel Duvarlardan (Kusursuz Koordinat)
        if (arena != null && arena.topWall != null && arena.bottomWall != null && arena.leftWall != null && arena.rightWall != null)
        {
            float topY = arena.topWall.position.y;
            float bottomY = arena.bottomWall.position.y;
            float leftX = arena.leftWall.position.x;
            float rightX = arena.rightWall.position.x;

            // Güvenli bölge (Duvarlardan %15-20 içeri)
            float padX = (rightX - leftX) * 0.15f;
            float padY = (topY - bottomY) * 0.15f;

            spawnAreaMin = new Vector2(leftX + padX, bottomY + padY);
            spawnAreaMax = new Vector2(rightX - padX, topY - padY);
            boundsReady = true;

            Debug.Log($"[PowerupSpawner] Sınırlar DUVARLARDAN alındı: {spawnAreaMin} → {spawnAreaMax}");
        }
        // 2. ÖNCELİK: Sprite Bounds (Yedek)
        else if (arena != null && arena.arenaSpriteRenderer != null)
        {
            Bounds b = arena.arenaSpriteRenderer.bounds;

            float padX = b.extents.x * 0.75f;
            float padY = b.extents.y * 0.75f;

            spawnAreaMin = new Vector2(b.center.x - padX, b.center.y - padY);
            spawnAreaMax = new Vector2(b.center.x + padX, b.center.y + padY);
            boundsReady = true;

            Debug.Log($"[PowerupSpawner] Sınırlar SPRITE'TAN alındı: {spawnAreaMin} → {spawnAreaMax}");
        }
        else
        {
            Debug.LogWarning("[PowerupSpawner] ArenaAutoLayout bulunamadı, yedek sınırlar kullanılıyor.");
        }
    }

    // -------------------------------------------------------
    // Update
    // -------------------------------------------------------

    void Update()
    {
        if (availablePowerups == null || availablePowerups.Length == 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        timer += Time.deltaTime;

        if (timer >= currentInterval)
        {
            SpawnRandomPowerup();
            timer = 0f;
            currentInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    // -------------------------------------------------------
    // Spawn
    // -------------------------------------------------------

    private void SpawnRandomPowerup()
    {
        // Ağırlıklı rastgele seçim
        float totalWeight = 0f;
        foreach (var p in availablePowerups)
            totalWeight += p.spawnChanceWeight;

        float randomVal = Random.Range(0f, totalWeight);
        PowerupData selectedPowerup = null;

        foreach (var p in availablePowerups)
        {
            if (randomVal <= p.spawnChanceWeight)
            {
                selectedPowerup = p;
                break;
            }
            randomVal -= p.spawnChanceWeight;
        }

        if (selectedPowerup == null || selectedPowerup.visualPrefab == null)
        {
            if (selectedPowerup != null)
                Debug.LogWarning($"[PowerupSpawner] '{selectedPowerup.powerupName}' visualPrefab boş!");
            return;
        }

        // Çakışma kontrolü — oyuncu/spawner üstüne düşmesin
        Vector3 spawnPos = Vector3.zero;
        bool validPosition = false;
        for (int attempt = 0; attempt < 5; attempt++)
        {
            float rx = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float ry = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            spawnPos = new Vector3(rx, ry, 0f);

            Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPos, itemWorldSize * 0.5f);
            bool blocked = false;
            foreach (var h in hits)
            {
                if (h.CompareTag("Player") || h.CompareTag("Wall") || h.name.Contains("Spawner"))
                {
                    blocked = true;
                    break;
                }
            }
            if (!blocked)
            {
                validPosition = true;
                break;
            }
        }
        if (!validPosition)
        {
            Debug.LogWarning($"[PowerupSpawner] Uygun spawn noktası bulunamadı! Son denenen yer: {spawnPos}. Duvar/Player/Spawner çakışması olabilir.");
            return; // 5 denemede uygun yer bulunamadı, bu turu atla
        }

        GameObject obj = Instantiate(selectedPowerup.visualPrefab, spawnPos, Quaternion.identity);

        // ── BOYUT AYARI ──
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = powerupSortingOrder;

            if (sr.sprite != null)
            {
                float spriteWorldWidth = sr.sprite.bounds.size.x;
                if (spriteWorldWidth > 0.01f)
                {
                    float neededScale = itemWorldSize / spriteWorldWidth;
                    obj.transform.localScale = new Vector3(neededScale, neededScale, 1f);
                }
            }
        }

        // ── PICKUP BİLEŞENİ ──
        PowerupPickup pickup = obj.GetComponent<PowerupPickup>();
        if (pickup == null)
            pickup = obj.AddComponent<PowerupPickup>();

        // Collider
        CircleCollider2D col = obj.GetComponent<CircleCollider2D>();
        if (col == null)
            col = obj.AddComponent<CircleCollider2D>();

        col.isTrigger = true;
        float scaleX = Mathf.Max(obj.transform.localScale.x, 0.01f);
        col.radius = itemWorldSize * 0.5f / scaleX;

        pickup.powerupData = selectedPowerup;

        // ── FLOATING BİLEŞENİ ──
        FloatingItem floater = obj.GetComponent<FloatingItem>();
        if (floater == null)
            floater = obj.AddComponent<FloatingItem>();

        floater.SetStartPosition(spawnPos);

        Debug.Log($"[PowerupSpawner] '{selectedPowerup.powerupName}' oluşturuldu → {spawnPos}  scale={obj.transform.localScale.x:F2}");
    }
}
