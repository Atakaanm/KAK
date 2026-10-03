"""Faz 13 K2: Karanlık Mağara sanatı (tek palet: Endesga 32, tools/kak_palette.py).
  arena        → Assets/Sprites/Arena/Cave_arena_01.png (341 px × 6; zindan/buz geometrisi: oynanabilir alan 27 px'ten,
                  köşe kaideleri, alt girinti, yan duvar süs yerleri 0,33/0,64)
  tiles        → Assets/Art/Tiles/Cave/ (zindan karolarının mağara rampasına boyanmışı)
  guardians    → Assets/Sprites/Spawner_Cave/<Yön>/idle.png, attack_1..6.png (Taş Muhafız, mor-koyu taş)
  projectiles  → Assets/Art/Projectiles/bat.png, stalactite.png, spore.png, web.png
  preview      → Assets/Sprites/Arena/Cave_preview.png (DÜNYALAR kartı: karanlık + meşale çemberi)
Kullanım: python3 tools/kak_gen_cave.py [all|arena|tiles|guardians|projectiles|preview] [--preview]
"""
import math, os, random, sys
from PIL import Image, ImageDraw, ImageFilter
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import C, outline, ROOT, SCRATCH  # noqa: E402

INK, NIGHT, SLATE_D, SLATE, SLATE_L, MIST = C("murekkep"), C("gece"), C("arduvaz_koyu"), C("arduvaz"), C("arduvaz_acik"), C("sis")
BROWN_D, BROWN, COPPER = C("kahve_koyu"), C("kahve"), C("bakir")
PURPLE, PINK = C("mor"), C("pembe")
FOREST, GREEN = C("orman"), C("yesil")
CYAN_D, CYAN, CYAN_B = C("camgobegi_koyu"), C("camgobegi"), C("camgobegi_parlak")
WHITE = C("beyaz")

