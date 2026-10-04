using TMPro;
using UnityEngine;

/// <summary>Faz 15 K6: menüdeki SANDIK düğmesi — anahtar sayısı rozeti; açılabilecek sandık varsa rozet nabız gibi atar.</summary>
public class ChestButton : MonoBehaviour
{
    public RectTransform badge;
    public TMP_Text badgeText;
    public RectTransform chestIcon;

    void OnEnable()
    {
        ChestSystem.Changed += Refresh;
        Refresh();
    }

    void OnDisable() => ChestSystem.Changed -= Refresh;

    public void Refresh()
    {
        int keys = ChestSystem.Keys;
        if (badge != null) badge.gameObject.SetActive(keys > 0);
        if (badgeText != null) badgeText.SetText("{0}", keys);
    }

    void Update()
    {
        bool can = ChestSystem.CanOpen(0);
        float s = can ? 1f + 0.12f * Mathf.Max(0f, Mathf.Sin(Time.unscaledTime * 5f)) : 1f;
        if (badge != null) badge.localScale = new Vector3(s, s, 1f);
        if (chestIcon != null) chestIcon.localRotation = Quaternion.Euler(0f, 0f, can ? Mathf.Sin(Time.unscaledTime * 9f) * 4f * Mathf.Max(0f, Mathf.Sin(Time.unscaledTime * 1.3f)) : 0f);
    }
}
