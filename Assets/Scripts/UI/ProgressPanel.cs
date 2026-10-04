using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz 15 K4/K5: GELİŞİM (ortak izler, gruplu) ya da EŞYA (eşya başına izler) ekranı — kaydırılabilir liste.
/// Satırlar açılışta şablondan bir kez çoğaltılır. Görünen: açık izler + sıradaki en fazla 2 kilitli iz ("?" merak; kalabalık yok).
/// Yükseltince satır animasyonu + ses; yeni iz açılınca "YENİ!" ve bilgi şeridi. Kurulum: KakGelisimUi.Build.
/// </summary>
public class ProgressPanel : MonoBehaviour
{
    public RectTransform content;
    public UpgradeRow rowTemplate;
    public RectTransform headerTemplate;
    public TMP_Text walletText, summaryText, scrollText;
    public WalletHud menuWallet;
    public InfoToast toast;
    [Header("Eşya modu (K5)")]
    public bool itemsMode;
    [Tooltip("Eşya modunda başlık sırası ve ikonları (bölümün eşya listesi)")]
    public PowerupData[] items;
    [Tooltip("Kilitli izlerden kaç tanesi görünsün (merak)")]
    public int lockedPreview = 2;

    class Group
    {
        public RectTransform header;
        public TMP_Text stats;
        public PowerupType item;
        public readonly List<UpgradeRow> rows = new List<UpgradeRow>();
    }

    readonly List<UpgradeRow> rows = new List<UpgradeRow>();
    readonly List<Group> groups = new List<Group>();
    readonly List<UpgradeTrackData> unlockedBefore = new List<UpgradeTrackData>();
    readonly List<UpgradeTrackData> lockedSorted = new List<UpgradeTrackData>();
    bool built;

    static readonly UpgradeGroup[] Order = { UpgradeGroup.Survival, UpgradeGroup.Movement, UpgradeGroup.Gain, UpgradeGroup.Items };

    public static Color GroupColor(UpgradeGroup g)
    {
        switch (g)
        {
            case UpgradeGroup.Survival: return KakPalette.CamgobegiParlak; // iyi / koruma
            case UpgradeGroup.Movement: return KakPalette.AcikYesil;
            case UpgradeGroup.Gain: return KakPalette.Altin;               // ödül
            default: return KakPalette.Pembe;                               // eşya
        }
    }

    static string GroupKey(UpgradeGroup g)
    {
        switch (g)
        {
            case UpgradeGroup.Survival: return "grp_survival";
            case UpgradeGroup.Movement: return "grp_movement";
            case UpgradeGroup.Gain: return "grp_gain";
            default: return "grp_items";
        }
    }

    public static string ItemKey(PowerupType t)
    {
        switch (t)
        {
            case PowerupType.Heal: return "it_heal";
            case PowerupType.Shield: return "it_shield";
            case PowerupType.TimeSlow: return "it_slow";
            case PowerupType.Ghost: return "it_ghost";
            case PowerupType.Shackle: return "it_shackle";
            default: return "pu_" + t.ToString().ToLowerInvariant();
        }
    }

    void OnEnable()
    {
        Build();
        Loc.Changed += Refresh;
        Refresh();
    }

    void OnDisable() => Loc.Changed -= Refresh;

    void Build()
    {
        if (built || content == null || rowTemplate == null) return;
        built = true;
        var cat = UpgradeCatalog.Load();
        if (cat == null || cat.tracks == null) return;
        rowTemplate.gameObject.SetActive(false);
        if (headerTemplate != null) headerTemplate.gameObject.SetActive(false);

        if (itemsMode)
        {
            if (items == null) return;
            foreach (var pd in items)
            {
                if (pd == null) continue;
                var g = NewGroup(Loc.T(ItemKey(pd.type)), "", pd.harmful ? KakPalette.Tehlike : KakPalette.Pembe, pd.icon);
                g.item = pd.type;
                foreach (var t in cat.tracks)
                    if (t is ItemTrackData it && it.item == pd.type) AddRow(g, t);
            }
        }
        else
        {
            foreach (var gr in Order)
            {
                Group g = null;
                foreach (var t in cat.tracks)
                {
                    if (t == null || t is ItemTrackData || t.group != gr) continue;
                    if (g == null) g = NewGroup(Loc.T(GroupKey(gr)), GroupKey(gr), GroupColor(gr), null);
                    AddRow(g, t);
                }
            }
        }
    }

    Group NewGroup(string label, string locKey, Color color, Sprite icon)
    {
        var g = new Group();
        if (headerTemplate != null)
        {
            var h = Instantiate(headerTemplate, content);
            h.gameObject.SetActive(true);
            h.name = "Header_" + label;
            var t = h.Find("Label") != null ? h.Find("Label").GetComponent<TMP_Text>() : null;
            if (t != null)
            {
                var lt = t.GetComponent<LocText>();
                if (lt != null) { if (string.IsNullOrEmpty(locKey)) Destroy(lt); else lt.key = locKey; }
                t.SetText(label);
                t.color = color;
            }
            var ic = h.Find("Icon") != null ? h.Find("Icon").GetComponent<Image>() : null;
            if (ic != null) { ic.sprite = icon; ic.enabled = icon != null; }
            if (t != null && icon == null) t.rectTransform.anchoredPosition = new Vector2(8f, t.rectTransform.anchoredPosition.y);
            g.stats = h.Find("Stats") != null ? h.Find("Stats").GetComponent<TMP_Text>() : null;
            g.header = h;
        }
        groups.Add(g);
        return g;
    }

