using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 15 K6: oyunda anahtar — süre eşiklerinde birer (en fazla 3), rekorda +1, oyun sonunda kayda geçer; menüde sandık paneli açılır.</summary>
public class AnahtarTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator SureEsikleri_UcAnahtar_Rekor_KaydaGecer()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        if (EndlessEventManager.Instance != null) EndlessEventManager.Instance.enabled = false;
        var km = KeyMilestones.Instance;
        Assert.IsNotNull(km, "KeyMilestones kurulmadı");
        var sm = GameManager.Instance.scoreManager;
        sm.AdvanceTime(59f);
        yield return null; yield return null;
        Assert.AreEqual(0, km.RunKeys);
        sm.AdvanceTime(2f);
        yield return null; yield return null;
        Assert.AreEqual(1, km.RunKeys, "60. saniyede anahtar gelmedi");
        sm.AdvanceTime(400f);
        for (int i = 0; i < 6; i++) yield return null;
        var e = EconomyData.Load();
        Assert.AreEqual(e.keyMilestones.Length, km.RunKeys, "Anahtar sınırı aşıldı ya da eşik atlandı");

        int before = SaveSystem.Data.keys;
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitReal(0.3f);
        var gm = GameManager.Instance;
        Assert.IsTrue(gm.IsGameOver);
        int expected = km.RunKeys + (gm.IsNewBest ? e.keyOnRecord : 0);
        Assert.AreEqual(expected, gm.RunKeys);
        Assert.AreEqual(before + expected, SaveSystem.Data.keys, "Anahtar kayda geçmedi");
    }

    [UnityTest]
    public IEnumerator Menu_SandikPaneli_AcilirAcilisOynar()
    {
        SaveSystem.Data.keys = 1;
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        Assert.IsNotNull(menu.chestPanel, "Sandık paneli kurulmamış");
        menu.OnChestsClicked();
        yield return null;
        var cp = menu.chestPanel.GetComponent<ChestPanel>();
        Assert.IsTrue(menu.chestPanel.activeSelf);
        cp.Open(0);
        Assert.AreEqual(0, SaveSystem.Data.keys, "Ahşap sandık anahtarı düşmedi (ahşapta ek anahtar şansı yok)");
        Assert.IsNotNull(cp.opening);
        Assert.IsTrue(cp.opening.Playing, "Açılış animasyonu başlamadı");
        cp.opening.Skip();
        yield return KakTestUtil.WaitReal(0.2f);
        Assert.IsTrue(cp.opening.okButton.activeSelf, "Atlayınca TAMAM görünmedi");
        cp.opening.Close();
        Assert.IsFalse(cp.opening.gameObject.activeSelf);
        Assert.AreEqual(1, SaveSystem.Data.chestsOpened);
    }
}
