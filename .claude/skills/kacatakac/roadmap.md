# KaçAtaKaç — Vizyon ve Yol Haritası

> Hazırlanma: 2026-09-25. Her faz başında bu dosya gözden geçirilir ve güncellenir.
> Faz promptları: [prompts/](prompts/). Ana prompt: [prompts/00-ana-prompt.md](prompts/00-ana-prompt.md)

## 1. Vizyon (kullanıcının sözleriyle, toparlanmış)

İki ana oyun yapısı:

1. **Sonsuz Mod:** Sadece taştan kaçış. Gittikçe zorlaşır, amaç rekor kırmak. Oyunun çekirdeği ve vitrini.
2. **Bölüm Modu (Level Level):** Her dünyanın kendi arenası, fiziği, düşmanları ve kazanma koşulu var.
   - **Buz Arenası:** Zemin kaygan. Karakter hemen duramaz, gecikmeli durur ve kayar.
   - **Futbol Arenası:** Kenardaki futbolcular sarı ve kırmızı kart fırlatır. Oyuncunun ayağında top var, tek tuşla kaleye şut atıp gol yapmaya çalışır. Hem kaçıp hem gol atar.
   - **Karanlık Arena:** Görüş sınırlı, ışık oyunu. Mermiler karanlıkta parlar.
   - Ve daha fazlası: farklı düşman tipleri, farklı saldırı desenleri, farklı oyun kuralları.

**Görsel hedef:** "Basit ama süper." Az detay, çok cila. Tutarlı sanat stili, güçlü ışık, parçacıklar ve his (juice).

## 2. Değerlendirme: 2.5D bu oyuna uyar mı?

**Evet, bu oyun türü için en doğru bakış açısı.** Şu anki yapı teknik olarak "yukarıdan bakış (top-down) + Y derinlik sıralaması". Brawl Stars, Archero, Enter the Gungeon ve Nuclear Throne da aynı mantıkla çalışır. Arena tabanlı kaç-hayatta kal oyunlarında oyuncu tüm alanı tek ekranda görmek ister, bu bakış açısı bunu veriyor.

2.5D'nin bu oyuna kattıkları:
- **Yükseklik hissi:** Gökten düşen taşın yerde büyüyen gölgesi, kavisli (lob) atışlar, zıplayan top. Hepsi "Z yüksekliği + gölge" ile yapılır.
- **Derinlik:** Karakter heykelin önünde veya arkasında kalır (YDepthSorter zaten var).
- **Işık:** Karanlık arenada meşale ışığı ve dinamik gölge (URP 2D Lights).

### Mimari bu vizyonu nereye kadar taşır?

Mevcut kod temiz ve ScriptableObject tabanlı, bu iyi bir temel. Ama **"4 köşede sabit duran, oyuncuya düz taş atan fırlatıcı"** varsayımına sıkı bağlı. Futbol, kart atan hareketli düşmanlar ve kazanma koşulları için şu soyutlamalar gerekli (Faz 2):

| Bugün | Olması gereken |
|---|---|
| `CornerShooter` (4 köşe, sabit) | `Enemy` = Hareket + Saldırı deseni + Görsel (EnemyData ile) |
| `SpawnerDirectionAnimator` ve `PlayerDirectionSprite` (iki kopya kod) | Tek `DirectionalSpriteAnimator` (idle/run/attack durumları) |
| Mermi = düz giden taş | `ProjectileData`: hareket tipi (düz, güdümlü, seken, kavisli) ve etki (hasar, yavaşlatma, sarı kart, kırmızı kart) |
| Kazanma koşulu yok, sadece ölüm | `GameMode`: Survival, Timed, Football (gol sayısı), yıldız eşikleri |
| Arena = tek sprite + otomatik duvar | Arena **prefab**'ı: zemin, oynanabilir alan, spawn noktaları, kaleler, ışıklar |
| PlayerPrefs dağınık | `SaveSystem` (JSON): mod rekorları, bölüm yıldızları, jeton, kilitler, ayarlar |
| Yöneticiler birbirini `FindAnyObjectByType` ile arıyor | `GameEvents` olay sistemi + sahnede sabit yöneticiler |

Bu değişikliklerden sonra yeni bir dünya eklemek yeni kod yazmaktan çok **yeni veri ve prefab** üretmek olur. Asıl hedef bu.

## 3. Görsel teşhis: menü ve grafik neden "patates" görünüyor

İncelemede somut nedenler bulundu:

