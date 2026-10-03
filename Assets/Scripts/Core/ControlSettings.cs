using System;
using UnityEngine;

/// <summary>
/// Kontrol düzeni. G1: boyut (KÜÇÜK/ORTA/BÜYÜK). Faz 12 H3: serbest düzen — joystick ve aksiyon düğmesi ekranın
/// istenen yerine (arenanın üstü dahil), ayrı boyutlarla, sağ/sol el aynalaması. Ayarlar → KONTROLLERİ DÜZENLE
/// (ControlLayoutEditor). Oyunda VirtualJoystick ve DashButton uygular, Changed ile anında.
/// </summary>
public static class ControlSettings
{
    static readonly float[] LegacyScales = { 0.8f, 1f, 1.25f };
    public const float MinScale = 0.6f, MaxScale = 1.6f;

    public static event Action Changed;

    static SettingsData S => SaveSystem.Data.settings;
    static float Legacy => LegacyScales[Mathf.Clamp(S.controlSize, 0, LegacyScales.Length - 1)];

    /// <summary>Konumlar oyuncunun seçtiği yerde mi (false: oyunun varsayılan yerleşimi).</summary>
    public static bool Custom => S.controlCustom;
    /// <summary>Varsayılan yerleşim aynalı mı (sağ el: joystick sağda, düğme solda).</summary>
    public static bool Mirrored => S.controlMirror;
    public static float JoyScale => S.joyScale > 0f ? Mathf.Clamp(S.joyScale, MinScale, MaxScale) : Legacy;
    public static float ButtonScale => S.btnScale > 0f ? Mathf.Clamp(S.btnScale, MinScale, MaxScale) : Legacy;
    /// <summary>Eski çağrılar için (joystick ölçeği).</summary>
    public static float Scale => JoyScale;
    /// <summary>Ekran oranı (0-1): joystick dinlenme merkezi / düğme merkezi (yalnız Custom iken).</summary>
    public static Vector2 JoyPos => new Vector2(S.joyX, S.joyY);
    public static Vector2 ButtonPos => new Vector2(S.btnX, S.btnY);

    /// <summary>Düzeni kaydeder. customPositions false ise konumlar yok sayılır (varsayılan yerleşim + aynalama).</summary>
    public static void SetLayout(bool customPositions, Vector2 joy, Vector2 btn, bool mirror, float joyScale, float btnScale)
    {
        var s = S;
        s.controlCustom = customPositions;
        s.controlMirror = mirror;
        s.joyX = Mathf.Clamp01(joy.x); s.joyY = Mathf.Clamp01(joy.y);
        s.btnX = Mathf.Clamp01(btn.x); s.btnY = Mathf.Clamp01(btn.y);
        s.joyScale = Mathf.Clamp(joyScale, MinScale, MaxScale);
        s.btnScale = Mathf.Clamp(btnScale, MinScale, MaxScale);
        SaveSystem.Save();
        Changed?.Invoke();
    }

    /// <summary>Oyunun varsayılan düzeni (orta boy, aynasız).</summary>
    public static void ResetLayout()
    {
        var s = S;
        s.controlCustom = false; s.controlMirror = false;
        s.joyScale = 0f; s.btnScale = 0f; s.controlSize = 1;
        SaveSystem.Save();
        Changed?.Invoke();
    }

    /// <summary>
    /// Varsayılan yerleşimde joystick ve düğme merkezleri (ekran oranı). ScreenComposer'ın kare arena hesabıyla aynı:
    /// joystick kontrol alanının sol %60'ının ortasında (yükseklik %45), düğme %80'de.
    /// </summary>
    public static void DefaultPositions(ScreenComposer.Layout L, float screenH, bool mirror, out Vector2 joy, out Vector2 btn)
    {
        float cy = L.safeBottomPx + (L.controlPx - L.safeBottomPx) * 0.45f;
        float y = screenH > 0f ? cy / screenH : 0.15f;
        joy = new Vector2(mirror ? 0.7f : 0.3f, y);
        btn = new Vector2(mirror ? 0.2f : 0.8f, y);
    }

    /// <summary>Oyun ekranının yerleşimi: sahnede ScreenComposer varsa onun hesabı, yoksa (menü) aynı formül kare arenayla.</summary>
    public static ScreenComposer.Layout CurrentLayout()
    {
        var c = UnityEngine.Object.FindAnyObjectByType<ScreenComposer>();
        if (c != null && c.Current.uiScale > 0f) return c.Current;
        return ScreenComposer.Compute(Screen.width, Screen.height, Screen.safeArea, new Bounds(Vector3.zero, new Vector3(8.2f, 8.2f, 0f)),
                                      1080f, 210f, 520f);
    }

    /// <summary>Kayıttaki düzenin konumları (custom değilse varsayılan + aynalama).</summary>
    public static void Positions(out Vector2 joy, out Vector2 btn)
    {
        if (Custom) { joy = JoyPos; btn = ButtonPos; return; }
        DefaultPositions(CurrentLayout(), Screen.height, Mirrored, out joy, out btn);
    }
}
