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
    /// <summary>Faz 13 F1 (yumuşak): bu kadar saniye yeni sarı kart görmezsen sayaç sıfırlanır (2 sarı = kırmızı bir anda gelmesin).</summary>
    public float yellowForgetSeconds = 15f;
    /// <summary>Faz 13 F1: kırmızı kart (ve 2 sarı) oyundan atar (Futbol teması). Kapalıysa eskisi gibi 1 can.</summary>
    [System.NonSerialized] public bool redCardEliminates;
    float lastYellow = -99f;
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

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        // Hareket bileşeni hız çarpanını her fizik adımında aramasın: kendini bildir
        var move = GetComponent<PlayerMovement2D>();
        if (move != null) move.AttachStatus(this);
    }

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
                if (Time.time - lastYellow > yellowForgetSeconds) YellowCards = 0;
                lastYellow = Time.time;
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
        WorldPopup.Show(Loc.T(redCardEliminates ? "st_out" : "st_red"), transform.position, KakPalette.Tehlike, 1.1f);
        GameEvents.RaiseRedCard(transform.position);
        if (health == null) return;
        if (redCardEliminates) health.Eliminate();
        else health.TakeDamage(Mathf.Max(1, damage));
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
        // Faz 13 F1 (yumuşak): bir sarı kartın varken başının üstünde sarı kart durur ("bir daha = atılırsın"),
        // unutulmadan önceki son 3 sn yanıp söner; 15 sn yeni kart görmezsen sayaç sıfırlanır
        bool warnYellow = YellowCards > 0 && Time.time - lastYellow <= yellowForgetSeconds;
        if (YellowCards > 0 && !warnYellow) YellowCards = 0;
        if (icon != null && Time.time >= iconUntil)
        {
            if (warnYellow && yellowIcon != null)
            {
                icon.sprite = yellowIcon;
                float left = yellowForgetSeconds - (Time.time - lastYellow);
                icon.enabled = left > 3f || Mathf.Sin(Time.time * 12f) > 0f;
            }
            else if (icon.enabled) icon.enabled = false;
        }
        if (icon != null && icon.enabled)
            icon.transform.localPosition = new Vector3(0f, 0.75f + Mathf.Sin(Time.time * 6f) * 0.04f, 0f);
    }
}
