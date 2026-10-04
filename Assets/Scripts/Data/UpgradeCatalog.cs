using UnityEngine;

/// <summary>Faz 15 K3: ortak gelişim izlerinin listesi (Resources/UpgradeCatalog). Sıra = ekrandaki sıra.</summary>
[CreateAssetMenu(fileName = "UpgradeCatalog", menuName = "KacAtaKac/Upgrade Catalog")]
public class UpgradeCatalog : ScriptableObject
{
    public UpgradeTrackData[] tracks;

    static UpgradeCatalog cached;
    public static UpgradeCatalog Load()
    {
        if (cached == null) cached = Resources.Load<UpgradeCatalog>("UpgradeCatalog");
        return cached;
    }

    public UpgradeTrackData Find(string id)
    {
        if (tracks == null) return null;
        foreach (var t in tracks) if (t != null && t.id == id) return t;
        return null;
    }
}
