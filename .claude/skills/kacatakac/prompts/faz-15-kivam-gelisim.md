# FAZ 15 — Sonsuz kıvamı ve gelişim (survivor.io ilhamı)

> Kaynak: kullanıcı (2026-10-04): "Adım adım zorluğu artan, her artışta çaktırmadan kaosun arttığı sonsuz mod. İlk oyunlar baya yavaş, ilerledikçe hızlanacak; skor hızlandırma gibi yetenekler geldikçe oyun onlar için daha hızlı olacak. Subway Surfers gibi oyuncuyu hemen bitirip sıkmayacağız ama hep yavaş da oynatmayacağız. Karakter gelişiminde sadece 3 özellik hoşuma gitmedi, birçok özellik istiyorum. Eşyalara geliştirme (kalma süresi, çıkma sıklığı, yerde bekleme süresi), mini animasyonlarla. Sandık açma, geliştirme miktarını artıracak şeyler. Meşale ele cuk otursun, sağa sola dönerken hangi eldeyse düzgün dursun. İleride ayakkabı, şapka, kalkan, kılıç; sandıktan bulup geliştirecekler, ön yolunu hazırla. Taşlar ve piksel güzel, biraz daha cafcaflı renkler. Survivor.io'dan ilham al: orada silahını büyütüp yaratıkları parçalıyorsun, burada kaçıyorsun. Sonsuzu harika kıvama getir, sonra bölümlere (level) geçeceğiz."
>
> Kullanıcı kararları (hepsi önerilen): **ortak gelişim + karakter özelliği** (tüm karakterler tek gelişim yolunu paylaşır, her karakterin kendine özgü pasifi var), **oyun içi 3 karttan 1 seçme Faz 15'in sonunda**, **sandık = altın + oyunda düşen anahtar** (gerçek para yok, ileride reklam/satın alma bağlanabilir).

