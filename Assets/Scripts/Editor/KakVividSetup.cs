using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Faz 15 K2: "cafcaflı piksel" — renk tonuna göre seçici canlılık (URP Color Curves · Hue vs Sat): sıcak tonlar (taş, altın,
/// tehlike), camgöbeği (iyi) ve pembe/mor canlanır; yeşil zemin bir tık geri çekilir (değer hiyerarşisi korunur). Seviye 2'de bloom güçlenir.
/// Seviye kullanıcı onayıyla seçilir (sanat-rehberi: görsel karar kullanıcının). Köprü: invoke KakVividSetup SetLevel 1
/// </summary>
public static class KakVividSetup
{
    const string ProfilePath = "Assets/Settings/GameVolumeProfile.asset";

    public static string SetLevel(string level)
    {
        int lv = Mathf.Clamp(int.Parse(level), 0, 2);
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null) return "profil yok: " + ProfilePath;

        if (!profile.TryGet(out ColorCurves cc)) cc = profile.Add<ColorCurves>(true);
        if (lv == 0)
        {
            cc.active = false;
        }
        else
        {
            cc.active = true;
            // y: 0,5 = değişmez, 1 = 2× doygunluk. Ton: 0 kırmızı · 0,08 turuncu · 0,13 altın · 0,33 yeşil · 0,5 camgöbeği · 0,83 pembe
            float b = lv == 1 ? 0.75f : 0.95f;   // sıcak/altın
            float c = lv == 1 ? 0.68f : 0.85f;   // camgöbeği, pembe
            float g = lv == 1 ? 0.45f : 0.42f;   // yeşil zemin geri
            var keys = new[]
            {
                new Keyframe(0.00f, b), new Keyframe(0.14f, b), new Keyframe(0.21f, 0.5f), new Keyframe(0.27f, g), new Keyframe(0.41f, g),
                new Keyframe(0.46f, 0.5f), new Keyframe(0.51f, c), new Keyframe(0.57f, 0.5f), new Keyframe(0.74f, 0.5f), new Keyframe(0.84f, c),
                new Keyframe(0.93f, b),
            };
            var curve = new TextureCurve(keys, 0.5f, true, new Vector2(0f, 1f));
            cc.hueVsSat.Override(curve);
        }
        if (profile.TryGet(out Bloom bloom)) bloom.intensity.Override(lv == 2 ? 0.6f : 0.35f);
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        string msg = "canlılık seviyesi " + lv + (lv == 0 ? " (şimdiki)" : lv == 1 ? " (canlı)" : " (çok canlı + parlama)");
        Debug.Log("[KakVividSetup] " + msg);
        return msg;
    }
}
