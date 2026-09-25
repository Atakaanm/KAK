using UnityEngine;

/// <summary>
/// Oyunun renk paleti (Endesga 32) ve renk rolleri — sanat-rehberi.md §2-3.
/// Kodda rastgele renk yazmak yerine buradan al: KakPalette.Tehlike, KakPalette.Altin ...
/// Görsel kaynak: Assets/Art/Palette/kak_palette.png
/// </summary>
public static class KakPalette
{
    static Color H(uint rgb) => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);

    // Ortam
    public static readonly Color Murekkep = H(0x181425);       // kontur, en koyu
    public static readonly Color Gece = H(0x262b44);
    public static readonly Color DerinCamgobegi = H(0x193c3e);
    public static readonly Color Orman = H(0x265c42);
    public static readonly Color Yesil = H(0x3e8948);
    public static readonly Color AcikYesil = H(0x63c74d);
    public static readonly Color ArduvazKoyu = H(0x3a4466);
    public static readonly Color Arduvaz = H(0x5a6988);
    public static readonly Color ArduvazAcik = H(0x8b9bb4);
    public static readonly Color Sis = H(0xc0cbdc);
    public static readonly Color Beyaz = H(0xffffff);

    // Taş / toprak (tehlike nesnelerinin gövdesi)
    public static readonly Color KahveKoyu = H(0x3e2731);
    public static readonly Color Kahve = H(0x733e39);
    public static readonly Color Bakir = H(0xb86f50);
    public static readonly Color Ten = H(0xe4a672);

    // Roller
    public static readonly Color TehlikeKoyu = H(0xa22633);
    public static readonly Color Tehlike = H(0xe43b44);
    public static readonly Color TehlikeParlak = H(0xff0044);
    public static readonly Color Turuncu = H(0xf77622);
    public static readonly Color TuruncuKoyu = H(0xbe4a2f);
    public static readonly Color Altin = H(0xfeae34);         // ödül, UI vurgu
    public static readonly Color AltinAcik = H(0xfee761);
    public static readonly Color Krem = H(0xead4aa);          // UI metni
    public static readonly Color CamgobegiKoyu = H(0x124e89);
    public static readonly Color Camgobegi = H(0x0099db);     // iyi / powerup
    public static readonly Color CamgobegiParlak = H(0x2ce8f5);
    public static readonly Color Mor = H(0x68386c);
    public static readonly Color Pembe = H(0xb55088);

    public static Color WithAlpha(Color c, float a) { c.a = a; return c; }
}
