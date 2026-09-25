# FAZ 2 — Mimari Genişletme (Çok Modlu Oyuna Hazırlık)

**Süre tahmini:** 3-5 oturum · **Dal:** `faz-2-mimari` · **Ön koşul:** Faz 1 bitti

## Amaç
Kodu "4 köşeden taş atan fırlatıcı" varsayımından kurtarıp **farklı arenalar, düşmanlar, mermiler ve kazanma koşulları** eklenebilecek hale getirmek. Faz sonunda oyun dışarıdan **aynı** görünmeli: Sonsuz Mod bire bir çalışmalı. Ama altyapı Buz, Futbol ve Karanlık dünyalarını kaldırabilmeli.

## ⭐ Kapsam: Sonsuz öncelikli plan
Güncel sıra (`roadmap.md` §4) gereği bu faz **ikiye bölündü**:

| Şimdi (Faz 2A — Sonsuz'un ihtiyacı) | Sonra (Faz 2B — bölüm dünyalarından önce) |
|---|---|
| 2.1 GameEvents | 2.3 Düşman sistemi (hareket desenleri) — Sonsuz'da sadece `Stationary` + `Aimed/Predictive/Spread/Burst` saldırı desenleri şimdi yazılır |
| 2.2 Tek animatör | 2.6 Arena prefab ve `EnemyPlacement` (Faz 1.5'teki `ArenaTileSet` + `SpawnPoint` zaten temel olur) |
| 2.4 Mermi hareket/etki + 2.5D yükseklik (göktaşı, seken ve parçalanan taş için şart) | 2.7 GameMode sistemi (Sonsuz şimdilik tek mod; `GameManager` → `EndlessSurvivalMode` sarmalaması yeterli) |
| 2.5 Durum efektleri (Shield/Ghost/Speed/SloMo taşıması + Dash'in ölümsüzlüğü) | 2.9 WorldData, bölüm verisi |
| 2.8 SaveSystem | |

Faz 2A'da 2B'ye ait bir şeyi önceden yazma. Sadece arayüzleri kapatmayacak şekilde tasarla (ör. `AttackPattern` SO'su sonradan yeni desenlerle genişleyebilmeli).

**Performans notları:** Olay sistemi C# `event Action<T>` (UnityEvent değil, GC'siz). Mermi hareket stratejileri `Update` başına tahsis yapmamalı. `ProjectileMotion` her mermide `new` ile değil, SO üzerinden paylaşılan, durumsuz strateji olarak çalışmalı (durum mermide tutulur).

## İlke
Kalıtımdan çok **bileşen + veri**. Yeni bir düşman ya da mod çoğunlukla yeni bir SO ve prefab ile eklenmeli. Her adımdan sonra Sonsuz Mod çalışır durumda olmalı (büyük patlama yok, adım adım taşıma).

## Görevler (bu sırayla, her biri ayrı commit)

### 2.1 Olay sistemi — `GameEvents`
- Statik C# event'leri: `PlayerDamaged(int)`, `PlayerHealed`, `PlayerDied`, `ScoreChanged(int)`, `PowerupCollected(PowerupData)`, `GameStarted`, `GameEnded(GameResult)`, `StageChanged(string)`, `GoalScored(int team)`. Sahne değişiminde `ClearAll()`.
- Ses, UI ve efekt birbirini `FindAnyObjectByType` ile aramak yerine olay dinlesin. Önce AudioManager ve HealthUI'ı taşı.

### 2.2 Tek animatör — `DirectionalSpriteAnimator`
- `PlayerDirectionSprite` ve `SpawnerDirectionAnimator` tek bileşende birleşsin: durumlar `Idle / Run / Attack / Hurt / Special`.
- Görsel verisi yeni bir SO'da: `DirectionalSpriteSet` (yön başına kareler + fps). **5 yön + aynalama** desteği olsun (W, NW ve SW için flipX).
- Yön kaynağı arayüzle soyutlansın: `IFacingSource` (hareket yönü veya hedefe bakış).
- Eski bileşenler geçiş süresince `[Obsolete]` olarak kalsın. Sahnedekileri yeni bileşene çeviren bir editör aracı yaz.

### 2.3 Düşman sistemi — `EnemyData` + `Enemy`
- `EnemyData` (SO): ad, `DirectionalSpriteSet`, `MovementPattern`, `AttackPattern`, `ProjectileData`, ateş aralığı, **telegraph süresi** (ateşten önceki uyarı: parlama veya animasyon), can (ileride vurulabilir düşman için).
- `MovementPattern` (SO, strateji deseni): `Stationary`, `PatrolPath` (waypoint'ler arasında), `SideLine` (kenar çizgisinde gidip gelme — futbolcular için), `Chase`, `Wander`.
- `AttackPattern` (SO): `Aimed` (oyuncuya), `Predictive` (oyuncunun gideceği yere), `Spread(n, açı)`, `Burst(n, aralık)`, `Spiral`, `Lob` (kavisli, hedef noktada gölge uyarısı).
- `Enemy` bileşeni = `EnemyMover` + `EnemyAttacker` + `DirectionalSpriteAnimator`. `CornerShooter` bunun "Stationary + Aimed" özel hali olarak yeniden yazılsın. Mevcut `RockThrower` bire bir aynı davransın.

### 2.4 Mermi sistemi genişletme
- `ProjectileData`'ya ekle: `ProjectileMotion` (Straight, Homing(güç, süre), Bounce(n), Arc(yükseklik)), `ProjectileEffect` listesi, `impactVfx`, `trail`, `spinVisual`, `shadow` (Arc için yerde gölge).
- `ProjectileEffect` (SO): `Damage(n)`, `ApplyStatus(StatusEffectData)`, `Knockback(güç)`.
- 2.5D yükseklik: `Projectile`'a `height` (görsel Z) ekle. Görsel child yukarı kayar, gölge yerde kalır. Çarpışma sadece `height` sıfıra yakınken olur. Gökten düşen taş ve lob atış bunun üzerine kurulacak.

### 2.5 Durum efektleri — `StatusEffectData` + `PlayerStatus`
- `Slow(%)`, `Stun(sn)`, `Freeze`, `SlipperyOverride`, `CardWarning` (sarı kart sayacı: 2 sarı = kırmızı). Yığılma kuralı (yenile, üst üste ekle, yok say) SO'da seçilsin.
- Oyuncunun üstünde küçük ikon ve renk tonu. Mevcut Shield, Ghost ve Speed buraya taşınsın, `PlayerHealth` içindeki renk karmaşası tek yerde çözülsün.

### 2.6 Arena prefab'ı ve oynanabilir alan
- `ArenaData.arenaPrefab` gerçek kullanıma girsin. Prefab içinde: zemin sprite'ı, `PlayableArea` (BoxCollider2D veya Rect), duvarlar, `SpawnPoint` işaretçileri (id ve yön), isteğe bağlı `Goal` alanları, ışıklar, dekor.
- `LevelData` spawner listesi yerine `EnemyPlacement[]` kullansın (`EnemyData` + `spawnPointId`). Böylece 4 köşe zorunluluğu kalkar.
- `ArenaData`'ya zemin fizik ayarları: `acceleration`, `deceleration`, `maxSpeedMultiplier`, `slideFactor` (buz için hazırlık; mevcut `floorFriction` bunlara dönüşsün).

### 2.7 Oyun modu — `GameModeData` + `GameModeController`
- `GameModeData` (SO): `modeType`, kazanma koşulları (`surviveSeconds`, `targetScore`, `goalsToWin`), kaybetme koşulları (can bitti, süre doldu, kırmızı kart), yıldız eşikleri (1-3), ödül.
- `GameModeController` (soyut) → `EndlessSurvivalMode` (mevcut davranış), `TimedSurvivalMode` (Stage/dalga), `FootballMode` (sadece iskelet, Faz 7'de doldurulacak).
- `GameManager.GameOver/LevelComplete` → `GameModeController.EndGame(GameResult)`. `GameResult`: kazandı/kaybetti, skor, süre, yıldız, toplanan jeton.
- `WaveManager` → `TimedSurvivalMode` içinde kullanılsın. `LevelComplete` yorumu açılsın.

### 2.8 Kayıt sistemi — `SaveSystem`
- `Application.persistentDataPath/save.json`: `bestScoreEndless`, bölüm başına `{yıldız, enİyiSkor, kilitAçık}`, `coins`, açık karakterler, seçili karakter, ayarlar (müzik, efekt, titreşim).
- Mevcut `PlayerPrefs` değerlerini ilk açılışta içe aktar (migration). `GameSettings.BestScore` ve `AudioManager` yeni sisteme bağlansın.

### 2.9 Dünya ve bölüm verisi
- `WorldData` (SO): ad, ikon, tema rengi, `LevelData[]`, açılma koşulu (önceki dünyada X yıldız).
- `LevelData`'ya ekle: `GameModeData`, `EnemyPlacement[]`, `introText`. Artık kullanılmayan alanlar (`hasKey` vb.) temizlensin ya da `GameModeData`'ya taşınsın.

## Regresyon testi (her alt görevden sonra)
Faz 1 test listesinin 1-3. maddeleri + "RockThrower'ların ateş hızı, taş hızı ve zorluk kademeleri öncekiyle aynı hissettiriyor mu?"

## Kabul kriterleri
- Sonsuz Mod önceki haliyle aynı oynanıyor.
- Aynı arenada kod yazmadan, sadece yeni `EnemyData` + `LevelData` ile **"2 köşe fırlatıcı + kenarda devriye gezen, 3'lü yelpaze atan 1 düşman, 60 sn hayatta kal"** tipinde bir test bölümü kurulabiliyor. Bunu bir `Test_Level` olarak ekle.
- `SKILL.md`'deki mimari haritası yeni yapıya göre yeniden yazıldı.
