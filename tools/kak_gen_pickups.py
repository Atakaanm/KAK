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


# ── Pet'ler (Faz 3c.5): 1 piksel = 0.024 birim; oyuncunun yaklaşık üçte biri ──
SIS = hx("c0cbdc")
GECE = hx("262b44")
CYAN = hx("0099db")
CYAN_L = hx("2ce8f5")
CYAN_D = hx("124e89")
GREEN_L = hx("63c74d")
GREEN = hx("3e8948")


def grid(rows, palette):
    h, w = len(rows), max(len(r) for r in rows)
    a = np.zeros((h, w, 4), dtype=np.uint8)
    for y, r in enumerate(rows):
        for x, ch in enumerate(r):
            if ch in palette:
                a[y, x] = palette[ch]
    return Image.fromarray(a, "RGBA")


def _ell_fill(a, cx, cy, rx, ry, col, cond=None):
    h, w = a.shape[:2]
    for y in range(h):
        for x in range(w):
            dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
            if dx * dx + dy * dy <= 1.0 and (cond is None or cond(x, y, dx, dy)):
                a[y, x] = col


def firefly(frame):
    """Ateşböceği (14x16): kanatlar (2 kare çırpma), koyu baş ve göz, parlayan altın karın."""
    a = np.zeros((16, 14, 4), dtype=np.uint8)
    wy = 3.0 if frame == 0 else 5.0
    _ell_fill(a, 3.5, wy, 3.0, 2.0, SIS)          # sol kanat
    _ell_fill(a, 10.5, wy, 3.0, 2.0, SIS)         # sağ kanat
    _ell_fill(a, 7.0, 5.0, 2.6, 2.4, GECE)        # baş
    a[5, 8] = WHITE                               # göz
    a[4, 8] = WHITE
    _ell_fill(a, 7.0, 11.5, 3.2, 3.8, GOLD)       # karın
    _ell_fill(a, 7.0, 11.5, 1.8, 2.4, GOLD_L)     # parlak çekirdek
    a[8, 6] = GECE; a[8, 7] = GECE                # gövde bağlantısı
    return outline(Image.fromarray(a, "RGBA"))


def outline(img):
    """Dolu piksellerin çevresine 1 px mürekkep kontur ekler (şeffaf komşulara)."""
    a = np.array(img)
    h, w = a.shape[:2]
    out = a.copy()
    for y in range(h):
        for x in range(w):
            if a[y, x, 3]:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < w and 0 <= ny < h and a[ny, nx, 3] and tuple(a[ny, nx]) != INK:
                    out[y, x] = INK
                    break
    return Image.fromarray(out, "RGBA")


def turtle(frame):
    """Yandan kaplumbağa (20x14, sağa bakar): camgöbeği kubbe kabuk (kalkan rengi), açık yeşil baş ve bacaklar."""
    w, h = 20, 14
    a = np.zeros((h, w, 4), dtype=np.uint8)
    off = 0 if frame == 0 else 1
    for lx in (4 + off, 12 - off):                # bacaklar (yürüme)
        for yy in (11, 12):
            a[yy, lx] = GREEN_L
            a[yy, lx + 1] = GREEN_L
    _ell_fill(a, 17.0, 7.5, 2.2, 2.0, GREEN_L)    # baş
    a[7, 17] = INK                                # göz
    # Kabuk: yarım elips kubbe + desen + parlama
    _ell_fill(a, 9.0, 10.5, 7.5, 8.5, CYAN, lambda x, y, dx, dy: y <= 10)
    _ell_fill(a, 9.0, 10.5, 7.5, 8.5, CYAN_L, lambda x, y, dx, dy: y <= 10 and dx * 0.7 + dy * 0.7 < -0.72)
    for (x, y) in ((6, 6), (9, 4), (12, 6), (7, 8), (11, 8), (9, 7)):
        a[y, x] = CYAN_D
    for xx in range(3, 16):                       # karın şeridi
        a[10, xx] = GREEN
    return outline(Image.fromarray(a, "RGBA"))


def pets():
    out = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "Pets")
    os.makedirs(out, exist_ok=True)
    for i in range(2):
        firefly(i).save(os.path.join(out, f"firefly_{i}.png"))
        turtle(i).save(os.path.join(out, f"turtle_{i}.png"))
    print("firefly_0..1, turtle_0..1")


def main():
    os.makedirs(OUT, exist_ok=True)
    for i, w in enumerate([14, 10, 4, 10]):
        coin(w).save(os.path.join(OUT, f"coin_{i}.png"))
    print("coin_0..3.png")
    pets()


if __name__ == "__main__":
    main()