1. **Piksel yoğunluğu karışık.** Karakter 48px, dünya pikseli yaklaşık 0.008 birim. Arena 2048px'lik büyütülmüş piksel sanatı, bir "sanat pikseli" yaklaşık 0.03 birim. Spawner'lar 4x ölçekli, piksel başına yaklaşık 0.04 birim. Aynı ekranda üç farklı piksel boyutu var. Piksel sanatını amatör gösteren bir numaralı etken bu.
2. **Piksel sanatı bulanık.** Sprite'lar `Bilinear` filtreyle içe aktarılmış. Piksel sanatı için `Point (no filter)` + sıkıştırma kapalı olmalı.
3. **Arena görselinin sağ alt köşesinde yapay zeka üreticisi filigranı (✦) var.** Temizlenmeli ya da görsel yeniden üretilmeli.
4. **Fizik duvarları görsel duvarlarla hizasız.** Duvarlar sprite'ın dış kenarından hesaplanıyor (0.15 birim içeri). Görseldeki iç duvar ise yaklaşık 0.6 birim içeride. Oyuncu ve mermiler duvar çiziminin üstüne girebiliyor. Powerup alanı da bu yanlış sınırlardan hesaplanıyor.
5. **Render hattı 2D ışığa uygun değil.** URP "Universal (3D) Renderer" kullanılıyor, 2D Light'lar bu hatta çalışmaz. Karanlık arena ve genel ışık cilası için **2D Renderer**'a geçilmeli.
6. **His (juice) yok:** Ekran sarsıntısı, vuruş duraklaması (hit-stop), parçacık, toz, taş kırılma efekti, geçiş animasyonları eksik. "Basit ama süper" hissinin %70'i bunlardan gelir.
7. **Menü sahnesinde sadece "Oyna" butonu var.** Mod seçimi, bölüm haritası, karakter ve ayarlar ekranı yok.

### Sanat yönü kararı (Faz 0'da kesinleşecek)

| Seçenek | Artı | Eksi |
|---|---|---|
| **A) Disiplinli piksel sanatı** (öneri) | Mevcut işi korur. Tek kişiyle üretilebilir. 2D ışık ve parçacıkla çok iyi görünür (Enter the Gungeon, Nuclear Throne). Yapay zekayla üretime uygun. | Her yeni karakter için yön başına animasyon kareleri gerekir. Aynalama ile 8 yön yerine 5 yön çizilir. |
| B) Low-poly 3D + yukarıdan kamera (Brawl Stars tarzı) | En "premium" görünüm. 8 yön ve ışık bedava. Mixamo animasyonları kullanılabilir. | Görsel katmanın büyük kısmı ve fizik baştan yazılır, 3D modelleme öğrenmek gerekir. Birkaç ay gecikme. |
| C) Yüksek çözünürlüklü çizim/vektör 2D | Temiz ve modern görünür. | Tutarlı çok yönlü animasyon üretmek en zor olan bu. |

**Öneri A:** Tek ve sabit piksel yoğunluğu (örneğin karakter 32×32, 32 PPU, tüm dünya aynı ızgarada). Sabit bir renk paleti (örneğin 32 renk). Bunlara Pixel Perfect Camera, 2D Lights, bloom, parçacık ve ekran sarsıntısı eklenir. Hem "basit ama süper" hedefine hem tek kişilik üretime en uygun seçenek bu.

## 4. Fazlar

| Faz | Ad | Amaç | Çıktı |
|---|---|---|---|
| 0 | Karar ve hazırlık | Sanat yönü, git düzeni, mevcut işin kaydı | Kararlar `learnings.md`'de, temiz `main` |
| 1 | Stabilizasyon | Bilinen tüm hataları kapat, sağlam temel | Retry/menü döngüsü çalışır, item'lar doğru yerde, duvarlar hizalı |
| 2 | Mimari genişletme | GameMode, Enemy, Projectile etkileri, Arena prefab, Save, Events | Sonsuz mod aynen çalışır (regresyon yok), yeni sistemler hazır |
| 3 | Görsel temel | Piksel standardı, 2D Renderer, ışık, post-process, juice | Mevcut arena "süper" görünür |
| 4 | Menü ve UI | Ana menü, mod seçimi, bölüm haritası, karakter, HUD, geçişler | Mağazaya konabilecek kalitede arayüz |
| 5 | Sonsuz mod cilası | Taş çeşitleri, gökten düşen taşlar, olaylar, combo | Bağımlılık yapan sonsuz mod |
| 6 | Dünya 1: Buz | Kaygan fizik, kardan adam/yeti düşmanlar, 10 bölüm | İlk bölüm dünyası |
| 7 | Dünya 2: Futbol | Top sürme, şut, kale, kart atan futbolcular | Gol + kaçış modu |
| 8 | Dünya 3: Karanlık | Işık/görüş mekaniği, parlayan mermiler | Atmosferik dünya |
| 9 | Meta ilerleme | Jeton, mağaza, karakterler, yetenekler, ekipman | Uzun vadeli motivasyon |
| 10 | Mobil ve yayın | Android/iOS build, performans, cihaz testi, mağaza | Google Play / App Store |

