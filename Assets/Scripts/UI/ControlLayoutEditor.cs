using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Faz 12 H3: KONTROLLERİ DÜZENLE ekranı. Oyun ekranının taslağı (HUD bandı, arena, kontrol alanı) üzerinde joystick
/// ve aksiyon düğmesi sürüklenip istenen yere konur (arenanın üstü dahil), seçilenin boyutu kaydırıcıyla değişir,
/// SAĞ / SOL ikisini aynalar, VARSAYILAN oyunun kendi düzenine döner, TAMAM kaydeder (ControlSettings).
/// Konumlar ekran oranıyla saklanır; ekran tam kanvas olduğu için oyunda aynı yere gelir.
/// Prefab: Resources/ControlLayoutEditor (KakControlLayoutSetup kurar). Açılış: ControlLayoutEditor.Open(herhangi bir UI).
/// </summary>
public class ControlLayoutEditor : MonoBehaviour
{
    public const string ResourcePath = "ControlLayoutEditor";

    public RectTransform area;          // tam ekran, konumların referansı
    public RectTransform hudBand, arenaBox, controlBox;
    public ControlHandle joystick, button;
    public Slider sizeSlider;
    public TMP_Text sizeLabel;
    public Button mirrorButton, defaultButton, doneButton;

    ControlHandle selected;
    bool defaultPositions, mirror;

    public static ControlLayoutEditor Open(Transform anyUi)
    {
        var prefab = Resources.Load<GameObject>(ResourcePath);
        var canvas = anyUi != null ? anyUi.GetComponentInParent<Canvas>() : Object.FindAnyObjectByType<Canvas>();
        if (prefab == null || canvas == null) return null;
        var root = canvas.rootCanvas.transform;
        var go = Instantiate(prefab, root, false);
        go.name = ResourcePath;
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.transform.SetAsLastSibling();
        return go.GetComponent<ControlLayoutEditor>();
    }

    void Start()
    {
        joystick.editor = this; button.editor = this;
        if (sizeSlider != null)
        {
            sizeSlider.minValue = ControlSettings.MinScale;
            sizeSlider.maxValue = ControlSettings.MaxScale;
            sizeSlider.onValueChanged.AddListener(OnSize);
        }
        if (mirrorButton != null) mirrorButton.onClick.AddListener(OnMirror);
        if (defaultButton != null) defaultButton.onClick.AddListener(OnDefault);
        if (doneButton != null) doneButton.onClick.AddListener(OnDone);

        LayoutSchematic();
        ControlSettings.Positions(out Vector2 j, out Vector2 b);
        defaultPositions = !ControlSettings.Custom;
        mirror = ControlSettings.Mirrored;
        joystick.Set(j, ControlSettings.JoyScale);
        button.Set(b, ControlSettings.ButtonScale);
        Select(joystick);
    }

    /// <summary>Oyun ekranının taslağı: HUD bandı üstte, kare arena, altta kontrol alanı (oyundaki hesapla aynı).</summary>
    void LayoutSchematic()
    {
        var L = ControlSettings.CurrentLayout();
        float h = Mathf.Max(1f, Screen.height);
        float hud = L.hudPx / h, ctrl = L.controlPx / h;
        Box(hudBand, 1f - hud, 1f);
        Box(arenaBox, ctrl, 1f - hud);
        Box(controlBox, 0f, ctrl);
    }

    static void Box(RectTransform r, float y0, float y1)
    {
        if (r == null) return;
        r.anchorMin = new Vector2(0f, y0); r.anchorMax = new Vector2(1f, y1);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    public void Select(ControlHandle h)
    {
        selected = h;
        joystick.SetSelected(h == joystick);
        button.SetSelected(h == button);
        if (sizeSlider != null) sizeSlider.SetValueWithoutNotify(h.Scale);
        if (sizeLabel != null) sizeLabel.text = Loc.T(h == joystick ? "ctl_joy" : "ctl_btn") + "  " + Loc.T("ctl_size");
    }

    /// <summary>Ekran noktası → alan oranı (0-1).</summary>
    public Vector2 ToNormalized(Vector2 screen, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screen, cam, out Vector2 local);
        Rect r = area.rect;
        return new Vector2((local.x - r.xMin) / r.width, (local.y - r.yMin) / r.height);
    }

    public void OnHandleMoved() { defaultPositions = false; }

    void OnSize(float v)
    {
        if (selected == null) return;
        selected.Set(selected.Pos, v);
    }

    void OnMirror()
    {
        mirror = !mirror;
        joystick.Set(new Vector2(1f - joystick.Pos.x, joystick.Pos.y), joystick.Scale);
        button.Set(new Vector2(1f - button.Pos.x, button.Pos.y), button.Scale);
    }

    void OnDefault()
    {
        defaultPositions = true;
        mirror = false;
        ControlSettings.DefaultPositions(ControlSettings.CurrentLayout(), Screen.height, false, out Vector2 j, out Vector2 b);
        joystick.Set(j, 1f);
        button.Set(b, 1f);
        Select(selected != null ? selected : joystick);
    }

    void OnDone()
    {
        ControlSettings.SetLayout(!defaultPositions, joystick.Pos, button.Pos, defaultPositions && mirror, joystick.Scale, button.Scale);
        Destroy(gameObject);
    }
}
