using UnityEngine;

/// <summary>
/// Firlatici (spawner) verilerini tutan ScriptableObject.
/// Unity Editorede: Assets > Create > KacAtaKac > Spawner Data
/// Tas atan, top atan gibi farkli firlatilar icin ayri ayri olusturulur.
/// </summary>
[CreateAssetMenu(fileName = "New SpawnerData", menuName = "KacAtaKac/Spawner Data")]
public class SpawnerData : ScriptableObject
{
    [Header("Genel Bilgi")]
    public string spawnerName = "RockThrower";

    [Header("Ates Ayarlari")]
    public float shootInterval = 1.5f;
    public ProjectileData projectileData;

    [Header("Gorsel - 8 Yon Idle Spritelari")]
    public Sprite idleNorth;
    public Sprite idleSouth;
    public Sprite idleEast;
    public Sprite idleWest;
    public Sprite idleNorthEast;
    public Sprite idleNorthWest;
    public Sprite idleSouthEast;
    public Sprite idleSouthWest;

    [Header("Gorsel - 8 Yon Saldiri Animasyonlari")]
    public Sprite[] attackNorth;
    public Sprite[] attackSouth;
    public Sprite[] attackEast;
    public Sprite[] attackWest;
    public Sprite[] attackNorthEast;
    public Sprite[] attackNorthWest;
    public Sprite[] attackSouthEast;
    public Sprite[] attackSouthWest;

    [Header("Prefab (opsiyonel)")]
    public GameObject spawnerPrefab;
}