def arena():
    N = 341
    rnd = random.Random(13)
    img = Image.new("RGBA", (N, N), INK)
    px = img.load()
    rim, wall = 10, 27
    # Zemin: küçük düzensiz taş levhalar (Voronoi), kabartmalı kenar (sol-üst açık, sağ-alt koyu), yakın tonlar
    import numpy as np
    seeds = np.array([(rnd.uniform(wall - 10, N - wall + 10), rnd.uniform(wall - 10, N - wall + 10)) for _ in range(230)])
    tones = [rnd.random() for _ in range(len(seeds))]
    ys, xs = np.mgrid[wall:N - wall, wall:N - wall]
    P = np.stack([xs.ravel(), ys.ravel()], 1).astype(float)
    best = np.full(len(P), 1e9); second = np.full(len(P), 1e9); idx = np.zeros(len(P), int)
    for i, sd in enumerate(seeds):
        d = ((P - sd) ** 2).sum(1)
        m1 = d < best
        second = np.where(m1, best, np.minimum(second, d))
        idx = np.where(m1, i, idx)
        best = np.where(m1, d, best)
    edge = np.sqrt(second) - np.sqrt(best)
    for k in range(len(P)):
        x, y = int(P[k, 0]), int(P[k, 1])
        t = tones[idx[k]]
        base = SLATE_D if t < 0.55 else NIGHT if t < 0.85 else BROWN_D
        e = edge[k]
        sx, sy = seeds[idx[k]]
        if e < 1.0: c = INK
        elif e < 2.0:
            # kenar: ışık sol-üstten → hücrenin sol-üst kenarı açık, sağ-alt kenarı koyu
            c = SLATE if (x - sx) + (y - sy) < 0 else NIGHT if base != NIGHT else INK
        else: c = base
        px[x, y] = c
    # çakıllar ve çatlaklar
    for _ in range(160):
        X, Y = rnd.randint(wall + 2, N - wall - 3), rnd.randint(wall + 2, N - wall - 3)
        if px[X, Y] in (SLATE_D, BROWN_D, SLATE):
            px[X, Y] = SLATE_L if rnd.random() < 0.6 else BROWN
            if rnd.random() < 0.4: px[X + 1, Y] = SLATE
    for _ in range(18):
        x, y = rnd.randint(wall + 8, N - wall - 8), rnd.randint(wall + 8, N - wall - 8)
        ang = rnd.uniform(0, math.pi * 2)
        for _s in range(rnd.randint(6, 13)):
            ang += rnd.uniform(-0.6, 0.6)
            x += math.cos(ang); y += math.sin(ang)
            X, Y = int(x), int(y)
            if wall + 2 < X < N - wall - 2 and wall + 2 < Y < N - wall - 2: px[X, Y] = INK
    # yosun öbekleri (duvar diplerine yakın) ve su birikintileri
    for _ in range(9):
        cx, cy = rnd.randint(wall + 10, N - wall - 10), rnd.randint(wall + 10, N - wall - 10)
        if abs(cx - N / 2) < 80 and abs(cy - N / 2) < 80: continue
        r = rnd.randint(5, 10)
        for y in range(cy - r, cy + r):
            for x in range(cx - r, cx + r):
                if (x - cx) ** 2 + ((y - cy) * 1.4) ** 2 < r * r and rnd.random() < 0.7:
                    px[x, y] = FOREST if rnd.random() < 0.75 else GREEN
    for _ in range(5):
        cx, cy = rnd.randint(wall + 16, N - wall - 16), rnd.randint(wall + 16, N - wall - 16)
        rx, ry = rnd.randint(7, 13), rnd.randint(4, 7)
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                if ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2 <= 1:
                    px[x, y] = NIGHT if ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2 > 0.55 else CYAN_D
        px[cx - rx // 3, cy - 1] = SLATE_L; px[cx - rx // 3 + 1, cy - 1] = SLATE_L
    # İç duvar yüzü: kaba kaya, içe doğru koyulaşan; dış kenar kayalar
    for y in range(N):
        for x in range(N):
            d = min(x, y, N - 1 - x, N - 1 - y)
            along = x if (y < wall or y >= N - wall) else y
            n = math.sin(along * 0.37) + math.sin(along * 0.13 + d) * 0.7
            if rim <= d < wall:
                t = (d - rim) / (wall - rim)
                col = SLATE if t < 0.3 else SLATE_D if t < 0.75 else NIGHT
                if n > 1.1 and t < 0.6: col = SLATE_L
                if (along + d * 3) % 19 == 0: col = INK
                px[x, y] = col
            elif d < rim:
                k = (along + int(n * 3)) % 17
                col = NIGHT if d == 0 else SLATE_D if d < 4 else SLATE
                if k == 0: col = INK
                if k in (1, 2) and d > 2: col = SLATE_L
                px[x, y] = col
    for i in range(wall, N - wall):
        for (x, y) in ((i, wall), (wall, i), (i, N - wall - 1), (N - wall - 1, i)):
            px[x, y] = INK
    # alt çıkış girintisi
    for y in range(N - wall, N):
        for x in range(int(N * 0.46), int(N * 0.54)):
            px[x, y] = INK
    # üst duvardan sarkıtlar (kahverengi kaya)
    for x in range(wall + 5, N - wall - 5, 9):
        L = 4 + (x * 7) % 6
        for k in range(L):
            half = 1 if k < L - 2 else 0
            for dx in range(-half, half + 1):
                px[x + dx, wall + k] = BROWN if dx < 0 else BROWN_D
            if k == L - 1: px[x, wall + k] = COPPER
    # Köşe kaideleri: iri kaya blokları (fırlatıcılar üstünde durur)
    def boulder(cx, cy, r):
        for y in range(cy - r - 2, cy + r + 3):
            for x in range(cx - r - 2, cx + r + 3):
                dx, dy = (x - cx) / r, (y - cy) / (r * 0.8)
                wob = 1 + 0.08 * math.sin(math.atan2(dy, dx) * 5)
                d = (dx * dx + dy * dy) / (wob * wob)
                if d <= 1:
                    l = (x - cx + r * 0.4) ** 2 + (y - cy + r * 0.5) ** 2
                    px[x, y] = SLATE_L if l < (r * 0.45) ** 2 else SLATE if d < 0.7 else SLATE_D
                elif d <= 1.2 and y > cy:
                    px[x, y] = INK
    for fx, fy in ((0.137, 0.127), (0.863, 0.127), (0.137, 0.86), (0.863, 0.86)):
        boulder(int(fx * N), int(fy * N), 19)
    # Yan duvar süsleri: mor kristal kümeleri (mağaranın kendi ışığı)
    def crystal(cx, cy):
        for (ox, hgt, wd) in ((0, 13, 3), (-4, 8, 2), (4, 9, 2)):
            for k in range(hgt):
                half = max(0, int(wd * (1 - k / hgt) + 0.6))
                for dx in range(-half, half + 1):
                    px[cx + ox + dx, cy + 5 - k] = PINK if dx < 0 else PURPLE if dx == 0 else NIGHT
            px[cx + ox, cy + 5 - hgt + 1] = WHITE
    for fy in (0.33, 0.64):
        crystal(int(0.052 * N), int(fy * N)); crystal(int(0.948 * N), int(fy * N))
    # parlayan mantarlar (duvar diplerinde)
    for (fx, fy) in ((0.2, 0.09), (0.7, 0.09), (0.09, 0.47), (0.91, 0.8), (0.4, 0.92), (0.62, 0.92)):
        cx, cy = int(fx * N), int(fy * N)
        for k in range(3):
            px[cx, cy + k] = MIST
        for dx in range(-2, 3):
            px[cx + dx, cy - 1] = CYAN if abs(dx) < 2 else CYAN_D
        px[cx, cy - 2] = CYAN_B
    big = img.resize((N * 6, N * 6), Image.NEAREST)
    gl = Image.new("L", big.size, 0)
    dr = ImageDraw.Draw(gl)
    for fy in (0.33, 0.64):
        for fx in (0.052, 0.948):
            cx, cy = int(fx * N * 6), int(fy * N * 6)
            dr.ellipse((cx - 100, cy - 100, cx + 100, cy + 100), fill=110)
    gl = gl.filter(ImageFilter.GaussianBlur(65))
    glow = Image.new("RGBA", big.size, PINK[:3] + (0,)); glow.putalpha(gl)
    out = Image.alpha_composite(big, glow)
    path = os.path.join(ROOT, "Assets", "Sprites", "Arena", "Cave_arena_01.png")
    out.convert("RGB").save(path)
    return path

CAVE_RAMP = [INK, NIGHT, SLATE_D, BROWN_D, SLATE, SLATE_L]
def recolor(src, dst, ramp, lift=0, keep_alpha=True):
    im = Image.open(src).convert("RGBA")
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a == 0: continue
            l = (0.3 * r + 0.59 * g + 0.11 * b) / 255
            i = max(0, min(len(ramp) - 1, int(l * 1.2 * (len(ramp) - 1) + 0.5) + lift))
            c = ramp[i]
            px[x, y] = (c[0], c[1], c[2], a)
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    im.save(dst)

def tiles():
    for n, lift in (("backdrop_tile", -1), ("corridor_tile", 0), ("corridor_wall", 0), ("ledge_tile", 1)):
        recolor(os.path.join(ROOT, "Assets", "Art", "Tiles", "Dungeon", n + ".png"),
                os.path.join(ROOT, "Assets", "Art", "Tiles", "Cave", n + ".png"), CAVE_RAMP, lift)

# Taş Muhafız (mağara): zindan fırlatıcısının koyu mor taşa boyanmışı. Gözleri ShooterEyes parlatır.
GUARD_RAMP = [INK, NIGHT, PURPLE, SLATE_D, SLATE, SLATE_L]
DIRS8 = ["South", "South_East", "East", "North_East", "North", "North_West", "West", "South_West"]
def guardians():
    for d in DIRS8:
        src = os.path.join(ROOT, "Assets", "Sprites", "Spawner", d)
        dst = os.path.join(ROOT, "Assets", "Sprites", "Spawner_Cave", d)
        files = [f for f in os.listdir(src) if not f.endswith(".meta")]
        idle = [f for f in files if f.endswith(".png")][0]          # adlar tutarsız: south.png, south-east.png, "west 1.png"
        frames = sorted([f for f in files if f.endswith(".gif")], key=lambda f: int(f.rsplit("_", 1)[1].split(".")[0]))
        recolor(os.path.join(src, idle), os.path.join(dst, "idle.png"), GUARD_RAMP)
        for i, f in enumerate(frames, 1):
            recolor(os.path.join(src, f), os.path.join(dst, "attack_%d.png" % i), GUARD_RAMP)

def bat():
    img = Image.new("RGBA", (24, 14), (0, 0, 0, 0)); px = img.load()
    dr = ImageDraw.Draw(img)
    # kanat zarları: tepesi kemikli, alt kenarı tırtıklı (sağ ve sol ayna)
    left = [(11, 5), (8, 2), (4, 1), (1, 3), (0, 7), (2, 6), (4, 9), (6, 7), (8, 10), (10, 8)]
    right = [(23 - x, y) for (x, y) in left]
    dr.polygon(left, fill=PURPLE); dr.polygon(right, fill=PURPLE)
    for (x, y) in ((8, 2), (4, 1), (1, 3)): px[x, y] = PINK          # kemik uçları
    for (x, y) in ((15, 2), (19, 1), (22, 3)): px[x, y] = PINK
    for k in range(3, 9): px[5, k] = NIGHT; px[18, k] = NIGHT         # zar çizgileri
    # gövde + kulaklar
    for y in range(4, 11):
        for x in range(10, 14):
            px[x, y] = PURPLE if x < 12 else NIGHT
    px[10, 3] = PURPLE; px[13, 3] = NIGHT; px[10, 2] = PURPLE; px[13, 2] = NIGHT
    px[11, 6] = C("tehlike_parlak"); px[12, 6] = C("tehlike_parlak")  # parlayan gözler
    px[11, 9] = MIST; px[12, 9] = MIST                                 # dişler
    return outline(img)

def stalactite():
    img = Image.new("RGBA", (12, 26), (0, 0, 0, 0)); px = img.load()
    for k in range(24):
        half = max(0, int(4.5 * (1 - k / 24) + 0.5))
        for dx in range(-half, half + 1):
            px[6 + dx, 1 + k] = SLATE_L if dx < -1 else SLATE if dx < 1 else BROWN_D
    for k in (5, 11, 17): px[6, k] = BROWN
    return outline(img)

def spore():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0)); px = img.load()
    for y in range(16):
        for x in range(16):
            d = math.hypot(x - 7.5, y - 7.5)
            if d <= 6.5:
                px[x, y] = CYAN_B if d < 2.5 else C("acik_yesil") if d < 4.5 else GREEN
    for (x, y) in ((4, 4), (11, 5), (5, 11), (10, 10)): px[x, y] = WHITE
    return outline(img, FOREST)

