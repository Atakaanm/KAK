using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Faz 15 K3: ortak gelişim izlerini (16) ve kataloğu kurar, karakter pasiflerini yazar (tekrar çalıştırılabilir).
/// Kullanıcı: "sadece 3 yetenekli olup onları geliştirmeleri hoşuma gitmedi, birçok özellik istiyorum".
/// İzler toplam gelişim seviyesiyle adım adım açılır (3c drip-feed). Köprü: invoke KakProgressionSetup Setup
/// </summary>
public static class KakProgressionSetup
{
    const string Dir = "Assets/Data/Upgrades";
    const string CatalogPath = "Assets/Resources/UpgradeCatalog.asset";

    struct T
    {
        public string id; public UpgradeGroup g; public StatId s; public float flat, pct; public int[] costs; public int unlock;
        public T(string id, UpgradeGroup g, StatId s, float flat, float pct, int unlock, params int[] costs)
        { this.id = id; this.g = g; this.s = s; this.flat = flat; this.pct = pct; this.unlock = unlock; this.costs = costs; }
    }

    // Sıra = ekrandaki sıra. Fiyatlar: ilk seviyeler 1-3 oyunda, son seviyeler uzun hedef (denge K9'da).
    static readonly T[] Tracks =
    {
        new T("hearts", UpgradeGroup.Survival, StatId.MaxHearts, 1f, 0f, 0, 60, 250, 600, 1000),
        new T("invuln", UpgradeGroup.Survival, StatId.InvulnTime, 0.1f, 0f, 8, 120, 260, 480, 800, 1300),
        new T("slim", UpgradeGroup.Survival, StatId.Hurtbox, 0f, -0.03f, 10, 150, 320, 600, 1000, 1600),
        new T("sense", UpgradeGroup.Survival, StatId.WarningTime, 0f, 0.08f, 16, 140, 300, 560, 950, 1500),
        new T("startshield", UpgradeGroup.Survival, StatId.StartShield, 1f, 0f, 26, 900),
        new T("shieldregen", UpgradeGroup.Survival, StatId.ShieldRegen, 1f, 0f, 30, 1200, 2200, 3600),
        new T("revive", UpgradeGroup.Survival, StatId.Revive, 1f, 0f, 35, 2500),
        new T("speed", UpgradeGroup.Movement, StatId.MoveSpeed, 0f, 0.03f, 0, 80, 200, 450, 800, 1300),
        new T("magnet", UpgradeGroup.Gain, StatId.Magnet, 0.35f, 0f, 0, 100, 220, 420, 720, 1150),
        new T("coin", UpgradeGroup.Gain, StatId.CoinGain, 0f, 0.08f, 2, 150, 330, 620, 1050, 1700),
        new T("score", UpgradeGroup.Gain, StatId.ScoreGain, 0f, 0.05f, 4, 130, 290, 540, 900, 1450),
        new T("nearbonus", UpgradeGroup.Gain, StatId.NearMissBonus, 0f, 0.2f, 6, 110, 250, 470, 800, 1300),
        new T("nearradius", UpgradeGroup.Gain, StatId.NearMissRadius, 0f, 0.06f, 19, 200, 420, 750, 1250),
        new T("combokeep", UpgradeGroup.Gain, StatId.ComboKeep, 0.08f, 0f, 13, 220, 460, 820, 1350),
        new T("combomax", UpgradeGroup.Gain, StatId.ComboMax, 0.25f, 0f, 22, 260, 540, 950, 1550),
        new T("power", UpgradeGroup.Items, StatId.PowerDuration, 0f, 0.06f, 0, 80, 200, 450, 800, 1300),
    };

