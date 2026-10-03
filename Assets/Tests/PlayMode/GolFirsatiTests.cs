using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 13 F2: gol fırsatı — serbest top, dokununca sürülür, üst kaleye götürünce GOL!, vurulunca düşer.</summary>
public class GolFirsatiTests
{
    const string Level = "Assets/Data/Worlds/Football/Endless_Football_LevelData.asset";
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    static IEnumerator LoadFootball()
    {
        GameSettings.SelectedLevel = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(Level);
        yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        yield return null;
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
    }

    static void Teleport(PlayerHealth ph, Vector2 p)
    {
        var rb = ph.GetComponent<Rigidbody2D>();
        rb.position = p;
        ph.transform.position = p;
    }

    [UnityTest]
    public IEnumerator Top_Al_KaleyeGotur_Gol()
    {
        yield return LoadFootball();
        var gc = Object.FindAnyObjectByType<GoalChance>();
        Assert.IsNotNull(gc, "Futbolda gol fırsatı yok");
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        ph.SetInvulnerable(99f);
        var mv = ph.GetComponent<PlayerMovement2D>();
        gc.SpawnNow();
        Assert.AreEqual(GoalChance.State.OnGround, gc.Current);
        Teleport(ph, gc.BallPosition);
        yield return null; yield return null;
        Assert.AreEqual(GoalChance.State.Dribbling, gc.Current, "Topa dokununca alınmadı");
        Assert.AreSame(ph, gc.Carrier);

        var sm = GameManager.Instance.scoreManager;
        int before = sm.ScoreInt;
        Rect mouth = gc.GoalMouth;
        Teleport(ph, new Vector2(mouth.center.x, mouth.yMin + 0.05f));
        mv.InputOverride = Vector2.up;
        yield return KakTestUtil.WaitUntil(() => gc.Goals > 0, 3f, "Kaleye götürünce gol olmadı");
        mv.InputOverride = null;
        Assert.GreaterOrEqual(sm.ScoreInt - before, gc.goalScore, "Gol bonusu skora eklenmedi");
        Assert.AreEqual(GoalChance.State.Waiting, gc.Current, "Golden sonra top kalkmadı");
    }

    [UnityTest]
    public IEnumerator Vurulunca_TopDuser()
    {
        yield return LoadFootball();
        var gc = Object.FindAnyObjectByType<GoalChance>();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        ph.SetMaxHealth(3);
        gc.SpawnNow();
        Teleport(ph, gc.BallPosition);
        yield return null; yield return null;
        Assert.AreEqual(GoalChance.State.Dribbling, gc.Current);
        ph.TakeDamage(1);
        yield return null;
        Assert.AreEqual(GoalChance.State.OnGround, gc.Current, "Vurulunca top düşmedi");
        Assert.AreEqual(2, ph.CurrentHealth);
    }

    [UnityTest]
    public IEnumerator Zindanda_Yok()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        Assert.IsNull(Object.FindAnyObjectByType<GoalChance>(), "Gol fırsatı yalnız futbolda olmalı");
    }
}
