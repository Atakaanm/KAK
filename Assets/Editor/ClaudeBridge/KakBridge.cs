using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

/// <summary>
/// Claude <-> Unity Editor köprüsü.
///
/// Proje kökündeki .claude-bridge/inbox klasörüne JSON komut dosyası yazılır,
/// köprü bunu EditorApplication.update içinde işler ve sonucu outbox'a yazar.
/// Ayrıca tüm Console loglarını .claude-bridge/console.log dosyasına, derleme
/// sonuçlarını compile.json'a, editör durumunu heartbeat.json'a yazar.
///
/// İstemci: tools/kak_bridge.py
/// Komutlar: ping, state, refresh, compile, play, stop, pause, screenshot, tests, menu,
///           invoke, openScene, saveScenes, clearConsole
///
/// Asenkron komutlar (refresh, play, stop, tests, screenshot) domain reload'dan
/// sağ çıkmak için bekleme durumunu SessionState'te tutar.
/// </summary>
[InitializeOnLoad]
public static class KakBridge
{
    // -------------------------------------------------------
    // Yollar
    // -------------------------------------------------------
    static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".claude-bridge"));
    static string Inbox => Path.Combine(Root, "inbox");
    static string Outbox => Path.Combine(Root, "outbox");
    static string ConsolePath => Path.Combine(Root, "console.log");
    static string CompilePath => Path.Combine(Root, "compile.json");
    static string HeartbeatPath => Path.Combine(Root, "heartbeat.json");

    const string PendingKey = "KakBridge.Pending";
    const string TestCmdKey = "KakBridge.TestCmdId";

    static double lastPoll;
    static double lastBeat;
    static readonly object logLock = new object();
    static readonly List<string> compileErrors = new List<string>();
    static readonly List<string> compileWarnings = new List<string>();

    // -------------------------------------------------------
    // Veri sınıfları (JsonUtility)
    // -------------------------------------------------------
    [Serializable]
    public class Cmd
    {
        public string id;
        public string cmd;
        public string arg;
        public string arg2;
        public string path;
        public int width;
        public int height;
        public float seconds;
    }

    [Serializable]
    class Pending
    {
        public Cmd c;
        public string stage;
        public double start;
        public int frames;
        public int startFrame;
        public long startUnixMs;
    }

    [Serializable]
    class Result
    {
        public string id;
        public string cmd;
        public bool ok;
        public string message;
        public string data;
        public long unixMs;
    }

    [Serializable]
    class CompileInfo
    {
        public long unixMs;
        public bool success;
        public List<string> errors = new List<string>();
        public int warningCount;
        public List<string> warnings = new List<string>();
    }

    [Serializable]
    class Heartbeat
    {
        public long unixMs;
        public bool isCompiling;
        public bool isUpdating;
        public bool isPlaying;
        public bool isPaused;
        public bool appFocused;
        public string activeScene;
        public string pending;
    }

    // -------------------------------------------------------
    // Kurulum
    // -------------------------------------------------------
    static KakBridge()
    {
        try
        {
            Directory.CreateDirectory(Inbox);
            Directory.CreateDirectory(Outbox);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[KakBridge] Klasör oluşturulamadı: " + e.Message);
            return;
        }

        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Application.logMessageReceivedThreaded -= OnLog;
        Application.logMessageReceivedThreaded += OnLog;
        CompilationPipeline.compilationStarted -= OnCompileStarted;
        CompilationPipeline.compilationStarted += OnCompileStarted;
        CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompiled;
        CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompiled;
        CompilationPipeline.compilationFinished -= OnCompileFinished;
        CompilationPipeline.compilationFinished += OnCompileFinished;

        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(new TestCallbacks());

        AppendLog("BRIDGE", "domain yüklendi");
    }

    static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // -------------------------------------------------------
    // Log
    // -------------------------------------------------------
    static void OnLog(string condition, string stackTrace, LogType type)
    {
        bool withStack = type == LogType.Error || type == LogType.Exception || type == LogType.Assert;
        AppendLog(type.ToString().ToUpperInvariant(), withStack ? condition + "\n" + stackTrace : condition);
    }

    static void AppendLog(string tag, string msg)
    {
        try
        {
            lock (logLock)
            {
                File.AppendAllText(ConsolePath,
                    DateTime.Now.ToString("HH:mm:ss.fff") + " [" + tag + "] " + msg + "\n");
            }
        }
        catch { /* log yazılamazsa sessiz geç */ }
    }

    // -------------------------------------------------------
    // Derleme
    // -------------------------------------------------------
    static void OnCompileStarted(object ctx)
    {
        compileErrors.Clear();
        compileWarnings.Clear();
        AppendLog("BRIDGE", "derleme başladı");
    }

    static void OnAssemblyCompiled(string asm, CompilerMessage[] messages)
    {
        foreach (var m in messages)
        {
            string line = m.file + "(" + m.line + "," + m.column + "): " + m.message;
            if (m.type == CompilerMessageType.Error) compileErrors.Add(line);
            else if (m.type == CompilerMessageType.Warning) compileWarnings.Add(line);
        }
    }

    static void OnCompileFinished(object ctx)
    {
        var info = new CompileInfo
        {
            unixMs = NowMs(),
            success = compileErrors.Count == 0,
            errors = new List<string>(compileErrors),
            warningCount = compileWarnings.Count,
            warnings = compileWarnings.Take(30).ToList()
        };
        WriteFile(CompilePath, JsonUtility.ToJson(info, true));
        AppendLog("BRIDGE", "derleme bitti: " + (info.success ? "BAŞARILI" : compileErrors.Count + " hata"));
    }

    // -------------------------------------------------------
    // Ana döngü
    // -------------------------------------------------------
    static void Tick()
    {
        double t = EditorApplication.timeSinceStartup;

        if (t - lastBeat > 1.0)
        {
            lastBeat = t;
            WriteHeartbeat();
        }

        if (t - lastPoll < 0.2) return;
        lastPoll = t;

        try
        {
            if (HasPending())
            {
                ProcessPending();
                return; // bekleyen iş bitene kadar yeni komut alma
            }

            if (!Directory.Exists(Inbox)) return;
            var files = Directory.GetFiles(Inbox, "*.json").OrderBy(f => f).ToArray();
            if (files.Length == 0) return;

            string file = files[0];
            string json = File.ReadAllText(file);
            File.Delete(file);
            var c = JsonUtility.FromJson<Cmd>(json);
            if (c == null || string.IsNullOrEmpty(c.cmd))
            {
                AppendLog("BRIDGE", "geçersiz komut: " + json);
                return;
            }
            if (string.IsNullOrEmpty(c.id)) c.id = Path.GetFileNameWithoutExtension(file);
            AppendLog("BRIDGE", "komut: " + c.cmd + " " + c.arg);
            Execute(c);
        }
        catch (Exception e)
        {
            AppendLog("BRIDGE", "Tick hatası: " + e);
        }
    }

    static void WriteHeartbeat()
    {
        var hb = new Heartbeat
        {
            unixMs = NowMs(),
            isCompiling = EditorApplication.isCompiling,
            isUpdating = EditorApplication.isUpdating,
            isPlaying = EditorApplication.isPlaying,
            isPaused = EditorApplication.isPaused,
            appFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive,
            activeScene = EditorSceneManager.GetActiveScene().path,
            pending = SessionState.GetString(PendingKey, "")
        };
        WriteFile(HeartbeatPath, JsonUtility.ToJson(hb, true));
    }

    // -------------------------------------------------------
    // Komutlar
    // -------------------------------------------------------
    static void Execute(Cmd c)
    {
        switch (c.cmd)
        {
            case "ping":
                Reply(c, true, "pong");
                break;

            case "state":
                WriteHeartbeat();
                Reply(c, true, "state", File.ReadAllText(HeartbeatPath));
                break;

            case "refresh":
                AssetDatabase.Refresh();
                SetPending(c, "wait-compile");
                break;

            case "compile":
                AssetDatabase.Refresh();
                CompilationPipeline.RequestScriptCompilation();
                SetPending(c, "wait-compile");
                break;

            case "play":
                if (EditorApplication.isPlaying) { Reply(c, true, "zaten oynuyor"); break; }
                if (EditorApplication.isCompiling) { Reply(c, false, "derleme sürüyor"); break; }
                EditorApplication.EnterPlaymode();
                SetPending(c, "wait-play");
                break;

            case "stop":
                if (!EditorApplication.isPlaying) { Reply(c, true, "zaten durmuş"); break; }
                EditorApplication.ExitPlaymode();
                SetPending(c, "wait-stop");
                break;

            case "pause":
                EditorApplication.isPaused = c.arg != "off";
                Reply(c, true, "pause=" + EditorApplication.isPaused);
                break;

            case "screenshot":
                StartScreenshot(c);
                break;

            case "tests":
                StartTests(c);
                break;

            case "menu":
                bool ok = EditorApplication.ExecuteMenuItem(c.arg);
                Reply(c, ok, ok ? "çalıştı: " + c.arg : "menü bulunamadı: " + c.arg);
                break;

            case "invoke":
                Invoke(c);
                break;

            case "openScene":
                if (EditorApplication.isPlaying) { Reply(c, false, "Play modunda sahne açılamaz"); break; }
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene(c.arg, OpenSceneMode.Single);
                Reply(c, true, "açıldı: " + c.arg);
                break;

            case "saveScenes":
                bool saved = EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
                Reply(c, saved, "sahneler ve asset'ler kaydedildi");
                break;

            case "clearConsole":
                lock (logLock) { WriteFile(ConsolePath, ""); }
                Reply(c, true, "console.log temizlendi");
                break;

            default:
                Reply(c, false, "bilinmeyen komut: " + c.cmd);
                break;
        }
    }

    // -------------------------------------------------------
    // Bekleyen işler
    // -------------------------------------------------------
    static bool HasPending() => !string.IsNullOrEmpty(SessionState.GetString(PendingKey, ""));

    static void SetPending(Cmd c, string stage)
    {
        var p = new Pending
        {
            c = c,
            stage = stage,
            start = EditorApplication.timeSinceStartup,
            frames = 0,
            startFrame = Time.frameCount,
            startUnixMs = NowMs()
        };
        SessionState.SetString(PendingKey, JsonUtility.ToJson(p));
    }

    static void SavePending(Pending p) => SessionState.SetString(PendingKey, JsonUtility.ToJson(p));
    static void ClearPending() => SessionState.EraseString(PendingKey);

    static void ProcessPending()
    {
        var p = JsonUtility.FromJson<Pending>(SessionState.GetString(PendingKey, ""));
        if (p == null || p.c == null) { ClearPending(); return; }

        double elapsed = EditorApplication.timeSinceStartup - p.start;
        // Editör yeniden başlatıldıysa timeSinceStartup sıfırlanır
        if (elapsed < 0) elapsed = (NowMs() - p.startUnixMs) / 1000.0;
        float timeout = p.c.seconds > 0 ? p.c.seconds : 180f;

        bool busy = EditorApplication.isCompiling || EditorApplication.isUpdating;

        switch (p.stage)
        {
            case "wait-compile":
                if (elapsed > 2.0 && !busy)
                {
                    string compile = File.Exists(CompilePath) ? File.ReadAllText(CompilePath) : "";
                    var info = string.IsNullOrEmpty(compile) ? null : JsonUtility.FromJson<CompileInfo>(compile);
                    bool fresh = info != null && info.unixMs >= p.startUnixMs;
                    bool ok = !fresh || info.success;
                    ClearPending();
                    Reply(p.c, ok, fresh ? (ok ? "derlendi" : "DERLEME HATASI") : "derlenecek değişiklik yok", fresh ? compile : "");
                }
                break;

            case "wait-play":
                if (EditorApplication.isPlaying && !busy)
                {
                    p.frames++;
                    SavePending(p);
                    if (p.frames > 5) { ClearPending(); Reply(p.c, true, "Play modunda"); }
                }
                break;

            case "wait-stop":
                if (!EditorApplication.isPlaying && !busy) { ClearPending(); Reply(p.c, true, "durduruldu"); }
                break;

            case "screenshot-wait":
                // Game view boyutu değiştikten sonra UI (Canvas) yeniden yerleşsin diye
                // en az 20 OYUN karesi ve 1.5 sn bekle (editör tick'i arka planda oyun karesinden hızlı olabilir)
                p.frames++;
                SavePending(p);
                RepaintGameView();
                if (Time.frameCount - p.startFrame >= 20 && elapsed >= 1.5)
                {
                    ScreenCapture.CaptureScreenshot(p.c.path);
                    p.stage = "screenshot-file";
                    SavePending(p);
                }
                break;

            case "screenshot-file":
                p.frames++;
                SavePending(p);
                if (File.Exists(p.c.path) && new FileInfo(p.c.path).Length > 0)
                {
                    ClearPending();
                    string dims = "";
                    try
                    {
                        var bytes = File.ReadAllBytes(p.c.path);
                        if (bytes.Length > 24) dims = ((bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19]) + "x" + ((bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23]);
                    }
                    catch { }
                    bool sizeOk = p.c.width <= 0 || dims == p.c.width + "x" + p.c.height;
                    Reply(p.c, sizeOk, (sizeOk ? "kaydedildi: " : "BOYUT UYUŞMUYOR (" + dims + "): ") + p.c.path, "png=" + dims + " oyun karesi +" + (Time.frameCount - p.startFrame));
                }
                else if (p.frames > 200)
                {
                    ClearPending();
                    Reply(p.c, false, "ekran görüntüsü oluşmadı (Game view görünür mü?)");
                }
                break;

            case "tests-running":
                // Sonuç TestCallbacks.RunFinished içinde yazılır
                if (string.IsNullOrEmpty(SessionState.GetString(TestCmdKey, "")))
                    ClearPending();
                break;
        }

        if (HasPending() && elapsed > timeout)
        {
            ClearPending();
            SessionState.EraseString(TestCmdKey);
            Reply(p.c, false, "zaman aşımı (" + p.stage + ", " + (int)elapsed + " sn)");
        }
    }

    // -------------------------------------------------------
    // Ekran görüntüsü + Game view boyutu
    // -------------------------------------------------------
    static void StartScreenshot(Cmd c)
    {
        if (!EditorApplication.isPlaying) { Reply(c, false, "ekran görüntüsü için Play modu gerekli"); return; }
        if (string.IsNullOrEmpty(c.path))
            c.path = Path.Combine(Root, "screenshots", "shot_" + DateTime.Now.ToString("HHmmss") + ".png");
        Directory.CreateDirectory(Path.GetDirectoryName(c.path));
        if (File.Exists(c.path)) File.Delete(c.path);

        if (c.width > 0 && c.height > 0)
        {
            string err = SetGameViewSize(c.width, c.height);
            if (err != null) { Reply(c, false, "Game view boyutu ayarlanamadı: " + err); return; }
        }
        SetPending(c, "screenshot-wait");
    }

    static Type GameViewType => typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");

    static EditorWindow GetGameView()
    {
        var gvType = GameViewType;
        var all = Resources.FindObjectsOfTypeAll(gvType);
        if (all != null && all.Length > 0) return (EditorWindow)all[0];
        return EditorWindow.GetWindow(gvType, false, "Game", false);
    }

    static void RepaintGameView()
    {
        var gv = GetGameView();
        if (gv != null) gv.Repaint();
    }

    /// <summary>
    /// Play modu görünümüne sabit çözünürlük atar. Resmi API (PlayModeWindow) kullanılır;
    /// Device Simulator açıksa önce Game view'a geçilmelidir (Simulator, Screen boyutunu cihaza kilitler).
    /// Hata varsa mesaj döner.
    /// </summary>
    static string SetGameViewSize(int w, int h)
    {
        try
        {
            if (PlayModeWindow.GetViewType() != PlayModeWindow.PlayModeViewTypes.GameView)
                PlayModeWindow.SetViewType(PlayModeWindow.PlayModeViewTypes.GameView);
            PlayModeWindow.SetCustomRenderingResolution((uint)w, (uint)h, "KAK " + w + "x" + h);

            // ScreenCapture farklı bir Game view örneğinden okuyabiliyor: hepsini aynı boyuta ayarla
            int index = FindOrAddSizeIndex(w, h);
            var prop = GameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var o in Resources.FindObjectsOfTypeAll(GameViewType))
            {
                if (index >= 0 && prop != null && prop.CanWrite) prop.SetValue(o, index);
                ((EditorWindow)o).Repaint();
            }
            return null;
        }
        catch (Exception e)
        {
            return e.GetType().Name + ": " + (e.InnerException?.Message ?? e.Message);
        }
    }

    /// <summary>Geçerli Game view boyut grubunda w×h sabit çözünürlüğün indeksini bulur, yoksa ekler.</summary>
    static int FindOrAddSizeIndex(int w, int h)
    {
        try
        {
            var asm = typeof(EditorWindow).Assembly;
            var sizesType = asm.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instance = singleton.GetProperty("instance").GetValue(null);
            var groupType = sizesType.GetProperty("currentGroupType").GetValue(instance);
            var group = sizesType.GetMethod("GetGroup").Invoke(instance, new object[] { (int)groupType });
            var groupT = group.GetType();
            int total = (int)groupT.GetMethod("GetTotalCount").Invoke(group, null);
            var getSize = groupT.GetMethod("GetGameViewSize");
            for (int i = 0; i < total; i++)
            {
                var size = getSize.Invoke(group, new object[] { i });
                var st = size.GetType();
                if ((int)st.GetProperty("width").GetValue(size) == w && (int)st.GetProperty("height").GetValue(size) == h
                    && Convert.ToInt32(st.GetProperty("sizeType").GetValue(size)) == 1)
                    return i;
            }
            var gvsType = asm.GetType("UnityEditor.GameViewSize");
            var gvsTypeEnum = asm.GetType("UnityEditor.GameViewSizeType");
            var ctor = gvsType.GetConstructor(new[] { gvsTypeEnum, typeof(int), typeof(int), typeof(string) });
            groupT.GetMethod("AddCustomSize").Invoke(group, new[] { ctor.Invoke(new object[] { Enum.ToObject(gvsTypeEnum, 1), w, h, "KAK " + w + "x" + h }) });
            return (int)groupT.GetMethod("GetTotalCount").Invoke(group, null) - 1;
        }
        catch (Exception e)
        {
            AppendLog("BRIDGE", "boyut indeksi bulunamadı: " + e.Message);
            return -1;
        }
    }

    // -------------------------------------------------------
    // Testler
    // -------------------------------------------------------
    static void StartTests(Cmd c)
    {
        if (EditorApplication.isPlaying) { Reply(c, false, "önce Play modundan çık"); return; }
        var mode = c.arg == "EditMode" ? TestMode.EditMode : TestMode.PlayMode;
        var filter = new Filter { testMode = mode };
        if (!string.IsNullOrEmpty(c.arg2)) filter.groupNames = new[] { c.arg2 };

        SessionState.SetString(TestCmdKey, c.id + "|" + c.cmd);
        SetPending(c, "tests-running");
        EditorSceneManager.SaveOpenScenes();
        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.Execute(new ExecutionSettings(filter));
    }

    class TestCallbacks : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun) { AppendLog("TESTS", "başladı"); }

        public void RunFinished(ITestResultAdaptor result)
        {
            string key = SessionState.GetString(TestCmdKey, "");
            var sb = new StringBuilder();
            sb.AppendLine("SONUÇ: " + result.TestStatus + " | geçti " + result.PassCount + ", kaldı " + result.FailCount
                          + ", atlandı " + result.SkipCount + ", süre " + result.Duration.ToString("F1") + " sn");
            Collect(result, sb);
            AppendLog("TESTS", "bitti: " + result.TestStatus);

            if (!string.IsNullOrEmpty(key))
            {
                var parts = key.Split('|');
                SessionState.EraseString(TestCmdKey);
                ClearPending();
                Reply(new Cmd { id = parts[0], cmd = parts.Length > 1 ? parts[1] : "tests" },
                      result.FailCount == 0 && result.TestStatus != TestStatus.Failed, "testler bitti", sb.ToString());
            }
        }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (!result.HasChildren)
                AppendLog("TEST", result.TestStatus + " " + result.Test.FullName);
        }

        static void Collect(ITestResultAdaptor r, StringBuilder sb)
        {
            if (!r.HasChildren)
            {
                string mark = r.TestStatus == TestStatus.Passed ? "✅" : r.TestStatus == TestStatus.Failed ? "❌" : "⏭";
                sb.AppendLine(mark + " " + r.Test.FullName + " (" + r.Duration.ToString("F1") + " sn)");
                if (r.TestStatus == TestStatus.Failed)
                {
                    sb.AppendLine("    " + (r.Message ?? "").Trim().Replace("\n", "\n    "));
                    if (!string.IsNullOrEmpty(r.StackTrace))
                        sb.AppendLine("    " + r.StackTrace.Split('\n').FirstOrDefault(l => l.Contains(".cs:")) );
                }
                return;
            }
            foreach (var child in r.Children) Collect(child, sb);
        }
    }

    // -------------------------------------------------------
    // Statik metot çağırma (editör araçlarını tetiklemek için)
    // -------------------------------------------------------
    static void Invoke(Cmd c)
    {
        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(c.arg, false))
            .FirstOrDefault(t => t != null);
        if (type == null) { Reply(c, false, "tip bulunamadı: " + c.arg); return; }

        var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        var method = string.IsNullOrEmpty(c.path)
            ? type.GetMethod(c.arg2, flags, null, Type.EmptyTypes, null)
            : type.GetMethod(c.arg2, flags, null, new[] { typeof(string) }, null);
        if (method == null) { Reply(c, false, "metot bulunamadı: " + c.arg + "." + c.arg2); return; }

        try
        {
            object ret = method.Invoke(null, string.IsNullOrEmpty(c.path) ? null : new object[] { c.path });
            Reply(c, true, "çağrıldı: " + c.arg + "." + c.arg2, ret?.ToString());
        }
        catch (TargetInvocationException e)
        {
            Reply(c, false, "hata: " + e.InnerException);
        }
    }

    // -------------------------------------------------------
    // Yardımcılar
    // -------------------------------------------------------
    static void Reply(Cmd c, bool ok, string message, string data = null)
    {
        var r = new Result { id = c.id, cmd = c.cmd, ok = ok, message = message, data = data ?? "", unixMs = NowMs() };
        WriteFile(Path.Combine(Outbox, c.id + ".json"), JsonUtility.ToJson(r, true));
        AppendLog("BRIDGE", "cevap: " + c.cmd + " → " + (ok ? "OK" : "HATA") + " " + message);
    }

    static void WriteFile(string path, string content)
    {
        try
        {
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, content);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[KakBridge] Dosya yazılamadı: " + path + " " + e.Message);
        }
    }
}
