using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz 15 K4: küçük piksel kıvılcım patlaması (yükseltme, sandık). Bulunduğu noktadan kare parçalar saçılır, yavaşlar, söner.
/// Havuzlu Image'lar, zaman ölçeğinden bağımsız. Burst(renk) ile tetiklenir.
/// </summary>
public class UiBurst : MonoBehaviour
{
    public Sprite sprite;
    public int count = 14;
    public float life = 0.55f;
    public float speed = 520f;

    RectTransform[] bits;
    Image[] imgs;
    Vector2[] vel;
    float[] age;
    Color tint;
    bool running;

    void Build()
    {
        bits = new RectTransform[count];
        imgs = new Image[count];
        vel = new Vector2[count];
        age = new float[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Spark" + i, typeof(RectTransform));
            bits[i] = (RectTransform)go.transform;
            bits[i].SetParent(transform, false);
            imgs[i] = go.AddComponent<Image>();
            imgs[i].sprite = sprite;
            imgs[i].raycastTarget = false;
            go.SetActive(false);
        }
    }

    public void Burst(Color color)
    {
        if (bits == null) Build();
        tint = color;
        for (int i = 0; i < count; i++)
        {
            float a = (i + Random.Range(-0.3f, 0.3f)) / count * Mathf.PI * 2f;
            vel[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed * Random.Range(0.55f, 1.1f);
            age[i] = 0f;
            float s = Random.Range(10f, 18f);
            bits[i].sizeDelta = new Vector2(s, s);
            bits[i].anchoredPosition = Vector2.zero;
            bits[i].gameObject.SetActive(true);
            imgs[i].color = i % 3 == 0 ? Color.white : color;
        }
        running = true;
    }

    void Update()
    {
        if (!running) return;
        float dt = Time.unscaledDeltaTime;
        bool any = false;
        for (int i = 0; i < count; i++)
        {
            if (!bits[i].gameObject.activeSelf) continue;
            age[i] += dt;
            float k = age[i] / life;
            if (k >= 1f) { bits[i].gameObject.SetActive(false); continue; }
            any = true;
            vel[i] *= Mathf.Exp(-6f * dt);
            bits[i].anchoredPosition += vel[i] * dt;
            var c = imgs[i].color;
            c.a = 1f - k * k;
            imgs[i].color = c;
        }
        running = any;
    }
}
