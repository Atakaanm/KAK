using UnityEditor;
using UnityEngine;

/// <summary>
/// Denge sayıları (Faz 11): elle oluşturulmuş veri dosyalarındaki değerleri tek yerden ayarlar. Tekrar çalıştırılabilir.
/// Karakter tablosu KakMetaSetup'ta, zorluk kademeleri KakEndlessSetup'ta; burada kalanlar (güçlendirmeler...).
/// </summary>
public static class KakBalance
{
    [MenuItem("KacAtaKac/Denge/Güçlendirmeleri Ayarla")]
    public static string ApplyPowerups()
    {
        // G1: hız güçlendirmesi ×1,5 telefonda "troll" gibiydi
        var speed = AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/SpeedData.asset");
        if (speed == null) return "HATA: SpeedData yok";
        speed.powerMultiplier = 1.25f;
        speed.duration = 5f;
        EditorUtility.SetDirty(speed);
        AssetDatabase.SaveAssets();
        return "[KakBalance] Hız güçlendirmesi ×1,25 / 5 sn";
    }

    /// <summary>G1: göktaşlarına "!" uyarısı (Assets/Art/Projectiles/warn_mark.png).</summary>
    [MenuItem("KacAtaKac/Denge/Göktaşı Uyarısını Bağla")]
    public static string ApplyMeteorWarning()
    {
        var mark = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Projectiles/warn_mark.png");
        if (mark == null) return "HATA: warn_mark.png yok";
        int n = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:ProjectileData"))
        {
            var d = AssetDatabase.LoadAssetAtPath<ProjectileData>(AssetDatabase.GUIDToAssetPath(guid));
            if (d == null || d.motion != ProjectileMotion.Meteor) continue;
            d.warningSprite = mark;
            EditorUtility.SetDirty(d);
            n++;
        }
        AssetDatabase.SaveAssets();
        return "[KakBalance] " + n + " göktaşına uyarı işareti bağlandı";
    }

    /// <summary>
    /// G4: Pranga (kötü eşya) — ikon içe aktarma, ShackleData, ShacklePickup prefab'ı (kalp prefab'ından), Sonsuz bölüme ekleme.
    /// Alınırsa 4 sn %40 yavaşlatır; Kolay kademeden sonra seyrek çıkar.
    /// </summary>
    [MenuItem("KacAtaKac/Denge/Prangayı Kur")]
    public static string SetupShackle()
    {
        const string IconPath = "Assets/Sprites/Powerups/ShackleIcon.png";
        const string DataPath = "Assets/Data/Powerups/ShackleData.asset";
        const string PrefabPath = "Assets/Prefabs/Powerups/ShacklePickup.prefab";

        // İkon: kalp ikonuyla aynı içe aktarma
        var ti = AssetImporter.GetAtPath(IconPath) as TextureImporter;
        var heartTi = AssetImporter.GetAtPath("Assets/Sprites/Powerups/HeartIcon.png") as TextureImporter;
        if (ti == null || heartTi == null) return "HATA: ikon içe aktarılamadı";
        var settings = new TextureImporterSettings();
        heartTi.ReadTextureSettings(settings);
        ti.SetTextureSettings(settings);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.SaveAndReimport();
        var icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);

