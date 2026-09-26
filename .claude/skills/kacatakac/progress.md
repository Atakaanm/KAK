# KaçAtaKaç — İlerleme ve Durum

> Her iş sonunda güncellenir. En son güncelleme: **2026-09-26**

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

**Faz 3c — bağlılık katmanı (odak Sonsuz Mod):** altın, adım adım açılan özellikler, istatistikli kilitli karakterler, görevler, pet'ler, günlük ödül. Araştırma, tasarım ve uygulama sırası: `3c.md`. (Denetim D0-D6 tamamlandı: `denetim.md`.)
Faz 2B altyapısı yarım (kod var, sahne/UI yok, commit 6f1618f), 3c'den sonra bitirilecek.

### Denetim sonucu (sürüyor)
- [x] **D0 Hijyen:** Plastic/Visual Scripting/AI Navigation/Multiplayer Center paketleri kaldırıldı; `.plastic/` ve `.slnx` git dışı; Faz 10 oyuncu ayarları commit edildi. **Editör kilidi çözüldü:** `SaveOpenScenes` adsız sahnede "Farklı Kaydet" penceresi açıyordu (köprü + tüm Kak*Setup araçları → `SaveNamedScenes`); PlayMode testlerinden sonra editör geçici InitTestScene'de kalıyordu → köprü sahneyi geri açıyor.
- [x] **D1 Adil çarpışma:** `PlayerHitbox` (ayak izi r=0,2 duvarlar için + gövde kapsülü 0,36×0,80 taşlar için; eski tek daire r=0,40 gövdenin 2 katıydı), `ShieldBubble` (ölçek/renk yerine piksel balon), katı köşe kaideleri, bot gerçek ölçüleri okuyor. Testler 40/40 + 4/4.
- [x] **D2 Temizlik:** 10 eski editör aracı + 3 InitializeOnLoad üretici silindi; ölü kod (FloatingText, CameraFitWidth, WaveManager/WaveData, TutorialInfo, Readme) ve 6 kullanılmayan UI sprite'ı; LevelData/MainMenuController/ArenaAutoLayout ölü alanları; **SampleScene → Game**, SampleSceneProfile → GameVolumeProfile (GUID korunarak). WorldPopup artık Nunito (önce LiberationSans), `Show(scale)` hatası, afişte gömülü materyal kopyası. Testler 40/40 + 4/4.
- [x] **D3 Performans (ölçerek):** SRP Batcher kapalı + dinamik batching → draw 75 → 43 (tepe 194 → 96), CPU aynı. Mobile URP render ölçeği 0,8 → 1 (piksel sanatı bulanıktı), HDR/gölge kapalı, Bloom çeyrek + 4 geçiş. Fontlar statik atlas (açılışta glif üretimi yok, git gürültüsü bitti), TMP varsayılan Nunito. Powerup havuzu, küçük tahsisler. Bench'e kırılım + `-kakuncapped`. Testler 41/41 + 6/6. Ayrıntı: `denetim.md` D3 tablosu.
- [x] **D4 Oyuncu bilgisi:** aktif güçlendirme göstergesi (`PowerupHud`: arenanın altında ikon + süre halkası), ilk oyun ipuçları (`OnboardingHints`: hareket → dash → yakın geçiş, bir kez, deneyimli kayıtta yok; `SaveData.seen` = 3c adım adım açılmanın temeli). `PowerupPickup.Apply` statik, Dev menü "Tüm Powerup'ları Ver". Test kaydı artık her testte temiz. Testler 46/46 + 6/6.
- [x] **D5 Doğrulama:** `SahneDenetimTests` (EditMode, izin listeli), LevelManager tema referansları bağlandı, denge ölçümü (bot kaidelere takılıyordu → düzeltildi; usta 65-143 sn, acemi 80-93 sn), 5 oranlı ekran seti. Testler 46/46 + 8/8.
- [x] **D6 Belgeler:** SKILL.md mimari haritası ve teknik ortam güncel koda göre yeniden yazıldı, README yenilendi, git izin kuralı güncellendi.
- **Denetim tamamlandı.** Sıradaki: **Faz 3c** (`3c.md`: araştırma + tasarım + uygulama sırası).

