using UnityEngine;

/// <summary>
/// Faz 15 K1: oyuncunun kalıcı gücü (0 = hiç yükseltme yok, 1 = her şey en üstte). Tempo bununla ölçeklenir:
/// güçlenen oyuncu oyuna biraz daha hızlı başlar ve rampa kısalır (TempoProfile). Ortak gelişim seviyelerinin
/// toplamından (Progression).
/// </summary>
public static class PlayerPower
{
    /// <summary>Testler ve denge ölçümü için sabit güç (negatif = kapalı).</summary>
    public static float TestOverride = -1f;

    public static float Normalized(PlayerData p)
    {
        if (TestOverride >= 0f) return Mathf.Clamp01(TestOverride);
        return Progression.PowerNormalized(); // K3: ortak gelişim toplamı
    }
}
