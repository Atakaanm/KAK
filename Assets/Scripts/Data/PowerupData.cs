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
    [Tooltip("G4: bu zorluk kademesinden önce çıkmaz (0 = Başlangıç)")]
    public int minStage = 0;

    [Header("Kötü eşya (G4)")]
    [Tooltip("Alınmaması gereken eşya (Pranga): güç süresi yükseltmesi uzatmaz, kırmızı gösterilir")]
    public bool harmful = false;

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
    Ghost,          // İçlerinden geçilebilir hayalet olma durumu
    Shackle,        // G4: Pranga (kötü eşya) — yavaşlatır. Sona eklendi: kayıtlı enum değerleri değişmesin
    Glove,          // G7: Eldiven — kartopunu yakala, geri fırlat
    IceBoots,       // G7: Buz ayakkabısı — kaymadan hareket
    Fire            // G7: Ateş — ısınırsın (soğuk göstergesi düşer)
}
