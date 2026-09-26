using UnityEditor.SceneManagement;

/// <summary>Editör araçlarının ortak, pencere açmayan yardımcıları (köprü/otomasyon güvenli).</summary>
public static class KakEditorUtil
{
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
}
