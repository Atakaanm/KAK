using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public float scorePerSecond = 10f;

    private float currentScore = 0f;
    private float baseScorePerSecond;
    private int lastDisplayedScore = -1;

    public int ScoreInt => Mathf.FloorToInt(currentScore);

    // -------------------------------------------------------
    // Olaylar (Events)
    // -------------------------------------------------------

    /// <summary>Skor değiştiğinde tetiklenir. Yeni skor değerini taşır.</summary>
    [Header("Olaylar")]
    public UnityEvent<int> onScoreChanged;

    /// <summary>Belirli bir skor eşiğine ulaşıldığında tetiklenir (pulse efekti için).</summary>
    public UnityEvent<int> onMilestoneReached;

    [Header("Milestone Ayarları")]
    [Tooltip("Her kaç puanda bir milestone eventi ateşlensin?")]
    public int milestoneInterval = 50;
    private int lastMilestone = 0;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    void Start()
    {
        baseScorePerSecond = scorePerSecond;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        float currentScoreMult = 1f;
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.isActiveAndEnabled)
        {
            currentScoreMult = DifficultyManager.Instance.GetScoreSpeedMultiplier();
        }

        currentScore += baseScorePerSecond * currentScoreMult * Time.deltaTime;

        int scoreInt = ScoreInt;

        // Skor değiştiyse event'i tetikle ve text'i güncelle
        if (scoreInt != lastDisplayedScore)
        {
            lastDisplayedScore = scoreInt;

            if (scoreText != null)
            {
                scoreText.SetText("SCORE: {0}", scoreInt);
            }

            onScoreChanged?.Invoke(scoreInt);

            // Milestone kontrolü
            if (milestoneInterval > 0)
            {
                int currentMilestone = (scoreInt / milestoneInterval) * milestoneInterval;
                if (currentMilestone > lastMilestone)
                {
                    lastMilestone = currentMilestone;
                    onMilestoneReached?.Invoke(currentMilestone);
                }
            }
        }
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Oyun içi olaylardan (düşman öldürme, powerup vb.) puan ekler.
    /// </summary>
    public void AddScore(int amount)
    {
        currentScore += amount;
        onScoreChanged?.Invoke(ScoreInt);
    }

    /// <summary>
    /// Skoru sıfırlar (yeni oyun / retry için).
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0f;
        lastDisplayedScore = -1;
        lastMilestone = 0;
        if (scoreText != null) scoreText.text = "SCORE: 0";
    }
}