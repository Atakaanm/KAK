using UnityEngine.SceneManagement;

/// <summary>
/// Sahne gecislerini yoneten yardimci sinif.
/// Her yerden cagrilabilir, MonoBehaviour degildir.
/// Geçişler kısa kararmayla (SceneFader) yapılır; zaman ölçeği ve fizik adımı varsayılana döner.
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
        SceneFader.Load(GAME_SCENE);
    }

    /// <summary>
    /// Ana menuye doner.
    /// </summary>
    public static void LoadMenu()
    {
        SceneFader.Load(MENU_SCENE);
    }

    /// <summary>
    /// Aktif sahneyi yeniden yukler (retry).
    /// </summary>
    public static void ReloadCurrentScene()
    {
        SceneFader.Load(SceneManager.GetActiveScene().buildIndex);
    }
}
