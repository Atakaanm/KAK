using UnityEngine;

/// <summary>
/// Oyuncu görselinin "canlılığı": yürürken hafif zıplama, dururken ezilme, hasarda büzülme,
/// koşarken ayak tozu. Sadece görsel alt objenin ölçeği/konumu değişir (fizik etkilenmez).
/// </summary>
public class PlayerJuice : MonoBehaviour
{
    public PlayerMovement2D movement;
    public Transform visual;              // ölçeği/konumu oynatılacak görsel
    public ParticleSystem dust;           // ortak kırıntı sistemi de olabilir

    [Header("Zıplama")]
    public float bobHeight = 0.05f;
    public float bobSpeed = 16f;
    [Header("Ezilme")]
    public float landSquash = 0.14f;
    public float hurtSquash = 0.22f;
    public float recover = 12f;
    [Header("Toz")]
    public float dustInterval = 0.12f;

    Vector3 baseScale, basePos;
    float phase, squash, dustTimer;
    bool wasMoving;
    ParticleSystem.EmitParams ep;

    void Awake()
    {
        if (visual != null)
        {
            baseScale = visual.localScale;
            basePos = visual.localPosition;
        }
    }

    void OnEnable() { GameEvents.PlayerDamaged += OnDamaged; }
    void OnDisable() { GameEvents.PlayerDamaged -= OnDamaged; }

    // İki kişilikte yalnız kendi hasarımız (olay oyuncu konumuyla gelir)
    void OnDamaged(int hp, Vector3 pos) { if (((Vector2)pos - (Vector2)transform.position).sqrMagnitude < 0.04f) squash = -hurtSquash; }

    float appear = 1f;
    /// <summary>Faz 12 H1: yumuşak beliriş (küçükten hafif taşarak büyür; iki kişilikte dönüş, canlanma).</summary>
    public void Appear() { appear = 0f; }

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        float u = t - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }

    void Update()
    {
        if (visual == null || movement == null) return;
        bool moving = movement.MovementInput.sqrMagnitude > 0.01f;
        float dt = Time.deltaTime;

        if (moving) phase += dt * bobSpeed;
        else if (wasMoving) { squash = landSquash; phase = 0f; }
        wasMoving = moving;

        squash = Mathf.Lerp(squash, 0f, 1f - Mathf.Exp(-recover * dt));
        float bob = moving ? Mathf.Abs(Mathf.Sin(phase)) * bobHeight : 0f;
        if (appear < 1f) appear = Mathf.Min(1f, appear + dt / 0.3f);
        float a = appear < 1f ? EaseOutBack(appear) : 1f;
        // squash > 0: yere basma (yassı-geniş), < 0: büzülme (dar-uzun)
        visual.localScale = new Vector3(baseScale.x * (1f + squash) * a, baseScale.y * (1f - squash) * a, baseScale.z);
        visual.localPosition = basePos + new Vector3(0f, bob, 0f);

        if (moving && dust != null)
        {
            dustTimer -= dt;
            if (dustTimer <= 0f)
            {
                dustTimer = dustInterval;
                ep.position = transform.position + new Vector3(Random.Range(-0.08f, 0.08f), -0.38f, 0f);
                ep.velocity = new Vector2(-movement.MovementInput.x * 0.3f, 0.25f);
                ep.startLifetime = 0.35f;
                ep.startColor = KakPalette.WithAlpha(KakPalette.Sis, 0.55f);
                ep.startSize = 0.06f;
                dust.Emit(ep, 1);
            }
        }
    }
}
