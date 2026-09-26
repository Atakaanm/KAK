# Yayın Kontrol Listesi — Kaç Ata Kaç

> ✅ Claude tamamladı · 👤 Kullanıcının yapması gerekiyor (hesap, para, cihaz, indirme)

## Proje ayarları
- ✅ Ürün adı "Kaç Ata Kaç", şirket "Atakaan", paket kimliği `com.atakaan.kacatakac`, sürüm 0.9.0 (KacAtaKac/Yayın/Oyuncu Ayarlarını Uygula)
- ✅ Sadece dikey, durum çubuğu gizli, ekran kararmaz (oyun sırasında)
- ✅ Android: IL2CPP + ARM64, min Android 7.1 (API 25), hedef API otomatik · iOS: min 14.0
- ✅ Uygulama ikonu (`Assets/Art/Icon/app_icon.png`), açılış ekranı koyu
- ✅ TR/EN dil (cihaz diline göre; Ayarlar'dan değiştirilebilir)
- ✅ Performans (macOS dev build, Apple M4): ~60 FPS, GC ≈ 30 B/kare, en kötü kare 33 ms, 82 batch
- ✅ Mağaza metinleri ve ekran görüntüleri (`Docs/Magaza/`), gizlilik politikası taslağı (`Docs/Gizlilik.md`)

## 👤 Senin yapman gerekenler
1. **Android Build Support modülünü kur:** Unity Hub → Installs → 6000.3.8f1 → ⚙ → Add modules → *Android Build Support* (+ *OpenJDK* ve *Android SDK & NDK Tools*). (Birkaç GB indirme.)
   - iOS için: *iOS Build Support* + Mac'te Xcode (şu an Xcode yolu bozuk görünüyor: `xcode-select -p` hatası; Xcode'u App Store'dan güncelle/aç).
2. Kurulumdan sonra bana "Android build al" de → Claude `KacAtaKac/Yayın` menüsüne Android build + cihazda ölçüm adımlarını ekler.
3. **İmzalama anahtarı (keystore):** Player Settings → Publishing Settings → Keystore Manager → yeni keystore oluştur, şifreleri güvenli sakla (Claude şifre görmez/girmez).
4. **Google Play Console** hesabı (tek seferlik 25$) → uygulama oluştur → İç test kanalı → .aab yükle.
5. **Telefonda oyna ve geri bildir:** denge (çok zor/kolay?), dokunmatik his (joystick, dash), performans, yazı boyutları.
6. Gizlilik politikasını bir web sayfasında yayınla (ör. GitHub Pages) → Play Console'a bağlantı ver. İletişim e-postasını ekle.
7. İçerik derecelendirme anketi ve veri güvenliği formu (veri toplanmıyor → "Veri toplanmaz").

## Sonraki teknik adımlar (Claude)
- Cihazda Frame Debugger ile batch analizi → gerekirse URP 2D Renderer'a geçiş
- Ses efektleri ve müzik (şu an klipler eksik; AudioManager hazır)
- Input System paketine tam geçiş (şu an "Both" modunda, sorun yok)
