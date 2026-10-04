using UnityEngine;

/// <summary>
/// Faz 15 K3: karakterin son özellik değerleri. Kaynaklar: karakter tabanı (PlayerData) + karakter pasifi + ortak gelişim
/// (+ ileride eşya geliştirme, ekipman, oyun içi kartlar). Oyun (PlayerStats) ve menü (kart, detay) aynı hesabı kullanır.
/// </summary>
public static class StatBuilder
{
    /// <summary>Kalkan yenilenmesi seviyesi → aralık (sn); 0 = yok.</summary>
    public static readonly float[] ShieldRegenSeconds = { 0f, 60f, 45f, 35f };
    public const float MagnetPullSpeed = 7f;

    public static StatSheet Build(PlayerData p, StatSheet into = null)
    {
        var s = into ?? new StatSheet();
        s.Clear();
        if (p != null) s.AddAll(p.passive);
        Progression.AddModifiers(s);
        ItemProgress.AddModifiers(s); // K5: Pranga direnci
        Equipment.AddModifiers(s);    // K7: takılı ekipman (KakScope.Equipment)
        return s;
    }

    public static int StartHealth(PlayerData p) => p == null ? 1 : Mathf.Clamp(p.startHealth, 1, Mathf.Max(1, p.maxHealth));
    /// <summary>Oyuna başlarken can: taban + gelişim, karakterin üst sınırına kadar (karakter kimliği: Tank 5, Çevik 2...).</summary>
    public static int Hearts(PlayerData p, StatSheet s) => p == null ? 1 : Mathf.Clamp(Mathf.RoundToInt(s.Apply(StatId.MaxHearts, StartHealth(p))), 1, Mathf.Max(1, p.maxHealth));
    public static float MoveSpeed(PlayerData p, StatSheet s) => p == null ? 4f : s.Apply(StatId.MoveSpeed, p.moveSpeed);
    public static float HurtScale(PlayerData p, StatSheet s) => Mathf.Clamp(s.Apply(StatId.Hurtbox, p != null ? p.hurtboxScale : 1f), 0.5f, 1.5f);
    public static float Invuln(PlayerData p, StatSheet s) => s.Apply(StatId.InvulnTime, p != null ? p.invincibilityDuration : 0.35f);
    public static float Magnet(StatSheet s) => Mathf.Max(0f, s.Apply(StatId.Magnet, 0f));
    public static float CoinMult(PlayerData p, StatSheet s) => s.Apply(StatId.CoinGain, p != null ? p.coinMultiplier : 1f);
    public static float ScoreMult(StatSheet s) => s.Apply(StatId.ScoreGain, 1f);
    public static float NearBonusMult(StatSheet s) => s.Apply(StatId.NearMissBonus, 1f);
    public static float NearRadiusMult(StatSheet s) => Mathf.Clamp(s.Apply(StatId.NearMissRadius, 1f), 0.5f, 2f);
    public static float ComboKeep(StatSheet s, float baseKeep) => Mathf.Clamp(s.Apply(StatId.ComboKeep, baseKeep), 0f, 0.95f);
    public static float ComboMax(StatSheet s, float baseMax) => s.Apply(StatId.ComboMax, baseMax);
    public static float PowerDuration(PlayerData p, StatSheet s) => s.Apply(StatId.PowerDuration, p != null ? p.powerupDurationMultiplier : 1f);
    public static float ItemFrequency(PlayerData p, StatSheet s) => Mathf.Max(0.1f, s.Apply(StatId.ItemFrequency, p != null ? p.powerupSpawnRateMultiplier : 1f));
    public static float ItemGroundTime(StatSheet s) => Mathf.Max(0.3f, s.Apply(StatId.ItemGroundTime, 1f));
    public static int Revives(StatSheet s) => Mathf.Max(0, Mathf.RoundToInt(s.Flat(StatId.Revive)));
    public static bool StartShield(PlayerData p, StatSheet s) => (p != null && p.startWithShield) || s.Flat(StatId.StartShield) >= 0.5f;
    public static float WarningMult(StatSheet s) => Mathf.Clamp(s.Apply(StatId.WarningTime, 1f), 0.5f, 2f);
    public static float ShackleResist(StatSheet s) => Mathf.Clamp(s.Apply(StatId.ShackleResist, 0f), 0f, 0.8f);

    public static float ShieldRegenInterval(StatSheet s)
    {
        int lvl = Mathf.RoundToInt(s.Flat(StatId.ShieldRegen));
        if (lvl <= 0) return 0f;
        return ShieldRegenSeconds[Mathf.Min(lvl, ShieldRegenSeconds.Length - 1)];
    }
}
