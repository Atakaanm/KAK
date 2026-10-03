using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 13 F1: Futbol Arenası — hakemler, top, sarı/kırmızı kart (2 sarı = kırmızı, 15 sn'de unutulur), olaylar, katalog.</summary>
public class FutbolTests
{
    const string Level = "Assets/Data/Worlds/Football/Endless_Football_LevelData.asset";
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { KakTestUtil.ResetWorld(); yield return null; }

    static ProjectileData Data(string n) => UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Worlds/Football/" + n + ".asset");

    static IEnumerator LoadFootball()
    {
        GameSettings.SelectedLevel = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(Level);
        Assert.IsNotNull(GameSettings.SelectedLevel, "Futbol seviyesi yok (KakFootballSetup.Setup)");
        yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        yield return null;
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
    }

    [UnityTest]
    public IEnumerator Arena_Hakem_DurumBileseni_Olaylar()
    {
        yield return LoadFootball();
        Assert.AreEqual("football", EndlessWorlds.WorldIdOf(LevelManager.Instance.currentLevel));
        Assert.IsFalse(DarkWorld.Active, "Futbol karanlık olmamalı");
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        Assert.IsNotNull(ph.GetComponent<PlayerStatus>(), "Kartlar için durum bileşeni yok");
        var anim = Object.FindAnyObjectByType<SpawnerDirectionAnimator>();
        string n = anim.GetComponent<SpriteRenderer>().sprite.name;
        Assert.IsTrue(n == "idle" || n.StartsWith("attack"), "Fırlatıcı hakem değil: " + n);
        var ev = Object.FindAnyObjectByType<EndlessEventManager>();
        Assert.AreSame(Data("Ball_Top"), ev.swarmData, "Kontra atak top değil");
        Assert.AreSame(Data("Bottle_Sise"), ev.meteorData, "Tehlike anında yukarıdan şişe gelmeli");
        Assert.AreEqual(ProjectileMotion.Homing, Data("YellowCard_SariKart").motion, "Sarı kart takip etmeli");
        Assert.Less(Data("RedCard_KirmiziKart").speed, Data("YellowCard_SariKart").speed, "Kırmızı kart daha yavaş olmalı");
        string title = null;
        System.Action<string> onEv = t => title = t;
        GameEvents.EndlessEventStarted += onEv;
        ev.StartEvent(2);
        GameEvents.EndlessEventStarted -= onEv;
        Assert.AreEqual("ev_whistle", title, "Sessizlik olayı HAKEM DÜDÜĞÜ değil");
    }

    [UnityTest]
    public IEnumerator IkiSari_OyunuBitirir_Uyari_Unutulur()
    {
        // Kullanıcı: "iki sarı kart oyunu kaybettirsin"; yumuşak: 1 sarıdayken baş üstünde uyarı, 15 sn'de unutulur
        yield return LoadFootball();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        ph.SetMaxHealth(3);
        var st = ph.GetComponent<PlayerStatus>();
        Assert.IsTrue(st.redCardEliminates, "Futbolda kırmızı kart atmalı");
        var y = Data("YellowCard_SariKart");
        Assert.IsTrue(st.Apply(y.effect, y.effectDuration, y.effectStrength, y.damage));
        Assert.AreEqual(1, st.YellowCards);
        Assert.IsTrue(st.Slowed, "Sarı kart yavaşlatmadı");
        Assert.AreEqual(3, ph.CurrentHealth, "Tek sarı can götürmemeli");
        yield return new WaitForSeconds(2f); // durum simgesinin süresi bitsin → kalıcı sarı uyarı
        var icon = ph.transform.Find("StatusIcon").GetComponent<SpriteRenderer>();
        Assert.IsTrue(icon.enabled && icon.sprite != null && icon.sprite.name.Contains("Yellow"), "1 sarıdayken baş üstünde sarı kart uyarısı yok");

        // Unutulma: 15 sn yeni kart yoksa sayaç sıfırlanır
        typeof(PlayerStatus).GetField("lastYellow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(st, Time.time - st.yellowForgetSeconds - 1f);
        yield return null;
        Assert.AreEqual(0, st.YellowCards, "Eski sarı kart unutulmadı");

        // İki sarı → oyundan atılır (can sayısından bağımsız)
        Assert.IsTrue(st.Apply(y.effect, y.effectDuration, y.effectStrength, y.damage));
        Assert.IsTrue(st.Apply(y.effect, y.effectDuration, y.effectStrength, y.damage));
        yield return null;
        Assert.IsTrue(ph.IsDead, "İki sarı oyunu bitirmedi");
        Assert.IsTrue(GameManager.Instance.IsGameOver);
    }

    [UnityTest]
    public IEnumerator KirmiziKart_TekAtar()
    {
        yield return LoadFootball();
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        ph.SetMaxHealth(3);
        var st = ph.GetComponent<PlayerStatus>();
        var r = Data("RedCard_KirmiziKart");
        Assert.IsTrue(st.Apply(r.effect, r.effectDuration, r.effectStrength, r.damage));
        yield return null;
        Assert.IsTrue(ph.IsDead, "Kırmızı kart tek atmalı");
    }

    [UnityTest]
    public IEnumerator Top_Seker()
    {
        yield return LoadFootball();
        KakTestUtil.MakePlayerSafe();
        var b = Data("Ball_Top");
        Assert.AreEqual(ProjectileMotion.Bounce, b.motion);
        Rect r = Projectile.ArenaRect;
        var p = Projectile.Launch(b.projectilePrefab, b, new Vector3(r.xMax - 0.6f, r.center.y + 1.5f, 0f), Vector2.right, 2f);
        yield return KakTestUtil.WaitUntil(() => p.MoveDirection.x < 0f || !p.gameObject.activeSelf, 3f, "Top duvardan sekmedi");
        Assert.IsTrue(p.gameObject.activeSelf, "Top sekmeden kayboldu");
    }

    [Test]
    public void Katalog_DortDunya_FutbolKilidi()
    {
        var cat = EndlessWorlds.Load();
        Assert.AreEqual(new[] { "dungeon", "ice", "cave", "football" }, System.Array.ConvertAll(cat.worlds, w => w.id), "Dünya sırası");
        Assert.Greater(cat.Find("football").unlockGames, cat.Find("cave").unlockGames);
    }
}
