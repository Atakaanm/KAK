#!/usr/bin/env python3
"""ADA — kız karakter (esmer, uzun koyu saçlı, zayıf). Ata'nın 40 karesinden (8 yön durma + 8×4 koşu) türetilir:
aynı iskelet ve koşu döngüsü, böylece hareket tutarlı. Her kare için:
  1) renk rolleri (kontur, saç, ten, üst, alt, ayakkabı) → yeni palet (Endesga 32)
  2) gövde 1 px incelir (omuz altında orta sütun çıkarılır)
  3) yöne göre uzun saç: önden yüzü çerçeveler ve omuzlara iner (gövdenin arkasında), arkadan sırta dökülür,
     yandan başın arkasından sırta iner (koşarken geriye savrulur)
  4) yeni saçın çevresine kontur, saç tokası (pembe)
Çıktı: Assets/Sprites/Characters/Ada/<Yön>/<ad>.png (gif kaynaklar da png olur). Önizleme: --preview yol.png
Kullanım: python3 tools/kak_gen_girl.py [--preview out.png]
"""
import os
import sys
import glob
from PIL import Image

ROOT = os.path.join(os.path.dirname(__file__), "..")
SRC = os.path.join(ROOT, "Assets", "Sprites", "Player")
DST = os.path.join(ROOT, "Assets", "Sprites", "Characters", "Ada")


def rgb(h):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16))


INK = rgb("181425")
HAIR, HAIR_HL = rgb("3e2731"), rgb("733e39")
CLIP = rgb("b55088")
# Kaynak renk → rol
SRC_HAIR = {rgb("733e39"), rgb("d77643"), rgb("be4a2f")}
SRC_SHIRT = {rgb("0099db"), rgb("124e89"), rgb("2ce8f5"), rgb("fee761")}
SRC_PANTS = {rgb("262b44"), rgb("3a4466")}
SHIRT_SHADE = rgb("5a6988")
SKIN_NEW = {rgb("e4a672"), rgb("c28569"), rgb("b86f50")}  # üstte gömlek gölgesi, bel altında pantolon

# Yeni renkler
MAP = {
    # saç: koyu kahve-siyah, tutam parlaması kahve
    rgb("733e39"): HAIR, rgb("d77643"): HAIR_HL, rgb("be4a2f"): HAIR_HL,
    # ten: esmer (bir kademe koyu)
    rgb("ead4aa"): rgb("e4a672"), rgb("e8b796"): rgb("c28569"), rgb("e4a672"): rgb("c28569"),
    rgb("c28569"): rgb("b86f50"), rgb("b86f50"): rgb("733e39"),
    # üst: beyaz tişört (Bloom'da parlamasın diye ana ton açık gri, parlama beyaz)
    rgb("0099db"): rgb("c0cbdc"), rgb("2ce8f5"): rgb("ffffff"), rgb("124e89"): rgb("8b9bb4"), rgb("fee761"): rgb("c0cbdc"),
    # alt: mor tayt, pembe parlama
    rgb("262b44"): rgb("68386c"), rgb("3a4466"): rgb("b55088"),
    # ayakkabı aksanı: pembe (kırmızı tehlikeye ayrılmış)
    rgb("e43b44"): rgb("b55088"), rgb("a22633"): rgb("68386c"), rgb("ff0044"): rgb("b55088"),
}

# Yön → saç kuralı
DIRS = {
    "South": "front", "South_East": "front_r", "South_West": "front_l",
    "East": "side_r", "West": "side_l",
    "North": "back", "North_East": "back_r", "North_West": "back_l",
}


def top_row(px, w, h):
    for y in range(h):
        for x in range(w):
            if px[x, y][3]:
                return y
    return 0


def bbox_of(px, w, h, colors, y_max=None):
    xs, ys = [], []
    for y in range(h if y_max is None else min(h, y_max + 1)):
        for x in range(w):
            p = px[x, y]
            if p[3] and p[:3] in colors:
                xs.append(x); ys.append(y)
    if not xs:
        return None
    return min(xs), min(ys), max(xs), max(ys)


def first_row(px, w, h, colors):
    for y in range(h):
        for x in range(w):
            p = px[x, y]
            if p[3] and p[:3] in colors:
                return y
    return h


def opaque(p):
    return p[3] > 0


