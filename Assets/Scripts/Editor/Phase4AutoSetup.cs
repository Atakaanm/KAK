using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class Phase4AutoSetup : EditorWindow
{
    [MenuItem("KacAtaKac/Faz 4 Hatalarını Otomatik Düzelt")]
    public static void AutoSetupPhase4()
    {
        Debug.Log("Faz 4 otomatik kurulum başlatılıyor...");

        // 1. _Recovery klasörünü sil
        string recoveryPath = "Assets/_Recovery";
        if (AssetDatabase.IsValidFolder(recoveryPath))
        {
            AssetDatabase.DeleteAsset(recoveryPath);
            Debug.Log("_Recovery klasörü silindi.");
        }

        // 2. Projectile'a Rigidbody2D ekle
        string[] projectileGuids = AssetDatabase.FindAssets("t:GameObject Projectile");
        if (projectileGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(projectileGuids[0]);
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (projPrefab != null && projPrefab.GetComponent<Rigidbody2D>() == null)
            {
                // Instantiate, add, apply, destroy (prefab modification)
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(projPrefab);
                Rigidbody2D rb = instance.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                
                PrefabUtility.SaveAsPrefabAsset(instance, path);
                DestroyImmediate(instance);
                Debug.Log("Projectile prefab'ına Rigidbody2D eklendi.");
            }
        }

        // 3. Sahnedeki GameManager ve diğerlerini bul
        GameManager gm = FindAnyObjectByType<GameManager>();
        AudioManager am = FindAnyObjectByType<AudioManager>();

        if (am != null)
        {
            if (am.GetComponent<AudioSource>() == null)
            {
                AudioSource src1 = am.gameObject.AddComponent<AudioSource>();
                AudioSource src2 = am.gameObject.AddComponent<AudioSource>();
                am.musicSource = src1;
                am.sfxSource = src2;
                Debug.Log("AudioManager'a AudioSource'lar eklendi.");
            }
        }

        // 4. Pause Menüsü Kurulumu
        PauseManager pm = FindAnyObjectByType<PauseManager>();
        Canvas uiCanvas = FindAnyObjectByType<Canvas>();

        if (pm == null && uiCanvas != null)
        {
            GameObject pmObj = new GameObject("PauseManager");
            pm = pmObj.AddComponent<PauseManager>();
            
            // UI Panel oluştur
            GameObject pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(uiCanvas.transform, false);
            RectTransform rt = pausePanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image panelImg = pausePanel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.7f);

            // Buton oluştur (Resume)
            GameObject resumeBtn = new GameObject("ResumeButton");
            resumeBtn.transform.SetParent(pausePanel.transform, false);
            RectTransform rRt = resumeBtn.AddComponent<RectTransform>();
            rRt.sizeDelta = new Vector2(200, 60);
            rRt.anchoredPosition = new Vector2(0, 50);
            Image rImg = resumeBtn.AddComponent<Image>();
            rImg.color = Color.white;
            Button rB = resumeBtn.AddComponent<Button>();

            // Buton metni
            GameObject resumeTxt = new GameObject("Text");
            resumeTxt.transform.SetParent(resumeBtn.transform, false);
            TextMeshProUGUI rtTxt = resumeTxt.AddComponent<TextMeshProUGUI>();
            rtTxt.text = "DEVAM ET";
            rtTxt.alignment = TextAlignmentOptions.Center;
            rtTxt.color = Color.black;
            
            // Buton oluştur (Menu)
            GameObject menuBtn = new GameObject("MenuButton");
            menuBtn.transform.SetParent(pausePanel.transform, false);
            RectTransform mRt = menuBtn.AddComponent<RectTransform>();
            mRt.sizeDelta = new Vector2(200, 60);
            mRt.anchoredPosition = new Vector2(0, -50);
            Image mImg = menuBtn.AddComponent<Image>();
            mImg.color = Color.white;
            Button mB = menuBtn.AddComponent<Button>();

            GameObject menuTxt = new GameObject("Text");
            menuTxt.transform.SetParent(menuBtn.transform, false);
            TextMeshProUGUI mtTxt = menuTxt.AddComponent<TextMeshProUGUI>();
            mtTxt.text = "ANA MENU";
            mtTxt.alignment = TextAlignmentOptions.Center;
            mtTxt.color = Color.black;

            pm.pausePanel = pausePanel;
            pm.resumeButton = rB;
            pm.mainMenuButton = mB;

            // Pause butonunu oyun ekranında oluştur
            GameObject pBtnObj = new GameObject("PauseButton");
            pBtnObj.transform.SetParent(uiCanvas.transform, false);
            RectTransform pRt = pBtnObj.AddComponent<RectTransform>();
            pRt.anchorMin = new Vector2(1, 1);
            pRt.anchorMax = new Vector2(1, 1);
            pRt.anchoredPosition = new Vector2(-60, -60);
            pRt.sizeDelta = new Vector2(80, 80);
            Image pImg = pBtnObj.AddComponent<Image>();
            pImg.color = Color.gray;
            Button pB = pBtnObj.AddComponent<Button>();
            
            GameObject pTxt = new GameObject("Text");
            pTxt.transform.SetParent(pBtnObj.transform, false);
            TextMeshProUGUI ptTxt = pTxt.AddComponent<TextMeshProUGUI>();
            ptTxt.text = "II";
            ptTxt.alignment = TextAlignmentOptions.Center;
            ptTxt.fontSize = 40;
            ptTxt.color = Color.white;

            pm.pauseButton = pB;
            pausePanel.SetActive(false);

            Debug.Log("Pause UI ve PauseManager sahneye eklendi.");
        }

        Debug.Log("Faz 4 Kurulumu Tamamlandı! Değişiklikleri kaydetmeyi unutmayın.");
    }
}
