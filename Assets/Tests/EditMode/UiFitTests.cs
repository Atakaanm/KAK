using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Kısa/geniş ekranda modal paneller sığmalı (mağaza görüntüleri sırasında bulundu: 3×2 karakter paneli
/// 1080×1520'de üstten kesiliyordu). Kanvas genişliğe göre ölçeklendiği için kanvas yüksekliği = 1080 × en/boy.
/// </summary>
public class UiFitTests
{
    static Vector2 Canvas(float w, float h) => new Vector2(1080f, 1080f * h / w);

    [Test]
    public void UzunTelefonda_PanelKuculmez()
    {
        Assert.AreEqual(1f, UiFitToScreen.Scale(Canvas(1080, 2340), new Vector2(960, 1560), 40f));
        Assert.AreEqual(1f, UiFitToScreen.Scale(Canvas(1080, 1920), new Vector2(960, 1560), 40f));
    }

    [Test]
    public void KisaEkranda_PanelSigar()
    {
        foreach (var screen in new[] { new Vector2(1080, 1520), new Vector2(1536, 2048), new Vector2(1200, 1600) })
        {
            var area = Canvas(screen.x, screen.y);
            var panel = new Vector2(960, 1560);
            float s = UiFitToScreen.Scale(area, panel, 40f);
            Assert.Less(s, 1f, screen.ToString());
            Assert.LessOrEqual(panel.y * s, area.y - 80f + 0.01f, screen + " yükseklik");
            Assert.LessOrEqual(panel.x * s, area.x - 80f + 0.01f, screen + " genişlik");
        }
    }
}
