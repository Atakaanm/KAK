"""Faz 12 H4: Görünmezlik eşyası ikonu (üstü çizili göz, mor parıltı) + fırlatıcının "?" işareti (dünya sprite'ı).
Kullanım: python3 tools/kak_gen_invisible.py [--preview]
Çıktılar: Assets/Sprites/Powerups/InvisibleIcon.png (256 px, diğer ikonlarla aynı) · Assets/Resources/QuestionMark.png (piksel, PPU 41,667)
"""
import os, sys
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import C, outline, glow_icon, ROOT, SCRATCH  # noqa: E402

def eye_slash():
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0)); px = img.load()
    WH, MIST, IRIS, IRIS_L, PUP = C("beyaz"), C("sis"), C("mor"), C("pembe"), C("murekkep")
    # badem göz
    for y in range(8, 25):
        for x in range(3, 30):
            dx, dy = (x - 16) / 13.0, (y - 16) / 7.5
            if abs(dy) <= (1 - dx * dx) ** 0.5 * 1.0 if abs(dx) < 1 else False:
                px[x, y] = WH if y < 17 else MIST
    # iris + bebek + parıltı
    for y in range(10, 23):
        for x in range(10, 23):
            d = ((x + 0.5 - 16.5) ** 2 + (y + 0.5 - 16.5) ** 2) ** 0.5
            if d <= 5.6: px[x, y] = IRIS_L if d > 4.0 else IRIS if d > 2.2 else PUP
    px[14, 14] = WH; px[15, 14] = WH; px[14, 15] = WH
    img = outline(img)
    px = img.load()
    # çapraz çizgi (koyu, açık kenarlı): görünmez
    for i in range(-12, 13):
        x, y = 16 + i, 16 + i
        for w in (-1, 0, 1):
            if 0 <= x + w < 32 and 0 <= y < 32: px[x + w, y] = C("murekkep")
        if 0 <= x + 2 < 32 and 0 <= y < 32: px[x + 2, y] = C("sis")
    return img

def question_mark():
    img = Image.new("RGBA", (12, 16), (0, 0, 0, 0)); px = img.load()
    Y, YD = C("altin_acik"), C("altin")
    pat = ["..XXXXX.",
           ".XX...XX",
           ".XX...XX",
           "......XX",
           ".....XX.",
           "....XX..",
           "...XX...",
           "...XX...",
           "........",
           "...XX...",
           "...XX..."]
    for j, row in enumerate(pat):
        for i, ch in enumerate(row):
            if ch == "X": px[2 + i, 2 + j] = Y if j < 6 else YD
    return outline(img)

if __name__ == "__main__":
    glow_icon(eye_slash(), C("pembe")[:3], os.path.join(ROOT, "Assets", "Sprites", "Powerups", "InvisibleIcon.png"))
    question_mark().save(os.path.join(ROOT, "Assets", "Resources", "QuestionMark.png"))
    if "--preview" in sys.argv:
        sheet = Image.new("RGBA", (256 + 140, 256), (58, 68, 102, 255))
        sheet.alpha_composite(Image.open(os.path.join(ROOT, "Assets", "Sprites", "Powerups", "InvisibleIcon.png")).convert("RGBA"), (0, 0))
        sheet.alpha_composite(question_mark().resize((96, 128), Image.NEAREST), (280, 60))
        sheet.save(os.path.join(SCRATCH, "invisible_preview.png"))
