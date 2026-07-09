using UnityEngine;

/// <summary>
/// 2.5D Derinlik Sıralama (Depth Sorting) Bileşeni.
///
/// İKİ MODDA çalışır:
///
/// 1. TEK NESNE MODU — Aynı GameObject'te SpriteRenderer varsa
///    sadece onu kendi Y pozisyonuna göre sıralar.
///    Örnek: Player, Projectile prefab'ı
///
/// 2. PARENT MODU — Aynı GameObject'te SpriteRenderer YOKSA
///    tüm child SpriteRenderer'ları bulur ve her birini
///    kendi Y pozisyonuna göre bağımsız sıralar.
///    Örnek: Spawners parent objesi → tüm spawner'lar otomatik kapsanır.
///
/// Kural: Y küçük = alt ekran = önde (yüksek sortingOrder)
///        Y büyük = üst ekran = arkada (düşük sortingOrder)
/// </summary>
public class YDepthSorter : MonoBehaviour
{
    [Header("Sıralama Ayarları")]
    [Tooltip("Temel sıra değeri. Farklı katmanlar çakışmasın diye farklı değer ver.")]
    public int baseOrder = 0;

    [Tooltip("Y ekseninin sıralamaya etkisi. Büyük değer = daha belirgin katman ayrımı.")]
    public float yScale = 10f;

    [Tooltip("Her frame hesapla mı? Hareket eden nesnelerde true, statik nesnelerde false.")]
    public bool updateEveryFrame = true;

    // -------------------------------------------------------
    // Dahili Durum
    // -------------------------------------------------------

    // Tek nesne modu
    private SpriteRenderer selfRenderer;

    // Parent modu
    private SpriteRenderer[] childRenderers;
    private float[] childLastY;

    private bool isParentMode = false;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    void Awake()
    {
        selfRenderer = GetComponent<SpriteRenderer>();

        if (selfRenderer == null)
        {
            // Parent modu: tüm child SpriteRenderer'ları topla
            isParentMode = true;
            childRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            childLastY = new float[childRenderers.Length];
            for (int i = 0; i < childLastY.Length; i++) childLastY[i] = float.MaxValue;

            if (childRenderers.Length == 0)
            {
                Debug.LogWarning($"[YDepthSorter] '{gameObject.name}' üzerinde ne SpriteRenderer ne de child SpriteRenderer bulundu!");
            }
            else
            {
                Debug.Log($"[YDepthSorter] Parent modu: '{gameObject.name}' altında {childRenderers.Length} SpriteRenderer bulundu.");
            }
        }
    }

    void Start()
    {
        ForceUpdate();
    }

    void LateUpdate()
    {
        if (!updateEveryFrame) return;

        if (isParentMode)
            UpdateChildren();
        else
            UpdateSelf();
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Manuel güncelleme tetikler (statik nesneler veya pozisyon atlamaları için).
    /// </summary>
    public void ForceUpdate()
    {
        if (isParentMode)
            UpdateChildren();
        else
            UpdateSelf();
    }

    // -------------------------------------------------------
    // Dahili Güncelleme
    // -------------------------------------------------------

    void UpdateSelf()
    {
        if (selfRenderer == null) return;

        float y = transform.position.y;
        selfRenderer.sortingOrder = baseOrder - Mathf.RoundToInt(y * yScale);
    }

    void UpdateChildren()
    {
        if (childRenderers == null) return;

        for (int i = 0; i < childRenderers.Length; i++)
        {
            SpriteRenderer sr = childRenderers[i];
            if (sr == null) continue;

            float y = sr.transform.position.y;

            // Y değişmediyse atla (performans)
            if (Mathf.Abs(y - childLastY[i]) < 0.01f) continue;

            childLastY[i] = y;
            sr.sortingOrder = baseOrder - Mathf.RoundToInt(y * yScale);
        }
    }
}

