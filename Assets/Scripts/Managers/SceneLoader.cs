using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahne gecislerini yoneten yardimci sinif.
/// Her yerden cagrilabilir, MonoBehaviour degildir.
/// </summary>
public static class SceneLoader
{
    public const string MENU_SCENE = "MainMenu";
    public const string GAME_SCENE = "SampleScene";

    /// <summary>
    /// Oyun sahnesine gecer.
    /// </summary>
    public static void LoadGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GAME_SCENE);
    }

    /// <summary>
    /// Ana menuye doner.
    /// </summary>
    public static void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MENU_SCENE);
    }

    /// <summary>
    /// Aktif sahneyi yeniden yukler (retry).
    /// </summary>
    public static void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
