using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adım adım açılan bir özelliğin menü butonu (FeatureGate).
///  - Kilitli: soluk, kilit ikonu, altında kalan koşul ("2 oyun sonra"); dokununca sallanır, açılmaz
///  - Yeni açıldı: köşede nabız atan "YENİ!" rozeti; ilk dokunuşta tanıtılmış sayılır
/// MainMenuController panel açmadan önce TryUse() çağırır.
/// </summary>
public class FeatureButton : MonoBehaviour
{
    public Feature feature;
    [Tooltip("Kilitliyken soluklaşacak grafikler (zemin, ikon, yazı)")]
    public Graphic[] tintTargets;
    public RectTransform newBadge;
    public TMP_Text lockHint;
    public Image icon;
    public Sprite lockSprite;

    Color[] baseColors;
    Sprite baseIcon;
    float shake, badgeT;

    public bool Locked => !FeatureGate.IsUnlocked(feature);

    void Awake()
    {
        if (tintTargets != null)
        {
            baseColors = new Color[tintTargets.Length];
            for (int i = 0; i < tintTargets.Length; i++) if (tintTargets[i] != null) baseColors[i] = tintTargets[i].color;
        }
        if (icon != null) baseIcon = icon.sprite;
    }

    void OnEnable() { Loc.Changed += Refresh; Refresh(); }
    void OnDisable() => Loc.Changed -= Refresh;

    public void Refresh()
    {
        bool locked = Locked;
        if (tintTargets != null && baseColors != null)
            for (int i = 0; i < tintTargets.Length; i++)
                if (tintTargets[i] != null)
                {
                    Color c = baseColors[i];
                    tintTargets[i].color = locked ? new Color(c.r * 0.55f, c.g * 0.55f, c.b * 0.6f, c.a) : c;
                }
        if (icon != null && lockSprite != null) icon.sprite = locked ? lockSprite : baseIcon;
        if (lockHint != null)
        {
            lockHint.gameObject.SetActive(locked);
            if (locked) lockHint.text = FeatureGate.LockedHint(feature);
        }
        if (newBadge != null) newBadge.gameObject.SetActive(!locked && FeatureGate.IsNew(feature));
    }

    /// <summary>Kilitliyse sallanır ve false döner; yeniyse tanıtıldı olarak işaretler.</summary>
    public bool TryUse()
    {
        if (Locked) { shake = 1f; return false; }
        if (FeatureGate.IsNew(feature))
        {
            FeatureGate.MarkIntroduced(feature);
            SaveSystem.Save();
            Refresh();
        }
        return true;
    }

    void Update()
    {
        if (shake > 0f)
        {
            shake = Mathf.MoveTowards(shake, 0f, Time.unscaledDeltaTime * 3f);
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(shake * 40f) * 6f * shake);
        }
        if (newBadge != null && newBadge.gameObject.activeSelf)
        {
            badgeT += Time.unscaledDeltaTime;
            float s = 1f + 0.08f * Mathf.Sin(badgeT * 6f);
            newBadge.localScale = new Vector3(s, s, 1f);
        }
    }
}
