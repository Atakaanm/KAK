---
name: kacatakac
description: KaçAtaKaç (KAK) Unity oyununun proje hafızası — oyunun vizyonu, mimarisi, sistemlerin birbirine nasıl bağlandığı, denge değerleri, bilinen hatalar ve yapılan işlerin günlüğü. Bu projede herhangi bir kod, sahne, denge, özellik veya hata işine başlamadan ÖNCE yükle; iş bittikten sonra progress.md ve learnings.md dosyalarını güncelle.
---

# KaçAtaKaç — Proje Skill'i

Bu skill yaşayan bir belgedir. Her oturumda öğrenilenler buraya eklenir.
- **Bu dosya (SKILL.md):** Oyunun ne olduğu, mimari, kurallar. Nadiren değişir.
- **[progress.md](progress.md):** Nerede kaldık, yol haritası, bilinen hatalar. Her iş sonunda güncellenir.
- **[learnings.md](learnings.md):** Tarihli günlük: kararlar, keşifler, kullanıcı tercihleri, tuzaklar.
- **[denetim.md](denetim.md):** 2026-09-26 kapsamlı denetim bulguları ve D0-D6 düzeltme sonuçları (ölçümler dahil).
- **[3c.md](3c.md):** Faz 3c bağlılık katmanı: araştırma özeti + kaynaklar, açılma takvimi, karakter/pet/görev/ekonomi tasarımı, uygulama sırası.
- **[roadmap.md](roadmap.md):** Vizyon (Sonsuz + Buz/Futbol/Karanlık dünyaları), 2.5D ve görsel değerlendirmesi, Faz 0-10.
- **[sanat-rehberi.md](sanat-rehberi.md):** Görsel anayasa: palet, renk rolleri, değer hiyerarşisi, **ekran kompozisyonu [KESİN]** (HUD bandı + ekran enine ölçekli kare arena + koridor/kontrol alanı), ışık ve his ölçüleri. Görsel işlerden önce mutlaka oku. Kullanıcı: "renk uyumu çok önemli".
- **[prompts/](prompts/):** `00-ana-prompt.md` (her oturumun çalışma kuralları) + `faz-XX-*.md` (her fazın detaylı görev promptu). Kullanıcı "faz N'i uygula" dediğinde ana prompt ve ilgili faz dosyasını oku, uygula.

## 1. Oyunun özü

**KaçAtaKaç**, mobil (dikey/yatay uyumlu) **2.5D arcade hayatta kalma** oyunu.
Oyuncu kapalı bir arenada; 4 köşedeki fırlatıcı (spawner) oyuncuya doğru taş atar.
Amaç: kaç, hayatta kal, skoru büyüt. Skor zamanla artar (saniyede 10 puan × zorluk çarpanı).

