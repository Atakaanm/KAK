/// <summary>
/// Günlük ödül (Faz 3c.6): 7 günlük artan takvim. Art arda gelinirse seri ilerler (7'den sonra baştan),
/// bir gün kaçarsa 1. güne döner. Açılış: FeatureGate.DailyReward (2. oyun günü). Gün = yerel tarih.
/// </summary>
public static class DailyReward
{
    public static readonly int[] Rewards = { 30, 40, 60, 80, 100, 150, 250 };

    /// <summary>Testler için "bugün" (gün numarası); -1 = gerçek tarih.</summary>
    public static int TodayOverride = -1;

    public static int Today => TodayOverride >= 0 ? TodayOverride
        : (int)(System.DateTime.Now.Date.Ticks / System.TimeSpan.TicksPerDay);

    public static bool CanClaim()
    {
        if (!FeatureGate.IsUnlocked(Feature.DailyReward)) return false;
        return SaveSystem.Data.lastClaimDay != Today;
    }

    /// <summary>Bugün alınacak ödülün günü (1-7).</summary>
    public static int NextDay
    {
        get
        {
            var d = SaveSystem.Data;
            bool continues = d.lastClaimDay == Today - 1 && d.dailyStreak > 0;
            return continues ? d.dailyStreak % Rewards.Length + 1 : 1;
        }
    }

    /// <summary>Ödülü verir ve kaydeder. Verilen altını döndürür (alınamazsa 0).</summary>
    public static int Claim()
    {
        if (!CanClaim()) return 0;
        var d = SaveSystem.Data;
        int day = NextDay;
        int reward = Rewards[day - 1];
        d.dailyStreak = day;
        d.lastClaimDay = Today;
        d.coins += reward;
        d.totalCoins += reward;
        SaveSystem.Save();
        return reward;
    }
}
