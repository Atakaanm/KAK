using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pet paneli (Faz 3c.5): katalogdaki petler; satın al / seç / çıkar (pet olmadan oyna).</summary>
public class PetPanel : MonoBehaviour
{
    [System.Serializable]
    public class Card
    {
        public RectTransform root;
        public Image background, portrait;
        public TMP_Text nameText, traitText, actionText;
        public Button actionButton;
        public Image actionBackground, actionCoin;
    }

    public Card[] cards;
    public TMP_Text walletText;
    public WalletHud menuWallet;
    public Sprite goldSprite, stoneSprite;

    PetCatalog catalog;

    void Awake()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int k = i;
            if (cards[i].actionButton != null) cards[i].actionButton.onClick.AddListener(() => OnCardAction(k));
        }
    }

    void OnEnable() { Loc.Changed += Refresh; Refresh(); }
    void OnDisable() => Loc.Changed -= Refresh;

    public void Refresh()
    {
        if (catalog == null) catalog = PetCatalog.Load();
        var list = catalog != null ? catalog.pets : null;
        for (int i = 0; i < cards.Length; i++)
        {
            var c = cards[i];
            bool has = list != null && i < list.Length && list[i] != null;
            c.root.gameObject.SetActive(has);
            if (!has) continue;
            var p = list[i];
            bool owned = PetCatalog.Owned(p), selected = owned && SaveSystem.Data.selectedPet == p.id, afford = PetCatalog.CanAfford(p);
            c.portrait.sprite = p.frames != null && p.frames.Length > 0 ? p.frames[0] : null;
            c.portrait.color = owned ? Color.white : new Color(0.35f, 0.33f, 0.45f, 1f);
            c.nameText.text = Loc.T(p.nameKey);
            c.nameText.color = selected ? KakPalette.Murekkep : KakPalette.Krem;
            c.traitText.text = Loc.T(p.traitKey);
            c.traitText.color = selected ? KakPalette.KahveKoyu : KakPalette.Sis;
            c.background.sprite = selected ? goldSprite : stoneSprite;
            if (selected) c.actionText.text = Loc.T("remove");
            else if (owned) c.actionText.text = Loc.T("select");
            else c.actionText.SetText("{0}", p.price);
            c.actionCoin.gameObject.SetActive(!owned);
            c.actionBackground.sprite = (!owned && afford) ? goldSprite : stoneSprite;
            c.actionBackground.color = (!owned && !afford) ? new Color(0.6f, 0.6f, 0.65f, 1f) : Color.white;
            c.actionText.color = (!owned && afford) ? KakPalette.Murekkep : (!owned ? KakPalette.ArduvazAcik : KakPalette.Krem);
        }
        if (walletText != null) walletText.SetText("{0}", SaveSystem.Data.coins);
        if (menuWallet != null) menuWallet.Refresh();
    }

    public void OnCardAction(int index)
    {
        if (catalog == null || catalog.pets == null || index >= catalog.pets.Length) return;
        var p = catalog.pets[index];
        var am = AudioManager.Instance;
        if (PetCatalog.Owned(p)) PetCatalog.Toggle(p);
        else if (PetCatalog.TryBuy(p) && am != null) am.PlaySfx(am.stageSfx);
        Refresh();
    }
}
