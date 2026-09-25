# KaçAtaKaç — İlerleme ve Durum

> Her iş sonunda güncellenir. En son güncelleme: **2026-09-25**

## Son durum (özet)

Oynanabilir bir **Endless** çekirdek hazır: hareket, 8 yönlü animasyon, 4 köşe fırlatıcı, mermi havuzu, can/kalp arayüzü, 6 kademeli zorluk, 5 güçlendirme, skor ve en iyi skor, duraklatma, ana menü, ses ayarları, sanal joystick.
Son commit'ler (Temmuz 2026): HD ana menü, altın buton, premium fontlar (Cinzel Decorative, Nunito), joystick.
**Commit edilmemiş iş:** `Assets/Scenes/SampleScene.unity` üzerinde büyük bir değişiklik var (~1000 satır). Kullanıcının Editor'de süren işi, dokunmadan önce sor.

## Tamamlananlar

- [x] ScriptableObject mimarisi (Player/Arena/Spawner/Projectile/Level/DifficultyStage/Powerup)
- [x] 8 yönlü oyuncu hareketi + animasyonu, 8 yönlü spawner idle/saldırı animasyonu
- [x] ProjectilePool (nesne havuzu), arena sınırı dışında havuza iade
- [x] Can sistemi, vuruş flaşı, ölümsüzlük süresi, kalp kaybetme/kazanma animasyonları
- [x] Endless zorluk: Başlangıç(0) → Kolay(50) → Orta(150) → Zor(350) → Cehennem(600) → İmkansız(1000)
- [x] Güçlendirmeler: Heart, Shield, Speed, SloMo, Ghost (ağırlıklı rastgele çıkma)
- [x] Skor + BestScore (PlayerPrefs), Game Over paneli
- [x] Duraklatma (uygulama arka plana gidince otomatik)
- [x] Ana menü sahnesi + AudioManager + ayarlar paneli (kod)
- [x] Mobil sanal joystick
- [x] 2.5D Y derinlik sıralaması (YDepthSorter)
- [x] Ekrana göre kamera ve arena yerleşimi (CameraFitWidth, ArenaAutoLayout)

## Yarım kalanlar (kodu var, oyuna bağlı değil)

