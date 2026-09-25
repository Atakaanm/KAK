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

    [Header("Bölüm modu")]
    public TMP_Text titleText;
    public TMP_Text scoreLabel;
    public UnityEngine.UI.Image[] stars;
    public Sprite starOn, starOff;
    public UnityEngine.UI.Button nextButton;

    /// <summary>Bölüm başarıyla bitti: TEBRİKLER + yıldızlar (tek tek dolar) + süre + SONRAKİ.</summary>
    public void ShowLevelComplete(int starCount, bool newBestStars, bool hasNext, float seconds)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(RunLevel(starCount, newBestStars, hasNext, seconds));
    }

    System.Collections.IEnumerator RunLevel(int starCount, bool newBestStars, bool hasNext, float seconds)
    {
        if (titleText != null) { titleText.text = Loc.T("level_complete"); titleText.color = KakPalette.Altin; }
        if (scoreLabel != null) scoreLabel.text = Loc.T("time");
        int m = Mathf.FloorToInt(seconds / 60f), s = Mathf.FloorToInt(seconds % 60f);
        if (scoreText != null) scoreText.SetText("{0}:{1:00}", m, s);
        if (bestText != null) bestText.text = "";
        if (statsText != null) statsText.text = "";
        if (newBestBadge != null) newBestBadge.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(hasNext);
        if (buttons != null) { buttons.alpha = 0f; buttons.interactable = false; }
        if (stars != null) foreach (var st in stars) if (st != null) { st.gameObject.SetActive(true); st.sprite = starOff; st.transform.localScale = Vector3.one; }

        for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.25f;
            if (dim != null) dim.alpha = k;
            if (panel != null) panel.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, 1f - (1f - k) * (1f - k));
            yield return null;
        }
        if (dim != null) dim.alpha = 1f;
        if (panel != null) panel.localScale = Vector3.one;

        // Yıldızlar tek tek
        if (stars != null)
        {
            for (int i = 0; i < stars.Length && i < starCount; i++)
            {
                var st = stars[i];
                if (st == null) continue;
                st.sprite = starOn;
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.scoreSfx, 1f + i * 0.15f);
                for (float t = 0f; t < 0.22f; t += Time.unscaledDeltaTime)
                {
                    float k = t / 0.22f;
                    st.transform.localScale = Vector3.one * (k < 0.6f ? Mathf.Lerp(0.3f, 1.35f, k / 0.6f) : Mathf.Lerp(1.35f, 1f, (k - 0.6f) / 0.4f));
                    yield return null;
                }
                st.transform.localScale = Vector3.one;
            }
        }
        if (newBestStars && newBestBadge != null) newBestBadge.gameObject.SetActive(true);

        for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
        {
            if (buttons != null) buttons.alpha = t / 0.25f;
            yield return null;
        }
        if (buttons != null) { buttons.alpha = 1f; buttons.interactable = true; }
    }

    public void Show(int score, int best, bool newBest, float seconds, int nearMiss, float maxCombo)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Run(score, best, newBest, seconds, nearMiss, maxCombo));
    }

    IEnumerator Run(int score, int best, bool newBest, float seconds, int nearMiss, float maxCombo)
    {
        // Sonsuz/başarısız düzen: yıldızlar ve SONRAKİ gizli, başlık "OYUN BİTTİ"
        if (titleText != null) { titleText.text = Loc.T("game_over"); titleText.color = KakPalette.Tehlike; }
        if (scoreLabel != null) scoreLabel.text = Loc.T("score");
        if (stars != null) foreach (var st in stars) if (st != null) st.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        bool level = GameManager.Instance != null && GameManager.Instance.IsLevelMode;
        if (newBestBadge != null) newBestBadge.gameObject.SetActive(false);
        if (buttons != null) { buttons.alpha = 0f; buttons.interactable = false; }
        if (statsText != null)
        {
            int m = Mathf.FloorToInt(seconds / 60f), s = Mathf.FloorToInt(seconds % 60f);
            statsText.SetText(Loc.T("run_stats"), m, s, nearMiss, maxCombo);
            statsText.alpha = 0f;
        }
        if (bestText != null)
        {
            if (level) bestText.text = Loc.T("level_failed");
            else bestText.SetText(Loc.T("best_short"), best);
        }
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
