using UnityEngine;
using UnityEditor;

/// <summary>
/// KacAtaKac > Auto-Wire Scene  (Ctrl+Shift+W)
/// KacAtaKac > Fix Sorting Orders
/// KacAtaKac > Diagnose Scene
/// KacAtaKac > Fix Player Visibility
/// </summary>
public static class SceneAutoWire
{
    // ================================================================
    // 0) DİAGNOSTİK — Tüm olası sorunları Console'a yazar
    // ================================================================

    [MenuItem("KacAtaKac/Diagnose Scene")]
    public static void DiagnoseScene()
    {
        Debug.Log("====== KacAtaKac SAHNE TANILAMASı BAŞLIYOR ======");

        // ── Player ───────────────────────────────────────────────────
        var playerMovement = Object.FindAnyObjectByType<PlayerMovement2D>();
        if (playerMovement == null)
        {
            Debug.LogError("❌ PlayerMovement2D BULUNAMADI — Player sahnede yok!");
        }
        else
        {
            Debug.Log($"✅ Player bulundu: {playerMovement.gameObject.name}  pos={playerMovement.transform.position}  active={playerMovement.gameObject.activeInHierarchy}");

            // Tüm SpriteRenderer'ları player altında kontrol et
            var srs = playerMovement.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            if (srs.Length == 0)
            {
                Debug.LogError("❌ Player'ın hiç SpriteRenderer child'ı YOK!");
            }
            else
            {
                foreach (var sr in srs)
                {
                    bool spriteNull = sr.sprite == null;
                    bool alphaZero  = sr.color.a < 0.01f;
                    bool disabled   = !sr.enabled;
                    bool inactive   = !sr.gameObject.activeInHierarchy;
                    string status   = (spriteNull || alphaZero || disabled || inactive) ? "❌" : "✅";
                    Debug.Log($"{status} Player SR [{sr.gameObject.name}]  " +
                              $"sprite={sr.sprite?.name ?? "NULL"}  " +
                              $"alpha={sr.color.a:F2}  " +
                              $"enabled={sr.enabled}  " +
                              $"active={sr.gameObject.activeInHierarchy}  " +
                              $"sortingLayer={sr.sortingLayerName}  " +
                              $"sortingOrder={sr.sortingOrder}");
                }
            }

            // PlayerDirectionSprite kontrolü
            var pds = playerMovement.GetComponentInChildren<PlayerDirectionSprite>();
            if (pds == null)
            {
                Debug.LogWarning("⚠️ PlayerDirectionSprite bulunamadı — sprite animasyonu yok.");
            }
            else
            {
                if (pds.spriteRenderer == null)
                    Debug.LogError("❌ PlayerDirectionSprite.spriteRenderer = NULL! Sprite render edilemiyor.");
                if (pds.south?.idle == null)
                    Debug.LogWarning("⚠️ PlayerDirectionSprite.south.idle sprite atanmamış — karakter görünmez kalabilir!");
            }
        }

        // ── Arena ─────────────────────────────────────────────────────
        var arena = Object.FindAnyObjectByType<ArenaAutoLayout>();
        if (arena == null)
        {
            Debug.LogError("❌ ArenaAutoLayout BULUNAMADI!");
        }
        else
        {
            if (arena.arenaSpriteRenderer == null)
                Debug.LogError("❌ ArenaAutoLayout.arenaSpriteRenderer = NULL!");
            else
            {
                var b = arena.arenaSpriteRenderer.bounds;
                Debug.Log($"✅ Arena bounds: center={b.center}  size={b.size}  sortingOrder={arena.arenaSpriteRenderer.sortingOrder}");
            }
        }

        // ── Spawnerlar ────────────────────────────────────────────────
        var spawners = Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
        Debug.Log($"ℹ️ CornerShooter sayısı: {spawners.Length}");
        foreach (var cs in spawners)
        {
            if (cs.target == null)
                Debug.LogError($"❌ {cs.gameObject.name} target=NULL — Player'ı görmüyor, ateş etmez!");
            if (cs.projectilePrefab == null)
                Debug.LogError($"❌ {cs.gameObject.name} projectilePrefab=NULL — Mermi fırlatamaz!");
        }

        // ── PowerupSpawner ────────────────────────────────────────────
        var ps = Object.FindAnyObjectByType<PowerupSpawner>();
        if (ps != null)
        {
            Debug.Log($"ℹ️ PowerupSpawner spawnAreaMin={ps.spawnAreaMin}  spawnAreaMax={ps.spawnAreaMax}");
            if (ps.spawnAreaMin == ps.spawnAreaMax)
                Debug.LogError("❌ PowerupSpawner spawnArea sıfır büyüklükte — item spawn olamaz!");
        }

        // ── Camera ────────────────────────────────────────────────────
        var cam = Camera.main;
        if (cam == null)
            Debug.LogError("❌ Main Camera bulunamadı!");
        else
            Debug.Log($"✅ Camera: orthographic={cam.orthographic}  size={cam.orthographicSize}  cullingMask={cam.cullingMask}  pos={cam.transform.position}");

        Debug.Log("====== TANI TAMAMLANDI ======");
        EditorUtility.DisplayDialog("Diagnose", "Sonuçlar Console'a yazıldı.\nConsole'u aç: Window > General > Console", "Tamam");
    }

