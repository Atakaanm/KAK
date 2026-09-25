using TMPro;
using UnityEngine;

/// <summary>
/// Bölüm (Stage) modu: hedefi takip eder (hayatta kal / gol at), HUD'da gösterir, kazanınca yıldızları hesaplar.
/// Yıldız: 1 = bitir · 2 = en az 2 can ile bitir · 3 = hiç hasar almadan bitir.
/// </summary>
public class LevelModeController : MonoBehaviour
{
    public static LevelModeController Instance { get; private set; }

    public TMP_Text goalText;

    public LevelData Level { get; private set; }
    public bool Active { get; private set; }
    public int Goals { get; private set; }
    public float Remaining { get; private set; }
    int hits;
    bool done;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnEnable()
    {
        GameEvents.PlayerDamaged += OnDamaged;
        GameEvents.GoalScored += OnGoal;
    }

    void OnDisable()
    {
        GameEvents.PlayerDamaged -= OnDamaged;
        GameEvents.GoalScored -= OnGoal;
    }

    public void Begin(LevelData level)
    {
        Level = level;
        Active = level != null && level.levelType == LevelType.Stage;
        Goals = 0;
        hits = 0;
        done = false;
        Remaining = level != null ? level.surviveSeconds : 0f;
        if (goalText != null) goalText.gameObject.SetActive(Active);
        Refresh();
    }

    void OnDamaged(int hp, Vector3 pos) => hits++;

    void OnGoal(int total)
    {
        if (!Active) return;
        Goals = total;
        Refresh();
        if (Level.goal == LevelGoal.Goals && Goals >= Level.goalsToWin) Win();
    }

    void Update()
    {
        if (!Active || done) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (Level.goal == LevelGoal.Survive)
        {
            Remaining -= Time.deltaTime;
            if (Remaining <= 0f) { Remaining = 0f; Win(); }
            Refresh();
        }
    }

    void Refresh()
    {
        if (goalText == null || !Active) return;
        if (Level.goal == LevelGoal.Survive)
        {
            int s = Mathf.CeilToInt(Remaining);
            goalText.SetText("{0}:{1:00}", s / 60, s % 60);
            goalText.color = Remaining < 5f ? KakPalette.Tehlike : KakPalette.Krem;
        }
        else goalText.SetText(Loc.T("goal_count"), Goals, Level.goalsToWin);
    }

    public int ComputeStars()
    {
        var ph = FindAnyObjectByType<PlayerHealth>();
        int stars = 1;
        if (ph != null && ph.currentHealth >= 2) stars++;
        if (hits == 0) stars++;
        return stars;
    }

    void Win()
    {
        if (done) return;
        done = true;
        int stars = ComputeStars();
        float seconds = GameManager.Instance != null && GameManager.Instance.scoreManager != null ? GameManager.Instance.scoreManager.ElapsedSeconds : 0f;
        bool better = SaveSystem.Data.RecordLevel(Level.levelId, stars, seconds);
        SaveSystem.Save();
        if (GameManager.Instance != null) GameManager.Instance.LevelComplete(stars, better);
    }
}
