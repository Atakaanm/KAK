using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

/// <summary>Faz 12 H3: serbest kontrol düzeni (sürükle, boyut, sağ/sol el, varsayılan).</summary>
public class KontrolDuzeniTests
{
    [UnitySetUp] public IEnumerator SetUp() { KakTestUtil.ResetWorld(); yield return null; }
    [UnityTearDown] public IEnumerator TearDown() { ControlSettings.ResetLayout(); GameSettings.TwoPlayer = false; KakTestUtil.ResetWorld(); yield return null; }

    static Vector2 ScreenCenter(RectTransform rt) => RectTransformUtility.WorldToScreenPoint(null, rt.TransformPoint(rt.rect.center));
    static Vector2 Norm(Vector2 screen) => new Vector2(screen.x / Screen.width, screen.y / Screen.height);

    static PointerEventData Pointer(Vector2 pos) => new PointerEventData(EventSystem.current) { position = pos, pointerId = 7 };

    [UnityTest]
    public IEnumerator Varsayilan_SolEl_Aynalaninca_SagEl()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var joy = Object.FindAnyObjectByType<VirtualJoystick>();
        var btn = Object.FindAnyObjectByType<DashButton>(FindObjectsInactive.Include);
        Canvas.ForceUpdateCanvases();
        Assert.Less(Norm(ScreenCenter(joy.background)).x, 0.5f, "Varsayılanda joystick solda olmalı");

        ControlSettings.SetLayout(false, Vector2.zero, Vector2.zero, true, 1f, 1f);
        btn.gameObject.SetActive(true); // düğme eldivenle/yetenekle görünür; düzen açılınca uygulanır
        Canvas.ForceUpdateCanvases();
        Assert.Greater(Norm(ScreenCenter(joy.background)).x, 0.5f, "Aynalanınca joystick sağda olmalı");
        Assert.Less(((RectTransform)btn.transform).anchorMin.x, 0.5f, "Aynalanınca düğme solda olmalı");

