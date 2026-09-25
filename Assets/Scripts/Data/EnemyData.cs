using UnityEngine;

/// <summary>
/// Bölüm düşmanı: görsel (SpawnerData yön sprite'ları), hareket deseni, saldırı deseni, mermi.
/// Yeni düşman = yeni veri dosyası.
/// </summary>
[CreateAssetMenu(fileName = "New EnemyData", menuName = "KacAtaKac/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Kardan Adam";

    [Header("Görsel")]
    [Tooltip("8 yön idle/saldırı kareleri (fırlatıcılarla aynı yapı)")]
    public SpawnerData visuals;
    [Tooltip("Tek sprite (yönsüz düşmanlar için, ör. kardan adam)")]
    public Sprite staticSprite;
    public float visualScale = 2.4f;
    public Color tint = Color.white;

    [Header("Hareket")]
    public EnemyMovement movement = EnemyMovement.Stationary;
    public float moveSpeed = 1.2f;
    [Tooltip("SideLine/GoalKeeper: gidip gelme mesafesi (dünya birimi, merkezden)")]
    public float range = 2.5f;
    [Tooltip("Charge: hücum hızı ve bekleme")]
    public float chargeSpeed = 5f;
    public float chargeCooldown = 3f;
    [Tooltip("Temas hasarı veriyor mu (Charge için)")]
    public bool contactDamage = true;

    [Header("Saldırı")]
    public EnemyAttack attack = EnemyAttack.Aimed;
    public ProjectileData projectile;
    public float fireInterval = 2f;
    public float telegraph = 0.4f;
    [Tooltip("Spread/Burst: mermi sayısı ve açı")]
    public int count = 3;
    public float spreadAngle = 40f;
}

public enum EnemyMovement
{
    Stationary,  // yerinde durur
    SideLine,    // bir eksende gidip gelir (kenar çizgisi)
    Wander,      // alan içinde rastgele dolaşır
    Charge,      // oyuncuya doğru hücum eder, sonra durur
    GoalKeeper   // yatayda kale çizgisinde topu/oyuncuyu izler
}

public enum EnemyAttack
{
    None,
    Aimed,       // oyuncuya
    Predictive,  // oyuncunun gideceği yere
    Spread,      // yelpaze
    Burst,       // arka arkaya
    Lob          // gökten düşen (meteor davranışlı mermi)
}
