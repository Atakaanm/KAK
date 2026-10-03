using UnityEditor;

/// <summary>
/// Faz 13: karanlık dünya kurulumları. K1: ışık/parıltı sprite'larının içe aktarımı (Resources; üretici tools/kak_gen_dark.py).
/// Tekrar çalıştırılabilir.
/// </summary>
public static class KakDarkSetup
{
    [MenuItem("KacAtaKac/Dünyalar/Karanlık Sprite'larını İçe Aktar")]
    public static string ImportSprites()
    {
        foreach (var n in new[] { "LightCircle", "LightEdge", "LightGlow", "Glint" })
            KakIceSetup.SpriteImport("Assets/Resources/" + n + ".png", 100f, false); // yumuşak: bilinear
        foreach (var n in new[] { "Torch", "GlowEyes" })
            KakIceSetup.SpriteImport("Assets/Resources/" + n + ".png", KakArtImportRules.ArtPixelPPU, true); // piksel
        AssetDatabase.SaveAssets();
        return "[KakDarkSetup] karanlık sprite'ları içe aktarıldı";
    }
}
