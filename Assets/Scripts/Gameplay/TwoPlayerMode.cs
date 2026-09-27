using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// G5 (Faz 11): tek telefonda iki kişilik mod kurulumu. 1. oyuncunun kopyasıyla 2. oyuncuyu doğurur, joystick bölgesini
/// ikiye böler (sol Ata, sağ Ada; klavyede WASD / oklar), HUD'da iki kalp satırı + portre + dönüş sayacı kurar.
/// Oyun kuralları: GameManager.HandlePlayerDown (düşen 10 sn sonra döner), PlayerRegistry (hedefleme).
/// </summary>
public static class TwoPlayerMode
{
    const float RowScale = 0.72f;

    public static PlayerMovement2D SpawnSecond(PlayerMovement2D p1, PlayerData data2, LevelManager lm)
    {
        var p1go = p1.gameObject;
        Vector3 basePos = p1go.transform.position;
        var go = Object.Instantiate(p1go, basePos + Vector3.right * 0.7f, p1go.transform.rotation, p1go.transform.parent);
        go.name = "Player2";
        MovePlayer(p1go, basePos + Vector3.left * 0.7f);

        var mv2 = go.GetComponent<PlayerMovement2D>();
        var ph1 = p1go.GetComponent<PlayerHealth>();
        var ph2 = go.GetComponent<PlayerHealth>();
        p1.keyboardScheme = 1;   // WASD
        mv2.keyboardScheme = 2;  // oklar

        SetupJoysticks(p1, mv2);
        SetupHud(ph1, ph2, lm != null ? lm.CurrentCharacter : null, data2);
        if (lm != null) lm.ApplyPlayerDataTo(data2, mv2, ph2, go.GetComponentInChildren<PlayerDirectionSprite>(true), false);
        return mv2;
    }

    static void MovePlayer(GameObject go, Vector3 pos)
    {
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = pos;
        go.transform.position = pos;
    }

    static void SetupJoysticks(PlayerMovement2D p1, PlayerMovement2D p2)
    {
        var j1 = p1.joystick;
        if (j1 == null) return;
        var zone = j1.transform as RectTransform;
        zone.anchorMax = new Vector2(0.5f, zone.anchorMax.y);
        var clone = Object.Instantiate(zone.gameObject, zone.parent);
        clone.name = "JoystickZone2";
        var z2 = clone.transform as RectTransform;
        z2.anchorMin = new Vector2(0.5f, zone.anchorMin.y);
        z2.anchorMax = new Vector2(1f, zone.anchorMax.y);
        z2.offsetMin = zone.offsetMin; z2.offsetMax = zone.offsetMax;
        p2.joystick = clone.GetComponent<VirtualJoystick>();
    }

    static void SetupHud(PlayerHealth ph1, PlayerHealth ph2, PlayerData d1, PlayerData d2)
    {
        var ui1 = ph1 != null ? ph1.healthUI : null;
        if (ui1 == null) return;
        var r1 = ui1.transform as RectTransform;
        var ui2go = Object.Instantiate(ui1.gameObject, r1.parent);
        ui2go.name = "HealthPanel2";
        var ui2 = ui2go.GetComponent<HealthUI>();
        ph2.healthUI = ui2;

        Vector2 basePos = r1.anchoredPosition;
        Row(r1, basePos + new Vector2(70f, 36f), d1, ph1);
        Row((RectTransform)ui2go.transform, basePos + new Vector2(70f, -40f), d2, ph2);

        var coins = Object.FindAnyObjectByType<CoinHud>();
        if (coins != null) coins.gameObject.SetActive(false); // iki satır kalp bandı doldurur; altın yine sayılır
    }

    static void Row(RectTransform panel, Vector2 pos, PlayerData data, PlayerHealth ph)
    {
        panel.localScale = Vector3.one * RowScale;
        panel.anchoredPosition = pos;
        // Satırın solunda karakter portresi (kim kim)
        var icon = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)icon.transform;
        rt.SetParent(panel, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-8f, 0f);
        rt.sizeDelta = new Vector2(96f, 96f); // 48 px × 2 (satır ölçeğiyle ~1,4)
        var img = icon.GetComponent<Image>();
        img.sprite = data != null ? data.Portrait : null;
        img.preserveAspect = true;
        img.raycastTarget = false;
        // Dönüş sayacı
        var txtGo = new GameObject("Respawn", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = (RectTransform)txtGo.transform;
        trt.SetParent(panel, false);
        trt.anchorMin = trt.anchorMax = new Vector2(0f, 0.5f);
        trt.pivot = new Vector2(0f, 0.5f);
        trt.anchoredPosition = new Vector2(20f, 0f);
        trt.sizeDelta = new Vector2(320f, 100f);
        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 72;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = KakPalette.AltinAcik;
        tmp.raycastTarget = false;
        var badge = panel.gameObject.AddComponent<RespawnBadge>();
        badge.player = ph;
        badge.text = tmp;
        badge.hearts = panel.GetComponent<HealthUI>();
    }
}

/// <summary>G5: kalp satırında dönüş sayacı — düşmüş oyuncunun kalpleri gizlenir, "7" gibi saniye görünür.</summary>
public class RespawnBadge : MonoBehaviour
{
    public PlayerHealth player;
    public TMP_Text text;
    public HealthUI hearts;
    int shown = -1;

    void Update()
    {
        if (player == null || text == null) return;
        int sec = player.RespawnRemaining > 0f ? Mathf.CeilToInt(player.RespawnRemaining) : 0;
        if (sec == shown) return;
        shown = sec;
        text.gameObject.SetActive(sec > 0);
        if (sec > 0) text.SetText("{0}", sec);
        if (hearts != null) hearts.SetHeartsVisible(sec == 0);
    }
}
