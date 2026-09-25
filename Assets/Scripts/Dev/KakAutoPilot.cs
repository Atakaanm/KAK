#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

/// <summary>
/// Test botu: Oyuncuyu otomatik oynatır, mermilerden kaçar.
/// Denge ölçümü (hayatta kalma süresi), regresyon ve performans testleri için.
///
/// Yöntem: Belirli aralıklarla 16 yön + durma adayını değerlendirir.
/// Her aday için oyuncunun ve tüm aktif mermilerin kısa gelecekteki konumlarını
/// tahmin eder, çarpışma yakınlığı + duvar yakınlığı + merkezden uzaklık maliyeti
/// en düşük yönü seçer.
///
/// skill: 1 = iyi oyuncu (uzun öngörü, hızlı tepki), 0 = acemi (kısa öngörü, yavaş tepki, gürültü)
/// Sadece editör ve development build'de derlenir.
/// </summary>
[DisallowMultipleComponent]
public class KakAutoPilot : MonoBehaviour
{
    [Range(0f, 1f)] public float skill = 1f;
    [Tooltip("En yüksek beceride kaç saniye ileriyi tahmin eder")]
    public float maxLookAhead = 0.8f;
    [Tooltip("Oyuncu çarpışma yarıçapı (dünya birimi)")]
    public float playerRadius = 0.25f;

    // Rapor
    public float SurvivalTime { get; private set; }
    public int HitsTaken { get; private set; }
    public bool Finished { get; private set; }

    PlayerMovement2D movement;
    PlayerHealth health;
    Rect area;
    float decisionTimer;
    int lastHealth;

    static readonly Vector2[] Candidates = BuildCandidates();

