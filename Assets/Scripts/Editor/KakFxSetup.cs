using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Faz 3 görsel temelini kurar (tekrar çalıştırılabilir):
///  - Taş prefab'ı: dönen Visual alt objesi (yeni sıcak taş sprite'ı) + dönmeyen yumuşak gölge, adil hitbox
///  - Sahne: FeedbackManager + kırıntı parçacık sistemi, kamera sarsıntısı, oyuncu canlılığı (PlayerJuice)
///  - Renk tonlaması: tek Volume profili (Bloom, kontrast/doygunluk, gölge-ışık tonlaması, tonemapping yok)
///
/// Menü: KacAtaKac/Görsel Temeli Kur (Faz 3)
/// Köprü: python3 tools/kak_bridge.py invoke KakFxSetup Setup
/// </summary>
public static class KakFxSetup
{
    const string GameScenePath = KakEditorUtil.GameScenePath;
    const string ProjectilePrefab = "Assets/Prefabs/Projectile.prefab";
    const string RockSprite = "Assets/Art/Projectiles/rock.png";
    const string ShadowSprite = "Assets/Sprites/Player/Black.png";
    const string RockData = "Assets/Data/Rock_ProjectileData.asset";

    [MenuItem("KacAtaKac/Görsel Temeli Kur (Faz 3)")]
    public static void SetupMenu() => Debug.Log(Setup());

