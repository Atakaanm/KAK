#!/usr/bin/env python3
"""
Dungeon ekran çerçevesi için GEÇİCİ piksel sanat karoları üretir (Faz 1.5).
Renkler mevcut arena görselinden örneklendi; kalıcı set Faz 3'te palet standardıyla gelecek.

Çıktı: Assets/Art/Tiles/Dungeon/
  backdrop_tile.png   arena dışı karanlık taş duvar (dikişsiz)
  corridor_tile.png   arena kapısından inen koridor zemini (dikişsiz)
  corridor_wall.png   koridorun yan duvar şeridi (dikey dikişsiz)
  ledge_tile.png      HUD bandının alt kenarındaki taş korniş (yatay dikişsiz)
  torch_0..3.png      meşale animasyon kareleri
  glow.png            yumuşak turuncu ışık halesi (bilinear)
  vignette.png        kenar karartma (bilinear)
Piksel karolar 1 texel = 1 sanat pikseli; Unity'de PPU 41.667 (arena ölçeğiyle aynı yoğunluk).
"""
import os
import random

import numpy as np
from PIL import Image

OUT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Assets", "Art", "Tiles", "Dungeon")
rng = random.Random(7)


def clamp(c):
    return tuple(int(max(0, min(255, v))) for v in c)


def mul(c, k, tint=(1, 1, 1)):
    return clamp((c[0] * k * tint[0], c[1] * k * tint[1], c[2] * k * tint[2]))


# Arenadan örneklenen temel renkler
FLOOR = (84, 120, 84)
FLOOR2 = (72, 120, 84)
FLOOR_HI = (96, 144, 100)
MORTAR = (24, 52, 48)
WALLBLOCK = (110, 141, 105)
COOL = (0.85, 0.95, 1.15)  # karanlıkta hafif soğuk kayma


