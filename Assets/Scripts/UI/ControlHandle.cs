using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Faz 12 H3: düzen ekranında sürüklenen kontrol (joystick / aksiyon düğmesi). Dokununca seçilir.</summary>
public class ControlHandle : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [System.NonSerialized] public ControlLayoutEditor editor;
    public Image highlight;
    public Vector2 Pos { get; private set; }
    public float Scale { get; private set; } = 1f;

    RectTransform rt;
    Vector2 grabOffset;

    void Awake() { rt = (RectTransform)transform; }

    public void Set(Vector2 pos, float scale)
    {
        if (rt == null) rt = (RectTransform)transform;
        Pos = new Vector2(Mathf.Clamp(pos.x, 0.04f, 0.96f), Mathf.Clamp(pos.y, 0.03f, 0.97f));
        Scale = Mathf.Clamp(scale, ControlSettings.MinScale, ControlSettings.MaxScale);
        rt.anchorMin = rt.anchorMax = Pos;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one * Scale;
    }

    public void SetSelected(bool on)
    {
        if (highlight != null) highlight.enabled = on;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (editor == null) return;
        editor.Select(this);
        grabOffset = Pos - editor.ToNormalized(e.position, e.pressEventCamera);
    }

    public void OnDrag(PointerEventData e)
    {
        if (editor == null) return;
        Set(editor.ToNormalized(e.position, e.pressEventCamera) + grabOffset, Scale);
        editor.OnHandleMoved();
    }
}
