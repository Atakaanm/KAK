#!/usr/bin/env python3
"""
Siyah zemin üzerine üretilmiş (yapay zeka) ikonları oyuna hazırlar.

1) Kenarlardan flood-fill ile "zemin" bölgesini bulur (koyu + sönük parlama pikselleri).
2) Zemin bölgesinde siyahtan ön-çarpımı geri alır: alpha = parlaklık, renk = renk / alpha
   → parlama (glow) yarı saydam korunur, saf siyah tamamen saydam olur.
3) Nesnenin içindeki koyu çizgiler (kenardan ulaşılamayan) opak kalır.
4) İsteğe bağlı: belirli bir bölgeyi sil (ör. görsele gömülü yazı).
5) İçeriğe göre kırpar, kareye tamamlar, hedef boyuta küçültür (varsayılan 256).

Kullanım:
  python3 tools/kak_icon_cleanup.py <girdi.png> [--out cikti.png] [--size 256] [--erase-below 0.73] [--bg 70]
Çıktı verilmezse girdinin üzerine yazar (git geçmişi yedektir).
"""
import argparse
from collections import deque

import numpy as np
from PIL import Image


def clean(path, out, size, erase_below, bg_thr):
    im = Image.open(path).convert("RGB")
    a = np.asarray(im).astype(np.float64)
    h, w, _ = a.shape
    maxc = a.max(axis=2)

    if erase_below is not None:
        a[int(h * erase_below):, :, :] = 0
        maxc = a.max(axis=2)

    # 1) Kenardan flood-fill: bg_thr altındaki pikseller zemin olabilir
    cand = maxc < bg_thr
    bg = np.zeros((h, w), dtype=bool)
    q = deque()
    for x in range(w):
        for y in (0, h - 1):
            if cand[y, x] and not bg[y, x]:
                bg[y, x] = True; q.append((y, x))
    for y in range(h):
        for x in (0, w - 1):
            if cand[y, x] and not bg[y, x]:
                bg[y, x] = True; q.append((y, x))
    while q:
        y, x = q.popleft()
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < h and 0 <= nx < w and cand[ny, nx] and not bg[ny, nx]:
                bg[ny, nx] = True
                q.append((ny, nx))

    # 2) Zemin ve hemen yanındaki parlama halkası: siyahtan ön-çarpımı geri al
    alpha = np.ones((h, w))
    glow_alpha = np.clip(maxc / bg_thr, 0, 1)
    alpha[bg] = glow_alpha[bg]
    rgb = a.copy()
    safe = np.maximum(alpha, 1e-3)[..., None]
    rgb[bg] = np.clip(a[bg] / safe[bg], 0, 255)
    alpha[maxc < 6] = 0  # saf siyah tamamen saydam

    rgba = np.dstack([rgb, alpha * 255]).astype(np.uint8)
    img = Image.fromarray(rgba, "RGBA")

    # 5) İçeriğe göre kırp (alpha > ~8%), kareye tamamla, küçült
    ys, xs = np.where(rgba[..., 3] > 20)
    if len(xs):
        x0, x1, y0, y1 = xs.min(), xs.max(), ys.min(), ys.max()
        side = int(max(x1 - x0, y1 - y0) * 1.06) + 2
        cx, cy = (x0 + x1) // 2, (y0 + y1) // 2
        box = (cx - side // 2, cy - side // 2, cx - side // 2 + side, cy - side // 2 + side)
        canvas = Image.new("RGBA", (side, side), (0, 0, 0, 0))
        canvas.paste(img.crop((max(box[0], 0), max(box[1], 0), min(box[2], w), min(box[3], h))),
                     (max(-box[0], 0), max(-box[1], 0)))
        img = canvas
    img = img.resize((size, size), Image.LANCZOS)
    img.save(out)
    op = (np.asarray(img)[..., 3] > 250).mean() * 100
    tr = (np.asarray(img)[..., 3] < 5).mean() * 100
    print(f"{path} → {out}  {size}x{size}  opak %{op:.0f}, saydam %{tr:.0f}")


def main():
    p = argparse.ArgumentParser()
    p.add_argument("input")
    p.add_argument("--out")
    p.add_argument("--size", type=int, default=256)
    p.add_argument("--erase-below", type=float, default=None)
    p.add_argument("--bg", type=float, default=70)
    a = p.parse_args()
    clean(a.input, a.out or a.input, a.size, a.erase_below, a.bg)


if __name__ == "__main__":
    main()
