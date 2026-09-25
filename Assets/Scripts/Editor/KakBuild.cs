using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Yayın ayarları ve build'ler (Faz 10).
///  - ConfigurePlayer: şirket/ürün adı, paket kimliği, sürüm, dikey kilit, IL2CPP + ARM64, ikon, açılış ekranı
///  - BuildMacDev: performans ölçümü için macOS development build (Builds/Mac)
/// Menü: KacAtaKac/Yayın/...
/// </summary>
public static class KakBuild
{
    public const string BundleId = "com.atakaan.kacatakac";

    [MenuItem("KacAtaKac/Yayın/Oyuncu Ayarlarını Uygula")]
    public static string ConfigurePlayer()
    {
        PlayerSettings.companyName = "Atakaan";
        PlayerSettings.productName = "Kaç Ata Kaç";
        PlayerSettings.bundleVersion = "0.9.0";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, BundleId);
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.iOS.buildNumber = "1";

        // Sadece dikey
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.statusBarHidden = true;

        // Android: IL2CPP + ARM64 (Google Play gereği), minimum Android 7.0
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.iOS.targetOSVersionString = "14.0";

        // İkon
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Icon/app_icon.png");
        if (icon != null) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);

        // Açılış ekranı: koyu zemin
        PlayerSettings.SplashScreen.backgroundColor = KakPalette.Murekkep;
        PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;

        AssetDatabase.SaveAssets();
        return "[KakBuild] Oyuncu ayarları uygulandı: " + PlayerSettings.productName + " " + PlayerSettings.bundleVersion + " (" + BundleId + ")";
    }

    [MenuItem("KacAtaKac/Yayın/macOS Development Build (ölçüm)")]
    public static string BuildMacDev()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Mac/KacAtaKac.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.Development
        };
        var report = BuildPipeline.BuildPlayer(opts);
        var s = report.summary;
        return "[KakBuild] macOS build: " + s.result + " | " + s.totalSize / (1024 * 1024) + " MB | " + s.totalTime.TotalSeconds.ToString("F0") + " sn | hata " + s.totalErrors;
    }
}
