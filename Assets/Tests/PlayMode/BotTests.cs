using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Test botuyla uzun oyun testleri: kararlılık, havuz sızıntısı, denge ölçümü.
/// Çalıştırma: python3 tools/kak_bridge.py tests PlayMode BotTests
/// </summary>
public class BotTests
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

    [UnityTest]
    [Timeout(120000)]
    public IEnumerator UstaBot_60Saniye_Oynar()
    {
        // Bu test log temizliğini değil kararlılık ve dengeyi ölçer; hatalar sayılıp raporlanır
        LogAssert.ignoreFailingMessages = true;
        int errorCount = 0;
        string firstError = null;
        Application.LogCallback onLog = (msg, stack, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                errorCount++;
                if (firstError == null) firstError = msg;
            }
        };
        Application.logMessageReceived += onLog;

        yield return KakTestUtil.LoadGameWithLevel();
        var player = Object.FindAnyObjectByType<PlayerMovement2D>();
        Assert.IsNotNull(player);
        var bot = player.gameObject.AddComponent<KakAutoPilot>();
        bot.skill = 1f;
        var perf = player.gameObject.AddComponent<KakPerfProbe>();

        float end = Time.realtimeSinceStartup + 60f;
        int maxPool = 0;
        while (Time.realtimeSinceStartup < end && !bot.Finished)
        {
            if (ProjectilePool.Instance != null)
                maxPool = Mathf.Max(maxPool, ProjectilePool.Instance.transform.childCount);
            yield return null;
        }

        Application.logMessageReceived -= onLog;
        int score = KakTestUtil.Score();
        Debug.Log($"[BotTests] Usta bot: {bot.SurvivalTime:F1} sn, vuruş {bot.HitsTaken}, skor {score}, en fazla havuz nesnesi {maxPool}, bitti={bot.Finished}, hata logu {errorCount}" + (firstError != null ? " (ilk: " + firstError + ")" : ""));

        Debug.Log(perf.Report());
        // Editör ölçümü Scene view ve editör yükünü içerir; kesin bütçe (< 60) cihazda ölçülür (Faz 10).
        // Burada sadece büyük gerilemeleri yakalayan geniş bir sınır var.
        Assert.Less(perf.AvgBatches, 200f, "Batch sayısı ciddi şekilde arttı (editör sınırı 200)");

        Assert.Greater(bot.SurvivalTime, 20f, "Usta bot 20 sn bile dayanamadı: bot veya denge sorunlu");
        Assert.Less(maxPool, 150, "Mermi havuzu aşırı büyüdü (sızıntı?)");
    }
}
