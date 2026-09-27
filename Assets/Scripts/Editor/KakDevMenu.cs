using UnityEditor;
using UnityEngine;

/// <summary>
/// Geliştirme menüsü: KacAtaKac/Dev/...
/// Köprü üzerinden de çağrılabilir: python3 tools/kak_bridge.py menu "KacAtaKac/Dev/Test Botunu Başlat (usta)"
/// </summary>
public static class KakDevMenu
{
    [MenuItem("KacAtaKac/Dev/Test Botunu Başlat (usta)")]
    public static void StartBotExpert() => StartBot(1f);

    [MenuItem("KacAtaKac/Dev/Test Botunu Başlat (acemi)")]
    public static void StartBotNovice() => StartBot(0.25f);

    [MenuItem("KacAtaKac/Dev/Test Botunu Durdur")]
    public static void StopBot()
    {
        foreach (var bot in Object.FindObjectsByType<KakAutoPilot>(FindObjectsSortMode.None))
            Object.Destroy(bot);
    }

    [MenuItem("KacAtaKac/Dev/Powerup'lar Sık Çıksın")]
    public static void FastPowerups()
    {
        var ps = Object.FindAnyObjectByType<PowerupSpawner>();
        if (ps != null) ps.SetSpawnInterval(0.4f, 0.8f);
    }

    [MenuItem("KacAtaKac/Dev/Ölümsüzlük Aç-Kapa")]
    public static void ToggleGodMode()
    {
        PlayerHealth.DevGodMode = !PlayerHealth.DevGodMode;
        Debug.Log("[KakDevMenu] Ölümsüzlük: " + (PlayerHealth.DevGodMode ? "AÇIK" : "KAPALI"));
    }

    [MenuItem("KacAtaKac/Dev/UI - Ayarları Aç")]
    public static void OpenSettings() { var m = Object.FindAnyObjectByType<MainMenuController>(); if (m != null) m.OnSettingsClicked(); }

    [MenuItem("KacAtaKac/Dev/UI - Karakterleri Aç")]
    public static void OpenCharacters() { var m = Object.FindAnyObjectByType<MainMenuController>(); if (m != null) m.OnCharactersClicked(); }

    [MenuItem("KacAtaKac/Dev/UI - Duraklat")]
    public static void PauseGame() { var p = Object.FindAnyObjectByType<PauseManager>(); if (p != null) p.Pause(); }

