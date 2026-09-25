using TMPro;
using UnityEngine;

/// <summary>
/// Dinamik TMP font atlaslarına oyunda kullanılan tüm karakterleri önceden ekler.
/// Önlenen sorun: bir yazı ilk kez görününce (ör. "YAKIN!") glif üretimi takılma yapıyordu (build'de ~0,5 sn).
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
            if (f != null && f.atlasPopulationMode == AtlasPopulationMode.Dynamic)
                f.TryAddCharacters(chars, out _);
        }
    }
}
