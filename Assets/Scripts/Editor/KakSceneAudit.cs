using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahne denetimi: eksik script'ler, kopmuş (Missing) referanslar, bizim bileşenlerimizde boş kalmış alanlar.
/// Sahneyi ek (additive) açar ve kapatır: editörde açık sahneye dokunmaz. `SahneDenetimTests` bunu kullanır.
/// Menü: KacAtaKac/Denetim/Sahneleri Denetle · Köprü: invoke KakSceneAudit Run
/// </summary>
public static class KakSceneAudit
{
    public static readonly string[] Scenes = { KakEditorUtil.MenuScenePath, KakEditorUtil.GameScenePath };

    public class Report
    {
        public string scene;
        public int missingScripts, brokenRefs;
        public readonly List<string> broken = new List<string>();
        public readonly List<string> empty = new List<string>();  // "Yol / Tip.alan"
    }

    [MenuItem("KacAtaKac/Denetim/Sahneleri Denetle")]
    public static void Menu() => Debug.Log(Run());

    public static string Run()
    {
        var sb = new StringBuilder();
        foreach (var path in Scenes)
        {
            var r = Audit(path);
            sb.AppendLine($"[{r.scene}] eksik script {r.missingScripts}, kopuk referans {r.brokenRefs}, boş alan {r.empty.Count}");
            foreach (var b in r.broken) sb.AppendLine("  KOPUK: " + b);
            foreach (var e in r.empty) sb.AppendLine("  boş: " + e);
        }
        return sb.ToString();
    }

    public static Report Audit(string scenePath)
    {
        var r = new Report { scene = System.IO.Path.GetFileNameWithoutExtension(scenePath) };
        var existing = SceneManager.GetSceneByPath(scenePath);
        bool wasLoaded = existing.IsValid() && existing.isLoaded;
        var scene = wasLoaded ? existing : EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        try
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    foreach (var c in t.GetComponents<Component>())
                    {
                        if (c == null) { r.missingScripts++; r.broken.Add(Path(t) + " (eksik script)"); continue; }
                        if (!(c is MonoBehaviour mb)) continue;
                        bool ours = mb.GetType().Assembly.GetName().Name == "KacAtaKac";
                        var it = new SerializedObject(c).GetIterator();
                        while (it.NextVisible(true))
                        {
                            if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                            if (it.objectReferenceValue == null && it.objectReferenceInstanceIDValue != 0)
                            { r.brokenRefs++; r.broken.Add(Path(t) + " / " + c.GetType().Name + "." + it.propertyPath); }
                            else if (ours && it.objectReferenceValue == null && !it.propertyPath.Contains("Array") && it.depth == 0)
                                r.empty.Add(Path(t) + " / " + c.GetType().Name + "." + it.propertyPath);
                        }
                    }
        }
        finally
        {
            if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
        }
        return r;
    }

    static string Path(Transform t) => t.parent == null ? t.name : Path(t.parent) + "/" + t.name;
}
