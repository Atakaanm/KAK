using UnityEngine;

/// <summary>
/// Mermi verilerini tutan ScriptableObject.
/// Unity Editorede: Assets > Create > KacAtaKac > Projectile Data
/// Tas, top, ates topu gibi farkli mermiler icin ayri ayri olusturulur.
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

    [Header("Prefab ve Gorsel")]
    public GameObject projectilePrefab;
    public Sprite projectileSprite;
}
