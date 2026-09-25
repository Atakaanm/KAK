# FAZ 1.5 — Ekran Kompozisyonu: Tek Parça Dungeon Ekranı

**Süre tahmini:** 2-3 oturum · **Dal:** `faz-1.5-ekran` · **Ön koşul:** Faz 1 (`PlayableWorldRect`, retry düzeltmesi)

## Sorun
Kare arena, dikey telefonun ortasına yapıştırılmış bir resim gibi duruyor. Ekranın yaklaşık %54'ü (üstte mavi, altta gri) tasarlanmamış boşluk. Joystick bu boşlukta sıradan bir "varsayılan mobil kontrolcü" gibi görünüyor.

## Hedef
Ekranın her pikseli oyun dünyasına ait olsun. Kompozisyon `sanat-rehberi.md` §5'teki gibi [KESİN]:
```
[ ♥ ♥ ♥           SKOR / SÜRE          ⏸ ]  HUD bandı = üst duvarın 2.5D ön yüzü
[ ┌──────────────────────────────────┐ ]
[ │  KARE ARENA — ekran enine ölçekli │ ]  oyun alanı dünya biriminde her cihazda aynı
[ └─────────── arena kapısı ────────┘ ]
[    karanlık taş koridor · meşale · sis  ]  boyu cihaz oranına göre değişir
[   (joystick)                  (AKSİYON) ]  kontrol alanı
```

## Temel kurallar
- **Arena kare kalır ve genişliğe sığar.** Kamera, arena + yan duvar payı ekran enine tam sığacak şekilde ortografik boyutu hesaplar. Arena ekranda telefonun enine göre büyür veya küçülür. Oyun alanı dünya biriminde sabit olduğu için denge ve rekorlar adil kalır.
- **Dikey fazlalık iki bantta toplanır:** üstte HUD bandı (Safe Area + sabit yükseklikte duvar yüzü), altta koridor/kontrol alanı (geri kalan her şey). Kısa telefonda koridor kısalır. Kontrol alanı için minimum yükseklik tanımla. Minimumun altına düşülürse (tablet, 3:4) arena biraz küçülür ve yanlara duvar dışı dekor gelir.
- **Kontroller arenaya asla binmez.** Joystick sadece kontrol alanında belirir.

## Görevler

### 1.5.1 `ScreenComposer` (düzen hesaplayıcısı)
- Girdi: ekran boyutu, `Screen.safeArea`, arena dünya boyutu, HUD bant yüksekliği (dünya birimi), kontrol alanı minimum yüksekliği.
- Çıktı: kamera ortografik boyutu ve konumu, arena dünya dikdörtgeni, HUD bandı dikdörtgeni, koridor ve kontrol alanı dikdörtgeni (hem dünya hem ekran koordinatında).
- Tek kaynak: `ArenaAutoLayout`, `CameraFitWidth`, HUD yerleşimi ve joystick alanı bu çıktıyı okur. Kopya hesaplama kalmasın, eski sınıflardaki çakışan mantık silinsin.
- Ekran boyutu değişirse yeniden hesaplar (editörde Game view oran değişimi dahil). Değişmezse her kare hesaplamaz (performans).
- Faz 3'teki Pixel Perfect Camera ile uyumlu tasarla: ortografik boyut piksel ızgarasına yuvarlanabilir olsun (tam sayı ölçek veya kabul edilmiş kırpma).

### 1.5.2 Çerçeve ve koridor dünyası
- Arena dışını dolduran bileşen: `ArenaFrameBuilder`. `ArenaTileSet` (SO) kullanır: duvar üst yüzü (HUD bandı), yan duvarlar, köşeler, **arena kapısı** (alt duvarın ortasında), koridor zemini (tekrar eden), koridor duvarları, dekor (meşale, zincir, kırık taş, yosun, örümcek ağı).
- Koridor zemini kapıdan aşağı doğru devam eden taş yol olsun. Kenarlara doğru koyulaşsın (vignette), hafif sis katmanı (tek saydam katman, overdraw bütçesi).
- Meşaleler: arenanın yan duvarlarında ve koridorda. Faz 3'te 2D ışık olacaklar, şimdilik sprite + titreşen parlama.
- **Geçici karolar:** Kalıcı set Faz 3'te üretilecek. Şimdilik mevcut Dungeon görselinden kesilmiş karolar ya da düz palet renkli yer tutucular kullanılabilir. Önemli olan sistemin çalışması. **Mevcut arena görselindeki filigranı (sağ alt ✦) temizle** (bir editör aracıyla o bölgeyi komşu tuğla deseniyle doldur ya da o parçayı karo sistemine bırak).

