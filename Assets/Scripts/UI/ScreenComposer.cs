using UnityEngine;

/// <summary>
/// Dikey ekran kompozisyonu (sanat-rehberi.md §5):
///   [ HUD bandı (Safe Area + HUD) ]
///   [ KARE ARENA — ekran enine sığar ]
///   [ koridor / kontrol alanı (kalan her şey) ]
/// Kamera boyutunu ve konumunu hesaplar, HUD ve kontrol alanı RectTransform'larını yerleştirir,
/// dünya çerçevesine (DungeonFrame) kamera dikdörtgenini bildirir.
/// Kısa ekranlarda (tablet) kontrol alanı minimumun altına düşerse arena küçülür, yanlar çerçeveyle dolar.
/// Hesap sadece ekran boyutu veya Safe Area değişince yapılır (Play modunda; editörde ForceRecalculate ile).
/// </summary>
[RequireComponent(typeof(Camera))]
public class ScreenComposer : MonoBehaviour
{
    [Header("Referanslar")]
    public ArenaAutoLayout arena;
    public RectTransform hudBand;        // Canvas altında, üste yapışık
    public RectTransform hudContent;     // hudBand içinde; Safe Area kadar aşağıda başlar
    public RectTransform controlArea;    // Canvas altında, alta yapışık
    public RectTransform controlContent; // controlArea içinde; alt Safe Area kadar yukarıda
    public DungeonFrame frame;

    [Header("Boyutlar (Canvas referans birimi, 1080 genişlikte)")]
    public float referenceWidth = 1080f;
    public float hudBandHeight = 210f;
    public float minControlHeight = 520f;

    [Header("Kamera")]
    public Color backgroundColor = new Color(0.03f, 0.045f, 0.05f, 1f);

    public struct Layout
    {
        public float orthoSize;
        public Vector2 cameraCenter;
        public Rect cameraWorld;
        public Rect arenaWorld;
        public float uiScale;          // 1 canvas birimi = uiScale piksel
        public float hudPx;            // HUD bandı yüksekliği (Safe Area dahil)
        public float controlPx;        // kontrol alanı yüksekliği (Safe Area dahil)
        public float safeTopPx;
        public float safeBottomPx;
        public bool arenaFitsWidth;
    }

    public Layout Current { get; private set; }

    Camera cam;
    int lastW = -1, lastH = -1;
    Rect lastSafe;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        lastW = -1;
    }

    void LateUpdate()
    {
        if (Screen.width != lastW || Screen.height != lastH || Screen.safeArea != lastSafe)
            Recalculate();
    }

    public void ForceRecalculate()
    {
        lastW = -1;
        Recalculate();
    }

    public void Recalculate()
    {
        if (arena == null || arena.arenaSpriteRenderer == null) return;
        if (cam == null) cam = GetComponent<Camera>();

        lastW = Screen.width;
        lastH = Screen.height;
        lastSafe = Screen.safeArea;

        Current = Compute(Screen.width, Screen.height, Screen.safeArea, arena.arenaSpriteRenderer.bounds,
                          referenceWidth, hudBandHeight, minControlHeight);
        Apply(Current);
        KakLog.Info($"[ScreenComposer] {Screen.width}x{Screen.height} safe={Screen.safeArea} genişliğe sığdı={Current.arenaFitsWidth} ortho={Current.orthoSize:F2} hud={Current.hudPx:F0}px kontrol={Current.controlPx:F0}px ui={Current.uiScale:F2}");
    }

    /// <summary>Saf hesap (test edilebilir): ekran boyutu + safe area + arena → kamera ve bantlar.</summary>
    public static Layout Compute(float w, float h, Rect safe, Bounds arenaBounds,
                                 float refWidth, float hudRef, float controlMinRef)
    {
        var L = new Layout();
        L.uiScale = w / refWidth; // CanvasScaler: genişliğe göre (match = 0)
        L.safeTopPx = Mathf.Max(0f, h - safe.yMax);
        L.safeBottomPx = Mathf.Max(0f, safe.yMin);

        float hudPx = L.safeTopPx + hudRef * L.uiScale;
        float ctrlMinPx = L.safeBottomPx + controlMinRef * L.uiScale;

        float aw = arenaBounds.size.x, ah = arenaBounds.size.y;
        float wpp = aw / w;                 // piksel başına dünya birimi (genişliğe sığdır)
        float arenaPxH = ah / wpp;
        L.arenaFitsWidth = true;
        if (hudPx + arenaPxH + ctrlMinPx > h)
        {
            // Kısa ekran: arena küçülür, yanlarda çerçeve görünür
            arenaPxH = Mathf.Max(h * 0.3f, h - hudPx - ctrlMinPx);
            wpp = ah / arenaPxH;
            L.arenaFitsWidth = false;
        }

        L.orthoSize = h * wpp * 0.5f;
        float arenaTopPx = h - hudPx;
        float camY = arenaBounds.max.y + L.orthoSize - arenaTopPx * wpp;
        L.cameraCenter = new Vector2(arenaBounds.center.x, camY);
        float halfW = L.orthoSize * (w / h);
        L.cameraWorld = new Rect(L.cameraCenter.x - halfW, camY - L.orthoSize, halfW * 2f, L.orthoSize * 2f);
        L.arenaWorld = new Rect(arenaBounds.min.x, arenaBounds.min.y, aw, ah);
        L.hudPx = hudPx;
        L.controlPx = Mathf.Max(0f, arenaTopPx - arenaPxH);
        return L;
    }

    void Apply(Layout L)
    {
        cam.orthographic = true;
        cam.orthographicSize = L.orthoSize;
        var p = transform.position;
        transform.position = new Vector3(L.cameraCenter.x, L.cameraCenter.y, p.z);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;

        float s = Mathf.Max(0.0001f, L.uiScale);
        if (hudBand != null)
        {
            hudBand.anchorMin = new Vector2(0f, 1f);
            hudBand.anchorMax = new Vector2(1f, 1f);
            hudBand.pivot = new Vector2(0.5f, 1f);
            hudBand.anchoredPosition = Vector2.zero;
            hudBand.sizeDelta = new Vector2(0f, L.hudPx / s);
        }
        if (hudContent != null)
        {
            hudContent.anchorMin = Vector2.zero;
            hudContent.anchorMax = Vector2.one;
            hudContent.offsetMin = Vector2.zero;
            hudContent.offsetMax = new Vector2(0f, -L.safeTopPx / s);
        }
        if (controlArea != null)
        {
            controlArea.anchorMin = new Vector2(0f, 0f);
            controlArea.anchorMax = new Vector2(1f, 0f);
            controlArea.pivot = new Vector2(0.5f, 0f);
            controlArea.anchoredPosition = Vector2.zero;
            controlArea.sizeDelta = new Vector2(0f, L.controlPx / s);
        }
        if (controlContent != null)
        {
            controlContent.anchorMin = Vector2.zero;
            controlContent.anchorMax = Vector2.one;
            controlContent.offsetMin = new Vector2(0f, L.safeBottomPx / s);
            controlContent.offsetMax = Vector2.zero;
        }

        if (frame != null) frame.Layout(L.cameraWorld, L.arenaWorld);
    }
}
