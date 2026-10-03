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

    // Yakın geçiş takibi (NearMissTracker). Faz 12 H1: oyuncu başına ayrı (iki kişilikte bir oyuncunun yanından geçen
    // taş, diğerinin izleyicisi tarafından hemen "geçti" sayılıyordu)
    public const int NearMissSlots = 2;
    [System.NonSerialized] public readonly float[] NearMissEnterTime = { -1f, -1f };
    [System.NonSerialized] public readonly bool[] NearMissAwarded = new bool[NearMissSlots];
    [System.NonSerialized] public readonly bool[] NearMissDisqualified = new bool[NearMissSlots];
    public ProjectileData Data => data;
    /// <summary>Uçuş yönü (testler, analiz).</summary>
    public Vector2 MoveDirection => moveDirection;
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
    private SpriteRenderer warningRenderer; // göktaşı "!" uyarısı (ilk göktaşında oluşturulur)
    private Vector3 baseScale = Vector3.one; // G7: büyüyen kartopu
    private float launchSpeed;               // Faz 12 H2: büyüdükçe yavaşlama bu hızdan
    private Vector3 visualBaseScale = Vector3.one;
    /// <summary>G7: oyuncunun fırlattığı kartopu — oyuncuya değmez, çarptığı düşman mermisini yok eder.</summary>
    [System.NonSerialized] public bool friendly;
    private const float WarningHeight = 0.42f; // yerden yükseklik (dünya birimi)
    private Vector3 visualBasePos;
    private Color baseTint = Color.white;
    private SpriteRenderer visualRenderer;
    private int baseVisualOrder;
    // Faz 13 K1: karanlık dünyada taş parıltısı (DarkWorld açar)
    public static bool DarkMode;
    private SpriteRenderer glintRenderer;

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
            visualBaseScale = visual.localScale;
            visualRenderer = visual.GetComponent<SpriteRenderer>();
        }
        else visualRenderer = GetComponent<SpriteRenderer>();
        if (visualRenderer != null) { baseTint = visualRenderer.color; baseVisualOrder = visualRenderer.sortingOrder; }
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
        if (warningRenderer != null) warningRenderer.enabled = false;
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
        p.launchSpeed = p.speed;
        float vs = data != null ? data.visualScale : 1f;
        obj.transform.localScale = prefab.transform.localScale * scaleMult * vs;
        p.baseScale = obj.transform.localScale;
        if (DarkMode) p.SetupDark();
        return p;
    }

    /// <summary>Faz 13 K1: karanlıkta küçük parıltı (yön okunur) ya da tamamen görünür (glowInDark).</summary>
    void SetupDark()
    {
        if (data != null && data.glowInDark && visualRenderer != null) visualRenderer.sortingOrder = DarkWorld.GlowOrder;
        if (glintRenderer == null)
        {
            var go = new GameObject("Glint");
            go.transform.SetParent(transform, false);
            glintRenderer = go.AddComponent<SpriteRenderer>();
            glintRenderer.sprite = Resources.Load<Sprite>("Glint");
            if (visualRenderer != null) glintRenderer.sharedMaterial = visualRenderer.sharedMaterial;
            glintRenderer.sortingOrder = DarkWorld.GlowOrder - 2;
        }
        glintRenderer.color = data != null ? data.glowColor : new Color(1f, 0.62f, 0.3f, 0.85f);
        glintRenderer.enabled = true;
        UpdateGlint();
    }

    void UpdateGlint()
    {
        if (glintRenderer == null || !glintRenderer.enabled) return;
        // Kök ölçeğinden bağımsız ~0,26 birim, hafif titreşim; göktaşında iniş noktası "!" ile yeter
        if (motion == ProjectileMotion.Meteor) { glintRenderer.enabled = false; return; }
        float ls = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
        float s = (0.26f + 0.04f * Mathf.Sin(age * 18f)) / 0.32f / ls;
        glintRenderer.transform.localScale = new Vector3(s, s, 1f);
        if (visual != null) glintRenderer.transform.position = visual.position;
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
        rotationSpeed = d.noSpin || d.keepUpright ? 0f : (Mathf.Abs(d.rotationSpeed) > 0.01f ? d.rotationSpeed : Random.Range(-180f, 180f));
        if (d.noSpin && visual != null) visual.localRotation = Quaternion.identity;

        if (d.projectileSprite != null && visualRenderer != null) visualRenderer.sprite = d.projectileSprite;
        if (motion == ProjectileMotion.Meteor && d.warningSprite != null) ShowWarning(d.warningSprite);
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
        for (int i = 0; i < NearMissSlots; i++)
        {
            NearMissEnterTime[i] = -1f;
            NearMissAwarded[i] = false;
            NearMissDisqualified[i] = false;
        }
        data = null;
        motion = ProjectileMotion.Straight;
        splitDone = false;
        bouncesLeft = 0;
        if (col != null) col.enabled = true;
        if (visual != null) { visual.localPosition = visualBasePos; visual.localRotation = Quaternion.identity; visual.localScale = visualBaseScale; }
        if (shadow != null) shadow.localScale = shadowBaseScale;
        if (visualRenderer != null) visualRenderer.color = baseTint;
        if (trailRenderer != null) trailRenderer.Clear();
        if (warningRenderer != null) warningRenderer.enabled = false;
        if (glintRenderer != null) glintRenderer.enabled = false;
        if (visualRenderer != null) visualRenderer.sortingOrder = baseVisualOrder;
        friendly = false;
    }

    /// <summary>G7: kartopu yol aldıkça büyür (collider da ölçekle büyür).</summary>
    const float AppearTime = 0.1f;

    /// <summary>Faz 12 H5: taş fırlatıcıdan küçükten büyüyerek çıkar (birden belirmez). Yalnız görsel.</summary>
    void UpdateAppear()
    {
        if (visual == null || age > AppearTime + 0.05f) return;
        if (data != null && data.growPerSecond > 0f) return; // büyüyen taşta UpdateGrowth uygular
        float a = age < AppearTime ? Mathf.Lerp(0.55f, 1f, age / AppearTime) : 1f;
        visual.localScale = visualBaseScale * a;
    }

    void UpdateGrowth()
    {
        UpdateAppear();
        if (data == null || data.growPerSecond <= 0f) return;
        // Faz 12 H2: çarpana kadar büyür (üst sınır yüksek); büyüdükçe ağırlaşıp yavaşlar → büyük ama okunur ve kaçılabilir
        float max = Mathf.Max(1f, data.maxGrowScale);
        float k = Mathf.Min(max, 1f + data.growPerSecond * age);
        // Bağışlayıcı: görüntü tam büyür, çarpışma alanı daha az (growHitShare) → "değmedi ki!" hissi yok, oyun zorlaşmaz
        float kHit = 1f + (k - 1f) * Mathf.Clamp01(data.growHitShare);
        transform.localScale = baseScale * kHit;
        float vis = k / kHit;
        float appear = age < AppearTime ? Mathf.Lerp(0.55f, 1f, age / AppearTime) : 1f;
        if (visual != null) visual.localScale = visualBaseScale * vis * appear;
        if (shadow != null) shadow.localScale = shadowBaseScale * vis;
        if (data.growSlowdown < 0.999f && max > 1.001f)
            speed = launchSpeed * Mathf.Lerp(1f, data.growSlowdown, (k - 1f) / (max - 1f));
    }

    /// <summary>G7: dost kartopu değdiği düşman mermisini (göktaşı hariç) yok eder, kendisi de dağılır.</summary>
    void UpdateFriendly()
    {
        if (!friendly || col == null) return;
        float r = col.bounds.extents.x;
        var act = Active;
        for (int i = act.Count - 1; i >= 0; i--)
        {
            var o = act[i];
            if (o == null || o == this || o.friendly || o.motion == ProjectileMotion.Meteor || o.col == null) continue;
            float rr = r + o.col.bounds.extents.x;
            if (((Vector2)o.transform.position - (Vector2)transform.position).sqrMagnitude > rr * rr) continue;
            GameEvents.RaiseProjectileHitWall(o.transform.position, Vector2.zero);
            GameEvents.RaiseSnowballSmashed(o.transform.position);
            o.ReturnToPool();
            ReturnToPool();
            return;
        }
    }

    void ShowWarning(Sprite sprite)
    {
        if (warningRenderer == null)
        {
            var go = new GameObject("Warning");
            go.transform.SetParent(transform, false);
            warningRenderer = go.AddComponent<SpriteRenderer>();
            if (visualRenderer != null)
            {
                warningRenderer.sortingLayerID = visualRenderer.sortingLayerID;
                warningRenderer.sharedMaterial = visualRenderer.sharedMaterial; // aynı atlas/materyal: batch bozulmasın
            }
        }
        warningRenderer.sprite = sprite;
        warningRenderer.color = Color.white;
        warningRenderer.enabled = true;
        UpdateWarning(0f);
    }

    /// <summary>Uyarı: kök ölçeğinden bağımsız sabit boy, inişe yaklaştıkça hızlanan yanıp sönme.</summary>
    void UpdateWarning(float k)
    {
        if (warningRenderer == null || !warningRenderer.enabled) return;
        float inv = 1f / Mathf.Max(0.01f, transform.localScale.x);
        float freq = Mathf.Lerp(7f, 24f, k);
        float pulse = 0.5f + 0.5f * Mathf.Sin(age * freq);
        var t = warningRenderer.transform;
        t.localPosition = new Vector3(0f, WarningHeight * inv, 0f);
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one * inv * Mathf.Lerp(0.9f, 1.1f, pulse);
        var c = warningRenderer.color;
        c.a = Mathf.Lerp(0.45f, 1f, pulse);
        warningRenderer.color = c;
        if (visualRenderer != null) warningRenderer.sortingOrder = DarkMode ? DarkWorld.GlowOrder + 3 : visualRenderer.sortingOrder + 5;
    }

    // -------------------------------------------------------
    // Statik Bounds API
    // -------------------------------------------------------

    public static void SetArenaBounds(Bounds bounds)
    {
        arenaBounds = bounds;
        boundsInitialized = true;
    }

    /// <summary>Faz 12 H4: oynanabilir alan (kör atış, rastgele göktaşı). Kurulmadıysa boş.</summary>
    public static Rect ArenaRect => boundsInitialized ? new Rect(arenaBounds.min, arenaBounds.size) : new Rect(-3f, 1f, 6f, 6f);

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
        UpdateGrowth();
        UpdateGlint();

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
        UpdateFriendly();
        if (!gameObject.activeSelf) return;

        var homeTo = motion == ProjectileMotion.Homing ? HomingTarget() : null;
        if (motion == ProjectileMotion.Homing && data != null && age < data.homingDuration && homeTo != null)
        {
            Vector2 to = (Vector2)homeTo.position - (Vector2)transform.position;
            if (to.sqrMagnitude > 0.01f)
            {
                float maxStep = data.homingTurnRate * Time.fixedDeltaTime;
                float ang = Vector2.SignedAngle(moveDirection, to);
                float step = Mathf.Clamp(ang, -maxStep, maxStep);
                moveDirection = Quaternion.Euler(0f, 0f, step) * moveDirection;
            }
        }

        Vector2 delta = moveDirection.normalized * speed * Time.fixedDeltaTime;
        if (motion == ProjectileMotion.Wave && data != null)
        {
            // Faz 13 K2: yanal salınım (konum türevi: A·2πf·cos) — ana yön değişmez, yol okunur
            Vector2 perp = new Vector2(-moveDirection.y, moveDirection.x).normalized;
            float w = 2f * Mathf.PI * data.waveFrequency;
            delta += perp * (data.waveAmplitude * w * Mathf.Cos(w * age) * Time.fixedDeltaTime);
        }
        if (rb != null) rb.MovePosition(rb.position + delta);
        else transform.position += (Vector3)delta;

        if (motion == ProjectileMotion.Wave && visual != null)
        {
            // yarasa: dik durur, sola gidiyorsa ayna, kanat çırpması (dikey ölçek)
            visual.localRotation = Quaternion.identity;
            float flap = 0.75f + 0.25f * Mathf.Abs(Mathf.Sin(age * 14f));
            var sx = visualBaseScale.x * (moveDirection.x < 0f ? -1f : 1f);
            if (age > AppearTime) visual.localScale = new Vector3(sx, visualBaseScale.y * flap, visualBaseScale.z);
        }
        else if (data != null && data.noSpin && visual != null)
            visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg);
        else if (visual != null) visual.Rotate(0f, 0f, rotationSpeed * Time.fixedDeltaTime);
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
        UpdateWarning(k);

        if (k >= 1f)
        {
            // İniş: alan hasarı + kırıntı
            // G5: inişte yarıçaptaki her yaşayan oyuncu hasar alır
            var players = PlayerRegistry.All;
            for (int i = players.Count - 1; i >= 0; i--)
            {
                var ph = players[i];
                if (ph == null || ph.IsDead || ph.IsGhost) continue;
                if (Vector2.Distance(ph.transform.position, transform.position) > data.meteorRadius) continue;
                PlayerHealth.LastHitSource = "Göktaşı";
                ph.TakeDamage(damage);
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
        if (motion == ProjectileMotion.Meteor || friendly) return; // G7: oyuncunun kartopu oyuncuya değmez
        if (other.CompareTag("Player"))
        {
            // Taş yalnızca gövdeye vurur (ayak izi duvarlar içindir). Bkz. PlayerHitbox.
            if (PlayerHitbox.AnyRegistered && !PlayerHitbox.IsHurtbox(other)) return;
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsGhost) return; // hayalet: içinden geçer
            // G7: eldiven — kartopu hasar vermez, yakalanır (fırlatma düğmesiyle geri atılır)
            if (data != null && data.catchable)
            {
                var catcher = other.GetComponentInParent<SnowballCatcher>();
                if (catcher != null && catcher.TryCatch(data)) { ReturnToPool(); return; }
            }
            PlayerHealth.LastHitSource = data != null ? data.projectileName : "Taş";
            var status = other.GetComponentInParent<PlayerStatus>();
            if (status != null && data != null && data.effect != ProjectileEffect.Damage)
                status.Apply(data.effect, data.effectDuration, data.effectStrength, damage);
            else if (playerHealth != null)
                playerHealth.TakeDamage(damage);
            ReturnToPool();
        }
    }

    /// <summary>Güdümlü taş hedefi: en yakın yaşayan oyuncu (tek oyuncuda o oyuncu).</summary>
    // Faz 12 H4: görünmez oyuncuyu izlemez (kilidi kaybeder, düz devam eder)
    Transform HomingTarget() => PlayerRegistry.All.Count > 0 ? PlayerRegistry.NearestVisible(transform.position) : PlayerTarget;

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
