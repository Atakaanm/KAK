using UnityEngine;

/// <summary>
/// Haritada çıkan güçlendirmelerin (PowerUp) üzerine eklenecek kod.
/// Oyuncu buna değdiğinde ilgili efekti verip kendini yok eder.
/// Son 3 saniyede yanıp sönerek kaybolacağını haber verir.
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
    // private bool isWarning = false;

    void Start()
    {
        // Collider'ın trigger olduğundan emin ol
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        float remaining = lifetime - timer;

        // Son warningTime saniyesinde yanıp sönme efekti
        if (remaining <= warningTime && remaining > 0f)
        {
            // isWarning = true;

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

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyEffect(other.gameObject);
            Destroy(gameObject);
        }
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
        
        ShowPickupFeedback(transform.position, powerupData.powerupName + "!");
        GameEvents.RaisePowerupCollected(powerupData, transform.position);

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

    private void ShowPickupFeedback(Vector3 position, string text)
    {
        GameObject feedbackObj = new GameObject("PowerupFeedback");
        feedbackObj.transform.position = position + Vector3.up * 0.5f;
        
        TextMesh tm = feedbackObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.characterSize = 0.15f;
        tm.fontSize = 48;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = Color.yellow;
        
        feedbackObj.AddComponent<FloatingText>();
    }
}
