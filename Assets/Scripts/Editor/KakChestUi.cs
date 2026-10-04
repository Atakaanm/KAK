using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KakUiKit;

/// <summary>
/// Faz 15 K6: sandık arayüzü ve ekonomi verisi.
///  - SetupEconomy: Resources/EconomyData (anahtar eşikleri, 3 sandık, garanti) — tekrar çalıştırılabilir
///  - BuildMenu: menüde SANDIK düğmesi (anahtar rozeti), ChestPanel (3 kart), ChestOpening (açılış katmanı)
///  - BuildKeysBadge: oyun sonu panelinin sağ üstünde "+2 anahtar" rozeti
/// KakUiSetup.SetupMenu / SetupGameUi çağırır.
/// </summary>
public static class KakChestUi
{
    const string Dir = "Assets/Art/UI/Chests/";
    const string EconomyPath = "Assets/Resources/EconomyData.asset";

    static Sprite C(string f) => AssetDatabase.LoadAssetAtPath<Sprite>(Dir + f);

    [MenuItem("KacAtaKac/Sandıkları Kur (Faz 15)")]
    public static string SetupEconomy()
    {
        var e = AssetDatabase.LoadAssetAtPath<EconomyData>(EconomyPath);
        if (e == null)
        {
            e = ScriptableObject.CreateInstance<EconomyData>();
            AssetDatabase.CreateAsset(e, EconomyPath);
        }
        e.keyMilestones = new[] { 60f, 120f, 180f };
        e.keyOnRecord = 1;
        e.pityEvery = 10;
        e.bestScoreForDouble = 10000f;
        e.bestScoreCap = 1f;
        e.chests = new[]
        {
            new ChestDef { id = "wood", nameKey = "chest_wood", keyCost = 1, goldMin = 60, goldMax = 120, scrollMin = 1, scrollMax = 2, scrollChance = 0.7f, bonusKeyChance = 0f,
                           closed = C("chest_wood_closed.png"), open = C("chest_wood_open.png"), glow = KakPalette.AltinAcik },
            new ChestDef { id = "silver", nameKey = "chest_silver", keyCost = 3, goldMin = 220, goldMax = 360, scrollMin = 3, scrollMax = 5, scrollChance = 1f, bonusKeyChance = 0.2f,
                           closed = C("chest_silver_closed.png"), open = C("chest_silver_open.png"), glow = KakPalette.CamgobegiParlak },
            new ChestDef { id = "gold", nameKey = "chest_gold", keyCost = 10, goldMin = 900, goldMax = 1300, scrollMin = 10, scrollMax = 14, scrollChance = 1f, bonusKeyChance = 0.5f,
                           closed = C("chest_gold_closed.png"), open = C("chest_gold_open.png"), glow = KakPalette.Altin },
        };
        EditorUtility.SetDirty(e);
        AssetDatabase.SaveAssets();
        string msg = "ekonomi: " + EconomyPath + " (3 sandık, anahtar 60/120/180 sn + rekor)";
        Debug.Log("[KakChestUi] " + msg);
        return msg;
    }

