using UnityEngine;

/// <summary>
/// Oyun genelinde ses yonetimini saglayan singleton manager.
/// DontDestroyOnLoad ile sahneler arasi hayatta kalir.
/// Muzik ve efekt ses seviyelerini PlayerPrefs'e kaydeder.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Ses Kaynaklari")]
    public AudioSource musicSource;   // Arka plan muzigi icin
    public AudioSource sfxSource;     // Efekt sesleri icin

    [Header("Ses Klipleri - Muzik")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Ses Klipleri - Efekt")]
    public AudioClip buttonClickSfx;
    public AudioClip hitSfx;
    public AudioClip deathSfx;
    public AudioClip shootSfx;
    public AudioClip scoreSfx;

    // PlayerPrefs anahtarlari
    private const string MUSIC_ON_KEY = "MusicOn";
    private const string SFX_ON_KEY = "SfxOn";
    private const string VIBRATION_ON_KEY = "VibrationOn";

    private bool isMusicOn = true;
    private bool isSfxOn = true;
    private bool isVibrationOn = true;

    public bool IsMusicOn => isMusicOn;
    public bool IsSfxOn => isSfxOn;
    public bool IsVibrationOn => isVibrationOn;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Kayitli ses ayarlarini yukler.
    /// </summary>
    void LoadSettings()
    {
        isMusicOn = PlayerPrefs.GetInt(MUSIC_ON_KEY, 1) == 1;
        isSfxOn = PlayerPrefs.GetInt(SFX_ON_KEY, 1) == 1;
        isVibrationOn = PlayerPrefs.GetInt(VIBRATION_ON_KEY, 1) == 1;

        if (musicSource != null)
        {
            musicSource.mute = !isMusicOn;
        }
    }

    /// <summary>
    /// Ayarlari PlayerPrefs'e kaydeder.
    /// </summary>
    void SaveSettings()
    {
        PlayerPrefs.SetInt(MUSIC_ON_KEY, isMusicOn ? 1 : 0);
        PlayerPrefs.SetInt(SFX_ON_KEY, isSfxOn ? 1 : 0);
        PlayerPrefs.SetInt(VIBRATION_ON_KEY, isVibrationOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ── Muzik Kontrolleri ──────────────────────────────

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;

        if (musicSource != null)
        {
            musicSource.mute = !isMusicOn;
        }

        SaveSettings();
    }

    public void SetMusicOn(bool on)
    {
        isMusicOn = on;

        if (musicSource != null)
        {
            musicSource.mute = !isMusicOn;
        }

        SaveSettings();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // ── Efekt Sesleri ─────────────────────────────────

    public void ToggleSfx()
    {
        isSfxOn = !isSfxOn;
        SaveSettings();
    }

    public void SetSfxOn(bool on)
    {
        isSfxOn = on;
        SaveSettings();
    }

    /// <summary>
    /// Tek seferlik efekt sesi calar.
    /// </summary>
    public void PlaySfx(AudioClip clip)
    {
        if (!isSfxOn || sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickSfx);
    }

    public void PlayHitSfx() { PlaySfx(hitSfx); }
    public void PlayDeathSfx() { PlaySfx(deathSfx); }
    public void PlayShootSfx() { PlaySfx(shootSfx); }
    public void PlayScoreSfx() { PlaySfx(scoreSfx); }
    public void PlayGameMusic() { PlayMusic(gameMusic); }
    public void PlayMenuMusic() { PlayMusic(menuMusic); }

    // ── Titresim ──────────────────────────────────────

    public void ToggleVibration()
    {
        isVibrationOn = !isVibrationOn;
        SaveSettings();
    }

    public void SetVibrationOn(bool on)
    {
        isVibrationOn = on;
        SaveSettings();
    }

    /// <summary>
    /// Titresim calistirir (mobilde).
    /// </summary>
    public void TriggerVibration()
    {
        if (!isVibrationOn) return;

        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        #endif
    }
}