### ⭐ Güncel öncelik: ÖNCE SONSUZ MOD (kullanıcı kararı, 2026-09-25)

Oyunun asıl oynanmaya değer kısmı ve temel mekaniği Sonsuz Mod. Bölümler sonra, büyük ölçüde görsel (karo seti) ve ayar değişikliğiyle gelecek. Bu yüzden uygulama sırası:

| Sıra | Faz | Not |
|---|---|---|
| 1 | **0** Karar, hazırlık, otonom test altyapısı | Git, Unity MCP / PlayMode testleri, ekran görüntüsü ve bot aracı, stil onayı |
| 2 | **1** Stabilizasyon | Retry, item konumu, duvarlar |
| 3 | **1.5** Ekran kompozisyonu | HUD bandı + kare arena (ekran enine ölçekli) + koridor/kontrol alanı, karo tabanlı çerçeve → [prompt](prompts/faz-1.5-tam-ekran-arena.md) |
| 4 | **3** Görsel temel | Yeni arena üzerinde piksel standardı, ışık, juice |
| 5 | **2 (sadece Sonsuz'un ihtiyacı olanlar)** | 2.1 Events, 2.2 tek animatör, 2.4 mermi hareket/etki + yükseklik, 2.5 durum efektleri, 2.8 SaveSystem. GameMode/WorldData/Enemy desenleri ertelenir ama kapı açık bırakılır. |
| 6 | **5** Sonsuz mod cilası | Taş çeşitleri, göktaşı, olaylar, near-miss, combo, **dash** |
| 7 | **4 (Sonsuz odaklı)** | Ana menü, oyun sonu, ayarlar, karakter seçimi. Bölüm haritası sonra. |
| 8 | **10** Yayın (v1.0 = sadece Sonsuz Mod) | Önce Sonsuz ile mağazaya çık, geri bildirim topla |
| 9+ | 2'nin kalanı → 6, 7, 8, 9 | Dünyalar güncellemelerle gelir |

**Not (bölümler "sadece reskin" mi?):** Buz ve Karanlık büyük ölçüde evet: karo seti + zemin ayarı + ışık ayarı + düşman verisi. Futbol ek olarak top, şut ve kale mekaniği ister (birkaç küçük sistem). Kart etkisi ise durum efektleriyle hallolur. Bu yüzden Sonsuz'u yaparken mermi etkisi, durum efekti ve spawn noktası sistemlerini genel kurmak, sonradan yeniden yazmayı önler.

**Sıralama mantığı (orijinal plan):** Önce temel sağlamlaşır (1). Sonra mimari, yeni modları kaldıracak hale gelir (2). Görsel standart erken belirlenir ki sonraki bütün içerik ona göre üretilsin (3). Yeni dünyalar ancak bunlardan sonra gelir. Faz 3 ve 4 bazen yer değiştirebilir. Ama Faz 1 ve 2 atlanmamalı, atlanırsa her yeni mod yamalı kodla yapılır.

## 5. Değişmez kurallar (tüm fazlar)

- Görsel anayasa: [sanat-rehberi.md](sanat-rehberi.md) (renk uyumu bir numaralı kriter). Performans bütçesi: [prompts/00-ana-prompt.md](prompts/00-ana-prompt.md).

- Her faz ayrı git dalında yapılır (`faz-1-stabilizasyon` gibi) ve kullanıcı Unity'de test edip onaylayınca birleştirilir.
- Her fazın sonunda **Sonsuz Mod regresyon testi** yapılır: menü → oyna → öl → tekrar dene → menü → oyna.
- Yeni içerik veri ve prefab ile eklenir. Yeni mod için gereken kod genel olmalı ("FootballMode" dışında futbola özel kod olmamalı).
- Mobil öncelikli: tek parmak joystick + en fazla bir aksiyon butonu. 60 FPS hedefi.
