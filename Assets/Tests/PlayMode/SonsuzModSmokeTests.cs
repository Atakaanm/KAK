using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Sonsuz Mod'un temel akışlarını doğrulayan duman testleri.
/// Her faz sonunda hepsi yeşil olmalı (regresyon testi).
/// </summary>
public class SonsuzModSmokeTests
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        KakTestUtil.ResetWorld();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        KakTestUtil.ResetWorld();
        yield return null;
    }

    [UnityTest]
    public IEnumerator OyunSahnesi_DogrudanAcilinca_LevelDataUygulanir()
    {
        yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        yield return KakTestUtil.WaitReal(0.5f);

        Assert.IsNotNull(LevelManager.Instance, "LevelManager yok");
        Assert.IsNotNull(LevelManager.Instance.currentLevel,
            "Sahne doğrudan açılınca LevelData uygulanmadı (defaultLevel boş)");
    }

    [UnityTest]
    public IEnumerator OyunSahnesi_SkorZamanlaArtar()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.MakePlayerSafe();
        int s0 = KakTestUtil.Score();
        yield return KakTestUtil.WaitReal(1.5f);
        int s1 = KakTestUtil.Score();
        Assert.Greater(s1, s0, "Skor artmıyor");
    }

    [UnityTest]
    public IEnumerator Menu_Oyna_OyunuLevelDataIleBaslatir()
    {
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        Assert.IsNotNull(menu, "MainMenuController yok");
        menu.OnPlayClicked();

        yield return KakTestUtil.WaitUntil(() => SceneManager.GetActiveScene().name == KakTestUtil.GameScene, 15f, "oyun sahnesi yüklenmedi");
        yield return KakTestUtil.WaitReal(0.5f);

        Assert.IsNotNull(LevelManager.Instance, "LevelManager yok");
        Assert.IsNotNull(LevelManager.Instance.currentLevel, "Menüden gelince LevelData uygulanmadı");
        KakTestUtil.MakePlayerSafe();
        int s0 = KakTestUtil.Score();
        yield return KakTestUtil.WaitReal(1f);
        Assert.Greater(KakTestUtil.Score(), s0, "Skor artmıyor");
    }

    [UnityTest]
    public IEnumerator Olum_GameOverPaneliAcilir()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.KillPlayer();
        yield return null;

        Assert.IsTrue(GameManager.Instance.IsGameOver, "Ölüm sonrası IsGameOver false");
        Assert.IsNotNull(GameManager.Instance.gameOverPanel, "gameOverPanel referansı yok");
        // Panel kısa ölüm yavaş çekiminden sonra açılır
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance.gameOverPanel.activeInHierarchy, 3f, "Game Over paneli açılmadı");
        Assert.AreEqual(0f, Time.timeScale, "Oyun sonunda zaman durmalı");
    }

    /// <summary>
    /// Bilinen kritik hata (Faz 1.1): GameManager DontDestroyOnLoad olduğu için
    /// Retry sonrası eski örnek isGameOver=true ile yaşar.
    /// </summary>
    [UnityTest]
    public IEnumerator Retry_IkinciOyunTamamenCalisir()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.KillPlayer();
        yield return null;
        Assert.IsTrue(GameManager.Instance.IsGameOver);

        GameManager.Instance.RetryGame();
        yield return KakTestUtil.WaitUntil(() => Time.timeScale > 0.5f && Object.FindAnyObjectByType<PlayerHealth>() != null, 15f, "Retry sonrası sahne gelmedi");
        yield return KakTestUtil.WaitReal(0.5f);

        Assert.IsFalse(GameManager.Instance.IsGameOver, "Retry sonrası oyun hâlâ 'bitti' durumunda");
        Assert.IsNotNull(LevelManager.Instance, "Retry sonrası LevelManager yok");
        Assert.IsNotNull(LevelManager.Instance.currentLevel, "Retry sonrası LevelData uygulanmadı");

        KakTestUtil.MakePlayerSafe();
        int s0 = KakTestUtil.Score();
        yield return KakTestUtil.WaitReal(1f);
        Assert.Greater(KakTestUtil.Score(), s0, "Retry sonrası skor artmıyor");

        KakTestUtil.KillPlayer();
        yield return null;
        Assert.IsTrue(GameManager.Instance.IsGameOver, "İkinci ölümde Game Over tetiklenmedi");
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance.gameOverPanel != null && GameManager.Instance.gameOverPanel.activeInHierarchy,
            3f, "İkinci ölümde Game Over paneli açılmadı");
    }

    [UnityTest]
    public IEnumerator GameOver_Menu_TekrarOyna_Calisir()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.KillPlayer();
        yield return null;
        GameManager.Instance.GoToMainMenu();
        yield return KakTestUtil.WaitUntil(() => SceneManager.GetActiveScene().name == KakTestUtil.MenuScene, 15f, "menü yüklenmedi");
        yield return null;

        var menu = Object.FindAnyObjectByType<MainMenuController>();
        Assert.IsNotNull(menu);
        menu.OnPlayClicked();
        yield return KakTestUtil.WaitUntil(() => SceneManager.GetActiveScene().name == KakTestUtil.GameScene, 15f, "oyun sahnesi yüklenmedi");
        yield return KakTestUtil.WaitReal(0.5f);

        Assert.IsFalse(GameManager.Instance.IsGameOver, "Menüden dönüşte oyun 'bitti' durumunda");
        KakTestUtil.MakePlayerSafe();
        int s0 = KakTestUtil.Score();
        yield return KakTestUtil.WaitReal(1f);
        Assert.Greater(KakTestUtil.Score(), s0, "Menüden dönüşte skor artmıyor");
    }

    [UnityTest]
    public IEnumerator Oyun_20Saniye_HataLoguOlmadanCalisir()
    {
        var problems = new System.Collections.Generic.List<string>();
        Application.LogCallback onLog = (msg, stack, type) =>
        {
            if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                problems.Add(type + ": " + msg);
        };
        Application.logMessageReceived += onLog;

        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.MakePlayerSafe();
        yield return KakTestUtil.WaitReal(20f);

        Application.logMessageReceived -= onLog;
        Assert.IsEmpty(problems, "20 sn oyunda uyarı/hata logu:\n" + string.Join("\n", problems));
    }
}
