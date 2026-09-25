using System;
using UnityEngine;

/// <summary>
/// Oyun olayları (C# event, GC'siz). Ses, efekt ve UI birbirini aramak yerine bunları dinler.
/// Dinleyiciler OnEnable'da abone olup OnDisable'da çıkar; sahne geçişinde ClearAll güvenlik ağıdır.
/// </summary>
public static class GameEvents
{
    /// <summary>Oyuncu hasar aldı (kalan can, konum).</summary>
    public static event Action<int, Vector3> PlayerDamaged;
    /// <summary>Kalkan bir vuruşu engelledi (konum).</summary>
    public static event Action<Vector3> ShieldBlocked;
    public static event Action<Vector3> PlayerDied;
    public static event Action<PowerupData, Vector3> PowerupCollected;
    /// <summary>Mermi duvara çarpıp yok oldu (konum, hız).</summary>
    public static event Action<Vector3, Vector2> ProjectileHitWall;
    /// <summary>Gökten düşen taş yere indi (konum).</summary>
    public static event Action<Vector3> MeteorLanded;
    /// <summary>Zorluk kademesi değişti (kademe adı, indeks).</summary>
    public static event Action<string, int> StageChanged;

    public static void RaisePlayerDamaged(int hp, Vector3 pos) => PlayerDamaged?.Invoke(hp, pos);
    public static void RaiseShieldBlocked(Vector3 pos) => ShieldBlocked?.Invoke(pos);
    public static void RaisePlayerDied(Vector3 pos) => PlayerDied?.Invoke(pos);
    public static void RaisePowerupCollected(PowerupData data, Vector3 pos) => PowerupCollected?.Invoke(data, pos);
    public static void RaiseProjectileHitWall(Vector3 pos, Vector2 vel) => ProjectileHitWall?.Invoke(pos, vel);
    public static void RaiseMeteorLanded(Vector3 pos) => MeteorLanded?.Invoke(pos);
    public static void RaiseStageChanged(string name, int index) => StageChanged?.Invoke(name, index);

    public static void ClearAll()
    {
        PlayerDamaged = null;
        ShieldBlocked = null;
        PlayerDied = null;
        PowerupCollected = null;
        ProjectileHitWall = null;
        StageChanged = null;
        MeteorLanded = null;
    }
}
