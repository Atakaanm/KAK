using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 3c.4: görevler oyun sonunda işlenir ve ekranda gösterilir.</summary>
public class GorevTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator GorevlerKapaliyken_BlokGizli()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance.IsGameOver, 3f, "oyun bitmedi");
        Assert.IsNull(GameManager.Instance.LastMissions);
        var gos = Object.FindAnyObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        yield return KakTestUtil.WaitReal(1.2f);
        Assert.IsFalse(gos.missionsBlock.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator OyunSonu_GorevTamamlanir_OdulVeSatir()
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = FeatureGate.MissionsGames;
        d.missions = new List<MissionState>
        {
            MissionSystem.Create(MissionType.PlayGames, 0),
            MissionSystem.Create(MissionType.NearMisses, 0),
            MissionSystem.Create(MissionType.Dashes, 0),
        };
        d.missions[0].progress = d.missions[0].target - 1; // bu oyunla tamamlanacak
        int reward = d.missions[0].reward;
        yield return KakTestUtil.LoadGameWithLevel();
        int coinsBefore = d.coins;
        KakTestUtil.KillPlayer();
        yield return KakTestUtil.WaitUntil(() => GameManager.Instance.IsGameOver, 3f, "oyun bitmedi");
        Assert.AreEqual(reward, GameManager.Instance.MissionReward);
        Assert.AreEqual(1, GameManager.Instance.JustCompleted.Count);
        Assert.GreaterOrEqual(d.coins, coinsBefore + reward, "Görev ödülü cüzdana eklenmedi");
        var gos = Object.FindAnyObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        yield return KakTestUtil.WaitUntil(() => gos.missionsBlock.gameObject.activeSelf, 4f, "görev bloğu görünmedi");
        StringAssert.Contains("+" + reward, gos.missionProgress[0].text);
    }
}
