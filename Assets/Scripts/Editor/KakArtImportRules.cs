using UnityEditor;
using UnityEngine;

/// <summary>
/// Assets/Art/ altındaki görseller için otomatik içe aktarma standardı (sanat-rehberi.md §1).
/// - Piksel sanatı: Point filtre, sıkıştırma yok, mipmap yok, Full Rect mesh (Tiled çizim için)
/// - Yumuşak efekt dokuları (dosya adında "glow", "vignette", "soft"): Bilinear
/// - PPU: piksel sanatı 41.667 (1 sanat pikseli = 0.024 dünya birimi; karakter ve arena ile aynı yoğunluk),
///   yumuşak efektler ve UI 100.
/// </summary>
public class KakArtImportRules : AssetPostprocessor
{
    const string ArtRoot = "Assets/Art/";
    public const float ArtPixelPPU = 41.6667f;

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ArtRoot)) return;
        var ti = (TextureImporter)assetImporter;
        string file = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
        bool soft = file.Contains("glow") || file.Contains("vignette") || file.Contains("soft");

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.filterMode = soft ? FilterMode.Bilinear : FilterMode.Point;
        ti.wrapMode = TextureWrapMode.Clamp;

        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteExtrude = 0;
        ti.SetTextureSettings(settings);

        // Standart: 1 sanat pikseli = 0.024 dünya birimi (karakterlerle aynı yoğunluk). Yumuşak efektler ve UI 100.
        bool ui = assetPath.StartsWith(ArtRoot + "UI/");
        ti.spritePixelsPerUnit = (soft || ui) ? 100f : ArtPixelPPU;
    }

    /// <summary>Kurallar değişince veya dosyalar kuraldan önce eklendiyse klasörü yeniden içe aktarır.</summary>
    [MenuItem("KacAtaKac/Art Klasörünü Yeniden İçe Aktar")]
    public static string ReimportArt()
    {
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" });
        foreach (var g in guids)
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(g), ImportAssetOptions.ForceUpdate);
        return "[KakArtImportRules] " + guids.Length + " doku yeniden içe aktarıldı.";
    }
}
