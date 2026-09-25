using UnityEditor;
using UnityEngine;

/// <summary>
/// Köprü üzerinden çağrılan geliştirme ayarları.
/// Örnek: python3 tools/kak_bridge.py invoke KakDevSetup RunInBackground
/// </summary>
public static class KakDevSetup
{
    /// <summary>Editör odakta değilken Play modunun durmaması için (mobilde etkisi yok).</summary>
    public static string RunInBackground()
    {
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        return "runInBackground=" + PlayerSettings.runInBackground;
    }
}
