# FAZ 3 — Görsel Temel ("Basit ama Süper")

**Süre tahmini:** 3-4 oturum · **Dal:** `faz-3-gorsel` · **Ön koşul:** Faz 0 sanat kararı (aşağıdaki metin A: piksel sanatı seçildiğini varsayar)

## Amaç
Mevcut Dungeon arenası ve Sonsuz Mod, telefonda açıldığında "vay" dedirtecek kadar cilalı görünsün. Yeni içerik değil, **standart ve cila**. Burada belirlenen standart sonraki tüm dünyalar için şablon olacak.

## Görevler

### 3.1 Sanat standardı belgesi — `SKILL.md` → "Sanat Standardı"
- PPU, karakter boyutu, referans çözünürlük, palet (hex listesi), yön sayısı ve aynalama, animasyon fps (idle 6, run 10-12, attack 12), dış hat kuralı (1px koyu kontur veya konturu yok), ışık yönü (sol üst), gölge (yarı saydam elips).
- **Yapay zeka ile sprite üretim prompt şablonu:** Aynı stil, palet ve kamera açısını koruyan, karakter/düşman/mermi/arena için hazır İngilizce promptlar. Üretimden sonra: palete indirgeme, 1:1 piksele küçültme, arka plan temizliği, filigran kontrolü.
- Çizim listesi: hangi karakter için hangi karelerin gerektiği (5 yön × idle/run/attack/hurt).

### 3.2 İçe aktarma ve ölçek düzeni
- Editör aracı `KacAtaKac/Sprite Import Standardı Uygula`: seçili klasördeki sprite'lara `Filter Mode = Point`, `Compression = None`, `PPU = standart`, `Pivot = Bottom Center` (karakterler, ayak altı) uygulasın. Bir `AssetPostprocessor` ile yeni eklenenlere otomatik uygulansın.
- Sahnedeki tüm `localScale` hilelerini (spawner 4x, arena 0.4x, player 0.8x) kaldır. Ölçek 1 olsun, boyut PPU'dan gelsin.
- Mevcut görselleri standarda getir: Arena 2048px'lik büyütülmüş görsel. Gerçek piksel boyutuna küçült (örneğin 8'e böl → 256px) ya da yeniden üret. **Sağ alt köşedeki yapay zeka filigranını (✦) temizle.**
- `Pixel Perfect Camera` bileşeni ekle ve `CameraFitWidth` ile uyumunu çöz (ikisi birlikte zoom yönetmemeli).

### 3.3 Render hattı → URP 2D Renderer
- Yeni `Renderer2D` asset'i oluştur, Mobile ve PC RP asset'lerine ata. Sprite materyallerini `Sprite-Lit-Default`'a çevir.
- Işık düzeni: Global Light 2D (düşük yoğunluk, hafif mavi-mor), arenadaki meşalelere Point Light 2D (turuncu, titreşen: `FlickerLight` scripti), oyuncunun etrafında hafif ışık.
- Duvarlar ve heykeller için `ShadowCaster2D` (mobil performansı ölç, gerekirse sadece yüksek kalitede açık olsun).
- Post-process (Volume): hafif Bloom (meşale ve parlayan mermiler), Vignette, Color Adjustments / renk tonlama, Film Grain yok.

### 3.4 His (juice) paketi — `FeedbackManager` + `GameEvents`
- **Ekran sarsıntısı:** vuruşta güçlü ve kısa, taş duvara çarpınca çok hafif. Ayarlardan kapatılabilsin.
- **Hit-stop:** vuruşta 50-80 ms zaman durması (gerçek zamanlı).
- **Vuruş flaşı:** beyaz flaş shader'ı ya da materyali (kırmızı tint yerine).
- **Squash & stretch:** yürürken hafif zıplama, dururken ezilme, hasar alınca büzülme.
- **Parçacıklar:** koşarken toz, taş yere çarpınca kırıntı, powerup alınca halka, ölümde patlama ve yavaş çekim.
- **Gölgeler:** tüm karakter, mermi ve item'ların altında yumuşak elips gölge.
- **Taş görünümü:** dönen taşa hafif iz (trail), yakın geçişte (near-miss) küçük "whoosh" sesi ve beyaz çizgi.
- **Sayılar ve yazılar:** `TextMesh` yerine TMP, piksel font, pop animasyonu.
- **Kamera:** oyuncuyu çok hafif takip (arena ekrandan büyükse) ya da sabit + hafif nefes efekti.

### 3.5 Ses kimliği (kısa)
- Taş fırlatma, çarpma, vuruş, powerup, ölüm, UI tıklama, zorluk kademesi geçişi (uyarı sesi) için ses listesi hazırla. Ücretsiz kaynak öner (ör. Kenney, sfxr/jsfxr ile üretim).
- Müzik: menü ve oyun, kademe arttıkça katman ekleyen dinamik müzik (ileride).

## Test
- Telefon çözünürlüğünde (Game view 1080×1920 ve 1170×2532) ekran görüntüsü al, önce ve sonra olarak bana göster.
- Profiler: 60 FPS, orta seviye Android hedefi, draw call sayısı raporu.

## Kabul kriterleri
- Tüm sprite'lar tek piksel yoğunluğunda ve keskin. Filigran yok.
- 2D ışık, bloom ve juice paketi aktif. Ayarlarda sarsıntı ve titreşim kapatılabiliyor.
- Sanat standardı ve yapay zeka prompt şablonları `SKILL.md`'de.
