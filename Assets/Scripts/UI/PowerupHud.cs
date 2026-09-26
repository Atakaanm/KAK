using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Aktif güçlendirme göstergesi: arenanın hemen altında (kontrol alanının üstü) ortalanmış çipler.
/// Her çip: koyu taban + ikon + kalan süre halkası (radyal, boşalır). Kalkan vurulana kadar dolu halka ile durur.
/// Süre oyun saatiyle akar (KakTime.WaitGameplay ile aynı: duraklatmada durur). Çipler havuzlu, karede tahsis yok.
/// Kurulum: KakEndlessSetup (sprite'lar ve konum). GameEvents.PowerupActivated / ShieldBlocked / PlayerDied dinler.
/// </summary>
public class PowerupHud : MonoBehaviour
{
    public Sprite chipBase;
    public Sprite ringSprite;
    public float chipSize = 104f;
    public float spacing = 120f;
    [Tooltip("Son bu kadar saniyede halka yanıp söner")]
    public float warnTime = 1.5f;

    const int MaxChips = 4;

    class Chip
    {
        public RectTransform root;
        public Image ring, icon;
        public PowerupType type;
        public float total, remaining; // total 0 = süresiz (kalkan)
        public bool active;
        public float appear;
    }

    readonly Chip[] chips = new Chip[MaxChips];
    int activeCount;

    void Awake()
    {
        for (int i = 0; i < MaxChips; i++) chips[i] = CreateChip(i);
    }

    void OnEnable()
    {
        GameEvents.PowerupActivated += OnActivated;
        GameEvents.ShieldBlocked += OnShieldBlocked;
        GameEvents.PlayerDied += OnDied;
    }

    void OnDisable()
    {
        GameEvents.PowerupActivated -= OnActivated;
        GameEvents.ShieldBlocked -= OnShieldBlocked;
        GameEvents.PlayerDied -= OnDied;
    }

    Chip CreateChip(int i)
    {
        var go = new GameObject("Chip" + i, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(transform, false);
        rt.sizeDelta = new Vector2(chipSize, chipSize);
        var bg = go.AddComponent<Image>();
        bg.sprite = chipBase;
        bg.raycastTarget = false;

        var ring = NewImage("Ring", rt, ringSprite, chipSize * 0.94f);
        ring.type = Image.Type.Filled;
        ring.fillMethod = Image.FillMethod.Radial360;
        ring.fillOrigin = (int)Image.Origin360.Top;
        ring.fillClockwise = false;
        ring.color = KakPalette.CamgobegiParlak;

        var icon = NewImage("Icon", rt, null, chipSize * 0.62f);
        icon.preserveAspect = true;

        go.SetActive(false);
        return new Chip { root = rt, ring = ring, icon = icon };
    }

    static Image NewImage(string name, RectTransform parent, Sprite sprite, float size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(size, size);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        return img;
    }

    void OnActivated(PowerupData data, float seconds)
    {
        if (data == null) return;
        Chip c = Find(data.type);
        if (c == null)
        {
            c = FreeChip();
            if (c == null) return;
            c.active = true;
            c.type = data.type;
            c.appear = 0f;
            c.root.gameObject.SetActive(true);
            activeCount++;
        }
        c.icon.sprite = data.icon;
        c.total = seconds;
        c.remaining = seconds;
        c.ring.fillAmount = 1f;
        Layout();
    }

    void OnShieldBlocked(Vector3 pos)
    {
        var c = Find(PowerupType.Shield);
        if (c != null) Hide(c);
    }

    void OnDied(Vector3 pos)
    {
        for (int i = 0; i < MaxChips; i++) if (chips[i].active) Hide(chips[i]);
    }

    Chip Find(PowerupType t)
    {
        for (int i = 0; i < MaxChips; i++) if (chips[i].active && chips[i].type == t) return chips[i];
        return null;
    }

    Chip FreeChip()
    {
        for (int i = 0; i < MaxChips; i++) if (!chips[i].active) return chips[i];
        return null;
    }

    void Hide(Chip c)
    {
        c.active = false;
        c.root.gameObject.SetActive(false);
        activeCount--;
        Layout();
    }

    /// <summary>Aktif çipleri toplama sırasıyla yan yana ortalar.</summary>
    void Layout()
    {
        int n = 0;
        for (int i = 0; i < MaxChips; i++)
        {
            if (!chips[i].active) continue;
            chips[i].root.anchoredPosition = new Vector2((n - (activeCount - 1) * 0.5f) * spacing, 0f);
            n++;
        }
    }

    void Update()
    {
        if (activeCount == 0) return;
        float real = Time.unscaledDeltaTime;
        float dt = (!KakTime.Paused && Time.timeScale > 0f) ? real : 0f;
        for (int i = 0; i < MaxChips; i++)
        {
            var c = chips[i];
            if (!c.active) continue;

            // Beliriş: küçükten hafif taşarak
            if (c.appear < 1f)
            {
                c.appear = Mathf.Min(1f, c.appear + real / 0.25f);
                float k = c.appear;
                float s = k < 0.6f ? Mathf.Lerp(0.5f, 1.12f, k / 0.6f) : Mathf.Lerp(1.12f, 1f, (k - 0.6f) / 0.4f);
                c.root.localScale = new Vector3(s, s, 1f);
            }

            if (c.total <= 0f)
            {
                // Süresiz (kalkan): dolu halka, hafif nefes
                c.ring.fillAmount = 1f;
                c.ring.color = KakPalette.WithAlpha(KakPalette.CamgobegiParlak, 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 4f));
                continue;
            }

            c.remaining -= dt;
            if (c.remaining <= 0f) { Hide(c); continue; }
            c.ring.fillAmount = c.remaining / c.total;
            bool warn = c.remaining < warnTime;
            float a = warn ? (Mathf.Sin(Time.unscaledTime * 18f) > 0f ? 1f : 0.25f) : 1f;
            c.ring.color = KakPalette.WithAlpha(warn ? KakPalette.Turuncu : KakPalette.CamgobegiParlak, a);
        }
    }

    // Testler için
    public int ActiveCount => activeCount;
    public float Remaining(PowerupType t) { var c = Find(t); return c != null ? c.remaining : -1f; }
}
