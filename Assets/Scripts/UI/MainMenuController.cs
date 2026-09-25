using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ana menu sahnesinin tum UI mantigi.
/// Buton tiklamalari, panel acma/kapama, skor gosterimi burada yonetilir.
///
/// Kurulum:
/// 1. MainMenu sahnesine bos bir GameObject olustur, bu scripti ekle.
/// 2. Inspector'dan buton ve panel referanslarini bagla.
/// 3. Butonlarin OnClick eventlerine asagidaki metodlari ata.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Butonlar")]
    public Button playButton;
    public Button charactersButton;
    public Button settingsButton;
    public Button leaderboardButton;

    [Header("Paneller")]
    public GameObject settingsPanel;
    public GameObject charactersPanel;    // Ileride karakter secim paneli

    [Header("Skor Gosterimi")]
    public TMP_Text bestScoreText;

    [Header("Level Ayari")]
    public LevelData defaultLevel;        // Varsayilan olarak oynanacak level
    public string gameSceneName = "SampleScene"; // Oyun sahnesinin ismi

    [Header("Animasyon (opsiyonel)")]
    public Animator menuAnimator;         // Menu animasyonlari icin

    void Start()
    {
        // Zaman olcegini sifirla (oyundan donerken donmus olabilir)
        KakTime.ResetAll();

        // En iyi skoru goster
        UpdateBestScore();

        // Panelleri kapat
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (charactersPanel != null) charactersPanel.SetActive(false);

        // Buton eventlerini bagla
        SetupButtons();

        // Menu muzigini baslat
        if (AudioManager.Instance != null && AudioManager.Instance.menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
        }
    }

    void SetupButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (charactersButton != null)
            charactersButton.onClick.AddListener(OnCharactersClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
    }

    void UpdateBestScore()
    {
        if (bestScoreText != null)
        {
            int best = GameSettings.BestScore;
            bestScoreText.text = "En İyi Skor: " + best.ToString();
        }
    }

    // ── Buton Metodlari ──────────────────────────────────

    /// <summary>
    /// Oyna butonuna basildiginda oyun sahnesine gecer.
    /// </summary>
    public void OnPlayClicked()
    {
        PlayButtonSound();

        // Secili level'i GameSettings'e kaydet
        if (defaultLevel != null)
        {
            GameSettings.SelectedLevel = defaultLevel;
        }

        // Oyun sahnesine gec (zaman ölçeği SceneLoader'da sıfırlanır)
        if (gameSceneName == SceneLoader.GAME_SCENE) SceneLoader.LoadGame();
        else SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Karakterler butonuna basildiginda karakter secim panelini acar.
    /// </summary>
    public void OnCharactersClicked()
    {
        PlayButtonSound();

        if (charactersPanel != null)
        {
            charactersPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Ayarlar butonuna basildiginda ayarlar panelini acar.
    /// </summary>
    public void OnSettingsClicked()
    {
        PlayButtonSound();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Liderlik butonuna basildiginda. (Ileride local/online leaderboard)
    /// </summary>
    public void OnLeaderboardClicked()
    {
        PlayButtonSound();
        // Simdilik sadece en iyi skoru guncelle
        UpdateBestScore();
        KakLog.Info("[MainMenu] Leaderboard tiklandi — ileride eklenecek.");
    }

    // ── Panel Kapatma ─────────────────────────────────

    /// <summary>
    /// Ayarlar panelini kapatir (panel icerisindeki X butonuna baglanir).
    /// </summary>
    public void CloseSettingsPanel()
    {
        PlayButtonSound();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Karakterler panelini kapatir.
    /// </summary>
    public void CloseCharactersPanel()
    {
        PlayButtonSound();

        if (charactersPanel != null)
        {
            charactersPanel.SetActive(false);
        }
    }

    // ── Yardimci ──────────────────────────────────────

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
