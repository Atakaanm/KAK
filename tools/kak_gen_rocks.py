"""Faz 14 Ö4: taş türleri bakışta ayırt edilsin (hepsi aynı sprite'tı, yalnız silik renk farkı vardı → haksız sürpriz).
Aynı taş ailesi (rock.png), davranışı söyleyen tek işaret:
  rock_homing.png  — ortada kırmızı göz: "seni görüyor, peşinden gelir"
  rock_split.png   — parlayan turuncu çatlaklar: "parçalanacak"
  rock_bounce.png  — yuvarlak, düzgün, parlak mor taş: "seker"
Kullanım: python3 tools/kak_gen_rocks.py [--preview]
"""
import math, os, sys
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import C, outline, ROOT, SCRATCH  # noqa: E402

D = os.path.join(ROOT, "Assets", "Art", "Projectiles")

def base():
    return Image.open(os.path.join(D, "rock.png")).convert("RGBA")

def homing():
    im = base(); px = im.load()
    cx, cy = 13, 13
    for y in range(cy - 3, cy + 4):
        for x in range(cx - 4, cx + 5):
            dx, dy = (x - cx) / 4.2, (y - cy) / 3.0
            if dx * dx + dy * dy <= 1: px[x, y] = C("beyaz") if dx * dx + dy * dy < 0.75 else C("murekkep")
    for (x, y) in ((13, 12), (14, 12), (13, 13), (14, 13), (13, 14), (14, 14)): px[x, y] = C("tehlike_parlak")
    px[12, 12] = C("beyaz")
    return im

def split():
    im = base(); px = im.load()
    cracks = [[(13, 5), (12, 8), (14, 11), (13, 14)], [(13, 14), (9, 17), (7, 20)], [(13, 14), (17, 17), (20, 19)], [(14, 11), (18, 9)]]
    for path in cracks:
        for (x0, y0), (x1, y1) in zip(path, path[1:]):
            n = max(abs(x1 - x0), abs(y1 - y0))
            for i in range(n + 1):
                x, y = round(x0 + (x1 - x0) * i / n), round(y0 + (y1 - y0) * i / n)
                # 2 px kalın, beyaz-sarı çekirdek: taşın kendi turuncu lekelerinden ayrılsın
                for (dx, dy, c) in ((1, 0, "turuncu"), (0, 1, "turuncu"), (0, 0, "beyaz" if i % 3 == 0 else "altin_acik")):
                    X, Y = x + dx, y + dy
                    if 0 <= X < 28 and 0 <= Y < 28 and px[X, Y][3] > 0 and (c != "turuncu" or px[X, Y][:3] != C("altin_acik")[:3]):
                        px[X, Y] = C(c)
    return im

def bounce():
    im = Image.new("RGBA", (28, 28), (0, 0, 0, 0)); px = im.load()
    for y in range(28):
        for x in range(28):
            d = math.hypot(x - 13.5, y - 13.5)
            if d <= 11:
                l = (x - 13.5) + (y - 13.5)
                px[x, y] = C("pembe") if l < -9 else C("mor") if l < 8 else C("kahve_koyu")
    for a in range(200, 290, 6):                     # parlak yay (lastik/düzgün taş hissi)
        r = math.radians(a)
        px[round(13.5 + math.cos(r) * 7.5), round(13.5 + math.sin(r) * 7.5)] = C("beyaz")
    return outline(im)

if __name__ == "__main__":
    homing().save(os.path.join(D, "rock_homing.png"))
    split().save(os.path.join(D, "rock_split.png"))
    bounce().save(os.path.join(D, "rock_bounce.png"))
    if "--preview" in sys.argv:
        sheet = Image.new("RGBA", (4 * 180 + 40, 200), (58, 92, 66, 255))
        for i, n in enumerate(("rock", "rock_homing", "rock_split", "rock_bounce")):
            sheet.alpha_composite(Image.open(os.path.join(D, n + ".png")).convert("RGBA").resize((168, 168), Image.NEAREST), (20 + i * 180, 16))
        sheet.save(os.path.join(SCRATCH, "audit", "rocks.png"))
