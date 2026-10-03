using UnityEngine;

/// <summary>
/// Yakın geçiş: taş oyuncunun yakınından geçip çarpmadan uzaklaşırsa ödül (skor bonusu + combo).
/// Taş başına oyuncu başına bir kez. Hasar alınırsa o sırada yakındaki taşlar sayılmaz.
/// Faz 12 H1: düşmüş oyuncu (iki kişilik) puan kazanmaz; iki oyuncunun takibi ayrı (Projectile.NearMiss* yuvaları).
/// </summary>
public class NearMissTracker : MonoBehaviour
{
    [Tooltip("Taş ile oyuncu merkezi arası bu mesafeye girerse 'yakın'")]
    public float radius = 0.75f;
    [Tooltip("Bu mesafeden daha yakınsa zaten çarpışma bölgesi (sayılmaz)")]
    public float minRadius = 0.3f;

    PlayerMovement2D movement;
    PlayerHealth health;
    float lastDamageTime = -99f;

    void Awake() { movement = GetComponent<PlayerMovement2D>(); health = GetComponent<PlayerHealth>(); }
    void OnEnable() { GameEvents.PlayerDamaged += OnDamaged; }
    void OnDisable() { GameEvents.PlayerDamaged -= OnDamaged; }
    void OnDamaged(int hp, Vector3 pos)
    {
        // Olay oyuncu konumuyla gelir: iki kişilikte yalnız kendi hasarımız
        if (((Vector2)pos - (Vector2)transform.position).sqrMagnitude < 0.04f) lastDamageTime = Time.time;
    }

    int Slot()
    {
        int i = health != null ? PlayerRegistry.All.IndexOf(health) : 0;
        return Mathf.Clamp(i, 0, Projectile.NearMissSlots - 1);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (health != null && health.IsDead) return;
        int slot = Slot();
        Vector2 p = transform.position;
        float r2 = radius * radius;
        var list = Projectile.Active;
        for (int i = 0; i < list.Count; i++)
        {
            var pr = list[i];
            if (pr == null || pr.Motion == ProjectileMotion.Meteor || pr.friendly) continue;
            float d2 = ((Vector2)pr.transform.position - p).sqrMagnitude;
            if (d2 < r2)
            {
                if (pr.NearMissEnterTime[slot] < 0f) pr.NearMissEnterTime[slot] = Time.time;
                if (d2 < minRadius * minRadius) pr.NearMissDisqualified[slot] = true;
            }
            else if (pr.NearMissEnterTime[slot] >= 0f && !pr.NearMissAwarded[slot])
            {
                pr.NearMissAwarded[slot] = true;
                bool clean = !pr.NearMissDisqualified[slot] && lastDamageTime < pr.NearMissEnterTime[slot];
                if (clean) GameEvents.RaiseNearMiss(pr.transform.position, movement != null && movement.IsDashing);
            }
        }
    }
}
