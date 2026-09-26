using TMPro;
using UnityEngine;

/// <summary>Menüde cüzdan: altın ikonu + miktar. Altın özelliği kilitliyken gizli. Satın almadan sonra Refresh().</summary>
public class WalletHud : MonoBehaviour
{
    public RectTransform content;
    public TMP_Text amount;

    void OnEnable() => Refresh();

    public void Refresh()
    {
        bool on = FeatureGate.IsUnlocked(Feature.Coins);
        if (content != null) content.gameObject.SetActive(on);
        if (on && amount != null) amount.SetText("{0}", SaveSystem.Data.coins);
    }
}
