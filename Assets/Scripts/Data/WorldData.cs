using UnityEngine;

/// <summary>Bölüm dünyası: sıralı bölümler, tema rengi, açılma koşulu (toplam yıldız).</summary>
[CreateAssetMenu(fileName = "New WorldData", menuName = "KacAtaKac/World Data")]
public class WorldData : ScriptableObject
{
    public string worldId = "ice";
    public string nameTR = "BUZ GÖLÜ";
    public string nameEN = "FROZEN LAKE";
    public Color themeColor = Color.cyan;
    [Tooltip("Açılmak için gereken toplam yıldız (önceki dünyalardan)")]
    public int starsToUnlock = 0;
    public LevelData[] levels;

    public string DisplayName => Loc.Current == Loc.Lang.TR ? nameTR : nameEN;
}
