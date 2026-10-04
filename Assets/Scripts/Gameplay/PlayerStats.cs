using UnityEngine;

/// <summary>
/// Faz 15 K3: oyuncunun bu oyunluk özellik sayfası (LevelManager.ApplyPlayerDataTo kurar). İki kişilikte her oyuncuda ayrı
/// (karakter tabanı farklı, ortak gelişim aynı). Primary = 1. oyuncu (skor, altın, eşya gibi ortak değerler).
/// Kendi işleri: altın mıknatısı, kalkan yenilenmesi, ikinci şans hakkı. Okuma tahsis yapmaz.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Primary { get; private set; }

    public PlayerData Data { get; private set; }
    public readonly StatSheet Sheet = new StatSheet();
    public int RevivesLeft { get; private set; }
    public float MagnetRadius { get; private set; }
    public float ShieldRegenEvery { get; private set; }

    PlayerHealth health;
    float regenTimer;
    const float GhostMagnetPerLevel = 1.2f;

    public void Build(PlayerData data, bool primary)
    {
        Data = data;
        StatBuilder.Build(data, Sheet);
        RevivesLeft = StatBuilder.Revives(Sheet);
        MagnetRadius = StatBuilder.Magnet(Sheet);
        ShieldRegenEvery = StatBuilder.ShieldRegenInterval(Sheet);
        regenTimer = 0f;
        if (primary) Primary = this;
        if (health == null) health = GetComponent<PlayerHealth>();
    }

    /// <summary>Oyun içi kaynak (K8 kartları) eklendikten sonra türev değerleri tazeler.</summary>
    public void Refresh()
    {
        MagnetRadius = StatBuilder.Magnet(Sheet);
        ShieldRegenEvery = StatBuilder.ShieldRegenInterval(Sheet);
    }

    public float PowerDuration => StatBuilder.PowerDuration(Data, Sheet);
    public float ShackleResist => StatBuilder.ShackleResist(Sheet);
    public float CoinMult => StatBuilder.CoinMult(Data, Sheet);

    public bool UseRevive()
    {
        if (RevivesLeft <= 0) return false;
        RevivesLeft--;
        return true;
    }

    void Awake() => health = GetComponent<PlayerHealth>();

    void OnDestroy()
    {
        if (Primary == this) Primary = null;
    }

    void Update()
    {
        if (health == null || health.IsDead) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Mıknatıs: yarıçap içindeki altınlar oyuncuya kayar (K5: geliştirilmiş hayaletken ek mıknatıs)
        float magnet = MagnetRadius + (health.IsGhost ? GhostMagnetPerLevel * ItemProgress.Power(PowerupType.Ghost) : 0f);
        if (magnet > 0.01f)
        {
            Vector3 p = transform.position;
            float r2 = magnet * magnet;
            var list = Coin.Active;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var c = list[i];
                if (c != null && (c.transform.position - p).sqrMagnitude <= r2) c.PullTowards(p, StatBuilder.MagnetPullSpeed);
            }
        }

        // Kalkan yenilenmesi: kalkansız geçen her ShieldRegenEvery saniyede bir kalkan
        if (ShieldRegenEvery > 0f)
        {
            if (health.HasShield) regenTimer = 0f;
            else
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= ShieldRegenEvery)
                {
                    regenTimer = 0f;
                    health.ActivateShield();
                    var am = AudioManager.Instance;
                    if (am != null) am.PlaySfx(am.shieldSfx);
                }
            }
        }
    }
}