    public static void BuildMenu(Transform canvas, RectTransform safe, WalletHud wallet, MainMenuController mmc)
    {
        SetupEconomy();
        var white = S("white_ui.png");
        var coin = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png");

        // ── Menü düğmesi: renkli sandık + yazı + anahtar rozeti ──
        var btn = Button(Place(Rect(safe, "ChestButton"), new Vector2(0.5f, 0f), new Vector2(0f, 285f), new Vector2(300f, 130f)), "@chests", Style.Stone, 38);
        var lbl = btn.transform.Find("Label") as RectTransform;
        if (lbl != null) lbl.offsetMin = new Vector2(96f, 0f);
        var icon = Img(Place(Rect(btn.transform, "Chest"), new Vector2(0f, 0.5f), new Vector2(62f, 2f), new Vector2(84f, 74f)), C("chest_wood_closed.png"), false);
        var badge = Place(Rect(btn.transform, "KeyBadge"), new Vector2(1f, 1f), new Vector2(-6f, 6f), new Vector2(92f, 52f), new Vector2(1f, 1f));
        Img(badge, S("btn_gold_9s.png"), true);
        Img(Place(Rect(badge, "Key"), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(34f, 34f)), C("key.png"), false);
        var btext = Text(Place(Rect(badge, "Count"), new Vector2(0f, 0.5f), new Vector2(46f, 0f), new Vector2(44f, 44f), new Vector2(0f, 0.5f)),
                         "0", 30, KakPalette.Murekkep, TextAlignmentOptions.MidlineLeft, false, false);
        var cb = GetOrAdd<ChestButton>(btn.gameObject);
        cb.badge = badge; cb.badgeText = btext; cb.chestIcon = icon.rectTransform;
        EditorUtility.SetDirty(cb);

        // ── Sandık paneli ──
        var old = canvas.Find("ChestPanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var (root, panel) = Modal(canvas, "ChestPanel", new Vector2(960f, 1560f));
        Text(Place(Rect(panel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(800f, 110f)), "@chests_title", 78, KakPalette.Altin);
        var krow = Place(Rect(panel, "Keys"), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(300f, 64f));
        Img(Place(Rect(krow, "Icon"), new Vector2(0.5f, 0.5f), new Vector2(-50f, 0f), new Vector2(56f, 56f)), C("key.png"), false);
        var ktext = Text(Place(Rect(krow, "Amount"), new Vector2(0.5f, 0.5f), new Vector2(50f, 0f), new Vector2(140f, 64f)), "0", 50, KakPalette.AltinAcik,
                         TextAlignmentOptions.MidlineLeft);
        Text(Place(Rect(panel, "Hint"), new Vector2(0.5f, 1f), new Vector2(0f, -248f), new Vector2(860f, 40f)), "@chest_keys_hint", 24, KakPalette.Sis,
             TextAlignmentOptions.Center, false, false);

        var cp = GetOrAdd<ChestPanel>(root.gameObject);
        cp.cards = new ChestPanel.Card[3];
        for (int i = 0; i < 3; i++)
        {
            var card = Place(Rect(panel, "Card" + i), new Vector2(0.5f, 1f), new Vector2(0f, -440f - i * 320f), new Vector2(860f, 290f));
            Img(card, S("btn_stone_9s.png"), true);
            var ch = Img(Place(Rect(card, "Chest"), new Vector2(0f, 0.5f), new Vector2(150f, 0f), new Vector2(224f, 196f)), C("chest_wood_closed.png"), false);
            var holder = ch.rectTransform; // zıplama Chest'in kendisine uygulanır: tutucu ile sarmalamaya gerek yok
            var nm = Text(Place(Rect(card, "Name"), new Vector2(0f, 0.5f), new Vector2(290f, 50f), new Vector2(330f, 60f), new Vector2(0f, 0.5f)),
                          "Ahşap sandık", 40, KakPalette.Krem, TextAlignmentOptions.MidlineLeft);
            var hint = Text(Place(Rect(card, "Hint"), new Vector2(0f, 0.5f), new Vector2(290f, -10f), new Vector2(330f, 70f), new Vector2(0f, 0.5f)),
                            "", 26, KakPalette.Sis, TextAlignmentOptions.MidlineLeft, false, false);
            hint.textWrappingMode = TextWrappingModes.Normal;
            var open = Button(Place(Rect(card, "Open"), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(210f, 120f), new Vector2(1f, 0.5f)),
                              "1", Style.Gold, 50, "Chests/key.png");
            var cost = open.transform.Find("Label").GetComponent<TMP_Text>();
            var keyIcon = open.transform.Find("Icon") as RectTransform;
            if (keyIcon != null) { keyIcon.GetComponent<Image>().color = Color.white; Place(keyIcon, new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(52f, 52f)); }
            Text(Place(Rect(card, "OpenLabel"), new Vector2(1f, 0.5f), new Vector2(-129f, 80f), new Vector2(200f, 40f)), "@chest_open", 30, KakPalette.AltinAcik);
            if (i == 0) OnClick(open, cp.Open0); else if (i == 1) OnClick(open, cp.Open1); else OnClick(open, cp.Open2);
            cp.cards[i] = new ChestPanel.Card { body = card, chest = ch, nameText = nm, hintText = hint, costText = cost, buttonBackground = open.GetComponent<Image>() };
            _ = holder;
        }
        var pity = Text(Place(Rect(panel, "Pity"), new Vector2(0.5f, 0f), new Vector2(0f, 215f), new Vector2(860f, 44f)), "", 28, KakPalette.AltinAcik,
                        TextAlignmentOptions.Center, false, false);
        var close = Button(Place(Rect(panel, "CloseButton"), new Vector2(0.5f, 0f), new Vector2(0f, 95f), new Vector2(560f, 120f)), "@close", Style.Gold, 58);
        cp.keysText = ktext; cp.pityText = pity; cp.menuWallet = wallet;
        cp.goldSprite = S("btn_gold_9s.png"); cp.stoneSprite = S("btn_stone_9s.png");

        // ── Açılış katmanı ──
        cp.opening = BuildOpening(canvas, white, coin);
        EditorUtility.SetDirty(cp);

        if (mmc != null)
        {
            mmc.chestPanel = root.gameObject;
            OnClick(btn, mmc.OnChestsClicked);
            OnClick(close, mmc.CloseChestPanel);
            EditorUtility.SetDirty(mmc);
        }
        root.gameObject.SetActive(false);
    }

    static ChestOpening BuildOpening(Transform canvas, Sprite white, Sprite coin)
    {
        var old = canvas.Find("ChestOpening");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var root = Stretch(Rect(canvas, "ChestOpening"));
        var dim = Img(Stretch(Rect(root, "Dim")), white, false, KakPalette.WithAlpha(KakPalette.Murekkep, 0.96f), true);
        dim.preserveAspect = false;
        var skip = GetOrAdd<Button>(dim.gameObject);
        skip.transition = Selectable.Transition.None;
        var rays = Img(Place(Rect(root, "Rays"), new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(1000f, 1000f)), C("rays.png"), false);
        var chest = Img(Place(Rect(root, "Chest"), new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(448f, 392f)), C("chest_wood_closed.png"), false);
        var burstRt = Place(Rect(root, "Burst"), new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(10f, 10f));
        var burst = GetOrAdd<UiBurst>(burstRt.gameObject);
        burst.sprite = white; burst.count = 28; burst.speed = 900f; burst.life = 0.8f;
        var pity = Text(Place(Rect(root, "Pity"), new Vector2(0.5f, 0.5f), new Vector2(0f, 600f), new Vector2(900f, 80f)), "@reward_pity", 54, KakPalette.AltinAcik);
        var slots = new RectTransform[3];
        var icons = new Image[3];
        var texts = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            var s = Place(Rect(root, "Reward" + i), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f - i * 120f), new Vector2(620f, 104f));
            Img(s, S("btn_stone_9s.png"), true);
            icons[i] = Img(Place(Rect(s, "Icon"), new Vector2(0f, 0.5f), new Vector2(70f, 0f), new Vector2(72f, 72f)), coin, false);
            texts[i] = Text(Place(Rect(s, "Text"), new Vector2(0f, 0.5f), new Vector2(130f, 0f), new Vector2(460f, 80f), new Vector2(0f, 0.5f)),
                            "+0", 50, KakPalette.Krem, TextAlignmentOptions.MidlineLeft);
            slots[i] = s;
        }
        var tap = Text(Place(Rect(root, "TapHint"), new Vector2(0.5f, 0f), new Vector2(0f, 260f), new Vector2(600f, 60f)), "@chest_tap", 32, KakPalette.Sis,
                       TextAlignmentOptions.Center, false, false);
        var ok = Button(Place(Rect(root, "OkButton"), new Vector2(0.5f, 0f), new Vector2(0f, 230f), new Vector2(560f, 130f)), "@chest_ok", Style.Gold, 60);
        var flash = Img(Stretch(Rect(root, "Flash")), white, false, new Color(1f, 1f, 1f, 0f));
        flash.preserveAspect = false;

        var co = GetOrAdd<ChestOpening>(root.gameObject);
        co.chest = chest; co.rays = rays; co.flash = flash; co.burst = burst;
        co.slots = slots; co.slotIcons = icons; co.slotTexts = texts;
        co.pityLabel = pity; co.tapHint = tap; co.okButton = ok.gameObject;
        co.coinSprite = coin; co.scrollSprite = S("icon_scroll.png"); co.keySprite = C("key.png");
        OnClick(skip, co.Skip);
        OnClick(ok, co.Close);
        EditorUtility.SetDirty(co);
        root.gameObject.SetActive(false);
        return co;
    }

    /// <summary>Oyun sonu paneli: sağ üstte zıplayarak gelen "+N anahtar" rozeti.</summary>
    public static void BuildKeysBadge(RectTransform gpanel, GameOverScreen gos)
    {
        var row = Place(Rect(gpanel, "KeysBadge"), new Vector2(1f, 1f), new Vector2(-30f, -40f), new Vector2(170f, 76f), new Vector2(1f, 1f));
        Img(row, S("badge_9s.png"), true);
        Img(Place(Rect(row, "Key"), new Vector2(0f, 0.5f), new Vector2(42f, 0f), new Vector2(52f, 52f)), C("key.png"), false);
        var t = Text(Place(Rect(row, "Text"), new Vector2(0f, 0.5f), new Vector2(76f, 0f), new Vector2(90f, 60f), new Vector2(0f, 0.5f)),
                     "+1", 44, KakPalette.AltinAcik, TextAlignmentOptions.MidlineLeft);
        gos.keysRow = row; gos.keysText = t;
        row.gameObject.SetActive(false);
        EditorUtility.SetDirty(gos);
    }
}
