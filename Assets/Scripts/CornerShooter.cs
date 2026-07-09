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

    [Header("Pool Ayarları")]
    [Tooltip("Başlangıçta kaç mermi pool'a eklensin")]
    public int prewarmCount = 5;

    private float timer;
    // Prefabın orijinal scale'i — DifficultyManager çarpanı buna uygulanır
    private Vector3 prefabOriginalScale = Vector3.one;

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

        // Pool'u önceden ısıt (sahne yüklenir yüklenmez hazır olsun)
        if (projectilePrefab != null && ProjectilePool.Instance != null)
        {
            // Prefabın orijinal scale'ini kaydet
            prefabOriginalScale = projectilePrefab.transform.localScale;
            ProjectilePool.Instance.Prewarm(projectilePrefab, prewarmCount);
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

        if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSfx();

        if (spawnerVisual != null)
        {
            spawnerVisual.PlayAttackVisual();
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        spawnPos.z = 0f;

        Vector3 targetPos = target.position;
        targetPos.z = 0f;

        // --- POOL'DAN AL (Instantiate yerine) ---
        GameObject projectileObj;
        if (ProjectilePool.Instance != null)
        {
            projectileObj = ProjectilePool.Instance.Get(projectilePrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Pool yoksa güvenli fallback: normal Instantiate
            projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        }

        if (projectileObj == null) return;

        Projectile projectile = projectileObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            Vector2 dir = (targetPos - spawnPos).normalized;
            projectile.moveDirection = dir;

            // Reset durumu (pool'dan gelen eski mermi temizlensin)
            projectile.ResetState();

            // ProjectileData bilgilerini mermiye aktar
            if (spawnerData != null && spawnerData.projectileData != null)
            {
                projectile.Init(spawnerData.projectileData);
            }

            // DifficultyManager varsa mermi hizina ve boyutuna carpani uygula
            if (DifficultyManager.Instance != null)
            {
                projectile.speed *= DifficultyManager.Instance.GetProjectileSpeedMultiplier();
                // Scale: prefabın orijinal boyutunu baz al (birikimsiz)
                projectile.transform.localScale = prefabOriginalScale * DifficultyManager.Instance.GetProjectileScaleMultiplier();
            }
        }
    }
}