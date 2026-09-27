#!/usr/bin/env python3
"""KaçAtaKaç — Buz dünyası görselleri (Faz 11 G7). Tekrar çalıştırılabilir.
  python3 tools/kak_gen_ice.py [arena|tiles|snowman|projectiles|icons|all] [--preview]
Arena: zindanla aynı geometri (oynanabilir alan %7,95 kenar, 4 köşe kaide, yan duvar süsleri, alt girinti),
341 sanat pikseli × 6 = 2046 px (PPU 100, ölçek 0,4 → piksel = 0,024 dünya birimi, karakterlerle aynı yoğunluk).
Renk (sanat-rehberi): zemin koyu donmuş göl (değer %25-40), duvar kar blokları, tehlike (kartopu) parlak beyaz + koyu kontur.
"""
import math, os, random, sys
from PIL import Image, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, os.path.join(ROOT, "tools"))
from kak_palette import ROLE  # noqa: E402

def C(k, a=255):
    h = ROLE[k]
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)

INK, NIGHT, SLATE_D, SLATE, SLATE_L, MIST, WHITE = C("murekkep"), C("gece"), C("arduvaz_koyu"), C("arduvaz"), C("arduvaz_acik"), C("sis"), C("beyaz")
CYAN_D, CYAN, CYAN_B = C("camgobegi_koyu"), C("camgobegi"), C("camgobegi_parlak")
SCRATCH = "/private/tmp/claude-501/-Users-atakaan-KacAtaKac/d6de2e7c-7c40-4265-8d27-a2463a16f01f/scratchpad"

