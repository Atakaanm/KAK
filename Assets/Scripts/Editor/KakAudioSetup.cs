using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Ses kurulumu: Assets/Audio içe aktarma ayarları + Resources/AudioManager prefab'ı (tüm klipler bağlı).
/// Menü sahnesindeki eski AudioManager objesi kaldırılır (prefab her sahnede otomatik oluşur).
/// Menü: KacAtaKac/Sesleri Kur
/// </summary>
public class KakAudioSetup : AssetPostprocessor
{
    const string Prefab = "Assets/Resources/AudioManager.prefab";

    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith("Assets/Audio/")) return;
        var ai = (AudioImporter)assetImporter;
        bool music = assetPath.Contains("/Music/");
        ai.forceToMono = true;
        ai.loadInBackground = music;
        var s = ai.defaultSampleSettings;
        s.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
        s.compressionFormat = AudioCompressionFormat.Vorbis;
        s.quality = music ? 0.5f : 0.7f;
        ai.defaultSampleSettings = s;
    }

    [MenuItem("KacAtaKac/Sesleri Kur")]
    public static void SetupMenu() => Debug.Log(Setup());

    static AudioClip C(string p) => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + p);

    public static string Setup()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        foreach (var g in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" }))
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(g), ImportAssetOptions.ForceUpdate);
        var go = new GameObject("AudioManager");
        var music = go.AddComponent<AudioSource>();
        music.playOnAwake = false; music.loop = true;
        var sfx = go.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        var am = go.AddComponent<AudioManager>();
        am.musicSource = music; am.sfxSource = sfx;
        am.menuMusic = C("Music/menu_loop.wav");
        am.gameMusic = C("Music/game_loop.wav");
        am.buttonClickSfx = C("Sfx/click.wav");
        am.hitSfx = C("Sfx/hit.wav");
        am.deathSfx = C("Sfx/death.wav");
        am.shootSfx = C("Sfx/throw.wav");
        am.scoreSfx = C("Sfx/powerup.wav");
        am.nearMissSfx = C("Sfx/nearmiss.wav");
        am.dashSfx = C("Sfx/dash.wav");
        am.meteorSfx = C("Sfx/meteor.wav");
        am.crumbleSfx = C("Sfx/crumble.wav");
        am.shieldSfx = C("Sfx/shield.wav");
        am.stageSfx = C("Sfx/stage.wav");
        am.eventSfx = C("Sfx/event.wav");
        PrefabUtility.SaveAsPrefabAsset(go, Prefab);
        Object.DestroyImmediate(go);

        // Menü sahnesindeki eski obje
        var active = EditorSceneManager.GetActiveScene().path;
        EditorSceneManager.SaveOpenScenes();
        var menu = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        foreach (var old in Object.FindObjectsByType<AudioManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(old.gameObject);
        EditorSceneManager.MarkSceneDirty(menu);
        EditorSceneManager.SaveScene(menu);
        EditorSceneManager.OpenScene(string.IsNullOrEmpty(active) ? "Assets/Scenes/SampleScene.unity" : active, OpenSceneMode.Single);
        return "[KakAudioSetup] AudioManager prefab'ı: " + Prefab + " (12 efekt, 2 müzik)";
    }
}
