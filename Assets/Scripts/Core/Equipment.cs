using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OwnedGear
{
    public string uid;
    public string id;
    public int rarity;
    public int level;
}

[Serializable]
public class SlotGear
{
    public EquipmentSlot slot;
    public string uid;
}

/// <summary>
/// Faz 15 K7: ekipman altyapısı (kullanıcı: "ileride ayakkabı, şapka, kalkan, kılıç; sandıktan bulup geliştirecekler").
/// Envanter (SaveData.gear), takılı yuvalar (SaveData.equipped), 3 aynı parça → bir üst nadirlik, özellikler sayfaya.
/// İçerik ve ekran KakScope.Equipment ile kapalı; açılınca StatBuilder takılı parçaları ekler.
/// </summary>
public static class Equipment
{
    public const float RarityStep = 0.35f, LevelStep = 0.05f;
    public static event Action Changed;

    static List<OwnedGear> Gear { get { var d = SaveSystem.Data; if (d.gear == null) d.gear = new List<OwnedGear>(); return d.gear; } }
    static List<SlotGear> Slots { get { var d = SaveSystem.Data; if (d.equipped == null) d.equipped = new List<SlotGear>(); return d.equipped; } }

    public static IReadOnlyList<OwnedGear> All => Gear;

    public static EquipmentData Def(string id)
    {
        var c = EquipmentCatalog.Load();
        return c != null ? c.Find(id) : null;
    }

    public static float Mult(OwnedGear g) => g == null ? 0f : (1f + RarityStep * g.rarity) * (1f + LevelStep * g.level);

    public static OwnedGear Add(string id, int rarity = 0)
    {
        var g = new OwnedGear { uid = Guid.NewGuid().ToString("N").Substring(0, 12), id = id, rarity = Mathf.Clamp(rarity, 0, (int)EquipmentRarity.Legend) };
        Gear.Add(g);
        Changed?.Invoke();
        return g;
    }

    public static OwnedGear Find(string uid)
    {
        foreach (var g in Gear) if (g != null && g.uid == uid) return g;
        return null;
    }

    public static OwnedGear Equipped(EquipmentSlot slot)
    {
        foreach (var s in Slots) if (s != null && s.slot == slot) return Find(s.uid);
        return null;
    }

    public static bool Equip(string uid)
    {
        var g = Find(uid);
        var def = g != null ? Def(g.id) : null;
        if (def == null) return false;
        Unequip(def.slot, false);
        Slots.Add(new SlotGear { slot = def.slot, uid = uid });
        Changed?.Invoke();
        return true;
    }

    public static void Unequip(EquipmentSlot slot, bool notify = true)
    {
        Slots.RemoveAll(s => s == null || s.slot == slot);
        if (notify) Changed?.Invoke();
    }

    public static bool CanMerge(OwnedGear g)
    {
        if (g == null || g.rarity >= (int)EquipmentRarity.Legend) return false;
        int n = 0;
        foreach (var o in Gear) if (o != null && o.id == g.id && o.rarity == g.rarity) n++;
        return n >= 3;
    }

    /// <summary>Bu parça + aynı tür ve nadirlikte 2 parça → bir üst nadirlik (en yüksek seviye korunur; takılıysa yenisi takılır).</summary>
    public static OwnedGear Merge(string uid)
    {
        var g = Find(uid);
        if (!CanMerge(g)) return null;
        var used = new List<OwnedGear> { g };
        foreach (var o in Gear)
            if (used.Count < 3 && o != null && o != g && o.id == g.id && o.rarity == g.rarity) used.Add(o);
        int level = 0;
        bool wasEquipped = false;
        foreach (var u in used)
        {
            level = Mathf.Max(level, u.level);
            foreach (var s in Slots) if (s != null && s.uid == u.uid) wasEquipped = true;
        }
        foreach (var u in used) { Gear.Remove(u); Slots.RemoveAll(s => s != null && s.uid == u.uid); }
        var result = Add(g.id, g.rarity + 1);
        result.level = level;
        if (wasEquipped) Equip(result.uid);
        Changed?.Invoke();
        return result;
    }

    /// <summary>Takılı parçaların özellikleri (KakScope.Equipment açıkken).</summary>
    public static void AddModifiers(StatSheet sheet)
    {
        if (!KakScope.Equipment) return;
        foreach (var s in Slots)
        {
            if (s == null) continue;
            var g = Find(s.uid);
            var def = g != null ? Def(g.id) : null;
            if (def == null || def.modifiers == null) continue;
            float m = Mult(g);
            foreach (var mod in def.modifiers) sheet.Add(mod.stat, mod.flat * m, mod.percent * m);
        }
    }
}
