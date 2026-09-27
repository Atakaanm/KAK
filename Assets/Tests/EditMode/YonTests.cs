using NUnit.Framework;
using UnityEngine;

/// <summary>G1: çapraz koşuda sprite titremesi — histerezisli yön seçimi.</summary>
public class YonTests
{
    static Vector2 Deg(float d) => new Vector2(Mathf.Cos(d * Mathf.Deg2Rad), Mathf.Sin(d * Mathf.Deg2Rad));

    [Test]
    public void SinirdaTitreme_YonDegistirmez()
    {
        int o = DirectionUtil.OctantSticky(Deg(0f), -1);     // Doğu
        Assert.AreEqual(0, o);
        // 22,5° sınırı etrafında gidip gelen joystick: Doğu'da kalmalı
        foreach (float a in new[] { 20f, 25f, 21f, 30f, 24f }) o = DirectionUtil.OctantSticky(Deg(a), o);
        Assert.AreEqual(0, o);
        o = DirectionUtil.OctantSticky(Deg(40f), o);           // sınırı açıkça geçti → KuzeyDoğu
        Assert.AreEqual(1, o);
        foreach (float a in new[] { 25f, 20f, 26f }) o = DirectionUtil.OctantSticky(Deg(a), o);
        Assert.AreEqual(1, o, "geri dönüşte de titrememeli");
        o = DirectionUtil.OctantSticky(Deg(5f), o);
        Assert.AreEqual(0, o);
    }

    [Test]
    public void GirdiYoksa_YonKorunur()
    {
        Assert.AreEqual(3, DirectionUtil.OctantSticky(Vector2.zero, 3));
    }
}
