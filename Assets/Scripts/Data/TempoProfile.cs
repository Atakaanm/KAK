using UnityEngine;

/// <summary>
/// Faz 15 K1: sonsuz modun tempo eğrisi. Sert kademeler yerine oyun boyunca tek sürekli değer τ (0 → 1):
/// her kaos düğmesi (atış hızı, taş hızı, boyut, çift atış, olay sıklığı...) τ'nun eğrisidir, küçük adımlarla ve
/// sırayla artar; oyuncu "zorlaştı" diye bir an fark etmez. Taş türü karışımı kademe verilerinden (DifficultyStageData)
/// τ'ya göre harmanlanır. Profil yoksa DifficultyManager eski kademe sistemiyle çalışır (gizli dünyalar).
/// Kurulum: KacAtaKac/Tempo Profilini Kur (KakTempoSetup).
/// </summary>
[CreateAssetMenu(fileName = "New TempoProfile", menuName = "KacAtaKac/Tempo Profile")]
public class TempoProfile : ScriptableObject
{
    [Header("Zaman → tempo")]
    [Tooltip("Güçsüz oyuncu τ = 1'e bu kadar saniyede ulaşır")]
    public float rampSeconds = 360f;
    [Tooltip("Tam güçlü oyuncunun rampası (güce göre aradeğer)")]
    public float rampSecondsFullPower = 300f;
    [Tooltip("Tam güçlü oyuncu oyuna bu tempoyla başlar (güçlendikçe oyun da hızlanır, ama tam telafi etmez)")]
    [Range(0f, 0.6f)] public float startTempoFullPower = 0.3f;
    [Tooltip("Güç başlangıcı bu sürede yumuşakça devreye girer (güçlü oyuncu da sakin başlar, hızlı ısınır; ani sıçrama yok)")]
    public float powerWarmupSeconds = 20f;

    [Header("Düğmeler (τ → değer)")]
    [Tooltip("Toplam atış hızı: fırlatıcının kendi aralığı başına atış (1 = tek fırlatıcı normal hızda). Fırlatıcı sayısı artınca toplam sıçramaz, paylaşılır")]
    public AnimationCurve fireRate = AnimationCurve.Linear(0f, 0.85f, 1f, 5.6f);
    public AnimationCurve projectileSpeed = AnimationCurve.Linear(0f, 0.84f, 1f, 1.36f);
    public AnimationCurve projectileScale = AnimationCurve.Linear(0f, 1f, 1f, 1.2f);
    public AnimationCurve playerSpeed = AnimationCurve.Linear(0f, 1f, 1f, 1.1f);
    [Tooltip("Skor/sn çarpanı: hızlı oyun = hızlı skor")]
    public AnimationCurve scoreSpeed = AnimationCurve.Linear(0f, 1f, 1f, 1.75f);
    [Tooltip("Bir atışın hemen ardından ikinci atış olasılığı")]
    public AnimationCurve doubleShot = AnimationCurve.Constant(0f, 1f, 0f);
    [Tooltip("Olaylar arası süre çarpanı (küçük = sık)")]
    public AnimationCurve eventInterval = AnimationCurve.Constant(0f, 1f, 1f);

    [Header("Fırlatıcılar")]
    [Tooltip("i. eleman: i+1. fırlatıcının uyandığı tempo (artan sırada)")]
    public float[] throwerTempo = { 0f, 0.083f, 0.208f, 0.667f };
    [Tooltip("Yeni fırlatıcı atış payını bu sürede yavaş yavaş alır (ilk atışlar seyrek)")]
    public float wakeSeconds = 10f;
    [Tooltip("Yeni taş türleri kademe zamanından sonra, kademe aralığının bu payında yavaşça karışıma girer (asla erken değil)")]
    [Range(0.05f, 1f)] public float typeBlend = 0.5f;
    [Tooltip("İkinci atışın gecikmesi (sn)")]
    public float doubleShotDelay = 0.22f;

    [Header("Nefes payı (olay bitince)")]
    public float breatherSeconds = 6f;
    [Range(0f, 0.5f)] public float breatherRateDrop = 0.15f;
    [Range(0f, 0.3f)] public float breatherSpeedDrop = 0.06f;

    [Header("Merhamet (görünmez)")]
    [Tooltip("Hasar alınca tempo artışı bu kadar durur")]
    public float mercySeconds = 8f;

    [Header("Çırak (ilk oyunlar yavaş)")]
    [Tooltip("Oyun sayısı 0, 1, 2... için rampa uzatma çarpanı")]
    public float[] apprenticeRampMult = { 1.35f, 1.2f, 1.1f };
    [Tooltip("Oyun sayısı 0, 1, 2... için taş hızı çarpanı (apprenticeFadeSeconds içinde 1'e döner)")]
    public float[] apprenticeSpeed = { 0.85f, 0.9f, 0.95f };
    public float apprenticeFadeSeconds = 60f;

    [Header("His")]
    [Tooltip("τ = 1'de müzik perdesi (çaktırmadan hızlanma hissi)")]
    public float musicPitchAtMax = 1.04f;

    /// <summary>Kademe verisinin tempo karşılığı (taş türü karışımı bu noktalar arasında harmanlanır).</summary>
    public float StageTempo(DifficultyStageData s) => s == null || rampSeconds <= 0f ? 0f : s.minSeconds / rampSeconds;

    /// <summary>Bu tempoda kaç fırlatıcı uyanık.</summary>
    public int ThrowersAt(float tau)
    {
        int n = 0;
        if (throwerTempo != null)
            for (int i = 0; i < throwerTempo.Length; i++) if (tau >= throwerTempo[i]) n = i + 1;
        return Mathf.Max(1, n);
    }

    public float RampFor(float power, int gamesPlayed)
    {
        float r = Mathf.Lerp(rampSeconds, rampSecondsFullPower, Mathf.Clamp01(power));
        return r * Pick(apprenticeRampMult, gamesPlayed, 1f);
    }

    public float StartTempoFor(float power) => startTempoFullPower * Mathf.Clamp01(power);

    public float ApprenticeSpeed(int gamesPlayed, float seconds)
    {
        float a = Pick(apprenticeSpeed, gamesPlayed, 1f);
        if (apprenticeFadeSeconds <= 0f) return a;
        return Mathf.Lerp(a, 1f, Mathf.Clamp01(seconds / apprenticeFadeSeconds));
    }

    static float Pick(float[] arr, int i, float fallback) => arr != null && i >= 0 && i < arr.Length ? arr[i] : fallback;
}
