using UnityEngine;
using UnityEditor;
using System.IO;

public class ButtonGenerator 
{
    [InitializeOnLoadMethod]
    static void Generate() 
    {
        string directoryInfo = "Assets/Sprites/UI";
        string path = directoryInfo + "/GoldPill.png";
        
        // Zaten var ise tekrar olusturma
        if (File.Exists(path)) return;
        
        if (!Directory.Exists(directoryInfo))
        {
            Directory.CreateDirectory(directoryInfo);
        }
        
        int width = 512;
        int height = 140;
        int r = 70; // Yari cap (Radius), tam hap (pill) seklini verir
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        
        Color bottomGold = new Color(0.85f, 0.55f, 0.0f); // #D98C00
        Color topGold = new Color(1.0f, 0.90f, 0.35f);   // #FFE659

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                Color c = new Color(0,0,0,0); // Sifir saydam arka plan
                
                float distLeft = Vector2.Distance(new Vector2(x, y), new Vector2(r, r));
                float distRight = Vector2.Distance(new Vector2(x, y), new Vector2(width - r, r));
                
                float distToEdge = -1; // Kenara olan uzaklik (aa icin)
                
                if (x >= r && x <= width - r) {
                    // Duz orta kisim (Body)
                    distToEdge = Mathf.Min(y, height - 1 - y);
                } else if (x < r && distLeft <= r) {
                    // Sol kavis (Left cap)
                    distToEdge = r - distLeft;
                } else if (x > width - r && distRight <= r) {
                    // Sag kavis (Right cap)
                    distToEdge = r - distRight;
                }
                
                if (distToEdge >= 0) {
                    // Resmin icindeyiz
                    float t = y / (float)height;
                    Color gold = Color.Lerp(bottomGold, topGold, t);
                    
                    // Kenar puruzsuzlestirme (Anti-aliasing)
                    float alpha = Mathf.Clamp01(distToEdge);
                    
                    // Ic golge ve Highlight (Bevel yani 3D hissi efekti)
                    if (distToEdge < 10) {
                        float edgeT = distToEdge / 10f;
                        if (y < r) {
                           // Alt kenarlar - daha koyu gölge
                           gold = Color.Lerp(new Color(0.4f, 0.2f, 0f, 1f), gold, edgeT);
                        } else {
                           // Üst kenarlar - parlama (highlight)
                           gold = Color.Lerp(new Color(1f, 1f, 0.8f, 1f), gold, edgeT);
                        }
                    }
                    
                    gold.a = alpha;
                    c = gold;
                }
                
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        
        // Otomatik Sprite Type yapma
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null) {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }
}
