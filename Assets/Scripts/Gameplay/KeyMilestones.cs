using UnityEngine;

/// <summary>
/// Faz 15 K6: oyunda anahtar — EconomyData.keyMilestones saniyelerinde hayatta kalınca birer anahtar (afiş + ses).
/// Kayda oyun sonunda GameManager.FinalizeRun işler (rekor ödülüyle). LevelManager sonsuz modda kurar.
/// </summary>
public class KeyMilestones : MonoBehaviour
{
    public static KeyMilestones Instance { get; private set; }
    public int RunKeys { get; private set; }

    float[] marks;
    int next;

    public static KeyMilestones Ensure()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("KeyMilestones");
        return go.AddComponent<KeyMilestones>();
    }

    void Awake()
    {
        Instance = this;
        var e = EconomyData.Load();
        marks = e != null ? e.keyMilestones : null;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Update()
    {
        var gm = GameManager.Instance;
        if (marks == null || next >= marks.Length || gm == null || gm.IsGameOver || gm.scoreManager == null) return;
        if (gm.IsLevelMode) return;
        if (gm.scoreManager.ElapsedSeconds < marks[next]) return;
        next++;
        RunKeys++;
        var p = Projectile.PlayerTarget;
        WorldPopup.Show(Loc.T("key_earned"), (p != null ? p.position : Vector3.zero) + Vector3.up * 0.9f, KakPalette.AltinAcik, 1.2f);
        var am = AudioManager.Instance;
        if (am != null) am.PlaySfx(am.stageSfx, 1.25f);
    }
}
