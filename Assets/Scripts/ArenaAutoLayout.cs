using UnityEngine;

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

    [Header("Bounds Settings")]
    public float wallThickness = 0.5f;
    public float wallInset = 0.15f;

    [Header("Spawner Settings")]
    public float spawnerInsetX = 1.0f; // Köşelerden biraz daha içeri alındı
    public float spawnerInsetY = 1.0f;
    public float spawnerTopDepthOffset = 0.35f; // Üstteki fırlatıcıları bir miktar yukarı aldım (Ayakları daha iyi bassın diye)

    [Header("UI Düzeni")]
    public RectTransform healthUI;
    public RectTransform scoreUI;
    public float uiWorldVerticalOffset = 0.85f; // Arenanın üst kenarından yukarı pay (azaltıldı)
    public float uiWorldHorizontalInset = 1.25f; // Köşelerden içeri doğru girme payı (arttırıldı, eksi değil artı kullanıyoruz)

    [Header("Player Start")]
    public Vector2 playerLocalOffset = Vector2.zero;

    void Start()
    {
        if (Application.isPlaying)
        {
            ApplyLayout(); // Oyun başladığı an performans için 1 kez tüm yapıyı diz
        }
    }

    void LateUpdate()
    {
        if (Application.isPlaying)
        {
            // Oyundayken duvar formlarını hesaplamayalım (eski isteğin doğrultusunda)
            // ANCAK adam telefonu çevirirse skor ve kalpler kaymasın diye sadece UI'ın yerini canlı güncel tutalım:
            ApplyUILayout();
            return;
        }

        // Editörde geliştirme yaparken ise duvarları, oyuncuyu vs. anlık canlı gösterebiliriz
        ApplyLayout();
        ApplyUILayout();
    }

    void ApplyLayout()
    {
        if (arenaSpriteRenderer == null)
            return;

        Bounds arenaBounds = arenaSpriteRenderer.bounds;

        Vector3 centerWorld = arenaBounds.center;
        float width = arenaBounds.size.x;
        float height = arenaBounds.size.y;

        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        // Bounds root merkezde dursun
        if (boundsRoot != null)
            boundsRoot.position = centerWorld;

        // Duvarlar
        SetupHorizontalWall(topWall, centerWorld + new Vector3(0f, halfH - wallInset, 0f), width, wallThickness);
        SetupHorizontalWall(bottomWall, centerWorld + new Vector3(0f, -halfH + wallInset, 0f), width, wallThickness);
        SetupVerticalWall(leftWall, centerWorld + new Vector3(-halfW + wallInset, 0f, 0f), wallThickness, height);
        SetupVerticalWall(rightWall, centerWorld + new Vector3(halfW - wallInset, 0f, 0f), wallThickness, height);

        // Spawners root merkezde dursun
        if (spawnersRoot != null)
            spawnersRoot.position = centerWorld;

        // Spawnerlar - arenanın köşelerinden içeri doğru
        // NOT: 2.5D yüzünden top fırlatıcıları daha aşağıya çekildi
        SetupSpawner(topLeftSpawner, centerWorld + new Vector3(-halfW + spawnerInsetX, halfH - spawnerInsetY - spawnerTopDepthOffset, 0f));
        SetupSpawner(topRightSpawner, centerWorld + new Vector3(halfW - spawnerInsetX, halfH - spawnerInsetY - spawnerTopDepthOffset, 0f));
        SetupSpawner(bottomLeftSpawner, centerWorld + new Vector3(-halfW + spawnerInsetX, -halfH + spawnerInsetY, 0f));
        SetupSpawner(bottomRightSpawner, centerWorld + new Vector3(halfW - spawnerInsetX, -halfH + spawnerInsetY, 0f));

        // Player başlangıcı (Sadece Editor'deyken merkeze kilitle)
        if (!Application.isPlaying && player != null)
        {
            player.position = centerWorld + new Vector3(playerLocalOffset.x, playerLocalOffset.y, 0f);
        }
    }

    void ApplyUILayout()
    {
        // UI öğeleri eğer inspector'a sürüklenmişse onları ekran sınırlarına göre değil,
        // arenanın gerçek dünya referanslarına göre hizalayalım.
        if (arenaSpriteRenderer == null || Camera.main == null)
            return;

        Bounds arenaBounds = arenaSpriteRenderer.bounds;
        Vector3 center = arenaBounds.center;
        float halfW = arenaBounds.size.x * 0.5f;
        float halfH = arenaBounds.size.y * 0.5f;

        if (healthUI != null)
        {
            // Sadece oyun oynanırken süzülme efekti aktif olsun
            float hoverEffect = Application.isPlaying ? Mathf.Sin(Time.time * 3.5f) * 0.12f : 0f;

            // Arenanın sol üst köşesi + içeri doğru Inset kadar kaydır
            Vector3 worldPos = center + new Vector3(-halfW + uiWorldHorizontalInset, halfH + uiWorldVerticalOffset + hoverEffect, 0f);
            healthUI.position = Camera.main.WorldToScreenPoint(worldPos);
        }

        if (scoreUI != null)
        {
            // Arenanın sağ üst köşesi - içeri doğru Inset kadar kaydır
            Vector3 worldPos = center + new Vector3(halfW - uiWorldHorizontalInset, halfH + uiWorldVerticalOffset, 0f);
            scoreUI.position = Camera.main.WorldToScreenPoint(worldPos);
        }
    }

    void SetupHorizontalWall(Transform wall, Vector3 worldPos, float width, float thickness)
    {
        if (wall == null) return;

        wall.position = worldPos;

        BoxCollider2D col = wall.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(width, thickness);
            col.offset = Vector2.zero;
        }
    }

    void SetupVerticalWall(Transform wall, Vector3 worldPos, float thickness, float height)
    {
        if (wall == null) return;

        wall.position = worldPos;

        BoxCollider2D col = wall.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(thickness, height);
            col.offset = Vector2.zero;
        }
    }

    void SetupSpawner(Transform spawner, Vector3 worldPos)
    {
        if (spawner == null) return;
        spawner.position = worldPos;
    }
}