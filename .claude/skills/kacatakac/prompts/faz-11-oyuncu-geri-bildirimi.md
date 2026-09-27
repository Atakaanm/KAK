# FAZ 11 — Telefon testi geri bildirimi: his, denge, ilerleme, iki kişilik mod, hikâye, Buz

> Kaynak: kullanıcının iPhone 15 Pro Max'te ilk oynayışı (2026-09-27). Kullanıcının sözleri toparlandı, Claude'un eklemeleri **(+)** ile işaretli.
> Dal adları: `faz-11-g1-his` … `faz-11-g7-buz`. Her alt faz: plan → uygula → test (EditMode/PlayMode + bot ölçümü) → ekran görüntüsü → skill güncelle → PR + birleştir → telefona kur (kullanıcı kabloyu takınca).
> Genel ilke (kullanıcı): **Oyun her şeyi hemen vermemeli; oyuncu adım adım güçlenmeli ki oyun bir çırpıda bitip köşeye bırakılmasın.** Oyunun kimliği "saldıramazsın, kaçarsın" korunur.

## G1 — His ve kontrol hataları
1. **Ölünce karakter kıpırdamasın:** son darbede hız anında 0, girdi kapalı, fizik durur (şu an birkaç kare kayıyor → ölüm hissini öldürüyor). (+) Kısa donma karesi + sarsıntı zaten var; karakter son pozda kalsın.
2. **Çapraz koşu hatası:** joystick 8 yön sınırında titreyince sprite her karede iki yön arasında gidip geliyor ("hem sağ hem sol koşuyor", hızlı görünüyor). → yön seçiminde **histerezis** (sınırı ±12° geçmeden yön değişmez), (+) koşu animasyon hızı gerçek hıza bağlı, (+) test: her yönde hız eşit.
3. **Oyuncu çok hızlı; hız güçlendirmesiyle "troll".** Başlangıç hızı düşsün (5 → ~3,9), hız güçlendirmesi ×1,5 → ×1,25. (+) Zorluk kademesinin oyuncu hız çarpanı yumuşasın.
4. **Göktaşı uyarısı:** gölge büyümesi yetmiyor → düşeceği yerde **"!" işareti** (yanıp sönen, sıcak renk), düşüşe yakın hızlanır.
5. **Altınlar yan yana 2-3'lü çıkmasın:** tek tek, aralarında zaman ve mesafe olsun.
6. **Dash rafa:** buton ve girdi kapalı (kod kalır; ileride karakter yeteneği olabilir). Karakter panelindeki DASH çubuğu kalkar.
7. **Ayarlar: kontrol boyutu** (joystick ölçeği Küçük/Orta/Büyük). (+) Kayıtta saklanır, oyunda hemen uygulanır.

## G2 — Zorluk ve skor dengesi
1. **Zorluk süreye bağlı, skordan ayrı** (şu an skor eşikleriyle ilerliyor; skor hızlandırıcıları zorluğu da hızlandırıyor → oynanamaz olur). Kademeler süreyle: Başlangıç 0 → Kolay ~30 sn → Orta ~75 sn → Zor ~150 sn → Cehennem ~240 sn → İmkansız ~360 sn. "Aşırı zor" 1 dakikada gelmesin.
2. **Taş dengesi:** büyük/küçük karışımı kademeye göre; başta küçük ve yavaş, büyükler seyrek. (+) Aynı anda ekrandaki büyük taş sayısına üst sınır.
3. **Yakın geçiş = skor hızı artışı** (Subway Surfers çarpanı gibi): her yakın geçiş skor çarpanını büyütür (üst sınırlı). (+) Hasar alınca çarpan kısmen düşer. Skor hızı zorluktan bağımsız.
4. (+) Bot ölçümüyle doğrula: usta bot medyan 3-5 dk, acemi 60-90 sn.

## G3 — Adım adım güçlenme: karakter ve pet gelişimi
1. **Tek canla başlangıç.** Can, altınla kalıcı olarak yükseltilir (seviye seviye).
2. **Karakter detay ekranı:** karakter kartına dokun → büyük portre + istatistikler (Can, Hız, Boyut…), her birinde "seviye atla" düğmesi (altın). Kostüm yuvası "yakında" (ileride kıyafet giydirme).
3. **Karakter arketipleri:** küçük olan avantajlı (küçük gövde), hantal olanın canı çok ama yavaş, hızlı olanın canı az. Her karakter herkes gibi değil; ileride her birine kendi yeteneği (dash herkeste olmayacak).
4. **Pet gelişimi:** (örn.) pet senin yerine vurulur; seviye atladıkça daha sık/daha güçlü.
5. (+) Ekonomi: altın kazancı ile yükseltme fiyatları, ilk yükseltme 1-2 oyunda, sonrakiler giderek pahalı.

## G4 — Kötü eşya: Pranga (gülle-zincir)
- Alınmaması gereken eşya: alırsan birkaç saniye **yavaşlarsın**, taşlardan zor kaçarsın. Görünüşü net (koyu gülle + zincir, kırmızı çerçeve). (+) Kolay kademeden sonra seyrek çıkar, HUD'da kırmızı ikon + süre.

## G5 — İki kişilik mod (yerel, tek telefon)
- Menüde **2 KİŞİ**. Ata (sol joystick) + Ada (sağ joystick). İki kişi oynar ya da tek kişi ikisini birden yönetir ("akıl oyunu").
- İki kişilikte yetenek yok (dash vb.).
- Biri ölünce oyun bitmez: **10 sn sonra döner**, döndüğünde **2-3 sn dokunulmaz**. İkisi aynı anda ölüyse oyun biter.
- (+) Ortak skor ve altın, geri dönüş sayacı HUD'da.
- İleride (not): **online battle royale** (10 kişi, sona kalan kazanır).

## G6 — Hafif hikâye / evren
- Subway Surfers'taki kadar hafif bir sebep: karakterler neden kaçıyor? Evren, dünyalar arası bağ. (+) İlk açılışta atlanabilir 3 kartlık giriş, dünya açıklamaları, mağaza metnine yansıtma.

## G7 — Buz arenası (ilk yeni dünya)
- **Kayma:** karakter kayar, anında duramaz; aşağı-yukarı giderken daha hızlı ama patinajlı. Dengeyi Claude kurar.
- **Kardan adamlar** köşelerde, **kartopu** atar; kartopu yol aldıkça **büyür**.
- **Eldiven** (eşya): kartopunu yakalar, eldivenliyken aksiyon düğmesi "fırlat" olur; fırlatılan kartopu başka kartopunu yok eder.
- **Buz ayakkabısı** (eşya): kayma olmadan rahat hareket (süreli).
- **Ateş** (eşya): toplanmazsa karakter yavaş yavaş **donar** (soğuk göstergesi); ateş ısıtır.
- (+) Dünya seçimi menüden (Zindan / Buz), Buz kendi sonsuz moduyla.

## Sonraki (not, şimdilik yapılmıyor)
- Futbol arenası, **boks ringi**, Karanlık dünya, (belki) Uzay dünyası (göktaşları).
- Kostüm/kıyafet sistemi, karakterlere özel yetenekler, online battle royale.
- Duvardan seken, takip eden, çarpınca parçalanan taşlar (Faz 2A'da altyapı var) → dünya ve kademelere dengeli yayılır.
