using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Ayarlarda "Kontroller: DÜZENLE" — serbest kontrol düzeni ekranını açar (Faz 12 H3; G1'de boyut döngüsüydü).</summary>
public class ControlSizeButton : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text label;

    void OnEnable() { Loc.Changed += Refresh; Refresh(); }
    void OnDisable() { Loc.Changed -= Refresh; }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        ControlLayoutEditor.Open(transform);
    }

    void Refresh()
    {
        if (label != null) label.text = Loc.T("edit_controls");
    }
}
