using UnityEngine;

public enum ItemStat
{
    Duration,   // etki süresi (yüzde)
    Frequency,  // çıkma sıklığı (yüzde; Pranga'da eksi = daha seyrek)
    GroundTime, // yerde bekleme süresi (yüzde)
    Power,      // eşyaya özel güç (kalkan 2 vuruş, daha güçlü yavaşlatma, şifa ışığı, hayalet mıknatısı)
    Resist      // Pranga direnci (StatId.ShackleResist'e eklenir)
}

/// <summary>
/// Faz 15 K5: eşya geliştirme izi (kullanıcı: "kalma süresini, çıkma sıklığını, yerde bekleme süresini arttırsınlar").
/// Ortak gelişim iziyle aynı altyapı (Progression: seviye, altın, kayıt; UpgradeRow animasyonları); üst seviyeler
/// sandıktan çıkan Parşömen de ister (scrollCosts). Kurulum: KakItemUpgradeSetup.
/// </summary>
[CreateAssetMenu(fileName = "New ItemTrack", menuName = "KacAtaKac/Item Track")]
public class ItemTrackData : UpgradeTrackData
{
    [Header("Eşya")]
    public PowerupType item = PowerupType.Shield;
    public ItemStat kind = ItemStat.Duration;
    [Tooltip("Seviye başına parşömen (0 = yalnız altın)")]
    public int[] scrollCosts;

    public int ScrollCost(int level) => scrollCosts != null && level >= 0 && level < scrollCosts.Length ? scrollCosts[level] : 0;
}
