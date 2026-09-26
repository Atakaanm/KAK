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
    /// <summary>Süreli etki başladı (veri, gerçek süre sn; 0 = vurulana kadar, ör. kalkan). Anlık etkilerde (can) çağrılmaz.</summary>
    public static event Action<PowerupData, float> PowerupActivated;
    /// <summary>Mermi duvara çarpıp yok oldu (konum, hız).</summary>
    public static event Action<Vector3, Vector2> ProjectileHitWall;
    /// <summary>Gökten düşen taş yere indi (konum).</summary>
    public static event Action<Vector3> MeteorLanded;
    /// <summary>Taş oyuncunun çok yakınından geçti (konum, dash sırasında mı).</summary>
    public static event Action<Vector3, bool> NearMiss;
    /// <summary>Oyuncu dash attı (başlangıç, yön).</summary>
    public static event Action<Vector3, Vector2> DashUsed;
    /// <summary>Combo çarpanı değişti (yeni çarpan).</summary>
    public static event Action<float> ComboChanged;
    /// <summary>Sonsuz mod olayı başladı (görünen ad).</summary>
    public static event Action<string> EndlessEventStarted;
    /// <summary>Kırmızı kart (Futbol): top düşer.</summary>
    public static event Action<Vector3> RedCard;
    /// <summary>Gol atıldı (toplam gol).</summary>
    public static event Action<int> GoalScored;
    /// <summary>Bölüm tamamlandı (yıldız).</summary>
    public static event Action<int> LevelCompleted;
    /// <summary>Zorluk kademesi değişti (kademe adı, indeks).</summary>
    public static event Action<string, int> StageChanged;

    public static void RaisePlayerDamaged(int hp, Vector3 pos) => PlayerDamaged?.Invoke(hp, pos);
    public static void RaiseShieldBlocked(Vector3 pos) => ShieldBlocked?.Invoke(pos);
    public static void RaisePlayerDied(Vector3 pos) => PlayerDied?.Invoke(pos);
    public static void RaisePowerupCollected(PowerupData data, Vector3 pos) => PowerupCollected?.Invoke(data, pos);
    public static void RaisePowerupActivated(PowerupData data, float seconds) => PowerupActivated?.Invoke(data, seconds);
    public static void RaiseProjectileHitWall(Vector3 pos, Vector2 vel) => ProjectileHitWall?.Invoke(pos, vel);
    public static void RaiseMeteorLanded(Vector3 pos) => MeteorLanded?.Invoke(pos);
    public static void RaiseNearMiss(Vector3 pos, bool dashing) => NearMiss?.Invoke(pos, dashing);
    public static void RaiseDashUsed(Vector3 pos, Vector2 dir) => DashUsed?.Invoke(pos, dir);
    public static void RaiseComboChanged(float mult) => ComboChanged?.Invoke(mult);
    public static void RaiseEndlessEventStarted(string name) => EndlessEventStarted?.Invoke(name);
    public static void RaiseRedCard(Vector3 pos) => RedCard?.Invoke(pos);
    public static void RaiseGoalScored(int total) => GoalScored?.Invoke(total);
    public static void RaiseLevelCompleted(int stars) => LevelCompleted?.Invoke(stars);
    public static void RaiseStageChanged(string name, int index) => StageChanged?.Invoke(name, index);

    public static void ClearAll()
    {
        PlayerDamaged = null;
        ShieldBlocked = null;
        PlayerDied = null;
        PowerupCollected = null;
        PowerupActivated = null;
        ProjectileHitWall = null;
        StageChanged = null;
        MeteorLanded = null;
        NearMiss = null;
        DashUsed = null;
        ComboChanged = null;
        EndlessEventStarted = null;
        RedCard = null;
        GoalScored = null;
        LevelCompleted = null;
    }
}
