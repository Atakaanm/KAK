using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz 1.5 ekran kompozisyonunu oyun sahnesine kurar (tekrar çalıştırılabilir):
///  - Kamera: ScreenComposer eklenir, arka plan düz koyu renk
///  - HUD: HudBand/HudContent (üst), kalpler solda, skor sağda, pause en sağda
///  - Kontrol: ControlArea/ControlContent/JoystickZone (alt), kayan joystick
///  - Dünya: DungeonFrame (arka plan, koridor, duvarlar, korniş, vinyet, meşaleler)
///
/// Menü: KacAtaKac/Ekran Kompozisyonunu Kur
/// Köprü: python3 tools/kak_bridge.py invoke KakScreenSetup Setup
/// </summary>
public static class KakScreenSetup
{
    const string GameScenePath = KakEditorUtil.GameScenePath;
    const string TileDir = "Assets/Art/Tiles/Dungeon/";

    [MenuItem("KacAtaKac/Ekran Kompozisyonunu Kur")]
    public static void SetupMenu() => Debug.Log(Setup());

    public static string Setup()
    {
        if (Application.isPlaying) return "HATA: Play modunda çalıştırılamaz.";
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != GameScenePath)
        {
            KakEditorUtil.SaveNamedScenes();
            scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        }
        var log = new StringBuilder("[KakScreenSetup] ");

        var arena = Object.FindAnyObjectByType<ArenaAutoLayout>();
        var cam = Camera.main;
        var canvasGo = GameObject.Find("HUDCanvas");
        if (arena == null || cam == null || canvasGo == null)
            return "HATA: ArenaAutoLayout / Main Camera / HUDCanvas bulunamadı.";
        var canvas = canvasGo.GetComponent<Canvas>();

        // ── Kamera ─────────────────────────────────────────────
        var composer = cam.GetComponent<ScreenComposer>();
        if (composer == null) composer = Undo.AddComponent<ScreenComposer>(cam.gameObject);
        composer.arena = arena;

