using UnityEngine;

/// <summary>Faz 15 K7: ekipman listesi (Resources/EquipmentCatalog). Altyapı: içerik KakScope.Equipment ile kapalı.</summary>
[CreateAssetMenu(fileName = "EquipmentCatalog", menuName = "KacAtaKac/Equipment Catalog")]
public class EquipmentCatalog : ScriptableObject
{
    public EquipmentData[] items;

    static EquipmentCatalog cached;
    public static EquipmentCatalog Load()
    {
        if (cached == null) cached = Resources.Load<EquipmentCatalog>("EquipmentCatalog");
        return cached;
    }

    public EquipmentData Find(string id)
    {
        if (items == null) return null;
        foreach (var e in items) if (e != null && e.id == id) return e;
        return null;
    }
}
