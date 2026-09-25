using TMPro;
using UnityEngine;

/// <summary>
/// Dünyada yükselip sönen kısa yazılar ("YAKIN!", "+10", "Kalkan!"). Havuzlu TextMeshPro, tahsissiz.
/// Eski TextMesh + new GameObject yönteminin yerini alır.
/// </summary>
public class WorldPopup : MonoBehaviour
{
    public static WorldPopup Instance { get; private set; }

    public int poolSize = 10;
    public float rise = 0.9f;
    public float life = 0.8f;
    public float fontSize = 3.2f;
    public TMP_FontAsset font;

    TextMeshPro[] items;
    float[] age;
    Vector3[] start;
    int next;

    void Awake()
    {
        Instance = this;
        items = new TextMeshPro[poolSize];
        age = new float[poolSize];
        start = new Vector3[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("Popup" + i);
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<TextMeshPro>();
            if (font != null) t.font = font;
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = fontSize;
            t.fontStyle = FontStyles.Bold;
            t.outlineWidth = 0.25f;
            t.outlineColor = KakPalette.Murekkep;
            t.sortingOrder = 300;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            go.SetActive(false);
            items[i] = t;
            age[i] = 99f;
        }
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public static void Show(string text, Vector3 pos, Color color, float scale = 1f)
    {
        if (Instance != null) Instance.ShowInternal(text, pos, color, scale);
    }

    void ShowInternal(string text, Vector3 pos, Color color, float scale)
    {
        int i = next;
        next = (next + 1) % items.Length;
        var t = items[i];
        t.gameObject.SetActive(true);
        t.SetText(text);
        t.color = color;
        t.transform.localScale = Vector3.one * scale;
        start[i] = pos + new Vector3(0f, 0.45f, 0f);
        t.transform.position = start[i];
        age[i] = 0f;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        for (int i = 0; i < items.Length; i++)
        {
            if (age[i] >= life) continue;
            age[i] += dt;
            float k = age[i] / life;
            var t = items[i];
            if (k >= 1f) { t.gameObject.SetActive(false); continue; }
            // Hızlı çıkış, yavaşlayarak yükselme
            float ease = 1f - (1f - k) * (1f - k);
            t.transform.position = start[i] + new Vector3(0f, rise * ease, 0f);
            float pop = k < 0.15f ? Mathf.Lerp(0.6f, 1.15f, k / 0.15f) : Mathf.Lerp(1.15f, 1f, (k - 0.15f) / 0.2f);
            t.transform.localScale = Vector3.one * pop;
            Color c = t.color; c.a = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f; t.color = c;
        }
    }
}