def web():
    img = Image.new("RGBA", (28, 28), (0, 0, 0, 0)); px = img.load()
    c = 13.5
    for a in range(8):
        ang = a * math.pi / 4
        for r in range(1, 14):
            x, y = int(c + math.cos(ang) * r), int(c + math.sin(ang) * r)
            px[x, y] = MIST
    for R in (4, 8, 12):
        for t in range(0, 360, 4):
            ang = math.radians(t)
            wob = R - 0.8 * abs(math.sin(ang * 4))
            x, y = int(c + math.cos(ang) * wob), int(c + math.sin(ang) * wob)
            if px[x, y][3] == 0: px[x, y] = SLATE_L if R == 12 else MIST
    px[13, 13] = WHITE; px[14, 14] = WHITE
    return img

def projectiles():
    d = os.path.join(ROOT, "Assets", "Art", "Projectiles")
    bat().save(os.path.join(d, "bat.png"))
    stalactite().save(os.path.join(d, "stalactite.png"))
    spore().save(os.path.join(d, "spore.png"))
    web().save(os.path.join(d, "web.png"))
    web().save(os.path.join(ROOT, "Assets", "Resources", "StatusSlow.png"))  # oyuncu başının üstünde "yapıştın" simgesi

def preview():
    src = Image.open(os.path.join(ROOT, "Assets", "Sprites", "Arena", "Cave_arena_01.png")).convert("RGBA").resize((320, 320), Image.LANCZOS)
    dark = Image.new("RGBA", src.size, (5, 5, 10, 235))
    m = Image.new("L", src.size, 0)
    ImageDraw.Draw(m).ellipse((100, 110, 220, 230), fill=255)
    m = m.filter(ImageFilter.GaussianBlur(14))
    dark.putalpha(Image.eval(m, lambda v: int(235 * (1 - v / 255))))
    out = Image.alpha_composite(src, dark)
    tor = Image.open(os.path.join(ROOT, "Assets", "Resources", "Torch.png")).convert("RGBA").resize((16, 36), Image.NEAREST)
    out.alpha_composite(tor, (164, 150))
    out.save(os.path.join(ROOT, "Assets", "Sprites", "Arena", "Cave_preview.png"))

