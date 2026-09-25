using UnityEngine;

/// <summary>
/// Arena verilerini tutan ScriptableObject.
/// Unity Editorede: Assets > Create > KacAtaKac > Arena Data
/// Zindan, stadyum, orman gibi farkli arenalar icin ayri ayri olusturulur.
/// </summary>
[CreateAssetMenu(fileName = "New ArenaData", menuName = "KacAtaKac/Arena Data")]
public class ArenaData : ScriptableObject
{
    [Header("Genel Bilgi")]
    public string arenaName = "Dungeon";

    [Header("Gorsel")]
    public Sprite arenaSprite;

    [Header("Fizik & Zemin Kuralları (Friction & Modifiers)")]
    public float floorFriction = 1.0f;      // 1.0=Normal zemin, 0.1=Kaygan Buz, 2.0=Hemen durur
    public float movementSpeedMultiplier = 1.0f; // 1.0=Normal hız, 0.5=Bataklık yavaşlatması

    [Header("Boyut (World Units)")]
    public float arenaWidth = 10f;
    public float arenaHeight = 10f;

    [Header("Duvar Ayarlari")]
    public float wallThickness = 0.5f;
    public float wallInset = 0.15f;

    [Header("Oynanabilir Alan (arena görseli içinde, 0-1 normalize)")]
    [Tooltip("Görseldeki iç zemin: x,y sol-alt köşe; width,height boyut. Duvar colliderları, powerup alanı ve mermi sınırları buradan hesaplanır.")]
    public Rect playableAreaNormalized = new Rect(0.0795f, 0.0795f, 0.841f, 0.841f);

    [Header("Fırlatıcı Duruş Noktaları (görsel içinde, 0-1 normalize, ayak konumu)")]
    public bool useSpawnerAnchors = true;
    public Vector2 anchorTopLeft = new Vector2(0.138f, 0.834f);
    public Vector2 anchorTopRight = new Vector2(0.862f, 0.834f);
    public Vector2 anchorBottomLeft = new Vector2(0.130f, 0.126f);
    public Vector2 anchorBottomRight = new Vector2(0.870f, 0.126f);

    [Header("Spawner Yerlesim Ayarlari (anchor kapalıysa)")]
    public float spawnerInsetX = 1.0f;
    public float spawnerInsetY = 1.0f;
    public float spawnerTopDepthOffset = 0.35f;

    [Header("Prefab (opsiyonel)")]
    public GameObject arenaPrefab;
}
