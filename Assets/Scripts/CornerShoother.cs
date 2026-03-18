using UnityEngine;

public class CornerShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform target;
    public float shootInterval = 1.5f;

    public Transform firePoint;
    public SpawnerDirectionAnimator spawnerVisual;

    [Header("Data (opsiyonel)")]
    public SpawnerData spawnerData; // Atanirsa shootInterval ve mermi tipi buradan alinir

    private float timer;

    void Start()
    {
        // Data varsa degerleri oradan al
        if (spawnerData != null)
        {
            shootInterval = spawnerData.shootInterval;

            if (spawnerData.projectileData != null && spawnerData.projectileData.projectilePrefab != null)
            {
                projectilePrefab = spawnerData.projectileData.projectilePrefab;
            }
        }
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

    void Shoot()
    {
        if (projectilePrefab == null || target == null) return;

        if (spawnerVisual != null)
        {
            spawnerVisual.PlayAttackVisual();
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        // 2D duzlemde (Z=0) olmasi sart, aksi halde mermi gorunmez
        spawnPos.z = 0f;

        Vector3 targetPos = target.position;
        targetPos.z = 0f;

        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            Vector2 dir = (targetPos - spawnPos).normalized;
            projectile.moveDirection = dir;

            // ProjectileData bilgilerini mermiye aktar
            if (spawnerData != null && spawnerData.projectileData != null)
            {
                projectile.Init(spawnerData.projectileData);
            }

            // DifficultyManager varsa mermi hizina carpani uygula
            if (DifficultyManager.Instance != null)
            {
                projectile.speed *= DifficultyManager.Instance.GetProjectileSpeedMultiplier();
            }
        }
    }
}