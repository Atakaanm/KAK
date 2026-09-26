# Yayın ve para kazanma hazırlığı (2026-09-26)

> Kullanıcı isteği: "Android ve App Store'da paylaşabilmem için teknik eksikleri ve gereksinimleri internetten araştır, hepsinin listesini çıkar, ekle, çıkarılmaya hazırla. Reklamdan para kazanılacaksa 'reklam izle, bir canla devam et' gibi mekanik: şimdiden koyma, ön hazırlık ekle. Sonra ikinci karakter: esmer, uzun saçlı, zayıf bir kız; 8 yön + koşu animasyonları (32-64 kare), dandik olmasın."

## Fazlar
- [x] **Y1 Araştırma:** `Docs/Yayin-Gereksinimleri.md` (Play + App Store, kaynaklı; ✅/🔧/👤/⏳)
- [x] **Y2 Teknik:** hedef API 36, AAB, ARM64, sürüm kodu, `appCategory="game"` (Android 16 büyük ekran muafiyeti, `KakAndroidManifest`), iOS yalnızca iPhone + min 15.0 + alt kenar hareketi erteleme + şifreleme beyanı (`KakIosPostBuild`) + `PrivacyInfo.xcprivacy`, uygulama içi gizlilik bağlantısı (`StoreLinks`), `KakBuild.BuildAndroidRelease/BuildIosProject`, güncel `Gizlilik.md`, `Yayin-Kontrol-Listesi.md`
- [x] **Y3 Reklam hazırlığı (kapalı):** `AdConfig` (Resources, test kimlikleri), `AdService` + `IAdProvider` (Null / Simulated / AdMob[KAK_ADMOB]), `ContinuePanel` ("DEVAM ET?" 5 sn), `GameManager.FinalizeRun/Revive` (kayıt bir kez), `GameOverScreen.OnDoubleCoins`, Dev: Reklam Simülasyonu Aç-Kapa, `Docs/Reklam-Hazirlik.md`. Testler `ReklamTests` (5)
- [ ] **Y4 Kız karakter (ADA):** 8 yön idle + 8×4 koşu karesi, `tools/kak_gen_girl.py`, karakter olarak ekle (başlangıçta seçilebilir), ayrıca Ata için eksik koşu kareleri
- [ ] **Y5 Mağaza görselleri:** Play 1080×1920 + öne çıkan grafik 1024×500, App Store 1320×2868, ikon 1024 (TR/EN, başlıklı)

## Önemli bulgular
- Google Play API 36 zorunluluğu 31.08.2026'da başladı (uzatma 01.11.2026). Unity 6.3 destekliyor.
- iPadOS 26 `UIRequiresFullScreen`'i yok sayıyor → dikey oyun için iPhone-only en temiz yol.
- Kişisel Play hesabı: 12 test kullanıcısı × 14 gün kapalı test zorunlu.
- Bu Mac'te Xcode yok, Unity'de Android/iOS modülü yok → build alınamaz (kullanıcı adımı).
