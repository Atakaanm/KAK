using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz 15 K4: gelişim ekranında tek iz satırı: ikon (grup rengi), ad, etki, seviye kareleri, fiyatlı YÜKSELT düğmesi.
/// Kilitliyse gri + "?" + "Gelişim N'de açılır". Mini animasyonlar: yükseltince ikon zıplar, kıvılcım saçılır, "+1" yükselir,
/// yeni kare dolar; altın yetmezse düğme sallanır; yeni açılınca "YENİ!" parlar. Zaman ölçeğinden bağımsız.
/// </summary>
public class UpgradeRow : MonoBehaviour
{
    public Image background, icon, lockIcon, coin;
    public TMP_Text nameText, descText, priceText, plusText, newText;
    public Image[] pips;
    public GameObject scrollBox;      // K5: üst seviyeler parşömen de ister
    public TMP_Text scrollCostText;
    public Button button;
    public Image buttonBackground;
    public UiBurst burst;
    public Sprite goldSprite, stoneSprite;

    [System.NonSerialized] public UpgradeTrackData track;
    [System.NonSerialized] public ProgressPanel panel;

    float punch, shake, plus, fresh;
    int popPip = -1;
    Vector2 buttonHome;
    bool homeSet;

    public void Click()
    {
        if (panel != null) panel.OnRowClicked(this);
    }

    public void Bind()
    {
        if (track == null) return;
        bool unlocked = Progression.Unlocked(track);
        int lvl = Progression.Level(track);
        int cost = Progression.Cost(track);
        bool max = cost < 0;
        Color g = ProgressPanel.GroupColor(track.group);

        if (icon != null)
        {
            icon.sprite = track.icon;
            icon.enabled = unlocked && track.icon != null;
            icon.color = g;
        }
        if (lockIcon != null) lockIcon.enabled = !unlocked;
        if (nameText != null)
        {
            nameText.SetText(unlocked ? Loc.T(track.nameKey) : "???");
            nameText.color = unlocked ? KakPalette.Krem : KakPalette.ArduvazAcik;
        }
        if (descText != null)
        {
            if (unlocked) descText.SetText(Loc.T(track.descKey));
            else descText.SetText(Loc.T("up_unlock_at"), track.unlockAtTotal);
            descText.color = unlocked ? KakPalette.Sis : KakPalette.ArduvazAcik;
        }
        if (pips != null)
            for (int i = 0; i < pips.Length; i++)
            {
                var p = pips[i];
                if (p == null) continue;
                p.gameObject.SetActive(unlocked && i < track.maxLevel);
                p.color = i < lvl ? g : KakPalette.WithAlpha(KakPalette.Gece, 0.9f);
            }
        if (background != null) background.color = unlocked ? Color.white : new Color(0.62f, 0.64f, 0.72f, 0.75f);

        int scroll = max ? 0 : Progression.ScrollCost(track);
        if (scrollBox != null) scrollBox.SetActive(unlocked && scroll > 0);
        if (scrollCostText != null && scroll > 0)
        {
            scrollCostText.SetText("{0}", scroll);
            scrollCostText.color = SaveSystem.Data.scrolls >= scroll ? KakPalette.Krem : KakPalette.Tehlike;
        }

        if (button != null)
        {
            button.gameObject.SetActive(unlocked);
            bool afford = !max && Progression.CanUpgrade(track);
            if (buttonBackground != null) buttonBackground.sprite = afford ? goldSprite : stoneSprite;
            if (priceText != null)
            {
                if (max) priceText.SetText(Loc.T("max"));
                else priceText.SetText("{0}", cost);
                priceText.color = afford ? KakPalette.Murekkep : (max ? KakPalette.AltinAcik : KakPalette.Sis);
            }
            if (coin != null) coin.enabled = !max;
        }
    }

    public void PlayUpgrade()
    {
        punch = 1f;
        plus = 1f;
        popPip = Progression.Level(track) - 1;
        if (burst != null) burst.Burst(ProgressPanel.GroupColor(track.group));
    }

    public void PlayDenied() => shake = 1f;
    public void PlayNew() => fresh = 2.2f;

    void OnDisable()
    {
        punch = shake = plus = fresh = 0f;
        Apply();
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        if (punch <= 0f && shake <= 0f && plus <= 0f && fresh <= 0f) return;
        punch = Mathf.Max(0f, punch - dt / 0.32f);
        shake = Mathf.Max(0f, shake - dt / 0.35f);
        plus = Mathf.Max(0f, plus - dt / 0.8f);
        fresh = Mathf.Max(0f, fresh - dt);
        Apply();
    }

    void Apply()
    {
        // İkon: hızlı büyüyüp yaylanarak oturur
        if (icon != null)
        {
            float k = 1f - punch;
            float s = punch > 0f ? 1f + 0.45f * Mathf.Sin(k * Mathf.PI) * (1f - k * 0.5f) : 1f;
            icon.rectTransform.localScale = new Vector3(s, s, 1f);
        }
        // Yeni dolan kare
        if (pips != null && popPip >= 0 && popPip < pips.Length && pips[popPip] != null)
        {
            float s = punch > 0f ? 1f + 0.6f * punch : 1f;
            pips[popPip].rectTransform.localScale = new Vector3(s, s, 1f);
        }
        // Düğme sallanması
        if (button != null)
        {
            var rt = (RectTransform)button.transform;
            if (!homeSet) { buttonHome = rt.anchoredPosition; homeSet = true; }
            rt.anchoredPosition = buttonHome + new Vector2(Mathf.Sin(shake * 40f) * 14f * shake, 0f);
        }
        // "+1" yükselir ve söner
        if (plusText != null)
        {
            plusText.gameObject.SetActive(plus > 0f);
            if (plus > 0f)
            {
                plusText.rectTransform.anchoredPosition = new Vector2(0f, 40f + (1f - plus) * 70f);
                var c = KakPalette.AltinAcik;
                c.a = Mathf.Clamp01(plus * 1.6f);
                plusText.color = c;
            }
        }
        // "YENİ!" parlar
        if (newText != null)
        {
            newText.gameObject.SetActive(fresh > 0f);
            if (fresh > 0f)
            {
                float s = 1f + 0.12f * Mathf.Sin(Time.unscaledTime * 10f);
                newText.rectTransform.localScale = new Vector3(s, s, 1f);
            }
        }
    }
}
