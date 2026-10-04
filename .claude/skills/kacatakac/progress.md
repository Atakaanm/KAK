# KaçAtaKaç — İlerleme ve Durum

> Her iş sonunda güncellenir. En son güncelleme: **2026-10-04**

## Son durum (özet)

**Sonsuz Mod, telefonda oynanan sürüm (Faz 11 sonrası):** Zindan + Buz Gölü dünyaları, 7 taş türü + 5 kartopu türü, süreye bağlı 6 kademe, olaylar, yakın geçiş skor çarpanı, tek canla başlayıp altınla güçlenme (5 karakter, karakter detay ekranı, pet seviyeleri), görevler, günlük ödül, adım adım açılma, 2 kişilik yerel mod, hikâye girişi, TR/EN, prosedürel sesler. Reklam altyapısı var ama kapalı.
Build: iPhone 15 Pro Max'e kurulu (ücretsiz Apple hesabı → 7 günde bir yeniden kurulum), iOS simülatör ve Android test APK doğrulandı. Testler: PlayMode 95/95 (+1 atlanan denge testi), EditMode 27/27.
Bekleyen: kullanıcının telefonda oynayıp geri bildirim vermesi (tek can zorluğu, yükseltme hızı, buz kayması).

## Tamamlananlar

- [x] ScriptableObject mimarisi (Player/Arena/Spawner/Projectile/Level/DifficultyStage/Powerup)
- [x] 8 yönlü oyuncu hareketi + animasyonu, 8 yönlü spawner idle/saldırı animasyonu
- [x] ProjectilePool (nesne havuzu), arena sınırı dışında havuza iade
- [x] Can sistemi, vuruş flaşı, ölümsüzlük süresi, kalp kaybetme/kazanma animasyonları
- [x] Endless zorluk süreye bağlı (G2): Başlangıç 0 → Kolay 30 → Orta 75 → Zor 150 → Cehennem 240 → İmkansız 360 sn
- [x] Güçlendirmeler: Heart, Shield, Speed, SloMo, Ghost + Pranga + buz eşyaları (ağırlıklı rastgele çıkma)
- [x] Skor + rekor (SaveSystem; dünya başına ve 2 kişilik ayrı), Game Over paneli
- [x] Duraklatma (uygulama arka plana gidince otomatik)
- [x] Ana menü sahnesi + AudioManager + ayarlar paneli (kod)
- [x] Mobil sanal joystick
- [x] 2.5D Y derinlik sıralaması (YDepthSorter)
- [x] Ekrana göre kamera ve arena yerleşimi (CameraFitWidth, ArenaAutoLayout)

## Yarım kalanlar (kodu var, oyuna bağlı değil)