def outline(img, col=INK):
    w, h = img.size
    px = img.load()
    add = []
    for y in range(h):
        for x in range(w):
            if px[x, y][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    X, Y = x + dx, y + dy
                    if 0 <= X < w and 0 <= Y < h and px[X, Y][3] > 0 and px[X, Y] != col:
                        add.append((x, y)); break
    for p in add: px[p] = col
    return img

# ─────────────────────────────── ARENA ───────────────────────────────
def arena():
    N = 341
    rnd = random.Random(7)
    img = Image.new("RGBA", (N, N), NIGHT)
    px = img.load()
    rim, wall = 10, 27                       # dış kar blokları, iç duvar yüzü (oynanabilir alan 27 px'ten)
    # Zemin: koyu donmuş göl — kabartmalı buz levhaları (derz + açık kenar), çapraz parlama, kar birikintisi
    slab = 29
    tile_seed = {}
    for y in range(wall, N - wall):
        for x in range(wall, N - wall):
            u, v = (x - wall) % slab, (y - wall) % slab
            tx, ty = (x - wall) // slab, (y - wall) // slab
            ts = tile_seed.setdefault((tx, ty), rnd.random())
            c = SLATE_D
            # levha içinde hafif ton farkı (bazı levhalar bir kademe açık)
            if ts > 0.86: c = SLATE
            if u == 0 or v == 0: c = NIGHT
            elif u == 1 or v == 1: c = SLATE if c == SLATE_D else SLATE_L
            elif u == slab - 1 or v == slab - 1: c = NIGHT if c == SLATE_D else SLATE_D
            else:
                # çapraz yansıma: her levhada sol üstte kısa "/" parlama çizgileri
                d = u + v
                if ts < 0.6 and (d == 9 or d == 12) and 3 < u < 12 and 3 < v < 12: c = SLATE_L if d == 9 else SLATE
                if ts >= 0.6 and d == 30 and 8 < u < 22: c = SLATE
            px[x, y] = c
    # ince çatlaklar (az ve kısa)
    for _ in range(14):
        x, y = rnd.randint(wall + 6, N - wall - 6), rnd.randint(wall + 6, N - wall - 6)
        ang = rnd.uniform(0, math.pi * 2)
        for s_ in range(rnd.randint(5, 11)):
            ang += rnd.uniform(-0.5, 0.5)
            x += math.cos(ang); y += math.sin(ang)
            X, Y = int(x), int(y)
            if wall + 2 < X < N - wall - 2 and wall + 2 < Y < N - wall - 2 and px[X, Y] in (SLATE_D, SLATE): px[X, Y] = SLATE_L
    # donmuş kırağı noktaları
    for _ in range(90):
        X, Y = rnd.randint(wall + 2, N - wall - 3), rnd.randint(wall + 2, N - wall - 3)
        if px[X, Y] in (SLATE_D, SLATE): px[X, Y] = MIST if rnd.random() < 0.8 else WHITE
    # kırağı çiçekleri (birkaç levhada işlenmiş kar kristali)
    for _ in range(7):
        tx, ty = rnd.randint(0, 9), rnd.randint(0, 9)
        cx, cy = wall + tx * slab + slab // 2, wall + ty * slab + slab // 2
        for k in range(1, 5):
            for dx, dy in ((k, 0), (-k, 0), (0, k), (0, -k)):
                px[cx + dx, cy + dy] = MIST if k < 4 else SLATE_L
        for k in (1, 2):
            for dx, dy in ((k, k), (-k, k), (k, -k), (-k, -k)):
                px[cx + dx, cy + dy] = SLATE_L
        px[cx, cy] = WHITE
    # duvar diplerinde kar birikintisi (dalgalı kenar)
    for i in range(wall, N - wall):
        for side in range(4):
            depth = int(3 + 2.2 * math.sin(i * 0.19 + side) + 1.6 * math.sin(i * 0.071 + side * 2) + (2 if side == 0 else 0))
            for k in range(max(0, depth)):
                x, y = [(i, wall + k), (wall + k, i), (i, N - wall - 1 - k), (N - wall - 1 - k, i)][side]
                px[x, y] = WHITE if k < depth - 2 else MIST if k < depth - 1 else SLATE_L
    # İç duvar yüzü: kar duvarı, içe doğru gölgelenen
    for y in range(N):
        for x in range(N):
            d = min(x, y, N - 1 - x, N - 1 - y)
            if rim <= d < wall:
                t = (d - rim) / (wall - rim)
                col = MIST if t < 0.35 else SLATE_L if t < 0.7 else SLATE
                # kar blokları: yatay derz
                along = x if (y < wall or y >= N - wall) else y
                if (d - rim) in (5, 11) or along % 16 == (0 if (d - rim) < 5 else 8 if (d - rim) < 11 else 3):
                    col = SLATE_L if col == MIST else SLATE if col == SLATE_L else SLATE_D
                px[x, y] = col
            elif d < rim:
                # dış kar blokları: iri, üstü kar kapaklı, derzleri seyrek
                along = x if (y < rim or y >= N - rim) else y
                k = along % 23
                if d == 0: col = SLATE
                elif d <= 3: col = WHITE                     # kar kapağı
                elif d <= 6: col = MIST
                else: col = SLATE_L
                if k == 0 and d > 2: col = SLATE
                if k in (1, 22) and d > 3: col = SLATE_L
                px[x, y] = col
    # duvar-zemin birleşiminde gölge çizgisi
    for i in range(wall, N - wall):
        for (x, y) in ((i, wall), (wall, i), (i, N - wall - 1), (N - wall - 1, i)):
            px[x, y] = NIGHT
    # alt çıkış girintisi
    for y in range(N - wall, N):
        for x in range(int(N * 0.46), int(N * 0.54)):
            px[x, y] = NIGHT if y > N - wall + 3 else INK
    # Köşe kaideleri: kar yığınları (fırlatıcılar üstünde durur)
    def mound(cx, cy, r):
        for y in range(cy - r - 2, cy + r + 3):
            for x in range(cx - r - 2, cx + r + 3):
                dx, dy = (x - cx) / r, (y - cy) / (r * 0.8)
                d = dx * dx + dy * dy
                if d <= 1:
                    l = (x - cx + r * 0.4) ** 2 + (y - cy + r * 0.5) ** 2
                    col = WHITE if l < (r * 0.55) ** 2 else MIST if d < 0.75 else SLATE_L
                    px[x, y] = col
                elif d <= 1.18 and y > cy:
                    px[x, y] = NIGHT  # zemindeki gölge
    for fx, fy in ((0.137, 0.127), (0.863, 0.127), (0.137, 0.86), (0.863, 0.86)):
        mound(int(fx * N), int(fy * N), 19)
    # Yan duvar süsleri (meşale yerleri): buz kristalleri
    def crystal(cx, cy):
        # üç sivri buz kristali (ortadaki uzun)
        for (ox, hgt, wd) in ((0, 13, 3), (-4, 8, 2), (4, 9, 2)):
            for k in range(hgt):
                half = max(0, int(wd * (1 - k / hgt) + 0.6))
                for dx in range(-half, half + 1):
                    x, y = cx + ox + dx, cy + 5 - k
                    px[x, y] = CYAN_B if dx < 0 else CYAN if dx == 0 else CYAN_D
            px[cx + ox, cy + 5 - hgt + 1] = WHITE
    for fy in (0.33, 0.64):
        crystal(int(0.052 * N), int(fy * N)); crystal(int(0.948 * N), int(fy * N))
    # üst iç duvardan sarkan buz sarkıtları
    for x in range(wall + 6, N - wall - 6, 11):
        L = 3 + (x * 7) % 5
        for k in range(L):
            X, Y = x, wall + k
            px[X, Y] = CYAN if k < L - 1 else CYAN_B
            if k < L - 2: px[X + 1, Y] = CYAN_D
    img = outline_arena(img)
    big = img.resize((N * 6, N * 6), Image.NEAREST)
    # kristallerin soğuk ışıltısı (yumuşak, tek katman)
    from PIL import ImageDraw
    gl = Image.new("L", big.size, 0)
    dr = ImageDraw.Draw(gl)
    for fy in (0.33, 0.64):
        for fx in (0.052, 0.948):
            cx, cy = int(fx * N * 6), int(fy * N * 6)
            dr.ellipse((cx - 110, cy - 110, cx + 110, cy + 110), fill=120)
    gl = gl.filter(ImageFilter.GaussianBlur(70))
    glow = Image.new("RGBA", big.size, CYAN_B[:3] + (0,)); glow.putalpha(gl)
    out = Image.alpha_composite(big, glow)
    path = os.path.join(ROOT, "Assets", "Sprites", "Arena", "Ice_arena_01.png")
    out.convert("RGB").save(path)
    return path

def outline_arena(img):
    return img

# ─────────────────────────────── ÇERÇEVE KAROLARI ───────────────────────────────
ICE_RAMP = [INK, NIGHT, SLATE_D, SLATE, SLATE_L, MIST, WHITE]
def recolor_to_ice(src, dst, lift=0):
    im = Image.open(src).convert("RGBA")
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a == 0: continue
            l = (0.3 * r + 0.59 * g + 0.11 * b) / 255
            i = max(0, min(len(ICE_RAMP) - 1, int(l * 1.25 * (len(ICE_RAMP) - 1) + 0.5) + lift))
            c = ICE_RAMP[i]
            px[x, y] = (c[0], c[1], c[2], a)
    im.save(dst)

def tiles():
    os.makedirs(os.path.join(ROOT, "Assets", "Art", "Tiles", "Ice"), exist_ok=True)
    for n, lift in (("backdrop_tile", 0), ("corridor_tile", 1), ("corridor_wall", 1), ("ledge_tile", 2)):
        recolor_to_ice(os.path.join(ROOT, "Assets", "Art", "Tiles", "Dungeon", n + ".png"),
                       os.path.join(ROOT, "Assets", "Art", "Tiles", "Ice", n + ".png"), lift)

# ─────────────────────────────── KARDAN ADAM ───────────────────────────────
TANGERINE, CARROT_D = C("turuncu"), C("turuncu_koyu")
SCARF, SCARF_D = C("tehlike"), C("tehlike_koyu")
STICK, STICK_D = C("kahve"), C("kahve_koyu")
DIRS8 = ["South", "South_East", "East", "North_East", "North", "North_West", "West", "South_West"]
# yön vektörü (ekran: x sağ, y aşağı): burnun ve kolun gittiği taraf
DVEC = {"South": (0, 1), "South_East": (0.7, 0.7), "East": (1, 0), "North_East": (0.7, -0.7),
        "North": (0, -1), "North_West": (-0.7, -0.7), "West": (-1, 0), "South_West": (-0.7, 0.7)}

def ball(px, cx, cy, r, light=(-0.45, -0.55)):
    for y in range(int(cy - r - 1), int(cy + r + 2)):
        for x in range(int(cx - r - 1), int(cx + r + 2)):
            dx, dy = (x + 0.5 - cx) / r, (y + 0.5 - cy) / r
            d = dx * dx + dy * dy
            if d <= 1:
                l = (dx - light[0]) ** 2 + (dy - light[1]) ** 2
                px[x, y] = WHITE if l < 0.55 else MIST if l < 1.35 else SLATE_L

def line(px, x0, y0, x1, y1, c, c2=None):
    n = int(max(abs(x1 - x0), abs(y1 - y0))) + 1
    for i in range(n + 1):
        t = i / max(1, n)
        x, y = int(round(x0 + (x1 - x0) * t)), int(round(y0 + (y1 - y0) * t))
        px[x, y] = c
        if c2 is not None: px[x, y + 1] = c2

def snowman(direction, frame=-1):
    """frame -1 = bekleme, 0-3 = atış (kol geri → yukarı → ileri → dönüş)"""
    img = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
    px = img.load()
    vx, vy = DVEC[direction]
    back = vy < -0.3
    ball(px, 24, 38, 9)
    ball(px, 24, 26.5, 7)
    ball(px, 24 + vx * 0.8, 17, 5.2)
    # atkı (boyun) + sarkan uç
    for x in range(18, 31):
        px[x, 21] = SCARF; px[x, 22] = SCARF_D
    # atkının ucu: yönün tersine savrulur, 2 piksel genişlikte dolu
    tail_x = 24 - int(round(vx * 5)) if abs(vx) > 0.2 else 27
    for k in range(3):
        px[tail_x, 23 + k] = SCARF
        px[tail_x + 1, 23 + k] = SCARF_D
    # silindir şapka
    hx = 24 + int(round(vx * 0.8))
    for y in range(6, 13):
        for x in range(hx - 4, hx + 5):
            px[x, y] = NIGHT if y < 11 else INK
    for x in range(hx - 6, hx + 7): px[x, 12] = INK
    for x in range(hx - 3, hx + 4): px[x, 10] = SCARF_D  # şapka bandı
    # yüz
    if not back:
        ex = [hx - 2, hx + 2] if abs(vx) < 0.5 else ([hx + 1, hx + 3] if vx > 0 else [hx - 3, hx - 1])
        if abs(vx) > 0.9: ex = [hx + 2] if vx > 0 else [hx - 2]
        for e in ex: px[e, 16] = INK
        # havuç burun yöne doğru
        nx, ny = hx + int(round(vx * 1.5)), 18
        L = 1 if abs(vx) < 0.3 else 3
        for k in range(L + 1):
            px[nx + int(round(vx * k)), ny + (1 if vy > 0.5 and k == L else 0)] = TANGERINE if k < L else CARROT_D
        if abs(vx) < 0.3: px[hx, 18] = TANGERINE; px[hx, 19] = CARROT_D
        # kömür düğmeler
        if vy > -0.3:
            for yy in (25, 28): px[24 + int(vx * 2), yy] = INK
    # kollar (dal): bekleme ya da atış
    side = 1 if vx >= 0 else -1
    other = -side
    # diğer kol hep aşağıda
    line(px, 24 + other * 6, 26, 24 + other * 12, 31, STICK)
    px[24 + other * 12, 32] = STICK_D
    if frame < 0:
        line(px, 24 + side * 6, 26, 24 + side * 12, 30, STICK)
        px[24 + side * 13, 29] = STICK_D
    else:
        # atış kolu: 0 geri-aşağı (kartopu), 1 yukarı (kartopu), 2 ileri (bırakır), 3 dönüş
        poses = [(side * 11, 32), (side * 8, 12), (int(vx * 12) or side * 12, 20 + int(vy * 6)), (side * 12, 28)]
        tx, ty = poses[frame]
        line(px, 24 + side * 6, 25, 24 + tx, ty, STICK)
        if frame in (0, 1):  # elde kartopu
            ball(px, 24 + tx + side, ty - 2, 2.6)
    return outline(img)

def snowmen():
    base = os.path.join(ROOT, "Assets", "Sprites", "Spawner_Ice")
    for d in DIRS8:
        os.makedirs(os.path.join(base, d), exist_ok=True)
        snowman(d).save(os.path.join(base, d, "idle.png"))
        for f in range(4):
            snowman(d, f).save(os.path.join(base, d, "attack_%d.png" % (f + 1)))

def preview_snowmen():
    sheet = Image.new("RGBA", (8 * 52, 5 * 52), (58, 68, 102, 255))
    for i, d in enumerate(DIRS8):
        for j, f in enumerate([-1, 0, 1, 2, 3]):
            sheet.alpha_composite(snowman(d, f), (i * 52 + 2, j * 52 + 2))
    sheet.resize((sheet.width * 3, sheet.height * 3), Image.NEAREST).save(os.path.join(SCRATCH, "snowmen.png"))

# ─────────────────────────────── MERMİLER ───────────────────────────────
def snowball_sprite():
    # taşla (rock.png 28 px) aynı ayak izi: çarpışma alanı görselle örtüşsün
    img = Image.new("RGBA", (28, 28), (0, 0, 0, 0))
    px = img.load()
    ball(px, 14, 14, 11.5)
    for (x, y, c) in ((10, 8, WHITE), (17, 11, MIST), (7, 15, MIST), (18, 19, SLATE_L), (13, 21, SLATE_L), (20, 15, SLATE_L)):
        px[x, y] = c
    return outline(img)

def icicle_sprite():
    img = Image.new("RGBA", (14, 30), (0, 0, 0, 0))
    px = img.load()
    for k in range(26):
        half = max(0, int(5 * (1 - k / 26) + 0.5))
        for dx in range(-half, half + 1):
            x, y = 7 + dx, 2 + k
            px[x, y] = WHITE if dx <= -half + 1 and k < 18 else CYAN_B if dx < 0 else CYAN if dx < 2 else CYAN_D
    return outline(img)

def projectiles():
    d = os.path.join(ROOT, "Assets", "Art", "Projectiles")
    snowball_sprite().save(os.path.join(d, "snowball.png"))
    icicle_sprite().save(os.path.join(d, "icicle.png"))

# ─────────────────────────────── EŞYA İKONLARI (256, piksel ×8 + ışıma) ───────────────────────────────
def glow_icon(art32, glow_rgb, path):
    big = art32.resize((224, 224), Image.NEAREST)
    canvas = Image.new("RGBA", (256, 256), (0, 0, 0, 0)); canvas.alpha_composite(big, (16, 16))
    m = canvas.split()[3]
    ga = m.filter(ImageFilter.MaxFilter(11)).filter(ImageFilter.GaussianBlur(7)).point(lambda v: min(255, int(v * 1.1)))
    glow = Image.new("RGBA", canvas.size, glow_rgb + (0,)); glow.putalpha(ga)
    edge = Image.new("RGBA", canvas.size, glow_rgb + (0,)); edge.putalpha(m.filter(ImageFilter.MaxFilter(5)).point(lambda v: 170 if v > 0 else 0))
    Image.alpha_composite(Image.alpha_composite(glow, edge), canvas).save(path)

def mitten():
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0)); px = img.load()
    # eldiven gövdesi (camgöbeği yün) + başparmak + beyaz manşet
    for y in range(6, 24):
        for x in range(9, 23):
            dx, dy = (x - 16) / 7, (y - 13) / 9
            if dx * dx + dy * dy <= 1: px[x, y] = CYAN if x < 19 else CYAN_D
    for y in range(12, 20):
        for x in range(5, 11):
            dx, dy = (x - 8) / 3, (y - 16) / 4
            if dx * dx + dy * dy <= 1: px[x, y] = CYAN if x > 6 else CYAN_B
    for y in range(22, 28):
        for x in range(9, 24):
            px[x, y] = WHITE if (x + y) % 3 else MIST
    for x in range(11, 21, 3): px[x, 10] = CYAN_B; px[x + 1, 11] = CYAN_B  # örgü
    ball(px, 24, 8, 4)  # tuttuğu kartopu
    return outline(img)

