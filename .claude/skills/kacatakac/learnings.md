# KaçAtaKaç — Öğrenme Günlüğü

> En yeni kayıt en üstte. Format: `## YYYY-AA-GG — başlık`, sonra kısa maddeler.
> Kaydedilecekler: kararlar ve gerekçeleri, keşfedilen tuzaklar, kullanıcının tercihleri/geri bildirimleri, işe yarayan/yaramayan yaklaşımlar.
> Kod veya git geçmişinden zaten okunabilecek şeyleri tekrar yazma.

## 2026-09-26 — Denetim D0-D1: editör kilidi ve adil çarpışma

- **Tuzak (editörü 10 dk kilitledi):** `EditorSceneManager.SaveOpenScenes()` aktif sahne adsızsa (Untitled) modal "Farklı Kaydet" penceresi açar. Arka planda çalışan editörde köprü donar, kalp atışı durur. Kural: otomasyonda asla `SaveOpenScenes`; `KakEditorUtil.SaveNamedScenes()` (editör asm) / köprüde `SaveNamedScenes()`.
- **Tuzak:** PlayMode testlerinden sonra test aracı sahne düzenini geri yükleyemeyebiliyor ("InitTestScene... doesn't exist"). Editör geçici sahnede kalır, sonraki kaydetme adsız sahneye denk gelir. Köprü artık testten önce gerçek sahneyi açıyor, sonra geri yüklüyor.
- Kilit sırasında AppleScript/System Events zaman aşımına uğradı (-1712, erişilebilirlik izni yok). Modal pencereyi Claude kapatamıyor, kullanıcıya haber ver.
- **Çarpışma kararı (2.5D):** iki collider. Ayak izi (katı, ayaklarda) duvarlar için, gövde kapsülü (trigger, çocuk "Hurtbox", etiket Player) taşlar için. Taş `PlayerHitbox.Hurt` dışındaki Player collider'larını yok sayar. Trigger-trigger teması 2D'de çalışıyor (kinematik taş rb + dinamik oyuncu rb), testle doğrulandı.
- Ölçü kaynağı: karakter idle kareleri 17×40 opak px → 0,41×0,96 birim; taş görseli ~28 px (0,336 yarıçap), collider 0,26. Hedef: temas mesafesi görsel temastan biraz küçük (cömert).
- Kalkan gibi durumlar karakteri boyamamalı/ölçeklememeli: okunurluk + çarpışma değişmez. Durum = ayrı katman (balon, ikon).
- Arena kaideleri görselin parçası, collider'ları oyun sırasında `ArenaAutoLayout` kurar (normalize ölçüler, tema ile kapatılabilir: `solidPedestals`).
- Near-miss bandı genişledi (temas 0,66 → ~0,44-0,66): yakın geçişler daha sık olacak. Denge etkisini D5'te ölç.

## 2026-09-25 — Faz 10 tamamlandı (kullanıcı adımları hariç): yayın hazırlığı

- **Gerçek build ölçümü editörden çok farklı:** GC editörde ~100 KB/kare, build'de ~30 B/kare; batch editörde 138, build'de 82. Performans kararlarını build ölçümüyle ver (`KakBuild.BuildMacDev` + `-kakbench`).
- 552 ms'lik takılma: TMP dinamik font atlasının ilk kez görülen karakterleri üretmesi. `FontWarmup` ile tüm Loc karakterleri açılışta ekleniyor → en kötü kare 33 ms.
- **Tuzak:** `PlayerSettings.companyName/productName` değişince `Application.persistentDataPath` ve PlayerPrefs konumu da değişir → kayıt "kaybolmuş" gibi görünür. `SaveSystem` eski konumdan taşıyor.
- **Tuzak:** Otomatik ekran görüntüsü oturumları kullanıcının gerçek kaydına yazıyordu (sahte rekor 281). Köprü artık Play başlatınca `SaveSystem.OverridePath`'i geçici dosyaya çeviriyor.
- Kullanıcının Mac'i İngilizce → oyun varsayılan EN açıldı. Kullanıcının kaydında dil TR'ye sabitlendi.
- Android/iOS modülleri kurulu değil. Birkaç GB'lık indirme kullanıcının onayına bırakıldı (kontrol listesinde).
- Sesler dış kaynak olmadan numpy ile sentezlendi. Claude sesleri duyamadığı için kullanıcı geri bildirimi gerekiyor.

