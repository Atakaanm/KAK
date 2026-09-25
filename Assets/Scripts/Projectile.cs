using UnityEngine;

/// <summary>
/// Havuzlanan mermi. Davranış ProjectileData.motion ile seçilir:
///   Straight · Bounce (duvardan seker) · Homing (kısa süre takip) · Split (parçalanır) · Meteor (gökten düşer)
/// 2.5D yükseklik: Meteor'da görsel alt obje yukarıda, gölge yerde büyür; çarpışma sadece inişte.
/// Başlatma: Projectile.Launch(...) ya da Projectile.LaunchMeteor(...) (havuzdan alır).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float speed = 1.5f;
    public Vector2 moveDirection;
    public float lifeTime = 5f;
    public float rotationSpeed;
    public int damage = 1;

    [Header("Görsel")]
    [Tooltip("Dönen görsel alt obje (gölge dönmesin diye). Boşsa kök döner.")]
    public Transform visual;
    [Tooltip("Yerdeki gölge (Meteor'da uyarı olarak büyür)")]
    public Transform shadow;

    [Header("Görsel Efektler (Opsiyonel)")]
    [Tooltip("TrailRenderer varsa otomatik bulunur, yoksa dışarıdan atanabilir")]
    public TrailRenderer trailRenderer;

    /// <summary>Sahnedeki aktif mermiler (bot, near-miss ve debug araçları için; tahsissiz okunur).</summary>
    public static readonly System.Collections.Generic.List<Projectile> Active = new System.Collections.Generic.List<Projectile>(64);

    /// <summary>Oyuncu (Homing ve Meteor için; LevelManager/oyuncu kendini kaydeder).</summary>
    public static Transform PlayerTarget;

    /// <summary>Merminin anlık hız vektörü (dünya birimi/sn).</summary>
    public Vector2 Velocity => motion == ProjectileMotion.Meteor ? Vector2.zero : moveDirection.normalized * speed;
    public ProjectileMotion Motion => motion;

    // Yakın geçiş takibi (NearMissTracker)
    [System.NonSerialized] public float NearMissEnterTime = -1f;
    [System.NonSerialized] public bool NearMissAwarded;
    [System.NonSerialized] public bool NearMissDisqualified;
    public ProjectileData Data => data;
    /// <summary>Meteor için yere kalan süre oranı (1 = yeni atıldı, 0 = iniş). Diğerlerinde 0.</summary>
    public float FallRemaining01 => motion == ProjectileMotion.Meteor && data != null ? Mathf.Clamp01(1f - age / data.meteorFallTime) : 0f;

    // Arena sınırları — LevelManager tarafından statik olarak set edilir (oynanabilir alan)
    private static Bounds arenaBounds;
    private static bool boundsInitialized = false;
    // Merkez duvar iç yüzünü bu kadar geçince kırılır (taşın yarıçapı kadar)
    private const float BOUNDS_PADDING = 0.2f;
    // Seken taş duvar yüzünden bu kadar içeride döner
    private const float BOUNCE_INSET = 0.28f;

    private Rigidbody2D rb;
    private Collider2D col;
    private float defaultSpeed;
    private ProjectileData data;
    private ProjectileMotion motion = ProjectileMotion.Straight;
    private float age;
    private int bouncesLeft;
    private bool splitDone;
    private Vector3 shadowBaseScale = Vector3.one;
    private Vector3 visualBasePos;
    private Color baseTint = Color.white;
    private SpriteRenderer visualRenderer;

    void Awake()
    {
        defaultSpeed = speed;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        if (trailRenderer == null) trailRenderer = GetComponentInChildren<TrailRenderer>();
        if (visual == null) visual = transform.Find("Visual");
        if (shadow == null) shadow = transform.Find("Shadow");
        if (shadow != null) shadowBaseScale = shadow.localScale;
        if (visual != null)
        {
            visualBasePos = visual.localPosition;
            visualRenderer = visual.GetComponent<SpriteRenderer>();
        }
        else visualRenderer = GetComponent<SpriteRenderer>();
        if (visualRenderer != null) baseTint = visualRenderer.color;
    }

    void OnEnable()
    {
        Active.Add(this);
        age = 0f;
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }
    }

    void OnDisable()
    {
        Active.Remove(this);
        if (trailRenderer != null) trailRenderer.emitting = false;
    }

    void Start()
    {
        if (!boundsInitialized) FindArenaBounds();
        if (ProjectilePool.Instance == null) Destroy(gameObject, lifeTime + 2f);
    }

    // -------------------------------------------------------
    // Başlatma API
    // -------------------------------------------------------

    /// <summary>
    /// Havuzdan mermi alır, veriyi ve zorluk çarpanlarını uygular, yöne fırlatır.
    /// </summary>
    public static Projectile Launch(GameObject prefab, ProjectileData data, Vector3 from, Vector2 direction,
                                    float speedMult = 1f, float scaleMult = 1f)
    {
        if (prefab == null) return null;
        from.z = 0f;
        GameObject obj = ProjectilePool.Instance != null
            ? ProjectilePool.Instance.Get(prefab, from, Quaternion.identity)
            : Instantiate(prefab, from, Quaternion.identity);
        if (obj == null) return null;
        var p = obj.GetComponent<Projectile>();
        if (p == null) return null;

        p.ResetState();
        p.Init(data);
        p.moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.down;
        p.speed *= speedMult;
        float vs = data != null ? data.visualScale : 1f;
        obj.transform.localScale = prefab.transform.localScale * scaleMult * vs;
        return p;
    }

    /// <summary>Gökten düşen taş: hedef noktada gölge büyür, süre sonunda iner.</summary>
    public static Projectile LaunchMeteor(GameObject prefab, ProjectileData data, Vector3 groundPos, float scaleMult = 1f)
    {
        var p = Launch(prefab, data, groundPos, Vector2.down, 0f, scaleMult);
        if (p != null) p.motion = ProjectileMotion.Meteor;
        return p;
    }

    /// <summary>
    /// ProjectileData'dan değerleri mermiye uygular (Launch içinde çağrılır).
    /// </summary>
    public void Init(ProjectileData d)
    {
        data = d;
        motion = d != null ? d.motion : ProjectileMotion.Straight;
        if (d == null) return;

        speed = d.speed;
        damage = d.damage;
        lifeTime = d.lifeTime;
        bouncesLeft = d.bounces;
        rotationSpeed = Mathf.Abs(d.rotationSpeed) > 0.01f ? d.rotationSpeed : Random.Range(-180f, 180f);

        if (d.projectileSprite != null && visualRenderer != null) visualRenderer.sprite = d.projectileSprite;
        if (visualRenderer != null) visualRenderer.color = baseTint * d.tint;
    }

    /// <summary>
    /// Pool'dan yeniden kullanılmadan önce durumu sıfırlar. Ölçeğe dokunmaz (Launch ayarlar).
    /// </summary>
    public void ResetState()
    {
        age = 0f;
        rotationSpeed = 0f;
        speed = defaultSpeed;
        NearMissEnterTime = -1f;
        NearMissAwarded = false;
        NearMissDisqualified = false;
        data = null;
        motion = ProjectileMotion.Straight;
        splitDone = false;
        bouncesLeft = 0;
        if (col != null) col.enabled = true;
        if (visual != null) { visual.localPosition = visualBasePos; visual.localRotation = Quaternion.identity; }
        if (shadow != null) shadow.localScale = shadowBaseScale;
        if (visualRenderer != null) visualRenderer.color = baseTint;
        if (trailRenderer != null) trailRenderer.Clear();
    }

    // -------------------------------------------------------
    // Statik Bounds API
    // -------------------------------------------------------

    public static void SetArenaBounds(Bounds bounds)
    {
        arenaBounds = bounds;
        boundsInitialized = true;
    }

    public static void ResetBounds()
    {
        boundsInitialized = false;
    }

    static void FindArenaBounds()
    {
        ArenaAutoLayout arena = Object.FindAnyObjectByType<ArenaAutoLayout>();
        if (arena != null && arena.arenaSpriteRenderer != null)
        {
            Rect play = arena.PlayableWorldRect;
            arenaBounds = new Bounds(play.center, new Vector3(play.width, play.height, 1f));
            boundsInitialized = true;
        }
    }

    // -------------------------------------------------------
    // Update
    // -------------------------------------------------------

    void Update()
    {
        age += Time.deltaTime;

        if (motion == ProjectileMotion.Meteor)
        {
            UpdateMeteor();
            return;
        }

        if (age >= lifeTime)
        {
            ReturnToPool();
            return;
        }

        if (motion == ProjectileMotion.Split && !splitDone && data != null && age >= data.splitAfter)
        {
            DoSplit();
            return;
        }

        if (boundsInitialized && !IsInsideArenaBounds())
        {
            if (motion == ProjectileMotion.Bounce && bouncesLeft > 0)
            {
                Bounce();
            }
            else
            {
                GameEvents.RaiseProjectileHitWall(transform.position, Velocity);
                ReturnToPool();
            }
        }
    }

    void FixedUpdate()
    {
        if (motion == ProjectileMotion.Meteor) return;

        if (motion == ProjectileMotion.Homing && data != null && age < data.homingDuration && PlayerTarget != null)
        {
            Vector2 to = (Vector2)PlayerTarget.position - (Vector2)transform.position;
            if (to.sqrMagnitude > 0.01f)
            {
                float maxStep = data.homingTurnRate * Time.fixedDeltaTime;
                float ang = Vector2.SignedAngle(moveDirection, to);
                float step = Mathf.Clamp(ang, -maxStep, maxStep);
                moveDirection = Quaternion.Euler(0f, 0f, step) * moveDirection;
            }
        }

        Vector2 delta = moveDirection.normalized * speed * Time.fixedDeltaTime;
        if (rb != null) rb.MovePosition(rb.position + delta);
        else transform.position += (Vector3)delta;

        if (visual != null) visual.Rotate(0f, 0f, rotationSpeed * Time.fixedDeltaTime);
        else if (rb != null) rb.rotation += rotationSpeed * Time.fixedDeltaTime;
    }

    // -------------------------------------------------------
    // Davranışlar
    // -------------------------------------------------------

    void Bounce()
    {
        Vector3 p = transform.position;
        float minX = arenaBounds.min.x + BOUNCE_INSET, maxX = arenaBounds.max.x - BOUNCE_INSET;
        float minY = arenaBounds.min.y + BOUNCE_INSET, maxY = arenaBounds.max.y - BOUNCE_INSET;
        Vector2 d = moveDirection;
        if (p.x < minX) { d.x = Mathf.Abs(d.x); p.x = minX; }
        else if (p.x > maxX) { d.x = -Mathf.Abs(d.x); p.x = maxX; }
        if (p.y < minY) { d.y = Mathf.Abs(d.y); p.y = minY; }
        else if (p.y > maxY) { d.y = -Mathf.Abs(d.y); p.y = maxY; }
        moveDirection = d;
        if (rb != null) rb.position = p; else transform.position = p;
        bouncesLeft--;
        GameEvents.RaiseProjectileHitWall(p, -Velocity * 0.3f);
    }

    void DoSplit()
    {
        splitDone = true;
        ProjectileData childData = data.splitInto != null ? data.splitInto : data;
        GameObject prefab = ProjectilePool.Instance != null ? ProjectilePool.Instance.GetPrefabOf(gameObject) : null;
        if (prefab != null && data.splitCount > 0)
        {
            float speedMult = data.speed > 0.01f ? speed / data.speed : 1f; // zorluk çarpanını koru
            float baseAng = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            float spread = data.splitCount > 1 ? data.splitSpread : 0f;
            // Zorluk ölçek çarpanını koru (çocuğun kendi visualScale'i ayrıca uygulanır)
            float scaleMult = transform.localScale.x / Mathf.Max(0.0001f, prefab.transform.localScale.x * Mathf.Max(0.01f, data.visualScale));
            for (int i = 0; i < data.splitCount; i++)
            {
                float t = data.splitCount > 1 ? (float)i / (data.splitCount - 1) - 0.5f : 0f;
                float a = (baseAng + t * spread) * Mathf.Deg2Rad;
                var child = Launch(prefab, childData, transform.position, new Vector2(Mathf.Cos(a), Mathf.Sin(a)), speedMult, scaleMult);
                // Parçalar tekrar bölünmesin
                if (child != null && child.motion == ProjectileMotion.Split) child.splitDone = true;
            }
        }
        GameEvents.RaiseProjectileHitWall(transform.position, Vector2.zero);
        ReturnToPool();
    }

    void UpdateMeteor()
    {
        if (data == null) { ReturnToPool(); return; }
        float fall = Mathf.Max(0.05f, data.meteorFallTime);
        float k = Mathf.Clamp01(age / fall);           // 0 → 1
        float height = data.meteorStartHeight * (1f - k * k); // hızlanarak düşer
        if (col != null) col.enabled = false;
        if (visual != null) visual.localPosition = visualBasePos + new Vector3(0f, height, 0f);
        if (shadow != null) shadow.localScale = shadowBaseScale * Mathf.Lerp(0.25f, 1.4f, k);

        if (k >= 1f)
        {
            // İniş: alan hasarı + kırıntı
            if (PlayerTarget != null && Vector2.Distance(PlayerTarget.position, transform.position) <= data.meteorRadius)
            {
                var ph = PlayerTarget.GetComponent<PlayerHealth>();
                if (ph != null && !ph.IsGhost)
                {
                    PlayerHealth.LastHitSource = "Göktaşı";
                    ph.TakeDamage(damage);
                }
            }
            GameEvents.RaiseProjectileHitWall(transform.position, Vector2.zero);
            GameEvents.RaiseMeteorLanded(transform.position);
            ReturnToPool();
        }
    }

    // -------------------------------------------------------
    // Çarpışma
    // -------------------------------------------------------

    void OnTriggerEnter2D(Collider2D other)
    {
        if (motion == ProjectileMotion.Meteor) return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsGhost) return; // hayalet: içinden geçer
            if (playerHealth != null)
            {
                PlayerHealth.LastHitSource = data != null ? data.projectileName : "Taş";
                playerHealth.TakeDamage(damage);
            }
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        if (ProjectilePool.Instance != null) ProjectilePool.Instance.Return(gameObject);
        else Destroy(gameObject);
    }

    bool IsInsideArenaBounds()
    {
        Vector3 pos = transform.position;
        return pos.x >= arenaBounds.min.x - BOUNDS_PADDING && pos.x <= arenaBounds.max.x + BOUNDS_PADDING
            && pos.y >= arenaBounds.min.y - BOUNDS_PADDING && pos.y <= arenaBounds.max.y + BOUNDS_PADDING;
    }
}
