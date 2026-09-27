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
}
