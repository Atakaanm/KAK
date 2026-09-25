using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>Faz 4 arayüz davranışları.</summary>
public class UiTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator Menu_AyarAnahtari_KaydaYazar()
    {
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        KakToggle music = null;
        foreach (var t in Object.FindObjectsByType<KakToggle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.setting == KakToggle.Setting.Music) music = t;
        Assert.IsNotNull(music, "Müzik anahtarı yok");
        bool before = SaveSystem.Data.settings.music;
        music.OnPointerClick(null);
        Assert.AreEqual(!before, SaveSystem.Data.settings.music, "Anahtar kayda yazmadı");
        music.OnPointerClick(null);
        Assert.AreEqual(before, SaveSystem.Data.settings.music);
    }

    [UnityTest]
    public IEnumerator OyunSonu_SkorSayilir_ButonlarAktiflesir()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var gm = GameManager.Instance;
        Assert.IsNotNull(gm.gameOverScreen, "GameOverScreen bağlı değil");
        yield return KakTestUtil.WaitReal(1.5f); // skor birikmesi için
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => gm.gameOverPanel.activeInHierarchy, 3f, "oyun sonu paneli açılmadı");
        Assert.IsFalse(gm.gameOverScreen.buttons.interactable, "Butonlar animasyon bitmeden tıklanabilir olmamalı");
        yield return KakTestUtil.WaitUntil(() => gm.gameOverScreen.buttons.interactable && gm.gameOverScreen.buttons.alpha > 0.99f, 5f, "oyun sonu butonları aktifleşmedi");
        Assert.Greater(gm.scoreManager.ScoreInt, 0);
        Assert.AreEqual(gm.scoreManager.ScoreInt.ToString(), gm.gameOverScreen.scoreText.text, "Skor sayma son değerde bitmedi");
    }

    [UnityTest]
    public IEnumerator Pause_YenidenBaslat_YeniOyunAcar()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var pm = Object.FindAnyObjectByType<PauseManager>();
        pm.Pause();
        Assert.AreEqual(0f, Time.timeScale);
        var oldGm = GameManager.Instance;
        pm.restartButton.onClick.Invoke();
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance != null && GameManager.Instance != oldGm, 5f, "yeniden başlamadı");
        yield return KakTestUtil.WaitReal(0.5f);
        Assert.IsFalse(KakTime.Paused, "Yeni oyun duraklatılmış başladı");
        Assert.AreEqual(1f, Time.timeScale, 0.001f);
    }
}