    void AddRow(Group g, UpgradeTrackData t)
    {
        var r = Instantiate(rowTemplate, content);
        r.name = "Row_" + t.id;
        r.track = t;
        r.panel = this;
        r.gameObject.SetActive(true);
        rows.Add(r);
        g.rows.Add(r);
    }

    public void Refresh()
    {
        // Kilitli izlerden yalnız en yakın lockedPreview tanesi görünür
        lockedSorted.Clear();
        foreach (var r in rows) if (r != null && !Progression.Unlocked(r.track)) lockedSorted.Add(r.track);
        lockedSorted.Sort((a, b) => a.unlockAtTotal.CompareTo(b.unlockAtTotal));
        if (itemsMode) ItemProgress.Snapshot();

        foreach (var g in groups)
        {
            bool any = false;
            foreach (var r in g.rows)
            {
                if (r == null) continue;
                bool vis = Progression.Unlocked(r.track) || lockedSorted.IndexOf(r.track) < lockedPreview;
                r.gameObject.SetActive(vis);
                if (vis) { r.Bind(); any = true; }
            }
            if (g.header != null) g.header.gameObject.SetActive(any);
            if (g.stats != null) g.stats.SetText(itemsMode ? ItemSummary(g) : "");
        }
        if (walletText != null) walletText.SetText("{0}", SaveSystem.Data.coins);
        if (scrollText != null) scrollText.SetText("{0}", SaveSystem.Data.scrolls);
        if (menuWallet != null) menuWallet.Refresh();
        if (summaryText != null)
        {
            var next = Progression.NextLocked();
            string head = string.Format(Loc.T("progress_level"), Progression.TotalLevels());
            summaryText.SetText(next != null ? head + "  ·  " + string.Format(Loc.T("progress_next"), next.unlockAtTotal) : head + "  ·  " + Loc.T("progress_all"));
        }
    }

    /// <summary>Eşya başlığındaki çarpan özeti: yalnız o eşyanın sahip olduğu izler (Kalp'te süre yok).</summary>
    static string ItemSummary(Group g)
    {
        if (g.item == PowerupType.Shackle) return "";
        bool dur = false, freq = false, ground = false;
        foreach (var r in g.rows)
        {
            if (r == null || !(r.track is ItemTrackData it)) continue;
            if (it.kind == ItemStat.Duration) dur = true;
            else if (it.kind == ItemStat.Frequency) freq = true;
            else if (it.kind == ItemStat.GroundTime) ground = true;
        }
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        string s = "";
        if (dur) s += Loc.T("it_dur") + " ×" + ItemProgress.DurationMult(g.item).ToString("0.00", inv);
        if (freq) s += (s.Length > 0 ? " · " : "") + Loc.T("it_freq") + " ×" + ItemProgress.FrequencyMult(g.item).ToString("0.00", inv);
        if (ground) s += (s.Length > 0 ? " · " : "") + Loc.T("it_ground") + " ×" + ItemProgress.GroundMult(g.item).ToString("0.00", inv);
        return s;
    }

    public void OnRowClicked(UpgradeRow row)
    {
        if (row == null || row.track == null) return;
        unlockedBefore.Clear();
        var cat = UpgradeCatalog.Load();
        if (cat != null) foreach (var t in cat.tracks) if (t != null && Progression.Unlocked(t)) unlockedBefore.Add(t);
        var am = AudioManager.Instance;
        if (Progression.TryUpgrade(row.track))
        {
            row.PlayUpgrade();
            if (am != null) am.PlaySfx(am.stageSfx);
            // Yeni açılan izler (bu ekrandakiler parlar; hepsi için bilgi şeridi)
            if (cat != null)
                foreach (var t in cat.tracks)
                {
                    if (t == null || unlockedBefore.Contains(t) || !Progression.Unlocked(t)) continue;
                    var r = RowOf(t.id);
                    if (r != null) r.PlayNew();
                    if (toast != null) toast.Show(string.Format(Loc.T("up_new_unlocked"), TrackTitle(t)), t is ItemTrackData ? KakPalette.Pembe : GroupColor(t.group));
                }
        }
        else
        {
            row.PlayDenied();
            if (am != null) am.PlaySfx(am.hitSfx, 1.4f, 0.4f);
        }
        Refresh();
    }

    /// <summary>Eşya izinde "Kalkan · Sıklık", ortak izde izin adı.</summary>
    public static string TrackTitle(UpgradeTrackData t)
    {
        if (t is ItemTrackData it) return Loc.T(ItemKey(it.item)) + " · " + Loc.T(t.nameKey);
        return Loc.T(t.nameKey);
    }

    /// <summary>Testler: iz satırı (açılış sonrası).</summary>
    public UpgradeRow RowOf(string id)
    {
        foreach (var r in rows) if (r != null && r.track != null && r.track.id == id) return r;
        return null;
    }
}