def brick_tile(w, h, bw, bh, base_cols, mortar, hi, lo, crack_p=0.08, moss=None, seed=1):
    r = random.Random(seed)
    img = np.zeros((h, w, 3), dtype=np.uint8)
    img[:, :] = mortar
    rows = h // bh
    for row in range(rows):
        y0 = row * bh
        offset = (bw // 2) if row % 2 else 0
        x = -offset
        while x < w:
            col = r.choice(base_cols)
            jitter = r.uniform(0.9, 1.08)
            c = mul(col, jitter)
            for yy in range(y0 + 1, y0 + bh):
                for xx in range(x + 1, x + bw):
                    X = xx % w
                    img[yy % h, X] = c
            # üst-sol ışık kenarı, alt gölge
            for xx in range(x + 1, x + bw):
                img[(y0 + 1) % h, xx % w] = hi if r.random() < 0.8 else c
                img[(y0 + bh - 1) % h, xx % w] = lo
            # çatlak
            if r.random() < crack_p:
                cx = r.randint(x + 3, x + bw - 3)
                cy = r.randint(y0 + 2, y0 + bh - 2)
                for k in range(r.randint(2, 4)):
                    img[(cy + k) % h, (cx + r.choice([-1, 0, 1])) % w] = mortar
            # yosun
            if moss and r.random() < 0.12:
                mx = r.randint(x + 1, x + bw - 2)
                for k in range(r.randint(2, 5)):
                    img[(y0 + bh - 1) % h, (mx + k) % w] = moss
            x += bw
    return img


def save(arr, name, mode="RGB"):
    Image.fromarray(arr, mode).save(os.path.join(OUT, name))
    print("  " + name + " " + str(arr.shape[1]) + "x" + str(arr.shape[0]))


def main():
    os.makedirs(OUT, exist_ok=True)

    # Arena dışı karanlık: değer ~%15-20, soğuk
    dk = 0.36
    back = brick_tile(160, 80, 20, 10,
                      [mul(FLOOR, dk, COOL), mul(FLOOR2, dk, COOL), mul(FLOOR, dk * 0.85, COOL)],
                      mul(MORTAR, 0.55, COOL), mul(FLOOR_HI, dk * 1.05, COOL), mul(MORTAR, 0.7, COOL),
                      crack_p=0.12, moss=mul((60, 90, 50), dk * 1.2), seed=3)
    save(back, "backdrop_tile.png")

    # Koridor zemini: arenadan bir kademe koyu
    ck = 0.62
    cor = brick_tile(120, 80, 20, 10,
                     [mul(FLOOR, ck), mul(FLOOR2, ck), mul(FLOOR, ck * 0.9)],
                     mul(MORTAR, 0.8), mul(FLOOR_HI, ck * 1.05), mul(MORTAR, 0.9),
                     crack_p=0.15, moss=mul((70, 110, 60), ck), seed=5)
    save(cor, "corridor_tile.png")

    # Koridor yan duvarı: dış duvar blokları, koyu
    wk = 0.55
    wall = brick_tile(12, 40, 12, 10,
                      [mul(WALLBLOCK, wk), mul(WALLBLOCK, wk * 0.9)],
                      mul(MORTAR, 0.6), mul(WALLBLOCK, wk * 1.2), mul(MORTAR, 0.7), crack_p=0.2, seed=9)
    # içe bakan kenarda koyu kontur
    wall[:, -1] = mul(MORTAR, 0.4)
    save(wall, "corridor_wall.png")

    # HUD bandı alt kenarı: korniş (arenanın üst duvarının üstünde)
    lk = 0.7
    ledge = brick_tile(40, 8, 20, 8,
                       [mul(WALLBLOCK, lk), mul(WALLBLOCK, lk * 0.92)],
                       mul(MORTAR, 0.6), mul(WALLBLOCK, lk * 1.25), mul(MORTAR, 0.5), crack_p=0.1, seed=11)
    ledge[-1, :] = mul(MORTAR, 0.35)
    save(ledge, "ledge_tile.png")

    # Meşale: 12x22 sanat pikseli, 4 kare
    BR = (104, 64, 36); BR_D = (60, 36, 22); BR_H = (140, 92, 52)
    F1 = (255, 244, 170); F2 = (255, 196, 64); F3 = (240, 120, 32); F4 = (170, 60, 24)
    shapes = [
        [(5, 3), (6, 3), (4, 4), (5, 4), (6, 4), (7, 4), (4, 5), (5, 5), (6, 5), (7, 5), (5, 2)],
        [(6, 2), (5, 3), (6, 3), (4, 4), (5, 4), (6, 4), (7, 4), (4, 5), (5, 5), (6, 5), (7, 5), (6, 1)],
        [(5, 3), (6, 3), (7, 3), (4, 4), (5, 4), (6, 4), (7, 4), (4, 5), (5, 5), (6, 5), (7, 5), (7, 2)],
        [(5, 2), (5, 3), (6, 3), (4, 4), (5, 4), (6, 4), (7, 4), (4, 5), (5, 5), (6, 5), (7, 5), (4, 3)],
    ]
    for i, sh in enumerate(shapes):
        t = np.zeros((22, 12, 4), dtype=np.uint8)
        # dış alev (turuncu-kırmızı hale)
        for (x, y) in sh:
            for dx, dy in ((0, 0), (-1, 0), (1, 0), (0, 1)):
                X, Y = x + dx, y + dy + 1
                if 0 <= X < 12 and 0 <= Y < 22 and t[Y, X, 3] == 0:
                    t[Y, X] = (*F4, 255)
        for (x, y) in sh:
            t[y + 1, x] = (*F3, 255)
        for (x, y) in sh:
            if y >= 3:
                t[y + 2, x] = (*F2, 255)
        t[6 + (i % 2), 5] = (*F1, 255); t[6, 6] = (*F1, 255); t[7, 6 - (i % 2)] = (*F1, 255)
        # kase
        for x in range(3, 9):
            t[9, x] = (*BR_H, 255); t[10, x] = (*BR, 255)
        for x in range(4, 8):
            t[11, x] = (*BR_D, 255)
        # sap ve duvar tutucusu
        for y in range(12, 20):
            t[y, 5] = (*BR, 255); t[y, 6] = (*BR_D, 255)
        for x in range(2, 10):
            t[17, x] = (*BR_D, 255)
        t[16, 2] = (*BR_H, 255); t[16, 9] = (*BR_H, 255)
        save(t, "torch_%d.png" % i, "RGBA")

    # Işık halesi (yumuşak)
    n = 128
    yy, xx = np.mgrid[0:n, 0:n]
    d = np.sqrt((xx - n / 2 + 0.5) ** 2 + (yy - n / 2 + 0.5) ** 2) / (n / 2)
    a = np.clip(1 - d, 0, 1) ** 2.2
    glow = np.zeros((n, n, 4), dtype=np.uint8)
    glow[..., 0] = 255; glow[..., 1] = 150; glow[..., 2] = 60
    glow[..., 3] = (a * 255 * 0.55).astype(np.uint8)
    save(glow, "glow.png", "RGBA")

    # Vinyet: dikey telefon oranı, kenarlar koyu
    W, H = 256, 512
    yy, xx = np.mgrid[0:H, 0:W]
    dx = (xx - W / 2) / (W / 2)
    dy = (yy - H / 2) / (H / 2)
    d = np.sqrt(dx ** 2 * 1.0 + dy ** 2 * 0.55)
    a = np.clip((d - 0.55) / 0.6, 0, 1) ** 1.6
    vig = np.zeros((H, W, 4), dtype=np.uint8)
    vig[..., 0] = 6; vig[..., 1] = 10; vig[..., 2] = 14
    vig[..., 3] = (a * 255 * 0.85).astype(np.uint8)
    save(vig, "vignette.png", "RGBA")


if __name__ == "__main__":
    print("Çıktı: " + OUT)
    main()
