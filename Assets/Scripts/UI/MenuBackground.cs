using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu arka planina hareketlilik katar.
/// Yavasce yukari kayan parcacik efekti gibi bir his yaratir.
/// Logo veya basliga hafif "float" efekti verir.
/// </summary>
public class MenuBackground : MonoBehaviour
{
    [Header("Logo / Baslik Float Efekti")]
    public RectTransform titleTransform;
    public float floatAmplitude = 8f;     // Yukari-asagi piksel miktari
    public float floatSpeed = 1.5f;       // Sallanma hizi

    [Header("Arka Plan Parcaciklari (opsiyonel)")]
    public RectTransform[] particles;     // Kucuk parlak noktalar
    public float particleSpeed = 20f;     // Yukari kayma hizi
    public float particleResetY = -600f;  // Reset noktasi (aşağı)
    public float particleTopY = 600f;     // Ust sinir

    private Vector2 titleStartPos;
    private Vector2[] particleStartPositions;

    void Start()
    {
        if (titleTransform != null)
        {
            titleStartPos = titleTransform.anchoredPosition;
        }

        if (particles != null && particles.Length > 0)
        {
            particleStartPositions = new Vector2[particles.Length];
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] != null)
                {
                    particleStartPositions[i] = particles[i].anchoredPosition;
                }
            }
        }
    }

    void Update()
    {
        // Logo float efekti
        if (titleTransform != null)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            titleTransform.anchoredPosition = titleStartPos + new Vector2(0f, yOffset);
        }

        // Parcacik yukari kayma efekti
        if (particles != null)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] == null) continue;

                Vector2 pos = particles[i].anchoredPosition;
                pos.y += particleSpeed * Time.deltaTime;

                if (pos.y > particleTopY)
                {
                    pos.y = particleResetY;
                    pos.x = Random.Range(-200f, 200f); // Rastgele X konumu
                }

                particles[i].anchoredPosition = pos;
            }
        }
    }
}
