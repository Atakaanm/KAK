using UnityEngine;

/// <summary>
/// Karakter listesi ve sahiplik/satın alma kuralları (Faz 3c.3). Resources/CharacterCatalog.
/// Karakterler düz daha güçlü değil, farklı oyun tarzı (ödünleşimli istatistikler): 3c.md.
/// </summary>
[CreateAssetMenu(fileName = "CharacterCatalog", menuName = "KacAtaKac/Character Catalog")]
public class CharacterCatalog : ScriptableObject
{
    public PlayerData[] characters;

    static CharacterCatalog cached;

    public static CharacterCatalog Load()
    {
        if (cached == null) cached = Resources.Load<CharacterCatalog>("CharacterCatalog");
        return cached;
    }

    public PlayerData Find(string id)
    {
        if (characters == null) return null;
        foreach (var c in characters) if (c != null && c.id == id) return c;
        return null;
    }

    /// <summary>Seçili karakter (kayıttan); yoksa ilk karakter; katalog yoksa fallback.</summary>
    public static PlayerData Selected(PlayerData fallback = null)
    {
        var cat = Load();
        if (cat == null || cat.characters == null || cat.characters.Length == 0) return fallback;
        var p = cat.Find(SaveSystem.Data.selectedCharacter);
        if (p == null || !Owned(p)) p = cat.characters[0];
        return p != null ? p : fallback;
    }

    public static bool Owned(PlayerData p)
    {
        if (p == null) return false;
        if (p.unlockPrice <= 0) return true;
        var list = SaveSystem.Data.unlockedCharacters;
        return list != null && list.Contains(p.id);
    }

    public static bool CanAfford(PlayerData p) => p != null && SaveSystem.Data.coins >= p.unlockPrice;

    /// <summary>Altınla satın alır ve seçer. Başarılıysa true (kayıt yazılır).</summary>
    public static bool TryBuy(PlayerData p)
    {
        if (p == null || Owned(p) || !CanAfford(p)) return false;
        var d = SaveSystem.Data;
        d.coins -= p.unlockPrice;
        if (!d.unlockedCharacters.Contains(p.id)) d.unlockedCharacters.Add(p.id);
        d.selectedCharacter = p.id;
        SaveSystem.Save();
        return true;
    }

    public static bool Select(PlayerData p)
    {
        if (!Owned(p)) return false;
        SaveSystem.Data.selectedCharacter = p.id;
        SaveSystem.Save();
        return true;
    }
}
