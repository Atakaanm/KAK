using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz 5 Sonsuz Mod içeriğini sahneye kurar (tekrar çalıştırılabilir):
/// olay yöneticisi + şerit uyarısı, dash + yakın geçiş (oyuncu), dash butonu, combo ve afiş yazıları,
/// dünya yazıları havuzu, zorluk eşikleri (süreye yayılmış denge).
/// Menü: KacAtaKac/Sonsuz Mod İçeriğini Kur (Faz 5)
/// </summary>
public static class KakEndlessSetup
{
    const string GameScenePath = KakEditorUtil.GameScenePath;

    [MenuItem("KacAtaKac/Sonsuz Mod İçeriğini Kur (Faz 5)")]
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
        var log = new StringBuilder("[KakEndlessSetup] ");
        var managers = GameObject.Find("Managers");
        if (managers == null) return "HATA: Managers yok.";

        // ── Olay yöneticisi ─────────────────────────────
        var em = Child<EndlessEventManager>(managers.transform, "EndlessEventManager");
        em.projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        em.meteorData = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Projectiles/Meteor_Goktasi.asset");
        em.boulderData = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Projectiles/Boulder_Kaya.asset");
        em.rockData = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Rock_ProjectileData.asset");
        var lane = em.transform.Find("LaneWarning");
        if (lane == null) { lane = new GameObject("LaneWarning").transform; lane.SetParent(em.transform, false); }
        var lsr = lane.GetComponent<SpriteRenderer>();
        if (lsr == null) lsr = lane.gameObject.AddComponent<SpriteRenderer>();
        lsr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/white.png");
        lsr.drawMode = SpriteDrawMode.Tiled;
        lsr.sortingOrder = -50;
        lsr.enabled = false;
        em.laneWarning = lsr;
        EditorUtility.SetDirty(em);

        // ── Dünya yazıları ─────────────────────────────
        var popup = Child<WorldPopup>(managers.transform, "WorldPopup");
        Undo.RecordObject(popup, "WorldPopup font");
        popup.font = KakUiKit.Nunito;               // UI ile aynı font (boşken TMP varsayılanı LiberationSans çıkıyordu)
        popup.fontMaterial = KakUiKit.NunitoOutline;
        EditorUtility.SetDirty(popup);

        // ── Oyuncu: dash + yakın geçiş ─────────────────
        var move = Object.FindAnyObjectByType<PlayerMovement2D>();
        PlayerDash dash = null;
        if (move != null)
        {
            dash = move.GetComponent<PlayerDash>();
            if (dash == null) dash = Undo.AddComponent<PlayerDash>(move.gameObject);
            var vis = move.transform.Find("Visual");
            dash.source = vis != null ? vis.GetComponent<SpriteRenderer>() : null;
            EditorUtility.SetDirty(dash);
            if (move.GetComponent<NearMissTracker>() == null) Undo.AddComponent<NearMissTracker>(move.gameObject);
        }

        // ── HUD: combo + afiş ──────────────────────────
        var canvas = GameObject.Find("HUDCanvas");
        var hudBand = canvas.transform.Find("HudBand") as RectTransform;
        var hudContent = hudBand != null ? hudBand.Find("HudContent") as RectTransform : null;
        var extras = canvas.GetComponent<HudExtras>();
        if (extras == null) extras = Undo.AddComponent<HudExtras>(canvas);
        var scoreText = FindDeep(canvas.transform, "ScoreText")?.GetComponent<TMP_Text>();
        TMP_FontAsset font = KakUiKit.Nunito;

