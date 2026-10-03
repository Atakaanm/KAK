using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KakUiKit;

/// <summary>
/// Faz 12 H3: KONTROLLERİ DÜZENLE ekranının prefab'ını kurar (Resources/ControlLayoutEditor.prefab). Menüdeki ayarlardan
/// (ControlSizeButton) ve istenirse oyundan açılır; sahne YAML'ına dokunmaz. Tekrar çalıştırılabilir.
/// </summary>
public static class KakControlLayoutSetup
{
    const string PrefabPath = "Assets/Resources/ControlLayoutEditor.prefab";

    [MenuItem("KacAtaKac/Arayüz/Kontrol Düzeni Ekranını Kur")]
    public static string Build()
    {
        var go = new GameObject("ControlLayoutEditor", typeof(RectTransform));
        try
        {
            var root = Stretch((RectTransform)go.transform);
            var ed = go.AddComponent<ControlLayoutEditor>();

            // Arka plan: oyunu ve menüyü kapatır (dokunmayı da yutar)
            var dim = Stretch(Rect(root, "Dim"));
            Img(dim, S("white_ui.png"), false, KakPalette.Gece, true).preserveAspect = false;

            var area = Stretch(Rect(root, "Area"));
            ed.area = area;
            // Oyun ekranı taslağı (yerleri ControlLayoutEditor.LayoutSchematic ayarlar)
            ed.hudBand = Stretch(Rect(area, "HudBand"));
            Img(ed.hudBand, S("white_ui.png"), false, KakPalette.WithAlpha(KakPalette.ArduvazKoyu, 0.9f)).preserveAspect = false;
            ed.arenaBox = Stretch(Rect(area, "Arena"));
            Img(ed.arenaBox, S("panel_9s.png"), true, KakPalette.WithAlpha(Color.white, 0.35f));
            var arenaLbl = Place(Rect(ed.arenaBox, "Label"), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(800f, 100f));
            Text(arenaLbl, "@ctl_arena", 72, KakPalette.WithAlpha(KakPalette.Sis, 0.45f));
            ed.controlBox = Stretch(Rect(area, "Control"));
            Img(ed.controlBox, S("white_ui.png"), false, KakPalette.WithAlpha(KakPalette.ArduvazKoyu, 0.55f)).preserveAspect = false;

            // Araç çubuğu: güvenli alanın üstünde (başlık, ipucu, boyut, düğmeler)
            var safe = Stretch(Rect(area, "Safe"));
            GetOrAdd<SafeAreaFitter>(safe.gameObject);
            var bar = Place(Rect(safe, "Toolbar"), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(1040f, 430f), new Vector2(0.5f, 1f));
            Img(bar, S("panel_9s.png"), true, null, true);
            Text(Place(Rect(bar, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -54f), new Vector2(980f, 70f)), "@ctl_title", 50, KakPalette.AltinAcik);
            var hint = Text(Place(Rect(bar, "Hint"), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(980f, 50f)), "@ctl_hint", 30, KakPalette.Sis);
            hint.textWrappingMode = TextWrappingModes.Normal;

            ed.sizeLabel = Text(Place(Rect(bar, "SizeLabel"), new Vector2(0f, 1f), new Vector2(40f, -190f), new Vector2(380f, 70f), new Vector2(0f, 0.5f)),
                                "@ctl_size", 36, KakPalette.Krem, TextAlignmentOptions.MidlineLeft);
            ed.sizeSlider = Slider(Place(Rect(bar, "Slider"), new Vector2(1f, 1f), new Vector2(-40f, -190f), new Vector2(560f, 64f), new Vector2(1f, 0.5f)));

            ed.mirrorButton = Button(Place(Rect(bar, "Mirror"), new Vector2(0.5f, 0f), new Vector2(-330f, 80f), new Vector2(300f, 112f)), "@ctl_mirror", Style.Stone, 38);
            ed.defaultButton = Button(Place(Rect(bar, "Default"), new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(330f, 112f)), "@ctl_default", Style.Stone, 38);
            ed.doneButton = Button(Place(Rect(bar, "Done"), new Vector2(0.5f, 0f), new Vector2(340f, 80f), new Vector2(300f, 112f)), "@ctl_done", Style.Gold, 46);

            // Sürüklenen kontroller (araç çubuğunun üstünde çizilir: üste konsalar da tutulabilir)
            ed.joystick = Handle(area, "Joystick", "joystick_base_soft.png", 300f, "joystick_knob_soft.png", 128f, "@ctl_joy");
            ed.button = Handle(area, "Button", "dash_base_soft.png", 200f, "dash_icon_soft.png", 110f, "@ctl_btn");

            var saved = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            return saved != null ? "[KakControlLayoutSetup] " + PrefabPath + " kuruldu" : "HATA: prefab kaydedilemedi";
        }
        finally { Object.DestroyImmediate(go); }
    }

    static ControlHandle Handle(RectTransform parent, string name, string baseFile, float size, string iconFile, float iconSize, string label)
    {
        var rt = Place(Rect(parent, name), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
        var hl = Place(Rect(rt, "Highlight"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 1.22f, size * 1.22f));
        var hlImg = Img(hl, S(baseFile), false, KakPalette.WithAlpha(KakPalette.Altin, 0.85f));
        var body = Stretch(Rect(rt, "Body"));
        Img(body, S(baseFile), false, Color.white);
        Img(Place(Rect(rt, "Icon"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(iconSize, iconSize)), S(iconFile), false, KakPalette.Krem);
        Text(Place(Rect(rt, "Label"), new Vector2(0.5f, 0f), new Vector2(0f, -36f), new Vector2(360f, 56f)), label, 34, KakPalette.Krem);
        var hit = rt.GetComponent<Image>();
        if (hit == null) hit = rt.gameObject.AddComponent<Image>();
        hit.color = new Color(0f, 0f, 0f, 0f);
        hit.raycastTarget = true; // tüm daire tutulur
        var h = GetOrAdd<ControlHandle>(rt.gameObject);
        h.highlight = hlImg;
        return h;
    }

    static Slider Slider(RectTransform rt)
    {
        var bg = Stretch(Rect(rt, "Background"));
        Img(bg, S("btn_stone_9s.png"), true);
        var fillArea = Stretch(Rect(rt, "Fill Area"));
        fillArea.offsetMin = new Vector2(14f, 14f); fillArea.offsetMax = new Vector2(-14f, -14f);
        var fill = Stretch(Rect(fillArea, "Fill"));
        Img(fill, S("white_ui.png"), false, KakPalette.Altin).preserveAspect = false;
        var handleArea = Stretch(Rect(rt, "Handle Slide Area"));
        handleArea.offsetMin = new Vector2(30f, 0f); handleArea.offsetMax = new Vector2(-30f, 0f);
        var handle = Place(Rect(handleArea, "Handle"), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(84f, 84f));
        var hImg = Img(handle, S("joystick_knob_soft.png"), false, Color.white, true);
        var s = GetOrAdd<Slider>(rt.gameObject);
        s.fillRect = fill;
        s.handleRect = handle;
        s.targetGraphic = hImg;
        s.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        s.minValue = ControlSettings.MinScale;
        s.maxValue = ControlSettings.MaxScale;
        s.value = 1f;
        return s;
    }
}
