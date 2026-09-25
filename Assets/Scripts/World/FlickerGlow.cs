using UnityEngine;

/// <summary>
/// Işık halesine titreşim: Perlin gürültüsüyle ölçek ve saydamlık dalgalanması.
/// Faz 3'te Light2D yoğunluğuna da uygulanacak.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FlickerGlow : MonoBehaviour
{
    public float speed = 6f;
    public float scaleAmount = 0.08f;
    public float alphaMin = 0.7f;
    public float alphaMax = 1f;

    SpriteRenderer sr;
    Vector3 baseScale;
    Color baseColor;
    float seed;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = sr.color;
        seed = Random.value * 100f;
    }

    void Update()
    {
        float n = Mathf.PerlinNoise(seed, Time.time * speed);
        transform.localScale = baseScale * (1f + (n - 0.5f) * 2f * scaleAmount);
        Color c = baseColor;
        c.a = baseColor.a * Mathf.Lerp(alphaMin, alphaMax, n);
        sr.color = c;
    }
}