- [ ] **Stage (dalga) modu:** `WaveManager` + `WaveData` hazır, ama hiçbir LevelData Stage tipinde değil. `HandleAllWavesComplete` → `LevelComplete()` çağrısı yorumda.
- [ ] **Karakter seçimi:** `Girl_PlayerData` var, `MainMenuController.charactersPanel` var; menü sahnesinde sadece **PlayButton** bulunuyor (Characters/Settings/Leaderboard butonları sahnede yok).
- [ ] **Yetenekler (AbilityData: Dash/Shield/Shockwave):** veri sınıfı var, hiçbir kod kullanmıyor.
- [ ] **Ekipman (EquipmentData: Head/Body/Feet):** veri sınıfı var, kullanılmıyor.
- [ ] **LevelData alanları kullanılmıyor:** `targetScore`, `timeLimit`, `hasKey`, `hasCoins`, `hasBoss`, `isLocked/unlockPrice`.
- [ ] **Kullanılmayan diğerleri:** `PlayerData.powerupSpawnRateMultiplier`, `DifficultyStageData.availableProjectiles` (kademeye göre farklı mermi), `ScoreManager.onMilestoneReached`, `DifficultyManager.onStageChanged` (UI'a bağlı değil).
- [ ] Ses klipleri AudioManager'a atanmış mı? Kontrol edilmedi.

## Yol haritası (README + kod ipuçları)

1. Kritik hataları düzelt (aşağıda, özellikle Retry)
2. Karakter seçim ekranı (Boy/Girl)
3. Mağaza / kilit açma sistemi (para birimi: `hasCoins` alanı ipucu)
4. Yeni arena temaları (zemin sürtünmesi hazır: buz, bataklık)
5. Yeni mermi tipleri (kademeye özel mermiler)
6. Yetenek butonu (Dash vb.), ekipman
7. Stage modu / boss
8. Mobil build ayarları (Android/iOS), Google Play / App Store yayını

## Aktif faz

**Faz 3 — Görsel Temel** (başlıyor; ilk adım palet seçimi → kullanıcı onayı). Faz 0 ✅, 1 ✅, 1.5 ✅ (2026-09-25).
**Öncelik: Sonsuz Mod.** Sıra: 0 ✅ → 1 ✅ → 1.5 ✅ → 3 → 2A → 5 → 4 → 10.

### Faz 1.5 sonucu (testler: 20/20 yeşil)
- [x] ScreenComposer (kamera + HUD bandı + kontrol alanı, Safe Area, tablet modu)
- [x] DungeonFrame (skybox yerine karanlık dünya, koridor, meşaleler, vinyet), geçici karolar
- [x] Kayan joystick (kontrol alanında, arenaya binmez), soluk taş temalı görünüm
- [x] HUD bandı yerleşimi, CanvasScaler genişliğe göre
- [x] Arena filigranı temizlendi, Art import kuralları
- [x] ScreenLayoutTests
- ↪ Aksiyon butonu yuvası → Faz 5 (dash ile birlikte)
- ↪ Pause butonu ve skor fontu stili → Faz 4 (UI), geçici olarak eski halinde
- Renk testi (9:19.5): koyu %51 / orta %47 / açık %2 (önce: %19 / %78 / %4)

### Faz 1 sonucu (testler: 11/11 yeşil)
- [x] 1.1 Yaşam döngüsü: GameManager sahneye özel, singleton temizliği, yöneticiler sahnede kalıcı (`KakSceneSetup`), `defaultLevel`, deterministik zorluk/spawner sırası, `KakTime`, pause/SloMo hataları, ölümde görünürlük, sesler, `Wall` etiketi, GC (SetText, KakLog), targetFrameRate
- [x] 1.2 Oynanabilir alan: `ArenaAutoLayout.PlayableWorldRect` tek kaynak; duvarlar, fırlatıcı duruş noktaları, mermi sınırı, powerup alanı, bot. Powerup ikonları saydam ve 256 px.
- [x] 1.5 Testler: `ArenaTests` (powerup konumu, duvar, mermi sınırı), `BotTests`
- ↪ 1.3 Joystick BG düzeltmesi → Faz 1.5 (joystick yeniden yazılıyor)
- ↪ 1.3 Fizik katmanları ve collision matrix → Faz 3 performans turu
- ↪ 1.4 Diagnose Scene genişletme (filtre modu, eksik referans) → Faz 3 (içe aktarma standardıyla birlikte)
- Varsayım: Kalkan "vurulana kadar" sürer (`ShieldData.duration` kullanılmıyor, davranış korunuyor)

### Sıradaki adım
Faz 3.1: 3 palet adayı hazırlandı (bkz. Onay bekleyenler) → kullanıcı seçince `KakPalette` + palet denetçisi. Seçim beklenirken palete bağlı olmayan işler: 3.4 URP 2D Renderer + ışık, 3.5 juice paketi (sarsıntı, hit-stop, parçacık), 3.2 import standardı ve ölçek temizliği.

### Onay bekleyenler
- **Faz 3 palet seçimi**: A) arenadan türetilmiş özel palet, B) Endesga 32, C) Sweetie 16 (önizleme: `.claude-bridge/screenshots/palet_adaylari.png`)
- Sanat stili: piksel sanatı (32 px, PPU 32). Varsayım: evet, devam.
- Hedef platform ve test cihazı (Android/iOS?). Faz 10'a kadar engel değil.
- Kullanıcı tüm izinleri verdi (2026-09-25): faz dallarına push, PR birleştirme, onaysız plan başlatma, test için Unity'yi kullanma.

## Bilinen hatalar ve riskler

