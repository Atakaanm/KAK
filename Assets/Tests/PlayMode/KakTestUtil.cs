using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PlayMode testleri için ortak yardımcılar: temiz başlangıç, sahne yükleme, gerçek zamanlı bekleme.
/// </summary>
public static class KakTestUtil
{
    public const string GameScene = SceneLoader.GAME_SCENE;
    public const string MenuScene = "MainMenu";

    /// <summary>
    /// Önceki testten kalan kalıcı (DontDestroyOnLoad) oyun objelerini ve statik seçimleri temizler.
    /// </summary>
    public static void ResetWorld()
    {
        // Testler oyuncunun gerçek kaydına yazmasın ve her test temiz kayıtla başlasın
        // (önceki testlerin oyun sayısı vb. birikmesin: ör. "deneyimli oyuncu" kuralı yanlış tetiklenir)
        string testSave = System.IO.Path.Combine(Application.temporaryCachePath, "kak_playmode_test_save.json");
        SaveSystem.OverridePath = testSave;
        if (System.IO.File.Exists(testSave)) System.IO.File.Delete(testSave);
        SaveSystem.Unload();
        KakTime.ResetAll();
        GameSettings.Reset();
        AdService.ResetForTests();

        foreach (var gm in Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(gm.gameObject);
        foreach (var am in Object.FindObjectsByType<AudioManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(am.gameObject);
    }

    public const string EndlessLevelPath = "Assets/Data/Endless_Level1_LevelData.asset";

    /// <summary>Menüdeki "Oyna" akışıyla aynı: seçili LevelData ile oyun sahnesini açar.</summary>
    public static IEnumerator LoadGameWithLevel()
    {
#if UNITY_EDITOR
        GameSettings.SelectedLevel = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(EndlessLevelPath);
        Assert.IsNotNull(GameSettings.SelectedLevel, "LevelData bulunamadı: " + EndlessLevelPath);
#endif
        yield return LoadScene(GameScene);
    }

    public static IEnumerator LoadScene(string name)
    {
        var op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
        while (!op.isDone) yield return null;
        // Awake/Start'ların çalışması için birkaç kare
        yield return null;
        yield return null;
    }

    /// <summary>timeScale'den bağımsız bekleme.</summary>
    public static IEnumerator WaitReal(float seconds)
    {
        float end = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < end) yield return null;
    }

    /// <summary>Koşul sağlanana kadar (ya da zaman aşımına kadar) bekler.</summary>
    public static IEnumerator WaitUntil(System.Func<bool> condition, float timeout, string description)
    {
        float end = Time.realtimeSinceStartup + timeout;
        while (!condition())
        {
            if (Time.realtimeSinceStartup > end)
                Assert.Fail("Zaman aşımı (" + timeout + " sn): " + description);
            yield return null;
        }
    }

    /// <summary>Oyuncuyu testin süresi boyunca mermilerden etkilenmez yapar.</summary>
    public static void MakePlayerSafe()
    {
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        if (ph != null) ph.MakeGhost(9999f);
    }

    public static void KillPlayer()
    {
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        Assert.IsNotNull(ph, "PlayerHealth bulunamadı");
        // Hayalet/ölümsüzlük korumalarını aşmak için canı doğrudan bitir
        ph.currentHealth = 1;
        typeof(PlayerHealth).GetField("isGhost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, false);
        typeof(PlayerHealth).GetField("isInvincible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, false);
        typeof(PlayerHealth).GetField("hasShield", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, false);
        typeof(PlayerHealth).GetField("invulnerableUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, -1f);
        ph.TakeDamage(1);
    }

    public static int Score()
    {
        var gm = GameManager.Instance;
        Assert.IsNotNull(gm, "GameManager.Instance yok");
        Assert.IsNotNull(gm.scoreManager, "ScoreManager yok");
        return gm.scoreManager.ScoreInt;
    }
}
