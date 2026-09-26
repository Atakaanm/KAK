#!/usr/bin/env python3
"""
UI görsel seti (Faz 4): palet renkleriyle piksel görünümlü 9-slice butonlar, panel, ikonlar.
Çıktı: Assets/Art/UI/  (_9s dosyaları 9-slice: kenar 6 sanat pikseli; Unity'de Image.pixelsPerUnitMultiplier ile büyütülür)
"""
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Assets", "Art", "UI")


def hx(h):
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


INK = hx("181425"); NIGHT = hx("262b44"); SLATE_D = hx("3a4466"); SLATE = hx("5a6988"); SLATE_L = hx("8b9bb4")
FOG = hx("c0cbdc"); CREAM = hx("ead4aa"); GOLD = hx("feae34"); GOLD_L = hx("fee761"); GOLD_D = hx("f77622")
BROWN_D = hx("3e2731"); BROWN = hx("733e39"); RED = hx("e43b44"); TEAL_D = hx("193c3e"); GREEN = hx("3e8948")


def plaque(w, h, fill_top, fill_bot, rim_light, rim_dark, outline=INK, bevel=2, corner=2):
    """Köşeleri kırpık, üstü ışıklı, altı gölgeli piksel levha."""
    a = np.zeros((h, w, 4), dtype=np.uint8)
    for y in range(h):
        t = y / max(1, h - 1)
        c = tuple(int(fill_top[i] * (1 - t) + fill_bot[i] * t) for i in range(3))
        a[y, :, :3] = c
        a[y, :, 3] = 255
    # bevel
    a[1:1 + bevel, 1:-1, :3] = rim_light
    a[-1 - bevel:-1, 1:-1, :3] = rim_dark
    a[1:-1, 1:1 + 1, :3] = rim_light
    a[1:-1, -2:-1, :3] = rim_dark
    # kontur
    a[0, :, :3] = outline; a[-1, :, :3] = outline; a[:, 0, :3] = outline; a[:, -1, :3] = outline
    # kırpık köşeler
    for k in range(corner):
        for (y, x) in [(k, 0), (0, k), (k, w - 1), (0, w - 1 - k), (h - 1 - k, 0), (h - 1, k), (h - 1 - k, w - 1), (h - 1, w - 1 - k)]:
            a[y, x, 3] = 0
    for (y, x) in [(corner, 1), (1, corner), (corner, w - 2), (1, w - 1 - corner), (h - 1 - corner, 1), (h - 2, corner),
                   (h - 1 - corner, w - 2), (h - 2, w - 1 - corner)]:
        a[y, x, :3] = outline
    return a


def save(a, name):
    Image.fromarray(a, "RGBA").save(os.path.join(OUT, name))
    print("  " + name, a.shape[1], "x", a.shape[0])


def icon(pattern, color, scale=1):
    rows = pattern.strip("\n").split("\n")
    h, w = len(rows), max(len(r) for r in rows)
    a = np.zeros((h, w, 4), dtype=np.uint8)
    for y, r in enumerate(rows):
        for x, ch in enumerate(r):
            if ch == "#":
                a[y, x] = (*color, 255)
            elif ch == "o":
                a[y, x] = (*INK, 255)
    return a


def main():
    os.makedirs(OUT, exist_ok=True)
    save(plaque(24, 16, GOLD_L, GOLD, (255, 250, 210), GOLD_D), "btn_gold_9s.png")
    save(plaque(24, 16, SLATE, SLATE_D, SLATE_L, NIGHT), "btn_stone_9s.png")
    save(plaque(24, 24, hx("2e3350"), NIGHT, SLATE, INK), "panel_9s.png")
    save(plaque(24, 16, BROWN, BROWN_D, hx("b86f50"), INK), "badge_9s.png")

    pause = icon("""
##..##
##..##
##..##
##..##
##..##
##..##
""", CREAM)
    save(pause, "icon_pause.png")
    gear = icon("""
...##...
.#.##.#.
..####..
####.###
###.####
..####..
.#.##.#.
...##...
""", CREAM)
    save(gear, "icon_settings.png")
    person = icon("""
..##..
.####.
..##..
.####.
######
.#..#.
.#..#.
""", CREAM)
    save(person, "icon_character.png")
    lock = icon("""
.####.
.#..#.
.#..#.
######
##.###
##.###
######
""", SLATE_L)
    save(lock, "icon_lock.png")
    play = icon("""
#.....
###...
#####.
######
#####.
###...
#.....
""", INK)
    save(play, "icon_play.png")
    check = icon("""
.......
......#
.....##
#...##.
##.##..
.###...
..#....
""", CREAM)
    save(check, "icon_check.png")
    # Açma/kapama anahtarı
    save(plaque(16, 8, GREEN, hx("265c42"), hx("63c74d"), INK, bevel=1, corner=1), "toggle_on_9s.png")
    save(plaque(16, 8, SLATE_D, NIGHT, SLATE, INK, bevel=1, corner=1), "toggle_off_9s.png")
    knob = plaque(8, 8, CREAM, hx("c28569"), (255, 255, 255), BROWN, bevel=1, corner=1)
    save(knob, "toggle_knob.png")
    # Tam ekran karartma için beyaz nokta (UI Image renklendirilir)
    save(np.full((4, 4, 4), 255, dtype=np.uint8), "white_ui.png")


if __name__ == "__main__":
    main()
