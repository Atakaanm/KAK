using UnityEngine;

/// <summary>Faz 15 K8: kaçış kartları listesi + cesaret çubuğu ayarları (Resources/PerkCatalog).</summary>
[CreateAssetMenu(fileName = "PerkCatalog", menuName = "KacAtaKac/Perk Catalog")]
public class PerkCatalog : ScriptableObject
{
    public PerkData[] perks;

    [Header("Cesaret çubuğu")]
    [Tooltip("İlk kart için gereken cesaret; her seviyede stepNeed artar")]
    public float baseNeed = 7f;
    public float stepNeed = 4f;
    public float nearMissCourage = 1f;
    public float coinCourage = 0.5f;
    [Tooltip("Hayatta kalınan saniye başına")]
    public float secondCourage = 0.05f;
    [Tooltip("Kart seçince güvenli devam (sn)")]
    public float safeAfterPick = 1.2f;

    static PerkCatalog cached;
    public static PerkCatalog Load()
    {
        if (cached == null) cached = Resources.Load<PerkCatalog>("PerkCatalog");
        return cached;
    }
}
