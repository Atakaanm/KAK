"""Faz 13 F1: Futbol Arenası sanatı (tek palet: Endesga 32).
  arena       → Assets/Sprites/Arena/Football_arena_01.png (341 px × 6; zindan geometrisi: oynanabilir alan 27 px'ten,
                 köşe kaideleri = hakem platformları, üst duvarda kale, alt girinti = oyuncu tüneli)
  tiles       → Assets/Art/Tiles/Football/ (tribün kalabalığı, tünel betonu, reklam panoları)
  referees    → Assets/Sprites/Spawner_Football/<Yön>/idle.png, attack_1..4.png (Ata'nın kareleri hakem formasına boyanır)
  projectiles → Assets/Art/Projectiles/ball.png, card_yellow.png, card_red.png
  preview     → Assets/Sprites/Arena/Football_preview.png
Kullanım: python3 tools/kak_gen_football.py [all|arena|tiles|referees|projectiles|preview] [--preview]
"""
import math, os, random, sys, glob
from PIL import Image, ImageDraw, ImageFilter
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import C, outline, ROOT, SCRATCH  # noqa: E402

INK, NIGHT, SLATE_D, SLATE, SLATE_L, MIST, WHITE = C("murekkep"), C("gece"), C("arduvaz_koyu"), C("arduvaz"), C("arduvaz_acik"), C("sis"), C("beyaz")
G_D, G, G_L = C("orman"), C("yesil"), C("acik_yesil")
RED, RED_D, GOLD, GOLD_L, CYAN, PINK = C("tehlike"), C("tehlike_koyu"), C("altin"), C("altin_acik"), C("camgobegi"), C("pembe")

