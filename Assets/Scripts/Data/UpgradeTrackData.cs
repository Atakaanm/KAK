using UnityEngine;

public enum UpgradeGroup { Survival, Movement, Gain, Items }

/// <summary>
/// Faz 15 K3: ortak gelişim izi (tüm karakterler paylaşır). Her seviye bir StatModifier ekler (sabit ve/veya yüzde).
/// Fiyat tablosu ya da taban × büyüme; toplam gelişim seviyesi unlockAtTotal'a gelince açılır (adım adım).
/// Kurulum: KacAtaKac/Gelişim İzlerini Kur (KakProgressionSetup).
/// </summary>
[CreateAssetMenu(fileName = "New UpgradeTrack", menuName = "KacAtaKac/Upgrade Track")]
public class UpgradeTrackData : ScriptableObject
{
    [Tooltip("Kayıt anahtarı (SaveData.upgrades)")]
    public string id = "hearts";
    public UpgradeGroup group = UpgradeGroup.Survival;
    public StatId stat = StatId.MaxHearts;
    public float flatPerLevel;
    public float percentPerLevel;
    public int maxLevel = 5;

    [Header("Fiyat")]
    [Tooltip("Seviye başına altın (boşsa baseCost × costGrowth^seviye)")]
    public int[] costs;
    public int baseCost = 100;
    public float costGrowth = 1.6f;

    [Header("Açılma")]
    [Tooltip("Toplam gelişim seviyesi bu kadar olunca açılır (0 = baştan)")]
    public int unlockAtTotal;

    [Header("Görünüm")]
    public Sprite icon;
    public string nameKey = "up_hearts";
    public string descKey = "up_hearts_d";

    public int Cost(int level)
    {
        if (level < 0 || level >= maxLevel) return -1;
        if (costs != null && level < costs.Length) return costs[level];
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, level) / 5f) * 5;
    }

    public StatModifier ModifierAt(int level) => new StatModifier(stat, flatPerLevel * level, percentPerLevel * level);
}
