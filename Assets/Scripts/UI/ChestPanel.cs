using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz 15 K6: SANDIKLAR paneli (menü). Üç sandık kartı (ahşap, gümüş, altın): anahtar maliyeti, ödül aralığı;
/// açılabilen kart hafifçe zıplar. AÇ → ChestOpening animasyonu. Yetmezse kart sallanır. Kurulum: KakChestUi.
/// </summary>
public class ChestPanel : MonoBehaviour
{
    [System.Serializable]
    public class Card
    {
        public RectTransform body;
        public Image chest;
        public TMP_Text nameText, hintText, costText;
        public Image buttonBackground;
    }

    public Card[] cards;
    public TMP_Text keysText, pityText;
    public ChestOpening opening;
    public WalletHud menuWallet;
    public Sprite goldSprite, stoneSprite;

    float[] shake;
    Vector2[] home;

    void OnEnable()
    {
        ChestSystem.Changed += Refresh;
        Loc.Changed += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        ChestSystem.Changed -= Refresh;
        Loc.Changed -= Refresh;
    }

    public void Refresh()
    {
        if (cards == null) return;
        if (shake == null || shake.Length != cards.Length) shake = new float[cards.Length];
        if (home == null || home.Length != cards.Length)
        {
            home = new Vector2[cards.Length];
            for (int i = 0; i < cards.Length; i++) if (cards[i] != null && cards[i].chest != null) home[i] = cards[i].chest.rectTransform.anchoredPosition;
        }
        float scale = ChestSystem.GoldScale();
        for (int i = 0; i < cards.Length; i++)
        {
            var c = cards[i];
            var def = ChestSystem.Def(i);
            if (c == null || c.body == null) continue;
            c.body.gameObject.SetActive(def != null);
            if (def == null) continue;
            if (c.chest != null) c.chest.sprite = def.closed;
            if (c.nameText != null) c.nameText.SetText(Loc.T(def.nameKey));
            if (c.hintText != null)
                c.hintText.SetText(string.Format(Loc.T("chest_hint"), Mathf.RoundToInt(def.goldMin * scale), Mathf.RoundToInt(def.goldMax * scale), def.scrollMin, def.scrollMax));
            if (c.costText != null) c.costText.SetText("{0}", def.keyCost);
            bool can = ChestSystem.CanOpen(i);
            if (c.buttonBackground != null) c.buttonBackground.sprite = can ? goldSprite : stoneSprite;
            if (c.costText != null) c.costText.color = can ? KakPalette.Murekkep : KakPalette.Sis;
        }
        if (keysText != null) keysText.SetText("{0}", ChestSystem.Keys);
        if (pityText != null)
        {
            int left = ChestSystem.UntilPity;
            pityText.SetText(left <= 1 ? Loc.T("chest_pity_now") : string.Format(Loc.T("chest_pity"), left));
        }
        if (menuWallet != null) menuWallet.Refresh();
    }

    public void Open0() => Open(0);
    public void Open1() => Open(1);
    public void Open2() => Open(2);

    public void Open(int i)
    {
        var am = AudioManager.Instance;
        if (ChestSystem.TryOpen(i, out var r))
        {
            if (opening != null) opening.Play(ChestSystem.Def(r.defIndex), r);
        }
        else
        {
            if (shake != null && i < shake.Length) shake[i] = 1f;
            if (am != null) am.PlaySfx(am.hitSfx, 1.4f, 0.4f);
        }
        Refresh();
    }

    void Update()
    {
        if (cards == null || home == null) return;
        float dt = Time.unscaledDeltaTime;
        for (int i = 0; i < cards.Length; i++)
        {
            var c = cards[i];
            if (c == null || c.chest == null) continue;
            float s = shake != null && i < shake.Length ? shake[i] : 0f;
            if (s > 0f) shake[i] = Mathf.Max(0f, s - dt / 0.35f);
            // açılabilen sandık hafifçe zıplar (çağırır); yetmeyen sallanır
            bool can = ChestSystem.CanOpen(i);
            float bob = can ? Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3.2f + i)) * 10f : 0f;
            c.chest.rectTransform.anchoredPosition = home[i] + new Vector2(Mathf.Sin(s * 40f) * 12f * s, bob);
        }
    }
}
