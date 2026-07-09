using UnityEngine;

/// <summary>
/// Yerden alınan eşyaların (güçlendirmelerin) verilerini tutan ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "New PowerupData", menuName = "KacAtaKac/RPG/Powerup Data")]
public class PowerupData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string powerupName = "Speed Boost";
    public PowerupType type = PowerupType.SpeedBoost;

    [Header("Özellikler")]
    public float duration = 5f;             // Eşyanın etki süresi
    public float powerMultiplier = 1.5f;    // Etki çarpanı (Speed hızı x1.5 yapar)
    public int healthAmount = 1;            // Eğer kalp eşyasıysa kaç can verecek

    [Header("Doğma Oranı (Spawn)")]
    public float spawnChanceWeight = 1f;    // 1f normal, 0.1f nadir

    [Header("Görsel & Obje")]
    public Sprite icon;                     // UI ikon
    public GameObject visualPrefab;         // Yerde dönen obje
}

public enum PowerupType
{
    Heal,           // Can verir (Kalp)
    Shield,         // Tek kullanımlık kalkan verir
    SpeedBoost,     // Hızlandırır (Hız botu)
    TimeSlow,       // Zamanı yavaşlatır
    Ghost           // İçlerinden geçilebilir hayalet olma durumu
}
