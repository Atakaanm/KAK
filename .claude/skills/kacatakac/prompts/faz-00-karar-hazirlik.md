# FAZ 0 — Karar ve Hazırlık

**Süre tahmini:** 1 oturum · **Dal:** gerekmez (main'de düzen)

## Amaç
Kodlamaya başlamadan önce sanat yönünü ve çalışma düzenini kesinleştir, mevcut yarım işi güvene al.

## Görevler

1. **Mevcut işi kaydet**
   - `git status` ile bak. `SampleScene.unity`'deki commit edilmemiş büyük değişikliğin ne olduğunu bana sor. Gerekirse `git diff --stat` ve obje isimleriyle özetle.
   - Onayımla commit et: `chore: mevcut sahne düzenlemeleri kaydedildi`.
   - `.claude/` klasörünü (skill ve promptlar) ve `CLAUDE.md`'yi commit et.
   - `.gitignore`'u kontrol et: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.slnx`, `.vscode/` hariç tutulmalı. `.plastic/` klasörünü git'e koymak gerekip gerekmediğini bana sor (Plastic kullanılmıyorsa kaldırılmalı).

2. **Sanat yönü kararı** (bana soru olarak sor, `roadmap.md` §3'teki tabloyu göster)
   - A) Disiplinli piksel sanatı (öneri) · B) Low-poly 3D · C) Yüksek çözünürlüklü 2D
   - A seçilirse şunları da kararlaştır:
     - Karakter boyutu: **32×32** (öneri) ya da 48×48
     - PPU (Pixels Per Unit): karakter boyutuyla aynı (32), böylece 1 karakter = 1 dünya birimi
     - Referans çözünürlük (Pixel Perfect Camera): dikey mobil için örneğin **360×640**
     - Palet: hazır bir palet (örneğin Lospec'ten "Endesga 32" veya "Resurrect 64")
     - Yön sayısı: 8 yön yerine **5 çizim + aynalama** (S, SE, E, NE, N; W tarafı flipX)
   - Kararları `learnings.md`'ye ve `SKILL.md`'ye "Sanat Standardı" bölümü olarak yaz.

3. **Ekran yönü kararı:** Dikey (portrait, öneri: tek elle oynanır) mı, yatay mı? Futbol modu yatay daha rahat olabilir. Karar tüm UI'ı etkiler, şimdi sor.

4. **Hedef cihaz:** Önce Android mi iOS mu? Test cihazım ne? Kaydet.

## Kabul kriterleri
- `git status` temiz, `main` GitHub'a pushlanmış (onayımla).
- Sanat standardı, ekran yönü ve hedef platform `SKILL.md`'de yazılı.
