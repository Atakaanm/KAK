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

    static string Durum(PlayerHealth ph)
    {
        var sr = ph.playerSpriteRenderer;
        if (sr == null) return ph.name + ": renderer yok";
        bool own = sr.transform.IsChildOf(ph.transform);
        return $"{ph.name}: kendi={own} enabled={sr.enabled} aktif={sr.gameObject.activeInHierarchy} renk={sr.color} sprite={(sr.sprite != null ? sr.sprite.name : "yok")} ölçek={sr.transform.lossyScale}";
    }

    const string IceLevel = "Assets/Data/Worlds/Ice/Endless_Ice_LevelData.asset";

    static IEnumerator LoadTwoPlayer(bool ice)
    {
        GameSettings.TwoPlayer = true;
        if (ice)
        {
            GameSettings.SelectedLevel = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(IceLevel);
            yield return KakTestUtil.LoadScene(KakTestUtil.GameScene);
        }
        else yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
    }

    [UnityTest] public IEnumerator DonenOyuncuGorunur_DuserkenSahadanKalkar_Zindan() { yield return DonenOyuncu(false); }
    [UnityTest] public IEnumerator DonenOyuncuGorunur_DuserkenSahadanKalkar_Buz() { yield return DonenOyuncu(true); }

    IEnumerator DonenOyuncu(bool ice)
    {
        // Faz 12 H1: Ada düşüp dönünce görüntüsü yoktu; düşmüş oyuncu vurulduğu yerde kalıp yakın geçiş puanı topluyordu
        yield return LoadTwoPlayer(ice);
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = false;
        var gm = GameManager.Instance;
        gm.respawnSeconds = 1f;
        foreach (int i in new[] { 1, 0 })
        {
            var ph = PlayerRegistry.All[i];
            yield return new WaitForSeconds(0.6f); // önceki dönüşün dokunulmazlığı bitsin
            var cm = ph.GetComponent<ColdMeter>();
            if (ice)
            {
                Assert.IsNotNull(cm, ph.name + " soğuk göstergesi yok");
                ph.GetComponent<PlayerMovement2D>().FreezeFor(5f); // donukken düşsün (buz bloğu)
                yield return null;
            }
            KakTestUtil.KillPlayer(ph);
            yield return new WaitForSeconds(0.5f);
            Assert.IsTrue(ph.IsDead);
            var sr = ph.playerSpriteRenderer;
            Assert.IsTrue(sr == null || !sr.enabled || sr.color.a < 0.05f, "Düşen oyuncu sahada görünüyor: " + Durum(ph));
            foreach (var c in ph.GetComponentsInChildren<Collider2D>()) Assert.IsFalse(c.enabled, "Düşen oyuncunun çarpışması açık: " + c.name);
            Assert.IsFalse(PlayerRegistry.IsTargetable(ph), "Düşen oyuncu hedef alınıyor");

            yield return KakTestUtil.WaitUntil(() => !ph.IsDead, 3f, "Düşen oyuncu dönmedi");
            yield return new WaitForSeconds(gm.respawnInvulnerable + 0.3f);
            sr = ph.playerSpriteRenderer;
            Assert.IsNotNull(sr);
            Assert.IsTrue(sr.transform.IsChildOf(ph.transform), "Renderer başka oyuncunun: " + Durum(ph));
            Assert.IsTrue(sr.enabled && sr.gameObject.activeInHierarchy && sr.sprite != null && sr.color.a > (ph.IsGhost ? 0.45f : 0.95f), "Dönen oyuncu görünmüyor: " + Durum(ph));
            foreach (var c in ph.GetComponentsInChildren<Collider2D>()) Assert.IsTrue(c.enabled, "Dönen oyuncunun çarpışması kapalı: " + c.name);
            Assert.IsTrue(ph.GetComponent<Rigidbody2D>().simulated, "Dönen oyuncunun fiziği kapalı");
            Assert.IsTrue(ph.healthUI.GetComponentsInChildren<UnityEngine.UI.Image>().Length > 0);
            if (ice)
            {
                Assert.IsFalse(ph.GetComponent<PlayerMovement2D>().IceFrozen, "Dönen oyuncu donuk");
                Assert.Less(cm.Value, 0.2f, "Dönen oyuncu sıcak başlamalı");
                var block = ph.transform.Find("IceBlock");
                Assert.IsTrue(block == null || !block.GetComponent<SpriteRenderer>().enabled, "Buz bloğu dönüşte görünüyor");
            }
        }
    }

    static string Tumu(PlayerHealth ph)
    {
        var sb = new System.Text.StringBuilder(ph.name + ":");
        foreach (var r in ph.GetComponentsInChildren<Renderer>(true))
        {
            var sr = r as SpriteRenderer;
            sb.Append($" [{r.name} en={r.enabled} act={r.gameObject.activeInHierarchy} ord={r.sortingOrder} ölçek={r.transform.lossyScale.x:0.00}");
            if (sr != null) sb.Append($" spr={(sr.sprite != null ? sr.sprite.name : "yok")} a={sr.color.a:0.00}");
            sb.Append(']');
        }
        return sb.ToString();
    }

    [UnityTest]
    public IEnumerator IkiKisilik_Dayaniklilik_DonenHepGorunur()
    {
        // Faz 12 H1: "Ada dönünce görüntüsü yok" ara sıra oluyordu → gerçek taşlar ve sık eşyalarla defalarca düşür/döndür
        yield return LoadTwoPlayer(true);
        foreach (var s in Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None)) s.enabled = true;
        var sp = Object.FindAnyObjectByType<PowerupSpawner>();
        sp.SetSpawnInterval(0.4f, 0.8f);
        var gm = GameManager.Instance;
        gm.respawnSeconds = 1f;
        var p1 = PlayerRegistry.All[0];
        var p2 = PlayerRegistry.All[1];
        Assert.Greater(p2.playerSpriteRenderer.color.a, 0.95f, "2. oyuncu baştan saydam (asıl renk hatası)");
        for (int round = 0; round < 5; round++)
        {
            p1.SetInvulnerable(999f);
            p1.currentHealth = p1.MaxHealth;
            var mv1 = p1.GetComponent<PlayerMovement2D>();
            var mv2 = p2.GetComponent<PlayerMovement2D>();
            mv1.InputOverride = Random.insideUnitCircle.normalized; // eşyaları toplasın
            mv2.InputOverride = Random.insideUnitCircle.normalized;
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
            if (!p2.IsDead) KakTestUtil.KillPlayer(p2);
            yield return KakTestUtil.WaitUntil(() => !p2.IsDead, 4f, "Dönmedi");
            yield return new WaitForSeconds(gm.respawnInvulnerable + 0.5f);
            // Tek kare yetmez: vuruş sonrası yanıp sönme (kalkan patlayınca da) meşru. 0,6 sn boyunca en az bir kez
            // görünür ve hiçbir karede saydam olmamalı.
            bool seen1 = false, seen2 = false;
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                foreach (var ph in new[] { p1, p2 })
                {
                    var sr = ph.playerSpriteRenderer;
                    if (ph.IsDead || sr == null) continue;
                    Assert.IsTrue(sr.sprite != null && sr.transform.lossyScale.x > 1.5f, $"Tur {round}: {Tumu(ph)}");
                    Assert.Greater(sr.color.a, ph.IsGhost ? 0.45f : 0.95f, $"Tur {round}: oyuncu saydam → {Tumu(ph)}");
                    if (sr.enabled && sr.gameObject.activeInHierarchy) { if (ph == p1) seen1 = true; else seen2 = true; }
                }
                yield return null;
            }
            Assert.IsTrue((seen1 || p1.IsDead) && (seen2 || p2.IsDead), $"Tur {round}: oyuncu görünmüyor → {Tumu(p1)} / {Tumu(p2)}");
        }
        foreach (var ph in PlayerRegistry.All) ph.GetComponent<PlayerMovement2D>().InputOverride = null;
    }

    [UnityTest]
    public IEnumerator Buz_IkiKisilik_SogukCubuguOyuncuBasina_AtesIsıtir()
    {
        // Faz 12 H1: tek çubuk "en çok üşüyeni" gösteriyordu; Ata ateş alınca azalmıyor sanıldı
        yield return LoadTwoPlayer(true);
        var p1 = PlayerRegistry.All[0].GetComponent<ColdMeter>();
        var p2 = PlayerRegistry.All[1].GetComponent<ColdMeter>();
        Assert.IsNotNull(p1); Assert.IsNotNull(p2);
        var huds = Object.FindObjectsByType<ColdHud>(FindObjectsSortMode.None);
        Assert.AreEqual(2, huds.Length, "Her oyuncunun kendi soğuk çubuğu olmalı");
        Assert.AreNotSame(huds[0].target, huds[1].target);
        foreach (var h in huds) Assert.IsTrue(h.transform.IsChildOf(h.target.GetComponent<PlayerHealth>().healthUI.transform), "Çubuk oyuncunun kalp satırında değil");

        // İki oyuncu uzakta: Ata ateş alır → yalnız Ata ısınır
        p1.transform.position = new Vector3(-1.5f, 0f, 0f);
        p2.transform.position = new Vector3(1.5f, 0f, 0f);
        typeof(ColdMeter).GetProperty("Value").SetValue(p1, 0.8f);
        typeof(ColdMeter).GetProperty("Value").SetValue(p2, 0.8f);
        var fire = UnityEditor.AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/FireData.asset");
        PowerupPickup.Apply(fire, p1.gameObject, p1.transform.position);
        Assert.Less(p1.Value, 0.3f, "Ateş alan ısınmadı");
        Assert.Greater(p2.Value, 0.75f, "Uzaktaki arkadaş ısınmamalı");

        // Yan yana: arkadaş yarı ısınır
        p2.transform.position = p1.transform.position + new Vector3(0.8f, 0f, 0f);
        typeof(ColdMeter).GetProperty("Value").SetValue(p1, 0.8f);
        PowerupPickup.Apply(fire, p1.gameObject, p1.transform.position);
        Assert.Less(p2.Value, 0.75f, "Yakındaki arkadaş ısınmadı");
        Assert.Greater(p2.Value, p1.Value, "Arkadaş yarı ısınmalı (ateşi alandan az)");
    }

    [UnityTest]
    public IEnumerator IkiKisilik_EsyalarDahaSik()
    {
        yield return LoadTwoPlayer(false);
        var sp = Object.FindAnyObjectByType<PowerupSpawner>();
        GameSettings.TwoPlayer = false;
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        float soloMax = Object.FindAnyObjectByType<PowerupSpawner>().spawnIntervalMax;
        yield return LoadTwoPlayer(false);
        float duoMax = Object.FindAnyObjectByType<PowerupSpawner>().spawnIntervalMax;
        Assert.AreEqual(soloMax / TwoPlayerMode.ItemRate, duoMax, 0.01f, "İki kişilikte eşya aralığı kısalmadı");
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
