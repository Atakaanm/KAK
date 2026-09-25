using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Sahne geçişlerinde kısa kararma (gerçek zamanla). SceneLoader kullanır; kendini ilk ihtiyaçta yaratır.
/// </summary>
public class SceneFader : MonoBehaviour
{
    static SceneFader instance;
    public const float FadeOut = 0.2f;
    public const float FadeIn = 0.28f;

    CanvasGroup group;
    bool busy;
    bool hasPending;
    int pendingIndex;
    string pendingName;

    static SceneFader Get()
    {
        if (instance != null) return instance;
        var go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        instance = go.AddComponent<SceneFader>();
        instance.group = go.AddComponent<CanvasGroup>();
        instance.group.alpha = 0f;
        instance.group.blocksRaycasts = false;
        var img = new GameObject("Black", typeof(RectTransform)).AddComponent<Image>();
        img.transform.SetParent(go.transform, false);
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        img.color = KakPalette.Murekkep;
        return instance;
    }

    public static void Load(int buildIndex) => Get().StartLoad(buildIndex, null);
    public static void Load(string sceneName) => Get().StartLoad(-1, sceneName);

    void StartLoad(int index, string name)
    {
        if (busy)
        {
            // Geçiş sürerken gelen istek kaybolmasın (ör. menü belirirken OYNA'ya hızlı dokunuş)
            hasPending = true;
            pendingIndex = index;
            pendingName = name;
            return;
        }
        StartCoroutine(Run(index, name));
    }

    IEnumerator Run(int index, string name)
    {
        busy = true;
        group.blocksRaycasts = true;
        for (float t = 0f; t < FadeOut; t += Time.unscaledDeltaTime)
        {
            group.alpha = t / FadeOut;
            yield return null;
        }
        group.alpha = 1f;
        KakTime.ResetAll();
        var op = index >= 0 ? SceneManager.LoadSceneAsync(index) : SceneManager.LoadSceneAsync(name);
        while (!op.isDone) yield return null;
        yield return null;
        for (float t = 0f; t < FadeIn; t += Time.unscaledDeltaTime)
        {
            group.alpha = 1f - t / FadeIn;
            yield return null;
        }
        group.alpha = 0f;
        group.blocksRaycasts = false;
        busy = false;

        if (hasPending)
        {
            hasPending = false;
            StartCoroutine(Run(pendingIndex, pendingName));
        }
    }
}
