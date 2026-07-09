using UnityEngine;

/// <summary>
/// Oyuncunun satın alıp giyebileceği (RPG Ekipmanı) eşyaların verilerini tutar.
/// </summary>
[CreateAssetMenu(fileName = "New Equipment", menuName = "KacAtaKac/RPG/Equipment Data")]
public class EquipmentData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string equipmentName = "Leather Hat";
    public EquipmentSlot slot = EquipmentSlot.Head;

    [Header("Değer Artışları (Bonus Stats)")]
    public int healthBonus = 0;                     // Extra kalp veren miğfer vb.
    public float moveSpeedBonus = 0f;               // Karakter taban hızına ek (örn: 0.5f)
    public float powerupDurationBonus = 0f;         // (örn: 0.2f olursa PowerUp'lar %20 uzun sürer)

    [Header("Görsel (Opsiyonel)")]
    public Sprite inventoryIcon;                    // Mağazada gözüken ikon
    // Eğer kafada görselleşecekse (Player görsel scripti destekliyorsa)
    // public Sprite inGameSprite;                  
}

public enum EquipmentSlot
{
    Head,       // Şapka, miğfer vb.
    Body,       // Kıyafet, zırh vb.
    Feet        // Ayakkabı, bot (özellikle hız veren)
}
