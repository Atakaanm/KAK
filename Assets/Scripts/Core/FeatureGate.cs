/// <summary>
/// Adım adım açılan özellikler (Faz 3c). Her şey baştan gösterilmez: oyuncu çekirdek oynanışı öğrendikçe
/// yeni katmanlar açılır. Koşullar kayıttaki istatistiklere bağlı; "YENİ!" tanıtımları SaveData.seen ile bir kez.
/// Açılış takvimi ve gerekçesi: .claude/skills/kacatakac/3c.md
/// </summary>
public enum Feature { Coins }

public static class FeatureGate
{
    public static bool IsUnlocked(Feature f)
    {
        var d = SaveSystem.Data;
        switch (f)
        {
            case Feature.Coins: return d.gamesPlayed >= 1; // ilk oyun saf kaçış; altın ikinci oyundan itibaren
            default: return false;
        }
    }

    /// <summary>Özelliğin "YENİ!" tanıtımı gösterildi mi (bir kez).</summary>
    public static bool Introduced(Feature f) => SaveSystem.Data.HasSeen("feat_" + f);
    public static void MarkIntroduced(Feature f) => SaveSystem.Data.MarkSeen("feat_" + f);
}
