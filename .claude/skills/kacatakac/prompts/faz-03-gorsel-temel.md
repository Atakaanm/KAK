# FAZ 3 — Görsel Temel: Renk Uyumu, Piksel Standardı, Işık ve His

**Süre tahmini:** 4-5 oturum · **Dal:** `faz-3-gorsel` · **Ön koşul:** Faz 0 (stil onayı), Faz 1.5 (ekran kompozisyonu, karo sistemi)

## Amaç
Sonsuz Mod'un tek ekranı telefonda "vay" dedirtecek kadar uyumlu ve cilalı görünsün. Kullanıcının bir numaralı kriteri **renk uyumu**. Bu fazda kurulan standart ve araçlar, sonraki tüm dünyaların şablonu olacak. Anayasa: `sanat-rehberi.md`.

## Görevler (sırayla)

### 3.1 Palet (önce bu, her şey buna bağlı)
- 3 aday palet hazırla: (a) mevcut Dungeon arenasından türetilmiş özel palet, (b) Endesga 32, (c) Resurrect 64'ün 32 renkli alt kümesi. Her biri için **mevcut ekranın o palete indirgenmiş halini** üret (editör aracıyla ekran görüntüsünü palete indirge) ve yan yana göster.
- Ben seçeyim. Seçilen paleti `Assets/Art/Palette/kak_palette.png` + `KakPalette` SO olarak kaydet, renk rollerini isimlendir (`sanat-rehberi.md` §3). `sanat-rehberi.md` §2'yi `[KESİN]` yap.
- **Palet denetçisi** editör aracı: `KacAtaKac/Palet Denetimi`. Seçili klasördeki sprite'larda palet dışı pikselleri say, raporla, isteğe bağlı en yakın renge indirge (orijinalin yedeğini al).

### 3.2 İçe aktarma standardı ve ölçek temizliği
- `AssetPostprocessor`: `Assets/Art/**` altındaki sprite'lara otomatik `Point`, `Compression None`, mipmap kapalı, PPU 32, karakterlerde pivot alt orta uygula. Mevcut sprite'lar için toplu uygulama aracı yaz.
- Klasör yapısı: `Assets/Art/{Palette, Characters, Enemies, Projectiles, Pickups, Tiles/Dungeon, UI, VFX}`. Eski `Sprites/` taşınırken referansların kopmaması için taşımayı `AssetDatabase.MoveAsset` ile yap.
- Sahnedeki ölçek hilelerini kaldır (spawner 4x, arena 0.4x, oyuncu 0.8x ve benzerleri). Boyut PPU'dan gelsin. Çarpışma kutularını yeniden ayarla, oyun hissinin değişmediğini ölç (oyuncu hitbox'ı, taş boyutu).
- **Pixel Perfect Camera** ekle ve `ScreenComposer` ile birlikte çalışacak şekilde ayarla (referans çözünürlük, yuvarlama, kırpma stratejisi). Farklı oranlarda titreme (jitter) ya da bulanıklık olmamalı.

### 3.3 Kalıcı görsel set (üretim)
- **Çizim listesi** ve yapay zeka prompt şablonlarını `sanat-rehberi.md` §8'e yaz:
  - Oyuncu (Boy): 5 yön × (idle 2-4 kare, koşu 6 kare, hasar 2 kare, dash 3 kare). W tarafı aynalama.
  - Fırlatıcı: 5 yön × (idle 2 kare, saldırı telegraph 3 kare + atış 3 kare).
  - Taş ve çakıl (+ 3 kırıntı parçası), gölge.
  - Powerup ikonları (5): aynı çerçeve ve dilde (camgöbeği ve yeşil çerçeve, iç ikon).
  - Dungeon karo seti: zemin (3 varyasyon + çatlak/yosun dekor), duvar üst yüzü, yan duvar, köşe, kapı, koridor zemini, koridor duvarı, meşale (4 kare animasyon), dekorlar.
- Üretimde bana yardımcı ol: promptları ver, gelen görselleri 1:1 piksele küçült, palete indirge, filigran kontrol et, sprite sheet'e dilimle, animasyonları ata. Mümkün olan her adımı editör aracıyla otomatikleştir (ör. `KacAtaKac/Sprite Sheet Dilimle ve Ata`).
- Görsel gelene kadar mevcut görseller palete indirgenmiş halde kullanılır.

### 3.4 Render hattı → URP 2D Renderer ve ışık
- `Renderer2DData` oluştur, Mobile ve PC RP asset'lerine ata. Sprite materyalleri `Sprite-Lit-Default`. UI unlit kalır.
- Işık planı (`sanat-rehberi.md` §6): soğuk Global Light 2D, sıcak titreşen meşale ışıkları (`FlickerLight`: gürültü tabanlı, GC'siz), oyuncunun çevresinde hafif ışık. Ekranda en fazla 6 ışık (bütçe).
- Normal map kullanma (piksel sanatında maliyetli ve tutarsız). Işık sadece renk ve yoğunluk olarak.
- Post-process Volume: Bloom (yüksek eşik), Vignette, Color Adjustments/LUT. Mobil kalite seviyesinde maliyetini ölç.

### 3.5 His (juice) paketi — `FeedbackManager` (`GameEvents` dinler)
- Ekran sarsıntısı (ayarlanabilir, kapatılabilir), hit-stop (gerçek zamanlı 60 ms), beyaz vuruş flaşı (materyal özelliğiyle, `MaterialPropertyBlock`, GC'siz).
- Squash & stretch: yürüme zıplaması, durma ezilmesi, hasar büzülmesi.
- Parçacıklar (havuzlu): koşu tozu, taş duvara ya da yere çarpınca kırıntı, powerup alınca halka, ölüm patlaması + yavaş çekim anı.
- Taşa sıcak kenar ışığı/kontur + gölge: zemin üzerinde okunurluk (renk rolleri).
- Tehlike telegraph'ı: fırlatıcı atıştan önce sıcak parlar (0.3-0.5 sn), ses ipucu.
- Floating text: TMP + piksel font, havuzlu (şu anki `TextMesh` + `new GameObject` kaldırılsın).

### 3.6 Renk doğrulama araçları
- `KacAtaKac/Renk Testleri`: Game view ekran görüntüsünü al → gri tonlama, deuteranopi, protanopi, bulanıklık versiyonlarını üret → `Screenshots/renk/` klasörüne kaydet. Claude bu görselleri okuyup raporlar: oyuncu ve taş her versiyonda seçilebiliyor mu?
- Değer histogramı raporu: 60-30-10 dağılımına ne kadar yakın?

## Performans kontrolü
Profiler'da: batch < 60, 0 GC tahsisi, ışık ve bloom maliyeti. Mobil kalite seviyesi ile yüksek kalite seviyesi arasında fark tablosu. Ölçüm sonuçları `progress.md`'ye.

## Kabul kriterleri
- Tüm sahne tek palet ve tek piksel yoğunluğunda. Palet denetçisi 0 palet dışı piksel raporluyor (VFX hariç).
- Renk testleri geçiyor. Kullanıcı önce/sonra ekran görüntülerini onayladı.
- Performans bütçesi içinde.
