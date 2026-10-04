using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Faz 15 K7: tools/kak_hand_anchors.py çıktısını (Assets/Data/Anchors/hand_anchors.json) karakter başına HandAnchorSet'e yazar
/// ve karakter verisine bağlar (tekrar çalıştırılabilir). Önce: python3 tools/kak_hand_anchors.py
/// Köprü: invoke KakHandAnchorSetup Setup
/// </summary>
public static class KakHandAnchorSetup
{
    const string JsonPath = "Assets/Data/Anchors/hand_anchors.json";

    [System.Serializable] class Frame { public string path; public float dx, dy; public bool front, mirror; }
    [System.Serializable] class Char { public string name; public List<Frame> frames; }
    [System.Serializable] class Root { public List<Char> chars; }

    static readonly Dictionary<string, string> DataPaths = new Dictionary<string, string>
    {
        { "Boy", "Assets/Data/Boy_PlayerData.asset" },
        { "Ada", "Assets/Data/Characters/Ada_PlayerData.asset" },
        { "Swift", "Assets/Data/Characters/Swift_PlayerData.asset" },
        { "Tank", "Assets/Data/Characters/Tank_PlayerData.asset" },
        { "Lucky", "Assets/Data/Characters/Lucky_PlayerData.asset" },
    };

    [MenuItem("KacAtaKac/El Noktalarını Kur (Faz 15)")]
    public static string Setup()
    {
        if (!File.Exists(JsonPath)) return "yok: " + JsonPath + " (önce python3 tools/kak_hand_anchors.py)";
        var root = JsonUtility.FromJson<Root>(File.ReadAllText(JsonPath));
        int sets = 0, frames = 0, missing = 0;
        foreach (var c in root.chars)
        {
            var list = new List<HandAnchorSet.Entry>();
            foreach (var f in c.frames)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(f.path);
                if (sp == null) { missing++; continue; }
                list.Add(new HandAnchorSet.Entry { sprite = sp, offsetPx = new Vector2(f.dx, f.dy), front = f.front, mirror = f.mirror });
            }
            string path = "Assets/Data/Anchors/Hands_" + c.name + ".asset";
            var set = AssetDatabase.LoadAssetAtPath<HandAnchorSet>(path);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<HandAnchorSet>();
                AssetDatabase.CreateAsset(set, path);
            }
            set.entries = list.ToArray();
            EditorUtility.SetDirty(set);
            sets++; frames += list.Count;
            if (DataPaths.TryGetValue(c.name, out var dp))
            {
                var pd = AssetDatabase.LoadAssetAtPath<PlayerData>(dp);
                if (pd != null) { pd.hands = set; EditorUtility.SetDirty(pd); }
            }
        }
        AssetDatabase.SaveAssets();
        string msg = sets + " karakter, " + frames + " kare" + (missing > 0 ? " (" + missing + " sprite bulunamadı)" : "");
        Debug.Log("[KakHandAnchorSetup] " + msg);
        return msg;
    }
}