    static Vector2[] BuildCandidates()
    {
        var list = new Vector2[17];
        for (int i = 0; i < 16; i++)
        {
            float a = i * Mathf.PI * 2f / 16f;
            list[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }
        list[16] = Vector2.zero;
        return list;
    }

    void Start()
    {
        movement = GetComponent<PlayerMovement2D>();
        health = GetComponent<PlayerHealth>();
        lastHealth = health != null ? health.currentHealth : 0;
        area = FindPlayableArea();
        Debug.Log($"[KakAutoPilot] Başladı. skill={skill:F2} alan={area}");
    }

    Rect FindPlayableArea()
    {
        var arena = FindAnyObjectByType<ArenaAutoLayout>();
        if (arena != null && arena.arenaSpriteRenderer != null)
        {
            Rect r = arena.PlayableWorldRect;
            // Oyuncu merkezi duvara bu kadar yaklaşabilir
            return Rect.MinMaxRect(r.xMin + playerRadius, r.yMin + playerRadius, r.xMax - playerRadius, r.yMax - playerRadius);
        }
        return new Rect(transform.position.x - 3f, transform.position.y - 3f, 6f, 6f);
    }

    void OnDisable()
    {
        if (movement != null) movement.InputOverride = null;
    }

    void Update()
    {
        if (movement == null) return;

        bool over = GameManager.Instance != null && GameManager.Instance.IsGameOver;
        if (over)
        {
            if (!Finished)
            {
                Finished = true;
                Debug.Log($"[KakAutoPilot] Oyun bitti. Hayatta kalma: {SurvivalTime:F1} sn, alınan vuruş: {HitsTaken}");
            }
            movement.InputOverride = Vector2.zero;
            return;
        }

        SurvivalTime += Time.deltaTime;

        if (health != null)
        {
            if (health.currentHealth < lastHealth) HitsTaken += lastHealth - health.currentHealth;
            lastHealth = health.currentHealth;
        }

        // Karar süresi oyun zamanıyla (testlerde zaman hızlandırılınca bot adil kalsın)
        decisionTimer -= Time.deltaTime;
        if (decisionTimer > 0f) return;
        decisionTimer = Mathf.Lerp(0.22f, 0.04f, skill);

        float horizon = Mathf.Lerp(0.25f, maxLookAhead, skill);
        float speed = Mathf.Max(0.1f, movement.CurrentSpeed);
        Vector2 p = transform.position;

        Vector2 best = Vector2.zero;
        float bestCost = float.MaxValue;
        for (int i = 0; i < Candidates.Length; i++)
        {
            float c = Cost(Candidates[i], p, speed, horizon);
            if (c < bestCost)
            {
                bestCost = c;
                best = Candidates[i];
            }
        }
        movement.InputOverride = best;

        // Tehlike yüksekse ve en iyi seçenek bile riskliyse dash (usta bot)
        if (dash == null) dash = GetComponent<PlayerDash>();
        if (dash != null && skill > 0.6f && bestCost > dashThreshold && dash.Ready)
            dash.TryDash();
    }

    PlayerDash dash;
    [Tooltip("En iyi yönün maliyeti bunu aşarsa dash (usta bot)")]
    public float dashThreshold = 1.2f;

    float Cost(Vector2 dir, Vector2 p, float speed, float horizon)
    {
        const int steps = 5;
        float cost = 0f;
        var active = Projectile.Active;

        for (int s = 1; s <= steps; s++)
        {
            float t = horizon * s / steps;
            Vector2 pp = p + dir * speed * t;
            pp.x = Mathf.Clamp(pp.x, area.xMin, area.xMax);
            pp.y = Mathf.Clamp(pp.y, area.yMin, area.yMax);
            float weight = 1.5f - (float)s / steps; // yakın gelecek daha önemli

            for (int i = 0; i < active.Count; i++)
            {
                var pr = active[i];
                if (pr == null) continue;
                Vector2 q;
                float r;
                if (pr.Motion == ProjectileMotion.Meteor)
                {
                    // Gökten düşen: iniş noktası sabit; iniş anına yakın tahminler daha önemli
                    q = pr.transform.position;
                    float landIn = pr.FallRemaining01 * (pr.Data != null ? pr.Data.meteorFallTime : 1f);
                    if (Mathf.Abs(landIn - t) > 0.35f) continue;
                    r = playerRadius + (pr.Data != null ? pr.Data.meteorRadius : 0.55f) + 0.1f;
                }
                else
                {
                    q = (Vector2)pr.transform.position + pr.Velocity * t;
                    if (pr.Motion == ProjectileMotion.Bounce) q = Reflect(q);
                    r = playerRadius + ProjectileRadius(pr) + 0.15f;
                }
                float d2 = (pp - q).sqrMagnitude;
                cost += Mathf.Exp(-d2 / (r * r)) * weight * 4f;
            }

            // Duvar yakınlığı: köşeye sıkışmamak için
            float edge = Mathf.Min(Mathf.Min(pp.x - area.xMin, area.xMax - pp.x), Mathf.Min(pp.y - area.yMin, area.yMax - pp.y));
            if (edge < 0.8f) cost += (0.8f - edge) * 0.6f * weight;
        }

        // Merkeze hafif çekim (manevra alanı)
        cost += (p + dir * speed * horizon - area.center).sqrMagnitude * 0.015f;
        // Düşük beceride karar gürültüsü
        cost += Random.value * (1f - skill) * 0.6f;
        return cost;
    }

    static float ProjectileRadius(Projectile pr)
    {
        // Taş prefab'ının çarpışma yarıçapı (0.26) × ölçek; collider okumadan tahsissiz
        return 0.26f * Mathf.Abs(pr.transform.lossyScale.x);
    }

    /// <summary>Seken taş için duvardan yansıyan tahmini konum (oynanabilir alan içinde katlama).</summary>
    Vector2 Reflect(Vector2 q)
    {
        Rect a = area;
        float w = a.width + 2f * playerRadius, h = a.height + 2f * playerRadius;
        float x0 = a.xMin - playerRadius, y0 = a.yMin - playerRadius;
        float fx = Mathf.PingPong(q.x - x0, w) + x0;
        float fy = Mathf.PingPong(q.y - y0, h) + y0;
        return new Vector2(fx, fy);
    }
}
#endif
