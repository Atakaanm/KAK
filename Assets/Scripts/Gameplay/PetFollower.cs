using UnityEngine;

/// <summary>
/// Oyuncuyu takip eden pet (Faz 3c.5). Oyuncunun arkasında, gecikmeli yumuşak takip; uçanlar süzülür.
/// Sıralama oyuncuya göre (önünde/arkasında), gölgeli. Pasif: altın mıknatısı ya da aralıklı kalkan.
/// LevelManager oyun başında Spawn eder (FeatureGate.Pets açık ve pet seçiliyse).
/// </summary>
public class PetFollower : MonoBehaviour
{
    public PetData data;
    public Transform player;
    public SpriteRenderer body;
    public SpriteRenderer glow;
    public SpriteRenderer shadow;

    SpriteRenderer playerSprite;
    PlayerMovement2D movement;
    PlayerHealth health;
    Vector3 vel;
    float t, shieldTimer;
    Vector2 side = Vector2.left;

    public static PetFollower Instance { get; private set; }
    public float ShieldTimer => shieldTimer; // testler için

    /// <summary>Pet nesnesini koddan kurar (prefab gerekmez).</summary>
    public static PetFollower Spawn(PetData data, Transform player, Sprite shadowSprite, Sprite glowSprite)
    {
        if (data == null || player == null) return null;
        var go = new GameObject("Pet_" + data.id);
        var pf = go.AddComponent<PetFollower>();
        pf.data = data;
        pf.player = player;
        pf.body = new GameObject("Body").AddComponent<SpriteRenderer>();
        pf.body.transform.SetParent(go.transform, false);
        pf.body.sprite = data.frames != null && data.frames.Length > 0 ? data.frames[0] : null;
        if (shadowSprite != null)
        {
            pf.shadow = new GameObject("Shadow").AddComponent<SpriteRenderer>();
            pf.shadow.transform.SetParent(go.transform, false);
            pf.shadow.sprite = shadowSprite;
            pf.shadow.color = new Color(1f, 1f, 1f, 0.3f);
            pf.shadow.transform.localScale = new Vector3(0.07f, 0.03f, 1f);
        }
        if (data.glow && glowSprite != null)
        {
            pf.glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
            pf.glow.transform.SetParent(pf.body.transform, false);
            pf.glow.sprite = glowSprite;
            pf.glow.color = data.glowColor;
            pf.glow.transform.localScale = Vector3.one * 0.35f;
        }
        go.transform.position = player.position + new Vector3(-0.6f, 0.3f, 0f);
        return pf;
    }

    void Awake() => Instance = this;
    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        if (player != null)
        {
            movement = player.GetComponent<PlayerMovement2D>();
            health = player.GetComponent<PlayerHealth>();
            if (health != null) playerSprite = health.playerSpriteRenderer;
        }
    }

    void Update()
    {
        if (player == null || data == null) return;
        float dt = Time.deltaTime;
        t += dt;

        // Hareket yönünün arkasında dur (dönerken yan değiştirir)
        if (movement != null && movement.LastDirection.sqrMagnitude > 0.01f) side = -movement.LastDirection.normalized;
        Vector3 target = player.position + (Vector3)(side * 0.6f) + new Vector3(0f, data.flying ? 0.35f : -0.1f, 0f);
        transform.position = Vector3.SmoothDamp(transform.position, target, ref vel, 0.18f);

        // Kare animasyonu + uçanlar için süzülme
        if (data.frames != null && data.frames.Length > 0)
            body.sprite = data.frames[(int)(t * data.fps) % data.frames.Length];
        float hover = data.flying ? 0.22f + Mathf.Sin(t * 5f) * 0.05f : 0f;
        body.transform.localPosition = new Vector3(0f, hover, 0f);
        body.flipX = vel.x < -0.05f;
        if (shadow != null) shadow.transform.localPosition = new Vector3(0f, -0.08f, 0f);
        if (glow != null) glow.color = new Color(data.glowColor.r, data.glowColor.g, data.glowColor.b, data.glowColor.a * (0.75f + 0.25f * Mathf.Sin(t * 6f)));

        // Sıralama: oyuncudan aşağıdaysa önünde
        if (playerSprite != null)
        {
            int o = playerSprite.sortingOrder + (transform.position.y < player.position.y - 0.05f ? 2 : -2);
            body.sortingLayerID = playerSprite.sortingLayerID;
            body.sortingOrder = o;
            if (glow != null) { glow.sortingLayerID = playerSprite.sortingLayerID; glow.sortingOrder = o - 1; }
            if (shadow != null) { shadow.sortingLayerID = playerSprite.sortingLayerID; shadow.sortingOrder = playerSprite.sortingOrder - 5; }
        }

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        switch (data.passive)
        {
            case PetPassive.Magnet: Magnet(); break;
            case PetPassive.ShieldRegen: ShieldRegen(dt); break;
        }
    }

    void Magnet()
    {
        float r2 = data.magnetRadius * data.magnetRadius;
        var list = Coin.Active;
        for (int i = 0; i < list.Count; i++)
        {
            var c = list[i];
            if (c != null && (c.transform.position - player.position).sqrMagnitude < r2)
                c.PullTowards(player.position, data.magnetSpeed);
        }
    }

    void ShieldRegen(float dt)
    {
        if (health == null) return;
        if (health.HasShield) { shieldTimer = 0f; return; }
        shieldTimer += dt;
        if (shieldTimer >= data.shieldInterval)
        {
            shieldTimer = 0f;
            health.ActivateShield();
            // Not: ShieldBlocked tetiklenmez (o "vuruş engellendi" demek: görev sayacı ve HUD'u bozar)
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.shieldSfx, 1.2f, 0.7f);
        }
    }
}