if __name__ == "__main__":
    what = sys.argv[1] if len(sys.argv) > 1 and not sys.argv[1].startswith("--") else "all"
    if what in ("arena", "all"): arena()
    if what in ("tiles", "all"): tiles()
    if what in ("guardians", "all"): guardians()
    if what in ("projectiles", "all"): projectiles()
    if what in ("preview", "all"): preview()
    if "--preview" in sys.argv:
        a = Image.open(os.path.join(ROOT, "Assets", "Sprites", "Arena", "Cave_arena_01.png")).resize((600, 600), Image.NEAREST)
        sheet = Image.new("RGBA", (600 + 330, 600), (24, 20, 37, 255))
        sheet.paste(a, (0, 0))
        y = 10
        for n, s in (("bat", 5), ("stalactite", 4), ("spore", 5), ("web", 4)):
            im = Image.open(os.path.join(ROOT, "Assets", "Art", "Projectiles", n + ".png")).convert("RGBA")
            sheet.alpha_composite(im.resize((im.width * s, im.height * s), Image.NEAREST), (620, y)); y += im.height * s + 10
        g = Image.open(os.path.join(ROOT, "Assets", "Sprites", "Spawner_Cave", "South", "idle.png")).convert("RGBA").resize((144, 144), Image.NEAREST)
        sheet.alpha_composite(g, (780, 10))
        sheet.alpha_composite(Image.open(os.path.join(ROOT, "Assets", "Sprites", "Arena", "Cave_preview.png")).convert("RGBA").resize((150, 150)), (770, 200))
        sheet.save(os.path.join(SCRATCH, "cave_preview.png"))
