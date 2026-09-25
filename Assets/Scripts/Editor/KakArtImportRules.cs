using UnityEditor;
using UnityEngine;

/// <summary>
/// Assets/Art/ altındaki görseller için otomatik içe aktarma standardı (sanat-rehberi.md §1).
/// - Piksel sanatı: Point filtre, sıkıştırma yok, mipmap yok, Full Rect mesh (Tiled çizim için)
/// - Yumuşak efekt dokuları (dosya adında "glow", "vignette", "soft"): Bilinear
/// - PPU: Tiles/Dungeon geçici karoları arena ölçeğinde 41.667; diğerleri şimdilik 100
///   (Faz 3'te tüm proje tek PPU standardına geçecek).
/// </summary>
public class KakArtImportRules : AssetPostprocessor
{
    const string ArtRoot = "Assets/Art/";
    public const float DungeonTilePPU = 41.6667f;

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

        if (assetPath.StartsWith(ArtRoot + "Tiles/Dungeon/") && !soft)
            ti.spritePixelsPerUnit = DungeonTilePPU;
        else
            ti.spritePixelsPerUnit = 100f;
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
