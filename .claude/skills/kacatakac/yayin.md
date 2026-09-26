# Yayın ve para kazanma hazırlığı (2026-09-26)

> Kullanıcı isteği: "Android ve App Store'da paylaşabilmem için teknik eksikleri ve gereksinimleri internetten araştır, hepsinin listesini çıkar, ekle, çıkarılmaya hazırla. Reklamdan para kazanılacaksa 'reklam izle, bir canla devam et' gibi mekanik: şimdiden koyma, ön hazırlık ekle. Sonra ikinci karakter: esmer, uzun saçlı, zayıf bir kız; 8 yön + koşu animasyonları (32-64 kare), dandik olmasın."

## Fazlar
- [x] **Y1 Araştırma:** `Docs/Yayin-Gereksinimleri.md` (Play + App Store, kaynaklı; ✅/🔧/👤/⏳)
- [x] **Y2 Teknik:** hedef API 36, AAB, ARM64, sürüm kodu, `appCategory="game"` (Android 16 büyük ekran muafiyeti, `KakAndroidManifest`), iOS yalnızca iPhone + min 15.0 + alt kenar hareketi erteleme + şifreleme beyanı (`KakIosPostBuild`) + `PrivacyInfo.xcprivacy`, uygulama içi gizlilik bağlantısı (`StoreLinks`), `KakBuild.BuildAndroidRelease/BuildIosProject`, güncel `Gizlilik.md`, `Yayin-Kontrol-Listesi.md`
- [x] **Y3 Reklam hazırlığı (kapalı):** `AdConfig` (Resources, test kimlikleri), `AdService` + `IAdProvider` (Null / Simulated / AdMob[KAK_ADMOB]), `ContinuePanel` ("DEVAM ET?" 5 sn), `GameManager.FinalizeRun/Revive` (kayıt bir kez), `GameOverScreen.OnDoubleCoins`, Dev: Reklam Simülasyonu Aç-Kapa, `Docs/Reklam-Hazirlik.md`. Testler `ReklamTests` (5)
- [x] **Y4 Kız karakter (ADA):** 40 kare (8 yön idle + 8×4 koşu), `tools/kak_gen_girl.py` Ata'nın karelerinden türetiyor: esmer ten, koyu kahve uzun saç (yöne göre kural: arkada bele kadar, önde yüzü çerçeveleyen tutamlar, yanda arkaya savrulan tutam), inceltilmiş gövde (omuz altı orta sütun çıkarılır), açık gri üst + pembe-mor alt. Bedava, Ata ile aynı istatistikler; katalog sırası Ata, Ada, Çevik, Tank, Şanslı. Karakter paneli 3×2 ızgara (6 yuva). **Yan bulgu:** varyant karakterlerin koşu kareleri (.gif) renklendirilmemişti, Doğu dışı yönlerde mavi Ata görünüyordu → `kak_recolor_characters.py` .gif de işliyor, `RemapSprite` .png'ye düşüyor; test `Varyantlar_HerYondeKendiKarelerini_Kullanir`. Testler 75/75 (+1 atlandı: denge) + 18/18
- [x] **Y5 Mağaza görselleri:** `tools/kak_store_shots.py` → `Docs/Magaza/play` (TR/EN 6'şar 1080×1920 + öne çıkan grafik 1024×500), `appstore` (TR/EN 6'şar 1320×2868), `ikon_1024.png` (RGB) + `ikon_512.png` (RGBA). Oyun, başlık bandının altındaki alanın boyutunda render ediliyor (1080×1520 / 1320×2308): ölçekleme yok. `listing.md` yeni özelliklerle, sınırlar betikle kontrol edildi. **Yan bulgular:** (1) 3×2 karakter paneli kısa ekranda kesiliyordu → `UiFitToScreen` (tüm modaller); (2) EN "Block 1 rocks" → tekil anahtar (`mission_ShieldBlocks_1`); (3) oyun sonu/günlük panelin dinamik metinleri dil değişince yenilenmiyor (oyuncu dili ayarlardan değiştirdiği için pratikte görünmez; çekimde dil başına ayrı koşu)

## Önemli bulgular
- Google Play API 36 zorunluluğu 31.08.2026'da başladı (uzatma 01.11.2026). Unity 6.3 destekliyor.
- iPadOS 26 `UIRequiresFullScreen`'i yok sayıyor → dikey oyun için iPhone-only en temiz yol.
- Kişisel Play hesabı: 12 test kullanıcısı × 14 gün kapalı test zorunlu.
- Bu Mac'te Xcode yok, Unity'de Android/iOS modülü yok → build alınamaz (kullanıcı adımı).
