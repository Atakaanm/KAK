using UnityEngine;

/// <summary>
/// Modal paneli ekrana sığdırır. Kanvas genişliğe göre ölçeklendiği için (match = 0) kısa/geniş ekranlarda
/// (tablet, 4:3, 3:2) uzun paneller (Karakter 1560) kanvas yüksekliğini aşıp kesiliyordu. Bu katman, panel
/// sığmıyorsa orantılı küçülür; uzun telefonlarda ölçek 1 kalır. Panelin kendi açılış animasyonu
/// (GameOverScreen/ContinuePanel localScale) alttaki "Panel"de olduğu için çakışmaz.
/// Kurulum: KakUiKit.Modal → kök/Fit/Panel.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UiFitToScreen : MonoBehaviour
{
    public RectTransform content;   // sığdırılacak panel
    public float margin = 40f;      // her kenarda boşluk (kanvas birimi)

    RectTransform rt;
    Vector2 lastSize;

    void OnEnable() => Fit();

    void Update()
    {
        if (rt != null && rt.rect.size != lastSize) Fit(); // ekran boyutu değişti
    }

    public void Fit()
    {
        if (rt == null) rt = (RectTransform)transform;
        lastSize = rt.rect.size;
        if (content == null) return;
        float s = Scale(rt.rect.size, content.rect.size, margin);
        rt.localScale = new Vector3(s, s, 1f);
    }

    /// <summary>Saf hesap (test edilebilir): panel alana sığmıyorsa küçültme oranı, sığıyorsa 1.</summary>
    public static float Scale(Vector2 area, Vector2 panel, float margin)
    {
        float sx = panel.x > 0f ? (area.x - margin * 2f) / panel.x : 1f;
        float sy = panel.y > 0f ? (area.y - margin * 2f) / panel.y : 1f;
        return Mathf.Clamp(Mathf.Min(sx, sy), 0.4f, 1f);
    }
}