def ice_boots():
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0)); px = img.load()
    LEATHER, LEATHER_D = C("bakir"), C("kahve")
    for y in range(5, 22):
        for x in range(10, 19): px[x, y] = LEATHER if x < 16 else LEATHER_D
    for y in range(17, 25):
        for x in range(10, 27): px[x, y] = LEATHER if y < 21 else LEATHER_D
    for x in range(9, 20): px[x, 5] = WHITE; px[x, 6] = MIST   # kürk
    for x in range(10, 27): px[x, 25] = SLATE_L
    for x in range(11, 27, 3): px[x, 26] = MIST; px[x, 27] = WHITE  # çiviler
    for y in (9, 12, 15): px[12, y] = MIST; px[16, y] = MIST        # bağcık
    return outline(img)

def fire():
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0)); px = img.load()
    RED, OR, YE, WH = C("tehlike"), C("turuncu"), C("altin"), C("altin_acik")
    for y in range(4, 28):
        for x in range(6, 27):
            t = (y - 4) / 24
            w = 3 + t * 8 if t < 0.75 else 9 - (t - 0.75) * 20
            flick = math.sin(y * 0.9) * 1.5 * (1 - t)
            dx = x - 16 - flick
            if abs(dx) <= w:
                r = abs(dx) / max(1, w)
                px[x, y] = WH if (r < 0.3 and t > 0.45) else YE if r < 0.55 and t > 0.3 else OR if r < 0.85 else RED
    # odun
    for x in range(8, 25):
        px[x, 27] = C("kahve"); px[x, 28] = C("kahve_koyu")
    return outline(img)