    [MenuItem("KacAtaKac/Dev/Oyuncuyu Öldür")]
    public static void KillPlayer()
    {
        PlayerHealth.DevGodMode = false;
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        if (ph == null) return;
        ph.currentHealth = 1;
        ph.SetInvulnerable(0f);
        typeof(PlayerHealth).GetField("isInvincible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, false);
        typeof(PlayerHealth).GetField("isGhost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, false);
        typeof(PlayerHealth).GetField("hasShield", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, false);
        ph.TakeDamage(1);
    }

    [MenuItem("KacAtaKac/Dev/Kalkan Ver")]
    public static void GiveShield() { var h = Object.FindAnyObjectByType<PlayerHealth>(); if (h != null) h.ActivateShield(); }

    /// <summary>Oyuncuyu sürekli bir yöne yürütür (görsel kontrol: duvar kenarları). Köprü: invoke KakDevMenu WalkPlayer up|down|left|right|stop</summary>
    public static void WalkPlayer(string dir)
    {
        var m = Object.FindAnyObjectByType<PlayerMovement2D>();
        if (m == null) return;
        switch (dir)
        {
            case "up": m.InputOverride = Vector2.up; break;
            case "down": m.InputOverride = Vector2.down; break;
            case "left": m.InputOverride = Vector2.left; break;
            case "right": m.InputOverride = Vector2.right; break;
            default: m.InputOverride = null; break;
        }
    }

    /// <summary>Oyuncunun üstünde örnek dünya yazıları (5 sn kalır; ekran görüntüsü için).</summary>
    [MenuItem("KacAtaKac/Dev/Dünya Yazısı Göster")]
    public static void ShowPopups()
    {
        var p = Projectile.PlayerTarget;
        if (WorldPopup.Instance == null || p == null) return;
        WorldPopup.Instance.life = 5f;
        WorldPopup.Show(Loc.T("super_dodge"), p.position + new Vector3(0f, 0.6f, 0f), KakPalette.CamgobegiParlak, 1.3f);
        WorldPopup.Show(Loc.T("near_miss"), p.position + new Vector3(-1.2f, -0.6f, 0f), KakPalette.Krem, 1f);
        WorldPopup.Show("+10", p.position + new Vector3(1.2f, -0.6f, 0f), KakPalette.AltinAcik, 1f);
    }

    /// <summary>Kalkan, hız, hayalet ve yavaş çekimi aynı anda verir (HUD göstergesi kontrolü).</summary>
    [MenuItem("KacAtaKac/Dev/Tüm Powerup'ları Ver")]
    public static void GiveAllPowerups()
    {
        var player = Object.FindAnyObjectByType<PlayerMovement2D>();
        if (player == null) return;
        foreach (var guid in AssetDatabase.FindAssets("t:PowerupData", new[] { "Assets/Data/Powerups" }))
        {
            var d = AssetDatabase.LoadAssetAtPath<PowerupData>(AssetDatabase.GUIDToAssetPath(guid));
            if (d != null && d.type != PowerupType.Heal && !d.harmful) PowerupPickup.Apply(d, player.gameObject, player.transform.position);
        }
    }

    /// <summary>Kayıttaki oyun sayısını ayarlar (açılma takvimini denemek için; Play'de geçici kayda yazar). Köprü: invoke KakDevMenu SetGamesPlayed 4</summary>
    public static void SetGamesPlayed(string n)
    {
        if (int.TryParse(n, out int v)) SaveSystem.Data.gamesPlayed = v;
    }

    /// <summary>Cüzdandaki altını ayarlar (Play'de geçici kayda). Köprü: invoke KakDevMenu SetCoins 900</summary>
    public static void SetCoins(string n)
    {
        if (int.TryParse(n, out int v)) SaveSystem.Data.coins = v;
        foreach (var w in Object.FindObjectsByType<WalletHud>(FindObjectsSortMode.None)) w.Refresh();
    }

    /// <summary>Karakteri açıp seçer ve oyun sahnesini yeniden yükler (Play'de). Köprü: invoke KakDevMenu PlayAs Tank</summary>
    public static void PlayAs(string id)
    {
        var d = SaveSystem.Data;
        if (!d.unlockedCharacters.Contains(id)) d.unlockedCharacters.Add(id);
        d.selectedCharacter = id;
        SceneLoader.LoadGame();
    }

    /// <summary>Açık karakter panelinde bir kartın butonuna basar (0-3). Köprü: invoke KakDevMenu PressCharacterCard 1</summary>
    public static void PressCharacterCard(string index)
    {
        var p = Object.FindAnyObjectByType<CharacterPanel>();
        if (p != null && int.TryParse(index, out int i)) p.OnCardAction(i);
    }

    /// <summary>Peti açıp seçer, petleri açık sayar ve oyunu yeniden yükler. Köprü: invoke KakDevMenu PlayWithPet Firefly</summary>
    public static void PlayWithPet(string id)
    {
        var d = SaveSystem.Data;
        if (d.gamesPlayed < FeatureGate.PetsGames) d.gamesPlayed = FeatureGate.PetsGames;
        if (!d.unlockedPets.Contains(id)) d.unlockedPets.Add(id);
        d.selectedPet = id;
        SceneLoader.LoadGame();
    }

    /// <summary>Açık pet panelinde bir kartın butonuna basar. Köprü: invoke KakDevMenu PressPetCard 0</summary>
    public static void PressPetCard(string index)
    {
        var p = Object.FindAnyObjectByType<PetPanel>();
        if (p != null && int.TryParse(index, out int i)) p.OnCardAction(i);
    }

    public static void OpenPets() { var m = Object.FindAnyObjectByType<MainMenuController>(); if (m != null) m.OnPetsClicked(); }

    /// <summary>Günlük ödülü açık sayar ve paneli açar (Play'de, menüde). Köprü: invoke KakDevMenu OpenDaily 3 (seri)</summary>
    public static void OpenDaily(string streak)
    {
        var d = SaveSystem.Data;
        if (d.playDays < FeatureGate.DailyDays) d.playDays = FeatureGate.DailyDays;
        int.TryParse(streak, out int st);
        d.dailyStreak = st;
        d.lastClaimDay = st > 0 ? DailyReward.Today - 1 : -1;
        var m = Object.FindAnyObjectByType<MainMenuController>();
        if (m != null && m.dailyPanel != null) m.dailyPanel.gameObject.SetActive(true);
    }

    public static void ClaimDaily() { var p = Object.FindAnyObjectByType<DailyRewardPanel>(); if (p != null) p.OnClaim(); }

    /// <summary>Rekoru ayarlar (yeni rekor kutlamasını denemek için). Köprü: invoke KakDevMenu SetBest 10</summary>
    public static void SetBest(string n) { if (int.TryParse(n, out int v)) SaveSystem.Data.bestScoreEndless = v; }

    /// <summary>Play'de sahte ödüllü reklamı açar/kapatır (devam et ve 2× altın akışlarını denemek için).</summary>
    [MenuItem("KacAtaKac/Dev/Reklam Simülasyonu Aç-Kapa")]
    public static void ToggleAdSimulation()
    {
        if (AdService.OverrideConfig != null) { AdService.ResetForTests(); Debug.Log("[Dev] Reklam simülasyonu KAPALI"); return; }
        var c = ScriptableObject.CreateInstance<AdConfig>();
        c.enabled = true;
        c.provider = AdProviderKind.Simulated;
        AdService.OverrideConfig = c;
        AdService.OverrideProvider = new SimulatedAdProvider();
        Debug.Log("[Dev] Reklam simülasyonu AÇIK");
    }

    [MenuItem("KacAtaKac/Dev/UI - Dili Değiştir (TR-EN)")]
    public static void ToggleLanguage() { Loc.Current = Loc.Current == Loc.Lang.TR ? Loc.Lang.EN : Loc.Lang.TR; }

    /// <summary>Dili doğrudan seçer (mağaza görüntüleri). Köprü: invoke KakDevMenu SetLanguage EN</summary>
    public static void SetLanguage(string lang)
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Dev] SetLanguage yalnızca Play'de (edit modunda gerçek kayda yazardı)"); return; }
        Loc.Current = lang == "EN" ? Loc.Lang.EN : Loc.Lang.TR;
        // Skor yazısı yalnızca skor değişince yenileniyor: donmuş karede de doğru dil görünsün
        var s = Object.FindAnyObjectByType<ScoreManager>();
        if (s != null && s.scoreText != null) s.scoreText.SetText(Loc.T("hud_score"), s.ScoreInt);
    }

