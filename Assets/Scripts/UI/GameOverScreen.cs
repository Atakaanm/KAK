using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Oyun sonu ekranı: panel gelir → skor sayarak artar → rekorsa "YENİ REKOR!" rozeti → istatistikler.
/// Gerçek zamanla çalışır (timeScale = 0). GameManager.DeathSequence çağırır.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    public RectTransform panel;
    public TMP_Text scoreText;
    public TMP_Text bestText;
    public TMP_Text statsText;
    public RectTransform newBestBadge;
    public CanvasGroup buttons;
    public CanvasGroup dim;

    public void Show(int score, int best, bool newBest, float seconds, int nearMiss, float maxCombo)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Run(score, best, newBest, seconds, nearMiss, maxCombo));
    }

    IEnumerator Run(int score, int best, bool newBest, float seconds, int nearMiss, float maxCombo)
    {
        if (newBestBadge != null) newBestBadge.gameObject.SetActive(false);
        if (buttons != null) { buttons.alpha = 0f; buttons.interactable = false; }
        if (statsText != null)
        {
            int m = Mathf.FloorToInt(seconds / 60f), s = Mathf.FloorToInt(seconds % 60f);
            statsText.SetText("SÜRE {0}:{1:00}   YAKIN {2}   COMBO x{3:1}", m, s, nearMiss, maxCombo);
            statsText.alpha = 0f;
        }
        if (bestText != null) bestText.SetText("EN İYİ {0}", best);
        if (scoreText != null) scoreText.SetText("0");

        // Panel girişi
        for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.25f;
            if (dim != null) dim.alpha = k;
            if (panel != null) panel.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, 1f - (1f - k) * (1f - k));
            yield return null;
        }
        if (dim != null) dim.alpha = 1f;
        if (panel != null) panel.localScale = Vector3.one;

        // Skor sayma
        float dur = Mathf.Clamp(score / 800f, 0.4f, 1.1f);
        for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / dur, 3f);
            if (scoreText != null) scoreText.SetText("{0}", Mathf.RoundToInt(score * k));
            yield return null;
        }
        if (scoreText != null) scoreText.SetText("{0}", score);

        if (newBest && newBestBadge != null)
        {
            newBestBadge.gameObject.SetActive(true);
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                float k = t / 0.3f;
                newBestBadge.localScale = Vector3.one * (k < 0.6f ? Mathf.Lerp(0f, 1.25f, k / 0.6f) : Mathf.Lerp(1.25f, 1f, (k - 0.6f) / 0.4f));
                yield return null;
            }
            newBestBadge.localScale = Vector3.one;
        }

        for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.25f;
            if (statsText != null) statsText.alpha = k;
            if (buttons != null) buttons.alpha = k;
            yield return null;
        }
        if (statsText != null) statsText.alpha = 1f;
        if (buttons != null) { buttons.alpha = 1f; buttons.interactable = true; }
    }
}
