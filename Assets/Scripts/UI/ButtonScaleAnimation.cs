using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Butonlara basildiginda kucuk bir "pop" animasyonu ekler.
/// DOTween gerektirmez, sadece scale uzerinden calisir.
/// Herhangi bir UI elemanina eklenir (buton, panel vs).
/// </summary>
public class ButtonScaleAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animasyon Ayarlari")]
    public float pressedScale = 0.9f;     // Basildiginda kucultme orani
    public float animationSpeed = 10f;    // Buyume/kucultme hizi

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isInitialized = false;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Yumusak gecis (lerp)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    void OnDisable()
    {
        // Panel kapanirken scale'i sifirla
        if (isInitialized)
        {
            transform.localScale = originalScale;
            targetScale = originalScale;
        }
    }
}
