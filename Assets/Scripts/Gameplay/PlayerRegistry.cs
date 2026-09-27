using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sahnedeki oyuncular (G5: iki kişilik mod). Fırlatıcı nişanı, güdümlü taş, göktaşı, olaylar ve altın yerleşimi
/// tek bir hedef yerine buradan okur. Tek oyuncuda davranış eskisiyle aynı (tek kayıt).
/// PlayerHealth kendini OnEnable/OnDisable'da kaydeder. Tahsissiz okunur.
/// </summary>
public static class PlayerRegistry
{
    public static readonly List<PlayerHealth> All = new List<PlayerHealth>(2);

    public static void Register(PlayerHealth p) { if (p != null && !All.Contains(p)) All.Add(p); }
    public static void Unregister(PlayerHealth p) { All.Remove(p); }

    public static int AliveCount
    {
        get
        {
            int n = 0;
            for (int i = 0; i < All.Count; i++) if (All[i] != null && !All[i].IsDead) n++;
            return n;
        }
    }

    /// <summary>Konuma en yakın yaşayan oyuncu (yoksa null).</summary>
    public static Transform NearestAlive(Vector2 from)
    {
        Transform best = null;
        float bd = float.MaxValue;
        for (int i = 0; i < All.Count; i++)
        {
            var p = All[i];
            if (p == null || p.IsDead) continue;
            float d = ((Vector2)p.transform.position - from).sqrMagnitude;
            if (d < bd) { bd = d; best = p.transform; }
        }
        return best;
    }

    /// <summary>Rastgele yaşayan oyuncu (fırlatıcı nişanı: iki kişide taşlar ikisine de gelsin).</summary>
    public static Transform RandomAlive()
    {
        int alive = AliveCount;
        if (alive == 0) return null;
        int k = Random.Range(0, alive);
        for (int i = 0; i < All.Count; i++)
        {
            var p = All[i];
            if (p == null || p.IsDead) continue;
            if (k-- == 0) return p.transform;
        }
        return null;
    }
}
