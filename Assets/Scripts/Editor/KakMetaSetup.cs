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
            // Kalplerin altında (skorun altındaki combo yazısının simetriği): 4-5 kalpli karakterlerde bandın ortası dolar
            var root = Place(Rect(hudContent, "CoinHud"), new Vector2(0f, 0f), new Vector2(46f, 44f), new Vector2(220f, 50f), new Vector2(0f, 0.5f));
            var content = Stretch(Rect(root, "Content"));
            var icon = Img(Place(Rect(content, "Icon"), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(40f, 40f)),
                           AssetDatabase.LoadAssetAtPath<Sprite>(string.Format(CoinArt, 0)), false);
            var text = Text(Place(Rect(content, "Count"), new Vector2(0f, 0.5f), new Vector2(110f, 0f), new Vector2(150f, 56f)),
                            "0", 40, KakPalette.AltinAcik, TextAlignmentOptions.MidlineLeft);
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
            KakUiSetup.BuildMissionsBlock(gos.panel, gos);
            KakUiSetup.BuildPolish(gos.panel, gos);
            KakUiSetup.BuildContinuePanel(canvas.transform, Object.FindAnyObjectByType<GameManager>());
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

    // ─────────────────────────────────────────────────────────────
    // Karakterler (Faz 3c.3)
    // ─────────────────────────────────────────────────────────────
    const string PlayerSprites = "Assets/Sprites/Player/";
    const string CharSprites = "Assets/Sprites/Characters/";
    const string BoyData = "Assets/Data/Boy_PlayerData.asset";

    struct CharDef
    {
        public string id; public int hp; public float speed, dash, hurt, coin, puRate, puDur; public bool shield; public int price;
    }

    // Ödünleşimli istatistikler (düz güç değil): 3c.md
    static readonly CharDef[] Chars =
    {
        new CharDef { id = "Swift", hp = 2, speed = 5.75f, dash = 2.0f, hurt = 0.85f, coin = 1f, puRate = 1f, puDur = 1f, price = 300 },
        new CharDef { id = "Tank", hp = 4, speed = 4.5f, dash = 3.2f, hurt = 1f, coin = 1f, puRate = 1f, puDur = 1f, shield = true, price = 800 },
        new CharDef { id = "Lucky", hp = 3, speed = 5f, dash = 0f, hurt = 1f, coin = 1.25f, puRate = 1.3f, puDur = 1.2f, price = 1500 },
    };

    [MenuItem("KacAtaKac/Karakterleri Kur (Faz 3c)")]
    public static void SetupCharactersMenu() => Debug.Log(SetupCharacters());

    /// <summary>
    /// Varyant sprite'ların içe aktarma ayarlarını Ata'dan kopyalar, PlayerData'ları ve Resources/CharacterCatalog'u kurar.
    /// Önce: python3 tools/kak_recolor_characters.py
    /// </summary>
    public static string SetupCharacters()
    {
        var log = new StringBuilder("[KakMetaSetup] ");
        var boy = AssetDatabase.LoadAssetAtPath<PlayerData>(BoyData);
        if (boy == null) return "HATA: Boy_PlayerData yok.";
        boy.id = "Boy"; boy.nameKey = "char_Boy"; boy.traitKey = "trait_Boy"; boy.unlockPrice = 0; boy.isLocked = false;
        EditorUtility.SetDirty(boy);

        // 1) Varyant sprite'ların içe aktarma ayarları = kaynak (PPU 100, nokta filtre, merkez pivot)
        int copied = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites/Characters" }))
        {
            string dst = AssetDatabase.GUIDToAssetPath(guid);
            string rel = dst.Substring(CharSprites.Length);
            rel = rel.Substring(rel.IndexOf('/') + 1); // <Id>/ sonrası
            var srcImp = AssetImporter.GetAtPath(PlayerSprites + rel) as TextureImporter;
            var dstImp = AssetImporter.GetAtPath(dst) as TextureImporter;
            if (srcImp == null || dstImp == null) continue;
            var st = new TextureImporterSettings();
            srcImp.ReadTextureSettings(st);
            dstImp.SetTextureSettings(st);
            dstImp.textureCompression = srcImp.textureCompression;
            dstImp.maxTextureSize = srcImp.maxTextureSize;
            dstImp.SaveAndReimport();
            copied++;
        }
        log.Append(copied + " sprite ayarı kopyalandı. ");

        // 2) PlayerData'lar
        System.IO.Directory.CreateDirectory("Assets/Data/Characters");
        var list = new System.Collections.Generic.List<PlayerData> { boy };
        foreach (var c in Chars)
        {
            string path = "Assets/Data/Characters/" + c.id + "_PlayerData.asset";
            var pd = AssetDatabase.LoadAssetAtPath<PlayerData>(path);
            if (pd == null)
            {
                pd = ScriptableObject.CreateInstance<PlayerData>();
                EditorUtility.CopySerialized(boy, pd);
                AssetDatabase.CreateAsset(pd, path);
            }
            else EditorUtility.CopySerialized(boy, pd);
            pd.name = c.id + "_PlayerData";
            pd.playerName = c.id;
            pd.id = c.id; pd.nameKey = "char_" + c.id; pd.traitKey = "trait_" + c.id;
            pd.maxHealth = c.hp; pd.moveSpeed = c.speed; pd.dashCooldown = c.dash; pd.hurtboxScale = c.hurt;
            pd.coinMultiplier = c.coin; pd.powerupSpawnRateMultiplier = c.puRate; pd.powerupDurationMultiplier = c.puDur;
            pd.startWithShield = c.shield; pd.unlockPrice = c.price; pd.isLocked = true; pd.portrait = null;
            foreach (var dir in new[] { pd.north, pd.south, pd.east, pd.west, pd.northEast, pd.northWest, pd.southEast, pd.southWest })
                Remap(dir, c.id);
            EditorUtility.SetDirty(pd);
            list.Add(pd);
        }

        // 3) Katalog (Resources)
        const string catPath = "Assets/Resources/CharacterCatalog.asset";
        var cat = AssetDatabase.LoadAssetAtPath<CharacterCatalog>(catPath);
        if (cat == null) { cat = ScriptableObject.CreateInstance<CharacterCatalog>(); AssetDatabase.CreateAsset(cat, catPath); }
        cat.characters = list.ToArray();
        EditorUtility.SetDirty(cat);
        AssetDatabase.SaveAssets();
        return log.Append(list.Count + " karakter kataloğa eklendi.").ToString();
    }

    /// <summary>Yön verisindeki sprite'ları varyant klasöründeki karşılığına çevirir (aynı göreli yol).</summary>
    static void Remap(PlayerData.DirectionData d, string id)
    {
        if (d == null) return;
        d.idle = RemapSprite(d.idle, id);
        if (d.runFrames != null)
        {
            var frames = new Sprite[d.runFrames.Length];
            for (int i = 0; i < frames.Length; i++) frames[i] = RemapSprite(d.runFrames[i], id);
            d.runFrames = frames;
        }
    }

    static Sprite RemapSprite(Sprite s, string id)
    {
        if (s == null) return null;
        string p = AssetDatabase.GetAssetPath(s);
        if (!p.StartsWith(PlayerSprites)) return s;
        var v = AssetDatabase.LoadAssetAtPath<Sprite>(CharSprites + id + "/" + p.Substring(PlayerSprites.Length));
        return v != null ? v : s;
    }

    // ─────────────────────────────────────────────────────────────
    // Pet'ler (Faz 3c.5)
    // ─────────────────────────────────────────────────────────────
    [MenuItem("KacAtaKac/Petleri Kur (Faz 3c)")]
    public static void SetupPetsMenu() => Debug.Log(SetupPets());

    /// <summary>Pet verileri (Assets/Data/Pets), Resources/PetCatalog, LevelManager gölge/ışık sprite'ları. Önce: tools/kak_gen_pickups.py</summary>
    public static string SetupPets()
    {
        System.IO.Directory.CreateDirectory("Assets/Data/Pets");
        var firefly = PetAsset("Firefly", 400, PetPassive.Magnet, "Assets/Art/Pets/firefly_{0}.png", 8f, true, true);
        var turtle = PetAsset("Turtle", 1200, PetPassive.ShieldRegen, "Assets/Art/Pets/turtle_{0}.png", 4f, false, false);
        const string catPath = "Assets/Resources/PetCatalog.asset";
        var cat = AssetDatabase.LoadAssetAtPath<PetCatalog>(catPath);
        if (cat == null) { cat = ScriptableObject.CreateInstance<PetCatalog>(); AssetDatabase.CreateAsset(cat, catPath); }
        cat.pets = new[] { firefly, turtle };
        EditorUtility.SetDirty(cat);
        AssetDatabase.SaveAssets();

        // Oyun sahnesi: LevelManager pet gölgesi/ışığı
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != KakEditorUtil.GameScenePath)
        {
            KakEditorUtil.SaveNamedScenes();
            scene = EditorSceneManager.OpenScene(KakEditorUtil.GameScenePath, OpenSceneMode.Single);
        }
        var lm = Object.FindAnyObjectByType<LevelManager>();
        if (lm != null)
        {
            Undo.RecordObject(lm, "pet sprites");
            lm.petShadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/Black.png");
            lm.petGlowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tiles/Dungeon/glow.png");
            EditorUtility.SetDirty(lm);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        return "[KakMetaSetup] 2 pet (Ateşböceği, Kaplumbağa) + PetCatalog.";
    }

    static PetData PetAsset(string id, int price, PetPassive passive, string framePattern, float fps, bool flying, bool glow)
    {
        string path = "Assets/Data/Pets/" + id + "_PetData.asset";
        var pd = AssetDatabase.LoadAssetAtPath<PetData>(path);
        if (pd == null) { pd = ScriptableObject.CreateInstance<PetData>(); AssetDatabase.CreateAsset(pd, path); }
        pd.id = id; pd.nameKey = "pet_" + id; pd.traitKey = "pettrait_" + id; pd.price = price; pd.passive = passive;
        pd.fps = fps; pd.flying = flying; pd.glow = glow;
        pd.frames = new[] { AssetDatabase.LoadAssetAtPath<Sprite>(string.Format(framePattern, 0)), AssetDatabase.LoadAssetAtPath<Sprite>(string.Format(framePattern, 1)) };
        EditorUtility.SetDirty(pd);
        return pd;
    }

    /// <summary>Resources/AdConfig (varsayılan KAPALI, Google test kimlikleri). Docs/Reklam-Hazirlik.md</summary>
    public static string SetupAds()
    {
        const string path = "Assets/Resources/AdConfig.asset";
        if (AssetDatabase.LoadAssetAtPath<AdConfig>(path) == null)
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AdConfig>(), path);
            AssetDatabase.SaveAssets();
            return "[KakMetaSetup] AdConfig oluşturuldu (kapalı).";
        }
        return "[KakMetaSetup] AdConfig zaten var.";
    }
}
