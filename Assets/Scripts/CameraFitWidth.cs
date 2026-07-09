using UnityEngine;

[ExecuteAlways]
public class CameraFitWidth : MonoBehaviour
{
    public Camera cam;
    public Transform arenaVisual;

    private float lastScreenWidth = -1f;
    private float lastScreenHeight = -1f;

    void LateUpdate()
    {
        // Ekran boyutu değişmediyse (örneğin cihaz dönmediyse) hesaplama yapma
        if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
            return;

        // Ekran boyutu değiştiyse yeni boyutu kaydet
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        UpdateCameraSize();
    }

    void UpdateCameraSize()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam == null || arenaVisual == null || !cam.orthographic)
            return;

        SpriteRenderer sr = arenaVisual.GetComponent<SpriteRenderer>();
        if (sr == null)
            return;

        // Kamera aspect (en/boy) oranı
        float aspect = (float)Screen.width / Screen.height;

        // Arena'nın dünya (world-space) boyutları
        float arenaWidth = sr.bounds.size.x;
        float arenaHeight = sr.bounds.size.y;

        // Genişliği sığdırmak için gereken boyut
        float orthoSizeForWidth = arenaWidth / (2f * aspect);

        // Yüksekliği sığdırmak için gereken boyut
        float orthoSizeForHeight = arenaHeight / 2f;

        // Ekran dikey veya yatay olsun, hangisinde daha fazla "uzaklaşmak" (zoom out) gerekiyorsa onu seç.
        // Böylece Arena her zaman tüm ekranın içine sığar.
        cam.orthographicSize = Mathf.Max(orthoSizeForWidth, orthoSizeForHeight);
    }

    /// <summary>
    /// Önbelleği geçersiz kılarak kamera boyutunu yeniden hesaplar.
    /// </summary>
    public void ForceRecalculate()
    {
        lastScreenWidth = -1f;
        lastScreenHeight = -1f;
        UpdateCameraSize();
    }
}