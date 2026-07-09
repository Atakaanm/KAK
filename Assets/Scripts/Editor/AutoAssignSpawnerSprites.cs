using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Sahnedeki tüm SpawnerDirectionAnimator bileşenlerini bulur ve
/// Assets/Sprites/Spawner/... altındaki animasyonları (PNG/GIF) otomatik bağlar.
/// </summary>
public static class AutoAssignSpawnerSprites
{
    [MenuItem("KacAtaKac/Spawner Görsellerini Otomatik Ekle")]
    public static void AssignSprites()
    {
        var animators = Object.FindObjectsByType<SpawnerDirectionAnimator>(FindObjectsSortMode.None);
        if (animators.Length == 0)
        {
            EditorUtility.DisplayDialog("Hata", "Sahnede SpawnerDirectionAnimator bulunamadı!", "Tamam");
            return;
        }

        int count = 0;
        foreach (var anim in animators)
        {
            Undo.RecordObject(anim, "Assign Spawner Sprites");

            anim.north = LoadDirectionData("North", "north");
            anim.south = LoadDirectionData("South", "south");
            anim.east = LoadDirectionData("East", "east");
            anim.west = LoadDirectionData("West", "west");
            
            anim.northEast = LoadDirectionData("North_East", "north-east");
            anim.northWest = LoadDirectionData("North_West", "north-west");
            anim.southEast = LoadDirectionData("South_East", "south-east");
            anim.southWest = LoadDirectionData("South_West", "south-west");

            EditorUtility.SetDirty(anim);
            count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Başarılı", $"{count} adet Spawner'ın tüm atış görselleri klasörlerden otomatik yüklendi!", "Süper");
    }

    private static SpawnerDirectionAnimator.DirectionAnimation LoadDirectionData(string folderName, string idleName)
    {
        var data = new SpawnerDirectionAnimator.DirectionAnimation();
        string folderPath = $"Assets/Sprites/Spawner/{folderName}";

        // 1. Idle Sprite'ı yükle
        string idlePathPNG = $"{folderPath}/{idleName}.png";
        string idlePathGIF = $"{folderPath}/{idleName}.gif";
        data.idle = AssetDatabase.LoadAssetAtPath<Sprite>(idlePathPNG);
        if (data.idle == null) data.idle = AssetDatabase.LoadAssetAtPath<Sprite>(idlePathGIF);

        // 2. Attack (Shoot) Framelerini yükle
        List<Sprite> attackFrames = new List<Sprite>();
        
        // Klasördeki tüm assetleri ara
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        
        // Önce tüm bu klasördeki texture'ları Sprite olarak force et
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ForceImportAsSprite(path);
        }

        // Attack_1, Attack_2 diye numaralandırılmış olabilir, güvenli yoldan 1'den 20'ye kadar arayalım
        for (int i = 1; i <= 20; i++)
        {
            string framePathPNG = $"{folderPath}/{folderName}_Attack_{i}.png";
            string framePathGIF = $"{folderPath}/{folderName}_Attack_{i}.gif";
            
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(framePathPNG);
            if (s == null) s = AssetDatabase.LoadAssetAtPath<Sprite>(framePathGIF);

            if (s != null)
                attackFrames.Add(s);
        }

        data.attackFrames = attackFrames.ToArray();
        return data;
    }

    private static void ForceImportAsSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            // Pixel art için genellikle Point filter istenir ama mevcut ayarı bozmayalım
            importer.SaveAndReimport();
        }
    }
}
