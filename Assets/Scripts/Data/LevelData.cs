using UnityEngine;

/// <summary>
/// Seviye verilerini tutan ScriptableObject.
/// Unity Editorede: Assets > Create > KacAtaKac > Level Data
/// Her level icin bir tane olusturulur. Hangi arena, hangi karakter,
/// hangi spawnerlar ve hangi ozellikler acik — hepsi burada.
/// Endless mod icin de ayni yapi kullanilir.
/// </summary>
[CreateAssetMenu(fileName = "New LevelData", menuName = "KacAtaKac/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Genel Bilgi")]
    public string levelName = "Level 1";
    public int levelIndex = 0;
    public LevelType levelType = LevelType.Endless;

    [Header("Arena")]
    public ArenaData arenaData;

    [Header("Oyuncu")]
    public PlayerData playerData;

    [Header("Firlaticilar")]
    public SpawnerData[] spawnerDataList; // Her kose icin bir SpawnerData

    [Header("Dalgalar (Stage modu)")]
    [Tooltip("levelType == Stage ise bu dalgalar WaveManager tarafından işlenir")]
    public WaveData[] waves;

    [Header("Güçlendirmeler (Powerups)")]
    public PowerupData[] availablePowerups; // Bu levida çıkabilecek poweruplar

    [Header("Zorluk Asamalari (Endless mod)")]
    public DifficultyStageData[] difficultyStages;

    [Header("Stage Mod Ayarlari")]
    public int targetScore = 0;        // Stage modda kazanma kosulu (0 = yok)
    public float timeLimit = 0f;       // Sure siniri (0 = sinir yok)

    [Header("Ozellik Acma/Kapama")]
    public bool hasKey = false;         // Bu levelde anahtar var mi
    public bool hasCoins = false;       // Bu levelde jeton/para var mi
    public bool hasBoss = false;        // Bu levelde boss var mi
    public bool enableEndlessScore = true; // Skor sayaci acik mi
    public bool enableDifficulty = true;   // Zorluk artisi acik mi

    [Header("Magaza / Kilit")]
    public bool isLocked = false;
    public int unlockPrice = 0;
}

public enum LevelType
{
    Endless, // Sonsuz mod: skor arttikca zorlasir
    Stage    // Level-bazli: belirli hedef, belirli arena
}
