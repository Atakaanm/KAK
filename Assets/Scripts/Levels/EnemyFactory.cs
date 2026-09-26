using UnityEngine;

/// <summary>Bölüm düşmanlarını çalışma zamanında kurar (görsel, gölge, derinlik sıralaması, davranış).</summary>
public static class EnemyFactory
{
    public static EnemyAgent Create(EnemySpawn spawn, Rect play, GameObject projectilePrefab, Sprite shadowSprite, Transform parent)
    {
        var d = spawn.data;
        var root = new GameObject("Enemy_" + d.enemyName);
        root.transform.SetParent(parent, false);
        Vector3 pos = new Vector3(play.xMin + spawn.position.x * play.width, play.yMin + spawn.position.y * play.height, 0f);

        if (shadowSprite != null)
        {
            var sh = new GameObject("Shadow").AddComponent<SpriteRenderer>();
            sh.transform.SetParent(root.transform, false);
            sh.sprite = shadowSprite;
            sh.color = new Color(0f, 0f, 0f, 0.35f);
            Vector2 sz = shadowSprite.bounds.size;
            sh.transform.localScale = new Vector3(0.8f / sz.x, 0.3f / sz.y, 1f);
            sh.transform.localPosition = new Vector3(0f, -0.48f, 0f);
            sh.sortingOrder = 1;
        }

        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = Vector3.one * d.visualScale;
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.color = d.tint;
        sr.sortingOrder = 50;
        var ysort = visual.AddComponent<YDepthSorter>();
        ysort.baseOrder = 100;

        var agent = root.AddComponent<EnemyAgent>();
        agent.visualRenderer = sr;

        if (d.visuals != null)
        {
            var anim = visual.AddComponent<SpawnerDirectionAnimator>();
            anim.spriteRenderer = sr;
            anim.target = Projectile.PlayerTarget;
            var v = d.visuals;
            anim.north = Dir(v.idleNorth, v.attackNorth); anim.south = Dir(v.idleSouth, v.attackSouth);
            anim.east = Dir(v.idleEast, v.attackEast); anim.west = Dir(v.idleWest, v.attackWest);
            anim.northEast = Dir(v.idleNorthEast, v.attackNorthEast); anim.northWest = Dir(v.idleNorthWest, v.attackNorthWest);
            anim.southEast = Dir(v.idleSouthEast, v.attackSouthEast); anim.southWest = Dir(v.idleSouthWest, v.attackSouthWest);
            sr.sprite = v.idleSouth;
            agent.directional = anim;
        }
        else sr.sprite = d.staticSprite;

        agent.Setup(d, pos, play, projectilePrefab);
        return agent;
    }

    static SpawnerDirectionAnimator.DirectionAnimation Dir(Sprite idle, Sprite[] attack)
        => new SpawnerDirectionAnimator.DirectionAnimation { idle = idle, attackFrames = attack };
}
