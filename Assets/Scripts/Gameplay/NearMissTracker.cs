using UnityEngine;

/// <summary>
/// Yakın geçiş: taş oyuncunun yakınından geçip çarpmadan uzaklaşırsa ödül (skor bonusu + combo).
/// Taş başına bir kez. Hasar alınırsa o sırada yakındaki taşlar sayılmaz.
/// </summary>
public class NearMissTracker : MonoBehaviour
{
    [Tooltip("Taş ile oyuncu merkezi arası bu mesafeye girerse 'yakın'")]
    public float radius = 0.75f;
    [Tooltip("Bu mesafeden daha yakınsa zaten çarpışma bölgesi (sayılmaz)")]
    public float minRadius = 0.3f;

    PlayerMovement2D movement;
    float lastDamageTime = -99f;

    void Awake() { movement = GetComponent<PlayerMovement2D>(); }
    void OnEnable() { GameEvents.PlayerDamaged += OnDamaged; }
    void OnDisable() { GameEvents.PlayerDamaged -= OnDamaged; }
    void OnDamaged(int hp, Vector3 pos) { lastDamageTime = Time.time; }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        Vector2 p = transform.position;
        float r2 = radius * radius;
        var list = Projectile.Active;
        for (int i = 0; i < list.Count; i++)
        {
            var pr = list[i];
            if (pr == null || pr.Motion == ProjectileMotion.Meteor) continue;
            float d2 = ((Vector2)pr.transform.position - p).sqrMagnitude;
            if (d2 < r2)
            {
                if (pr.NearMissEnterTime < 0f) pr.NearMissEnterTime = Time.time;
                if (d2 < minRadius * minRadius) pr.NearMissDisqualified = true;
            }
            else if (pr.NearMissEnterTime >= 0f && !pr.NearMissAwarded)
            {
                pr.NearMissAwarded = true;
                bool clean = !pr.NearMissDisqualified && lastDamageTime < pr.NearMissEnterTime;
                if (clean) GameEvents.RaiseNearMiss(pr.transform.position, movement != null && movement.IsDashing);
            }
        }
    }
}