    [MenuItem("KacAtaKac/Gelişim İzlerini Kur (Faz 15)")]
    public static string Setup()
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Data", "Upgrades");
        var list = new List<UpgradeTrackData>();
        foreach (var t in Tracks)
        {
            string path = Dir + "/Up_" + t.id + ".asset";
            var a = AssetDatabase.LoadAssetAtPath<UpgradeTrackData>(path);
            if (a == null)
            {
                a = ScriptableObject.CreateInstance<UpgradeTrackData>();
                AssetDatabase.CreateAsset(a, path);
            }
            a.id = t.id; a.group = t.g; a.stat = t.s;
            a.flatPerLevel = t.flat; a.percentPerLevel = t.pct;
            a.costs = t.costs; a.maxLevel = t.costs.Length;
            a.unlockAtTotal = t.unlock;
            a.nameKey = "up_" + t.id; a.descKey = "up_" + t.id + "_d";
            var icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Upgrades/up_" + t.id + ".png");
            if (icon != null) a.icon = icon;
            EditorUtility.SetDirty(a);
            list.Add(a);
        }

        list.AddRange(SetupItemTracks());

        var cat = AssetDatabase.LoadAssetAtPath<UpgradeCatalog>(CatalogPath);
        if (cat == null)
        {
            cat = ScriptableObject.CreateInstance<UpgradeCatalog>();
            AssetDatabase.CreateAsset(cat, CatalogPath);
        }
        cat.tracks = list.ToArray();
        EditorUtility.SetDirty(cat);

