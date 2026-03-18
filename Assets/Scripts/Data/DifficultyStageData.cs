using UnityEngine;

/// <summary>
/// Sonsuz (Endless) modda zorluk asamalarini tanimlayan ScriptableObject.
/// Unity Editorede: Assets > Create > KacAtaKac > Difficulty Stage
/// Skor araligina gore oyunun zorlugu bu dataya bakilarak ayarlanir.
/// </summary>
[CreateAssetMenu(fileName = "New DifficultyStage", menuName = "KacAtaKac/Difficulty Stage")]
public class DifficultyStageData : ScriptableObject
{
    [Header("Genel Bilgi")]
    public string stageName = "Kolay";

    [Header("Skor Araligi")]
    public int minScore = 0;
    public int maxScore = 100; // -1 = sonsuz (son asama icin)

    [Header("Spawner Ayarlari")]
    public int activeSpawnerCount = 2;
    public float shootIntervalMultiplier = 1.0f;  // 1.0 = normal, 0.5 = 2 kat hizli
    public float projectileSpeedMultiplier = 1.0f; // 1.0 = normal, 1.5 = %50 hizli mermi

    [Header("Mermi Cesitleri (opsiyonel)")]
    public ProjectileData[] availableProjectiles;
}
