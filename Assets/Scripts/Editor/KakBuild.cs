using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Yayın ayarları ve build'ler (Faz 10, güncelleme 2026-09-26: Docs/Yayin-Gereksinimleri.md).
///  - ConfigurePlayer: şirket/ürün adı, paket kimliği, sürüm (+ sürüm kodu), dikey kilit, Android API 36 + AAB + ARM64,
///    iOS yalnızca iPhone (iPadOS 26 tüm yönleri istiyor), alt kenar hareketi ertelenir, ikon, açılış ekranı
///  - BuildAndroidRelease (.aab, imza: senin keystore'un), BuildIosProject (Xcode projesi), BuildMacDev (ölçüm)
/// Menü: KacAtaKac/Yayın/...
/// </summary>
public static class KakBuild
{
    public const string BundleId = "com.atakaan.kacatakac";
    /// <summary>Mağaza sürümü. Her yüklemede artır; sürüm kodu buradan türetilir (1.0.0 → 10000).</summary>
    public const string Version = "1.0.0";

    public static int VersionCode(string v)
    {
        var p = v.Split('.');
        int major = int.Parse(p[0]), minor = p.Length > 1 ? int.Parse(p[1]) : 0, patch = p.Length > 2 ? int.Parse(p[2]) : 0;
        return major * 10000 + minor * 100 + patch;
    }

    [MenuItem("KacAtaKac/Yayın/Oyuncu Ayarlarını Uygula")]
    public static string ConfigurePlayer()
    {
        PlayerSettings.companyName = "Atakaan";
        PlayerSettings.productName = "Kaç Ata Kaç";
        PlayerSettings.bundleVersion = Version;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, BundleId);
        PlayerSettings.Android.bundleVersionCode = VersionCode(Version);
        PlayerSettings.iOS.buildNumber = VersionCode(Version).ToString();

        // Sadece dikey
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.statusBarHidden = true;

        // Android: IL2CPP + ARM64 (64-bit zorunlu), min Android 7.1, hedef API 36 (31.08.2026'dan beri zorunlu),
        // App Bundle (.aab), kare zamanlaması düzgün (titreme az). 16 KB sayfa uyumu Unity 6.3'te hazır.
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
        PlayerSettings.Android.optimizedFramePacing = true;
        EditorUserBuildSettings.buildAppBundle = true;

        // iOS: yalnızca iPhone (iPadOS 26'da UIRequiresFullScreen yok sayılıyor; iPad sürümü tüm yönleri desteklemeli.
        // iPhone uygulaması iPad'de uyumluluk modunda çalışır). Joystick alt kenarda: ana ekran hareketi ertelenir.
        PlayerSettings.iOS.targetOSVersionString = "15.0"; // Unity 6.3 varsayılanı (desteklenen en düşük)
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneOnly;
        PlayerSettings.iOS.deferSystemGesturesMode = UnityEngine.iOS.SystemGestureDeferMode.BottomEdge;
        PlayerSettings.iOS.hideHomeButton = false;
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;

        // İkon
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Icon/app_icon.png");
        if (icon != null) PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);

        // Açılış ekranı: koyu zemin
        PlayerSettings.SplashScreen.backgroundColor = KakPalette.Murekkep;
        PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;

        AssetDatabase.SaveAssets();
        return "[KakBuild] Oyuncu ayarları uygulandı: " + PlayerSettings.productName + " " + PlayerSettings.bundleVersion + " (" + BundleId + ")";
    }

    static string[] Scenes() => EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

    /// <summary>
    /// Google Play için .aab. Önce Android Build Support modülü (👤) ve keystore (👤: Player Settings → Publishing
    /// Settings → Keystore Manager; şifreleri Claude görmez). Keystore yoksa Unity hata ile durur.
    /// </summary>
    [MenuItem("KacAtaKac/Yayın/Android Release (.aab)")]
    public static string BuildAndroidRelease()
    {
        ConfigurePlayer();
        var opts = new BuildPlayerOptions
        {
            scenes = Scenes(),
            locationPathName = "Builds/Android/KacAtaKac-" + Version + ".aab",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        return Report("Android .aab", BuildPipeline.BuildPlayer(opts));
    }

    /// <summary>App Store için Xcode projesi (Builds/iOS). Sonra Xcode 26 ile Archive → App Store Connect (👤).</summary>
    [MenuItem("KacAtaKac/Yayın/iOS Xcode Projesi")]
    public static string BuildIosProject()
    {
        ConfigurePlayer();
        var opts = new BuildPlayerOptions
        {
            scenes = Scenes(),
            locationPathName = "Builds/iOS",
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };
        return Report("iOS Xcode", BuildPipeline.BuildPlayer(opts));
    }

    static string Report(string what, BuildReport report)
    {
        var s = report.summary;
        return "[KakBuild] " + what + ": " + s.result + " | " + s.totalSize / (1024 * 1024) + " MB | " + s.totalTime.TotalSeconds.ToString("F0") + " sn | hata " + s.totalErrors;
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
