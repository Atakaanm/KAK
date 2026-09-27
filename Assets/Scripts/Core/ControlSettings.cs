using System;
using UnityEngine;

/// <summary>Kontrol boyutu ayarı (G1): joystick ve aksiyon düğmesi ölçeği. Ayarlardan değişir, oyunda anında uygulanır.</summary>
public static class ControlSettings
{
    static readonly float[] Scales = { 0.8f, 1f, 1.25f };
    static readonly string[] Keys = { "size_s", "size_m", "size_l" };

    public static event Action Changed;

    public static int Level => Mathf.Clamp(SaveSystem.Data.settings.controlSize, 0, Scales.Length - 1);
    public static float Scale => Scales[Level];
    public static string LabelKey => Keys[Level];

    public static void Set(int level)
    {
        SaveSystem.Data.settings.controlSize = Mathf.Clamp(level, 0, Scales.Length - 1);
        SaveSystem.Save();
        Changed?.Invoke();
    }

    public static void Cycle() => Set((Level + 1) % Scales.Length);
}
