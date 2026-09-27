using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Ayarlarda kontrol boyutu: KÜÇÜK → ORTA → BÜYÜK (G1).</summary>
public class ControlSizeButton : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text label;

    void OnEnable() { ControlSettings.Changed += Refresh; Loc.Changed += Refresh; Refresh(); }
    void OnDisable() { ControlSettings.Changed -= Refresh; Loc.Changed -= Refresh; }

    public void OnPointerClick(PointerEventData eventData)
    {
        ControlSettings.Cycle();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
    }

    void Refresh()
    {
        if (label != null) label.text = Loc.T(ControlSettings.LabelKey);
    }
}
