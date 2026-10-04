using UnityEngine;

/// <summary>
/// Faz 15 K8 "Taş kırıcı": survivor.io'daki silahın kaçış karşılığı — belli aralıkla yakındaki en tehlikeli taşı (en yakın) kırar.
/// Aralık seviyeyle kısalır (12 / 9 / 7 sn), menzil 2,2 birim. Göktaşı (düşen) kırılmaz. Kırılınca parçacık + ses.
/// </summary>
public class RockBreaker : MonoBehaviour
{
    public int level = 1;
    public float range = 2.2f;
    static readonly float[] Intervals = { 12f, 12f, 9f, 7f };
    float timer;
    PlayerHealth health;

    void Awake() => health = GetComponent<PlayerHealth>();

    public float Interval => Intervals[Mathf.Clamp(level, 0, Intervals.Length - 1)];
    public int Broken { get; private set; }

    /// <summary>Testler: bekleme bitmiş say.</summary>
    public void ForceReady() => timer = Interval;

    void Update()
    {
        if (health == null || health.IsDead) return;
        timer += Time.deltaTime;
        if (timer < Interval) return;
        var target = Nearest();
        if (target == null) return; // menzilde taş yoksa hazır bekler
        timer = 0f;
        Broken++;
        Vector3 at = target.transform.position;
        if (ProjectilePool.Instance != null) ProjectilePool.Instance.Return(target.gameObject);
        GameEvents.RaiseProjectileHitWall(at, Vector2.zero); // kırılma efekti (FeedbackManager)
        var am = AudioManager.Instance;
        if (am != null) am.PlaySfx(am.crumbleSfx, 1.2f);
    }

    Projectile Nearest()
    {
        Projectile best = null;
        float bd = range * range;
        Vector2 p = transform.position;
        var list = Projectile.Active;
        for (int i = 0; i < list.Count; i++)
        {
            var pr = list[i];
            if (pr == null || pr.Motion == ProjectileMotion.Meteor || pr.friendly) continue;
            float d = ((Vector2)pr.transform.position - p).sqrMagnitude;
            if (d < bd) { bd = d; best = pr; }
        }
        return best;
    }
}

/// <summary>Faz 15 K8 "Hayalet anı": vurulunca 1,2 sn × seviye hayalet (taşların içinden geçer).</summary>
public class GhostMoment : MonoBehaviour
{
    public int level = 1;
    PlayerHealth health;

    void Awake() => health = GetComponent<PlayerHealth>();
    void OnEnable() => GameEvents.PlayerDamaged += OnDamaged;
    void OnDisable() => GameEvents.PlayerDamaged -= OnDamaged;

    void OnDamaged(int hp, Vector3 pos)
    {
        if (health == null || health.IsDead || hp <= 0) return;
        if (((Vector2)pos - (Vector2)transform.position).sqrMagnitude > 0.04f) return; // başka oyuncu
        health.MakeGhost(1.2f * Mathf.Max(1, level));
    }
}
