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
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    /// <summary>G3: karakterin ulaşabileceği en fazla can (oyun içi kalp toplama bu sınıra kadar büyütür).</summary>
    [System.NonSerialized] public int HealthCap = 3;
    public float invincibilityDuration = 0.35f;

    private Coroutine hitFlashCoroutine;
    private Coroutine invincibilityCoroutine;
    private Coroutine ghostCoroutine;

    static readonly WaitForSeconds BlinkWait = new WaitForSeconds(0.1f);
    const float GhostAlpha = 0.5f;

    void OnEnable()
    {
        if (Projectile.PlayerTarget == null || !Projectile.PlayerTarget.gameObject.activeInHierarchy) Projectile.PlayerTarget = transform;
        PlayerRegistry.Register(this);
    }
    void OnDisable()
    {
        if (Projectile.PlayerTarget == transform) Projectile.PlayerTarget = null;
        PlayerRegistry.Unregister(this);
    }

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
            maxHealth = CharacterProgress.Hearts(playerData); // G3
            HealthCap = playerData.maxHealth;
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

    /// <summary>G5: dönüşte kullanılacak başlangıç canı (LevelManager ayarlar).</summary>
    [System.NonSerialized] public int StartHearts = 1;
    /// <summary>G5: düşmüşken dönüşe kalan saniye (HUD sayacı); 0 = düşmüş değil.</summary>
    [System.NonSerialized] public float RespawnRemaining;

    /// <summary>G5: iki kişilikte düşmüş oyuncu yarı saydam ve gri; gövdesi taşlara çarpmaz.</summary>
    public void SetDownVisual(bool down)
    {
        if (playerSpriteRenderer != null) playerSpriteRenderer.color = down ? new Color(0.55f, 0.55f, 0.65f, 0.35f) : originalColor;
        foreach (var c in GetComponentsInChildren<Collider2D>(true)) if (c.isTrigger) c.enabled = !down;
        if (!down) RefreshTint();
    }

    /// <summary>G3: can doluyken kalp toplayınca bu oyunluk kalp sayısı büyür (HealthCap'e kadar).</summary>
    public void GrowMaxHealth(int n)
    {
        int grow = Mathf.Min(n, HealthCap - maxHealth);
        if (grow <= 0) return;
        maxHealth += grow;
        currentHealth += grow;
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
        if (coldTint > 0.01f) c = Color.Lerp(c, new Color(0.62f, 0.85f, 1f, c.a), coldTint); // G7: üşüyen oyuncu mavileşir
        if (isGhost) c.a = GhostAlpha;
        playerSpriteRenderer.color = c;
    }

    float coldTint;
    /// <summary>G7: soğuk göstergesi (0-1) karakteri hafifçe maviye boyar. Vuruş flaşı sırasında dokunmaz.</summary>
    public void SetColdTint(float v)
    {
        coldTint = Mathf.Clamp01(v);
        if (hitFlashCoroutine == null && invincibilityCoroutine == null && !isDead) RefreshTint();
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
            var mv = GetComponent<PlayerMovement2D>();
            if (mv != null) mv.StopImmediately(); // son pozda donsun

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

            // G5: iki kişilikte diğeri yaşıyorsa oyun bitmez, bu oyuncu 10 sn sonra döner
            if (GameManager.Instance != null && GameManager.Instance.HandlePlayerDown(this))
            {
                GameEvents.RaisePlayerDowned(transform.position);
                return;
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