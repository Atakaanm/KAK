#!/usr/bin/env python3
"""KaçAtaKaç paleti (Endesga 32) ve yardımcılar: renk indirgeme, dış hat, küçültme."""
import numpy as np
from PIL import Image

HEX = ("be4a2f d77643 ead4aa e4a672 b86f50 733e39 3e2731 a22633 e43b44 f77622 feae34 fee761 "
       "63c74d 3e8948 265c42 193c3e 124e89 0099db 2ce8f5 ffffff c0cbdc 8b9bb4 5a6988 3a4466 "
       "262b44 181425 ff0044 68386c b55088 f6757a e8b796 c28569").split()
PALETTE = np.array([[int(h[i:i + 2], 16) for i in (0, 2, 4)] for h in HEX], dtype=np.int32)
ROLE = {  # sanat-rehberi.md renk rolleri
    "murekkep": "181425", "gece": "262b44", "derin_camgobegi": "193c3e", "orman": "265c42",
    "yesil": "3e8948", "acik_yesil": "63c74d", "arduvaz_koyu": "3a4466", "arduvaz": "5a6988",
    "arduvaz_acik": "8b9bb4", "sis": "c0cbdc", "beyaz": "ffffff", "krem": "ead4aa",
    "ten": "e4a672", "kahve_koyu": "3e2731", "kahve": "733e39", "bakir": "b86f50",
    "tehlike_koyu": "a22633", "tehlike": "e43b44", "tehlike_parlak": "ff0044",
    "turuncu_koyu": "be4a2f", "turuncu": "f77622", "turuncu_acik": "d77643",
    "altin": "feae34", "altin_acik": "fee761", "camgobegi_koyu": "124e89",
    "camgobegi": "0099db", "camgobegi_parlak": "2ce8f5", "mor": "68386c", "pembe": "b55088",
}


def quantize(rgb, palette=PALETTE, subset=None):
    """Algısal (redmean) en yakın palet rengi. rgb: (...,3) uint8."""
    p = palette if subset is None else np.array(subset, dtype=np.int32)
    a = rgb.reshape(-1, 3).astype(np.int32)
    out = np.empty_like(a)
    for i in range(0, len(a), 150000):
        c = a[i:i + 150000]
        rm = (c[:, None, 0] + p[None, :, 0]) / 2
        dr = c[:, None, 0] - p[None, :, 0]
        dg = c[:, None, 1] - p[None, :, 1]
        db = c[:, None, 2] - p[None, :, 2]
        d = (2 + rm / 256) * dr * dr + 4 * dg * dg + (2 + (255 - rm) / 256) * db * db
        out[i:i + 150000] = p[d.argmin(1)]
    return out.reshape(rgb.shape).astype(np.uint8)


def quantize_rgba(img, alpha_cut=128):
    a = np.asarray(img.convert("RGBA")).copy()
    a[..., :3] = quantize(a[..., :3])
    a[..., 3] = np.where(a[..., 3] >= alpha_cut, 255, 0)
    return Image.fromarray(a, "RGBA")


def outline(img, color=(0x18, 0x14, 0x25), diagonal=False):
    """Opak piksellerin dışına 1 px kontur (piksel sanatı)."""
    a = np.asarray(img.convert("RGBA")).copy()
    op = a[..., 3] > 0
    pad = np.pad(op, 1)
    n = pad[:-2, 1:-1] | pad[2:, 1:-1] | pad[1:-1, :-2] | pad[1:-1, 2:]
    if diagonal:
        n |= pad[:-2, :-2] | pad[:-2, 2:] | pad[2:, :-2] | pad[2:, 2:]
    ring = n & ~op
    a[ring, :3] = color
    a[ring, 3] = 255
    return Image.fromarray(a, "RGBA")


def pad_canvas(img, px=1):
    w, h = img.size
    c = Image.new("RGBA", (w + 2 * px, h + 2 * px), (0, 0, 0, 0))
    c.paste(img, (px, px))
    return c


def palette_image():
    return Image.fromarray(PALETTE.astype(np.uint8).reshape(1, -1, 3), "RGB")
