using UnityEngine;

/// <summary>
/// Basit kare animasyonu (meşale alevi vb.). Tahsissiz; rastgele faz ile senkron dışı çalışır.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipbook : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 8f;
    public bool randomStart = true;

    SpriteRenderer sr;
    float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (randomStart) t = Random.value * 10f;
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        t += Time.deltaTime;
        int i = (int)(t * fps) % frames.Length;
        if (sr.sprite != frames[i]) sr.sprite = frames[i];
    }
}
