#!/usr/bin/env python3
"""
Faz 3 sanat geçişi (sanat-rehberi.md): mevcut piksel sanatı karakterleri palete ve dış hat kuralına getirir,
taş için okunur "tehlike" sprite'ı üretir. Tekrar çalıştırılırsa zaten işlenmiş dosyaları atlar (işaret dosyası).

  python3 tools/kak_art_pass.py player     # Assets/Sprites/Player/** : palete indir + 1 px kontur
  python3 tools/kak_art_pass.py spawner    # Assets/Sprites/Spawner/** : aydınlat + palete indir + kontur
  python3 tools/kak_art_pass.py rock       # Assets/Art/Projectiles/rock.png : sıcak rampa, 26 px + kontur
Orijinaller git geçmişindedir.
"""
import glob
import os
import sys

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_palette import PALETTE, quantize_rgba, outline, pad_canvas  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MARK = ".kak_art_pass_done"


def save_like(img, path):
    """Aynı biçimde kaydet: GIF ise saydamlıklı paletli GIF, değilse PNG."""
    if path.lower().endswith(".gif"):
        rgba = np.asarray(img.convert("RGBA"))
        colors = [tuple(c) for c in PALETTE.tolist()]
        lut = {c: i + 1 for i, c in enumerate(colors)}
        idx = np.zeros(rgba.shape[:2], dtype=np.uint8)
        for y in range(rgba.shape[0]):
            for x in range(rgba.shape[1]):
                r, g, b, a = rgba[y, x]
                idx[y, x] = 0 if a < 128 else lut.get((int(r), int(g), int(b)), 1)
        p = Image.fromarray(idx, "P")
        pal = [0, 0, 0] + [v for c in colors for v in c]
        p.putpalette(pal + [0] * (768 - len(pal)))
        p.save(path, "GIF", transparency=0)
    else:
        img.save(path)


def process_folder(folder, fn):
    mark = os.path.join(folder, MARK)
    if os.path.exists(mark):
        print("atlandı (zaten işlenmiş): " + folder)
        return
    files = [f for f in glob.glob(os.path.join(folder, "**", "*"), recursive=True)
             if f.lower().endswith((".png", ".gif"))]
    for f in files:
        img = Image.open(f).convert("RGBA")
        save_like(fn(img), f)
    open(mark, "w").write("Faz 3 sanat geçişi uygulandı\n")
    print("%d dosya işlendi: %s" % (len(files), folder))


def player_fn(img):
    return outline(quantize_rgba(img))


def spawner_fn(img):
    a = np.asarray(img).astype(float)
    a[..., :3] = np.clip(a[..., :3] * 1.35 + 10, 0, 255)
    return outline(quantize_rgba(Image.fromarray(a.astype(np.uint8), "RGBA")))


def rock():
    src = Image.open(os.path.join(ROOT, "Assets/Sprites/Projectiles/Rock_02.png")).convert("RGBA")
    rc = src.crop(src.getbbox())
    side = max(rc.size)
    sq = Image.new("RGBA", (side, side))
    sq.paste(rc, ((side - rc.width) // 2, (side - rc.height) // 2))
    s = np.asarray(sq.resize((26, 26), Image.LANCZOS)).astype(float)
    alpha = s[..., 3] >= 128
    lum = (0.299 * s[..., 0] + 0.587 * s[..., 1] + 0.114 * s[..., 2])
    # Lav çatlakları: kırmızı baskın pikseller
    lava = (s[..., 0] > s[..., 2] + 25) & (s[..., 0] > 110)
    # Sıcak rampa (koyu → açık): kahve_koyu, kahve, bakır, ten
    ramp = np.array([[0x3e, 0x27, 0x31], [0x73, 0x3e, 0x39], [0xb8, 0x6f, 0x50], [0xe4, 0xa6, 0x72]])
    v = lum[alpha]
    qs = np.quantile(v, [0.25, 0.55, 0.85]) if len(v) else [60, 110, 160]
    level = np.digitize(lum, qs)
    out = np.zeros((26, 26, 4), dtype=np.uint8)
    out[..., :3] = ramp[np.clip(level, 0, 3)]
    out[lava & alpha, :3] = (0xf7, 0x76, 0x22)
    out[..., 3] = np.where(alpha, 255, 0)
    img = outline(pad_canvas(Image.fromarray(out, "RGBA")))
    dst = os.path.join(ROOT, "Assets/Art/Projectiles")
    os.makedirs(dst, exist_ok=True)
    img.save(os.path.join(dst, "rock.png"))
    print("taş: Assets/Art/Projectiles/rock.png " + str(img.size))


def main():
    what = sys.argv[1:] or ["player", "spawner", "rock"]
    if "player" in what:
        process_folder(os.path.join(ROOT, "Assets/Sprites/Player"), player_fn)
    if "spawner" in what:
        process_folder(os.path.join(ROOT, "Assets/Sprites/Spawner"), spawner_fn)
    if "rock" in what:
        rock()


if __name__ == "__main__":
    main()