    // ================================================================
    // FIX PLAYER VISIBILITY — Tüm görünürlük sorunlarını düzeltmeye çalışır
    // ================================================================

    [MenuItem("KacAtaKac/Fix Player Visibility")]
    public static void FixPlayerVisibility()
    {
        int fixed_ = 0;
        var playerMovement = Object.FindAnyObjectByType<PlayerMovement2D>();
        if (playerMovement == null)
        {
            EditorUtility.DisplayDialog("Hata", "Player (PlayerMovement2D) sahnede bulunamadı!", "Tamam");
            return;
        }

        // 1. Player objesini aktif et
        if (!playerMovement.gameObject.activeSelf)
        {
            Undo.RecordObject(playerMovement.gameObject, "Fix Player Active");
            playerMovement.gameObject.SetActive(true);
            fixed_++;
            Debug.Log("[Fix] Player aktif edildi.");
        }

        // 2. Tüm child SpriteRenderer'ları düzelt
        var srs = playerMovement.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (var sr in srs)
        {
            Undo.RecordObject(sr, "Fix Player SR");
            Undo.RecordObject(sr.gameObject, "Fix Player SR GO");

            // Aktif et
            if (!sr.gameObject.activeSelf)
            { sr.gameObject.SetActive(true); fixed_++; }

            // Enable et
            if (!sr.enabled)
            { sr.enabled = true; fixed_++; }

            // Alpha sıfırsa düzelt
            if (sr.color.a < 0.01f)
            {
                Color c = sr.color; c.a = 1f;
                sr.color = c; fixed_++;
                Debug.Log($"[Fix] {sr.gameObject.name} alpha 0'dan 1'e düzeltildi.");
            }

            // sortingOrder: "Visual" adındaki child → 10
            if (sr.gameObject.name.Contains("Visual") || sr.gameObject.name.Contains("visual"))
            {
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 10;
                fixed_++;
            }
            else if (sr.sortingOrder < 5)
            {
                sr.sortingOrder = 10;
                fixed_++;
            }

            EditorUtility.SetDirty(sr);
        }

        // 3. PlayerDirectionSprite — spriteRenderer boşsa child'dan bul
        var pds = playerMovement.GetComponentInChildren<PlayerDirectionSprite>();
        if (pds != null && pds.spriteRenderer == null)
        {
            Undo.RecordObject(pds, "Fix PDS SpriteRenderer");
            var sr = playerMovement.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) { pds.spriteRenderer = sr; fixed_++; Debug.Log($"[Fix] PlayerDirectionSprite.spriteRenderer → {sr.gameObject.name}"); }
        }

