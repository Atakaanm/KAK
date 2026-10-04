using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz 15 K4: üstten kayarak gelen kısa bilgi şeridi ("Yeni gelişim açıldı", "altın iadesi"). Sıralı, zaman ölçeğinden bağımsız.
/// </summary>
public class InfoToast : MonoBehaviour
{
    public RectTransform box;
    public TMP_Text text;
    public Image accent;
    public float hold = 2.4f;

    readonly Queue<string> msgs = new Queue<string>();
    readonly Queue<Color> colors = new Queue<Color>();
    float t = -1f;
    float hiddenY, shownY;
    bool setUp;

    void SetUp()
    {
        if (setUp || box == null) return;
        setUp = true;
        shownY = box.anchoredPosition.y;
        hiddenY = shownY + box.rect.height + 80f;
        box.anchoredPosition = new Vector2(box.anchoredPosition.x, hiddenY);
        box.gameObject.SetActive(false);
    }

    void Awake() => SetUp();

    public void Show(string msg, Color c)
    {
        SetUp();
        msgs.Enqueue(msg);
        colors.Enqueue(c);
        if (t < 0f) Next();
    }

    public bool Showing => t >= 0f;

    void Next()
    {
        if (msgs.Count == 0) { t = -1f; if (box != null) box.gameObject.SetActive(false); return; }
        if (text != null) text.SetText(msgs.Dequeue());
        var c = colors.Dequeue();
        if (accent != null) accent.color = c;
        t = 0f;
        if (box != null) { box.gameObject.SetActive(true); transform.SetAsLastSibling(); }
    }

    void Update()
    {
        if (t < 0f || box == null) return;
        t += Time.unscaledDeltaTime;
        const float slide = 0.25f;
        float y;
        if (t < slide) y = Mathf.Lerp(hiddenY, shownY, Ease(t / slide));
        else if (t < slide + hold) y = shownY;
        else if (t < slide * 2f + hold) y = Mathf.Lerp(shownY, hiddenY, Ease((t - slide - hold) / slide));
        else { Next(); return; }
        box.anchoredPosition = new Vector2(box.anchoredPosition.x, y);
    }

    static float Ease(float k) => 1f - (1f - k) * (1f - k);
}
