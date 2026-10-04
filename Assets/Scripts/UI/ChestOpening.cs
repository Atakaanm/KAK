using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz 15 K6: sandık açılış animasyonu (zaman ölçeğinden bağımsız):
///  1) sandık giderek sallanır  2) beyaz parlama + kıvılcım, kapak açılır, ışık hüzmeleri döner
///  3) ödüller tek tek zıplayarak gelir, sayılar sayılır  4) TAMAM. Ekrana dokununca sona atlar.
/// </summary>
public class ChestOpening : MonoBehaviour
{
    public Image chest, rays, flash;
    public UiBurst burst;
    public RectTransform[] slots;
    public Image[] slotIcons;
    public TMP_Text[] slotTexts;
    public TMP_Text pityLabel, tapHint;
    public GameObject okButton;
    public Sprite coinSprite, scrollSprite, keySprite;

    const float ShakeTime = 0.9f, BurstTime = 0.35f, SlotTime = 0.4f;

    ChestDef def;
    ChestReward reward;
    float t;
    int count;
    readonly int[] amounts = new int[3];
    readonly string[] keys = new string[3];
    bool burstDone, playing;

    public bool Playing => playing;

    public void Play(ChestDef d, ChestReward r)
    {
        def = d; reward = r; t = 0f; burstDone = false; playing = true;
        count = 0;
        Add(r.gold, coinSprite, "reward_gold");
        Add(r.scrolls, scrollSprite, "reward_scrolls");
        Add(r.keys, keySprite, "reward_keys");
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        if (chest != null) { chest.sprite = d != null ? d.closed : null; chest.rectTransform.localScale = Vector3.one; }
        if (rays != null) { rays.color = new Color(1f, 1f, 1f, 0f); }
        if (flash != null) flash.color = new Color(1f, 1f, 1f, 0f);
        if (pityLabel != null) pityLabel.gameObject.SetActive(false);
        if (okButton != null) okButton.SetActive(false);
        if (tapHint != null) tapHint.gameObject.SetActive(true);
        for (int i = 0; i < slots.Length; i++) if (slots[i] != null) slots[i].gameObject.SetActive(false);
        var am = AudioManager.Instance;
        if (am != null) am.PlaySfx(am.shootSfx, 0.7f, 0.6f);
    }

    void Add(int amount, Sprite icon, string key)
    {
        if (amount <= 0 || count >= slots.Length) return;
        amounts[count] = amount;
        keys[count] = key;
        if (slotIcons[count] != null) slotIcons[count].sprite = icon;
        count++;
    }

    /// <summary>Ekrana dokunma: animasyonu sona atlar; sonda TAMAM ile kapanır.</summary>
    public void Skip()
    {
        if (!playing) return;
        t = Mathf.Max(t, ShakeTime + BurstTime + SlotTime * count + 0.01f);
    }

    public void Close()
    {
        playing = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!playing) return;
        float dt = Time.unscaledDeltaTime;
        t += dt;
        float tBurst = ShakeTime, tSlots = ShakeTime + BurstTime, tEnd = tSlots + SlotTime * count;

        if (t < tBurst)
        {
            // 1) giderek sallanır, hafif büyür
            float k = t / ShakeTime;
            float amp = 4f + 22f * k * k;
            if (chest != null)
            {
                chest.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 48f) * amp * 0.35f);
                chest.rectTransform.localScale = Vector3.one * (1f + 0.08f * k);
            }
            return;
        }
        if (!burstDone)
        {
            // 2) patlama: parlama, açık sandık, kıvılcım, ses
            burstDone = true;
            if (chest != null) { chest.sprite = def != null ? def.open : chest.sprite; chest.rectTransform.localRotation = Quaternion.identity; }
            if (burst != null) burst.Burst(def != null ? def.glow : KakPalette.AltinAcik);
            var am = AudioManager.Instance;
            if (am != null) am.PlaySfx(am.stageSfx, 1.1f);
            if (pityLabel != null) pityLabel.gameObject.SetActive(reward.pity);
        }
        float sb = Mathf.Clamp01((t - tBurst) / BurstTime);
        if (flash != null) flash.color = new Color(1f, 1f, 1f, (1f - sb) * 0.85f);
        if (chest != null) chest.rectTransform.localScale = Vector3.one * (1.08f + 0.18f * Mathf.Sin(sb * Mathf.PI) * (1f - sb * 0.5f));
        if (rays != null)
        {
            var c = def != null ? def.glow : Color.white;
            c.a = Mathf.Min(0.85f, sb * 0.85f);
            rays.color = c;
            rays.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -Time.unscaledTime * 25f);
        }

        // 3) ödüller tek tek
        for (int i = 0; i < count; i++)
        {
            float st = tSlots + SlotTime * i;
            if (t < st || slots[i] == null) continue;
            if (!slots[i].gameObject.activeSelf)
            {
                slots[i].gameObject.SetActive(true);
                var am = AudioManager.Instance;
                if (am != null) am.PlaySfx(am.coinSfx, 1f + 0.1f * i, 0.9f);
            }
            float k = Mathf.Clamp01((t - st) / SlotTime);
            float pop = k < 0.5f ? Mathf.Lerp(0.2f, 1.2f, k / 0.5f) : Mathf.Lerp(1.2f, 1f, (k - 0.5f) / 0.5f);
            slots[i].localScale = Vector3.one * pop;
            if (slotTexts[i] != null) slotTexts[i].SetText(Loc.T(keys[i]).Replace("{0}", Mathf.RoundToInt(amounts[i] * Mathf.Min(1f, k * 1.4f)).ToString()));
        }

        // 4) son
        if (t >= tEnd)
        {
            if (okButton != null && !okButton.activeSelf) okButton.SetActive(true);
            if (tapHint != null) tapHint.gameObject.SetActive(false);
        }
    }
}
