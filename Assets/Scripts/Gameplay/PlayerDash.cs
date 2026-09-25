using UnityEngine;

/// <summary>
/// Dash: kısa atılma + atılma boyunca ölümsüzlük + bekleme süresi. Aksiyon butonu (DashButton) veya Space.
/// Arkada soluklaşan kopyalar bırakır (havuzlu, tahsissiz). Değerler sanat-rehberi/denge tablosunda.
/// </summary>
[RequireComponent(typeof(PlayerMovement2D))]
public class PlayerDash : MonoBehaviour
{
    [Header("Ayarlar")]
    public float distance = 1.7f;
    public float duration = 0.14f;
    public float cooldown = 2.6f;
    [Tooltip("Dash bittikten sonra ek ölümsüzlük")]
    public float graceAfter = 0.08f;

    [Header("Görsel")]
    public SpriteRenderer source;           // oyuncunun görseli (kopyalanacak)
    public int afterImages = 3;
    public float afterImageLife = 0.22f;

    PlayerMovement2D movement;
    PlayerHealth health;
    float readyAt;
    SpriteRenderer[] ghosts;
    float[] ghostAge;
    int nextGhost;
    float ghostTimer;

    public float Cooldown01 => cooldown <= 0f ? 0f : Mathf.Clamp01((readyAt - Time.time) / cooldown); // 1 = yeni kullanıldı
    public bool Ready => Time.time >= readyAt;

    void Awake()
    {
        movement = GetComponent<PlayerMovement2D>();
        health = GetComponent<PlayerHealth>();
        ghosts = new SpriteRenderer[afterImages];
        ghostAge = new float[afterImages];
        for (int i = 0; i < afterImages; i++)
        {
            var go = new GameObject("DashGhost" + i);
            go.transform.SetParent(null);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.enabled = false;
            ghosts[i] = sr;
            ghostAge[i] = 99f;
        }
    }

    void OnDestroy()
    {
        if (ghosts == null) return;
        foreach (var g in ghosts) if (g != null) Destroy(g.gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) TryDash();

        // Kopyaların sönmesi
        for (int i = 0; i < ghosts.Length; i++)
        {
            if (!ghosts[i].enabled) continue;
            ghostAge[i] += Time.unscaledDeltaTime;
            float k = 1f - ghostAge[i] / afterImageLife;
            if (k <= 0f) { ghosts[i].enabled = false; continue; }
            Color c = KakPalette.CamgobegiParlak; c.a = 0.5f * k;
            ghosts[i].color = c;
        }

        if (movement.IsDashing && source != null)
        {
            ghostTimer -= Time.deltaTime;
            if (ghostTimer <= 0f)
            {
                ghostTimer = duration / Mathf.Max(1, afterImages);
                SpawnGhost();
            }
        }
    }

    public bool TryDash()
    {
        if (!Ready) return false;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return false;
        if (KakTime.Paused) return false;
        if (health != null && health.IsDead) return false;

        Vector2 dir = movement.MovementInput.sqrMagnitude > 0.01f ? movement.MovementInput : movement.LastDirection;
        movement.Dash(dir, distance, duration);
        if (health != null) health.SetInvulnerable(duration + graceAfter);
        readyAt = Time.time + cooldown;
        ghostTimer = 0f;
        GameEvents.RaiseDashUsed(transform.position, dir);
        return true;
    }

    void SpawnGhost()
    {
        var g = ghosts[nextGhost];
        nextGhost = (nextGhost + 1) % ghosts.Length;
        g.sprite = source.sprite;
        g.flipX = source.flipX;
        g.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        g.transform.localScale = source.transform.lossyScale;
        g.sortingOrder = source.sortingOrder - 1;
        g.enabled = true;
        ghostAge[System.Array.IndexOf(ghosts, g)] = 0f;
    }
}
