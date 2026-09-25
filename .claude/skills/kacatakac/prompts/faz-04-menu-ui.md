# FAZ 4 — Menü ve UI Yeniden Tasarımı

**Süre tahmini:** 3-4 oturum · **Dal:** `faz-4-ui` · **Ön koşul:** Faz 2 (SaveSystem, WorldData), Faz 3 (sanat standardı)

## Amaç
Mağaza sayfasına konabilecek kalitede, akıcı ve tutarlı bir arayüz. Şu an menüde sadece "Oyna" butonu var.

## ⭐ Kapsam: Sonsuz öncelikli plan
v1.0 sadece Sonsuz Mod. Bu fazda: **açılış, ana menü, karakter seçimi (Boy/Girl), ayarlar, oyun içi HUD, pause, oyun sonu ekranı**. "Bölümler" butonu menüde durur ama "Yakında" rozetiyle kilitli. Dünya seçimi ve bölüm haritası (4.3) bölüm dünyaları fazına ertelendi.

**Renk ve stil:** Tüm UI `sanat-rehberi.md` paletinden gelir. UI vurgu rengi altın/krem, tehlike ve uyarı sıcak, olumlu durumlar camgöbeği. UI'ın taş/metal levha dili Faz 1.5'teki HUD bandı ve koridor ile aynı dünyaya ait olmalı. Mevcut `GoldPill`, `PremiumGoldButton` ve Cinzel/Nunito fontlarının piksel sanat ve paletle uyumunu değerlendir, uymuyorsa yenisini öner (piksel font + 9-slice piksel butonlar).

**Performans:** Canvas'ları statik ve dinamik diye ayır (sık değişen skor ayrı Canvas). Kullanılmayan `Raycast Target`'ları kapat. Menü arka planındaki canlı arena, oyun sahnesinin hafifletilmiş hali olsun (az mermi, ışık bütçesi).

## Ekran akışı
```
Açılış (logo + yükleme)
  → Ana Menü ─┬─ SONSUZ MOD → oyun
              ├─ BÖLÜMLER → Dünya Seçimi → Bölüm Haritası (yıldızlar, kilitler) → Bölüm Önizleme → oyun
              ├─ KARAKTERLER → karakter seçimi (kilitli/açık, istatistik, fiyat)
              ├─ MAĞAZA (Faz 9'a kadar "yakında")
              └─ AYARLAR (müzik, efekt, titreşim, ekran sarsıntısı, dil, sıfırla)
Oyun içi: HUD · Pause · Game Over / Bölüm Sonu (yıldız animasyonu, skor, rekor, jeton, Tekrar / Menü / Sonraki)
```

## Görevler

### 4.1 UI altyapısı
- `UIScreen` temel sınıfı (Show/Hide, giriş/çıkış animasyonları, geri tuşu davranışı) + `UIRouter` (ekran yığını, Android geri tuşu).
- Coroutine ya da basit bir tween yardımcısı (DOTween eklenmek istenirse bana sor): fade, slide, scale-pop, stagger.
- Sahne geçişi: `SceneLoader` → kararma veya piksel "iris" geçişi + asenkron yükleme.
- **Safe Area** bileşeni (çentikli telefonlar). Canvas Scaler: referans çözünürlük Faz 0 kararına göre.
- Piksel font (TMP SDF, Point filtre) + başlık fontu. Mevcut Cinzel/Nunito piksel sanatla uyumlu mu, bana sor.

### 4.2 Ana menü
- Arka planda **canlı arena**: yavaş taşlar uçuşur, meşaleler titrer. Menü, oyunun kendisini gösterir.
- Logo animasyonu, büyük "OYNA" (Sonsuz) butonu, altında "BÖLÜMLER", küçük ikon butonlar (karakter, ayarlar), en iyi skor, jeton.
- Tüm butonlarda basma animasyonu (mevcut `ButtonScaleAnimation`) + ses + hafif titreşim.

### 4.3 Bölüm haritası
- Dünya başına kaydırılabilir yol haritası, düğümlerde bölüm numarası, 0-3 yıldız, kilit. Dünyanın tema renkleri (Buz mavi, Futbol yeşil, Karanlık mor).
- Bölüm önizleme paneli: hedef ("60 sn hayatta kal", "3 gol at"), düşman ikonları, yıldız koşulları.

### 4.4 Oyun içi HUD
- Kalpler (mevcut animasyonlar korunur), skor (sayarak artar, rekoru geçince parlar), mod hedefi (süre çubuğu, gol sayacı), aktif durum efektleri (ikon + geri sayım halkası), pause butonu.
- Zorluk kademesi geçişinde ekran ortasında kısa banner ("ZOR!", `GameEvents.StageChanged`).

### 4.5 Oyun sonu ekranı
- Sıralı animasyon: panel gelir → skor sayılır → rekorsa "YENİ REKOR!" ve konfeti → yıldızlar tek tek dolar → jeton sayılır → butonlar belirir.
- Sonsuz modda istatistik: süre, alınan powerup, yakın geçiş sayısı.

## Test
- 3 farklı en-boy oranında (16:9, 19.5:9, 4:3 tablet) tüm ekranlar düzgün mü, çentik güvenli mi?
- Tüm akış: Menü → Bölümler → Bölüm 1 → kazan → Sonraki → kaybet → Tekrar → Menü → Ayarlar → Sonsuz.

## Kabul kriterleri
- Tüm ekranlar aynı görsel dili konuşuyor, animasyonlu geçişler var, Android geri tuşu doğru çalışıyor.