- ✅ **Item'lar ekranın yanlış yerinde çıkıyor** — Faz 1: asıl sorun powerup ikonlarının opak siyah kare zemini (5 ikonun hepsi RGB) + Retry sonrası bozuk yönetici yapısı. Konumlar artık `PlayableWorldRect` içinde (42/42 test). (kullanıcı bildirdi, 2026-09-25). Neden henüz bulunamadı. Hesaplanan spawn alanı kağıt üstünde arenanın içinde görünüyor, Play modunda teşhis gerekli (Faz 1.2).
- ✅ (Faz 1) **Fizik duvarları görselle hizasız.** Sprite dış kenarından 0.15 birim içerideler, görseldeki iç duvar yaklaşık 0.6 birim içeride. Karakter ve mermiler duvar çiziminin üstüne girebiliyor.
- 🟠 **Görsel kalite:** karışık piksel yoğunluğu (karakter 48px/ölçek 0.8, arena 2048px/ölçek 0.4, spawner ölçek 4), sprite'lar Bilinear (bulanık), arena görselinin sağ alt köşesinde yapay zeka filigranı (✦), URP Universal Renderer (2D Light çalışmaz). Faz 3'te çözülecek.
- 🟠 Menü sahnesinde sadece Oyna butonu var. Faz 4.
- 🟠 **Arena ekranın sadece ~%46'sını kaplıyor** (kare arena, 9:19.5 telefon). Üstte ve altta ~%54 ölü alan var. Kullanıcı bunu en büyük görsel sorun olarak görüyor. Çözüm Faz 1.5 (onaylı kompozisyon): üstte HUD bandı (duvar yüzü), ortada kare arena (ekran enine ölçekli), altta koridor ve kontrol alanı (sol joystick, sağ aksiyon).

> Durum: 🔴 kritik · 🟠 orta · 🟡 düşük · ✅ düzeltildi. "Doğrulanmadı" = kod okunarak bulundu, Play modunda test edilmedi.

- ✅ (Faz 1) **Retry / menüye dönüp tekrar oynama bozuk (✔ testle doğrulandı: `Retry_IkinciOyunTamamenCalisir`, `GameOver_Menu_TekrarOyna_Calisir`).** `GameManager` `DontDestroyOnLoad` kullanıyor ve `ScoreManager` aynı objede. Sahne yeniden yüklenince eski GameManager `isGameOver = true` ile yaşamaya devam ediyor, yenisi yok ediliyor. Sonuçlar: skor artmaz, ikinci ölümde Game Over açılmaz, powerup çıkmaz, `gameOverPanel/scoreText` referansları ölü. Üstelik `LevelManager/DifficultyManager` sahnede hazır değil, onları GameManager.Awake yaratıyordu, yani LevelData hiç uygulanmaz. **Öneri:** GameManager'dan `DontDestroyOnLoad`'ı kaldır (sahneye özel olsun), `LevelManager` ve `DifficultyManager`'ı sahneye kalıcı obje olarak ekle.
- ✅ (Faz 1) **SampleScene doğrudan Play'e basılınca LevelData yok** (✔ testle doğrulandı). Runtime'da yaratılan LevelManager'ın `defaultLevel`'ı boş, bu yüzden "HİÇBİR LEVEL DATA" hatası verir. Sadece menüden girince çalışır. **Öneri:** LevelManager sahneye eklenip `defaultLevel = Endless_Level1_LevelData` atanmalı.
- 🟠 **Joystick scripti `JoystickHandle` üzerinde** (dokümana göre `JoystickBG`'de olmalı). Dokunma alanı sadece küçük topla sınırlı olabilir. Doğrulanmadı.
- ✅ (Faz 1) `SceneLoader` `timeScale`'i sıfırlıyor ama `fixedDeltaTime`'ı sıfırlamıyor. SloMo sırasında ölünürse sonraki oyunda fizik adımı 0.008 kalır. `PauseManager` ise `fixedDeltaTime = 0` yapıyor.
- ✅ (Faz 1) Ghost ve Speed süreleri `WaitForSeconds` kullanıyor, bu yüzden SloMo sırasında uzuyorlar.
- ✅ (Faz 1) `DifficultyManager` aktif spawner'ları dizideki sıraya göre seçiyor, bu sıra da `FindObjectsByType(None)` ile geliyor ve garanti değil. Hangi köşelerin aktif olacağı öngörülemez.
- 🟡 `ShieldData.duration = 10` hiçbir işe yaramıyor, kalkan vurulana kadar sürüyor.
- ✅ (Faz 1) README güncel değil ("Unity 2022+", `CornerShoother`, eksik dosyalar).
- ℹ️ Input System paketi kurulu ama kod eski Input Manager kullanıyor. Şu an sorun yok (Active Input Handling = Both olmalı).
- ℹ️ Plastic (Unity Version Control) kimlik doğrulama hatası loglarda görünüyor. Oyunu etkilemiyor.

- ✅ (Faz 1) **`Wall` etiketi projede tanımlı değil** (✔ test buldu): `PowerupSpawner.SpawnRandomPowerup` içindeki `CompareTag("Wall")` her çağrıldığında hata logluyor.
- ℹ️ ~~HUD oranlara göre bozuk~~ — yanlış alarm: ekran görüntüsü aracının zamanlama hatasıydı (düzeltildi). HUD yine de Faz 1.5'te yeniden kurulacak.
- 🟠 **Karakter çok küçük:** arena genişliğinin yaklaşık 1/20'si, taşlardan küçük (ekran görüntüsü). Faz 3'te ölçek standardı.
- 🟠 **Taşlar zeminle karışıyor** (renk testi): gri tonlama ve bulanık görünümde neredeyse kayboluyor. En belirgin öğe sarı joystick. Faz 3'te renk rolleri.
- ✅ (Faz 1) Ölüm anında invincibility coroutine'i timeScale=0'da donarsa oyuncu sprite'ı gizli kalabilir (ekran görüntüsünde Game Over'da oyuncu görünmedi, doğrulanacak).

