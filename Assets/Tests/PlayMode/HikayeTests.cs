using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 11 G6: ilk açılış hikâyesi.</summary>
public class HikayeTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator IlkAcilista_HikayeGosterilir_BirKez()
    {
        SaveSystem.Data.seen.Remove(IntroStory.SeenKey);
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        yield return null;
        var intro = Object.FindAnyObjectByType<IntroStory>(FindObjectsInactive.Include);
        Assert.IsNotNull(intro, "Hikâye paneli yok");
        Assert.IsTrue(intro.gameObject.activeInHierarchy, "İlk açılışta hikâye görünmedi");
        Assert.AreEqual(3, intro.pages.Length);
        Assert.IsTrue(intro.pages[0].activeSelf && !intro.pages[1].activeSelf);
        intro.OnNext();
        Assert.IsTrue(intro.pages[1].activeSelf, "İLERİ çalışmadı");
        intro.OnSkip();
        Assert.IsFalse(intro.gameObject.activeSelf);
        Assert.IsTrue(SaveSystem.Data.HasSeen(IntroStory.SeenKey), "Görüldü kaydedilmedi");

        // İkinci açılışta yok
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        yield return null;
        intro = Object.FindAnyObjectByType<IntroStory>(FindObjectsInactive.Include);
        Assert.IsFalse(intro.gameObject.activeInHierarchy, "Hikâye ikinci kez gösterildi");
    }
}
