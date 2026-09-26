#!/usr/bin/env python3
"""Yumuşak (kenar yumuşatmalı) UI şekilleri: Assets/Art/UI/*_soft.png
Beyaz üretilir, arayüzde renklendirilir (Image.color). 4x süper örnekleme ile kenar yumuşatma.
Kullanım: python3 tools/kak_gen_soft_ui.py
"""
import os
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "UI")
SS = 4


def ring(size=256, outer=124, thickness=20):
    """Radyal dolum (süre halkası) için halka."""
    big = Image.new("L", (size * SS, size * SS), 0)
    d = ImageDraw.Draw(big)
    c = size * SS / 2
    ro, ri = outer * SS, (outer - thickness) * SS
    d.ellipse((c - ro, c - ro, c + ro, c + ro), fill=255)
    d.ellipse((c - ri, c - ri, c + ri, c + ri), fill=0)
    a = big.resize((size, size), Image.LANCZOS)
    img = Image.new("RGBA", (size, size), (255, 255, 255, 0))
    img.putalpha(a)
    return img


def main():
    ring().save(os.path.join(OUT, "ring_soft.png"))
    print("ring_soft.png")


if __name__ == "__main__":
    main()
