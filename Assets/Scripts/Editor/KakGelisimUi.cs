using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KakUiKit;

/// <summary>
/// Faz 15 K4: GELİŞİM merkezi (menü). KakUiSetup.SetupMenu çağırır:
///  - MenuHub/MenuTabs: sekmeler (0 KAHRAMAN = CharactersPanel, 1 GELİŞİM = ProgressPanel)
///  - ProgressPanel: sekme çubuğu, cüzdan, özet satırı, kaydırılabilir gruplu iz listesi (satır şablonu + grup başlığı şablonu), KAPAT
///  - InfoToast: üstten kayan bilgi şeridi
///  - Menü düğmesi KARAKTER → GELİŞİM
/// </summary>
public static class KakGelisimUi
{
    public static void Build(Transform canvas, WalletHud wallet, RectTransform charsRoot, MainMenuController mmc, Button menuButton)
    {
        var white = S("white_ui.png");
        var coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png");

        // ── Sekmeler ──
        var hub = Stretch(Rect(canvas, "MenuHub"));
        var tabs = GetOrAdd<MenuTabs>(hub.gameObject);

        // ── GELİŞİM ve EŞYA panelleri (aynı liste kalıbı) ──
        var toast = BuildToast(canvas, white);
        var level = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Endless_Level1_LevelData.asset");
        var (proot, close) = BuildListPage(canvas, "ProgressPanel", 1, false, null, tabs, wallet, toast, white, coinSprite);
        var (iroot, closeI) = BuildListPage(canvas, "ItemsPanel", 2, true, level != null ? level.availablePowerups : null, tabs, wallet, toast, white, coinSprite);

        // ── KAHRAMAN paneli: başlık yerine sekme çubuğu ──
        if (charsRoot != null)
        {
            var cpanel = charsRoot.Find("Fit/Panel") as RectTransform;
            if (cpanel != null)
            {
                var title = cpanel.Find("Title");
                if (title != null) Object.DestroyImmediate(title.gameObject);
                TabBar(cpanel, 0, tabs);
            }
        }
        tabs.pages = new[] { charsRoot != null ? charsRoot.gameObject : null, proot.gameObject, iroot.gameObject };
        EditorUtility.SetDirty(tabs);

        var detail = Object.FindAnyObjectByType<CharacterDetailPanel>(FindObjectsInactive.Include);
        if (detail != null) { detail.tabs = tabs; EditorUtility.SetDirty(detail); }

        // ── Menü ──
        if (menuButton != null)
            Button((RectTransform)menuButton.transform, "@progress", Style.Stone, 38, "icon_upgrade.png");
        if (mmc != null)
        {
            mmc.hubTabs = tabs;
            mmc.toast = toast;
            OnClick(close, mmc.CloseCharactersPanel);
            OnClick(closeI, mmc.CloseCharactersPanel);
            EditorUtility.SetDirty(mmc);
        }
        proot.gameObject.SetActive(false);
        iroot.gameObject.SetActive(false);
    }

