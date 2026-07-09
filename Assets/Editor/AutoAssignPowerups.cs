using UnityEngine;
using UnityEditor;

public class AutoAssignPowerups
{
    // Script kaydedilip derlendiği anda otomatik çalışır! USER'ın bir şey yapmasına gerek yok.
    [InitializeOnLoadMethod]
    public static void AutoFixLevelData()
    {
        // Tüm Level Data dosyalarını bul
        string[] levelGuids = AssetDatabase.FindAssets("t:LevelData");
        
        // Tüm yaratılmış PowerUp Data dosyalarını bul
        string[] powerupGuids = AssetDatabase.FindAssets("t:PowerupData");
        
        if (powerupGuids.Length == 0) return; // Güçlendirici yoksa bir şey yapma

        // Powerupları yükle
        PowerupData[] allPowerups = new PowerupData[powerupGuids.Length];
        for (int i = 0; i < powerupGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(powerupGuids[i]);
            allPowerups[i] = AssetDatabase.LoadAssetAtPath<PowerupData>(path);
        }

        // Levellara ekle
        bool changed = false;
        foreach (string guid in levelGuids)
        {
            string levelPath = AssetDatabase.GUIDToAssetPath(guid);
            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(levelPath);
            
            if (level != null && (level.availablePowerups == null || level.availablePowerups.Length == 0))
            {
                level.availablePowerups = allPowerups;
                EditorUtility.SetDirty(level);
                changed = true;
                Debug.Log("[Oto-Tamir] " + level.name + " dosyasina güçlendiriciler otomatik eklendi!");
            }
        }

        if (changed)
        {
            AssetDatabase.SaveAssets();
        }
    }
}
