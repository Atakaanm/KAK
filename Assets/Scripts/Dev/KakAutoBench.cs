#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Otomatik performans ölçümü (development build): uygulama "-kakbench SANIYE DOSYA" ile açılırsa
/// oyuna girer, botu oynatır (ölümsüz), performansı ölçer, raporu dosyaya yazıp kapanır.
/// Örnek: KacAtaKac.app/Contents/MacOS/KacAtaKac -kakbench 60 /tmp/bench.txt -screen-width 540 -screen-height 1170
/// </summary>
public class KakAutoBench : MonoBehaviour
{
    float seconds = 60f;
    string outFile;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var args = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(args, "-kakbench");
        // iOS: simctl launch argümanları C#'a ulaşmıyor → ortam değişkeni
        // (SIMCTL_CHILD_KAK_BENCH=60 xcrun simctl launch ... ; rapor persistentDataPath/bench.txt)
        string env = System.Environment.GetEnvironmentVariable("KAK_BENCH");
        if (i < 0 && string.IsNullOrEmpty(env)) return;
        var go = new GameObject("KakAutoBench");
        DontDestroyOnLoad(go);
        var b = go.AddComponent<KakAutoBench>();
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        string sec = i >= 0 ? (i + 1 < args.Length ? args[i + 1] : null) : env;
        if (sec != null && float.TryParse(sec, System.Globalization.NumberStyles.Float, inv, out float s)) b.seconds = s;
        b.outFile = i >= 0 && i + 2 < args.Length ? args[i + 2] : Path.Combine(Application.persistentDataPath, "bench.txt");
        Debug.Log("[KakAutoBench] başladı: " + b.seconds + " sn → " + b.outFile);
    }

    IEnumerator Start()
    {
        // -kakquality N: kalite seviyesini zorla (0 = Mobile; telefondaki URP ayarlarını Mac'te ölçmek için)
        // -kakuncapped: vsync ve 60 FPS sınırı kapalı → gerçek kare maliyeti (boşluk payı) görünür
        var args = System.Environment.GetCommandLineArgs();
        int q = System.Array.IndexOf(args, "-kakquality");
        if (q >= 0 && q + 1 < args.Length && int.TryParse(args[q + 1], out int level)) QualitySettings.SetQualityLevel(level, true);
        bool uncapped = System.Array.IndexOf(args, "-kakuncapped") >= 0;
        if (uncapped) { QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; }

        // Menüden oyuna (gerçek akış)
        yield return new WaitForSeconds(1f);
        var menu = FindAnyObjectByType<MainMenuController>();
        if (menu != null) menu.OnPlayClicked();
        while (SceneManager.GetActiveScene().name != SceneLoader.GAME_SCENE) yield return null;
        yield return new WaitForSeconds(1f);

        PlayerHealth.DevGodMode = true;
        var player = FindAnyObjectByType<PlayerMovement2D>();
        player.gameObject.AddComponent<KakAutoPilot>().skill = 1f;
        var perf = player.gameObject.AddComponent<KakPerfProbe>();
        var ev = FindAnyObjectByType<EndlessEventManager>();

        float end = Time.realtimeSinceStartup + seconds;
        int eventsStarted = 0;
        while (Time.realtimeSinceStartup < end)
        {
            // Yoğun anları da ölç: arada bir olay başlat
            if (ev != null && !ev.Running && Time.realtimeSinceStartup > end - seconds + 10f * (eventsStarted + 1))
            {
                ev.StartEvent(eventsStarted % 4);
                eventsStarted++;
            }
            yield return null;
        }

        var sm = GameManager.Instance != null ? GameManager.Instance.scoreManager : null;
        int scoreAtEnd = sm != null ? sm.ScoreInt : -1;
        int activeAtEnd = Projectile.Active.Count;

        // Kırılım: grupları sırayla kapatıp batch/SetPass payını ölç (render maliyeti nereden geliyor?)
        var breakdown = new StringBuilder("\n-- kırılım (grup kapalıyken ort. batch / SetPass) --");
        yield return Breakdown(breakdown);

        string report = perf.Report()
            + $"\nçözünürlük {Screen.width}x{Screen.height}, süre {seconds}s, skor {scoreAtEnd}, olay {eventsStarted}, aktif taş {activeAtEnd}"
            + $"\ncihaz {SystemInfo.deviceModel} | {SystemInfo.graphicsDeviceName} | {SystemInfo.graphicsDeviceType} | kalite {QualitySettings.names[QualitySettings.GetQualityLevel()]}{(uncapped ? " | SINIRSIZ FPS" : "")}"
            + breakdown;
        File.WriteAllText(outFile, report);
        yield return null;
        Debug.Log("[KakAutoBench] " + report);
        Application.Quit();
    }

    IEnumerator Breakdown(StringBuilder sb)
    {
        string[] groups = { "", "HUDCanvas", "DungeonFrame", "ArenaVisual", "Spawners", "#taşlar", "Global Volume", "Directional Light", "Player", "" };
        var shooters = FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
        foreach (var g in groups)
        {
            GameObject go = null;
            if (g == "#taşlar")
            {
                foreach (var c in shooters) c.enabled = false;
                foreach (var p in Projectile.Active.ToArray()) ProjectilePool.Instance.Return(p.gameObject);
            }
            else if (g.Length > 0)
            {
                go = FindByName(g);
                if (go != null) go.SetActive(false);
            }
            yield return new WaitForSecondsRealtime(0.5f);
            float b = 0f, sp = 0f;
            yield return Measure(2.5f, r => { b = r.Item1; sp = r.Item2; });
            sb.Append($"\n{(g.Length == 0 ? "(hepsi açık)" : g + " kapalı")}: {b:F0} / {sp:F0}{(g.Length > 0 && g != "#taşlar" && go == null ? " (bulunamadı)" : "")}");
            if (go != null) go.SetActive(true);
            if (g == "#taşlar") foreach (var c in shooters) c.enabled = true;
        }
    }

    static GameObject FindByName(string name)
    {
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (t.name == name) return t.gameObject;
        return null;
    }

    static IEnumerator Measure(float secs, System.Action<(float, float)> done)
    {
        var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        var setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        long sb = 0, ss = 0; int n = 0;
        float end = Time.realtimeSinceStartup + secs;
        while (Time.realtimeSinceStartup < end)
        {
            yield return null;
            if (batches.Valid) sb += batches.LastValue;
            if (setPass.Valid) ss += setPass.LastValue;
            n++;
        }
        batches.Dispose(); setPass.Dispose();
        done(n > 0 ? ((float)sb / n, (float)ss / n) : (0f, 0f));
    }
}
#endif
