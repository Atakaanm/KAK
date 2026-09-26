using UnityEngine;

/// <summary>
/// Karakter verilerini tutan ScriptableObject.
/// Unity Editorde: Assets > Create > KacAtaKac > Player Data
/// Her karakter icin (Boy, Girl vb.) ayri bir PlayerData olusturulur.
/// </summary>
[CreateAssetMenu(fileName = "New PlayerData", menuName = "KacAtaKac/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Genel Bilgi")]
    public string playerName = "Boy";
    public PlayerType playerType = PlayerType.Boy;

    [Header("Base İstatistikler")]
    public int maxHealth = 3;
    public float moveSpeed = 5f;
    public float invincibilityDuration = 0.35f;

    [Header("RPG / Gelişim İstastikleri")]
    public float powerupDurationMultiplier = 1.0f;  // Örn: Hızlandırma powerup'ı ne kadar sürer?
    public float powerupSpawnRateMultiplier = 1.0f; // Bu karaktere özel yerden çıkma şansı katsayısı

    [Header("Yetenek (Aktif)")]
    public AbilityData activeAbility;               // Karakterin kendine has özelliği (örn Dash)

    [Header("Giyili Ekipmanlar (Başlangıç veya Load)")]
    public EquipmentData equippedHead;              // Kafasına taktığı şapka
    public EquipmentData equippedBody;              // Üstündeki giysi
    public EquipmentData equippedFeet;              // Ayakkabı

    // ── 8 yonlu animasyon yapisi ──────────────────────────────────────────
    // Her yon icin tek bir blok: 1 idle sprite + istedigin kadar run frame.
    // PlayerDirectionSprite.cs ile birebir eslesiyor.
    // Inspector'da her yon katlanabilir blok olarak gorunur.
    [System.Serializable]
    public class DirectionData
    {
        public Sprite idle;        // Duruyorken gozukecek sprite
        public Sprite[] runFrames; // Kosarken donecek kareler (istedigin kadar ekle)
    }

    [Header("8 Yon Animasyonlari")]
    public DirectionData north;
    public DirectionData south;
    public DirectionData east;
    public DirectionData west;
    public DirectionData northEast;
    public DirectionData northWest;
    public DirectionData southEast;
    public DirectionData southWest;

    [Header("Prefab (opsiyonel)")]
    public GameObject playerPrefab;

    [Header("Magaza / Kilit")]
    public bool isLocked = false;
    [Tooltip("0 = başlangıç karakteri (sahip olunur)")]
    public int unlockPrice = 0;

    [Header("Karakter (Faz 3c)")]
    [Tooltip("Kayıt anahtarı (SaveData.selectedCharacter / unlockedCharacters)")]
    public string id = "Boy";
    public string nameKey = "char_Boy";
    [Tooltip("Özel yetenek açıklaması (Loc anahtarı, boş olabilir)")]
    public string traitKey = "";
    [Tooltip("Dash bekleme süresi (sn); 0 = varsayılan")]
    public float dashCooldown = 0f;
    [Tooltip("Gövde (hurtbox) ölçeği: küçük = taşlar daha zor vurur")]
    public float hurtboxScale = 1f;
    [Tooltip("Oyun sonunda toplanan altın çarpanı")]
    public float coinMultiplier = 1f;
    public bool startWithShield = false;
    [Tooltip("Menüde karakter kartındaki portre (boşsa güney idle)")]
    public Sprite portrait;

    public Sprite Portrait => portrait != null ? portrait : (south != null ? south.idle : null);
}

public enum PlayerType
{
    Boy,
    Girl,
    Monster,
    Custom
}
