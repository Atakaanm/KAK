using System.Collections;
using TMPro;
using UnityEngine.UI;
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
    [Tooltip("Altın satırı: '+12' ve cüzdan toplamı (altın kilitliyse gizli)")]
    public RectTransform coinsRow;
    public TMP_Text coinsText;
    [Tooltip("Bu oyunla yeni açılan özellik afişi (panelin üstünde)")]
    public RectTransform unlockBanner;
    public TMP_Text unlockText;
    [Header("Cila (Faz 3c.7)")]
    public UiConfetti confetti;
    [Tooltip("Sıradaki satın alma hedefi: çubuk + '120/300 ÇEVİK'")]
    public RectTransform goalRow;
    public Image goalFill;
    public TMP_Text goalText;

    [Header("Görevler (Faz 3c.4)")]
    public RectTransform missionsBlock;
    public TMP_Text[] missionTexts;
    public TMP_Text[] missionProgress;
    public Image[] missionChecks;
    [Tooltip("Görevler görünürken panel yüksekliği (butonlar alta sabit, satırlar araya girer)")]
    public float heightWithMissions = 1530f;
    public float heightWithoutMissions = 1290f;
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

    /// <param name="coins">Bu oyunda kazanılan altın (-1: altın kilitli, satır gizli)</param>
    /// <param name="wallet">Cüzdandaki toplam altın (kazanılan dahil)</param>
    /// <param name="unlock">Yeni açılan özellik metni ("YENİ AÇILDI: GÖREVLER!"), yoksa null</param>
    public void Show(int score, int best, bool newBest, float seconds, int nearMiss, float maxCombo, int coins = -1, int wallet = 0, string unlock = null)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Run(score, best, newBest, seconds, nearMiss, maxCombo, coins, wallet, unlock));
    }

    IEnumerator Run(int score, int best, bool newBest, float seconds, int nearMiss, float maxCombo, int coins = -1, int wallet = 0, string unlock = null)
    {
        if (coinsRow != null) coinsRow.gameObject.SetActive(false);
        if (unlockBanner != null) unlockBanner.gameObject.SetActive(false);
        if (goalRow != null) goalRow.gameObject.SetActive(false);
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
            if (confetti != null) confetti.Burst();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.stageSfx, 1.1f);
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                float k = t / 0.3f;
                newBestBadge.localScale = Vector3.one * (k < 0.6f ? Mathf.Lerp(0f, 1.25f, k / 0.6f) : Mathf.Lerp(1.25f, 1f, (k - 0.6f) / 0.4f));
                yield return null;
            }
            newBestBadge.localScale = Vector3.one;
        }

        // Altın: satır belirir, kazanç sayılır, cüzdan toplamı gösterilir
        if (coins >= 0 && coinsRow != null && coinsText != null)
        {
            coinsRow.gameObject.SetActive(true);
            float cd = Mathf.Clamp(coins / 40f, 0.3f, 0.9f);
            for (float t = 0f; t < cd; t += Time.unscaledDeltaTime)
            {
                float k = 1f - Mathf.Pow(1f - t / cd, 2f);
                coinsText.SetText(Loc.T("coins_run"), Mathf.RoundToInt(coins * k), wallet - coins + Mathf.RoundToInt(coins * k));
                coinsRow.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(t * 30f) * (1f - k));
                yield return null;
            }
            coinsText.SetText(Loc.T("coins_run"), coins, wallet);
            coinsRow.localScale = Vector3.one;
        }

        // Sıradaki hedef: kazanılanla dolan çubuk (hedefe yaklaşma etkisi)
        var goal = coins >= 0 ? NextGoal.Find() : null;
        if (goal.HasValue && goalRow != null && goalFill != null && goalText != null)
        {
            var g = goal.Value;
            goalRow.gameObject.SetActive(true);
            float from = Mathf.Clamp01((wallet - Mathf.Max(0, coins)) / (float)g.price), to = Mathf.Clamp01(wallet / (float)g.price);
            bool ready = wallet >= g.price;
            goalText.text = ready ? string.Format(Loc.T("goal_ready"), g.name) : string.Format(Loc.T("goal_progress"), g.name, wallet, g.price);
            goalText.color = ready ? KakPalette.AltinAcik : KakPalette.Sis;
            for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime)
            {
                goalFill.fillAmount = Mathf.Lerp(from, to, 1f - (1f - t / 0.5f) * (1f - t / 0.5f));
                yield return null;
            }
            goalFill.fillAmount = to;
            goalFill.color = ready ? KakPalette.AltinAcik : KakPalette.Altin;
        }

        // Yeni açılan özellik: afiş taşarak belirir (adım adım açılmanın ödül anı)
        if (!string.IsNullOrEmpty(unlock) && unlockBanner != null && unlockText != null)
        {
            unlockText.text = unlock;
            unlockBanner.gameObject.SetActive(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.stageSfx);
            for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
            {
                float k = t / 0.35f;
                float s = k < 0.6f ? Mathf.Lerp(0f, 1.2f, k / 0.6f) : Mathf.Lerp(1.2f, 1f, (k - 0.6f) / 0.4f);
                unlockBanner.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            unlockBanner.localScale = Vector3.one;
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

    /// <summary>
    /// Görev satırlarını gösterir (Show'dan hemen sonra çağrılır). null: görevler kapalı, blok gizli ve panel kısa.
    /// Bu oyunla tamamlananlar altın renkte, onaylı ve ödüllü ("+40"), sırayla zıplar.
    /// </summary>
    public void ShowMissions(System.Collections.Generic.List<MissionState> missions, System.Collections.Generic.List<MissionState> justCompleted)
    {
        bool on = missions != null && missions.Count > 0 && missionsBlock != null;
        if (panel != null) panel.sizeDelta = new Vector2(panel.sizeDelta.x, on ? heightWithMissions : heightWithoutMissions);
        if (missionsBlock != null) missionsBlock.gameObject.SetActive(on);
        if (!on) return;
        for (int i = 0; i < missionTexts.Length; i++)
        {
            bool has = i < missions.Count && missions[i] != null;
            missionTexts[i].transform.parent.gameObject.SetActive(has);
            if (!has) continue;
            var m = missions[i];
            bool done = m.done;
            bool fresh = justCompleted != null && justCompleted.Contains(m);
            missionTexts[i].text = MissionSystem.Describe(m);
            missionTexts[i].color = done ? KakPalette.AltinAcik : KakPalette.Krem;
            if (done) missionProgress[i].SetText("+{0}", m.reward);
            else missionProgress[i].SetText("{0}/{1}", m.progress, m.target);
            missionProgress[i].color = done ? KakPalette.Altin : KakPalette.Sis;
            missionChecks[i].color = done ? KakPalette.AcikYesil : KakPalette.WithAlpha(KakPalette.ArduvazAcik, 0.35f);
            if (fresh) StartCoroutine(PopRow(missionTexts[i].transform.parent as RectTransform, 0.9f + i * 0.25f));
        }
    }

    IEnumerator PopRow(RectTransform row, float delay)
    {
        for (float t = 0f; t < delay; t += Time.unscaledDeltaTime) yield return null;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.coinSfx, 1.2f);
        for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.3f;
            float s = 1f + 0.12f * Mathf.Sin(k * Mathf.PI);
            row.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        row.localScale = Vector3.one;
    }
}