**Önceki: Faz 2B — Bölüm dünyaları için mimari** (yarım). Sonsuz Mod v1.0 hazır: 0 ✅ 1 ✅ 1.5 ✅ 3 ✅ 2A ✅ 5 ✅ 4 ✅ 10 ✅* (2026-09-25).
Sonra: 6 (Buz) → 7 (Futbol) → 8 (Karanlık) → 9 (Meta).
*Faz 10'un mobil build, cihaz testi ve mağaza hesabı adımları kullanıcıya bağlı → `Docs/Yayin-Kontrol-Listesi.md`.

### Faz 10 sonucu (testler: 36/36 PlayMode + 4/4 EditMode)
- [x] Oyuncu ayarları (`KakBuild.ConfigurePlayer`): "Kaç Ata Kaç", `com.atakaan.kacatakac`, 0.9.0, dikey kilit, Android IL2CPP+ARM64 API25+, iOS 14+, ikon, açılış ekranı
- [x] macOS development build + `KakAutoBench` (build kendi kendine oynar, rapor yazar): **~60 FPS, GC ~30 B/kare, en kötü kare 33 ms, 82 batch** (Apple M4)
- [x] `FontWarmup`: TMP dinamik atlas takılması (552 ms) giderildi
- [x] TR/EN yerelleştirme (`Loc`, `LocText`, `LanguageButton`), cihaz diline göre varsayılan, `LocTests`
- [x] Ses: prosedürel 8-bit efektler (12) + menü/oyun müziği (`tools/kak_gen_audio.py`), `AudioManager` olayları dinliyor, ton değişimi, tekrar sınırı, her sahnede prefab'dan oluşuyor; `AudioTests`
- [x] Mağaza metinleri TR/EN, gizlilik politikası taslağı, yayın kontrol listesi, mağaza görüntüleri (`Docs/`)
- [x] Kayıt: şirket adı değişince eski konumdan taşıma; köprü Play oturumları gerçek kayda yazmıyor
- 👤 Android/iOS build modülleri kurulu değil (sadece WebGL + Mac). Kurulum ve keystore kullanıcıda.

### Faz 4 sonucu (testler: 34/34 + denge ayrı)
- [x] UI görsel seti (`tools/kak_gen_ui.py`): piksel 9-slice altın/taş buton, panel, rozet, anahtar, ikonlar (palet renkleri); `_9s` import kuralı
- [x] `KakUiKit` + `KakUiSetup` (Arayüzü Kur): tüm UI kod ile tekrarlanabilir üretiliyor
- [x] Fontlar: Nunito ExtraBold (UI, Türkçe tam) + Cinzel Decorative (sadece logo), konturlu materyaller. Piksel font indirilmedi (dış kaynak gerekmesin diye)
- [x] Ana menü: canlı arena arka planı (`MenuBackdrop` + DungeonFrame prefab + süzülen taşlar), logo, rekor ve istatistik, OYNA / KARAKTER / AYARLAR / BÖLÜMLER (yakında), Ayarlar paneli (müzik, efekt, titreşim, sarsıntı, iki dokunuşla sıfırlama), Karakter paneli (tek karakter + 2 kilitli)
- [x] Oyun içi: taş levha pause butonu, pause paneli (ayarlar + Devam/Yeniden Başla/Ana Menü), oyun sonu ekranı (`GameOverScreen`: skor sayma, YENİ REKOR rozeti, süre/yakın geçiş/combo)
- [x] Sahne geçişi: `SceneFader` (kararma, sıraya alma), `SafeAreaFitter`
- [x] Testlerin gerçek kayda yazması düzeltildi (`KakTestUtil` geçici kayıt). Kullanıcının kaydından test istatistikleri temizlendi, rekor 4775 korundu.
- ↪ Karakter seçimi gerçek değil: ikinci karakterin görselleri yok (Faz 9)

