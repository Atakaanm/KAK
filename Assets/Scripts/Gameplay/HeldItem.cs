using UnityEngine;

/// <summary>
/// Faz 15 K7: oyuncunun elindeki eşya (meşale; ileride kılıç, kalkan). Her karede karakterin o anki sprite'ının sağ el noktasına
/// (HandAnchorSet) piksel ızgarasında oturur; el gövdenin arkasındaysa gövdenin arkasında, öndeyse önünde çizilir; batıya dönünce
/// aynalanır. Nokta yoksa gövdeye göre yedek konum. Tahsis yapmaz (LateUpdate: animasyon karesinden sonra).
/// </summary>
public class HeldItem : MonoBehaviour
{
    [Tooltip("Eşyanın sprite'ı; tutma noktası sprite pivotudur (sapın alt ucu)")]
    public Sprite sprite;
    [Tooltip("Pivottan ele düzeltme (eşya pikseli)")]
    public Vector2 gripOffsetPx;
    [Tooltip("Gövdenin önünde/arkasında sıralama farkı")]
    public int sortingDelta = 1;
    public bool flipWithFacing = true;

    public SpriteRenderer Renderer { get; private set; }
    public bool LastFront { get; private set; }
    public bool LastFound { get; private set; }

    SpriteRenderer body;
    HandAnchorSet anchors;
    PlayerMovement2D mv;

    public static HeldItem Attach(GameObject player, Sprite itemSprite, string name = "Held")
    {
        var go = new GameObject(name);
        go.transform.SetParent(player.transform, false);
        var h = go.AddComponent<HeldItem>();
        h.sprite = itemSprite;
        return h;
    }

    void Awake()
    {
        Renderer = GetComponent<SpriteRenderer>();
        if (Renderer == null) Renderer = gameObject.AddComponent<SpriteRenderer>();
    }

    void Start() => Bind();

    /// <summary>Karakter değişince (LevelManager.ApplyPlayerDataTo) yeniden bağlanır.</summary>
    public void Bind()
    {
        var root = transform.parent;
        if (root == null) return;
        var vis = root.GetComponentInChildren<PlayerDirectionSprite>();
        body = vis != null ? vis.spriteRenderer : root.GetComponentInChildren<SpriteRenderer>();
        mv = root.GetComponent<PlayerMovement2D>();
        anchors = mv != null && mv.playerData != null ? mv.playerData.hands : null;
        if (Renderer != null)
        {
            Renderer.sprite = sprite;
            if (body != null) Renderer.sharedMaterial = body.sharedMaterial;
        }
    }

    void LateUpdate()
    {
        if (body == null) Bind();
        if (Renderer == null || body == null) return;
        if (anchors == null && mv != null && mv.playerData != null && mv.playerData.hands != null) anchors = mv.playerData.hands; // karakter sonradan uygulandıysa
        Renderer.sprite = sprite;
        Renderer.enabled = sprite != null && body.enabled;
        if (!Renderer.enabled) return;

        Sprite s = body.sprite;
        Vector2 px;
        bool front, mirror;
        if (anchors != null && anchors.TryGet(s, out var e)) { px = e.offsetPx; front = e.front; mirror = e.mirror; LastFound = true; }
        else
        {
            // Yedek: bakış yönüne göre gövde yanı, bel hizası
            mirror = mv != null && mv.LastDirection.x < -0.1f;
            px = new Vector2(mirror ? -5f : 5f, 4f);
            front = mv == null || mv.LastDirection.y <= 0.1f;
            LastFound = false;
        }
        LastFront = front;

        // Sprite pikseli → dünya: gövde sprite'ının PPU'su ve ölçeği (piksel ızgarasına oturt)
        float ppu = s != null ? s.pixelsPerUnit : 100f;
        var bt = body.transform;
        Vector3 local = new Vector3(Mathf.Round(px.x) + gripOffsetPx.x, -(Mathf.Round(px.y) + gripOffsetPx.y), 0f) / ppu;
        Vector3 world = bt.TransformPoint(local);
        transform.position = world;
        float bodyScale = Mathf.Abs(bt.lossyScale.x);
        float itemPpu = sprite != null ? sprite.pixelsPerUnit : ppu;
        float k = bodyScale * itemPpu / ppu; // eşya pikseli = karakter pikseli
        var parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(k / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)), k / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)), 1f);
        Renderer.flipX = flipWithFacing && mirror; // görünen kareye göre (batı yönlü kareler)
        Renderer.sortingLayerID = body.sortingLayerID;
        Renderer.sortingOrder = body.sortingOrder + (front ? sortingDelta : -sortingDelta);
    }
}
