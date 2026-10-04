"""Faz 15 K4: ortak gelişim izlerinin ikonları (16 px beyaz şekil → 128 px; renk grubundan koddan gelir).
Aynı dil: kak_gen_icons.py arayüz ikonları (tek kalınlık, dolu siluet, okunur küçük boyut).
Çıktı: Assets/Art/UI/Upgrades/up_<id>.png
Kullanım: python3 tools/kak_gen_upicons.py [--preview]
"""
import math, os, sys
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import C, ROOT, SCRATCH  # noqa: E402

N = 16
OUT = os.path.join(ROOT, "Assets", "Art", "UI", "Upgrades")

def heart(x, y, cx=7.5, cy=7.0, s=6.3):
    X, Y = (x - cx) / s, (y - cy) / s
    return (X * X + Y * Y - 1) ** 3 - X * X * (-Y) ** 3 <= 0

def ring(x, y, cx, cy, r0, r1):
    d = math.hypot(x - cx, y - cy)
    return r0 <= d <= r1

def seg(x, y, x0, y0, x1, y1, w):
    dx, dy = x1 - x0, y1 - y0
    t = max(0.0, min(1.0, ((x - x0) * dx + (y - y0) * dy) / (dx * dx + dy * dy)))
    return math.hypot(x - (x0 + dx * t), y - (y0 + dy * t)) <= w

def shield(x, y):
    if y < 2 or y > 14: return False
    t = (y - 2) / 12.0
    half = 6 if t < 0.5 else 6 * math.cos((t - 0.5) * math.pi) + 0.4
    return abs(x - 7.5) <= half

def plus(x, y, cx, cy, r=2):
    return (abs(x - cx) <= 0.6 and abs(y - cy) <= r) or (abs(y - cy) <= 0.6 and abs(x - cx) <= r)

ICONS = {
    "hearts": lambda x, y: heart(x, y),
    "invuln": lambda x, y: (heart(x, y, 6.5, 8.0, 5.6) and not heart(x, y, 6.5, 8.0, 3.4)) or plus(x, y, 12.5, 3.0, 2),
    "slim": lambda x, y: ring(x, y, 7.5, 3.0, 0, 2.0) or (6 <= x <= 9 and 5 <= y <= 10) or seg(x, y, 6.5, 10, 5.5, 15, 0.8) or seg(x, y, 8.5, 10, 9.5, 15, 0.8)
                         or seg(x, y, 2, 7, 4, 7, 0.6) or seg(x, y, 11, 7, 13, 7, 0.6),
    "sense": lambda x, y: ((x - 7.5) ** 2 / 52 + (y - 7.5) ** 2 / 14 <= 1 and not ((x - 7.5) ** 2 / 26 + (y - 7.5) ** 2 / 5 <= 1)) or ring(x, y, 7.5, 7.5, 0, 1.6),
    "startshield": lambda x, y: shield(x, y),
    "shieldregen": lambda x, y: (shield(x, y) and not (abs(x - 7.5) <= 0.6 and 5 <= y <= 11) and not (abs(y - 7.5) <= 0.6 and 5 <= x <= 10)) ,
    "revive": lambda x, y: (heart(x, y, 7.5, 9.0, 5.4)) and not (seg(x, y, 7.5, 6.5, 7.5, 12, 0.7) or seg(x, y, 7.5, 6.5, 5.5, 8.5, 0.7) or seg(x, y, 7.5, 6.5, 9.5, 8.5, 0.7)),
    "speed": lambda x, y: seg(x, y, 2, 3, 7, 7.5, 1.0) or seg(x, y, 7, 7.5, 2, 12, 1.0) or seg(x, y, 8, 3, 13, 7.5, 1.0) or seg(x, y, 13, 7.5, 8, 12, 1.0),
    "magnet": lambda x, y: (ring(x, y, 7.5, 7.0, 3.2, 6.2) and y >= 6.5) or (2 <= x <= 4.8 and 2 <= y <= 7) or (10.2 <= x <= 13 and 2 <= y <= 7),
    "coin": lambda x, y: (ring(x, y, 6.5, 9.0, 0, 5.5) and not (abs(x - 6.5) <= 0.6 and 6 <= y <= 12)) or plus(x, y, 13, 3, 2),
    "score": lambda x, y: star(x, y),
    "nearbonus": lambda x, y: ring(x, y, 9.5, 9.5, 0, 4.0) or seg(x, y, 1, 4, 6, 4, 0.6) or seg(x, y, 2, 1, 7, 1, 0.5) or seg(x, y, 1, 7, 4, 7, 0.5),
    "nearradius": lambda x, y: ring(x, y, 7.5, 7.5, 0, 1.8) or ring(x, y, 7.5, 7.5, 4.0, 5.0) or ring(x, y, 7.5, 7.5, 6.6, 7.4),
    "combokeep": lambda x, y: seg(x, y, 2, 4, 8, 12, 1.0) or seg(x, y, 8, 4, 2, 12, 1.0) or (10 <= x <= 14 and 9 <= y <= 14) or (ring(x, y, 12.0, 8.5, 1.3, 2.4) and y <= 8.5),
    "combomax": lambda x, y: seg(x, y, 2, 6, 8, 14, 1.0) or seg(x, y, 8, 6, 2, 14, 1.0) or seg(x, y, 12, 2, 12, 9, 0.7) or seg(x, y, 12, 2, 9.5, 4.5, 0.7) or seg(x, y, 12, 2, 14.5, 4.5, 0.7),
    "kind_freq": lambda x, y: ring(x, y, 4.0, 11.0, 0, 2.6) or ring(x, y, 11.5, 11.0, 0, 2.6) or ring(x, y, 7.7, 4.6, 0, 2.6) or plus(x, y, 13.0, 3.0, 2),
    "kind_ground": lambda x, y: (ring(x, y, 7.5, 6.0, 0, 4.6) and not ring(x, y, 7.5, 6.0, 0, 1.7)) or (abs(x - 7.5) <= max(0.0, (13.5 - y) * 0.55) and 8 <= y <= 13.5) or (2 <= x <= 13 and y == 15),
    "kind_power": lambda x, y: seg(x, y, 9.5, 1, 4.5, 8.5, 1.2) or seg(x, y, 4.5, 8.5, 10.5, 7.5, 1.0) or seg(x, y, 10.5, 7.5, 5.5, 15, 1.2),
    "power": lambda x, y: (2 <= y <= 3 or 12 <= y <= 13) and 3 <= x <= 12 or (4 <= y <= 11 and abs(x - 7.5) <= 0.6 + abs(y - 7.5) * 0.75),
}

