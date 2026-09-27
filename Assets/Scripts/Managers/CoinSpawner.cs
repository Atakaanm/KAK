using UnityEngine;

/// <summary>
/// Arenada altın doğurur (Faz 3c.1, G1'de değişti): kümeler yerine **tek tek**, aralarında zaman ve mesafe
/// (kullanıcı: yan yana 2-3'lü çıkmaları hoş değildi). Risk/ödül: altın oyuncudan biraz uzakta çıkar.
/// Kademe arttıkça biraz sıklaşır. Sadece FeatureGate açıksa çalışır (ilk oyun saf kaçış). Kurulum: KakMetaSetup.
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    [Header("Zamanlama (G1)")]
    public float firstCoinDelay = 3f;
    public float spawnEveryMin = 2.2f;
    public float spawnEveryMax = 3.6f;
    [Tooltip("Her kademede aralık bu oranla çarpılır")]
    public float stageEveryFactor = 0.95f;
    [Header("Dağılım (G1)")]
    [Tooltip("Arenada aynı anda en fazla bu kadar altın")]
    public int maxOnField = 4;
    [Tooltip("Yeni altın, sahnedeki altınlardan ve bir önceki doğum yerinden en az bu kadar uzakta")]
    public float minCoinSpacing = 1.4f;
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
    Vector2 lastSpawn = new Vector2(9999f, 9999f);

    void Start()
    {
        ActiveThisRun = coinPrefab != null && FeatureGate.IsUnlocked(Feature.Coins)
                        && (GameManager.Instance == null || !GameManager.Instance.IsLevelMode);
        arena = FindAnyObjectByType<ArenaAutoLayout>();
        shooters = FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
        if (arena != null) pedestalCount = arena.PedestalRects(pedestals);
        next = firstCoinDelay;
        if (ActiveThisRun && ProjectilePool.Instance != null) ProjectilePool.Instance.Prewarm(coinPrefab, maxOnField + 2);
    }

    void Update()
    {
        if (!ActiveThisRun || arena == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        timer += Time.deltaTime;
        if (timer < next) return;
        timer = 0f;
        int stage = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentStageIndex : 0;
        next = Random.Range(spawnEveryMin, spawnEveryMax) * Mathf.Pow(stageEveryFactor, stage);
        if (Coin.Active.Count < maxOnField) SpawnCoin();
    }

    /// <summary>Tek altın doğurur (testler ve olaylar da çağırabilir). Doğarsa 1, yer bulamazsa 0.</summary>
    public int SpawnCoin()
    {
        if (coinPrefab == null || arena == null) return 0;
        Rect play = arena.PlayableWorldRect;
        Rect area = Rect.MinMaxRect(play.xMin + edgeMargin, play.yMin + edgeMargin, play.xMax - edgeMargin, play.yMax - edgeMargin);
        var pt = GameSettings.TwoPlayer ? PlayerRegistry.RandomAlive() : Projectile.PlayerTarget;
        Vector2 player = pt != null ? (Vector2)pt.position : area.center;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 c = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
            float dp = Vector2.Distance(c, player);
            if (dp < minPlayerDistance || dp > maxPlayerDistance) continue;
            if (!Valid(c, area) || !FarFromCoins(c)) continue;

            if (ProjectilePool.Instance != null) ProjectilePool.Instance.Get(coinPrefab, c, Quaternion.identity);
            else Instantiate(coinPrefab, c, Quaternion.identity);
            lastSpawn = c;
            GameEvents.RaiseCoinClusterSpawned(c);
            return 1;
        }
        return 0;
    }

    bool FarFromCoins(Vector2 p)
    {
        float min2 = minCoinSpacing * minCoinSpacing;
        if ((p - lastSpawn).sqrMagnitude < min2) return false;
        for (int i = 0; i < Coin.Active.Count; i++)
            if (((Vector2)Coin.Active[i].transform.position - p).sqrMagnitude < min2) return false;
        return true;
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
