using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sahne denetimi: kopmuş (Missing) referanslar, eksik script'ler, boş kalmış önemli alanlar.
/// Menü: KacAtaKac/Denetim/Sahneleri Denetle · Köprü: invoke KakSceneAudit Run
/// </summary>
public static class KakSceneAudit
{
    [MenuItem("KacAtaKac/Denetim/Sahneleri Denetle")]
    public static void Menu() => Debug.Log(Run());

    public static string Run()
    {
        var sb = new StringBuilder();
        string active = EditorSceneManager.GetActiveScene().path;
        // Kayıtlı ve değişmiş sahneleri kaydet. Adsız (Untitled) sahneye dokunma:
        // SaveOpenScenes adsız sahnede "Farklı Kaydet" penceresi açar ve editörü kilitler.
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var s = EditorSceneManager.GetSceneAt(i);
            if (s.isDirty && !string.IsNullOrEmpty(s.path)) EditorSceneManager.SaveScene(s);
        }
        foreach (var path in new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/SampleScene.unity" })
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int missingScripts = 0, missingRefs = 0, nullRefs = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    var comps = t.GetComponents<Component>();
                    foreach (var c in comps)
                    {
                        if (c == null) { missingScripts++; sb.AppendLine("  EKSİK SCRIPT: " + Path(t)); continue; }
                        if (!(c is MonoBehaviour mb)) continue;
                        string asm = mb.GetType().Assembly.GetName().Name;
                        bool ours = asm == "KacAtaKac";
                        var so = new SerializedObject(c);
                        var it = so.GetIterator();
                        while (it.NextVisible(true))
                        {
                            if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                            if (it.objectReferenceValue == null && it.objectReferenceInstanceIDValue != 0)
                            { missingRefs++; sb.AppendLine("  KOPUK: " + Path(t) + " / " + c.GetType().Name + "." + it.propertyPath); }
                            else if (ours && it.objectReferenceValue == null && !it.propertyPath.Contains("Array") && it.depth == 0)
                            { nullRefs++; sb.AppendLine("  boş: " + Path(t) + " / " + c.GetType().Name + "." + it.propertyPath); }
                        }
                    }
                }
            }
            sb.Insert(0, $"[{System.IO.Path.GetFileNameWithoutExtension(path)}] eksik script {missingScripts}, kopuk referans {missingRefs}, boş alan {nullRefs}\n");
        }
        if (!string.IsNullOrEmpty(active)) EditorSceneManager.OpenScene(active, OpenSceneMode.Single);
        return sb.ToString();
    }

    /// <summary>Test/otomasyon için: sahne yolu verilir, sonuç (eksik script + kopuk referans sayısı) döner. Sahneyi açar.</summary>
    public static int CountBroken(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        int broken = 0;
        foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                foreach (var c in t.GetComponents<Component>())
                {
                    if (c == null) { broken++; continue; }
                    var it = new SerializedObject(c).GetIterator();
                    while (it.NextVisible(true))
                        if (it.propertyType == SerializedPropertyType.ObjectReference
                            && it.objectReferenceValue == null && it.objectReferenceInstanceIDValue != 0)
                            broken++;
                }
        return broken;
    }

    static string Path(Transform t) => t.parent == null ? t.name : Path(t.parent) + "/" + t.name;
}
