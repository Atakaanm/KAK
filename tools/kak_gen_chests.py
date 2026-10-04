"""Faz 15 K6: sandık ve anahtar görselleri (piksel; aynı ışık sol-üst, 1 px kontur, paletten).
Çıktı (Assets/Art/UI/Chests/): chest_<tür>_closed.png, chest_<tür>_open.png (32×28 → ×8), key.png (16 → ×8), rays.png (ışık hüzmesi, 64 → ×4)
Türler: wood (ahşap), silver (gümüş), gold (altın).
Kullanım: python3 tools/kak_gen_chests.py [--preview]
"""
import math, os, sys
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import C, outline, ROOT, SCRATCH  # noqa: E402

OUT = os.path.join(ROOT, "Assets", "Art", "UI", "Chests")
W, H = 32, 28

KINDS = {
    #          gövde açık, gövde, gövde koyu, bant açık, bant, bant koyu, kilit
    "wood":   (C("turuncu_acik"), C("bakir"), C("kahve"), C("arduvaz_acik"), C("arduvaz"), C("arduvaz_koyu"), C("altin")),
    "silver": (C("sis"), C("arduvaz_acik"), C("arduvaz"), C("beyaz"), C("camgobegi_parlak"), C("camgobegi"), C("camgobegi_parlak")),
    "gold":   (C("altin_acik"), C("altin"), C("turuncu"), C("krem"), C("tehlike"), C("tehlike_koyu"), C("beyaz")),
}

def chest(kind, opened):
    hi, mid, dark, bhi, band, bdark, lock = KINDS[kind]
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0)); px = img.load()
    body_top = 13
    # gövde (alt kutu)
    for y in range(body_top, H - 1):
        for x in range(3, W - 3):
            c = mid
            if y == body_top or x == 3: c = hi
            if y >= H - 4 or x >= W - 5: c = dark
            px[x, y] = c
    # metal bantlar (dikey) ve alt kenar
    for bx in (7, W - 9):
        for y in range(body_top, H - 1):
            for x in (bx, bx + 1):
                px[x, y] = bhi if x == bx else band
    for x in range(3, W - 3):
        px[x, H - 2] = bdark
    if not opened:
        # kapak (yarım yuvarlak)
        for y in range(3, body_top):
            for x in range(3, W - 3):
                dx = (x - (W - 1) / 2) / ((W - 6) / 2)
                top = 3 + 6 * (1 - math.sqrt(max(0.0, 1 - dx * dx)))
                if y >= top:
                    c = mid
                    if y <= top + 1 or x == 3: c = hi
                    if x >= W - 5: c = dark
                    px[x, y] = c
        for bx in (7, W - 9):
            for y in range(3, body_top):
                if px[bx, y][3] > 0: px[bx, y] = bhi; px[bx + 1, y] = band
        for x in range(3, W - 3):
            px[x, body_top - 1] = bdark
        # kilit
        for y in range(body_top - 2, body_top + 4):
            for x in range(W // 2 - 2, W // 2 + 2):
                px[x, y] = lock
        px[W // 2 - 1, body_top + 1] = C("murekkep"); px[W // 2 - 1, body_top + 2] = C("murekkep")
    else:
        # açık kapak arkada (ince şerit) + içten taşan ışık
        for y in range(2, 6):
            for x in range(4, W - 4):
                px[x, y] = dark if y == 5 else (hi if y == 2 else mid)
        for y in range(6, body_top + 1):
            for x in range(5, W - 5):
                k = (y - 6) / (body_top - 6)
                px[x, y] = C("beyaz") if k < 0.35 else C("altin_acik") if k < 0.8 else C("altin")
        for x in range(4, W - 4):
            px[x, body_top] = hi
    return outline(img)

def key():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0)); px = img.load()
    for y in range(16):
        for x in range(16):
            d = math.hypot(x - 4.5, y - 4.5)
            if 1.6 <= d <= 3.6: px[x, y] = C("altin_acik") if (x + y) < 9 else C("altin")
    for i in range(6, 14):
        px[i, i] = C("altin"); px[i - 1, i] = C("altin_acik")
    for (x, y) in ((10, 12), (11, 11), (12, 14), (13, 13)):
        px[x, y] = C("altin")
    return outline(img)

def rays():
    n = 64
    img = Image.new("RGBA", (n, n), (0, 0, 0, 0)); px = img.load()
    c = (n - 1) / 2
    for y in range(n):
        for x in range(n):
            a = math.atan2(y - c, x - c)
            r = math.hypot(x - c, y - c) / c
            if r > 1: continue
            beam = 0.5 + 0.5 * math.cos(a * 12)
            alpha = (beam ** 3) * (1 - r) ** 1.2 + 0.35 * (1 - r) ** 3
            a8 = int(min(1.0, alpha) * 255)
            a8 = (a8 // 40) * 40  # basamaklı (piksel hissi)
            if a8 > 0: px[x, y] = (255, 240, 200, a8)
    return img

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for k in KINDS:
        chest(k, False).resize((W * 8, H * 8), Image.NEAREST).save(os.path.join(OUT, f"chest_{k}_closed.png"))
        chest(k, True).resize((W * 8, H * 8), Image.NEAREST).save(os.path.join(OUT, f"chest_{k}_open.png"))
    key().resize((128, 128), Image.NEAREST).save(os.path.join(OUT, "key.png"))
    rays().resize((256, 256), Image.NEAREST).save(os.path.join(OUT, "rays.png"))
    print("sandıklar →", OUT)
    if "--preview" in sys.argv:
        sheet = Image.new("RGBA", (3 * 2 * 270 + 300, 280), C("gece"))
        x = 10
        for k in KINDS:
            for st in ("closed", "open"):
                sheet.alpha_composite(Image.open(os.path.join(OUT, f"chest_{k}_{st}.png")), (x, 20)); x += 270
        sheet.alpha_composite(Image.open(os.path.join(OUT, "key.png")), (x, 60))
        sheet.save(os.path.join(SCRATCH, "audit", "chests.png"))
        print("önizleme:", os.path.join(SCRATCH, "audit", "chests.png"))