def star(x, y, cx=7.5, cy=8.0, r1=7.2, r0=3.0):
    a = math.atan2(y - cy, x - cx) + math.pi / 2
    k = (a % (2 * math.pi / 5)) / (2 * math.pi / 5)
    edge = r0 + (r1 - r0) * abs(1 - 2 * k) ** 1.4
    return math.hypot(x - cx, y - cy) <= edge

def draw(fn):
    img = Image.new("RGBA", (N, N), (0, 0, 0, 0)); px = img.load()
    for y in range(N):
        for x in range(N):
            if fn(x, y): px[x, y] = (255, 255, 255, 255)
    return img.resize((N * 8, N * 8), Image.NEAREST)

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for k, fn in ICONS.items():
        draw(fn).save(os.path.join(OUT, "up_" + k + ".png"))
    print(len(ICONS), "ikon →", OUT)
    # Menü / sekme: GELİŞİM ikonu (yukarı ok + taban çizgisi; arayüz ikonlarıyla aynı 16 px dil)
    up = lambda x, y: seg(x, y, 7.5, 2, 7.5, 11, 1.2) or seg(x, y, 7.5, 2, 3, 6.5, 1.2) or seg(x, y, 7.5, 2, 12, 6.5, 1.2) or (2 <= x <= 13 and 13 <= y <= 14)
    draw(up).save(os.path.join(ROOT, "Assets", "Art", "UI", "icon_upgrade.png"))
    # K5/K6: parşömen (eşya geliştirmenin üst seviyeleri; sandıktan çıkar) — renkli küçük piksel çizim
    sc = Image.new("RGBA", (N, N), (0, 0, 0, 0)); px = sc.load()
    for y in range(N):
        for x in range(N):
            body = 3 <= x <= 12 and 3 <= y <= 12
            roll_top = 2 <= x <= 13 and 2 <= y <= 3
            roll_bot = 2 <= x <= 13 and 12 <= y <= 13
            if roll_top or roll_bot: px[x, y] = C("bakir") if y in (3, 12) else C("turuncu_acik")
            elif body: px[x, y] = C("krem")
            if body and y in (6, 8, 10) and 5 <= x <= 10: px[x, y] = C("kahve")
    from kak_gen_ice import outline
    outline(sc).resize((N * 8, N * 8), Image.NEAREST).save(os.path.join(ROOT, "Assets", "Art", "UI", "icon_scroll.png"))
    if "--preview" in sys.argv:
        cols = {"hearts": "camgobegi_parlak", "invuln": "camgobegi_parlak", "slim": "camgobegi_parlak", "sense": "camgobegi_parlak",
                "startshield": "camgobegi_parlak", "shieldregen": "camgobegi_parlak", "revive": "camgobegi_parlak", "speed": "acik_yesil",
                "power": "pembe"}
        keys = list(ICONS)
        sheet = Image.new("RGBA", (len(keys) * 150 + 20, 170), C("gece"))
        for i, k in enumerate(keys):
            im = Image.open(os.path.join(OUT, "up_" + k + ".png")).convert("RGBA")
            tint = Image.new("RGBA", im.size, C(cols.get(k, "altin"))); tint.putalpha(im.split()[3])
            sheet.alpha_composite(tint, (20 + i * 150, 20))
        sheet.save(os.path.join(SCRATCH, "audit", "upicons.png"))
        print("önizleme:", os.path.join(SCRATCH, "audit", "upicons.png"))