## 2026-09-25 — Faz 4 tamamlandı: menü ve arayüz

- Menü önceden resimsel, gerçekçi bir mağara görseliydi (oyunun piksel diliyle çelişiyordu). Artık canlı piksel arena kullanılıyor, tek stil.
- Tüm UI editör koduyla üretiliyor (`KakUiKit`/`KakUiSetup`). Elle sahne düzenleme yok, tekrar çalıştırınca aynı sonuç.
- **Tuzak tekrarı:** `GetComponent<T>() ?? AddComponent<T>()` yine kullanıldı ve patladı. Artık `KakUiKit.GetOrAdd<T>(go)` var. Yeni kodda `??` + Unity nesnesi görürsen düzelt (`grep -rn "?? .*AddComponent"`).
- **Test kirliliği:** PlayMode testleri gerçek `save.json`'a yazıyordu (menüde "37 oyun"). Artık `KakTestUtil.ResetWorld` geçici kayıt kullanıyor. Kural: testler kullanıcı verisine dokunmaz.
- **Test zayıflığı:** CanvasGroup varsayılan olarak etkileşimli olduğu için "butonlar aktifleşti" koşulu panel açılmadan sağlanmıştı. Koşulu anlamlı bir önkoşulla (panel açıldı, sonra etkileşim) bekle, süreyi sorgula.
- `SceneFader` meşgulken gelen yükleme istekleri sıraya alınıyor (test bu hatayı yakaladı).

## 2026-09-25 — Faz 5 tamamlandı: Sonsuz Mod içeriği ve denge

- Eski denge çok dikti: skor 10/sn ile 15. sn'de Orta, 50. sn'de Cehennem. Eşikler süreye yayıldı, çarpanlar yumuşatıldı. Zorluk artık çeşitlilikten (taş türleri + olaylar) geliyor.
- **Denge ölçümünde bot sınırı:** Tepkisel, 0,25-0,8 sn ufuklu, 16 yönlü plancı. Seken ve hızlı taşlarda zayıf, usta ile acemi arası fark küçük. Vuran taş türü istatistiği (`PlayerHealth.LastHitSource`) hangi içeriğin öldürdüğünü gösteriyor, bu değerli. Ama "usta insan süresi" için güvenilir değil. **Denge kararları kullanıcı oyun testiyle kesinleşmeli.**
- `KakTime.TestSpeed`: testlerde oyunu hızlandırır, fizik adımına dokunmaz.
- Tuzak: `enableWordWrapping` TMP'de eski (Unity 6), `textWrappingMode = TextWrappingModes.NoWrap` kullan.
- Olaylar fırlatıcıları geçici açıp kapattığı için kademe değişimiyle çakışabiliyordu. `DifficultyManager.RefreshSpawnerActivation()` olay sonunda doğru durumu geri yüklüyor (testli).

## 2026-09-25 — Faz 2A tamamlandı: mermi davranışları ve kayıt

- Mermi davranışı tek sınıfta (`Projectile`), `ProjectileData.motion` ile seçiliyor. Yeni taş türü = yeni veri dosyası. Kalıtım/strateji SO yerine enum + switch seçildi: 5 davranış için daha basit, tahsissiz, havuzla uyumlu (YAGNI). Davranış sayısı çok artarsa (bölüm düşmanları) strateji SO'ya geçilebilir.
- Meteor: collider düşerken kapalı, inişte mesafe kontrolü (alan hasarı). Gölge 0.25x → 1.4x büyüyerek uyarı veriyor.
- Parçalanan taş zorluk çarpanlarını çocuklara aktarıyor (hız oranı, ölçek oranı).
- SaveSystem: JsonUtility + atomik yazma. `OverridePath` ile testler gerçek kaydı bozmuyor.
- 4 hareket testi ilk denemede geçti. Testleri yazarken fırlatıcıları ve powerup spawner'ı kapatmak, testi deterministik yapıyor.

