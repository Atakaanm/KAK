using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Stage (Dalga) modu için dalga yöneticisi.
///
/// LevelData.levelType == Stage olduğunda LevelManager bu bileşeni aktifleştirir.
/// Her dalga için tanımlı düşman sayısını, gecikmeyi ve dalga başı/sonu olaylarını yönetir.
///
/// Mevcut sistemle entegrasyon:
///   • CornerShooter spawner'larına bağlı çalışır.
///   • DifficultyStageData yerine WaveData içinden parametre alır.
///   • Tüm dalgalar bittiğinde GameManager.LevelComplete() çağırır.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    // -------------------------------------------------------
    // Referanslar
    // -------------------------------------------------------
    [Header("Referanslar")]
    public CornerShooter[] spawners;

    // -------------------------------------------------------
    // Dalga Verisi
    // -------------------------------------------------------
    [Header("Dalga Listesi")]
    public WaveData[] waves;

    // -------------------------------------------------------
    // Durum
    // -------------------------------------------------------
    [Header("Durum (Salt-okunur)")]
    [SerializeField] private int currentWaveIndex = -1;
    [SerializeField] private bool waveInProgress = false;
    [SerializeField] private bool allWavesComplete = false;

    // Orijinal mermi prefab'larını sakla — wave override sonrası geri yükleme için
    private GameObject[] originalPrefabs;

    public int CurrentWaveIndex => currentWaveIndex;
    public bool WaveInProgress => waveInProgress;
    public bool AllWavesComplete => allWavesComplete;

    // -------------------------------------------------------
    // Olaylar
    // -------------------------------------------------------
    [Header("Olaylar")]
    public UnityEvent<int> onWaveStart;       // Kaçıncı dalga başladı
    public UnityEvent<int> onWaveComplete;    // Kaçıncı dalga bitti
    public UnityEvent onAllWavesComplete;     // Tüm dalgalar bitti

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Otomatik spawner bulma
        if (spawners == null || spawners.Length == 0)
        {
            spawners = FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
        }
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// LevelManager veya oyun başında çağrılır.
    /// Dalga verisini atar ve ilk dalgayı başlatır.
    /// </summary>
    public void Init(WaveData[] waveList)
    {
        waves = waveList;
        currentWaveIndex = -1;
        allWavesComplete = false;

        // Spawner'ların orijinal mermi prefab'larını kaydet
        if (spawners != null)
        {
            originalPrefabs = new GameObject[spawners.Length];
            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] != null)
                    originalPrefabs[i] = spawners[i].projectilePrefab;
            }
        }

        StartNextWave();
    }

    /// <summary>
    /// Bir sonraki dalgayı başlatır. Dalgalar bittiyse bitiş olayını tetikler.
    /// </summary>
    public void StartNextWave()
    {
        if (allWavesComplete) return;

        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            HandleAllWavesComplete();
            return;
        }

        StartCoroutine(RunWave(waves[currentWaveIndex]));
    }

    /// <summary>
    /// Mevcut dalgayı durdurur (Game Over gibi durumlarda).
    /// </summary>
    public void StopAllWaves()
    {
        StopAllCoroutines();
        waveInProgress = false;
        // Tüm spawner'ları durdur
        SetSpawnersActive(false);
    }

    // -------------------------------------------------------
    // Dalga Rutini
    // -------------------------------------------------------
    IEnumerator RunWave(WaveData wave)
    {
        waveInProgress = true;

        Debug.Log($"[WaveManager] 🌊 Dalga {currentWaveIndex + 1} başlıyor...");
        onWaveStart?.Invoke(currentWaveIndex);

        // Dalga öncesi bekleme süresi
        if (wave.delayBefore > 0f)
        {
            SetSpawnersActive(false);
            yield return new WaitForSeconds(wave.delayBefore);
        }

        // Spawner'ları ayarla ve aktifleştir
        ApplyWaveToSpawners(wave);
        SetSpawnersActive(true);

        // Dalga süresi kadar bekle
        float elapsed = 0f;
        float duration = wave.waveDuration;

        while (elapsed < duration)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                StopAllWaves();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Dalga bitti
        SetSpawnersActive(false);
        waveInProgress = false;

        Debug.Log($"[WaveManager] ✅ Dalga {currentWaveIndex + 1} tamamlandı.");
        onWaveComplete?.Invoke(currentWaveIndex);

        // Bir sonraki dalgaya geçiş gecikmesi
        if (wave.delayAfter > 0f)
        {
            yield return new WaitForSeconds(wave.delayAfter);
        }

        StartNextWave();
    }

    // -------------------------------------------------------
    // Yardımcı Metotlar
    // -------------------------------------------------------

    void ApplyWaveToSpawners(WaveData wave)
    {
        if (spawners == null) return;

        // Önce TÜM spawner'ları orijinal prefab'larına geri yükle
        if (originalPrefabs != null)
        {
            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] != null && i < originalPrefabs.Length)
                    spawners[i].projectilePrefab = originalPrefabs[i];
            }
        }

        int activeCount = Mathf.Min(wave.activeSpawnerCount, spawners.Length);

        for (int i = 0; i < spawners.Length; i++)
        {
            if (spawners[i] == null) continue;

            bool shouldBeActive = i < activeCount;
            spawners[i].gameObject.SetActive(shouldBeActive);

            if (shouldBeActive)
            {
                spawners[i].shootInterval = wave.shootInterval;

                // Mermi tipi varsa uygula
                if (wave.projectileOverride != null)
                {
                    spawners[i].projectilePrefab = wave.projectileOverride.projectilePrefab;
                }
            }
        }
    }

    void SetSpawnersActive(bool active)
    {
        if (spawners == null) return;

        foreach (var s in spawners)
        {
            if (s != null) s.gameObject.SetActive(active);
        }
    }

    void HandleAllWavesComplete()
    {
        allWavesComplete = true;
        waveInProgress = false;

        Debug.Log("[WaveManager] 🏆 TÜM DALGALAR TAMAMLANDI!");
        onAllWavesComplete?.Invoke();

        // Skora göre level kazanma
        // GameManager.Instance?.LevelComplete(); // İleride eklenebilir
    }
}

// -------------------------------------------------------
// Dalga Veri Sınıfı (ScriptableObject değil — LevelData içine gömülür)
// -------------------------------------------------------
[System.Serializable]
public class WaveData
{
    [Header("Dalga Kimliği")]
    public string waveName = "Dalga 1";

    [Header("Zamanlama")]
    [Tooltip("Dalga başlamadan önceki bekleme süresi (sn)")]
    public float delayBefore = 2f;

    [Tooltip("Bu dalganın toplam süresi (sn)")]
    public float waveDuration = 20f;

    [Tooltip("Dalga bittikten sonra bir sonrakine geçiş süresi (sn)")]
    public float delayAfter = 1.5f;

    [Header("Spawner Ayarları")]
    [Tooltip("Bu dalgada aktif olacak spawner sayısı")]
    public int activeSpawnerCount = 2;

    [Tooltip("Ateş aralığı (sn) — düşük değer = daha hızlı ateş")]
    public float shootInterval = 1.5f;

    [Header("Mermi Tipi (boş bırakılırsa spawner'ın kendi tipi kullanılır)")]
    public ProjectileData projectileOverride;
}
