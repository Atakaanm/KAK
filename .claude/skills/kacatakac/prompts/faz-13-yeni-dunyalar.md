# FAZ 13 — Yeni dünyalar: Karanlık Mağara ve Futbol Arenası

> Kaynak: kullanıcı (2026-10-03): "iki yeni arena gelecek biri karanlık arena… karakter elinde meşale tutacak, meşaleye de parayla arttıra arttıra ne kadar önünü aydınlatırsa o kadar çok görecek… mağara olacak… futbol arenası… sarı kart kırmızı kart… topu bir yerden alıp kaleye şut". Ne atılacağına karar vermemişti; Claude'un önerilerini onayladı ("mantıklı dediğin gibi yapalım").
> **Ana ilke Faz 12'den devam: yumuşak oynanış.** Karanlıkta haksız/görünmez vuruş olmasın: her tehlike bir ipucu verir (parıltı, ses, "!").
> Dal adları: `faz-13-k1-karanlik` … Her alt faz: plan → uygula → test → ekran görüntüsü → skill → PR + birleştir. Sonunda telefona kurulum.

## K1 — Karanlık sistemi ve meşale (dünyadan bağımsız)
- Tema `darkness` (0-1) > 0 ise `DarkWorld.Apply`: kamera görüşünü kaplayan karanlık katmanı (SpriteMask dışında görünür), her oyuncuda **meşale ışığı** (maske deliği + yumuşak kenar halkası + sıcak parıltı, hafif titrer), elinde piksel meşale.
- **Meşale seviyesi** (kalıcı, altınla): yarıçap Sv1 1,7 → Sv2 2,1 → Sv3 2,5 → Sv4 2,9 birim; fiyatlar 250 / 600 / 1200. `SaveData.torchLevel`. Satın alma DÜNYALAR panelindeki Mağara kartında.
- **Karanlıkta ipuçları (yumuşak):** her taşın küçük parıltısı (yönü okunur), fırlatıcıların parlayan gözleri (atıştan önce turuncu yanar = uyarı), göktaşı/sarkıt "!" işareti, altın ve eşyalar karanlıkta parlar. İki kişilikte iki ışık.
- Testler: karanlık katmanı, meşale yarıçapı seviyeyle, satın alma, ipuçları karanlığın üstünde.

## K2 — Karanlık Mağara dünyası
- Sanat (`tools/kak_gen_cave.py`): mağara arenası (zindan/buz geometrisi: aynı oynanabilir alan, kaideler, alt girinti), çerçeve karoları, fırlatıcılar = **Taş Muhafızlar'ın mağara hali** (zindan fırlatıcısı koyu taşa boyanır, gözler parlar; hikâyeye uygun).
- Atılanlar (önerilen, onaylı): **yarasa** (dalgalı uçar: yeni hareket `Wave`), **sarkıt** (gökten düşer, "!" + damla), **parlayan spor** (yavaş, yavaşlatır, karanlıkta tamamen görünür), **örümcek ağı** (yapıştırır: kısa güçlü yavaşlama), **kaya**. Olay: GÖÇÜK! (sarkıt yağmuru), YARASA SÜRÜSÜ.
- Kademeler zindanın ×1,5 süresi, hızlar biraz düşük (karanlık zaten zor). Bot ölçümü (bot her şeyi görür → iyimser; acemi/usta hedef zindanın biraz üstü).
- DÜNYALAR: 4 kart (sığacak boyda), Mağara 6. oyunda açılır, dünya başına rekor (genel `SaveData.worldBests`).

## F1 — Futbol Arenası dünyası
- Sanat: yeşil çim şeritli saha, beyaz çizgiler, kaleler; fırlatıcılar = hakemler/futbolcular (8 yön, renkli forma), kenarda tribün karoları.
- Atılanlar: **futbol topu** (seker), **sarı kart** (yavaşlatır, 2 sarı = kırmızı; Faz 2B altyapısı `ProjectileEffect.YellowCard/RedCard`), **kırmızı kart** (can götürür, seyrek), (+) düdük olayı: "HAKEM DÜDÜĞÜ!" → kısa ateşkes ardından top yağmuru.
- Kartlar karakteri yavaşlatırken oyuncu üstünde kart simgesi (PlayerStatus ikonu).

## F2 — Gol fırsatı
- Ara sıra sahada top belirir ("GOL FIRSATI!"). Dokununca top ayağında (önünde) gider; rakip kaleye (üst kale) taşıyınca **GOL!** → bonus skor + altın, konfeti. Vurulunca top düşer. Kaçarken yapılan küçük bir hedef: oyunu bölmez, isteğe bağlı.

## Sonra (not)
- Boks ringi, kostümler, karakter yetenekleri, online battle royale (Faz 11 notları).
- 120 FPS ayarı (iPhone Pro), duraklat menüsünden kontrol düzeni.
