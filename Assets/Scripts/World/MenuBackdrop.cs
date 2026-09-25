using UnityEngine;

/// <summary>
/// Ana menü arka planı: oyunun arenası ve dungeon çerçevesi (canlı), önünden süzülen dekoratif taşlar.
/// Kamera arenayı ekran genişliğine sığdırır, arenayı ekranın üst-orta kısmına koyar.
/// </summary>
public class MenuBackdrop : MonoBehaviour
{
    public Camera cam;
    public SpriteRenderer arena;
    public DungeonFrame frame;
    [Tooltip("Arenanın merkezi ekranın bu yüksekliğinde (0 alt, 1 üst)")]
    public float arenaCenterY = 0.56f;

    [Header("Dekoratif taşlar")]
    public Sprite rockSprite;
    public int rockCount = 6;
    public float rockSpeed = 1.2f;

    Transform[] rocks;
    Vector2[] vel;
    float[] spin;
    int lastW, lastH;
    Rect playRect;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        rocks = new Transform[rockCount];
        vel = new Vector2[rockCount];
        spin = new float[rockCount];
        for (int i = 0; i < rockCount; i++)
        {
            var go = new GameObject("MenuRock" + i);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = rockSprite;
            sr.sortingOrder = 50;
            rocks[i] = go.transform;
            float s = Random.Range(0.75f, 1.3f);
            go.transform.localScale = Vector3.one * s;
            spin[i] = Random.Range(-200f, 200f);
        }
        Layout();
        for (int i = 0; i < rockCount; i++) Respawn(i, true);
    }

    void Layout()
    {
        lastW = Screen.width; lastH = Screen.height;
        if (cam == null || arena == null) return;
        Bounds b = arena.bounds;
        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        cam.orthographic = true;
        cam.orthographicSize = b.size.x / aspect * 0.5f;
        float camY = b.center.y - (arenaCenterY - 0.5f) * cam.orthographicSize * 2f;
        cam.transform.position = new Vector3(b.center.x, camY, cam.transform.position.z);
        float h = cam.orthographicSize * 2f, w = h * aspect;
        var camRect = new Rect(b.center.x - w * 0.5f, camY - h * 0.5f, w, h);
        if (frame != null) frame.Layout(camRect, new Rect(b.min.x, b.min.y, b.size.x, b.size.y));
        float inset = b.size.x * 0.1f;
        playRect = Rect.MinMaxRect(b.min.x + inset, b.min.y + inset, b.max.x - inset, b.max.y - inset);
    }

    void Respawn(int i, bool anywhere)
    {
        Vector2 p = anywhere
            ? new Vector2(Random.Range(playRect.xMin, playRect.xMax), Random.Range(playRect.yMin, playRect.yMax))
            : new Vector2(Random.value < 0.5f ? playRect.xMin : playRect.xMax, Random.Range(playRect.yMin, playRect.yMax));
        rocks[i].position = p;
        Vector2 target = new Vector2(Random.Range(playRect.xMin, playRect.xMax), Random.Range(playRect.yMin, playRect.yMax));
        vel[i] = (target - p).normalized * rockSpeed * Random.Range(0.7f, 1.3f);
        if (vel[i].sqrMagnitude < 0.01f) vel[i] = Vector2.right * rockSpeed;
    }

    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH) Layout();
        if (rocks == null) return;
        float dt = Time.deltaTime;
        for (int i = 0; i < rocks.Length; i++)
        {
            rocks[i].position += (Vector3)(vel[i] * dt);
            rocks[i].Rotate(0f, 0f, spin[i] * dt);
            Vector2 p = rocks[i].position;
            if (p.x < playRect.xMin - 0.3f || p.x > playRect.xMax + 0.3f || p.y < playRect.yMin - 0.3f || p.y > playRect.yMax + 0.3f)
                Respawn(i, false);
        }
    }
}
