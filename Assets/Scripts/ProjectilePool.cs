using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mermi (Projectile) nesneleri için Object Pool sistemi.
/// Instantiate/Destroy yerine nesneleri geri dönüştürür.
/// CornerShooter tarafından kullanılır.
///
/// Kullanım:
///   GameObject obj = ProjectilePool.Instance.Get(prefab, position, rotation);
///   ProjectilePool.Instance.Return(obj);   // OnTrigger veya sınır dışı çıkınca çağır
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [Header("Pool Ayarları")]
    [Tooltip("Her prefab başına hazır tutulacak başlangıç mermi sayısı")]
    public int initialPoolSize = 10;

    [Tooltip("Pool dolduğunda otomatik büyütülsün mü?")]
    public bool autoExpand = true;

    // Prefab -> kuyruk eşlemesi
    private Dictionary<GameObject, Queue<GameObject>> pools
        = new Dictionary<GameObject, Queue<GameObject>>();

    // Her nesnenin kaynaklandığı prefabı hatırla (Return için)
    private Dictionary<GameObject, GameObject> prefabLookup
        = new Dictionary<GameObject, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Pool'dan bir mermi al (yoksa yarat).
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        EnsurePool(prefab);

        GameObject obj;
        Queue<GameObject> queue = pools[prefab];

        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
            if (obj == null)
            {
                // Yanlışlıkla Destroy edilmiş — yeni yarat
                obj = CreateNew(prefab);
            }
        }
        else if (autoExpand)
        {
            obj = CreateNew(prefab);
        }
        else
        {
            Debug.LogWarning("[ProjectilePool] Pool doldu ve autoExpand kapalı! Prefab: " + prefab.name);
            return null;
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Mermiyi pool'a iade et (Destroy yerine çağır).
    /// </summary>
    public void Return(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);

        if (prefabLookup.TryGetValue(obj, out GameObject prefab) && pools.ContainsKey(prefab))
        {
            pools[prefab].Enqueue(obj);
        }
        else
        {
            // Kaynağı bilinmiyor — güvenli yaklaşım: yok et
            Destroy(obj);
        }
    }

    /// <summary>
    /// Belirli bir prefab için pool'u önceden ısıt (sahne yüklenince çağırabilirsin).
    /// </summary>
    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null) return;

        EnsurePool(prefab);

        for (int i = 0; i < count; i++)
        {
            GameObject obj = CreateNew(prefab);
            obj.SetActive(false);
            pools[prefab].Enqueue(obj);
        }

        KakLog.Info($"[ProjectilePool] '{prefab.name}' için {count} mermi ısıtıldı.");
    }

    // -------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------

    void EnsurePool(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
        }
    }

    GameObject CreateNew(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform); // Pool'un altında tut
        prefabLookup[obj] = prefab;
        return obj;
    }

    // -------------------------------------------------------
    // Temizlik
    // -------------------------------------------------------

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        pools.Clear();
        prefabLookup.Clear();
    }
}
