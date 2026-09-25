using System.Collections;
using UnityEngine;

/// <summary>
/// Sonsuz Mod olayları: belirli aralıklarla ritmi değiştiren kısa bölümler.
///   Taş Yağmuru (gökten taşlar) · Çapraz Ateş (tüm fırlatıcılar birlikte) ·
///   Sessizlik (3 sn ateş yok → yoğun salvo) · Yuvarlanan Kaya (şerit uyarısı → arenayı tarayan kaya)
/// Kademeye göre açılır, her olay başında GameEvents.EndlessEventStarted (banner + sarsıntı).
/// </summary>
public class EndlessEventManager : MonoBehaviour
{
    public static EndlessEventManager Instance { get; private set; }

    [Header("Zamanlama (oyun sn)")]
    public float firstEventAt = 30f;
    public float intervalMin = 30f;
    public float intervalMax = 45f;

    [Header("Veriler")]
    public GameObject projectilePrefab;
    public ProjectileData meteorData;
    public ProjectileData boulderData;
    public ProjectileData rockData;
    public SpriteRenderer laneWarning;   // yuvarlanan kaya şerit uyarısı

    [Header("Açılış kademeleri")]
    public int meteorShowerStage = 1;
    public int crossfireStage = 1;
    public int calmStage = 2;
    public int rollingStage = 3;

    public bool Running { get; private set; }
    public string CurrentEvent { get; private set; }

