# FAZ 6 — Dünya 1: Buz Arenası

**Süre tahmini:** 3-4 oturum · **Dal:** `faz-6-buz` · **Ön koşul:** Faz 2, 3, 4

## Fantezi
Donmuş bir göl arenası. Karakter hızlanırken ve dururken kayar, **anında duramaz**. Oyuncu kaymayı hesaba katarak önceden yön vermeyi öğrenir. Düşmanlar kartopu ve buz sarkıtı atar.

## Görevler

### 6.1 Buz fiziği (sadece `ArenaData` / zemin ayarlarıyla)
- `PlayerMovement2D`'yi ivme ve fren tabanlı yap: `acceleration`, `deceleration`, `turnResponsiveness`. Normal zemin eski anlık hissi korusun (regresyon yok).
- Buz ayarı: düşük fren (0.5-1 sn'de durma), yön değiştirirken kayma izi.
- **His:** Kayarken ayak altında kar tozu ve iz, kayma sesi (hıza göre ses yüksekliği), karakter kayarken hafif eğik poz.
- Buz ayarlarını test için bir denge sahnesi hazırla. Ben 3 farklı değer setini deneyip seçeyim ("hafif kaygan", "orta", "çok kaygan").

### 6.2 Zemin öğeleri (`ArenaHazard` bileşeni, prefab tabanlı)
- **Kar yığını:** üstünde normal sürtünme (güvenli ada).
- **Çatlak buz:** üstünde 1.5 sn durunca kırılır, su deliği olur (düşersen 1 can gider ve başlangıca dönersin).
- **Rüzgâr bölgesi:** oyuncuyu bir yöne iter (ok efektiyle).

### 6.3 Düşmanlar (`EnemyData` ile)
- **Kardan Adam:** Stationary + Aimed kartopu. Kartopu isabet ederse kısa `Slow`.
- **Yeti:** SideLine + Lob buz kayası. Yere düşünce buz parçalarına ayrılır.
- **Penguen:** Arenada karnının üstünde kayarak çizgi halinde geçer (Charge hareketi, telegraph'lı).
- **Buz Sarkıtı:** Tavandan düşen göktaşı varyantı, gölge uyarısıyla.

### 6.4 Bölüm tasarımı (10 bölüm, `WorldData: Buz`)
- 1-2: Sadece kaymayı öğret (1 kardan adam, 30 sn hayatta kal).
- 3-4: Kar yığınları + 2 kardan adam.
- 5-6: Yeti tanıtımı, lob atışlar.
- 7-8: Çatlak buz + karışık düşmanlar.
- 9: Rüzgâr + penguen.
- 10: **Mini boss:** Büyük Yeti. Gezer, 3 fazlı saldırı deseni vardır, 90 sn hayatta kal.
- Her bölüm için yıldız koşulları: 1 = hayatta kal, 2 = en az 2 can ile bitir, 3 = hiç hasar alma ya da X powerup topla.
- Bölümleri `LevelData` olarak oluşturan bir editör aracı yaz (tablodan toplu üretim).

### 6.5 Görsel
- Faz 3 sanat standardında buz arenası, kar parçacıkları (ekran boyu hafif kar yağışı), soğuk mavi global ışık, buz yansıması için parlak zemin detayları.

## Kabul kriterleri
- 10 bölüm baştan sona oynanabiliyor, yıldızlar kaydediliyor, Dünya 2'nin kilidi Buz'dan toplanan yıldızla açılıyor.
- Buz hissi ayarları benim onayladığım değerlerde.
