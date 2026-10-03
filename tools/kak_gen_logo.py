"""Faz 14 Ö2: piksel logo "KAÇ ATA KAÇ" — oyunun piksel diliyle (süslü Cinzel yazı yerine).
Kalın 2 px gövdeli el yapımı harfler, altın rampa (açık → turuncu), üst kenar parlaması, koyu kontur, yumuşak gölge.
Çıktı: Assets/Art/UI/logo_pixel.png (×8 büyütülmüş, UI içe aktarma kuralı: PPU 100, Point)
Kullanım: python3 tools/kak_gen_logo.py [--preview]
"""
import os, sys
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import C, ROOT, SCRATCH  # noqa: E402

G = {
    "K": ["XX....XX", "XX...XX.", "XX..XX..", "XX.XX...", "XXXX....", "XX.XX...", "XX..XX..", "XX...XX.", "XX....XX"],
    "A": ["..XXXX..", ".XXXXXX.", "XX....XX", "XX....XX", "XXXXXXXX", "XXXXXXXX", "XX....XX", "XX....XX", "XX....XX"],
    "T": ["XXXXXXXX", "XXXXXXXX", "...XX...", "...XX...", "...XX...", "...XX...", "...XX...", "...XX...", "...XX..."],
    "Ç": ["..XXXXX.", ".XXXXXXX", "XX......", "XX......", "XX......", "XX......", "XX......", ".XXXXXXX", "..XXXXX.",
          "....XX..", "...XX..."],
    " ": ["...."] * 9,
}

def render(text, scale=8):
    INK, SHADOW = C("murekkep"), C("kahve_koyu")
    ramp = [C("altin_acik"), C("altin_acik"), C("altin"), C("altin"), C("altin"), C("turuncu"), C("turuncu"), C("turuncu_koyu"), C("turuncu_koyu"), C("altin"), C("altin")]
    W = sum(len(G[ch][0]) + 1 for ch in text) + 4
    H = 11 + 4
    mask = [[0] * W for _ in range(H)]
    x = 2
    for ch in text:
        g = G[ch]
        for j, row in enumerate(g):
            for i, c in enumerate(row):
                if c == "X": mask[2 + j][x + i] = 1
        x += len(g[0]) + 1
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0)); px = img.load()
    # gölge (sağ-alt 1 px), kontur, dolgu
    for y in range(H):
        for xx in range(W):
            if mask[y][xx] and y + 1 < H and xx + 1 < W and not mask[y + 1][xx + 1]: px[xx + 1, y + 1] = SHADOW
    for y in range(H):
        for xx in range(W):
            if mask[y][xx]: continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                X, Y = xx + dx, y + dy
                if 0 <= X < W and 0 <= Y < H and mask[Y][X]: px[xx, y] = INK; break
    for y in range(H):
        for xx in range(W):
            if not mask[y][xx]: continue
            c = ramp[min(len(ramp) - 1, y - 2)]
            if y - 1 >= 0 and not mask[y - 1][xx]: c = C("beyaz") if y - 2 < 3 else C("altin_acik")  # üst kenar parlaması
            px[xx, y] = c
    return img.resize((W * scale, H * scale), Image.NEAREST)

if __name__ == "__main__":
    out = render("KAÇ ATA KAÇ")
    path = os.path.join(ROOT, "Assets", "Art", "UI", "logo_pixel.png")
    out.save(path)
    print(path, out.size)
    if "--preview" in sys.argv:
        bg = Image.new("RGBA", (out.width + 80, out.height + 80), (38, 43, 68, 255))
        bg.alpha_composite(out, (40, 40))
        bg.save(os.path.join(SCRATCH, "logo_preview.png"))
