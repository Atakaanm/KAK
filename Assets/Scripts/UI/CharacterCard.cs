using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Karakter panelinde bir kart: portre, ad, özellik, can/hız/dash göstergeleri, aksiyon butonu.</summary>
public class CharacterCard : MonoBehaviour
{
    public Image background;
    public Image portrait;
    public TMP_Text nameText;
    public TMP_Text traitText;
    public Image[] hearts;
    public Image speedFill;
    [Tooltip("Seçili (altın) kartta koyulaşan yazılar: özellik, HIZ/DASH etiketleri")]
    public TMP_Text[] softTexts;
    public Image dashFill;
    public Button actionButton;
    public Image actionBackground;
    public TMP_Text actionText;
    public Image actionCoin;
    public Sprite goldSprite, stoneSprite;

    [HideInInspector] public int index;
    [HideInInspector] public CharacterPanel panel;

    float pop;

    void Awake()
    {
        if (actionButton != null) actionButton.onClick.AddListener(() => { if (panel != null) panel.OnCardAction(index); });
    }

    public void Bind(PlayerData p)
    {
        bool owned = CharacterCatalog.Owned(p);
        bool selected = owned && SaveSystem.Data.selectedCharacter == p.id;
        bool afford = CharacterCatalog.CanAfford(p);

        if (portrait != null)
        {
            portrait.sprite = p.Portrait;
            // Sahip olunmayan karakter: siluet (merak), ama rengi seçilebilir kadar görünür
            portrait.color = owned ? Color.white : new Color(0.35f, 0.33f, 0.45f, 1f);
        }
        if (nameText != null) nameText.text = Loc.T(p.nameKey);
        if (traitText != null) traitText.text = string.IsNullOrEmpty(p.traitKey) ? "" : Loc.T(p.traitKey);
        if (hearts != null)
            for (int i = 0; i < hearts.Length; i++) if (hearts[i] != null) hearts[i].gameObject.SetActive(i < p.maxHealth);
        // Hız ve dash göstergeleri: karakterler arasında göreli (0.3-1)
        if (speedFill != null) speedFill.fillAmount = Mathf.InverseLerp(4f, 6.25f, p.moveSpeed) * 0.7f + 0.3f;
        float cd = p.dashCooldown > 0f ? p.dashCooldown : 2.6f;
        if (dashFill != null) dashFill.fillAmount = Mathf.InverseLerp(3.6f, 1.6f, cd) * 0.7f + 0.3f;
        if (background != null) background.sprite = selected ? goldSprite : stoneSprite;
        // Altın zeminde açık renk yazı okunmaz: seçili kartta koyu
        Color soft = selected ? KakPalette.KahveKoyu : KakPalette.Sis;
        if (softTexts != null) foreach (var t in softTexts) if (t != null) t.color = soft;
        if (nameText != null) nameText.color = selected ? KakPalette.Murekkep : KakPalette.Krem;

        if (actionText != null)
        {
            if (selected) actionText.text = Loc.T("selected");
            else if (owned) actionText.text = Loc.T("select");
            else actionText.SetText("{0}", p.unlockPrice);
            actionText.color = selected ? KakPalette.Murekkep : (owned || afford) ? KakPalette.Krem : KakPalette.ArduvazAcik;
        }
        if (actionCoin != null) actionCoin.gameObject.SetActive(!owned);
        if (actionBackground != null)
        {
            actionBackground.sprite = (!owned && afford) ? goldSprite : stoneSprite;
            actionBackground.color = selected ? new Color(1f, 1f, 1f, 0f) : (!owned && !afford) ? new Color(0.6f, 0.6f, 0.65f, 1f) : Color.white;
            if (!owned && afford && actionText != null) actionText.color = KakPalette.Murekkep;
        }
        if (actionButton != null) actionButton.interactable = !selected;
    }

    public void Pop() => pop = 1f;

    void Update()
    {
        if (pop <= 0f) return;
        pop = Mathf.MoveTowards(pop, 0f, Time.unscaledDeltaTime * 3f);
        float s = 1f + 0.12f * Mathf.Sin(pop * Mathf.PI);
        transform.localScale = new Vector3(s, s, 1f);
    }
}