    /// <summary>G3: karakter detay ekranını açar (karakter paneli açık olmalı). Köprü: invoke KakDevMenu OpenCharacterDetail 0</summary>
    public static void OpenCharacterDetail(string index)
    {
        var p = Object.FindAnyObjectByType<CharacterPanel>();
        if (p != null && int.TryParse(index, out int i)) p.OpenDetail(i);
    }

    /// <summary>G3: açık detay ekranında yükseltme düğmesine basar (Health/Speed/Power). Köprü: invoke KakDevMenu PressUpgrade Health</summary>
    public static void PressUpgrade(string stat)
    {
        var d = Object.FindAnyObjectByType<CharacterDetailPanel>();
        if (d == null) return;
        if (stat == "Speed") d.OnUpgradeSpeed(); else if (stat == "Power") d.OnUpgradePower(); else d.OnUpgradeHealth();
    }

    /// <summary>G7: eldiven verir ve oyuncunun üstüne bir kartopu atar (yakala → fırlat düğmesi). Köprü: invoke KakDevMenu TestGlove</summary>
    public static void TestGlove()
    {
        var ph = Object.FindAnyObjectByType<PlayerHealth>();
        var glove = AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/GloveData.asset");
        var snow = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Worlds/Ice/Snowball_Kartopu.asset");
        if (ph == null || glove == null || snow == null) return;
        PowerupPickup.Apply(glove, ph.gameObject, ph.transform.position);
        Projectile.Launch(snow.projectilePrefab, snow, ph.transform.position + new Vector3(0f, 1.6f, 0f), Vector2.down, 2f);
    }

    /// <summary>G7: oyuncunun yanına ateş bırakır. Köprü: invoke KakDevMenu DropFire</summary>
    public static void DropFire()
    {
        var sp = Object.FindAnyObjectByType<PowerupSpawner>();
        var fire = AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/FireData.asset");
        if (sp != null && fire != null) sp.SpawnSpecific(fire);
    }

    /// <summary>G7: DÜNYALAR panelini açar. Köprü: invoke KakDevMenu OpenWorlds</summary>
    public static void OpenWorlds() { var m = Object.FindAnyObjectByType<MainMenuController>(); if (m != null) m.OnLevelsClicked(); }

    /// <summary>G7: bir dünyayı başlatır (dungeon / ice). Köprü: invoke KakDevMenu PlayWorld ice</summary>
    public static void PlayWorld(string id)
    {
        var cat = EndlessWorlds.Load();
        var w = cat != null ? cat.Find(id) : null;
        if (w == null || w.level == null) return;
        SaveSystem.Data.selectedWorld = id;
        GameSettings.SelectedLevel = w.level;
        SceneLoader.LoadGame();
    }

    /// <summary>G6: hikâyede sonraki karta geçer / kapatır. Köprü: invoke KakDevMenu IntroNext</summary>
    public static void IntroNext() { var i = Object.FindAnyObjectByType<IntroStory>(); if (i != null) i.OnNext(); }

    /// <summary>G5: iki kişilik oyunu başlatır. Köprü: invoke KakDevMenu PlayTwoPlayer</summary>
    public static void PlayTwoPlayer() { GameSettings.TwoPlayer = true; SceneLoader.LoadGame(); }