    static (RectTransform root, Button close) BuildListPage(Transform canvas, string name, int tab, bool itemsMode, PowerupData[] items,
                                                            MenuTabs tabs, WalletHud wallet, InfoToast toast, Sprite white, Sprite coinSprite)
    {
        var old = canvas.Find(name);
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var (proot, ppanel) = Modal(canvas, name, new Vector2(960f, 1560f));
        TabBar(ppanel, tab, tabs);
        // Cüzdan (+ eşya sayfasında parşömen)
        float wx = itemsMode ? -150f : 0f;
        var wrow = Place(Rect(ppanel, "Wallet"), new Vector2(0.5f, 1f), new Vector2(wx, -190f), new Vector2(360f, 64f));
        Img(Place(Rect(wrow, "Icon"), new Vector2(0.5f, 0.5f), new Vector2(-70f, 0f), new Vector2(52f, 52f)), coinSprite, false);
        var wtext = Text(Place(Rect(wrow, "Amount"), new Vector2(0.5f, 0.5f), new Vector2(40f, 0f), new Vector2(200f, 64f)),
                         "0", 50, KakPalette.AltinAcik, TextAlignmentOptions.MidlineLeft);
        TMP_Text stext = null;
        if (itemsMode)
        {
            var srow = Place(Rect(ppanel, "Scrolls"), new Vector2(0.5f, 1f), new Vector2(190f, -190f), new Vector2(300f, 64f));
            Img(Place(Rect(srow, "Icon"), new Vector2(0.5f, 0.5f), new Vector2(-60f, 0f), new Vector2(56f, 56f)), S("icon_scroll.png"), false);
            stext = Text(Place(Rect(srow, "Amount"), new Vector2(0.5f, 0.5f), new Vector2(50f, 0f), new Vector2(160f, 64f)),
                         "0", 50, KakPalette.Krem, TextAlignmentOptions.MidlineLeft);
        }
        var summary = Text(Place(Rect(ppanel, "Summary"), new Vector2(0.5f, 1f), new Vector2(0f, -248f), new Vector2(860f, 40f)),
                           "GELİŞİM SEVİYESİ 0", 26, KakPalette.Sis, TextAlignmentOptions.Center, false, false);

        // Kaydırılabilir liste
        var scroll = Rect(ppanel, "Scroll");
        scroll.anchorMin = new Vector2(0f, 0f); scroll.anchorMax = new Vector2(1f, 1f);
        scroll.offsetMin = new Vector2(44f, 185f); scroll.offsetMax = new Vector2(-44f, -282f);
        var viewport = Stretch(Rect(scroll, "Viewport"));
        GetOrAdd<RectMask2D>(viewport.gameObject);
        var vimg = Img(viewport, white, false, new Color(0f, 0f, 0f, 0.001f), true);
        vimg.preserveAspect = false;
        var content = Rect(viewport, "Content");
        content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(0f, 1000f);
        var vlg = GetOrAdd<VerticalLayoutGroup>(content.gameObject);
        vlg.spacing = 12f; vlg.padding = new RectOffset(0, 0, 4, 24);
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        var fitter = GetOrAdd<ContentSizeFitter>(content.gameObject);
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var sr = GetOrAdd<ScrollRect>(scroll.gameObject);
        sr.viewport = viewport; sr.content = content;
        sr.horizontal = false; sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.scrollSensitivity = 30f; sr.inertia = true; sr.decelerationRate = 0.12f;

        var header = BuildHeaderTemplate(content, white);
        var row = BuildRowTemplate(content, white, coinSprite);

        var close = Button(Place(Rect(ppanel, "CloseButton"), new Vector2(0.5f, 0f), new Vector2(0f, 95f), new Vector2(560f, 120f)), "@close", Style.Gold, 58);

        var pp = GetOrAdd<ProgressPanel>(proot.gameObject);
        pp.content = content; pp.rowTemplate = row; pp.headerTemplate = header;
        pp.walletText = wtext; pp.scrollText = stext; pp.summaryText = summary; pp.menuWallet = wallet;
        pp.itemsMode = itemsMode; pp.items = items; pp.toast = toast;
        EditorUtility.SetDirty(pp);
        return (proot, close);
    }

    static void TabBar(RectTransform panel, int active, MenuTabs tabs)
    {
        var bar = Place(Rect(panel, "Tabs"), new Vector2(0.5f, 1f), new Vector2(0f, -95f), new Vector2(880f, 120f));
        var heroes = Button(Place(Rect(bar, "TabHeroes"), new Vector2(0.5f, 0.5f), new Vector2(-293f, 0f), new Vector2(286f, 106f)),
                            "@tab_heroes", active == 0 ? Style.Gold : Style.Stone, 34, "icon_character.png");
        var prog = Button(Place(Rect(bar, "TabProgress"), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(286f, 106f)),
                          "@tab_progress", active == 1 ? Style.Gold : Style.Stone, 34, "icon_upgrade.png");
        var items = Button(Place(Rect(bar, "TabItems"), new Vector2(0.5f, 0.5f), new Vector2(293f, 0f), new Vector2(286f, 106f)),
                           "@tab_items", active == 2 ? Style.Gold : Style.Stone, 34, "icon_scroll.png");
        OnClick(heroes, tabs.ShowHeroes);
        OnClick(prog, tabs.ShowProgress);
        OnClick(items, tabs.ShowItems);
    }

    static RectTransform BuildHeaderTemplate(RectTransform content, Sprite white)
    {
        var h = Rect(content, "HeaderTemplate");
        GetOrAdd<LayoutElement>(h.gameObject).preferredHeight = 70f;
        Img(Place(Rect(h, "Icon"), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(60f, 60f)), null, false);
        Text(Place(Rect(h, "Label"), new Vector2(0f, 0.5f), new Vector2(74f, 0f), new Vector2(420f, 56f), new Vector2(0f, 0.5f)),
             "@grp_survival", 34, KakPalette.CamgobegiParlak, TextAlignmentOptions.MidlineLeft);
        Text(Place(Rect(h, "Stats"), new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(420f, 40f), new Vector2(1f, 0.5f)),
             "", 22, KakPalette.Sis, TextAlignmentOptions.MidlineRight, false, false);
        var oldLine = h.Find("Line");
        if (oldLine != null) Object.DestroyImmediate(oldLine.gameObject);
        return h;
    }

