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
    const string GameScene = "Assets/Scenes/SampleScene.unity";
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
        var levels = Button(Place(Rect(safe, "LevelsButton"), new Vector2(0.5f, 0f), new Vector2(0f, 180f), new Vector2(680f, 130f)), "@levels_soon", Style.Stone, 40, "icon_lock.png");
        levels.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.75f, 1f);

        // Ayarlar paneli
        var (sroot, spanel) = Modal(canvas, "SettingsPanel", new Vector2(860f, 1400f));
        Text(Place(Rect(spanel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(800f, 120f)), "@settings", 80, KakPalette.Altin);
        ToggleRow(spanel, "MusicRow", "@music", KakToggle.Setting.Music, -250f);
        ToggleRow(spanel, "SfxRow", "@sfx", KakToggle.Setting.Sfx, -370f);
        ToggleRow(spanel, "VibrationRow", "@vibration", KakToggle.Setting.Vibration, -490f);
        ToggleRow(spanel, "ShakeRow", "@shake", KakToggle.Setting.ScreenShake, -610f);
        LanguageRow(spanel, -740f);
        var reset = Button(Place(Rect(spanel, "ResetButton"), new Vector2(0.5f, 0f), new Vector2(0f, 330f), new Vector2(700f, 120f)), "@reset", Style.Stone, 40);
        var closeS = Button(Place(Rect(spanel, "CloseButton"), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(620f, 150f)), "@close", Style.Gold, 64);

        // Karakter paneli
        var (croot, cpanel) = Modal(canvas, "CharactersPanel", new Vector2(900f, 1100f));
        Text(Place(Rect(cpanel, "Title"), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(800f, 120f)), "@character", 80, KakPalette.Altin);
        string[] names = { "ALİ", "?", "?" };
        for (int i = 0; i < 3; i++)
        {
            var card = Place(Rect(cpanel, "Card" + i), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 270f, 40f), new Vector2(240f, 340f));
            Img(card, S(i == 0 ? "btn_gold_9s.png" : "btn_stone_9s.png"), true);
            var face = Place(Rect(card, "Face"), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(190f, 190f));
            if (i == 0) Img(face, AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/South/south.png"), false);
            else Img(face, S("icon_lock.png"), false, KakPalette.ArduvazAcik);
            Text(Place(Rect(card, "Name"), new Vector2(0.5f, 0f), new Vector2(0f, 45f), new Vector2(220f, 60f)),
                 i == 0 ? "@selected" : "@soon", 34, i == 0 ? KakPalette.Murekkep : KakPalette.Krem, TextAlignmentOptions.Center, false, i != 0);
        }
        var closeC = Button(Place(Rect(cpanel, "CloseButton"), new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(620f, 150f)), "@close", Style.Gold, 64);

        // Kontrolcü bağlantıları
        var mmc = Object.FindAnyObjectByType<MainMenuController>();
        if (mmc != null)
        {
            Undo.RecordObject(mmc, "menu");
            mmc.playButton = play;
            mmc.charactersButton = chars;
            mmc.settingsButton = sett;
            mmc.levelsButton = levels;
            mmc.settingsPanel = sroot.gameObject;
            mmc.charactersPanel = croot.gameObject;
            mmc.bestScoreText = bestT;
            mmc.statsText = statsT;
            mmc.resetProgressButton = reset;
            mmc.resetProgressLabel = reset.transform.Find("Label").GetComponent<TMP_Text>();
            if (mmc.defaultLevel == null) mmc.defaultLevel = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Endless_Level1_LevelData.asset");
            OnClick(closeS, mmc.CloseSettingsPanel);
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
}
