#!/usr/bin/env python3
"""
KaçAtaKaç — renk ve okunabilirlik testleri (sanat-rehberi.md §4).

Bir ekran görüntüsünden şunları üretir:
  - gri tonlama (değer testi)
  - deuteranopi ve protanopi simülasyonu (Machado 2009, şiddet 1.0)
  - bulanıklık (siluet/okunabilirlik testi)
  - hepsini yan yana koyan tek bir karşılaştırma sayfası (_kontrol.png)
ve konsola değer (açıklık) dağılımı + baskın renk aileleri raporu yazar.

Kullanım:
  python3 tools/kak_color.py <görüntü.png> [<görüntü2.png> ...]
  python3 tools/kak_color.py --palette <palet.png> <görüntü.png>   # palet dışı piksel oranı da raporlanır
"""
import os
import sys

import numpy as np
from PIL import Image, ImageFilter, ImageDraw

DEUTER = np.array([[0.367322, 0.860646, -0.227968],
                   [0.280085, 0.672501, 0.047413],
                   [-0.011820, 0.042940, 0.968881]])
PROTAN = np.array([[0.152286, 1.052583, -0.204868],
                   [0.114503, 0.786281, 0.099216],
                   [-0.003882, -0.048116, 1.051998]])


def srgb_to_linear(c):
    c = c / 255.0
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def linear_to_srgb(c):
    c = np.clip(c, 0, 1)
    return np.where(c <= 0.0031308, c * 12.92, 1.055 * (c ** (1 / 2.4)) - 0.055) * 255.0


def simulate(arr, m):
    lin = srgb_to_linear(arr[..., :3].astype(np.float64))
    out = lin @ m.T
    return linear_to_srgb(out).astype(np.uint8)


def luminance(arr):
    lin = srgb_to_linear(arr[..., :3].astype(np.float64))
    y = 0.2126 * lin[..., 0] + 0.7152 * lin[..., 1] + 0.0722 * lin[..., 2]
    # Algısal açıklık (L*) 0-100
    return np.where(y > 0.008856, 116 * np.cbrt(y) - 16, 903.3 * y)


def hue_family(arr):
    img = Image.fromarray(arr[..., :3]).convert("HSV")
    hsv = np.asarray(img).astype(np.float64)
    h = hsv[..., 0] * 360 / 255
    s = hsv[..., 1] / 255
    v = hsv[..., 2] / 255
    fam = np.full(h.shape, "nötr/gri", dtype=object)
    colored = (s > 0.18) & (v > 0.12)
    bins = [(0, 20, "kırmızı"), (20, 45, "turuncu"), (45, 70, "sarı"), (70, 160, "yeşil"),
            (160, 200, "camgöbeği"), (200, 260, "mavi"), (260, 320, "mor"), (320, 361, "kırmızı")]
    for lo, hi, name in bins:
        fam[colored & (h >= lo) & (h < hi)] = name
    return fam


def palette_check(arr, palette_path):
    pal = np.asarray(Image.open(palette_path).convert("RGB")).reshape(-1, 3)
    pal = np.unique(pal, axis=0).astype(np.int32)
    px = arr[..., :3].reshape(-1, 3).astype(np.int32)
    # Örnekleme (büyük görüntülerde hız için)
    if len(px) > 200000:
        idx = np.random.default_rng(0).choice(len(px), 200000, replace=False)
        px = px[idx]
    d = ((px[:, None, :] - pal[None, :, :]) ** 2).sum(-1).min(1)
    return (d > 12).mean() * 100, len(pal)


def label(img, text):
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, img.width, 34], fill=(0, 0, 0))
    d.text((8, 8), text, fill=(255, 255, 255))
    return img


def process(path, palette=None):
    src = Image.open(path).convert("RGB")
    arr = np.asarray(src)
    base, _ = os.path.splitext(path)

    gray = Image.fromarray(np.clip(luminance(arr) * 2.55, 0, 255).astype(np.uint8)).convert("RGB")
    deut = Image.fromarray(simulate(arr, DEUTER))
    prot = Image.fromarray(simulate(arr, PROTAN))
    blur = src.filter(ImageFilter.GaussianBlur(radius=max(2, src.width // 90)))

    panels = [("orijinal", src), ("gri ton", gray), ("deuteranopi", deut), ("protanopi", prot), ("bulanık", blur)]
    w = 360
    h = int(src.height * w / src.width)
    sheet = Image.new("RGB", (w * len(panels), h), (20, 20, 20))
    for i, (name, im) in enumerate(panels):
        sheet.paste(label(im.resize((w, h), Image.LANCZOS), name), (i * w, 0))
    out = base + "_kontrol.png"
    sheet.save(out)

    L = luminance(arr)
    dark = (L < 25).mean() * 100
    mid = ((L >= 25) & (L < 60)).mean() * 100
    light = (L >= 60).mean() * 100
    fam = hue_family(arr)
    names, counts = np.unique(fam, return_counts=True)
    order = np.argsort(-counts)

    print("== " + os.path.basename(path) + "  (" + str(src.width) + "x" + str(src.height) + ")")
    print("   Değer dağılımı: koyu %%%.0f | orta %%%.0f | açık %%%.0f   (hedef kabaca 60 ortam / 30 ikincil / 10 vurgu)" % (dark, mid, light))
    print("   Renk aileleri: " + ", ".join("%s %%%.0f" % (names[i], counts[i] * 100 / fam.size) for i in order[:6]))
    if palette:
        off, n = palette_check(arr, palette)
        print("   Palet dışı piksel: %%%.1f  (palet: %d renk)" % (off, n))
    print("   Karşılaştırma sayfası: " + out)


def main():
    args = sys.argv[1:]
    palette = None
    if "--palette" in args:
        i = args.index("--palette")
        palette = args[i + 1]
        del args[i:i + 2]
    if not args:
        print(__doc__)
        return 1
    for p in args:
        process(p, palette)
    return 0


if __name__ == "__main__":
    sys.exit(main())
