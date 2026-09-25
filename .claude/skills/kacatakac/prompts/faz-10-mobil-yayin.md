# FAZ 10 — Mobil Optimizasyon ve Yayın

**Süre tahmini:** 3-5 oturum · **Dal:** `faz-10-yayin`

## Amaç
Gerçek cihazda akıcı çalışan, mağaza kurallarına uygun ve yayına hazır bir build.

## Görevler

### 10.1 Build ayarları
- Hedef platformu Android (ve/veya iOS) yap. Paket adı (ör. `com.atakaan.kacatakac`), sürüm, ikon (adaptive icon), açılış ekranı, ekran yönü kilidi.
- Android: IL2CPP, ARM64, minimum API seviyesi (Google Play güncel gereksinimine göre kontrol et), App Bundle (.aab), imzalama anahtarı. **Keystore'u ben oluşturup saklarım, sen şifre isteme.**
- iOS: Xcode projesi, bundle id, imzalama (Apple Developer hesabı gerekir).

### 10.2 Performans
- Sprite Atlas'lar (dünya başına + UI), draw call hedefi.
- Havuzlama: mermi, parçacık, powerup, floating text. Oyun sırasında `Instantiate` ve GC tahsisi sıfıra yakın olmalı (Profiler ile ölç).
- Build'de `Debug.Log`'ları kaldır (`Conditional` wrapper).
- 30/60 FPS ayarı (`Application.targetFrameRate`), pil dostu mod.
- Orta ve düşük seviye Android cihazda test, kalite seviyeleri (gölge ve ışık sayısı).

### 10.3 Girdi
- Eski `Input` API'sinden **Input System**'e geçiş (paket zaten kurulu): dokunmatik, gamepad ve klavye tek bir Action Map'te. Joystick ve aksiyon butonu bu sisteme bağlansın.
- Titreşim: Android'de süre ve şiddet kontrollü titreşim (Handheld.Vibrate çok kaba).

### 10.4 Yayın gereksinimleri
- Gizlilik politikası sayfası (reklam veya analitik varsa zorunlu), veri güvenliği formu için hangi verilerin toplandığının listesi.
- Reklam/IAP seçildiyse: Unity Ads / LevelPlay veya AdMob + Unity IAP entegrasyonu, GDPR/ATT onay akışı.
- Analitik (isteğe bağlı): bölüm başına ölüm noktaları, Sonsuz'da ortalama süre.
- Mağaza materyalleri: 6-8 ekran görüntüsü (Faz 3 kalitesinde), 30 sn tanıtım videosu, kısa ve uzun açıklama (TR + EN), anahtar kelimeler.
- Kapalı test (Google Play Internal/Closed testing) → geri bildirim → düzeltme → yayın.

### 10.5 Yerelleştirme
- Tüm metinler tek tabloda (Unity Localization paketi veya basit CSV). TR + EN.

## Kabul kriterleri
- Gerçek telefonda 60 FPS, 15 dk oyunda ısınma veya çökme yok.
- Mağaza kapalı test sürümü yüklendi.
