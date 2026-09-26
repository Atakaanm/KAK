using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>Faz 3c.5: petler — açılma, takip, mıknatıs, kalkan, satın al/seç/çıkar.</summary>
public class PetTests
{
    [UnitySetUp]
    public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }

    [UnityTearDown]
    public IEnumerator TearDown() { PlayerHealth.DevGodMode = false; KakTestUtil.ResetWorld(); yield return null; }

    static PetData Pet(string id) => PetCatalog.Load().Find(id);

    static void Own(string id, int games = FeatureGate.PetsGames)
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = games;
        d.unlockedPets.Add(id);
        d.selectedPet = id;
    }

    static void Quiet()
    {
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        var ps = Object.FindAnyObjectByType<PowerupSpawner>();
        if (ps != null) ps.enabled = false;
        var cs = Object.FindAnyObjectByType<CoinSpawner>();
        if (cs != null) cs.enabled = false;
        foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
    }

    [UnityTest]
    public IEnumerator PetOzelligiKapaliyken_PetCikmaz()
    {
        Own("Firefly", games: 1);
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        Assert.IsNull(PetFollower.Instance, "Petler kilitliyken pet çıktı");
    }

    [UnityTest]
    public IEnumerator Atesbocegi_OyuncuyuTakipEder_AltiniCeker()
    {
        Own("Firefly");
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        yield return null;
        var pet = PetFollower.Instance;
        Assert.IsNotNull(pet, "Pet çıkmadı");
        var player = Object.FindAnyObjectByType<PlayerHealth>().transform;

        var cs = Object.FindAnyObjectByType<CoinSpawner>();
        Assert.Greater(cs.SpawnCluster(), 0);
        yield return null;
        var coin = Coin.Active[0];
        coin.transform.position = player.position + new Vector3(1.6f, 0f, 0f);
        float d0 = Vector2.Distance(coin.transform.position, player.position);
        yield return KakTestUtil.WaitReal(0.15f);
        bool pulled = !coin.gameObject.activeSelf || Vector2.Distance(coin.transform.position, player.position) < d0 - 0.3f;
        Assert.IsTrue(pulled, "Mıknatıs altını çekmedi");
        Assert.Less(Vector2.Distance(pet.transform.position, player.position), 1.5f, "Pet oyuncuyu takip etmiyor");
    }

    [UnityTest]
    public IEnumerator Kaplumbaga_AraliklaKalkanVerir()
    {
        Own("Turtle");
        yield return KakTestUtil.LoadGameWithLevel();
        Quiet();
        yield return null;
        var pet = PetFollower.Instance;
        Assert.IsNotNull(pet);
        pet.data = Object.Instantiate(pet.data); // asset'i değiştirme
        pet.data.shieldInterval = 0.4f;
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        Assert.IsFalse(ph.HasShield);
        yield return KakTestUtil.WaitUntil(() => ph.HasShield, 3f, "Kaplumbağa kalkan vermedi");
        Assert.AreEqual(0, GameManager.Instance.scoreManager.ShieldBlocks, "Kalkan vermek 'engelleme' sayılmamalı");
    }

    [UnityTest]
    public IEnumerator Panel_SatinAl_Sec_Cikar()
    {
        var d = SaveSystem.Data;
        d.gamesPlayed = FeatureGate.PetsGames;
        d.coins = 500;
        yield return KakTestUtil.LoadScene(KakTestUtil.MenuScene);
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        menu.OnPetsClicked();
        yield return KakTestUtil.WaitReal(0.3f);
        var panel = Object.FindAnyObjectByType<PetPanel>();
        Assert.IsNotNull(panel, "Pet paneli açılmadı");
        panel.OnCardAction(0); // Ateşböceği 400
        Assert.AreEqual(100, d.coins);
        Assert.AreEqual("Firefly", d.selectedPet);
        panel.OnCardAction(0); // çıkar
        Assert.AreEqual("", d.selectedPet, "Seçili pet çıkarılamadı");
        panel.OnCardAction(1); // Kaplumbağa 1200: yetmez
        Assert.IsFalse(PetCatalog.Owned(Pet("Turtle")));
        Assert.AreEqual(100, d.coins);
    }
}
