using UnityEngine;

/// <summary>
/// Faz 13 K1: karanlık dünya (tema `darkness` > 0). Kamera görüşünü kaplayan karanlık katmanı SpriteMask dışında görünür;
/// her oyuncunun meşalesi maskede delik açar (yumuşak kenar halkası + sıcak hâle, hafif titrer). Yumuşak oynanış: karanlıkta
/// görünmez vuruş olmasın diye taşların parıltısı, fırlatıcıların gözleri (atıştan önce turuncu yanar), "!" uyarıları, altın ve
/// eşyalar karanlığın üstünde çizilir (GlowOrder). LevelManager her bölümde önce Reset, tema varsa Apply çağırır.
/// </summary>
public static class DarkWorld
{
    public const int DarkOrder = 250;   // oyun nesnelerinin (≤ ~150) üstü, dünya yazılarının (300) altı
    public const int GlowOrder = 262;   // karanlıkta görünen ipuçları

    public static bool Active { get; private set; }
    public static float Darkness { get; private set; }

    static Material spriteMat;
    /// <summary>
    /// Oyunun sprite materyali (oyuncunun görselinden). AddComponent ile kurulan SpriteRenderer URP'de "Sprite-Lit-Default"
    /// alıyor; Universal Renderer'da ışıksız sahnede saydamlığı soluk çiziliyordu → karanlık katmanı görünmüyordu.
    /// </summary>
    public static Material SpriteMaterial
    {
        get
        {
            if (spriteMat != null) return spriteMat;
            var ph = PlayerRegistry.All.Count > 0 ? PlayerRegistry.All[0] : Object.FindAnyObjectByType<PlayerHealth>();
            if (ph != null && ph.playerSpriteRenderer != null) spriteMat = ph.playerSpriteRenderer.sharedMaterial;
            return spriteMat;
        }
    }

    /// <summary>Yeni sprite çiziciyi oyunun materyaliyle kurar.</summary>
    public static void UseGameMaterial(SpriteRenderer r)
    {
        var m = SpriteMaterial;
        if (r != null && m != null) r.sharedMaterial = m;
    }

    public static void Reset()
    {
        Active = false;
        Darkness = 0f;
        Projectile.DarkMode = false;
    }

    public static void Apply(WorldTheme t)
    {
        Reset();
        if (t == null || t.darkness <= 0.01f) return;
        Active = true;
        Darkness = Mathf.Clamp01(t.darkness);
        Projectile.DarkMode = true;
        DarknessOverlay.Create(Darkness);
        for (int i = 0; i < PlayerRegistry.All.Count; i++) PlayerLight.Attach(PlayerRegistry.All[i]);
        var sp = Object.FindAnyObjectByType<PowerupSpawner>();
        if (sp != null) sp.powerupSortingOrder = GlowOrder;
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsInactive.Include, FindObjectsSortMode.None)) ShooterEyes.Attach(s);
    }
}

/// <summary>Faz 13 K1: kamerayı kaplayan karanlık (maske dışında görünür). Kamerayı izler.</summary>
public class DarknessOverlay : MonoBehaviour
{
    SpriteRenderer sr;
    Camera cam;

    public static DarknessOverlay Create(float darkness)
    {
        var existing = Object.FindAnyObjectByType<DarknessOverlay>();
        if (existing != null) { existing.sr.color = new Color(0.02f, 0.02f, 0.04f, darkness); return existing; }
        var go = new GameObject("Darkness");
        var o = go.AddComponent<DarknessOverlay>();
        o.sr = go.AddComponent<SpriteRenderer>();
        o.sr.sprite = Resources.Load<Sprite>("UiWhite");
        DarkWorld.UseGameMaterial(o.sr);
        o.sr.color = new Color(0.02f, 0.02f, 0.04f, darkness);
        o.sr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        o.sr.sortingOrder = DarkWorld.DarkOrder;
        o.LateUpdate();
        return o;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            var composer = Object.FindAnyObjectByType<ScreenComposer>();
            cam = composer != null ? composer.GetComponent<Camera>() : null;
            if (cam == null) cam = Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
        }
        if (cam == null || sr == null || sr.sprite == null) return;
        float h = cam.orthographicSize * 2f + 2f, w = h * cam.aspect + 2f;
        Vector2 s = sr.sprite.bounds.size;
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
        transform.localScale = new Vector3(w / Mathf.Max(0.0001f, s.x), h / Mathf.Max(0.0001f, s.y), 1f);
    }
}

/// <summary>
/// Faz 13 K1: oyuncunun meşalesi — karanlıkta delik (SpriteMask), yumuşak kenar, sıcak hâle, elde piksel meşale.
/// Yarıçap TorchProgress'ten (seviye), hafif titrer. İki kişilikte her oyuncuda. Düşen oyuncuda (SetDownVisual) kapanır.
/// </summary>
public class PlayerLight : MonoBehaviour
{
    SpriteMask mask;
    SpriteRenderer edge, glow, torch;
    HeldItem held; // Faz 15 K7: meşale her karede sağ ele oturur (HandAnchorSet)
    PlayerMovement2D mv;
    float seed;
    /// <summary>Şu anki ışık yarıçapı (dünya birimi, titreme dahil).</summary>
    public float Radius { get; private set; }

    public static PlayerLight Attach(PlayerHealth ph)
    {
        if (ph == null) return null;
        var l = ph.GetComponent<PlayerLight>();
        if (l == null) l = ph.gameObject.AddComponent<PlayerLight>();
        return l;
    }

