using UnityEngine;

/// <summary>
/// Faz 13 F2: gol fırsatı (Futbol). Kullanıcı: "topu bir yerden alıp kaleye şut çektirebiliriz". Ara sıra sahaya serbest top
/// düşer ("GOL FIRSATI!"); dokunan oyuncunun ayağına geçer (sürer), üst kaleye götürünce GOL! → bonus skor + altın.
/// Vurulunca ya da düşünce top yere düşer (yeniden alınabilir). Kaçışı bölmez: isteğe bağlı küçük hedef.
/// Yumuşak: top taşlardan uzak bir yere düşer, kale sürerken parlar (nereye götüreceğin belli), yerde kalma süresi sonunda yanıp söner.
/// Tema `goalChance` açıksa LevelManager.ApplyTheme kurar.
/// </summary>
public class GoalChance : MonoBehaviour
{
    public float firstDelay = 22f;
    public float intervalMin = 28f, intervalMax = 42f;
    public float lifeOnGround = 12f;
    public float pickRadius = 0.45f;
    public int goalScore = 300;
    public int goalCoins = 5;

    public enum State { Waiting, OnGround, Dribbling }
    public State Current { get; private set; } = State.Waiting;
    public PlayerHealth Carrier { get; private set; }
    public int Goals { get; private set; }
    public Vector2 BallPosition => ballPos;

    SpriteRenderer ball, ring, goalGlow, marker;
    float next, groundUntil, roll, noPickUntil;
    Vector2 ballPos;
    Rect play;

    public static GoalChance Create(Sprite ballSprite)
    {
        var existing = Object.FindAnyObjectByType<GoalChance>();
        if (existing != null) return existing;
        var go = new GameObject("GoalChance");
        var g = go.AddComponent<GoalChance>();
        g.ball = g.Make("Ball", ballSprite, 0, Color.white);
        g.ring = g.Make("Ring", Resources.Load<Sprite>("Glint"), 0, KakPalette.WithAlpha(KakPalette.AltinAcik, 0.6f));
        g.goalGlow = g.Make("GoalGlow", Resources.Load<Sprite>("LightGlow"), 30, KakPalette.WithAlpha(KakPalette.AltinAcik, 0f));
        g.marker = g.Make("Marker", Resources.Load<Sprite>("BallMarker"), 140, Color.white); // tehlikeli toplardan ayırt edilsin
        g.marker.transform.localScale = Vector3.one * 1.6f;
        g.Hide();
        return g;
    }

