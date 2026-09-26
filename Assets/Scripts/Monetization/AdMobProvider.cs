#if KAK_ADMOB
// Gerçek AdMob bağdaştırıcısı. Yalnızca Google Mobile Ads Unity eklentisi projeye eklenip
// Player Settings → Scripting Define Symbols'a KAK_ADMOB yazılınca derlenir (Docs/Reklam-Hazirlik.md).
// Not: SDK olmadan derlenip test edilemedi; eklenti sürümüne göre küçük uyarlama gerekebilir.
using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

public class AdMobProvider : IAdProvider
{
    readonly AdConfig cfg;
    RewardedAd ad;
    bool loading;

    public AdMobProvider(AdConfig config) { cfg = config; }

    public void Initialize(Action<bool> done)
    {
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        // 1) Onay (AB/GDPR): Google UMP. Her açılışta güncellenir; gerekirse form gösterilir.
        ConsentInformation.Update(new ConsentRequestParameters(), updateError =>
        {
            if (updateError != null) Debug.LogWarning("[AdMob] Onay güncellenemedi: " + updateError.Message);
            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null) Debug.LogWarning("[AdMob] Onay formu: " + formError.Message);
                if (!ConsentInformation.CanRequestAds()) { done?.Invoke(false); return; }
                // 2) iOS ATT: kullanıcı AB onayını reddettiyse sorulmaz (Apple incelemesi için önerilen sıra).
                //    Unity "iOS 14 Advertising Support" paketi eklendiyse: ATTrackingStatusBinding.RequestAuthorizationTracking()
                MobileAds.Initialize(_ => done?.Invoke(true));
            });
        });
    }

    public bool IsRewardedReady => ad != null && ad.CanShowAd();

    public void LoadRewarded()
    {
        if (loading || IsRewardedReady) return;
        loading = true;
        ad?.Destroy();
        ad = null;
        RewardedAd.Load(cfg.RewardedUnit, new AdRequest(), (RewardedAd loaded, LoadAdError error) =>
        {
            loading = false;
            if (error != null || loaded == null) { Debug.LogWarning("[AdMob] Ödüllü reklam yüklenemedi: " + error); return; }
            ad = loaded;
        });
    }

    public void ShowRewarded(Action<bool> result)
    {
        if (!IsRewardedReady) { result?.Invoke(false); return; }
        bool earned = false, done = false;
        void Finish(bool ok) { if (done) return; done = true; result?.Invoke(ok); }
        ad.OnAdFullScreenContentClosed += () => Finish(earned);
        ad.OnAdFullScreenContentFailed += err => Finish(false);
        ad.Show(reward => earned = true);
    }
}
#endif
