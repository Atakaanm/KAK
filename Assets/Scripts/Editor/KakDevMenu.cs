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
