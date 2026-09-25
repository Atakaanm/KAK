using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

/// <summary>
/// Ekran kompozisyonu (ScreenComposer.Compute saf hesabı) ve kayan joystick testleri.
/// </summary>
public class ScreenLayoutTests
{
    static readonly Bounds Arena = new Bounds(new Vector3(0f, 4.5f, 0f), new Vector3(8.192f, 8.192f, 1f));
    const float Ref = 1080f, Hud = 210f, CtrlMin = 520f;

    static ScreenComposer.Layout L(float w, float h, Rect? safe = null)
        => ScreenComposer.Compute(w, h, safe ?? new Rect(0, 0, w, h), Arena, Ref, Hud, CtrlMin);

    [TestCase(1080, 1920)]
    [TestCase(1170, 2532)]
    [TestCase(1080, 2400)]
    [TestCase(1080, 2520)]
    [TestCase(1284, 2778)]
    public void Telefonlarda_ArenaGenisligeSigar_VeTamamenGorunur(int w, int h)
    {
        var l = L(w, h);
        Assert.IsTrue(l.arenaFitsWidth, "Telefonda arena genişliğe sığmalı");
        Assert.AreEqual(Arena.size.x, l.cameraWorld.width, 0.01f, "Arena ekran enini tam doldurmalı");
        Assert.LessOrEqual(Arena.max.y, l.cameraWorld.yMax + 0.001f, "Arena üstten kesiliyor");
        Assert.GreaterOrEqual(Arena.min.y, l.cameraWorld.yMin - 0.001f, "Arena alttan kesiliyor");
        Assert.GreaterOrEqual(l.controlPx, CtrlMin * l.uiScale - 1f, "Kontrol alanı minimumun altında");
        // HUD + arena + kontrol = ekran
        float arenaPx = Arena.size.y / (l.cameraWorld.height / h);
        Assert.AreEqual(h, l.hudPx + arenaPx + l.controlPx, 2f, "Bantlar ekranı tam doldurmuyor");
    }

    [Test]
    public void Tablet_KontrolAlaniKorunur_ArenaKuculur()
    {
        var l = L(1536, 2048);
        Assert.IsFalse(l.arenaFitsWidth);
        Assert.GreaterOrEqual(l.controlPx, CtrlMin * l.uiScale - 1f);
        Assert.Greater(l.cameraWorld.width, Arena.size.x, "Tablette yanlarda çerçeve görünmeli");
        Assert.LessOrEqual(Arena.max.y, l.cameraWorld.yMax + 0.001f);
    }

    [Test]
    public void Centik_HUDGuvenliAlanKadarAsagiIner()
    {
        var normal = L(1170, 2532);
        var notch = L(1170, 2532, new Rect(0, 102, 1170, 2532 - 102 - 141)); // üst 141, alt 102 px
        Assert.AreEqual(normal.hudPx + 141f, notch.hudPx, 0.5f, "Üst güvenli alan HUD bandına eklenmeli");
        Assert.AreEqual(102f, notch.safeBottomPx, 0.5f);
        Assert.Less(notch.controlPx, normal.controlPx, "Çentikte kontrol alanı küçülür, arena kaymaz ekrandan taşmaz");
        Assert.LessOrEqual(Arena.max.y, notch.cameraWorld.yMax - 141f * (notch.cameraWorld.height / 2532f) + 0.01f,
            "Arena çentiğin altına girmemeli");
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        KakTestUtil.ResetWorld();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Joystick_DokununcaYonVerir_BirakincaSifirlanir()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        KakTestUtil.MakePlayerSafe();
        var joy = Object.FindAnyObjectByType<VirtualJoystick>();
        Assert.IsNotNull(joy, "VirtualJoystick yok");
        var move = Object.FindAnyObjectByType<PlayerMovement2D>();
        Assert.AreSame(joy, move.joystick, "Oyuncu yeni joystick'e bağlı değil");

        var zone = (RectTransform)joy.transform;
        var es = EventSystem.current;
        Assert.IsNotNull(es, "EventSystem yok");

        // Bölgenin ortasına dokun, sağa sürükle
        Vector3[] c = new Vector3[4];
        zone.GetWorldCorners(c);
        Vector2 center = RectTransformUtility.WorldToScreenPoint(null, (c[0] + c[2]) * 0.5f);
        var ev = new PointerEventData(es) { position = center, pointerId = 0 };
        joy.OnPointerDown(ev);
        ev.position = center + new Vector2(200f, 0f);
        joy.OnDrag(ev);
        yield return null;
        Assert.Greater(joy.Direction.x, 0.9f, "Sağa sürükleyince yön sağ olmalı: " + joy.Direction);

        Vector3 p0 = move.transform.position;
        yield return KakTestUtil.WaitReal(0.4f);
        Assert.Greater(move.transform.position.x, p0.x + 0.2f, "Oyuncu joystick'le hareket etmedi");

        joy.OnPointerUp(ev);
        yield return null;
        Assert.AreEqual(Vector2.zero, joy.Direction, "Bırakınca yön sıfırlanmalı");
    }

    [UnityTest]
    public IEnumerator HudVeKontrol_ArenayaBinmez()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var composer = Object.FindAnyObjectByType<ScreenComposer>();
        Assert.IsNotNull(composer, "ScreenComposer yok");
        composer.ForceRecalculate();
        var cam = composer.GetComponent<Camera>();
        var arena = Object.FindAnyObjectByType<ArenaAutoLayout>();
        Bounds b = arena.arenaSpriteRenderer.bounds;
        Vector3 top = cam.WorldToScreenPoint(new Vector3(b.center.x, b.max.y, 0f));
        Vector3 bottom = cam.WorldToScreenPoint(new Vector3(b.center.x, b.min.y, 0f));

        Vector3[] c = new Vector3[4];
        composer.hudBand.GetWorldCorners(c);          // overlay canvas: dünya köşeleri = ekran pikseli
        Assert.GreaterOrEqual(c[0].y, top.y - 2f, "HUD bandı arenanın üstüne biniyor");
        composer.controlArea.GetWorldCorners(c);
        Assert.LessOrEqual(c[1].y, bottom.y + 2f, "Kontrol alanı arenanın üstüne biniyor");
    }
}