    static UpgradeRow BuildRowTemplate(RectTransform content, Sprite white, Sprite coinSprite)
    {
        var r = Rect(content, "RowTemplate");
        var le = GetOrAdd<LayoutElement>(r.gameObject);
        le.preferredHeight = 150f; le.minHeight = 150f;
        var bg = Img(r, S("btn_stone_9s.png"), true);

        var icon = Img(Place(Rect(r, "Icon"), new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(96f, 96f)), null, false);
        var lockI = Img(Place(Rect(r, "Lock"), new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(64f, 64f)), S("icon_lock.png"), false, KakPalette.ArduvazAcik);
        var name = Text(Place(Rect(r, "Name"), new Vector2(0f, 0.5f), new Vector2(150f, 34f), new Vector2(450f, 50f), new Vector2(0f, 0.5f)),
                        "Can", 38, KakPalette.Krem, TextAlignmentOptions.MidlineLeft);
        var desc = Text(Place(Rect(r, "Desc"), new Vector2(0f, 0.5f), new Vector2(150f, -6f), new Vector2(460f, 40f), new Vector2(0f, 0.5f)),
                        "+1 kalp", 25, KakPalette.Sis, TextAlignmentOptions.MidlineLeft, false, false);
        desc.textWrappingMode = TextWrappingModes.Normal;
        desc.enableAutoSizing = true; desc.fontSizeMin = 18f; desc.fontSizeMax = 25f;
        var pips = new Image[5];
        for (int i = 0; i < pips.Length; i++)
        {
            pips[i] = Img(Place(Rect(r, "Pip" + i), new Vector2(0f, 0.5f), new Vector2(150f + i * 42f, -48f), new Vector2(34f, 14f), new Vector2(0f, 0.5f)), white, false);
            pips[i].preserveAspect = false;
        }

        var btn = Button(Place(Rect(r, "Upgrade"), new Vector2(1f, 0.5f), new Vector2(-18f, -8f), new Vector2(210f, 96f), new Vector2(1f, 0.5f)),
                         "60", Style.Gold, 38);
        var coin = Img(Place(Rect(btn.transform, "Coin"), new Vector2(0.5f, 0.5f), new Vector2(-58f, 0f), new Vector2(38f, 38f)), coinSprite, false);
        var price = btn.transform.Find("Label").GetComponent<TMP_Text>();
        price.rectTransform.offsetMin = new Vector2(34f, 0f);

        // K5: parşömen maliyeti (düğmenin altında)
        // düğmenin üst kenarına yapışık küçük rozet (satırın üst payında)
        var sbox = Place(Rect(r, "Scroll"), new Vector2(1f, 1f), new Vector2(-26f, -1f), new Vector2(96f, 30f), new Vector2(1f, 1f));
        Img(sbox, S("btn_stone_9s.png"), true);
        Img(Place(Rect(sbox, "Icon"), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(26f, 26f)), S("icon_scroll.png"), false);
        var scost = Text(Place(Rect(sbox, "Amount"), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(56f, 30f), new Vector2(0f, 0.5f)),
                         "2", 24, KakPalette.Krem, TextAlignmentOptions.MidlineLeft);
        sbox.gameObject.SetActive(false);
        var plus = Text(Place(Rect(r, "Plus"), new Vector2(0f, 0.5f), new Vector2(80f, 40f), new Vector2(140f, 56f)), "+1", 44, KakPalette.AltinAcik);
        plus.gameObject.SetActive(false);
        var fresh = Text(Place(Rect(r, "New"), new Vector2(1f, 1f), new Vector2(-250f, -20f), new Vector2(130f, 40f)), "@new_badge", 26, KakPalette.AltinAcik);
        fresh.gameObject.SetActive(false);
        var burstRt = Place(Rect(r, "Burst"), new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(10f, 10f));
        var burst = GetOrAdd<UiBurst>(burstRt.gameObject);
        burst.sprite = white;

        var row = GetOrAdd<UpgradeRow>(r.gameObject);
        row.background = bg; row.icon = icon; row.lockIcon = lockI; row.coin = coin;
        row.nameText = name; row.descText = desc; row.priceText = price; row.plusText = plus; row.newText = fresh;
        row.scrollBox = sbox.gameObject; row.scrollCostText = scost;
        row.pips = pips; row.button = btn; row.buttonBackground = btn.GetComponent<Image>(); row.burst = burst;
        row.goldSprite = S("btn_gold_9s.png"); row.stoneSprite = S("btn_stone_9s.png");
        OnClick(btn, row.Click);
        EditorUtility.SetDirty(row);
        return row;
    }

    static InfoToast BuildToast(Transform canvas, Sprite white)
    {
        var root = Stretch(Rect(canvas, "InfoToast"));
        var box = Place(Rect(root, "Box"), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(940f, 120f));
        Img(box, S("panel_9s.png"), true);
        var accent = Img(Place(Rect(box, "Accent"), new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(14f, 72f)), white, false, KakPalette.Altin);
        accent.preserveAspect = false;
        var text = Text(Place(Rect(box, "Text"), new Vector2(0.5f, 0.5f), new Vector2(20f, 0f), new Vector2(840f, 90f)), "", 34, KakPalette.Krem);
        text.textWrappingMode = TextWrappingModes.Normal;
        text.enableAutoSizing = true; text.fontSizeMin = 22f; text.fontSizeMax = 34f;
        var t = GetOrAdd<InfoToast>(root.gameObject);
        t.box = box; t.text = text; t.accent = accent;
        EditorUtility.SetDirty(t);
        return t;
    }
}
