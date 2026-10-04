using UnityEngine;

public enum PerkBehaviour { None, HeartPiece, RockBreaker, GhostMoment }

/// <summary>
/// Faz 15 K8: oyun içi kaçış kartı (survivor.io'daki yeteneklerin kaçış karşılığı; oyun bitince sıfırlanır).
/// Her seviye perLevel özelliklerini ekler; bazı kartların davranışı var (kalp parçası, taş kırıcı, hayalet anı).
/// Katalog: Resources/PerkCatalog. Kurulum: KacAtaKac/Kaçış Kartlarını Kur (KakPerkSetup).
/// </summary>
[CreateAssetMenu(fileName = "New Perk", menuName = "KacAtaKac/Perk")]
public class PerkData : ScriptableObject
{
    public string id = "magnet";
    public string nameKey = "perk_magnet";
    public string descKey = "perk_magnet_d";
    public Sprite icon;
    public Color color = Color.white;
    public int maxLevel = 3;
    [Tooltip("Seçilme ağırlığı")]
    public float weight = 1f;
    [Tooltip("Seviye başına eklenen özellikler")]
    public StatModifier[] perLevel;
    public PerkBehaviour behaviour = PerkBehaviour.None;
}
