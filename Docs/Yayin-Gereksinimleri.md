# Yayın Gereksinimleri — Google Play ve App Store (2026-09-26 itibarıyla)

> Kaynaklar internetten araştırıldı (bağlantılar en altta). Durum: ✅ projede hazır · 🔧 bu çalışmada eklendi/hazırlandı · 👤 senin yapman gereken (hesap, ödeme, indirme, imza, form) · ⏳ reklam SDK'sı eklenince
>
> Kısa özet: **Oyun tarafındaki teknik eksikler kapatıldı.** Kalanların hepsi hesap, imza, indirme ve konsol formları. Bunları kimse senin yerine yapamaz (ödeme ve kimlik bilgisi gerektiriyor).

## A. Google Play

### A1. Teknik (derleme)
| Gereksinim | Durum | Not |
|---|---|---|
| **Hedef API 36 (Android 16)** — 31 Ağustos 2026'dan beri yeni uygulama ve güncellemelerde zorunlu (1 Kasım 2026'ya kadar uzatma istenebilir) | 🔧 | `KakBuild.ConfigurePlayer`: hedef API 36 (Unity 6.3 destekliyor) |
| **64-bit** (ARM64) | ✅ | IL2CPP + ARM64 |
| **16 KB bellek sayfası uyumu** (Android 15+ hedefleyen uygulamalar, 1 Kasım 2025'ten beri) | ✅ | Unity 6.3 destekliyor. Eklenecek her yerel eklenti (reklam SDK'sı) de uyumlu sürüm olmalı ⏳ |
| **Android App Bundle (.aab)** (APK kabul edilmiyor) | 🔧 | `buildAppBundle = true`, `KakBuild.BuildAndroidRelease` |
| **Android 16 büyük ekran kuralı:** API 36'da tablet ve katlanabilirlerde (≥600dp) yön kilidi yok sayılır; **oyunlar `android:appCategory="game"` ile muaf** | 🔧 | Derleme sonrası manifest'e ekleniyor (`KakAndroidManifest`) |
| Uygulama imzalama: yükleme anahtarı (keystore) + Play App Signing | 👤 | Keystore'u sen oluştur ve sakla (şifreyi Claude görmez) |
| Sürüm kodu her yüklemede artmalı | 🔧 | `bundleVersionCode` sürümden türetiliyor (1.0.0 → 10000; sonraki yükleme için `KakBuild.Version`'ı artır) |
| Min SDK | ✅ | API 25 (Android 7.1). Reklam SDK'ları 23+ istiyor, uyumlu |
| `com.google.android.gms.permission.AD_ID` izni (reklam kimliği kullanılıyorsa, API 33+) | ⏳ | AdMob SDK'sı kendiliğinden ekliyor; Play Console'da "Reklam kimliği" beyanı 👤 |

### A2. Play Console (👤)
| Gereksinim | Not |
|---|---|
| Geliştirici hesabı (25 $, tek sefer) + kimlik doğrulama | Kişisel hesap açarsan aşağıdaki test kuralı geçerli |
| **Kapalı test: en az 12 test kullanıcısı, 14 gün kesintisiz** (13 Kasım 2023 sonrası açılan kişisel hesaplar; kurumsal hesaplar muaf) | 12 kişi bul (arkadaş/aile), Google hesaplarıyla katılsınlar, 14 gün çıkmasınlar |
| Gizlilik politikası URL'si (herkese açık) | 🔧 `Docs/Gizlilik.md` güncellendi; GitHub'da herkese açık. İstersen GitHub Pages ile güzel bir sayfa (tek tık) |
| Veri güvenliği (Data safety) formu | Şimdilik: veri toplanmıyor. Reklam eklenince: cihaz kimlikleri, uygulama etkileşimi, tanılama ⏳ |
| "Reklam içeriyor mu?" beyanı | Şimdilik hayır; reklam açılınca evet ⏳ |
| İçerik derecelendirmesi (IARC anketi) | Hafif çizgi film şiddeti (taş) → büyük olasılıkla PEGI 7 / Herkes 10+ |
| **Hedef kitle ve içerik:** 13 yaş altı seçilirse "Aileler" politikası (yalnızca sertifikalı reklam SDK'ları, kişiselleştirilmiş reklam yok) | **Öneri: 13+** (reklam ve analitik esnekliği). Karar senin |
| Uygulama erişimi (giriş gerekmiyor), haber uygulaması değil, devlet uygulaması değil | Tek tıkla beyanlar |
| Mağaza varlıkları: ikon 512×512 PNG (≤1 MB), **öne çıkan grafik 1024×500** (şeffaflık yok), telefon ekran görüntüsü 2-8 adet (**en-boy en fazla 2:1**, 9:16 önerilir) | 🔧 `Docs/Magaza/play/`: TR ve EN 6'şar görüntü (1080×1920) + öne çıkan grafik; ikon `ikon_512.png`. Eski 1242×2688 görüntüler (2,16:1, Play'e uymuyordu) kaldırıldı |

## B. App Store (Apple)

### B1. Teknik
| Gereksinim | Durum | Not |
|---|---|---|
| **Xcode 26 + iOS 26 SDK ile derleme** (28 Nisan 2026'dan beri zorunlu) | 👤 | Mac'te Xcode kurulu değil: App Store'dan Xcode 26'yı kur (~15 GB) |
| Unity iOS Build Support modülü | 👤 | Unity Hub → Add modules |
| **iPad kararı:** iPadOS 26'da `UIRequiresFullScreen` yok sayılıyor; iPad uygulamaları tüm yönleri desteklemeli. Dikey oyun için en güvenli yol **yalnızca iPhone** (iPad'de uyumluluk modunda yine çalışır) | 🔧 | `targetDevice = iPhoneOnly` (13" iPad ekran görüntüsü de gerekmez) |
| Min iOS | ✅ | 15.0 (Unity 6.3 varsayılanı; AdMob 13+ istiyor) |
| **Gizlilik bildirimi (PrivacyInfo.xcprivacy)**, "gerekçeli API" kullanımı (UserDefaults = PlayerPrefs) | 🔧 | `Assets/Plugins/iOS/PrivacyInfo.xcprivacy` (takip yok, veri toplanmıyor, UserDefaults CA92.1). Reklam SDK'sı kendi bildirimini getirir ⏳ |
| Şifreleme ihracat beyanı (`ITSAppUsesNonExemptEncryption = NO`) | 🔧 | Derleme sonrası Info.plist'e ekleniyor (`KakIosPostBuild`) |
| Uygulama içi gizlilik politikası bağlantısı (Apple 5.1.1: uygulama içinden kolayca erişilebilir olmalı) | 🔧 | Ayarlar → "Gizlilik Politikası" |
| ATT izni (`NSUserTrackingUsageDescription`) — yalnızca kişiselleştirilmiş reklam/izleme varsa | ⏳ | Metin hazır (`KakIosPostBuild`, reklam açılınca eklenir) |
| SKAdNetwork kimlikleri (reklam ağı listesi, Info.plist) | ⏳ | AdMob/ağ listesi SDK eklenince |
| İmzalama: Apple Developer Program (99 $/yıl), Team ID, sertifika/profil (Xcode otomatik imzalama) | 👤 | |

### B2. App Store Connect (👤)
| Gereksinim | Not |
|---|---|
| **Yaş derecelendirmesi yeni anketi** (13+/16+/18+ eklendi; Ocak 2026'dan beri zorunlu, Eylül 2026'dan beri her gönderimde) | Oyun içi kontroller, yetenekler, şiddet temaları soruları: hafif çizgi film şiddeti → büyük olasılıkla 9+ |
| Gizlilik etiketleri ("App Privacy") | Şimdilik "Veri toplanmıyor"; reklam açılınca güncellenir ⏳ |
| Gizlilik politikası URL'si, destek URL'si | GitHub sayfası kullanılabilir |
| Ekran görüntüsü: **6,9" iPhone 1320×2868** (en az 1, en fazla 10) | 🔧 `Docs/Magaza/appstore/`: TR ve EN 6'şar |
| Uygulama ikonu 1024×1024, şeffaflık yok | 🔧 `Docs/Magaza/ikon_1024.png` |
| Kategori: Oyunlar › Arcade; açıklama, anahtar kelimeler, alt başlık | 🔧 `Docs/Magaza/listing.md` güncellendi |

## C. Reklam ile para kazanma (hazırlık — `Docs/Reklam-Hazirlik.md`)
| Gereksinim | Durum |
|---|---|
| Ödüllü reklam altyapısı (`AdService`, sağlayıcı arayüzü, oyun içi test sağlayıcısı) | 🔧 |
| "Reklam izle, 1 canla devam et" (oyun başına 1 kez) ve "Altını 2 katına çıkar" | 🔧 (varsayılan kapalı) |
| AdMob hesabı + uygulama kimlikleri + reklam birimleri | 👤 |
| Google Mobile Ads Unity eklentisi + UMP onay formu (AB/GDPR) + iOS ATT + SKAdNetwork | ⏳ |
| Play "reklam içerir" + reklam kimliği beyanı, veri güvenliği; Apple gizlilik etiketleri | 👤 ⏳ |

## Kaynaklar
- [Google Play hedef API gereksinimleri](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en) · [Android Developers: target SDK](https://developer.android.com/google/play/requirements/target-sdk)
- [16 KB sayfa boyutu (Android Developers Blog)](https://android-developers.googleblog.com/2025/05/prepare-play-apps-for-devices-with-16kb-page-size.html) · [Unity: 16 KB desteği](https://support.unity.com/hc/en-us/articles/39786627094164-What-do-I-need-to-know-about-Google-s-16-KB-page-size-support-requirement-for-November-1st-2024-and-how-does-it-affect-your-SDKs)
- [Unity: AndroidApiLevel36](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AndroidSdkVersions.AndroidApiLevel36.html)
- [Android 16 yön ve yeniden boyutlandırma değişiklikleri (oyun muafiyeti)](https://android-developers.googleblog.com/2025/01/orientation-and-resizability-changes-in-android-16.html) · [Oyunlarda büyük ekran](https://developer.android.com/games/develop/multiplatform/support-large-screen-resizability)
- [Kişisel hesaplar için test gereksinimi (12 test kullanıcısı / 14 gün)](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en)
- [Reklam kimliği (Play Console)](https://support.google.com/googleplay/android-developer/answer/6048248?hl=en)
- [Play mağaza görselleri](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en)
- [Apple SDK minimum gereksinimleri (Xcode 26)](https://www.developer.apple.com/news/upcoming-requirements/)
- [Apple yaş derecelendirmesi güncellemesi](https://developer.apple.com/news/?id=ks775ehf)
- [Apple ekran görüntüsü özellikleri](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/)
- [UIRequiresFullScreen / iPadOS 26](https://developer.apple.com/forums/thread/792735)
- [Unity: Apple privacy manifest](https://docs.unity3d.com/2022.3/Documentation/Manual/apple-privacy-manifest-policy.html)
- [AdMob Unity: UMP onayı](https://developers.google.com/admob/unity/privacy)
