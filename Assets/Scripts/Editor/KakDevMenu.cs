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

    [MenuItem("KacAtaKac/Dev/UI - Dili Değiştir (TR-EN)")]
    public static void ToggleLanguage() { Loc.Current = Loc.Current == Loc.Lang.TR ? Loc.Lang.EN : Loc.Lang.TR; }

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
