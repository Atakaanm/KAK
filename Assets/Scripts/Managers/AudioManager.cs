using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyun genelinde ses: müzik (sahneye göre menü/oyun), efektler (GameEvents'i dinler).
/// Tek kaynak: Resources/AudioManager prefab'ı; hangi sahneden başlanırsa başlansın bir kez oluşur (DontDestroyOnLoad).
/// Sık çalan efektlerde küçük ton ve seviye değişimi (kulak yormasın), aynı sesin üst üste binmesi sınırlı.
/// Ayarlar SaveSystem.Data.settings'te tutulur.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Ses Kaynaklari")]
    public AudioSource musicSource;   // Arka plan muzigi icin
    public AudioSource sfxSource;     // Efekt sesleri icin (tek seferlik)

    [Header("Ses Klipleri - Muzik")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    [Range(0f, 1f)] public float musicVolume = 0.35f;

    [Header("Ses Klipleri - Efekt")]
    public AudioClip buttonClickSfx;
    public AudioClip hitSfx;
    public AudioClip deathSfx;
    public AudioClip shootSfx;
    public AudioClip scoreSfx;        // powerup
    public AudioClip nearMissSfx;
    public AudioClip dashSfx;
    public AudioClip meteorSfx;
    public AudioClip crumbleSfx;
    public AudioClip shieldSfx;
    public AudioClip stageSfx;
    public AudioClip eventSfx;

    [Header("Çeşitlilik")]
    public float pitchJitter = 0.06f;
    [Tooltip("Aynı klip bu süreden sık çalınmaz (sn) — yoğun anlarda gürültüyü önler")]
    public float minRepeat = 0.05f;

    private bool isMusicOn = true;
    private bool isSfxOn = true;
    private bool isVibrationOn = true;

    public bool IsMusicOn => isMusicOn;
    public bool IsSfxOn => isSfxOn;
    public bool IsVibrationOn => isVibrationOn;

    AudioSource[] voices;
    int nextVoice;
    readonly System.Collections.Generic.Dictionary<AudioClip, float> lastPlayed = new System.Collections.Generic.Dictionary<AudioClip, float>();

    /// <summary>Sahnede yoksa Resources/AudioManager prefab'ından oluşturur.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        EnsureExists();
        SceneManager.sceneLoaded -= OnAnySceneLoaded;
        SceneManager.sceneLoaded += OnAnySceneLoaded;
    }

    static void OnAnySceneLoaded(Scene s, LoadSceneMode m) => EnsureExists();

    public static void EnsureExists()
    {
        if (Instance != null || FindAnyObjectByType<AudioManager>() != null) return;
        var prefab = Resources.Load<GameObject>("AudioManager");
        if (prefab != null) Instantiate(prefab).name = "AudioManager";
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Efekt sesleri için küçük bir ses havuzu (ton değişimi her ses için ayrı)
        voices = new AudioSource[4];
        for (int i = 0; i < voices.Length; i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }
        if (musicSource != null) { musicSource.loop = true; musicSource.playOnAwake = false; musicSource.volume = musicVolume; }
        LoadSettings();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.PlayerDamaged += OnDamaged;
        GameEvents.ShieldBlocked += OnShield;
        GameEvents.PowerupCollected += OnPowerup;
        GameEvents.NearMiss += OnNearMiss;
        GameEvents.DashUsed += OnDash;
        GameEvents.MeteorLanded += OnMeteor;
        GameEvents.ProjectileHitWall += OnWall;
        GameEvents.StageChanged += OnStage;
        GameEvents.EndlessEventStarted += OnEvent;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.PlayerDamaged -= OnDamaged;
        GameEvents.ShieldBlocked -= OnShield;
        GameEvents.PowerupCollected -= OnPowerup;
        GameEvents.NearMiss -= OnNearMiss;
        GameEvents.DashUsed -= OnDash;
        GameEvents.MeteorLanded -= OnMeteor;
        GameEvents.ProjectileHitWall -= OnWall;
        GameEvents.StageChanged -= OnStage;
        GameEvents.EndlessEventStarted -= OnEvent;
    }

    void Start() => OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusic(scene.name == SceneLoader.GAME_SCENE ? gameMusic : menuMusic);
    }

    // ── Olaylar ─────────────────────────────────────────
    void OnDamaged(int hp, Vector3 pos) { /* PlayerHealth.TakeDamage PlayHitSfx çağırır */ }
    void OnShield(Vector3 pos) => PlaySfx(shieldSfx);
    void OnPowerup(PowerupData d, Vector3 pos) => PlaySfx(scoreSfx);
    void OnNearMiss(Vector3 pos, bool dash) => PlaySfx(nearMissSfx, dash ? 1.15f : 1f);
    void OnDash(Vector3 pos, Vector2 dir) => PlaySfx(dashSfx);
    void OnMeteor(Vector3 pos) => PlaySfx(meteorSfx, 1f, 0.8f);
    void OnWall(Vector3 pos, Vector2 vel) => PlaySfx(crumbleSfx, 1f, 0.5f);
    void OnStage(string name, int index) { if (index > 0) PlaySfx(stageSfx); }
    void OnEvent(string key) => PlaySfx(eventSfx);

    /// <summary>
    /// Kayitli ses ayarlarini yukler.
    /// </summary>
    void LoadSettings()
    {
        var st = SaveSystem.Data.settings;
        isMusicOn = st.music;
        isSfxOn = st.sfx;
        isVibrationOn = st.vibration;
        if (musicSource != null) musicSource.mute = !isMusicOn;
    }

    /// <summary>
    /// Ayarlari SaveSystem'e kaydeder.
    /// </summary>
    void SaveSettings()
    {
        var st = SaveSystem.Data.settings;
        st.music = isMusicOn;
        st.sfx = isSfxOn;
        st.vibration = isVibrationOn;
        SaveSystem.Save();
    }

    // ── Muzik ──────────────────────────────────────────
    public void ToggleMusic() => SetMusicOn(!isMusicOn);

    public void SetMusicOn(bool on)
    {
        isMusicOn = on;
        if (musicSource != null) musicSource.mute = !isMusicOn;
        SaveSettings();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    // ── Efekt Sesleri ─────────────────────────────────
    public void ToggleSfx() => SetSfxOn(!isSfxOn);

    public void SetSfxOn(bool on)
    {
        isSfxOn = on;
        SaveSettings();
    }

    /// <summary>Tek seferlik efekt (küçük ton/seviye değişimiyle).</summary>
    public void PlaySfx(AudioClip clip) => PlaySfx(clip, 1f, 1f);

    public void PlaySfx(AudioClip clip, float pitch, float volume = 1f)
    {
        if (!isSfxOn || clip == null || voices == null) return;
        float now = Time.unscaledTime;
        if (lastPlayed.TryGetValue(clip, out float last) && now - last < minRepeat) return;
        lastPlayed[clip] = now;

        var v = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        v.pitch = pitch * (1f + Random.Range(-pitchJitter, pitchJitter));
        v.PlayOneShot(clip, volume * Random.Range(0.9f, 1f));
    }

    public void PlayButtonClick() => PlaySfx(buttonClickSfx, 1f, 0.8f);
    public void PlayHitSfx() => PlaySfx(hitSfx);
    public void PlayDeathSfx() => PlaySfx(deathSfx);
    public void PlayShootSfx() => PlaySfx(shootSfx, 1f, 0.55f);
    public void PlayScoreSfx() => PlaySfx(scoreSfx);
    public void PlayGameMusic() => PlayMusic(gameMusic);
    public void PlayMenuMusic() => PlayMusic(menuMusic);

    // ── Titresim ──────────────────────────────────────
    public void ToggleVibration() => SetVibrationOn(!isVibrationOn);

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
