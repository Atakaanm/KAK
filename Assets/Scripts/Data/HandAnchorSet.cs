using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Faz 15 K7: bir karakterin her karesinde sağ el noktası (eşya ele "cuk" otursun). tools/kak_hand_anchors.py üretir,
/// KakHandAnchorSetup bu SO'ya yazar. Nokta sprite merkezine göre piksel (sağ +, aşağı +); front = gövdenin önünde mi.
/// Koşu karelerinde eşyayı tutan kol sallanmaz: nokta duruş elinin gövde salınımıyla kaymış hâli.
/// </summary>
[CreateAssetMenu(fileName = "HandAnchors", menuName = "KacAtaKac/Hand Anchor Set")]
public class HandAnchorSet : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public Sprite sprite;
        public Vector2 offsetPx;
        public bool front;
        public bool mirror; // batı yönlü kare: eşya aynalanır
    }

    public Entry[] entries;

    Dictionary<Sprite, Entry> map;

    public bool TryGet(Sprite s, out Entry e)
    {
        if (map == null)
        {
            map = new Dictionary<Sprite, Entry>(entries != null ? entries.Length : 0);
            if (entries != null) foreach (var en in entries) if (en.sprite != null) map[en.sprite] = en;
        }
        if (s != null && map.TryGetValue(s, out e)) return true;
        e = default;
        return false;
    }

    void OnValidate() => map = null;
}
