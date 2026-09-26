# Yayın Kontrol Listesi — Kaç Ata Kaç

> Ayrıntılı gereksinimler ve kaynaklar: [Yayin-Gereksinimleri.md](Yayin-Gereksinimleri.md) · Reklam: [Reklam-Hazirlik.md](Reklam-Hazirlik.md)
> ✅ hazır · 👤 senin adımın (hesap, ödeme, indirme, imza, form: Claude yapamaz ya da yapmamalı)

## Proje (✅ tamam)
- ✅ Ad "Kaç Ata Kaç", paket `com.atakaan.kacatakac`, sürüm **1.0.0** (kod 10000), sadece dikey
- ✅ Android: IL2CPP + ARM64, **hedef API 36**, min API 25, **.aab**, 16 KB uyumlu (Unity 6.3), büyük ekranda oyun muafiyeti (`appCategory="game"`)
- ✅ iOS: min 15.0, **yalnızca iPhone**, alt kenar hareketi ertelenir, şifreleme beyanı, gizlilik bildirimi (`PrivacyInfo.xcprivacy`)
- ✅ Uygulama içi gizlilik politikası bağlantısı (Ayarlar), güncel politika metni (`Gizlilik.md`)
- ✅ Reklam altyapısı (kapalı): devam et + 2× altın
- ✅ Mağaza metinleri ve görselleri (`Docs/Magaza/`)

## 👤 Senin adımların (sırayla)
1. **Unity modülleri:** Unity Hub → Installs → 6000.3.8f1 → ⚙ → Add modules → *Android Build Support* (+ OpenJDK + Android SDK & NDK Tools) ve *iOS Build Support*.
2. **Xcode 26** (Mac App Store, ~15 GB), bir kez aç, lisansı kabul et. Sonra Terminal'de: `sudo xcode-select -s /Applications/Xcode.app` (şifre ister).
3. **Android keystore:** Unity → Player Settings → Android → Publishing Settings → Keystore Manager → yeni keystore. Şifreleri güvenli sakla, dosyayı yedekle (kaybedersen güncelleme yapamazsın). Claude şifre görmez ve girmez.
4. **Google Play Console** (25 $): uygulama oluştur → Gizlilik politikası URL'si → Uygulama içeriği formları (veri güvenliği: veri toplanmıyor; reklam: hayır; hedef kitle: **13+ önerilir**; içerik derecelendirmesi) → mağaza girişi (metin + `Docs/Magaza/play/` görselleri).
5. **Kişisel hesapsa: kapalı test**, en az **12 test kullanıcısı, 14 gün** kesintisiz. Sonra üretim erişimi başvurusu.
6. **Apple Developer Program** (99 $/yıl) → App Store Connect'te uygulama → yaş derecelendirmesi anketi → gizlilik etiketleri (veri toplanmıyor) → `Docs/Magaza/appstore/` görselleri → Xcode'da Archive → yükle → TestFlight → incelemeye gönder.
7. **Build:** Claude'a "Android build al" / "iOS projesi çıkar" de (`KacAtaKac/Yayın/Android Release (.aab)` ve `iOS Xcode Projesi`).
8. **Telefonda oyna:** denge, altın ekonomisi, sesler, dokunmatik his. Geri bildirim ver.
9. **İletişim adresi:** gizlilik politikasına ve mağaza girişine bir destek e-postası (hangisini kullanacağını sen seç).
10. **Gizlilik politikası sayfası:** şu an `https://github.com/Atakaanm/KAK/blob/main/Docs/Gizlilik.md` (herkese açık, kullanılabilir). Daha şık bir sayfa istersen GitHub Pages açılabilir (repo ayarı, senin onayınla).
11. (İsteğe bağlı) **Reklam:** `Reklam-Hazirlik.md` adımları (AdMob hesabı, SDK, onay formu, beyanlar).
