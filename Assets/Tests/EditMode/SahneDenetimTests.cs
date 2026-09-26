using System.Linq;
using NUnit.Framework;

/// <summary>
/// Denetim D5: sahnelerde eksik script ve kopuk referans olmamalı; bizim bileşenlerimizde boş kalan
/// alanlar yalnızca bilinçli olanlar olmalı (aşağıdaki liste). Yeni bir boş alan çıkarsa ya bağla ya da
/// gerekçesiyle listeye ekle. (Bu test, WorldPopup fontunun boş kalması gibi sessiz hataları yakalar.)
/// </summary>
public class SahneDenetimTests
{
    static readonly string[] BilincliBos =
    {
        "PlayerMovement2D.playerData",       // LevelData.playerData çalışma anında uygulanır
        "PlayerHealth.playerData",
        "CornerShooter.firePoint",           // yoksa fırlatıcının kendi konumu
        "CornerShooter.spawnerData",         // LevelData.spawnerDataList çalışma anında
        "CornerShooter.overrideProjectile",  // sadece bölüm modunda
        "LevelManager.currentLevel",         // çalışma anında atanır
        // Faz 2B: bölüm sonu ekranı henüz kurulmadı (kurulunca bunları listeden çıkar)
        "GameOverScreen.titleText", "GameOverScreen.scoreLabel", "GameOverScreen.starOn",
        "GameOverScreen.starOff", "GameOverScreen.nextButton",
    };

    [Test]
    public void Sahneler_EksikScriptVeKopukReferansYok()
    {
        foreach (var path in KakSceneAudit.Scenes)
        {
            var r = KakSceneAudit.Audit(path);
            Assert.AreEqual(0, r.missingScripts + r.brokenRefs, $"[{r.scene}] " + string.Join("\n", r.broken));
        }
    }

    [Test]
    public void Sahneler_BosAlanlar_SadeceBilincliOlanlar()
    {
        foreach (var path in KakSceneAudit.Scenes)
        {
            var r = KakSceneAudit.Audit(path);
            var beklenmedik = r.empty.Where(e => !BilincliBos.Any(b => e.EndsWith(" / " + b))).ToList();
            Assert.IsEmpty(beklenmedik, $"[{r.scene}] beklenmedik boş alanlar:\n" + string.Join("\n", beklenmedik));
        }
    }
}
