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
    public PlayerData playerData; // Atanirsa maxHealth ve invincibility buradan alinir

    private Color originalColor;
    private bool isInvincible = false;
    public float invincibilityDuration = 0.35f;

    void Start()
    {
        // Data varsa degerleri oradan al
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

        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;

        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        #endif

        if (playerSpriteRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(HitFlashRoutine());
        }

        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            if (healthUI != null)
            {
                healthUI.UpdateHearts(currentHealth);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }

        StartCoroutine(InvincibilityRoutine());
    }

    IEnumerator HitFlashRoutine()
    {
        playerSpriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        playerSpriteRenderer.color = originalColor;
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}