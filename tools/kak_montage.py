#!/usr/bin/env python3
"""Ekran görüntülerini aynı yükseklikte yan yana dizer (etiketli).
Kullanım: python3 tools/kak_montage.py cikti.png girdi1.png girdi2.png ... [--h 900]"""
import os, sys
from PIL import Image, ImageDraw

def main():
    args = sys.argv[1:]
    h = 900
    if "--h" in args:
        i = args.index("--h"); h = int(args[i + 1]); del args[i:i + 2]
    if len(args) < 2:
        print(__doc__); return 1
    out, ins = args[0], args[1:]
    ims = [Image.open(p).convert("RGB") for p in ins]
    ims = [im.resize((int(im.width * h / im.height), h), Image.LANCZOS) for im in ims]
    gap = 12
    W = sum(im.width for im in ims) + gap * (len(ims) + 1)
    sheet = Image.new("RGB", (W, h + 40), (18, 18, 18))
    x = gap
    d = ImageDraw.Draw(sheet)
    for p, im in zip(ins, ims):
        sheet.paste(im, (x, 34))
        d.text((x, 10), os.path.splitext(os.path.basename(p))[0], fill=(230, 230, 230))
        x += im.width + gap
    sheet.save(out)
    print(out)
    return 0

if __name__ == "__main__":
    sys.exit(main())
