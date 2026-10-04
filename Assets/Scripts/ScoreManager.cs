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

    [Header("Skor çarpanı (G2: Subway Surfers gibi, yakın geçişle büyür)")]
    [Tooltip("Her yakın geçişte skor hızına eklenen çarpan")]
    public float nearMissMultStep = 0.1f;
    [Tooltip("Hasar almadan geçen her bu kadar saniyede küçük artış")]
    public float survivalStepSeconds = 20f;
    public float survivalMultStep = 0.05f;
    public float multMax = 3f;
    [Tooltip("Hasar alınca çarpan kazancının bu oranı kalır (0 = sıfırlanır)")]
    public float multKeepOnHit = 0.5f;

    [Header("Yakın Geçiş")]
    public int nearMissBonus = 5;
    public float dashNearMissMultiplier = 2f;

    public float ComboMultiplier { get; private set; } = 1f;
    public float MaxCombo { get; private set; } = 1f;
    public int NearMissCount { get; private set; }
    /// <summary>Bu oyunda toplanan altın (oyun sonunda cüzdana eklenir).</summary>
    public int Coins { get; private set; }
    /// <summary>Görevler için: bu oyundaki dash ve kalkanla engelleme sayısı.</summary>
    public int DashCount { get; private set; }
    public int ShieldBlocks { get; private set; }
    void OnDash(Vector3 p, Vector2 d) => DashCount++;
    void OnShieldBlocked(Vector3 p) => ShieldBlocks++;

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
        GameEvents.DashUsed += OnDash;
        GameEvents.ShieldBlocked += OnShieldBlocked;
    }

    void OnDisable()
    {
        GameEvents.PlayerDamaged -= OnPlayerDamaged;
        GameEvents.NearMiss -= OnNearMiss;
        GameEvents.DashUsed -= OnDash;
        GameEvents.ShieldBlocked -= OnShieldBlocked;
    }

    void OnPlayerDamaged(int hp, Vector3 pos)
    {
        comboTimer = 0f;
        if (ComboMultiplier > 1f)
        {
            ComboMultiplier = 1f + (ComboMultiplier - 1f) * multKeepOnHit;
            if (ComboMultiplier < 1.001f) ComboMultiplier = 1f;
            GameEvents.RaiseComboChanged(ComboMultiplier);
        }
    }

    void AddMult(float amount)
    {
        float before = ComboMultiplier;
        ComboMultiplier = Mathf.Min(multMax, ComboMultiplier + amount);
        if (ComboMultiplier > MaxCombo) MaxCombo = ComboMultiplier;
        if (ComboMultiplier > before + 0.0001f) GameEvents.RaiseComboChanged(ComboMultiplier);
    }

    [Header("Gelişim (Faz 15 K3, oyun başında PlayerStats'tan)")]
    public float scoreGain = 1f;
    public float nearMissGain = 1f;
    float baseKeep = -1f, baseMax = -1f;

    /// <summary>Skor ustası, kıl payı ustası, çarpan hafızası ve tavanı (LevelManager oyun başında çağırır).</summary>
    public void ApplyStats(StatSheet s)
    {
        if (s == null) return;
        if (baseKeep < 0f) { baseKeep = multKeepOnHit; baseMax = multMax; }
        scoreGain = StatBuilder.ScoreMult(s);
        nearMissGain = StatBuilder.NearBonusMult(s);
        multKeepOnHit = StatBuilder.ComboKeep(s, baseKeep);
        multMax = StatBuilder.ComboMax(s, baseMax);
    }

    /// <summary>Bir kıl payının skor bonusu (HUD da aynı hesabı gösterir).</summary>
    public int NearMissBonusNow(bool dashing = false) => Mathf.RoundToInt(nearMissBonus * nearMissGain * ComboMultiplier * (dashing ? dashNearMissMultiplier : 1f));

    void OnNearMiss(Vector3 pos, bool dashing)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        NearMissCount++;
        float bonus = NearMissBonusNow(dashing);
        AddScore(Mathf.RoundToInt(bonus));
        AddMult(nearMissMultStep); // G2: yakın geçiş skor hızını artırır
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
        while (comboTimer >= survivalStepSeconds)
        {
            comboTimer -= survivalStepSeconds;
            AddMult(survivalMultStep);
        }

        float currentScoreMult = ComboMultiplier * scoreGain;
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
    /// <summary>Oyun süresini ileri sarar (dev/test: zorluk süreye bağlı, G2).</summary>
    public void AdvanceTime(float seconds) { ElapsedSeconds += Mathf.Max(0f, seconds); }

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