# FAZ 14 — Odak ve kalite ("AI slop" temizliği)

> Kaynak: kullanıcı (2026-10-04): "sence de AI slop değil mi, çok fazla. Nişleştirelim, güzelleştirelim, odaklanmamız gereken yerleri incele, faz faz kaliteli ve odaklı yap."
> Kullanıcı kararları (hepsi Claude'un önerisi): **tek dünya kusursuz (Taş Zindanı)**, meta = **altın + karakter gelişimi**, **az ama net eşya** (5) ve olay (3), **2 kişilik kalır ama ikinci planda**.
> İlke: **genişlik değil derinlik.** Hiçbir sistem silinmez; kapsam dışı olanlar `KakScope` ile gizlenir, kodu durur, ileride güncellemeyle tek tek ve cilalı döner.

## Denetim bulguları (2026-10-04, ekran görüntüleriyle)
1. **Genişlik > derinlik:** 4 dünya (her biri ayrı hile: soğuk, meşale, kart, gol), 22 mermi, 13 olay, 10 eşya. Oyuncu hiçbirini öğrenemiyor; dünyalar sığ deri değişimi.
2. **Görsel tutarsızlık:** Zindan'ın ressamsı yüksek çözünürlüklü piksel sanatı ↔ diğer dünyaların iri prosedürel pikselleri ↔ Cinzel süslü fantastik logo ↔ yazı ağırlıklı paneller. Farklı oyunlardan toplanmış gibi.
3. **Yazı gürültüsü:** "YAKIN!", "PRANGA!", "TOP KAÇTI!", "ISINDIN!" gibi dünya yazıları her yerde.
4. **Meta yığını:** basit bir kaçış oyununda 5 karakter × 3 özellik, pet, görev, günlük ödül, meşale, hikâye, dünya kilitleri. Menüde 7 düğme + rozet + "YENİ!".
5. **Güçlü olan:** "saldıramazsın, kaçarsın" — tek ekran arena, okunur uyarılar (telegraph, "!", şerit), yakın geçiş gerilimi, kısa seanslar. Zindan en sağlam ve en güzel kısım.

## Kimlik (her kararın ölçütü)
**Kaç Ata Kaç = tek ekranlık, tek parmakla oynanan bir kaçış arcade'i.** 30 sn – 3 dk seanslar, ustalıkla uzayan koşular, her ölümde "bir daha". Her şey okunur, her ölüm adil, ekran sakin.
Soru: "Bu, kaçışı daha iyi/daha okunur yapıyor mu?" Hayırsa → çıkar ya da gizle.

## Fazlar
### Ö1 — Kapsamı daralt (geri alınabilir)
- `KakScope` (Core): Worlds, Pets, Missions, DailyReward kapalı. FeatureGate kapalı özelliği hiç açmaz; menüde düğmesi görünmez; DÜNYALAR yok, seçili dünya hep Zindan.
- Zindan eşyaları 5: Kalp, Kalkan, Yavaşlat (SloMo), Hayalet, Pranga (Hız ve Görünmezlik çıkar). Olaylar 3: Taş Yağmuru (gökten, "!"), Çapraz Ateş (dört köşe), Yuvarlanan Kaya (şerit). Sessizlik çıkar.
- Testler: ürün kapsamını doğrulayan test; gizli sistemlerin testleri kapsamı kendisi açar.

### Ö2 — Ana menü ve görsel kimlik
- Piksel logo (oyunun piksel diliyle; süslü Cinzel yerine), tek büyük OYNA, altında KARAKTER ve AYARLAR, 2 KİŞİ küçük ikincil. Rozet/"YENİ!" yalnız gerçekten yeni bir şey varsa.
- Arka plan: canlı arena kalır; istatistik satırı sade.

### Ö3 — Oyun içi sadelik
- Dünya yazıları: yalnız anlamlı anlar (yeni kademe, eşya adı kısa). "YAKIN!" yazısı yerine küçük kıvılcım + skor sayısı. Pranga/eşya yazıları ikonla.
- Oyun sonu: skor, en iyi, altın, sıradaki hedef, TEKRAR. Görev bloğu yok.
- HUD: kalp, skor, altın, duraklat — hepsi aynı ölçek ve hizada.

### Ö4 — Görsel tutarlılık ve cila
- 5 eşya ikonu tek stilde (aynı çerçeve/ışık), karakter ekranı: büyük sprite, az yazı (özellik = ikon + çubuk).
- Taş türleri (7) okunurluk denetimi: boyut/renk farkı net mi; gerekirse sadeleştir.
- Font ve renk: tek UI fontu, palet rolleri (altın = ödül, sıcak = tehlike, camgöbeği = iyi).

### Ö5 — His ve akış
- İlk açılış: hikâye kısa (gerekirse tek kart), ilk oyunda yalnız hareket ipucu.
- Zindan zorluk eğrisi (tek dünya artık tüm oyun): bot + telefon hissi; ses seviyeleri.

### Ö6 — Telefonda doğrulama
- Kurulum, kullanıcının "AI slop" gözüyle değerlendirmesi, kalan pürüzler.

## Kapsam dışı (kodu durur, sonra döner)
Buz Gölü, Karanlık Mağara (+ meşale), Futbol Arenası (+ gol fırsatı), petler, görevler, günlük ödül, Hız ve Görünmezlik eşyaları, Sessizlik olayı.
