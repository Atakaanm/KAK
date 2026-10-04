using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Faz 15 K8: HUD'da ince cesaret çubuğu (kıl payı, altın ve süreyle dolar; dolunca kart seçimi). Tek kişilik sonsuzda görünür.</summary>
public class CourageBar : MonoBehaviour
{
    public GameObject root;
    public Image fill;
    public TMP_Text levelText;
    float shown, flash;
    int lastLevel = -1;

    void Update()
    {
        var p = RunPerks.Instance;
        bool on = p != null && !GameSettings.TwoPlayer && GameManager.Instance != null && !GameManager.Instance.IsLevelMode;
        if (root != null && root.activeSelf != on) root.SetActive(on);
        if (!on) return;
        float target = Mathf.Clamp01(p.Courage / Mathf.Max(0.01f, p.Need));
        shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * 2.5f);
        if (p.Level != lastLevel)
        {
            if (lastLevel >= 0) { flash = 1f; shown = 0f; }
            lastLevel = p.Level;
            if (levelText != null) levelText.SetText("{0}", p.Level + 1);
        }
        flash = Mathf.Max(0f, flash - Time.unscaledDeltaTime * 2f);
        if (fill != null)
        {
            fill.fillAmount = shown;
            fill.color = Color.Lerp(KakPalette.CamgobegiParlak, Color.white, flash + (target > 0.92f ? 0.3f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 10f)) : 0f));
        }
    }
}
