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

    /// <summary>
    /// Histerezisli yön: açı mevcut dilimin sınırını <paramref name="hysteresisDeg"/> kadar geçmeden dilim değişmez.
    /// Joystick çapraz sınırda titreyince sprite'ın her karede iki yön arasında gidip gelmesini önler (G1).
    /// </summary>
    public static int OctantSticky(Vector2 dir, int current, float hysteresisDeg = 12f)
    {
        if (dir.sqrMagnitude < 1e-6f) return current < 0 ? 6 : current;
        if (current < 0 || current > 7) return Octant(dir);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float delta = Mathf.Abs(Mathf.DeltaAngle(angle, current * 45f));
        return delta <= 22.5f + hysteresisDeg ? current : Octant(dir);
    }

    public static T PickOctant<T>(int octant, T east, T northEast, T north, T northWest, T west, T southWest, T south, T southEast)
    {
        switch (octant)
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