        var data = AssetDatabase.LoadAssetAtPath<PowerupData>(DataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<PowerupData>();
            AssetDatabase.CreateAsset(data, DataPath);
        }
        data.powerupName = "Shackle";
        data.type = PowerupType.Shackle;
        data.duration = 4f;
        data.powerMultiplier = 0.6f;
        data.healthAmount = 0;
        data.spawnChanceWeight = 0.7f;
        data.minStage = 1;
        data.harmful = true;
        data.icon = icon;

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            AssetDatabase.CopyAsset("Assets/Prefabs/Powerups/HeartPickup.prefab", PrefabPath);
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        root.name = "ShacklePickup";
        foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true)) if (sr.sprite != null && sr.sprite.name.Contains("Heart")) sr.sprite = icon;
        var pick = root.GetComponentInChildren<PowerupPickup>(true);
        if (pick != null) pick.powerupData = data;
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        data.visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        EditorUtility.SetDirty(data);

        var level = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Endless_Level1_LevelData.asset");
        if (level != null && System.Array.IndexOf(level.availablePowerups, data) < 0)
        {
            var list = new System.Collections.Generic.List<PowerupData>(level.availablePowerups) { data };
            level.availablePowerups = list.ToArray();
            EditorUtility.SetDirty(level);
        }
        AssetDatabase.SaveAssets();
        return "[KakBalance] Pranga kuruldu (" + (level != null ? level.availablePowerups.Length : 0) + " eşya Sonsuz bölümde)";
    }

    /// <summary>
    /// Faz 12 H4: Görünmezlik eşyası (fırlatıcılar göremez, rastgele atar). İkon/işaret: tools/kak_gen_invisible.py.
    /// Zindan ve Buz sonsuz bölümlerine eklenir. Hayalet'ten ayrı (o: taşlar içinden geçer).
    /// </summary>
    [MenuItem("KacAtaKac/Denge/Görünmezliği Kur")]
    public static string SetupInvisible()
    {
        const string IconPath = "Assets/Sprites/Powerups/InvisibleIcon.png";
        const string DataPath = "Assets/Data/Powerups/InvisibleData.asset";
        const string PrefabPath = "Assets/Prefabs/Powerups/InvisiblePickup.prefab";

        var ti = AssetImporter.GetAtPath(IconPath) as TextureImporter;
        var heartTi = AssetImporter.GetAtPath("Assets/Sprites/Powerups/HeartIcon.png") as TextureImporter;
        if (ti == null || heartTi == null) return "HATA: ikon yok (önce tools/kak_gen_invisible.py)";
        var settings = new TextureImporterSettings();
        heartTi.ReadTextureSettings(settings);
        ti.SetTextureSettings(settings);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.SaveAndReimport();
        var mark = AssetImporter.GetAtPath("Assets/Resources/QuestionMark.png") as TextureImporter;
        if (mark != null)
        {
            mark.textureType = TextureImporterType.Sprite;
            mark.spriteImportMode = SpriteImportMode.Single;
            mark.spritePixelsPerUnit = KakArtImportRules.ArtPixelPPU; // oyun dünyasıyla aynı piksel yoğunluğu
            mark.filterMode = FilterMode.Point;
            mark.textureCompression = TextureImporterCompression.Uncompressed;
            mark.mipmapEnabled = false;
            mark.SaveAndReimport();
        }
        var icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);

        var data = AssetDatabase.LoadAssetAtPath<PowerupData>(DataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<PowerupData>();
            AssetDatabase.CreateAsset(data, DataPath);
        }
        data.powerupName = "Invisible";
        data.type = PowerupType.Invisible;
        data.duration = 5f;
        data.powerMultiplier = 1f;
        data.healthAmount = 0;
        data.spawnChanceWeight = 0.5f;
        data.minStage = 1; // ilk saniyelerde değil, Kolay'dan sonra
        data.harmful = false;
        data.icon = icon;

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            AssetDatabase.CopyAsset("Assets/Prefabs/Powerups/HeartPickup.prefab", PrefabPath);
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        root.name = "InvisiblePickup";
        foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true)) if (sr.sprite != null && sr.sprite.name.Contains("Heart")) sr.sprite = icon;
        var pick = root.GetComponentInChildren<PowerupPickup>(true);
        if (pick != null) pick.powerupData = data;
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        data.visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        EditorUtility.SetDirty(data);

        int added = 0;
        foreach (var path in new[] { "Assets/Data/Endless_Level1_LevelData.asset", "Assets/Data/Worlds/Ice/Endless_Ice_LevelData.asset" })
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (level == null || System.Array.IndexOf(level.availablePowerups, data) >= 0) continue;
            var list = new System.Collections.Generic.List<PowerupData>(level.availablePowerups) { data };
            level.availablePowerups = list.ToArray();
            EditorUtility.SetDirty(level);
            added++;
        }
        AssetDatabase.SaveAssets();
        return "[KakBalance] Görünmezlik kuruldu (" + added + " bölüme eklendi)";
    }
}
