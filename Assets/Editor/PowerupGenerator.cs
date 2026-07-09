using UnityEngine;
using UnityEditor;
using System.IO;

public class PowerupGenerator
{
    [MenuItem("KacAtaKac/Generate Powerups")]
    public static void GenerateDefaultPowerups()
    {
        string dataDir = "Assets/Data/Powerups";
        string prefabDir = "Assets/Prefabs/Powerups";

        if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
        if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

        CreatePowerup(dataDir, prefabDir, "Heart", PowerupType.Heal, Color.white, 0f, 0f, 1, 1.0f, "Assets/Sprites/Powerups/HeartIcon.png");
        CreatePowerup(dataDir, prefabDir, "Shield", PowerupType.Shield, Color.white, 10f, 0f, 0, 0.6f, "Assets/Sprites/Powerups/ShieldIcon.png");
        CreatePowerup(dataDir, prefabDir, "Speed", PowerupType.SpeedBoost, Color.white, 6f, 1.5f, 0, 0.8f, "Assets/Sprites/Powerups/SpeedIcon.png");
        CreatePowerup(dataDir, prefabDir, "Ghost", PowerupType.Ghost, Color.white, 5f, 0f, 0, 0.4f, "Assets/Sprites/Powerups/GhostIcon.png");
        CreatePowerup(dataDir, prefabDir, "SloMo", PowerupType.TimeSlow, Color.white, 4f, 0.4f, 0, 0.5f, "Assets/Sprites/Powerups/TimeIcon.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("5 Temel PowerUp Data ve Prefablari özel ikonlarla olusturuldu!");

        // Otomatik entegrasyonu hemen çalıştır (Unity derlemesini beklemeden)
        AutoAssignPowerups.AutoFixLevelData();
    }

    private static void CreatePowerup(string dataDir, string prefabDir, string name, PowerupType type, Color color, 
        float duration, float mult, int health, float spawnWeight, string imagePath)
    {
        // 0. Resmi Pixel Art ve Sprite Olarak Ayarla
        TextureImporter importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point; // Pixel art için bulanıklığı engeller
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        // Sprite objesini al
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);

        // 1. Prefab oluştur
        string prefabPath = prefabDir + "/" + name + "Pickup.prefab";
        GameObject dummyObj = new GameObject(name + "Pickup");
        
        SpriteRenderer sr = dummyObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 20; // Güvence (Arenanın önüne geçmesi için)

        if (sprite != null) 
        {
            sr.sprite = sprite;
            // 1024x1024 devasa görseller olduğu için sahnede ekranı kaplamasın diye ufacık (0.08f) yapıyoruz.
            dummyObj.transform.localScale = new Vector3(0.08f, 0.08f, 1f); 
        } 
        else 
        {
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }
        sr.color = color;

        CircleCollider2D col = dummyObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        dummyObj.AddComponent<FloatingItem>();
        PowerupPickup pickupComponent = dummyObj.AddComponent<PowerupPickup>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(dummyObj, prefabPath);
        GameObject.DestroyImmediate(dummyObj);

        // 2. Data oluştur
        string dataPath = dataDir + "/" + name + "Data.asset";
        PowerupData data = AssetDatabase.LoadAssetAtPath<PowerupData>(dataPath);
        
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<PowerupData>();
            AssetDatabase.CreateAsset(data, dataPath);
        }

        data.powerupName = name;
        data.type = type;
        data.duration = duration;
        data.powerMultiplier = mult;
        data.healthAmount = health;
        data.spawnChanceWeight = spawnWeight;
        data.visualPrefab = prefab;
        if (sprite != null) data.icon = sprite; // UI vs için data'ya da aktar

        EditorUtility.SetDirty(data);

        // Data'yı prefaba bağla (bu adım için prefaba data referansını kaydetmeliyiz)
        GameObject loadedPrefab = PrefabUtility.LoadPrefabContents(prefabPath);
        loadedPrefab.GetComponent<PowerupPickup>().powerupData = data;
        PrefabUtility.SaveAsPrefabAsset(loadedPrefab, prefabPath);
        PrefabUtility.UnloadPrefabContents(loadedPrefab);
    }
}