- 🟠 **Arka plan Unity varsayılan skybox'ı** (üstte mavi, altta gri). Hem çirkin hem gereksiz render maliyeti. Faz 1.5.
- 🟡 Sahne ölçekleri tutarsız (TopRightSpawner ölçek 1 + Visual 4, diğerleri ölçek 4 + Visual 1; Player 0.8 + Visual 3). Faz 3 ölçek temizliği.
- 🟡 Powerup geri bildirim yazısı eski `TextMesh` + her seferinde `new GameObject`. Faz 3 (TMP + havuz).
- 🟡 SloMo sırasında ölünce oyuncu kırmızı tonda kalıyor (ölüm göstergesi olarak bırakıldı, Faz 3 ölüm efektiyle değişecek).

## Denge değerleri (referans)

| Parametre | Değer |
|---|---|
| Oyuncu hızı / can / ölümsüzlük | 5 / 3 / 0.35 sn |
| Taş hızı / hasar / ömür | 1.5 / 1 / 5 sn |
| Fırlatıcı ateş aralığı | 1.5 sn |
| Skor | 10 / sn × kademe çarpanı |
| Powerup çıkma aralığı / yerde kalma | 5–10 sn / 8 sn |
| Powerup ağırlıkları | Heart 1.0, Speed 0.8, Shield 0.6, SloMo 0.5, Ghost 0.4 |

| Kademe | minSkor | Spawner | Ateş× | MermiHız× | Boyut× | OyuncuHız× | Skor× |
|---|---|---|---|---|---|---|---|
| Başlangıç | 0 | 2 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 |
| Kolay | 50 | 2 | 0.85 | 1.15 | 1.0 | 1.05 | 1.1 |
| Orta | 150 | 3 | 0.7 | 1.3 | 1.15 | 1.1 | 1.2 |
| Zor | 350 | 3 | 0.55 | 1.5 | 1.3 | 1.15 | 1.3 |
| Cehennem | 600 | 4 | 0.4 | 1.75 | 1.45 | 1.2 | 1.5 |
| İmkansız | 1000 | 4 | 0.3 | 2.0 | 1.6 | 1.25 | 1.8 |
