using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Ses sistemi: her sahnede AudioManager var, doğru müzik çalıyor, efektler bağlı.</summary>
public class AudioTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator OyunSahnesi_OyunMuzigiCalar_KlipleriBagli()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        var am = AudioManager.Instance;
        Assert.IsNotNull(am, "AudioManager oluşmadı");
        Assert.IsNotNull(am.gameMusic, "Oyun müziği bağlı değil");
        Assert.AreEqual(am.gameMusic, am.musicSource.clip, "Oyun sahnesinde oyun müziği çalmalı");
        Assert.IsTrue(am.musicSource.isPlaying, "Müzik çalmıyor");
        foreach (var c in new[] { am.hitSfx, am.deathSfx, am.shootSfx, am.scoreSfx, am.nearMissSfx, am.dashSfx, am.meteorSfx, am.buttonClickSfx })
            Assert.IsNotNull(c, "Eksik efekt klibi");
    }

    [UnityTest]
    public IEnumerator Menu_MenuMuzigiCalar()
    {
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        yield return null;
        var am = AudioManager.Instance;
        Assert.IsNotNull(am);
        Assert.AreEqual(am.menuMusic, am.musicSource.clip);
    }
}
