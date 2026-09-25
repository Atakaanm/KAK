using System.Diagnostics;

/// <summary>
/// Bilgi logları sadece editörde ve development build'de derlenir.
/// [Conditional] sayesinde release build'de çağrı ve argüman hesaplaması tamamen kalkar (GC ve CPU maliyeti yok).
/// Uyarı ve hatalar için doğrudan Debug.LogWarning / Debug.LogError kullan.
/// </summary>
public static class KakLog
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Info(string message, UnityEngine.Object context = null)
    {
        UnityEngine.Debug.Log(message, context);
    }
}
