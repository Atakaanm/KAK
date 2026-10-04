using System;
using UnityEngine;

public struct ChestReward
{
    public int gold, scrolls, keys;
    public bool pity;      // garanti açılışı (bir üst sandığın ödülü)
    public int defIndex;   // ödülü veren sandık türü
}

/// <summary>
/// Faz 15 K6: sandık açma (anahtarla). Her pityEvery. açılış bir üst sandığın ödülünü verir; altın rekorla ölçeklenir.
/// Kayıt: SaveData.keys / chestsOpened, ödüller coins / scrolls / keys.
/// </summary>
public static class ChestSystem
{
    public static event Action Changed;

    public static int Keys => SaveSystem.Data.keys;

    public static ChestDef Def(int i)
    {
        var e = EconomyData.Load();
        return e != null && e.chests != null && i >= 0 && i < e.chests.Length ? e.chests[i] : null;
    }

    public static bool CanOpen(int i)
    {
        var c = Def(i);
        return c != null && SaveSystem.Data.keys >= c.keyCost;
    }

    /// <summary>Garantiye kalan açılış (1 = sıradaki açılış garanti).</summary>
    public static int UntilPity
    {
        get
        {
            var e = EconomyData.Load();
            int every = e != null ? Mathf.Max(1, e.pityEvery) : 10;
            return every - SaveSystem.Data.chestsOpened % every;
        }
    }

    public static float GoldScale()
    {
        var e = EconomyData.Load();
        if (e == null || e.bestScoreForDouble <= 0f) return 1f;
        return 1f + Mathf.Min(e.bestScoreCap, SaveSystem.Data.bestScoreEndless / e.bestScoreForDouble);
    }

    /// <summary>Sandığı açar (anahtar düşer, ödül cüzdana). rng testler için.</summary>
    public static bool TryOpen(int i, out ChestReward reward, System.Random rng = null)
    {
        reward = default;
        var e = EconomyData.Load();
        var c = Def(i);
        var d = SaveSystem.Data;
        if (e == null || c == null || d.keys < c.keyCost) return false;
        rng = rng ?? new System.Random();
        d.keys -= c.keyCost;
        d.chestsOpened++;
        bool pity = e.pityEvery > 0 && d.chestsOpened % e.pityEvery == 0 && i + 1 < e.chests.Length;
        int di = pity ? i + 1 : i;
        var def = e.chests[di];
        reward.pity = pity;
        reward.defIndex = di;
        reward.gold = Mathf.RoundToInt(rng.Next(def.goldMin, def.goldMax + 1) * GoldScale() / 5f) * 5;
        if (rng.NextDouble() < def.scrollChance) reward.scrolls = rng.Next(def.scrollMin, def.scrollMax + 1);
        if (rng.NextDouble() < def.bonusKeyChance) reward.keys = 1;
        d.coins += reward.gold;
        d.totalCoins += reward.gold;
        d.scrolls += reward.scrolls;
        d.keys += reward.keys;
        SaveSystem.Save();
        Changed?.Invoke();
        return true;
    }

    public static void RaiseChanged() => Changed?.Invoke();
}
