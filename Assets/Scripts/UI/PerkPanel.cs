using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz 15 K8: kaçış kartı seçimi. Cesaret çubuğu dolunca oyun durur, 3 piksel kart sırayla zıplayarak gelir; dokunulan kart seçilir.
/// Yanlışlıkla dokunmayı önlemek için kartlar gelene kadar seçim kapalı. Kart: ikon, ad, tek satır, seviye kareleri.
/// Kök (bu nesne) her zaman açık: RunPerks.ShowPanel'e bağlanır; içerik "root" açılıp kapanır. Kurulum: KakPerkUi.
/// </summary>
public class PerkPanel : MonoBehaviour
{
    [Serializable]
    public class CardUi
    {
        public RectTransform body;
        public Image background, icon;
        public TMP_Text nameText, descText;
        public Image[] pips;
    }

    public GameObject root;
    public CardUi[] cards;
    public UiBurst burst;

    RunPerks perks;
    float t;
    const float Stagger = 0.12f, PopTime = 0.28f;

    public bool Open => root != null && root.activeSelf;

    void Awake()
    {
        RunPerks.ShowPanel = Show;
        if (root != null) root.SetActive(false);
    }

    void OnDestroy()
    {
        if (RunPerks.ShowPanel == (Action<RunPerks>)Show) RunPerks.ShowPanel = null;
    }

    void Show(RunPerks p)
    {
        perks = p;
        t = 0f;
        if (root == null) return;
        root.SetActive(true);
        transform.SetAsLastSibling();
        for (int i = 0; i < cards.Length; i++)
        {
            var c = cards[i];
            bool has = i < p.OfferCount && p.Offer[i] != null;
            if (c.body != null) { c.body.gameObject.SetActive(has); c.body.localScale = Vector3.zero; }
            if (!has) continue;
            var d = p.Offer[i];
            int lvl = p.LevelOf(d);
            if (c.icon != null) { c.icon.sprite = d.icon; c.icon.color = d.color; c.icon.enabled = d.icon != null; }
            if (c.nameText != null) c.nameText.SetText(Loc.T(d.nameKey));
            if (c.descText != null) c.descText.SetText(Loc.T(d.descKey));
            if (c.pips != null)
                for (int k = 0; k < c.pips.Length; k++)
                {
                    if (c.pips[k] == null) continue;
                    c.pips[k].gameObject.SetActive(k < d.maxLevel);
                    c.pips[k].color = k < lvl ? d.color : (k == lvl ? KakPalette.AltinAcik : KakPalette.WithAlpha(KakPalette.Gece, 0.9f));
                }
        }
        var am = AudioManager.Instance;
        if (am != null) am.PlaySfx(am.stageSfx, 1.2f);
    }

    public void Pick0() => Pick(0);
    public void Pick1() => Pick(1);
    public void Pick2() => Pick(2);

    public void Pick(int i)
    {
        if (perks == null || !Open) return;
        if (t < Stagger * 2f + PopTime) return; // kartlar gelmeden seçilmez
        if (burst != null && i < cards.Length && cards[i].body != null)
        {
            burst.transform.position = cards[i].body.position;
            burst.Burst(perks.Offer[i] != null ? perks.Offer[i].color : KakPalette.AltinAcik);
        }
        perks.Choose(i);
        root.SetActive(false);
        var am = AudioManager.Instance;
        if (am != null) am.PlaySfx(am.scoreSfx, 1.1f);
    }

    void Update()
    {
        if (!Open) return;
        if (!KakTime.Paused) KakTime.SetPaused(true); // açıkken oyun durur (duraklat/devam bunu bozmasın)
        t += Time.unscaledDeltaTime;
        for (int i = 0; i < cards.Length; i++)
        {
            var b = cards[i].body;
            if (b == null || !b.gameObject.activeSelf) continue;
            float k = Mathf.Clamp01((t - Stagger * i) / PopTime);
            float s = k < 0.7f ? Mathf.Lerp(0f, 1.12f, k / 0.7f) : Mathf.Lerp(1.12f, 1f, (k - 0.7f) / 0.3f);
            float bob = k >= 1f ? Mathf.Sin(Time.unscaledTime * 2.6f + i) * 6f : 0f;
            b.localScale = Vector3.one * s;
            b.anchoredPosition = new Vector2(b.anchoredPosition.x, bob);
        }
    }
}
