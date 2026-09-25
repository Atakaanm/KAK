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
    public int coins;
    public string selectedCharacter = "Boy";
    public List<string> unlockedCharacters = new List<string> { "Boy" };

    public SettingsData settings = new SettingsData();

    public void Upgrade()
    {
        if (settings == null) settings = new SettingsData();
        if (unlockedCharacters == null || unlockedCharacters.Count == 0) unlockedCharacters = new List<string> { "Boy" };
        if (string.IsNullOrEmpty(selectedCharacter)) selectedCharacter = "Boy";
        version = SaveSystem.CurrentVersion;
    }
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
