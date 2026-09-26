using UnityEngine;

/// <summary>
/// Oyuncunun iki ayrı çarpışma alanı (2.5D için doğru kurulum):
///  - Ayak izi (kökteki CircleCollider2D, katı): duvarlarla çarpışır. Ayakların altında durur,
///    böylece üst (arka) duvarda gövde duvarın önünde görünür, alt duvarda ayak duvara girmez.
///  - Gövde (çocuk "Hurtbox", dikey kapsül, trigger): taşlar SADECE buna vurur. Görünen gövdeden
///    biraz küçük: "değmedi ki!" hissi olmaz, kıl payı kaçışlar mümkün olur.
/// Ölçüler dünya birimindedir (1 sanat pikseli = 0.024), kök ölçeğinden bağımsızdır.
/// Awake'te her zaman uygulanır; editörde "KacAtaKac/Oyuncu Çarpışmasını Kur" sahneye yazar.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerHitbox : MonoBehaviour
{
    public const string HurtboxName = "Hurtbox";

    [Header("Ayak izi (duvarlar)")]
    public float footRadius = 0.2f;
    public float footOffsetY = -0.26f;

    [Header("Gövde (taşlar)")]
    [Tooltip("Kapsül boyutu (dünya). Görünen gövde ~0.41 x 0.96")]
    public Vector2 hurtSize = new Vector2(0.36f, 0.8f);
    public float hurtOffsetY = 0f;

    /// <summary>Sahnedeki oyuncunun gövde collider'ı (taşlar yalnızca buna vurur).</summary>
    public static Collider2D Hurt { get; private set; }

    public CapsuleCollider2D HurtCollider { get; private set; }

    /// <summary>Kaçış hesapları için gövdeyi temsil eden yaklaşık daire yarıçapı (bot, analiz).</summary>
    public float ApproxHurtRadius => (hurtSize.x + hurtSize.y) * 0.25f;

    void Awake() { Apply(); }
    void OnEnable() { if (HurtCollider != null) Hurt = HurtCollider; }
    void OnDisable() { if (Hurt == HurtCollider) Hurt = null; }

    /// <summary>Collider'ları ayarlar, gövde çocuğunu yoksa oluşturur. Tekrar çağrılabilir.</summary>
    public void Apply()
    {
        float sx = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.x));
        float sy = Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.y));

        var foot = GetComponent<CircleCollider2D>();
        foot.isTrigger = false;
        foot.radius = footRadius / sx;
        foot.offset = new Vector2(0f, footOffsetY / sy);

        Transform child = transform.Find(HurtboxName);
        if (child == null)
        {
            child = new GameObject(HurtboxName).transform;
            child.SetParent(transform, false);
        }
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        child.gameObject.tag = gameObject.tag;
        child.gameObject.layer = gameObject.layer;

        var cap = child.GetComponent<CapsuleCollider2D>();
        if (cap == null) cap = child.gameObject.AddComponent<CapsuleCollider2D>();
        cap.isTrigger = true;
        cap.direction = CapsuleDirection2D.Vertical;
        cap.size = new Vector2(hurtSize.x / sx, hurtSize.y / sy);
        cap.offset = new Vector2(0f, hurtOffsetY / sy);

        HurtCollider = cap;
        if (isActiveAndEnabled) Hurt = cap;
    }

    /// <summary>Oynanabilir alan içinde oyuncu MERKEZİNİN gidebileceği dikdörtgen (ayak izine göre).</summary>
    public Rect CenterBounds(Rect playable)
    {
        return Rect.MinMaxRect(
            playable.xMin + footRadius, playable.yMin - footOffsetY + footRadius,
            playable.xMax - footRadius, playable.yMax - footOffsetY - footRadius);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position;
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.9f);
        Gizmos.DrawWireSphere(p + new Vector3(0f, footOffsetY, 0f), footRadius);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.9f);
        Gizmos.DrawWireCube(p + new Vector3(0f, hurtOffsetY, 0f), new Vector3(hurtSize.x, hurtSize.y, 0f));
    }
}
