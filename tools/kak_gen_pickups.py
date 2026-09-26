#!/usr/bin/env python3
"""Toplanabilir nesnelerin piksel sanatı (palet: Endesga 32, KakPalette ile aynı).
Çıktı: Assets/Art/Pickups/coin_0..3.png (14x14, dönme animasyonu). 1 piksel = 0.024 birim (KakArtImportRules).
Kullanım: python3 tools/kak_gen_pickups.py
"""
import os
import numpy as np
from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "Pickups")


def hx(h):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), 255)


INK = hx("181425")       # Murekkep
GOLD = hx("feae34")      # Altin
GOLD_L = hx("fee761")    # AltinAcik
SHADE = hx("f77622")     # Turuncu
SHADE_D = hx("be4a2f")   # TuruncuKoyu
WHITE = hx("ffffff")
CLEAR = (0, 0, 0, 0)


def _ell(x, y, cx, cy, rx, ry):
    dx, dy = (x + 0.5 - cx) / max(rx, 0.5), (y + 0.5 - cy) / max(ry, 0.5)
    return dx * dx + dy * dy


def coin(width, size=14):
    """width: dönme anındaki görünen genişlik (piksel). 14 = yüz, 4 = kenar.
    Kontur + gövde; sol üstte açık, sağ altta gölge kenar; ortada dikey kabartma çizgisi."""
    img = np.zeros((size, size, 4), dtype=np.uint8)
    cx, cy = size / 2, size / 2
    rx, ry = width / 2, size / 2
    for y in range(size):
        for x in range(size):
            if _ell(x, y, cx, cy, rx, ry) > 1.0:
                continue
            if _ell(x, y, cx, cy, rx - 1, ry - 1) > 1.0:
                img[y, x] = INK
                continue
            if width <= 4:
                img[y, x] = GOLD_L if x + 0.5 < cx else SHADE
                continue
            col = GOLD
            if _ell(x, y, cx, cy, rx - 2, ry - 2) > 1.0:  # iç kenar halkası
                lx, ly = x + 0.5 - cx, y + 0.5 - cy
                col = GOLD_L if (lx + ly) < 0 else SHADE
            img[y, x] = col
    # Dikey kabartma çizgisi (yüz genişliğiyle daralır)
    h0, h1 = 4, size - 4
    if width >= 12:
        for y in range(h0, h1):
            img[y, int(cx) - 1] = GOLD_L
            img[y, int(cx)] = SHADE
    elif width >= 8:
        for y in range(h0, h1):
            img[y, int(cx)] = SHADE
    # Parıltı
    if width >= 12:
        img[3, 4] = WHITE
    return Image.fromarray(img, "RGBA")


def main():
    os.makedirs(OUT, exist_ok=True)
    for i, w in enumerate([14, 10, 4, 10]):
        coin(w).save(os.path.join(OUT, f"coin_{i}.png"))
    print("coin_0..3.png")


if __name__ == "__main__":
    main()
