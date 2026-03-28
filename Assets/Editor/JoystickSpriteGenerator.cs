using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editorde otomatik olarak joystick gorselerini olusturur.
/// Buyuk seffaf daire (arka plan) ve kucuk tok daire (handle/top).
/// </summary>
public class JoystickSpriteGenerator
{
    [InitializeOnLoadMethod]
    static void Generate()
    {
        string dir = "Assets/Sprites/UI";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        GenerateCircle(dir + "/JoystickBG.png", 256, new Color(1f, 1f, 1f, 0.15f), new Color(1f, 1f, 1f, 0.3f), true);
        GenerateCircle(dir + "/JoystickHandle.png", 96, new Color(1f, 0.85f, 0.3f, 0.9f), new Color(1f, 0.95f, 0.6f, 1f), false);
    }

    static void GenerateCircle(string path, int size, Color baseColor, Color edgeColor, bool hollow)
    {
        if (File.Exists(path)) return;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;
        float radius = center - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                
                if (dist <= radius)
                {
                    float t = dist / radius; // 0 = merkez, 1 = kenar

                    Color c;
                    if (hollow)
                    {
                        // Arka plan: sadece kenar cizgisi, ic kisim cok seffaf
                        if (t > 0.85f)
                        {
                            float edgeT = (t - 0.85f) / 0.15f;
                            c = Color.Lerp(new Color(1f, 1f, 1f, 0.05f), edgeColor, edgeT);
                        }
                        else
                        {
                            c = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t * 0.5f));
                        }
                    }
                    else
                    {
                        // Handle: tok dolu daire, ustu parlak
                        c = Color.Lerp(baseColor, edgeColor, 1f - t);
                        // Ust yaridaki highlight
                        if (y > center)
                        {
                            float highlightT = (y - center) / center;
                            c = Color.Lerp(c, new Color(1f, 1f, 0.9f, c.a), highlightT * 0.4f);
                        }
                    }

                    // Anti-aliasing (kenar yumusatma)
                    float aa = Mathf.Clamp01(radius - dist);
                    c.a *= aa;

                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }
}
