using TMPro;
using UnityEngine;

/// <summary>
/// HUD ek göstergeleri: combo çarpanı (skorun altında) ve olay/kademe afişi (arenanın üst kısmında).
/// GameEvents dinler; zaman ölçeğinden bağımsız animasyon.
/// </summary>
public class HudExtras : MonoBehaviour
{
    public TMP_Text comboText;
    public TMP_Text bannerText;
    public float bannerTime = 1.6f;

    float bannerAge = 99f;
    float comboPulse;

    void OnEnable()
    {
        GameEvents.ComboChanged += OnCombo;
        GameEvents.EndlessEventStarted += OnEvent;
        GameEvents.StageChanged += OnStage;
        GameEvents.NearMiss += OnNearMiss;
        GameEvents.PowerupCollected += OnPowerup;
        if (comboText != null) comboText.gameObject.SetActive(false);
        if (bannerText != null) bannerText.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        GameEvents.ComboChanged -= OnCombo;
        GameEvents.EndlessEventStarted -= OnEvent;
        GameEvents.StageChanged -= OnStage;
        GameEvents.NearMiss -= OnNearMiss;
        GameEvents.PowerupCollected -= OnPowerup;
    }

    void OnCombo(float mult)
    {
        if (comboText == null) return;
        bool show = mult > 1.001f;
        comboText.gameObject.SetActive(show);
        if (show)
        {
            comboText.SetText("x{0:1}", mult);
            comboText.color = mult >= 1.5f ? KakPalette.Altin : KakPalette.Krem;
            comboPulse = 1f;
        }
    }

    void OnEvent(string title) => ShowBanner(title, KakPalette.Tehlike);

    void OnStage(string name, int index)
    {
        if (index <= 0) return;
        ShowBanner(name.ToUpperInvariant() + "!", KakPalette.Altin);
    }

    void OnNearMiss(Vector3 pos, bool dashing)
    {
        WorldPopup.Show(dashing ? "SÜPER KAÇIŞ!" : "YAKIN!", pos, dashing ? KakPalette.CamgobegiParlak : KakPalette.Krem, dashing ? 1.1f : 0.85f);
    }

    void OnPowerup(PowerupData data, Vector3 pos)
    {
        if (data == null) return;
        string name = data.type switch
        {
            PowerupType.Heal => "+1 CAN",
            PowerupType.Shield => "KALKAN",
            PowerupType.SpeedBoost => "HIZ!",
            PowerupType.TimeSlow => "YAVAŞ ÇEKİM",
            PowerupType.Ghost => "HAYALET",
            _ => data.powerupName
        };
        WorldPopup.Show(name, pos, KakPalette.CamgobegiParlak, 1f);
    }

    void ShowBanner(string text, Color color)
    {
        if (bannerText == null) return;
        bannerText.gameObject.SetActive(true);
        bannerText.SetText(text);
        bannerText.color = color;
        bannerAge = 0f;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        if (bannerText != null && bannerAge < bannerTime)
        {
            bannerAge += dt;
            float k = bannerAge / bannerTime;
            float s = k < 0.12f ? Mathf.Lerp(1.6f, 1f, k / 0.12f) : 1f;
            bannerText.transform.localScale = Vector3.one * s;
            Color c = bannerText.color; c.a = k < 0.75f ? 1f : 1f - (k - 0.75f) / 0.25f; bannerText.color = c;
            if (k >= 1f) bannerText.gameObject.SetActive(false);
        }
        if (comboText != null && comboPulse > 0f)
        {
            comboPulse = Mathf.MoveTowards(comboPulse, 0f, dt * 3f);
            comboText.transform.localScale = Vector3.one * (1f + comboPulse * 0.25f);
        }
    }
}
