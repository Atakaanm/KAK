#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sahte ödüllü reklam (yalnızca editör / development build): akışları gerçek SDK olmadan denemek için.
/// Ekranı kaplayan "TEST REKLAMI" katmanı gösterir, geri sayımdan sonra ödül verir. Release build'de derlenmez.
/// </summary>
public class SimulatedAdProvider : IAdProvider
{
    public float duration = 2f;
    /// <summary>Testler: false → reklam kapatılmış/başarısız sayılır.</summary>
    public bool succeed = true;
    public bool ready = true;

    public void Initialize(Action<bool> done) => done?.Invoke(true);
    public bool IsRewardedReady => ready;
    public void LoadRewarded() { }

    public void ShowRewarded(Action<bool> result)
    {
        if (duration <= 0f) { result?.Invoke(succeed); return; }
        var go = new GameObject("SimulatedAd");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<Runner>().Run(this, result);
    }

    class Runner : MonoBehaviour
    {
        public void Run(SimulatedAdProvider p, Action<bool> result) => StartCoroutine(Show(p, result));

        IEnumerator Show(SimulatedAdProvider p, Action<bool> result)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;
            var bg = new GameObject("Bg", typeof(RectTransform)).AddComponent<Image>();
            bg.transform.SetParent(transform, false);
            var rt = bg.rectTransform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            bg.color = new Color(0.09f, 0.08f, 0.15f, 0.96f);
            var text = new GameObject("Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(transform, false);
            var tr = text.rectTransform; tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 64;
            for (float t = p.duration; t > 0f; t -= Time.unscaledDeltaTime)
            {
                text.SetText("TEST REKLAMI\n{0}", Mathf.CeilToInt(t));
                yield return null;
            }
            result?.Invoke(p.succeed);
            Destroy(gameObject);
        }
    }
}
#endif
