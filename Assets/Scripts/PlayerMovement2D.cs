using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Data (opsiyonel)")]
    public PlayerData playerData;

    [Header("Mobil Kontrol")]
    public VirtualJoystick joystick; // Inspector'dan baglanir
    [Tooltip("G5: klavye 0 = WASD + oklar, 1 = yalnız WASD (1. oyuncu), 2 = yalnız oklar (2. oyuncu)")]
    public int keyboardScheme = 0;

    [Header("Arena Zemin Fizikleri")]
    public float arenaFriction = 1.0f;          // 1.0 = Normal zemin, kuculdukce (or. 0.1) kayganlasir
    public float arenaSpeedMultiplier = 1.0f;   // 1.0 = Normal hiz, kuculdukce bataklik gibi yavaslatir
    [Tooltip("G7: dikey hız çarpanı (buz: aşağı-yukarı daha hızlı)")]
    public float verticalSpeedMultiplier = 1f;
    /// <summary>G7: soğuk göstergesinin yavaşlatması (ColdMeter ayarlar).</summary>
    [System.NonSerialized] public float coldSpeedMultiplier = 1f;
    float gripUntil = -1f, iceFrozenUntil = -1f;
    /// <summary>G7: buz ayakkabısı — süre boyunca kaymadan (normal zemin gibi) hareket.</summary>
    public void Grip(float seconds) { gripUntil = Mathf.Max(gripUntil, Time.time + seconds); }
    public bool Gripping => Time.time < gripUntil;
    /// <summary>G7: soğuktan donma — kısa süre hareketsiz.</summary>
    public void FreezeFor(float seconds) { iceFrozenUntil = Time.time + seconds; if (rb != null) rb.linearVelocity = Vector2.zero; }
    public bool IceFrozen => Time.time < iceFrozenUntil;

    private Rigidbody2D rb;
    private PlayerStatus status;
    private PlayerHealth health;

    /// <summary>Ölüyken karakter olduğu yerde donar (G1: ölümde kayma ölüm hissini öldürüyordu).</summary>
    public bool Frozen => (health != null && health.IsDead) || Time.time < iceFrozenUntil;
    private Vector2 movementInput;
    private float baseMoveSpeed;

    private float currentSpeedBoostMult = 1f;
    private Coroutine speedBoostCoroutine;

    public Vector2 MovementInput => movementInput;

    // Dash: kısa süre sabit hızlı atılma (girdi ve zemin sürtünmesi yok sayılır)
    private float dashTimer;
    private Vector2 dashVelocity;
    public bool IsDashing => dashTimer > 0f;
    /// <summary>Son hareket yönü (dururken dash için).</summary>
    public Vector2 LastDirection { get; private set; } = Vector2.down;

    public void Dash(Vector2 dir, float distance, float duration)
    {
        if (dir.sqrMagnitude < 0.0001f) dir = LastDirection;
        dashVelocity = dir.normalized * (distance / Mathf.Max(0.01f, duration));
        dashTimer = duration;
    }

    /// <summary>
    /// Dışarıdan hareket girdisi (test botu, ileride öğretici/replay). Null ise joystick/klavye kullanılır.
    /// </summary>
    public Vector2? InputOverride { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        status = GetComponent<PlayerStatus>();
        health = GetComponent<PlayerHealth>();
    }

    /// <summary>Hızı ve atılmayı anında sıfırlar (ölüm anı).</summary>
    public void StopImmediately()
    {
        movementInput = Vector2.zero;
        dashTimer = 0f;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    /// <summary>PlayerStatus sonradan eklenirse kendini bağlar (bölüm modu).</summary>
    public void AttachStatus(PlayerStatus s) { status = s; }

    void Start()
    {
        if (playerData != null)
        {
            moveSpeed = playerData.moveSpeed;
        }
        baseMoveSpeed = moveSpeed;
    }

    void Update()
    {
        if (Frozen) { movementInput = Vector2.zero; return; }
        if (InputOverride.HasValue)
        {
            movementInput = Vector2.ClampMagnitude(InputOverride.Value, 1f);
            return;
        }

        // Oncelik joystick'te: eger joystick bagli ve input varsa onu kullan
        if (joystick != null && joystick.Direction.sqrMagnitude > 0.01f)
        {
            movementInput = joystick.Direction;
        }
        else
        {
            // Klavye inputu (editorde test icin)
            float x, y;
            if (keyboardScheme == 0)
            {
                x = Input.GetAxisRaw("Horizontal");
                y = Input.GetAxisRaw("Vertical");
            }
            else if (keyboardScheme == 1)
            {
                x = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
                y = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
            }
            else
            {
                x = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
                y = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
            }
            movementInput = new Vector2(x, y).normalized;
        }
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (speedBoostCoroutine != null) StopCoroutine(speedBoostCoroutine);
        speedBoostCoroutine = StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    /// <summary>Şu anki gerçek hız (zemin, powerup ve zorluk çarpanları dahil, dünya birimi/sn).</summary>
    public float CurrentSpeed
    {
        get
        {
            float s = baseMoveSpeed * arenaSpeedMultiplier * currentSpeedBoostMult * coldSpeedMultiplier;
            if (DifficultyManager.Instance != null && DifficultyManager.Instance.isActiveAndEnabled)
                s *= DifficultyManager.Instance.GetPlayerSpeedMultiplier();
            return s;
        }
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
        baseMoveSpeed = speed;
    }

    private System.Collections.IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        currentSpeedBoostMult = multiplier;
        
        // Zamanın yavaşlamasına bağımsız sürmesini istersen Realtime kullanabiliriz,
        // ama normal saniye sayması için yield return new WaitForSeconds daha iyidir.
        yield return KakTime.WaitGameplay(duration);

        currentSpeedBoostMult = 1f;
    }

    void FixedUpdate()
    {
        if (Frozen) { rb.linearVelocity = Vector2.zero; dashTimer = 0f; return; }
        if (movementInput.sqrMagnitude > 0.01f) LastDirection = movementInput.normalized;
        if (dashTimer > 0f)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = dashVelocity;
            return;
        }

        float currentSpeed = baseMoveSpeed * arenaSpeedMultiplier * currentSpeedBoostMult * coldSpeedMultiplier * (status != null ? status.SpeedMultiplier : 1f);
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.isActiveAndEnabled)
        {
            currentSpeed *= DifficultyManager.Instance.GetPlayerSpeedMultiplier();
        }

        // Eğer zaman yavaşlatma efekti aktifse, oyuncu mermilerden daha hızlı kaçabilmek için kendi hızını korumalıdır.
        // Motorun fizik hızını (Time.timeScale) ters orantıyla dengeleyerek gerçek zamanlı hızını sabit tutuyoruz.
        float slow = KakTime.BaseScale;
        if (slow > 0.01f && slow < 1f && !KakTime.HitStopping)
        {
            currentSpeed /= slow;
        }

        Vector2 targetVelocity = movementInput * currentSpeed;
        targetVelocity.y *= verticalSpeedMultiplier;
        float friction = Gripping ? 1f : arenaFriction;

        if (friction >= 0.99f)
        {
            // Normal zemin, hicbir takilma olmadan hizlanip durur (Anlik / Direct velocity)
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            // Buzlu vb. zemin: ivmelenerek hizlanir, birakinca kaymaya devam eder (Lerp)
            float lerpSpeed = friction * 10f; // 0.1 friction -> 1f lerp hizi (guzel bir kayma hissi)
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, lerpSpeed * Time.fixedDeltaTime);
        }
    }
}