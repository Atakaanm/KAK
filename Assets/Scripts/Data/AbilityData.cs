using UnityEngine;

/// <summary>
/// Karakterin butonla basıp kullandığı (Dash, Kalkan) temel yetenek verilerini tutar.
/// </summary>
[CreateAssetMenu(fileName = "New AbilityData", menuName = "KacAtaKac/RPG/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string abilityName = "Dash";
    public AbilityType type = AbilityType.Dash;

    [Header("Zamanlamalar (Saniye)")]
    public float cooldown = 3f;             // Ne sıklıkla kullanılabilir
    public float duration = 0.2f;           // Ne kadar sürüyor (Dash için çok kısa)

    [Header("Büyüklük / Güç")]
    public float magnitude = 3f;            // Örn: Dash ise hızı 3 ile çarpar

    [Header("Görsel")]
    public Sprite icon;                     // Butonda gözükecek ikon
}

public enum AbilityType
{
    Dash,           // İleriye anlık hızlı fırlama
    Shield,         // Karakterin kendine kalkan alması
    Shockwave       // Etraftaki taşları geri itme vb.
}