    void Awake()
    {
        mv = GetComponent<PlayerMovement2D>();
        seed = Random.value * 10f;
        mask = Child("LightMask").gameObject.AddComponent<SpriteMask>();
        mask.sprite = Resources.Load<Sprite>("LightCircle");
        mask.alphaCutoff = 0.5f;
        edge = Renderer("LightEdge", "LightEdge", DarkWorld.DarkOrder + 1, new Color(0.02f, 0.02f, 0.04f, DarkWorld.Darkness));
        glow = Renderer("LightGlow", "LightGlow", DarkWorld.DarkOrder + 2, new Color(1f, 0.62f, 0.25f, 0.14f));
        torch = Renderer("Torch", "Torch", DarkWorld.GlowOrder + 1, Color.white);
        held = torch.GetComponent<HeldItem>();
        if (held == null) held = torch.gameObject.AddComponent<HeldItem>();
        held.sprite = torch.sprite;
        held.gripOffsetPx = new Vector2(0f, -4f); // sap: merkezin 4 piksel altı elde
    }

    Transform Child(string n)
    {
        var t = transform.Find(n);
        if (t == null) { t = new GameObject(n).transform; t.SetParent(transform, false); }
        return t;
    }

    SpriteRenderer Renderer(string n, string sprite, int order, Color c)
    {
        var t = Child(n);
        var r = t.GetComponent<SpriteRenderer>();
        if (r == null) r = t.gameObject.AddComponent<SpriteRenderer>();
        r.sprite = Resources.Load<Sprite>(sprite);
        r.sortingOrder = order;
        r.color = c;
        DarkWorld.UseGameMaterial(r);
        return r;
    }

    static void SetWorldSize(Transform t, Sprite s, float worldDiameter, float parentScale)
    {
        if (s == null) return;
        float k = worldDiameter / Mathf.Max(0.0001f, s.bounds.size.x) / Mathf.Max(0.0001f, parentScale);
        t.localScale = new Vector3(k, k, 1f);
    }

    void LateUpdate()
    {
        float ps = Mathf.Abs(transform.lossyScale.x);
        float tt = Time.time + seed;
        float flicker = 1f + 0.025f * Mathf.Sin(tt * 7.3f) + 0.015f * Mathf.Sin(tt * 13.1f);
        Radius = TorchProgress.CurrentRadius * flicker;
        // Işık ayaklardan biraz yukarıda (gövde ortası)
        Vector3 c = new Vector3(0f, 0.1f / Mathf.Max(0.0001f, ps), 0f);
        if (mask != null) { mask.transform.localPosition = c; SetWorldSize(mask.transform, mask.sprite, Radius * 2f, ps); }
        if (edge != null) { edge.transform.localPosition = c; SetWorldSize(edge.transform, edge.sprite, Radius * 2f, ps); }
        if (glow != null)
        {
            glow.transform.localPosition = c;
            SetWorldSize(glow.transform, glow.sprite, Radius * 2.4f, ps);
            var g = glow.color; g.a = 0.12f + 0.03f * Mathf.Sin(tt * 9f); glow.color = g;
        }
        // Meşalenin konumu, ölçeği ve önde/arkada sıralaması HeldItem'da (el noktası)
    }
}

/// <summary>Faz 13 K1: karanlıkta fırlatıcının parlayan gözleri; atıştan önce turuncu yanar (uyarı).</summary>
public class ShooterEyes : MonoBehaviour
{
    CornerShooter shooter;
    SpriteRenderer eyes, glint;

    public static ShooterEyes Attach(CornerShooter s)
    {
        if (s == null) return null;
        var e = s.GetComponent<ShooterEyes>();
        if (e == null) e = s.gameObject.AddComponent<ShooterEyes>();
        return e;
    }

    void Awake()
    {
        shooter = GetComponent<CornerShooter>();
        eyes = Make("Eyes", "GlowEyes", DarkWorld.GlowOrder);
        glint = Make("EyeGlow", "Glint", DarkWorld.GlowOrder - 1);
    }

    SpriteRenderer Make(string n, string sprite, int order)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = Resources.Load<Sprite>(sprite);
        r.sortingOrder = order;
        DarkWorld.UseGameMaterial(r);
        float ls = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
        go.transform.localScale = Vector3.one * (1f / ls);
        return r;
    }

    void LateUpdate()
    {
        if (shooter == null || eyes == null) return;
        bool on = shooter.isActiveAndEnabled && DarkWorld.Active;
        eyes.enabled = on; glint.enabled = on;
        if (!on) return;
        var vis = shooter.spawnerVisual != null ? shooter.spawnerVisual.GetComponent<SpriteRenderer>() : null;
        Vector3 top = vis != null ? new Vector3(vis.bounds.center.x, vis.bounds.max.y - vis.bounds.size.y * 0.22f, 0f) : transform.position + Vector3.up * 0.5f;
        eyes.transform.position = top;
        glint.transform.position = top;
        float k = shooter.Telegraph01; // atış yaklaşınca 0→1
        Color calm = KakPalette.WithAlpha(KakPalette.Tehlike, 0.6f), hot = KakPalette.AltinAcik;
        eyes.color = Color.Lerp(calm, hot, k);
        var g = KakPalette.WithAlpha(KakPalette.Turuncu, 0.15f + 0.55f * k);
        glint.color = g;
        float ls = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
        glint.transform.localScale = Vector3.one * ((0.25f + 0.35f * k) / 0.32f / ls);
    }
}
