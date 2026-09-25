using UnityEngine;

/// <summary>
/// Köşe fırlatıcısı: aralıklarla oyuncuya taş atar.
/// Taş türü: zorluk kademesinin listesinden (DifficultyStageData.availableProjectiles), yoksa kendi verisi.
/// Meteor türü taşlar oyuncunun olduğu yere gökten düşer (fırlatıcı yukarı fırlatır).
/// </summary>
public class CornerShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform target;
    public float shootInterval = 1.5f;

    public Transform firePoint;
    public SpawnerDirectionAnimator spawnerVisual;

    [Header("Data (opsiyonel)")]
    public SpawnerData spawnerData; // Atanirsa shootInterval ve mermi tipi buradan alinir

    [Header("Pool Ayarları")]
    [Tooltip("Başlangıçta kaç mermi pool'a eklensin")]
    public int prewarmCount = 5;

    [Header("Uyarı (telegraph)")]
    [Tooltip("Atıştan önce fırlatıcının sıcak renkle parladığı süre (sn)")]
    public float telegraphTime = 0.35f;
    private SpriteRenderer visualRenderer;
    private Color visualBaseColor = Color.white;

    [Header("Meteor")]
    [Tooltip("Meteor hedefinin oyuncu etrafındaki rastgele sapması")]
    public float meteorScatter = 0.9f;

    private float timer;

    void Start()
    {
        if (spawnerData != null)
        {
            shootInterval = spawnerData.shootInterval;
            if (spawnerData.projectileData != null && spawnerData.projectileData.projectilePrefab != null)
                projectilePrefab = spawnerData.projectileData.projectilePrefab;
        }

        if (target == null && Projectile.PlayerTarget != null) target = Projectile.PlayerTarget;
        if (spawnerVisual != null) visualRenderer = spawnerVisual.GetComponent<SpriteRenderer>();
        if (visualRenderer != null) visualBaseColor = visualRenderer.color;

        if (projectilePrefab != null && ProjectilePool.Instance != null)
            ProjectilePool.Instance.Prewarm(projectilePrefab, prewarmCount);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Atıştan önce sıcak parlama: oyuncu nereden taş geleceğini okur
        if (visualRenderer != null)
        {
            float t = shootInterval - timer;
            if (t < telegraphTime)
            {
                float k = 1f - Mathf.Clamp01(t / telegraphTime);
                visualRenderer.color = Color.Lerp(visualBaseColor, KakPalette.Turuncu, k * 0.75f);
            }
            else if (visualRenderer.color != visualBaseColor) visualRenderer.color = visualBaseColor;
        }

        if (timer >= shootInterval)
        {
            Shoot();
            timer = 0f;
        }
    }

    void OnDisable()
    {
        if (visualRenderer != null) visualRenderer.color = visualBaseColor;
    }

    /// <summary>Zamanlayıcıdan bağımsız hemen ateş (olaylar). angleOffset: hedefe göre sapma (derece).</summary>
    public void FireNow(float angleOffset = 0f)
    {
        Shoot(angleOffset);
        timer = 0f;
    }

    ProjectileData PickData()
    {
        ProjectileData d = DifficultyManager.Instance != null ? DifficultyManager.Instance.PickProjectile() : null;
        if (d == null && spawnerData != null) d = spawnerData.projectileData;
        return d;
    }

    void Shoot(float angleOffset = 0f)
    {
        if (target == null) target = Projectile.PlayerTarget;
        if (projectilePrefab == null || target == null) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSfx();
        if (spawnerVisual != null) spawnerVisual.PlayAttackVisual();

        ProjectileData data = PickData();
        GameObject prefab = data != null && data.projectilePrefab != null ? data.projectilePrefab : projectilePrefab;

        float speedMult = 1f, scaleMult = 1f;
        if (DifficultyManager.Instance != null)
        {
            speedMult = DifficultyManager.Instance.GetProjectileSpeedMultiplier();
            scaleMult = DifficultyManager.Instance.GetProjectileScaleMultiplier();
        }

        if (data != null && data.motion == ProjectileMotion.Meteor)
        {
            Vector3 ground = target.position + (Vector3)(Random.insideUnitCircle * meteorScatter);
            Projectile.LaunchMeteor(prefab, data, ground, scaleMult);
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = target.position;
        Vector2 dir = (Vector2)(targetPos - spawnPos);
        if (Mathf.Abs(angleOffset) > 0.01f) dir = Quaternion.Euler(0f, 0f, angleOffset) * dir;
        Projectile.Launch(prefab, data, spawnPos, dir, speedMult, scaleMult);
    }
}
