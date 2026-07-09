using UnityEngine;
using UnityEditor;
using System.IO;

public class DifficultyStageGenerator
{
    [MenuItem("KacAtaKac/Generate Difficulty Stages")]
    public static void GenerateInitialStages()
    {
        string dir = "Assets/Data";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        CreateStage(dir, "Stage1_Baslangic", "Baslangic", 0, 49, 2, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
        CreateStage(dir, "Stage2_Kolay",     "Kolay",    50, 149, 2, 0.85f, 1.15f, 1.0f, 1.05f, 1.1f);
        CreateStage(dir, "Stage3_Orta",      "Orta",    150, 349, 3, 0.70f, 1.30f, 1.15f, 1.10f, 1.2f);
        CreateStage(dir, "Stage4_Zor",       "Zor",     350, 599, 3, 0.55f, 1.50f, 1.30f, 1.15f, 1.3f);
        CreateStage(dir, "Stage5_Cehennem",  "Cehennem",600, 999, 4, 0.40f, 1.75f, 1.45f, 1.20f, 1.5f);
        CreateStage(dir, "Stage6_Imkansiz",  "Imkansiz",1000,-1,  4, 0.30f, 2.00f, 1.60f, 1.25f, 1.8f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("6 Kademeli Zorluk Ayarlari Assets/Data klasorune olusturuldu!");
    }

    private static void CreateStage(string dir, string fileName, string stageName, int minScore, int maxScore, 
        int spawnerCount, float shootMult, float projSpeedMult, float projScaleMult, float playerSpeedMult, float scoreSpeedMult)
    {
        string path = dir + "/" + fileName + ".asset";
        DifficultyStageData data = AssetDatabase.LoadAssetAtPath<DifficultyStageData>(path);
        
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<DifficultyStageData>();
            AssetDatabase.CreateAsset(data, path);
        }

        data.stageName = stageName;
        data.minScore = minScore;
        data.maxScore = maxScore;
        data.activeSpawnerCount = spawnerCount;
        data.shootIntervalMultiplier = shootMult;
        data.projectileSpeedMultiplier = projSpeedMult;
        data.projectileScaleMultiplier = projScaleMult;
        data.playerSpeedMultiplier = playerSpeedMult;
        data.scoreSpeedMultiplier = scoreSpeedMult;

        EditorUtility.SetDirty(data);
    }
}
