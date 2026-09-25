# FAZ 1 — Stabilizasyon (Hataları Kapat, Temeli Sağlamlaştır)

**Süre tahmini:** 1-2 oturum · **Dal:** `faz-1-stabilizasyon`

## Amaç
Sonsuz Mod'un menüden başlayıp ölüp tekrar oynanabildiği, item'ların arenanın doğru yerinde çıktığı, duvarların görselle hizalı olduğu **hatasız** bir temel. Yeni özellik yok, sadece düzeltme.

## Bağlam (kod incelemesinden)
- `GameManager` `DontDestroyOnLoad` ve `ScoreManager` aynı objede. Retry veya menü dönüşünde eski örnek `isGameOver=true` ile yaşıyor, yenisi yok ediliyor. `LevelManager` ve `DifficultyManager` sahnede yok, GameManager.Awake'te runtime'da yaratılıyor. Retry sonrası hiç yaratılmıyorlar.
- Sahne doğrudan Play'e basılınca `LevelManager.defaultLevel` boş kalıyor, bu yüzden LevelData uygulanmıyor.
- Duvarlar `arenaSpriteRenderer.bounds` dış kenarından 0.15 birim içeride. Görseldeki iç duvar yaklaşık 0.6 birim içeride.
- Kullanıcı bildirimi: **"Item görselleri ekranın başka yerinde çıkıyor."**

## Görevler

### 1.1 Yaşam döngüsü ve Retry
- `GameManager`'dan `DontDestroyOnLoad`'ı kaldır. Sahneye özel singleton olsun (`OnDestroy`'da `Instance = null`).
- Sahneler arası kalıcı olan sadece `AudioManager` ve statik `GameSettings`/`SaveSystem` olsun.
- `LevelManager`, `DifficultyManager`, `ProjectilePool`, `PowerupSpawner`, `WaveManager` için bir editör aracı yaz: `KacAtaKac/Sahne Yöneticilerini Kur`. Bu araç eksik yöneticileri `Managers` adlı bir objenin altına kalıcı olarak eklesin ve referanslarını bağlasın. `LevelManager.defaultLevel = Endless_Level1_LevelData` atansın.
- Runtime'da "yoksa yarat" mantığı sadece yedek olarak kalsın ve `Debug.LogWarning` versin.
- Tüm singleton'larda `OnDestroy → if (Instance == this) Instance = null;`
- `Projectile` statik `boundsInitialized` alanı sahne başında sıfırlansın (`ResetBounds` LevelManager.Awake'te çağrılsın).
- `SceneLoader` her geçişte `Time.timeScale = 1` ve `Time.fixedDeltaTime = 0.02f` ayarlasın. `PauseManager` `fixedDeltaTime = 0` yapmasın.

### 1.2 Item (powerup) konum hatası
- **Önce teşhis et, sonra düzelt.** Play modunda `[PowerupSpawner] Sınırlar ... alındı` log'unu ve spawn pozisyonlarını bana okut ya da Editor.log'dan oku. Arena ve duvar dünya koordinatlarıyla karşılaştır.
- Olası nedenler: yanlış sınır kaynağı, prefab'ın kök ve child pivotu, ölçek, `FloatingItem` başlangıç Y'si, retry'dan kalan eski spawner. Nedeni bulup `learnings.md`'ye yaz.
- Kalıcı çözüm için **oynanabilir alan kavramı** getir. `ArenaData`'ya `Rect playableAreaNormalized` ekle (sprite içinde 0-1 aralığında iç zemin, Dungeon için yaklaşık x:0.075-0.925, y:0.075-0.925; görselden ölçüp doğrula). Duvar colliderları, powerup spawn alanı, mermi sınırları ve oyuncu başlangıcı tek kaynaktan, `ArenaAutoLayout.PlayableWorldRect`'ten hesaplansın.
- Editörde `OnDrawGizmos` ile oynanabilir alanı yeşil dikdörtgen olarak çiz, hizalamayı gözle doğrulayabileyim.

### 1.3 Küçük hatalar
- Ghost ve Speed süreleri: oyun zamanı mı gerçek zaman mı olmalı, bana sor. Öneri: SloMo sırasında uzamasınlar, `WaitForSecondsRealtime` kullanılsın.
- `VirtualJoystick` `JoystickHandle` üzerinde. `JoystickBG`'ye taşı, dokunma alanını genişlet. İsteğe bağlı: ekranın sol yarısına dokununca joystick orada belirsin (floating joystick). Bunu bana sor.
- `DifficultyManager` spawner sırası rastgele. Köşeleri deterministik sırala (ör. SolAlt, SağÜst, SolÜst, SağAlt; köşegenler önce açılsın ki oyuncu çapraz ateş alsın).
- Game Over'da `AudioManager.PlayDeathSfx()`, vuruşta `PlayHitSfx()` çağrılmıyor, bağla. Kliplerin atanıp atanmadığını kontrol et, boşsa bana söyle.
- `ShieldData.duration` kullanılmıyor. Ya kalkana süre ekle ya alanı "0 = vurulana kadar" olarak belgele. Bana sor.
- README'yi gerçek duruma göre güncelle (Unity 6, dosya adları, yol haritası linki).

### 1.4 Tanı aracı
- `KacAtaKac/Diagnose Scene` aracını genişlet: eksik referans, boş Data alanı, yanlış filtre modu (Bilinear piksel sprite) ve tag eksikliği (`Player`, `Wall`) raporlasın.

## Test listesi (bana ver, sonuçlarını iste)
1. MainMenu'den Oyna → 60 sn oyna → skor artıyor mu, zorluk log'ları geliyor mu?
2. Öl → Game Over paneli, skor ve en iyi skor doğru mu? → **Tekrar Dene** → skor 0'dan artıyor mu, powerup çıkıyor mu? → tekrar öl → panel geliyor mu?
3. Game Over → Menü → Oyna → aynı kontroller.
4. SampleScene'i doğrudan aç → Play → oyun LevelData ile başlıyor mu?
5. 20 powerup çıkana kadar bekle: hepsi yeşil oynanabilir alanın içinde mi?
6. Duvarlara yürü: karakter görsel duvarın hizasında duruyor mu?
7. SloMo al → ölmeden bitmesini bekle → fizik normal mi? SloMo'dayken öl → Retry → hareket normal mi?
8. Pause → Resume → Pause → Menü.

## Kabul kriterleri
- Yukarıdaki 8 testin hepsi geçti ve Console'da kırmızı hata yok.
- `progress.md`'de 🔴 ve 🟠 hatalar ✅ olarak işaretli, nedenleri `learnings.md`'de.
