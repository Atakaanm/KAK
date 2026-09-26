using UnityEngine;

/// <summary>Pet listesi ve sahiplik/satın alma (Resources/PetCatalog). Seçili pet yoksa ("") pet çıkmaz.</summary>
[CreateAssetMenu(fileName = "PetCatalog", menuName = "KacAtaKac/Pet Catalog")]
public class PetCatalog : ScriptableObject
{
    public PetData[] pets;

    static PetCatalog cached;

    public static PetCatalog Load()
    {
        if (cached == null) cached = Resources.Load<PetCatalog>("PetCatalog");
        return cached;
    }

    public PetData Find(string id)
    {
        if (pets == null || string.IsNullOrEmpty(id)) return null;
        foreach (var p in pets) if (p != null && p.id == id) return p;
        return null;
    }

    public static PetData Selected()
    {
        var cat = Load();
        if (cat == null) return null;
        var p = cat.Find(SaveSystem.Data.selectedPet);
        return p != null && Owned(p) ? p : null;
    }

    public static bool Owned(PetData p) => p != null && SaveSystem.Data.unlockedPets != null && SaveSystem.Data.unlockedPets.Contains(p.id);
    public static bool CanAfford(PetData p) => p != null && SaveSystem.Data.coins >= p.price;

    public static bool TryBuy(PetData p)
    {
        if (p == null || Owned(p) || !CanAfford(p)) return false;
        var d = SaveSystem.Data;
        d.coins -= p.price;
        d.unlockedPets.Add(p.id);
        d.selectedPet = p.id;
        SaveSystem.Save();
        return true;
    }

    /// <summary>Sahip olunan peti seçer; zaten seçiliyse bırakır (pet olmadan oyna).</summary>
    public static bool Toggle(PetData p)
    {
        if (!Owned(p)) return false;
        var d = SaveSystem.Data;
        d.selectedPet = d.selectedPet == p.id ? "" : p.id;
        SaveSystem.Save();
        return true;
    }
}