        ControlSettings.ResetLayout();
        Canvas.ForceUpdateCanvases();
        Assert.Less(Norm(ScreenCenter(joy.background)).x, 0.5f);
    }

    [UnityTest]
    public IEnumerator Serbest_ArenaninUstune_Konur_Calisir_Varsayilana_Doner()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var joy = Object.FindAnyObjectByType<VirtualJoystick>();
        var btn = Object.FindAnyObjectByType<DashButton>(FindObjectsInactive.Include);
        var homeParent = joy.transform.parent;

        // Arenanın ortasına, büyük joystick; düğme sol-alt, küçük
        ControlSettings.SetLayout(true, new Vector2(0.5f, 0.6f), new Vector2(0.2f, 0.25f), false, 1.3f, 0.8f);
        btn.gameObject.SetActive(true);
        yield return null;
        Canvas.ForceUpdateCanvases();
        Vector2 c = Norm(ScreenCenter(joy.background));
        Assert.AreEqual(0.5f, c.x, 0.02f, "Joystick seçilen yerde değil (x)");
        Assert.AreEqual(0.6f, c.y, 0.02f, "Joystick seçilen yerde değil (y)");
        Assert.AreEqual(1.3f, joy.background.localScale.x, 0.001f);
        var brt = (RectTransform)btn.transform;
        Assert.AreEqual(new Vector2(0.2f, 0.25f), brt.anchorMin);
        Assert.AreEqual(230f * 0.8f, brt.sizeDelta.x, 1f, "Düğmenin dokunma alanı boyuta uymuyor");

        // Arenanın üstünde dokun-sürükle → oyuncu sağa gider
        Vector2 p = ScreenCenter(joy.background);
        var e = Pointer(p + new Vector2(0f, 20f));
        joy.OnPointerDown(e);
        e.position = p + new Vector2(200f, 20f);
        joy.OnDrag(e);
        Assert.Greater(joy.Direction.x, 0.8f, "Arenanın üstündeki joystick çalışmıyor");
        joy.OnPointerUp(e);
        Assert.AreEqual(Vector2.zero, joy.Direction);

        // Paneller hâlâ joystick'in üstünde (duraklat / oyun sonu)
        var root = joy.GetComponentInParent<Canvas>().rootCanvas.transform;
        Assert.AreSame(root, joy.transform.parent);
        var pause = root.Find("PausePanel");
        if (pause != null) Assert.Greater(pause.GetSiblingIndex(), joy.transform.GetSiblingIndex(), "Duraklat paneli joystick'in altında kaldı");

        ControlSettings.ResetLayout();
        Assert.AreSame(homeParent, joy.transform.parent, "Varsayılana dönünce joystick kontrol alanına dönmedi");
        Assert.AreEqual(1f, joy.background.localScale.x, 0.001f);
    }

    [UnityTest]
    public IEnumerator DuzenEkrani_Surukle_Boyut_Tamam_Kaydeder()
    {
        yield return KakTestUtil.LoadGameWithLevel();
        var ed = ControlLayoutEditor.Open(Object.FindAnyObjectByType<Canvas>().transform);
        Assert.IsNotNull(ed, "Düzen ekranı açılmadı (Resources/ControlLayoutEditor)");
        yield return null;
        Assert.IsFalse(ControlSettings.Custom);

        // Joystick'i sürükle: ekranın ortasına
        var h = ed.joystick;
        Vector2 from = ScreenCenter((RectTransform)h.transform);
        var e = Pointer(from);
        h.OnPointerDown(e);
        e.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        h.OnDrag(e);
        Assert.AreEqual(0.5f, h.Pos.x, 0.02f);
        Assert.AreEqual(0.5f, h.Pos.y, 0.02f);

        // Düğmeyi seç, boyutunu büyüt
        ed.button.OnPointerDown(Pointer(ScreenCenter((RectTransform)ed.button.transform)));
        ed.sizeSlider.value = 1.4f;
        Assert.AreEqual(1.4f, ed.button.Scale, 0.01f);

        ed.doneButton.onClick.Invoke();
        yield return null;
        Assert.IsTrue(ControlSettings.Custom, "TAMAM kaydetmedi");
        Assert.AreEqual(0.5f, ControlSettings.JoyPos.x, 0.02f);
        Assert.AreEqual(1.4f, ControlSettings.ButtonScale, 0.01f);
        Assert.IsTrue(Object.FindAnyObjectByType<ControlLayoutEditor>() == null, "Ekran kapanmadı");

        // VARSAYILAN + SAĞ/SOL → varsayılan yerleşim, aynalı (konumlar özel değil)
        ed = ControlLayoutEditor.Open(Object.FindAnyObjectByType<Canvas>().transform);
        yield return null;
        ed.defaultButton.onClick.Invoke();
        ed.mirrorButton.onClick.Invoke();
        Assert.Greater(ed.joystick.Pos.x, 0.5f, "SAĞ/SOL joystick'i sağa almadı");
        ed.doneButton.onClick.Invoke();
        yield return null;
        Assert.IsFalse(ControlSettings.Custom);
        Assert.IsTrue(ControlSettings.Mirrored);
        Assert.AreEqual(1f, ControlSettings.JoyScale, 0.001f);
    }

    [UnityTest]
    public IEnumerator Yumusak_JoystickRampasi_VeFizikAraDegerleme()
    {
        // Faz 12 H5: küçük itiş yavaş ve hassas, yarı yolda tam hız; oyuncu fiziği ara değerlemeli (takılma yok)
        yield return KakTestUtil.LoadGameWithLevel();
        var mv = Object.FindAnyObjectByType<PlayerMovement2D>();
        Assert.AreEqual(RigidbodyInterpolation2D.Interpolate, mv.GetComponent<Rigidbody2D>().interpolation);
        var joy = Object.FindAnyObjectByType<VirtualJoystick>();
        Canvas.ForceUpdateCanvases();
        Vector2 c = ScreenCenter(joy.background);
        var e = Pointer(c);
        joy.OnPointerDown(e);
        // Taban yarıçapı (ekran pikseli)
        Vector3[] corners = new Vector3[4];
        joy.background.GetWorldCorners(corners);
        float radius = (RectTransformUtility.WorldToScreenPoint(null, corners[2]).x - RectTransformUtility.WorldToScreenPoint(null, corners[0]).x) * 0.5f;
        Vector2 center = ScreenCenter(joy.background);
        e.position = center + new Vector2(radius * 0.2f, 0f);
        joy.OnDrag(e);
        float small = joy.Direction.magnitude;
        Assert.Greater(small, 0.25f, "Küçük itiş hiç yürütmüyor");
        Assert.Less(small, 0.75f, "Küçük itiş tam hız (rampa yok)");
        e.position = center + new Vector2(radius * 0.9f, 0f);
        joy.OnDrag(e);
        Assert.AreEqual(1f, joy.Direction.magnitude, 0.01f, "Tam itişte tam hız olmalı");
        joy.OnPointerUp(e);
    }

    [UnityTest]
    public IEnumerator IkiKisilikte_SerbestDuzenUygulanmaz()
    {
        ControlSettings.SetLayout(true, new Vector2(0.5f, 0.6f), new Vector2(0.2f, 0.25f), false, 1.2f, 1f);
        GameSettings.TwoPlayer = true;
        yield return KakTestUtil.LoadGameWithLevel();
        yield return null;
        var joys = Object.FindObjectsByType<VirtualJoystick>(FindObjectsSortMode.None);
        Assert.AreEqual(2, joys.Length);
        foreach (var j in joys)
            Assert.AreNotSame(j.GetComponentInParent<Canvas>().rootCanvas.transform, j.transform.parent, "İki kişilikte joystick kontrol alanında kalmalı");
    }
}
