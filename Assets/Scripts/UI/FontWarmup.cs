using TMPro;
using UnityEngine;

/// <summary>
/// Dinamik TMP font atlaslarına oyunda kullanılan tüm karakterleri önceden ekler.
/// Önlenen sorun: bir yazı ilk kez görününce (ör. "YAKIN!") glif üretimi takılma yapıyordu (build'de ~0,5 sn).
/// Oyunun fontları artık statik atlasta (KakFontSetup), bu yüzden normalde iş yapmaz; yeni bir dinamik
/// font eklenirse güvenlik ağıdır. TMP'nin yedek fontu (LiberationSans) ısıtılmaz: yalnızca eksik karakterde kullanılır.
/// </summary>
public static class FontWarmup
{
    static bool done;

    public static void Run()
    {
        if (done) return;
        done = true;
        string chars = Loc.AllCharacters() + "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZabcçdefgğhıijklmnoöprsştuüvyzQWXqwx";
        foreach (var f in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            if (f != null && f.atlasPopulationMode == AtlasPopulationMode.Dynamic && !f.name.StartsWith("LiberationSans"))
                f.TryAddCharacters(chars, out _);
        }
    }
}
