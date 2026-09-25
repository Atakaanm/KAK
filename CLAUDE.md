# KaçAtaKaç (KAK) — Claude talimatları

Bu bir Unity 6 (6000.3.8f1, URP) mobil 2.5D arcade oyun projesi. Kullanıcıyla **Türkçe** konuş.

## Her oturumda

1. Projeyle ilgili herhangi bir işe başlamadan önce **`kacatakac` skill'ini yükle** (`.claude/skills/kacatakac/`). Mimari, kurallar, nerede kalındığı ve bilinen hatalar orada.
2. İş bittiğinde, cevabı bitirmeden önce skill'i güncelle:
   - `progress.md`: durum, yapılanlar, hatalar (açılan/kapanan)
   - `learnings.md`: en üste tarihli kısa kayıt (karar, tuzak, kullanıcı tercihi)
   - Mimari/kural değiştiyse `SKILL.md`
   Soru-cevap gibi hiçbir şey değişmeyen konuşmalarda güncelleme gerekmez, ama kullanıcı yeni bir tercih veya bilgi verdiyse `learnings.md`'ye yaz.
3. Sahne/prefab YAML dosyalarını elle düzenleme (sadece çok küçük, kesin değişiklikler hariç). `.meta` dosyalarına dokunma.
4. Derleme hatası kontrolü: `grep -E "error CS|Exception" ~/Library/Logs/Unity/Editor.log`
