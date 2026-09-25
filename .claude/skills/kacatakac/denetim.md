# Kapsamlı Denetim — 2026-09-26

> Kullanıcı isteği: "Detaylıca incele, eksiklikleri ve sıkıntıları listele, düzeltmek için yol oluştur, faz faz tek tek yavaş yavaş yap."
> Ardından: **Faz 3c** (bağlılık, görsel kalite, oynanış ve izleme zevki, adım adım açılan özellikler, karakterler, pet'ler, altın). Odak yine Sonsuz Mod.
> Durum: ⬜ yapılmadı · 🔄 sürüyor · ✅ bitti

## Bulgular

### 🔴 Kritik
| # | Bulgu | Etki | Faz |
|---|---|---|---|
| K1 | **Oyuncu çarpışma dairesi gövdenin 2 katı:** yarıçap 0,40, gövde yarı genişliği 0,19. Temas mesafesi 0,66, görsel temas 0,53. | Taş görsel olarak ~0,13 birim uzaktayken vuruyor → "değmedi ki!" hissi, haksız ölümler | D1 |
| K2 | Kalkan, oyuncunun **kök ölçeğini** %15 büyütüyor → çarpışma dairesi de büyüyor | Kalkanlıyken daha kolay vurulmak | D1 |
| K3 | Test botu oyuncu yarıçapını 0,25 sanıyor (gerçek 0,40) | Denge ölçümleri yanlış (bot beklenenden kötü) | D1 |
| K4 | Faz 10 oyuncu ayarları (`ProjectSettings.asset`: paket adı, dikey kilit, IL2CPP) **commit edilmemiş** | Başka makinede/klonda ayarlar yok | D0 |
| K5 | **Unity Version Control (Plastic)** eklentisi kurulu ama kullanılmıyor, loglarda kimlik doğrulama hataları var. Editörü kilitleyen giriş penceresi şüphesi (2026-09-26'da editör 10+ dk kilitlendi) | Otomasyon ve kullanıcı akışı kilitlenir | D0 |
| K6 | **Eski, tehlikeli editör araçları** menüde: Auto-Wire Scene (Ctrl+Shift+W), Fix Sorting Orders, Fix Player Visibility, "Faz 4 Hatalarını Düzelt", Generate Difficulty Stages / Powerups, Setup Default Data | Yanlışlıkla çalıştırılırsa kurulumu, dengeyi ve powerup'ları eski hale döndürür | D2 |
| K7 | `AutoAssignPowerups` her derlemede sessizce çalışıp powerup'ı boş LevelData'ları dolduruyor; `ButtonGenerator` / `JoystickSpriteGenerator` eski görselleri yeniden üretiyor | Bölümler (Faz 6+) bozulur, gereksiz dosyalar | D2 |

### 🟠 Yüksek
| # | Bulgu | Faz |
|---|---|---|
| Y1 | Kalkan görseli karakteri düz camgöbeğine boyuyor (palet dışı, karakter okunmuyor). Hayalet sadece yarı saydam. Kalkan balonu olmalı. | D1 |
| Y2 | Aktif powerup'ların kalan süresi HUD'da görünmüyor (kalkan var mı? hız ne kadar kaldı?) | D4 |
| Y3 | İlk oyunda öğretici yok (sürükle, dash, yakın geçiş anlatılmıyor) | D4 → 3c |
| Y4 | Powerup'lar `Instantiate`/`Destroy` (havuz yok), `FloatingItem` her kare ölçek değiştiriyor | D3 |
| Y5 | Performans: 82 batch (hedef < 60), Universal Renderer'da sprite birleşmesi yok. 2D Renderer denemesi gerekli (Karanlık dünya için de şart) | D3 |
| Y6 | TMP fontları dinamik: `FontWarmup` çalışma zamanında glif ekliyor → font dosyaları editörde sürekli değişiyor (git gürültüsü), ilk açılışta maliyet. Statik atlas (TR+EN) daha doğru | D3 |
| Y7 | Kullanılmayan paketler: Visual Scripting, AI Navigation, Multiplayer Center (+ Plastic) → derleme süresi, build boyutu | D0 |
| Y8 | Sahne bağlantılarını denetleyen otomatik test yok (kopuk referans, boş alan) | D5 |
| Y9 | Faz 2B yarım (kod var, sahne/UI/içerik yok). `LevelData`'da kullanılmayan alanlar (hasKey, hasCoins, hasBoss, isLocked, unlockPrice, targetScore, timeLimit, waves + WaveManager) | 3c sonrası |

### 🟡 Orta
| # | Bulgu | Faz |
|---|---|---|
| O1 | Denge ölçümü K3 yüzünden yanlış → düzeltme sonrası yeniden ölç | D5 |
| O2 | Eski Input (`Input.GetAxisRaw`, `GetKeyDown`) + Input System birlikte ("Both") | ileride |
| O3 | Ölü dosyalar: `FloatingText.cs`, `CameraFitWidth.cs` (yedek), `TutorialInfo/` (Unity şablonu), eski UI sprite'ları (GoldPill, PremiumGoldButton, PremiumPill, MenuBackground_HD, eski JoystickBG/Handle) | D2 |
| O4 | Oyun sahnesinin adı hâlâ şablon adı "SampleScene" | D2 |
| O5 | IDE dosyası `KacAtaKac.slnx` ve `.plastic/` git'te | D0 |
| O6 | Coroutine döngülerinde `new WaitForSeconds` (PlayerHealth, EndlessEventManager), olay seçiminde `new List` → küçük tahsisler | D3 |
| O7 | `PlayerMovement2D.FixedUpdate` tembel `GetComponent<PlayerStatus>` (bileşen yoksa her adımda) | D3 |
| O8 | Kalkan kodu `TakeDamage` ve `ConsumeShield` içinde kopya | D1 |
| O9 | Sesler dinlenmeden üretildi, müzik döngüleri kısa (14-20 sn) | 3c (kullanıcı geri bildirimi) |
| O10 | `UnityConnectSettings.m_Enabled` kendiliğinden 1 oldu (Unity servisleri) | D0 |
| O11 | Menüde KARAKTER paneli işlevsiz (tek karakter), BÖLÜMLER kilitli | 3c |

### ⚪ Düşük
- Birleştirilmiş uzak dallar (`faz-*`) duruyor
- README'nin bazı bölümleri eski
- `Kalp.png` 296 KB (küçültülebilir)
- HUD'da kademe göstergesi yok (sadece geçiş afişi)

## Düzeltme yol haritası (sırayla, her fazın sonunda test + commit)

| Faz | İçerik | Doğrulama |
|---|---|---|
| **D0 Hijyen** | Plastic eklentisini ve `.plastic/`'i kaldır, `.slnx` git dışı, kullanılmayan paketleri kaldır, ProjectSettings'i commit et, UnityConnect kararı | Derleme temiz, testler yeşil, editör kilitlenmiyor |
| **D1 Adil çarpışma** | Oyuncu yarıçapı ~0,24 (temas < görsel), kalkan ölçek yerine balon efekti, hayalet görünümü, bot yarıçapı, kopya kod | Yeni test: görsel temas mesafesi ≥ çarpışma mesafesi; bot testi |
| **D2 Temizlik** | Eski editör araçları ve InitializeOnLoad jeneratörleri kaldırılır; ölü dosyalar; sahne adı "Game" | Menüde sadece güncel araçlar, testler yeşil |
| **D3 Performans** | Powerup havuzu, coroutine tahsisleri, GetComponent önbelleği, statik font atlası, 2D Renderer deneyi + build ölçümü | Build bench: batch, GC, en kötü kare |
| **D4 Oyuncu bilgisi** | HUD'da aktif powerup ikonları ve süre halkaları, kalkan/hayalet görselleri, ilk oyun ipuçları | Ekran görüntüleri, UI testleri |
| **D5 Doğrulama** | Sahne denetimi testi (EditMode), yeniden denge ölçümü, tam regresyon | Tüm testler yeşil |
| **D6 Belgeler** | README, skill, yol haritası | — |
| **Faz 3c** | Derin araştırma → bağlılık ve meta ilerleme (altın, karakterler + istatistikler + kilitler, pet'ler, adım adım açılan özellikler, görevler/ödüller), görsel kalite ve his cilası. **Odak: Sonsuz Mod** | Ayrı plan (3c.md) |
| sonra | Faz 2B'yi bitir → 6 Buz → 7 Futbol → 8 Karanlık | — |
