using UnityEngine.SceneManagement;

/// <summary>
/// Sahne gecislerini yoneten yardimci sinif.
/// Her yerden cagrilabilir, MonoBehaviour degildir.
/// Her geçişte zaman ölçeği ve fizik adımı varsayılana döner (SloMo/pause kalıntısı kalmaz).
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
        KakTime.ResetAll();
        SceneManager.LoadScene(GAME_SCENE);
    }

    /// <summary>
    /// Ana menuye doner.
    /// </summary>
    public static void LoadMenu()
    {
        KakTime.ResetAll();
        SceneManager.LoadScene(MENU_SCENE);
    }

    /// <summary>
    /// Aktif sahneyi yeniden yukler (retry).
    /// </summary>
    public static void ReloadCurrentScene()
    {
        KakTime.ResetAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
