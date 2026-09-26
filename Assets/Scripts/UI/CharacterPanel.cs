using TMPro;
using UnityEngine;

/// <summary>
/// Karakter paneli (Faz 3c.3): katalogdaki karakterleri kartlara bağlar; SEÇ / satın al.
/// Yetmeyen altında kart sallanır. Satın alınca kart zıplar, ses çalar, cüzdan güncellenir.
/// </summary>
public class CharacterPanel : MonoBehaviour
{
    public CharacterCard[] cards;
    public TMP_Text walletText;
    public WalletHud menuWallet;

    CharacterCatalog catalog;

    void OnEnable()
    {
        Loc.Changed += Refresh;
        Refresh();
    }

    void OnDisable() => Loc.Changed -= Refresh;

    public void Refresh()
    {
        if (catalog == null) catalog = CharacterCatalog.Load();
        var list = catalog != null ? catalog.characters : null;
        for (int i = 0; i < cards.Length; i++)
        {
            var c = cards[i];
            if (c == null) continue;
            bool has = list != null && i < list.Length && list[i] != null;
            c.gameObject.SetActive(has);
            if (!has) continue;
            c.index = i;
            c.panel = this;
            c.Bind(list[i]);
        }
        if (walletText != null) walletText.SetText("{0}", SaveSystem.Data.coins);
        if (menuWallet != null) menuWallet.Refresh();
    }

    public void OnCardAction(int index)
    {
        if (catalog == null || catalog.characters == null || index >= catalog.characters.Length) return;
        var p = catalog.characters[index];
        var am = AudioManager.Instance;
        if (CharacterCatalog.Owned(p))
        {
            CharacterCatalog.Select(p); // tıklama sesi butondan
        }
        else if (CharacterCatalog.TryBuy(p))
        {
            if (am != null) am.PlaySfx(am.stageSfx);
            cards[index].Pop();
        }
        else
        {
            cards[index].Pop(); // altın yetmiyor: kart sallanır gibi zıplar, fiyat soluk
        }
        Refresh();
    }
}
