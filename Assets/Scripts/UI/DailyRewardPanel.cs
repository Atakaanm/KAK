using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Günlük ödül paneli (Faz 3c.6): 7 gün kutusu (alınanlar onaylı, bugünkü altın ve nabız atan),
/// AL → ödül verilir, kutu zıplar, cüzdan güncellenir; buton KAPAT olur. Menü açılışında kendiliğinden açılır.
/// </summary>
public class DailyRewardPanel : MonoBehaviour
{
    [System.Serializable]
    public class Tile
    {
        public RectTransform root;
        public Image background, check;
        public TMP_Text dayText, amountText;
    }

    public Tile[] tiles;
    public Button claimButton;
    public TMP_Text claimLabel;
    public Sprite goldSprite, stoneSprite;
    public WalletHud menuWallet;

    bool claimed;
    int todayIndex;
    float t;

    void OnEnable()
    {
        claimed = !DailyReward.CanClaim();
        FeatureGate.MarkIntroduced(Feature.DailyReward);
        Refresh();
    }

    public void Refresh()
    {
        int next = DailyReward.NextDay; // bugün alınacak gün (alındıysa bir sonraki)
        todayIndex = claimed ? SaveSystem.Data.dailyStreak - 1 : next - 1;
        for (int i = 0; i < tiles.Length && i < DailyReward.Rewards.Length; i++)
        {
            var tl = tiles[i];
            bool done = claimed ? i <= todayIndex : i < todayIndex;
            bool today = i == todayIndex;
            tl.dayText.text = string.Format(Loc.T("daily_day"), i + 1);
            tl.amountText.SetText("{0}", DailyReward.Rewards[i]);
            tl.background.sprite = today ? goldSprite : stoneSprite;
            tl.background.color = done && !today ? new Color(0.7f, 0.7f, 0.75f, 1f) : Color.white;
            tl.dayText.color = today ? KakPalette.Murekkep : KakPalette.Sis;
            tl.amountText.color = today ? KakPalette.Murekkep : KakPalette.AltinAcik;
            tl.check.gameObject.SetActive(done);
            tl.root.localScale = Vector3.one;
        }
        if (claimLabel != null) claimLabel.text = Loc.T(claimed ? "close" : "daily_claim");
        if (menuWallet != null) menuWallet.Refresh();
    }

    /// <summary>AL (ya da alındıysa KAPAT).</summary>
    public void OnClaim()
    {
        if (claimed) { gameObject.SetActive(false); return; }
        int reward = DailyReward.Claim();
        claimed = true;
        Refresh();
        if (reward > 0) StartCoroutine(Celebrate(tiles[todayIndex].root));
    }

    IEnumerator Celebrate(RectTransform tile)
    {
        var am = AudioManager.Instance;
        for (int i = 0; i < 5; i++)
        {
            if (am != null) am.PlaySfx(am.coinSfx, 1f + i * 0.08f, 0.8f);
            float d = 0f;
            while (d < 0.08f) { d += Time.unscaledDeltaTime; yield return null; }
        }
        for (float k = 0f; k < 1f; k += Time.unscaledDeltaTime / 0.35f)
        {
            float s = 1f + 0.25f * Mathf.Sin(k * Mathf.PI);
            tile.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        tile.localScale = Vector3.one;
    }

    void Update()
    {
        if (claimed || tiles == null || todayIndex < 0 || todayIndex >= tiles.Length) return;
        t += Time.unscaledDeltaTime;
        float s = 1f + 0.05f * Mathf.Sin(t * 5f);
        tiles[todayIndex].root.localScale = new Vector3(s, s, 1f);
    }
}
