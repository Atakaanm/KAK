using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KakUiKit;

/// <summary>
/// Faz 15 K8: kaçış kartları (13) + katalog + oyun sahnesi arayüzü (HUD cesaret çubuğu, kart seçim katmanı).
/// Setup: veri (tekrar çalıştırılabilir). BuildUi: KakUiSetup.SetupGameUi çağırır. Köprü: invoke KakPerkSetup Setup
/// </summary>
public static class KakPerkSetup
{
    const string Dir = "Assets/Data/Perks";
    const string CatalogPath = "Assets/Resources/PerkCatalog.asset";

    static Sprite Icon(string n) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Upgrades/up_" + n + ".png");

    [MenuItem("KacAtaKac/Kaçış Kartlarını Kur (Faz 15)")]
    public static string Setup()
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Data", "Perks");
        var list = new List<PerkData>
        {
            P("magnet", "magnet", KakPalette.Altin, 3, 1f, PerkBehaviour.None, M(StatId.Magnet, 0.7f, 0f)),
            P("shieldregen", "shieldregen", KakPalette.CamgobegiParlak, 3, 0.8f, PerkBehaviour.None, M(StatId.ShieldRegen, 1f, 0f)),
            P("swift", "speed", KakPalette.AcikYesil, 3, 1f, PerkBehaviour.None, M(StatId.MoveSpeed, 0f, 0.05f)),
            P("gold", "coin", KakPalette.Altin, 3, 1f, PerkBehaviour.None, M(StatId.CoinGain, 0f, 0.15f)),
            P("score", "score", KakPalette.Altin, 4, 1f, PerkBehaviour.None, M(StatId.ScoreGain, 0f, 0.10f)),
            P("closecall", "nearbonus", KakPalette.Altin, 3, 1f, PerkBehaviour.None, M(StatId.NearMissBonus, 0f, 0.30f), M(StatId.NearMissRadius, 0f, 0.06f)),
            P("slim", "slim", KakPalette.CamgobegiParlak, 3, 0.9f, PerkBehaviour.None, M(StatId.Hurtbox, 0f, -0.05f)),
            P("recover", "invuln", KakPalette.CamgobegiParlak, 3, 0.9f, PerkBehaviour.None, M(StatId.InvulnTime, 0.15f, 0f)),
            P("sense", "sense", KakPalette.CamgobegiParlak, 3, 0.9f, PerkBehaviour.None, M(StatId.WarningTime, 0f, 0.12f)),
            P("heart", "hearts", KakPalette.TehlikeParlak, 3, 0.9f, PerkBehaviour.HeartPiece),
            P("items", "kind_freq", KakPalette.Pembe, 3, 0.9f, PerkBehaviour.None, M(StatId.ItemFrequency, 0f, 0.20f)),
            P("breaker", "breaker", KakPalette.CamgobegiParlak, 3, 0.8f, PerkBehaviour.RockBreaker),
            P("ghost", "ghostmoment", KakPalette.Sis, 3, 0.8f, PerkBehaviour.GhostMoment),
        };
        var cat = AssetDatabase.LoadAssetAtPath<PerkCatalog>(CatalogPath);
        if (cat == null)
        {
            cat = ScriptableObject.CreateInstance<PerkCatalog>();
            AssetDatabase.CreateAsset(cat, CatalogPath);
        }
        cat.perks = list.ToArray();
        cat.baseNeed = 7f; cat.stepNeed = 4f;
        cat.nearMissCourage = 1f; cat.coinCourage = 0.5f; cat.secondCourage = 0.05f; cat.safeAfterPick = 1.2f;
        EditorUtility.SetDirty(cat);
        AssetDatabase.SaveAssets();
        string msg = list.Count + " kaçış kartı → " + CatalogPath;
        Debug.Log("[KakPerkSetup] " + msg);
        return msg;
    }

    static StatModifier M(StatId s, float flat, float pct) => new StatModifier(s, flat, pct);

    static PerkData P(string id, string icon, Color color, int max, float weight, PerkBehaviour b, params StatModifier[] mods)
    {
        string path = Dir + "/Perk_" + id + ".asset";
        var p = AssetDatabase.LoadAssetAtPath<PerkData>(path);
        if (p == null)
        {
            p = ScriptableObject.CreateInstance<PerkData>();
            AssetDatabase.CreateAsset(p, path);
        }
        p.id = id; p.nameKey = "perk_" + id; p.descKey = "perk_" + id + "_d";
        p.icon = Icon(icon); p.color = color; p.maxLevel = max; p.weight = weight; p.behaviour = b; p.perLevel = mods;
        EditorUtility.SetDirty(p);
        return p;
    }

    /// <summary>HUD cesaret çubuğu (HudBand altı) + kart seçim katmanı (HUDCanvas).</summary>
    public static void BuildUi(Transform canvas)
    {
        Setup();
        var white = S("white_ui.png");

        // ── Cesaret çubuğu ──
        var band = FindDeep(canvas, "HudBand") as RectTransform;
        if (band != null)
        {
            // Tutucu her zaman açık (CourageBar.Update çalışsın); çubuk (Bar) tek kişilik sonsuzda görünür
            var holder = Place(Rect(band, "Courage"), new Vector2(0.5f, 0f), new Vector2(0f, -10f), new Vector2(620f, 18f), new Vector2(0.5f, 1f));
            var bar = Stretch(Rect(holder, "Bar"));
            var bg = Img(bar, white, false, KakPalette.WithAlpha(KakPalette.Gece, 0.9f));
            bg.preserveAspect = false;
            var fill = Img(Stretch(Rect(bar, "Fill")), white, false, KakPalette.CamgobegiParlak);
            fill.preserveAspect = false;
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillAmount = 0f;
            var badge = Place(Rect(bar, "Level"), new Vector2(0f, 0.5f), new Vector2(-30f, 0f), new Vector2(46f, 46f));
            Img(badge, S("btn_gold_9s.png"), true);
            var lt = Text(Stretch(Rect(badge, "Text")), "1", 28, KakPalette.Murekkep, TextAlignmentOptions.Center, false, false);
            var cb = GetOrAdd<CourageBar>(holder.gameObject);
            cb.root = bar.gameObject; cb.fill = fill; cb.levelText = lt;
            EditorUtility.SetDirty(cb);
        }

        // ── Kart seçim katmanı ──
        var old = canvas.Find("PerkLayer");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var layer = Stretch(Rect(canvas, "PerkLayer"));
        var root = Stretch(Rect(layer, "Root"));
        var dim = Img(Stretch(Rect(root, "Dim")), white, false, KakPalette.WithAlpha(KakPalette.Murekkep, 0.82f), true);
        dim.preserveAspect = false;
        Text(Place(Rect(root, "Title"), new Vector2(0.5f, 0.5f), new Vector2(0f, 470f), new Vector2(980f, 110f)), "@perk_title", 76, KakPalette.AltinAcik);
        Text(Place(Rect(root, "Subtitle"), new Vector2(0.5f, 0.5f), new Vector2(0f, 385f), new Vector2(980f, 60f)), "@perk_pick", 36, KakPalette.Krem,
             TextAlignmentOptions.Center, false, false);
        var pp = GetOrAdd<PerkPanel>(layer.gameObject);
        pp.root = root.gameObject;
        pp.cards = new PerkPanel.CardUi[3];
        for (int i = 0; i < 3; i++)
        {
            var card = Place(Rect(root, "Card" + i), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 330f, 0f), new Vector2(310f, 500f));
            var bg = Img(card, S("btn_stone_9s.png"), true, null, true);
            var btn = GetOrAdd<Button>(card.gameObject);
            btn.targetGraphic = bg;
            if (card.GetComponent<ButtonScaleAnimation>() == null) card.gameObject.AddComponent<ButtonScaleAnimation>();
            var icon = Img(Place(Rect(card, "Icon"), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(144f, 144f)), null, false);
            var nm = Text(Place(Rect(card, "Name"), new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(290f, 60f)), "Mıknatıs", 36, KakPalette.Krem);
            nm.enableAutoSizing = true; nm.fontSizeMin = 24f; nm.fontSizeMax = 36f;
            var desc = Text(Place(Rect(card, "Desc"), new Vector2(0.5f, 1f), new Vector2(0f, -340f), new Vector2(270f, 110f)), "", 26, KakPalette.Sis,
                            TextAlignmentOptions.Center, false, false);
            desc.textWrappingMode = TextWrappingModes.Normal;
            var pips = new Image[5];
            for (int k = 0; k < pips.Length; k++)
            {
                pips[k] = Img(Place(Rect(card, "Pip" + k), new Vector2(0.5f, 0f), new Vector2((k - 2) * 40f, 44f), new Vector2(32f, 14f)), white, false);
                pips[k].preserveAspect = false;
            }
            if (i == 0) OnClick(btn, pp.Pick0); else if (i == 1) OnClick(btn, pp.Pick1); else OnClick(btn, pp.Pick2);
            pp.cards[i] = new PerkPanel.CardUi { body = card, background = bg, icon = icon, nameText = nm, descText = desc, pips = pips };
        }
        var burstRt = Place(Rect(layer, "Burst"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 10f));
        var burst = GetOrAdd<UiBurst>(burstRt.gameObject);
        burst.sprite = white; burst.count = 18; burst.speed = 700f;
        pp.burst = burst;
        root.gameObject.SetActive(false);
        EditorUtility.SetDirty(pp);
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            var r = FindDeep(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
