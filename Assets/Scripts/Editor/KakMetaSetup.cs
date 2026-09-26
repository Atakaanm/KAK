using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static KakUiKit;

/// <summary>
/// Faz 3c (bağlılık katmanı) kurulumu: altın prefab'ı, CoinSpawner, HUD altın sayacı, oyun sonu altın satırı.
/// Tekrar çalıştırılabilir. Menü: KacAtaKac/Bağlılık Katmanını Kur (Faz 3c) · Köprü: invoke KakMetaSetup Setup
/// </summary>
public static class KakMetaSetup
{
    const string CoinPrefabPath = "Assets/Prefabs/Coin.prefab";
    const string CoinArt = "Assets/Art/Pickups/coin_{0}.png";

    [MenuItem("KacAtaKac/Bağlılık Katmanını Kur (Faz 3c)")]
    public static void SetupMenu() => Debug.Log(Setup());

    public static string Setup()
    {
        if (Application.isPlaying) return "HATA: Play modunda çalıştırılamaz.";
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != KakEditorUtil.GameScenePath)
        {
            KakEditorUtil.SaveNamedScenes();
            scene = EditorSceneManager.OpenScene(KakEditorUtil.GameScenePath, OpenSceneMode.Single);
        }
        var log = new StringBuilder("[KakMetaSetup] ");

        var coin = CreateCoinPrefab(log);

        // ── CoinSpawner (Managers altında) ──
        var managers = GameObject.Find("Managers");
        if (managers == null) return "HATA: Managers yok (Sahne Yöneticilerini Kur).";
        var cs = managers.GetComponentInChildren<CoinSpawner>(true);
        if (cs == null)
        {
            var go = new GameObject("CoinSpawner");
            Undo.RegisterCreatedObjectUndo(go, "CoinSpawner");
            go.transform.SetParent(managers.transform, false);
            cs = go.AddComponent<CoinSpawner>();
        }
        cs.coinPrefab = coin;
        EditorUtility.SetDirty(cs);

        // ── HUD: bandın ortasında altın sayacı ──
        var canvas = GameObject.Find("HUDCanvas");
        var hudContent = canvas != null ? canvas.transform.Find("HudBand/HudContent") : null;
        if (hudContent != null)
        {
            var root = Place(Rect(hudContent, "CoinHud"), new Vector2(0.5f, 0.5f), new Vector2(-40f, 0f), new Vector2(260f, 90f));
            var content = Stretch(Rect(root, "Content"));
            var icon = Img(Place(Rect(content, "Icon"), new Vector2(0.5f, 0.5f), new Vector2(-60f, 0f), new Vector2(58f, 58f)),
                           AssetDatabase.LoadAssetAtPath<Sprite>(string.Format(CoinArt, 0)), false);
            var text = Text(Place(Rect(content, "Count"), new Vector2(0.5f, 0.5f), new Vector2(50f, 0f), new Vector2(160f, 80f)),
                            "0", 52, KakPalette.AltinAcik, TextAlignmentOptions.MidlineLeft);
            var hud = GetOrAdd<CoinHud>(root.gameObject);
            hud.icon = icon; hud.countText = text; hud.content = content;
            EditorUtility.SetDirty(hud);
            log.Append("HUD altın sayacı. ");
        }
        else log.Append("UYARI: HudBand/HudContent yok. ");

        // ── Oyun sonu: altın satırı ──
        var gos = Object.FindAnyObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        if (gos != null && gos.panel != null)
        {
            KakUiSetup.BuildCoinsRow(gos.panel, gos);
            KakUiSetup.BuildUnlockBanner(gos.panel, gos);
            log.Append("Oyun sonu altın satırı + açılış afişi. ");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return log.Append("Sahne kaydedildi.").ToString();
    }

    /// <summary>Altın prefab'ı: kök (trigger collider + Coin), Visual (dönen sprite), Shadow.</summary>
    static GameObject CreateCoinPrefab(StringBuilder log)
    {
        var frames = new Sprite[4];
        for (int i = 0; i < 4; i++) frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(string.Format(CoinArt, i));
        if (frames[0] == null) { log.Append("HATA: altın görselleri yok (tools/kak_gen_pickups.py). "); return null; }

        var root = new GameObject("Coin");
        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f; // görsel 0.34 çap; cömert toplama

        var visual = new GameObject("Visual").AddComponent<SpriteRenderer>();
        visual.transform.SetParent(root.transform, false);
        visual.sprite = frames[0];
        visual.sortingOrder = 12;

        var shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/Black.png");
        if (shadowSprite != null)
        {
            var sh = new GameObject("Shadow").AddComponent<SpriteRenderer>();
            sh.transform.SetParent(root.transform, false);
            sh.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            sh.transform.localScale = new Vector3(0.09f, 0.04f, 1f); // 360 px @100 → 0.32 x 0.14
            sh.sprite = shadowSprite;
            sh.color = new Color(1f, 1f, 1f, 0.35f);
            sh.sortingOrder = 11;
        }

        var c = root.AddComponent<Coin>();
        c.frames = frames;
        c.visual = visual;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, CoinPrefabPath);
        Object.DestroyImmediate(root);
        log.Append("Coin.prefab. ");
        return prefab;
    }
}
