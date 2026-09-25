# FAZ 7 — Dünya 2: Futbol Arenası

**Süre tahmini:** 4-5 oturum · **Dal:** `faz-7-futbol` · **Ön koşul:** Faz 2 (`FootballMode` iskeleti, durum efektleri), Faz 4

## Fantezi
Bir futbol sahasındasın, ayağında top var. Kenar çizgisindeki rakip futbolcular sana **sarı ve kırmızı kart** fırlatıyor. Kartlardan kaçarken tek tuşla şut atıp **gol** atmaya çalışıyorsun. Kaçış ve hedef aynı anda.

## Önce tasarım kararları (bana sor)
1. Ekran yönü: dikey sahada kaleler üstte ve altta mı, yoksa bu dünya yatay mı? (Faz 0'daki genel karara göre öner.)
2. Kaç kale: sadece rakip kale (üstte) mi, yoksa iki kale ve kendi kaleni koruma da mı var?
3. Kaleci olsun mu? (Öneri: bölüm 4'ten sonra, kalede gidip gelen kaleci.)
4. Kart kuralları (öneri):
   - **Sarı kart:** vurunca 2 sn `Slow` + 1 sarı sayacı. **2 sarı = kırmızı.**
   - **Kırmızı kart** (nadir, yavaş ama büyük): vurunca 1 can gider ve top elden çıkar.
5. Top kaybı: Kart yiyince top düşer mi? (Öneri: kırmızıda düşer, sarıda düşmez.)

## Görevler

### 7.1 Top ve top sürme — `Ball` + `BallCarrier`
- `Ball`: Dynamic Rigidbody2D, düşük sürtünme, duvardan sekme, 2.5D yükseklik (sert şutta hafif havalanır, gölgesi yerde kalır).
- **Top sürme:** Oyuncu topa değince top ayağına yapışır ve hareket yönünün biraz önünde, hafif gecikmeyle takip eder (ip gibi yumuşak bağ). Koşarken top ayakta küçük sekmeler yapar.
- **Aksiyon butonu (tek tuş):** Basınca bakılan yöne şut. **Basılı tutup bırakınca güçlü şut** (şarj çubuğu). Top yokken aynı buton kısa `Dash` olarak çalışsın mı? Bana sor.
- Hafif otomatik nişan (aim assist): Şut yönü kaleye 15 derece yakınsa kaleye doğru bükülsün.
- Mobil: sağ alt köşede büyük aksiyon butonu (Faz 4 HUD'una eklenir).

### 7.2 Kale ve gol — `Goal`
- Trigger alanı, gol olunca: top ağlara gömülür, konfeti, tribün sesi, yavaş çekim, skor. Top ortaya döner, oyuncu kısa süre ölümsüz olur.
- `FootballMode`: kazanma = `goalsToWin` gol, kaybetme = can bitti veya süre doldu. Yıldızlar: süre ya da hasarsız bitirme.

### 7.3 Düşmanlar
- **Kenar Futbolcusu:** SideLine hareketi + Aimed sarı kart (dönerek uçan kart, kart görseli spin).
- **Hakem** (bölüm 5+): Arenada Wander. Arada durur, düdük çalar (telegraph), kırmızı kart gösterip fırlatır.
- **Defans:** Oyuncuya doğru kayarak müdahale eder (Charge). Değerse top düşer.
- **Kaleci:** Kalede sağa sola gider, şutu tutabilir. Güçlü şut kaleciyi geçer.

### 7.4 Bölüm tasarımı (10 bölüm)
- 1: Sadece top sürme ve gol (düşman yok, 3 gol).
- 2-3: 1-2 kenar futbolcusu.
- 4-5: Kaleci tanıtımı + güçlü şut öğretimi.
- 6-7: Hakem + kırmızı kart.
- 8-9: Defans + karışık.
- 10: **Final maçı:** süre sınırı, tam kadro, tribün coşkusu.

### 7.5 Görsel ve ses
- Çim sahası (çizgili biçilmiş çim deseni), beyaz çizgiler, kale ağları, tribün silüeti (arka katman, gol olunca zıplayan seyirci).
- Sesler: düdük, tribün uğultusu (skorla yükselir), şut, direk sesi.

## Kabul kriterleri
- Top sürme ve şut dokunmatikte rahat ve tatmin edici. **Bunu telefonda test edip onaylamam gerekiyor.**
- 10 bölüm oynanabiliyor. Kart kuralları ve top kaybı kararlaştırıldığı gibi çalışıyor.
- Futbola özel kod sadece `Ball`, `BallCarrier`, `Goal`, `FootballMode` içinde. Düşmanlar genel `Enemy` sistemiyle yapılmış.
