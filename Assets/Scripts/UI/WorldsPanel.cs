using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// G7 (Faz 11): DÜNYALAR paneli — Sonsuz modun arenaları (Taş Zindanı, Buz Gölü...). Kart: önizleme, ad, açıklama, dünyanın
/// rekoru, SEÇ / SEÇİLİ / kilit ("N oyun sonra"). Seçilen dünyayı OYNA başlatır. Kurulum: KakUiSetup.BuildWorldsPanel.
/// </summary>
public class WorldsPanel : MonoBehaviour
{
    [System.Serializable]
    public class Card
    {
        public RectTransform root;
        public Image background, preview;
        public TMP_Text nameText, descText, bestText, actionText;
        public Button actionButton;
        public Image actionBackground;
        // Faz 13 K2: karanlık dünyada meşale yükseltmesi (TorchProgress)
        public Button torchButton;
        public Image torchBackground;
        public TMP_Text torchText;
    }

    public Card[] cards;
    public Sprite goldSprite, stoneSprite;
    public MainMenuController menu;

    EndlessWorlds catalog;

    void Awake()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int k = i;
            if (cards[i].actionButton != null) cards[i].actionButton.onClick.AddListener(() => OnSelect(k));
            if (cards[i].torchButton != null) cards[i].torchButton.onClick.AddListener(OnTorch);
        }
    }

    void OnEnable() { Loc.Changed += Refresh; Refresh(); }
    void OnDisable() => Loc.Changed -= Refresh;

    public void Refresh()
    {
        if (catalog == null) catalog = EndlessWorlds.Load();
        var list = catalog != null ? catalog.worlds : null;
        var sel = EndlessWorlds.Selected();
        for (int i = 0; i < cards.Length; i++)
        {
            var c = cards[i];
            bool has = list != null && i < list.Length && list[i] != null;
            c.root.gameObject.SetActive(has);
            if (!has) continue;
            var w = list[i];
            bool open = EndlessWorlds.Unlocked(w), selected = sel == w;
            c.preview.sprite = w.preview;
            c.preview.color = open ? Color.white : new Color(0.3f, 0.3f, 0.4f, 1f);
            c.nameText.text = Loc.T(w.nameKey);
            c.nameText.color = selected ? KakPalette.Murekkep : KakPalette.Altin;
            c.descText.text = Loc.T(w.descKey);
            c.descText.color = selected ? KakPalette.KahveKoyu : KakPalette.Sis;
            c.bestText.text = open ? string.Format(Loc.T("best"), EndlessWorlds.Best(w.id)) : FeatureGateHint(w);
            c.bestText.color = selected ? KakPalette.KahveKoyu : KakPalette.AltinAcik;
            c.background.sprite = selected ? goldSprite : stoneSprite;
            c.actionText.text = Loc.T(selected ? "selected" : open ? "select" : "locked_short");
            c.actionButton.interactable = open && !selected;
            c.actionBackground.sprite = open && !selected ? goldSprite : stoneSprite;
            c.actionText.color = open && !selected ? KakPalette.Murekkep : KakPalette.Krem;
            BindTorch(c, w, open);
        }
    }

    /// <summary>Faz 13 K2: karanlık dünyanın kartında meşale düğmesi: "MEŞALE 1/4" + sonraki fiyat; altın yetince altın renkli.</summary>
    void BindTorch(Card c, EndlessWorlds.World w, bool open)
    {
        if (c.torchButton == null) return;
        bool dark = w.level != null && w.level.theme != null && w.level.theme.darkness > 0.01f;
        c.torchButton.gameObject.SetActive(dark && open);
        if (!dark || !open) return;
        int lv = TorchProgress.Level + 1, max = TorchProgress.MaxLevel + 1, cost = TorchProgress.NextCost;
        string head = string.Format(Loc.T("torch_level"), lv + "/" + max);
        c.torchText.text = cost < 0 ? Loc.T("torch_max") : "<size=22>" + head + "</size>\n" + cost + " " + Loc.T("coins_word");
        bool can = TorchProgress.CanUpgrade;
        c.torchButton.interactable = cost > 0;
        if (c.torchBackground != null) c.torchBackground.sprite = can ? goldSprite : stoneSprite;
        c.torchText.color = can ? KakPalette.Murekkep : KakPalette.Krem;
    }

    public void OnTorch()
    {
        var am = AudioManager.Instance;
        if (TorchProgress.TryUpgrade()) { if (am != null) am.PlaySfx(am.stageSfx); }
        else if (am != null) am.PlayButtonClick();
        Refresh();
        foreach (var wh in FindObjectsByType<WalletHud>(FindObjectsSortMode.None)) wh.Refresh();
    }

    static string FeatureGateHint(EndlessWorlds.World w)
    {
        int n = w.unlockGames - SaveSystem.Data.gamesPlayed;
        return string.Format(Loc.T(n == 1 ? "locked_game_1" : "locked_games"), n);
    }

    public void OnSelect(int index)
    {
        if (catalog == null || catalog.worlds == null || index >= catalog.worlds.Length) return;
        var am = AudioManager.Instance;
        if (EndlessWorlds.Select(catalog.worlds[index]) && am != null) am.PlaySfx(am.stageSfx);
        Refresh();
        if (menu != null) menu.RefreshWorld();
    }
}
