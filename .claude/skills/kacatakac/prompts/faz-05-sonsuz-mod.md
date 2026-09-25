# FAZ 5 — Sonsuz Mod Cilası ("Sadece Taştan Kaç")

**Süre tahmini:** 2-3 oturum · **Dal:** `faz-5-sonsuz` · **Ön koşul:** Faz 2 ve 3

## Amaç
Sonsuz Mod'u "bir el daha" dedirten, okunabilir ama acımasız bir deneyime çevirmek. Tema tek: **taş**. Çeşitlilik taşın davranışından ve olaylardan gelsin.

## Tasarım ilkeleri
- **Okunabilirlik:** Her tehlike bir uyarı (telegraph) ile gelir. Ölüm oyuncunun hatası gibi hissettirmeli, şans gibi değil.
- **Kademeli tanıtım:** Her yeni taş tipi önce tek başına, yavaş gelir. Sonra karışıma girer.
- **Ritim:** Yoğun 20-30 sn → 3-5 sn nefes → yeni olay.

## Görevler

### 5.1 Taş çeşitleri (hepsi `ProjectileData` + Faz 2 hareket/etki sistemi ile)
| Taş | Davranış | Açıldığı kademe |
|---|---|---|
| Çakıl | Küçük, hızlı, düz | 0 |
| Kaya | Büyük, yavaş, geniş hitbox | 1 |
| Seken taş | Duvarlardan 1-2 kez seker | 2 |
| Parçalanan taş | Ortada 3 küçük parçaya bölünür | 3 |
| Göktaşı | Gökten düşer. Yerde 1 sn büyüyen gölge uyarısı, iniş alanında hasar ve kırıntı (2.5D yükseklik) | 3 |
| Yuvarlanan kaya | Arenayı bir kenardan diğerine tarar. Yanında şerit uyarısı var | 4 |
| Güdümlü taş | Kısa süre oyuncuyu takip eder, sonra düz gider | 5 |

### 5.2 Olaylar (her 30-45 sn, `EndlessEventData` SO listesi)
- **Taş Yağmuru:** 5 sn boyunca rastgele göktaşları.
- **Çapraz Ateş:** 4 köşe aynı anda.
- **Sessizlik:** 3 sn ateş yok, sonra yoğun dalga (gerilim).
- **Deprem:** ekran sallanır, zeminde çatlak çizgisi boyunca hasar.
- Olay başında kısa banner ve ses. Olaylar kademeye göre havuzdan seçilir.

### 5.3 Skor derinliği
- **Yakın geçiş (near-miss):** Taş oyuncuya çok yakın geçince +bonus, "YAKIN!" yazısı, hafif yavaş çekim anı.
- **Combo çarpanı:** Hasar almadan geçen her 10 sn ile x1.1, x1.2... Hasar alınca sıfırlanır. HUD'da görünür.
- Kademe eşiklerini ve çarpanlarını test oyunlarıyla yeniden ayarla. İlk ölüm ortalama 60-90. saniyede, iyi oyuncu 3-5 dakika. Denge tablosunu `progress.md`'de güncelle.

### 5.4 Denge aracı
- Editörde `KacAtaKac/Denge Simülasyonu`: kademe başına saniyede atılan mermi, ekrandaki ortalama mermi sayısı ve boş alan yüzdesi tablosu. Zorluk eğrisini sayıyla görelim.
- Oyun içi gizli debug paneli (editör ve development build'de): kademe atla, ölümsüzlük, zaman x2.

## Test
- 5 deneme oyna, her birinde ölüm saniyesini ve nedenini not et. "Haksız ölüm" hissi olan tehlikeyi işaretle, uyarısını güçlendir.

## Kabul kriterleri
- 7 taş tipi ve 4 olay çalışıyor, hepsinin uyarısı var.
- Near-miss ve combo HUD'da görünüyor, skor tablosu güncel.