        var combo = Text(hudContent, "ComboText", font, 44, TextAlignmentOptions.MidlineRight);
        SetRect(combo.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-150f, -6f), new Vector2(240f, 60f));
        extras.comboText = combo;

        var banner = Text(hudBand, "EventBanner", font, 64, TextAlignmentOptions.Center);
        SetRect(banner.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1000f, 110f));
        // Kontur paylaşılan materyalden: outlineWidth ayarlamak sahneye gömülü materyal kopyası üretir (batch bozulur)
        extras.bannerText = banner;
        EditorUtility.SetDirty(extras);

        // ── Kontrol alanı: dash butonu (sağ) ───────────
        var control = canvas.transform.Find("ControlArea/ControlContent") as RectTransform;
        if (control != null && dash != null)
        {
            var btnT = control.Find("DashButton") as RectTransform;
            if (btnT == null)
            {
                var go = new GameObject("DashButton", typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(go, "DashButton");
                go.transform.SetParent(control, false);
                btnT = (RectTransform)go.transform;
            }
            SetRect(btnT, new Vector2(0.8f, 0.45f), new Vector2(0.8f, 0.45f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(230f, 230f));
            var hit = btnT.GetComponent<Image>();
            if (hit == null) hit = btnT.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f); // dokunma alanı
            hit.raycastTarget = true;

            var visual = ImageChild(btnT, "Visual", "Assets/Art/UI/dash_base_soft.png", new Vector2(200f, 200f));
            var fill = ImageChild(visual.rectTransform, "Cooldown", "Assets/Art/UI/dash_fill_soft.png", new Vector2(188f, 188f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = (int)Image.Origin360.Top;
            fill.fillClockwise = false;
            fill.fillAmount = 0f;
            var icon = ImageChild(visual.rectTransform, "Icon", "Assets/Art/UI/dash_icon_soft.png", new Vector2(120f, 120f));
            icon.color = KakPalette.Krem;

            var db = btnT.GetComponent<DashButton>();
            if (db == null) db = btnT.gameObject.AddComponent<DashButton>();
            db.dash = dash;
            db.cooldownFill = fill;
            db.icon = icon;
            db.visual = visual.rectTransform;
            EditorUtility.SetDirty(db);

            // Aktif güçlendirme göstergesi: kontrol alanının üst ortası (arenanın hemen altı)
            var hudT = control.Find("PowerupHud") as RectTransform;
            if (hudT == null)
            {
                var go = new GameObject("PowerupHud", typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(go, "PowerupHud");
                go.transform.SetParent(control, false);
                hudT = (RectTransform)go.transform;
            }
            SetRect(hudT, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), new Vector2(600f, 120f));
            var ph = hudT.GetComponent<PowerupHud>();
            if (ph == null) ph = hudT.gameObject.AddComponent<PowerupHud>();
            ph.chipBase = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/dash_base_soft.png");
            ph.ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/ring_soft.png");
            EditorUtility.SetDirty(ph);

            // İlk oyun ipuçları (bir kez): kontrol alanında yazı
            var hint = Text(control, "HintText", font, 46, TextAlignmentOptions.Center);
            SetRect(hint.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 150f));
            hint.textWrappingMode = TextWrappingModes.NoWrap; // satırlar Loc'ta \n ile
            var ob = hint.GetComponent<OnboardingHints>();
            if (ob == null) ob = hint.gameObject.AddComponent<OnboardingHints>();
            ob.hintText = hint;
            ob.dashButton = db;
            EditorUtility.SetDirty(ob);
            hint.gameObject.SetActive(true); // bileşen Start'ta kendini gizler

            // Joystick bölgesi butonla çakışmasın: sol %60
            var zone = control.Find("JoystickZone") as RectTransform;
            if (zone != null) { zone.anchorMin = Vector2.zero; zone.anchorMax = new Vector2(0.6f, 1f); }
        }

        // ── Denge: kademe eşikleri süreye yayılır ──────
        // Denge v2 (2026-09-25, bot ölçümüne göre): zorluk artık çeşitlilikten (taş türleri, olaylar) de geliyor,
        // bu yüzden hız/sıklık çarpanları eskisinden yumuşak.   eşik      spawner aralık× hız×  boyut× oyuncu× skor×
        SetStage("Assets/Data/Stage1_Baslangic.asset", 0, 199,     2, 1.00f, 1.00f, 1.00f, 1.00f, 1.0f);
        SetStage("Assets/Data/Stage2_Kolay.asset", 200, 474,       2, 0.90f, 1.10f, 1.00f, 1.05f, 1.1f);
        SetStage("Assets/Data/Stage3_Orta.asset", 475, 899,        3, 0.82f, 1.20f, 1.08f, 1.08f, 1.2f);
        SetStage("Assets/Data/Stage4_Zor.asset", 900, 1549,        3, 0.72f, 1.30f, 1.15f, 1.12f, 1.3f);
        SetStage("Assets/Data/Stage5_Cehennem.asset", 1550, 2599,  4, 0.62f, 1.42f, 1.22f, 1.16f, 1.5f);
        SetStage("Assets/Data/Stage6_Imkansiz.asset", 2600, -1,    4, 0.52f, 1.55f, 1.30f, 1.20f, 1.8f);
        AssetDatabase.SaveAssets();

        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        log.Append("Olaylar, dash, yakın geçiş, combo, afiş, dash butonu, eşikler kuruldu. ");
        log.Append(saved ? "Sahne kaydedildi." : "SAHNE KAYDEDİLEMEDİ!");
        return log.ToString();
    }

    static T Child<T>(Transform parent, string name) where T : Component
    {
        var existing = parent.GetComponentInChildren<T>(true);
        if (existing != null) return existing;
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<T>();
    }

    static TMP_Text Text(Transform parent, string name, TMP_FontAsset font, float size, TextAlignmentOptions align)
    {
        var t = parent.Find(name);
        TextMeshProUGUI tmp;
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, name);
            go.transform.SetParent(parent, false);
            tmp = go.AddComponent<TextMeshProUGUI>();
        }
        else tmp = t.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.fontSharedMaterial = KakUiKit.NunitoOutline;
        // Eski outlineWidth çağrılarından kalan gömülü materyal kopyasını bırak (sahneden silinsin)
        var so = new SerializedObject(tmp);
        var inst = so.FindProperty("m_fontMaterial");
        if (inst != null && inst.objectReferenceValue != null) { inst.objectReferenceValue = null; so.ApplyModifiedPropertiesWithoutUndo(); }
        tmp.fontSize = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.color = KakPalette.Krem;
        return tmp;
    }

    static Image ImageChild(RectTransform parent, string name, string spritePath, Vector2 size)
    {
        var t = parent.Find(name) as RectTransform;
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            t = (RectTransform)go.transform;
        }
        SetRect(t, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        var img = t.GetComponent<Image>();
        if (img == null) img = t.gameObject.AddComponent<Image>();
        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        img.raycastTarget = false;
        return img;
    }

    static void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot; rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    static void SetStage(string path, int min, int max, int spawners, float interval, float speed, float scale, float player, float score)
    {
        var st = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(path);
        if (st == null) return;
        st.minScore = min;
        st.maxScore = max;
        st.activeSpawnerCount = spawners;
        st.shootIntervalMultiplier = interval;
        st.projectileSpeedMultiplier = speed;
        st.projectileScaleMultiplier = scale;
        st.playerSpeedMultiplier = player;
        st.scoreSpeedMultiplier = score;
        EditorUtility.SetDirty(st);
    }
}
