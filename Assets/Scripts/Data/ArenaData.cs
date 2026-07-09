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

    [Header("Spawner Yerlesim Ayarlari")]
    public float spawnerInsetX = 1.0f;
    public float spawnerInsetY = 1.0f;
    public float spawnerTopDepthOffset = 0.35f;

    [Header("Prefab (opsiyonel)")]
    public GameObject arenaPrefab;
}
