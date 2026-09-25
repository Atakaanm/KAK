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
        SaveSystem.Save();

        StartCoroutine(DeathSequence(finalScore, bestScore));
        lastSeconds = seconds;
    }

    private float lastSeconds;

    private System.Collections.IEnumerator DeathSequence(int finalScore, int bestScore)
    {
        // Kısa yavaş çekim: ölüm anı okunsun, parçacıklar ve sarsıntı görünsün
        KakTime.SetTimeScale(deathSlowMoScale);
        yield return new WaitForSecondsRealtime(deathSlowMoDuration);
        KakTime.SetTimeScale(0f);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverScreen != null)
        {
            int near = scoreManager != null ? scoreManager.NearMissCount : 0;
            float combo = scoreManager != null ? scoreManager.MaxCombo : 1f;
            gameOverScreen.Show(finalScore, bestScore, IsNewBest, lastSeconds, near, combo);
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
