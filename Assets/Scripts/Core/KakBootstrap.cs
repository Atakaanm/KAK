using UnityEngine;

/// <summary>
/// Oyun açılışında bir kez çalışan genel ayarlar.
/// </summary>
public static class KakBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Application.targetFrameRate = 60;
        KakTime.ResetAll();
    }
}
