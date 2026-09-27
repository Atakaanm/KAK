using UnityEngine;

/// <summary>
/// G7 (Faz 11): Eldiven — süre boyunca gelen kartopunu hasar almadan yakalar. Eldeki kartopu aksiyon düğmesiyle
/// (dash düğmesinin yerinde, kartopu ikonuyla; klavyede Space / 2. oyuncuda Enter) baktığın yöne fırlatılır ve
/// değdiği kartopunu parçalar. İki kişilikte tek düğme olduğu için kısa gecikmeyle kendiliğinden fırlatılır.
/// </summary>
public class SnowballCatcher : MonoBehaviour
{
    public float throwSpeedMultiplier = 2.4f;
    public float autoThrowDelay = 0.5f; // iki kişilik mod

    float gloveUntil = -1f, caughtAt;
    ProjectileData held;
    PlayerMovement2D mv;
    DashButton button;

    public bool GloveActive => Time.time < gloveUntil;
    public bool Holding => held != null;
    public Sprite HeldSprite => held != null ? held.projectileSprite : null;

    void Awake() { mv = GetComponent<PlayerMovement2D>(); }

    public void Activate(float seconds) { gloveUntil = Mathf.Max(gloveUntil, Time.time + seconds); }

    public bool TryCatch(ProjectileData d)
    {
        if (!GloveActive || Holding || d == null) return false;
        held = d;
        caughtAt = Time.time;
        var am = AudioManager.Instance;
        if (am != null) am.PlaySfx(am.shieldSfx);
        WorldPopup.Show(Loc.T("caught"), transform.position + Vector3.up * 0.6f, KakPalette.CamgobegiParlak, 0.9f);
        ShowButton(true);
        return true;
    }

    public void Throw()
    {
        if (!Holding) return;
        var d = held;
        held = null;
        Vector2 dir = mv != null && mv.MovementInput.sqrMagnitude > 0.01f ? mv.MovementInput : (mv != null ? mv.LastDirection : Vector2.up);
        var prefab = d.projectilePrefab;
        if (prefab != null)
        {
            var p = Projectile.Launch(prefab, d, transform.position + (Vector3)(dir.normalized * 0.45f), dir, throwSpeedMultiplier, 0.9f);
            if (p != null) p.friendly = true;
        }
        var am = AudioManager.Instance;
        if (am != null) am.PlayShootSfx();
        ShowButton(false);
    }

    void Update()
    {
        if (!Holding) return;
        if (GameSettings.TwoPlayer) { if (Time.time - caughtAt >= autoThrowDelay) Throw(); return; }
        var key = mv != null && mv.keyboardScheme == 2 ? KeyCode.Return : KeyCode.Space;
        if (Input.GetKeyDown(key)) Throw();
    }

    void ShowButton(bool holding)
    {
        if (GameSettings.TwoPlayer) return;
        if (button == null) button = FindAnyObjectByType<DashButton>(FindObjectsInactive.Include);
        if (button == null) return;
        button.catcher = this;
        bool dashOn = button.dash != null && button.dash.available;
        button.gameObject.SetActive(holding || dashOn);
    }
}
