# FAZ 8 — Dünya 3: Karanlık Arena

**Süre tahmini:** 3 oturum · **Dal:** `faz-8-karanlik` · **Ön koşul:** Faz 3 (2D Renderer ve ışıklar)


> **Güncel plan notu:** Bu dünya Sonsuz Mod v1.0 yayınından sonra gelir. Ön koşul: Faz 2B. Dünyanın görseli bir `ArenaTileSet` (arena + HUD duvar yüzü + koridor/kontrol alanı teması) ve paletin bu dünyaya ait alt tonlarıyla kurulur (`sanat-rehberi.md`). Kompozisyon değişmez: HUD bandı + kare arena + koridor/kontrol alanı. Performans bütçesi ana prompttaki gibidir.

## Fantezi
Zifiri karanlık bir mahzen. Sadece karakterin etrafı aydınlık. Düşmanlar ancak ateş ederken (namlu parlaması) ve mermiler **parlayarak** görünür. Atmosfer, gerilim ve ses ön planda.

## Görevler

### 8.1 Görüş mekaniği
- Global Light 2D neredeyse sıfır. Oyuncuda yumuşak kenarlı Point Light 2D (yarıçap `ArenaData`'dan), hafif nefes alan yoğunluk.
- Mermiler: kendi küçük ışığı + bloom (yoğun sahnede performansı ölç, gerekirse ışık yerine emissive sprite + bloom kullan).
- Düşmanlar: ateşten önce gözleri parlar (telegraph), ateş anında kısa flaş.
- **Ses ile yön:** stereo pan ile düşman atışının hangi taraftan geldiği duyulsun.

### 8.2 Işık öğeleri
- **Meşale pickup'ı:** Görüş yarıçapını 8 sn boyunca büyütür (`StatusEffect`).
- **Duvar meşaleleri:** Oyuncu yanından geçince yanar ve bölüm boyunca yanık kalır. Kalıcı olarak aydınlanan alan artar (keşif hissi).
- **Işık bombası:** Tüm arenayı 1 sn aydınlatır (nadir powerup).
- **Karanlık dalgası:** Belirli aralıklarla oyuncu ışığı da kısılır.

### 8.3 Düşmanlar
- **Gölge Atıcı:** Karanlıkta görünmeden yer değiştirir (Wander + görünmezlik), ateşte belirir.
- **Ateş Böceği Sürüsü:** Parlayan, yavaşça oyuncuya süzülen küçük tehlikeler.
- **Fener Bekçisi:** Işık konisi olan devriye. Koniye girersen tüm atıcılar sana kilitlenir (gizlilik anı).

### 8.4 Bölümler (10) ve görsel
- Öğretimden başlayan kademeli bölüm dizisi (Buz ve Futbol yapısıyla aynı).
- Görsel: mor-lacivert tonlar, sis parçacıkları, uzakta titreşen ışıklar.

## Kabul kriterleri
- Mobilde 60 FPS (ışık sayısı sınırı belirlenip belgelendi).
- Karanlık haksız değil: her tehlike ya ışık ya ses ile önceden haber veriliyor.
