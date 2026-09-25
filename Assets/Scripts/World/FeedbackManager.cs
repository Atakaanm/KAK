using UnityEngine;

/// <summary>
/// Oyun hissi (juice) merkezi: GameEvents'i dinler; ekran sarsıntısı, hit-stop ve parçacıkları tetikler.
/// Parçacıklar tek bir ParticleSystem'den Emit ile çıkar (tahsissiz, tek draw call).
/// Ölçüler sanat-rehberi.md §7'den.
/// </summary>
public class FeedbackManager : MonoBehaviour
{
    [Header("Referanslar (KakFxSetup kurar)")]
    public ParticleSystem chips;      // kare "piksel" parçacıklar

    [Header("Vuruş")]
    public float hitShake = 0.22f;
    public float hitShakeTime = 0.2f;
    public float hitStop = 0.06f;

    [Header("Ölüm")]
    public float deathShake = 0.45f;
    public float deathShakeTime = 0.45f;

    [Header("Duvar çarpması")]
    public int wallChips = 5;

    ParticleSystem.EmitParams ep;

    void OnEnable()
    {
        GameEvents.PlayerDamaged += OnPlayerDamaged;
        GameEvents.ShieldBlocked += OnShieldBlocked;
        GameEvents.PlayerDied += OnPlayerDied;
        GameEvents.PowerupCollected += OnPowerup;
        GameEvents.ProjectileHitWall += OnWallHit;
        GameEvents.StageChanged += OnStageChanged;
        GameEvents.MeteorLanded += OnMeteorLanded;
    }

    void OnDisable()
    {
        GameEvents.PlayerDamaged -= OnPlayerDamaged;
        GameEvents.ShieldBlocked -= OnShieldBlocked;
        GameEvents.PlayerDied -= OnPlayerDied;
        GameEvents.PowerupCollected -= OnPowerup;
        GameEvents.ProjectileHitWall -= OnWallHit;
        GameEvents.StageChanged -= OnStageChanged;
        GameEvents.MeteorLanded -= OnMeteorLanded;
    }

    void OnMeteorLanded(Vector3 pos)
    {
        Shake(0.1f, 0.18f);
        Burst(pos, 10, 2.2f, 0.4f, KakPalette.Bakir, KakPalette.KahveKoyu, 0.08f);
    }

    void OnPlayerDamaged(int hp, Vector3 pos)
    {
        Shake(hitShake, hitShakeTime);
        KakTime.HitStop(hitStop);
        Burst(pos, 14, 2.6f, 0.45f, KakPalette.Tehlike, KakPalette.Bakir, 0.09f);
    }

    void OnShieldBlocked(Vector3 pos)
    {
        Shake(hitShake * 0.6f, hitShakeTime * 0.8f);
        KakTime.HitStop(hitStop * 0.7f);
        Burst(pos, 16, 3.2f, 0.4f, KakPalette.CamgobegiParlak, KakPalette.Beyaz, 0.08f);
    }

    void OnPlayerDied(Vector3 pos)
    {
        Shake(deathShake, deathShakeTime);
        Burst(pos, 36, 3.8f, 0.8f, KakPalette.Tehlike, KakPalette.TehlikeKoyu, 0.11f);
    }

    void OnPowerup(PowerupData data, Vector3 pos)
    {
        Color a = KakPalette.Camgobegi, b = KakPalette.CamgobegiParlak;
        if (data != null)
        {
            switch (data.type)
            {
                case PowerupType.Heal: a = KakPalette.Tehlike; b = KakPalette.Pembe; break;
                case PowerupType.SpeedBoost: a = KakPalette.Altin; b = KakPalette.AltinAcik; break;
                case PowerupType.TimeSlow: a = KakPalette.CamgobegiKoyu; b = KakPalette.CamgobegiParlak; break;
                case PowerupType.Ghost: a = KakPalette.Sis; b = KakPalette.Beyaz; break;
            }
        }
        Ring(pos, 18, 2.4f, 0.5f, a, b, 0.08f);
    }

    void OnWallHit(Vector3 pos, Vector2 vel)
    {
        // Taş duvara çarpınca kırılır: geri seken kırıntılar
        Vector2 back = vel.sqrMagnitude > 0.001f ? -vel.normalized : Vector2.up;
        for (int i = 0; i < wallChips; i++)
        {
            Vector2 dir = (back + Random.insideUnitCircle * 0.9f).normalized;
            EmitOne(pos, dir * Random.Range(1f, 2.4f), Random.Range(0.25f, 0.45f),
                    Random.value < 0.5f ? KakPalette.Bakir : KakPalette.Kahve, Random.Range(0.05f, 0.09f));
        }
    }

    void OnStageChanged(string name, int index)
    {
        if (index > 0) Shake(0.12f, 0.35f);
    }

    // ── Yardımcılar ──────────────────────────────────────
    static void Shake(float amp, float dur)
    {
        if (KakCameraShake.Instance != null) KakCameraShake.Instance.Shake(amp, dur);
    }

    void Burst(Vector3 pos, int count, float speed, float life, Color a, Color b, float size)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            EmitOne(pos, dir * speed * Random.Range(0.35f, 1f), life * Random.Range(0.6f, 1f),
                    Color.Lerp(a, b, Random.value), size * Random.Range(0.7f, 1.2f));
        }
    }

    void Ring(Vector3 pos, int count, float speed, float life, Color a, Color b, float size)
    {
        for (int i = 0; i < count; i++)
        {
            float ang = i * Mathf.PI * 2f / count;
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            EmitOne(pos + (Vector3)(dir * 0.15f), dir * speed, life, i % 2 == 0 ? a : b, size);
        }
    }

    void EmitOne(Vector3 pos, Vector2 vel, float life, Color color, float size)
    {
        if (chips == null) return;
        ep.position = new Vector3(pos.x, pos.y, 0f);
        ep.velocity = vel;
        ep.startLifetime = life;
        ep.startColor = color;
        ep.startSize = size;
        ep.rotation = 0f;
        chips.Emit(ep, 1);
    }
}
