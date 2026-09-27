using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 11 G5: tek telefonda iki kişilik mod.</summary>
public class IkiKisilikTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { GameSettings.TwoPlayer = false; KakTestUtil.ResetWorld(); yield return null; }

    [UnityTest]
    public IEnumerator IkiOyuncu_BiriDuserOyunSurer_Doner_IkisiDuserseBiter()
    {
        GameSettings.TwoPlayer = true;
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;

        Assert.AreEqual(2, PlayerRegistry.All.Count, "İki oyuncu doğmadı");
        var lm = LevelManager.Instance;
        Assert.IsNotNull(lm.SecondPlayer);
        Assert.AreEqual("Boy", Object.FindAnyObjectByType<LevelManager>().CurrentCharacter.id, "1. oyuncu Ata olmalı");
        Assert.AreEqual("Ada", lm.SecondPlayer.playerData.id, "2. oyuncu Ada olmalı");
        var ada = CharacterCatalog.Load().Find("Ada");
        Assert.AreSame(ada.south.idle, lm.SecondPlayer.GetComponentInChildren<PlayerDirectionSprite>().south.idle, "2. oyuncu Ada gibi görünmüyor");
        Assert.AreNotSame(PlayerRegistry.All[0].healthUI, PlayerRegistry.All[1].healthUI, "Kalp satırları ayrı olmalı");
        Assert.AreNotSame(PlayerRegistry.All[0].GetComponent<PlayerMovement2D>().joystick, lm.SecondPlayer.joystick, "Joystick'ler ayrı olmalı");

        var gm = GameManager.Instance;
        gm.respawnSeconds = 1f;
        var p1 = PlayerRegistry.All[0];
        var p2 = PlayerRegistry.All[1];

        KakTestUtil.KillPlayer(p1);
        yield return null;
        Assert.IsFalse(gm.IsGameOver, "Biri düşünce oyun bitti");
        Assert.IsTrue(p1.IsDead);
        Assert.Greater(p1.RespawnRemaining, 0f, "Dönüş sayacı başlamadı");
        Assert.AreEqual(1, PlayerRegistry.AliveCount);

        yield return KakTestUtil.WaitUntil(() => !p1.IsDead, 3f, "Düşen oyuncu dönmedi");
        Assert.IsTrue(p1.IsInvulnerable, "Dönüşte dokunulmazlık yok");
        Assert.IsFalse(gm.IsGameOver);

        // İkisi aynı anda düşerse oyun biter
        KakTestUtil.KillPlayer(p2);
        yield return null;
        Assert.IsFalse(gm.IsGameOver);
        KakTestUtil.KillPlayer(p1);
        yield return null;
        Assert.IsTrue(gm.IsGameOver, "İkisi de düşünce oyun bitmedi");
    }

    [UnityTest]
    public IEnumerator TekKisilikte_TekOyuncu()
    {
        GameSettings.TwoPlayer = false;
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        Assert.AreEqual(1, PlayerRegistry.All.Count);
        Assert.IsNull(LevelManager.Instance.SecondPlayer);
    }

    [UnityTest]
    public IEnumerator TaslarIkiOyuncuyaDaVurur()
    {
        // Eski hata: statik PlayerHitbox.Hurt yalnız son oyuncuyu tutuyordu → 1. oyuncu taşlardan etkilenmiyordu
        GameSettings.TwoPlayer = true;
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        foreach (var ph in PlayerRegistry.All.ToArray())
        {
            ph.SetMaxHealth(3);
            int before = ph.CurrentHealth;
            Vector3 p = ph.transform.position;
            Projectile.Launch(prefab, null, p + new Vector3(0f, 2f, 0f), Vector2.down, 3f); // yukarıdan: diğer oyuncuya değmez
            yield return KakTestUtil.WaitUntil(() => ph.CurrentHealth < before, 3f, ph.name + " taştan etkilenmedi");
        }
    }
}
