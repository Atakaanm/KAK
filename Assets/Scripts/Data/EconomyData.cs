using System;
using UnityEngine;

/// <summary>
/// Faz 15 K6: anahtar ve sandık ekonomisi (tek yerde, Resources/EconomyData). Kullanıcı kararı: sandık = altın + oyunda düşen
/// anahtar; gerçek para yok (ileride reklam/satın alma bağlanabilir). Kurulum: KacAtaKac/Sandıkları Kur (KakChestSetup).
/// </summary>
[CreateAssetMenu(fileName = "EconomyData", menuName = "KacAtaKac/Economy Data")]
public class EconomyData : ScriptableObject
{
    [Header("Anahtar (oyunda)")]
    [Tooltip("Bu saniyelerde hayatta kalınca birer anahtar (oyun başına en fazla bu kadar)")]
    public float[] keyMilestones = { 60f, 120f, 180f };
    [Tooltip("Yeni rekorda ek anahtar")]
    public int keyOnRecord = 1;

    [Header("Sandıklar")]
    public ChestDef[] chests;
    [Tooltip("Her N. açılışta bir üst sandığın ödülü (garanti)")]
    public int pityEvery = 10;
    [Tooltip("Sandık altını en iyi skorla artar: × (1 + min(cap, rekor / bu))")]
    public float bestScoreForDouble = 10000f;
    public float bestScoreCap = 1f;

    static EconomyData cached;
    public static EconomyData Load()
    {
        if (cached == null) cached = Resources.Load<EconomyData>("EconomyData");
        return cached;
    }
}

[Serializable]
public class ChestDef
{
    public string id = "wood";
    public string nameKey = "chest_wood";
    public int keyCost = 1;
    public int goldMin = 60, goldMax = 120;
    public int scrollMin = 1, scrollMax = 2;
    [Range(0f, 1f)] public float scrollChance = 0.7f;
    [Range(0f, 1f)] public float bonusKeyChance;
    public Sprite closed, open;
    public Color glow = Color.white;
}
