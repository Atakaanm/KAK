using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// G7 (Faz 11): Buz dünyasında soğuk göstergesi. Zamanla dolar; yarıdan sonra oyuncu yavaşlar ve maviye döner,
/// tamamen dolunca donar (hareketsiz, taşlara açık) ve ateş bulana kadar birkaç saniyede bir yeniden donar.
/// Can doğrudan gitmez (tek canla başlangıçta 35. saniyede ölüm haksızdı; bot ölçümü). Ateş toplayınca düşer.
/// Tema: WorldTheme.coldEnabled/coldSeconds. Garantili ateş: IceWorld (FireDrops).
/// </summary>
public class ColdMeter : MonoBehaviour
{
    public static readonly List<ColdMeter> All = new List<ColdMeter>(2);

    public float coldSeconds = 35f;
    public float slowStart = 0.5f;
    public float minSpeedMultiplier = 0.55f;
    public float freezeSeconds = 1.3f;
    [Tooltip("Donma sonrası değer. Faz 12 H1: donma soğuğu atar (yavaşlama eşiğinin altı) — eskisi 0,82'de kalıp oyuncuyu " +
             "mavi ve yavaş bırakıyordu, 'donma geçmedi' gibi görünüyordu. Ceza = donukken taşlara açık kalmak.")]
    public float afterFreeze = 0.45f;
    /// <summary>Ateş yakındaki arkadaşı da yarı ısıtır (birim).</summary>
    public const float ShareRadius = 1.5f;

    public float Value { get; private set; }
    public bool Warning => Value >= slowStart;

    PlayerMovement2D mv;
    PlayerHealth ph;
    SpriteRenderer iceBlock;
    float lastTint = -1f;

    void Awake()
    {
        mv = GetComponent<PlayerMovement2D>();
        ph = GetComponent<PlayerHealth>();
    }

    void OnEnable() => All.Add(this);
    void OnDisable()
    {
        All.Remove(this);
        if (mv != null) mv.coldSpeedMultiplier = 1f;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (ph != null && ph.IsDead) return;
        if (mv != null && mv.IceFrozen) { UpdateBlock(true); return; }
        UpdateBlock(false);

        Value = Mathf.Min(1f, Value + Time.deltaTime / Mathf.Max(1f, coldSeconds));
        float slow = Mathf.InverseLerp(slowStart, 1f, Value);
        if (mv != null) mv.coldSpeedMultiplier = Mathf.Lerp(1f, minSpeedMultiplier, slow);
        float tint = slow * 0.75f;
        if (ph != null && Mathf.Abs(tint - lastTint) > 0.04f) { ph.SetColdTint(tint); lastTint = tint; }
        if (Value >= 1f) Freeze();
    }

    void Freeze()
    {
        if (mv != null) mv.FreezeFor(freezeSeconds);
        Value = afterFreeze;
        if (mv != null) mv.coldSpeedMultiplier = 1f;
        if (ph != null) { ph.SetColdTint(0f); lastTint = 0f; }
        WorldPopup.Show(Loc.T("frozen"), transform.position + Vector3.up * 0.6f, KakPalette.CamgobegiParlak, 1f);
        var am = AudioManager.Instance;
        if (am != null) am.PlaySfx(am.crumbleSfx);
        GameEvents.RaisePlayerFrozen(transform.position);
    }

    /// <summary>Ateş: soğuğu düşürür.</summary>
    public void Warm(float amount)
    {
        Value = Mathf.Max(0f, Value - amount);
        lastTint = -1f;
    }

    /// <summary>Faz 12 H1: tamamen ısıtır (iki kişilikte düşen oyuncu sıcak döner).</summary>
    public void ResetCold()
    {
        Value = 0f;
        if (mv != null) mv.coldSpeedMultiplier = 1f;
        if (ph != null) ph.SetColdTint(0f);
        lastTint = 0f;
        UpdateBlock(false);
    }

    void UpdateBlock(bool frozen)
    {
        if (!frozen) { if (iceBlock != null && iceBlock.enabled) iceBlock.enabled = false; return; }
        if (iceBlock == null)
        {
            var go = new GameObject("IceBlock");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            iceBlock = go.AddComponent<SpriteRenderer>();
            iceBlock.sprite = Resources.Load<Sprite>("IceBlock");
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) { iceBlock.sortingLayerID = sr.sortingLayerID; iceBlock.sortingOrder = sr.sortingOrder + 3; }
        }
        iceBlock.enabled = iceBlock.sprite != null;
    }

    /// <summary>En çok üşüyen yaşayan oyuncunun değeri (HUD).</summary>
    public static float MaxValue()
    {
        float m = 0f;
        for (int i = 0; i < All.Count; i++) if (All[i] != null && All[i].isActiveAndEnabled) m = Mathf.Max(m, All[i].Value);
        return m;
    }
}
