using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI kurucu yardımcıları: tek görsel dil (sanat-rehberi.md). Butonlar, paneller, yazılar, anahtarlar.
/// Fontlar: Nunito ExtraBold (UI, Türkçe tam), Cinzel Decorative (sadece logo).
/// </summary>
public static class KakUiKit
{
    public const string UiDir = "Assets/Art/UI/";
    public const float PixelMult = 0.2f; // 9-slice kenarları 5x büyüsün (piksel görünüm)

    static TMP_FontAsset nunito, cinzel;
    static Material nunitoOutline, cinzelOutline;

    public static TMP_FontAsset Nunito => nunito != null ? nunito : nunito = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Nunito-ExtraBold SDF.asset");
    public static TMP_FontAsset Cinzel => cinzel != null ? cinzel : cinzel = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/CinzelDecorative-Bold SDF.asset");

    /// <summary>Konturlu font materyali (tek sefer oluşturulur, paylaşılır).</summary>
    public static Material OutlineMat(TMP_FontAsset font, string name, float width)
    {
        string path = "Assets/Fonts/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(font.material);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.shaderKeywords = font.material.shaderKeywords;
        mat.SetTexture("_MainTex", font.material.GetTexture("_MainTex"));
        mat.EnableKeyword("OUTLINE_ON");
        mat.SetFloat("_OutlineWidth", width);
        mat.SetColor("_OutlineColor", KakPalette.Murekkep);
        mat.SetFloat("_FaceDilate", width * 0.5f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    public static Material NunitoOutline => nunitoOutline != null ? nunitoOutline : nunitoOutline = OutlineMat(Nunito, "Nunito-ExtraBold Outline", 0.22f);
    public static Material CinzelOutline => cinzelOutline != null ? cinzelOutline : cinzelOutline = OutlineMat(Cinzel, "Cinzel Outline", 0.25f);

    /// <summary>Unity nesnelerinde `??` güvenli değildir (sahte null); bunu kullan.</summary>
    public static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    public static Sprite S(string file) => AssetDatabase.LoadAssetAtPath<Sprite>(UiDir + file);

    public static RectTransform Rect(Transform parent, string name)
    {
        var t = parent.Find(name) as RectTransform;
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, name);
            go.transform.SetParent(parent, false);
            t = (RectTransform)go.transform;
        }
        t.localScale = Vector3.one;
        return t;
    }

