using TMPro;
using UnityEngine;

/// <summary>Statik UI yazısını dile göre ayarlar (anahtar Loc tablosunda).</summary>
[RequireComponent(typeof(TMP_Text))]
public class LocText : MonoBehaviour
{
    public string key;

    void OnEnable()
    {
        Loc.Changed += Apply;
        Apply();
    }

    void OnDisable() => Loc.Changed -= Apply;

    public void Apply()
    {
        if (string.IsNullOrEmpty(key)) return;
        GetComponent<TMP_Text>().text = Loc.T(key);
    }
}
