# KaçAtaKaç — Sanat ve Renk Rehberi

> Tüm görsel işlerin anayasası. Yeni sprite, UI, parçacık veya ışık eklemeden önce oku.
> Kullanıcı notu: **"Renk uyumu çok önemli."** Bu dosyadaki kurallar pazarlığa açık değil. Değişiklik ancak kullanıcı onayıyla yapılır.
> Durum: Taslak (Faz 0'da stil, Faz 3'te palet kesinleşecek). Kesinleşen değerler `[KESİN]` ile işaretlenir.

## 1. Stil
- **Piksel sanatı.** Standart yoğunluk **[KESİN]: 1 sanat pikseli = 0,024 dünya birimi** (mevcut 48 px karakterlerle ve arena görselinin sanat pikseliyle aynı). `Assets/Art/**` için PPU 41,667 otomatik (`KakArtImportRules`). 48 px karakterler/fırlatıcılar = görsel ölçeği 2,4 @ PPU 100. Taş 26 px (+kontur).
- Keskin pikseller: `Filter Mode = Point`, `Compression = None`, mipmap kapalı.
- **Tek piksel yoğunluğu:** Ekrandaki her şey aynı piksel ızgarasında. Sprite'ları `localScale` ile büyütüp küçültmek yasak (parçacıklar ve kısa efekt animasyonları hariç).
- Dış hat: karakterler, düşmanlar ve mermilerde 1 px koyu kontur (paletin en koyu tonu, saf siyah değil). Zemin ve duvarlarda kontur yok.
- Işık yönü: sol üst. Gölge: yarı saydam koyu elips, ayak altında.