## 2026-09-25 — Faz 3 tamamlandı: görsel temel

- Kullanıcı `/goal` ile "fazlar bitene kadar durma" dedi. Palet seçimine cevap gelmeden Endesga 32 (önerim) varsayımla uygulandı.
- **Palete zorla indirgemek yapay zeka görsellerini bozuyor** (meşale parıltısı sert turuncu leke, zemin düz yeşil, koyulaştırınca gürültü). Çözüm: yüksek çözünürlüklü görseller global renk tonlamasıyla bütünleşir, palet piksel sanatına ve üretilen öğelere uygulanır.
- Taş için en iyi okunurluk: parlaklığa göre 4 kademeli **sıcak rampa** (kahve → bakır → ten) + lav çatlakları turuncu + 1 px koyu kontur + yumuşak gölge. Yeşil zeminin tamamlayıcısı olduğu için gri tonlamada bile seçiliyor. Palet indirgemeli gri taş benekliydi.
- Karakterlere 1 px kontur (Endesga 181425) koyu zeminde seçilirliği ciddi artırdı.
- Tuzak: `GetComponent<T>() ?? AddComponent<T>()` Unity nesnelerinde çalışmaz (sahte null). Açık `if (x == null)` kullan.
- Tuzak: `Assets/Sprites/Player/Black.png` karakter karesi değil, yumuşak gölge (256 alfa kademesi). Toplu işlemlerde klasördeki özel dosyaları kontrol et.
- Tuzak: Unity arka planda kendi kendine derleyince köprünün `refresh` cevabı "derlenecek değişiklik yok" diyebilir. İstemci artık son derleme sonucunu her zaman gösteriyor.
- Performans: 138 batch. Sprite Atlas ve URP dinamik batching sayıyı değiştirmedi. Universal (3D) Renderer'da sprite birleşmesi zayıf. 2D Renderer değerlendirmesi Faz 10'da cihaz ölçümüyle yapılacak.
- Ölüm sekansı: panel 0,7 sn yavaş çekimden sonra açılıyor. Testler `WaitUntil` ile bekliyor.

## 2026-09-25 — Faz 1.5 tamamlandı: tek parça dungeon ekranı

