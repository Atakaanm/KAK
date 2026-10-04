using System;
using UnityEngine;

/// <summary>
/// Faz 15 K8: cesaret çubuğu ve kaçış kartları (kullanıcı kararı: survivor.io'daki oyun içi seçim, bizde kaçış yetenekleriyle).
/// Kıl payı, altın ve hayatta kalma çubuğu doldurur; dolunca oyun durur, 3 kart çıkar (PerkPanel), seçilen kart oyuncunun
/// özellik sayfasına eklenir (PlayerStats.ApplyLive) ya da davranış verir. Tek kişilik sonsuz modda; bot oynuyorsa ilk kart seçilir.
/// </summary>
public class RunPerks : MonoBehaviour
{
    public static RunPerks Instance { get; private set; }

    public float Courage { get; private set; }
    public int Level { get; private set; }
    public float Need => catalog != null ? catalog.baseNeed + catalog.stepNeed * Level : 9999f;
    public bool Choosing { get; private set; }
    public PerkData[] Offer { get; } = new PerkData[3];
    public int OfferCount { get; private set; }
    public event Action Changed;
    /// <summary>Kart seçim ekranı (PerkPanel kendini bağlar).</summary>
    public static Action<RunPerks> ShowPanel;

    PerkCatalog catalog;
    int[] levels;
    float[] pickWeights;

    public static RunPerks Ensure()
    {
        if (Instance != null) return Instance;
        return new GameObject("RunPerks").AddComponent<RunPerks>();
    }

    void Awake()
    {
        Instance = this;
        catalog = PerkCatalog.Load();
        int n = catalog != null && catalog.perks != null ? catalog.perks.Length : 0;
        levels = new int[n];
        pickWeights = new float[n];
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnEnable()
    {
        GameEvents.NearMiss += OnNearMiss;
        GameEvents.CoinCollected += OnCoin;
    }

    void OnDisable()
    {
        GameEvents.NearMiss -= OnNearMiss;
        GameEvents.CoinCollected -= OnCoin;
    }

    bool Active
    {
        get
        {
            var gm = GameManager.Instance;
            return catalog != null && levels.Length > 0 && gm != null && !gm.IsGameOver && !gm.IsLevelMode && !GameSettings.TwoPlayer;
        }
    }

    void OnNearMiss(Vector3 pos, bool dash) { if (Active) Add(catalog.nearMissCourage); }
    void OnCoin(int total, Vector3 pos) { if (Active) Add(catalog.coinCourage); }

    public void Add(float amount)
    {
        if (amount <= 0f) return;
        Courage += amount;
        Changed?.Invoke();
    }

    public int LevelOf(PerkData p)
    {
        if (catalog == null || p == null) return 0;
        int i = Array.IndexOf(catalog.perks, p);
        return i >= 0 ? levels[i] : 0;
    }

    void Update()
    {
        if (!Active || Choosing) return;
        Courage += catalog.secondCourage * Time.deltaTime;
        if (Courage >= Need) LevelUp();
    }

    void LevelUp()
    {
        Courage -= Need;
        Level++;
        if (!BuildOffer()) { Changed?.Invoke(); return; }
        Changed?.Invoke();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Bot (test/denge) seçim ekranında beklemez
        if (FindAnyObjectByType<KakAutoPilot>() != null) { Choose(0); return; }
#endif
        if (ShowPanel == null) { Choose(0); return; }
        Choosing = true;
        KakTime.SetPaused(true);
        ShowPanel(this);
    }

    /// <summary>Ağırlıklı, tekrarsız 3 kart (en üst seviyedekiler hariç).</summary>
    bool BuildOffer()
    {
        OfferCount = 0;
        var perks = catalog.perks;
        float total = 0f;
        for (int i = 0; i < perks.Length; i++)
        {
            pickWeights[i] = perks[i] != null && levels[i] < perks[i].maxLevel ? Mathf.Max(0.01f, perks[i].weight) : 0f;
            total += pickWeights[i];
        }
        while (OfferCount < Offer.Length && total > 0f)
        {
            float r = UnityEngine.Random.value * total;
            for (int i = 0; i < perks.Length; i++)
            {
                if (pickWeights[i] <= 0f) continue;
                r -= pickWeights[i];
                if (r > 0f) continue;
                Offer[OfferCount++] = perks[i];
                total -= pickWeights[i];
                pickWeights[i] = 0f;
                break;
            }
        }
        return OfferCount > 0;
    }

    public void Choose(int index)
    {
        if (index < 0 || index >= OfferCount) return;
        var p = Offer[index];
        int i = Array.IndexOf(catalog.perks, p);
        if (i >= 0) levels[i]++;
        Apply(p);
        if (Choosing)
        {
            Choosing = false;
            KakTime.SetPaused(false);
        }
        var st = PlayerStats.Primary;
        var ph = st != null ? st.GetComponent<PlayerHealth>() : null;
        if (ph != null) ph.SetInvulnerable(catalog.safeAfterPick); // güvenli devam
        Changed?.Invoke();
    }

    void Apply(PerkData p)
    {
        var st = PlayerStats.Primary;
        if (st == null || p == null) return;
        st.Sheet.AddAll(p.perLevel);
        st.ApplyLive();
        var ph = st.GetComponent<PlayerHealth>();
        switch (p.behaviour)
        {
            case PerkBehaviour.HeartPiece:
                if (ph != null)
                {
                    if (ph.CurrentHealth >= ph.MaxHealth && ph.MaxHealth < ph.HealthCap) ph.GrowMaxHealth(1);
                    else ph.Heal(1);
                }
                break;
            case PerkBehaviour.RockBreaker:
            {
                var rb = st.GetComponent<RockBreaker>();
                if (rb == null) rb = st.gameObject.AddComponent<RockBreaker>();
                rb.level = LevelOf(p);
                break;
            }
            case PerkBehaviour.GhostMoment:
            {
                var gm = st.GetComponent<GhostMoment>();
                if (gm == null) gm = st.gameObject.AddComponent<GhostMoment>();
                gm.level = LevelOf(p);
                break;
            }
        }
    }
}
