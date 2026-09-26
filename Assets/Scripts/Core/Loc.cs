using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basit yerelleştirme (TR/EN). Dil: kayıttaki ayar, yoksa cihaz dili (Türkçe → TR, diğer → EN).
/// Kullanım: Loc.T("play") · statik UI yazıları için LocText bileşeni.
/// Yeni metin: Table'a anahtar ekle (TR, EN).
/// </summary>
public static class Loc
{
    public enum Lang { TR, EN }

    public static event Action Changed;

    static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>
    {
        // Menü
        { "subtitle", new[] { "SONSUZ TAŞ YAĞMURU", "ENDLESS ROCK STORM" } },
        { "best", new[] { "EN İYİ  {0}", "BEST  {0}" } },
        { "stats", new[] { "{0} oyun  •  en uzun {1}:{2:00}", "{0} games  •  longest {1}:{2:00}" } },
        { "tagline", new[] { "Taşlardan kaç, rekoru kır!", "Dodge the rocks, beat your record!" } },
        { "play", new[] { "OYNA", "PLAY" } },
        { "character", new[] { "KARAKTER", "HERO" } },
        { "settings", new[] { "AYARLAR", "SETTINGS" } },
        { "levels_soon", new[] { "BÖLÜMLER • YAKINDA", "LEVELS • SOON" } },
        { "music", new[] { "Müzik", "Music" } },
        { "sfx", new[] { "Efektler", "Sound FX" } },
        { "vibration", new[] { "Titreşim", "Vibration" } },
        { "shake", new[] { "Ekran Sarsıntısı", "Screen Shake" } },
        { "language", new[] { "Dil", "Language" } },
        { "reset", new[] { "İLERLEMEYİ SIFIRLA", "RESET PROGRESS" } },
        { "reset_confirm", new[] { "EMİN MİSİN? TEKRAR DOKUN", "SURE? TAP AGAIN" } },
        { "close", new[] { "KAPAT", "CLOSE" } },
        { "selected", new[] { "SEÇİLİ", "SELECTED" } },
        { "soon", new[] { "YAKINDA", "SOON" } },
        // Oyun
        { "paused", new[] { "DURAKLATILDI", "PAUSED" } },
        { "resume", new[] { "DEVAM", "RESUME" } },
        { "restart", new[] { "YENİDEN BAŞLA", "RESTART" } },
        { "main_menu", new[] { "ANA MENÜ", "MAIN MENU" } },
        { "game_over", new[] { "OYUN BİTTİ", "GAME OVER" } },
        { "score", new[] { "SKOR", "SCORE" } },
        { "new_best", new[] { "YENİ REKOR!", "NEW BEST!" } },
        { "best_short", new[] { "EN İYİ {0}", "BEST {0}" } },
        { "retry", new[] { "TEKRAR", "RETRY" } },
        { "run_stats", new[] { "SÜRE {0}:{1:00}   YAKIN {2}   COMBO x{3:1}", "TIME {0}:{1:00}   CLOSE {2}   COMBO x{3:1}" } },
        { "hud_score", new[] { "SKOR: {0}", "SCORE: {0}" } },
        { "near_miss", new[] { "YAKIN!", "CLOSE!" } },
        { "super_dodge", new[] { "SÜPER KAÇIŞ!", "SUPER DODGE!" } },
        // Altın (Faz 3c.1)
        { "coins_run", new[] { "+{0} ALTIN   •   TOPLAM {1}", "+{0} GOLD   •   TOTAL {1}" } },
        { "hint_coin", new[] { "Altınları topla!", "Grab the gold!" } },
        // Adım adım açılan özellikler (FeatureGate)
        { "feat_Coins", new[] { "ALTIN", "GOLD" } },
        { "feat_Missions", new[] { "GÖREVLER", "MISSIONS" } },
        { "feat_Characters", new[] { "KARAKTERLER", "HEROES" } },
        { "feat_DailyReward", new[] { "GÜNLÜK ÖDÜL", "DAILY REWARD" } },
        { "feat_Pets", new[] { "PETLER", "PETS" } },
        { "locked_games", new[] { "{0} oyun sonra", "in {0} games" } },
        { "locked_tomorrow", new[] { "yarın", "tomorrow" } },
        { "new_badge", new[] { "YENİ!", "NEW!" } },
        { "unlock_new", new[] { "YENİ AÇILDI: {0}!", "UNLOCKED: {0}!" } },
        { "locked_toast", new[] { "Kilitli: {0}", "Locked: {0}" } },
        // Karakterler (Faz 3c.3)
        { "char_Boy", new[] { "ATA", "ATA" } },
        { "char_Swift", new[] { "ÇEVİK", "SWIFT" } },
        { "char_Tank", new[] { "TANK", "TANK" } },
        { "char_Lucky", new[] { "ŞANSLI", "LUCKY" } },
        { "trait_Boy", new[] { "Dengeli, her işe yarar", "Balanced all-rounder" } },
        { "trait_Swift", new[] { "Hızlı, küçük, sık dash", "Fast, small, quick dash" } },
        { "trait_Tank", new[] { "4 can, kalkanla başlar", "4 hearts, starts shielded" } },
        { "trait_Lucky", new[] { "+%25 altın, bol güçlendirme", "+25% gold, more power-ups" } },
        { "select", new[] { "SEÇ", "SELECT" } },
        { "stat_speed", new[] { "HIZ", "SPEED" } },
        { "stat_dash", new[] { "DASH", "DASH" } },
        // İlk oyun ipuçları (OnboardingHints)
        { "hint_move", new[] { "Parmağını sürükle,\ntaşlardan kaç!", "Drag your finger,\ndodge the rocks!" } },
        { "hint_dash", new[] { "DASH ile taşların\niçinden geç!", "DASH straight\nthrough rocks!" } },
        { "hint_near", new[] { "Kıl payı kaçış =\nbonus puan!", "Close call =\nbonus points!" } },
        { "pu_heal", new[] { "+1 CAN", "+1 LIFE" } },
        { "pu_shield", new[] { "KALKAN", "SHIELD" } },
        { "pu_speed", new[] { "HIZ!", "SPEED!" } },
        { "pu_slow", new[] { "YAVAŞ ÇEKİM", "SLOW-MO" } },
        { "pu_ghost", new[] { "HAYALET", "GHOST" } },
        { "ev_meteor", new[] { "TAŞ YAĞMURU!", "ROCK RAIN!" } },
        { "ev_crossfire", new[] { "ÇAPRAZ ATEŞ!", "CROSSFIRE!" } },
        { "ev_calm", new[] { "SESSİZLİK...", "SILENCE..." } },
        { "ev_rolling", new[] { "YUVARLANAN KAYA!", "ROLLING BOULDER!" } },
        // Bölümler
        { "levels", new[] { "BÖLÜMLER", "LEVELS" } },
        { "level_complete", new[] { "TEBRİKLER!", "LEVEL CLEAR!" } },
        { "level_failed", new[] { "Tekrar dene, yapabilirsin!", "Try again, you got this!" } },
        { "time", new[] { "SÜRE", "TIME" } },
        { "next", new[] { "SONRAKİ", "NEXT" } },
        { "goal_count", new[] { "GOL {0}/{1}", "GOALS {0}/{1}" } },
        { "goal_survive", new[] { "{0} SANİYE HAYATTA KAL!", "SURVIVE {0} SECONDS!" } },
        { "goal_score", new[] { "{0} GOL AT!", "SCORE {0} GOALS!" } },
        { "stars_needed", new[] { "{0} YILDIZ GEREKİR", "NEEDS {0} STARS" } },
        { "back", new[] { "GERİ", "BACK" } },
        { "goal", new[] { "GOOOL!", "GOAL!" } },
        { "st_frozen", new[] { "DONDUN!", "FROZEN!" } },
        { "st_yellow", new[] { "SARI KART!", "YELLOW CARD!" } },
        { "st_red", new[] { "KIRMIZI KART!", "RED CARD!" } },
        { "endless", new[] { "SONSUZ MOD", "ENDLESS" } },
        // Kademeler
        { "stage_Baslangic", new[] { "BAŞLANGIÇ", "WARM-UP" } },
        { "stage_Kolay", new[] { "KOLAY!", "EASY!" } },
        { "stage_Orta", new[] { "ORTA!", "MEDIUM!" } },
        { "stage_Zor", new[] { "ZOR!", "HARD!" } },
        { "stage_Cehennem", new[] { "CEHENNEM!", "INFERNO!" } },
        { "stage_Imkansiz", new[] { "İMKANSIZ!", "IMPOSSIBLE!" } },
    };

    static Lang? current;

    public static Lang Current
    {
        get
        {
            if (current == null)
            {
                string s = SaveSystem.Data.settings.language;
                if (s == "TR") current = Lang.TR;
                else if (s == "EN") current = Lang.EN;
                else current = Application.systemLanguage == SystemLanguage.Turkish ? Lang.TR : Lang.EN;
            }
            return current.Value;
        }
        set
        {
            current = value;
            SaveSystem.Data.settings.language = value.ToString();
            SaveSystem.Save();
            Changed?.Invoke();
        }
    }

    public static string T(string key)
    {
        if (Table.TryGetValue(key, out var v)) return v[(int)Current];
        return key;
    }

    public static bool Has(string key) => Table.ContainsKey(key);

    /// <summary>Tüm metinlerdeki karakterler (font atlasını önceden doldurmak için).</summary>
    public static string AllCharacters()
    {
        var sb = new System.Text.StringBuilder("0123456789:.x+!?•-");
        foreach (var kv in Table) foreach (var s in kv.Value) sb.Append(s);
        return sb.ToString();
    }

    /// <summary>Testler için dil önbelleğini sıfırlar.</summary>
    public static void ResetCache() => current = null;
}
