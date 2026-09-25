using UnityEngine;

/// <summary>
/// Mermi verilerini tutan ScriptableObject.
/// Unity Editorede: Assets > Create > KacAtaKac > Projectile Data
/// Taş, kaya, göktaşı gibi farklı mermiler için ayrı ayrı oluşturulur. Davranış (hareket tipi)
/// burada seçilir; yeni taş türü eklemek kod yazmadan yeni bir veri dosyası oluşturmaktır.
/// </summary>
[CreateAssetMenu(fileName = "New ProjectileData", menuName = "KacAtaKac/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [Header("Genel Bilgi")]
    public string projectileName = "Rock";

    [Header("Istatistikler")]
    public float speed = 1.5f;
    public int damage = 1;
    public float lifeTime = 5f;
    public float rotationSpeed = 0f; // 0 birakilirsa rastgele doner

    [Header("Hareket")]
    public ProjectileMotion motion = ProjectileMotion.Straight;

    [Tooltip("Bounce: duvardan kaç kez seker")]
    public int bounces = 2;

    [Tooltip("Homing: saniyedeki en fazla dönüş (derece) ve takip süresi")]
    public float homingTurnRate = 110f;
    public float homingDuration = 1.2f;

    [Tooltip("Split: kaç saniye sonra kaç parçaya bölünür, parçaların açısı ve verisi")]
    public float splitAfter = 0.9f;
    public int splitCount = 3;
    public float splitSpread = 50f;
    public ProjectileData splitInto;

    [Tooltip("Meteor: gökten düşme süresi, başlangıç yüksekliği, iniş hasar yarıçapı")]
    public float meteorFallTime = 1.1f;
    public float meteorStartHeight = 7f;
    public float meteorRadius = 0.55f;

    [Header("Görsel")]
    [Tooltip("Prefab ölçeğine çarpan (zorluk kademesi çarpanıyla birlikte uygulanır)")]
    public float visualScale = 1f;
    public Color tint = Color.white;

    [Header("Prefab ve Gorsel")]
    public GameObject projectilePrefab;
    public Sprite projectileSprite;
}

public enum ProjectileMotion
{
    Straight, // düz gider
    Bounce,   // duvarlardan seker
    Homing,   // kısa süre oyuncuyu takip eder, sonra düz gider
    Split,    // yolun ortasında parçalara bölünür
    Meteor    // gökten düşer: yerde büyüyen gölge uyarısı, inişte alan hasarı
}
