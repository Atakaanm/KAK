using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 15 K7: eldeki eşya her karede sağ el noktasına oturur; el önde/arkada sıralaması yönle doğru; batıda aynalanır.</summary>
public class ElNoktasiTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    static Sprite Frame(HandAnchorSet set, string spriteName, out HandAnchorSet.Entry entry)
    {
        foreach (var e in set.entries)
            if (e.sprite != null && e.sprite.name == spriteName) { entry = e; return e.sprite; }
        entry = default;
        return null;
    }

    [UnityTest]
    public IEnumerator Mesale_ElNoktasinaOturur_YoneGoreOndeArkada()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var mv = Object.FindAnyObjectByType<PlayerMovement2D>();
        var data = mv.playerData;
        Assert.IsNotNull(data, "Oyuncu verisi yok");
        Assert.IsNotNull(data.hands, "Karakterde el noktaları yok: KacAtaKac/El Noktalarını Kur");
        var vis = mv.GetComponentInChildren<PlayerDirectionSprite>();
        vis.enabled = false; // kareyi elle seçeceğiz
        var body = vis.spriteRenderer;

        var held = HeldItem.Attach(mv.gameObject, Resources.Load<Sprite>("Torch"), "TestTorch");
        yield return null;

        var east = Frame(data.hands, "east", out var eEast);
        Assert.IsNotNull(east, "Doğu duruş karesi el listesinde yok");
        body.sprite = east;
        yield return null; yield return null;
        Assert.IsTrue(held.LastFound, "El noktası bulunamadı (yedek konuma düştü)");
        Assert.IsTrue(held.LastFront, "Doğuda sağ el önde olmalı");
        Assert.Greater(held.Renderer.sortingOrder, body.sortingOrder);
        float ppu = east.pixelsPerUnit;
        Vector3 expect = body.transform.TransformPoint(new Vector3(Mathf.Round(eEast.offsetPx.x), -Mathf.Round(eEast.offsetPx.y), 0f) / ppu);
        Assert.Less(Vector2.Distance(held.transform.position, expect), 0.002f, "Eşya el noktasına oturmadı");

        var west = Frame(data.hands, "west", out _);
        Assert.IsNotNull(west);
        body.sprite = west;
        mv.InputOverride = Vector2.left;
        yield return new WaitForFixedUpdate();
        yield return null; yield return null;
        Assert.IsFalse(held.LastFront, "Batıda sağ el (uzak) arkada olmalı");
        Assert.Less(held.Renderer.sortingOrder, body.sortingOrder);
        Assert.IsTrue(held.Renderer.flipX, "Batıya bakınca eşya aynalanmalı");
        mv.InputOverride = Vector2.zero;
    }
}
