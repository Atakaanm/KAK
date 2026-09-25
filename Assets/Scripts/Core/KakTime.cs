using System.Collections;
using UnityEngine;

/// <summary>
/// Zaman ölçeği yönetimi için tek kaynak.
/// Time.timeScale değişirken fizik adımını (fixedDeltaTime) da eşler;
/// timeScale = 0 (pause/game over) durumunda fizik adımına dokunmaz.
/// </summary>
public static class KakTime
{
    public const float DefaultFixedDelta = 0.02f;

    public static void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        if (scale > 0.01f)
            Time.fixedDeltaTime = DefaultFixedDelta * scale;
    }

    public static void ResetAll()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = DefaultFixedDelta;
    }

    /// <summary>
    /// Oyuncunun yaşadığı süre kadar bekler: gerçek zamanla sayar (SloMo sırasında uzamaz)
    /// ama oyun duraklatıldığında (timeScale = 0) saymaz.
    /// Powerup süreleri (Hız, Hayalet, SloMo) bununla ölçülür.
    /// </summary>
    public static IEnumerator WaitGameplay(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (Time.timeScale > 0f) t += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
