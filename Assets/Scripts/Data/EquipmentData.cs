using UnityEngine;

/// <summary>
/// Ekipman verisi. Faz 15 K7 (altyapı, KakScope.Equipment kapalı): survivor.io gibi yuvalar ve nadirlik; özellikler StatModifier
/// (nadirlik başına × (1 + 0,35·nadirlik), seviye başına +%5). Ana el/yan el eşyaları HeldItem ile ele oturur.
/// Sandıktan çıkar (ileride), 3 aynı parça bir üst nadirliğe birleşir (Equipment.Merge).
/// </summary>
[CreateAssetMenu(fileName = "New Equipment", menuName = "KacAtaKac/RPG/Equipment Data")]
public class EquipmentData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string equipmentName = "Leather Hat";
    public EquipmentSlot slot = EquipmentSlot.Head;
    [Tooltip("Kayıt anahtarı (SaveData.gear)")]
    public string id = "";
    public string nameKey = "";

    [Header("Özellikler (Faz 15 K7)")]
    [Tooltip("Sıradan nadirlikteki etkiler; üst nadirlikler çarpanla güçlenir")]
    public StatModifier[] modifiers;

    [Header("Değer Artışları (eski, kullanılmıyor)")]
    public int healthBonus = 0;
    public float moveSpeedBonus = 0f;
    public float powerupDurationBonus = 0f;

    [Header("Görsel")]
    public Sprite inventoryIcon;
    [Tooltip("Ana el / yan el: elde görünen sprite (pivot = tutma noktası çevresi)")]
    public Sprite heldSprite;
    [Tooltip("Pivottan ele düzeltme (eşya pikseli)")]
    public Vector2 gripOffsetPx;
}

public enum EquipmentSlot
{
    Head,       // Şapka, miğfer vb.
    Body,       // Kıyafet, zırh vb.
    Feet,       // Ayakkabı, bot (özellikle hız veren)
    MainHand,   // Faz 15 K7: kılıç, meşale (sağ el)
    OffHand     // Faz 15 K7: kalkan (sol el)
}

public enum EquipmentRarity { Common, Good, Rare, Epic, Legend }
