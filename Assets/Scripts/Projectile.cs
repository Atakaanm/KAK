using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 1.5f;
    public Vector2 moveDirection;
    public float lifeTime = 5f;
    public float rotationSpeed;
    public int damage = 1;

    void Start()
    {
        if (Mathf.Abs(rotationSpeed) < 0.01f)
        {
            rotationSpeed = Random.Range(-360f, 360f);
        }

        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// ProjectileData'dan degerleri alarak mermiye uygular.
    /// CornerShooter tarafindan Instantiate sonrasi cagrilir.
    /// </summary>
    public void Init(ProjectileData data)
    {
        if (data == null) return;

        speed = data.speed;
        damage = data.damage;
        lifeTime = data.lifeTime;

        // rotationSpeed 0 ise rastgele doner, buyuk sifirdan farkli yoksa datadaki deger kullanilir
        if (Mathf.Abs(data.rotationSpeed) > 0.01f)
        {
            rotationSpeed = data.rotationSpeed;
        }

        // Sprite degistrme (opsiyonel)
        if (data.projectileSprite != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = data.projectileSprite;
        }
    }

    void Update()
    {
        transform.position += (Vector3)(moveDirection.normalized * speed * Time.deltaTime);
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}