using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Sprite Atlas'ları kurar (performans bütçesi: az doku değişimi = az batch).
///  - KAK_PixelArt (Point): dungeon karoları, taş, karakter ve fırlatıcı kareleri
///  - KAK_Soft (Bilinear): ışık halesi, vinyet, gölge, powerup ikonları, joystick
/// Arena görseli (2048) atlasa girmez.
/// Menü: KacAtaKac/Sprite Atlaslarını Kur
/// </summary>
public static class KakAtlasSetup
{
    const string Dir = "Assets/Art/Atlas/";

    [MenuItem("KacAtaKac/Sprite Atlaslarını Kur")]
    public static void SetupMenu() => Debug.Log(Setup());

    public static string Setup()
    {
        // Atlaslar editörde de kullanılsın (ölçümler cihazla tutarlı olsun)
        EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
        System.IO.Directory.CreateDirectory(Dir);

        var pixel = new List<string>();
        pixel.AddRange(Sprites("Assets/Art/Tiles/Dungeon").Where(p => !IsSoft(p)));
        pixel.AddRange(Sprites("Assets/Art/Projectiles"));
        pixel.AddRange(Sprites("Assets/Sprites/Player").Where(p => !p.EndsWith("Black.png")));
        pixel.AddRange(Sprites("Assets/Sprites/Spawner"));
        pixel.AddRange(Sprites("Assets/Sprites/Characters"));   // karakter varyantları (Faz 3c.3)
        pixel.AddRange(Sprites("Assets/Art/Pickups"));          // altın (Faz 3c.1)
        pixel.AddRange(Sprites("Assets/Art/Pets"));             // petler (Faz 3c.5)

        var soft = new List<string>();
        soft.AddRange(Sprites("Assets/Art/Tiles/Dungeon").Where(IsSoft));
        soft.Add("Assets/Sprites/Player/Black.png");
        soft.AddRange(Sprites("Assets/Sprites/Powerups"));

        string a = Make("KAK_PixelArt", pixel, FilterMode.Point);
        string b = Make("KAK_Soft", soft, FilterMode.Bilinear);
        AssetDatabase.SaveAssets();
        SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
        return "[KakAtlasSetup] " + a + " " + b;
    }

    static bool IsSoft(string p)
    {
        string f = System.IO.Path.GetFileNameWithoutExtension(p).ToLowerInvariant();
        return f.Contains("glow") || f.Contains("vignette") || f.Contains("soft");
    }

    static IEnumerable<string> Sprites(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder)) return Enumerable.Empty<string>();
        return AssetDatabase.FindAssets("t:Sprite", new[] { folder })
            .Select(AssetDatabase.GUIDToAssetPath).Distinct();
    }

    static string Make(string name, List<string> paths, FilterMode filter)
    {
        string path = Dir + name + ".spriteatlas";
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
        if (atlas == null)
        {
            atlas = new SpriteAtlas();
            AssetDatabase.CreateAsset(atlas, path);
        }

        var packing = atlas.GetPackingSettings();
        packing.enableRotation = false;
        packing.enableTightPacking = false;
        packing.padding = 4;
        atlas.SetPackingSettings(packing);

        var tex = atlas.GetTextureSettings();
        tex.filterMode = filter;
        tex.generateMipMaps = false;
        tex.sRGB = true;
        atlas.SetTextureSettings(tex);

        var platform = atlas.GetPlatformSettings("DefaultTexturePlatform");
        platform.maxTextureSize = 2048;
        platform.textureCompression = TextureImporterCompression.Uncompressed;
        atlas.SetPlatformSettings(platform);

        // İçeriği baştan kur
        var existing = atlas.GetPackables();
        if (existing.Length > 0) atlas.Remove(existing);
        var objs = paths.Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p)).Where(s => s != null).Cast<Object>().ToArray();
        atlas.Add(objs);
        EditorUtility.SetDirty(atlas);
        return name + "(" + objs.Length + ")";
    }
}
