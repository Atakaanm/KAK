using UnityEngine;

/// <summary>
/// Sahneler arasi veri tasimak icin kullanilan statik sinif.
/// Menu'den oyun sahnesine hangi LevelData ve PlayerData secildigini aktarir.
/// Bu sinif MonoBehaviour degildir, herhangi bir GameObject'e eklenmez.
/// </summary>
public static class GameSettings
{
    // Secili level — menu'den atanir, oyun sahnesi baslayinca okunur
    public static LevelData SelectedLevel;

    // Secili karakter — menu'den atanir (opsiyonel, LevelData icerisinde de olabilir)
    public static PlayerData SelectedPlayer;

    // En iyi skor
    private const string BEST_SCORE_KEY = "BestScore";

    public static int BestScore
    {
        get => PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(BEST_SCORE_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Level ve Player secimlerini sifirlar.
    /// </summary>
    public static void Reset()
    {
        SelectedLevel = null;
        SelectedPlayer = null;
    }
}
