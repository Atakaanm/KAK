using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sonsuz (Endless) modda zorluğu yöneten manager.
/// Faz 15 K1: TempoProfile atanmışsa sürekli tempo (τ) modu: tüm kaos düğmeleri τ'nun eğrisi, kademe afişi yok,
/// yeni fırlatıcı payını yavaş alır, olay sonrası nefes payı, hasar sonrası merhamet, ilk oyunlar yavaş (çırak),
/// güçlenen oyuncuya daha hızlı başlangıç. Profil yoksa eski kademe sistemi: süreye göre DifficultyStageData seçilir,
/// spawner ve mermi hızları kademe çarpanlarıyla güncellenir.
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

    [Header("Tempo (Faz 15 K1)")]
    [Tooltip("Atanırsa sürekli tempo eğrisi (kademe afişi yok); boşsa eski kademe sistemi")]
    public TempoProfile tempo;

    /// <summary>Testler: çırak yavaşlığını kapatır (KakTestUtil.ResetWorld açar, çırak testi kapatır).</summary>
    public static bool ApprenticeOff;

    public bool TempoMode => tempo != null;
    /// <summary>τ: 0 (oyun başı) → 1 (tavan).</summary>
    public float Tempo { get; private set; }
    /// <summary>Nefes payı (0-1): olay bitince yumuşakça 1'e çıkar, sonra söner.</summary>
    public float Relax { get; private set; }
    public int ActiveThrowers { get; private set; }
    /// <summary>Toplam atış hızı (fırlatıcının kendi aralığı başına atış).</summary>
    public float FireRate { get; private set; } = 1f;
    public float DoubleShotChance { get; private set; }
    public float DoubleShotDelay => tempo != null ? tempo.doubleShotDelay : 0.2f;
    public float EventIntervalMultiplier { get; private set; } = 1f;
    /// <summary>Bu oyunun gücü (0-1) ve oyun sayısı (çırak); Init'te bir kez.</summary>
    public float RunPower { get; private set; }
    public int RunGames { get; private set; }

    float tRamp = 360f, tStart;
    float mercyUntil, mercyPaused, relaxUntil;
    float[] wakeAt;
    float tSpeed = 1f, tScale = 1f, tPlayer = 1f, tScore = 1f;

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
        if (tempo != null && AudioManager.Instance != null) AudioManager.Instance.SetMusicPitch(1f);
    }

    void OnEnable() => GameEvents.PlayerDamaged += OnDamaged;
    void OnDisable() => GameEvents.PlayerDamaged -= OnDamaged;

    void OnDamaged(int hp, Vector3 pos)
    {
        // Merhamet (görünmez): vurulan oyuncu toparlanırken tempo artışı durur
        if (tempo != null) mercyUntil = Time.time + tempo.mercySeconds;
    }

    /// <summary>Olay bitince çağrılır: kısa sakinlik (gerilim → rahatlama).</summary>
    public void Breathe()
    {
        if (tempo != null) relaxUntil = Time.time + tempo.breatherSeconds;
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
    public void Init(DifficultyStageData[] stageList, CornerShooter[] spawners, ScoreManager score) => Init(stageList, spawners, score, null);

    public void Init(DifficultyStageData[] stageList, CornerShooter[] spawners, ScoreManager score, TempoProfile tempoProfile)
    {
        if (tempoProfile != null) tempo = tempoProfile;
        stages = stageList;
        if (spawners != null && spawners.Length > 0) allSpawners = spawners;
        if (score != null) scoreManager = score;
        if (scoreManager == null) scoreManager = FindAnyObjectByType<ScoreManager>();

        initialized = true;
        isActive = true;
        currentStageIndex = -1;
        currentStage = null;

        RecordOriginalIntervals();
        if (tempo != null) { InitTempo(); return; }
        if (stages != null && stages.Length > 0)
            ApplyStage(0);
    }

    // ── Tempo modu (Faz 15 K1) ─────────────────────────────

    void InitTempo()
    {
        var ch = LevelManager.Instance != null ? LevelManager.Instance.CurrentCharacter : null;
        RunPower = PlayerPower.Normalized(ch);
        RunGames = ApprenticeOff ? 999 : SaveSystem.Data.gamesPlayed;
        tRamp = tempo.RampFor(RunPower, RunGames);
        tStart = tempo.StartTempoFor(RunPower);
        mercyPaused = 0f; mercyUntil = 0f; relaxUntil = 0f; Relax = 0f;
        ActiveThrowers = 0;
        currentStageIndex = -1;
        int n = allSpawners != null ? allSpawners.Length : 0;
        wakeAt = new float[n];
        UpdateTempo(true);
        KakLog.Info("[DifficultyManager] Tempo modu: güç " + RunPower.ToString("F2") + ", oyun " + RunGames
            + ", rampa " + tRamp.ToString("F0") + " sn, başlangıç τ " + tStart.ToString("F2"));
    }

    void UpdateTempo(bool initial)
    {
        float t = scoreManager != null ? scoreManager.ElapsedSeconds : 0f;
        float dt = initial ? 0f : Time.deltaTime;
        if (Time.time < mercyUntil) mercyPaused += dt;
        float clock = Mathf.Max(0f, t - mercyPaused);
        // Güç başlangıcı ısınmada yumuşakça gelir: güçlü oyuncu da tek fırlatıcıyla başlar, ~20 sn'de hızlı ısınır
        float warm = tempo.powerWarmupSeconds > 0f ? Mathf.Clamp01(t / tempo.powerWarmupSeconds) : 1f;
        float start = tStart * warm * warm * (3f - 2f * warm);
        Tempo = Mathf.Clamp01(start + (1f - tStart) * clock / Mathf.Max(1f, tRamp));
        float relaxTarget = Time.time < relaxUntil ? 1f : 0f;
        Relax = initial ? relaxTarget : Mathf.MoveTowards(Relax, relaxTarget, dt); // ~1 sn'de yumuşak giriş/çıkış

        float tau = Tempo;
        float app = tempo.ApprenticeSpeed(RunGames, t);
        tSpeed = tempo.projectileSpeed.Evaluate(tau) * app * (1f - tempo.breatherSpeedDrop * Relax);
        tScale = tempo.projectileScale.Evaluate(tau);
        tPlayer = tempo.playerSpeed.Evaluate(tau);
        tScore = tempo.scoreSpeed.Evaluate(tau);
        FireRate = Mathf.Max(0.05f, tempo.fireRate.Evaluate(tau) * app * (1f - tempo.breatherRateDrop * Relax));
        DoubleShotChance = Relax > 0.01f ? 0f : Mathf.Max(0f, tempo.doubleShot.Evaluate(tau));
        EventIntervalMultiplier = Mathf.Max(0.3f, tempo.eventInterval.Evaluate(tau));

        if (allSpawners != null && originalShootIntervals != null)
        {
            int want = Mathf.Min(tempo.ThrowersAt(tau), allSpawners.Length);
            if (want != ActiveThrowers) SetThrowers(want, initial, t);
            // Toplam atış hızı uyanık fırlatıcılara ağırlıkla paylaşılır: yeni gelen payını yavaş alır, toplam sıçramaz
            float sumW = 0f;
            for (int i = 0; i < ActiveThrowers; i++) sumW += WakeWeight(i, t);
            for (int i = 0; i < ActiveThrowers && i < originalShootIntervals.Length; i++)
            {
                var s = allSpawners[i];
                if (s == null) continue;
                float w = Mathf.Max(0.04f, WakeWeight(i, t));
                s.shootInterval = originalShootIntervals[i] * Mathf.Max(w, sumW) / (w * FireRate);
            }
        }

        // Eski kademe dizini uyumluluk için τ'dan türer (eşya minStage, olaylar, istatistik); afiş yok
        int st = StageIndexAt(tau);
        if (st != currentStageIndex && stages != null && st >= 0 && st < stages.Length)
        {
            currentStageIndex = st;
            currentStage = stages[st];
            KakLog.Info("[DifficultyManager] Tempo " + tau.ToString("F2") + " → kademe dizini " + st);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicPitch(1f + (tempo.musicPitchAtMax - 1f) * tau);
    }

    float WakeWeight(int i, float t)
    {
        if (wakeAt == null || i >= wakeAt.Length) return 1f;
        return Mathf.Clamp01((t - wakeAt[i]) / Mathf.Max(0.01f, tempo.wakeSeconds));
    }

    void SetThrowers(int want, bool initial, float t)
    {
        for (int i = 0; i < allSpawners.Length; i++)
        {
            var s = allSpawners[i];
            if (s == null) continue;
            bool on = i < want;
            if (on && i >= ActiveThrowers)
            {
                // Başta uyanık olanlar tam payla, oyunda uyananlar yavaşça (ilk atış seyrek, ~wakeSeconds'ta tam)
                if (i < wakeAt.Length) wakeAt[i] = initial ? -99999f : t;
                if (!s.gameObject.activeSelf) s.gameObject.SetActive(true);
                if (!initial) s.Wake();
            }
            else if (!on && s.gameObject.activeSelf) s.gameObject.SetActive(false);
        }
        ActiveThrowers = want;
    }

    int StageIndexAt(float tau)
    {
        if (stages == null) return 0;
        int idx = 0;
        for (int i = 0; i < stages.Length; i++)
            if (stages[i] != null && tau >= tempo.StageTempo(stages[i])) idx = i;
        return idx;
    }

    /// <summary>Taş türü karışımı: yeni kademenin türleri kendi zamanından ÖNCE gelmez; o andan sonra kademe aralığının
    /// typeBlend payı içinde yavaşça çoğalır (önceki kademenin listesiyle harmanlanır).</summary>
    DifficultyStageData MixStage()
    {
        if (tempo == null || stages == null || currentStageIndex <= 0) return currentStage;
        int k = currentStageIndex;
        if (stages[k - 1] == null) return currentStage;
        float a = tempo.StageTempo(stages[k]);
        float b = k + 1 < stages.Length && stages[k + 1] != null ? tempo.StageTempo(stages[k + 1]) : a + 0.2f;
        float w = Mathf.Max(0.005f, (b - a) * tempo.typeBlend);
        float blend = Mathf.Clamp01((Tempo - a) / w);
        return Random.value < blend ? stages[k] : stages[k - 1];
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
        if (tempo != null) { UpdateTempo(false); return; }

        // G2: kademe oyun süresine bağlı (skora değil): yakın geçiş skor çarpanı zorluğu hızlandırmasın
        float t = scoreManager.ElapsedSeconds;

        // Hangi stage'deyiz kontrol et
        for (int i = stages.Length - 1; i >= 0; i--)
        {
            if (stages[i] != null && t >= stages[i].minSeconds)
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
            + " (≥ " + currentStage.minSeconds + " sn)"
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
    [Header("Taş dengesi (G2)")]
    [Tooltip("Bu görsel ölçekten büyük taşlar 'büyük' sayılır")]
    public float largeScaleThreshold = 1.4f;
    [Tooltip("Ekranda aynı anda en fazla bu kadar büyük taş (fazlasında seçim yeniden yapılır)")]
    public int maxLargeOnScreen = 2;

    public ProjectileData PickProjectile()
    {
        var d = PickWeighted();
        // Aynı anda çok sayıda dev kaya "bir büyük bir küçük" dengesizliği yaratıyordu: sınırı aşarsa küçük olanı seç
        for (int tries = 0; tries < 4 && d != null && d.visualScale >= largeScaleThreshold && LargeOnScreen() >= maxLargeOnScreen; tries++)
            d = PickWeighted();
        return d;
    }

    int LargeOnScreen()
    {
        int n = 0;
        var act = Projectile.Active;
        for (int i = 0; i < act.Count; i++)
            if (act[i] != null && act[i].Data != null && act[i].Data.visualScale >= largeScaleThreshold) n++;
        return n;
    }

    ProjectileData PickWeighted()
    {
        var stage = MixStage();
        if (stage == null || stage.availableProjectiles == null || stage.availableProjectiles.Length == 0)
            return null;
        var list = stage.availableProjectiles;
        var w = stage.projectileWeights;
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
        if (tempo != null) return tSpeed;
        if (currentStage != null)
            return currentStage.projectileSpeedMultiplier;
        return 1.0f;
    }

    public int CurrentStageIndex => Mathf.Max(0, currentStageIndex);

    /// <summary>Mevcut kademenin aktif spawner sayısını yeniden uygular (olaylar spawner'ları geçici açıp kapattıktan sonra).</summary>
    public void RefreshSpawnerActivation()
    {
        if (allSpawners == null) return;
        if (tempo != null)
        {
            for (int i = 0; i < allSpawners.Length; i++)
                if (allSpawners[i] != null) allSpawners[i].gameObject.SetActive(i < ActiveThrowers);
            return;
        }
        if (currentStage == null) return;
        for (int i = 0; i < allSpawners.Length; i++)
            if (allSpawners[i] != null) allSpawners[i].gameObject.SetActive(i < currentStage.activeSpawnerCount);
    }

    public float GetProjectileScaleMultiplier()
    {
        if (tempo != null) return tScale;
        if (currentStage != null)
            return currentStage.projectileScaleMultiplier;
        return 1.0f;
    }

    public float GetPlayerSpeedMultiplier()
    {
        if (tempo != null) return tPlayer;
        if (currentStage != null)
            return currentStage.playerSpeedMultiplier;
        return 1.0f;
    }

    public float GetScoreSpeedMultiplier()
    {
        if (tempo != null) return tScore;
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

    /// <summary>Reklamla canlanınca zorluk kaldığı yerden sürer.</summary>
    public void ResumeDifficulty()
    {
        if (stages != null && stages.Length > 0) isActive = true;
    }
}