### Faz 5 sonucu (testler: 31/31 + denge ölçümü ayrı)
- [x] Fırlatıcı uyarısı (telegraph): atıştan 0,35 sn önce sıcak renge döner
- [x] 7 taş türü kademelere dağıtıldı (Faz 2A) + çakıl okunurluk ayarı (0,75 ölçek, 2,1 hız)
- [x] Olaylar (`EndlessEventManager`, ilk 30. sn, sonra 30-45 sn arayla): Taş Yağmuru, Çapraz Ateş, Sessizlik, Yuvarlanan Kaya (şerit uyarısı). Olay bitince fırlatıcı aktifliği geri yüklenir.
- [x] Yakın geçiş (`NearMissTracker`): +5 × combo (dash'le ×2, "SÜPER KAÇIŞ!"), combo süresine +2 sn
- [x] Combo: hasarsız her 10 sn +0,1 (en fazla ×2,0), hasarda sıfırlanır; skor hızına çarpan; HUD'da "x1.3"
- [x] Dash (`PlayerDash` + `DashButton`, Space): 1,7 birim / 0,14 sn, ölümsüz, 2,6 sn bekleme, arkada soluklaşan kopyalar
- [x] HUD: combo metni, olay/kademe afişi (`HudExtras`), dünya yazıları havuzu (`WorldPopup`, eski TextMesh kaldırıldı)
- [x] Denge v2: kademe eşikleri süreye yayıldı (≈20/45/80/130/200 sn) ve çarpanlar yumuşatıldı (tablo aşağıda)
- [x] Denge ölçüm testi (`python3 tools/kak_bridge.py denge`, 2x hız, vuran taş türü analizi), havuz ön ısıtma 40
- ⚠️ Bot ölçümü: acemi 37-86 sn, usta 54-107 sn (hedef usta 180-300 sn). Bot tepkisel ve kısa ufuklu; usta bir insanı temsil etmiyor. **Gerçek denge kullanıcının telefonda oynamasıyla netleşecek.**

### Faz 2A sonucu (testler: 26/26 PlayMode + 3/3 EditMode)
- [x] 2.1 GameEvents (Faz 3'te) + MeteorLanded
- [x] 2.2 (hafif) `DirectionUtil`: iki animatördeki kopya açı kodu birleşti. Tam `DirectionalSpriteAnimator` → 2B
- [x] 2.4 Mermi hareketleri: Straight, Bounce, Homing, Split, Meteor (2.5D yükseklik + gölge uyarısı + alan hasarı); `Projectile.Launch/LaunchMeteor` API; `ProjectilePool.GetPrefabOf`
- [x] Taş türleri (`KakContentSetup`, `Assets/Data/Projectiles/`): Taş, Çakıl, Kaya, Seken, Parçalanan, Göktaşı, Güdümlü + kademe dağılımı (ağırlıklı)
- [x] 2.5 (hafif) `PlayerHealth.SetInvulnerable` (dash için). Tam durum efekti sistemi (Slow, Stun, kart) → 2B (bölüm dünyaları)
- [x] 2.8 `SaveSystem` (JSON, atomik yazma, PlayerPrefs'ten taşıma): rekor, en iyi süre, oyun sayısı, toplam süre, powerup sayısı, jeton, karakterler, ayarlar
- Testler: `ProjectileMotionTests`, `SaveSystemTests` (EditMode)

### Faz 3 sonucu (testler: 22/22 yeşil)
- [x] Palet: Endesga 32 (varsayım, kullanıcı cevap vermedi), `KakPalette`, `tools/kak_palette.py`
- [x] Sanat geçişi (`tools/kak_art_pass.py`): karakter ve fırlatıcı kareleri palete + 1 px kontur; yeni sıcak taş sprite'ı (26 px, kontur, sıcak rampa)
- [x] Tek piksel yoğunluğu: 0,024 birim/sanat pikseli (fırlatıcılar 4x → 2,4x, taş 1,0 → 0,67 birim)
- [x] Taş prefab'ı: dönen Visual + dönmeyen gölge, hitbox 0,26
- [x] His paketi: `GameEvents`, `FeedbackManager` (sarsıntı, hit-stop, kırıntı/halka parçacıkları), `KakCameraShake`, `PlayerJuice` (zıplama, ezilme, toz), ölüm yavaş çekimi (0,7 sn)
- [x] `KakTime` katmanları: temel ölçek × pause × hit-stop (SloMo/pause/hit-stop birbirini bozmaz, testli)
- [x] Renk tonlaması (Volume), `KakArtImportRules` PPU standardı, Sprite Atlas (KAK_PixelArt, KAK_Soft), URP dinamik batching açık
- [x] `KakPerfProbe` + `KakDevSetup.Stats` (Game view render istatistiği)
- ⚠️ Performans: Game view'da **138 batch / 64 SetPass**, 1,2k üçgen (hedef < 60 batch). Atlas ve dinamik batching sayıyı düşürmedi (Universal Renderer + SRP Batcher'da sprite birleşmesi zayıf). → Faz 10: cihazda Frame Debugger + 2D Renderer'a geçiş değerlendirmesi. Editör GC ölçümü (~100 KB/kare) editör yükünü içeriyor, anlamlı değil. Cihazda ölçülecek.
- ↪ Floating text (TMP + havuz) → Faz 4. 2D Renderer + Light2D → Faz 8 (Karanlık) veya Faz 10 performans kararıyla birlikte.

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
**Faz 3c.1 Altın** (dal `faz-3c-altin`): havuzlu altın nesnesi (palet içi piksel sprite + parıltı), arenada doğma kuralı (6-10 sn'de bir 1-3'lü küme, oyuncudan uzak ama ulaşılabilir, kademeyle artar), toplama (gövde ya da ayak izi değince; ardışık toplamada ses tonu yükselir), HUD sayacı (skorun altında), oyun sonu altın satırı + sayma animasyonu, `SaveData.coins` + `totalCoins`. Testler + ekran görüntüsü. Ayrıntı: `3c.md`.

### Onay bekleyenler
- **Android Build Support modülü** kurulmalı (Unity Hub → 6000.3.8f1 → Add modules). Sonra "Android build al" → cihazda test.
- **Sesler** prosedürel üretildi, Claude dinleyemedi. Kulağa hoş gelmeyen varsa söyle (`tools/kak_gen_audio.py` ile yeniden üretilir).
- **Denge hissi:** telefonda birkaç oyun oyna → "çok zor / çok kolay / tam" + hangi taş/olay haksız hissettirdi. Değerler `KakEndlessSetup.SetStage` ve `KakContentSetup` içinde tek yerde.
- Palet: Endesga 32 varsayımla uygulandı (kullanıcı değiştirmek isterse: `KakPalette` + `tools/kak_palette.py` + `kak_art_pass.py`).
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
- ✅ (D0) Plastic (Unity Version Control) paketi kaldırıldı (proje git kullanıyor; `.plastic/` diskte duruyor, git dışı).

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

| Kademe | minSkor (~sn) | Spawner | Aralık× | Hız× | Boyut× | Oyuncu× | Skor× | Taş türleri |
|---|---|---|---|---|---|---|---|---|
| Başlangıç | 0 | 2 | 1.00 | 1.00 | 1.00 | 1.00 | 1.0 | Taş |
| Kolay | 200 (~20) | 2 | 0.90 | 1.10 | 1.00 | 1.05 | 1.1 | + Çakıl, Kaya |
| Orta | 475 (~45) | 3 | 0.82 | 1.20 | 1.08 | 1.08 | 1.2 | + Seken |
| Zor | 900 (~80) | 3 | 0.72 | 1.30 | 1.15 | 1.12 | 1.3 | + Parçalanan, Göktaşı |
| Cehennem | 1550 (~130) | 4 | 0.62 | 1.42 | 1.22 | 1.16 | 1.5 | + Güdümlü |
| İmkansız | 2600 (~200) | 4 | 0.52 | 1.55 | 1.30 | 1.20 | 1.8 | hepsi, özel ağırlıklı |

Olaylar: Taş Yağmuru ve Çapraz Ateş kademe 2'den, Sessizlik 3'ten, Yuvarlanan Kaya 4'ten itibaren.