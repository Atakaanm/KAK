using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// G7 (Faz 11): Buz dünyası kurulumu (LevelManager.ApplyTheme çağırır): oyunculara dikey hız çarpanı ve soğuk göstergesi,
/// HUD'da soğuk çubuğu, hafif kar yağışı, ilk girişte kısa uyarı. Tema: WorldTheme (verticalSpeedMultiplier, coldEnabled...).
/// </summary>
public static class IceWorld
{
    public const string SeenKey = "intro_ice";

    public static void Apply(WorldTheme t)
    {
        foreach (var mv in Object.FindObjectsByType<PlayerMovement2D>(FindObjectsSortMode.None))
        {
            mv.verticalSpeedMultiplier = t.verticalSpeedMultiplier;
            if (!t.coldEnabled) continue;
            var cm = mv.GetComponent<ColdMeter>();
            if (cm == null) cm = mv.gameObject.AddComponent<ColdMeter>();
            cm.coldSeconds = t.coldSeconds;
        }
        if (t.coldEnabled)
        {
            // Faz 12 H1: iki kişilikte her oyuncunun kendi soğuk çubuğu, kalp satırının yanında (tek çubuk "en çok
            // üşüyeni" gösteriyordu: Ada'nın satırında durduğu için "sadece Ada'da var", Ata ateş alınca azalmıyor sanıldı)
            bool perPlayer = false;
            if (GameSettings.TwoPlayer && PlayerRegistry.All.Count > 1)
                foreach (var ph in PlayerRegistry.All)
                {
                    var cm = ph != null ? ph.GetComponent<ColdMeter>() : null;
                    if (cm != null && ph.healthUI != null && ColdHud.CreateFor(cm, (RectTransform)ph.healthUI.transform) != null) perPlayer = true;
                }
            if (!perPlayer) ColdHud.Create();
            FireDrops.Create();
        }
        if (t.snowfall) Snowfall.Create();
        if (t.coldEnabled && !SaveSystem.Data.HasSeen(SeenKey))
        {
            SaveSystem.Data.MarkSeen(SeenKey);
            var arena = Object.FindAnyObjectByType<ArenaAutoLayout>();
            Vector3 c = arena != null ? (Vector3)arena.PlayableWorldRect.center : Vector3.zero;
            WorldPopup.Show(Loc.T("ice_intro"), c + Vector3.up * 1.2f, KakPalette.CamgobegiParlak, 1.1f);
        }
    }
}

/// <summary>
/// G7: HUD'da soğuk çubuğu (kar tanesi + dolum); yarıdan sonra yanıp söner. Tek kişilikte HUD bandının altında
/// (en çok üşüyen oyuncu), iki kişilikte her oyuncunun kalp satırının sağında kendi çubuğu (Faz 12 H1).
/// </summary>
public class ColdHud : MonoBehaviour
{
    Image fill;
    /// <summary>Gösterilen oyuncu (null: en çok üşüyen).</summary>
    public ColdMeter target;

    public static ColdHud Create()
    {
        var composer = Object.FindAnyObjectByType<ScreenComposer>();
        var parent = composer != null && composer.hudContent != null ? composer.hudContent : null;
        if (parent == null || parent.GetComponentInChildren<ColdHud>() != null) return null;
        return Build(parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-20f, 46f), null);
    }

    /// <summary>İki kişilik: oyuncunun kalp satırının sağına kendi çubuğu.</summary>
    public static ColdHud CreateFor(ColdMeter meter, RectTransform heartRow)
    {
        if (meter == null || heartRow == null || heartRow.GetComponentInChildren<ColdHud>() != null) return null;
        return Build(heartRow, new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f), meter);
    }

    static ColdHud Build(RectTransform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, ColdMeter meter)
    {
        var white = Resources.Load<Sprite>("UiWhite");
        var go = new GameObject("ColdHud", typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(320f, 40f);

        var icon = NewImage("Icon", rt, Resources.Load<Sprite>("Snowflake"), new Vector2(-150f, 0f), new Vector2(52f, 52f));
        icon.color = Color.white;
        var bg = NewImage("Bar", rt, white, new Vector2(20f, 0f), new Vector2(260f, 22f));
        bg.color = KakPalette.Gece;
        var f = NewImage("Fill", bg.rectTransform, white, Vector2.zero, new Vector2(252f, 14f));
        f.type = Image.Type.Filled; f.fillMethod = Image.FillMethod.Horizontal; f.fillAmount = 0f;
        var hud = go.AddComponent<ColdHud>();
        hud.fill = f;
        hud.target = meter;
        return hud;
    }

    static Image NewImage(string name, RectTransform parent, Sprite s, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.sprite = s; img.raycastTarget = false; img.preserveAspect = s != null && name == "Icon";
        return img;
    }

    void Update()
    {
        if (fill == null) return;
        float v = target != null ? target.Value : ColdMeter.MaxValue();
        fill.fillAmount = v;
        Color c = Color.Lerp(KakPalette.CamgobegiParlak, Color.white, v);
        if (v >= 0.5f) c.a = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * Mathf.Lerp(3f, 10f, v)));
        fill.color = c;
    }
}