## Araştırma özeti (2026-10-04)
- **Survivor.io** (Habby): oyun içinde her seviye atlamada 3 rastgele yetenekten 1 seçilir (aktif ≤6, pasif ≤6; pasif en fazla Sv5, aktif Sv5 + eşleşen pasif = EVRİM). Kalıcı katman: Evrim tablosu (altınla sırayla temel özellikler ATK/HP/DEF/iyileşme, özel düğümler), 6 ekipman yuvası (silah, kolye, eldiven, zırh, kemer, ayakkabı), nadirlik gri → yeşil → mavi → mor → sarı → kırmızı, **3 aynı parça birleşince bir üst nadirlik** ve yeni bir yuva yeteneği açılır; seviye = altın + yuvaya özel "tasarım". Sandık türleri (sıradan / anahtarlı + 10 açılışta garanti / S sınıfı). Kaynaklar: [OCG ekipman](https://onechilledgamer.com/survivor-io-equipment-guide/), [BlueStacks yetenek/evrim](https://www.bluestacks.com/blog/game-guides/survivor-io/sio-skills-evolution-guide-en.html), [mturbogamer Gold DNA](https://mturbogamer.com/2022/09/survivor-io-how-to-get-key-evolution-gold-dna/).
- **Subway Surfers:** koşu yavaş başlar, sürekli hızlanır, bir hız tavanına ulaşır; ondan sonra tek iş hayatta kalmak. Skor çarpanı görevlerle kalıcı artar; güçlendirmelerin süresi altınla 6 seviye yükseltilir; "headstart" ile yavaş baştan atlanır. Kaynak: [Subway Surfers wiki çarpan](https://subwaysurf.fandom.com/wiki/Multiplier), [Mobi.gg](https://mobi.gg/en/tips/subway-surfers-high-score/).
- **Bizim farkımız:** saldıramazsın, kaçarsın. Survivor.io'daki "silah" bizde **kaçış yeteneği**: mıknatıs, kalkan yenilenmesi, yavaşlatma halkası, kıl payı patlaması, ara sıra taş kıran kılıç...

## Kimlik (Faz 14'ten güncellendi)
**Kaç Ata Kaç = tek ekranlık, tek parmakla oynanan kaçış arcade'i + kalıcı gelişim.** Döngü: **kaç → sıyır → topla → güçlen → daha hızlı oyna.**
Ölçüt (Faz 14'ten sürer): "Bu, kaçışı daha iyi/okunur yapıyor mu?" Yeni ölçüt: "Oyuncu bir sonraki oyunda kendini biraz daha güçlü hissediyor mu, oyun onu biraz daha hızlı karşılıyor mu?"
Faz 14 dersi: **genişlik değil derinlik.** Her yeni sistem tek ekranda, az yazıyla, ikonla anlatılır; adım adım açılır (3c drip-feed). Menüde tek ana eylem OYNA kalır.

## Fazlar

### K1 — Tempo ve kaos eğrisi (sonsuz kıvamı)
**Sorun:** zorluk 6 sert kademe (0/30/75/150/240/360 sn); her geçişte afiş + aynı anda birçok değer sıçrıyor → "zorlaştı" hissi kademeli değil, kopuk.
**Çözüm:** `TempoProfile` (SO) + `TempoDirector` (Managers): oyun boyunca tek sürekli değer **τ (tempo, 0 → 1)**. Her kaos düğmesi τ'nun eğrisi:
- mermi hızı, atış aralığı, aktif fırlatıcı sayısı, taş türü ağırlıkları, çift atış olasılığı, olay sıklığı, taş boyutu, skor/sn.
- **Düğmeler sırayla açılır** (aynı anda hepsi değil): önce hız ve aralık ince ince, sonra 2. fırlatıcı, sonra türler, sonra çift atış... Her artış küçük, fark edilmez.
- **Yeni fırlatıcı yumuşak uyanır:** gözleri yanar, ilk atışları yavaş, 10 sn'de tam hıza çıkar (sayının 1 → 2'ye sıçraması yerine).
- **Nefes payı (gerilim–rahatlama):** olay bitince 6 sn hafif sakinlik; hasar alınca tempo artışı 8 sn durur (merhamet, görünmez).
- **Kademe afişi yok.** Eski `CurrentStageIndex` uyumluluk için τ eşiklerinden türetilir (eşya `minStage`, olaylar, testler).
- **İlk oyunlar yavaş (çırak):** `gamesPlayed < 3` → ilk 40 sn ek yavaşlık (hız ×0,85).
- **Güce göre tempo:** `Güç` (gelişim seviyelerinin normalize toplamı, K3) → başlangıç τ0 = 0,3 × güç, rampa süresi 360 → 300 sn. Skor/sn = taban × (1 + 0,75 τ) → güçlenen oyuncu daha hızlı oyunda daha hızlı skor yapar ("skor hızlandırma").
- Tavan: τ = 1 yaklaşık 6. dakikada; sonrasında yalnız olaylar yoğunlaşır (Subway gibi hız tavanı).
- Eski kademeler (Buz vb. gizli dünyalar) profil yoksa aynen çalışır (geri uyumluluk).
**Hedef (bot, aynı gün taban çizgisiyle):** güç 0 acemi medyan 60-120 sn (ilk 30 sn sakin), güç 0 usta 150-300 sn, tam güç usta 240-420 sn. Hiçbir saniyede bir düğme %8'den fazla sıçramaz (test).
**Testler:** τ artar (nefes payı dışında), sıçrama sınırı, yumuşak uyanma, çırak yavaşlığı, güç ofseti, profilsiz geri uyum.

### K2 — Cafcaflı piksel (görsel canlılık)
Kullanıcı: "taşlar ve piksel güzel, biraz daha cafcaflı renkler, kullanıcıyı yakalamak için".
- Taşlar: daha doygun sıcak kenar ışığı + küçük parıltı pikseli; tür işaretleri (göz, çatlak, mor) daha parlak.
- Altın: dönen parıltı animasyonu, toplanınca renkli piksel konfeti. Eşyalar: renk rolüne göre nabız gibi atan hâle halkası.
- Efektler: yakın geçiş kıvılcımı, eşya alma patlaması, skor açılır yazıları renkli; meşale ışık havuzları daha sıcak.
- Global renk tonlaması (Volume) doygunluk/kontrast hafif yukarı; zemin yine geri planda (değer hiyerarşisi korunur).
- **Önce 2 önizleme** (şimdiki / canlı / çok canlı) yan yana → kullanıcı seçer (görsel onay kullanıcıda). Gri ton + renk körlüğü testi (`kak_color.py`).

### K3 — Özellik (stat) çekirdeği
- `StatId` (sona eklenen enum): MaxHearts, MoveSpeed, Agility, Hurtbox, InvulnTime, Magnet, CoinGain, ScoreGain, NearMissBonus, NearMissRadius, ComboKeep, ComboMax, PowerDuration, ItemFrequency, ItemGroundTime, Revive, StartShield, ShieldRegen, WarningTime, ShackleResist...
- `StatSheet`: kaynakların toplamı (sabit + yüzde): karakter tabanı (PlayerData) + ortak gelişim + karakter pasifi + eşya geliştirmeleri + ekipman (K7) + oyun içi kartlar (K8). Oyun başında ve değişince bir kez hesaplanır, oyunda dizi okuması (0 GC).
- Oynanış okuyucuları: PlayerMovement2D (hız, çeviklik), PlayerHealth (can, dokunulmazlık, diriliş), PlayerHitbox (gövde), Coin (mıknatıs, değer), ScoreManager (skor, kıl payı, çarpan), PowerupSpawner/PowerupPickup (sıklık, yerde kalma, süre).
- Veri: `UpgradeTrackData` (SO): id, StatId, seviye başına değer, fiyat eğrisi, açılma koşulu, ikon, Loc anahtarları. Ayar kodda değil veride.
- Kayıt: `SaveData.upgrades` (id → seviye). **Göç:** eski `charLevels` (karakter başına Can/Hız/Güç) için harcanan altın iade edilir, bir kez bilgi gösterilir; eski liste silinmez.

### K4 — Ortak gelişim ekranı + karakter pasifleri
- **GELİŞİM** ekranı (KARAKTER düğmesinin yerine bir merkez; sekmeler: Karakter · Gelişim · Eşyalar). Gelişim izleri gruplu: Hayatta kalma (Can, İnce yapı, Dokunulmazlık, İkinci şans, Başlangıç kalkanı), Hareket (Hız, Çeviklik), Kazanç (Mıknatıs, Altın bereketi, Skor ustası, Kıl payı ustası, Çarpan hafızası, Çarpan tavanı).
- **Adım adım açılır** (gelişim seviyesi toplamıyla); kilitli iz gri ve "?" ile merak uyandırır.
- **Mini animasyonlar:** yükseltince ikon zıplar, çubuk dolar, kıvılcım patlar, "+1" yükselir, ses; yeterli altın yoksa hafif sallanma.
- Karakter pasifleri (veride `StatModifier` listesi): Ata dengeli (+%10 güç süresi), Ada kıl payı ustası, Çevik ince yapı + çeviklik, Tank başlangıç kalkanı + 2 can, Şanslı altın + eşya sıklığı. Kart: portre, ad, pasif ikonu + tek satır.

### K5 — Eşya geliştirme
- Her eşya (Kalp, Kalkan, Yavaşlat, Hayalet) için 3 iz: **Süre** (Kalp'te: alınca kısa dokunulmazlık), **Sıklık**, **Yerde kalma**; en üst seviyede **özel**: Kalkan 2 vuruş, Yavaşlat daha güçlü, Hayalet + mıknatıs, Kalp + kalkan parçası. Pranga için tek iz: **Pranga direnci** (daha kısa, daha zayıf).
- Fiyat: altın; üst seviyeler + **Parşömen** (sandıktan; sandığın anlamı bu).
- Eşyalar sekmesi: kartta eşyanın gerçek sayıları (sn, sıklık), yükseltme animasyonu (ikon döner, hâle büyür, sayı tıkır tıkır artar).

### K6 — Anahtar ve sandık
- **Anahtar:** oyunda süre eşiklerinde (60/120/180/240 sn, oyun başına en fazla 3) ve nadiren arenada düşer (risk/ödül). Yeni rekor +1.
- **Sandıklar:** Ahşap (1 anahtar: altın + parşömen), Gümüş (3: daha çok + nadir ödül şansı), Altın (10 ya da her 10 açılışta garanti: büyük ödül; ileride ekipman). Garanti sayacı kayıtta.
- **Açılış animasyonu:** sandık sallanır, kapak ışık hüzmeleriyle patlar, ödüller kart kart nadirlik parıltısıyla çıkar, sayılar sayılır; dokununca hızlanır.
- **Geliştirme miktarını artıranlar:** Altın bereketi izi, oyunda "Altın yağmuru" anı, rekor ödülü, sandık altını en iyi skora göre ölçeklenir.
- Menü: OYNA (büyük) · altında GELİŞİM ve SANDIK (anahtar sayısı rozetiyle) · 2 KİŞİ küçük. Ekonomi değerleri tek SO'da (`EconomyData`).

### K7 — El noktaları, eldeki eşya, ekipman altyapısı
- **El noktaları:** `tools/kak_hand_anchors.py` her karakterin her karesinde (5 karakter × 8 yön × 5 kare) sağ/sol eli bulur (ten rengi kümeleri, gövde bandında, uç noktalar) + önizleme sayfası (noktalı) + elle düzeltme JSON'u → editör aracı `CharacterAnchors` SO'suna yazar (kare → sağ el, sol el, önde/arkada, baş, ayak).
- **`HeldItem`:** eşyayı her karede seçili ele oturtur (piksel ızgarasına), el arkadaysa gövdenin arkasında, öndeyse önünde çizilir; sağa/sola dönüşte el ve yön doğru. **Meşale** bununla yeniden bağlanır (Karanlık Mağara açılınca hazır).
- **Ekipman altyapısı:** yuvalar Şapka, Ayakkabı, Ana el (kılıç, meşale), Yan el (kalkan); nadirlik Sıradan/İyi/Nadir/Destansı/Efsane; 3 aynı → bir üst nadirlik; seviye = altın + yuva tasarımı; envanter kaydı (`SaveData.inventory`, takılı yuvalar). Ekipman özellikleri `StatModifier` (K3). İçerik ve ekran `KakScope.Equipment` kapalı; kılıç fikri: "ara sıra yaklaşan taşı parçalar" (survivor.io silah karşılığı, ileride).

### K8 — Oyun içi kart seçimi (kaçış yetenekleri)
- **Cesaret çubuğu:** kıl payı ve altınla dolar; dolunca oyun yarım saniyede yavaşlar → 3 piksel kart (ikon, ad, tek satır) → dokun seç → 1 sn güvenli devam.
- Havuz (~12): Mıknatıs halkası, Kalkan yenilenmesi, Yavaşlatma halkası, Kıl payı patlaması, Hız+, Altın+, Skor+, Hayalet anı (vurulunca 2 sn), Dash (düğme belirir), Taş kırıcı (her 15 sn en yakın taşı kırar), Uyarı gözü ("!" erken), Kalp parçası (3 parça = +1 kalp). Kart seviyesi 1-5; ileride evrim (ikili eşleşme).
- Kartlar `StatModifier` + küçük davranış bileşenleri. Tempo artmaya devam eder (K1); denge botla.

### K9 — Denge, telefon, mağaza
- Bot ölçümleri (K1 hedefleri + gelişimli profiller), ekonomi akışı simülasyonu (kaç oyunda ne açılır), telefona kurulum, kullanıcı değerlendirmesi, mağaza metni ve görselleri yeni özelliklerle.

## Kurallar (her K'de)
- Ana prompt (`00-ana-prompt.md`): 0 GC oyun içi, ayarlar veride, Loc TR/EN, sahne YAML'ı elle değil editör aracıyla, testler + skill güncellemesi, faz başı dal → PR → merge.
- Yeni sistemler `KakScope` ile açılır/kapanır; gizli dünyaları bozmaz.
- Görsel/his kararlarında önizleme → kullanıcı onayı; beklerken bağımsız işe geç ("Onay bekleyenler").
