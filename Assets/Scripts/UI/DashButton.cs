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
    /// <summary>G7: eldivenle yakalanmış kartopu varken düğme "fırlat" olur.</summary>
    [System.NonSerialized] public SnowballCatcher catcher;
    Sprite dashIcon;
    public Image cooldownFill;      // Filled / Radial360
    public Image icon;
    public RectTransform visual;
    [Tooltip("İpucu gösterilirken buton belirgin şekilde nabız atar (OnboardingHints)")]
    public bool highlight;

    float pulse;

    // Faz 12 H3: serbest düzen / aynalama (ControlSettings). İlk yerleşim saklanır.
    bool homeSaved;
    Transform homeParent;
    int homeSibling;
    Vector2 homeAnchor, homeSize;
    RectTransform rt;

    void OnEnable() { ControlSettings.Changed += ApplyLayout; ApplyLayout(); }
    void OnDisable() { ControlSettings.Changed -= ApplyLayout; }

    public void ApplyLayout()
    {
        if (rt == null) rt = transform as RectTransform;
        if (rt == null || GameSettings.TwoPlayer) return;
        if (!homeSaved)
        {
            homeSaved = true;
            homeParent = rt.parent; homeSibling = rt.GetSiblingIndex();
            homeAnchor = rt.anchorMin; homeSize = rt.sizeDelta;
        }
        rt.sizeDelta = homeSize * ControlSettings.ButtonScale; // dokunma alanı da büyür/küçülür
        var canvas = rt.GetComponentInParent<Canvas>();
        var root = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
        if (ControlSettings.Custom && root != null)
        {
            if (rt.parent != root)
            {
                rt.SetParent(root, false);
                int idx = homeParent != null && homeParent.parent != null && homeParent.parent.parent == root
                    ? homeParent.parent.GetSiblingIndex() + 1 : 0;
                rt.SetSiblingIndex(Mathf.Clamp(idx + 1, 0, root.childCount - 1)); // joystick bölgesinin üstünde
            }
            rt.anchorMin = rt.anchorMax = ControlSettings.ButtonPos;
        }
        else
        {
            if (homeParent != null && rt.parent != homeParent)
            {
                rt.SetParent(homeParent, false);
                rt.SetSiblingIndex(Mathf.Min(homeSibling, homeParent.childCount - 1));
            }
            Vector2 a = ControlSettings.Mirrored ? new Vector2(1f - homeAnchor.x, homeAnchor.y) : homeAnchor;
            rt.anchorMin = rt.anchorMax = a;
        }
        rt.anchoredPosition = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (catcher != null && catcher.Holding)
        {
            catcher.Throw();
            pulse = 1f;
            return;
        }
        if (dash == null) dash = FindAnyObjectByType<PlayerDash>();
        if (dash != null && dash.TryDash())
        {
            pulse = 1f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        }
    }

    void Update()
    {
        // G7: elde kartopu → ikon kartopu, düğme hazır parlar
        if (icon != null)
        {
            if (dashIcon == null) dashIcon = icon.sprite;
            bool throwing = catcher != null && catcher.Holding;
            var want = throwing ? catcher.HeldSprite : dashIcon;
            if (want != null && icon.sprite != want) { icon.sprite = want; icon.preserveAspect = true; }
            if (throwing)
            {
                icon.color = Color.white;
                pulse = Mathf.MoveTowards(pulse, 0f, Time.unscaledDeltaTime * 4f);
                if (visual != null) visual.localScale = Vector3.one * ((1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.08f - pulse * 0.12f) * ControlSettings.ButtonScale);
                return;
            }
        }
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
            visual.localScale = Vector3.one * ((idle - pulse * 0.12f) * ControlSettings.ButtonScale);
        }
    }
}
