using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Arena yerleşimi: oynanabilir alan, duvarlar, powerup konumları, mermi sınırları.
/// </summary>
public class ArenaTests
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        KakTestUtil.ResetWorld();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        KakTestUtil.ResetWorld();
        yield return null;
    }

    static ArenaAutoLayout Arena()
    {
        var a = Object.FindAnyObjectByType<ArenaAutoLayout>();
        Assert.IsNotNull(a, "ArenaAutoLayout yok");
        return a;
    }

    [UnityTest]
    public IEnumerator Poweruplar_OynanabilirAlandaVeFirlaticilardanUzakDogar()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.MakePlayerSafe();
        var arena = Arena();
        Rect play = arena.PlayableWorldRect;

        var spawner = Object.FindAnyObjectByType<PowerupSpawner>();
        Assert.IsNotNull(spawner, "PowerupSpawner yok");
        spawner.SetSpawnInterval(0.05f, 0.1f);

        // Oyuncuyu kenara çek ki powerup'ları toplamasın
        var player = Object.FindAnyObjectByType<PlayerMovement2D>();
        player.InputOverride = new Vector2(-1f, -1f);

        var seen = new HashSet<int>();
        var bad = new List<string>();
        var corners = new[] { arena.topLeftSpawner, arena.topRightSpawner, arena.bottomLeftSpawner, arena.bottomRightSpawner };
        float end = Time.realtimeSinceStartup + 6f;
        while (Time.realtimeSinceStartup < end)
        {
            foreach (var p in Object.FindObjectsByType<PowerupPickup>(FindObjectsSortMode.None))
            {
                if (!seen.Add(p.GetInstanceID())) continue;
                Vector2 pos = p.transform.position;
                // FloatingItem süzülmesi için küçük dikey tolerans
                if (pos.x < play.xMin || pos.x > play.xMax || pos.y < play.yMin - 0.2f || pos.y > play.yMax + 0.2f)
                    bad.Add("alan dışı " + pos);
                foreach (var c in corners)
                    if (c != null && ((Vector2)c.position - pos).magnitude < spawner.spawnerClearance - 0.25f)
                        bad.Add("fırlatıcıya yakın " + pos + " ↔ " + c.name);
            }
            yield return null;
        }

        Debug.Log($"[ArenaTests] {seen.Count} powerup gözlendi, oynanabilir alan {play}");
        Assert.GreaterOrEqual(seen.Count, 8, "Yeterince powerup çıkmadı");
        Assert.IsEmpty(bad, string.Join("\n", bad));
    }

    [UnityTest]
    public IEnumerator Oyuncu_DuvarinIcYuzundeDurur()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.MakePlayerSafe();
        Rect play = Arena().PlayableWorldRect;
        var player = Object.FindAnyObjectByType<PlayerMovement2D>();

        var dirs = new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        foreach (var d in dirs)
        {
            player.InputOverride = d;
            yield return KakTestUtil.WaitReal(2.5f);
            Vector2 p = player.transform.position;
            // Oyuncu merkezi alanın içinde kalmalı ve duvara 1 birimden fazla uzak durmamalı
            Assert.IsTrue(play.Contains(p), $"Oyuncu oynanabilir alanın dışına çıktı: {p} alan {play} yön {d}");
            float gap = d.x > 0 ? play.xMax - p.x : d.x < 0 ? p.x - play.xMin : d.y > 0 ? play.yMax - p.y : p.y - play.yMin;
            Assert.Less(gap, 1.0f, $"Oyuncu duvara ulaşamadı (görünmez duvar?) boşluk {gap:F2} yön {d}");
        }
        player.InputOverride = null;
    }

    [UnityTest]
    public IEnumerator Mermiler_DuvariAsmadanKaybolur()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.MakePlayerSafe();
        Rect play = Arena().PlayableWorldRect;
        Rect limit = Rect.MinMaxRect(play.xMin - 0.6f, play.yMin - 0.6f, play.xMax + 0.6f, play.yMax + 0.6f);

        int maxSeen = 0;
        var bad = new List<string>();
        float end = Time.realtimeSinceStartup + 8f;
        while (Time.realtimeSinceStartup < end)
        {
            maxSeen = Mathf.Max(maxSeen, Projectile.Active.Count);
            foreach (var pr in Projectile.Active)
            {
                Vector2 p = pr.transform.position;
                if (!limit.Contains(p) && bad.Count < 5) bad.Add(p.ToString());
            }
            yield return null;
        }
        Assert.Greater(maxSeen, 0, "Hiç mermi görülmedi");
        Assert.IsEmpty(bad, "Duvarı aşan mermiler: " + string.Join(", ", bad));
    }
}