        // 4. PlayerHealth — playerSpriteRenderer boşsa doldur
        var ph = playerMovement.GetComponent<PlayerHealth>();
        if (ph == null) ph = playerMovement.GetComponentInChildren<PlayerHealth>();
        if (ph != null && ph.playerSpriteRenderer == null)
        {
            Undo.RecordObject(ph, "Fix PH SpriteRenderer");
            var sr = playerMovement.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) { ph.playerSpriteRenderer = sr; fixed_++; }
        }

        string msg = fixed_ > 0
            ? $"{fixed_} sorun düzeltildi. Ctrl+S ile kaydet."
            : "Görünür bir sorun bulunamadı. Console > Diagnose çalıştır.";

        Debug.Log($"[Fix Player Visibility] {msg}");
        EditorUtility.DisplayDialog("Fix Player Visibility", msg, "Tamam");
    }


    [MenuItem("KacAtaKac/Auto-Wire Scene %#w")]   // Ctrl+Shift+W
    public static void Wire()
    {
        int fixes = 0;

        // ── Temel bileşenleri bul ─────────────────────────────────────
        var levelManager   = Object.FindAnyObjectByType<LevelManager>();
        var gameManager    = Object.FindAnyObjectByType<GameManager>();
        var diffManager    = Object.FindAnyObjectByType<DifficultyManager>();
        var scoreManager   = Object.FindAnyObjectByType<ScoreManager>();
        var arenaLayout    = Object.FindAnyObjectByType<ArenaAutoLayout>();
        var playerMovement = Object.FindAnyObjectByType<PlayerMovement2D>();
        var playerHealth   = Object.FindAnyObjectByType<PlayerHealth>();
        var playerVisual   = Object.FindAnyObjectByType<PlayerDirectionSprite>();
        var powerupSpawner = Object.FindAnyObjectByType<PowerupSpawner>();
        var waveManager    = Object.FindAnyObjectByType<WaveManager>();
        var spawners       = Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
        var spawnerVisuals = Object.FindObjectsByType<SpawnerDirectionAnimator>(FindObjectsSortMode.None);

        Transform playerTransform = playerMovement != null ? playerMovement.transform : null;

        // ── LevelManager ──────────────────────────────────────────────
        if (levelManager != null)
        {
            Undo.RecordObject(levelManager, "Auto-Wire LevelManager");

            if (levelManager.playerMovement == null && playerMovement != null)
                { levelManager.playerMovement = playerMovement; fixes++; }
            if (levelManager.playerHealth == null && playerHealth != null)
                { levelManager.playerHealth = playerHealth; fixes++; }
            if (levelManager.playerVisual == null && playerVisual != null)
                { levelManager.playerVisual = playerVisual; fixes++; }
            if (levelManager.arenaLayout == null && arenaLayout != null)
                { levelManager.arenaLayout = arenaLayout; fixes++; }
            if ((levelManager.spawners == null || levelManager.spawners.Length == 0) && spawners.Length > 0)
                { levelManager.spawners = spawners; fixes++; }
            if ((levelManager.spawnerVisuals == null || levelManager.spawnerVisuals.Length == 0) && spawnerVisuals.Length > 0)
                { levelManager.spawnerVisuals = spawnerVisuals; fixes++; }
            if (levelManager.difficultyManager == null && diffManager != null)
                { levelManager.difficultyManager = diffManager; fixes++; }
            if (levelManager.powerupSpawner == null && powerupSpawner != null)
                { levelManager.powerupSpawner = powerupSpawner; fixes++; }
            if (levelManager.waveManager == null && waveManager != null)
                { levelManager.waveManager = waveManager; fixes++; }

            EditorUtility.SetDirty(levelManager);
        }

        // ── GameManager ───────────────────────────────────────────────
        if (gameManager != null)
        {
            Undo.RecordObject(gameManager, "Auto-Wire GameManager");
            if (gameManager.scoreManager == null && scoreManager != null)
                { gameManager.scoreManager = scoreManager; fixes++; }
            if (gameManager.levelManager == null && levelManager != null)
                { gameManager.levelManager = levelManager; fixes++; }
            if (gameManager.difficultyManager == null && diffManager != null)
                { gameManager.difficultyManager = diffManager; fixes++; }
            EditorUtility.SetDirty(gameManager);
        }

        // ── DifficultyManager ─────────────────────────────────────────
        if (diffManager != null)
        {
            Undo.RecordObject(diffManager, "Auto-Wire DifficultyManager");
            if (diffManager.scoreManager == null && scoreManager != null)
                { diffManager.scoreManager = scoreManager; fixes++; }
            if ((diffManager.allSpawners == null || diffManager.allSpawners.Length == 0) && spawners.Length > 0)
                { diffManager.allSpawners = spawners; fixes++; }
            EditorUtility.SetDirty(diffManager);
        }

        // ── CornerShooter → target = Player ──────────────────────────
        foreach (var cs in spawners)
        {
            if (cs == null) continue;
            Undo.RecordObject(cs, "Auto-Wire CornerShooter");

            if (cs.target == null && playerTransform != null)
                { cs.target = playerTransform; fixes++; }

            if (cs.firePoint == null)
            {
                Transform fp = cs.transform.Find("FirePoint");
                if (fp != null) { cs.firePoint = fp; fixes++; }
            }

            if (cs.spawnerVisual == null)
            {
                var vis = cs.GetComponentInChildren<SpawnerDirectionAnimator>();
                if (vis != null) { cs.spawnerVisual = vis; fixes++; }
            }

            EditorUtility.SetDirty(cs);
        }

        // ── SpawnerDirectionAnimator ──────────────────────────────────
        foreach (var sd in spawnerVisuals)
        {
            if (sd == null) continue;
            Undo.RecordObject(sd, "Auto-Wire SpawnerDirectionAnimator");

            if (sd.target == null && playerTransform != null)
                { sd.target = playerTransform; fixes++; }

            if (sd.spriteRenderer == null)
            {
                var sr = sd.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) { sd.spriteRenderer = sr; fixes++; }
            }

            EditorUtility.SetDirty(sd);
        }

        // ── PlayerDirectionSprite ─────────────────────────────────────
        if (playerVisual != null)
        {
            Undo.RecordObject(playerVisual, "Auto-Wire PlayerDirectionSprite");

            if (playerVisual.movement == null && playerMovement != null)
                { playerVisual.movement = playerMovement; fixes++; }

            if (playerVisual.spriteRenderer == null)
            {
                var sr = FindChildSpriteRenderer(playerVisual.transform, "Visual");
                if (sr != null) { playerVisual.spriteRenderer = sr; fixes++; }
            }

            EditorUtility.SetDirty(playerVisual);
        }

        // ── PlayerHealth ──────────────────────────────────────────────
        if (playerHealth != null)
        {
            Undo.RecordObject(playerHealth, "Auto-Wire PlayerHealth");

            if (playerHealth.playerSpriteRenderer == null)
            {
                var sr = FindChildSpriteRenderer(playerHealth.transform, "Visual");
                if (sr != null) { playerHealth.playerSpriteRenderer = sr; fixes++; }
            }

            if (playerHealth.healthUI == null)
            {
                var hui = Object.FindAnyObjectByType<HealthUI>();
                if (hui != null) { playerHealth.healthUI = hui; fixes++; }
            }

            EditorUtility.SetDirty(playerHealth);
        }

        // ── ArenaAutoLayout → player ──────────────────────────────────
        if (arenaLayout != null && arenaLayout.player == null && playerTransform != null)
        {
            Undo.RecordObject(arenaLayout, "Auto-Wire ArenaAutoLayout");
            arenaLayout.player = playerTransform;
            EditorUtility.SetDirty(arenaLayout);
            fixes++;
        }

        // ── PowerupSpawner — arena sınırlarını güncelle ───────────────
        if (powerupSpawner != null && arenaLayout != null && arenaLayout.arenaSpriteRenderer != null)
        {
            Undo.RecordObject(powerupSpawner, "Auto-Wire PowerupSpawner Bounds");
            Bounds b = arenaLayout.arenaSpriteRenderer.bounds;
            float padX = b.extents.x * 0.75f;
            float padY = b.extents.y * 0.75f;
            powerupSpawner.spawnAreaMin = new Vector2(b.center.x - padX, b.center.y - padY);
            powerupSpawner.spawnAreaMax = new Vector2(b.center.x + padX, b.center.y + padY);
            EditorUtility.SetDirty(powerupSpawner);
            fixes++;
        }

        // ── Sonuç ─────────────────────────────────────────────────────
        string msg = fixes > 0
            ? $"{fixes} referans bağlandı.\n\nCtrl+S ile sahneyi kaydet!"
            : "Tüm referanslar zaten bağlıydı.";

        Debug.Log($"[Auto-Wire] {msg}");
        EditorUtility.DisplayDialog("Auto-Wire", msg, "Tamam");
    }

    // ================================================================
    // 2) FIX SORTING ORDERS
    //    Karakteri ve spawnerları doğru katmana alır.
    //    Arena sprite'ının üstünde görünürler.
    // ================================================================

    [MenuItem("KacAtaKac/Fix Sorting Orders")]
    public static void FixSortingOrders()
    {
        int fixed_ = 0;

        // ── Arena sprite → en altta (order -100) ────────────────────────
        var arenaLayout = Object.FindAnyObjectByType<ArenaAutoLayout>();
        if (arenaLayout != null && arenaLayout.arenaSpriteRenderer != null)
        {
            Undo.RecordObject(arenaLayout.arenaSpriteRenderer, "Fix Arena Sorting");
            arenaLayout.arenaSpriteRenderer.sortingLayerName = "Default";
            arenaLayout.arenaSpriteRenderer.sortingOrder = -100;
            EditorUtility.SetDirty(arenaLayout.arenaSpriteRenderer);
            fixed_++;
        }

        // ── Player "Visual" child → baseOrder 100 ─────────────────────────
        var playerMovement = Object.FindAnyObjectByType<PlayerMovement2D>();
        if (playerMovement != null)
        {
            SpriteRenderer playerSr = FindChildSpriteRenderer(playerMovement.transform, "Visual");
            if (playerSr != null)
            {
                Undo.RecordObject(playerSr, "Fix Player Sorting");
                playerSr.sortingLayerName = "Default";
                playerSr.sortingOrder = 100;
                EditorUtility.SetDirty(playerSr);
                fixed_++;

                // YDepthSorter baseOrder güncelle
                var yds = playerSr.GetComponent<YDepthSorter>();
                if (yds != null)
                {
                    Undo.RecordObject(yds, "Fix Player YDepthSorter");
                    yds.baseOrder = 100;
                    EditorUtility.SetDirty(yds);
                }
            }
        }

        // ── Spawnerlar → order 50 ──────────────────────────────────────
        var spawnerVisuals = Object.FindObjectsByType<SpawnerDirectionAnimator>(FindObjectsSortMode.None);
        foreach (var sd in spawnerVisuals)
        {
            if (sd == null || sd.spriteRenderer == null) continue;
            Undo.RecordObject(sd.spriteRenderer, "Fix Spawner Sorting");
            sd.spriteRenderer.sortingLayerName = "Default";
            sd.spriteRenderer.sortingOrder = 50;
            EditorUtility.SetDirty(sd.spriteRenderer);
            fixed_++;
            
            // YDepthSorter baseOrder güncelle (eğer varsa)
            var yds = sd.spriteRenderer.GetComponent<YDepthSorter>();
            if (yds != null)
            {
                Undo.RecordObject(yds, "Fix Spawner YDepthSorter");
                yds.baseOrder = 50;
                EditorUtility.SetDirty(yds);
            }
        }

        var spawnersRoot = GameObject.Find("Spawners");
        if (spawnersRoot != null)
        {
            var yds = spawnersRoot.GetComponent<YDepthSorter>();
            if (yds != null)
            {
                Undo.RecordObject(yds, "Fix SpawnerRoot YDepthSorter");
                yds.baseOrder = 50;
                EditorUtility.SetDirty(yds);
            }
        }

        var spawners = Object.FindObjectsByType<CornerShooter>(FindObjectsSortMode.None);
        foreach (var cs in spawners)
        {
            if (cs == null) continue;
            var sr = cs.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sortingOrder != 50)
            {
                Undo.RecordObject(sr, "Fix Spawner SR Sorting");
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 50;
                EditorUtility.SetDirty(sr);
                fixed_++;
            }
        }

        string msg = fixed_ > 0
            ? $"{fixed_} SpriteRenderer sorting order düzeltildi.\n\nCtrl+S ile kaydet!"
            : "Sorting zaten doğruydu.";

        Debug.Log($"[Fix Sorting] {msg}");
        EditorUtility.DisplayDialog("Fix Sorting Orders", msg, "Tamam");
    }

    // ================================================================
    // Yardımcı
    // ================================================================

    static SpriteRenderer FindChildSpriteRenderer(Transform root, string preferredChildName)
    {
        // 1. Önce tam isim eşleşmesi
        Transform named = root.Find(preferredChildName);
        if (named != null)
        {
            var sr = named.GetComponent<SpriteRenderer>();
            if (sr != null) return sr;
        }

        // 2. İsim içeriyor mu?
        foreach (Transform child in root)
        {
            if (child.name.Contains(preferredChildName))
            {
                var sr = child.GetComponent<SpriteRenderer>();
                if (sr != null) return sr;
            }
        }

        // 3. Herhangi bir child'da SpriteRenderer var mı?
        return root.GetComponentInChildren<SpriteRenderer>();
    }
}
