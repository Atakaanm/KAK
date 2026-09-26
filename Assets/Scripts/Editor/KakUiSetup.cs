using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static KakUiKit;

/// <summary>
/// Faz 4 arayüzü kurar (tekrar çalıştırılabilir):
///  GameUi: HUD yazı stili, pause butonu, pause paneli (ayarlarla), oyun sonu ekranı
///  Menu:   ana menü (canlı arena arka planı, logo, istatistik, OYNA/KARAKTER/AYARLAR/BÖLÜMLER, paneller)
/// Menü: KacAtaKac/Arayüzü Kur (Faz 4)
/// </summary>
public static class KakUiSetup
{
    const string GameScene = KakEditorUtil.GameScenePath;
    const string MenuScene = "Assets/Scenes/MainMenu.unity";
    const string FramePrefab = "Assets/Prefabs/DungeonFrame.prefab";

    [MenuItem("KacAtaKac/Arayüzü Kur (Faz 4)")]
    public static void SetupMenuItem() => Debug.Log(SetupAll());

    public static string SetupAll()
    {
        if (Application.isPlaying) return "HATA: Play modunda çalıştırılamaz.";
        var sb = new StringBuilder("[KakUiSetup] ");
        sb.Append(SetupGameUi()).Append(' ');
        sb.Append(SetupMenu());
        AssetDatabase.SaveAssets();
        return sb.ToString();
    }

