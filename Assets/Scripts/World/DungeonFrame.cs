using UnityEngine;

/// <summary>
/// Arenanın dışındaki ekran alanını oyun dünyasıyla doldurur (sanat-rehberi.md §5):
/// karanlık taş arka plan, arena kapısından inen koridor, koridor duvarları, HUD bandı kornişi,
/// meşaleler ve kenar karartması. ScreenComposer kamera dikdörtgenini bildirince yerleşir.
/// Tüm parçalar SpriteRenderer (Tiled) — tek tip materyal, az draw call.
/// Görünüm tek bir karo seti değiştirilerek değiştirilebilir (Faz 3: ArenaTileSet).
/// </summary>
public class DungeonFrame : MonoBehaviour
{
    [Header("Parçalar (KakScreenSetup tarafından kurulur)")]
    public SpriteRenderer backdrop;
    public SpriteRenderer corridorFloor;
    public SpriteRenderer corridorWallLeft;
    public SpriteRenderer corridorWallRight;
    public SpriteRenderer ledge;
    public SpriteRenderer vignette;
    public Transform[] torches;          // her meşale: flipbook + glow çocuğu

    [Header("Ölçüler (dünya birimi)")]
    public float corridorWidth = 2.6f;
    public float corridorTopOverlap = 0.25f;   // koridor arenanın alt duvarının altına biraz girsin
    public float torchSpacing = 2.4f;
    public float firstTorchOffset = 1.4f;       // arena altından ilk meşaleye mesafe
    public float torchWallGap = 0.05f;
    public float margin = 1f;                    // kamera dışına taşma payı

    public void Layout(Rect cam, Rect arenaRect)
    {
        Rect outer = new Rect(cam.xMin - margin, cam.yMin - margin, cam.width + 2f * margin, cam.height + 2f * margin);

        Place(backdrop, outer.center, outer.size);
        if (vignette != null)
        {
            vignette.transform.position = new Vector3(cam.center.x, cam.center.y, 0f);
            var sp = vignette.sprite;
            if (sp != null)
            {
                Vector2 ss = sp.bounds.size;
                vignette.transform.localScale = new Vector3(cam.width / ss.x, cam.height / ss.y, 1f);
            }
        }

        // Koridor: arena altından ekran altına
        float top = arenaRect.yMin + corridorTopOverlap;
        float bottom = outer.yMin;
        float h = Mathf.Max(0.01f, top - bottom);
        float cx = arenaRect.center.x;
        bool hasCorridor = top > cam.yMin + 0.05f;
        SetActive(corridorFloor, hasCorridor);
        SetActive(corridorWallLeft, hasCorridor);
        SetActive(corridorWallRight, hasCorridor);
        if (hasCorridor)
        {
            Place(corridorFloor, new Vector2(cx, bottom + h * 0.5f), new Vector2(corridorWidth, h));
            float ww = WallWidth(corridorWallLeft);
            Place(corridorWallLeft, new Vector2(cx - corridorWidth * 0.5f - ww * 0.5f, bottom + h * 0.5f), new Vector2(ww, h));
            Place(corridorWallRight, new Vector2(cx + corridorWidth * 0.5f + ww * 0.5f, bottom + h * 0.5f), new Vector2(ww, h));
            if (corridorWallRight != null) corridorWallRight.flipX = true;
        }

        // HUD bandı kornişi: arenanın hemen üstünde, ekran boyunca
        if (ledge != null)
        {
            float lh = ledge.sprite != null ? ledge.sprite.bounds.size.y : 0.3f;
            Place(ledge, new Vector2(cam.center.x, arenaRect.yMax + lh * 0.5f), new Vector2(outer.width, lh));
            SetActive(ledge, arenaRect.yMax + lh < cam.yMax + 0.01f);
        }

        // Meşaleler: koridor duvarları boyunca, iki yanda simetrik
        if (torches != null)
        {
            float ww = WallWidth(corridorWallLeft);
            float xL = cx - corridorWidth * 0.5f - ww - torchWallGap;
            float xR = cx + corridorWidth * 0.5f + ww + torchWallGap;
            int pairs = torches.Length / 2;
            for (int i = 0; i < pairs; i++)
            {
                float y = arenaRect.yMin - firstTorchOffset - i * torchSpacing;
                bool on = hasCorridor && y > cam.yMin + 0.4f;
                PlaceTorch(torches[i * 2], new Vector2(xL, y), on, false);
                PlaceTorch(torches[i * 2 + 1], new Vector2(xR, y), on, true);
            }
        }
    }

    static float WallWidth(SpriteRenderer sr) => sr != null && sr.sprite != null ? sr.sprite.bounds.size.x : 0.4f;

    static void Place(SpriteRenderer sr, Vector2 center, Vector2 size)
    {
        if (sr == null) return;
        sr.transform.position = new Vector3(center.x, center.y, 0f);
        sr.transform.localScale = Vector3.one;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
    }

    static void SetActive(Component c, bool on)
    {
        if (c != null && c.gameObject.activeSelf != on) c.gameObject.SetActive(on);
    }

    static void PlaceTorch(Transform t, Vector2 pos, bool on, bool right)
    {
        if (t == null) return;
        if (t.gameObject.activeSelf != on) t.gameObject.SetActive(on);
        t.position = new Vector3(pos.x, pos.y, 0f);
        var s = t.localScale;
        t.localScale = new Vector3(right ? -Mathf.Abs(s.x) : Mathf.Abs(s.x), s.y, s.z);
    }
}
