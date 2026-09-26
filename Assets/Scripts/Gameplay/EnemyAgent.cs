using UnityEngine;

/// <summary>
/// Bölüm düşmanı: EnemyData'ya göre hareket eder ve saldırır.
/// Hareket: Stationary, SideLine (yatay gidip gelme), Wander, Charge (uyarı → hücum), GoalKeeper.
/// Saldırı: Aimed, Predictive, Spread, Burst, Lob (gökten düşen). Atıştan önce sıcak renk uyarısı.
/// </summary>
public class EnemyAgent : MonoBehaviour
{
    public EnemyData data;
    public SpriteRenderer visualRenderer;
    public SpawnerDirectionAnimator directional;
    [Tooltip("Kaleci/izleme hedefi (boşsa oyuncu)")]
    public Transform trackTarget;

    Vector3 home;
    Rect area;
    float fireTimer, stateTimer;
    int burstLeft;
    Vector2 wanderTarget, chargeDir;
    enum ChargeState { Idle, Windup, Charging }
    ChargeState charge;
    Color baseColor = Color.white;
    GameObject prefab;
    float sideDir = 1f;

    public void Setup(EnemyData d, Vector3 pos, Rect playArea, GameObject projectilePrefab)
    {
        data = d;
        home = pos;
        transform.position = pos;
        area = playArea;
        prefab = projectilePrefab;
        fireTimer = Random.Range(0.3f, 1f) * (d != null ? d.fireInterval : 2f);
        stateTimer = d != null ? d.chargeCooldown * Random.Range(0.5f, 1f) : 2f;
        wanderTarget = pos;
        if (visualRenderer != null) baseColor = visualRenderer.color;
    }

    Transform Player => Projectile.PlayerTarget;
    bool Over => GameManager.Instance != null && GameManager.Instance.IsGameOver;

    void Update()
    {
        if (data == null || Over) return;
        float dt = Time.deltaTime;
        Move(dt);
        Attack(dt);
        ContactDamage();
    }

    void Move(float dt)
    {
        Vector3 p = transform.position;
        switch (data.movement)
        {
            case EnemyMovement.SideLine:
                p.x += sideDir * data.moveSpeed * dt;
                if (p.x > home.x + data.range) { p.x = home.x + data.range; sideDir = -1f; }
                if (p.x < home.x - data.range) { p.x = home.x - data.range; sideDir = 1f; }
                break;

            case EnemyMovement.GoalKeeper:
            {
                Transform t = trackTarget != null ? trackTarget : Player;
                if (t != null)
                {
                    float target = Mathf.Clamp(t.position.x, home.x - data.range, home.x + data.range);
                    p.x = Mathf.MoveTowards(p.x, target, data.moveSpeed * dt);
                }
                break;
            }

            case EnemyMovement.Wander:
                if (((Vector2)p - wanderTarget).sqrMagnitude < 0.05f)
                    wanderTarget = new Vector2(Random.Range(area.xMin + 0.6f, area.xMax - 0.6f), Random.Range(area.yMin + 0.6f, area.yMax - 0.6f));
                p = Vector2.MoveTowards(p, wanderTarget, data.moveSpeed * dt);
                break;

            case EnemyMovement.Charge:
                stateTimer -= dt;
                if (charge == ChargeState.Idle && stateTimer <= 0f && Player != null)
                {
                    charge = ChargeState.Windup;
                    stateTimer = 0.55f;
                    chargeDir = ((Vector2)Player.position - (Vector2)p).normalized;
                }
                else if (charge == ChargeState.Windup)
                {
                    Flash(1f - stateTimer / 0.55f);
                    if (stateTimer <= 0f) { charge = ChargeState.Charging; stateTimer = 1.1f; Flash(0f); }
                }
                else if (charge == ChargeState.Charging)
                {
                    p += (Vector3)(chargeDir * data.chargeSpeed * dt);
                    bool hitWall = p.x < area.xMin + 0.3f || p.x > area.xMax - 0.3f || p.y < area.yMin + 0.3f || p.y > area.yMax - 0.3f;
                    if (stateTimer <= 0f || hitWall)
                    {
                        charge = ChargeState.Idle;
                        stateTimer = data.chargeCooldown;
                        if (hitWall) GameEvents.RaiseProjectileHitWall(p, chargeDir * 2f);
                    }
                }
                else
                {
                    // Dinlenirken yavaşça başlangıç noktasına dön
                    p = Vector3.MoveTowards(p, home, data.moveSpeed * 0.4f * dt);
                }
                break;
        }
        p.x = Mathf.Clamp(p.x, area.xMin + 0.25f, area.xMax - 0.25f);
        p.y = Mathf.Clamp(p.y, area.yMin + 0.25f, area.yMax - 0.25f);
        transform.position = p;
    }

