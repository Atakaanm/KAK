using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Mobil icin sanal joystick (analog cubuk).
/// Sol alt koseye yerlestirilen bir daire arka plan + hareket eden kucuk top.
///
/// Kurulum:
/// 1. Canvas altina JoystickBG (Image, buyuk daire) olustur.
/// 2. JoystickBG icine JoystickHandle (Image, kucuk daire) olustur.
/// 3. Bu scripti JoystickBG objesine ekle.
/// 4. Handle referansini bagla.
/// 5. PlayerMovement2D scriptindeki joystick referansini bagla.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referanslar")]
    public RectTransform handle;           // Hareket eden kucuk top
    public RectTransform background;       // Buyuk daire (arka plan)

    [Header("Ayarlar")]
    [Range(0f, 1f)]
    public float handleRange = 0.4f;       // Handle'in ne kadar uzağa gidebileceği (0-1)
    public float deadZone = 0.1f;          // Bu kadar küçük hareketleri yoksay

    private Vector2 inputDirection = Vector2.zero;
    private Canvas parentCanvas;

    /// <summary>
    /// Dışarıdan okunacak input yönü (-1 ile 1 arası, normalize).
    /// PlayerMovement2D bu değeri okuyacak.
    /// </summary>
    public Vector2 Direction => inputDirection;

    void Start()
    {
        // En yakın parent Canvas'ı bul (koordinat hesaplaması için gerekli)
        parentCanvas = GetComponentInParent<Canvas>();

        if (background == null)
        {
            background = GetComponent<RectTransform>();
        }

        // Handle'ı başlangıçta ortala
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        // Dokunulan noktayı background'un local koordinatına çevir
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        // Background boyutuna göre normalize et (-1 ile 1 arası)
        Vector2 bgSize = background.rect.size;
        Vector2 normalizedInput = new Vector2(
            localPoint.x / (bgSize.x * 0.5f),
            localPoint.y / (bgSize.y * 0.5f)
        );

        // Büyüklüğü 1'i geçmesin (daire dışına çıkmasın)
        if (normalizedInput.magnitude > 1f)
        {
            normalizedInput = normalizedInput.normalized;
        }

        // Handle'ı hareket ettir
        float maxOffset = bgSize.x * 0.5f * handleRange;
        handle.anchoredPosition = normalizedInput * maxOffset;

        // Dead zone kontrolü
        if (normalizedInput.magnitude < deadZone)
        {
            inputDirection = Vector2.zero;
        }
        else
        {
            inputDirection = normalizedInput.normalized;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Parmaği kaldirilinca her seyi sifirla
        inputDirection = Vector2.zero;

        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }
}