def arena():
    N = 341
    rnd = random.Random(21)
    img = Image.new("RGBA", (N, N), NIGHT)
    px = img.load()
    rim, wall = 10, 27
    # Çim: yatay biçilmiş şeritler (koyu/açık), hafif çim tanesi
    stripe = 24
    for y in range(wall, N - wall):
        for x in range(wall, N - wall):
            band = ((y - wall) // stripe) % 2
            c = G_D if band == 0 else G
            if rnd.random() < 0.012 and band == 0: c = G  # seyrek çim tanesi (yoğunu parazit gibi görünüyordu)
            px[x, y] = c
    # Saha çizgileri (beyaz): kenar, orta çizgi, orta yuvarlak, ceza ve kale alanları, köşe yayları
    L = wall + 7; R = N - wall - 8; T = wall + 7; B = N - wall - 8
    def hline(x0, x1, y):
        for x in range(x0, x1 + 1): px[x, y] = WHITE; px[x, y + 1] = MIST
    def vline(x, y0, y1):
        for y in range(y0, y1 + 1): px[x, y] = WHITE; px[x + 1, y] = MIST
    hline(L, R, T); hline(L, R, B); vline(L, T, B); vline(R, T, B)
    cy = N // 2
    hline(L, R, cy)
    cx = N // 2
    for a in range(0, 360):
        r = 38
        x, y = int(cx + math.cos(math.radians(a)) * r), int(cy + math.sin(math.radians(a)) * r)
        px[x, y] = WHITE
    for (x, y) in ((cx, cy), (cx + 1, cy), (cx, cy + 1), (cx + 1, cy + 1)): px[x, y] = WHITE
    for top in (True, False):
        y0 = T if top else B
        d = 1 if top else -1
        bw, bd, gw, gd = 150, 52, 70, 20
        for k in range(bd + 1):
            px[cx - bw // 2, y0 + d * k] = WHITE; px[cx + bw // 2, y0 + d * k] = WHITE
        hline(cx - bw // 2, cx + bw // 2, y0 + d * bd)
        for k in range(gd + 1):
            px[cx - gw // 2, y0 + d * k] = WHITE; px[cx + gw // 2, y0 + d * k] = WHITE
        hline(cx - gw // 2, cx + gw // 2, y0 + d * gd)
        # ceza yayı
        for a in range(20, 161):
            ang = math.radians(a if top else -a)
            x, y = int(cx + math.cos(ang) * 30), int(y0 + d * bd + math.sin(math.radians(a)) * 14 * d)
            if (y - (y0 + d * bd)) * d > 0: px[x, y] = WHITE
    # Duvar: reklam panoları (iç), tribün (dış)
    ads = [CYAN, RED, GOLD, PINK, WHITE, G_L]
    for y in range(N):
        for x in range(N):
            d = min(x, y, N - 1 - x, N - 1 - y)
            along = x if (y < wall or y >= N - wall) else y
            if rim + 6 <= d < wall:
                seg = (along // 34) % len(ads)
                t = d - (rim + 6)
                col = ads[seg] if 2 <= t <= 7 else SLATE_D if t < 2 else NIGHT
                if 3 <= t <= 6 and (along % 34) in range(8, 26) and (along + t) % 3 == 0: col = WHITE if ads[seg] != WHITE else SLATE  # yazı benzeri
                px[x, y] = col
            elif d < rim + 6:
                # tribün: sıra sıra seyirci — 2 piksellik baş (ten) + altında forma rengi, aralarda boşluk; koyu koltuklar
                row = d % 4
                seat = (along // 3)
                rr = random.Random(seat * 31 + d // 4)
                occupied = rr.random() < 0.6
                shirt = rr.choice([RED, CYAN, GOLD, WHITE, PINK])
                col = SLATE_D if row == 0 else NIGHT
                if occupied and along % 3 != 2:
                    if row == 1: col = C("ten") if rr.random() < 0.8 else C("kahve")
                    elif row == 2: col = shirt
                px[x, y] = col
    for i in range(wall, N - wall):
        for (x, y) in ((i, wall), (wall, i), (i, N - wall - 1), (N - wall - 1, i)):
            px[x, y] = INK
    # Üst kale: ağ (beyaz kafes) duvarın içinde
    gw = 64
    for y in range(wall - 14, wall):
        for x in range(cx - gw // 2, cx + gw // 2 + 1):
            edge = x in (cx - gw // 2, cx + gw // 2) or y == wall - 14
            px[x, y] = WHITE if edge else (MIST if (x + y) % 4 == 0 or (x - y) % 4 == 0 else NIGHT)
    # Alt girinti: oyuncu tüneli
    for y in range(N - wall, N):
        for x in range(int(N * 0.46), int(N * 0.54)):
            px[x, y] = INK if y > N - wall + 3 else NIGHT
    # Köşe kaideleri: hakem platformları (yuvarlak, gri) + köşe bayrağı
    def platform(cx_, cy_, r):
        for y in range(cy_ - r - 2, cy_ + r + 3):
            for x in range(cx_ - r - 2, cx_ + r + 3):
                dx, dy = (x - cx_) / r, (y - cy_) / (r * 0.8)
                dd = dx * dx + dy * dy
                if dd <= 1:
                    px[x, y] = SLATE_L if dd < 0.45 and y < cy_ else SLATE if dd < 0.8 else SLATE_D
                elif dd <= 1.2 and y > cy_: px[x, y] = G_D
    for fx, fy in ((0.137, 0.127), (0.863, 0.127), (0.137, 0.86), (0.863, 0.86)):
        platform(int(fx * N), int(fy * N), 19)
    for (fx, fy) in ((0.095, 0.095), (0.905, 0.095), (0.095, 0.9), (0.905, 0.9)):
        x0, y0 = int(fx * N), int(fy * N)
        for k in range(9): px[x0, y0 - k] = WHITE
        for k in range(4):
            for j in range(3): px[x0 + 1 + k, y0 - 8 + j] = GOLD if (k + j) % 2 else RED
    # Yan süsler: projektör direkleri (meşale yerleri)
    def floodlight(cx_, cy_):
        for k in range(12): px[cx_, cy_ + 6 - k] = SLATE_L
        for dx in range(-3, 4):
            for dy in range(-2, 1): px[cx_ + dx, cy_ - 6 + dy] = GOLD_L if dy == -1 else WHITE
    for fy in (0.33, 0.64):
        floodlight(int(0.052 * N), int(fy * N)); floodlight(int(0.948 * N), int(fy * N))
    big = img.resize((N * 6, N * 6), Image.NEAREST)
    gl = Image.new("L", big.size, 0)
    dr = ImageDraw.Draw(gl)
    for fy in (0.33, 0.64):
        for fx in (0.052, 0.948):
            cx_, cy_ = int(fx * N * 6), int((fy * N - 7) * 6)
            dr.ellipse((cx_ - 90, cy_ - 90, cx_ + 90, cy_ + 90), fill=120)
    gl = gl.filter(ImageFilter.GaussianBlur(60))
    glow = Image.new("RGBA", big.size, GOLD_L[:3] + (0,)); glow.putalpha(gl)
    out = Image.alpha_composite(big, glow)
    path = os.path.join(ROOT, "Assets", "Sprites", "Arena", "Football_arena_01.png")
    out.convert("RGB").save(path)
    return path

def tiles():
    d = os.path.join(ROOT, "Assets", "Art", "Tiles", "Football")
    os.makedirs(d, exist_ok=True)
    rnd = random.Random(5)
    bd = Image.new("RGBA", (160, 80), NIGHT); p = bd.load()
    for y in range(80):
        for x in range(160):
            row = y % 4
            rr = random.Random((x // 3) * 31 + (y // 4) * 977)
            c = SLATE_D if row == 0 else NIGHT
            if rr.random() < 0.6 and x % 3 != 2:
                if row == 1: c = C("ten") if rr.random() < 0.8 else C("kahve")
                elif row == 2: c = rr.choice([RED, CYAN, GOLD, WHITE, PINK])
            p[x, y] = c
    bd.save(os.path.join(d, "backdrop_tile.png"))
    co = Image.new("RGBA", (120, 80), SLATE_D); p = co.load()
    for y in range(80):
        for x in range(120):
            if x % 30 == 0 or y % 20 == 0: p[x, y] = NIGHT
            elif rnd.random() < 0.04: p[x, y] = SLATE
    co.save(os.path.join(d, "corridor_tile.png"))
    cw = Image.new("RGBA", (12, 40), SLATE); p = cw.load()
    for y in range(40):
        for x in range(12):
            p[x, y] = SLATE_L if x < 3 else SLATE if x < 9 else SLATE_D
    cw.save(os.path.join(d, "corridor_wall.png"))
    lg = Image.new("RGBA", (40, 8), NIGHT); p = lg.load()
    for x in range(40):
        for y in range(8):
            p[x, y] = [CYAN, RED, GOLD, PINK][(x // 10) % 4] if 2 <= y <= 5 else SLATE_D
    lg.save(os.path.join(d, "ledge_tile.png"))

# Hakem: Ata'nın karelerinde gömlek → siyah, şort koyu, yaka beyaz kalır (piksel eşleme)
REF_MAP = {"0099db": "262b44", "124e89": "181425", "3a4466": "181425", "262b44": "3a4466"}
DIRS8 = ["South", "South_East", "East", "North_East", "North", "North_West", "West", "South_West"]
def referees():
    def rgb(h): return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16))
    m = {rgb(k): rgb(v) for k, v in REF_MAP.items()}
    src = os.path.join(ROOT, "Assets", "Sprites", "Player")
    for d in DIRS8:
        out = os.path.join(ROOT, "Assets", "Sprites", "Spawner_Football", d)
        os.makedirs(out, exist_ok=True)
        files = [f for f in os.listdir(os.path.join(src, d)) if not f.endswith(".meta")]
        idle = [f for f in files if f.endswith(".png")][0]
        runs = sorted([f for f in files if "_Run_" in f], key=lambda f: int(f.rsplit("_", 1)[1].split(".")[0]))
        for name, f in [("idle", idle)] + [("attack_%d" % (i + 1), r) for i, r in enumerate(runs)]:
            im = Image.open(os.path.join(src, d, f)).convert("RGBA")
            px = im.load()
            for y in range(im.height):
                for x in range(im.width):
                    r, g, b, a = px[x, y]
                    if a and (r, g, b) in m: px[x, y] = m[(r, g, b)] + (a,)
            # düdük: göğüste küçük sarı nokta (hakem okunsun)
            im.save(os.path.join(out, name + ".png"))

def ball():
    img = Image.new("RGBA", (22, 22), (0, 0, 0, 0)); px = img.load()
    c = 10.5
    for y in range(22):
        for x in range(22):
            d = math.hypot(x - c, y - c)
            if d <= 9.5:
                l = (x - c + 3) ** 2 + (y - c + 3) ** 2
                px[x, y] = WHITE if l < 60 else MIST if d < 8 else SLATE_L
    for (ox, oy) in ((0, 0), (-6, -3), (6, -3), (-4, 6), (4, 6)):
        for y in range(-2, 3):
            for x in range(-2, 3):
                if abs(x) + abs(y) <= 2 + (1 if (ox, oy) == (0, 0) else 0):
                    X, Y = int(c + ox + x), int(c + oy + y)
                    if math.hypot(X - c, Y - c) <= 9: px[X, Y] = NIGHT if (ox, oy) != (0, 0) else INK
    return outline(img)

def card(col, col_d):
    img = Image.new("RGBA", (12, 16), (0, 0, 0, 0)); px = img.load()
    for y in range(1, 15):
        for x in range(1, 11):
            px[x, y] = col if x < 9 and y > 2 else col_d if x >= 9 else col
    for x in range(2, 9): px[x, 2] = WHITE
    return outline(img)

def bottle():
    img = Image.new("RGBA", (10, 20), (0, 0, 0, 0)); px = img.load()
    GL, GD = C("acik_yesil"), C("yesil")
    for y in range(7, 19):
        for x in range(2, 8):
            px[x, y] = GL if x < 5 else GD
    for y in range(3, 7):
        for x in range(4, 6): px[x, y] = GL if x == 4 else GD
    for x in range(3, 7): px[x, 2] = C("altin")  # kapak
    for y in range(9, 16): px[3, y] = WHITE         # cam parlaması
    for x in range(2, 8): px[x, 12] = C("tehlike") if x > 2 else WHITE  # etiket
    return outline(img)

def projectiles():
    d = os.path.join(ROOT, "Assets", "Art", "Projectiles")
    ball().save(os.path.join(d, "ball.png"))
    bottle().save(os.path.join(d, "bottle.png"))
    card(GOLD_L, GOLD).save(os.path.join(d, "card_yellow.png"))
    card(RED, RED_D).save(os.path.join(d, "card_red.png"))
    card(GOLD_L, GOLD).save(os.path.join(ROOT, "Assets", "Resources", "StatusYellow.png"))  # baş üstü kart simgesi
    card(RED, RED_D).save(os.path.join(ROOT, "Assets", "Resources", "StatusRed.png"))

def preview():
    src = Image.open(os.path.join(ROOT, "Assets", "Sprites", "Arena", "Football_arena_01.png")).convert("RGBA").resize((320, 320), Image.LANCZOS)
    src.save(os.path.join(ROOT, "Assets", "Sprites", "Arena", "Football_preview.png"))

if __name__ == "__main__":
    what = sys.argv[1] if len(sys.argv) > 1 and not sys.argv[1].startswith("--") else "all"
    if what in ("arena", "all"): arena()
    if what in ("tiles", "all"): tiles()
    if what in ("referees", "all"): referees()
    if what in ("projectiles", "all"): projectiles()
    if what in ("preview", "all"): preview()
    if "--preview" in sys.argv:
        a = Image.open(os.path.join(ROOT, "Assets", "Sprites", "Arena", "Football_arena_01.png")).resize((600, 600), Image.NEAREST)
        sheet = Image.new("RGBA", (600 + 330, 600), (24, 20, 37, 255))
        sheet.paste(a, (0, 0))
        y = 10
        for n, s in (("ball", 5), ("card_yellow", 6), ("card_red", 6)):
            im = Image.open(os.path.join(ROOT, "Assets", "Art", "Projectiles", n + ".png")).convert("RGBA")
            sheet.alpha_composite(im.resize((im.width * s, im.height * s), Image.NEAREST), (620, y)); y += im.height * s + 10
        for i, dname in enumerate(("South", "East")):
            g = Image.open(os.path.join(ROOT, "Assets", "Sprites", "Spawner_Football", dname, "idle.png")).convert("RGBA").resize((144, 144), Image.NEAREST)
            sheet.alpha_composite(g, (770, 10 + i * 150))
        bd = Image.open(os.path.join(ROOT, "Assets", "Art", "Tiles", "Football", "backdrop_tile.png")).convert("RGBA").resize((320, 160), Image.NEAREST)
        sheet.alpha_composite(bd, (610, 420))
        sheet.save(os.path.join(SCRATCH, "football_preview.png"))
