using UnityEngine;

/// <summary>
/// Sonsuz (Endless) modda skora gore zorlugu yoneten manager.
/// Skoru dinler, esik degerlerine gore aktif DifficultyStageData'yi secer,
/// spawner hizlarini ve mermi hizlarini carpanla gunceller.
/// 
/// Kullanim: Sahneye bos bir GameObject koy, bu scripti ekle,
/// ScoreManager ve spawner referanslarini ata.
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

    private DifficultyStageData currentStage;
    private int currentStageIndex = -1;
    private bool isActive = true;

    // Her spawner icin orijinal ates araligi (geri yukleme icin)
    private float[] originalShootIntervals;

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
        // Orijinal spawner degerlerini kaydet
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
        }

        // Baslangicta ilk stage'i uygula
        if (stages != null && stages.Length > 0)
        {
            ApplyStage(0);
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
            if (currentScore >= stages[i].minScore)
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

        currentStageIndex = stageIndex;
        currentStage = stages[stageIndex];

        Debug.Log("[DifficultyManager] Stage gecisi: " + currentStage.stageName
            + " (Skor >= " + currentStage.minScore + ")");

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
                }
                else
                {
                    // Bu spawner bu asamada aktif degil
                    allSpawners[i].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Mevcut zorluk asamasinin mermi hiz carpanini dondurur.
    /// Projectile scripti bunu okuyup hizini ayarlar.
    /// </summary>
    public float GetProjectileSpeedMultiplier()
    {
        if (currentStage != null)
            return currentStage.projectileSpeedMultiplier;
        return 1.0f;
    }

    /// <summary>
    /// Mevcut asamanin ismini dondurur (debug / UI icin).
    /// </summary>
    public string GetCurrentStageName()
    {
        if (currentStage != null)
            return currentStage.stageName;
        return "Bilinmiyor";
    }

    /// <summary>
    /// Zorluk sistemini durdurur (ornegin oyun bitti ekraninda).
    /// </summary>
    public void StopDifficulty()
    {
        isActive = false;
    }
}
