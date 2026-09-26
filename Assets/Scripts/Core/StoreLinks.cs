/// <summary>
/// Mağaza ve yasal bağlantılar. Gizlilik politikası iki mağazada da zorunlu ve Apple 5.1.1 gereği uygulama içinden
/// erişilebilir olmalı (Ayarlar → Gizlilik Politikası). GitHub Pages açılırsa URL'yi buradan değiştir.
/// </summary>
public static class StoreLinks
{
    public const string PrivacyUrl = "https://github.com/Atakaanm/KAK/blob/main/Docs/Gizlilik.md";

    public static void OpenPrivacy() => UnityEngine.Application.OpenURL(PrivacyUrl);
}
