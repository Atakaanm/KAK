using System;
using UnityEngine;

/// <summary>
/// Faz 15 K3: oyuncu özellikleri. Her kaynak (karakter pasifi, ortak gelişim, eşya geliştirme, ekipman, oyun içi kart)
/// StatModifier verir; StatSheet toplar. Değerler sona eklenir (veride sayı olarak saklanır, sıra değişmez).
/// </summary>
public enum StatId
{
    MaxHearts,      // can (kalp): sabit +
    MoveSpeed,      // hız: yüzde
    Hurtbox,        // gövde ölçeği: yüzde (eksi = ince yapı, taş zor vurur)
    InvulnTime,     // vurulunca dokunulmazlık: sn +
    Magnet,         // altın çekme yarıçapı: birim +
    CoinGain,       // altın: yüzde
    ScoreGain,      // skor/sn: yüzde
    NearMissBonus,  // kıl payı bonusu: yüzde
    NearMissRadius, // kıl payı mesafesi: yüzde
    ComboKeep,      // vurulunca korunan çarpan payı: sabit +
    ComboMax,       // çarpan tavanı: sabit +
    PowerDuration,  // güçlendirme süresi: yüzde
    ItemFrequency,  // eşya çıkma sıklığı: yüzde
    ItemGroundTime, // eşyanın yerde kalma süresi: yüzde
    Revive,         // oyun başına ikinci şans: sabit +
    StartShield,    // kalkanla başla: sabit (≥1 = evet)
    ShieldRegen,    // kalkan yenilenmesi seviyesi: sabit + (aralık tablodan)
    WarningTime,    // uyarı süresi (göktaşı düşüşü, fırlatıcı parlaması): yüzde
    ShackleResist,  // pranga direnci: sabit + (0-0,8)
    Count           // sınır (her zaman sonda)
}

[Serializable]
public struct StatModifier
{
    public StatId stat;
    public float flat;
    public float percent; // 0,1 = %10

    public StatModifier(StatId stat, float flat, float percent) { this.stat = stat; this.flat = flat; this.percent = percent; }
}

/// <summary>Kaynakların toplamı: değer = (taban + sabit) × (1 + yüzde). Oyun başında bir kez hesaplanır; okuma tahsis yapmaz.</summary>
public class StatSheet
{
    readonly float[] flat = new float[(int)StatId.Count];
    readonly float[] pct = new float[(int)StatId.Count];

    public void Clear()
    {
        Array.Clear(flat, 0, flat.Length);
        Array.Clear(pct, 0, pct.Length);
    }

    public void Add(StatModifier m) => Add(m.stat, m.flat, m.percent);

    public void Add(StatId s, float f, float p)
    {
        int i = (int)s;
        if (i < 0 || i >= flat.Length) return;
        flat[i] += f;
        pct[i] += p;
    }

    public void AddAll(StatModifier[] mods)
    {
        if (mods == null) return;
        for (int i = 0; i < mods.Length; i++) Add(mods[i]);
    }

    public float Flat(StatId s) => flat[(int)s];
    public float Percent(StatId s) => pct[(int)s];
    public float Apply(StatId s, float baseValue) => (baseValue + flat[(int)s]) * Mathf.Max(0f, 1f + pct[(int)s]);
}
