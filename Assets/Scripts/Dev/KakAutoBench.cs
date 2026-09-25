#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
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
        if (i < 0) return;
        var go = new GameObject("KakAutoBench");
        DontDestroyOnLoad(go);
        var b = go.AddComponent<KakAutoBench>();
        if (i + 1 < args.Length && float.TryParse(args[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float s)) b.seconds = s;
        b.outFile = i + 2 < args.Length ? args[i + 2] : Path.Combine(Application.persistentDataPath, "bench.txt");
    }

    IEnumerator Start()
    {
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
        string report = perf.Report()
            + $"\nçözünürlük {Screen.width}x{Screen.height}, süre {seconds}s, skor {(sm != null ? sm.ScoreInt : -1)}, olay {eventsStarted}, aktif taş {Projectile.Active.Count}"
            + $"\ncihaz {SystemInfo.deviceModel} | {SystemInfo.graphicsDeviceName} | {SystemInfo.graphicsDeviceType}";
        File.WriteAllText(outFile, report);
        Debug.Log("[KakAutoBench] " + report);
        Application.Quit();
    }
}
#endif
