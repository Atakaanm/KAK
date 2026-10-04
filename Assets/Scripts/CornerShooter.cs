using UnityEngine;

/// <summary>
/// Köşe fırlatıcısı: aralıklarla oyuncuya taş atar.
/// Taş türü: zorluk kademesinin listesinden (DifficultyStageData.availableProjectiles), yoksa kendi verisi.
/// Meteor türü taşlar oyuncunun olduğu yere gökten düşer (fırlatıcı yukarı fırlatır).
/// </summary>
public class CornerShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform target;
    public float shootInterval = 1.5f;

    public Transform firePoint;
    public SpawnerDirectionAnimator spawnerVisual;

    [Header("Data (opsiyonel)")]
    public SpawnerData spawnerData; // Atanirsa shootInterval ve mermi tipi buradan alinir

    [Header("Pool Ayarları")]
    [Tooltip("Başlangıçta kaç mermi pool'a eklensin")]
    public int prewarmCount = 5;

    [Header("Uyarı (telegraph)")]
    [Tooltip("Atıştan önce fırlatıcının sıcak renkle parladığı süre (sn)")]
    public float telegraphTime = 0.35f;
    private SpriteRenderer visualRenderer;
    private Color visualBaseColor = Color.white;

    [Header("Bölüm")]
    [Tooltip("Bölüm modunda bu taş kullanılır (zorluk listesi yerine)")]
    public ProjectileData overrideProjectile;

    [Header("Meteor")]
    [Tooltip("Meteor hedefinin oyuncu etrafındaki rastgele sapması")]
    public float meteorScatter = 0.9f;

    private float timer;
    private float burstIn = -1f; // Faz 15 K1: çift atışın ikincisi (sn sonra)

    [Header("Görünmezlik (Faz 12 H4)")]
    [Tooltip("Görünmez oyuncuya doğru atılmayan koni (yarım açı, derece)")]
    public float blindAvoidAngle = 28f;
    [Tooltip("Kör atışta arenanın içine doğru yayılma (yarım açı, derece)")]
    public float blindSpread = 50f;
    SpriteRenderer confusedMark;
    /// <summary>Faz 13 K1: atış uyarısı (0 sakin → 1 atış anı); karanlıkta gözler buna göre yanar.</summary>
    public float Telegraph01 => telegraphTime > 0f ? Mathf.Clamp01(1f - (shootInterval - timer) / (telegraphTime * Projectile.WarningMult)) : 0f;
    /// <summary>Bu fırlatıcı şu an oyuncuyu göremiyor mu (testler ve "?" işareti).</summary>
    public bool Blind { get; private set; }

    void Start()
    {
        if (spawnerData != null)
        {
            shootInterval = spawnerData.shootInterval;
            if (spawnerData.projectileData != null && spawnerData.projectileData.projectilePrefab != null)
                projectilePrefab = spawnerData.projectileData.projectilePrefab;
        }

        if (target == null && Projectile.PlayerTarget != null) target = Projectile.PlayerTarget;
        if (spawnerVisual != null) visualRenderer = spawnerVisual.GetComponent<SpriteRenderer>();
        if (visualRenderer != null) visualBaseColor = visualRenderer.color;

        if (projectilePrefab != null && ProjectilePool.Instance != null)
            ProjectilePool.Instance.Prewarm(projectilePrefab, prewarmCount);
    }

    void Update()
    {
        timer += Time.deltaTime;
        UpdateConfused();

        // Atıştan önce sıcak parlama: oyuncu nereden taş geleceğini okur
        if (visualRenderer != null)
        {
            float t = shootInterval - timer;
            float tele = telegraphTime * Projectile.WarningMult; // Faz 15 K3: Sezgi
            if (t < tele)
            {
                float k = 1f - Mathf.Clamp01(t / tele);
                visualRenderer.color = Color.Lerp(visualBaseColor, KakPalette.Turuncu, k * 0.75f);
            }
            else if (visualRenderer.color != visualBaseColor) visualRenderer.color = visualBaseColor;
        }

        if (burstIn >= 0f)
        {
            burstIn -= Time.deltaTime;
            if (burstIn < 0f) Shoot(0f, false);
        }

        if (timer >= shootInterval)
        {
            Shoot(0f, true);
            timer = 0f;
        }
    }

    /// <summary>Faz 15 K1: oyunda yeni uyanan fırlatıcı: sayaç baştan (ilk atış hemen gelmez).</summary>
    public void Wake()
    {
        timer = 0f;
        burstIn = -1f;
    }

    void OnDisable()
    {
        burstIn = -1f;
        if (visualRenderer != null) visualRenderer.color = visualBaseColor;
        if (confusedMark != null) confusedMark.enabled = false;
    }

    /// <summary>
    /// Faz 12 H4: görünmez oyuncu → hedef yoksa kör. Fırlatıcının başında sallanan "?" (seni arıyor).
    /// Görünen başka oyuncu varsa (iki kişilik) ona atar, kör değildir.
    /// </summary>
    void UpdateConfused()
    {
        Blind = PlayerRegistry.AnyInvisible && PlayerRegistry.RandomVisible() == null;
        if (!Blind && confusedMark == null) return;
        if (confusedMark == null)
        {
            var sprite = Resources.Load<Sprite>("QuestionMark");
            if (sprite == null) return;
            var go = new GameObject("Confused");
            go.transform.SetParent(transform, false);
            confusedMark = go.AddComponent<SpriteRenderer>();
            confusedMark.sprite = sprite;
            confusedMark.sortingOrder = DarkWorld.Active ? DarkWorld.GlowOrder : 120;
            // Fırlatıcıların kök ölçekleri farklı (1 / 4): dünyada hep aynı boyda, 1,5 kat piksel
            float ls = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
            go.transform.localScale = Vector3.one * (1.5f / ls);
        }
        confusedMark.enabled = Blind;
        if (!Blind) return;
        Transform anchor = spawnerVisual != null ? spawnerVisual.transform : transform;
        float top = visualRenderer != null ? visualRenderer.bounds.max.y : anchor.position.y + 0.6f;
        float bob = Mathf.Sin(Time.time * 4f + transform.position.x) * 0.06f;
        confusedMark.transform.position = new Vector3(anchor.position.x, top + 0.18f + bob, 0f);
        confusedMark.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 3f) * 12f);
    }

    /// <summary>Kör atış yönü: arenanın içine doğru rastgele, görünmez oyuncunun bulunduğu dar koni hariç.</summary>
    Vector2 BlindDirection(Vector3 from)
    {
        Rect r = Projectile.ArenaRect;
        Vector2 toCenter = (r.width > 0.1f ? r.center : Vector2.zero) - (Vector2)from;
        float baseAng = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
        float avoid = float.NaN;
        for (int i = 0; i < PlayerRegistry.All.Count; i++)
        {
            var p = PlayerRegistry.All[i];
            if (p == null || p.IsDead) continue;
            Vector2 tp = (Vector2)p.transform.position - (Vector2)from;
            avoid = Mathf.Atan2(tp.y, tp.x) * Mathf.Rad2Deg;
            break;
        }
        float ang = baseAng + Random.Range(-blindSpread, blindSpread);
        if (!float.IsNaN(avoid))
        {
            float d = Mathf.DeltaAngle(avoid, ang);
            if (Mathf.Abs(d) < blindAvoidAngle) ang = avoid + Mathf.Sign(d == 0f ? (Random.value - 0.5f) : d) * (blindAvoidAngle + Random.Range(0f, 15f));
        }
        float rad = ang * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    /// <summary>Zamanlayıcıdan bağımsız hemen ateş (olaylar). angleOffset: hedefe göre sapma (derece).</summary>
    public void FireNow(float angleOffset = 0f)
    {
        Shoot(angleOffset, false);
        timer = 0f;
    }

    ProjectileData PickData()
    {
        if (overrideProjectile != null) return overrideProjectile;
        ProjectileData d = DifficultyManager.Instance != null ? DifficultyManager.Instance.PickProjectile() : null;
        if (d == null && spawnerData != null) d = spawnerData.projectileData;
        return d;
    }

    void Shoot(float angleOffset, bool allowBurst)
    {
        // Faz 15 K1: yüksek tempoda ara sıra hemen ardından ikinci atış (olay atışlarında yok)
        var dm = DifficultyManager.Instance;
        if (allowBurst && dm != null && dm.DoubleShotChance > 0f && Random.value < dm.DoubleShotChance)
            burstIn = dm.DoubleShotDelay;

        if (target == null) target = Projectile.PlayerTarget;
        // G5: iki kişilikte düşmüş oyuncuya atış yok; hedef yaşayan oyunculardan
        if (GameSettings.TwoPlayer && (target == null || !IsAlive(target))) target = PlayerRegistry.RandomAlive();
        // Faz 12 H4: görünmez oyuncu hedef alınmaz; görünen başka oyuncu varsa ona, yoksa kör atış
        bool blind = false;
        var th = target != null ? target.GetComponent<PlayerHealth>() : null;
        if (th != null && th.IsInvisible)
        {
            var seen = PlayerRegistry.RandomVisible();
            if (seen != null) target = seen; else blind = true;
        }
        if (projectilePrefab == null || target == null) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSfx();
        if (spawnerVisual != null) spawnerVisual.PlayAttackVisual();

        ProjectileData data = PickData();
        GameObject prefab = data != null && data.projectilePrefab != null ? data.projectilePrefab : projectilePrefab;

        float speedMult = 1f, scaleMult = 1f;
        if (DifficultyManager.Instance != null)
        {
            speedMult = DifficultyManager.Instance.GetProjectileSpeedMultiplier();
            scaleMult = DifficultyManager.Instance.GetProjectileScaleMultiplier();
        }

        if (data != null && data.motion == ProjectileMotion.Meteor)
        {
            Vector3 ground = target.position + (Vector3)(Random.insideUnitCircle * meteorScatter);
            if (blind) // görünmezken göktaşı arenada rastgele bir yere (oyuncudan uzak)
            {
                Rect ar = Projectile.ArenaRect;
                for (int k = 0; k < 6; k++)
                {
                    ground = new Vector3(Random.Range(ar.xMin + 0.4f, ar.xMax - 0.4f), Random.Range(ar.yMin + 0.4f, ar.yMax - 0.4f), 0f);
                    if (((Vector2)(ground - target.position)).sqrMagnitude > 1.5f * 1.5f) break;
                }
            }
            Projectile.LaunchMeteor(prefab, data, ground, scaleMult);
            NextTarget();
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = target.position;
        Vector2 dir = blind ? BlindDirection(spawnPos) : (Vector2)(targetPos - spawnPos);
        if (Mathf.Abs(angleOffset) > 0.01f) dir = Quaternion.Euler(0f, 0f, angleOffset) * dir;
        Projectile.Launch(prefab, data, spawnPos, dir, speedMult, scaleMult);
        NextTarget();
    }

    /// <summary>G5: iki kişilikte bir sonraki atışın hedefi (uyarı dönüşü doğru kişiye baksın).</summary>
    void NextTarget()
    {
        if (!GameSettings.TwoPlayer) return;
        var t = PlayerRegistry.RandomAlive();
        if (t != null) target = t;
    }

    static bool IsAlive(Transform t)
    {
        var h = t != null ? t.GetComponent<PlayerHealth>() : null;
        return h != null && !h.IsDead;
    }
}