def snowflake_ui():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0)); px = img.load()
    for k in range(-6, 7):
        px[8 + k, 8] = WHITE; px[8, 8 + k] = WHITE
    for k in range(-4, 5):
        px[8 + k, 8 + k] = MIST; px[8 + k, 8 - k] = MIST
    for (x, y) in ((6, 3), (10, 3), (6, 13), (10, 13), (3, 6), (3, 10), (13, 6), (13, 10)): px[x, y] = MIST
    return img.resize((96, 96), Image.NEAREST)

def icons():
    d = os.path.join(ROOT, "Assets", "Sprites", "Powerups")
    glow_icon(mitten(), CYAN_B[:3], os.path.join(d, "GloveIcon.png"))
    glow_icon(ice_boots(), CYAN_B[:3], os.path.join(d, "IceBootsIcon.png"))
    glow_icon(fire(), C("altin")[:3], os.path.join(d, "FireIcon.png"))
    snowflake_ui().save(os.path.join(ROOT, "Assets", "Art", "UI", "snowflake_ui.png"))

def preview_misc():
    sheet = Image.new("RGBA", (256 * 3 + 20, 256 + 120), (58, 68, 102, 255))
    for i, n in enumerate(("GloveIcon", "IceBootsIcon", "FireIcon")):
        sheet.alpha_composite(Image.open(os.path.join(ROOT, "Assets", "Sprites", "Powerups", n + ".png")).convert("RGBA"), (i * 262, 0))
    sheet.alpha_composite(snowball_sprite().resize((88, 88), Image.NEAREST), (20, 270))
    sheet.alpha_composite(icicle_sprite().resize((56, 120), Image.NEAREST), (160, 258))
    sheet.alpha_composite(snowflake_ui(), (280, 270))
    sheet.save(os.path.join(SCRATCH, "ice_misc.png"))

if __name__ == "__main__":
    what = sys.argv[1] if len(sys.argv) > 1 else "all"
    if what in ("arena", "all"):
        p = arena()
        if "--preview" in sys.argv:
            Image.open(p).resize((512, 512), Image.NEAREST).save(os.path.join(SCRATCH, "ice_arena.png"))
    if what in ("tiles", "all"):
        tiles()
    if what in ("projectiles", "all"):
        projectiles()
    if what in ("icons", "all"):
        icons()
        if "--preview" in sys.argv: preview_misc()
    if what in ("snowman", "all"):
        snowmen()
        if "--preview" in sys.argv: preview_snowmen()
    print("tamam:", what)