## 2. Palet  [KESİN — Endesga 32, 2026-09-25: kullanıcı cevap vermedi, Claude'un önerisi varsayım olarak uygulandı; değiştirilebilir]
- Palet: **Endesga 32** → `Assets/Art/Palette/kak_palette.png`, kodda `KakPalette` (rol isimleriyle), Python'da `tools/kak_palette.py`.
- **Uygulama kuralı (Faz 3'te öğrenildi):** Yapay zekayla üretilmiş yüksek çözünürlüklü, yumuşak geçişli görseller (arena, powerup ikonları) palete **zorla indirgenmez** (parlamalar lekeye döner, zemin düzleşir). Bütünlük global renk tonlamasıyla (Volume: kontrast +10, doygunluk +6, gölge soğuk-mor / ışık sıcak split toning, tonemapping yok) sağlanır. Palet, piksel sanatı karakterlere, üretilen karolara, parçacıklara, UI'a ve dış hatlara uygulanır.
- **Tek ana palet, en fazla 32 renk.** Aday: Lospec'ten "Endesga 32" ya da "Resurrect 64"ün 32 renklik alt kümesi, veya mevcut Dungeon arenasının renklerinden türetilmiş özel bir palet. Faz 3'te 3 aday bana gösterilir, ben seçerim.
- Palet dosyası: `Assets/Art/Palette/kak_palette.png` (renk başına 1 px) + `KakPalette` ScriptableObject (isimli renkler: `ZeminKoyu`, `TehlikeTuruncu`...).
- **Palet denetçisi** (editör aracı): Seçili sprite'larda paletin dışındaki pikselleri raporlar ve isteğe bağlı olarak en yakın palet rengine indirger. Her yeni görsel bu kontrolden geçer.
- UI, parçacık, ışık rengi ve post-process tonlaması da paletten seçilir. Rastgele hex yazmak yasak, kodda `KakPalette.Get("...")` kullanılır.

## 3. Renk rolleri (oyunun görsel dili)
| Rol | Renk ailesi | Kural |
|---|---|---|
| Ortam (zemin, duvar, koridor) | Soğuk, doygunluğu düşük yeşil-mavi-gri (mevcut Dungeon tonu) | Oynanış nesnelerinin arkasında geri çekilir |
| Işık kaynağı (meşale) | Sıcak turuncu-sarı | Ortamın tamamlayıcı rengi; sıcak-soğuk uyumu buradan gelir |
| **Tehlike** (taşlar, uyarılar, hasar) | Sıcak: turuncu, kırmızı | Oyuncu "sıcak = kaç" diye öğrenir. Taşlar zeminle karışmamalı: sıcak kenar ışığı + kontur + gölge |
| **İyi** (powerup, kalkan) | Camgöbeği ve yeşil | Hafif parlama (bloom) |
| **Ödül** (skor, rekor, jeton) | Altın ve krem | UI vurgu rengi |
| **Oyuncu** | Ekranın en yüksek kontrastı ve doygunluğu | Her an ilk bakışta bulunmalı |
| Düşmanlar (fırlatıcılar) | Orta ton, ortamdan bir kademe daha doygun | Saldırı öncesi sıcak parlama (telegraph) |

## 4. Değer (açıklık) hiyerarşisi
Koyudan açığa: **ekran çerçevesi ve koridor** (%10-20) → **arena zemini** (%25-40) → **duvar ve dekor** (%30-50) → **düşmanlar** (%40-60) → **mermiler ve powerup'lar** (%60-80, yüksek kontrast) → **oyuncu** (en yüksek kontrast) → **UI metni** (%85+).

- **60-30-10 kuralı:** Ekranın yaklaşık %60'ı ortam tonu, %30'u ikincil (duvar, dekor, koridor), %10'u vurgu (oyuncu, tehlike, ödül).
- **Gri tonlama testi:** Ekran görüntüsü siyah beyaza çevrildiğinde oyuncu, taşlar ve powerup'lar hâlâ açıkça ayırt edilmeli.
- **Renk körlüğü testi:** Deuteranopi ve protanopi simülasyonunda tehlike ile iyi ayırt edilmeli. Sadece renge güvenilmez, şekil ve animasyon farkı da olmalı.
- **Bulanıklık testi:** Ekran görüntüsü bulanıklaştırılınca bile tehlikenin nerede olduğu görünmeli.

## 5. Ekran kompozisyonu (dikey) [KESİN — kullanıcı onayı 2026-09-25]
```
[ ♥ ♥ ♥           SKOR / SÜRE          ⏸ ]  ← HUD bandı: üst duvarın 2.5D ön yüzü (Safe Area içinde)
[ ┌──────────────────────────────────┐ ]
[ │                                  │ ]
[ │      KARE ARENA (ekran eni)      │ ]  ← kare, telefonun enine göre ölçeklenir; oyun alanı her cihazda aynı (dünya biriminde)
[ │      4 köşe fırlatıcı            │ ]
[ └─────────── arena kapısı ────────┘ ]
[    karanlık taş koridor · meşale · sis  ]  ← boyu telefonun oranına göre uzar/kısalır
[   (joystick)                   (DASH)   ]  ← kontrol alanı: arenaya taşmaz
```
- Ekranda **tasarlanmamış boş alan yok**. Arena dışındaki her piksel dünyanın parçası (duvar, koridor, karanlık, dekor).
- Kontroller arenanın üstüne binmez, parmak tehlikeyi kapatmaz.
- Bölümlerde karo seti değişince çerçeve de değişir (buz koridoru, stadyum tüneli...).

## 6. Işık ve post-process
- Ortam ışığı (Global Light 2D): soğuk ve düşük. Meşaleler: sıcak ve titreşen. Oyuncunun çevresinde hafif ışık.
- Bloom: sadece parlak vurgular (meşale, powerup, tehlike telegraph'ı). Düşük eşik yasak (her şey parlamamalı).
- Vignette: koridora ve kenarlara doğru koyulaşma. Arena merkezi en aydınlık alan.
- Renk tonlaması: tek bir LUT/Color Adjustments profili, bütün sahneyi aynı havada toplar.
- Film grain, chromatic aberration ve motion blur yok (piksel sanatını kirletir).

## 7. Hareket ve his (juice) ölçüleri
- Ekran sarsıntısı: vuruşta 0.15 sn / küçük genlik. Ayarlardan kapatılabilir.
- Hit-stop: 60 ms. Vuruş flaşı: 1 kare beyaz.
- Animasyon fps: idle 6, koşu 10-12, saldırı 12, efekt 12-15.
- UI animasyonları: 0.15-0.3 sn, ease-out. Hiçbir menü geçişi 0.4 sn'yi geçmez.

## 8. Yapay zeka ile görsel üretimi
- Prompt şablonları (Faz 3'te doldurulacak): stil, kamera açısı ("top-down 3/4 view"), boyut, palet, arka plan ("transparent background, no watermark").
- Üretim sonrası zorunlu adımlar: 1:1 piksele küçült → palete indirge (palet denetçisi) → arka planı temizle → **filigran kontrolü** → kontur kontrolü → oyunda gri tonlama testi.