- **Kontrol:** Sanal joystick (mobil), WASD/ok tuşları (editör).
- **Can:** 3 kalp, vurulunca kırmızı flaş + kısa ölümsüzlük (0.35 sn).
- **Güçlendirmeler (5):** Heart (+1 can), Shield (1 vuruş bloklar), Speed (x1.5 hız, 6 sn), SloMo (zaman x0.4, 4 sn — oyuncu normal hızda kalır), Ghost (mermiler içinden geçer, 5 sn).
- **Modlar:** `Endless` (aktif, skor arttıkça 6 kademeli zorluk, 30-45 sn'de bir olay) ve `Stage` (bölüm: hayatta kal / gol; `LevelModeController`, Faz 2B altyapısı hazır, arayüzü bağlanmadı).
- **Hedef vizyon:** Sonsuz Mod (sadece taştan kaçış) + Bölüm Modu dünyaları: **Buz** (kaygan, anında duramazsın), **Futbol** (ayağında top, tek tuşla gol; futbolcular sarı/kırmızı kart atar), **Karanlık** (sınırlı görüş, parlayan mermiler) ve daha fazlası. Görsel hedef: "basit ama süper". Ayrıntı: [roadmap.md](roadmap.md).
- **Güncel öncelik: Sonsuz Mod.** Bölümler sonra ve büyük ölçüde reskin + ayarla gelecek. Sonsuz için yazılan sistemler (mermi etkileri, durum efektleri, spawn noktaları, karo tabanlı arena) genel kurulmalı ki bölümlerde yeniden kullanılsın.
- **Hedef platform:** Google Play / App Store. Monetizasyon/bağlılık planı (Faz 3c): altın toplama, istatistikli ve kilitli karakterler (`PlayerData.isLocked/unlockPrice`), pet'ler, adım adım açılan özellikler (`SaveData.seen`).

**Kullanıcının vizyonu (README + veri yapılarından):** ScriptableObject tabanlı modüler yapı. Yeni karakter, arena, mermi ve bölüm eklemek kod yazmadan, sadece veri dosyası oluşturarak yapılabilmeli. Bu ilkeyi koru.

## 2. Teknik ortam

- **Unity 6000.3.8f1** (Unity 6), **URP** (Universal Renderer, SRP Batcher kapalı), Apple Silicon (arm64). Oyuncu ayarları `KakBuild.ConfigurePlayer` (dikey, IL2CPP ARM64, Android API 25+, iOS 14+). Android/iOS build modülleri kullanıcıda kurulu değil; ölçüm için macOS dev build (`KakBuild.BuildMacDev` + `-kakbench`).
- Editör yolu: `/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app`
- Unity'yi açmak için: `open -a "/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app" --args -projectPath "/Users/atakaan/KacAtaKac"`
- Derleme hatalarına bakmak için: `grep -E "error CS|Exception" ~/Library/Logs/Unity/Editor.log`
- Serialization **Force Text** → `.unity`, `.prefab`, `.asset` dosyaları YAML olarak okunabilir.
- Paketler: URP 17.3, Input System 1.18 (kod hâlâ eski `Input.GetAxisRaw` da kullanıyor, Active Input Handling = Both), UGUI 2 (TextMeshPro dahil), 2D Sprite, Test Framework, Timeline. Plastic/Visual Scripting/AI Navigation/Multiplayer Center D0'da kaldırıldı.
- VCS: GitHub (`Atakaanm/KAK`, dal `main`). `.plastic/` diskte kalabilir ama git dışı, paket yok.
- Sahneler (Build sırası): `0 MainMenu`, `1 Game` (oyun sahnesi; 2026-09-26'ya kadar adı SampleScene idi). Sabitler: `SceneLoader.MENU_SCENE / GAME_SCENE`.

## 2.5 Otonom test altyapısı (Faz 0'da kuruldu) — Unity'yi Claude kontrol eder

**Köprü:** `Assets/Editor/ClaudeBridge/KakBridge.cs` (kendi asmdef'i, oyun kodu bozulsa da çalışır) ↔ istemci `tools/kak_bridge.py`. Unity açıkken **arka planda da** komut alır. Unity uyursa istemci 45 sn sonra pencereyi öne getirir. Dosyalar `.claude-bridge/` (git dışı).

```bash
python3 tools/kak_bridge.py hb            # editör canlı mı
python3 tools/kak_bridge.py refresh       # değişiklikleri içe aktar + derlemeyi bekle (compile.json'daki hataları basar)
python3 tools/kak_bridge.py tests PlayMode [SinifAdi]   # testler; sonuç ✅/❌ listesi
python3 tools/kak_bridge.py playfor 20    # Play → 20 sn → hata özeti → Stop
python3 tools/kak_bridge.py play | stop
python3 tools/kak_bridge.py shots onek    # Play modunda 5 oranda ekran görüntüsü (9:16, 9:19.5, 9:20, 9:21, 3:4)
python3 tools/kak_bridge.py menu "KacAtaKac/Dev/Test Botunu Başlat (usta)"
python3 tools/kak_bridge.py invoke TipAdi MetotAdi [stringArg]   # statik editör metodu çağır
python3 tools/kak_bridge.py log 80 | errors | clear
python3 tools/kak_color.py goruntu.png    # gri ton / renk körlüğü / bulanık karşılaştırma + değer dağılımı
python3 tools/kak_montage.py out.png a.png b.png ...   # yan yana
```

- **Derleme arka planda yavaş:** assembly değişikliğinde 3-5 dk sürebilir. `refresh` 11 dk bekler, sabırlı ol.
- **Play'e giriş** domain reload nedeniyle yaklaşık 20 sn sürer.
- **Assembly'ler:** `KacAtaKac` (Assets/Scripts), `KacAtaKac.Editor` (Assets/Scripts/Editor), `KakBridge.Editor`, `KacAtaKac.Tests.PlayMode` / `.EditMode`. `Assets/Editor/*.cs` hâlâ Assembly-CSharp-Editor'da.
- **Testler:** `Assets/Tests/PlayMode/SonsuzModSmokeTests.cs` (regresyon: sahne, skor, ölüm, Retry, menü döngüsü, 20 sn hatasız oyun), `BotTests.cs` (60 sn usta bot: kararlılık, havuz). `KakTestUtil.LoadGameWithLevel()` menüdeki gibi LevelData seçip sahneyi açar. Test sırasında `Debug.LogError` testi düşürür.
- **Test botu:** `Assets/Scripts/Dev/KakAutoPilot.cs` (sadece editör/dev build). `PlayerMovement2D.InputOverride` üzerinden oynar, `Projectile.Active` listesini okur. `skill` 0-1.
- **Device Simulator tuzağı:** Kullanıcının editöründe Play görünümü **Simulator** (1080×2280, üst güvenli alan 116 px). Simulator açıkken oyun `Screen` boyutunu simüle cihazdan okur. `shots` komutu otomatik olarak Game view'a geçer, bitince Simulator'a döner. Tek `shot` için önce `python3 tools/kak_bridge.py view game`, sonra `view sim`.
- **Geliştirici ölümsüzlüğü:** `PlayerHealth.DevGodMode` (menü: KacAtaKac/Dev/Ölümsüzlük Aç-Kapa; `shots` otomatik açar).
- **Git:** Bu makinede `/usr/bin/git` Xcode yolu yüzünden bozuk olabilir. Komutların başına `export DEVELOPER_DIR=/Library/Developer/CommandLineTools` ekle.

## 3. Mimari haritası

```
MainMenu sahnesi
  MainMenuController ──(OYNA)──► GameSettings.SelectedLevel = defaultLevel ──► SceneLoader.LoadGame (SceneFader)
  MenuBackdrop (canlı arena arka planı), Ayarlar / Karakter panelleri, LanguageButton
  AudioManager (Resources/AudioManager.prefab, her sahnede EnsureExists, DontDestroyOnLoad; ayarlar SaveSystem'de)

Game (oyun sahnesi)
  GameManager [+ ScoreManager]  (sahneye özel; GameOver, LevelComplete, Retry, menü, ölüm yavaş çekimi)
  Managers/ LevelManager, DifficultyManager, ProjectilePool, PowerupSpawner, FeedbackManager(+FxChips),
            EndlessEventManager(+LaneWarning), WorldPopup   ← KakSceneSetup/KakEndlessSetup kurar
  LevelManager.Start ─► LevelData uygular:
      theme (WorldTheme, 2B) → arena görseli/tonu, zemin sürtünmesi, DungeonFrame karoları
      PlayerData  → PlayerMovement2D, PlayerHealth, PlayerDirectionSprite
      ArenaData   → ArenaAutoLayout (PlayableWorldRect, duvarlar, katı kaideler, fırlatıcı duruş noktaları)
      SpawnerData → CornerShooter[] + SpawnerDirectionAnimator[]
      Endless     → DifficultyManager (kademeler) + EndlessEventManager açık
      Stage       → SetupStage (köşe fırlatıcı ayarı + EnemyFactory düşmanları) + LevelModeController.Begin
      Powerups    → PowerupSpawner.Init
      sonra: ArenaAutoLayout.ApplyLayout → ScreenComposer.ForceRecalculate → Projectile.SetArenaBounds
  CornerShooter ─► Projectile.Launch/LaunchMeteor (ProjectilePool) ─► PlayerHitbox.Hurt ─► PlayerHealth.TakeDamage
      (ProjectileData.effect ≠ Damage → PlayerStatus: yavaşlama, donma, sarı/kırmızı kart)
  PowerupSpawner ─► PowerupPickup (havuzlu) ─► PowerupPickup.Apply ─► GameEvents.PowerupActivated ─► PowerupHud
  NearMissTracker / PlayerDash / PlayerJuice / ShieldBubble (oyuncu üstünde)
  HUD: HudBand (HealthUI kalpler, skor, combo, olay afişi, pause) · ControlArea (VirtualJoystick, DashButton,
       PowerupHud, OnboardingHints) · PausePanel · GameOverPanel (GameOverScreen)
```

**Ekran kompozisyonu (Faz 1.5):** `Main Camera` üzerinde `ScreenComposer` (CameraFitWidth kaldırıldı) → kamera boyutu/konumu + `HUDCanvas/HudBand/HudContent` (kalpler, skor, pause) + `HUDCanvas/ControlArea/ControlContent/JoystickZone` (kayan `VirtualJoystick`, bileşen artık zone üzerinde) + `DungeonFrame` (arena dışı dünya: Backdrop, koridor, duvarlar, Ledge, Vignette, 8 meşale). Kurulum aracı: `KacAtaKac/Ekran Kompozisyonunu Kur` (`KakScreenSetup`). Sıralama: DungeonFrame -300…-260, arena -100, oyun nesneleri ≥ 0.
**Oynanabilir alan (Faz 1):** `ArenaAutoLayout.PlayableWorldRect` duvarlar, mermi sınırı, powerup alanı ve bot için tek kaynak. `ArenaData.playableAreaNormalized` + fırlatıcı duruş noktaları (`anchorTopLeft`...).
**Yöneticiler sahnede kalıcı:** `Managers/{LevelManager, DifficultyManager, ProjectilePool, PowerupSpawner}` (`KacAtaKac/Sahne Yöneticilerini Kur`). GameManager sahneye özel (DontDestroyOnLoad değil). Zaman ölçeği sadece `KakTime` üzerinden, bilgi logları `KakLog.Info`.

**Taşlar (Faz 2A):** `Projectile.Launch(prefab, data, from, dir, speedMult, scaleMult)` / `LaunchMeteor(...)`. Davranış `ProjectileData.motion` (Straight/Bounce/Homing/Split/Meteor). Türler `Assets/Data/Projectiles/`, kademe listeleri `DifficultyStageData.availableProjectiles + projectileWeights`, seçim `DifficultyManager.PickProjectile()`.
**Olaylar:** `GameEvents` (PlayerDamaged, ShieldBlocked, PlayerDied, PowerupCollected, ProjectileHitWall, MeteorLanded, StageChanged). **Zaman:** `KakTime` (temel ölçek × pause × hit-stop). **Kayıt:** `SaveSystem.Data` (+ `Save()`).
**Çarpışma (D1):** oyuncuda `PlayerHitbox`: kökteki `CircleCollider2D` = ayak izi (katı, r 0,2, ayaklarda; duvar/kaide), çocuk `Hurtbox` = gövde kapsülü (trigger, 0,36×0,80, etiket Player). Taşlar yalnızca `PlayerHitbox.Hurt`'a vurur. Kalkan = `ShieldBubble` (oyuncuyu ölçekleme/boyama yok). Köşe kaideleri katı (`ArenaAutoLayout.solidPedestals`, oyun sırasında kurulur). Kurulum: `KacAtaKac/Oyuncu Çarpışmasını Kur`. Testler: `CarpismaTests`.
**Oyuncu bilgisi (D4):** `PowerupHud` (ControlContent/PowerupHud; `GameEvents.PowerupActivated(data, sn)`, 0 = vurulana kadar), `OnboardingHints` (ControlContent/HintText; `SaveData.seen` + `HasSeen/MarkSeen` — bir kez gösterilen her şey için genel bayrak listesi). Güçlendirme uygulamak için tek giriş: `PowerupPickup.Apply(data, player, pos)`.
**Bağlılık (Faz 3c):** `FeatureGate` (adım adım açılma; `Feature` enum + `IsUnlocked/IsNew/LockedHint/UnlockedMask`; menü `FeatureButton`, oyun sonu `GameManager.NewlyUnlocked` afişi, menü `WalletHud`), altın: `Coin` (havuzlu) + `CoinSpawner` (Managers) + `CoinHud` (HudBand) + `ScoreManager.Coins/AddCoins` + `GameManager.RunCoins/RunCoinBonus` → `SaveData.coins/totalCoins`, `GameEvents.CoinCollected/CoinClusterSpawned`. Kurulum: `KacAtaKac/Bağlılık Katmanını Kur (Faz 3c)` (`KakMetaSetup`). Tasarım: `3c.md`.
**His:** `FeedbackManager` (Managers altında, tek ParticleSystem `FxChips`), `KakCameraShake` (kamera), `PlayerJuice` (oyuncu).

**Önemli singleton'lar:** `GameManager`, `LevelManager`, `DifficultyManager`, `ProjectilePool`, `AudioManager`, `WorldPopup`, `LevelModeController` — `Instance` statik alanı ile. Sahneye özel olanlar OnDestroy'da temizlenir.

**Zorluk nasıl uygulanıyor (DifficultyManager):** skor eşiğine göre stage seçer → her spawner için `shootInterval = orijinal × shootIntervalMultiplier`, `activeSpawnerCount` kadarını aktif eder; mermi hız/boyut, oyuncu hızı ve skor hızı çarpanlarını getter'larla verir (`CornerShooter`, `PlayerMovement2D`, `ScoreManager` okur).

**Veri dosyaları (`Assets/Data/`):** `Endless_Level1_LevelData` (tek aktif bölüm), `Boy_/Girl_PlayerData`, `Dungeon_ArenaData`, `RockThrower_SpawnerData` (4 köşede de aynı), `Projectiles/*` (7 taş türü), `Stage1..6` zorluk kademeleri, `Powerups/*Data`.
⚠ Bazı veri dosyalarında görsel alanları boş (`Dungeon_ArenaData.arenaSprite`, spawner sprite'ları). Kod bu durumda **sahnedeki mevcut ayarları** kullanıyor. Sahnede bilinçli boş bırakılan alanlar `SahneDenetimTests.BilincliBos` listesinde.

## 4. Editör araçları (üst menü "KacAtaKac")

Hepsi tekrar çalıştırılabilir (idempotent), sonucu string döndürür, pencere açmaz. Köprüden: `invoke <Sınıf> <Metot>`.

| Menü | Dosya (Scripts/Editor) | İşlev |
|---|---|---|
| Sahne Yöneticilerini Kur | `KakSceneSetup.SetupManagers` | Managers/{LevelManager, DifficultyManager, ProjectilePool, PowerupSpawner} + referanslar |
| Oyuncu Çarpışmasını Kur | `KakSceneSetup.SetupPlayerHitbox` | PlayerHitbox (ayak izi + gövde) |
| Ekran Kompozisyonunu Kur | `KakScreenSetup` | ScreenComposer, HUD bandı, kontrol alanı, joystick, DungeonFrame |
| Görsel Temeli Kur (Faz 3) | `KakFxSetup.Setup` | taş prefab'ı, FeedbackManager, kamera sarsıntısı, PlayerJuice, Volume |
| Taş Türlerini Kur | `KakContentSetup` | 7 taş türü + kademe dağılımı |
| Sonsuz Mod İçeriğini Kur (Faz 5) | `KakEndlessSetup.Setup` | olaylar, dash, yakın geçiş, combo, afiş, WorldPopup, kademe eşikleri |
| Arayüzü Kur (Faz 4) | `KakUiSetup.SetupAll` | menü ve oyun içi UI (KakUiKit ile) |
| Sesleri Kur | `KakAudioSetup` | AudioManager prefab'ı ve klipler |
| Sprite Atlaslarını Kur / Art Klasörünü Yeniden İçe Aktar | `KakAtlasSetup`, `KakArtImportRules` | atlas, PPU standardı |
| Yayın/Oyuncu Ayarlarını Uygula, Yayın/macOS Development Build | `KakBuild` | PlayerSettings, ölçüm build'i |
| Performans Ayarlarını Uygula | `KakFxSetup.SetupPerformance` | SRP Batcher kapalı + dinamik batching, Mobile URP (ölçek 1, HDR/gölge kapalı), Bloom çeyrek |
| Fontları Statik Atlasa Pişir | `KakFontSetup.Bake` | Nunito/Cinzel statik atlas (ASCII + Türkçe + Loc), TMP varsayılan font |
| Bağlılık Katmanını Kur (Faz 3c) | `KakMetaSetup.Setup` | Coin.prefab, CoinSpawner, HUD altın sayacı, oyun sonu altın satırı |
| Denetim/Sahneleri Denetle | `KakSceneAudit.Run` | eksik script, kopuk referans, boş alan raporu |
| Dev/* | `KakDevMenu` | bot, ölümsüzlük, kalkan ver, `WalkPlayer up/down/left/right/stop`, dünya yazısı, UI panelleri |

Yardımcılar: `KakEditorUtil` (GameScenePath/MenuScenePath, `SaveNamedScenes`, `DeleteAssets "a;b"`, `MoveAssets "a>b;c>d"` (GUID korur, Build Settings'i günceller)).
Eski araçlar (SceneAutoWire, Phase4AutoSetup, KacAtaKacSetup, *Generator, AutoAssign*) D2'de silindi; `[InitializeOnLoad]` ile asset değiştiren kod yok (sadece `KakBridge` ve import kuralları).

## 5. Kod kuralları (mevcut stile uy)

- Yorumlar ve log mesajları **Türkçe**. Log formatı: `Debug.Log("[SinifAdi] mesaj")`.
- `[Header("...")]` ile Inspector grupları; "Data (opsiyonel)" alanı varsa önce data, yoksa Inspector değeri.
- Referans eksikse `FindAnyObjectByType` ile otomatik bul, bulamazsan yarat. Bu proje genelinde bir desen.
- Unity 6 API'si: `rb.linearVelocity` (velocity değil), `FindAnyObjectByType`/`FindObjectsByType`.
- Yeni oynanış parametresi → önce ilgili ScriptableObject'e alan ekle, sonra LevelManager'ın `Apply*` metodunda uygula.
- Mermiler **her zaman** `ProjectilePool` üzerinden; `Instantiate/Destroy` sadece fallback.
- Render: SRP Batcher **kapalı**, dinamik batching açık (sprite'lar birleşsin). Aynı katmandaki sprite'lar için ortak atlas + ortak materyal. Yeni metin karakteri → `KakFontSetup.Bake`.
- Otomasyon/editör aracında **`EditorSceneManager.SaveOpenScenes()` yasak** (adsız sahnede modal pencere → editör kilitlenir). `KakEditorUtil.SaveNamedScenes()` kullan. Editör araçlarında `EditorUtility.DisplayDialog` da yasak (köprüden çağrılınca kilitler); sonucu string döndür/logla.
- Oyuncunun durumu (kalkan, hayalet, yavaşlama) karakteri ölçeklememeli; çarpışma alanı sabit kalır. Görsel geri bildirim ayrı katmanda.
- `Time.timeScale` değiştiren her yer `Time.fixedDeltaTime = 0.02f * Time.timeScale` ile eşlenir.

## 6. Çalışma kuralları (Claude için)

1. **Sahne/prefab YAML'ını elle düzenleme**: Sadece çok küçük ve kesin değişikliklerde. Büyük sahne işleri için kullanıcıya Editor adımlarını ver ya da bir `[MenuItem]` editör aracı yaz.
2. **`.meta` dosyalarına dokunma**. Yeni `.cs` oluştururken meta'yı Unity üretsin.
3. Değişiklikten sonra Unity penceresine geçince derlenir. Hataları `Editor.log`'dan kontrol et.
4. Kullanıcı Türkçe konuşuyor. Açıklamaları Türkçe, sade ve adım adım yap. Inspector'da ne yapacağını tam söyle.
5. Kaydedilmemiş sahne değişikliklerini (`git status`) göz önünde tut. Kullanıcının Editor'deki işini ezme.
6. Git: kullanıcı faz dalları, push ve PR birleştirme için kalıcı izin verdi (2026-09-25). Akış: faz dalı → commit → push → `gh pr create` → `gh pr merge --merge` → `origin/main`'den yeni dal. `.plastic/`, `*.slnx`, dinamik font gürültüsü ve UnityConnectSettings'teki kendiliğinden değişiklikler commit'e girmez.

## 7. Skill'i güncelleme protokolü (ZORUNLU)

Her anlamlı iş sonunda (özellik, hata düzeltme, denge değişikliği, yeni keşif, kullanıcı tercihi):
1. **progress.md** → "Son durum" ve ilgili bölümleri güncelle, bitenleri işaretle, yeni hataları ekle / düzeltilenleri kapat.
2. **learnings.md** → en üste tarihli (YYYY-AA-GG) kısa bir kayıt ekle: ne yapıldı, neden, hangi tuzak öğrenildi.
3. Mimari veya kural değiştiyse **bu dosyada** ilgili bölümü düzelt (eskiyen bilgiyi sil, çelişki bırakma).
4. Kullanıcıya skill'in güncellendiğini tek satırla söyle.