        // ── Canvas ölçekleme: genişliğe göre (arena genişliğe sığdığı için HUD oranı sabit kalır)
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            Undo.RecordObject(scaler, "CanvasScaler");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f;
        }
        // Canvas kökündeki Image (varsa) tıklamaları yutmasın
        var rootImg = canvasGo.GetComponent<Image>();
        if (rootImg != null) { Undo.RecordObject(rootImg, "root img"); rootImg.raycastTarget = false; }

        // ── HUD bandı ──────────────────────────────────────────
        var hudBand = GetOrCreateRect(canvasGo.transform, "HudBand", 0);
        var hudContent = GetOrCreateRect(hudBand, "HudContent", 0);
        composer.hudBand = hudBand;
        composer.hudContent = hudContent;

        var health = Find(canvasGo.transform, "HealthPanel");
        if (health != null)
        {
            Reparent(health, hudContent);
            SetRect(health, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(330f, 110f));
            string[] hearts = { "Heart1", "Heart2", "Heart3" };
            var imgs = new System.Collections.Generic.List<UnityEngine.UI.Image>();
            for (int i = 0; i < hearts.Length; i++)
            {
                var h = Find(health, hearts[i]);
                if (h != null)
                {
                    SetRect(h, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                            new Vector2(52f + i * 96f, 0f), new Vector2(150f, 82f));
                    var img = h.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) imgs.Add(img);
                }
            }
            // HealthUI tüm kalpleri yönetsin (eskiden sadece Heart1: klonlar Heart2/3'ün üstüne biniyordu)
            var hui = health.GetComponent<HealthUI>();
            if (hui != null) { Undo.RecordObject(hui, "hearts"); hui.hearts = imgs.ToArray(); EditorUtility.SetDirty(hui); }
        }
        var score = Find(canvasGo.transform, "ScoreText");
        if (score != null)
        {
            Reparent(score, hudContent);
            SetRect(score, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-150f, 0f), new Vector2(460f, 110f));
            var tmp = score.GetComponent<TMP_Text>();
            if (tmp != null) { Undo.RecordObject(tmp, "score"); tmp.alignment = TextAlignmentOptions.MidlineRight; tmp.enableAutoSizing = false; tmp.fontSize = 64; }
        }
        var pause = Find(canvasGo.transform, "PauseButton");
        if (pause != null)
        {
            Reparent(pause, hudContent);
            SetRect(pause, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-32f, 0f), new Vector2(96f, 96f));
        }

        // ── Kontrol alanı + kayan joystick ─────────────────────
        var control = GetOrCreateRect(canvasGo.transform, "ControlArea", 1);
        var controlContent = GetOrCreateRect(control, "ControlContent", 0);
        composer.controlArea = control;
        composer.controlContent = controlContent;

        var zone = GetOrCreateRect(controlContent, "JoystickZone", 0);
        SetRect(zone, new Vector2(0f, 0f), new Vector2(0.6f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var zoneImg = zone.GetComponent<Image>();
        if (zoneImg == null) zoneImg = Undo.AddComponent<Image>(zone.gameObject);
        zoneImg.color = new Color(0f, 0f, 0f, 0f);
        zoneImg.raycastTarget = true;

        var bg = Find(canvasGo.transform, "JoystickBG");
        var knob = bg != null ? Find(bg, "JoystickHandle") : null;
        if (bg != null)
        {
            Reparent(bg, zone);
            SetRect(bg, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 300f));
            var img = bg.GetComponent<Image>();
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/joystick_base_soft.png");
            if (img != null && sp != null) { Undo.RecordObject(img, "bg"); img.sprite = sp; img.color = Color.white; img.raycastTarget = false; }
        }
        if (knob != null)
        {
            SetRect(knob, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(128f, 128f));
            var img = knob.GetComponent<Image>();
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/joystick_knob_soft.png");
            if (img != null && sp != null) { Undo.RecordObject(img, "knob"); img.sprite = sp; img.color = Color.white; img.raycastTarget = false; }
            var oldJoy = knob.GetComponent<VirtualJoystick>();
            if (oldJoy != null) { Undo.DestroyObjectImmediate(oldJoy); log.Append("Eski joystick bileşeni tutamaktan kaldırıldı. "); }
        }
        var joy = zone.GetComponent<VirtualJoystick>();
        if (joy == null) joy = Undo.AddComponent<VirtualJoystick>(zone.gameObject);
        joy.background = bg as RectTransform;
        joy.handle = knob as RectTransform;
        joy.floating = true;
        var move = Object.FindAnyObjectByType<PlayerMovement2D>();
        if (move != null) { Undo.RecordObject(move, "joy"); move.joystick = joy; }

        // Panellerin HUD ve kontrolün üstünde çizilmesi için sona al
        foreach (var name in new[] { "PausePanel", "GameOverPanel" })
        {
            var t = Find(canvasGo.transform, name);
            if (t != null) t.SetAsLastSibling();
        }

        // ── Dünya çerçevesi ────────────────────────────────────
        var frame = BuildFrame(log);
        composer.frame = frame;

        EditorUtility.SetDirty(composer);
        composer.ForceRecalculate();
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        log.Append(saved ? "Sahne kaydedildi." : "SAHNE KAYDEDİLEMEDİ!");
        return log.ToString();
    }

    static DungeonFrame BuildFrame(StringBuilder log)
    {
        var root = GameObject.Find("DungeonFrame");
        if (root == null)
        {
            root = new GameObject("DungeonFrame");
            Undo.RegisterCreatedObjectUndo(root, "DungeonFrame");
            log.Append("DungeonFrame oluşturuldu. ");
        }
        var frame = root.GetComponent<DungeonFrame>();
        if (frame == null) frame = Undo.AddComponent<DungeonFrame>(root);

        frame.backdrop = Renderer(root.transform, "Backdrop", "backdrop_tile", -300);
        frame.corridorFloor = Renderer(root.transform, "CorridorFloor", "corridor_tile", -290);
        frame.corridorWallLeft = Renderer(root.transform, "CorridorWallLeft", "corridor_wall", -285);
        frame.corridorWallRight = Renderer(root.transform, "CorridorWallRight", "corridor_wall", -285);
        frame.ledge = Renderer(root.transform, "Ledge", "ledge_tile", -280);
        frame.vignette = Renderer(root.transform, "Vignette", "vignette", -270);
        frame.vignette.drawMode = SpriteDrawMode.Simple;

        var torchFrames = Enumerable.Range(0, 4)
            .Select(i => AssetDatabase.LoadAssetAtPath<Sprite>(TileDir + "torch_" + i + ".png"))
            .Where(s => s != null).ToArray();
        var glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TileDir + "glow.png");

        const int torchCount = 8;
        frame.torches = new Transform[torchCount];
        for (int i = 0; i < torchCount; i++)
        {
            var sr = Renderer(root.transform, "Torch" + i, "torch_0", -260);
            sr.drawMode = SpriteDrawMode.Simple;
            var fb = sr.GetComponent<SpriteFlipbook>();
            if (fb == null) fb = Undo.AddComponent<SpriteFlipbook>(sr.gameObject);
            fb.frames = torchFrames;
            fb.fps = 8f;

            var glowT = sr.transform.Find("Glow");
            GameObject glowGo;
            if (glowT == null)
            {
                glowGo = new GameObject("Glow");
                Undo.RegisterCreatedObjectUndo(glowGo, "Glow");
                glowGo.transform.SetParent(sr.transform, false);
            }
            else glowGo = glowT.gameObject;
            glowGo.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            glowGo.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
            var gsr = glowGo.GetComponent<SpriteRenderer>();
            if (gsr == null) gsr = Undo.AddComponent<SpriteRenderer>(glowGo);
            gsr.sprite = glowSprite;
            gsr.sortingOrder = -265;
            if (glowGo.GetComponent<FlickerGlow>() == null) Undo.AddComponent<FlickerGlow>(glowGo);

            frame.torches[i] = sr.transform;
        }
        EditorUtility.SetDirty(frame);
        return frame;
    }

    static SpriteRenderer Renderer(Transform parent, string name, string sprite, int order)
    {
        var t = parent.Find(name);
        GameObject go;
        if (t == null)
        {
            go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, name);
            go.transform.SetParent(parent, false);
        }
        else go = t.gameObject;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = Undo.AddComponent<SpriteRenderer>(go);
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TileDir + sprite + ".png");
        if (sr.sprite == null) Debug.LogWarning("[KakScreenSetup] Sprite bulunamadı: " + sprite);
        sr.sortingOrder = order;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        return sr;
    }

    // ── RectTransform yardımcıları ────────────────────────────
    static RectTransform GetOrCreateRect(Transform parent, string name, int siblingIndex)
    {
        var t = parent.Find(name) as RectTransform;
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, name);
            go.transform.SetParent(parent, false);
            t = (RectTransform)go.transform;
            t.anchorMin = Vector2.zero;
            t.anchorMax = Vector2.one;
            t.offsetMin = t.offsetMax = Vector2.zero;
        }
        t.SetSiblingIndex(Mathf.Min(siblingIndex, parent.childCount - 1));
        return t;
    }

    static Transform Find(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    static void Reparent(Transform t, Transform parent)
    {
        if (t.parent != parent) Undo.SetTransformParent(t, parent, "Reparent " + t.name);
    }

    static void SetRect(Transform t, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var rt = t as RectTransform;
        if (rt == null) return;
        Undo.RecordObject(rt, "rect " + t.name);
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
