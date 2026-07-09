using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private float defaultSpeed;

    void Awake()
    {
        defaultSpeed = speed;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        // TrailRenderer bileşeni varsa otomatik bağla
        if (trailRenderer == null)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }
    }
    public float speed = 1.5f;
    public Vector2 moveDirection;
    public float lifeTime = 5f;
    public float rotationSpeed;
    public int damage = 1;

    // Arena sınırları — LevelManager veya ArenaAutoLayout tarafından statik olarak set edilir
    private static Bounds arenaBounds;
    private static bool boundsInitialized = false;

    // Sınır dışı tolerans (biraz pay bırak ki tam kenarda patlamasın)
    private const float BOUNDS_PADDING = 0.5f;

    // Pool desteği — başlatma zamanı, lifetime hesabı için
    private float spawnTime;

    // Trail / iz efekti (opsiyonel — inspector'dan atanır)
    [Header("Görsel Efektler (Opsiyonel)")]
    [Tooltip("TrailRenderer varsa otomatik bulunur, yoksa dışarıdan atanabilir")]
    public TrailRenderer trailRenderer;


    void OnEnable()
    {
        // Pool'dan her alınışta (Instantiate ya da Get) yeniden başlat
        spawnTime = Time.time;

        // Trail'i sıfırla (önceki hareketten kalan iz silinsin)
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }
    }

    void Start()
    {
        if (Mathf.Abs(rotationSpeed) < 0.01f)
        {
            rotationSpeed = Random.Range(-360f, 360f);
        }

        // Arena bounds'u henüz set edilmediyse, SpriteRenderer'dan bul
        if (!boundsInitialized)
        {
            FindArenaBounds();
        }

        // Pool kullanılıyorsa Destroy kullanmıyoruz — LifeTime'ı Update'te kontrol et
        // Pool yoksa klasik Destroy yöntemi (güvenli fallback)
        if (ProjectilePool.Instance == null)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    // -------------------------------------------------------
    // Statik Bounds API
    // -------------------------------------------------------

    /// <summary>
    /// Arena sınırlarını statik olarak ayarlar.
    /// LevelManager veya ArenaAutoLayout tarafından çağrılır.
    /// </summary>
    public static void SetArenaBounds(Bounds bounds)
    {
        arenaBounds = bounds;
        boundsInitialized = true;
    }

    /// <summary>
    /// Sahne yeniden yüklenirken sınır bilgisini sıfırlar.
    /// </summary>
    public static void ResetBounds()
    {
        boundsInitialized = false;
    }

    /// <summary>
    /// Arena sprite renderer'dan sınırları otomatik bulur.
    /// </summary>
    static void FindArenaBounds()
    {
        // Sahnedeki ArenaAutoLayout'u bul
        ArenaAutoLayout arena = Object.FindAnyObjectByType<ArenaAutoLayout>();
        if (arena != null && arena.arenaSpriteRenderer != null)
        {
            arenaBounds = arena.arenaSpriteRenderer.bounds;
            boundsInitialized = true;
        }
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// ProjectileData'dan degerleri alarak mermiye uygular.
    /// CornerShooter tarafından Get() sonrası çağrılır.
    /// </summary>
    public void Init(ProjectileData data)
    {
        if (data == null) return;

        speed = data.speed;
        damage = data.damage;
        lifeTime = data.lifeTime;

        if (Mathf.Abs(data.rotationSpeed) > 0.01f)
        {
            rotationSpeed = data.rotationSpeed;
        }
        else
        {
            // Veri'de rotasyon yoksa rastgele bir dönüş hızı ata
            rotationSpeed = Random.Range(-180f, 180f);
        }

        if (data.projectileSprite != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = data.projectileSprite;
        }
    }

    /// <summary>
    /// Pool'dan yeniden kullanılmadan önce durumu sıfırlar.
    /// CornerShooter.Shoot() tarafından çağrılır.
    /// SCALE'e DOKUNMAZ — prefabın orijinal boyutu korunur.
    /// </summary>
    public void ResetState()
    {
        spawnTime = Time.time;
        rotationSpeed = 0f;
        speed = defaultSpeed;
        // localScale kasıtlı olarak sıfırlanmıyor:
        // Prefabın ayarlı boyutu korunmalı, DifficultyManager çarpanı
        // Init() sonrası ayrıca uygulanıyor.

        // Trail iz temizle
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    // -------------------------------------------------------
    // Update
    // -------------------------------------------------------

    void Update()
    {
        // Yaşam süresi doldu mu?
        if (ProjectilePool.Instance != null && Time.time - spawnTime >= lifeTime)
        {
            ReturnToPool();
            return;
        }

        // Arena sınırları dışına çıktıysa iade et / yok et
        if (boundsInitialized && !IsInsideArenaBounds())
        {
            ReturnToPool();
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            Vector2 newPos = rb.position + (moveDirection.normalized * speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            rb.rotation += rotationSpeed * Time.fixedDeltaTime;
        }
        else
        {
            transform.position += (Vector3)(moveDirection.normalized * speed * Time.fixedDeltaTime);
            transform.Rotate(0f, 0f, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // -------------------------------------------------------
    // Çarpışma
    // -------------------------------------------------------

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // Hayalet moddaysa mermi geçsin — hasar verme, pool'a iade etme
            if (playerHealth != null && playerHealth.IsGhost)
                return;

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            ReturnToPool();
        }
    }

    // -------------------------------------------------------
    // Disable / Pool Dönüşü
    // -------------------------------------------------------

    void OnDisable()
    {
        // Trail'i durdur
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }

    /// <summary>
    /// Mermiyi pool'a iade eder. Pool yoksa Destroy kullanır.
    /// </summary>
    void ReturnToPool()
    {
        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------
    // Yardımcı
    // -------------------------------------------------------

    bool IsInsideArenaBounds()
    {
        Vector3 pos = transform.position;
        float minX = arenaBounds.min.x - BOUNDS_PADDING;
        float maxX = arenaBounds.max.x + BOUNDS_PADDING;
        float minY = arenaBounds.min.y - BOUNDS_PADDING;
        float maxY = arenaBounds.max.y + BOUNDS_PADDING;

        return pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY;
    }
}