- [ ] **Bölüm (Stage) modu (Faz 2B):** `LevelModeController`, `WorldData`/`WorldCatalog` (yıldızlı bölümler) kodda var; sahne/arayüz bağlı değil. (Eski `WaveManager` D2'de silindi.)
- [ ] **Yetenekler (`AbilityData`) ve ekipman (`EquipmentData`):** `PlayerData.activeAbility/equippedHead/Body/Feet` alanları var, kod kullanmıyor → kostüm sistemi ve karakter yetenekleri gelince.

## Yol haritası (2026-10-03)

1. Telefon geri bildirimiyle ince ayar (tek can, yükseltme fiyatları, buz hissi)
2. Mağazaya çıkış: hesaplar, keystore, reklamın açılması kararı (`Docs/Yayin-Kontrol-Listesi.md`, yayin.md)
3. Yeni dünyalar: Futbol, Boks ringi, Karanlık (Buz'daki `EndlessWorlds` + tema deseniyle)
4. Kostümler (karakter detay ekranındaki "yakında" yuvası) ve karakterlere özel yetenekler
5. Online battle royale (10 kişi; Unity Multiplayer Services araştırması)

## Aktif faz

**Faz 11 (telefon geri bildirimi) tamamlandı (2026-09-27, PR #26-#32):** ayrıntı aşağıda "Sıradaki adım". Aşağıdaki paragraflar tarihçe.

**Yayın hazırlığı tamamlandı (yayin.md, 2026-09-26):** Y1-Y3 (Play/App Store gereksinimleri, API 36, iPhone-only, gizlilik, reklam altyapısı kapalı), Y4 kız karakter ADA (40 kare, bedava, panel 3×2), Y5 mağaza görselleri + metinleri (`Docs/Magaza`). Kalanların hepsi kullanıcı adımı: `Docs/Yayin-Kontrol-Listesi.md` (modüller, Xcode 26, keystore, konsol hesapları, 12 test kullanıcısı, build). Onay bekleyenler: ADA adı geçici, destek e-postası, hedef kitle (13+ önerisi), reklamın açılıp açılmayacağı.
**Faz 3c tamamlandı (2026-09-26):** altın, adım adım açılma, karakterler, görevler, petler, günlük ödül, cila (`3c.md`). Denetim D0-D6 da tamam (`denetim.md`). Sıradaki büyük iş: kullanıcının cihaz testi + denge geri bildirimi, sonra Faz 2B'nin bitirilmesi (bölüm seçimi) → Buz / Futbol / Karanlık dünyaları.
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

### Faz 3c sonucu (sürüyor)
- [x] **3c.1 Altın:** arenada altın kümeleri, HUD sayacı, ses/parıltı, oyun sonu altın satırı ve cüzdan, `FeatureGate` (1. oyun saf kaçış). Rekoru ≥1000 olan oyuncu da "deneyimli" (temel ipuçları yok). Testler 50/50 + 8/8.
- [x] **3c.2 Özellik kapısı:** açılma takvimi (altın → görevler → karakterler → günlük ödül → pet'ler), menüde kilitli/YENİ! butonu (şimdilik Karakter), cüzdan, oyun sonunda "YENİ AÇILDI" afişi, oyun günü sayacı. Kullanıcının kaydı: 3 oyun → karakterler 2 oyun sonra açılır. Testler 53/53 + 12/12.
- [x] **3c.3 Karakterler:** 4 karakter (Ata / Çevik 300 / Tank 800 / Şanslı 1500), ödünleşimli istatistikler oyunda uygulanıyor, gerçek karakter paneli (satın al/seç), altın sayacı kalplerin altına taşındı. **Eski HUD hatası bulundu ve düzeltildi:** HealthUI yalnızca Heart1'i yönetiyordu; klonlar Heart2/3'ün üstüne biniyor, can kaybında alttaki kalpler kaybolmuyordu (öldükten sonra HUD'da 2 kalp görünüyordu). Testler 59/59 + 12/12.
- [x] **3c.4 Görevler:** 3 aktif görev (7 tür), oyun sonunda ilerleme/ödül, seviye arttıkça hedef ve ödül büyür; oyun sonu ekranında görev bloğu. Testler 61/61 + 15/15.
- [x] **3c.5 Pet'ler:** Ateşböceği (altın mıknatısı) ve Kaplumbağa (aralıklı kalkan), takip davranışı, menüde PET butonu + pet paneli. İlk çizimde petler çok küçüktü (8 px): tek piksel yoğunluğunu bozmadan büyük ızgarada yeniden çizildi. Testler 65/65 + 15/15.
- [x] **3c.6 Günlük ödül:** 7 günlük artan takvim, seri/kaçırma kuralı, menüde kendiliğinden açılan panel. Testler 67/67 + 18/18.
- [x] **3c.7 Cila:** yeni rekor konfetisi + fanfar, oyun sonunda sıradaki hedef çubuğu ("ÇEVİK: 180/300"), menüde "alınabilir" işareti. Testler 69/69 + 18/18.
- **Faz 3c tamamlandı.**

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
**Faz 15 — sonsuz kıvamı ve gelişim** (`prompts/faz-15-kivam-gelisim.md`, 2026-10-04, dal `faz-15-kivam-gelisim`). Kararlar: ortak gelişim + karakter pasifi, oyun içi kart seçimi Faz 15 sonunda, sandık = altın + oyunda düşen anahtar.
- [x] K1 tempo ve kaos eğrisi: `TempoProfile` (Data/Tempo/Endless_Tempo, `KakTempoSetup`) + `DifficultyManager` tempo modu (profil yoksa eski kademe). Sürekli τ; toplam atış hızı eğrisi fırlatıcılara paylaştırılır (yeni fırlatıcı ~10 sn'de payını alır, toplam sıçramaz); taş türü yeni kademede asla erken gelmez (`typeBlend`); olay sonrası nefes payı, hasar sonrası merhamet (tempo 8 sn durur), çırak (0-2. oyun rampa ×1,35/1,2/1,1, taş hızı ×0,85→1), güç → başlangıç τ 0,3 ama 20 sn'de yumuşak ısınma, rampa 360→300; çift atış (τ 0,45+), olay sıklığı, müzik perdesi +%4; kademe afişi yok. Testler `TempoTests` 7 + `TempoProfilTests` 4 (saniyede %8 sıçrama sınırı). Denge: ilk ölçümde seken taş erken geliyordu (düzeltildi) → yeniden ölçülüyor
- [ ] K2 cafcaflı piksel — **kullanıcı seçimi bekliyor** (öneri: Canlı). `KakVividSetup.SetLevel 0/1/2` (GameVolumeProfile, Color Curves Hue vs Sat: sıcak/altın, camgöbeği, pembe canlanır; yeşil zemin geri; 2'de bloom 0,6). Şu an 0 (şimdiki). Seçilince: SetLevel → mağaza görselleri (`kak_store_shots.py all`, yeni 8'li senaryo: oyun, kart, gelişim, sandık, kıl payı, kahraman, rekor, 2 kişi)
- [x] K3 özellik çekirdeği: `StatId` (19), `StatSheet`, `StatBuilder` (taban + pasif + ortak gelişim + eşya), oyuncuda `PlayerStats` (mıknatıs, kalkan yenilenmesi, ikinci şans), `Progression` (ortak 16 iz, `UpgradeCatalog` Resources, adım adım açılma, `SaveData.upgrades`), eski karakter yükseltmeleri altın iadesiyle (`legacyRefund`, menüde bir kez). `PlayerPower` = gelişim toplamı. Testler `GelisimTests` 6 + `GelisimOyunTests` 3
- [x] K4 GELİŞİM merkezi (`KakGelisimUi`): menüde KARAKTER → GELİŞİM; sekmeler KAHRAMAN · GELİŞİM · EŞYA (`MenuTabs`); gruplu kaydırılabilir liste (`ProgressPanel`, `UpgradeRow`): açık izler + sıradaki 2 kilitli iz; yükseltince ikon zıplar + piksel kıvılcım (`UiBurst`) + "+1" + kare dolar; yetmezse sallanır; yeni açılan iz "YENİ!" + `InfoToast`. Karakter detayında pasif satırı + GELİŞİM düğmesi (Hız/Güç satırları kalktı). 16+3 ikon `tools/kak_gen_upicons.py`
- [x] K5 eşya geliştirme: `ItemTrackData` (16 iz: Kalp/Kalkan/Yavaş çekim/Hayalet sıklık + yerde kalma (+ süre) + özel güç; Pranga direnci + seyrek pranga), `ItemProgress` (oyun başı anlık görüntü), üst seviyeler parşömen ister (`SaveData.scrolls`; K6'da sandıktan). Güçlü kalkan 2 vuruş (`PlayerHealth.ActivateShield(charges)`), derin yavaşlık, şifa ışığı, hayalet mıknatısı. Testler `EsyaGelistirmeTests` 4
- [x] K6 anahtar + sandık: `EconomyData` (Resources; `KakChestUi.SetupEconomy`), anahtar oyunda 60/120/180 sn (`KeyMilestones`) + rekor, oyun sonunda sağ üstte "+N anahtar" rozeti; `ChestSystem` (ahşap 1 / gümüş 3 / altın 10 anahtar, ödül altın + parşömen + bazen anahtar, her 10. açılış bir üst sandığın ödülü, altın rekorla ×1-2); menü GELİŞİM · SANDIK · 2 KİŞİ, SANDIK düğmesinde anahtar rozeti; `ChestPanel` + `ChestOpening` (sallanma → parlama + kıvılcım + dönen ışık → ödüller tek tek, dokun = atla). Görseller `tools/kak_gen_chests.py`. Testler `SandikTests` 4 + `AnahtarTests` 2
- [x] K7 el noktaları: `tools/kak_hand_anchors.py` (duruş karesinde ten kümelerinden sağ el, baş dışarıda; gizli el gövdeye göre aynalanır; koşu karelerinde tutan kol sallanmaz → duruş noktası + baş salınımı; batı kareleri aynalanır; elle düzeltme `tools/hand_anchors_fix.json`) → `KakHandAnchorSetup` → `HandAnchorSet` (5 karakter, 200 kare, `PlayerData.hands`). `HeldItem`: o anki sprite'ın el noktasına piksel ızgarasında oturur, el önde/arkada sıralanır. Meşale (`PlayerLight`) buna bağlandı. Ekipman altyapısı (`KakScope.Equipment` kapalı): `EquipmentData` yuva (+Ana el/Yan el) + nadirlik + `StatModifier`, `Equipment` (envanter, takma, 3 aynı → üst nadirlik), `EquipmentCatalog` 5 örnek (`KakEquipmentSetup`), ana el eşyası ele oturur. Testler `ElNoktasiTests` 1 + `EkipmanTests` 2
- [x] K8 oyun içi kart seçimi: `RunPerks` (cesaret: kıl payı 1, altın 0,5, saniye 0,05; ihtiyaç 7 + 4·seviye), dolunca oyun durur → `PerkPanel` 3 kart (sırayla zıplar, gelmeden seçilmez) → seçilen kart oyuncu sayfasına (`PlayerStats.ApplyLive`, katlanmaz) + 1,2 sn güvenli devam; 13 kart (`KakPerkSetup`): mıknatıs, kalkan yenilenmesi, çevik adım, altın avcısı, skor ateşi, kıl payı, ince yapı, toparlanma, sezgi, kalp parçası, eşya yağmuru, **taş kırıcı** (`RockBreaker`: 12/9/7 sn'de yakındaki taşı kırar), hayalet anı (`GhostMoment`); HUD'da cesaret çubuğu (`CourageBar`). Tek kişilik sonsuz; bot ilk kartı seçer. Testler `KacisKartiTests` 3
- [ ] K9 denge, telefon, mağaza — **denge tamam** (kartlı, bot ilk kartı seçer): güç 0 skill=1 152/158/267 sn, skill=0,25 68/76/112 (hedef acemi 60-120, usta 150-300 ✓); yarım gelişim (güç 0,44) skill=1 217/218/399, skill=0,25 71/146/151, skor 2-3 kat. **Mağaza metni** Faz 15'e göre (`Docs/Magaza/listing.md`; görseller K2 kararından sonra). **Ekonomi tahmini:** oyun başı ~40 altın + 1-2 anahtar (ahşap sandık ~110 altın + parşömen) ≈ 200 altın/oyun → ilk 10 oyunda neredeyse her oyun bir gelişim; hepsini doldurmak ~200+ oyun (uzun kuyruk). Kalan: telefona kurulum + kullanıcı değerlendirmesi

**Faz 14 — odak ve kalite** (`prompts/faz-14-odak-kalite.md`, 2026-10-04). Sıra: Ö1 kapsamı daralt → Ö2 menü + piksel logo → Ö3 oyun içi sadelik → Ö4 görsel tutarlılık → Ö5 his/akış → Ö6 telefon.
- [x] Ö1 kapsam: `KakScope` (Worlds/Pets/Missions/DailyReward/CalmEvent kapalı; FeatureGate hiç açmaz, FeatureButton gizlenir, seçili dünya hep Zindan, Sessizlik olayı yok), Zindan 5 eşya (`KakBalance.ApplyFocusItems`). Gizli sistem testleri `KakScope.EnableAll`, ürün kapsamı `OdakKapsamTests`. Testler 131/131 + 27/27
- [x] Ö2 menü + logo: piksel logo (`tools/kak_gen_logo.py` → `Art/UI/logo_pixel.png`, el yapımı 2 px gövdeli harfler, altın rampa), alt başlık ve istatistik satırı kalktı, tek büyük OYNA, altında KARAKTER + 2 KİŞİ, ayarlar sağ üstte dişli; PET/DÜNYALAR kapsam açılınca geri gelir. (Mağaza görselleri eski logoyla: Ö6'da yenilenecek)
- [x] Ö3 oyun içi sadelik: yakın geçişte "YAKIN!" yazısı yerine küçük altın "+5" + camgöbeği kıvılcım, eşya adı yalnız ilk alışta (`SaveData.seen` "pu_named_<tür>"), oyun sonu istatistik satırı "SÜRE · YAKIN GEÇİŞ" (COMBO kalktı), görevsiz panel 1370 (sıradaki hedef satırı TEKRAR'a değmiyordu). Kademe ve olay afişleri kaldı. Testler 131/131 + 27/27
- [x] Ö4 görsel tutarlılık: 5 eşya ikonu tek stilde (`tools/kak_gen_icons.py`: 32 px, aynı kontur ve ışık, renk rolüne göre tek hâle), arayüz ikonları yeniden (oynat, karakter, dişli, duraklat, kilit, onay), karakter kartı sade (büyük karakter, açıklama yalnız detayda, dash çubuğu yok), taş türleri bakışta ayırt edilir (`tools/kak_gen_rocks.py`: güdümlü = kırmızı göz, parçalanan = parlayan çatlak, seken = yuvarlak mor taş `keepUpright`). Testler 131/131 + 27/27
- [x] Ö5 his ve akış: HUD skoru yalnız sayı ("SKOR:" kalktı), ilk oyun ipuçları zaten dash'siz, dokunulmazlık yanıp sönmesi süreye bağlı (yavaş karelerde dokunulmazlıktan uzun sürüyordu), Zindan bot ölçümü usta 91-246 / acemi 135-295 sn (tek dünya, 1-4 dk seans hedefine uygun). Editör sesi kapalı (kullanıcı isteği). Testler 131/131 + 27/27
- [x] Ö6a mağaza: `Docs/Magaza/listing.md` TR/EN odak kapsamına göre (dash/pet/görev/günlük ödül/Sessizlik yok; 5 eşya, 3 olay, altınla güçlenme, tek telefonda 2 kişi; anahtar kelimede "pet" → "2 kişilik"; sınırlar 76-79/80, 119-124/170, 78-79/100). Görseller: 1 oyun (piksel logo), 2 karakter, 3 güç, 4 kıl payı kaçış (göktaşı "!"), 5 rekor, 6 iki kişilik; eski 4_pet ve 6_gunluk silindi. `GiveAllPowerups` yalnız bölümün eşya listesini verir. Testler 131/131 (+1 atlanan) + 27/27
- [ ] Ö6b telefon: odak sürümünü iPhone'a kur, kullanıcı "AI slop" gözüyle değerlendirsin. **İmzalı build hazır** (2026-10-04, main 9b00b80): `Builds/iOS/DerivedData/Build/Products/Release-iphoneos/KaAtaKa.app` → telefon kabloyla bağlanınca yalnız `xcrun devicectl device install app` + `info apps` doğrulaması kaldı

**Faz 13 — yeni dünyalar TAMAM** (`prompts/faz-13-yeni-dunyalar.md`, PR #41-#44). Faz 13 sürümü **iPhone'a kuruldu** (2026-10-03, iOS 26.7). Sıradaki: kullanıcının 4 dünyayı denemesi ve geri bildirimi; sonra boks ringi, kostümler, karakter yetenekleri, online (Faz 11 notları), 120 FPS ayarı, duraklat menüsünden kontrol düzeni.
- [x] K1 karanlık sistemi (`DarkWorld`: tema `darkness` → karanlık katmanı SpriteMask dışında, `PlayerLight` meşale deliği + yumuşak kenar + hâle + elde meşale, `TorchProgress` Sv1-4 yarıçap 1,7/2,1/2,5/2,9, fiyat 250/600/1200, ipuçları: taş parıltısı, fırlatıcı gözleri (atıştan önce yanar), "!", altın, eşya). Testler 115/115 + 27/27
- [x] K2 Karanlık Mağara (`tools/kak_gen_cave.py`, `KakCaveSetup.Setup`): mağara arenası/karoları, Taş Muhafız (mağara, mor taş), yarasa (`ProjectileMotion.Wave`), sarkıt (göktaşı + "!"), spor ve örümcek ağı (yavaşlatır, can götürmez; oyunculara `PlayerStatus` + baş üstü simge), zindan taşları; GÖÇÜK! (sarkıt yağmuru) ve YARASA SÜRÜSÜ! (yeni 5. olay, şerit uyarısı + kaçış boşluğu); karanlık 0,95, duvar meşalesi yok; 6. oyunda açılır; dünya başına genel rekor (`SaveData.worldBests`, `EndlessWorlds.Best/SetBest`); DÜNYALAR 4 kart + mağara kartında MEŞALE yükseltme. Bot: usta 63 / acemi 90 sn (oynak), sonra yarasa yumuşatıldı. Testler 120/120 + 27/27
- [x] F1 Futbol Arenası (`tools/kak_gen_football.py`, `KakFootballSetup.Setup`): çim saha + çizgiler + üst kale + tribün/panolar, hakemler (Ata kareleri siyah formaya), top (seker), **sarı kart takip eder** (Homing 1,4 sn), **kırmızı kart yavaş ama tek atar**, **2 sarı = oyundan atıldın** (`PlayerHealth.Eliminate`, tema `redCardEliminates`; yumuşak: 1 sarıdayken baş üstünde uyarı, 15 sn'de unutulur), **ŞİŞE YAĞMURU!** (yukarıdan şişe, "!"), DEV TOP!, KONTRA ATAK!, HAKEM DÜDÜĞÜ!; 10. oyunda açılır. Bot: usta 82 / acemi 169 sn (eski kart kurallarıyla, oynak). Testler 125/125 + 27/27
- [x] F2 gol fırsatı (`GoalChance`, tema `goalChance`): ~30-40 sn'de bir serbest top (taşlardan uzak, altın hâle + zıplayan altın ok: tehlikeli toplardan ayırt edilir), dokununca ayağında sürülür, üst kale parlar; kaleye götür → GOOOL! +300×çarpan skor, +5 altın; vurulunca top seker ve 0,8 sn alınamaz. Testler 128/128 + 27/27

**Faz 12 — yumuşak oynanış** (`prompts/faz-12-yumusak-oynanis.md`, 2026-10-03). Sıra: H1 iki kişilik hatalar → H2 Buz ayarları → H3 serbest kontrol düzeni → H4 Görünmezlik → H5 akıcılık turu → telefona kurulum. Karanlık Mağara ve Futbol: kullanıcı ne atılacağına karar verince.
- [x] H1 iki kişilik: görünmez dönüş hatası (asıl renk Start'ta alınıyordu, soğuk göstergesi ilk karede rengi boşaltıyordu → Awake), düşen sahadan kalkar + güvenli nokta + yumuşak beliriş, yakın geçiş oyuncu başına, soğuk çubuğu oyuncu başına + ateş arkadaşı yarı ısıtır, donma soğuğu atar (0,45), eşya ×1,5. Testler 100/100 + 27/27
- [x] H2 Buz: olaylar dünyaya göre (`WorldTheme.eventRain*/eventRolling*` → `EndlessEventManager.ApplyTheme`; Buz: SARKIT YAĞMURU + DEV KARTOPU), kartopu çarpana kadar büyür (üst sınır 1,6-2,1) + büyüdükçe yavaşlar (`growSlowdown` 0,82-0,9) + bağışlayıcı çarpışma (`growHitShare` 0,5: görüntü tam, alan yarısı), buz ayakkabısı dikey artışı da kaldırır. Bot (bugünkü taban usta 75 / acemi 57 sn ile aynı aralıkta). Testler 102/102 + 27/27
- [x] H3 serbest kontrol düzeni: Ayarlar → Kontroller DÜZENLE → `ControlLayoutEditor` (Resources prefab, `KakControlLayoutSetup.Build`): oyun ekranı taslağı üzerinde joystick ve AKSİYON sürüklenir (arenanın üstü dahil), seçilenin boyutu kaydırıcıyla (0,6-1,6), SAĞ / SOL aynalar, VARSAYILAN, TAMAM. Oyunda `VirtualJoystick.ApplyLayout` / `DashButton.ApplyLayout` (serbest düzende kök kanvasa taşınır, dokunma alanı çapın 2,1 katı; iki kişilikte uygulanmaz). Testler 106/106 + 27/27. (Duraklat menüsünden açma: ileride)
- [x] H4 Görünmezlik (`PowerupType.Invisible`, 5 sn, ağırlık 0,5, minStage 1; `KakBalance.SetupInvisible`, ikon/işaret `tools/kak_gen_invisible.py`): fırlatıcılar göremez → arenaya rastgele atar (oyuncuya ±28° koni hariç), göktaşı rastgele yere, güdümlü kilidi kaybeder, olaylar görünmeze nişan almaz, fırlatıcı başında sallanan "?"; iki kişilikte görünen oyuncuya atar. Oyuncu titreşen saydam. Hayalet aynen kaldı. Testler 110/110 + 27/27
- [x] H5 akıcılık: oyuncu fiziğinde ara değerleme (50 Hz fizik / 60 Hz ekran takılması), ölümde gövde görünen yere sabitlenir (kıpırdama yok), joystick yumuşak rampa (küçük itiş %30 hız, yarı yolda tam), koşu animasyonu itişe göre, taşlar 0,1 sn'de büyüyerek belirir. 120 FPS kararı: şimdilik 60 (bütçe); istenirse ayar. Testler 111/111 + 27/27

**Faz 11 — telefon geri bildirimi TAMAM** (`prompts/faz-11-oyuncu-geri-bildirimi.md`). Sıradaki: kullanıcı telefonda dener (kablo takınca kurulum), geri bildirime göre ince ayar; sonra Futbol / Boks ringi dünyaları, kostüm, karakter yetenekleri, online (notlar prompt dosyasında).
- [x] G1 His ve kontrol (ölümde donma, çapraz koşu histerezisi, hız 5→4 + hız güçlendirmesi ×1,25, göktaşı "!", altın tek tek + aralıklı, dash rafa, kontrol boyutu ayarı). Testler 81/81 + 23/23
- [x] G2 Zorluk süreye bağlı (0/30/75/150/240/360 sn, `KakEndlessSetup.SetupStages`) + taş dağılımı yumuşak + ekranda en fazla 2 büyük taş + yakın geçiş skor çarpanı (+0,1, en fazla ×3, hasarda yarıya). Bot (3 can): usta 232 sn ortanca, acemi 135 sn → G3 tek canla yeniden ölç
- [x] G3 Tek can başlangıç + kalıcı yükseltmeler (Can/Hız/Güç süresi, `CharacterProgress`) + karakter detay ekranı (karta dokun) + arketipler (Ata 1→3, Ada 1→2 hızlı-ince, Çevik 1→2 küçük-çok hızlı, Tank 2→5 hantal-kalkanlı, Şanslı 1→3 altın) + dolu canla kalp = bu oyunluk +1 kalp + pet seviyeleri + karakter ekranı 2. oyunda. Bot (1 can): usta ortanca 88 sn, acemi 78 sn. Testler 83/83 + 27/27
- [x] G4 Pranga (PowerupType.Shackle, harmful, minStage 1; 4 sn ×0,6 hız, hız güçlendirmesiyle aynı kanal; kırmızı ikon/çip/yazı). Testler 84/84 + 27/27
- [x] G5 İki kişilik yerel mod: menüde 2 KİŞİ, Ata (sol joystick/WASD) + Ada (sağ/oklar), yeteneksiz ve petsiz, düşen 10 sn sonra 2,5 sn dokunulmaz döner, ikisi düşerse biter, ayrı rekor (`bestScore2P`), reklamla devam yok. Testler 86/86 + 27/27. İleride: online battle royale (not)
- [x] G6 Hafif hikâye: Ata ile Ada Taş Zindanı'nda altına dokunur → Taş Muhafızlar uyanır, lanet onları her yerde kovalar (yeni dünyaların gerekçesi). İlk açılışta 3 kart (`IntroStory`, atlanabilir, günlük ödülden önce), ayarlarda HİKÂYE, mağaza metninde. Testler 87/87 + 27/27
- [x] G7 Buz Gölü: kayma (sürtünme 0,33, dikey ×1,2), 8 yönlü kardan adam (4 kare atış), büyüyen kartopları (5 tür: kartopu, küçük, dev, seken, buz sarkıtı "!"), eldiven (yakala → kartopu ikonlu düğmeyle fırlat, düşman kartopunu parçalar), buz ayakkabısı (tutuş), ateş + soğuk göstergesi (donma: can götürmez, hareketsiz kalırsın; garantili ateş), kar yağışı, DÜNYALAR paneli (3. oyunda açılır, dünya başına rekor). Bot (1 can): usta 92 sn, acemi 64 sn. **Yan bulgu:** iki kişilikte 1. oyuncu taşlardan etkilenmiyordu (statik hurtbox) → düzeltildi + test. Testler 95/95 + 27/27
Her alt faz sonunda: testler + bot ölçümü + PR + (kullanıcı kablo takınca) telefona kurulum.
- [x] (2026-10-03) Dış eklentiler kuruldu: Unity resmi eklentisi + skill-creator (kullanım kuralları SKILL.md §6.7). Faz 11 sürümü telefonda kurulu; kullanıcının oyun geri bildirimi bekleniyor.

### Onay bekleyenler
- **Faz 15 K2 renk:** "Canlı" mı "Çok canlı + parlama" mı (önizleme gerçek oyundan; öneri Canlı) → `invoke KakVividSetup SetLevel 1|2`, ardından mağaza görselleri.
- **Faz 15 telefon testi:** tempo hissi (ilk oyunlar yavaş mı, güçlendikçe hızlanıyor mu), kart seçimi oyunu bölüyor mu, sandık/gelişim ekonomisi hızı, meşale/el.
- **Faz 14 odak sürümü (telefon):** tek dünya, 5 eşya, 3 olay, piksel logo, sade menü/HUD → kullanıcı "hâlâ AI slop mu?" gözüyle oynasın; mağaza metni ve görselleri (`Docs/Magaza`) de yeni kapsamla.
- **Faz 11 telefon testi:** tek can başlangıcı, yükseltme hızı, Pranga, 2 kişilik, Buz Gölü kayması → kullanıcının geri bildirimi.
- **Ücretsiz Apple hesabı:** telefondaki uygulama 7 günde bir yeniden kurulmalı (kablo takılınca `xcrun devicectl device install app`).
- **Sesler** prosedürel üretildi, Claude dinleyemedi. Kulağa hoş gelmeyen varsa söyle (`tools/kak_gen_audio.py` ile yeniden üretilir).
- **Denge hissi:** telefonda birkaç oyun oyna → "çok zor / çok kolay / tam" + hangi taş/olay haksız hissettirdi. Değerler `KakEndlessSetup.SetStage` ve `KakContentSetup` içinde tek yerde.
- Palet: Endesga 32 varsayımla uygulandı (kullanıcı değiştirmek isterse: `KakPalette` + `tools/kak_palette.py` + `kak_art_pass.py`).
- Kullanıcı tüm izinleri verdi (2026-09-25): faz dallarına push, PR birleştirme, onaysız plan başlatma, test için Unity'yi kullanma.
- **ADA** adı geçici (kullanıcı değiştirebilir: `Loc.cs` `char_Ada`).
- **Destek e-postası** gizlilik politikasına ve mağazaya (kişisel e-posta kendiliğinden yazılmadı).
- **Hedef kitle** (Play: 13+ önerildi) ve **reklam** açılacak mı (`Docs/Reklam-Hazirlik.md`).
- **Mağaza görselleri** onayı (`Docs/Magaza/`; değişiklik için `tools/kak_store_shots.py`).
- (İsteğe bağlı) **Metal araç zinciri** indirilsin mi (`xcodebuild -downloadComponent MetalToolchain`, Xcode 26'da ayrı; Unity log'undaki "metal" hatalarını giderir, build'i engellemiyor).

## Bilinen hatalar ve riskler

- ✅ (Faz 14 Ö5) **Ara sıra düşen test** `IkiKisilikTests.DonenOyuncuGorunur_*`: dönüşte yanıp sönme 25 × 0,1 sn sayıyordu, yük altında dokunulmazlıktan uzun sürüyordu → süreye bağlandı. `BuzTests.Kartopu_YolAldikcaBuyur` süre yerine koşul bekliyor.

- ✅ **Item'lar ekranın yanlış yerinde çıkıyor** — Faz 1: asıl sorun powerup ikonlarının opak siyah kare zemini (5 ikonun hepsi RGB) + Retry sonrası bozuk yönetici yapısı. Konumlar artık `PlayableWorldRect` içinde (42/42 test). (kullanıcı bildirdi, 2026-09-25). Neden henüz bulunamadı. Hesaplanan spawn alanı kağıt üstünde arenanın içinde görünüyor, Play modunda teşhis gerekli (Faz 1.2).
- ✅ (Faz 1) **Fizik duvarları görselle hizasız.** Sprite dış kenarından 0.15 birim içerideler, görseldeki iç duvar yaklaşık 0.6 birim içeride. Karakter ve mermiler duvar çiziminin üstüne girebiliyor.
- ✅ (Faz 1.5 / 3) **Görsel kalite:** tek piksel yoğunluğu (0,024 birim/piksel), Point filtre import kuralları, filigran temizlendi. 2D Light hâlâ yok (Universal Renderer; Karanlık dünyada karar). Eski kayıt: karışık piksel yoğunluğu (karakter 48px/ölçek 0.8, arena 2048px/ölçek 0.4, spawner ölçek 4), sprite'lar Bilinear (bulanık), arena görselinin sağ alt köşesinde yapay zeka filigranı (✦), URP Universal Renderer (2D Light çalışmaz). Faz 3'te çözülecek.
- ✅ (Faz 4) Menü sahnesinde sadece Oyna butonu var.
- ✅ (Faz 1.5) **Arena ekranın sadece ~%46'sını kaplıyor** (kare arena, 9:19.5 telefon). Üstte ve altta ~%54 ölü alan var. Kullanıcı bunu en büyük görsel sorun olarak görüyor. Çözüm Faz 1.5 (onaylı kompozisyon): üstte HUD bandı (duvar yüzü), ortada kare arena (ekran enine ölçekli), altta koridor ve kontrol alanı (sol joystick, sağ aksiyon).

> Durum: 🔴 kritik · 🟠 orta · 🟡 düşük · ✅ düzeltildi. "Doğrulanmadı" = kod okunarak bulundu, Play modunda test edilmedi.

- ✅ (Faz 1) **Retry / menüye dönüp tekrar oynama bozuk (✔ testle doğrulandı: `Retry_IkinciOyunTamamenCalisir`, `GameOver_Menu_TekrarOyna_Calisir`).** `GameManager` `DontDestroyOnLoad` kullanıyor ve `ScoreManager` aynı objede. Sahne yeniden yüklenince eski GameManager `isGameOver = true` ile yaşamaya devam ediyor, yenisi yok ediliyor. Sonuçlar: skor artmaz, ikinci ölümde Game Over açılmaz, powerup çıkmaz, `gameOverPanel/scoreText` referansları ölü. Üstelik `LevelManager/DifficultyManager` sahnede hazır değil, onları GameManager.Awake yaratıyordu, yani LevelData hiç uygulanmaz. **Öneri:** GameManager'dan `DontDestroyOnLoad`'ı kaldır (sahneye özel olsun), `LevelManager` ve `DifficultyManager`'ı sahneye kalıcı obje olarak ekle.
- ✅ (Faz 1) **SampleScene doğrudan Play'e basılınca LevelData yok** (✔ testle doğrulandı). Runtime'da yaratılan LevelManager'ın `defaultLevel`'ı boş, bu yüzden "HİÇBİR LEVEL DATA" hatası verir. Sadece menüden girince çalışır. **Öneri:** LevelManager sahneye eklenip `defaultLevel = Endless_Level1_LevelData` atanmalı.
- ✅ (Faz 1.5, bileşen artık JoystickZone'da) **Joystick scripti `JoystickHandle` üzerinde** (dokümana göre `JoystickBG`'de olmalı). Dokunma alanı sadece küçük topla sınırlı olabilir. Doğrulanmadı.
- ✅ (Faz 1) `SceneLoader` `timeScale`'i sıfırlıyor ama `fixedDeltaTime`'ı sıfırlamıyor. SloMo sırasında ölünürse sonraki oyunda fizik adımı 0.008 kalır. `PauseManager` ise `fixedDeltaTime = 0` yapıyor.
- ✅ (Faz 1) Ghost ve Speed süreleri `WaitForSeconds` kullanıyor, bu yüzden SloMo sırasında uzuyorlar.
- ✅ (Faz 1) `DifficultyManager` aktif spawner'ları dizideki sıraya göre seçiyor, bu sıra da `FindObjectsByType(None)` ile geliyor ve garanti değil. Hangi köşelerin aktif olacağı öngörülemez.
- 🟡 `ShieldData.duration = 10` hiçbir işe yaramıyor, kalkan vurulana kadar sürüyor.
- ✅ (Faz 1) README güncel değil ("Unity 2022+", `CornerShoother`, eksik dosyalar).
- ℹ️ Input System paketi kurulu ama kod eski Input Manager kullanıyor. Şu an sorun yok (Active Input Handling = Both olmalı).
- ✅ (D0) Plastic (Unity Version Control) paketi kaldırıldı (proje git kullanıyor; `.plastic/` diskte duruyor, git dışı).

- ✅ (Faz 1) **`Wall` etiketi projede tanımlı değil** (✔ test buldu): `PowerupSpawner.SpawnRandomPowerup` içindeki `CompareTag("Wall")` her çağrıldığında hata logluyor.
- ℹ️ ~~HUD oranlara göre bozuk~~ — yanlış alarm: ekran görüntüsü aracının zamanlama hatasıydı (düzeltildi). HUD yine de Faz 1.5'te yeniden kurulacak.
- ✅ (Faz 3, tek piksel yoğunluğu; taş 1,0 → 0,67 birim) **Karakter çok küçük:** arena genişliğinin yaklaşık 1/20'si, taşlardan küçük (ekran görüntüsü).
- ✅ (Faz 3, sıcak taş + kontur + renk rolleri) **Taşlar zeminle karışıyor** (renk testi): gri tonlama ve bulanık görünümde neredeyse kayboluyor. En belirgin öğe sarı joystick. Faz 3'te renk rolleri.
- ✅ (Faz 1) Ölüm anında invincibility coroutine'i timeScale=0'da donarsa oyuncu sprite'ı gizli kalabilir (ekran görüntüsünde Game Over'da oyuncu görünmedi, doğrulanacak).

- ✅ (Faz 1.5, DungeonFrame) **Arka plan Unity varsayılan skybox'ı** (üstte mavi, altta gri). Hem çirkin hem gereksiz render maliyeti. Faz 1.5.
- 🟡 Sahne ölçekleri tutarsız (TopRightSpawner ölçek 1 + Visual 4, diğerleri ölçek 4 + Visual 1; Player 0.8 + Visual 3). Faz 3 ölçek temizliği.
- ✅ (Faz 5, `WorldPopup` havuzu) Powerup geri bildirim yazısı eski `TextMesh` + her seferinde `new GameObject`.
- 🟡 SloMo sırasında ölünce oyuncu kırmızı tonda kalıyor (ölüm göstergesi olarak bırakıldı, Faz 3 ölüm efektiyle değişecek).

## Denge değerleri (referans, 2026-10-03)

> Tek kaynak: `KakEndlessSetup.SetupStages` (kademeler), `KakContentSetup` (taş dağılımı), `KakBalance` (güçlendirmeler, Pranga), `KakMetaSetup` (karakterler), `CharacterProgress` (yükseltme fiyatları), `KakIceSetup` (Buz Gölü).

| Parametre | Değer |
|---|---|
| Oyuncu hızı / can / ölümsüzlük | Ata 4 / 1 kalp (yükseltmeyle 3) / 0,35 sn |
| Taş hızı / hasar / fırlatıcı aralığı | 1,5 / 1 / 1,5 sn |
| Skor | 10 / sn × kademe çarpanı × skor çarpanı (yakın geçiş +0,1, her 20 sn +0,05, en fazla ×3, hasarda yarıya) |
| Powerup çıkma aralığı | 5–10 sn |
| Powerup ağırlıkları | Heart 1,0 · Speed 0,8 · Pranga 0,7 (Kolay'dan sonra) · Shield 0,6 · SloMo 0,5 · Ghost 0,4 · Buz: Ateş 1,7, Eldiven 0,8, Buz Ayakkabısı 0,8 |
| Yükseltme fiyatları | Can 60/250/600/1000 · Hız 80/200/450 (+%4) · Güç 80/200/450 (+%12 süre) · Pet 150/400/800 |

| Karakter | Can (başlangıç → üst) | Hız | Gövde | Ek | Fiyat |
|---|---|---|---|---|---|
| Ata | 1 → 3 | 4 | 1,0 | | bedava |
| Ada | 1 → 2 | 4,3 | 0,92 | | bedava |
| Çevik | 1 → 2 | 4,6 | 0,8 | | 300 |
| Tank | 2 → 5 | 3,5 | 1,2 | kalkan | 800 |
| Şanslı | 1 → 3 | 4 | 1,0 | altın ×1,25 | 1500 |

| Kademe | sn | Spawner | Aralık× | Hız× | Boyut× | Oyuncu× | Skor× |
|---|---|---|---|---|---|---|---|
| Başlangıç | 0 | 1 | 1,10 | 0,88 | 1,00 | 1,00 | 1,0 |
| Kolay | 30 | 2 | 1,05 | 0,96 | 1,00 | 1,02 | 1,1 |
| Orta | 75 | 3 | 0,95 | 1,05 | 1,04 | 1,04 | 1,2 |
| Zor | 150 | 3 | 0,84 | 1,15 | 1,08 | 1,06 | 1,35 |
| Cehennem | 240 | 4 | 0,74 | 1,26 | 1,13 | 1,08 | 1,5 |
| İmkansız | 360 | 4 | 0,64 | 1,38 | 1,20 | 1,10 | 1,75 |

Buz Gölü: kademeler ×1,5 süre, ×0,93 hız; sürtünme 0,33, dikey ×1,2, soğuk 35 sn. Bot ölçümü (1 can, ortanca): Zindan usta 88 / acemi 78 sn, Buz usta 92 / acemi 64 sn.
