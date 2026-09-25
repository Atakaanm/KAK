using System.Collections;
using UnityEngine;

/// <summary>
/// Zaman ölçeği için tek kaynak. Üç katman birleşir:
///   temel ölçek (1 normal, SloMo'da 0.4, oyun sonunda 0) × duraklatma × vuruş duraklaması (hit-stop)
/// Katmanlar birbirinin değerini bozmaz (ör. SloMo sırasında pause/resume, pause sırasında hit-stop).
/// fixedDeltaTime sadece temel ölçekle eşlenir (SloMo'da fizik akıcı kalsın).
/// </summary>
public static class KakTime
{
    public const float DefaultFixedDelta = 0.02f;
    const float HitStopScale = 0.02f;

    static float baseScale = 1f;

    /// <summary>Sadece test/denge ölçümü: oyunu hızlandırır (fizik adımı değişmez).</summary>
    public static float TestSpeed = 1f;
    static bool paused;
    static float hitStopEnd = -1f;

    public static float BaseScale => baseScale;
    public static bool Paused => paused;
    public static bool HitStopping => hitStopEnd > 0f;

    /// <summary>Temel ölçek (SloMo, oyun sonu). Pause ve hit-stop'u korur.</summary>
    public static void SetTimeScale(float scale)
    {
        baseScale = Mathf.Max(0f, scale);
        Apply();
    }

    public static void SetPaused(bool value)
    {
        paused = value;
        Apply();
    }

    /// <summary>Kısa vuruş duraklaması (gerçek zaman). Pause veya oyun sonunda yok sayılır.</summary>
    public static void HitStop(float seconds)
    {
        if (paused || baseScale <= 0.01f || seconds <= 0f) return;
        hitStopEnd = Mathf.Max(hitStopEnd, Time.unscaledTime + seconds);
        KakTimeRunner.Ensure();
        Apply();
    }

    /// <summary>KakTimeRunner her kare çağırır.</summary>
    public static void Tick()
    {
        if (hitStopEnd > 0f && Time.unscaledTime >= hitStopEnd)
        {
            hitStopEnd = -1f;
            Apply();
        }
    }

    public static void ResetAll()
    {
        baseScale = 1f;
        paused = false;
        hitStopEnd = -1f;
        Time.timeScale = TestSpeed;
        Time.fixedDeltaTime = DefaultFixedDelta;
    }

    /// <summary>Test hızını değiştirir ve uygular.</summary>
    public static void SetTestSpeed(float speed)
    {
        TestSpeed = Mathf.Max(0.1f, speed);
        Apply();
    }

    static void Apply()
    {
        float s = baseScale;
        if (hitStopEnd > 0f) s = Mathf.Min(s, HitStopScale);
        if (paused) s = 0f;
        Time.timeScale = s * TestSpeed;
        if (baseScale > 0.01f)
            Time.fixedDeltaTime = DefaultFixedDelta * baseScale;
    }

    /// <summary>
    /// Oyuncunun yaşadığı süre kadar bekler: gerçek zamanla sayar (SloMo sırasında uzamaz)
    /// ama oyun duraklatıldığında saymaz. Powerup süreleri (Hız, Hayalet, SloMo) bununla ölçülür.
    /// </summary>
    public static IEnumerator WaitGameplay(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (!paused && Time.timeScale > 0f) t += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
