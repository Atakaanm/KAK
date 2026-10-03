/// <summary>
/// Faz 14: ürün kapsamı — odak sürümünde gizlenen sistemler. Kullanıcı (2026-10-04): "AI slop, çok fazla; nişleştirelim".
/// Karar: tek dünya (Taş Zindanı) kusursuz, meta = altın + karakter gelişimi, 5 eşya / 3 olay, 2 kişilik ikinci planda.
/// Kapalı sistemlerin kodu durur; burada açılınca (cilalanıp) güncellemeyle döner. Ayrıntı: prompts/faz-14-odak-kalite.md
/// </summary>
public static class KakScope
{
    /// <summary>Buz Gölü, Karanlık Mağara (+ meşale), Futbol Arenası (+ gol fırsatı) ve DÜNYALAR paneli.</summary>
    public static bool Worlds = false;
    public static bool Pets = false;
    public static bool Missions = false;
    public static bool DailyReward = false;
    /// <summary>Sonsuz modun "Sessizlik" olayı (3 olay kalır: Taş Yağmuru, Çapraz Ateş, Yuvarlanan Kaya).</summary>
    public static bool CalmEvent = false;

    /// <summary>Özellik ürün kapsamında mı (kapsam dışıysa hiç açılmaz, menüde görünmez).</summary>
    public static bool Includes(Feature f)
    {
        switch (f)
        {
            case Feature.Worlds: return Worlds;
            case Feature.Pets: return Pets;
            case Feature.Missions: return Missions;
            case Feature.DailyReward: return DailyReward;
            default: return true;
        }
    }

    /// <summary>Gizli sistemlerin kendi testleri için: her şeyi aç.</summary>
    public static void EnableAll() { Worlds = Pets = Missions = DailyReward = CalmEvent = true; }

    /// <summary>Ürün kapsamına dön (test sıfırlaması).</summary>
    public static void ResetToProduct() { Worlds = Pets = Missions = DailyReward = CalmEvent = false; }
}
