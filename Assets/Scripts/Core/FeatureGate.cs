/// <summary>
/// Adım adım açılan özellikler (Faz 3c). Her şey baştan gösterilmez: oyuncu çekirdek oynanışı öğrendikçe
/// yeni katmanlar açılır (Habby/Archero tarzı "drip-feed"). Koşullar kayıttaki istatistiklere bağlı;
/// "YENİ!" tanıtımları SaveData.seen ile bir kez. Takvim ve gerekçe: .claude/skills/kacatakac/3c.md
/// </summary>
public enum Feature { Coins, Missions, Characters, DailyReward, Pets }

public static class FeatureGate
{
    public const int MissionsGames = 3;
    public const int CharactersGames = 5;
    public const int CharactersCoins = 150;
    public const int DailyDays = 2;
    public const int PetsGames = 10;

    public static readonly Feature[] All = { Feature.Coins, Feature.Missions, Feature.Characters, Feature.DailyReward, Feature.Pets };

    public static bool IsUnlocked(Feature f)
    {
        var d = SaveSystem.Data;
        switch (f)
        {
            case Feature.Coins: return d.gamesPlayed >= 1; // ilk oyun saf kaçış
            case Feature.Missions: return d.gamesPlayed >= MissionsGames;
            case Feature.Characters: return d.totalCoins >= CharactersCoins || d.gamesPlayed >= CharactersGames;
            case Feature.DailyReward: return d.playDays >= DailyDays;
            case Feature.Pets: return d.gamesPlayed >= PetsGames || (d.unlockedCharacters != null && d.unlockedCharacters.Count >= 2);
            default: return false;
        }
    }

    /// <summary>Açık özelliklerin bit maskesi (oyun başı / sonu karşılaştırması için).</summary>
    public static int UnlockedMask()
    {
        int m = 0;
        foreach (var f in All) if (IsUnlocked(f)) m |= 1 << (int)f;
        return m;
    }

    /// <summary>Kilitli özellik için kalan koşul (menüde "2 oyun sonra" gibi). Açıksa boş.</summary>
    public static string LockedHint(Feature f)
    {
        if (IsUnlocked(f)) return "";
        var d = SaveSystem.Data;
        switch (f)
        {
            case Feature.Coins: return string.Format(Loc.T("locked_games"), 1 - d.gamesPlayed);
            case Feature.Missions: return string.Format(Loc.T("locked_games"), MissionsGames - d.gamesPlayed);
            case Feature.Characters: return string.Format(Loc.T("locked_games"), CharactersGames - d.gamesPlayed);
            case Feature.DailyReward: return Loc.T("locked_tomorrow");
            case Feature.Pets: return string.Format(Loc.T("locked_games"), PetsGames - d.gamesPlayed);
            default: return "";
        }
    }

    public static string Name(Feature f) => Loc.T("feat_" + f);

    /// <summary>Özelliğin "YENİ!" tanıtımı gösterildi mi (bir kez).</summary>
    public static bool Introduced(Feature f) => SaveSystem.Data.HasSeen("feat_" + f);
    public static void MarkIntroduced(Feature f) => SaveSystem.Data.MarkSeen("feat_" + f);
    /// <summary>Açık ama henüz tanıtılmamış: menüde "YENİ!" rozeti.</summary>
    public static bool IsNew(Feature f) => IsUnlocked(f) && !Introduced(f);
}
