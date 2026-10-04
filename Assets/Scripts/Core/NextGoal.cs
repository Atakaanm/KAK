/// <summary>
/// Oyuncunun bir sonraki satın alma hedefi (Faz 3c.7, "hedefe yaklaşma" etkisi): açık özelliklerdeki en ucuz
/// gelişim/eşya izi, sahip olunmayan karakter ya da pet. Yoksa null.
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
            // Faz 15 K3/K5: açık gelişim ve eşya izlerinin en ucuzu (parşömeni yetiyorsa) — adım adım güçlenme
            var ucat = UpgradeCatalog.Load();
            if (ucat != null && ucat.tracks != null)
                foreach (var t in ucat.tracks)
                {
                    if (t == null || !Progression.Unlocked(t) || SaveSystem.Data.scrolls < Progression.ScrollCost(t)) continue;
                    int c = Progression.Cost(t);
                    if (c >= 0 && (best == null || c < best.Value.price))
                        best = new Goal { name = ProgressPanel.TrackTitle(t) + " " + (Progression.Level(t) + 1), price = c };
                }
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
                if (Progression.AnyAffordable()) return true; // Faz 15: ortak gelişim
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
