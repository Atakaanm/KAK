using System;
using UnityEngine;

/// <summary>Reklam sağlayıcı arayüzü (AdMob, simülasyon, yok). Yalnızca ödüllü reklam: oyuncu kendisi ister.</summary>
public interface IAdProvider
{
    void Initialize(Action<bool> done);
    bool IsRewardedReady { get; }
    void LoadRewarded();
    /// <summary>Reklamı gösterir; sonuç: true = ödül kazanıldı (izlendi), false = kapatıldı/hata.</summary>
    void ShowRewarded(Action<bool> result);
}

/// <summary>
/// Reklam servisi (Faz Y3, hazırlık). Oyun kodu yalnızca bunu çağırır: CanShow(yerleşim) → ShowRewarded(...).
/// Sağlayıcı AdConfig'ten seçilir; varsayılan kapalı (NullAdProvider). Oyun başına sınırlar burada tutulur.
/// Gerçek AdMob: Docs/Reklam-Hazirlik.md (SDK + KAK_ADMOB tanımı + AdMobProvider).
/// </summary>
public static class AdService
{
    static IAdProvider provider;
    static bool initialized;
    public static int RevivesThisRun { get; private set; }
    public static bool DoubleCoinsUsedThisRun { get; private set; }
    public static bool Showing { get; private set; }

    /// <summary>Testler: sağlayıcıyı ve yapılandırmayı zorla (null = normal).</summary>
    public static IAdProvider OverrideProvider;
    public static AdConfig OverrideConfig;

    public static AdConfig Config => OverrideConfig != null ? OverrideConfig : AdConfig.Load();

    public static IAdProvider Provider
    {
        get
        {
            if (OverrideProvider != null) return OverrideProvider;
            if (provider == null) provider = Create();
            return provider;
        }
    }

    static IAdProvider Create()
    {
        var c = Config;
        if (c == null || !c.enabled) return new NullAdProvider();
        switch (c.provider)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            case AdProviderKind.Simulated: return new SimulatedAdProvider();
#endif
#if KAK_ADMOB
            case AdProviderKind.AdMob: return new AdMobProvider(c);
#endif
            default: return new NullAdProvider();
        }
    }

    /// <summary>Açılışta bir kez (KakBootstrap). Onay (UMP/ATT) gerçek sağlayıcının Initialize'ında.</summary>
    public static void Initialize()
    {
        if (initialized) return;
        initialized = true;
        Provider.Initialize(ok => { if (ok) Provider.LoadRewarded(); });
    }

    /// <summary>Her yeni oyunda (GameManager.Awake) sınırları sıfırla.</summary>
    public static void BeginRun()
    {
        RevivesThisRun = 0;
        DoubleCoinsUsedThisRun = false;
    }

    public static bool CanShow(AdPlacement p)
    {
        var c = Config;
        if (c == null || !c.enabled || Showing) return false;
        switch (p)
        {
            case AdPlacement.Revive: if (!c.reviveEnabled || RevivesThisRun >= c.maxRevivesPerRun) return false; break;
            case AdPlacement.DoubleCoins: if (!c.doubleCoinsEnabled || DoubleCoinsUsedThisRun) return false; break;
        }
        return Provider.IsRewardedReady;
    }

    /// <summary>Ödüllü reklam. onReward yalnızca reklam izlenince; aksi halde onFail. Sonra yeni reklam yüklenir.</summary>
    public static void ShowRewarded(AdPlacement p, Action onReward, Action onFail)
    {
        if (!CanShow(p)) { onFail?.Invoke(); return; }
        Showing = true;
        Provider.ShowRewarded(ok =>
        {
            Showing = false;
            if (ok)
            {
                if (p == AdPlacement.Revive) RevivesThisRun++;
                else if (p == AdPlacement.DoubleCoins) DoubleCoinsUsedThisRun = true;
                onReward?.Invoke();
            }
            else onFail?.Invoke();
            Provider.LoadRewarded();
        });
    }

    /// <summary>Testler için sıfırla.</summary>
    public static void ResetForTests()
    {
        OverrideProvider = null;
        OverrideConfig = null;
        provider = null;
        initialized = false;
        Showing = false;
        BeginRun();
    }
}

/// <summary>Reklam yok (varsayılan): hiçbir zaman hazır değil → oyunda reklam butonu çıkmaz.</summary>
public class NullAdProvider : IAdProvider
{
    public void Initialize(Action<bool> done) => done?.Invoke(false);
    public bool IsRewardedReady => false;
    public void LoadRewarded() { }
    public void ShowRewarded(Action<bool> result) => result?.Invoke(false);
}