- Kompozisyon: HUD bandı + ekran enine sığan kare arena + koridor/kontrol alanı. Sonuç ekran görüntülerinde net: "ortaya yapıştırılmış resim" hissi gitti. Değer dağılımı %78 orta tondan %51 koyu / %47 orta'ya geçti.
- **Büyük tuzak:** Kullanıcının editöründe Play görünümü **Device Simulator**. Simulator açıkken oyun `Screen.width/height`'ı simüle cihazdan alıyor, Game view boyutunu değiştirmek oyunu etkilemiyor. Ama ScreenCapture gizli Game view'dan çekiyor. Önceki çoklu oran görüntüleri bu yüzden kısmen yanıltıcıydı (mantık testleri etkilenmedi). Çözüm: `PlayModeWindow.SetViewType/SetCustomRenderingResolution` (Unity 2022.2+ resmi API) + tüm GameView örneklerine aynı boyut + PNG boyut doğrulaması.
- **Ders:** Araç ile ölçüm arasındaki tutarsızlığı log ile doğrula (ScreenComposer'ın boyut logu sorunu tek adımda gösterdi).
- Geçici karolar prosedürel üretildi: arenadan örneklenen renkler, arena sanat pikseline yakın ölçek (PPU 41.667, Point). Arenanın "sanat pikseli" yapay zeka üretimi olduğu için düzensiz (5-6 px).
- Meşale haleleri 2.2 ölçekte koridoru turuncu lekelere boğdu, 1.3'e indirildi. **Işık/parlama öğeleri ortamı boğmamalı** (renk rehberi 60-30-10).
- Joystick tabanı ve tutamağı soluk taş/krem: önceki parlak sarı top ekranın en dikkat çekici öğesiydi, hiyerarşi düzeldi.
- `git checkout main && pull` sırasında Unity eski dosyaları görüp yeniden içe aktarabiliyor. Bundan sonra yeni dalı `git fetch && git checkout -b yeni origin/main` ile aç.

## 2026-09-25 — Faz 1 tamamlandı: stabilizasyon

- Kullanıcının "item'lar başka yerde çıkıyor" şikayetinin görünür nedeni: **5 powerup ikonunun hepsi saydamlıksız RGB** (siyah kare zemin). Yapay zekayla siyah zemin üzerine üretilmişler. `tools/kak_icon_cleanup.py` ile kenardan flood-fill + siyahtan ön-çarpımı geri alma (parlama yarı saydam korunur). **Kural:** Yapay zeka görselleri içeri alınırken saydamlık ve gömülü yazı kontrolü zorunlu (Ghost ikonunda "INTANGIBILITY POWERUP" yazısı vardı).
- Arena görseli simetrik: iç zemin her kenardan %7,95 (162 px / 2048). Piksel analiziyle ölçüldü (koyu kontur satır/sütunları). Göz kararı ölçüm yanıltmıştı (asimetrik sanılmıştı). **Ölçüm için piksel analizi kullan.**
- Kaide duruş noktaları: sol üst (0.138, 0.834), sol alt (0.130, 0.126), sağ taraf simetrik. Fırlatıcı "ayak" konumu = Shadow child'ının konumu.
- Tuzak: `PowerupSpawner` aralığını değiştirmek mevcut zamanlayıcıyı etkilemiyordu → `SetSpawnInterval`.
- Tuzak (araç): Game view boyutu değişince Canvas bir kaç OYUN karesi sonra yeniden yerleşiyor. Arka planda editör tick'i oyun karesinden hızlı olabiliyor, bu yüzden ekran görüntüsü artık `Time.frameCount` ile bekliyor. Bu hata yanlış "HUD bozuk" alarmına yol açtı. **Ders:** Görsel bir bulguyu raporlamadan önce aracın doğruluğunu da şüpheyle kontrol et.
- Tuzak: `ExecuteAlways` ile editörde transform'u değiştirmek sahneyi kirli işaretlemiyor. Kaydedilen sahnede eski konumlar kalabiliyor, runtime'da `Awake` düzeltiyor.
- Usta bot 60 sn'de 1-2 vuruş alıyor, 40-57 sn arası dayanıyor (powerup sık modunda daha kısa). Ölçümler gürültülü, dengeyi Faz 5'te 20 oyunluk ortalamalarla yap.
- Git: faz dalı → PR → birleştir (kullanıcı izniyle). PR #2 (Faz 0).

## 2026-09-25 — Faz 0: Otonom test altyapısı kuruldu

- Kullanıcı tüm izinleri verdi: "test etmen gerekirse test et, Unity açık, mükemmel hale getir."
- Hazır Unity MCP eklentisi yerine **projeye özel dosya tabanlı köprü** yazıldı. Gerekçe: üçüncü taraf kod yok, MCP için oturum yeniden başlatma gerekmiyor, tam kontrol. Unity **arka plandayken de** `EditorApplication.update` çalışıyor (heartbeat ile doğrulandı), odak çalmaya gerek yok. Sadece uzun derleme veya reload sonrası uyanma için istemci pencereyi öne getirebiliyor.
- Tuzak: Derleme arka planda yavaş (assembly değişikliğinde 3-5 dk, Burst ILPostProcess dahil). Play'e giriş ~20 sn (domain reload). "Enter Play Mode Options" ile reload kapatmak **yapılmadı**: kod statik singleton'lara (Instance, Projectile statik sınırları) dayanıyor, önce bunlar temizlenmeli.
- Tuzak: Test assembly'leri Assembly-CSharp'a referans veremez → oyun kodu `KacAtaKac.asmdef`'e alındı. Sahnelerdeki script referansları GUID ile olduğu için bozulmadı (testlerle doğrulandı).
- Tuzak: Unity Test Framework, test sırasında `Debug.LogError` görürse testi düşürür. Bot testi hataları sayıp raporluyor (`LogAssert.ignoreFailingMessages`), duman testleri ise temiz log bekliyor.
- Tuzak: Bu makinede `/usr/bin/git` "xcrun: invalid active developer path" veriyor (Xcode yarım veya güncelleniyor). `DEVELOPER_DIR=/Library/Developer/CommandLineTools` ile çözüldü, sistem ayarına dokunulmadı.
- `GameViewSizes` reflection'ı Unity 6000.3'te çalışıyor (selectedSizeIndex yazılabilir). Ekran görüntüleri doğru çözünürlükte.
- Test sonuçları (başlangıç): Retry ❌, menü dönüşü ❌, doğrudan sahne ❌, Wall etiketi ❌; skor, ölüm paneli ve menüden oyna ✅. Usta bot 60 sn'de 1 vuruş aldı. Mevcut zorluk usta bir oyuncu için ilk dakikada kolay.

## 2026-09-25 — Ekran kompozisyonu kesinleşti, promptlar otonom çalışmaya göre yeniden yazıldı

- Kullanıcı başka bir yapay zekanın önerisini getirdi (kare arenayı koru, üstü HUD duvarı, altı koridor + kontrol alanı). Değerlendirme: kontrollerin arenaya binmemesi, her cihazda aynı oyun alanı ve daha az iş açısından **benim "boyuna uzayan arena" önerimden daha iyi**. O öneri benimsendi. Benimsenmeyenler: "KAÇIŞ 0/3", çıkış kapısı hedefi, devriye ve ışık konileri (bölüm modu fikirleri, Sonsuz'a uymaz), kan izleri (oyunun tonuna ve yaş sınırına uymaz).
- Kullanıcı düzeltmesi: Arena her telefonda aynı piksel boyutunda değil, **telefonun enine göre ölçekleniyor**. Oyun alanı dünya biriminde sabit.
- Kullanıcı vurguları: **renk uyumu çok önemli**, **optimizasyona dikkat**. `sanat-rehberi.md` oluşturuldu, ana prompta performans bütçesi eklendi.
- Kullanıcı Claude'u "ultra max effort" ile günlerce kesintisiz çalıştırmak istiyor. Ana prompt otonom döngüye göre yeniden yazıldı: adım başına commit, `progress.md` "Sıradaki adım" ve "Onay bekleyenler" ile kaldığı yerden devam etme. Otomatik doğrulama için Faz 0'a Unity MCP, PlayMode testleri, ekran görüntüsü ve bot aracı eklendi. Bunlar olmadan otonom çalışma her adımda kullanıcı testine takılır.
- Dash, Sonsuz Mod'a Faz 5'te eklenecek (kontrol alanının sağ yarısı).
- Faz 2, 2A (Sonsuz için şimdi) ve 2B (bölümlerden önce) olarak bölündü.

## 2026-09-25 — Öncelik Sonsuz Mod, tam ekran arena kararı (kompozisyon kısmı yukarıdaki kayıtla değişti)

- Kullanıcı kararı: **Önce Sonsuz Mod tamamen bitecek.** Bölümler sonra, "görseli ve ayarları değiştir, bitti" yaklaşımıyla gelecek. Futbolun ek mekanik (top, şut, kale) istediği kullanıcıya not edildi.
- Kullanıcının en büyük görsel şikayeti: kare arena telefonda ekranın ~%46'sını kaplıyor, gerisi ölü alan. "Bu devirde sağı solu kullanamayan oyun olmaz."
- Önerilen çözüm (Faz 1.5): dikey, tek ekran, **karo tabanlı ve ekran oranına göre boyuna uzayan arena**. Genişlik sabit, yükseklik esnek (9:16–9:21). HUD üst duvarın 2.5D ön yüzünde. 6 fırlatıcı duvar nişlerinde. Kayan joystick. Karo seti değişimi, bölümlerin reskin'ini de kolaylaştırıyor.
- Önerilen yayın stratejisi: v1.0 sadece Sonsuz Mod, dünyalar güncellemelerle.
- Faz 0 kararları hâlâ bekliyor (sanat stili, platform). Dikey ekran önerildi, onay bekleniyor.

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
