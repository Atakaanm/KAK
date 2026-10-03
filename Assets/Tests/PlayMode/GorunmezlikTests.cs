using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 12 H4: Görünmezlik — fırlatıcılar göremez, rastgele atar; Hayalet'ten ayrı.</summary>
public class GorunmezlikTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { GameSettings.TwoPlayer = false; KakTestUtil.ResetWorld(); yield return null; }

    static PowerupData Invisible => UnityEditor.AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/InvisibleData.asset");

    static float AngleToPlayer(Projectile p, Transform player)
    {
        Vector2 to = (Vector2)player.position - (Vector2)p.transform.position;
        return Vector2.Angle(p.MoveDirection, to);
    }

    [UnityTest]
    public IEnumerator Gorunmezken_FirlaticilarRastgeleAtar_SanaDegil()
    {
        Assert.IsNotNull(Invisible, "InvisibleData yok (KakBalance.SetupInvisible)");
        yield return KakTestUtil.LoadGameWithLevel();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        PowerupPickup.Apply(Invisible, ph.gameObject, ph.transform.position);
        Assert.IsTrue(ph.IsInvisible);
        Assert.Less(ph.playerSpriteRenderer.color.a, 0.5f, "Görünmez oyuncu saydam görünmeli");
        var shooters = Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        int shots = 0;
        for (int round = 0; round < 6; round++)
            foreach (var s in shooters)
            {
                if (!s.gameObject.activeInHierarchy) continue;
                int before = Projectile.Active.Count;
                s.FireNow();
                if (Projectile.Active.Count > before)
                {
                    var p = Projectile.Active[Projectile.Active.Count - 1];
                    if (p.Motion == ProjectileMotion.Meteor) continue;
                    Assert.Greater(AngleToPlayer(p, ph.transform), s.blindAvoidAngle - 0.5f, "Kör fırlatıcı görünmez oyuncuya doğru attı");
                    shots++;
                }
            }
        Assert.Greater(shots, 0, "Hiç atış yapılmadı");
        yield return null;
        bool anyMark = false;
        foreach (var s in shooters)
        {
            if (!s.gameObject.activeInHierarchy) continue;
            Assert.IsTrue(s.Blind, "Fırlatıcı kör olmalı");
            var mark = s.transform.Find("Confused");
            if (mark != null && mark.GetComponent<SpriteRenderer>().enabled) anyMark = true;
        }
        Assert.IsTrue(anyMark, "Fırlatıcıların başında soru işareti yok");
    }

    [UnityTest]
    public IEnumerator Bitince_YineNisanAlir_GudumluKilitKaybeder()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        ph.MakeInvisible(0.4f);
        Assert.IsNull(PlayerRegistry.NearestVisible(ph.transform.position), "Görünmez oyuncu güdümlü hedef olmamalı");
        yield return new WaitForSeconds(0.6f);
        Assert.IsFalse(ph.IsInvisible);
        Assert.Greater(ph.playerSpriteRenderer.color.a, 0.95f, "Bitince saydam kaldı");
        Assert.AreSame(ph.transform, PlayerRegistry.NearestVisible(ph.transform.position));
        var s = System.Array.Find(Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None), x => x.gameObject.activeInHierarchy);
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        s.overrideProjectile = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Rock_ProjectileData.asset");
        s.FireNow();
        var shot = Projectile.Active[Projectile.Active.Count - 1];
        Assert.Less(AngleToPlayer(shot, ph.transform), 3f, "Görünür olunca fırlatıcı yine oyuncuya nişan almalı");
    }

    [UnityTest]
    public IEnumerator IkiKisilik_GorunenOyuncuyaAtar()
    {
        GameSettings.TwoPlayer = true;
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        var p1 = PlayerRegistry.All[0];
        var p2 = PlayerRegistry.All[1];
        p1.MakeInvisible(10f);
        Assert.AreSame(p2.transform, PlayerRegistry.RandomVisible(), "Görünen tek oyuncu 2. oyuncu");
        var s = System.Array.Find(Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None), x => x.gameObject.activeInHierarchy);
        s.overrideProjectile = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Rock_ProjectileData.asset");
        s.target = p1.transform;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
        s.FireNow();
        var shot = Projectile.Active[Projectile.Active.Count - 1];
        Assert.Less(AngleToPlayer(shot, p2.transform), 3f, "Fırlatıcı görünen oyuncuya atmalı");
        Assert.IsFalse(s.Blind, "Görünen oyuncu varken kör değil");
    }

    [Test]
    public void HayaletAyriKalir()
    {
        Assert.AreNotEqual(PowerupType.Ghost, PowerupType.Invisible);
        Assert.AreEqual(PowerupType.Invisible, Invisible.type);
        Assert.IsFalse(Invisible.harmful);
        Assert.GreaterOrEqual(Invisible.minStage, 1, "İlk saniyelerde çıkmasın");
    }
}