    float nextAt;
    ArenaAutoLayout arena;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        arena = FindAnyObjectByType<ArenaAutoLayout>();
        nextAt = firstEventAt;
        if (laneWarning != null) laneWarning.enabled = false;
    }

    float Elapsed => GameManager.Instance != null && GameManager.Instance.scoreManager != null
        ? GameManager.Instance.scoreManager.ElapsedSeconds : Time.timeSinceLevelLoad;

    int Stage => DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentStageIndex : 0;
    bool Over => GameManager.Instance != null && GameManager.Instance.IsGameOver;

    void Update()
    {
        if (Running || Over || LevelManager.Instance == null) return;
        if (Elapsed < nextAt) return;
        nextAt = Elapsed + Random.Range(intervalMin, intervalMax);
        StartRandomEvent();
    }

    public void StartRandomEvent()
    {
        int stage = Stage;
        var options = new System.Collections.Generic.List<int>(4);
        if (stage >= meteorShowerStage) options.Add(0);
        if (stage >= crossfireStage) options.Add(1);
        if (stage >= calmStage) options.Add(2);
        if (stage >= rollingStage) options.Add(3);
        if (options.Count == 0) return;
        StartEvent(options[Random.Range(0, options.Count)]);
    }

    /// <summary>0 Taş Yağmuru, 1 Çapraz Ateş, 2 Sessizlik, 3 Yuvarlanan Kaya</summary>
    public void StartEvent(int id)
    {
        if (Running) return;
        switch (id)
        {
            case 0: StartCoroutine(Run("TAŞ YAĞMURU!", MeteorShower())); break;
            case 1: StartCoroutine(Run("ÇAPRAZ ATEŞ!", Crossfire())); break;
            case 2: StartCoroutine(Run("SESSİZLİK...", Calm())); break;
            case 3: StartCoroutine(Run("YUVARLANAN KAYA!", Rolling())); break;
        }
    }

    IEnumerator Run(string title, IEnumerator body)
    {
        Running = true;
        CurrentEvent = title;
        GameEvents.RaiseEndlessEventStarted(title);
        yield return body;
        CurrentEvent = null;
        Running = false;
    }

    Rect Play => arena != null ? arena.PlayableWorldRect : new Rect(-3f, 1f, 6f, 6f);

    IEnumerator MeteorShower()
    {
        yield return new WaitForSeconds(1f);
        float end = Time.time + 5f;
        float nearTimer = 0f;
        while (Time.time < end && !Over)
        {
            Rect r = Play;
            Vector3 pos;
            nearTimer -= 0.35f;
            if (nearTimer <= 0f && Projectile.PlayerTarget != null)
            {
                pos = Projectile.PlayerTarget.position + (Vector3)(Random.insideUnitCircle * 1.2f);
                nearTimer = 1.1f;
            }
            else pos = new Vector3(Random.Range(r.xMin + 0.4f, r.xMax - 0.4f), Random.Range(r.yMin + 0.4f, r.yMax - 0.4f), 0f);
            pos.x = Mathf.Clamp(pos.x, r.xMin + 0.3f, r.xMax - 0.3f);
            pos.y = Mathf.Clamp(pos.y, r.yMin + 0.3f, r.yMax - 0.3f);
            Projectile.LaunchMeteor(projectilePrefab, meteorData, pos, ScaleMult);
            yield return new WaitForSeconds(0.35f);
        }
    }

    IEnumerator Crossfire()
    {
        var shooters = LevelManager.Instance.spawners;
        var wasActive = new bool[shooters.Length];
        for (int i = 0; i < shooters.Length; i++) { wasActive[i] = shooters[i].gameObject.activeSelf; shooters[i].gameObject.SetActive(true); shooters[i].enabled = false; }
        yield return new WaitForSeconds(0.8f);
        for (int v = 0; v < 3 && !Over; v++)
        {
            foreach (var s in shooters) if (s != null) s.FireNow();
            yield return new WaitForSeconds(0.9f);
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < shooters.Length; i++) { shooters[i].enabled = true; shooters[i].gameObject.SetActive(wasActive[i]); }
        if (DifficultyManager.Instance != null) DifficultyManager.Instance.RefreshSpawnerActivation();
    }

    IEnumerator Calm()
    {
        var shooters = LevelManager.Instance.spawners;
        foreach (var s in shooters) if (s != null) s.enabled = false;
        yield return new WaitForSeconds(3f);
        if (!Over)
        {
            for (int i = 0; i < shooters.Length; i++)
            {
                if (shooters[i] == null) continue;
                bool was = shooters[i].gameObject.activeSelf;
                shooters[i].gameObject.SetActive(true);
                shooters[i].FireNow();
                shooters[i].FireNow(12f);
                shooters[i].FireNow(-12f);
                shooters[i].gameObject.SetActive(was);
            }
        }
        foreach (var s in shooters) if (s != null) s.enabled = true;
        if (DifficultyManager.Instance != null) DifficultyManager.Instance.RefreshSpawnerActivation();
        yield return new WaitForSeconds(1f);
    }

    IEnumerator Rolling()
    {
        for (int n = 0; n < 2 && !Over; n++)
        {
            Rect r = Play;
            // Şerit: oyuncunun yakınından geçen yatay bir hat
            float y = Projectile.PlayerTarget != null ? Mathf.Clamp(Projectile.PlayerTarget.position.y + Random.Range(-0.6f, 0.6f), r.yMin + 0.6f, r.yMax - 0.6f)
                                                      : Random.Range(r.yMin + 0.6f, r.yMax - 0.6f);
            bool fromLeft = Random.value < 0.5f;
            if (laneWarning != null)
            {
                laneWarning.transform.position = new Vector3(r.center.x, y, 0f);
                laneWarning.size = new Vector2(r.width, 0.9f);
                laneWarning.enabled = true;
            }
            float warnEnd = Time.time + 1.1f;
            while (Time.time < warnEnd)
            {
                if (laneWarning != null)
                {
                    Color c = KakPalette.Tehlike; c.a = 0.18f + 0.18f * Mathf.Abs(Mathf.Sin(Time.time * 14f));
                    laneWarning.color = c;
                }
                yield return null;
            }
            if (laneWarning != null) laneWarning.enabled = false;
            if (Over) yield break;
            Vector3 from = new Vector3(fromLeft ? r.xMin - 0.1f : r.xMax + 0.1f, y, 0f);
            Projectile.Launch(projectilePrefab, boulderData, from, fromLeft ? Vector2.right : Vector2.left, 2.4f, 1.3f);
            yield return new WaitForSeconds(1.4f);
        }
    }

    float ScaleMult => DifficultyManager.Instance != null ? DifficultyManager.Instance.GetProjectileScaleMultiplier() : 1f;
}
