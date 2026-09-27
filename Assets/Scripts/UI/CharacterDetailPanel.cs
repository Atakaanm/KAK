using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Karakter detay / gelişim ekranı (Faz 11 G3): karakter kartına dokununca açılır. Büyük portre, özellik, üç kalıcı
/// yükseltme (Can, Hız, Güç süresi; altınla seviye seviye), kostüm yuvası (yakında), seç / satın al.
/// Kurulum: KakUiSetup.BuildCharacterDetailPanel.
/// </summary>
public class CharacterDetailPanel : MonoBehaviour
{
    [System.Serializable]
    public class StatRow
    {
        public Image[] pips;
        public Button button;
        public Image buttonBackground;
        public TMP_Text priceText;
        public Image coin;
        public Color filledColor = Color.white;
    }

    public TMP_Text nameText, traitText, walletText;
    public Image portrait;
    public StatRow healthRow, speedRow, powerRow;
    public Button actionButton;
    public Image actionBackground;
    public TMP_Text actionText;
    public Image actionCoin;
    public Sprite goldSprite, stoneSprite;
    public CharacterPanel panel;
    public RectTransform body; // "pop" animasyonu

    CharacterCatalog catalog;
    int index = -1;
    float pop;

    public PlayerData Current => catalog != null && catalog.characters != null && index >= 0 && index < catalog.characters.Length
        ? catalog.characters[index] : null;

    void OnEnable() { Loc.Changed += Refresh; CharacterProgress.Changed += Refresh; }
    void OnDisable() { Loc.Changed -= Refresh; CharacterProgress.Changed -= Refresh; }

    public void Open(int i)
    {
        if (catalog == null) catalog = CharacterCatalog.Load();
        index = i;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (panel != null) panel.Refresh();
    }

    public void OnUpgradeHealth() => Upgrade(CharStat.Health);
    public void OnUpgradeSpeed() => Upgrade(CharStat.Speed);
    public void OnUpgradePower() => Upgrade(CharStat.Power);

    void Upgrade(CharStat s)
    {
        var p = Current;
        if (p == null) return;
        var am = AudioManager.Instance;
        if (CharacterProgress.TryUpgrade(p, s))
        {
            if (am != null) am.PlaySfx(am.stageSfx);
            pop = 1f;
        }
        else if (am != null) am.PlayButtonClick();
        Refresh();
        if (panel != null) panel.Refresh();
    }

    public void OnAction()
    {
        if (panel != null) panel.OnCardAction(index);
        Refresh();
    }

    public void Refresh()
    {
        var p = Current;
        if (p == null) return;
        bool owned = CharacterCatalog.Owned(p);
        bool selected = SaveSystem.Data.selectedCharacter == p.id;

        if (nameText != null) nameText.text = Loc.T(p.nameKey);
        if (traitText != null) traitText.text = string.IsNullOrEmpty(p.traitKey) ? "" : Loc.T(p.traitKey);
        if (portrait != null) { portrait.sprite = p.Portrait; portrait.color = owned ? Color.white : new Color(0.35f, 0.35f, 0.45f, 1f); }
        if (walletText != null) walletText.SetText("{0}", SaveSystem.Data.coins);

        // Can: dolu kalpler = şu anki başlangıç canı, soluk = yükseltmeyle açılabilecek
        int hearts = CharacterProgress.Hearts(p);
        BindPips(healthRow, hearts, p.maxHealth);
        BindPips(speedRow, CharacterProgress.Level(p, CharStat.Speed), CharacterCatalogMax(CharStat.Speed));
        BindPips(powerRow, CharacterProgress.Level(p, CharStat.Power), CharacterCatalogMax(CharStat.Power));
        BindButton(healthRow, p, CharStat.Health, owned);
        BindButton(speedRow, p, CharStat.Speed, owned);
        BindButton(powerRow, p, CharStat.Power, owned);

        if (actionText != null)
        {
            if (selected) actionText.text = Loc.T("selected");
            else if (owned) actionText.text = Loc.T("select");
            else actionText.SetText("{0}", p.unlockPrice);
        }
        if (actionCoin != null) actionCoin.gameObject.SetActive(!owned);
        if (actionBackground != null) actionBackground.sprite = selected ? stoneSprite : goldSprite;
        if (actionButton != null) actionButton.interactable = !selected;
    }

    static int CharacterCatalogMax(CharStat s) => s == CharStat.Speed ? CharacterProgress.SpeedLevels : CharacterProgress.PowerLevels;

    static void BindPips(StatRow row, int filled, int total)
    {
        if (row == null || row.pips == null) return;
        for (int i = 0; i < row.pips.Length; i++)
        {
            var pip = row.pips[i];
            if (pip == null) continue;
            pip.gameObject.SetActive(i < total);
            pip.color = i < filled ? row.filledColor : new Color(0.22f, 0.24f, 0.36f, 1f);
        }
    }

    void BindButton(StatRow row, PlayerData p, CharStat s, bool owned)
    {
        if (row == null) return;
        int cost = CharacterProgress.Cost(p, s);
        bool max = cost < 0;
        bool can = CharacterProgress.CanUpgrade(p, s);
        if (row.priceText != null)
        {
            if (max) row.priceText.text = Loc.T("stat_max");
            else row.priceText.SetText("{0}", cost);
            row.priceText.color = max ? KakPalette.Sis : (can ? KakPalette.Murekkep : KakPalette.Sis);
        }
        if (row.coin != null) row.coin.gameObject.SetActive(!max);
        if (row.buttonBackground != null) row.buttonBackground.sprite = can ? goldSprite : stoneSprite;
        if (row.button != null) row.button.interactable = !max && owned;
    }

    void Update()
    {
        if (pop <= 0f || body == null) return;
        pop = Mathf.MoveTowards(pop, 0f, Time.unscaledDeltaTime * 4f);
        body.localScale = Vector3.one * (1f + 0.04f * Mathf.Sin(pop * Mathf.PI));
    }
}
