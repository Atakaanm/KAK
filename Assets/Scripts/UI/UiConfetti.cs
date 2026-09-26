using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI konfeti (Faz 3c.7): yeni rekorda panelin üstünden palet renklerinde dönen parçalar yağar.
/// Havuzlu Image'lar, zaman ölçeğinden bağımsız (oyun sonunda timeScale 0). Burst() ile tetiklenir.
/// </summary>
public class UiConfetti : MonoBehaviour
{
    public Sprite sprite;
    public int count = 48;
    public float life = 2.2f;

    RectTransform[] bits;
    Image[] imgs;
    Vector2[] vel;
    float[] spin, age;
    bool running;

    static readonly Color[] Colors =
    {
        new Color(0.996f, 0.682f, 0.204f), new Color(0.996f, 0.906f, 0.380f), new Color(0.173f, 0.910f, 0.961f),
        new Color(0.388f, 0.780f, 0.302f), new Color(0.710f, 0.314f, 0.533f), new Color(0.918f, 0.831f, 0.667f)
    };

    void Build()
    {
        bits = new RectTransform[count];
        imgs = new Image[count];
        vel = new Vector2[count];
        spin = new float[count];
        age = new float[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Bit" + i, typeof(RectTransform));
            bits[i] = (RectTransform)go.transform;
            bits[i].SetParent(transform, false);
            imgs[i] = go.AddComponent<Image>();
            imgs[i].sprite = sprite;
            imgs[i].raycastTarget = false;
            go.SetActive(false);
        }
    }

    public void Burst()
    {
        if (bits == null) Build();
        var rt = (RectTransform)transform;
        float w = rt.rect.width;
        for (int i = 0; i < count; i++)
        {
            bits[i].gameObject.SetActive(true);
            bits[i].anchoredPosition = new Vector2(Random.Range(-w * 0.45f, w * 0.45f), Random.Range(0f, 120f));
            bits[i].sizeDelta = new Vector2(Random.Range(12f, 22f), Random.Range(20f, 34f));
            bits[i].localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            imgs[i].color = Colors[i % Colors.Length];
            vel[i] = new Vector2(Random.Range(-160f, 160f), Random.Range(250f, 700f));
            spin[i] = Random.Range(-540f, 540f);
            age[i] = Random.Range(-0.25f, 0f); // hafif sıralı çıkış
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
            if (age[i] < 0f) { any = true; continue; }
            vel[i] += new Vector2(0f, -1400f) * dt;   // yerçekimi
            vel[i].x *= 1f - 1.2f * dt;               // hava direnci
            bits[i].anchoredPosition += vel[i] * dt;
            bits[i].localRotation *= Quaternion.Euler(0f, 0f, spin[i] * dt);
            var c = imgs[i].color; c.a = Mathf.Clamp01((life - age[i]) / 0.5f); imgs[i].color = c;
            if (age[i] >= life) bits[i].gameObject.SetActive(false); else any = true;
        }
        running = any;
    }

    public bool Running => running; // testler için
}
