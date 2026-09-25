using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Mobil sanal joystick (kayan).
/// Bu bileşen dokunma bölgesinin (JoystickZone) üzerindedir: bölgede nereye dokunulursa
/// joystick tabanı orada belirir, sürükleme yönü okunur, parmak kalkınca taban dinlenme
/// konumuna döner ve soluklaşır. Bölge arenanın altındaki kontrol alanındadır (arenaya binmez).
///
/// PlayerMovement2D, Direction'ı okur (-1..1, normalize).
/// </summary>
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referanslar")]
    public RectTransform handle;           // Hareket eden kucuk top
    public RectTransform background;       // Buyuk daire (taban)

    [Header("Ayarlar")]
    [Range(0f, 1f)]
    public float handleRange = 0.45f;      // Tutamağın tabandan ne kadar uzağa gidebileceği (0-1)
    public float deadZone = 0.12f;         // Bu kadar küçük hareketleri yoksay
    [Tooltip("Açık: dokunulan yerde belirir. Kapalı: sabit konumda durur.")]
    public bool floating = true;

    [Header("Görünürlük")]
    public float idleAlpha = 0.22f;        // Dokunulmuyorken (ipucu)
    public float activeAlpha = 0.85f;
    public float fadeSpeed = 10f;

    private Vector2 inputDirection = Vector2.zero;
    private Vector2 restPosition;
    private CanvasGroup group;
    private int activePointer = int.MinValue;
    private RectTransform zone;

    /// <summary>
    /// Dışarıdan okunacak input yönü (-1 ile 1 arası, normalize).
    /// PlayerMovement2D bu değeri okuyacak.
    /// </summary>
    public Vector2 Direction => inputDirection;
    public bool IsHeld => activePointer != int.MinValue;

    void Awake()
    {
        zone = transform as RectTransform;
        if (background == null) background = zone;
        if (background != null)
        {
            group = background.GetComponent<CanvasGroup>();
            if (group == null) group = background.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false; // dokunmayı bölge alır
            group.alpha = idleAlpha;
        }
        // Bölge görünmez ama dokunmayı yakalar
        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    void Start()
    {
        if (background != null) restPosition = background.anchoredPosition;
        if (handle != null) handle.anchoredPosition = Vector2.zero;
    }

    void Update()
    {
        if (group == null) return;
        float target = IsHeld ? activeAlpha : idleAlpha;
        if (!Mathf.Approximately(group.alpha, target))
            group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsHeld) return;
        activePointer = eventData.pointerId;

        if (floating && background != null && background != zone)
        {
            RectTransform parent = background.parent as RectTransform;
            if (parent != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                background.anchoredPosition = local - PivotOffset(background, parent);
            }
        }
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointer) return;
        if (background == null || handle == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        Vector2 bgSize = background.rect.size;
        // Tabanın merkezine göre (-1..1)
        Vector2 centerOffset = (new Vector2(0.5f, 0.5f) - background.pivot) * bgSize;
        Vector2 normalizedInput = new Vector2(
            (localPoint.x - centerOffset.x) / (bgSize.x * 0.5f),
            (localPoint.y - centerOffset.y) / (bgSize.y * 0.5f));

        if (normalizedInput.magnitude > 1f)
            normalizedInput = normalizedInput.normalized;

        float maxOffset = bgSize.x * 0.5f * handleRange;
        handle.anchoredPosition = normalizedInput * maxOffset;

        inputDirection = normalizedInput.magnitude < deadZone ? Vector2.zero : normalizedInput.normalized;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointer) return;
        activePointer = int.MinValue;
        inputDirection = Vector2.zero;
        if (handle != null) handle.anchoredPosition = Vector2.zero;
        if (floating && background != null && background != zone) background.anchoredPosition = restPosition;
    }

    void OnDisable()
    {
        activePointer = int.MinValue;
        inputDirection = Vector2.zero;
    }

    /// <summary>anchoredPosition, ebeveynin anchor noktasına göredir; yerel noktayı buna çevirir.</summary>
    static Vector2 PivotOffset(RectTransform child, RectTransform parent)
    {
        Vector2 anchorCenter = (child.anchorMin + child.anchorMax) * 0.5f;
        return (anchorCenter - parent.pivot) * parent.rect.size;
    }
}