    void Attack(float dt)
    {
        if (data.attack == EnemyAttack.None || data.projectile == null || prefab == null || Player == null) return;
        if (charge == ChargeState.Charging || charge == ChargeState.Windup) return;

        fireTimer -= dt;
        if (burstLeft > 0)
        {
            if (fireTimer <= 0f) { FireOne(0f); burstLeft--; fireTimer = 0.15f; if (burstLeft == 0) fireTimer = data.fireInterval; }
            return;
        }
        if (fireTimer < data.telegraph) Flash(1f - Mathf.Clamp01(fireTimer / Mathf.Max(0.01f, data.telegraph)));
        if (fireTimer > 0f) return;
        Flash(0f);
        if (directional != null) directional.PlayAttackVisual();

        switch (data.attack)
        {
            case EnemyAttack.Spread:
                for (int i = 0; i < data.count; i++)
                {
                    float t = data.count > 1 ? (float)i / (data.count - 1) - 0.5f : 0f;
                    FireOne(t * data.spreadAngle);
                }
                fireTimer = data.fireInterval;
                break;
            case EnemyAttack.Burst:
                burstLeft = Mathf.Max(1, data.count);
                fireTimer = 0f;
                break;
            case EnemyAttack.Lob:
                Projectile.LaunchMeteor(prefab, data.projectile, Player.position + (Vector3)(Random.insideUnitCircle * 0.6f));
                fireTimer = data.fireInterval;
                break;
            default:
                FireOne(0f);
                fireTimer = data.fireInterval;
                break;
        }
        if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSfx();
    }

    void FireOne(float angle)
    {
        Vector2 from = transform.position;
        Vector2 target = Player.position;
        if (data.attack == EnemyAttack.Predictive)
        {
            var mv = Player.GetComponent<Rigidbody2D>();
            if (mv != null)
            {
                float travel = Vector2.Distance(from, target) / Mathf.Max(0.5f, data.projectile.speed);
                target += mv.linearVelocity * travel * 0.8f;
            }
        }
        Vector2 dir = target - from;
        if (Mathf.Abs(angle) > 0.01f) dir = Quaternion.Euler(0f, 0f, angle) * dir;
        Projectile.Launch(prefab, data.projectile, from, dir);
    }

    void ContactDamage()
    {
        if (!data.contactDamage || data.movement != EnemyMovement.Charge || Player == null) return;
        if (((Vector2)Player.position - (Vector2)transform.position).sqrMagnitude < 0.45f * 0.45f)
        {
            var st = Player.GetComponent<PlayerStatus>();
            PlayerHealth.LastHitSource = data.enemyName;
            if (st != null) st.Apply(ProjectileEffect.Damage, 0f, 0f, 1);
            else Player.GetComponent<PlayerHealth>()?.TakeDamage(1);
        }
    }

    void Flash(float k)
    {
        if (visualRenderer == null) return;
        visualRenderer.color = Color.Lerp(baseColor * data.tint, KakPalette.Turuncu, k * 0.7f);
    }
}
