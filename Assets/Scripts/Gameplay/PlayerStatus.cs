using UnityEngine;

/// <summary>
/// Oyuncu durum efektleri: yavaşlama (kartopu, sarı kart), donma (buz sarkıtı), sarı kart sayacı (2 sarı = kırmızı).
/// Başın üstünde küçük ikon gösterir. PlayerMovement2D hız çarpanını buradan okur.
/// </summary>
public class PlayerStatus : MonoBehaviour
{
    public SpriteRenderer icon;           // başın üstündeki durum ikonu
    public Sprite slowIcon, freezeIcon, yellowIcon, redIcon;

    float slowUntil = -1f, slowMult = 1f, freezeUntil = -1f, iconUntil = -1f;
    PlayerHealth health;

    public int YellowCards { get; private set; }
    public bool Frozen => Time.time < freezeUntil;
    public bool Slowed => Time.time < slowUntil;

    public float SpeedMultiplier
    {
        get
        {
            if (Frozen) return 0f;
            return Slowed ? slowMult : 1f;
        }
    }

    void Awake() { health = GetComponent<PlayerHealth>(); }

    /// <summary>Mermi etkisini uygular. Kalkan/hayalet/ölümsüzlük etkileri engeller. Etki uygulandıysa true.</summary>
    public bool Apply(ProjectileEffect effect, float duration, float strength, int damage)
    {
        if (health != null && (health.IsGhost || health.IsInvulnerable || health.IsDead)) return false;
        if (health != null && health.ConsumeShield()) return true;

        switch (effect)
        {
            case ProjectileEffect.Damage:
                if (health != null) health.TakeDamage(damage);
                break;
            case ProjectileEffect.Slow:
                Slow(strength, duration, slowIcon);
                break;
            case ProjectileEffect.Freeze:
                freezeUntil = Time.time + duration;
                ShowIcon(freezeIcon, duration);
                WorldPopup.Show(Loc.T("st_frozen"), transform.position, KakPalette.CamgobegiParlak, 0.9f);
                break;
            case ProjectileEffect.YellowCard:
                YellowCards++;
                Slow(strength, duration, yellowIcon);
                if (YellowCards >= 2)
                {
                    YellowCards = 0;
                    RedCard(damage);
                }
                else WorldPopup.Show(Loc.T("st_yellow"), transform.position, KakPalette.AltinAcik, 1f);
                break;
            case ProjectileEffect.RedCard:
                RedCard(damage);
                break;
        }
        return true;
    }

    void Slow(float mult, float duration, Sprite ic)
    {
        slowMult = Mathf.Clamp(mult, 0.1f, 1f);
        slowUntil = Time.time + duration;
        ShowIcon(ic, duration);
    }

    void RedCard(int damage)
    {
        ShowIcon(redIcon, 1.2f);
        WorldPopup.Show(Loc.T("st_red"), transform.position, KakPalette.Tehlike, 1.1f);
        GameEvents.RaiseRedCard(transform.position);
        if (health != null) health.TakeDamage(Mathf.Max(1, damage));
    }

    void ShowIcon(Sprite s, float duration)
    {
        if (icon == null || s == null) return;
        icon.sprite = s;
        icon.enabled = true;
        iconUntil = Time.time + duration;
    }

    void Update()
    {
        if (icon != null && icon.enabled && Time.time >= iconUntil) icon.enabled = false;
        if (icon != null && icon.enabled)
            icon.transform.localPosition = new Vector3(0f, 0.75f + Mathf.Sin(Time.time * 6f) * 0.04f, 0f);
    }
}
