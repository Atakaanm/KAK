using UnityEditor;
using UnityEngine;

/// <summary>
/// Faz 15 K1: sonsuz modun tempo profilini kurar ve Zindan sonsuz bölümüne bağlar (tekrar çalıştırılabilir).
/// Eğriler eski 6 kademenin değerlerinden türetildi ama sürekli: ilk ~75 sn eskisinden yumuşak (ilk oyun yavaş),
/// sonu eskisine yakın. Fırlatıcılar eski sürelerde (30 / 75 / 240 sn) uyanır ama toplam atış hızı sıçramaz.
/// Köprü: invoke KakTempoSetup Setup
/// </summary>
public static class KakTempoSetup
{
    public const string ProfilePath = "Assets/Data/Tempo/Endless_Tempo.asset";
    const string LevelPath = "Assets/Data/Endless_Level1_LevelData.asset";

    [MenuItem("KacAtaKac/Tempo Profilini Kur (Faz 15)")]
    public static string Setup()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data/Tempo")) AssetDatabase.CreateFolder("Assets/Data", "Tempo");
        var p = AssetDatabase.LoadAssetAtPath<TempoProfile>(ProfilePath);
        if (p == null)
        {
            p = ScriptableObject.CreateInstance<TempoProfile>();
            AssetDatabase.CreateAsset(p, ProfilePath);
        }

        p.rampSeconds = 360f;
        p.rampSecondsFullPower = 300f;
        p.startTempoFullPower = 0.3f;
        p.powerWarmupSeconds = 20f;
        // τ = süre / 360 (güçsüz, çırak değil): 20 sn ≈ 0,056 · 30 sn ≈ 0,083 · 75 sn ≈ 0,208 · 150 sn ≈ 0,417 · 240 sn ≈ 0,667
        p.fireRate = Curve(0f, 0.85f, 0.056f, 1.05f, 0.111f, 1.45f, 0.208f, 2.6f, 0.417f, 3.5f, 0.667f, 4.8f, 1f, 5.6f);
        p.projectileSpeed = Curve(0f, 0.84f, 0.083f, 0.92f, 0.208f, 1.02f, 0.417f, 1.13f, 0.667f, 1.25f, 1f, 1.36f);
        p.projectileScale = Curve(0f, 1f, 0.083f, 1f, 0.208f, 1.04f, 0.417f, 1.08f, 0.667f, 1.13f, 1f, 1.2f);
        p.playerSpeed = Curve(0f, 1f, 0.083f, 1.02f, 0.208f, 1.04f, 0.417f, 1.06f, 0.667f, 1.08f, 1f, 1.1f);
        p.scoreSpeed = AnimationCurve.Linear(0f, 1f, 1f, 1.75f); // doğrusal: skor/sn = 1 + 0,75 τ (iki uçlu ClampedAuto S-eğrisi olurdu)
        p.doubleShot = Curve(0f, 0f, 0.45f, 0f, 0.667f, 0.08f, 1f, 0.18f);
        p.eventInterval = Curve(0f, 1f, 0.6f, 1f, 1f, 0.8f);
        p.throwerTempo = new[] { 0f, 0.083f, 0.208f, 0.667f };
        p.wakeSeconds = 10f;
        p.typeBlend = 0.5f;
        p.doubleShotDelay = 0.22f;
        p.breatherSeconds = 6f;
        p.breatherRateDrop = 0.15f;
        p.breatherSpeedDrop = 0.06f;
        p.mercySeconds = 8f;
        p.apprenticeRampMult = new[] { 1.35f, 1.2f, 1.1f };
        p.apprenticeSpeed = new[] { 0.85f, 0.9f, 0.95f };
        p.apprenticeFadeSeconds = 60f;
        p.musicPitchAtMax = 1.04f;
        EditorUtility.SetDirty(p);

        var level = AssetDatabase.LoadAssetAtPath<LevelData>(LevelPath);
        string msg = "profil: " + ProfilePath;
        if (level != null)
        {
            level.tempoProfile = p;
            EditorUtility.SetDirty(level);
            msg += " → " + level.name;
        }
        else msg += " (bölüm bulunamadı: " + LevelPath + ")";
        AssetDatabase.SaveAssets();
        Debug.Log("[KakTempoSetup] " + msg);
        return msg;
    }

    /// <summary>(τ, değer) çiftleri → aşma yapmayan (ClampedAuto) yumuşak eğri.</summary>
    static AnimationCurve Curve(params float[] kv)
    {
        var c = new AnimationCurve();
        for (int i = 0; i + 1 < kv.Length; i += 2) c.AddKey(kv[i], kv[i + 1]);
        for (int i = 0; i < c.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(c, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(c, i, AnimationUtility.TangentMode.ClampedAuto);
        }
        c.preWrapMode = WrapMode.ClampForever;
        c.postWrapMode = WrapMode.ClampForever;
        return c;
    }
}
