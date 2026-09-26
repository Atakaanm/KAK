using UnityEngine;

/// <summary>
/// Haritada çıkan güçlendirmelerin (PowerUp) üzerine eklenecek kod.
/// Oyuncu buna değdiğinde ilgili efekti verip havuza döner (ProjectilePool, yoksa Destroy).
/// Son 3 saniyede yanıp sönerek kaybolacağını haber verir. Havuzdan her çıkışta OnEnable ile sıfırlanır.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PowerupPickup : MonoBehaviour
{
    public PowerupData powerupData;

    [Header("Ömür Ayarları")]
    public float lifetime = 8f;           // Toplam yerde kalma süresi
    public float warningTime = 3f;        // Son kaç saniye yanıp sönecek

    [Header("Yanıp Sönme Ayarları")]
    public float blinkStartSpeed = 3f;    // Başlangıç blink hızı
    public float blinkEndSpeed = 12f;     // Bitiş blink hızı (giderek hızlanır)

    private float timer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool collected;

    void Awake()
    {
        // Collider'ın trigger olduğundan emin ol
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void OnEnable()
    {
        timer = 0f;
        collected = false;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    void Despawn()
    {
        if (ProjectilePool.Instance != null) ProjectilePool.Instance.Return(gameObject);
        else Destroy(gameObject);
    }

    void Update()
    {
        timer += Time.deltaTime;

        float remaining = lifetime - timer;

        // Son warningTime saniyesinde yanıp sönme efekti
        if (remaining <= warningTime && remaining > 0f)
        {
            if (spriteRenderer != null)
            {
                // Kalan süreye göre hızlanan blink
                float warningProgress = 1f - (remaining / warningTime); // 0 → 1
                float currentBlinkSpeed = Mathf.Lerp(blinkStartSpeed, blinkEndSpeed, warningProgress);

                // Sinüs dalgası ile alpha yanıp sönme
                float alpha = (Mathf.Sin(Time.time * currentBlinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
                // Minimum alpha 0.15 olsun ki tamamen görünmez olmasın
                alpha = Mathf.Lerp(0.15f, 1f, alpha);

                Color c = originalColor;
                c.a = alpha;
                spriteRenderer.color = c;
            }
        }

        if (timer >= lifetime) Despawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Oyuncunun iki collider'ı var (ayak izi + gövde): aynı adımda ikisi de değebilir, bir kez topla
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        ApplyEffect(other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject);
        Despawn();
    }

    private void ApplyEffect(GameObject player)
    {
        if (powerupData == null) return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerMovement2D movement = player.GetComponent<PlayerMovement2D>();

        // PlayerData içerisinde tanımladığımız katsayı
        float durationMultiplier = 1.0f;
        if (movement != null && movement.playerData != null)
        {
            durationMultiplier = movement.playerData.powerupDurationMultiplier;
        }

        float finalDuration = powerupData.duration * durationMultiplier;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayScoreSfx(); // Geçici powerup sesi
        
        GameEvents.RaisePowerupCollected(powerupData, transform.position);
        SaveSystem.Data.totalPowerups++;

        switch (powerupData.type)
        {
            case PowerupType.Heal:
                if (health != null) health.Heal(powerupData.healthAmount);
                break;

            case PowerupType.Shield:
                if (health != null) health.ActivateShield();
                break;

            case PowerupType.SpeedBoost:
                if (movement != null) movement.ApplySpeedBoost(powerupData.powerMultiplier, finalDuration);
                break;

            case PowerupType.Ghost:
                if (health != null) health.MakeGhost(finalDuration);
                break;

            case PowerupType.TimeSlow:
                if (GameManager.Instance != null) GameManager.Instance.TimeSlow(powerupData.powerMultiplier, finalDuration);
                break;
        }
    }

}
