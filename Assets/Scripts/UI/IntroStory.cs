using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// G6 (Faz 11): oyunun hafif hikâyesi — ilk açılışta 3 kart (atlanabilir), ayarlardan tekrar izlenir.
/// Ata ile Ada Taş Zindanı'nda altına dokununca Taş Muhafızlar uyanır; lanet onları her yerde kovalar.
/// Kurulum: KakUiSetup.BuildIntroPanel. Görüldü bayrağı: SaveData.seen "intro_story".
/// </summary>
public class IntroStory : MonoBehaviour
{
    public const string SeenKey = "intro_story";

    public GameObject[] pages;
    public TMP_Text storyText;
    public Image[] dots;
    public TMP_Text nextLabel;
    public RectTransform art; // sayfa geçişinde hafif kayma
    public static readonly string[] TextKeys = { "story_1", "story_2", "story_3" };

    int page;
    float slide;
    Action onClosed;

    public static bool ShouldShow => !SaveSystem.Data.HasSeen(SeenKey);

    public void Show(Action closed = null)
    {
        onClosed = closed;
        page = 0;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Refresh();
    }

    void OnEnable() => Loc.Changed += Refresh;
    void OnDisable() => Loc.Changed -= Refresh;

    public void OnNext()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (page >= pages.Length - 1) { Close(); return; }
        page++;
        slide = 1f;
        Refresh();
    }

    public void OnSkip()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        Close();
    }

    void Close()
    {
        SaveSystem.Data.MarkSeen(SeenKey);
        SaveSystem.Save();
        gameObject.SetActive(false);
        var cb = onClosed;
        onClosed = null;
        cb?.Invoke();
    }

    void Refresh()
    {
        for (int i = 0; i < pages.Length; i++) if (pages[i] != null) pages[i].SetActive(i == page);
        if (storyText != null) storyText.text = Loc.T(TextKeys[Mathf.Clamp(page, 0, TextKeys.Length - 1)]);
        if (dots != null) for (int i = 0; i < dots.Length; i++) if (dots[i] != null) dots[i].color = i == page ? KakPalette.Altin : KakPalette.Gece;
        if (nextLabel != null) nextLabel.text = Loc.T(page >= pages.Length - 1 ? "story_start" : "story_next");
    }

    void Update()
    {
        if (slide <= 0f || art == null) return;
        slide = Mathf.MoveTowards(slide, 0f, Time.unscaledDeltaTime * 4f);
        float k = slide * slide;
        art.anchoredPosition = new Vector2(80f * k, art.anchoredPosition.y);
    }
}
