using UnityEngine;

public enum PetPassive { Magnet, ShieldRegen }

/// <summary>Pet (Faz 3c.5): oyuncuyu takip eden küçük yol arkadaşı, tek pasif yetenek.</summary>
[CreateAssetMenu(fileName = "New PetData", menuName = "KacAtaKac/Pet Data")]
public class PetData : ScriptableObject
{
    public string id = "Firefly";
    public string nameKey = "pet_Firefly";
    public string traitKey = "pettrait_Firefly";
    public int price = 400;
    public Sprite[] frames;
    public float fps = 8f;
    public PetPassive passive = PetPassive.Magnet;
    [Header("Mıknatıs")]
    public float magnetRadius = 2.2f;
    public float magnetSpeed = 7f;
    [Header("Kalkan")]
    public float shieldInterval = 45f;
    [Header("Görünüm")]
    [Tooltip("Arkasında yumuşak ışık (ateşböceği)")]
    public bool glow;
    public Color glowColor = new Color(1f, 0.9f, 0.4f, 0.45f);
    [Tooltip("Uçan pet süzülür, yürüyen yerde durur")]
    public bool flying = true;
}
