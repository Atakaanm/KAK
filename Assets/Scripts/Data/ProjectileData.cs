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
    [Tooltip("Düşeceği yerde yanıp sönen uyarı işareti (G1: gölge tek başına yetmiyordu)")]
    public Sprite warningSprite;

    [Header("Kartopu (G7)")]
    [Tooltip("Yol aldıkça büyüme (saniyede ölçek artışı; 0 = büyümez)")]
    public float growPerSecond = 0f;
    [Tooltip("Büyümenin üst sınırı (başlangıç ölçeğinin katı)")]
    public float maxGrowScale = 1f;
    [Tooltip("Faz 12 H2: en büyük boyda hız çarpanı (büyüdükçe ağırlaşır; 1 = yavaşlamaz). Büyük ama yavaş = okunur, kaçılabilir")]
    public float growSlowdown = 1f;
    [Tooltip("Faz 12 H2: büyümenin çarpışma alanına yansıyan payı (0-1). Görüntü tam büyür; 0,6 = alan büyümenin %60'ı kadar")]
    public float growHitShare = 1f;

    [Header("Dalga (Faz 13 K2, yarasa)")]
    [Tooltip("Yanal salınım genliği (birim)")]
    public float waveAmplitude = 0.45f;
    [Tooltip("Saniyedeki salınım")]
    public float waveFrequency = 1.4f;

    [Header("Karanlık dünya (Faz 13 K1)")]
    [Tooltip("Karanlıkta taşın parıltı rengi (yönü okunsun)")]
    public Color glowColor = new Color(1f, 0.62f, 0.3f, 0.85f);
    [Tooltip("Karanlıkta tamamen görünür (parlayan spor gibi)")]
    public bool glowInDark = false;
    [Tooltip("Eldivenle yakalanabilir (kartopu)")]
    public bool catchable = false;

    [Header("Etki")]
    public ProjectileEffect effect = ProjectileEffect.Damage;
    [Tooltip("Slow/Freeze/YellowCard süresi (sn)")]
    public float effectDuration = 1.5f;
    [Tooltip("Slow: hız çarpanı (0.5 = yarı hız)")]
    public float effectStrength = 0.5f;
    [Tooltip("Döndürme yerine düz uçsun (kart gibi)")]
    public bool noSpin = false;

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
    Meteor,   // gökten düşer: yerde büyüyen gölge uyarısı, inişte alan hasarı
    Wave      // Faz 13 K2: dalgalanarak uçar (yarasa); sona eklendi
}
