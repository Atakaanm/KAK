using UnityEngine;

/// <summary>8 yönlü sprite seçimi için ortak yön hesabı (oyuncu, fırlatıcı ve ileride düşmanlar).</summary>
public static class DirectionUtil
{
    /// <summary>0=Doğu, 1=KuzeyDoğu, 2=Kuzey, 3=KuzeyBatı, 4=Batı, 5=GüneyBatı, 6=Güney, 7=GüneyDoğu</summary>
    public static int Octant(Vector2 dir)
    {
        if (dir.sqrMagnitude < 1e-6f) return 6;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // -180..180
        int i = Mathf.RoundToInt(angle / 45f);                    // -4..4
        return (i + 8) % 8;
    }

    public static T Pick<T>(Vector2 dir, T east, T northEast, T north, T northWest, T west, T southWest, T south, T southEast)
    {
        switch (Octant(dir))
        {
            case 0: return east;
            case 1: return northEast;
            case 2: return north;
            case 3: return northWest;
            case 4: return west;
            case 5: return southWest;
            case 7: return southEast;
            default: return south;
        }
    }
}
