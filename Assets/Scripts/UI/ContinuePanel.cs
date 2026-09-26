using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "Devam et?" paneli (Faz Y3): ölümden sonra, reklam hazırsa. Geri sayım halkası; İZLE → ödüllü reklam → canlan,
/// HAYIR ya da süre dolunca → oyun sonu. Zaman ölçeği 0 iken çalışır (gerçek zaman).
/// </summary>
public class ContinuePanel : MonoBehaviour
{
    public Image ring;
    public TMP_Text countText;
    public Button watchButton;
    public Button declineButton;
    public RectTransform panel;

    Action onAccept, onDecline;
    float total, left;
    bool waiting;

    void Awake()
    {
        if (watchButton != null) watchButton.onClick.AddListener(Accept);
        if (declineButton != null) declineButton.onClick.AddListener(Decline);
    }

    public bool Visible => gameObject.activeSelf;

    public void Show(float seconds, Action accept, Action decline)
    {
        onAccept = accept;
        onDecline = decline;
        total = left = Mathf.Max(1f, seconds);
        waiting = true;
        gameObject.SetActive(true);
        if (panel != null) panel.localScale = Vector3.one * 0.85f;
    }

    public void Hide()
    {
        waiting = false;
        gameObject.SetActive(false);
    }

    /// <summary>İZLE (testler de çağırır).</summary>
    public void Accept()
    {
        if (!waiting) return;
        waiting = false; // reklam sırasında geri sayım durur
        onAccept?.Invoke();
    }

    public void Decline()
    {
        if (!waiting) return;
        waiting = false;
        onDecline?.Invoke();
    }

    void Update()
    {
        if (panel != null && panel.localScale.x < 1f)
            panel.localScale = Vector3.one * Mathf.MoveTowards(panel.localScale.x, 1f, Time.unscaledDeltaTime * 2f);
        if (!waiting) return;
        left -= Time.unscaledDeltaTime;
        if (ring != null) ring.fillAmount = Mathf.Clamp01(left / total);
        if (countText != null) countText.SetText("{0}", Mathf.CeilToInt(Mathf.Max(0f, left)));
        if (left <= 0f) Decline();
    }
}
