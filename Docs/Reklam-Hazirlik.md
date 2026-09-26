# Reklam Hazırlığı — Kaç Ata Kaç

> Durum (2026-09-26): **altyapı hazır, reklam kapalı.** Oyunda reklam butonu çıkmaz; `Resources/AdConfig` açılıp bir SDK bağlanınca çıkar.
> Tasarım ilkesi: **yalnızca ödüllü reklam** (oyuncu kendisi ister). Geçiş (interstitial) ve banner yok: dikey, hızlı bir oyunda oyuncuyu kaçırır.

## Oyundaki yerleşimler
| Yerleşim | Ne zaman | Ödül | Sınır |
|---|---|---|---|
| **Devam et** (`AdPlacement.Revive`) | Ölünce, oyun sonu ekranından önce "DEVAM ET?" paneli (5 sn geri sayım) | 1 canla devam, 2,5 sn dokunulmazlık, ekrandaki taşlar temizlenir | Oyun başına 1 (`maxRevivesPerRun`) |
| **2× altın** (`AdPlacement.DoubleCoins`) | Oyun sonu ekranında, altın satırının sağında | Bu oyunun altını bir kez daha eklenir | Oyun başına 1 |

- Devam teklifi beklenirken oyun **kaydedilmez** (altın, görev, rekor). Oyuncu reddedince ya da ikinci ölümde **bir kez** kaydedilir. Reklam kapalıyken davranış eskisiyle aynı.
- Reklam yarıda kapatılırsa ödül verilmez, oyun sonu ekranına geçilir.

## Kod haritası
- `Assets/Scripts/Monetization/AdConfig.cs`: `Resources/AdConfig.asset` (açık/kapalı, sağlayıcı, yerleşimler, AdMob kimlikleri — varsayılan **Google test kimlikleri**, ATT metni, SKAdNetwork)
- `AdService.cs`: oyunun tek girişi: `CanShow(yerleşim)`, `ShowRewarded(yerleşim, ödül, başarısız)`, oyun başına sınırlar
- `SimulatedAdProvider.cs`: editör ve dev build'de sahte reklam (Dev menü: **Reklam Simülasyonu Aç-Kapa**)
- `AdMobProvider.cs`: gerçek AdMob + UMP onayı; yalnızca `KAK_ADMOB` tanımıyla derlenir (**SDK olmadan test edilemedi**; eklenti sürümüne göre küçük uyarlama gerekebilir)
- `GameManager`: `FinalizeRun` (tek kayıt), `Revive`, `ContinuePanel`; `GameOverScreen.OnDoubleCoins`
- `Editor/KakPlatformPostBuild.cs`: iOS Info.plist (ATT metni, SKAdNetwork, GADApplicationIdentifier) reklam açıkken eklenir

## Gerçek reklamı açma adımları
1. 👤 **AdMob hesabı** aç (admob.google.com), ödeme bilgilerini gir. Android ve iOS için uygulama ekle → **uygulama kimliği** (`ca-app-pub-…~…`) ve birer **ödüllü reklam birimi** (`ca-app-pub-…/…`) al.
2. **Google Mobile Ads Unity eklentisi**: github.com/googleads/googleads-mobile-unity/releases → `.unitypackage` içe aktar (External Dependency Manager ile gelir). Android'de **16 KB uyumlu** ve API 36 ile uyumlu en yeni sürüm.
3. `Assets → Google Mobile Ads → Settings`: Android/iOS uygulama kimliklerini gir.
4. Player Settings → Scripting Define Symbols: `KAK_ADMOB` ekle (Android ve iOS).
5. `Resources/AdConfig`: `enabled = true`, `provider = AdMob`, reklam birimi kimliklerini gir (önce test kimlikleriyle dene!).
6. **Onay (GDPR/AB):** AdMob → Privacy & messaging → GDPR mesajı oluştur (UMP). `AdMobProvider` her açılışta onayı güncelliyor ve gerekirse formu gösteriyor.
7. **iOS izleme (ATT):** Package Manager → "iOS 14 Advertising Support" (`com.unity.ads.ios-support`) ekle; `AdMobProvider.Initialize` içindeki notlu yere `ATTrackingStatusBinding.RequestAuthorizationTracking()` çağrısını ekle. AB onayı reddedildiyse ATT sorma.
8. **SKAdNetwork:** AdMob'un güncel SKAdNetwork listesini `AdConfig.skAdNetworkIds`'e yapıştır (developers.google.com/admob/ios/ios14).
9. **Beyanlar (👤):**
   - Play Console: "Reklam içeriyor: Evet", Reklam kimliği beyanı (AD_ID: evet, reklam için), Veri güvenliği: cihaz veya diğer kimlikler, uygulama etkileşimleri, tanılama (AdMob'un "Play data disclosure" rehberine göre)
   - App Store Connect: gizlilik etiketleri (Kimlik bilgileri: cihaz kimliği; Kullanım verisi: reklam verisi; Tanılama) + izleme varsa "Tracking"
   - `Assets/Plugins/iOS/PrivacyInfo.xcprivacy`: AdMob kendi bildirimini getirir; uygulama bildiriminde `NSPrivacyTracking` ve toplanan veri türleri güncellenmeli
   - `Docs/Gizlilik.md`: "Reklamlar" bölümünü yayınla (taslak aşağıda)
10. Test: gerçek cihazda **test kimlikleriyle** devam et ve 2× altın; sonra gerçek kimlikler. Kendi reklamına tıklama (hesap kapatılır).

## Gizlilik politikası — reklam bölümü (reklam açılınca eklenecek taslak)
> **Reklamlar:** Oyun, isteğe bağlı ödüllü reklamlar için Google AdMob kullanır. AdMob, reklam göstermek ve ölçmek için cihaz reklam kimliği, IP adresi (yaklaşık konum), cihaz bilgisi ve reklam etkileşimi gibi verileri işleyebilir. AB/EEA'da ilk açılışta onay istenir; iOS'ta izleme izni istenir ve reddedilirse kişiselleştirilmemiş reklam gösterilir. Ayrıntı: https://policies.google.com/technologies/ads
