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
        { "control_size", new[] { "Kontroller", "Controls" } },
        { "edit_controls", new[] { "DÜZENLE", "EDIT" } },
        { "ctl_title", new[] { "KONTROLLERİ DÜZENLE", "EDIT CONTROLS" } },
        { "ctl_hint", new[] { "Sürükle: yerini değiştir. Dokun: seç, boyutunu ayarla.", "Drag to move. Tap to select and resize." } },
        { "ctl_size", new[] { "BOYUT", "SIZE" } },
        { "ctl_mirror", new[] { "SAĞ / SOL", "LEFT / RIGHT" } },
        { "ctl_default", new[] { "VARSAYILAN", "DEFAULT" } },
        { "ctl_done", new[] { "TAMAM", "DONE" } },
        { "ctl_joy", new[] { "HAREKET", "MOVE" } },
        { "ctl_btn", new[] { "AKSİYON", "ACTION" } },
        { "ctl_arena", new[] { "ARENA", "ARENA" } },
        { "size_s", new[] { "KÜÇÜK", "SMALL" } },
        { "size_m", new[] { "ORTA", "MEDIUM" } },
        { "size_l", new[] { "BÜYÜK", "LARGE" } },
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
        { "run_stats", new[] { "SÜRE {0}:{1:00}   YAKIN GEÇİŞ {2}", "TIME {0}:{1:00}   CLOSE CALLS {2}" } },
        { "hud_score", new[] { "{0}", "{0}" } }, // Faz 14 Ö5: arcade gibi yalnız sayı ("SKOR:" etiketi gürültüydü)
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
        { "feat_Worlds", new[] { "BUZ GÖLÜ", "FROZEN LAKE" } },
        { "locked_games", new[] { "{0} oyun sonra", "in {0} games" } },
        { "locked_game_1", new[] { "{0} oyun sonra", "in {0} game" } },
        { "locked_tomorrow", new[] { "yarın", "tomorrow" } },
        { "new_badge", new[] { "YENİ!", "NEW!" } },
        { "unlock_new", new[] { "YENİ AÇILDI: {0}!", "UNLOCKED: {0}!" } },
        { "locked_toast", new[] { "Kilitli: {0}", "Locked: {0}" } },
        // Karakterler (Faz 3c.3)
        { "char_Boy", new[] { "ATA", "ATA" } },
        { "char_Ada", new[] { "ADA", "ADA" } },
        { "char_Swift", new[] { "ÇEVİK", "SWIFT" } },
        { "char_Tank", new[] { "TANK", "TANK" } },
        { "char_Lucky", new[] { "ŞANSLI", "LUCKY" } },
        { "trait_Boy", new[] { "Dengeli, 3 cana kadar gelişir", "Balanced, grows to 3 hearts" } },
        { "trait_Ada", new[] { "İnce ve hızlı, ama 2 can", "Slim and quick, but 2 hearts" } },
        { "trait_Swift", new[] { "Küçücük ve çok hızlı, 2 can", "Tiny and very fast, 2 hearts" } },
        { "trait_Tank", new[] { "Hantal ama 5 cana kadar, kalkanlı", "Bulky, up to 5 hearts, shielded" } },
        { "trait_Lucky", new[] { "+%25 altın, bol güçlendirme", "+25% gold, more power-ups" } },
        // G3: karakter gelişimi
        { "stat_health", new[] { "CAN", "HEARTS" } },
        { "stat_power", new[] { "GÜÇ SÜRESİ", "POWER TIME" } },
        { "stat_max", new[] { "EN ÜST", "MAX" } },
        { "costume_soon", new[] { "KOSTÜMLER · YAKINDA", "OUTFITS · SOON" } },
        { "upgrade_hint", new[] { "Altınla geliştir, adım adım güçlen", "Spend gold, grow step by step" } },
        { "pet_level", new[] { "SEV. {0}", "LV {0}" } },
        { "goal_hp", new[] { "{0} +1 CAN", "{0} +1 HEART" } },
        { "pu_shackle", new[] { "PRANGA!", "SHACKLED!" } },
        { "two_player", new[] { "2 KİŞİ", "2 PLAYERS" } },
        // G7: Buz dünyası
        { "pu_glove", new[] { "ELDİVEN!", "GLOVE!" } },
        { "pu_boots", new[] { "BUZ AYAKKABISI!", "ICE BOOTS!" } },
        { "pu_fire", new[] { "ISINDIN!", "WARMED UP!" } },
        { "caught", new[] { "YAKALADIN!", "CAUGHT!" } },
        { "frozen", new[] { "DONDUN!", "FROZEN!" } },
        { "cold_source", new[] { "Soğuk", "Cold" } },
        { "cold", new[] { "SOĞUK", "COLD" } },
        { "worlds", new[] { "DÜNYALAR", "WORLDS" } },
        { "locked_short", new[] { "KİLİTLİ", "LOCKED" } },
        { "world_dungeon", new[] { "TAŞ ZİNDANI", "STONE DUNGEON" } },
        { "world_ice", new[] { "BUZ GÖLÜ", "FROZEN LAKE" } },
        { "worlddesc_dungeon", new[] { "Taş Muhafızlar taş fırlatır", "Stone Guardians hurl rocks" } },
        { "worlddesc_ice", new[] { "Kayarsın, kartopları büyür, ateş topla donma!", "You slide, snowballs grow, grab fire or freeze!" } },
        { "ice_intro", new[] { "BUZ GÖLÜ: kayarsın, ateş topla!", "FROZEN LAKE: you slide, grab fire!" } },
        // G6: hikâye
        { "story_1", new[] { "Ata ile Ada, unutulmuş Taş Zindanı'nda efsanevi altınları arıyordu.", "Ata and Ada were searching the forgotten Stone Dungeon for legendary gold." } },
        { "story_2", new[] { "İlk altına dokundukları an Taş Muhafızlar uyandı!", "The moment they touched the first coin, the Stone Guardians woke up!" } },
        { "story_3", new[] { "Lanet onları her yerde kovalıyor. Kaç, altınları topla, laneti kır!", "The curse chases them everywhere. Run, grab the gold, break the curse!" } },
        { "story_next", new[] { "İLERİ", "NEXT" } },
        { "story_start", new[] { "BAŞLA", "START" } },
        { "story_skip", new[] { "ATLA", "SKIP" } },
        { "story", new[] { "HİKÂYE", "STORY" } },
        { "goal_speed", new[] { "{0} HIZ +1", "{0} SPEED +1" } },
        { "goal_power", new[] { "{0} GÜÇ +1", "{0} POWER +1" } },
        { "select", new[] { "SEÇ", "SELECT" } },
        { "stat_speed", new[] { "HIZ", "SPEED" } },
        { "stat_dash", new[] { "DASH", "DASH" } },
        // Görevler (Faz 3c.4)
        { "missions_title", new[] { "GÖREVLER", "MISSIONS" } },
        { "mission_SurviveSeconds", new[] { "Tek oyunda {0} sn hayatta kal", "Survive {0}s in one run" } },
        { "mission_NearMisses", new[] { "Tek oyunda {0} yakın geçiş", "{0} close calls in one run" } },
        { "mission_Dashes", new[] { "Tek oyunda {0} kez dash at", "Dash {0} times in one run" } },
        { "mission_CoinsInRun", new[] { "Tek oyunda {0} altın topla", "Grab {0} gold in one run" } },
        { "mission_ReachStage", new[] { "{0} kademesine ulaş", "Reach {0}" } },
        { "mission_ShieldBlocks", new[] { "Kalkanla {0} taş engelle", "Block {0} rocks with a shield" } },
        { "mission_ShieldBlocks_1", new[] { "Kalkanla {0} taş engelle", "Block {0} rock with a shield" } },
        { "mission_PlayGames", new[] { "{0} oyun oyna", "Play {0} games" } },
        // Pet'ler (Faz 3c.5)
        { "pets_title", new[] { "PETLER", "PETS" } },
        { "pet_button", new[] { "PET", "PET" } },
        { "pet_Firefly", new[] { "ATEŞBÖCEĞİ", "FIREFLY" } },
        { "pet_Turtle", new[] { "KAPLUMBAĞA", "TURTLE" } },
        { "pettrait_Firefly", new[] { "Yakındaki altını çeker", "Pulls nearby gold" } },
        { "pettrait_Turtle", new[] { "Arada bir senin yerine vurulur", "Takes a hit for you now and then" } },
        { "remove", new[] { "ÇIKAR", "REMOVE" } },
        // Günlük ödül (Faz 3c.6)
        { "daily_title", new[] { "GÜNLÜK ÖDÜL", "DAILY REWARD" } },
        { "daily_day", new[] { "GÜN {0}", "DAY {0}" } },
        { "daily_claim", new[] { "AL", "CLAIM" } },
        { "daily_hint", new[] { "Her gün gel, ödül büyüsün!", "Come back daily for bigger rewards!" } },
        // Cila (Faz 3c.7)
        { "goal_progress", new[] { "{0}: {1}/{2}", "{0}: {1}/{2}" } },
        { "goal_ready", new[] { "{0} alınabilir!", "{0} is ready to buy!" } },
        // Reklam (Faz Y3)
        { "continue_title", new[] { "DEVAM ET?", "CONTINUE?" } },
        { "continue_sub", new[] { "Reklam izle, 1 canla devam et", "Watch an ad, continue with 1 heart" } },
        { "continue_watch", new[] { "İZLE", "WATCH" } },
        { "continue_no", new[] { "HAYIR", "NO THANKS" } },
        { "double_coins", new[] { "2×", "2×" } },
        { "privacy", new[] { "Gizlilik Politikası", "Privacy Policy" } },
        // İlk oyun ipuçları (OnboardingHints)
        { "hint_move", new[] { "Parmağını sürükle,\ntaşlardan kaç!", "Drag your finger,\ndodge the rocks!" } },
        { "hint_dash", new[] { "DASH ile taşların\niçinden geç!", "DASH straight\nthrough rocks!" } },
        { "hint_near", new[] { "Kıl payı kaçış =\nbonus puan!", "Close call =\nbonus points!" } },
        { "pu_heal", new[] { "+1 CAN", "+1 LIFE" } },
        { "pu_shield", new[] { "KALKAN", "SHIELD" } },
        { "pu_speed", new[] { "HIZ!", "SPEED!" } },
        { "pu_slow", new[] { "YAVAŞ ÇEKİM", "SLOW-MO" } },
        { "pu_ghost", new[] { "HAYALET", "GHOST" } },
        { "pu_invisible", new[] { "GÖRÜNMEZ!", "INVISIBLE!" } },
        { "ev_meteor", new[] { "TAŞ YAĞMURU!", "ROCK RAIN!" } },
        { "ev_icicle", new[] { "SARKIT YAĞMURU!", "ICICLE RAIN!" } },
        { "ev_cavein", new[] { "GÖÇÜK!", "CAVE-IN!" } },
        { "ev_swarm", new[] { "YARASA SÜRÜSÜ!", "BAT SWARM!" } },
        { "ev_bigball", new[] { "DEV TOP!", "GIANT BALL!" } },
        { "ev_bottles", new[] { "ŞİŞE YAĞMURU!", "BOTTLE RAIN!" } },
        { "ev_counter", new[] { "KONTRA ATAK!", "COUNTER ATTACK!" } },
        { "ev_whistle", new[] { "HAKEM DÜDÜĞÜ!", "REFEREE'S WHISTLE!" } },
        { "ev_goalchance", new[] { "GOL FIRSATI!", "GOAL CHANCE!" } },
        { "to_goal", new[] { "KALEYE!", "TO THE GOAL!" } },
        { "goal_scored", new[] { "GOOOL!", "GOAAAL!" } },
        // Faz 15 K3: ortak gelişim
        { "grp_survival", new[] { "HAYATTA KALMA", "SURVIVAL" } },
        { "grp_movement", new[] { "HAREKET", "MOVEMENT" } },
        { "grp_gain", new[] { "KAZANÇ", "GAIN" } },
        { "grp_items", new[] { "EŞYA", "ITEMS" } },
        { "up_hearts", new[] { "Can", "Hearts" } },
        { "up_hearts_d", new[] { "+1 kalp (karakterin sınırına kadar)", "+1 heart (up to the hero's limit)" } },
        { "up_invuln", new[] { "Toparlanma", "Recovery" } },
        { "up_invuln_d", new[] { "Vurulunca +0,1 sn dokunulmazlık", "+0.1 s safety after a hit" } },
        { "up_slim", new[] { "İnce yapı", "Slim" } },
        { "up_slim_d", new[] { "Taşlar %3 daha zor vurur", "Rocks hit 3% less often" } },
        { "up_sense", new[] { "Sezgi", "Sixth sense" } },
        { "up_sense_d", new[] { "Uyarılar %8 daha erken", "Warnings come 8% earlier" } },
        { "up_startshield", new[] { "Hazır kalkan", "Ready shield" } },
        { "up_startshield_d", new[] { "Her oyuna kalkanla başla", "Start every run with a shield" } },
        { "up_shieldregen", new[] { "Kalkan yenilenmesi", "Shield regen" } },
        { "up_shieldregen_d", new[] { "Kalkansız kalınca kendiliğinden kalkan (60/45/35 sn)", "Free shield when you have none (60/45/35 s)" } },
        { "up_revive", new[] { "İkinci şans", "Second chance" } },
        { "up_revive_d", new[] { "Oyun başına bir kez yerinde kalk", "Get back up once per run" } },
        { "up_speed", new[] { "Hız", "Speed" } },
        { "up_speed_d", new[] { "+%3 hız", "+3% speed" } },
        { "up_magnet", new[] { "Mıknatıs", "Magnet" } },
        { "up_magnet_d", new[] { "Altınları uzaktan çek", "Pull gold from further away" } },
        { "up_coin", new[] { "Altın bereketi", "Gold rush" } },
        { "up_coin_d", new[] { "+%8 altın", "+8% gold" } },
        { "up_score", new[] { "Skor ustası", "Score master" } },
        { "up_score_d", new[] { "+%5 skor", "+5% score" } },
        { "up_nearbonus", new[] { "Kıl payı ustası", "Close-call pro" } },
        { "up_nearbonus_d", new[] { "Kıl payı bonusu +%20", "Close-call bonus +20%" } },
        { "up_nearradius", new[] { "Kıl payı mesafesi", "Close-call range" } },
        { "up_nearradius_d", new[] { "Kıl payı %6 uzaktan sayılır", "Close calls count from 6% further" } },
        { "up_combokeep", new[] { "Çarpan hafızası", "Combo memory" } },
        { "up_combokeep_d", new[] { "Vurulunca çarpanın daha çoğu kalır", "Keep more of your multiplier when hit" } },
        { "up_combomax", new[] { "Çarpan tavanı", "Combo cap" } },
        { "up_combomax_d", new[] { "Skor çarpanı +0,25 daha yükselir", "Score multiplier goes 0.25 higher" } },
        { "up_power", new[] { "Eşya ustası", "Power-up pro" } },
        { "up_power_d", new[] { "Güçlendirmeler +%6 uzun", "Power-ups last 6% longer" } },
        { "second_chance", new[] { "İKİNCİ ŞANS!", "SECOND CHANCE!" } },
        { "max", new[] { "TAM", "MAX" } },
        { "progress", new[] { "GELİŞİM", "UPGRADES" } },
        { "progress_level", new[] { "GELİŞİM SEVİYESİ {0}", "UPGRADE LEVEL {0}" } },
        { "progress_next", new[] { "Sıradaki açılış: seviye {0}", "Next unlock: level {0}" } },
        { "progress_all", new[] { "Bütün gelişimler açık", "Every upgrade unlocked" } },
        { "progress_shared", new[] { "Gelişimler bütün kahramanlar için ortak", "Upgrades are shared by every hero" } },
        { "up_unlock_at", new[] { "Gelişim seviyesi {0} olunca açılır", "Unlocks at upgrade level {0}" } },
        { "up_new_unlocked", new[] { "Yeni gelişim açıldı: {0}!", "New upgrade unlocked: {0}!" } },
        { "tab_heroes", new[] { "KAHRAMAN", "HEROES" } },
        { "tab_progress", new[] { "GELİŞİM", "UPGRADES" } },
        { "passive", new[] { "ÖZELLİK", "TRAIT" } },
        // Faz 15 K5: eşya geliştirme
        { "tab_items", new[] { "EŞYA", "ITEMS" } },
        // Faz 15 K6: anahtar ve sandık
        { "key_earned", new[] { "+1 ANAHTAR!", "+1 KEY!" } },
        { "keys_run", new[] { "+{0} ANAHTAR  •  TOPLAM {1}", "+{0} KEYS  •  TOTAL {1}" } },
        { "chests", new[] { "SANDIK", "CHESTS" } },
        { "chests_title", new[] { "SANDIKLAR", "CHESTS" } },
        { "chest_wood", new[] { "Ahşap sandık", "Wooden chest" } },
        { "chest_silver", new[] { "Gümüş sandık", "Silver chest" } },
        { "chest_gold", new[] { "Altın sandık", "Golden chest" } },
        { "chest_hint", new[] { "{0}-{1} altın · {2}-{3} parşömen", "{0}-{1} gold · {2}-{3} scrolls" } },
        { "chest_open", new[] { "AÇ", "OPEN" } },
        { "chest_pity", new[] { "{0} sandık sonra garanti: bir üst sandığın ödülü", "In {0} chests: the next chest tier's reward" } },
        { "chest_pity_now", new[] { "Sıradaki sandık garanti: bir üst ödül!", "Next chest is guaranteed a better reward!" } },
        { "chest_keys_hint", new[] { "Anahtar: oyunda 60, 120, 180. saniye ve yeni rekor", "Keys: survive 60, 120, 180 s or beat your best" } },
        { "chest_tap", new[] { "Dokun", "Tap" } },
        { "chest_ok", new[] { "TAMAM", "OK" } },
        { "reward_gold", new[] { "+{0} altın", "+{0} gold" } },
        { "reward_scrolls", new[] { "+{0} parşömen", "+{0} scrolls" } },
        { "reward_keys", new[] { "+{0} anahtar", "+{0} key" } },
        { "reward_pity", new[] { "GARANTİ ÖDÜL!", "GUARANTEED BONUS!" } },
        // Faz 15 K8: kaçış kartları
        { "perk_title", new[] { "SEVİYE ATLADIN!", "LEVEL UP!" } },
        { "perk_pick", new[] { "Bir kaçış yeteneği seç", "Pick an escape skill" } },
        { "perk_magnet", new[] { "Mıknatıs", "Magnet" } },
        { "perk_magnet_d", new[] { "Altınlar sana gelir", "Gold flies to you" } },
        { "perk_shieldregen", new[] { "Kalkan yenilenmesi", "Shield regen" } },
        { "perk_shieldregen_d", new[] { "Kalkansızken kendiliğinden kalkan", "Free shield when you have none" } },
        { "perk_swift", new[] { "Çevik adım", "Swift step" } },
        { "perk_swift_d", new[] { "+%5 hız", "+5% speed" } },
        { "perk_gold", new[] { "Altın avcısı", "Gold hunter" } },
        { "perk_gold_d", new[] { "+%15 altın", "+15% gold" } },
        { "perk_score", new[] { "Skor ateşi", "Score fire" } },
        { "perk_score_d", new[] { "+%10 skor", "+10% score" } },
        { "perk_closecall", new[] { "Kıl payı", "Close call" } },
        { "perk_closecall_d", new[] { "Kıl payı +%30 puan, daha uzaktan", "+30% close-call points, wider" } },
        { "perk_slim", new[] { "İnce yapı", "Slim" } },
        { "perk_slim_d", new[] { "Taşlar %5 daha zor vurur", "Rocks hit 5% less" } },
        { "perk_recover", new[] { "Toparlanma", "Recovery" } },
        { "perk_recover_d", new[] { "Vurulunca daha uzun güvende", "Longer safety after a hit" } },
        { "perk_sense", new[] { "Sezgi", "Sixth sense" } },
        { "perk_sense_d", new[] { "Uyarılar %12 erken", "Warnings 12% earlier" } },
        { "perk_heart", new[] { "Kalp parçası", "Heart piece" } },
        { "perk_heart_d", new[] { "+1 can (doluysa +1 kalp)", "+1 life (if full, +1 heart)" } },
        { "perk_items", new[] { "Eşya yağmuru", "Item shower" } },
        { "perk_items_d", new[] { "Eşyalar %20 sık", "Power-ups 20% more often" } },
        { "perk_breaker", new[] { "Taş kırıcı", "Rock breaker" } },
        { "perk_breaker_d", new[] { "Arada bir yakındaki taşı kırar", "Now and then smashes a nearby rock" } },
        { "perk_ghost", new[] { "Hayalet anı", "Ghost moment" } },
        { "perk_ghost_d", new[] { "Vurulunca kısa süre hayalet", "Briefly ghost after a hit" } },
        // Faz 15 K7: ekipman (altyapı)
        { "gear_torch", new[] { "Meşale", "Torch" } },
        { "gear_wood_sword", new[] { "Tahta kılıç", "Wooden sword" } },
        { "gear_leather_hat", new[] { "Deri şapka", "Leather hat" } },
        { "gear_light_shoes", new[] { "Hafif ayakkabı", "Light shoes" } },
        { "gear_wood_shield", new[] { "Tahta kalkan", "Wooden shield" } },
        { "scrolls", new[] { "PARŞÖMEN", "SCROLLS" } },
        { "it_heal", new[] { "Kalp", "Heart" } },
        { "it_shield", new[] { "Kalkan", "Shield" } },
        { "it_slow", new[] { "Yavaş çekim", "Slow-mo" } },
        { "it_ghost", new[] { "Hayalet", "Ghost" } },
        { "it_shackle", new[] { "Pranga (kaçın!)", "Shackle (avoid!)" } },
        { "it_dur", new[] { "Süre", "Duration" } },
        { "it_dur_d", new[] { "+%12 etki süresi", "+12% effect time" } },
        { "it_freq", new[] { "Sıklık", "Frequency" } },
        { "it_freq_d", new[] { "%15 daha sık çıkar", "Shows up 15% more often" } },
        { "it_ground", new[] { "Yerde kalma", "Ground time" } },
        { "it_ground_d", new[] { "Yerde %20 daha uzun bekler", "Waits 20% longer on the ground" } },
        { "it_heal_pow", new[] { "Şifa ışığı", "Healing glow" } },
        { "it_heal_pow_d", new[] { "Alınca 0,8 sn dokunulmazlık", "0.8 s of safety on pickup" } },
        { "it_shield_pow", new[] { "Güçlü kalkan", "Strong shield" } },
        { "it_shield_pow_d", new[] { "Kalkan 2 vuruş emer", "Shield blocks 2 hits" } },
        { "it_slow_pow", new[] { "Derin yavaşlık", "Deep slow" } },
        { "it_slow_pow_d", new[] { "Zaman %5 daha yavaş akar", "Time flows 5% slower" } },
        { "it_ghost_pow", new[] { "Hayalet mıknatısı", "Ghost magnet" } },
        { "it_ghost_pow_d", new[] { "Hayaletken altınları çeker", "Pulls gold while ghosted" } },
        { "it_shackle_res", new[] { "Pranga direnci", "Shackle resist" } },
        { "it_shackle_res_d", new[] { "Pranga %15 kısa sürer", "Shackle lasts 15% shorter" } },
        { "it_shackle_freq", new[] { "Seyrek pranga", "Rare shackle" } },
        { "it_shackle_freq_d", new[] { "Pranga %12 seyrek çıkar", "Shackle shows up 12% less" } },
        { "it_summary", new[] { "Süre ×{0:0.00} · Sıklık ×{1:0.00} · Yerde ×{2:0.00}", "Time ×{0:0.00} · Freq ×{1:0.00} · Ground ×{2:0.00}" } },
        { "legacy_refund", new[] { "Gelişim yenilendi! {0} altın iade edildi.", "Upgrades are new! {0} gold refunded." } },
        { "pas_boy", new[] { "Dengeli: güçlendirmeler %10 uzun", "Balanced: power-ups last 10% longer" } },
        { "pas_ada", new[] { "Kıl payı ustası: kıl payı bonusu +%25", "Close-call pro: +25% close-call bonus" } },
        { "pas_swift", new[] { "Sezgili: uyarılar %15 erken, ince yapı", "Intuitive: warnings 15% earlier, slim body" } },
        { "pas_tank", new[] { "Dayanıklı: kalkanla başlar, uzun toparlanır", "Sturdy: starts shielded, recovers longer" } },
        { "pas_lucky", new[] { "Şanslı: +%25 altın, sık eşya, mıknatıs", "Lucky: +25% gold, more power-ups, magnet" } },
        { "ball_lost", new[] { "TOP KAÇTI!", "BALL LOST!" } },
        { "world_football", new[] { "FUTBOL ARENASI", "FOOTBALL ARENA" } },
        { "worlddesc_football", new[] { "Sarı kart peşinden gelir. 2 sarı ya da kırmızı = oyundan atıldın!", "Yellow cards chase you. 2 yellows or a red = you're out!" } },
        { "world_cave", new[] { "KARANLIK MAĞARA", "DARK CAVE" } },
        { "worlddesc_cave", new[] { "Meşaleni yak, karanlıkta kaç. Muhafızların gözlerine dikkat!", "Light your torch and run in the dark. Watch the guardians' eyes!" } },
        { "torch_level", new[] { "MEŞALE {0}", "TORCH {0}" } },
        { "torch_max", new[] { "MEŞALE EN ÜST", "TORCH MAX" } },
        { "coins_word", new[] { "ALTIN", "GOLD" } },
        { "ev_snowroll", new[] { "DEV KARTOPU!", "GIANT SNOWBALL!" } },
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
        { "st_out", new[] { "OYUNDAN ATILDIN!", "YOU'RE OUT!" } },
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
