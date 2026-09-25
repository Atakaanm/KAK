using UnityEngine;

/// <summary>RectTransform'u ekranın güvenli alanına (çentik, alt çubuk) sığdırır. Menü ekranları için.</summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    Rect last;
    Vector2Int lastSize;

    void Update()
    {
        Rect safe = Screen.safeArea;
        if (safe == last && lastSize.x == Screen.width && lastSize.y == Screen.height) return;
        last = safe;
        lastSize = new Vector2Int(Screen.width, Screen.height);
        var rt = (RectTransform)transform;
        Vector2 min = safe.position, max = safe.position + safe.size;
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
