using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sonsuz Mod içeriği: taş türleri (ProjectileData) ve zorluk kademelerine dağılımı.
/// Tekrar çalıştırılabilir; mevcut dosyaları günceller. Denge değerleri progress.md'de.
/// Menü: KacAtaKac/Taş Türlerini Kur
/// </summary>
public static class KakContentSetup
{
    const string Dir = "Assets/Data/Projectiles/";
    const string Prefab = "Assets/Prefabs/Projectile.prefab";
    const string Sprite = "Assets/Art/Projectiles/rock.png";

    [MenuItem("KacAtaKac/Taş Türlerini Kur")]
    public static void SetupMenu() => Debug.Log(Setup());

    public static string Setup()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data/Projectiles"))
            AssetDatabase.CreateFolder("Assets/Data", "Projectiles");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Sprite);

        // Temel taş (mevcut dosya)
        var rock = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Rock_ProjectileData.asset");
        Configure(rock, "Taş", ProjectileMotion.Straight, 1.5f, 1f, Color.white, prefab, sprite);

        var pebble = GetOrCreate("Pebble_Cakil");
        Configure(pebble, "Çakıl", ProjectileMotion.Straight, 2.1f, 0.75f, new Color(1f, 0.95f, 0.9f), prefab, sprite);

        var boulder = GetOrCreate("Boulder_Kaya");
        Configure(boulder, "Kaya", ProjectileMotion.Straight, 1.05f, 1.55f, new Color(0.85f, 0.8f, 0.8f), prefab, sprite);
        boulder.rotationSpeed = 45f;

        var bouncer = GetOrCreate("Bouncer_Seken");
        Configure(bouncer, "Seken Taş", ProjectileMotion.Bounce, 1.7f, 0.9f, new Color(0.8f, 0.9f, 1f), prefab, sprite);
        bouncer.bounces = 2;
        bouncer.lifeTime = 9f;
        bouncer.rotationSpeed = 540f;

        var splitter = GetOrCreate("Splitter_Parcalanan");
        Configure(splitter, "Parçalanan Taş", ProjectileMotion.Split, 1.35f, 1.25f, new Color(1f, 0.75f, 0.6f), prefab, sprite);
        splitter.splitAfter = 0.9f;
        splitter.splitCount = 3;
        splitter.splitSpread = 55f;
        splitter.splitInto = pebble;

        var meteor = GetOrCreate("Meteor_Goktasi");
        Configure(meteor, "Göktaşı", ProjectileMotion.Meteor, 0f, 1.25f, new Color(1f, 0.85f, 0.75f), prefab, sprite);
        meteor.meteorFallTime = 1.15f;
        meteor.meteorStartHeight = 7f;
        meteor.meteorRadius = 0.55f;

        var homing = GetOrCreate("Homing_Gudumlu");
        Configure(homing, "Güdümlü Taş", ProjectileMotion.Homing, 1.45f, 0.95f, new Color(0.95f, 0.8f, 1f), prefab, sprite);
        homing.homingDuration = 1.1f;
        homing.homingTurnRate = 100f;

        // Kademe dağılımı (Faz 5 planı: kademeli tanıtım)
        Assign("Assets/Data/Stage1_Baslangic.asset", (rock, 1f));
        Assign("Assets/Data/Stage2_Kolay.asset", (rock, 3f), (pebble, 1.5f), (boulder, 1f));
        Assign("Assets/Data/Stage3_Orta.asset", (rock, 3f), (pebble, 2f), (boulder, 1f), (bouncer, 1.2f));
        Assign("Assets/Data/Stage4_Zor.asset", (rock, 2.5f), (pebble, 2f), (boulder, 1f), (bouncer, 1.2f), (splitter, 1f), (meteor, 0.8f));
        Assign("Assets/Data/Stage5_Cehennem.asset", (rock, 2f), (pebble, 2f), (boulder, 1.2f), (bouncer, 1.3f), (splitter, 1.2f), (meteor, 1f), (homing, 0.8f));
        Assign("Assets/Data/Stage6_Imkansiz.asset", (rock, 1.5f), (pebble, 2f), (boulder, 1.3f), (bouncer, 1.5f), (splitter, 1.5f), (meteor, 1.3f), (homing, 1.2f));

        AssetDatabase.SaveAssets();
        return "[KakContentSetup] 7 taş türü ve 6 kademe dağılımı güncellendi.";
    }

    static ProjectileData GetOrCreate(string name)
    {
        string path = Dir + name + ".asset";
        var d = AssetDatabase.LoadAssetAtPath<ProjectileData>(path);
        if (d == null)
        {
            d = ScriptableObject.CreateInstance<ProjectileData>();
            AssetDatabase.CreateAsset(d, path);
        }
        return d;
    }

    static void Configure(ProjectileData d, string name, ProjectileMotion motion, float speed, float scale, Color tint,
                          GameObject prefab, Sprite sprite)
    {
        if (d == null) return;
        d.projectileName = name;
        d.motion = motion;
        d.speed = speed;
        d.visualScale = scale;
        d.tint = tint;
        d.damage = 1;
        d.lifeTime = motion == ProjectileMotion.Bounce ? 9f : 6f;
        d.projectilePrefab = prefab;
        d.projectileSprite = sprite;
        EditorUtility.SetDirty(d);
    }

    static void Assign(string stagePath, params (ProjectileData data, float weight)[] items)
    {
        var st = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(stagePath);
        if (st == null) { Debug.LogWarning("[KakContentSetup] Kademe yok: " + stagePath); return; }
        var list = new List<ProjectileData>();
        var w = new List<float>();
        foreach (var (data, weight) in items) { if (data != null) { list.Add(data); w.Add(weight); } }
        st.availableProjectiles = list.ToArray();
        st.projectileWeights = w.ToArray();
        EditorUtility.SetDirty(st);
    }
}
