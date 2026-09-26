using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>Editör araçlarının ortak, pencere açmayan yardımcıları (köprü/otomasyon güvenli).</summary>
public static class KakEditorUtil
{
    public const string GameScenePath = "Assets/Scenes/Game.unity";
    public const string MenuScenePath = "Assets/Scenes/MainMenu.unity";

    /// <summary>
    /// Editörü aynı projeyle yeniden başlatır (yeni kurulan modüllerin görünmesi için). Önce kayıtlı sahneleri kaydeder.
    /// osascript "quit" bazen AppleEvent zaman aşımına düşüyor; bu yol güvenilir. Köprü: invoke KakEditorUtil RestartEditor
    /// </summary>
    public static void RestartEditor()
    {
        SaveNamedScenes();
        AssetDatabase.SaveAssets();
        EditorApplication.delayCall += () => EditorApplication.OpenProject(System.IO.Directory.GetCurrentDirectory());
    }

    /// <summary>
    /// Sadece diske kayıtlı ve değişmiş sahneleri kaydeder. EditorSceneManager.SaveOpenScenes adsız
    /// (Untitled) sahnede "Farklı Kaydet" penceresi açar ve arka planda çalışan editörü kilitler.
    /// </summary>
    public static bool SaveNamedScenes()
    {
        bool ok = true;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var s = EditorSceneManager.GetSceneAt(i);
            if (s.isDirty && !string.IsNullOrEmpty(s.path)) ok &= EditorSceneManager.SaveScene(s);
        }
        return ok;
    }

    /// <summary>
    /// Asset'leri Unity üzerinden siler (.meta ile birlikte). Yollar ';' ile ayrılır.
    /// Köprü: python3 tools/kak_bridge.py invoke KakEditorUtil DeleteAssets "Assets/a.cs;Assets/B"
    /// </summary>
    public static string DeleteAssets(string paths)
    {
        var sb = new StringBuilder();
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var raw in paths.Split(';'))
            {
                string p = raw.Trim();
                if (p.Length == 0) continue;
                bool ok = AssetDatabase.DeleteAsset(p);
                sb.AppendLine((ok ? "silindi: " : "YOK/silinemedi: ") + p);
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }
        AssetDatabase.Refresh();
        return sb.ToString();
    }

    /// <summary>
    /// Asset'leri Unity üzerinden taşır/yeniden adlandırır (GUID korunur, referanslar kopmaz).
    /// Çiftler ';' ile, kaynak ve hedef '>' ile ayrılır: "Assets/A.unity>Assets/B.unity;..."
    /// Sahne taşınırsa Build Settings yolu da güncellenir.
    /// </summary>
    public static string MoveAssets(string pairs)
    {
        var sb = new StringBuilder();
        foreach (var raw in pairs.Split(';'))
        {
            var parts = raw.Split('>');
            if (parts.Length != 2) continue;
            string from = parts[0].Trim(), to = parts[1].Trim();
            string err = AssetDatabase.MoveAsset(from, to);
            sb.AppendLine(string.IsNullOrEmpty(err) ? "taşındı: " + from + " → " + to : "HATA: " + from + ": " + err);
            if (string.IsNullOrEmpty(err) && to.EndsWith(".unity"))
            {
                var scenes = EditorBuildSettings.scenes;
                for (int i = 0; i < scenes.Length; i++)
                    if (scenes[i].path == from) scenes[i] = new EditorBuildSettingsScene(to, scenes[i].enabled);
                EditorBuildSettings.scenes = scenes;
            }
        }
        AssetDatabase.SaveAssets();
        return sb.ToString();
    }
}
