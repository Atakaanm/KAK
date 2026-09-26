using NUnit.Framework;
using TMPro;
using UnityEditor;

/// <summary>Denetim D3: oyunun fontları statik atlasta ve tüm metin karakterlerini içeriyor.</summary>
public class FontTests
{
    [Test]
    public void Fontlar_Statik_VeTumLocKarakterleriAtlasta()
    {
        foreach (var path in new[] { "Assets/Fonts/Nunito-ExtraBold SDF.asset" })
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            Assert.IsNotNull(font, path);
            Assert.AreEqual(AtlasPopulationMode.Static, font.atlasPopulationMode, path + " statik değil (KacAtaKac/Fontları Statik Atlasa Pişir)");
            string chars = Loc.AllCharacters() + "ÇçĞğİıÖöŞşÜü0123456789";
            var missing = new System.Text.StringBuilder();
            foreach (char c in chars)
                if (!char.IsWhiteSpace(c) && !font.HasCharacter(c, false, false) && missing.ToString().IndexOf(c) < 0) missing.Append(c);
            Assert.AreEqual("", missing.ToString(), path + " atlasında eksik karakter var → aracı yeniden çalıştır");
        }
    }

    [Test]
    public void TmpVarsayilanFont_Nunito()
    {
        var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Resources/TMP Settings.asset");
        var so = new SerializedObject(settings);
        var f = so.FindProperty("m_defaultFontAsset").objectReferenceValue as TMP_FontAsset;
        Assert.IsNotNull(f);
        StringAssert.StartsWith("Nunito", f.name);
    }
}
