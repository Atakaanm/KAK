using UnityEngine;
using UnityEditor;

/// <summary>
/// Sahnedeki PlayerDirectionSprite bileşenindeki tüm ayarlanmış (dolu) animasyonları,
/// Data klasöründeki PlayerData (Örn: Boy_PlayerData) içine otomatik kopyalar.
/// </summary>
public static class CopyPlayerVisualsToData
{
    [MenuItem("KacAtaKac/Player Görsellerini Data'ya Kopyala")]
    public static void CopyVisuals()
    {
        // 1. Sahnedeki Player'ı bul
        PlayerDirectionSprite pds = Object.FindAnyObjectByType<PlayerDirectionSprite>();
        if (pds == null)
        {
            EditorUtility.DisplayDialog("Hata", "Sahnede PlayerDirectionSprite bulunamadı!", "Tamam");
            return;
        }

        // 2. Data klasöründeki Boy_PlayerData'yı bul
        string path = "Assets/Data/Boy_PlayerData.asset";
        PlayerData data = AssetDatabase.LoadAssetAtPath<PlayerData>(path);

        if (data == null)
        {
            EditorUtility.DisplayDialog("Hata", $"'{path}' bulunamadı! Lütfen yolu kontrol et.", "Tamam");
            return;
        }

        // 3. Verileri kopyala (Undo destekli)
        Undo.RecordObject(data, "Copy Player Visuals");

        // Veri bloklarını null ise başlat (hata vermemesi için)
        if (data.north == null) data.north = new PlayerData.DirectionData();
        if (data.south == null) data.south = new PlayerData.DirectionData();
        if (data.east == null) data.east = new PlayerData.DirectionData();
        if (data.west == null) data.west = new PlayerData.DirectionData();
        if (data.northEast == null) data.northEast = new PlayerData.DirectionData();
        if (data.northWest == null) data.northWest = new PlayerData.DirectionData();
        if (data.southEast == null) data.southEast = new PlayerData.DirectionData();
        if (data.southWest == null) data.southWest = new PlayerData.DirectionData();

        // North
        data.north.idle = pds.north.idle;
        data.north.runFrames = pds.north.runFrames;
        
        // South
        data.south.idle = pds.south.idle;
        data.south.runFrames = pds.south.runFrames;

        // East
        data.east.idle = pds.east.idle;
        data.east.runFrames = pds.east.runFrames;

        // West
        data.west.idle = pds.west.idle;
        data.west.runFrames = pds.west.runFrames;

        // NorthEast
        data.northEast.idle = pds.northEast.idle;
        data.northEast.runFrames = pds.northEast.runFrames;

        // NorthWest
        data.northWest.idle = pds.northWest.idle;
        data.northWest.runFrames = pds.northWest.runFrames;

        // SouthEast
        data.southEast.idle = pds.southEast.idle;
        data.southEast.runFrames = pds.southEast.runFrames;

        // SouthWest
        data.southWest.idle = pds.southWest.idle;
        data.southWest.runFrames = pds.southWest.runFrames;

        // Değişiklikleri kaydet
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        Debug.Log("[CopyPlayerVisuals] Sahnedeki tüm yön animasyonları Boy_PlayerData'ya kopyalandı!");
        EditorUtility.DisplayDialog("Başarılı!", "Sahnedeki tüm karakter animasyonları kalıcı olarak 'Boy_PlayerData' içine aktarıldı.\n\nArtık LevelManager karakteri sileyemeyecek!", "Harika");
    }
}
