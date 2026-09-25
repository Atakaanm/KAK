using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Ayarlarda dil değiştirici: TR ⇄ EN.</summary>
public class LanguageButton : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text label;

    void OnEnable() { Loc.Changed += Refresh; Refresh(); }
    void OnDisable() => Loc.Changed -= Refresh;

    public void OnPointerClick(PointerEventData eventData)
    {
        Loc.Current = Loc.Current == Loc.Lang.TR ? Loc.Lang.EN : Loc.Lang.TR;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
    }

    void Refresh()
    {
        if (label != null) label.text = Loc.Current == Loc.Lang.TR ? "TÜRKÇE" : "ENGLISH";
    }
}
