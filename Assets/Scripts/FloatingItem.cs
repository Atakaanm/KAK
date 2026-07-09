using UnityEngine;

/// <summary>
/// Yerde duran eşyalara yukarı-aşağı süzülme (Floating) ve
/// küre gibi parlama (Pulse) efekti verir.
/// Hareketsiz duran objeleri daha canlı ve 'alınabilir' gösterir.
/// </summary>
public class FloatingItem : MonoBehaviour
{
    [Header("Animasyon Ayarları")]
    public float floatAmplitude = 0.15f;    // Ne kadar yukarı/aşağı gidecek
    public float floatSpeed = 2.5f;         // Dalgalanma hızı

    [Header("Küre Parlama Efekti")]
    public float pulseMinScale = 0.92f;     // Minimum ölçek (nefes efekti)
    public float pulseMaxScale = 1.08f;     // Maksimum ölçek
    public float pulseSpeed = 2f;           // Parlama hızı

    private float startY;
    private Vector3 baseScale;
    private float timeOffset;   // Rastgele başlama zamanı

    void Start()
    {
        startY = transform.position.y;
        baseScale = transform.localScale;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // Yukarı-aşağı süzülme
        float newY = startY + Mathf.Sin((Time.time * floatSpeed) + timeOffset) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Küre gibi nefes alıp veren ölçek animasyonu
        float pulseT = (Mathf.Sin((Time.time * pulseSpeed) + timeOffset * 0.7f) + 1f) * 0.5f;
        float currentPulse = Mathf.Lerp(pulseMinScale, pulseMaxScale, pulseT);
        transform.localScale = baseScale * currentPulse;
    }

    /// <summary>
    /// Oyun esnasında spawn edilirse pozisyonu güncellemek için.
    /// </summary>
    public void SetStartPosition(Vector3 pos)
    {
        transform.position = pos;
        startY = pos.y;
        baseScale = transform.localScale;
    }
}
