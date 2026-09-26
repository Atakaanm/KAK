using UnityEngine;

public enum AdProviderKind { None, Simulated, AdMob }
public enum AdPlacement { Revive, DoubleCoins }

/// <summary>
/// Reklam yapılandırması (Resources/AdConfig). Varsayılan KAPALI: reklam SDK'sı eklenip hesap kimlikleri girilene kadar
/// oyunda reklam butonu çıkmaz. Kimlikler varsayılan olarak Google'ın resmi TEST kimlikleridir (gerçek gelir değil).
/// Adım adım entegrasyon: Docs/Reklam-Hazirlik.md
/// </summary>
[CreateAssetMenu(fileName = "AdConfig", menuName = "KacAtaKac/Ad Config")]
public class AdConfig : ScriptableObject
{
    [Tooltip("Reklamlar açık mı. SDK + kimlikler + onay (UMP/ATT) hazır olmadan açma.")]
    public bool enabled = false;
    [Tooltip("None: reklam yok · Simulated: editör/dev build'de sahte reklam (akış testi) · AdMob: gerçek (KAK_ADMOB tanımı gerekir)")]
    public AdProviderKind provider = AdProviderKind.None;

    [Header("Yerleşimler (yalnızca ödüllü: oyuncu isterse)")]
    public bool reviveEnabled = true;
    [Tooltip("Oyun başına en fazla kaç kez 'reklam izle, devam et'")]
    public int maxRevivesPerRun = 1;
    [Tooltip("Devam teklifinin geri sayımı (sn)")]
    public float continueSeconds = 5f;
    public bool doubleCoinsEnabled = true;

    [Header("AdMob kimlikleri (varsayılan: Google TEST kimlikleri)")]
    public string androidAppId = "ca-app-pub-3940256099942544~3347511713";
    public string iosAppId = "ca-app-pub-3940256099942544~1458002511";
    public string androidRewardedUnit = "ca-app-pub-3940256099942544/5224354917";
    public string iosRewardedUnit = "ca-app-pub-3940256099942544/1712485313";

    [Header("iOS")]
    [TextArea] public string trackingDescription = "Reklamları sana daha uygun göstermek için bu izin kullanılır. İzin vermesen de oyun ve ödüllü reklamlar çalışır.";
    [Tooltip("Reklam ağının SKAdNetwork kimlikleri (AdMob listesi: developers.google.com/admob/ios/ios14)")]
    public string[] skAdNetworkIds = { "cstr6suwn9.skadnetwork" };

    static AdConfig cached;
    public static AdConfig Load()
    {
        if (cached == null) cached = Resources.Load<AdConfig>("AdConfig");
        return cached;
    }

    public string RewardedUnit =>
#if UNITY_IOS
        iosRewardedUnit;
#else
        androidRewardedUnit;
#endif
}
