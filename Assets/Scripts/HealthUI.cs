using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("Şablon (Opsiyonel)")]
    public Image[] hearts; // Inspector'dan dizilen kalpler

    private List<Image> activeHearts = new List<Image>();
    private readonly List<Image> clones = new List<Image>();

    // Her kalp için bağımsız animasyon coroutine'i
    private Dictionary<int, Coroutine> heartAnimCoroutines = new Dictionary<int, Coroutine>();

    [Header("Animasyon Ayarlari")]
    public float heartLoseDuration = 0.35f;
    public float heartGainDuration = 0.25f;

    /// <summary>
    /// Maksimum can sayısına göre kalp ikonlarını hazırlar.
    /// PlayerHealth'in Start'ında çağrılır.
    /// </summary>
    public void InitHearts(int maxHealth)
    {
        activeHearts.Clear();

        // Panelin tüm kalpleri: Inspector dizisi + "Heart" adlı çocuklar (önceki klonlar dahil), soldan sağa.
        // Not: eskiden dizi yalnızca Heart1'i içeriyordu; Heart2/3 yönetilmiyor, klonlar üstlerine biniyordu
        // (can kaybında alttaki kalpler kaybolmuyordu).
        if (hearts != null)
            foreach (var h in hearts)
                if (h != null && !activeHearts.Contains(h)) activeHearts.Add(h);
        foreach (Transform child in transform)
        {
            if (!child.name.Contains("Heart")) continue;
            var img = child.GetComponent<Image>();
            if (img != null && !activeHearts.Contains(img)) activeHearts.Add(img);
        }
        activeHearts.Sort((a, b) => a.rectTransform.anchoredPosition.x.CompareTo(b.rectTransform.anchoredPosition.x));

        if (activeHearts.Count == 0) return;

        // Eksik kalpleri klonla
        if (activeHearts.Count < maxHealth)
        {
            Image lastHeart = activeHearts[activeHearts.Count - 1];

            Vector3 offset = new Vector3(100f, 0, 0);
            if (activeHearts.Count >= 2)
            {
                offset = activeHearts[1].rectTransform.anchoredPosition3D - activeHearts[0].rectTransform.anchoredPosition3D;
            }

            int neededAmount = maxHealth - activeHearts.Count;
            for (int i = 0; i < neededAmount; i++)
            {
                Image newHeart = Instantiate(lastHeart, lastHeart.transform.parent);
                newHeart.rectTransform.anchoredPosition3D = lastHeart.rectTransform.anchoredPosition3D + offset * (i + 1);
                newHeart.gameObject.SetActive(true);
                activeHearts.Add(newHeart);
                clones.Add(newHeart);
            }
        }

        // Karakterin canından fazla kalp gizlenir (ör. 2 canlı karakter: 3. kalp "kaybedilmiş" görünmesin)
        for (int i = activeHearts.Count - 1; i >= maxHealth; i--)
        {
            if (activeHearts[i] != null) activeHearts[i].gameObject.SetActive(false);
            activeHearts.RemoveAt(i);
        }

        // Tüm kalplerin görünür ve tam ölçekli olduğundan emin ol
        for (int i = 0; i < activeHearts.Count; i++)
        {
            if (activeHearts[i] != null)
            {
                activeHearts[i].gameObject.SetActive(true);
                activeHearts[i].transform.localScale = Vector3.one;
                activeHearts[i].color = new Color(activeHearts[i].color.r, activeHearts[i].color.g, activeHearts[i].color.b, 1f);
            }
        }
    }

    public void UpdateHearts(int currentHealth)
    {
        // Geriye dönük uyumluluk
        if (activeHearts.Count == 0 && hearts != null)
        {
            activeHearts.AddRange(hearts);
        }

        for (int i = 0; i < activeHearts.Count; i++)
        {
            if (activeHearts[i] == null) continue;

            bool shouldBeActive = i < currentHealth;
            bool isCurrentlyVisible = activeHearts[i].gameObject.activeSelf &&
                                       activeHearts[i].transform.localScale.x > 0.1f;

            if (!shouldBeActive && isCurrentlyVisible)
            {
                // Kalp kaybedildi — animasyonla küçült ve gizle
                AnimateHeartLoss(i);
            }
            else if (shouldBeActive && !isCurrentlyVisible)
            {
                // Kalp kazanıldı — animasyonla büyüt ve göster
                AnimateHeartGain(i);
            }
        }
    }

    void AnimateHeartLoss(int index)
    {
        if (heartAnimCoroutines.ContainsKey(index) && heartAnimCoroutines[index] != null)
        {
            StopCoroutine(heartAnimCoroutines[index]);
        }
        heartAnimCoroutines[index] = StartCoroutine(HeartLossRoutine(index));
    }

    void AnimateHeartGain(int index)
    {
        if (heartAnimCoroutines.ContainsKey(index) && heartAnimCoroutines[index] != null)
        {
            StopCoroutine(heartAnimCoroutines[index]);
        }
        heartAnimCoroutines[index] = StartCoroutine(HeartGainRoutine(index));
    }

    IEnumerator HeartLossRoutine(int index)
    {
        Image heart = activeHearts[index];
        if (heart == null) yield break;

        heart.gameObject.SetActive(true);

        float elapsed = 0f;
        Vector3 startScale = heart.transform.localScale;
        Color startColor = heart.color;

        // Önce büyüt (darbe etkisi), sonra küçült ve kaybet
        float punchDuration = heartLoseDuration * 0.25f;
        float shrinkDuration = heartLoseDuration * 0.75f;

        // Darbe etkisi — hafif büyüt
        while (elapsed < punchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / punchDuration;
            float scale = 1f + 0.3f * (1f - t); // 1.3'ten 1.0'a
            heart.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // Küçült ve kaybet
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / shrinkDuration;

            // Ease out cubic
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            float scale = Mathf.Lerp(1f, 0f, eased);
            heart.transform.localScale = new Vector3(scale, scale, 1f);

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, eased);
            heart.color = c;

            yield return null;
        }

        heart.transform.localScale = Vector3.zero;
        heart.gameObject.SetActive(false);
    }

    IEnumerator HeartGainRoutine(int index)
    {
        Image heart = activeHearts[index];
        if (heart == null) yield break;

        heart.gameObject.SetActive(true);
        heart.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        Color targetColor = heart.color;
        targetColor.a = 1f;

        while (elapsed < heartGainDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / heartGainDuration;

            // Bounce etkisi
            float eased;
            if (t < 0.6f)
            {
                eased = (t / 0.6f);
                eased = eased * eased * (3f - 2f * eased); // smoothstep
                eased *= 1.2f; // Overshoot
            }
            else
            {
                eased = Mathf.Lerp(1.2f, 1f, (t - 0.6f) / 0.4f);
            }

            heart.transform.localScale = new Vector3(eased, eased, 1f);
            Color c = targetColor;
            c.a = Mathf.Clamp01(t * 2f);
            heart.color = c;

            yield return null;
        }

        heart.transform.localScale = Vector3.one;
        heart.color = targetColor;
    }
}