    public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    public static RectTransform Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        return rt;
    }

    public static Image Img(RectTransform rt, Sprite sprite, bool sliced, Color? color = null, bool raycast = false)
    {
        var img = rt.GetComponent<Image>();
        if (img == null) img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        img.pixelsPerUnitMultiplier = sliced ? PixelMult : 1f;
        img.preserveAspect = !sliced && sprite != null;
        img.color = color ?? Color.white;
        img.raycastTarget = raycast;
        return img;
    }

    public static TextMeshProUGUI Text(RectTransform rt, string text, float size, Color color,
                                       TextAlignmentOptions align = TextAlignmentOptions.Center, bool logo = false, bool outline = true)
    {
        var t = rt.GetComponent<TextMeshProUGUI>();
        if (t == null) t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        // "@anahtar" → yerelleştirilmiş metin (LocText ile dil değişince güncellenir)
        var lt = rt.GetComponent<LocText>();
        if (text != null && text.StartsWith("@"))
        {
            string key = text.Substring(1);
            if (lt == null) lt = rt.gameObject.AddComponent<LocText>();
            lt.key = key;
            text = global::Loc.T(key);
        }
        else if (lt != null) Object.DestroyImmediate(lt);
        t.font = logo ? Cinzel : Nunito;
        if (outline) t.fontSharedMaterial = logo ? CinzelOutline : NunitoOutline;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        t.characterSpacing = logo ? 4f : 2f;
        return t;
    }

    public enum Style { Gold, Stone }

    /// <summary>9-slice piksel buton + yazı (+ isteğe bağlı ikon). Basma animasyonu ve tıklama sesi dahil.</summary>
    public static Button Button(RectTransform rt, string label, Style style, float fontSize = 64f, string iconFile = null)
    {
        var img = Img(rt, S(style == Style.Gold ? "btn_gold_9s.png" : "btn_stone_9s.png"), true, null, true);
        var b = rt.GetComponent<Button>();
        if (b == null) b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var colors = b.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.disabledColor = new Color(0.55f, 0.55f, 0.6f, 1f);
        b.colors = colors;
        b.transition = Selectable.Transition.ColorTint;
        b.onClick.RemoveAllListeners();
        for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(b.onClick, i);
        if (rt.GetComponent<ButtonScaleAnimation>() == null) rt.gameObject.AddComponent<ButtonScaleAnimation>();

        var lbl = Stretch(Rect(rt, "Label"));
        Color textColor = style == Style.Gold ? KakPalette.Murekkep : KakPalette.Krem;
        var t = Text(lbl, label, fontSize, textColor, TextAlignmentOptions.Center, false, style != Style.Gold);
        if (iconFile != null)
        {
            var ic = Rect(rt, "Icon");
            float s = fontSize * 0.9f;
            Place(ic, new Vector2(0f, 0.5f), new Vector2(fontSize * 0.9f, 0f), new Vector2(s, s));
            Img(ic, S(iconFile), false, style == Style.Gold ? KakPalette.Murekkep : KakPalette.Krem);
            lbl.offsetMin = new Vector2(fontSize * 1.1f, 0f);
        }
        else
        {
            var old = rt.Find("Icon");
            if (old != null) Object.DestroyImmediate(old.gameObject);
        }
        return b;
    }

    public static void OnClick(Button b, UnityAction action)
    {
        UnityEventTools.AddPersistentListener(b.onClick, action);
    }

    /// <summary>Ekranı kaplayan karartma + ortada panel (kök/Dim, kök/Fit/Panel). Fit katmanı kısa ekranda paneli
    /// sığdırır (UiFitToScreen). Döner: (kök, panel).</summary>
    public static (RectTransform root, RectTransform panel) Modal(Transform parent, string name, Vector2 panelSize, float dimAlpha = 0.72f)
    {
        var root = Stretch(Rect(parent, name));
        var dim = Stretch(Rect(root, "Dim"));
        Img(dim, S("white_ui.png"), false, KakPalette.WithAlpha(KakPalette.Murekkep, dimAlpha), true).preserveAspect = false;
        var fit = Stretch(Rect(root, "Fit"));
        var legacy = root.Find("Panel"); // eski kurulum (Fit'ten önce): içeriğiyle birlikte taşı
        if (legacy != null) Undo.SetTransformParent(legacy, fit, "Modal Fit");
        var panel = Place(Rect(fit, "Panel"), new Vector2(0.5f, 0.5f), Vector2.zero, panelSize);
        Img(panel, S("panel_9s.png"), true, null, true);
        GetOrAdd<UiFitToScreen>(fit.gameObject).content = panel;
        return (root, panel);
    }

    /// <summary>Ayar satırı: solda yazı, sağda anahtar.</summary>
    /// <summary>Dil satırı: solda "Dil", sağda TÜRKÇE/ENGLISH butonu.</summary>
    public static LanguageButton LanguageRow(RectTransform parent, float y)
    {
        var row = Place(Rect(parent, "LanguageRow"), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(640f, 110f));
        var lbl = Place(Rect(row, "Label"), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(300f, 100f), new Vector2(0f, 0.5f));
        Text(lbl, "@language", 52, KakPalette.Krem, TextAlignmentOptions.MidlineLeft);
        var btnRt = Place(Rect(row, "Button"), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(300f, 96f), new Vector2(1f, 0.5f));
        Img(btnRt, S("btn_stone_9s.png"), true, null, true);
        var lblT = Text(Stretch(Rect(btnRt, "Label")), "TÜRKÇE", 40, KakPalette.Krem);
        var lb = btnRt.GetComponent<LanguageButton>();
        if (lb == null) lb = btnRt.gameObject.AddComponent<LanguageButton>();
        lb.label = lblT;
        if (btnRt.GetComponent<ButtonScaleAnimation>() == null) btnRt.gameObject.AddComponent<ButtonScaleAnimation>();
        return lb;
    }

    public static KakToggle ToggleRow(RectTransform parent, string name, string label, KakToggle.Setting setting, float y)
    {
        var row = Place(Rect(parent, name), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(640f, 110f));
        var lbl = Place(Rect(row, "Label"), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(400f, 100f), new Vector2(0f, 0.5f));
        Text(lbl, label, 52, KakPalette.Krem, TextAlignmentOptions.MidlineLeft);
        var sw = Place(Rect(row, "Switch"), new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(170f, 84f), new Vector2(1f, 0.5f));
        var bg = Img(sw, S("toggle_on_9s.png"), true, null, true);
        bg.pixelsPerUnitMultiplier = 0.1f;
        var knob = Place(Rect(sw, "Knob"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(72f, 72f));
        Img(knob, S("toggle_knob.png"), false);
        var tg = sw.GetComponent<KakToggle>();
        if (tg == null) tg = sw.gameObject.AddComponent<KakToggle>();
        tg.setting = setting;
        tg.background = bg;
        tg.knob = knob;
        tg.onSprite = S("toggle_on_9s.png");
        tg.offSprite = S("toggle_off_9s.png");
        tg.knobTravel = 84f;
        return tg;
    }

    public static void DestroyChildrenExcept(Transform parent, params string[] keep)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var c = parent.GetChild(i);
            if (System.Array.IndexOf(keep, c.name) >= 0) continue;
            Undo.DestroyObjectImmediate(c.gameObject);
        }
    }
}
