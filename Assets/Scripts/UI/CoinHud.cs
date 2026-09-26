using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD bandının ortasında bu oyunda toplanan altın: ikon + sayı. Toplayınca zıplar.
/// Altın bu oyunda kapalıysa (FeatureGate / bölüm modu) gizli kalır. Kurulum: KakMetaSetup.
/// </summary>
public class CoinHud : MonoBehaviour
{
    public Image icon;
    public TMP_Text countText;
    public RectTransform content;

    float pop;

    void Start()
    {
        var spawner = FindAnyObjectByType<CoinSpawner>();
        bool on = spawner != null && spawner.ActiveThisRun;
        if (content != null) content.gameObject.SetActive(on);
        if (countText != null) countText.SetText("0");
    }

    void OnEnable() => GameEvents.CoinCollected += OnCoin;
    void OnDisable() => GameEvents.CoinCollected -= OnCoin;

    void OnCoin(int total, Vector3 pos)
    {
        if (countText != null) countText.SetText("{0}", total);
        pop = 1f;
    }

    void Update()
    {
        if (pop <= 0f || content == null) return;
        pop = Mathf.MoveTowards(pop, 0f, Time.unscaledDeltaTime * 5f);
        float s = 1f + 0.25f * pop;
        content.localScale = new Vector3(s, s, 1f);
    }
}
