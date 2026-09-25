# KaçAtaKaç — Öğrenme Günlüğü

> En yeni kayıt en üstte. Format: `## YYYY-AA-GG — başlık`, sonra kısa maddeler.
> Kaydedilecekler: kararlar ve gerekçeleri, keşfedilen tuzaklar, kullanıcının tercihleri/geri bildirimleri, işe yarayan/yaramayan yaklaşımlar.
> Kod veya git geçmişinden zaten okunabilecek şeyleri tekrar yazma.

## 2026-09-25 — Vizyon netleşti, yol haritası ve faz promptları yazıldı

- Kullanıcının vizyonu: Sonsuz mod (sadece taş, gittikçe zor) + bölüm bölüm dünyalar: Buz (kayma, gecikmeli durma), Futbol (topla gol at, futbolcular sarı/kırmızı kart atar), Karanlık arena ve dahası. "Görüntü aşırı iyi olsun, basit ama süper."
- Kullanıcının şikayetleri: item'lar ekranın başka yerinde çıkıyor, bölüm tasarımı yok, menü "patates".
- Görsel incelemesi: Arena üstten bakış yapay zeka üretimi piksel sanatı (sağ altta ✦ filigranı). Karakter 48×48, spawner 48px ama 4x ölçekli. Piksel yoğunlukları tutarsız, filtre Bilinear. URP Universal Renderer kullanılıyor, 2D Renderer değil.
- Önerilen sanat yönü: disiplinli piksel sanatı (32px, tek PPU, palet, 5 yön + aynalama) + 2D ışık + juice. Karar Faz 0'da kullanıcıda. Alternatif: low-poly 3D.
- 11 fazlık plan (0-10) `roadmap.md` ve `prompts/` altında. Kritik sıra: Faz 1 (hatalar) → Faz 2 (mimari: Enemy, GameMode, ProjectileData etkileri, Arena prefab, Save, Events). Bunlar yapılmadan yeni dünya eklenmemeli.

## 2026-09-25 — Skill oluşturuldu, ilk tam okuma

- Kullanıcı (Atakaan) oyunu Unity'de geliştiriyor ve Claude ile Türkçe çalışıyor. İstediği: Claude projeyi baştan sona anlasın, zamanla öğrendiklerini bu skill'e kaydederek gelişsin.
- Tüm `Assets/Scripts` (~40 dosya), veri dosyaları, sahne obje listesi, git geçmişi okundu. Editor.log'da derleme hatası yok.
- Sahne–script eşlemesi (SampleScene): `GameManager` objesinde GameManager + ScoreManager; `Player`'da PlayerMovement2D + PlayerHealth; `Player/Visual`'da PlayerDirectionSprite + YDepthSorter; `ArenaRoot`'ta ArenaAutoLayout; `Main Camera`'da CameraFitWidth; 4 köşe spawner'da CornerShooter, altındaki `Visual`'larda SpawnerDirectionAnimator; `HealthPanel`'de HealthUI; `JoystickHandle`'da VirtualJoystick; ayrı `PauseManager` objesi. **LevelManager, DifficultyManager ve ProjectilePool sahnede yok**, runtime'da yaratılıyorlar.
- Sahne→script eşlemesini hızlıca çıkarmak için yöntem: `.cs.meta` dosyalarından guid → sınıf adı tablosu kur, `.unity` YAML'ında `--- !u!114` (MonoBehaviour) bloklarının `m_Script guid` ve `m_GameObject fileID` değerlerini `--- !u!1` (GameObject) `m_Name` ile eşle. Python ile 20 satır.
- Görsellerin asıl kaynağı şu an sahnedeki bileşenler. Veri dosyalarındaki sprite/prefab alanlarının çoğu boş, kod da boşsa sahneyi koruyacak şekilde yazılmış.
- Kritik şüphe: Retry akışı `GameManager.DontDestroyOnLoad` yüzünden bozuk olabilir (ayrıntı progress.md'de). Kullanıcıya bildirildi, henüz düzeltilmedi.
