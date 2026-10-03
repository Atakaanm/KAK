using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 13 K2: Karanlık Mağara — dünya, yarasa dalgası, ağ/spor yavaşlatır, sürü olayı, katalog, meşale satın alma.</summary>
public class MagaraTests
{
    const string CaveLevel = "Assets/Data/Worlds/Cave/Endless_Cave_LevelData.asset";
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { PlayerHealth.DevGodMode = false; KakTestUtil.ResetWorld(); yield return null; }

    static ProjectileData Data(string n) => UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Worlds/Cave/" + n + ".asset");

    static IEnumerator LoadCave()
    {
        GameSettings.SelectedLevel = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(CaveLevel);
        Assert.IsNotNull(GameSettings.SelectedLevel, "Mağara seviyesi yok (KakCaveSetup.Setup)");
        yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        yield return null;
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
    }

    [UnityTest]
    public IEnumerator Magara_Karanlik_Muhafiz_DurumBileseni()
    {
        yield return LoadCave();
        Assert.IsTrue(DarkWorld.Active, "Mağara karanlık değil");
        Assert.AreEqual("cave", EndlessWorlds.WorldIdOf(LevelManager.Instance.currentLevel));
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        Assert.IsNotNull(ph.GetComponent<PlayerLight>(), "Meşale yok");
        Assert.IsNotNull(ph.GetComponent<PlayerStatus>(), "Ağ/spor için durum bileşeni yok");
        var anim = Object.FindAnyObjectByType<SpawnerDirectionAnimator>();
        Assert.IsNotNull(anim);
        var sr = anim.GetComponent<SpriteRenderer>();
        string n = sr.sprite != null ? sr.sprite.name : "yok";
        Assert.IsTrue(n == "idle" || n.StartsWith("attack"), "Fırlatıcı mağara muhafızı değil: " + n); // zindanınki "south" vb.
    }

    [UnityTest]
    public IEnumerator Yarasa_DalgalanarakUcar()
    {
        yield return LoadCave();
        KakTestUtil.MakePlayerSafe();
        var bat = Data("Bat_Yarasa");
        Assert.AreEqual(ProjectileMotion.Wave, bat.motion);
        Vector3 from = new Vector3(-2.5f, 3f, 0f);
        var p = Projectile.Launch(bat.projectilePrefab, bat, from, Vector2.right, 1f);
        float minY = 99f, maxY = -99f;
        for (float t = 0f; t < 1.2f; t += Time.deltaTime)
        {
            minY = Mathf.Min(minY, p.transform.position.y); maxY = Mathf.Max(maxY, p.transform.position.y);
            yield return null;
        }
        Assert.Greater(p.transform.position.x, from.x + 1f, "Yarasa ilerlemedi");
        Assert.Greater(maxY - minY, 0.4f, "Yarasa dalgalanmıyor");
    }

    [UnityTest]
    public IEnumerator Ag_YavaslatirCanGotürmez()
    {
        yield return LoadCave();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        ph.SetMaxHealth(3);
        var st = ph.GetComponent<PlayerStatus>();
        var web = Data("Web_Ag");
        Projectile.Launch(web.projectilePrefab, web, ph.transform.position + new Vector3(0f, 1.6f, 0f), Vector2.down, 2f);
        yield return KakTestUtil.WaitUntil(() => st.Slowed, 3f, "Ağ yavaşlatmadı");
        Assert.AreEqual(web.effectStrength, st.SpeedMultiplier, 0.01f);
        Assert.AreEqual(3, ph.CurrentHealth, "Ağ can götürmemeli");
        Assert.IsTrue(ph.transform.Find("StatusIcon").GetComponent<SpriteRenderer>().enabled, "Baş üstü simgesi yok");
    }

    [UnityTest]
    public IEnumerator SuruOlayi_YarasaGonderir()
    {
        yield return LoadCave();
        KakTestUtil.MakePlayerSafe();
        var ev = Object.FindAnyObjectByType<EndlessEventManager>();
        Assert.AreSame(Data("Bat_Yarasa"), ev.swarmData);
        Assert.AreSame(Data("Stalactite_Sarkit"), ev.meteorData, "Göçük olayı sarkıt değil");
        ev.StartEvent(4);
        yield return KakTestUtil.WaitUntil(() => Projectile.Active.FindAll(x => x.Data == Data("Bat_Yarasa")).Count >= 4, 5f, "Sürü gelmedi");
    }

    [UnityTest]
    public IEnumerator Katalog_Kilit_RekorVeMesale()
    {
        var cat = EndlessWorlds.Load();
        var cave = cat.Find("cave");
        Assert.IsNotNull(cave, "Katalogda mağara yok");
        Assert.IsNotNull(cat.Find("ice"), "Buz kayboldu");
        SaveSystem.Data.gamesPlayed = cave.unlockGames - 1;
        Assert.IsFalse(EndlessWorlds.Unlocked(cave));
        SaveSystem.Data.gamesPlayed = cave.unlockGames;
        Assert.IsTrue(EndlessWorlds.Unlocked(cave));
        EndlessWorlds.SetBest("cave", 321);
        Assert.AreEqual(321, EndlessWorlds.Best("cave"));
        Assert.AreNotEqual(321, EndlessWorlds.Best("dungeon"));

        // Menüde DÜNYALAR → mağara kartında meşale yükselt
        yield return KakTestUtil.LoadScene("MainMenu");
        SaveSystem.Data.coins = TorchProgress.Costs[0];
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        menu.OnLevelsClicked();
        yield return null;
        var wp = Object.FindAnyObjectByType<WorldsPanel>();
        Assert.IsNotNull(wp);
        int caveIndex = System.Array.IndexOf(cat.worlds, cave);
        Assert.IsTrue(wp.cards[caveIndex].torchButton.gameObject.activeSelf, "Mağara kartında meşale düğmesi yok");
        wp.cards[caveIndex].torchButton.onClick.Invoke();
        Assert.AreEqual(1, TorchProgress.Level, "Meşale yükselmedi");
        Assert.AreEqual(0, SaveSystem.Data.coins);
        Assert.IsFalse(wp.cards[0].torchButton.gameObject.activeSelf, "Zindan kartında meşale olmamalı");
    }
}