def slim(img, y0):
    """Omuz çizgisinden aşağıda her satırın orta sütununu çıkarır (gövde 1 px incelir)."""
    px = img.load()
    w, h = img.size
    for y in range(y0, h):
        xs = [x for x in range(w) if opaque(px[x, y])]
        if len(xs) < 5:
            continue
        a, b = xs[0], xs[-1]
        cx = (a + b) // 2
        for x in range(cx, b):
            px[x, y] = px[x + 1, y]
        px[b, y] = (0, 0, 0, 0)


def paint(px, w, h, x, y, col, added, over=False):
    if 0 <= x < w and 0 <= y < h:
        if over or not opaque(px[x, y]):
            px[x, y] = col + (255,)
            added.add((x, y))


def long_hair(img, rule, head, shoulder_y, frame, running):
    px = img.load()
    w, h = img.size
    hx0, hy0, hx1, hy1 = head
    added = set()
    cx = (hx0 + hx1) / 2.0
    hw = hx1 - hx0 + 1
    # Koşarken saç hafifçe zıplar (kareye göre 0/1 px)
    bob = (frame % 2) if running else 0
    length = 11 + bob  # saçın başın altından inme uzunluğu

    def strand(x0, x1, y0, y1, over=False, taper=True, drift=0):
        for y in range(y0, y1 + 1):
            k = (y - y0) / max(1, (y1 - y0))
            dx = int(round(drift * k))
            a, b = x0 + dx, x1 + dx
            if taper and k > 0.75:
                a += 1 if (x1 - x0) > 1 else 0
            for x in range(a, b + 1):
                col = HAIR_HL if (x == a + 1 and k < 0.45 and (b - a) >= 2) else HAIR
                paint(px, w, h, x, y, col, added, over)

    if rule == "back" or rule.startswith("back"):
        # Sırta dökülen saç: omuz altına kadar (pelerin gibi değil), 3/4'te arka tarafa kayar, yüzü örtmez
        three_q = rule != "back"
        shift = 0 if not three_q else (-1 if rule == "back_r" else 1)
        top = hy1 - 1
        blen = 8 + bob
        half = max(3, hw // 2 - 2) - (1 if three_q else 0)
        hl_x = int(cx - half / 2) + shift
        for y in range(top, top + blen + 1):
            k = (y - top) / blen
            cur = half - (1 if k > 0.65 else 0) - (1 if k > 0.9 else 0)
            for x in range(int(cx - cur) + shift, int(cx + cur) + 1 + shift):
                face_side = (rule == "back_r" and x > cx) or (rule == "back_l" and x < cx)
                if three_q and face_side and y <= hy1 + 2 and 0 <= x < w and px[x, y][3] and px[x, y][:3] in SKIN_NEW:
                    continue  # 3/4 arka: yalnızca yüzün döndüğü taraftaki yanak açık kalsın
                col = HAIR_HL if (x == hl_x and 0.1 < k < 0.6) else HAIR
                paint(px, w, h, x, y, col, added, over=True)
        paint(px, w, h, int(cx) + shift, top + blen + 1, HAIR, added, over=True)
    elif rule == "front" or rule.startswith("front"):
        # Yüzü çerçeveleyen yan tutamlar + omuz arkasına inen saç (gövdenin arkasında)
        sides = {"front": (True, True), "front_r": (True, False), "front_l": (False, True)}[rule]
        y_top = hy0 + 5
        y_end = shoulder_y + 6 + bob
        if sides[0]:
            strand(hx0 - 1, hx0 + 1, y_top, y_end)
            for y in range(y_top, hy1 + 1):  # yüz kenarı
                paint(px, w, h, hx0 + 1, y, HAIR, added, over=True)
        if sides[1]:
            strand(hx1 - 1, hx1 + 1, y_top, y_end)
            for y in range(y_top, hy1 + 1):
                paint(px, w, h, hx1 - 1, y, HAIR, added, over=True)
        # 3/4 görünüşte arkadaki taraf daha dolgun
        if rule == "front_r":
            strand(hx0 - 2, hx0 + 1, y_top + 2, y_end + 1)
        elif rule == "front_l":
            strand(hx1 - 1, hx1 + 2, y_top + 2, y_end + 1)
    else:
        # Yandan: başın arka yarısından sırta; koşarken geriye savrulur
        right_face = rule == "side_r"
        back_edge = hx0 if right_face else hx1
        inner = back_edge + 4 if right_face else back_edge - 4
        a, b = (back_edge - 1, inner) if right_face else (inner, back_edge + 1)
        drift = (-3 if right_face else 3) if running else 0
        strand(a, b, hy1 - 3, hy1 + length - 1, over=True, drift=drift)
    return added


def clip_pos(rule, head):
    hx0, hy0, hx1, hy1 = head
    if rule in ("front", "front_l", "side_l", "back_l"):
        return (hx1 - 3, hy0 + 3)
    return (hx0 + 3, hy0 + 3)


def outline_new(img, added):
    px = img.load()
    w, h = img.size
    for (x, y) in list(added):
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if 0 <= nx < w and 0 <= ny < h and not opaque(px[nx, ny]):
                px[nx, ny] = INK + (255,)


def make_girl(path, rule, frame, running):
    src = Image.open(path).convert("RGBA")
    w, h = src.size
    px = src.load()
    # Baş: saç rengi bacak/ayakkabı gölgelerinde de var → yalnızca üst bant (baş ~18 px)
    head = bbox_of(px, w, h, SRC_HAIR, top_row(px, w, h) + 17)
    shoulder = first_row(px, w, h, SRC_SHIRT)
    # Bel: omuzdan en az 6 satır aşağıda, satırda ≥3 pantolon pikseli olan ilk satır
    # (koşu karelerinde gömlek gölgesinde tek tük koyu piksel var; onları bel sanma)
    waist = h
    for y in range(min(h, shoulder + 6), h):
        n = sum(1 for x in range(w) if px[x, y][3] and px[x, y][:3] in SRC_PANTS)
        if n >= 3:
            waist = y
            break
    # 1) renkler (gömlek gölgesi bel altında pantolon)
    out = src.copy()
    op = out.load()
    for y in range(h):
        for x in range(w):
            p = op[x, y]
            if not opaque(p):
                continue
            c = p[:3]
            if c == SHIRT_SHADE:
                op[x, y] = (rgb("b55088") if y >= waist else rgb("8b9bb4")) + (p[3],)
            elif c in MAP:
                op[x, y] = MAP[c] + (p[3],)
    # 2) zayıf gövde
    if shoulder < h:
        slim(out, shoulder + 1)
    # 3) uzun saç
    if head:
        added = long_hair(out, rule, head, shoulder, frame, running)
        outline_new(out, added)
        # 4) saç tokası (ön ve yan görünüşte)
        if not rule.startswith("back"):
            x, y = clip_pos(rule, head)
            op = out.load()
            for dx in (0, 1):
                if 0 <= x + dx < w and opaque(op[x + dx, y]) and op[x + dx, y][:3] in (HAIR, HAIR_HL):
                    op[x + dx, y] = CLIP + (255,)
    return out


def all_frames():
    for d, rule in DIRS.items():
        folder = os.path.join(SRC, d)
        for f in sorted(os.listdir(folder)):
            if f.endswith(".meta") or not f.lower().endswith((".png", ".gif")):
                continue
            running = "_Run_" in f
            frame = int(f.split("_Run_")[1].split(".")[0]) - 1 if running else 0
            yield d, rule, os.path.join(folder, f), f, running, frame


def main():
    preview = None
    if "--preview" in sys.argv:
        preview = sys.argv[sys.argv.index("--preview") + 1]
    made = []
    for d, rule, path, name, running, frame in all_frames():
        img = make_girl(path, rule, frame, running)
        out_dir = os.path.join(DST, d)
        os.makedirs(out_dir, exist_ok=True)
        out_name = os.path.splitext(name)[0] + ".png"
        img.save(os.path.join(out_dir, out_name))
        made.append((d, running, frame, img))
    print(len(made), "kare ->", os.path.relpath(DST, ROOT))
    if preview:
        order = ["South", "South_East", "East", "North_East", "North", "North_West", "West", "South_West"]
        sc, cell = 5, 52
        sheet = Image.new("RGBA", (5 * cell * sc + 40, len(order) * cell * sc + 40), (62, 137, 72, 255))
        for r, d in enumerate(order):
            row = sorted([m for m in made if m[0] == d], key=lambda m: (m[1], m[2]))
            for c, m in enumerate(row):
                im = m[3]
                ox = (cell - im.width) // 2
                big = im.resize((im.width * sc, im.height * sc), Image.NEAREST)
                sheet.alpha_composite(big, (c * (cell * sc + 8) + ox * sc, r * (cell * sc + 5)))
        sheet.save(preview)


if __name__ == "__main__":
    main()
