"""Faz 13 K1: karanlık dünya için ışık ve parıltı sprite'ları (Assets/Resources, çalışma anında yüklenir).
  LightCircle.png  — maske deliği (sert daire, beyaz)          PPU 100
  LightEdge.png    — deliğin yumuşak kenarı (siyah halka)       PPU 100
  LightGlow.png    — meşalenin sıcak hâlesi (yumuşak, beyaz)    PPU 100 (renk kodda)
  Glint.png        — taş/göz parıltısı (yumuşak nokta, beyaz)   PPU 100
  Torch.png        — elde meşale (piksel, 8×18)                 PPU 41,667
  GlowEyes.png     — fırlatıcı gözleri (piksel, 9×3, beyaz)      PPU 41,667
Kullanım: python3 tools/kak_gen_dark.py [--preview]
"""
import math, os, sys
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import C, outline, ROOT, SCRATCH  # noqa: E402

RES = os.path.join(ROOT, "Assets", "Resources")

def radial(size, fn, rgb=(255, 255, 255)):
    img = Image.new("RGBA", (size, size), rgb + (0,)); px = img.load()
    c = (size - 1) / 2.0
    for y in range(size):
        for x in range(size):
            r = math.hypot(x - c, y - c) / (size / 2.0)
            px[x, y] = rgb + (int(max(0, min(1, fn(r))) * 255),)
    return img

def smooth(a, b, x):
    t = max(0.0, min(1.0, (x - a) / (b - a)))
    return t * t * (3 - 2 * t)

def torch():
    img = Image.new("RGBA", (8, 18), (0, 0, 0, 0)); px = img.load()
    for y in range(8, 18):
        px[3, y] = C("kahve"); px[4, y] = C("kahve_koyu")
    for x in range(2, 6): px[x, 7] = C("arduvaz_acik"); px[x, 8] = C("arduvaz")
    flame = ["..YY..", ".YWWY.", "YWWWWY", "OYWWYO", ".OYYO.", "..OO.."]
    cols = {"W": C("altin_acik"), "Y": C("altin"), "O": C("turuncu")}
    for j, row in enumerate(flame):
        for i, ch in enumerate(row):
            if ch in cols: px[1 + i, 1 + j] = cols[ch]
    return outline(img)

def eyes():
    img = Image.new("RGBA", (9, 3), (0, 0, 0, 0)); px = img.load()
    for (x, y) in ((1, 1), (2, 1), (6, 1), (7, 1)): px[x, y] = (255, 255, 255, 255)
    for (x, y) in ((1, 0), (7, 0), (2, 2), (6, 2)): px[x, y] = (255, 255, 255, 120)
    return img

if __name__ == "__main__":
    # Maske deliği 0,95; kenar halkası 0,93'te tam koyu olur ve 0,99'a kadar sürer: delik sınırında boşluk/nokta kalmaz
    # 512 px: 128 px dokuda maske sınırı büyütülünce tırtıklı/noktalı çizgi oluyordu. Halka deliğin içinde biter (0,955);
    # deliğin dışını karanlık katmanı kaplar → çift kararma çizgisi yok
    radial(512, lambda r: 1.0 if r <= 0.95 else 0.0).save(os.path.join(RES, "LightCircle.png"))
    radial(512, lambda r: smooth(0.5, 0.95, r) if r <= 0.955 else 0.0, (0, 0, 0)).save(os.path.join(RES, "LightEdge.png"))
    radial(128, lambda r: (1 - smooth(0.0, 1.0, r)) ** 1.6).save(os.path.join(RES, "LightGlow.png"))
    radial(32, lambda r: (1 - smooth(0.0, 1.0, r)) ** 1.3).save(os.path.join(RES, "Glint.png"))
    torch().save(os.path.join(RES, "Torch.png"))
    eyes().save(os.path.join(RES, "GlowEyes.png"))
    if "--preview" in sys.argv:
        sheet = Image.new("RGBA", (128 * 4 + 200, 160), (24, 20, 37, 255))
        for i, n in enumerate(("LightCircle", "LightEdge", "LightGlow", "Glint")):
            im = Image.open(os.path.join(RES, n + ".png")).convert("RGBA").resize((128, 128))
            bg = Image.new("RGBA", (128, 128), (90, 105, 136, 255)); bg.alpha_composite(im)
            sheet.alpha_composite(bg, (i * 132, 0))
        sheet.alpha_composite(torch().resize((48, 108), Image.NEAREST), (540, 10))
        sheet.alpha_composite(eyes().resize((72, 24), Image.NEAREST), (600, 60))
        sheet.save(os.path.join(SCRATCH, "dark_preview.png"))
