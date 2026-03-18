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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Menuden oyuna geciste yok olmamasi icin (ileride kullanilacak)
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Ileride menuden belirli bir level acmak icin kullanilacak.
    /// Ornek: GameManager.Instance.LoadLevel(level01Data);
    /// </summary>
    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null) return;

        // Sahneyi yeniden yukle ve LevelManager'a data gonder
        // Ileride LevelManager sahne gecisite data'yi PlayerPrefs veya static ile tasiyacak
        PlayerPrefs.SetString("PendingLevelName", levelData.levelName);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}