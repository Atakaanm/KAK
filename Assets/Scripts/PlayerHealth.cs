using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    public float hitFlashDuration = 0.12f;
    public Color hitColor = Color.red;

    public SpriteRenderer playerSpriteRenderer;
    public HealthUI healthUI;

    [Header("Data (opsiyonel)")]
    public PlayerData playerData;

    private Color originalColor;
    private Vector3 originalScale;
    private bool isInvincible = false;
    private bool hasShield = false;
    private bool isGhost = false;
    public bool IsGhost => isGhost;
    private bool isDead = false;
    public bool IsDead => isDead;
    public float invincibilityDuration = 0.35f;

    private Coroutine hitFlashCoroutine;
    private Coroutine invincibilityCoroutine;
    private Coroutine ghostCoroutine;

    void Start()
    {
        if (playerData != null)
        {
            maxHealth = playerData.maxHealth;
            invincibilityDuration = playerData.invincibilityDuration;
        }

        currentHealth = maxHealth;

        if (playerSpriteRenderer != null)
        {
            originalColor = playerSpriteRenderer.color;
        }
        originalScale = transform.localScale;

        if (healthUI != null)
        {
            healthUI.InitHearts(maxHealth);
            healthUI.UpdateHearts(currentHealth);
        }
    }

    /// <summary>Can verir (Max canı geçemez).</summary>
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }
    }

    /// <summary>1 vuruşluk bloklayan kalkan verir.</summary>
    public void ActivateShield()
    {
        hasShield = true;
        transform.localScale = originalScale * 1.15f; // %15 büyüt
        if (playerSpriteRenderer != null && !isGhost)
        {
            playerSpriteRenderer.color = Color.cyan;
        }
    }

    /// <summary>Mermilerin içinden geçmesini sağlayan hayalet formu.</summary>
    public void MakeGhost(float duration)
    {
        if (ghostCoroutine != null) StopCoroutine(ghostCoroutine);
        ghostCoroutine = StartCoroutine(GhostRoutine(duration));
    }

    IEnumerator GhostRoutine(float duration)
    {
        isGhost = true;
        if (playerSpriteRenderer != null)
        {
            Color c = originalColor;
            c.a = 0.5f;
            playerSpriteRenderer.color = c;
        }

        yield return KakTime.WaitGameplay(duration);

        isGhost = false;
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = hasShield ? Color.cyan : originalColor;
        }
        ghostCoroutine = null;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>Geliştirici ölümsüzlüğü (ekran görüntüleri, görsel testler). Release build'de yok.</summary>
    public static bool DevGodMode;
#endif

    public void TakeDamage(int damage)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (DevGodMode) return;
#endif
        if (isDead || isInvincible || isGhost) return;

        if (hasShield)
        {
            GameEvents.RaiseShieldBlocked(transform.position);
            hasShield = false;
            transform.localScale = originalScale;
            if (playerSpriteRenderer != null) playerSpriteRenderer.color = originalColor;
            if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
            return;
        }

        currentHealth -= damage;
        GameEvents.RaisePlayerDamaged(Mathf.Max(0, currentHealth), transform.position);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHitSfx();
            AudioManager.Instance.TriggerVibration();
        }

        if (playerSpriteRenderer != null)
        {
            if (hitFlashCoroutine != null) StopCoroutine(hitFlashCoroutine);
            hitFlashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;

            if (healthUI != null)
            {
                healthUI.UpdateHearts(currentHealth);
            }

            // Oyun sonu ekranında oyuncu görünür kalsın: yanıp sönme/flaş coroutine'leri
            // timeScale = 0'da donar, sprite gizli kalabilir.
            if (invincibilityCoroutine != null) { StopCoroutine(invincibilityCoroutine); invincibilityCoroutine = null; }
            if (hitFlashCoroutine != null) { StopCoroutine(hitFlashCoroutine); hitFlashCoroutine = null; }
            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.enabled = true;
                playerSpriteRenderer.color = hitColor;
            }

            GameEvents.RaisePlayerDied(transform.position);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            return;
        }

        if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
        invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
    }

    IEnumerator HitFlashRoutine()
    {
        playerSpriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        
        // Rengi mevcut duruma göre doğru geri yükle
        if (isGhost)
        {
            Color c = originalColor; c.a = 0.5f;
            playerSpriteRenderer.color = c;
        }
        else if (hasShield)
        {
            playerSpriteRenderer.color = Color.cyan;
        }
        else
        {
            playerSpriteRenderer.color = originalColor;
        }
        hitFlashCoroutine = null;
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            if (playerSpriteRenderer != null)
                playerSpriteRenderer.enabled = !playerSpriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        if (playerSpriteRenderer != null) playerSpriteRenderer.enabled = true;
        isInvincible = false;
        
        // Renk çakışmasını çöz
        if (playerSpriteRenderer != null)
        {
            if (isGhost)
            {
                Color c = originalColor; c.a = 0.5f;
                playerSpriteRenderer.color = c;
            }
            else if (hasShield)
            {
                playerSpriteRenderer.color = Color.cyan;
            }
        }
        invincibilityCoroutine = null;
    }
}