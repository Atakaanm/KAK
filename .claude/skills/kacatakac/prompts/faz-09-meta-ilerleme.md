# FAZ 9 — Meta İlerleme (Jeton, Mağaza, Karakterler, Yetenekler)

**Süre tahmini:** 3-4 oturum · **Dal:** `faz-9-meta` · **Ön koşul:** Faz 2 (SaveSystem), Faz 4 (UI)

## Amaç
Oyuncunun geri dönmesi için uzun vadeli hedefler. Mevcut kodda hazır duran ama kullanılmayan `AbilityData`, `EquipmentData`, `isLocked` ve `unlockPrice` alanlarını hayata geçirmek.

## Önce kararlar (bana sor)
- Para birimi tek mi (jeton), iki mi (jeton + elmas)? Öneri: tek.
- Uygulama içi satın alma ve reklam olacak mı? (Faz 10'u etkiler.) Öneri: sadece ödüllü reklam (ör. "ikinci şans", "jetonu 2x yap") + reklam kaldırma satın alımı.
- Karakterler sadece görünüm mü, yoksa farklı istatistik ve yetenek de var mı? Öneri: küçük farklar + her birine özgü tek bir yetenek.

## Görevler
1. **Jeton kazanımı:** Sonsuz modda skora göre, bölümde yıldıza göre. Arenada nadiren jeton çıkar (`hasCoins` fikri).
2. **Karakterler:** Boy, Girl + 2-3 yeni. Her biri `PlayerData` + `DirectionalSpriteSet` + `AbilityData`. Seçim ekranında önizleme animasyonu.
3. **Yetenekler (aksiyon butonu, Futbol dışı modlarda):** `Dash` (kısa ölümsüz atılma), `Shield` (bekleme süreli kalkan), `Shockwave` (yakındaki mermileri siler). Bekleme süresi halkası HUD'da.
4. **Ekipman (isteğe bağlı, sonra da yapılabilir):** Kafa, gövde, ayak slotları, küçük istatistik bonusları, görselde basit katman (şapka sprite'ı).
5. **Mağaza ekranı:** Karakterler, yetenek yükseltmeleri (seviye 1-5), kozmetikler. Satın alma onayı, yetersiz jetonda uyarı.
6. **Günlük ödül ve görevler** (isteğe bağlı): "Bugün 3 bölüm bitir", "Sonsuzda 500 puan".
7. Ekonomi dengesi tablosu (`progress.md`): bir karakteri açmak kaç oyun sürer?

## Kabul kriterleri
- Jeton kazan → karakter aç → seç → oyunda farklı yeteneği kullan döngüsü çalışıyor, kayıt kalıcı.