    /// <summary>G5: 1. (0) ya da 2. (1) oyuncuya ölümcül hasar (dönüş sayacını görmek için). Köprü: invoke KakDevMenu DownPlayer 0</summary>
    public static void DownPlayer(string index)
    {
        if (!int.TryParse(index, out int i) || i < 0 || i >= PlayerRegistry.All.Count) return;
        var ph = PlayerRegistry.All[i];
        ph.currentHealth = 1;
        typeof(PlayerHealth).GetField("invulnerableUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(ph, -1f);
        ph.TakeDamage(1);
    }

    /// <summary>G4: oyuncunun yanına Pranga bırakır (görsel kontrol). Köprü: invoke KakDevMenu DropShackle</summary>
    [MenuItem("KacAtaKac/Dev/Pranga Bırak")]
    public static void DropShackle()
    {
        var d = AssetDatabase.LoadAssetAtPath<PowerupData>("Assets/Data/Powerups/ShackleData.asset");
        var t = Projectile.PlayerTarget;
        if (d == null || d.visualPrefab == null || t == null) return;
        var go = Object.Instantiate(d.visualPrefab, t.position + new Vector3(0.9f, 0.5f, 0f), Quaternion.identity);
        var sr = go.GetComponentInChildren<SpriteRenderer>();
        var sp = Object.FindAnyObjectByType<PowerupSpawner>();
        if (sr != null && sr.sprite != null && sp != null) // spawner ile aynı boy
        {
            float s = sp.itemWorldSize / sr.sprite.bounds.size.x;
            go.transform.localScale = new Vector3(s, s, 1f);
            sr.sortingOrder = sp.powerupSortingOrder;
        }
    }

    /// <summary>Oyuncunun çevresine 3 göktaşı düşürür ("!" uyarısını görmek için). Köprü: invoke KakDevMenu DropMeteors</summary>
    [MenuItem("KacAtaKac/Dev/Göktaşı Düşür")]
    public static void DropMeteors()
    {
        var data = AssetDatabase.LoadAssetAtPath<ProjectileData>("Assets/Data/Projectiles/Meteor_Goktasi.asset");
        var prefab = data != null && data.projectilePrefab != null ? data.projectilePrefab : AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        var target = Projectile.PlayerTarget;
        if (data == null || prefab == null || target == null) return;
        for (int i = 0; i < 3; i++)
            Projectile.LaunchMeteor(prefab, data, target.position + (Vector3)(Random.insideUnitCircle * 1.6f));
    }

    /// <summary>Oyun süresini ileri sarar; zorluk süreye bağlı olduğu için kademe de ilerler (G2). Köprü: invoke KakDevMenu SkipTime 150</summary>
    public static void SkipTime(string seconds)
    {
        var s = Object.FindAnyObjectByType<ScoreManager>();
        if (s != null && float.TryParse(seconds, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v)) s.AdvanceTime(v);
    }

    /// <summary>Oyun zamanını dondurur/çözer (aynı anı farklı dil ve boyutlarda çekmek için). Köprü: invoke KakDevMenu Freeze 1</summary>
    public static void Freeze(string on) { Time.timeScale = on == "1" ? 0f : 1f; }

    /// <summary>Skoru ileri alır, zorluk kademesi skora bağlı olduğu için oyun da hızlanır. Köprü: invoke KakDevMenu AddScore 400</summary>
    public static void AddScore(string n)
    {
        var s = Object.FindAnyObjectByType<ScoreManager>();
        if (s != null && int.TryParse(n, out int v)) s.AddScore(v);
    }

    [MenuItem("KacAtaKac/Dev/Bot Raporu")]
    public static void Report()
    {
        foreach (var bot in Object.FindObjectsByType<KakAutoPilot>(FindObjectsSortMode.None))
            Debug.Log($"[KakAutoPilot] Rapor: süre {bot.SurvivalTime:F1} sn, vuruş {bot.HitsTaken}, bitti={bot.Finished}, skor={(GameManager.Instance != null && GameManager.Instance.scoreManager != null ? GameManager.Instance.scoreManager.ScoreInt : -1)}");
    }

    static void StartBot(float skill)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[KakDevMenu] Bot sadece Play modunda başlatılabilir.");
            return;
        }
        var player = Object.FindAnyObjectByType<PlayerMovement2D>();
        if (player == null)
        {
            Debug.LogWarning("[KakDevMenu] Oyuncu bulunamadı.");
            return;
        }
        var bot = player.GetComponent<KakAutoPilot>();
        if (bot == null) bot = player.gameObject.AddComponent<KakAutoPilot>();
        bot.skill = skill;
    }
}