### 1.5.3 HUD bandı
- Kalpler sol üst, skor/süre ortada ya da sağda, pause butonu sağ üst (şu anki siyah daire yerine palet renkli pause ikonu).
- HUD, duvar yüzünün **üstüne** oturur: duvarın taş dokusu arkada görünür, UI öğeleri taşa kazınmış veya asılmış levha gibi durur (Faz 3/4'te görsel cila).
- Safe Area: çentik ve dinamik ada alanında sadece duvar dokusu görünür, UI öğesi olmaz.
- `ArenaAutoLayout.ApplyUILayout`'taki dünya→ekran hesabı `ScreenComposer`'a taşınsın. UI her kare değil, düzen değişince güncellensin (şu an her `LateUpdate`'te hesaplanıyor, hover efekti UI animasyonuna taşınsın).

### 1.5.4 Kontrol alanı
- **Sol yarı: kayan joystick.** Kontrol alanının sol yarısında dokunulan yerde belirir. Tabanı yarı saydam, taş zemine gömülü (koyu halka), tutamak palet vurgusunda. Parmak kalkınca yavaşça kaybolur. Boşta iken "buraya dokun" ipucu olarak çok soluk bir iz kalır.
- **Sağ yarı: aksiyon butonu yeri.** Dash Faz 5'te gelecek. Şimdilik buton yerleşimi ve görsel yuvası hazırlansın (ya da gizli kalsın). Bana sor: şimdilik boş mu dursun, yoksa dekorla mı kapansın?
- Tek parmak desteği: Joystick alanı gerekirse tüm kontrol alanı olabilir (ayar).
- Dokunma alanları UI'ın üstünde değil, Input katmanında. Pause butonuna basınca joystick açılmamalı.
- `VirtualJoystick` yeniden yazılır. `PlayerMovement2D`'nin okuduğu arayüz (`Direction`) aynı kalır.

### 1.5.5 Oyuncu başlangıcı ve arena içi
- Oyuncu arenanın merkezinde başlar. Koridora çıkamaz (kapı kapalı, collider var). Kapı sadece görsel, ileride bölüm modunda "çıkış" olarak kullanılabilir.

## Test (otomatik ekran görüntüsü aracıyla)
- 1080×1920 (9:16), 1170×2532 (9:19.5), 1080×2400 (9:20), 1080×2520 (9:21), 1536×2048 (3:4): Her birinde ekran görüntüsü + gri tonlama kopyası. Bunları kendin incele, sonra bana göster.
- Kontroller: boş veya tasarlanmamış alan var mı? HUD çentikte mi? Arena kare ve ortalı mı? Kontrol alanı yeterli mi (en az 20 mm başparmak alanı)?
- Oynanış: joystick sadece kontrol alanında çalışıyor mu, pause çakışması var mı, Retry sonrası düzen doğru mu?
- Performans: düzen sadece ekran değişince hesaplanıyor mu (Profiler), sis katmanı overdraw'u.

## Kabul kriterleri
- 9:16–9:21 arası her oranda ekranın tamamı tasarlanmış görünüyor, tablette bilinçli kenar var.
- Tek bir `ArenaTileSet` değiştirilerek çerçeve ve koridorun görünümü tamamen değişiyor (test: renkleri değiştirilmiş ikinci set).
- Kullanıcı ekran görüntülerini onayladı (`progress.md` → Onay bekleyenler).
