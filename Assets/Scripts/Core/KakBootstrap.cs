using UnityEngine;

/// <summary>
/// Oyun açılışında bir kez çalışan genel ayarlar.
/// </summary>
public static class KakBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AfterLoad()
    {
        FontWarmup.Run();
        SaveSystem.TouchPlayDay(); // farklı günlerde oynama sayısı (günlük ödül açılması)
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // oyun sırasında ekran kararmasın
        KakTime.ResetAll();
    }
}
