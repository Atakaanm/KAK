using System;
using UnityEngine;

/// <summary>
/// Faz 13 K1: meşale — karanlık dünyalarda oyuncunun ışık yarıçapı. Altınla seviye seviye büyür (kalıcı).
/// Kullanıcı: "meşaleye parayla arttıra arttıra ne kadar önünü aydınlatırsa o kadar çok görecek". Satın alma: DÜNYALAR paneli.
/// </summary>
public static class TorchProgress
{
    /// <summary>Seviye başına ışık yarıçapı (dünya birimi; oynanabilir alan ~6,9 birim genişlik).</summary>
    public static readonly float[] Radius = { 1.7f, 2.1f, 2.5f, 2.9f };
    /// <summary>Bir sonraki seviyenin fiyatı (Sv1→2, 2→3, 3→4).</summary>
    public static readonly int[] Costs = { 250, 600, 1200 };

    public static event Action Changed;

    public static int Level => Mathf.Clamp(SaveSystem.Data.torchLevel, 0, Radius.Length - 1);
    public static int MaxLevel => Radius.Length - 1;
    public static float CurrentRadius => Radius[Level];
    /// <summary>Sonraki seviyenin fiyatı; en üst seviyede -1.</summary>
    public static int NextCost => Level < Costs.Length ? Costs[Level] : -1;
    public static bool CanUpgrade => NextCost > 0 && SaveSystem.Data.coins >= NextCost;

    public static bool TryUpgrade()
    {
        if (!CanUpgrade) return false;
        SaveSystem.Data.coins -= NextCost;
        SaveSystem.Data.torchLevel = Level + 1;
        SaveSystem.Save();
        Changed?.Invoke();
        return true;
    }
}