/// <summary>G7: hafif kar yağışı (tek parçacık sistemi, en fazla 70 parçacık; performans bütçesi).</summary>
public static class Snowfall
{
    public static void Create()
    {
        if (Object.FindAnyObjectByType<SnowfallMarker>() != null) return;
        var arena = Object.FindAnyObjectByType<ArenaAutoLayout>();
        if (arena == null) return;
        Rect r = arena.PlayableWorldRect;
        var go = new GameObject("Snowfall");
        go.AddComponent<SnowfallMarker>();
        go.transform.position = new Vector3(r.center.x, r.yMax + 0.5f, 0f);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 7f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.06f);
        main.startColor = new Color(1f, 1f, 1f, 0.75f);
        main.maxParticles = 70;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.prewarm = true;
        var em = ps.emission; em.rateOverTime = 9f;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(r.width + 1f, 0.1f, 0.1f);
        sh.rotation = new Vector3(90f, 0f, 0f);
        // Hız eğrileri aynı modda olmalı (yoksa her karede hata): üçü de "iki sabit arası"
        var vel = ps.velocityOverLifetime; vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
        vel.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        var rend = go.GetComponent<ParticleSystemRenderer>();
        var fb = Object.FindAnyObjectByType<FeedbackManager>();
        if (fb != null && fb.chips != null) rend.sharedMaterial = fb.chips.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        rend.sortingOrder = 40;
        ps.Play();
    }
}

public class SnowfallMarker : MonoBehaviour { }

/// <summary>
/// G7: garantili ateş — soğuk dünyada ısınma şansı rastgele eşya çıkışına kalmasın. Belirli aralıklarla (sahada ateş yoksa)
/// bir ateş doğurur; oyuncular üşüdükçe sıklaşır.
/// </summary>
public class FireDrops : MonoBehaviour
{
    public float firstDelay = 9f;
    public float intervalWarm = 16f;
    public float intervalCold = 8f;
    PowerupData fire;
    PowerupSpawner spawner;
    float next;

    public static void Create()
    {
        if (Object.FindAnyObjectByType<FireDrops>() != null) return;
        var go = new GameObject("FireDrops");
        go.AddComponent<FireDrops>();
    }

    void Start()
    {
        spawner = Object.FindAnyObjectByType<PowerupSpawner>();
        if (GameSettings.TwoPlayer) // Faz 12 H1: iki kişide ateş daha sık
        {
            firstDelay /= TwoPlayerMode.ItemRate;
            intervalWarm /= TwoPlayerMode.ItemRate;
            intervalCold /= TwoPlayerMode.ItemRate;
        }
        var level = LevelManager.Instance != null ? LevelManager.Instance.currentLevel : null;
        if (level != null && level.availablePowerups != null)
            foreach (var p in level.availablePowerups) if (p != null && p.type == PowerupType.Fire) fire = p;
        next = Time.time + firstDelay;
    }

    void Update()
    {
        if (fire == null || spawner == null || Time.time < next) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        float cold = ColdMeter.MaxValue();
        next = Time.time + Mathf.Lerp(intervalWarm, intervalCold, cold);
        if (FireOnField()) return;
        spawner.SpawnSpecific(fire);
    }

    static bool FireOnField()
    {
        foreach (var p in Object.FindObjectsByType<PowerupPickup>(FindObjectsSortMode.None))
            if (p.powerupData != null && p.powerupData.type == PowerupType.Fire && p.gameObject.activeInHierarchy) return true;
        return false;
    }
}
