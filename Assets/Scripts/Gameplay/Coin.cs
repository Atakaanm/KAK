using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toplanabilir altın (Faz 3c.1). Havuzlu (ProjectilePool), dönme animasyonu ve zıplama sadece görsel çocukta
/// (collider'lı kök her kare hareket etmez). Ömrünün sonunda yanıp söner ve havuza döner.
/// Oyuncunun herhangi bir collider'ı (ayak izi veya gövde) değince toplanır: ScoreManager.AddCoins.
/// Active listesi mıknatıs (pet) ve testler içindir.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class Coin : MonoBehaviour
{
    public static readonly List<Coin> Active = new List<Coin>(32);

    public Sprite[] frames;
    public SpriteRenderer visual;
    public int value = 1;
    public float fps = 10f;
    public float lifetime = 9f;
    public float warnTime = 2.5f;
    public float bobHeight = 0.05f;

    float age, phase;
    bool collected;

    void OnEnable()
    {
        age = 0f;
        collected = false;
        phase = Random.value * 10f;
        if (visual != null)
        {
            visual.enabled = true;
            visual.color = Color.white;
            visual.transform.localScale = Vector3.zero; // beliriş animasyonu
        }
        Active.Add(this);
    }

    void OnDisable() => Active.Remove(this);

    void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime) { Despawn(); return; }
        if (visual == null) return;

        if (frames != null && frames.Length > 0)
            visual.sprite = frames[(int)((age + phase) * fps) % frames.Length];

        // Beliriş: küçükten hafif taşarak (0,2 sn)
        float appear = Mathf.Clamp01(age / 0.2f);
        float s = appear < 1f ? Mathf.Lerp(0f, 1.2f, appear) : 1f;
        visual.transform.localScale = new Vector3(s, s, 1f);
        visual.transform.localPosition = new Vector3(0f, bobHeight * Mathf.Sin((age + phase) * 4f), 0f);

        // Son saniyeler: hızlanan yanıp sönme
        float remaining = lifetime - age;
        if (remaining < warnTime)
        {
            float speed = Mathf.Lerp(4f, 14f, 1f - remaining / warnTime);
            visual.enabled = Mathf.Sin(age * speed * Mathf.PI * 2f) > -0.3f;
        }
    }

    /// <summary>Mıknatıs: altını hedefe doğru çeker (pet).</summary>
    public void PullTowards(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        var sm = GameManager.Instance != null ? GameManager.Instance.scoreManager : null;
        if (sm != null) sm.AddCoins(value, transform.position);
        Despawn();
    }

    void Despawn()
    {
        if (ProjectilePool.Instance != null) ProjectilePool.Instance.Return(gameObject);
        else Destroy(gameObject);
    }
}
