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

        if (projectilePrefab != null && ProjectilePool.Instance != null)
            ProjectilePool.Instance.Prewarm(projectilePrefab, prewarmCount);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= shootInterval)
        {
            Shoot();
            timer = 0f;
        }
    }

    ProjectileData PickData()
    {
        ProjectileData d = DifficultyManager.Instance != null ? DifficultyManager.Instance.PickProjectile() : null;
        if (d == null && spawnerData != null) d = spawnerData.projectileData;
        return d;
    }

    void Shoot()
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
        Projectile.Launch(prefab, data, spawnPos, (Vector2)(targetPos - spawnPos), speedMult, scaleMult);
    }
}
