using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Faz 15 K7: ekipman altyapısının örnek içeriği (KakScope.Equipment kapalı; ekran ve sanat ileride). Her yuvadan bir parça:
/// meşale (ana el, ele oturur), tahta kılıç (ana el), deri şapka, hafif ayakkabı, tahta kalkan (yan el). Köprü: invoke KakEquipmentSetup Setup
/// </summary>
public static class KakEquipmentSetup
{
    const string Dir = "Assets/Data/Equipment";
    const string CatalogPath = "Assets/Resources/EquipmentCatalog.asset";

    [MenuItem("KacAtaKac/Ekipman Altyapısını Kur (Faz 15)")]
    public static string Setup()
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Data", "Equipment");
        var torch = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Torch.png");
        var list = new List<EquipmentData>
        {
            Item("torch", "gear_torch", EquipmentSlot.MainHand, torch, new Vector2(0f, -4f), new StatModifier(StatId.WarningTime, 0f, 0.05f)),
            Item("wood_sword", "gear_wood_sword", EquipmentSlot.MainHand, null, Vector2.zero, new StatModifier(StatId.ComboKeep, 0.05f, 0f)),
            Item("leather_hat", "gear_leather_hat", EquipmentSlot.Head, null, Vector2.zero, new StatModifier(StatId.CoinGain, 0f, 0.05f)),
            Item("light_shoes", "gear_light_shoes", EquipmentSlot.Feet, null, Vector2.zero, new StatModifier(StatId.MoveSpeed, 0f, 0.03f)),
            Item("wood_shield", "gear_wood_shield", EquipmentSlot.OffHand, null, Vector2.zero, new StatModifier(StatId.InvulnTime, 0.1f, 0f)),
        };
        var cat = AssetDatabase.LoadAssetAtPath<EquipmentCatalog>(CatalogPath);
        if (cat == null)
        {
            cat = ScriptableObject.CreateInstance<EquipmentCatalog>();
            AssetDatabase.CreateAsset(cat, CatalogPath);
        }
        cat.items = list.ToArray();
        EditorUtility.SetDirty(cat);
        AssetDatabase.SaveAssets();
        string msg = list.Count + " örnek ekipman → " + CatalogPath;
        Debug.Log("[KakEquipmentSetup] " + msg);
        return msg;
    }

    static EquipmentData Item(string id, string nameKey, EquipmentSlot slot, Sprite held, Vector2 grip, params StatModifier[] mods)
    {
        string path = Dir + "/Gear_" + id + ".asset";
        var e = AssetDatabase.LoadAssetAtPath<EquipmentData>(path);
        if (e == null)
        {
            e = ScriptableObject.CreateInstance<EquipmentData>();
            AssetDatabase.CreateAsset(e, path);
        }
        e.id = id; e.nameKey = nameKey; e.equipmentName = id; e.slot = slot;
        e.modifiers = mods; e.heldSprite = held; e.gripOffsetPx = grip;
        EditorUtility.SetDirty(e);
        return e;
    }
}
