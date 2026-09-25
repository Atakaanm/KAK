using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Piksel stilinde açma/kapama anahtarı. Ayar türüne bağlanır (SaveSystem + ilgili sistem).
/// </summary>
public class KakToggle : MonoBehaviour, IPointerClickHandler
{
    public enum Setting { Music, Sfx, Vibration, ScreenShake }

    public Setting setting;
    public Image background;
    public RectTransform knob;
    public Sprite onSprite, offSprite;
    public float knobTravel = 44f;

    void OnEnable() => Refresh();

    bool Value
    {
        get
        {
            var st = SaveSystem.Data.settings;
            switch (setting)
            {
                case Setting.Music: return st.music;
                case Setting.Sfx: return st.sfx;
                case Setting.Vibration: return st.vibration;
                default: return st.screenShake;
            }
        }
        set
        {
            var am = AudioManager.Instance;
            switch (setting)
            {
                case Setting.Music: if (am != null) am.SetMusicOn(value); else SaveSystem.Data.settings.music = value; break;
                case Setting.Sfx: if (am != null) am.SetSfxOn(value); else SaveSystem.Data.settings.sfx = value; break;
                case Setting.Vibration:
                    if (am != null) { am.SetVibrationOn(value); if (value) am.TriggerVibration(); }
                    else SaveSystem.Data.settings.vibration = value;
                    break;
                default: KakCameraShake.Enabled = value; if (value && KakCameraShake.Instance != null) KakCameraShake.Instance.Shake(0.15f, 0.25f); break;
            }
            SaveSystem.Save();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Value = !Value;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        Refresh();
    }

    public void Refresh()
    {
        bool on = Value;
        if (background != null) background.sprite = on ? onSprite : offSprite;
        if (knob != null) knob.anchoredPosition = new Vector2(on ? knobTravel * 0.5f : -knobTravel * 0.5f, 0f);
    }
}
