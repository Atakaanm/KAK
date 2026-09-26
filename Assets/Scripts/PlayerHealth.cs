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
    private bool isInvincible = false;
    private bool hasShield = false;
    public bool HasShield => hasShield;
    private ShieldBubble shieldBubble;
    private bool isGhost = false;
    public bool IsGhost => isGhost;
    private bool isDead = false;
    public bool IsDead => isDead;
    public float invincibilityDuration = 0.35f;

    private Coroutine hitFlashCoroutine;
    private Coroutine invincibilityCoroutine;
    private Coroutine ghostCoroutine;

    static readonly WaitForSeconds BlinkWait = new WaitForSeconds(0.1f);
    const float GhostAlpha = 0.5f;

    void OnEnable() { Projectile.PlayerTarget = transform; }
    void OnDisable() { if (Projectile.PlayerTarget == transform) Projectile.PlayerTarget = null; }

    /// <summary>Son vuruşun kaynağı (denge analizi ve ileride ölüm ekranı: "Göktaşı seni yakaladı").</summary>
    public static string LastHitSource = "";

    private float invulnerableUntil = -1f;
    /// <summary>Geçici ölümsüzlük (dash vb.). Oyun zamanıyla ölçülür.</summary>
    public void SetInvulnerable(float seconds) { invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds); }
    public bool IsInvulnerable => Time.time < invulnerableUntil;

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

        if (healthUI != null)
        {
            healthUI.InitHearts(maxHealth);
            healthUI.UpdateHearts(currentHealth);
        }
    }

    /// <summary>Karakter canını ayarlar ve doldurur (LevelManager; Start sırasından bağımsız).</summary>
    public void SetMaxHealth(int max)
    {
        maxHealth = Mathf.Max(1, max);
        currentHealth = maxHealth;
        if (healthUI != null)
        {
            healthUI.InitHearts(maxHealth);
            healthUI.UpdateHearts(currentHealth);
        }
    }

    /// <summary>Reklamla canlanma: ölü durumdan çıkar, can verir, kısa süre dokunulmaz (taşların içinden çıkabilsin).</summary>
    public void Revive(int health, float invulnerableSeconds)
    {
        isDead = false;
        isInvincible = false;
        currentHealth = Mathf.Clamp(health, 1, maxHealth);
        if (hitFlashCoroutine != null) { StopCoroutine(hitFlashCoroutine); hitFlashCoroutine = null; }
        if (invincibilityCoroutine != null) { StopCoroutine(invincibilityCoroutine); invincibilityCoroutine = null; }
        if (playerSpriteRenderer != null) playerSpriteRenderer.enabled = true;
        RefreshTint();
        if (healthUI != null) healthUI.UpdateHearts(currentHealth);
        SetInvulnerable(invulnerableSeconds);
        invincibilityCoroutine = StartCoroutine(InvincibilityBlink(invulnerableSeconds));
    }

    IEnumerator InvincibilityBlink(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (playerSpriteRenderer != null) playerSpriteRenderer.enabled = !playerSpriteRenderer.enabled;
            yield return BlinkWait;
            t += 0.1f;
        }
        if (playerSpriteRenderer != null) playerSpriteRenderer.enabled = true;
        invincibilityCoroutine = null;
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
        // Karakteri boyamak ya da büyütmek yerine balon: çarpışma alanı değişmez, karakter okunur kalır
        if (shieldBubble == null)
        {
            var hb = GetComponent<PlayerHitbox>();
            shieldBubble = ShieldBubble.Create(transform, playerSpriteRenderer, hb != null ? hb.hurtOffsetY : 0f);
        }
        shieldBubble.Show();
    }

    /// <summary>Görsel rengi duruma göre yeniden kurar (hayalet yarı saydam, aksi halde orijinal).</summary>
    void RefreshTint()
    {
        if (playerSpriteRenderer == null) return;
        Color c = originalColor;
        if (isGhost) c.a = GhostAlpha;
        playerSpriteRenderer.color = c;
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
        RefreshTint();

        yield return KakTime.WaitGameplay(duration);

        isGhost = false;
        if (hitFlashCoroutine == null) RefreshTint();
        ghostCoroutine = null;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>Geliştirici ölümsüzlüğü (ekran görüntüleri, görsel testler). Release build'de yok.</summary>
    public static bool DevGodMode;
#endif

    /// <summary>Kalkan varsa bir vuruşu engelleyip tüketir (durum efektleri için). Engellendiyse true.</summary>
    public bool ConsumeShield()
    {
        if (!hasShield) return false;
        GameEvents.RaiseShieldBlocked(transform.position);
        hasShield = false;
        if (shieldBubble != null) shieldBubble.Pop();
        if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
        invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
        return true;
    }

    public void TakeDamage(int damage)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (DevGodMode) return;
#endif
        if (isDead || isInvincible || isGhost || IsInvulnerable) return;

        if (ConsumeShield()) return;

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
        hitFlashCoroutine = null;
        RefreshTint();
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            if (playerSpriteRenderer != null)
                playerSpriteRenderer.enabled = !playerSpriteRenderer.enabled;
            yield return BlinkWait;
            elapsed += 0.1f;
        }
        if (playerSpriteRenderer != null) playerSpriteRenderer.enabled = true;
        isInvincible = false;
        if (hitFlashCoroutine == null) RefreshTint();
        invincibilityCoroutine = null;
    }
}