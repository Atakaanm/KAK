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

    /// <summary>Bu oyunda hayatta kalınan süre (oyun zamanı).</summary>
    public float ElapsedSeconds { get; private set; }

    // -------------------------------------------------------
    // Olaylar (Events)
    // -------------------------------------------------------

    /// <summary>Skor değiştiğinde tetiklenir. Yeni skor değerini taşır.</summary>
    [Header("Olaylar")]
    public UnityEvent<int> onScoreChanged;

    /// <summary>Belirli bir skor eşiğine ulaşıldığında tetiklenir (pulse efekti için).</summary>
    public UnityEvent<int> onMilestoneReached;

    [Header("Combo (hasar almadan geçen süre)")]
    public float comboStepSeconds = 10f;
    public float comboStep = 0.1f;
    public float comboMax = 2f;
    [Tooltip("Yakın geçiş combo süresine eklenen saniye")]
    public float nearMissComboSeconds = 2f;

    [Header("Yakın Geçiş")]
    public int nearMissBonus = 5;
    public float dashNearMissMultiplier = 2f;

    public float ComboMultiplier { get; private set; } = 1f;
    public float MaxCombo { get; private set; } = 1f;
    public int NearMissCount { get; private set; }
    /// <summary>Bu oyunda toplanan altın (oyun sonunda cüzdana eklenir).</summary>
    public int Coins { get; private set; }

    public void AddCoins(int amount, Vector3 pos)
    {
        Coins += amount;
        GameEvents.RaiseCoinCollected(Coins, pos);
    }
    private float comboTimer;

    void OnEnable()
    {
        GameEvents.PlayerDamaged += OnPlayerDamaged;
        GameEvents.NearMiss += OnNearMiss;
    }

    void OnDisable()
    {
        GameEvents.PlayerDamaged -= OnPlayerDamaged;
        GameEvents.NearMiss -= OnNearMiss;
    }

    void OnPlayerDamaged(int hp, Vector3 pos)
    {
        comboTimer = 0f;
        if (ComboMultiplier > 1f)
        {
            ComboMultiplier = 1f;
            GameEvents.RaiseComboChanged(ComboMultiplier);
        }
    }

    void OnNearMiss(Vector3 pos, bool dashing)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        NearMissCount++;
        float bonus = nearMissBonus * ComboMultiplier * (dashing ? dashNearMissMultiplier : 1f);
        AddScore(Mathf.RoundToInt(bonus));
        comboTimer += nearMissComboSeconds;
    }

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

        ElapsedSeconds += Time.deltaTime;

        comboTimer += Time.deltaTime;
        while (comboTimer >= comboStepSeconds && ComboMultiplier < comboMax - 0.001f)
        {
            comboTimer -= comboStepSeconds;
            ComboMultiplier = Mathf.Min(comboMax, ComboMultiplier + comboStep);
            if (ComboMultiplier > MaxCombo) MaxCombo = ComboMultiplier;
            GameEvents.RaiseComboChanged(ComboMultiplier);
        }

        float currentScoreMult = ComboMultiplier;
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.isActiveAndEnabled)
        {
            currentScoreMult *= DifficultyManager.Instance.GetScoreSpeedMultiplier();
        }

        currentScore += baseScorePerSecond * currentScoreMult * Time.deltaTime;

        int scoreInt = ScoreInt;

        // Skor değiştiyse event'i tetikle ve text'i güncelle
        if (scoreInt != lastDisplayedScore)
        {
            lastDisplayedScore = scoreInt;

            if (scoreText != null)
            {
                scoreText.SetText(Loc.T("hud_score"), scoreInt);
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
        if (scoreText != null) scoreText.SetText(Loc.T("hud_score"), 0);
    }
}