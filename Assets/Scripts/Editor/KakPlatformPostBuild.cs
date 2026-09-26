// Mağaza gereksinimleri için derleme sonrası düzeltmeler (Docs/Yayin-Gereksinimleri.md).
// İlgili Unity modülü (Android / iOS Build Support) kurulu ve hedef platform seçiliyken derlenir.
#if UNITY_ANDROID
using System.IO;
using System.Xml;
using UnityEditor.Android;

/// <summary>
/// Android 16 (API 36): tablet ve katlanabilirlerde (≥600dp) yön kilidi yok sayılır; oyunlar
/// android:appCategory="game" ile muaf. Dikey oyunun büyük ekranda bozulmaması için launcher manifest'ine eklenir.
/// </summary>
public class KakAndroidManifest : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 100;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        const string ns = "http://schemas.android.com/apk/res/android";
        foreach (var rel in new[] { "../launcher/src/main/AndroidManifest.xml", "src/main/AndroidManifest.xml" })
        {
            string file = Path.GetFullPath(Path.Combine(path, rel));
            if (!File.Exists(file)) continue;
            var doc = new XmlDocument();
            doc.Load(file);
            var app = doc.SelectSingleNode("/manifest/application") as XmlElement;
            if (app == null) continue;
            app.SetAttribute("appCategory", ns, "game");
            doc.Save(file);
        }
    }
}
#endif

#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

/// <summary>
/// iOS Info.plist: şifreleme ihracat beyanı (yalnızca muaf şifreleme: HTTPS) → App Store Connect her yüklemede sormaz.
/// Reklam açıksa (AdConfig) ATT açıklaması ve SKAdNetwork kimlikleri eklenir.
/// </summary>
public static class KakIosPostBuild
{
    [PostProcessBuild(100)]
    public static void OnPostprocessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;
        string plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        var root = plist.root;
        root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        var ads = UnityEngine.Resources.Load<AdConfig>("AdConfig");
        if (ads != null && ads.enabled && ads.provider != AdProviderKind.None)
        {
            root.SetString("NSUserTrackingUsageDescription", ads.trackingDescription);
            if (ads.skAdNetworkIds != null && ads.skAdNetworkIds.Length > 0)
            {
                var arr = root.CreateArray("SKAdNetworkItems");
                foreach (var id in ads.skAdNetworkIds)
                    arr.AddDict().SetString("SKAdNetworkIdentifier", id);
            }
            if (!string.IsNullOrEmpty(ads.iosAppId)) root.SetString("GADApplicationIdentifier", ads.iosAppId);
        }
        plist.WriteToFile(plistPath);
    }
}
#endif
