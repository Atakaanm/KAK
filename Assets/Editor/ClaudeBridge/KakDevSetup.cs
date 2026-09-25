using UnityEditor;
using UnityEngine;

/// <summary>
/// Köprü üzerinden çağrılan geliştirme ayarları.
/// Örnek: python3 tools/kak_bridge.py invoke KakDevSetup RunInBackground
/// </summary>
public static class KakDevSetup
{
    /// <summary>Play modu görünümü (Game / Simulator) ve çözünürlüğü.</summary>
    public static string ViewInfo()
    {
        var type = PlayModeWindow.GetViewType();
        PlayModeWindow.GetRenderingResolution(out uint w, out uint h);
        return "görünüm=" + type + " çözünürlük=" + w + "x" + h;
    }

    /// <summary>Otomatik ekran görüntüleri için Game view'a geç (Simulator Screen boyutunu kilitler).</summary>
    public static string UseGameView()
    {
        PlayModeWindow.SetViewType(PlayModeWindow.PlayModeViewTypes.GameView);
        return ViewInfo();
    }

    public static string GodModeOn() => SetGod(true);
    public static string GodModeOff() => SetGod(false);

    static string SetGod(bool on)
    {
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType("PlayerHealth", false);
            var f = t?.GetField("DevGodMode");
            if (f != null) { f.SetValue(null, on); return "DevGodMode=" + on; }
        }
        return "PlayerHealth.DevGodMode bulunamadı";
    }

    public static string UseSimulatorView()
    {
        PlayModeWindow.SetViewType(PlayModeWindow.PlayModeViewTypes.SimulatorView);
        return ViewInfo();
    }

    /// <summary>Editör odakta değilken Play modunun durmaması için (mobilde etkisi yok).</summary>
    public static string RunInBackground()
    {
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        return "runInBackground=" + PlayerSettings.runInBackground;
    }
}
