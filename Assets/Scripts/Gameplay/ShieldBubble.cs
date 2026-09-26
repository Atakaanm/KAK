using UnityEngine;

/// <summary>
/// Kalkan balonu: oyuncunun etrafında piksel sanat halka (kod ile üretilir, 1 piksel = 0.024 birim).
/// Karakterin rengini boyamaz, çarpışma alanını büyütmez. Kırılınca kısa bir "patlama" (büyüyüp söner).
/// PlayerHealth tarafından yönetilir: Show() / Pop().
/// </summary>
public class ShieldBubble : MonoBehaviour
{
    const int Size = 44;                 // piksel çap: gövdeyi (40 px) saracak kadar
    const float Ppu = 1f / 0.024f;       // sanat standardı
    const float PopTime = 0.18f;

    static Sprite cachedSprite;

    SpriteRenderer sr;
    SpriteRenderer follow;               // sıralama bu görselin önünde tutulur
    float popT = -1f;
    float t;

    /// <summary>Oyuncu kökünün altına balonu kurar (yoksa).</summary>
    public static ShieldBubble Create(Transform player, SpriteRenderer visual, float centerOffsetY)
    {
        var go = new GameObject("ShieldBubble");
        go.transform.SetParent(player, false);
        Vector3 s = player.lossyScale;
        go.transform.localPosition = new Vector3(0f, centerOffsetY / Mathf.Max(0.0001f, Mathf.Abs(s.y)), 0f);
        go.transform.localScale = new Vector3(1f / Mathf.Max(0.0001f, Mathf.Abs(s.x)), 1f / Mathf.Max(0.0001f, Mathf.Abs(s.y)), 1f);
        var b = go.AddComponent<ShieldBubble>();
        b.sr = go.AddComponent<SpriteRenderer>();
        b.sr.sprite = GetSprite();
        b.follow = visual;
        go.SetActive(false);
        return b;
    }

    public bool Visible => gameObject.activeSelf && popT < 0f;

    public void Show()
    {
        popT = -1f;
        t = 0f;
        transform.localScale = BaseScale;
        gameObject.SetActive(true);
        LateUpdate();
    }

    /// <summary>Kalkan kırıldı: kısa patlama, sonra gizlen.</summary>
    public void Pop()
    {
        if (!gameObject.activeSelf) return;
        popT = 0f;
    }

    public void HideImmediate() { popT = -1f; gameObject.SetActive(false); }

    Vector3 baseScale;
    Vector3 BaseScale
    {
        get
        {
            if (baseScale == Vector3.zero) baseScale = transform.localScale;
            return baseScale;
        }
    }

    void LateUpdate()
    {
        if (sr == null) return;
        if (follow != null)
        {
            sr.sortingLayerID = follow.sortingLayerID;
            sr.sortingOrder = follow.sortingOrder + 1;
        }

        if (popT >= 0f)
        {
            popT += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(popT / PopTime);
            transform.localScale = BaseScale * (1f + 0.35f * k);
            sr.color = new Color(1f, 1f, 1f, 1f - k);
            if (k >= 1f) HideImmediate();
            return;
        }

        // Hafif nefes alma: saydamlık (ölçek değil, pikseller titremesin)
        t += Time.deltaTime;
        sr.color = new Color(1f, 1f, 1f, 0.82f + 0.18f * Mathf.Sin(t * 5f));
    }

    static Sprite GetSprite()
    {
        if (cachedSprite != null) return cachedSprite;
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = "ShieldBubbleTex"
        };
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color rim = KakPalette.CamgobegiParlak;
        Color rimDark = KakPalette.Camgobegi;
        Color fill = KakPalette.WithAlpha(KakPalette.Camgobegi, 0.16f);
        Color shine = KakPalette.WithAlpha(KakPalette.Beyaz, 0.85f);
        float c = Size * 0.5f, r = c - 0.5f;
        var px = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float dx = x + 0.5f - c, dy = y + 0.5f - c;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                Color col = clear;
                if (d <= r)
                {
                    if (d > r - 1.2f) col = rim;                 // dış çizgi
                    else if (d > r - 2.2f) col = rimDark;        // iç gölge çizgisi
                    else col = fill;
                    // Sol üstte parlama yayı
                    float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (d > r - 4.2f && d <= r - 2.2f && ang > 115f && ang < 160f) col = shine;
                }
                px[y * Size + x] = col;
            }
        tex.SetPixels(px);
        tex.Apply(false, true);
        cachedSprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Ppu, 0, SpriteMeshType.FullRect);
        cachedSprite.name = "ShieldBubble";
        return cachedSprite;
    }
}
