#!/usr/bin/env python3
"""Karakter varyantları: Ata'nın (Assets/Sprites/Player) sprite'larından palet içi renk değişimiyle.
Çıktı: Assets/Sprites/Characters/<Id>/<Yön>/... (aynı klasör yapısı ve dosya adları).
İçe aktarma ayarları kaynaktan kopyalanır (KakMetaSetup.SetupCharacters). Palet: Endesga 32 (KakPalette).
Kullanım: python3 tools/kak_recolor_characters.py
"""
import os
import glob
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..")
SRC = os.path.join(ROOT, "Assets", "Sprites", "Player")
DST = os.path.join(ROOT, "Assets", "Sprites", "Characters")

# Gömlek: 0099db (Camgobegi) + 124e89 (CamgobegiKoyu gölge). Sıcak renkler tehlikeye ayrılmış: kırmızı/turuncu gömlek yok.
VARIANTS = {
    # Yeşil gömlek yok: arena zemini yeşil, oyuncu kaybolur (okunurluk birinci kural)
    "Swift": {"0099db": "b55088", "124e89": "68386c", "733e39": "3e2731"},   # Çevik: pembe-mor, koyu saç
    "Tank":  {"0099db": "8b9bb4", "124e89": "5a6988", "733e39": "3a4466"},   # Tank: çelik zırh + miğfer
    "Lucky": {"0099db": "feae34", "124e89": "f77622"},   # Şanslı: altın
}


def rgb(h):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16))


def main():
    files = [f for f in glob.glob(os.path.join(SRC, "*", "*.png"))]
    for vid, mapping in VARIANTS.items():
        m = {rgb(k): rgb(v) for k, v in mapping.items()}
        n = 0
        for f in files:
            rel = os.path.relpath(f, SRC)
            out = os.path.join(DST, vid, rel)
            os.makedirs(os.path.dirname(out), exist_ok=True)
            im = Image.open(f).convert("RGBA")
            px = im.load()
            for y in range(im.height):
                for x in range(im.width):
                    r, g, b, a = px[x, y]
                    if a and (r, g, b) in m:
                        px[x, y] = m[(r, g, b)] + (a,)
            im.save(out)
            n += 1
        print(vid, n, "sprite")


if __name__ == "__main__":
    main()
