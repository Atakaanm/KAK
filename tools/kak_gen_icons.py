"""Faz 14 Ö4: tek stilde ikonlar (görsel tutarlılık).
Eşyalar (32 px sanat → 256 px, aynı kontur, aynı ışık yönü sol-üst, renk rolüne göre tek hâle; glow_icon hattı):
  Assets/Sprites/Powerups/HeartIcon.png, ShieldIcon.png, TimeIcon.png, GhostIcon.png, ShackleIcon.png
Arayüz (16 px beyaz şekil → 128 px; renk koddan): Assets/Art/UI/icon_play.png, icon_character.png, icon_settings.png,
  icon_pause.png, icon_lock.png, icon_check.png
Kullanım: python3 tools/kak_gen_icons.py [--preview]
"""
import math, os, sys
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import C, outline, glow_icon, ROOT, SCRATCH  # noqa: E402

INK, NIGHT, SLATE_D, SLATE, SLATE_L, MIST, WHITE = C("murekkep"), C("gece"), C("arduvaz_koyu"), C("arduvaz"), C("arduvaz_acik"), C("sis"), C("beyaz")
RED_D, RED, RED_B, PINK = C("tehlike_koyu"), C("tehlike"), C("tehlike_parlak"), C("pembe")
CY_D, CY, CY_B = C("camgobegi_koyu"), C("camgobegi"), C("camgobegi_parlak")
GOLD, GOLD_L, BROWN = C("altin"), C("altin_acik"), C("kahve")

def blank(n=32): return Image.new("RGBA", (n, n), (0, 0, 0, 0))

def shade(px, x, y, cx, cy, r, light, mid, dark):
    """Sol-üstten ışık: merkeze göre konumla üç ton."""
    # küçük parlak bölge (sol-üst), geniş ana renk, sağ-alt kenarda koyu şerit (iki tonlu "bayrak" görünmesin)
    d = (x - cx) + (y - cy)
    px[x, y] = light if d < -r * 1.0 else mid if d < r * 0.85 else dark

def heart():
    img = blank(); px = img.load()
    for y in range(32):
        for x in range(32):
            X, Y = (x - 15.5) / 12.0, (y - 14.0) / 12.0
            if (X * X + Y * Y - 1) ** 3 - X * X * (-Y) ** 3 <= 0: shade(px, x, y, 15.5, 15, 12, PINK, RED, RED_D)
    for (x, y) in ((9, 9), (10, 9), (9, 10), (11, 8)): px[x, y] = WHITE
    return outline(img)

def shield():
    img = blank(); px = img.load()
    for y in range(4, 29):
        t = (y - 4) / 24.0
        half = 11 if t < 0.5 else int(11 * math.cos((t - 0.5) * math.pi) + 0.5)
        for x in range(16 - half, 16 + half):
            shade(px, x, y, 16, 14, 11, CY_B, CY, CY_D)
            if x in (16 - half, 16 + half - 1) or y == 4: px[x, y] = GOLD if y < 20 else GOLD
    for y in range(9, 22): px[16, y] = WHITE if y < 14 else MIST          # haç amblemi
    for x in range(12, 21): px[x, 13] = WHITE
    for (x, y) in ((8, 7), (9, 7), (8, 8)): px[x, y] = WHITE
    return outline(img)

def hourglass():
    img = blank(); px = img.load()
    for x in range(7, 25):
        for y in (4, 5, 26, 27): px[x, y] = GOLD if y in (4, 26) else BROWN
    for y in range(6, 26):
        t = abs(y - 16) / 10.0
        half = max(1, int(1 + 7 * t))
        for x in range(16 - half, 16 + half):
            edge = x in (16 - half, 16 + half - 1)
            px[x, y] = MIST if edge else NIGHT
        if y > 18:                                  # alttaki kum
            for x in range(16 - half + 1, 16 + half - 1): px[x, y] = CY if y > 20 else CY_B
        if 8 <= y <= 11:                            # üstteki kum
            for x in range(16 - half + 1, 16 + half - 1): px[x, y] = CY_B if y < 10 else CY
    for y in range(12, 19): px[16, y] = CY_B        # akan kum
    for y in range(6, 26): px[7, y] = BROWN; px[24, y] = BROWN
    return outline(img)

def ghost():
    img = blank(); px = img.load()
    for y in range(5, 28):
        for x in range(6, 26):
            dx = (x - 15.5) / 9.5
            top = 5 + 9 * (1 - math.sqrt(max(0, 1 - dx * dx)))
            wave = 25 + 2 * math.sin(x * 1.1)
            if top <= y <= wave: shade(px, x, y, 15.5, 14, 10, WHITE, MIST, SLATE_L)
    for (x, y) in ((12, 13), (12, 14), (19, 13), (19, 14)): px[x, y] = NIGHT   # gözler
    for x in range(14, 18): px[x, 18] = SLATE                                  # ağız
    return outline(img)

