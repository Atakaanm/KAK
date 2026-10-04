using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>Faz 15 K1: tempo profili — hiçbir düğme bir saniyede %8'den fazla sıçramaz, eğriler tek yönlü, güç ve çırak ayarları.</summary>
public class TempoProfilTests
{
    TempoProfile p;

    [SetUp]
    public void SetUp()
    {
        p = AssetDatabase.LoadAssetAtPath<TempoProfile>("Assets/Data/Tempo/Endless_Tempo.asset");
        Assert.IsNotNull(p, "Tempo profili yok: KacAtaKac/Tempo Profilini Kur");
    }

    [Test]
    public void Dugmeler_SaniyedeSekizYuzdedenFazlaSicramaz()
    {
        foreach (float power in new[] { 0f, 0.5f, 1f })
        foreach (int games in new[] { 0, 1, 999 })
        {
            float ramp = p.RampFor(power, games), start = p.StartTempoFor(power);
            float[] prev = null;
            for (int t = 0; t <= 480; t++)
            {
                float tau = Mathf.Clamp01(start + (1f - start) * t / ramp);
                float app = p.ApprenticeSpeed(games, t);
                var v = new[]
                {
                    p.fireRate.Evaluate(tau) * app, p.projectileSpeed.Evaluate(tau) * app, p.projectileScale.Evaluate(tau),
                    p.playerSpeed.Evaluate(tau), p.scoreSpeed.Evaluate(tau), 1f + p.doubleShot.Evaluate(tau), p.eventInterval.Evaluate(tau)
                };
                if (prev != null)
                    for (int k = 0; k < v.Length; k++)
                    {
                        float rel = Mathf.Abs(v[k] - prev[k]) / Mathf.Max(0.01f, prev[k]);
                        Assert.LessOrEqual(rel, 0.08f, $"düğme {k} {t}. sn'de %{rel * 100f:F1} sıçradı (güç {power}, oyun {games})");
                    }
                prev = v;
            }
        }
    }

    [Test]
    public void Egriler_TekYonlu_TavanMakul()
    {
        float last = -1f;
        foreach (var c in new[] { p.fireRate, p.projectileSpeed, p.projectileScale, p.playerSpeed, p.scoreSpeed })
        {
            last = -1f;
            for (int i = 0; i <= 100; i++)
            {
                float v = c.Evaluate(i / 100f);
                Assert.GreaterOrEqual(v, last - 1e-4f, "Eğri tempo artarken düştü");
                last = v;
            }
        }
        Assert.LessOrEqual(p.eventInterval.Evaluate(1f), p.eventInterval.Evaluate(0f), "Olaylar tempoyla seyrekleşmemeli");
        Assert.That(p.fireRate.Evaluate(0f), Is.InRange(0.6f, 1.0f), "İlk saniyeler sakin olmalı");
        Assert.That(p.fireRate.Evaluate(1f), Is.InRange(4.5f, 7f), "Tavan atış hızı eski İmkansız kademesine yakın olmalı");
    }

    [Test]
    public void Firlaticilar_EsiklerdeUyanir()
    {
        for (int i = 1; i < p.throwerTempo.Length; i++) Assert.Greater(p.throwerTempo[i], p.throwerTempo[i - 1]);
        Assert.AreEqual(1, p.ThrowersAt(0f));
        Assert.AreEqual(2, p.ThrowersAt(0.1f));
        Assert.AreEqual(3, p.ThrowersAt(0.3f));
        Assert.AreEqual(4, p.ThrowersAt(0.9f));
    }

    [Test]
    public void Guc_ve_Cirak()
    {
        Assert.Less(p.RampFor(1f, 999), p.RampFor(0f, 999), "Güçlü oyuncunun rampası kısa olmalı");
        Assert.That(p.StartTempoFor(1f), Is.InRange(0.1f, 0.5f), "Güç tempoyu tam telafi etmemeli");
        Assert.Greater(p.RampFor(0f, 0), p.RampFor(0f, 999), "İlk oyunun rampası uzun olmalı");
        Assert.Less(p.ApprenticeSpeed(0, 0f), 1f);
        Assert.AreEqual(1f, p.ApprenticeSpeed(999, 0f), 1e-4f);
        Assert.AreEqual(1f, p.ApprenticeSpeed(0, p.apprenticeFadeSeconds), 1e-4f);
    }
}