        int pas = SetupPassives();
        AssetDatabase.SaveAssets();
        string msg = list.Count + " iz (ortak + eşya) → " + CatalogPath + ", " + pas + " karakter pasifi";
        Debug.Log("[KakProgressionSetup] " + msg);
        return msg;
    }

    // ── Faz 15 K5: eşya geliştirme izleri (kullanıcı: kalma süresi, çıkma sıklığı, yerde bekleme süresi) ──
    struct I
    {
        public string id, nameKey, descKey; public PowerupType item; public ItemStat kind; public float flat, pct; public int unlock; public int[] gold, scroll;
        public I(string id, PowerupType item, ItemStat kind, float flat, float pct, int unlock, string nameKey, string descKey, int[] gold, int[] scroll)
        { this.id = id; this.item = item; this.kind = kind; this.flat = flat; this.pct = pct; this.unlock = unlock; this.nameKey = nameKey; this.descKey = descKey; this.gold = gold; this.scroll = scroll; }
    }

    static readonly int[] G5 = { 120, 260, 480, 800, 1300 }, S5 = { 0, 0, 0, 2, 4 };
    static readonly int[] G4 = { 100, 220, 420, 720 }, S4 = { 0, 0, 1, 3 };

    static readonly I[] Items =
    {
        new I("it_heal_freq", PowerupType.Heal, ItemStat.Frequency, 0f, 0.15f, 2, "it_freq", "it_freq_d", G5, S5),
        new I("it_heal_ground", PowerupType.Heal, ItemStat.GroundTime, 0f, 0.20f, 2, "it_ground", "it_ground_d", G4, S4),
        new I("it_heal_pow", PowerupType.Heal, ItemStat.Power, 0f, 0f, 12, "it_heal_pow", "it_heal_pow_d", new[] { 1500 }, new[] { 6 }),
        new I("it_shield_freq", PowerupType.Shield, ItemStat.Frequency, 0f, 0.15f, 3, "it_freq", "it_freq_d", G5, S5),
        new I("it_shield_ground", PowerupType.Shield, ItemStat.GroundTime, 0f, 0.20f, 3, "it_ground", "it_ground_d", G4, S4),
        new I("it_shield_pow", PowerupType.Shield, ItemStat.Power, 0f, 0f, 14, "it_shield_pow", "it_shield_pow_d", new[] { 1800 }, new[] { 8 }),
        new I("it_slow_dur", PowerupType.TimeSlow, ItemStat.Duration, 0f, 0.12f, 5, "it_dur", "it_dur_d", G5, S5),
        new I("it_slow_freq", PowerupType.TimeSlow, ItemStat.Frequency, 0f, 0.15f, 5, "it_freq", "it_freq_d", G5, S5),
        new I("it_slow_ground", PowerupType.TimeSlow, ItemStat.GroundTime, 0f, 0.20f, 5, "it_ground", "it_ground_d", G4, S4),
        new I("it_slow_pow", PowerupType.TimeSlow, ItemStat.Power, 0f, 0f, 16, "it_slow_pow", "it_slow_pow_d", new[] { 600, 1200, 2000 }, new[] { 2, 4, 6 }),
        new I("it_ghost_dur", PowerupType.Ghost, ItemStat.Duration, 0f, 0.12f, 7, "it_dur", "it_dur_d", G5, S5),
        new I("it_ghost_freq", PowerupType.Ghost, ItemStat.Frequency, 0f, 0.15f, 7, "it_freq", "it_freq_d", G5, S5),
        new I("it_ghost_ground", PowerupType.Ghost, ItemStat.GroundTime, 0f, 0.20f, 7, "it_ground", "it_ground_d", G4, S4),
        new I("it_ghost_pow", PowerupType.Ghost, ItemStat.Power, 0f, 0f, 18, "it_ghost_pow", "it_ghost_pow_d", new[] { 900, 1800 }, new[] { 3, 6 }),
        new I("it_shackle_res", PowerupType.Shackle, ItemStat.Resist, 0.15f, 0f, 9, "it_shackle_res", "it_shackle_res_d", new[] { 150, 320, 600, 1000 }, new[] { 0, 0, 1, 2 }),
        new I("it_shackle_freq", PowerupType.Shackle, ItemStat.Frequency, 0f, -0.12f, 9, "it_shackle_freq", "it_shackle_freq_d", new[] { 200, 450, 800 }, new[] { 0, 1, 2 }),
    };

    static List<UpgradeTrackData> SetupItemTracks()
    {
        var list = new List<UpgradeTrackData>();
        foreach (var t in Items)
        {
            string path = Dir + "/" + t.id + ".asset";
            var a = AssetDatabase.LoadAssetAtPath<ItemTrackData>(path);
            if (a == null)
            {
                a = ScriptableObject.CreateInstance<ItemTrackData>();
                AssetDatabase.CreateAsset(a, path);
            }
            a.id = t.id; a.group = UpgradeGroup.Items; a.item = t.item; a.kind = t.kind;
            a.stat = t.kind == ItemStat.Resist ? StatId.ShackleResist : StatId.PowerDuration; // genel sayfaya yalnız Resist eklenir
            a.flatPerLevel = t.flat; a.percentPerLevel = t.pct;
            a.costs = t.gold; a.scrollCosts = t.scroll; a.maxLevel = t.gold.Length;
            a.unlockAtTotal = t.unlock;
            a.nameKey = t.nameKey; a.descKey = t.descKey;
            var icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Upgrades/up_" + KindIcon(t.kind) + ".png");
            if (icon != null) a.icon = icon;
            EditorUtility.SetDirty(a);
            list.Add(a);
        }
        return list;
    }

    static string KindIcon(ItemStat k)
    {
        switch (k)
        {
            case ItemStat.Duration: return "power";
            case ItemStat.Frequency: return "kind_freq";
            case ItemStat.GroundTime: return "kind_ground";
            case ItemStat.Resist: return "startshield";
            default: return "kind_power";
        }
    }

    static int SetupPassives()
    {
        int n = 0;
        n += Passive("Assets/Data/Boy_PlayerData.asset", "pas_boy", new StatModifier(StatId.PowerDuration, 0f, 0.10f));
        n += Passive("Assets/Data/Characters/Ada_PlayerData.asset", "pas_ada", new StatModifier(StatId.NearMissBonus, 0f, 0.25f));
        n += Passive("Assets/Data/Characters/Swift_PlayerData.asset", "pas_swift", new StatModifier(StatId.WarningTime, 0f, 0.15f));
        n += Passive("Assets/Data/Characters/Tank_PlayerData.asset", "pas_tank", new StatModifier(StatId.InvulnTime, 0.15f, 0f));
        n += Passive("Assets/Data/Characters/Lucky_PlayerData.asset", "pas_lucky", new StatModifier(StatId.Magnet, 0.3f, 0f));
        return n;
    }

    static int Passive(string path, string key, params StatModifier[] mods)
    {
        var p = AssetDatabase.LoadAssetAtPath<PlayerData>(path);
        if (p == null) { Debug.LogWarning("[KakProgressionSetup] karakter yok: " + path); return 0; }
        p.passive = mods;
        p.passiveKey = key;
        EditorUtility.SetDirty(p);
        return 1;
    }
}