    static UnityEngine.SceneManagement.Scene Open(string path)
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path == path) return scene;
        KakEditorUtil.SaveNamedScenes();
        return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    // ════════════════════════════ OYUN SAHNESİ ════════════════════════════
    public static string SetupGameUi()
    {
        var scene = Open(GameScene);
        var canvas = GameObject.Find("HUDCanvas").transform;

        // DungeonFrame prefab'ı (menü de kullanır)
        var frameGo = GameObject.Find("DungeonFrame");
        if (frameGo != null) PrefabUtility.SaveAsPrefabAssetAndConnect(frameGo, FramePrefab, InteractionMode.AutomatedAction);

        // HUD yazıları
        foreach (var name in new[] { "ScoreText", "ComboText", "EventBanner" })
        {
            var t = FindDeep(canvas, name)?.GetComponent<TMP_Text>();
            if (t == null) continue;
            t.font = Nunito;
            t.fontSharedMaterial = NunitoOutline;
            t.color = name == "EventBanner" ? t.color : KakPalette.Krem;
        }

        // Pause butonu: taş levha + ikon
        var pauseBtn = FindDeep(canvas, "PauseButton") as RectTransform;
        if (pauseBtn != null)
        {
            var txt = pauseBtn.Find("Text");
            if (txt != null) Undo.DestroyObjectImmediate(txt.gameObject);
            Img(pauseBtn, S("btn_stone_9s.png"), true, null, true);
            var icon = Place(Rect(pauseBtn, "Icon"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(46f, 46f));
            Img(icon, S("icon_pause.png"), false, KakPalette.Krem);
            if (pauseBtn.GetComponent<ButtonScaleAnimation>() == null) pauseBtn.gameObject.AddComponent<ButtonScaleAnimation>();
        }

        // ── Pause paneli ──
        var pm = Object.FindAnyObjectByType<PauseManager>();
        var pausePanelT = FindDeep(canvas, "PausePanel") as RectTransform;
        if (pausePanelT != null) DestroyChildrenExcept(pausePanelT);
        var (proot, ppanel) = Modal(canvas, "PausePanel", new Vector2(820f, 1180f));
        var ptitle = Place(Rect(ppanel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(760f, 120f));
        Text(ptitle, "@paused", 76, KakPalette.Altin);
        ToggleRow(ppanel, "MusicRow", "@music", KakToggle.Setting.Music, -250f);
        ToggleRow(ppanel, "SfxRow", "@sfx", KakToggle.Setting.Sfx, -370f);
        ToggleRow(ppanel, "ShakeRow", "@shake", KakToggle.Setting.ScreenShake, -490f);
        var resume = Button(Place(Rect(ppanel, "ResumeButton"), new Vector2(0.5f, 0f), new Vector2(0f, 470f), new Vector2(620f, 150f)), "@resume", Style.Gold, 64, "icon_play.png");
        var restart = Button(Place(Rect(ppanel, "RestartButton"), new Vector2(0.5f, 0f), new Vector2(0f, 300f), new Vector2(620f, 130f)), "@restart", Style.Stone, 52);
        var menu = Button(Place(Rect(ppanel, "MenuButton"), new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(620f, 130f)), "@main_menu", Style.Stone, 52);
        proot.SetAsLastSibling();
        if (pm != null)
        {
            Undo.RecordObject(pm, "pause");
            pm.pausePanel = proot.gameObject;
            pm.resumeButton = resume;
            pm.restartButton = restart;
            pm.mainMenuButton = menu;
            if (pauseBtn != null) pm.pauseButton = pauseBtn.GetComponent<Button>();
            EditorUtility.SetDirty(pm);
        }
        proot.gameObject.SetActive(false);

        // ── Oyun sonu ekranı ──
        var gm = Object.FindAnyObjectByType<GameManager>();
        var oldGo = FindDeep(canvas, "GameOverPanel") as RectTransform;
        if (oldGo != null) DestroyChildrenExcept(oldGo);
        var (groot, gpanel) = Modal(canvas, "GameOverPanel", new Vector2(860f, 1240f));
        var dimGroup = KakUiKit.GetOrAdd<CanvasGroup>(groot.Find("Dim").gameObject);
        var title = Place(Rect(gpanel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(800f, 120f));
        Text(title, "@game_over", 84, KakPalette.Tehlike);
        var sl = Place(Rect(gpanel, "ScoreLabel"), new Vector2(0.5f, 1f), new Vector2(0f, -230f), new Vector2(600f, 60f));
        Text(sl, "@score", 44, KakPalette.ArduvazAcik, TextAlignmentOptions.Center, false, false);
        var sv = Place(Rect(gpanel, "ScoreValue"), new Vector2(0.5f, 1f), new Vector2(0f, -350f), new Vector2(760f, 170f));
        var scoreT = Text(sv, "0", 150, KakPalette.Krem);
        var badge = Place(Rect(gpanel, "NewBest"), new Vector2(0.5f, 1f), new Vector2(0f, -490f), new Vector2(520f, 96f));
        Img(badge, S("badge_9s.png"), true);
        Text(Stretch(Rect(badge, "Label")), "@new_best", 50, KakPalette.AltinAcik);
        var bt = Place(Rect(gpanel, "BestText"), new Vector2(0.5f, 1f), new Vector2(0f, -600f), new Vector2(760f, 70f));
        var bestT = Text(bt, "EN İYİ 0", 50, KakPalette.Altin);
        var st = Place(Rect(gpanel, "StatsText"), new Vector2(0.5f, 1f), new Vector2(0f, -690f), new Vector2(800f, 60f));
        var statsT = Text(st, "", 36, KakPalette.Sis, TextAlignmentOptions.Center, false, false);
        var btns = Place(Rect(gpanel, "Buttons"), new Vector2(0.5f, 0f), new Vector2(0f, 230f), new Vector2(760f, 380f));
        var cg = KakUiKit.GetOrAdd<CanvasGroup>(btns.gameObject);
        var retry = Button(Place(Rect(btns, "RetryButton"), new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(640f, 160f)), "@retry", Style.Gold, 68, "icon_play.png");
        var toMenu = Button(Place(Rect(btns, "MenuButton"), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(640f, 130f)), "@main_menu", Style.Stone, 52);
        var gos = KakUiKit.GetOrAdd<GameOverScreen>(groot.gameObject);
        gos.panel = gpanel; gos.scoreText = scoreT; gos.bestText = bestT; gos.statsText = statsT;
        gos.newBestBadge = badge; gos.buttons = cg; gos.dim = dimGroup;
        BuildCoinsRow(gpanel, gos);
        BuildUnlockBanner(gpanel, gos);
        BuildMissionsBlock(gpanel, gos);
        BuildPolish(gpanel, gos);
        BuildContinuePanel(canvas, gm);
        if (gm != null)
        {
            OnClick(retry, gm.RetryGame);
            OnClick(toMenu, gm.GoToMainMenu);
            Undo.RecordObject(gm, "gm ui");
            gm.gameOverPanel = groot.gameObject;
            gm.gameOverScreen = gos;
            gm.finalScoreText = scoreT;
            gm.bestScoreText = bestT;
            EditorUtility.SetDirty(gm);
        }
        EditorUtility.SetDirty(gos);
        groot.SetAsLastSibling();
        groot.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Oyun arayüzü kuruldu.";
    }

    // ════════════════════════════ ANA MENÜ ════════════════════════════
    public static string SetupMenu()
    {
        var scene = Open(MenuScene);
        var canvasGo = GameObject.Find("Canvas");
        var canvas = canvasGo.transform;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f;
        }

        // Eski öğeler
        foreach (var name in new[] { "Background", "TitleText", "PlayButton", "Text (TMP)" })
        {
            var t = canvas.Find(name);
            if (t != null) Undo.DestroyObjectImmediate(t.gameObject);
        }
        var rootImg = canvasGo.GetComponent<Image>();
        if (rootImg != null) rootImg.enabled = false;

        // ── Canlı arka plan (dünya) ──
        var cam = Camera.main;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.03f, 0.045f, 0.05f, 1f);
        var bd = GameObject.Find("MenuBackdrop");
        if (bd == null) { bd = new GameObject("MenuBackdrop"); Undo.RegisterCreatedObjectUndo(bd, "MenuBackdrop"); }
        var arenaT = bd.transform.Find("Arena");
        if (arenaT == null) { arenaT = new GameObject("Arena").transform; arenaT.SetParent(bd.transform, false); }
        var arenaSr = KakUiKit.GetOrAdd<SpriteRenderer>(arenaT.gameObject);
        arenaSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Arena/Dungeon_arena_01.png");
        arenaSr.sortingOrder = -100;
        arenaT.localScale = new Vector3(0.4f, 0.4f, 1f);
        arenaT.localPosition = Vector3.zero;
        var frameT = bd.transform.Find("DungeonFrame");
        if (frameT == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FramePrefab);
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, bd.transform);
                inst.name = "DungeonFrame";
                frameT = inst.transform;
            }
        }
        var mb = KakUiKit.GetOrAdd<MenuBackdrop>(bd);
        mb.cam = cam; mb.arena = arenaSr; mb.frame = frameT != null ? frameT.GetComponent<DungeonFrame>() : null;
        mb.rockSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Projectiles/rock.png");
        EditorUtility.SetDirty(mb);

        // ── UI ──
        var safe = Stretch(Rect(canvas, "Safe"));
        if (safe.GetComponent<SafeAreaFitter>() == null) safe.gameObject.AddComponent<SafeAreaFitter>();
        var shade = Stretch(Rect(safe, "Shade"));
        Img(shade, S("white_ui.png"), false, KakPalette.WithAlpha(KakPalette.Murekkep, 0.35f)).preserveAspect = false;

        var logo = Place(Rect(safe, "Logo"), new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(1040f, 220f));
        Text(logo, "KAÇ ATA KAÇ", 132, KakPalette.Altin, TextAlignmentOptions.Center, true);
        var sub = Place(Rect(safe, "Subtitle"), new Vector2(0.5f, 1f), new Vector2(0f, -385f), new Vector2(900f, 60f));
        Text(sub, "@subtitle", 42, KakPalette.Krem, TextAlignmentOptions.Center);
        var best = Place(Rect(safe, "BestScore"), new Vector2(0.5f, 1f), new Vector2(0f, -475f), new Vector2(900f, 80f));
        var bestT = Text(best, "EN İYİ 0", 62, KakPalette.AltinAcik);
        var stats = Place(Rect(safe, "Stats"), new Vector2(0.5f, 1f), new Vector2(0f, -550f), new Vector2(900f, 56f));
        var statsT = Text(stats, "", 38, KakPalette.Sis, TextAlignmentOptions.Center, false, false);

        var play = Button(Place(Rect(safe, "PlayButton"), new Vector2(0.5f, 0f), new Vector2(0f, 560f), new Vector2(680f, 200f)), "@play", Style.Gold, 96, "icon_play.png");
        var chars = Button(Place(Rect(safe, "CharactersButton"), new Vector2(0.5f, 0f), new Vector2(-175f, 360f), new Vector2(330f, 150f)), "@character", Style.Stone, 38, "icon_character.png");
        var sett = Button(Place(Rect(safe, "SettingsButton"), new Vector2(0.5f, 0f), new Vector2(175f, 360f), new Vector2(330f, 150f)), "@settings", Style.Stone, 38, "icon_settings.png");
        // 2. satır: PET (Faz 3c.5, kilitli/YENİ) + BÖLÜMLER (yakında)
        var pets = Button(Place(Rect(safe, "PetsButton"), new Vector2(0.5f, 0f), new Vector2(-175f, 190f), new Vector2(330f, 150f)), "@pet_button", Style.Stone, 38, "icon_pet.png");
        var levels = Button(Place(Rect(safe, "LevelsButton"), new Vector2(0.5f, 0f), new Vector2(175f, 190f), new Vector2(330f, 150f)), "@levels", Style.Stone, 38, "icon_lock.png");
        levels.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.75f, 1f);
        Text(Place(Rect((RectTransform)levels.transform, "LockHint"), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(300f, 34f)),
             "@soon", 26, KakPalette.Sis, TextAlignmentOptions.Center, false, false);
        var petsFeature = BuildFeatureButton(pets, Feature.Pets);
        var charsFeature = BuildFeatureButton(chars, Feature.Characters);
        var wallet = BuildWallet(safe);

        // Ayarlar paneli
        var (sroot, spanel) = Modal(canvas, "SettingsPanel", new Vector2(860f, 1400f));
        Text(Place(Rect(spanel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(800f, 120f)), "@settings", 80, KakPalette.Altin);
        ToggleRow(spanel, "MusicRow", "@music", KakToggle.Setting.Music, -250f);
        ToggleRow(spanel, "SfxRow", "@sfx", KakToggle.Setting.Sfx, -370f);
        ToggleRow(spanel, "VibrationRow", "@vibration", KakToggle.Setting.Vibration, -490f);
        ToggleRow(spanel, "ShakeRow", "@shake", KakToggle.Setting.ScreenShake, -610f);
        LanguageRow(spanel, -740f);
        var reset = Button(Place(Rect(spanel, "ResetButton"), new Vector2(0.5f, 0f), new Vector2(0f, 330f), new Vector2(700f, 120f)), "@reset", Style.Stone, 40);
        var privacy = Button(Place(Rect(spanel, "PrivacyButton"), new Vector2(0.5f, 0f), new Vector2(0f, 480f), new Vector2(560f, 96f)), "@privacy", Style.Stone, 34);
        var closeS = Button(Place(Rect(spanel, "CloseButton"), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(620f, 150f)), "@close", Style.Gold, 64);

        // Karakter paneli (Faz 3c.3): katalogdaki karakterler, satın al / seç
        var (croot, closeC) = BuildCharacterPanel(canvas, wallet);
        var (proot, closeP) = BuildPetPanel(canvas, wallet);
        var daily = BuildDailyPanel(canvas, wallet);

        // Kontrolcü bağlantıları
        var mmc = Object.FindAnyObjectByType<MainMenuController>();
        if (mmc != null)
        {
            Undo.RecordObject(mmc, "menu");
            mmc.playButton = play;
            mmc.charactersButton = chars;
            mmc.settingsButton = sett;
            mmc.levelsButton = levels;
            mmc.charactersFeature = charsFeature;
            mmc.petsFeature = petsFeature;
            mmc.petsPanel = proot.gameObject;
            mmc.dailyPanel = daily;
            OnClick(pets, mmc.OnPetsClicked);
            OnClick(closeP, mmc.ClosePetsPanel);
            mmc.settingsPanel = sroot.gameObject;
            mmc.charactersPanel = croot.gameObject;
            mmc.bestScoreText = bestT;
            mmc.statsText = statsT;
            mmc.resetProgressButton = reset;
            mmc.resetProgressLabel = reset.transform.Find("Label").GetComponent<TMP_Text>();
            if (mmc.defaultLevel == null) mmc.defaultLevel = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Endless_Level1_LevelData.asset");
            OnClick(closeS, mmc.CloseSettingsPanel);
            OnClick(privacy, mmc.OnPrivacyClicked);
            OnClick(closeC, mmc.CloseCharactersPanel);
            EditorUtility.SetDirty(mmc);
        }

        // Logo süzülmesi
        var mbg = KakUiKit.GetOrAdd<MenuBackground>(canvasGo);
        mbg.titleTransform = logo;
        mbg.floatAmplitude = 10f;
        mbg.particles = new RectTransform[0];

        sroot.SetAsLastSibling();
        croot.SetAsLastSibling();
        proot.SetAsLastSibling();
        proot.gameObject.SetActive(false);
        daily.transform.SetAsLastSibling();
        daily.gameObject.SetActive(false);
        sroot.gameObject.SetActive(false);
        croot.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        // Oyun sahnesine geri dön (varsayılan çalışma sahnesi)
        EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
        return "Ana menü kuruldu.";
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    /// <summary>Oyun sonu paneline altın satırı (ikon + "+12 ALTIN • TOPLAM 340"). KakMetaSetup da çağırır.</summary>
    public static void BuildCoinsRow(RectTransform gpanel, GameOverScreen gos)
    {
        var row = Place(Rect(gpanel, "CoinsRow"), new Vector2(0.5f, 1f), new Vector2(0f, -765f), new Vector2(760f, 64f));
        var icon = Img(Place(Rect(row, "Icon"), new Vector2(0.5f, 0.5f), new Vector2(-300f, 0f), new Vector2(52f, 52f)),
                       AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png"), false);
        var text = Text(Place(Rect(row, "Text"), new Vector2(0.5f, 0.5f), new Vector2(40f, 0f), new Vector2(620f, 64f)),
                        "+0", 40, KakPalette.AltinAcik, TextAlignmentOptions.MidlineLeft);
        var dbl = Button(Place(Rect(row, "DoubleCoins"), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(150f, 60f), new Vector2(1f, 0.5f)),
                         "@double_coins", Style.Gold, 34, "icon_play.png");
        OnClick(dbl, gos.OnDoubleCoins);
        dbl.gameObject.SetActive(false);
        gos.doubleCoinsButton = dbl;
        gos.coinsRow = row;
        gos.coinsText = text;
        row.gameObject.SetActive(false); // GameOverScreen altın açıksa gösterir
        EditorUtility.SetDirty(gos);
    }

    /// <summary>Oyun sonu: yeni açılan özellik afişi (panelin hemen üstünde, rozet zemin).</summary>
    public static void BuildUnlockBanner(RectTransform gpanel, GameOverScreen gos)
    {
        var banner = Place(Rect(gpanel, "UnlockBanner"), new Vector2(0.5f, 1f), new Vector2(0f, 75f), new Vector2(780f, 110f));
        Img(banner, S("badge_9s.png"), true);
        var text = Text(Stretch(Rect(banner, "Label")), "YENİ AÇILDI", 46, KakPalette.AltinAcik);
        gos.unlockBanner = banner;
        gos.unlockText = text;
        banner.gameObject.SetActive(false);
        EditorUtility.SetDirty(gos);
    }

    /// <summary>Menü butonuna özellik kapısı: kilit ikonu + kalan koşul yazısı + "YENİ!" rozeti.</summary>
    public static FeatureButton BuildFeatureButton(Button button, Feature feature)
    {
        var rt = (RectTransform)button.transform;
        var hint = Text(Place(Rect(rt, "LockHint"), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(300f, 34f)),
                        "", 26, KakPalette.Sis, TextAlignmentOptions.Center, false, false);
        var badge = Place(Rect(rt, "NewBadge"), new Vector2(1f, 1f), new Vector2(-20f, -4f), new Vector2(150f, 58f));
        badge.localRotation = Quaternion.Euler(0f, 0f, -8f);
        Img(badge, S("badge_9s.png"), true);
        Text(Stretch(Rect(badge, "Label")), "@new_badge", 30, KakPalette.AltinAcik);
        var dot = Img(Place(Rect(rt, "BuyDot"), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(38f, 38f)),
                      AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png"), false);
        var fb = GetOrAdd<FeatureButton>(rt.gameObject);
        fb.feature = feature;
        fb.buyDot = dot.rectTransform;
        var icon = rt.Find("Icon") != null ? rt.Find("Icon").GetComponent<Image>() : null;
        var label = rt.Find("Label") != null ? rt.Find("Label").GetComponent<TMP_Text>() : null;
        fb.tintTargets = new Graphic[] { button.GetComponent<Image>(), icon, label };
        fb.icon = icon;
        fb.lockSprite = S("icon_lock.png");
        fb.lockHint = hint;
        fb.newBadge = badge;
        EditorUtility.SetDirty(fb);
        return fb;
    }

    /// <summary>Menü sol üst: cüzdan (altın ikonu + miktar).</summary>
    public static WalletHud BuildWallet(RectTransform safe)
    {
        var root = Place(Rect(safe, "Wallet"), new Vector2(0f, 1f), new Vector2(40f, -60f), new Vector2(300f, 80f), new Vector2(0f, 0.5f));
        var content = Stretch(Rect(root, "Content"));
        Img(Place(Rect(content, "Icon"), new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(60f, 60f)),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png"), false);
        var amount = Text(Place(Rect(content, "Amount"), new Vector2(0f, 0.5f), new Vector2(170f, 0f), new Vector2(200f, 80f)),
                          "0", 54, KakPalette.AltinAcik, TextAlignmentOptions.MidlineLeft);
        var w = GetOrAdd<WalletHud>(root.gameObject);
        w.content = content;
        w.amount = amount;
        EditorUtility.SetDirty(w);
        return w;
    }

    /// <summary>Karakter paneli: başlık, cüzdan, 2x2 kart ızgarası, kapat. Kartları CharacterPanel çalışma anında doldurur.</summary>
    public static (RectTransform root, Button close) BuildCharacterPanel(Transform canvas, WalletHud menuWallet)
    {
        var old = canvas.Find("CharactersPanel");
        if (old != null) Object.DestroyImmediate(old.gameObject); // eski sahte panel / yeniden kurulum
        var (croot, cpanel) = Modal(canvas, "CharactersPanel", new Vector2(960f, 1560f));
        Text(Place(Rect(cpanel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(800f, 110f)), "@character", 78, KakPalette.Altin);
        var wrow = Place(Rect(cpanel, "Wallet"), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(360f, 64f));
        Img(Place(Rect(wrow, "Icon"), new Vector2(0.5f, 0.5f), new Vector2(-70f, 0f), new Vector2(52f, 52f)),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png"), false);
        var wtext = Text(Place(Rect(wrow, "Amount"), new Vector2(0.5f, 0.5f), new Vector2(40f, 0f), new Vector2(200f, 64f)),
                         "0", 50, KakPalette.AltinAcik, TextAlignmentOptions.MidlineLeft);

        var heart = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Kalp.png");
        var white = S("white_ui.png");
        var cards = new CharacterCard[4];
        for (int i = 0; i < 4; i++)
        {
            float x = (i % 2 == 0) ? -225f : 225f, y = (i < 2) ? -510f : -1070f;
            var card = Place(Rect(cpanel, "Card" + i), new Vector2(0.5f, 1f), new Vector2(x, y), new Vector2(430f, 540f));
            var bg = Img(card, S("btn_stone_9s.png"), true);
            var portrait = Img(Place(Rect(card, "Portrait"), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(192f, 192f)), null, false);
            var name = Text(Place(Rect(card, "Name"), new Vector2(0.5f, 1f), new Vector2(0f, -258f), new Vector2(400f, 56f)), "ATA", 44, KakPalette.Krem);
            var trait = Text(Place(Rect(card, "Trait"), new Vector2(0.5f, 1f), new Vector2(0f, -302f), new Vector2(410f, 40f)), "", 24, KakPalette.Sis,
                             TextAlignmentOptions.Center, false, false);
            var hearts = new Image[5];
            for (int h = 0; h < 5; h++)
                hearts[h] = Img(Place(Rect(card, "Heart" + h), new Vector2(0.5f, 1f), new Vector2((h - 2) * 50f, -348f), new Vector2(48f, 48f)), heart, false);
            var speed = Bar(card, "Speed", "@stat_speed", -392f, KakPalette.Camgobegi, white);
            var dash = Bar(card, "Dash", "@stat_dash", -428f, KakPalette.CamgobegiParlak, white);
            var btn = Button(Place(Rect(card, "Action"), new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(360f, 88f)), "SEÇ", Style.Stone, 40);
            var coin = Img(Place(Rect(btn.transform, "Coin"), new Vector2(0.5f, 0.5f), new Vector2(-95f, 0f), new Vector2(42f, 42f)),
                           AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png"), false);

            var cc = GetOrAdd<CharacterCard>(card.gameObject);
            cc.background = bg; cc.portrait = portrait; cc.nameText = name; cc.traitText = trait; cc.hearts = hearts;
            cc.speedFill = speed; cc.dashFill = dash; cc.actionButton = btn; cc.actionBackground = btn.GetComponent<Image>();
            cc.actionText = btn.transform.Find("Label").GetComponent<TMP_Text>(); cc.actionCoin = coin;
            cc.goldSprite = S("btn_gold_9s.png"); cc.stoneSprite = S("btn_stone_9s.png");
            cc.softTexts = new TMP_Text[] { trait, card.Find("SpeedLabel").GetComponent<TMP_Text>(), card.Find("DashLabel").GetComponent<TMP_Text>() };
            EditorUtility.SetDirty(cc);
            cards[i] = cc;
        }
        var close = Button(Place(Rect(cpanel, "CloseButton"), new Vector2(0.5f, 0f), new Vector2(0f, 95f), new Vector2(560f, 120f)), "@close", Style.Gold, 58);

        var panel = GetOrAdd<CharacterPanel>(croot.gameObject);
        panel.cards = cards; panel.walletText = wtext; panel.menuWallet = menuWallet;
        EditorUtility.SetDirty(panel);
        return (croot, close);
    }

    /// <summary>Etiketli yatay gösterge çubuğu (zemin + dolum). Dolum Image'ını döndürür.</summary>
    static Image Bar(RectTransform card, string name, string label, float y, Color color, Sprite white)
    {
        Text(Place(Rect(card, name + "Label"), new Vector2(0.5f, 1f), new Vector2(-135f, y), new Vector2(120f, 30f)), label, 22, KakPalette.Sis,
             TextAlignmentOptions.MidlineLeft, false, false);
        var bg = Img(Place(Rect(card, name + "Bar"), new Vector2(0.5f, 1f), new Vector2(50f, y), new Vector2(240f, 16f)), white, false,
                     KakPalette.Gece);
        bg.preserveAspect = false;
        var fill = Img(Stretch(Rect(bg.rectTransform, "Fill")), white, false, color);
        fill.preserveAspect = false;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0.6f;
        return fill;
    }

    /// <summary>Oyun sonu görev bloğu: başlık + 3 satır (onay, metin, ilerleme/ödül). Altın satırının altında.</summary>
    public static void BuildMissionsBlock(RectTransform gpanel, GameOverScreen gos)
    {
        var block = Place(Rect(gpanel, "Missions"), new Vector2(0.5f, 1f), new Vector2(0f, -975f), new Vector2(780f, 230f));
        Text(Place(Rect(block, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(760f, 36f)), "@missions_title", 30,
             KakPalette.ArduvazAcik, TextAlignmentOptions.Center, false, false);
        var texts = new TMP_Text[3];
        var progress = new TMP_Text[3];
        var checks = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var row = Place(Rect(block, "Row" + i), new Vector2(0.5f, 1f), new Vector2(0f, -68f - i * 56f), new Vector2(780f, 52f));
            Img(row, S("white_ui.png"), false, KakPalette.WithAlpha(KakPalette.Murekkep, 0.35f)).preserveAspect = false;
            checks[i] = Img(Place(Rect(row, "Check"), new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(34f, 34f)), S("icon_check.png"), false);
            texts[i] = Text(Place(Rect(row, "Text"), new Vector2(0f, 0.5f), new Vector2(64f, 0f), new Vector2(560f, 52f), new Vector2(0f, 0.5f)),
                            "", 30, KakPalette.Krem, TextAlignmentOptions.MidlineLeft, false, false);
            progress[i] = Text(Place(Rect(row, "Progress"), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(160f, 52f), new Vector2(1f, 0.5f)),
                               "0/0", 32, KakPalette.Sis, TextAlignmentOptions.MidlineRight);
        }
        gos.missionsBlock = block;
        gos.missionTexts = texts;
        gos.missionProgress = progress;
        gos.missionChecks = checks;
        block.gameObject.SetActive(false);
        EditorUtility.SetDirty(gos);
    }

    /// <summary>Pet paneli: başlık, cüzdan, 2 kart (büyük piksel portre, ad, pasif, SEÇ/ÇIKAR/fiyat), kapat.</summary>
    public static (RectTransform root, Button close) BuildPetPanel(Transform canvas, WalletHud menuWallet)
    {
        var old = canvas.Find("PetsPanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var (root, panel) = Modal(canvas, "PetsPanel", new Vector2(900f, 1120f));
        Text(Place(Rect(panel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(800f, 110f)), "@pets_title", 78, KakPalette.Altin);
        var wrow = Place(Rect(panel, "Wallet"), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(360f, 64f));
        Img(Place(Rect(wrow, "Icon"), new Vector2(0.5f, 0.5f), new Vector2(-70f, 0f), new Vector2(52f, 52f)),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png"), false);
        var wtext = Text(Place(Rect(wrow, "Amount"), new Vector2(0.5f, 0.5f), new Vector2(40f, 0f), new Vector2(200f, 64f)),
                         "0", 50, KakPalette.AltinAcik, TextAlignmentOptions.MidlineLeft);
        var cards = new PetPanel.Card[2];
        for (int i = 0; i < 2; i++)
        {
            var card = Place(Rect(panel, "Card" + i), new Vector2(0.5f, 1f), new Vector2(i == 0 ? -210f : 210f, -560f), new Vector2(400f, 600f));
            var bg = Img(card, S("btn_stone_9s.png"), true);
            var portrait = Img(Place(Rect(card, "Portrait"), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(160f, 160f)), null, false);
            var name = Text(Place(Rect(card, "Name"), new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(380f, 56f)), "PET", 42, KakPalette.Krem);
            var trait = Text(Place(Rect(card, "Trait"), new Vector2(0.5f, 1f), new Vector2(0f, -345f), new Vector2(380f, 70f)), "", 26, KakPalette.Sis,
                             TextAlignmentOptions.Center, false, false);
            trait.textWrappingMode = TextWrappingModes.Normal;
            var btn = Button(Place(Rect(card, "Action"), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(340f, 90f)), "SEÇ", Style.Stone, 40);
            var coin = Img(Place(Rect(btn.transform, "Coin"), new Vector2(0.5f, 0.5f), new Vector2(-95f, 0f), new Vector2(42f, 42f)),
                           AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png"), false);
            cards[i] = new PetPanel.Card
            {
                root = card, background = bg, portrait = portrait, nameText = name, traitText = trait,
                actionButton = btn, actionBackground = btn.GetComponent<Image>(), actionText = btn.transform.Find("Label").GetComponent<TMP_Text>(), actionCoin = coin
            };
        }
        var close = Button(Place(Rect(panel, "CloseButton"), new Vector2(0.5f, 0f), new Vector2(0f, 95f), new Vector2(560f, 120f)), "@close", Style.Gold, 58);
        var pp = GetOrAdd<PetPanel>(root.gameObject);
        pp.cards = cards; pp.walletText = wtext; pp.menuWallet = menuWallet;
        pp.goldSprite = S("btn_gold_9s.png"); pp.stoneSprite = S("btn_stone_9s.png");
        EditorUtility.SetDirty(pp);
        return (root, close);
    }

    /// <summary>Günlük ödül paneli: 7 gün kutusu (4 + 3, 7. gün çift genişlik), AL/KAPAT.</summary>
    public static DailyRewardPanel BuildDailyPanel(Transform canvas, WalletHud menuWallet)
    {
        var old = canvas.Find("DailyPanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var (root, panel) = Modal(canvas, "DailyPanel", new Vector2(920f, 1000f));
        Text(Place(Rect(panel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(860f, 110f)), "@daily_title", 72, KakPalette.Altin);
        Text(Place(Rect(panel, "Hint"), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(860f, 50f)), "@daily_hint", 30, KakPalette.Sis,
             TextAlignmentOptions.Center, false, false);
        var coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Pickups/coin_0.png");
        var tiles = new DailyRewardPanel.Tile[7];
        for (int i = 0; i < 7; i++)
        {
            bool big = i == 6;
            int row = i < 4 ? 0 : 1, col = i < 4 ? i : i - 4;
            float w = big ? 400f : 190f;
            float x = row == 0 ? -300f + col * 200f : (col == 0 ? -300f : col == 1 ? -100f : 200f);
            var tile = Place(Rect(panel, "Day" + (i + 1)), new Vector2(0.5f, 1f), new Vector2(x, row == 0 ? -380f : -640f), new Vector2(w, 230f));
            var bg = Img(tile, S("btn_stone_9s.png"), true);
            var day = Text(Place(Rect(tile, "Day"), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(w - 10f, 40f)), "GÜN 1", 28, KakPalette.Sis,
                           TextAlignmentOptions.Center, false, false);
            Img(Place(Rect(tile, "Coin"), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(big ? 84f : 64f, big ? 84f : 64f)), coinSprite, false);
            var amount = Text(Place(Rect(tile, "Amount"), new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(w - 10f, 50f)), "0", big ? 44f : 38f,
                              KakPalette.AltinAcik);
            var check = Img(Place(Rect(tile, "Check"), new Vector2(1f, 1f), new Vector2(-18f, -18f), new Vector2(32f, 32f)), S("icon_check.png"), false,
                            KakPalette.AcikYesil);
            tiles[i] = new DailyRewardPanel.Tile { root = tile, background = bg, check = check, dayText = day, amountText = amount };
        }
        var claim = Button(Place(Rect(panel, "ClaimButton"), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(600f, 140f)), "AL", Style.Gold, 66);
        var dp = GetOrAdd<DailyRewardPanel>(root.gameObject);
        dp.tiles = tiles;
        dp.claimButton = claim;
        dp.claimLabel = claim.transform.Find("Label").GetComponent<TMP_Text>();
        dp.goldSprite = S("btn_gold_9s.png"); dp.stoneSprite = S("btn_stone_9s.png");
        dp.menuWallet = menuWallet;
        OnClick(claim, dp.OnClaim);
        EditorUtility.SetDirty(dp);
        return dp;
    }

    /// <summary>Faz 3c.7 cilası: sıradaki hedef satırı (çubuk + metin) ve yeni rekor konfetisi.</summary>
    public static void BuildPolish(RectTransform gpanel, GameOverScreen gos)
    {
        var row = Place(Rect(gpanel, "GoalRow"), new Vector2(0.5f, 1f), new Vector2(0f, -825f), new Vector2(760f, 40f));
        var white = S("white_ui.png");
        var bg = Img(Place(Rect(row, "Bar"), new Vector2(0f, 0.5f), new Vector2(150f, 0f), new Vector2(280f, 14f)), white, false, KakPalette.Gece);
        bg.preserveAspect = false;
        var fill = Img(Stretch(Rect(bg.rectTransform, "Fill")), white, false, KakPalette.Altin);
        fill.preserveAspect = false;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        var text = Text(Place(Rect(row, "Text"), new Vector2(0f, 0.5f), new Vector2(310f, 0f), new Vector2(440f, 40f), new Vector2(0f, 0.5f)),
                        "", 28, KakPalette.Sis, TextAlignmentOptions.MidlineLeft, false, false);
        gos.goalRow = row; gos.goalFill = fill; gos.goalText = text;
        row.gameObject.SetActive(false);

        // Konfeti: panelin üst yarısında (rozetin çevresi), panel içeriğinin üstünde
        var conf = Place(Rect(gpanel, "Confetti"), new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(860f, 10f));
        conf.SetAsLastSibling();
        var c = GetOrAdd<UiConfetti>(conf.gameObject);
        c.sprite = white;
        gos.confetti = c;
        EditorUtility.SetDirty(gos);
    }

    /// <summary>"Devam et?" paneli (reklamla canlanma, Faz Y3). GameManager.continuePanel'e bağlanır, kapalı başlar.</summary>
    public static ContinuePanel BuildContinuePanel(Transform canvas, GameManager gm)
    {
        var old = canvas.Find("ContinuePanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var (root, panel) = Modal(canvas, "ContinuePanel", new Vector2(780f, 860f));
        Text(Place(Rect(panel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(740f, 110f)), "@continue_title", 78, KakPalette.Altin);
        Text(Place(Rect(panel, "Sub"), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(740f, 50f)), "@continue_sub", 32, KakPalette.Krem,
             TextAlignmentOptions.Center, false, false);
        var ringBg = Img(Place(Rect(panel, "RingBg"), new Vector2(0.5f, 1f), new Vector2(0f, -340f), new Vector2(200f, 200f)), S("ring_soft.png"), false,
                         KakPalette.WithAlpha(KakPalette.Murekkep, 0.8f));
        var ring = Img(Place(Rect(panel, "Ring"), new Vector2(0.5f, 1f), new Vector2(0f, -340f), new Vector2(200f, 200f)), S("ring_soft.png"), false, KakPalette.Altin);
        ring.type = Image.Type.Filled; ring.fillMethod = Image.FillMethod.Radial360; ring.fillOrigin = (int)Image.Origin360.Top; ring.fillClockwise = false;
        Img(Place(Rect(panel, "Heart"), new Vector2(0.5f, 1f), new Vector2(0f, -340f), new Vector2(96f, 96f)),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Kalp.png"), false);
        var count = Text(Place(Rect(panel, "Count"), new Vector2(0.5f, 1f), new Vector2(0f, -470f), new Vector2(200f, 60f)), "5", 48, KakPalette.AltinAcik);
        var watch = Button(Place(Rect(panel, "WatchButton"), new Vector2(0.5f, 0f), new Vector2(0f, 250f), new Vector2(560f, 140f)), "@continue_watch", Style.Gold, 64, "icon_play.png");
        var no = Button(Place(Rect(panel, "DeclineButton"), new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(420f, 100f)), "@continue_no", Style.Stone, 40);
        var cp = GetOrAdd<ContinuePanel>(root.gameObject);
        cp.ring = ring; cp.countText = count; cp.watchButton = watch; cp.declineButton = no; cp.panel = panel;
        EditorUtility.SetDirty(cp);
        if (gm != null) { Undo.RecordObject(gm, "continue"); gm.continuePanel = cp; EditorUtility.SetDirty(gm); }
        root.SetAsLastSibling();
        root.gameObject.SetActive(false);
        return cp;
    }
}
