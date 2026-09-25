using UnityEngine;

/// <summary>
/// Ekran sarsıntısı. ScreenComposer'ın hesapladığı kamera merkezine geçici ofset ekler.
/// Gerçek zamanla söner (hit-stop ve SloMo'dan etkilenmez). Ayarlardan kapatılabilir.
/// </summary>
[DefaultExecutionOrder(100)]
public class KakCameraShake : MonoBehaviour
{
    public static KakCameraShake Instance { get; private set; }
    public static bool Enabled
    {
        get => SaveSystem.Data.settings.screenShake;
        set { SaveSystem.Data.settings.screenShake = value; SaveSystem.Save(); }
    }

    ScreenComposer composer;
    float amplitude, duration, endTime = -1f, seed;

    void Awake()
    {
        Instance = this;
        composer = GetComponent<ScreenComposer>();
        seed = Random.value * 100f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Shake(float amp, float dur)
    {
        if (!Enabled || amp <= 0f || dur <= 0f) return;
        // Daha güçlü sarsıntı zayıfı ezer, zayıf olan güçlüyü kısaltmaz
        float remaining = endTime - Time.unscaledTime;
        if (remaining > 0f && amplitude * (remaining / duration) > amp) return;
        amplitude = amp;
        duration = dur;
        endTime = Time.unscaledTime + dur;
    }

    void LateUpdate()
    {
        if (endTime < 0f || composer == null) return;
        Vector2 center = composer.Current.cameraCenter;
        float z = transform.position.z;
        float remaining = endTime - Time.unscaledTime;
        if (remaining <= 0f)
        {
            endTime = -1f;
            transform.position = new Vector3(center.x, center.y, z);
            return;
        }
        float k = remaining / duration;
        float t = Time.unscaledTime * 40f;
        Vector2 off = new Vector2(Mathf.PerlinNoise(seed, t) - 0.5f, Mathf.PerlinNoise(seed + 7.3f, t) - 0.5f) * 2f * amplitude * k * k;
        transform.position = new Vector3(center.x + off.x, center.y + off.y, z);
    }
}
