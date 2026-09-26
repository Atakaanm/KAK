using UnityEngine;

/// <summary>
/// Arenada altın kümeleri doğurur (Faz 3c.1). Risk/ödül: altın oyuncudan biraz uzakta çıkar, almak için
/// taşların arasına girmek gerekir. Kademe arttıkça kümeler sıklaşır ve büyür.
/// Sadece FeatureGate açıksa çalışır (ilk oyun saf kaçış). Kurulum: KakMetaSetup.
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    [Header("Zamanlama")]
    public float firstDelay = 4f;
    public float intervalMin = 6f;
    public float intervalMax = 10f;
    [Tooltip("Her kademede aralık bu oranla çarpılır")]
    public float stageIntervalFactor = 0.93f;
    [Header("Küme")]
    public int clusterMin = 1;
    public int clusterMax = 3;
    public float spacing = 0.42f;
    [Header("Yerleşim")]
    public float minPlayerDistance = 1.3f;
    public float maxPlayerDistance = 3.2f;
    public float edgeMargin = 0.45f;
    public float spawnerClearance = 1.2f;

    /// <summary>Bu oyunda altın açık mı (oyun sonunda cüzdana ekleme buna bakar).</summary>
    public bool ActiveThisRun { get; private set; }

    ArenaAutoLayout arena;
    CornerShooter[] shooters;
    readonly Rect[] pedestals = new Rect[4];
    int pedestalCount;
    float timer, next;

    void Start()
    {
        ActiveThisRun = coinPrefab != null && FeatureGate.IsUnlocked(Feature.Coins)
                        && (GameManager.Instance == null || !GameManager.Instance.IsLevelMode);
        arena = FindAnyObjectByType<ArenaAutoLayout>();
        shooters = FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
        if (arena != null) pedestalCount = arena.PedestalRects(pedestals);
        next = firstDelay;
        if (ActiveThisRun && ProjectilePool.Instance != null) ProjectilePool.Instance.Prewarm(coinPrefab, 8);
    }

    void Update()
    {
        if (!ActiveThisRun || arena == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        timer += Time.deltaTime;
        if (timer < next) return;
        timer = 0f;
        int stage = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentStageIndex : 0;
        float factor = Mathf.Pow(stageIntervalFactor, stage);
        next = Random.Range(intervalMin, intervalMax) * factor;
        SpawnCluster(stage);
    }

    /// <summary>Bir küme doğurur (testler ve olaylar da çağırabilir). Doğan altın sayısını döndürür.</summary>
    public int SpawnCluster(int stage = 0)
    {
        if (coinPrefab == null || arena == null) return 0;
        Rect play = arena.PlayableWorldRect;
        Rect area = Rect.MinMaxRect(play.xMin + edgeMargin, play.yMin + edgeMargin, play.xMax - edgeMargin, play.yMax - edgeMargin);
        Vector2 player = Projectile.PlayerTarget != null ? (Vector2)Projectile.PlayerTarget.position : area.center;

        int count = Random.Range(clusterMin, clusterMax + 1) + stage / 2;
        // Küme dizilimi: yatay, dikey ya da çapraz çizgi
        Vector2 step = Random.value < 0.4f ? new Vector2(spacing, 0f) : Random.value < 0.5f ? new Vector2(0f, spacing) : new Vector2(spacing, spacing) * 0.75f;

        for (int attempt = 0; attempt < 14; attempt++)
        {
            Vector2 c = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
            float dp = Vector2.Distance(c, player);
            if (dp < minPlayerDistance || dp > maxPlayerDistance) continue;
            Vector2 start = c - step * (count - 1) * 0.5f;
            bool ok = true;
            for (int i = 0; i < count && ok; i++) ok = Valid(start + step * i, area);
            if (!ok) continue;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = start + step * i;
                if (ProjectilePool.Instance != null) ProjectilePool.Instance.Get(coinPrefab, pos, Quaternion.identity);
                else Instantiate(coinPrefab, pos, Quaternion.identity);
            }
            GameEvents.RaiseCoinClusterSpawned(c);
            return count;
        }
        return 0;
    }

    bool Valid(Vector2 p, Rect area)
    {
        if (!area.Contains(p)) return false;
        for (int i = 0; i < pedestalCount; i++)
        {
            var r = pedestals[i];
            if (Rect.MinMaxRect(r.xMin - 0.3f, r.yMin - 0.3f, r.xMax + 0.3f, r.yMax + 0.3f).Contains(p)) return false;
        }
        if (shooters != null)
            foreach (var s in shooters)
                if (s != null && ((Vector2)s.transform.position - p).sqrMagnitude < spawnerClearance * spawnerClearance) return false;
        return true;
    }
}
