using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Oyun kaydı: Application.persistentDataPath/save.json (atomik yazma: .tmp → yer değiştir).
/// İlk açılışta eski PlayerPrefs değerleri (BestScore, ses ayarları, sarsıntı) taşınır.
/// Kullanım: SaveSystem.Data.bestScoreEndless = x; SaveSystem.Save();
/// </summary>
public static class SaveSystem
{
    public const int CurrentVersion = 1;

    /// <summary>Testler için farklı dosya yolu.</summary>
    public static string OverridePath;

    public static string FilePath => !string.IsNullOrEmpty(OverridePath)
        ? OverridePath
        : Path.Combine(Application.persistentDataPath, "save.json");

    static SaveData data;
    public static SaveData Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
                if (data == null) throw new Exception("boş kayıt");
                data.Upgrade();
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SaveSystem] Kayıt okunamadı, yeni kayıt açılıyor: " + e.Message);
        }
        // Şirket/ürün adı değişince persistentDataPath değişir: eski konumdaki kaydı taşı (geliştirme makinesi)
        if (string.IsNullOrEmpty(OverridePath) && TryLoadLegacy()) { Save(); return; }

        data = new SaveData();
        MigrateFromPlayerPrefs(data);
        Save();
    }

    static bool TryLoadLegacy()
    {
        try
        {
            string root = Path.GetDirectoryName(Path.GetDirectoryName(Application.persistentDataPath));
            string legacy = Path.Combine(root, "DefaultCompany", "KacAtaKac", "save.json");
            if (!File.Exists(legacy) || legacy == FilePath) return false;
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(legacy));
            if (data == null) return false;
            data.Upgrade();
            Debug.Log("[SaveSystem] Eski konumdaki kayıt taşındı: " + legacy);
            return true;
        }
        catch { return false; }
    }

    public static void Save()
    {
        if (data == null) return;
        try
        {
            string path = FilePath;
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SaveSystem] Kayıt yazılamadı: " + e.Message);
        }
    }

    /// <summary>Bellekteki kaydı bırakır (bir sonraki erişimde dosyadan okunur). Testler için.</summary>
    public static void Unload() => data = null;

    /// <summary>Bugün ilk kez oynanıyorsa oyun günü sayısını artırır (yerel tarih).</summary>
    public static void TouchPlayDay()
    {
        int today = (int)(System.DateTime.Now.Date.Ticks / System.TimeSpan.TicksPerDay);
        var d = Data;
        if (d.lastPlayDay == today) return;
        d.lastPlayDay = today;
        d.playDays++;
        Save();
    }

    /// <summary>Tüm ilerlemeyi siler (Ayarlar → Sıfırla).</summary>
    public static void ResetAll()
    {
        data = new SaveData();
        Save();
    }

    static void MigrateFromPlayerPrefs(SaveData d)
    {
        d.bestScoreEndless = PlayerPrefs.GetInt("BestScore", 0);
        d.settings.music = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        d.settings.sfx = PlayerPrefs.GetInt("SfxOn", 1) == 1;
        d.settings.vibration = PlayerPrefs.GetInt("VibrationOn", 1) == 1;
        d.settings.screenShake = PlayerPrefs.GetInt("ScreenShakeOn", 1) == 1;
    }
}

[Serializable]
public class SaveData
{
    public int version = SaveSystem.CurrentVersion;

    [Header("Sonsuz Mod")]
    public int bestScoreEndless;
    public float bestTimeEndless;

    [Header("İstatistik")]
    public int gamesPlayed;
    public float totalPlaySeconds;
    public int totalPowerups;

    [Header("İlerleme")]
    public int coins;          // cüzdan (harcanabilir)
    public int totalCoins;     // şimdiye kadar kazanılan toplam (istatistik, açılma koşulları)
    public List<string> unlockedPets = new List<string>();
    public string selectedPet = "";
    public List<MissionState> missions = new List<MissionState>(); // aktif görevler (MissionSystem)
    public int missionsCompleted;
    public int dailyStreak;    // günlük ödül serisi (1-7), DailyReward
    public int lastClaimDay = -1;
    public int playDays;       // farklı günlerde oynama sayısı
    public int lastPlayDay;    // son oynanan gün (gün numarası)
    public string selectedCharacter = "Boy";
    public List<string> unlockedCharacters = new List<string> { "Boy" };

    public List<LevelProgress> levels = new List<LevelProgress>();

    public SettingsData settings = new SettingsData();

    // Bir kez gösterilen ipuçları ve adım adım açılan özellikler (id listesi)
    public List<string> seen = new List<string>();
    public bool HasSeen(string id) => seen != null && seen.Contains(id);
    public void MarkSeen(string id)
    {
        if (seen == null) seen = new List<string>();
        if (missions == null) missions = new List<MissionState>();
        if (unlockedPets == null) unlockedPets = new List<string>();
        if (selectedPet == null) selectedPet = "";
        if (!seen.Contains(id)) seen.Add(id);
    }

    public LevelProgress Level(string id)
    {
        if (levels == null) levels = new List<LevelProgress>();
        if (seen == null) seen = new List<string>();
        foreach (var l in levels) if (l.id == id) return l;
        return null;
    }

    public int Stars(string id) { var l = Level(id); return l != null ? l.stars : 0; }

    public int TotalStars
    {
        get { int n = 0; if (levels != null) foreach (var l in levels) n += l.stars; return n; }
    }

    /// <summary>Bölüm sonucunu kaydeder (daha iyi yıldız/süre korunur). Yeni rekor yıldızsa true.</summary>
    public bool RecordLevel(string id, int stars, float seconds)
    {
        var l = Level(id);
        if (l == null) { l = new LevelProgress { id = id }; levels.Add(l); }
        bool better = stars > l.stars;
        if (better) l.stars = stars;
        if (l.bestTime <= 0f || seconds < l.bestTime) l.bestTime = seconds;
        l.completed = true;
        return better;
    }

    public void Upgrade()
    {
        if (settings == null) settings = new SettingsData();
        if (levels == null) levels = new List<LevelProgress>();
        if (unlockedCharacters == null || unlockedCharacters.Count == 0) unlockedCharacters = new List<string> { "Boy" };
        if (string.IsNullOrEmpty(selectedCharacter)) selectedCharacter = "Boy";
        version = SaveSystem.CurrentVersion;
    }
}

[Serializable]
public class LevelProgress
{
    public string id;
    public int stars;
    public float bestTime;
    public bool completed;
}

[Serializable]
public class SettingsData
{
    public bool music = true;
    public bool sfx = true;
    public bool vibration = true;
    public bool screenShake = true;
    public string language = ""; // "" = cihaz dili, "TR" / "EN"
}
