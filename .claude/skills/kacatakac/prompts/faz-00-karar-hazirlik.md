# FAZ 0 — Karar, Hazırlık ve Otonom Çalışma Altyapısı

**Süre tahmini:** 1 oturum · **Dal:** `main` üzerinde düzen, sonra `faz-0-altyapi`

## Amaç
Uzun süre kesintisiz çalışabilmek için gerekenleri kurmak: temiz git, kesin kararlar ve **Claude'un Unity'yi kendi başına test edebilmesi**. Otomatik test olmadan otonom çalışmada her adım benim elle test etmemi bekler, ilerleme durur.

## Alınmış kararlar (tekrar sorma)
- Öncelik: **Sonsuz Mod**. v1.0 sadece Sonsuz ile yayınlanacak.
- **Dikey ekran.** Kompozisyon: HUD bandı + kare arena (ekran enine ölçekli) + koridor/kontrol alanı (`sanat-rehberi.md` §5).
- Renk uyumu en önemli kalite kriteri.

## Görevler

### 0.1 Git düzeni
- `git status`: `SampleScene.unity`'deki commit edilmemiş değişikliği özetle (`git diff --stat` + değişen obje adları), ne olduğunu bana sor, onayımla commit et.
- PR #1 (skill dosyaları) birleştirilmediyse bana hatırlat.
- `.gitignore`: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `*.csproj`, `*.sln*`, `.vscode/`, `.DS_Store` hariç tutulmalı. `.plastic/` ve `ignore.conf`: Plastic kullanılmıyorsa kaldırılmasını öner (bana sor).
- Büyük ikili dosyalar için Git LFS gerekli mi (görseller, sesler)? Repo boyutunu ölç, öner.

### 0.2 Otonom test altyapısı (en kritik)
Seçenekleri değerlendir, önerini sun ve kurulumda bana adım adım rehberlik et:
1. **Unity MCP sunucusu** (ör. `CoplayDev/unity-mcp` veya `CoderGamester/mcp-unity`; güncel sürümünü ve Unity 6 uyumunu kontrol et): Claude'un Play moduna girmesi, Console'u okuması, derlemeyi tetiklemesi, Game view ekran görüntüsü alması ve sahne objelerini sorgulaması için. **Öneri: kur.**
2. **Unity Test Framework** (paket zaten kurulu): `Assets/Tests/EditMode` ve `Assets/Tests/PlayMode` assembly'leri. Kritik akışlar için otomatik testler: sahne yükleme, Retry döngüsü, skor artışı, zorluk kademesi geçişi, havuz sızıntısı yok, powerup'ın oynanabilir alan içinde doğması.
   - Unity kapalıyken komut satırından çalıştırma: `"/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app/Contents/MacOS/Unity" -batchmode -projectPath /Users/atakaan/KacAtaKac -runTests -testPlatform PlayMode -testResults <yol>.xml -logFile <yol>.log`
3. **Ekran görüntüsü aracı:** `KacAtaKac/Ekran Görüntüleri Al` editör aracı (ya da Play modunda çalışan bir bileşen). Belirli oranlarda (1080×1920, 1170×2532, 1080×2400, 1536×2048) Game view görüntüsünü `Screenshots/` klasörüne kaydeder, ayrıca gri tonlama ve renk körlüğü simülasyonlu kopyalarını üretir. Claude bu görselleri okuyup kendi değerlendirir.
4. **Otomatik oyuncu (bot) testi:** Play modunda rastgele ya da kaçma algoritmalı bir bot N dakika oynar. Ortalama hayatta kalma süresi, hata ve FPS raporu çıkarır. Denge ve regresyon için kullanılır.

### 0.3 Çalışma izinleri (bana sor, kaydet)
- Faz dallarına Claude kendi push edebilir mi? (Öneri: evet, `main`'e asla.)
- "Otonom devam" modunda faz planını onaysız başlatabilir mi?
- Hedef test cihazı ve platform (Android mi iOS mu, model?).

### 0.4 Sanat stili onayı
- `sanat-rehberi.md` §1-4'ü bana özetle. Piksel sanatı (32 px, PPU 32) onayını al. Onay gelince §1'i `[KESİN]` yap.

## Kabul kriterleri
- Git temiz. Kararlar ve izinler `SKILL.md` ve `learnings.md`'de.
- Claude Play modunu başlatıp Console'u okuyabiliyor (MCP) **veya** PlayMode testlerini komut satırından çalıştırabiliyor. İkisinden en az biri çalışır durumda.
- Ekran görüntüsü aracı 4 oranda görüntü üretiyor.
- En az 3 PlayMode duman testi (smoke test) var. Retry testi Faz 1 düzeltmesinden önce **kırmızı** olmalı (hatayı yakaladığını kanıtlar).
