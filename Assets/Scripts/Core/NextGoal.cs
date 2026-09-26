/// <summary>
/// Oyuncunun bir sonraki satın alma hedefi (Faz 3c.7, "hedefe yaklaşma" etkisi): açık özelliklerdeki en ucuz
/// sahip olunmayan karakter ya da pet. Yoksa null.
/// </summary>
public static class NextGoal
{
    public struct Goal { public string name; public int price; public bool isPet; }

    public static Goal? Find()
    {
        Goal? best = null;
        if (FeatureGate.IsUnlocked(Feature.Characters))
        {
            var cat = CharacterCatalog.Load();
            if (cat != null && cat.characters != null)
                foreach (var c in cat.characters)
                    if (c != null && !CharacterCatalog.Owned(c) && (best == null || c.unlockPrice < best.Value.price))
                        best = new Goal { name = Loc.T(c.nameKey), price = c.unlockPrice };
        }
        if (FeatureGate.IsUnlocked(Feature.Pets))
        {
            var pc = PetCatalog.Load();
            if (pc != null && pc.pets != null)
                foreach (var p in pc.pets)
                    if (p != null && !PetCatalog.Owned(p) && (best == null || p.price < best.Value.price))
                        best = new Goal { name = Loc.T(p.nameKey), price = p.price, isPet = true };
        }
        return best;
    }

    /// <summary>Bu özellik kategorisinde şu an alınabilecek bir şey var mı (menü butonu noktası).</summary>
    public static bool CanBuyIn(Feature f)
    {
        if (!FeatureGate.IsUnlocked(f)) return false;
        if (f == Feature.Characters)
        {
            var cat = CharacterCatalog.Load();
            if (cat != null && cat.characters != null)
                foreach (var c in cat.characters) if (c != null && !CharacterCatalog.Owned(c) && CharacterCatalog.CanAfford(c)) return true;
        }
        else if (f == Feature.Pets)
        {
            var pc = PetCatalog.Load();
            if (pc != null && pc.pets != null)
                foreach (var p in pc.pets) if (p != null && !PetCatalog.Owned(p) && PetCatalog.CanAfford(p)) return true;
        }
        return false;
    }
}
