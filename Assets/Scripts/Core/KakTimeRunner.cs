using UnityEngine;

/// <summary>KakTime'ın zamanlayıcılarını (hit-stop) her kare ilerleten görünmez nesne.</summary>
public class KakTimeRunner : MonoBehaviour
{
    static KakTimeRunner instance;

    public static void Ensure()
    {
        if (instance != null) return;
        var go = new GameObject("KakTimeRunner") { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(go);
        instance = go.AddComponent<KakTimeRunner>();
    }

    void Update() => KakTime.Tick();
}
