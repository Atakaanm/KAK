using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Oyunun TMP fontlarını statik atlasa "pişirir": gereken tüm karakterler (ASCII + Türkçe + tüm Loc
/// metinleri) önceden atlasa eklenir, font Static moda alınır.
/// Kazanç: çalışma anında glif üretimi yok (mobilde açılış süresi), editörde Play sonrası font
/// dosyaları değişmez (git gürültüsü biter). Loc'a yeni karakter eklenirse aracı tekrar çalıştır;
/// `FontTests` eksik karakteri yakalar.
/// Menü: KacAtaKac/Fontları Statik Atlasa Pişir · Köprü: invoke KakFontSetup Bake
/// </summary>
public static class KakFontSetup
{
    public static readonly string[] FontPaths =
    {
        "Assets/Fonts/Nunito-ExtraBold SDF.asset",
        "Assets/Fonts/CinzelDecorative-Bold SDF.asset",
    };

    /// <summary>Atlasta bulunması gereken karakterler (sıralı, tekrarsız).</summary>
    public static string Charset()
    {
        var set = new SortedSet<char>();
        for (char c = (char)32; c <= (char)126; c++) set.Add(c);          // ASCII
        foreach (char c in "ÇçĞğİıÖöŞşÜüÂâÎîÛû") set.Add(c);              // Türkçe
        foreach (char c in "•×…–—‘’“”→★") set.Add(c);                      // tipografi
        foreach (char c in Loc.AllCharacters()) if (!char.IsControl(c)) set.Add(c);
        var sb = new StringBuilder(set.Count);
        foreach (char c in set) sb.Append(c);
        return sb.ToString();
    }

    [MenuItem("KacAtaKac/Fontları Statik Atlasa Pişir")]
    public static void BakeMenu() => Debug.Log(Bake());

    public static string Bake()
    {
        string chars = Charset();
        var log = new StringBuilder("[KakFontSetup] karakter: " + chars.Length + "\n");
        foreach (var path in FontPaths)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null) { log.AppendLine("YOK: " + path); continue; }

            font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            font.ClearFontAssetData(false);
            font.TryAddCharacters(chars, out string missing, true);

            // Fontun kendisinde olmayan karakterler (ör. Cinzel'de ★) sorun değil; atlasa sığmayanlar sorun
            var notInFace = new StringBuilder();
            var noRoom = new StringBuilder();
            if (!string.IsNullOrEmpty(missing))
                foreach (char c in missing)
                    (font.HasCharacter(c, false, false) ? noRoom : (font.sourceFontFile != null && !FaceHas(font, c) ? notInFace : noRoom)).Append(c);

            font.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(font);
            if (font.atlasTexture != null) EditorUtility.SetDirty(font.atlasTexture);
            if (font.material != null) EditorUtility.SetDirty(font.material);

            log.AppendLine($"{font.name}: glif {font.characterTable.Count}, atlas {font.atlasTextures.Length} x {font.atlasWidth}x{font.atlasHeight}"
                           + (notInFace.Length > 0 ? $", fontta yok: '{notInFace}'" : "")
                           + (noRoom.Length > 0 ? $", SIĞMADI: '{noRoom}'" : ""));
        }
        SetTmpDefaults(log);
        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    /// <summary>
    /// TMP varsayılan fontu = Nunito (fontu atanmamış yazı LiberationSans'a düşmesin);
    /// genel yedek = LiberationSans (statik atlasta olmayan karakter □ yerine yine de çizilsin).
    /// </summary>
    static void SetTmpDefaults(StringBuilder log)
    {
        var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Resources/TMP Settings.asset");
        var nunito = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPaths[0]);
        var liberation = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (settings == null || nunito == null) { log.AppendLine("TMP Settings bulunamadı"); return; }
        var so = new SerializedObject(settings);
        so.FindProperty("m_defaultFontAsset").objectReferenceValue = nunito;
        var fb = so.FindProperty("m_fallbackFontAssets");
        if (liberation != null)
        {
            fb.arraySize = 1;
            fb.GetArrayElementAtIndex(0).objectReferenceValue = liberation;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        log.AppendLine("TMP varsayılan font: Nunito, yedek: LiberationSans");
    }

    /// <summary>Kaynak fontun bu karakter için glifi var mı (atlas yerinden bağımsız).</summary>
    static bool FaceHas(TMP_FontAsset font, char c)
    {
        UnityEngine.TextCore.LowLevel.FontEngine.LoadFontFace(font.sourceFontFile, Mathf.RoundToInt(font.faceInfo.pointSize));
        return UnityEngine.TextCore.LowLevel.FontEngine.TryGetGlyphWithUnicodeValue(c, UnityEngine.TextCore.LowLevel.GlyphLoadFlags.LOAD_NO_BITMAP, out var g)
               && g != null && g.index != 0;
    }
}
