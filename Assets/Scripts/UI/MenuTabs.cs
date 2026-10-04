using UnityEngine;

/// <summary>
/// Faz 15 K4: GELİŞİM merkezi sekmeleri (KAHRAMAN · GELİŞİM; K5'te EŞYA). Her sekme kendi tam ekran paneli;
/// panellerin üstündeki sekme düğmeleri buraya bağlanır. Son açılan sekme hatırlanır (oturum boyunca).
/// </summary>
public class MenuTabs : MonoBehaviour
{
    public GameObject[] pages; // 0 kahramanlar, 1 gelişim, (2 eşya)
    public static int Last = 1; // ilk açılışta GELİŞİM

    public GameObject Current => pages != null && Last >= 0 && Last < pages.Length ? pages[Last] : null;

    public void ShowHeroes() => Show(0);
    public void ShowProgress() => Show(1);
    public void ShowItems() => Show(2);

    public void Show(int i)
    {
        if (pages == null || i < 0 || i >= pages.Length || pages[i] == null) return;
        Last = i;
        for (int k = 0; k < pages.Length; k++)
            if (pages[k] != null) pages[k].SetActive(k == i);
        pages[i].transform.SetAsLastSibling();
        var am = AudioManager.Instance;
        if (am != null) am.PlayButtonClick();
    }

    public void OpenLast() => Show(Mathf.Clamp(Last, 0, pages != null ? pages.Length - 1 : 0));

    public void CloseAll()
    {
        if (pages == null) return;
        foreach (var p in pages) if (p != null) p.SetActive(false);
    }
}
