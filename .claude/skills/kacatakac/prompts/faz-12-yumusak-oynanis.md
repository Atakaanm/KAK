# FAZ 12 — Telefon testi 2: iki kişilik hatalar, Buz ayarları, serbest kontrol düzeni, Görünmezlik, yumuşak oynanış

> Kaynak: kullanıcının Faz 11 sürümünü telefonda oynaması (2026-10-03). Kullanıcının sözleri toparlandı, Claude'un eklemeleri **(+)** ile işaretli.
> **Ana ilke (kullanıcı, "çok çok çok önemli"): oyunun oynanışı yumuşak (soft) olmalı.** Her kararda sor: haksız, sert, ani, kafa karıştırıcı bir an var mı? Varsa uyarı, geçiş, bağışlayıcılık ekle.
> Dal adları: `faz-12-h1-iki-kisilik` … Her alt faz: plan → uygula → test (EditMode/PlayMode + bot ölçümü) → ekran görüntüsü → skill güncelle → PR + birleştir. Sonunda telefona kurulum (kullanıcı kabloyu takınca).
> Kullanıcıya her adımda kısa Türkçe açıklama: nerede ne yapıldı.

## H1 — İki kişilik hatalar
1. **Ada dönünce görünmüyor** (hata): düşüp 10 sn sonra dönen 2. oyuncunun görüntüsü yok. Teşhis + test (dönen oyuncunun sprite'ı görünür, opak, kendi çocuğu).
2. **Düşen oyuncu sahadan kalksın:** şu an vurulduğu yerde soluk duruyor ve yanından geçen taşlarla "yakın geçiş" puanı kazanıyor (mantıksız). → Düşünce kısa "puf" efektiyle kaybolur (görüntü, gölge, çarpışma yok; yakın geçiş, altın toplama, hedeflenme yok). Dönüşte belirir.
   (+) Dönüş noktası arena ortası değil, o an **taşlardan en uzak güvenli nokta**; yumuşak belirme (büyüyerek) + 2,5 sn dokunulmaz yanıp sönme.
3. **Soğuk göstergesi iki kişilikte bozuk:** tek çubuk "en çok üşüyeni" gösteriyor, Ada'nın satırının yanında durduğu için "sadece Ada'da var" sanılıyor; Ata ateş alınca çubuk azalmıyor. → İki kişilikte **her oyuncunun kalp satırının yanında kendi soğuk çubuğu**; ateş alanın kendi çubuğu düşer.
   (+) Ateş yakındaki arkadaşı da yarı ısıtır (1,5 birim): birlikte oynamayı ödüllendirir.
4. **Donma sonrası soğuk geçmiyor:** donup çözülünce oyuncu mavi ve yavaş kalıyor (değer 0,82'de kalıyordu). → Donma "soğuğu atar": çözülünce değer yavaşlama eşiğinin altına (0,45) iner, mavi ton kalkar. Ceza = donukken taşlara açık kalmak; sonrası temiz başlangıç (yumuşak).
5. **İki kişilikte eşyalar ×1,5 sık** (ateş vb. bulup kurtulabilsinler); garantili ateş de daha sık.

## H2 — Buz Gölü ayarları
1. **Taş Yağmuru → Sarkıt Yağmuru:** Buz'da olay taş değil buz sarkıtı yağdırır, afiş "SARKIT YAĞMURU". (+) Olaylar dünyaya göre veri: diğer olayların da buz karşılığı (Yuvarlanan Kaya → Dev Kartopu) varsa onlar da.
2. **Kartopu çarpana kadar büyüsün** (şu an az büyüyor): yol aldıkça büyümeye devam eder ama oyunu aşırı zorlaştırmaz → (+) büyüdükçe ağırlaşıp biraz yavaşlar (büyük = okunur ve kaçılabilir), gölgesi büyür, ekranda dev kartopu sınırı. Bot ölçümüyle doğrula.
3. **Buz ayakkabısı hızlandırmasın:** amacı kaymayı ve savrulmayı kaldırıp **normal arenadaki gibi** hareket ettirmek. Dikey hız artışı da (×1,2) ayakkabıyla ×1'e döner. (+) HUD çipi/yazısı "KAYMAZ".

## H3 — Serbest kontrol düzeni (Küçük/Orta/Büyük yerine)
1. Ayarlar → **KONTROLLERİ DÜZENLE**: oyun ekranının önizlemesi üzerinde joystick ve aksiyon düğmesi **sürüklenip istenen yere** konur (arenanın üstü dahil, oyuncunun tercihi), **boyut kaydırıcısı** (her biri ayrı), **SAĞ EL / SOL EL** (joystick ile düğmenin yerini aynalar), **VARSAYILAN**.
2. Kayıtta saklanır, oyunda hemen uygulanır. (+) Joystick yerleştirilen yerde dinlenir, parmak biraz kaysa da tutar (kayan joystick korunur). Eski boyut ayarı yeni ölçeğe taşınır.
3. (+) İki kişilikte ekran ikiye bölünmüş kalır (her yarıda kayan joystick); düzen ayarı tek kişilik için.

## H4 — Görünmezlik (yeni eşya), Hayalet korunur
1. **Görünmezlik:** birkaç saniye fırlatıcılar seni göremez → taşları **rastgele yönlere** atar (sana doğru dar koni hariç), güdümlü taşlar kilidi kaybeder, göktaşları rastgele yere düşer. Kullanıcı: "Hayalet unique bir özellik, onu boşa götürmeyelim, yeni özellik olarak koyalım" (Pranga gibi ayrı eşya).
2. **Hayalet** aynı kalır (taşlar içinden geçer). Ayrım net olsun: Hayalet = taş geçer, Görünmez = taş sana atılmaz.
3. (+) Görünüş: karakter titreyen yarı saydam; fırlatıcıların başında "?" (seni arıyor). HUD çipi + süre. Ağırlık/kademe dengesi.

## H5 — Yumuşak oynanış (akıcılık) turu (+)
1. Hareket ve görüntü akıcılığı: fizik 50 Hz / ekran 60-120 Hz arası takılma (Rigidbody2D interpolation), joystick tepki eğrisi, animasyon geçişleri.
2. Sert anları yumuşatma: vuruş sarsıntısı ve hit-stop ölçüsü, eşyaların kaybolmadan önce yanıp sönmesi, taşların belirişi, panel geçişleri, ses seviyeleri.
3. iPhone 15 Pro Max (120 Hz) kare hızı kararı: 60 sabit / 120 seçeneği (pil). Ölçüm.

## Sonraki dünyalar (kullanıcı ne atılacağına karar verecek; tasarım önerisi hazır)
### Karanlık Mağara
- Hiçbir şey görünmez; karakter **elinde meşale** taşır, ışık çemberi kadarını görür. **Meşale altınla seviye seviye büyür** (kalıcı yükseltme; karakter detay ekranına yeni satır ya da dünya yükseltmesi).
- (+) Yumuşaklık için: fırlatıcıların gözleri karanlıkta parlar (yer ipucu), atılan şeyler kıvılcım/parıltı izi bırakır ve atış sesi verir (görünmez vuruş haksız olmasın), meşale sönmez (yakıt yok), duvar dibinde hafif yosun parıltısı.
- Ne atılacağı (seçenekler): yarasa sürüsü (dalgalı uçar), düşen sarkıtlar (damla sesiyle uyarı), parlayan mantar sporları (yavaşlatır), örümcek ağı (yapıştırır), kaya.
- Teknik: Light2D yok (Universal Renderer); ucuz karanlık maskesi (tek sprite, yumuşak kenarlı delik). Performans bütçesi.
### Futbol Arenası
- Sarı/kırmızı kart fırlatılır, karakterler kaçar (sarı: kısa yavaşlama, kırmızı: kısa donma/"oyundan atıldın" sayılmaz, yumuşak).
- (+) Toplar seker, hakem düdüğü olay uyarısı, ara sıra "gol fırsatı": yerde top belirir, kaleye götür/şut → bonus altın. Karar kullanıcıda.
