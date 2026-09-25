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

    [Header("Yerleşim Payları")]
    [Tooltip("Duvar iç yüzünden içeri pay (dünya birimi)")]
    public float edgeMargin = 0.35f;
    [Tooltip("Fırlatıcıların etrafında powerup çıkmayacak yarıçap")]
    public float spawnerClearance = 1.4f;

    [Header("Sorting")]
    [Tooltip("Powerup sprite'ının arena ve karakter üzerinde çıkması için sorting order")]
    public int powerupSortingOrder = 20;

    private PowerupData[] availablePowerups;
    private readonly Collider2D[] overlapBuffer = new Collider2D[8];
    private readonly System.Collections.Generic.List<Vector3> spawnerPositions = new System.Collections.Generic.List<Vector3>(4);
    private ContactFilter2D overlapFilter;
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
    /// <summary>Çıkma aralığını değiştirir ve zamanlayıcıyı yeniden kurar (olaylar, test, debug paneli).</summary>
    public void SetSpawnInterval(float min, float max)
    {
        spawnIntervalMin = min;
        spawnIntervalMax = max;
        timer = 0f;
        currentInterval = Random.Range(min, max);
    }

    public void RefreshArenaBounds()
    {
        var arena = FindAnyObjectByType<ArenaAutoLayout>();
        if (arena != null && arena.arenaSpriteRenderer != null)
        {
            Rect play = arena.PlayableWorldRect;
            float m = itemWorldSize * 0.5f + edgeMargin;
            spawnAreaMin = new Vector2(play.xMin + m, play.yMin + m);
            spawnAreaMax = new Vector2(play.xMax - m, play.yMax - m);
            boundsReady = true;

            spawnerPositions.Clear();
            foreach (var t in new[] { arena.topLeftSpawner, arena.topRightSpawner, arena.bottomLeftSpawner, arena.bottomRightSpawner })
                if (t != null) spawnerPositions.Add(t.position);

            KakLog.Info($"[PowerupSpawner] Sınırlar oynanabilir alandan: {spawnAreaMin} → {spawnAreaMax}");
        }
        else
        {
            Debug.LogWarning("[PowerupSpawner] ArenaAutoLayout bulunamadı, yedek sınırlar kullanılıyor.");
        }
    }

    bool NearSpawner(Vector2 p)
    {
        float r2 = spawnerClearance * spawnerClearance;
        for (int i = 0; i < spawnerPositions.Count; i++)
            if (((Vector2)spawnerPositions[i] - p).sqrMagnitude < r2) return true;
        return false;
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
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float rx = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float ry = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            spawnPos = new Vector3(rx, ry, 0f);
            if (NearSpawner(spawnPos)) continue;

            overlapFilter.useTriggers = true;
            int hitCount = Physics2D.OverlapCircle(spawnPos, itemWorldSize * 0.5f, overlapFilter, overlapBuffer);
            bool blocked = false;
            for (int h = 0; h < hitCount; h++)
            {
                var other = overlapBuffer[h];
                // Oyuncu, katı engeller (duvar, spawner) ve başka powerup'lar engeller; mermiler (trigger) engellemez
                if (other.CompareTag("Player") || !other.isTrigger || other.GetComponent<PowerupPickup>() != null)
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
            return; // 10 denemede uygun yer bulunamadı, bu turu atla
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

        KakLog.Info($"[PowerupSpawner] '{selectedPowerup.powerupName}' oluşturuldu → {spawnPos}  scale={obj.transform.localScale.x:F2}");
    }
}