    SpriteRenderer Make(string n, Sprite s, int order, Color c)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = s;
        r.color = c;
        r.sortingOrder = order;
        DarkWorld.UseGameMaterial(r);
        return r;
    }

    void Start()
    {
        next = firstDelay;
        var arena = FindAnyObjectByType<ArenaAutoLayout>();
        play = arena != null ? arena.PlayableWorldRect : Projectile.ArenaRect;
        if (goalGlow != null)
        {
            goalGlow.transform.position = new Vector3(play.center.x, play.yMax - 0.15f, 0f);
            float s = 2.4f / Mathf.Max(0.01f, goalGlow.sprite != null ? goalGlow.sprite.bounds.size.x : 1f);
            goalGlow.transform.localScale = new Vector3(s, s * 0.6f, 1f);
        }
    }

    void OnEnable() => GameEvents.PlayerDamaged += OnDamaged;
    void OnDisable() => GameEvents.PlayerDamaged -= OnDamaged;

    float Elapsed => GameManager.Instance != null && GameManager.Instance.scoreManager != null ? GameManager.Instance.scoreManager.ElapsedSeconds : Time.timeSinceLevelLoad;
    bool Over => GameManager.Instance != null && GameManager.Instance.IsGameOver;

    /// <summary>Üst kalenin ağzı (dünya): ortada ~1,5 birim genişlik, üst duvarın dibi.</summary>
    public Rect GoalMouth => new Rect(play.center.x - 0.75f, play.yMax - 0.5f, 1.5f, 1f);

    void Update()
    {
        if (Over) { Hide(); return; }
        switch (Current)
        {
            case State.Waiting:
                bool eventRunning = EndlessEventManager.Instance != null && EndlessEventManager.Instance.Running;
                if (Elapsed >= next && !eventRunning) SpawnNow();
                break;
            case State.OnGround: UpdateGround(); break;
            case State.Dribbling: UpdateDribble(); break;
        }
    }

    /// <summary>Topu sahaya düşürür (testler, geliştirici). Taşlardan uzak, kalenin uzağında (alt yarı) bir nokta.</summary>
    public void SpawnNow()
    {
        Vector2 best = new Vector2(play.center.x, play.yMin + play.height * 0.3f);
        float bestScore = -1f;
        for (int i = 0; i < 8; i++)
        {
            var p = new Vector2(Random.Range(play.xMin + 0.6f, play.xMax - 0.6f), Random.Range(play.yMin + 0.6f, play.center.y));
            float minD = 99f;
            foreach (var pr in Projectile.Active) if (pr != null) minD = Mathf.Min(minD, Vector2.Distance(p, pr.transform.position));
            if (minD > bestScore) { bestScore = minD; best = p; }
        }
        Drop(best, lifeOnGround);
        GameEvents.RaiseEndlessEventStarted("ev_goalchance"); // afiş: GOL FIRSATI!
        var am = AudioManager.Instance;
        if (am != null) am.PlaySfx(am.stageSfx);
    }

    void Drop(Vector2 at, float life)
    {
        Current = State.OnGround;
        Carrier = null;
        ballPos = at;
        groundUntil = Time.time + life;
        ball.enabled = true; ring.enabled = true; marker.enabled = true;
        goalGlow.enabled = false;
        Place();
    }

    void Hide()
    {
        if (ball != null) ball.enabled = false;
        if (ring != null) ring.enabled = false;
        if (marker != null) marker.enabled = false;
        if (goalGlow != null) goalGlow.enabled = false;
        Carrier = null;
        if (Current != State.Waiting) next = Elapsed + Random.Range(intervalMin, intervalMax);
        Current = State.Waiting;
    }

    void UpdateGround()
    {
        float left = groundUntil - Time.time;
        if (left <= 0f) { Hide(); return; }
        // son 3 sn yanıp söner
        bool vis = left > 3f || Mathf.Sin(Time.time * Mathf.Lerp(6f, 16f, 1f - left / 3f)) > -0.2f;
        ball.enabled = vis;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 5f);
        ring.color = KakPalette.WithAlpha(KakPalette.AltinAcik, 0.6f + 0.35f * pulse);
        ring.transform.localScale = Vector3.one * ((1.1f + 0.2f * pulse) / 0.32f);
        marker.enabled = vis;
        marker.transform.position = new Vector3(ballPos.x, ballPos.y + 0.55f + 0.1f * Mathf.Abs(Mathf.Sin(Time.time * 6f)), 0f);
        if (Time.time < noPickUntil) { Place(); return; } // top kaçtıktan hemen sonra aynı yerde geri alınmasın
        foreach (var ph in PlayerRegistry.All)
        {
            if (ph == null || ph.IsDead) continue;
            if (Vector2.Distance(ph.transform.position, ballPos) <= pickRadius)
            {
                Carrier = ph;
                Current = State.Dribbling;
                ring.enabled = false;
                marker.enabled = false;
                goalGlow.enabled = true;
                WorldPopup.Show(Loc.T("to_goal"), ph.transform.position + Vector3.up * 0.7f, KakPalette.AltinAcik, 0.9f);
                break;
            }
        }
        Place();
    }

    void UpdateDribble()
    {
        if (Carrier == null || Carrier.IsDead || !Carrier.isActiveAndEnabled) { Drop(ballPos, 6f); return; }
        var mv = Carrier.GetComponent<PlayerMovement2D>();
        Vector2 dir = mv != null ? (mv.MovementInput.sqrMagnitude > 0.01f ? mv.MovementInput.normalized : mv.LastDirection) : Vector2.up;
        Vector2 target = (Vector2)Carrier.transform.position + dir * 0.3f + new Vector2(0f, -0.22f);
        ballPos = Vector2.Lerp(ballPos, target, 1f - Mathf.Exp(-14f * Time.deltaTime));
        roll += (mv != null ? mv.CurrentSpeed * mv.MovementInput.magnitude : 0f) * Time.deltaTime * 360f;
        float g = 0.25f + 0.2f * Mathf.Abs(Mathf.Sin(Time.time * 4f));
        goalGlow.color = KakPalette.WithAlpha(KakPalette.AltinAcik, g); // kale parlar: nereye götüreceğin belli
        Place();
        if (GoalMouth.Contains(ballPos)) Goal();
    }

    void OnDamaged(int hp, Vector3 pos)
    {
        if (Current != State.Dribbling || Carrier == null) return;
        if (((Vector2)pos - (Vector2)Carrier.transform.position).sqrMagnitude > 0.04f) return; // başka oyuncu
        WorldPopup.Show(Loc.T("ball_lost"), Carrier.transform.position + Vector3.up * 0.6f, KakPalette.Sis, 0.8f);
        // top oyuncudan biraz uzağa seker, 0,8 sn alınamaz
        Vector2 away = (ballPos - (Vector2)Carrier.transform.position);
        if (away.sqrMagnitude < 0.0001f) away = Random.insideUnitCircle;
        Vector2 at = ballPos + away.normalized * 0.6f;
        at.x = Mathf.Clamp(at.x, play.xMin + 0.3f, play.xMax - 0.3f);
        at.y = Mathf.Clamp(at.y, play.yMin + 0.3f, play.yMax - 0.3f);
        Drop(at, 6f);
        noPickUntil = Time.time + 0.8f;
    }

    void Goal()
    {
        Goals++;
        var sm = GameManager.Instance != null ? GameManager.Instance.scoreManager : null;
        if (sm != null)
        {
            sm.AddScore(Mathf.RoundToInt(goalScore * sm.ComboMultiplier));
            sm.AddCoins(goalCoins, ballPos);
        }
        WorldPopup.Show(Loc.T("goal_scored"), new Vector3(play.center.x, play.yMax - 1.2f, 0f), KakPalette.AltinAcik, 1.8f);
        var am = AudioManager.Instance;
        if (am != null) am.PlaySfx(am.stageSfx);
        var cam = FindAnyObjectByType<KakCameraShake>();
        if (cam != null) cam.Shake(0.18f, 0.25f);
        Hide();
    }

    void Place()
    {
        if (ball == null) return;
        ball.transform.position = ballPos;
        ball.transform.rotation = Quaternion.Euler(0f, 0f, -roll);
        ring.transform.position = ballPos;
        // 2.5D sıralama (YDepthSorter ile aynı ölçü: 100 − y·10)
        int order = 100 - Mathf.RoundToInt(ballPos.y * 10f);
        ball.sortingOrder = order;
        ring.sortingOrder = order - 1;
    }
}
