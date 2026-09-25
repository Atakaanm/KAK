using UnityEngine;

/// <summary>
/// Dünya teması: arena görseli, oynanabilir alan, zemin fiziği, çerçeve karoları, ortam rengi, karanlık.
/// Bir dünyanın görünümü ve hissi tek dosyadan değişir (Buz, Stadyum, Karanlık Mahzen...).
/// </summary>
[CreateAssetMenu(fileName = "New WorldTheme", menuName = "KacAtaKac/World Theme")]
public class WorldTheme : ScriptableObject
{
    public string themeId = "dungeon";

    [Header("Arena")]
    public Sprite arenaSprite;
    public Rect playableAreaNormalized = new Rect(0.0795f, 0.0795f, 0.841f, 0.841f);
    public bool useSpawnerAnchors = true;

    [Header("Zemin fiziği")]
    [Tooltip("1 = normal, 0.1-0.3 = buz (kayar, geç durur)")]
    public float floorFriction = 1f;
    public float floorSpeedMultiplier = 1f;

    [Header("Çerçeve karoları (boşsa varsayılan dungeon)")]
    public Sprite backdropTile;
    public Sprite corridorTile;
    public Sprite corridorWall;
    public Sprite ledgeTile;

    [Header("Ortam")]
    public Color cameraBackground = new Color(0.03f, 0.045f, 0.05f, 1f);
    [Tooltip("Oyun alanı üzerine uygulanan renk (sprite çarpımı)")]
    public Color arenaTint = Color.white;
    [Range(0f, 1f)] [Tooltip("Karanlık dünya: oyuncu dışını karartma miktarı")]
    public float darkness = 0f;
    public float lightRadius = 2.2f;

    [Header("Meşaleler")]
    public bool torches = true;
}
