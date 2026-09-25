using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sonsuz (Endless) modda skora gore zorlugu yoneten manager.
/// Skoru dinler, esik degerlerine gore aktif DifficultyStageData'yi secer,
/// spawner hizlarini ve mermi hizlarini carpanla gunceller.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    [Header("Referanslar")]
    public ScoreManager scoreManager;

    [Header("Zorluk Asamalari")]
    public DifficultyStageData[] stages; // Skora gore siralanmis olmali (minScore artan)

    [Header("Spawner Referanslari")]
    public CornerShooter[] allSpawners; // Sahnedeki tum firlaticilar

    [Header("Etkinlikler (Events)")]
    public UnityEvent<string> onStageChanged;

    private DifficultyStageData currentStage;
    private int currentStageIndex = -1;
    private bool isActive = true;
    private bool initialized = false;

    // Her spawner icin orijinal ates araligi (geri yukleme icin)
    private float[] originalShootIntervals;
    private bool originalsRecorded = false;

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

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        // Normal akışta LevelManager, LevelData'yı uyguladıktan sonra Init() çağırır.
        // LevelManager yoksa (test sahnesi vb.) kendi kendine başla.
        if (initialized || LevelManager.Instance != null) return;

        AutoFindReferences();
        Init(stages, allSpawners, scoreManager);
    }

    /// <summary>
    /// Zorluk sistemini başlatır: aşamaları, spawner'ları (sıralı) ve skoru bağlar,
    /// orijinal ateş aralıklarını kaydeder ve ilk aşamayı uygular.
    /// LevelManager tarafından spawner'lar LevelData ile ayarlandıktan SONRA çağrılır.
    /// </summary>
    public void Init(DifficultyStageData[] stageList, CornerShooter[] spawners, ScoreManager score)
    {
        stages = stageList;
        if (spawners != null && spawners.Length > 0) allSpawners = spawners;
        if (score != null) scoreManager = score;
        if (scoreManager == null) scoreManager = FindAnyObjectByType<ScoreManager>();

        initialized = true;
        isActive = true;
        currentStageIndex = -1;
        currentStage = null;

        RecordOriginalIntervals();
        if (stages != null && stages.Length > 0)
            ApplyStage(0);
    }

    /// <summary>
    /// Inspector'dan atanmamış referansları otomatik bulur.
    /// </summary>
    void AutoFindReferences()
    {
        if (scoreManager == null)
        {
            scoreManager = FindAnyObjectByType<ScoreManager>();
            if (scoreManager != null)
                KakLog.Info("[DifficultyManager] ScoreManager otomatik bulundu.");
        }

        if (allSpawners == null || allSpawners.Length == 0)
        {
            allSpawners = FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
            if (allSpawners != null && allSpawners.Length > 0)
                KakLog.Info("[DifficultyManager] " + allSpawners.Length + " spawner otomatik bulundu.");
        }
    }

    /// <summary>
    /// Orijinal ateş aralıklarını kaydeder.
    /// LevelManager tarafından spawner'lar ayarlandıktan SONRA çağrılabilir.
    /// </summary>
    public void RecordOriginalIntervals()
    {
        if (allSpawners != null && allSpawners.Length > 0)
        {
            originalShootIntervals = new float[allSpawners.Length];
            for (int i = 0; i < allSpawners.Length; i++)
            {
                if (allSpawners[i] != null)
                {
                    originalShootIntervals[i] = allSpawners[i].shootInterval;
                }
            }
            originalsRecorded = true;
            KakLog.Info("[DifficultyManager] Orijinal ateş aralıkları kaydedildi (" + allSpawners.Length + " spawner).");
        }
    }

    void Update()
    {
        if (!isActive || scoreManager == null || stages == null || stages.Length == 0)
            return;

        int currentScore = scoreManager.ScoreInt;

        // Hangi stage'deyiz kontrol et
        for (int i = stages.Length - 1; i >= 0; i--)
        {
            if (stages[i] != null && currentScore >= stages[i].minScore)
            {
                if (i != currentStageIndex)
                {
                    ApplyStage(i);
                }
                break;
            }
        }
    }

    /// <summary>
    /// Belirtilen indexteki zorluk asamasini uygular.
    /// Spawner ates hizlarini ve aktif spawner sayisini gunceller.
    /// </summary>
    void ApplyStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stages.Length)
            return;

        if (stages[stageIndex] == null)
            return;

        currentStageIndex = stageIndex;
        currentStage = stages[stageIndex];

        KakLog.Info("[DifficultyManager] ⚡ STAGE GEÇİŞİ: " + currentStage.stageName
            + " (Skor >= " + currentStage.minScore + ")"
            + " | Ateş Çarpanı: " + currentStage.shootIntervalMultiplier
            + " | Mermi Hız Çarpanı: " + currentStage.projectileSpeedMultiplier
            + " | Aktif Spawner: " + currentStage.activeSpawnerCount);

        if (onStageChanged != null)
        {
            onStageChanged.Invoke(currentStage.stageName);
        }
        GameEvents.RaiseStageChanged(currentStage.stageName, stageIndex);

        // Orijinaller henüz kaydedilmediyse şimdi kaydet
        if (!originalsRecorded)
        {
            RecordOriginalIntervals();
        }

        // Spawner hizlarini guncelle
        if (allSpawners != null && originalShootIntervals != null)
        {
            for (int i = 0; i < allSpawners.Length; i++)
            {
                if (allSpawners[i] == null) continue;

                // Aktif spawner sayisi kontrolu
                if (i < currentStage.activeSpawnerCount)
                {
                    allSpawners[i].gameObject.SetActive(true);
                    // Ates araligini carpanla guncelle (dusuk carpan = daha hizli ates)
                    allSpawners[i].shootInterval = originalShootIntervals[i] * currentStage.shootIntervalMultiplier;
                    
                    KakLog.Info("[DifficultyManager] Spawner " + i + " interval: " 
                        + originalShootIntervals[i] + " → " + allSpawners[i].shootInterval);
                }
                else
                {
                    // Bu spawner bu asamada aktif degil
                    allSpawners[i].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>Mevcut kademenin taş listesinden ağırlıklı rastgele seçim; liste boşsa null.</summary>
    public ProjectileData PickProjectile()
    {
        if (currentStage == null || currentStage.availableProjectiles == null || currentStage.availableProjectiles.Length == 0)
            return null;
        var list = currentStage.availableProjectiles;
        var w = currentStage.projectileWeights;
        float total = 0f;
        for (int i = 0; i < list.Length; i++) total += (w != null && i < w.Length) ? Mathf.Max(0f, w[i]) : 1f;
        float r = Random.value * total;
        for (int i = 0; i < list.Length; i++)
        {
            r -= (w != null && i < w.Length) ? Mathf.Max(0f, w[i]) : 1f;
            if (r <= 0f) return list[i];
        }
        return list[list.Length - 1];
    }

    /// <summary>
    /// Mevcut zorluk asamasinin mermi hiz carpanini dondurur.
    /// </summary>
    public float GetProjectileSpeedMultiplier()
    {
        if (currentStage != null)
            return currentStage.projectileSpeedMultiplier;
        return 1.0f;
    }

    public int CurrentStageIndex => Mathf.Max(0, currentStageIndex);

    /// <summary>Mevcut kademenin aktif spawner sayısını yeniden uygular (olaylar spawner'ları geçici açıp kapattıktan sonra).</summary>
    public void RefreshSpawnerActivation()
    {
        if (currentStage == null || allSpawners == null) return;
        for (int i = 0; i < allSpawners.Length; i++)
            if (allSpawners[i] != null) allSpawners[i].gameObject.SetActive(i < currentStage.activeSpawnerCount);
    }

    public float GetProjectileScaleMultiplier()
    {
        if (currentStage != null)
            return currentStage.projectileScaleMultiplier;
        return 1.0f;
    }

    public float GetPlayerSpeedMultiplier()
    {
        if (currentStage != null)
            return currentStage.playerSpeedMultiplier;
        return 1.0f;
    }

    public float GetScoreSpeedMultiplier()
    {
        if (currentStage != null)
            return currentStage.scoreSpeedMultiplier;
        return 1.0f;
    }

    /// <summary>
    /// Mevcut asamanin ismini dondurur.
    /// </summary>
    public string GetCurrentStageName()
    {
        if (currentStage != null)
            return currentStage.stageName;
        return "Bilinmiyor";
    }

    /// <summary>
    /// Zorluk sistemini durdurur.
    /// </summary>
    public void StopDifficulty()
    {
        isActive = false;
    }
}
