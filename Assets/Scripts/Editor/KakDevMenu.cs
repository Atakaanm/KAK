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
            if (d != null && d.type != PowerupType.Heal) PowerupPickup.Apply(d, player.gameObject, player.transform.position);
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