    public static string Setup()
    {
        if (Application.isPlaying) return "HATA: Play modunda çalıştırılamaz.";
        var log = new StringBuilder("[KakFxSetup] ");
        SetupProjectilePrefab(log);

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != GameScenePath)
        {
            KakEditorUtil.SaveNamedScenes();
            scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        }
        var chips = SetupFeedback(log);
        SetupCameraShake(log);
        SetupPlayerJuice(chips, log);
        SetupVolume(log);
        NormalizeSpawnerScale(log);
        SetupRenderPipeline(log);

        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        log.Append(saved ? "Sahne kaydedildi." : "SAHNE KAYDEDİLEMEDİ!");
        return log.ToString();
    }

    // ── Taş prefab'ı ───────────────────────────────────────
    static void SetupProjectilePrefab(StringBuilder log)
    {
        var rock = AssetDatabase.LoadAssetAtPath<Sprite>(RockSprite);
        var shadow = AssetDatabase.LoadAssetAtPath<Sprite>(ShadowSprite);
        if (rock == null) { log.Append("UYARI: taş sprite'ı yok. "); return; }

        var root = PrefabUtility.LoadPrefabContents(ProjectilePrefab);
        try
        {
            root.transform.localScale = Vector3.one;

            // Kökteki eski görseli kaldır, dönen Visual alt objesine taşı
            var rootSr = root.GetComponent<SpriteRenderer>();
            int order = 50;
            if (rootSr != null) { order = rootSr.sortingOrder; Object.DestroyImmediate(rootSr); }

            var visual = root.transform.Find("Visual");
            if (visual == null)
            {
                visual = new GameObject("Visual").transform;
                visual.SetParent(root.transform, false);
            }
            var vsr = visual.GetComponent<SpriteRenderer>();
            if (vsr == null) vsr = visual.gameObject.AddComponent<SpriteRenderer>();
            vsr.sprite = rock;
            vsr.sortingOrder = order;

            var sh = root.transform.Find("Shadow");
            if (sh == null)
            {
                sh = new GameObject("Shadow").transform;
                sh.SetParent(root.transform, false);
            }
            var ssr = sh.GetComponent<SpriteRenderer>();
            if (ssr == null) ssr = sh.gameObject.AddComponent<SpriteRenderer>();
            ssr.sprite = shadow;
            ssr.color = new Color(0f, 0f, 0f, 0.35f);
            ssr.sortingOrder = order - 10;
            if (shadow != null)
            {
                Vector2 s = shadow.bounds.size;
                sh.localScale = new Vector3(0.54f / s.x, 0.2f / s.y, 1f);
            }
            sh.localPosition = new Vector3(0f, -0.26f, 0f);

            // Hitbox görselden biraz küçük (adil ve affedici)
            var col = root.GetComponent<CircleCollider2D>();
            if (col != null) { col.radius = 0.26f; col.offset = Vector2.zero; }

            var proj = root.GetComponent<Projectile>();
            if (proj != null) proj.visual = visual;

            PrefabUtility.SaveAsPrefabAsset(root, ProjectilePrefab);
            log.Append("Taş prefab'ı güncellendi. ");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        var data = AssetDatabase.LoadAssetAtPath<ProjectileData>(RockData);
        if (data != null)
        {
            data.projectileSprite = rock;
            EditorUtility.SetDirty(data);
        }
    }

    // ── Parçacıklar + FeedbackManager ─────────────────────
    static ParticleSystem SetupFeedback(StringBuilder log)
    {
        var managers = GameObject.Find("Managers");
        if (managers == null) { log.Append("UYARI: Managers yok (önce Sahne Yöneticilerini Kur). "); return null; }

        var fb = managers.GetComponentInChildren<FeedbackManager>();
        if (fb == null)
        {
            var go = new GameObject("FeedbackManager");
            Undo.RegisterCreatedObjectUndo(go, "FeedbackManager");
            go.transform.SetParent(managers.transform, false);
            fb = go.AddComponent<FeedbackManager>();
        }

        var chipsT = fb.transform.Find("FxChips");
        ParticleSystem ps;
        if (chipsT == null)
        {
            var go = new GameObject("FxChips");
            go.transform.SetParent(fb.transform, false);
            ps = go.AddComponent<ParticleSystem>();
        }
        else ps = chipsT.GetComponent<ParticleSystem>();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 1f;
        main.startLifetime = 0.5f;
        main.startSpeed = 0f;
        main.startSize = 0.08f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 400;
        main.gravityModifier = 0f;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        var em = ps.emission; em.enabled = false;
        var shape = ps.shape; shape.enabled = false;

        var lim = ps.limitVelocityOverLifetime;
        lim.enabled = true;
        lim.drag = 4f;
        lim.multiplyDragByParticleSize = false;
        lim.multiplyDragByParticleVelocity = false;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.4f));

        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        r.sortingOrder = 150;

        fb.chips = ps;
        EditorUtility.SetDirty(fb);
        log.Append("FeedbackManager + parçacıklar hazır. ");
        return ps;
    }

    static void SetupCameraShake(StringBuilder log)
    {
        var cam = Camera.main;
        if (cam == null) return;
        if (cam.GetComponent<KakCameraShake>() == null)
        {
            Undo.AddComponent<KakCameraShake>(cam.gameObject);
            log.Append("Kamera sarsıntısı eklendi. ");
        }
    }

    static void SetupPlayerJuice(ParticleSystem dust, StringBuilder log)
    {
        var move = Object.FindAnyObjectByType<PlayerMovement2D>();
        if (move == null) return;
        var juice = move.GetComponent<PlayerJuice>();
        if (juice == null) juice = Undo.AddComponent<PlayerJuice>(move.gameObject);
        juice.movement = move;
        juice.visual = move.transform.Find("Visual");
        juice.dust = dust;
        EditorUtility.SetDirty(juice);
        log.Append("Oyuncu canlılığı eklendi. ");
    }

    // ── Fırlatıcı ölçeği: karakterle aynı piksel yoğunluğu ───
    /// <summary>
    /// Fırlatıcıların kök ölçeği 1, görseli 2.4 (48 px × 2.4 / 100 PPU → 1 sanat pikseli = 0.024 birim,
    /// oyuncuyla aynı). Gölge ayak altına yeniden yerleşir.
    /// </summary>
    static void NormalizeSpawnerScale(StringBuilder log)
    {
        const float visualScale = 2.4f;
        int n = 0;
        foreach (var sh in Object.FindObjectsByType<CornerShooter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var root = sh.transform;
            Undo.RecordObject(root, "spawner ölçek");
            root.localScale = Vector3.one;
            foreach (Transform child in root)
            {
                Undo.RecordObject(child, "spawner çocuk");
                if (child.GetComponent<SpawnerDirectionAnimator>() != null)
                {
                    child.localScale = new Vector3(visualScale, visualScale, 1f);
                    child.localPosition = Vector3.zero;
                }
                else if (child.name == "Shadow")
                {
                    var sr = child.GetComponent<SpriteRenderer>();
                    Vector2 size = sr != null && sr.sprite != null ? (Vector2)sr.sprite.bounds.size : Vector2.one;
                    child.localScale = new Vector3(0.9f / size.x, 0.32f / size.y, 1f);
                    child.localPosition = new Vector3(0f, -0.5f, 0f);
                }
            }
            n++;
        }
        log.Append(n + " fırlatıcının ölçeği normalleştirildi. ");
    }

    // ── Render ayarları (performans) ───────────────────────
    /// <summary>
    /// URP Universal Renderer'da sprite'lar ancak dinamik batching ile birleşir.
    /// Projedeki tüm URP asset'lerinde açar.
    /// </summary>
    public static void SetupRenderPipeline(StringBuilder log)
    {
        int n = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset == null) continue;
            asset.supportsDynamicBatching = true;
            EditorUtility.SetDirty(asset);
            n++;
        }
        log.Append(n + " URP asset'inde dinamik batching açıldı. ");
    }

    // ── Renk tonlaması ─────────────────────────────────────
    static void SetupVolume(StringBuilder log)
    {
        var vol = Object.FindAnyObjectByType<Volume>();
        if (vol == null || vol.sharedProfile == null) { log.Append("UYARI: Global Volume yok. "); return; }
        var profile = vol.sharedProfile;

        if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
        bloom.active = true;
        bloom.threshold.Override(0.85f);
        bloom.intensity.Override(0.35f);
        bloom.scatter.Override(0.55f);
        bloom.highQualityFiltering.Override(false);

        // Kenar karartmayı sahnedeki çerçeve (DungeonFrame/Vignette) yapıyor
        if (profile.TryGet(out Vignette vig)) vig.active = false;

        // Tonemapping renkleri soldurur; paletin renkleri korunsun
        if (profile.TryGet(out Tonemapping tm)) { tm.active = true; tm.mode.Override(TonemappingMode.None); }

        if (!profile.TryGet(out ColorAdjustments ca)) ca = profile.Add<ColorAdjustments>(true);
        ca.active = true;
        ca.contrast.Override(10f);
        ca.saturation.Override(6f);
        ca.postExposure.Override(0f);

        // Gölgeler soğuk-mor (gece), ışıklar sıcak (meşale) → bütün sahneyi aynı havada toplar
        if (!profile.TryGet(out SplitToning st)) st = profile.Add<SplitToning>(true);
        st.active = true;
        st.shadows.Override(new Color(0.36f, 0.32f, 0.55f));
        st.highlights.Override(new Color(0.98f, 0.82f, 0.62f));
        st.balance.Override(-15f);

        EditorUtility.SetDirty(profile);
        log.Append("Renk tonlaması ayarlandı. ");
    }
}
