using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Aksiyon (dash) butonu: kontrol alanının sağ tarafı. Bekleme süresi dairesel dolumla gösterilir,
/// hazır olunca hafif parlar. Klavyede Space (PlayerDash).
/// </summary>
public class DashButton : MonoBehaviour, IPointerDownHandler
{
    public PlayerDash dash;
    public Image cooldownFill;      // Filled / Radial360
    public Image icon;
    public RectTransform visual;
    [Tooltip("İpucu gösterilirken buton belirgin şekilde nabız atar (OnboardingHints)")]
    public bool highlight;

    float pulse;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (dash == null) dash = FindAnyObjectByType<PlayerDash>();
        if (dash != null && dash.TryDash())
        {
            pulse = 1f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        }
    }

    void Update()
    {
        if (dash == null) return;
        float cd = dash.Cooldown01;
        if (cooldownFill != null) cooldownFill.fillAmount = cd;
        if (icon != null)
        {
            Color c = dash.Ready ? (highlight ? KakPalette.CamgobegiParlak : KakPalette.Krem) : KakPalette.ArduvazAcik;
            c.a = dash.Ready ? 1f : 0.6f;
            icon.color = c;
        }
        pulse = Mathf.MoveTowards(pulse, 0f, Time.unscaledDeltaTime * 4f);
        if (visual != null)
        {
            float amp = highlight ? 0.1f : 0.03f;
            float idle = dash.Ready ? 1f + Mathf.Sin(Time.unscaledTime * (highlight ? 7f : 4f)) * amp : 1f;
            visual.localScale = Vector3.one * (idle - pulse * 0.12f);
        }
    }
}
