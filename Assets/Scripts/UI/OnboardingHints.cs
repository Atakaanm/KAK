using TMPro;
using UnityEngine;

/// <summary>
/// İlk oyun ipuçları: her biri bir kez gösterilir (SaveData.seen). Aynı anda tek ipucu.
///  - hint_move: ilk oyunun başında; oyuncu ~1 sn hareket edince biter
///  - hint_dash: hareket öğrenildikten sonra, oyunun 10. sn'sinde dash hazırsa; ilk dash'te biter (dash butonu parlar)
///  - hint_near: ilk yakın geçişte kısa açıklama
/// Deneyimli kayıtlarda (en az 5 oyun) hiç gösterilmez. Faz 3c'de adım adım açılan özelliklerin de temeli.
/// Kurulum: KakEndlessSetup (ControlArea/ControlContent/HintText).
/// </summary>
public class OnboardingHints : MonoBehaviour
{
    public const string Move = "hint_move", Dash = "hint_dash", Near = "hint_near";

    public TMP_Text hintText;
    public DashButton dashButton;
    public float dashHintAfter = 10f;
    public int veteranGames = 5;
    public float nearHintTime = 2.8f;

    PlayerMovement2D move;
    string current;
    float shownFor, moveAccum, elapsed;
    bool nearPending;

    public string Current => current; // testler için

    void Start()
    {
        move = FindAnyObjectByType<PlayerMovement2D>();
        // Not: bileşen yazıyla aynı nesnede; nesneyi kapatmak Update'i de durdurur → sadece yazıyı gizle
        if (hintText != null) hintText.enabled = false;
        var d = SaveSystem.Data;
        if (d.gamesPlayed >= veteranGames) { d.MarkSeen(Move); d.MarkSeen(Dash); d.MarkSeen(Near); }
        if (!d.HasSeen(Move)) Show(Move);
    }

    void OnEnable()
    {
        GameEvents.DashUsed += OnDash;
        GameEvents.NearMiss += OnNearMiss;
        GameEvents.PlayerDied += OnDied;
    }

    void OnDisable()
    {
        GameEvents.DashUsed -= OnDash;
        GameEvents.NearMiss -= OnNearMiss;
        GameEvents.PlayerDied -= OnDied;
        if (dashButton != null) dashButton.highlight = false;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        float dt = (!KakTime.Paused && Time.timeScale > 0f) ? Time.unscaledDeltaTime : 0f;
        elapsed += dt;
        var d = SaveSystem.Data;

        if (current == null)
        {
            if (nearPending) { nearPending = false; if (!d.HasSeen(Near)) Show(Near); }
            else if (!d.HasSeen(Dash) && d.HasSeen(Move) && elapsed >= dashHintAfter
                     && dashButton != null && dashButton.dash != null && dashButton.dash.Ready)
                Show(Dash);
            return;
        }

        shownFor += dt;
        if (current == Move && move != null && move.MovementInput.sqrMagnitude > 0.04f)
        {
            moveAccum += dt;
            if (moveAccum >= 1.2f) Complete();
        }
        else if (current == Near && shownFor >= nearHintTime) Complete();

        if (hintText != null)
        {
            // Beliriş + hafif nefes (okunur ama göz yormaz)
            float pop = shownFor < 0.2f ? Mathf.Lerp(0.7f, 1f, shownFor / 0.2f) : 1f + Mathf.Sin(shownFor * 3f) * 0.03f;
            hintText.rectTransform.localScale = new Vector3(pop, pop, 1f);
        }
    }

    void Show(string id)
    {
        current = id;
        shownFor = 0f;
        moveAccum = 0f;
        if (hintText == null) return;
        hintText.SetText(Loc.T(id));
        var rt = hintText.rectTransform;
        if (id == Dash && dashButton != null)
        {
            // Dash butonunun üstünde
            var b = (RectTransform)dashButton.transform;
            rt.anchorMin = rt.anchorMax = b.anchorMin;
            rt.anchoredPosition = b.anchoredPosition + new Vector2(-90f, 185f);
            dashButton.highlight = true;
        }
        else
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.68f);
            rt.anchoredPosition = Vector2.zero;
        }
        hintText.color = id == Dash ? KakPalette.CamgobegiParlak : id == Near ? KakPalette.AltinAcik : KakPalette.Krem;
        hintText.enabled = true;
    }

    void Complete()
    {
        if (current == null) return;
        SaveSystem.Data.MarkSeen(current);
        SaveSystem.Save();
        if (current == Dash && dashButton != null) dashButton.highlight = false;
        current = null;
        if (hintText != null) hintText.enabled = false;
    }

    void OnDash(Vector3 pos, Vector2 dir)
    {
        if (current == Dash) Complete();
        else SaveSystem.Data.MarkSeen(Dash); // kendi keşfetti: bir daha anlatma
    }

    void OnNearMiss(Vector3 pos, bool dashing)
    {
        if (SaveSystem.Data.HasSeen(Near) || current == Near) return;
        if (current == null) Show(Near); else nearPending = true;
    }

    void OnDied(Vector3 pos)
    {
        if (hintText != null) hintText.enabled = false;
        if (dashButton != null) dashButton.highlight = false;
    }
}
