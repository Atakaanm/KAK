using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Managerlar")]
    public ScoreManager scoreManager;
    public LevelManager levelManager;       // Sahne olusturucu
    public DifficultyManager difficultyManager; // Zorluk yoneticisi

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text bestScoreText;

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    private float defaultTimeScale = 1f;
    private Coroutine timeSlowCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Menuden oyuna geciste yok olmamasi icin
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Time.timeScale = 1f;

        // --- EKSİK MANAGERLARI OTOMATİK YARAT ---
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
            if (levelManager == null)
            {
                GameObject lmObj = new GameObject("LevelManager");
                levelManager = lmObj.AddComponent<LevelManager>();
                Debug.Log("[GameManager] LevelManager sahnede yoktu, otomatik oluşturuldu!");
            }
        }

        if (difficultyManager == null)
        {
            difficultyManager = FindAnyObjectByType<DifficultyManager>();
            if (difficultyManager == null)
            {
                GameObject dmObj = new GameObject("DifficultyManager");
                difficultyManager = dmObj.AddComponent<DifficultyManager>();
                Debug.Log("[GameManager] DifficultyManager sahnede yoktu, otomatik oluşturuldu!");
            }
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;
        if (timeSlowCoroutine != null) StopCoroutine(timeSlowCoroutine);

        // Zorluk sistemini durdur
        if (difficultyManager != null)
        {
            difficultyManager.StopDifficulty();
        }

        int finalScore = 0;
        if (scoreManager != null)
        {
            finalScore = scoreManager.ScoreInt;
        }

        int bestScore = PlayerPrefs.GetInt("BestScore", 0);

        if (finalScore > bestScore)
        {
            bestScore = finalScore;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "SCORE: " + finalScore.ToString();
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = "BEST: " + bestScore.ToString();
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

    /// <summary>
    /// Stage modu için bölüm sonu.
    /// </summary>
    public void LevelComplete()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (scoreManager != null)
        {
            int finalScore = scoreManager.ScoreInt;
            if (finalScore > GameSettings.BestScore)
                GameSettings.BestScore = finalScore;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            var texts = gameOverPanel.GetComponentsInChildren<TMP_Text>();
            foreach (var t in texts)
            {
                if (t.gameObject.name.Contains("Title") || t.gameObject.name.Contains("GameOver"))
                    t.text = "TEBRİKLER!";
            }
        }
    }

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
    /// Oyun hızını geçici olarak yavaşlatır (Powerup için).
    /// </summary>
    public void TimeSlow(float multiplier, float duration)
    {
        if (isGameOver) return;
        if (timeSlowCoroutine != null) StopCoroutine(timeSlowCoroutine);
        timeSlowCoroutine = StartCoroutine(TimeSlowRoutine(multiplier, duration));
    }

    private System.Collections.IEnumerator TimeSlowRoutine(float multiplier, float duration)
    {
        Time.timeScale = defaultTimeScale * multiplier;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        
        // Zaman yavaşlamışken sayacın gerçek zamanda (realTime) sayması gerekir
        yield return new WaitForSecondsRealtime(duration);
        
        if (!isGameOver)
        {
            Time.timeScale = defaultTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }
}