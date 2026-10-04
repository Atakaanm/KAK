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
    public IntroStory intro; // G6
    public GameObject worldsPanel; // G7

    [Header("Paneller")]
    public GameObject settingsPanel;
    public GameObject charactersPanel;
    [Header("Gelişim merkezi (Faz 15 K4)")]
    public MenuTabs hubTabs;   // KAHRAMAN · GELİŞİM sekmeleri (charactersPanel = 0. sekme)
    public InfoToast toast;
    public GameObject chestPanel; // Faz 15 K6

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
        if (hubTabs != null) hubTabs.CloseAll();
        if (chestPanel != null) chestPanel.SetActive(false);
        // Faz 15 K3: eski karakter yükseltmeleri ortak gelişime taşındı → iade bir kez söylenir
        var save = SaveSystem.Data;
        if (toast != null && save.legacyRefund > 0 && !save.HasSeen("legacy_refund"))
        {
            toast.Show(string.Format(Loc.T("legacy_refund"), save.legacyRefund), KakPalette.Altin);
            save.MarkSeen("legacy_refund");
            SaveSystem.Save();
        }
        if (petsPanel != null) petsPanel.SetActive(false);
        if (dailyPanel != null) dailyPanel.gameObject.SetActive(false);
        // G6: ilk açılışta hikâye; günlük ödül ondan sonra
        if (intro != null && IntroStory.ShouldShow) intro.Show(TryOpenDaily);
        else
        {
            if (intro != null) intro.gameObject.SetActive(false);
            TryOpenDaily();
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
        var world = EndlessWorlds.Selected(); // G7: seçili dünyanın rekoru
        if (bestScoreText != null) bestScoreText.SetText(Loc.T("best"), world != null ? EndlessWorlds.Best(world.id) : d.bestScoreEndless);
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
        GameSettings.TwoPlayer = false;
        GameSettings.SelectedLevel = SelectedWorldLevel();
        SceneLoader.LoadGame();
    }

    /// <summary>G5: tek telefonda iki kişi (Ata sol, Ada sağ).</summary>
    public void OnTwoPlayerClicked()
    {
        PlayButtonSound();
        GameSettings.TwoPlayer = true;
        GameSettings.SelectedLevel = SelectedWorldLevel();
        SceneLoader.LoadGame();
    }

    public void OnCharactersClicked()
    {
        PlayButtonSound();
        if (charactersFeature != null && !charactersFeature.TryUse()) return; // kilitli: buton sallanır
        if (hubTabs != null) { hubTabs.OpenLast(); return; } // Faz 15 K4: son sekme (ilk kez GELİŞİM)
        Open(charactersPanel);
    }
    /// <summary>Ayarlar → Gizlilik Politikası (Apple 5.1.1: uygulama içinden erişilebilir olmalı).</summary>
    public void OnPrivacyClicked() { PlayButtonSound(); StoreLinks.OpenPrivacy(); }

    public void OnPetsClicked()
    {
        PlayButtonSound();
        if (petsFeature != null && !petsFeature.TryUse()) return;
        Open(petsPanel);
    }

    public void ClosePetsPanel() { PlayButtonSound(); Close(petsPanel); RefreshFeatures(); }

    public void OnSettingsClicked() { PlayButtonSound(); resetArmed = false; RefreshResetLabel(); Open(settingsPanel); }

    /// <summary>G7: DÜNYALAR — Sonsuz modun arenaları. Kilitliyse (ilk oyunlar) düğme sallanır.</summary>
    public void OnLevelsClicked()
    {
        PlayButtonSound();
        if (worldsFeature != null && !worldsFeature.TryUse()) return; // kilitli: buton sallanır
        if (worldsPanel != null) Open(worldsPanel);
    }

    public FeatureButton worldsFeature;
    public void CloseWorldsPanel() { PlayButtonSound(); Close(worldsPanel); }
    public void RefreshWorld() => UpdateStats();

    LevelData SelectedWorldLevel()
    {
        var w = EndlessWorlds.Selected();
        return w != null && w.level != null ? w.level : defaultLevel;
    }

    public void CloseSettingsPanel() { PlayButtonSound(); Close(settingsPanel); }
    public void OnChestsClicked() { PlayButtonSound(); Open(chestPanel); }
    public void CloseChestPanel() { PlayButtonSound(); Close(chestPanel); RefreshFeatures(); }

    public void CloseCharactersPanel()
    {
        PlayButtonSound();
        if (hubTabs != null) hubTabs.CloseAll(); else Close(charactersPanel);
        RefreshFeatures();
    }

    /// <summary>Satın alma sonrası menü butonlarının kilit/YENİ/alınabilir durumları.</summary>
    void RefreshFeatures()
    {
        if (charactersFeature != null) charactersFeature.Refresh();
        if (petsFeature != null) petsFeature.Refresh();
    }

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

    void TryOpenDaily()
    {
        if (dailyPanel != null && DailyReward.CanClaim()) StartCoroutine(OpenDailyDelayed());
    }

    /// <summary>G6: ayarlardan hikâyeyi tekrar izle.</summary>
    public void OnStoryClicked()
    {
        PlayButtonSound();
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (intro != null) intro.Show();
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
