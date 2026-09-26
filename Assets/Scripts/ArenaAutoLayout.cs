using UnityEngine;

/// <summary>
/// Arenanın fiziksel yerleşimi için tek kaynak.
/// Arena görselinin içindeki oynanabilir zemini (playableAreaNormalized) dünya koordinatına çevirir
/// ve duvar colliderlarını, fırlatıcı duruş noktalarını buna göre yerleştirir.
/// Powerup alanı, mermi sınırları ve test botu PlayableWorldRect'i okur.
/// </summary>
[ExecuteAlways]
public class ArenaAutoLayout : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer arenaSpriteRenderer;

    public Transform boundsRoot;
    public Transform topWall;
    public Transform bottomWall;
    public Transform leftWall;
    public Transform rightWall;

    public Transform spawnersRoot;
    public Transform topLeftSpawner;
    public Transform topRightSpawner;
    public Transform bottomLeftSpawner;
    public Transform bottomRightSpawner;

    public Transform player;

    [Header("Oynanabilir Alan (arena görseli içinde, 0-1 normalize)")]
    [Tooltip("ArenaData'dan kopyalanır. Görseldeki iç zemin: x,y sol-alt; width,height boyut.")]
    public Rect playableAreaNormalized = new Rect(0.0795f, 0.0795f, 0.841f, 0.841f);

    [Header("Bounds Settings")]
    public float wallThickness = 0.5f;
    [Tooltip("Eski ayar: sadece playableAreaNormalized boşsa (genişlik 0) kullanılır")]
    public float wallInset = 0.15f;

    [Header("Fırlatıcı Duruş Noktaları (0-1 normalize, ayak konumu)")]
    public bool useSpawnerAnchors = true;
    public Vector2 anchorTopLeft = new Vector2(0.138f, 0.834f);
    public Vector2 anchorTopRight = new Vector2(0.862f, 0.834f);
    public Vector2 anchorBottomLeft = new Vector2(0.130f, 0.126f);
    public Vector2 anchorBottomRight = new Vector2(0.870f, 0.126f);

    [Header("Köşe Kaideleri (katı, 0-1 normalize)")]
    [Tooltip("Fırlatıcıların durduğu kaideler katı olsun: oyuncu kaideye çıkamaz, fırlatıcının dibine giremez")]
    public bool solidPedestals = true;
    public Vector2 pedestalSize = new Vector2(0.1f, 0.1f);
    [Tooltip("Kaide merkezinin duruş noktasına göre kayması (sol köşeler için; sağda x aynalanır)")]
    public Vector2 pedestalOffsetTop = new Vector2(0f, 0.032f);
    public Vector2 pedestalOffsetBottom = new Vector2(0.003f, 0.007f);

    [Header("Spawner Settings (anchor kapalıysa)")]
    public float spawnerInsetX = 1.0f;
    public float spawnerInsetY = 1.0f;
    public float spawnerTopDepthOffset = 0.35f;

    [Header("Player Start")]
    public Vector2 playerLocalOffset = Vector2.zero;


    /// <summary>Oyuncunun yürüyebildiği iç zemin (dünya koordinatı). Duvarların iç yüzleri bu dikdörtgenin kenarlarıdır.</summary>
    public Rect PlayableWorldRect
    {
        get
        {
            if (arenaSpriteRenderer == null) return new Rect(-4f, -4f, 8f, 8f);
            Bounds b = arenaSpriteRenderer.bounds;
            Rect n = playableAreaNormalized;
            if (n.width <= 0f || n.height <= 0f)
            {
                float inner = wallInset + wallThickness * 0.5f;
                return Rect.MinMaxRect(b.min.x + inner, b.min.y + inner, b.max.x - inner, b.max.y - inner);
            }
            return new Rect(b.min.x + n.x * b.size.x, b.min.y + n.y * b.size.y, n.width * b.size.x, n.height * b.size.y);
        }
    }

    /// <summary>Arena görseli içindeki normalize noktayı dünya koordinatına çevirir.</summary>
    public Vector3 NormalizedToWorld(Vector2 n)
    {
        if (arenaSpriteRenderer == null) return transform.position;
        Bounds b = arenaSpriteRenderer.bounds;
        return new Vector3(b.min.x + n.x * b.size.x, b.min.y + n.y * b.size.y, 0f);
    }

    void Awake()
    {
        if (Application.isPlaying)
        {
            ApplyLayout(); // Awake'te çalışarak diğer Start()'lardan ÖNCE duvarları yerleştirir
        }
    }

    void LateUpdate()
    {
        // Oyunda duvarlar sabit (yeniden yerleşim ScreenComposer/LevelManager ile); editörde canlı önizleme
        if (!Application.isPlaying) ApplyLayout();
    }

    public void ApplyLayout()
    {
        if (arenaSpriteRenderer == null)
            return;

        Bounds arenaBounds = arenaSpriteRenderer.bounds;
        Vector3 centerWorld = arenaBounds.center;
        Rect play = PlayableWorldRect;
        float t = wallThickness;

        if (boundsRoot != null)
            boundsRoot.position = centerWorld;

        // Duvarlar: iç yüzleri oynanabilir alanın kenarında, kalınlık dışarı doğru
        float outerW = play.width + 2f * t;
        float outerH = play.height + 2f * t;
        SetupWall(topWall, new Vector3(play.center.x, play.yMax + t * 0.5f, 0f), new Vector2(outerW, t));
        SetupWall(bottomWall, new Vector3(play.center.x, play.yMin - t * 0.5f, 0f), new Vector2(outerW, t));
        SetupWall(leftWall, new Vector3(play.xMin - t * 0.5f, play.center.y, 0f), new Vector2(t, outerH));
        SetupWall(rightWall, new Vector3(play.xMax + t * 0.5f, play.center.y, 0f), new Vector2(t, outerH));

        if (spawnersRoot != null)
            spawnersRoot.position = centerWorld;

        if (useSpawnerAnchors)
        {
            PlaceSpawnerFeet(topLeftSpawner, NormalizedToWorld(anchorTopLeft));
            PlaceSpawnerFeet(topRightSpawner, NormalizedToWorld(anchorTopRight));
            PlaceSpawnerFeet(bottomLeftSpawner, NormalizedToWorld(anchorBottomLeft));
            PlaceSpawnerFeet(bottomRightSpawner, NormalizedToWorld(anchorBottomRight));
            if (Application.isPlaying) LayoutPedestals();
        }
        else
        {
            float halfW = arenaBounds.size.x * 0.5f;
            float halfH = arenaBounds.size.y * 0.5f;
            SetupSpawner(topLeftSpawner, centerWorld + new Vector3(-halfW + spawnerInsetX, halfH - spawnerInsetY - spawnerTopDepthOffset, 0f));
            SetupSpawner(topRightSpawner, centerWorld + new Vector3(halfW - spawnerInsetX, halfH - spawnerInsetY - spawnerTopDepthOffset, 0f));
            SetupSpawner(bottomLeftSpawner, centerWorld + new Vector3(-halfW + spawnerInsetX, -halfH + spawnerInsetY, 0f));
            SetupSpawner(bottomRightSpawner, centerWorld + new Vector3(halfW - spawnerInsetX, -halfH + spawnerInsetY, 0f));
        }

        // Player başlangıcı (Sadece Editor'deyken merkeze kilitle)
        if (!Application.isPlaying && player != null)
        {
            player.position = new Vector3(play.center.x + playerLocalOffset.x, play.center.y + playerLocalOffset.y, 0f);
        }
    }

    static void SetupWall(Transform wall, Vector3 worldPos, Vector2 size)
    {
        if (wall == null) return;
        wall.position = worldPos;
        BoxCollider2D col = wall.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            // Duvar objesinin ölçeği 1 değilse collider boyutunu ölçeğe göre düzelt
            Vector3 s = wall.lossyScale;
            col.size = new Vector2(size.x / Mathf.Max(Mathf.Abs(s.x), 0.0001f), size.y / Mathf.Max(Mathf.Abs(s.y), 0.0001f));
            col.offset = Vector2.zero;
        }
    }

    BoxCollider2D[] pedestals;

    /// <summary>Katı köşe kaidelerinin dünya dikdörtgenleri (bot ve analiz için). Kaide yoksa boş.</summary>
    public int PedestalRects(Rect[] into)
    {
        if (pedestals == null || !solidPedestals) return 0;
        int n = 0;
        for (int i = 0; i < pedestals.Length && n < into.Length; i++)
        {
            var b = pedestals[i].bounds;
            into[n++] = Rect.MinMaxRect(b.min.x, b.min.y, b.max.x, b.max.y);
        }
        return n;
    }

    /// <summary>Kaide collider'larını (oyun sırasında oluşturulur, sahneye yazılmaz) arena ölçeğine göre yerleştirir.</summary>
    void LayoutPedestals()
    {
        Transform parent = boundsRoot != null ? boundsRoot : transform;
        if (pedestals == null)
        {
            pedestals = new BoxCollider2D[4];
            int layer = topWall != null ? topWall.gameObject.layer : gameObject.layer;
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("Pedestal_" + i) { layer = layer };
                go.transform.SetParent(parent, false);
                pedestals[i] = go.AddComponent<BoxCollider2D>();
            }
        }
        Bounds b = arenaSpriteRenderer.bounds;
        Vector2 size = new Vector2(pedestalSize.x * b.size.x, pedestalSize.y * b.size.y);
        Vector2 flipX = new Vector2(-1f, 1f);
        Vector2[] centers =
        {
            anchorTopLeft + pedestalOffsetTop, anchorTopRight + Vector2.Scale(pedestalOffsetTop, flipX),
            anchorBottomLeft + pedestalOffsetBottom, anchorBottomRight + Vector2.Scale(pedestalOffsetBottom, flipX)
        };
        for (int i = 0; i < 4; i++)
        {
            var col = pedestals[i];
            col.gameObject.SetActive(solidPedestals);
            col.transform.position = NormalizedToWorld(centers[i]);
            Vector3 s = col.transform.lossyScale;
            col.size = new Vector2(size.x / Mathf.Max(Mathf.Abs(s.x), 0.0001f), size.y / Mathf.Max(Mathf.Abs(s.y), 0.0001f));
        }
    }

    /// <summary>Fırlatıcıyı, ayakları (Shadow child'ı varsa onun konumu) duruş noktasına gelecek şekilde yerleştirir.</summary>
    static void PlaceSpawnerFeet(Transform spawner, Vector3 feetWorld)
    {
        if (spawner == null) return;
        Vector3 feetOffset = Vector3.zero;
        Transform shadow = spawner.Find("Shadow");
        if (shadow != null) feetOffset = shadow.position - spawner.position;
        feetOffset.z = 0f;
        spawner.position = feetWorld - feetOffset;
    }

    void SetupSpawner(Transform spawner, Vector3 worldPos)
    {
        if (spawner == null) return;
        spawner.position = worldPos;
    }

    void OnDrawGizmos()
    {
        if (arenaSpriteRenderer == null) return;
        Rect r = PlayableWorldRect;
        Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.9f);
        Gizmos.DrawWireCube(r.center, new Vector3(r.width, r.height, 0f));
        if (useSpawnerAnchors)
        {
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(NormalizedToWorld(anchorTopLeft), 0.15f);
            if (solidPedestals)
            {
                Bounds ab = arenaSpriteRenderer.bounds;
                Vector3 ps = new Vector3(pedestalSize.x * ab.size.x, pedestalSize.y * ab.size.y, 0f);
                Vector2 fx = new Vector2(-1f, 1f);
                Gizmos.DrawWireCube(NormalizedToWorld(anchorTopLeft + pedestalOffsetTop), ps);
                Gizmos.DrawWireCube(NormalizedToWorld(anchorTopRight + Vector2.Scale(pedestalOffsetTop, fx)), ps);
                Gizmos.DrawWireCube(NormalizedToWorld(anchorBottomLeft + pedestalOffsetBottom), ps);
                Gizmos.DrawWireCube(NormalizedToWorld(anchorBottomRight + Vector2.Scale(pedestalOffsetBottom, fx)), ps);
            }
            Gizmos.DrawWireSphere(NormalizedToWorld(anchorTopRight), 0.15f);
            Gizmos.DrawWireSphere(NormalizedToWorld(anchorBottomLeft), 0.15f);
            Gizmos.DrawWireSphere(NormalizedToWorld(anchorBottomRight), 0.15f);
        }
    }
}
