using TMPro;
using UnityEngine;

/// <summary>
/// Oyun sahnesinin ana yöneticisi: oyun sonu, bölüm sonu, retry/menü geçişleri, zaman yavaşlatma.
/// Sahneye özeldir (DontDestroyOnLoad DEĞİL): her sahne yüklemesinde temiz bir örnek oluşur.
/// Sahneler arası kalıcı veri GameSettings'te, ses AudioManager'da tutulur.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managerlar")]
    public ScoreManager scoreManager;
    public LevelManager levelManager;       // Sahne olusturucu
    public DifficultyManager difficultyManager; // Zorluk yoneticisi

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public GameOverScreen gameOverScreen;
    public TMP_Text finalScoreText;
    public TMP_Text bestScoreText;

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;
    /// <summary>Son oyun yeni rekor mu (oyun sonu ekranı için).</summary>
    public bool IsNewBest { get; private set; }

    private Coroutine timeSlowCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        KakTime.ResetAll();
        AdService.BeginRun();
        unlockMaskAtStart = FeatureGate.UnlockedMask();
        if (FeatureGate.IsUnlocked(Feature.Missions)) MissionSystem.Ensure(); // tamamlananların yerine yenileri

        if (scoreManager == null) scoreManager = GetComponent<ScoreManager>();
        if (scoreManager == null) scoreManager = FindAnyObjectByType<ScoreManager>();

        // Yöneticiler sahnede kalıcı olmalı (KacAtaKac/Sahne Yöneticilerini Kur).
        // Yoksa yedek olarak yaratılır ve uyarı verilir.
        if (levelManager == null) levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager == null)
        {
            levelManager = new GameObject("LevelManager").AddComponent<LevelManager>();
            Debug.LogWarning("[GameManager] LevelManager sahnede yoktu, yedek olarak oluşturuldu. 'KacAtaKac/Sahne Yöneticilerini Kur' aracını çalıştır.");
        }

        if (difficultyManager == null) difficultyManager = FindAnyObjectByType<DifficultyManager>();
        if (difficultyManager == null)
        {
            difficultyManager = new GameObject("DifficultyManager").AddComponent<DifficultyManager>();
            Debug.LogWarning("[GameManager] DifficultyManager sahnede yoktu, yedek olarak oluşturuldu.");
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    [Header("Ölüm Sekansı")]
    [Tooltip("Ölümden sonra panel açılmadan önceki yavaş çekim süresi (gerçek sn)")]
    public float deathSlowMoDuration = 0.7f;
    public float deathSlowMoScale = 0.15f;

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        if (timeSlowCoroutine != null) StopCoroutine(timeSlowCoroutine);

        if (difficultyManager != null) difficultyManager.StopDifficulty();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayDeathSfx();

        // Reklamla devam teklifi varsa kayıt (altın, görev, rekor) bekletilir: oyuncu reddedince bir kez yapılır.
        // Teklif yoksa (varsayılan: reklam kapalı) eskisi gibi hemen.
        reviveOffered = !IsLevelMode && continuePanel != null && AdService.CanShow(AdPlacement.Revive);
        if (!reviveOffered) FinalizeRun();
        StartCoroutine(DeathSequence());
    }

    /// <summary>Oyunu kayda işler: skor/rekor, oyun sayısı, altın, görevler, açılan özellikler. Oyun başına bir kez.</summary>
    void FinalizeRun()
    {
        if (finalized) return;
        finalized = true;
        int finalScore = scoreManager != null ? scoreManager.ScoreInt : 0;
        float seconds = scoreManager != null ? scoreManager.ElapsedSeconds : 0f;
        var save = SaveSystem.Data;
        int bestScore = save.bestScoreEndless;
        IsNewBest = false;
        if (!IsLevelMode)
        {
            IsNewBest = finalScore > bestScore;
            if (IsNewBest) bestScore = save.bestScoreEndless = finalScore;
            if (seconds > save.bestTimeEndless) save.bestTimeEndless = seconds;
        }
        save.gamesPlayed++;
        save.totalPlaySeconds += seconds;

        // Altın: toplanan + hayatta kalma bonusu (skorun %1'i), yalnızca altın bu oyunda açıksa
        RunCoins = 0; RunCoinBonus = 0;
        var coinSpawner = FindAnyObjectByType<CoinSpawner>();
        coinsActiveThisRun = coinSpawner != null && coinSpawner.ActiveThisRun;
        if (coinsActiveThisRun)
        {
            RunCoins = scoreManager != null ? scoreManager.Coins : 0;
            var ch = levelManager != null ? levelManager.CurrentCharacter : null;
            if (ch != null && ch.coinMultiplier > 1f) RunCoins = Mathf.RoundToInt(RunCoins * ch.coinMultiplier);
            RunCoinBonus = finalScore / 100;
            save.coins += RunCoins + RunCoinBonus;
            save.totalCoins += RunCoins + RunCoinBonus;
        }
        // Görevler (bu oyun başında açıksa): oyunu işle, ödülü cüzdana ekle
        MissionReward = 0;
        LastMissions = null;
        JustCompleted.Clear();
        if ((unlockMaskAtStart & (1 << (int)Feature.Missions)) != 0)
        {
            MissionSystem.Ensure();
            var stats = new RunStats
            {
                seconds = seconds,
                nearMisses = scoreManager != null ? scoreManager.NearMissCount : 0,
                dashes = scoreManager != null ? scoreManager.DashCount : 0,
                coins = scoreManager != null ? scoreManager.Coins : 0,
                shieldBlocks = scoreManager != null ? scoreManager.ShieldBlocks : 0,
                maxStage = difficultyManager != null ? difficultyManager.CurrentStageIndex : 0,
            };
            MissionReward = MissionSystem.EvaluateRun(stats, JustCompleted);
            LastMissions = new System.Collections.Generic.List<MissionState>(save.missions);
        }

        // Adım adım açılma: bu oyunla yeni açılan ilk özellik
        NewlyUnlocked = -1;
        int gained = FeatureGate.UnlockedMask() & ~unlockMaskAtStart;
        foreach (var f in FeatureGate.All)
            if ((gained & (1 << (int)f)) != 0) { NewlyUnlocked = (int)f; break; }
        SaveSystem.Save();

        lastSeconds = seconds;
        lastFinalScore = finalScore;
        lastBestScore = bestScore;
    }

    private bool finalized, reviveOffered;
    private int lastFinalScore, lastBestScore;
    [Header("Reklamla devam (Faz Y3)")]
    public ContinuePanel continuePanel;
    [Tooltip("Canlanınca verilen can ve dokunulmazlık süresi")]
    public int reviveHealth = 1;
    public float reviveInvulnerable = 2.5f;
    /// <summary>Bu oyunda reklamla canlanıldı mı (testler, analitik).</summary>
    public int Revives { get; private set; }

    private float lastSeconds;
    /// <summary>Son oyunda toplanan altın ve hayatta kalma bonusu (oyun sonu ekranı).</summary>
    public int RunCoins { get; private set; }
    public int RunCoinBonus { get; private set; }
    private bool coinsActiveThisRun;
    private int unlockMaskAtStart;
    /// <summary>Oyun sonu görev durumu (null: görevler kapalı), bu oyunla tamamlananlar ve toplam ödül.</summary>
    public System.Collections.Generic.List<MissionState> LastMissions { get; private set; }
    public readonly System.Collections.Generic.List<MissionState> JustCompleted = new System.Collections.Generic.List<MissionState>();
    public int MissionReward { get; private set; }
    /// <summary>Bu oyunun sonunda yeni açılan özellik (yoksa -1). Oyun sonu ekranında afiş.</summary>
    public int NewlyUnlocked { get; private set; } = -1;

    private System.Collections.IEnumerator DeathSequence()
    {
        // Kısa yavaş çekim: ölüm anı okunsun, parçacıklar ve sarsıntı görünsün
        KakTime.SetTimeScale(deathSlowMoScale);
        yield return new WaitForSecondsRealtime(deathSlowMoDuration);
        KakTime.SetTimeScale(0f);

        if (reviveOffered)
        {
            // "Devam et?" — kabul: reklam → canlan; ret / süre doldu: kayıt + oyun sonu ekranı
            continuePanel.Show(AdService.Config != null ? AdService.Config.continueSeconds : 5f, AcceptContinue, DeclineContinue);
            yield break;
        }
        ShowGameOverScreen();
    }

    void AcceptContinue()
    {
        AdService.ShowRewarded(AdPlacement.Revive, Revive, DeclineContinue);
    }

    void DeclineContinue()
    {
        if (continuePanel != null) continuePanel.Hide();
        FinalizeRun();
        ShowGameOverScreen();
    }

    /// <summary>Reklam izlendi: oyuncu 1 canla ve kısa dokunulmazlıkla devam eder; yakındaki taşlar temizlenir.</summary>
    void Revive()
    {
        if (continuePanel != null) continuePanel.Hide();
        Revives++;
        isGameOver = false;
        reviveOffered = false;
        foreach (var p in Projectile.Active.ToArray())
            if (ProjectilePool.Instance != null) ProjectilePool.Instance.Return(p.gameObject);
        var ph = FindAnyObjectByType<PlayerHealth>();
        if (ph != null) ph.Revive(reviveHealth, reviveInvulnerable);
        if (difficultyManager != null) difficultyManager.ResumeDifficulty();
        KakTime.SetTimeScale(1f);
        GameEvents.RaisePlayerRevived(ph != null ? ph.transform.position : Vector3.zero);
    }

    void ShowGameOverScreen()
    {
        int finalScore = lastFinalScore, bestScore = lastBestScore;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverScreen != null)
        {
            int near = scoreManager != null ? scoreManager.NearMissCount : 0;
            float combo = scoreManager != null ? scoreManager.MaxCombo : 1f;
            string unlock = NewlyUnlocked >= 0 ? string.Format(Loc.T("unlock_new"), FeatureGate.Name((Feature)NewlyUnlocked)) : null;
            gameOverScreen.Show(finalScore, bestScore, IsNewBest, lastSeconds, near, combo,
                                coinsActiveThisRun ? RunCoins + RunCoinBonus : -1, SaveSystem.Data.coins, unlock);
            gameOverScreen.ShowMissions(LastMissions, JustCompleted);
        }
        else
        {
            if (finalScoreText != null) finalScoreText.SetText("SCORE: {0}", finalScore);
            if (bestScoreText != null) bestScoreText.SetText("BEST: {0}", bestScore);
        }
    }

    public void RetryGame()
    {
        SceneLoader.ReloadCurrentScene();
    }

    /// <summary>
    /// Ana menuye doner. Game Over ekranindaki buton buna baglanir.
    /// </summary>
    public void GoToMainMenu()
    {
        GameSettings.Reset();
        SceneLoader.LoadMenu();
    }

    /// <summary>Bölüm modunda mıyız (hedefli bölüm)?</summary>
    public bool IsLevelMode => LevelModeController.Instance != null && LevelModeController.Instance.Active;

    /// <summary>
    /// Bölüm (Stage) başarıyla bitti: yıldızlar hesaplandı ve kaydedildi (LevelModeController).
    /// </summary>
    public void LevelComplete(int stars, bool newBestStars)
    {
        if (isGameOver) return;
        isGameOver = true;
        if (timeSlowCoroutine != null) StopCoroutine(timeSlowCoroutine);
        if (difficultyManager != null) difficultyManager.StopDifficulty();
        GameEvents.RaiseLevelCompleted(stars);
        StartCoroutine(LevelCompleteSequence(stars, newBestStars));
    }

    private System.Collections.IEnumerator LevelCompleteSequence(int stars, bool newBestStars)
    {
        KakTime.SetTimeScale(0.3f);
        yield return new WaitForSecondsRealtime(0.5f);
        KakTime.SetTimeScale(0f);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverScreen != null)
        {
            var lvl = LevelModeController.Instance != null ? LevelModeController.Instance.Level : null;
            var catalog = WorldCatalog.Load();
            bool hasNext = catalog != null && catalog.Next(lvl) != null;
            gameOverScreen.ShowLevelComplete(stars, newBestStars, hasNext, lastSeconds > 0 ? lastSeconds : (scoreManager != null ? scoreManager.ElapsedSeconds : 0f));
        }
    }

    /// <summary>Bir sonraki bölüme geç (bölüm sonu ekranındaki SONRAKİ butonu).</summary>
    public void NextLevel()
    {
        var lvl = LevelModeController.Instance != null ? LevelModeController.Instance.Level : null;
        var catalog = WorldCatalog.Load();
        var next = catalog != null ? catalog.Next(lvl) : null;
        if (next == null) { GoToMainMenu(); return; }
        GameSettings.SelectedLevel = next;
        SceneLoader.LoadGame();
    }

    /// <summary>Bölümü tekrar oyna / sonsuz modu yeniden başlat (seçili bölüm korunur).</summary>
    /// <summary>
    /// Belirli bir level'i yukler.
    /// Ornek: GameManager.Instance.LoadLevel(level01Data);
    /// </summary>
    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null) return;

        GameSettings.SelectedLevel = levelData;
        SceneLoader.LoadGame();
    }

    /// <summary>
    /// Oyun hızını geçici olarak yavaşlatır (SloMo powerup'ı).
    /// Süre, oyuncunun yaşadığı gerçek zamanla ölçülür; pause sırasında saymaz.
    /// </summary>
    public void TimeSlow(float multiplier, float duration)
    {
        if (isGameOver) return;
        if (timeSlowCoroutine != null) StopCoroutine(timeSlowCoroutine);
        timeSlowCoroutine = StartCoroutine(TimeSlowRoutine(multiplier, duration));
    }

    private System.Collections.IEnumerator TimeSlowRoutine(float multiplier, float duration)
    {
        KakTime.SetTimeScale(multiplier);
        yield return KakTime.WaitGameplay(duration);

        if (!isGameOver)
            KakTime.SetTimeScale(1f);
        timeSlowCoroutine = null;
    }
}
