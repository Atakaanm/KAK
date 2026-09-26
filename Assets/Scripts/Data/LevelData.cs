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

    [Header("Güçlendirmeler (Powerups)")]
    public PowerupData[] availablePowerups; // Bu levida çıkabilecek poweruplar

    [Header("Zorluk Asamalari (Endless mod)")]
    public DifficultyStageData[] difficultyStages;

    [Header("Bölüm (Stage)")]
    public string levelId = "";
    public LevelGoal goal = LevelGoal.Survive;
    [Tooltip("Survive: bu kadar saniye hayatta kal")]
    public float surviveSeconds = 45f;
    [Tooltip("Goals: bu kadar gol at")]
    public int goalsToWin = 3;
    public WorldTheme theme;
    [Tooltip("Köşe fırlatıcıları bu bölümde çalışsın mı (kaç tanesi)")]
    public int cornerShooters = 0;
    [Tooltip("Köşe fırlatıcılarının atış aralığı ve taşı")]
    public float cornerInterval = 2f;
    public ProjectileData cornerProjectile;
    public EnemySpawn[] enemies;
    [TextArea] public string introKeyTR = "";

    [Header("Ozellik Acma/Kapama")]
    public bool enableDifficulty = true;   // Zorluk artisi acik mi

}

public enum LevelGoal
{
    Survive, // süre dolana kadar hayatta kal
    Goals    // N gol at (Futbol)
}

[System.Serializable]
public class EnemySpawn
{
    public EnemyData data;
    [Tooltip("Oynanabilir alan içinde normalize konum (0-1)")]
    public Vector2 position = new Vector2(0.5f, 0.9f);
}

public enum LevelType
{
    Endless, // Sonsuz mod: skor arttikca zorlasir
    Stage    // Level-bazli: belirli hedef, belirli arena
}
