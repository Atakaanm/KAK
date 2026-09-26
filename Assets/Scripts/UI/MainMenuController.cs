using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ana menü: OYNA (Sonsuz Mod), KARAKTER, AYARLAR, BÖLÜMLER (yakında).
/// En iyi skor ve istatistikler SaveSystem'den. Paneller açılırken hafif büyüme animasyonu.
/// Sahne ve bağlantılar KacAtaKac/Ana Menüyü Kur aracıyla kurulur.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Butonlar")]
    public Button playButton;
    public Button charactersButton;
    public Button settingsButton;
    public Button levelsButton;
    [Tooltip("Karakter butonunun kilit/YENİ durumu (FeatureGate)")]
    public FeatureButton charactersFeature;
    [Tooltip("Pet butonu ve paneli (Faz 3c.5)")]
    public FeatureButton petsFeature;
    public GameObject petsPanel;
    [Tooltip("Günlük ödül paneli (Faz 3c.6): alınabiliyorsa menü açılınca kendiliğinden açılır")]
    public DailyRewardPanel dailyPanel;

    [Header("Paneller")]
    public GameObject settingsPanel;
    public GameObject charactersPanel;

    [Header("Skor Gosterimi")]
    public TMP_Text bestScoreText;
    public TMP_Text statsText;

    [Header("Ayarlar")]
    public Button resetProgressButton;
    public TMP_Text resetProgressLabel;

    [Header("Level Ayari")]
    public LevelData defaultLevel;        // Varsayilan olarak oynanacak level

    bool resetArmed;

    void OnEnable() => Loc.Changed += UpdateStats;
    void OnDisable() => Loc.Changed -= UpdateStats;

    void Start()
    {
        KakTime.ResetAll();
        UpdateStats();
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (charactersPanel != null) charactersPanel.SetActive(false);
        if (petsPanel != null) petsPanel.SetActive(false);
        if (dailyPanel != null)
        {
            dailyPanel.gameObject.SetActive(false);
            if (DailyReward.CanClaim()) StartCoroutine(OpenDailyDelayed());
        }

        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (charactersButton != null) charactersButton.onClick.AddListener(OnCharactersClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (levelsButton != null) levelsButton.onClick.AddListener(OnLevelsClicked);
        if (resetProgressButton != null) resetProgressButton.onClick.AddListener(OnResetProgress);

        if (AudioManager.Instance != null && AudioManager.Instance.menuMusic != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
    }

    void UpdateStats()
    {
        var d = SaveSystem.Data;
        if (bestScoreText != null) bestScoreText.SetText(Loc.T("best"), d.bestScoreEndless);
        if (statsText != null)
        {
            int m = Mathf.FloorToInt(d.bestTimeEndless / 60f), s = Mathf.FloorToInt(d.bestTimeEndless % 60f);
            if (d.gamesPlayed > 0) statsText.SetText(Loc.T("stats"), d.gamesPlayed, m, s);
            else statsText.SetText(Loc.T("tagline"));
        }
    }

    // ── Butonlar ─────────────────────────────────────────
    public void OnPlayClicked()
    {
        PlayButtonSound();
        if (defaultLevel != null) GameSettings.SelectedLevel = defaultLevel;
        SceneLoader.LoadGame();
    }

    public void OnCharactersClicked()
    {
        PlayButtonSound();
        if (charactersFeature != null && !charactersFeature.TryUse()) return; // kilitli: buton sallanır
        Open(charactersPanel);
    }
    public void OnPetsClicked()
    {
        PlayButtonSound();
        if (petsFeature != null && !petsFeature.TryUse()) return;
        Open(petsPanel);
    }

    public void ClosePetsPanel() { PlayButtonSound(); Close(petsPanel); }

    public void OnSettingsClicked() { PlayButtonSound(); resetArmed = false; RefreshResetLabel(); Open(settingsPanel); }

    public void OnLevelsClicked()
    {
        PlayButtonSound();
        if (levelsButton != null) WobbleLocked(levelsButton.transform);
    }

    public void CloseSettingsPanel() { PlayButtonSound(); Close(settingsPanel); }
    public void CloseCharactersPanel() { PlayButtonSound(); Close(charactersPanel); }

    public void OnResetProgress()
    {
        PlayButtonSound();
        if (!resetArmed) { resetArmed = true; RefreshResetLabel(); return; }
        SaveSystem.ResetAll();
        resetArmed = false;
        RefreshResetLabel();
        UpdateStats();
        foreach (var t in FindObjectsByType<KakToggle>(FindObjectsInactive.Include, FindObjectsSortMode.None)) t.Refresh();
    }

    void RefreshResetLabel()
    {
        if (resetProgressLabel != null) resetProgressLabel.SetText(Loc.T(resetArmed ? "reset_confirm" : "reset"));
    }

    // ── Panel animasyonu ─────────────────────────────────
    System.Collections.IEnumerator OpenDailyDelayed()
    {
        yield return new WaitForSecondsRealtime(0.6f); // menü yerleşsin, sonra ödül
        Open(dailyPanel.gameObject);
    }

    void Open(GameObject panel)
    {
        if (panel == null) return;
        panel.SetActive(true);
        StartCoroutine(Pop(panel.transform, 0.9f, 1f, 0.18f));
    }

    void Close(GameObject panel)
    {
        if (panel != null) panel.SetActive(false);
    }

    System.Collections.IEnumerator Pop(Transform t, float from, float to, float dur)
    {
        for (float e = 0f; e < dur; e += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - e / dur, 3f);
            t.localScale = Vector3.one * Mathf.Lerp(from, to, k);
            yield return null;
        }
        t.localScale = Vector3.one * to;
    }

    void WobbleLocked(Transform t) => StartCoroutine(Wobble(t));

    System.Collections.IEnumerator Wobble(Transform t)
    {
        Vector3 p = t.localPosition;
        for (float e = 0f; e < 0.3f; e += Time.unscaledDeltaTime)
        {
            t.localPosition = p + new Vector3(Mathf.Sin(e * 60f) * 8f * (1f - e / 0.3f), 0f, 0f);
            yield return null;
        }
        t.localPosition = p;
    }

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
    }
}
