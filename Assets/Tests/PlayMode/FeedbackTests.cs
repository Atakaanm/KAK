using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Oyun hissi (sarsıntı, hit-stop, parçacık) ve zaman katmanları (SloMo × pause × hit-stop).</summary>
public class FeedbackTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator Vurus_SarsintiHitStopVeParcacikUretir()
    {
        KakCameraShake.Enabled = true;
        yield return KakTestUtil.LoadGameWithLevel();
        var fb = Object.FindAnyObjectByType<FeedbackManager>();
        Assert.IsNotNull(fb, "FeedbackManager yok");
        Assert.IsNotNull(fb.chips, "Parçacık sistemi yok");
        var composer = Object.FindAnyObjectByType<ScreenComposer>();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();

        ph.TakeDamage(1);
        yield return null;
        Assert.Less(Time.timeScale, 0.1f, "Vuruşta hit-stop olmadı");
        yield return KakTestUtil.WaitReal(0.03f);
        Vector2 camPos = composer.transform.position;
        Assert.Greater((camPos - composer.Current.cameraCenter).magnitude, 0.0001f, "Kamera sarsılmadı");
        Assert.Greater(fb.chips.particleCount, 0, "Parçacık çıkmadı");

        yield return KakTestUtil.WaitReal(0.6f);
        Assert.AreEqual(1f, Time.timeScale, 0.001f, "Hit-stop sonrası zaman normale dönmedi");
        camPos = composer.transform.position;
        Assert.Less((camPos - composer.Current.cameraCenter).magnitude, 0.001f, "Kamera merkeze dönmedi");
    }

    [UnityTest]
    public IEnumerator SloMo_PauseResume_HitStop_BirbiriniBozmaz()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.MakePlayerSafe();
        var pause = Object.FindAnyObjectByType<PauseManager>();
        Assert.IsNotNull(pause);

        GameManager.Instance.TimeSlow(0.4f, 5f);
        yield return null;
        Assert.AreEqual(0.4f, Time.timeScale, 0.001f);

        pause.Pause();
        KakTime.HitStop(0.2f); // pause sırasında yok sayılmalı
        Assert.AreEqual(0f, Time.timeScale);
        yield return KakTestUtil.WaitReal(0.3f);
        pause.Resume();
        Assert.AreEqual(0.4f, Time.timeScale, 0.001f, "Resume SloMo'yu geri getirmedi");

        KakTime.HitStop(0.05f);
        Assert.Less(Time.timeScale, 0.1f);
        yield return KakTestUtil.WaitReal(0.15f);
        Assert.AreEqual(0.4f, Time.timeScale, 0.001f, "Hit-stop sonrası SloMo'ya dönmedi");
    }
}