def shackle():
    img = blank(); px = img.load()
    for y in range(32):                               # gülle
        for x in range(32):
            d = math.hypot(x - 20, y - 20)
            if d <= 9: shade(px, x, y, 20, 20, 9, SLATE_L, SLATE, SLATE_D)
    for (x, y) in ((16, 15), (17, 15), (16, 16)): px[x, y] = MIST
    for k in range(4):                                # zincir halkaları
        cx, cy = 7 + k * 3, 6 + k * 3
        for (dx, dy) in ((0, 0), (1, 0), (0, 1), (1, 1)):
            if (dx, dy) != (1, 1) or k % 2: px[cx + dx, cy + dy] = SLATE_L if (dx + dy) == 0 else SLATE
    for y in range(2, 8):                             # kelepçe
        for x in range(2, 9):
            if 2 <= math.hypot(x - 5, y - 5) <= 3.4: px[x, y] = SLATE_L
    return outline(img)

def ui_icon(fn, n=16):
    img = Image.new("RGBA", (n, n), (0, 0, 0, 0)); px = img.load()
    for y in range(n):
        for x in range(n):
            if fn(x, y): px[x, y] = (255, 255, 255, 255)
    return img.resize((n * 8, n * 8), Image.NEAREST)

def i_play(x, y): return 4 <= x <= 12 and abs(y - 7.5) <= (12 - x) * 0.75 + 0.5
def i_pause(x, y): return 3 <= y <= 12 and (4 <= x <= 6 or 9 <= x <= 11)
def i_character(x, y):
    head = (x - 7.5) ** 2 + (y - 4.5) ** 2 <= 9
    body = 8 <= y <= 14 and abs(x - 7.5) <= 2 + (y - 8) * 0.6
    return head or body
def i_settings(x, y):
    dx, dy = x - 7.5, y - 7.5
    r = math.hypot(dx, dy)
    ang = math.atan2(dy, dx)
    teeth = r <= 7 and (math.cos(ang * 8) > 0.3 or r <= 5.2)
    return teeth and r >= 2.2
def i_lock(x, y):
    shackle_ = 2 <= y <= 7 and 3.0 <= math.hypot(x - 7.5, y - 7) <= 4.4 and y <= 7
    body = 7 <= y <= 14 and 3 <= x <= 12 and not (x in (7, 8) and 9 <= y <= 11)
    return shackle_ or body
def i_check(x, y):
    return (abs((y - 8) - (x - 2)) <= 1 and 2 <= x <= 6) or (abs((y - 11) + (x - 6) * 0.75) <= 1.1 and 6 <= x <= 14)

if __name__ == "__main__":
    pd = os.path.join(ROOT, "Assets", "Sprites", "Powerups")
    glow_icon(heart(), PINK[:3], os.path.join(pd, "HeartIcon.png"))
    glow_icon(shield(), CY_B[:3], os.path.join(pd, "ShieldIcon.png"))
    glow_icon(hourglass(), CY[:3], os.path.join(pd, "TimeIcon.png"))
    glow_icon(ghost(), MIST[:3], os.path.join(pd, "GhostIcon.png"))
    glow_icon(shackle(), RED[:3], os.path.join(pd, "ShackleIcon.png"))
    ud = os.path.join(ROOT, "Assets", "Art", "UI")
    for name, fn in (("icon_play", i_play), ("icon_pause", i_pause), ("icon_character", i_character),
                     ("icon_settings", i_settings), ("icon_lock", i_lock), ("icon_check", i_check)):
        ui_icon(fn).save(os.path.join(ud, name + ".png"))
    if "--preview" in sys.argv:
        sheet = Image.new("RGBA", (5 * 200 + 60, 420), (38, 43, 68, 255))
        for i, n in enumerate(("HeartIcon", "ShieldIcon", "TimeIcon", "GhostIcon", "ShackleIcon")):
            sheet.alpha_composite(Image.open(os.path.join(pd, n + ".png")).convert("RGBA").resize((180, 180)), (20 + i * 205, 20))
        for i, n in enumerate(("icon_play", "icon_character", "icon_settings", "icon_pause", "icon_lock", "icon_check")):
            im = Image.open(os.path.join(ud, n + ".png")).convert("RGBA")
            tint = Image.new("RGBA", im.size, C("krem")); tint.putalpha(im.split()[3])
            sheet.alpha_composite(tint, (30 + i * 170, 260))
        sheet.save(os.path.join(SCRATCH, "audit", "icons_new.png"))
