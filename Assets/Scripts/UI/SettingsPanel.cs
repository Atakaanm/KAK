using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Ayarlar panelindeki toggle'lari yoneten script.
/// Muzik, efekt sesi ve titresim acma/kapama islemleri.
///
/// Kurulum:
/// 1. Settings Panel icine bu scripti ekle.
/// 2. Toggle referanslarini bagla.
/// 3. Panel acildiginda OnEnable ile toggle'lar guncellenir.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("Toggle Butonlari")]
    public Toggle musicToggle;
    public Toggle sfxToggle;
    public Toggle vibrationToggle;

    [Header("Toggle Yazi Alanlari (opsiyonel)")]
    public TMP_Text musicStatusText;
    public TMP_Text sfxStatusText;
    public TMP_Text vibrationStatusText;

    [Header("Kapat Butonu")]
    public Button closeButton;

    void OnEnable()
    {
        // Panel acildiginda mevcut ayarlari toggle'lara yansit
        RefreshToggles();
        SetupListeners();
    }

    void OnDisable()
    {
        RemoveListeners();
    }

    void SetupListeners()
    {
        if (musicToggle != null)
            musicToggle.onValueChanged.AddListener(OnMusicToggled);

        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(OnSfxToggled);

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    void RemoveListeners()
    {
        if (musicToggle != null)
            musicToggle.onValueChanged.RemoveListener(OnMusicToggled);

        if (sfxToggle != null)
            sfxToggle.onValueChanged.RemoveListener(OnSfxToggled);

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.RemoveListener(OnVibrationToggled);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    /// <summary>
    /// Toggle'lari AudioManager'daki mevcut degerlere gore gunceller.
    /// </summary>
    void RefreshToggles()
    {
        if (AudioManager.Instance == null) return;

        if (musicToggle != null)
        {
            musicToggle.isOn = AudioManager.Instance.IsMusicOn;
        }

        if (sfxToggle != null)
        {
            sfxToggle.isOn = AudioManager.Instance.IsSfxOn;
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = AudioManager.Instance.IsVibrationOn;
        }

        UpdateStatusTexts();
    }

    // ── Toggle Olaylari ──────────────────────────────

    void OnMusicToggled(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicOn(isOn);
        }

        UpdateStatusTexts();
    }

    void OnSfxToggled(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxOn(isOn);
            AudioManager.Instance.PlayButtonClick(); // Degisikligi hemen duyur
        }

        UpdateStatusTexts();
    }

    void OnVibrationToggled(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVibrationOn(isOn);

            if (isOn)
            {
                AudioManager.Instance.TriggerVibration(); // Acildiginda hemen titret
            }
        }

        UpdateStatusTexts();
    }

    void OnCloseClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        gameObject.SetActive(false);
    }

    // ── Durum Yazilari ────────────────────────────────

    void UpdateStatusTexts()
    {
        if (AudioManager.Instance == null) return;

        if (musicStatusText != null)
        {
            musicStatusText.text = AudioManager.Instance.IsMusicOn ? "Açık" : "Kapalı";
        }

        if (sfxStatusText != null)
        {
            sfxStatusText.text = AudioManager.Instance.IsSfxOn ? "Açık" : "Kapalı";
        }

        if (vibrationStatusText != null)
        {
            vibrationStatusText.text = AudioManager.Instance.IsVibrationOn ? "Açık" : "Kapalı";
        }
    }
}
