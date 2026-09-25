using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Data (opsiyonel)")]
    public PlayerData playerData;

    [Header("Mobil Kontrol")]
    public VirtualJoystick joystick; // Inspector'dan baglanir

    [Header("Arena Zemin Fizikleri")]
    public float arenaFriction = 1.0f;          // 1.0 = Normal zemin, kuculdukce (or. 0.1) kayganlasir
    public float arenaSpeedMultiplier = 1.0f;   // 1.0 = Normal hiz, kuculdukce bataklik gibi yavaslatir

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private float baseMoveSpeed;

    private float currentSpeedBoostMult = 1f;
    private Coroutine speedBoostCoroutine;

    public Vector2 MovementInput => movementInput;

    /// <summary>
    /// Dışarıdan hareket girdisi (test botu, ileride öğretici/replay). Null ise joystick/klavye kullanılır.
    /// </summary>
    public Vector2? InputOverride { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

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
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
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
            float s = baseMoveSpeed * arenaSpeedMultiplier * currentSpeedBoostMult;
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
        yield return new WaitForSeconds(duration);
        
        currentSpeedBoostMult = 1f;
    }

    void FixedUpdate()
    {
        float currentSpeed = baseMoveSpeed * arenaSpeedMultiplier * currentSpeedBoostMult;
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.isActiveAndEnabled)
        {
            currentSpeed *= DifficultyManager.Instance.GetPlayerSpeedMultiplier();
        }

        // Eğer zaman yavaşlatma efekti aktifse, oyuncu mermilerden daha hızlı kaçabilmek için kendi hızını korumalıdır.
        // Motorun fizik hızını (Time.timeScale) ters orantıyla dengeleyerek gerçek zamanlı hızını sabit tutuyoruz.
        if (Time.timeScale > 0.01f && Time.timeScale < 1f)
        {
            currentSpeed /= Time.timeScale;
        }

        Vector2 targetVelocity = movementInput * currentSpeed;

        if (arenaFriction >= 0.99f)
        {
            // Normal zemin, hicbir takilma olmadan hizlanip durur (Anlik / Direct velocity)
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            // Buzlu vb. zemin: ivmelenerek hizlanir, birakinca kaymaya devam eder (Lerp)
            float lerpSpeed = arenaFriction * 10f; // 0.1 friction -> 1f lerp hizi (guzel bir kayma hissi)
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, lerpSpeed * Time.fixedDeltaTime);
        }
    }
}