#!/usr/bin/env python3
"""KaçAtaKaç mağaza görselleri: oyundan gerçek ekran görüntüsü + başlık bandı, öne çıkan grafik, ikon.

Kullanım (Unity açık, köprü çalışıyor):
  python3 tools/kak_store_shots.py capture [sahne ...]   # ham görüntüler → .claude-bridge/magaza_raw/
  python3 tools/kak_store_shots.py compose               # Docs/Magaza/play (1080×1920) + appstore (1320×2868)
  python3 tools/kak_store_shots.py feature               # Docs/Magaza/play/feature_{tr,en}_1024x500.png
  python3 tools/kak_store_shots.py icon                  # Docs/Magaza/ikon_1024.png + ikon_512.png
  python3 tools/kak_store_shots.py all

Her sahne Play modunda kurulur, oyun zamanı dondurulur ve AYNI an 4 kez çekilir
(Play/App Store boyutu × TR/EN). Oyun, başlık bandının altındaki alanın boyutunda render
edilir: ölçekleme yok, piksel sanatı keskin kalır, arayüz o orana göre yerleşir.
Kayıt: köprü Play'de geçici kayıt kullanır, gerçek kayda yazılmaz.
"""
import os
import subprocess
import sys
import time

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RAW = os.path.join(ROOT, ".claude-bridge", "magaza_raw")
OUT = os.path.join(ROOT, "Docs", "Magaza")
FONTS = os.path.join(ROOT, "Assets", "Fonts")
LANGS = ("TR", "EN")

# Tuval: (genişlik, yükseklik, başlık bandı). Oyun alanı = genişlik × (yükseklik - bant)
SIZES = {
    "play": (1080, 1920, 400),   # Google Play telefon, 9:16
    "app": (1320, 2868, 560),    # App Store 6,9" iPhone
}

# Endesga 32 (sanat-rehberi.md)
INK = (0x18, 0x14, 0x25)
NIGHT = (0x26, 0x2B, 0x44)
GOLD = (0xFE, 0xAE, 0x34)
GOLD_LIGHT = (0xFE, 0xE7, 0x61)
LOGO_FILL = (0xD7, 0x98, 0x47)
MIST = (0xC0, 0xCB, 0xDC)
GREEN = (0x3E, 0x89, 0x48)
CREAM = (0xEA, 0xD4, 0xAA)

# ---------------------------------------------------------------------------
# Sahneler: sıra = mağazadaki sıra. Başlık: (TR başlık, TR alt, EN başlık, EN alt).
# "logo": başlık yerine oyunun logosu. Kurulum: köprü komutları (tools/kak_bridge.py).
# ---------------------------------------------------------------------------
BASE = [
    ["scene", "Assets/Scenes/MainMenu.unity"], ["play"],
    ["invoke", "KakDevMenu", "SetGamesPlayed", "40"], ["invoke", "KakDevMenu", "SetCoins", "2450"],
    ["invoke", "KakDevMenu", "SetBest", "1287"],
]
SHOTS = [
    {"id": "1_oyun", "logo": True,
     "cap": ("KAÇ ATA KAÇ", "Taş yağmurundan kaç, rekorunu kır!", "KAÇ ATA KAÇ", "Dodge the endless rock storm!"),
     "setup": [["invoke", "KakDevMenu", "PlayAs", "Ada"], ["wait", 2], ["invoke", "KakDevMenu", "PlayWithPet", "Firefly"],
               ["wait", 3], ["menu", "KacAtaKac/Dev/Ölümsüzlük Aç-Kapa"], ["invoke", "KakDevMenu", "AddScore", "760"],
               ["menu", "KacAtaKac/Dev/Test Botunu Başlat (usta)"], ["wait", 6]]},
    {"id": "2_karakter", "per_lang": True,
     "cap": ("5 kahraman, 5 oyun tarzı", "Altın topla, yenilerinin kilidini aç", "5 heroes, 5 play styles", "Collect gold to unlock new heroes"),
     "setup": [["invoke", "SceneLoader", "LoadMenu"], ["wait", 2], ["menu", "KacAtaKac/Dev/UI - Karakterleri Aç"], ["wait", 1],
               ["invoke", "KakDevMenu", "PressCharacterCard", "1"], ["wait", 1]]},
    {"id": "3_guc",
     "cap": ("Güçlendirmeleri kap", "Kalkan, hız, yavaş çekim, hayalet, can", "Grab power-ups", "Shield, speed, slow-mo, ghost, extra life"),
     "setup": [["invoke", "KakDevMenu", "PlayAs", "Tank"], ["wait", 3], ["menu", "KacAtaKac/Dev/Ölümsüzlük Aç-Kapa"],
               ["invoke", "KakDevMenu", "AddScore", "620"], ["menu", "KacAtaKac/Dev/Test Botunu Başlat (usta)"], ["wait", 5],
               ["menu", "KacAtaKac/Dev/Tüm Powerup'ları Ver"], ["wait", 0.6]], "take_gap": 0.4},
    {"id": "4_pet", "per_lang": True,
     "cap": ("Petin hep yanında", "Ateşböceği altın çeker, kaplumbağa korur", "Your pet has your back", "Firefly pulls gold, turtle shields you"),
     "setup": [["invoke", "SceneLoader", "LoadMenu"], ["wait", 2], ["invoke", "KakDevMenu", "OpenPets"], ["wait", 1],
               ["invoke", "KakDevMenu", "PressPetCard", "0"], ["wait", 1]]},
    {"id": "5_rekor", "per_lang": True,
     "cap": ("Rekorunu kır", "Görevleri bitir, altınları topla", "Beat your best", "Finish missions, earn gold"),
     "setup": [["invoke", "KakDevMenu", "PlayAs", "Swift"], ["wait", 3], ["invoke", "KakDevMenu", "SetBest", "900"],
               ["menu", "KacAtaKac/Dev/Test Botunu Başlat (usta)"], ["wait", 38], ["invoke", "KakDevMenu", "AddScore", "1050"],
               ["wait", 1], ["menu", "KacAtaKac/Dev/Oyuncuyu Öldür"], ["wait", 4.5]]},
    {"id": "6_gunluk", "per_lang": True,
     "cap": ("Her gün yeni ödül", "7 günlük seriyi tamamla", "Rewards every day", "Complete the 7-day streak"),
     "setup": [["invoke", "SceneLoader", "LoadMenu"], ["wait", 2], ["invoke", "KakDevMenu", "OpenDaily", "3"], ["wait", 1.5]]},
]


# ---------------------------------------------------------------------------
# Yakalama
# ---------------------------------------------------------------------------
def bridge(*args):
    r = subprocess.run([sys.executable, os.path.join(ROOT, "tools", "kak_bridge.py"), *map(str, args)],
                       capture_output=True, text=True)
    line = (r.stdout.strip().splitlines() or [""])[-1]
    print("   ", " ".join(map(str, args))[:70], "→", line[:90])
    if r.returncode != 0:
        raise SystemExit("köprü hatası: " + r.stdout + r.stderr)


def run_steps(steps):
    for s in steps:
        if s[0] == "wait":
            time.sleep(s[1])
        else:
            bridge(*s)


def grab(shot_id, langs=LANGS):
    """Oyunu dondurur, aynı anı 2 boyut × verilen dillerde çeker."""
    os.makedirs(RAW, exist_ok=True)
    bridge("invoke", "KakDevMenu", "Freeze", "1")
    try:
        for lang in langs:
            bridge("invoke", "KakDevMenu", "SetLanguage", lang)
            for kind, (w, h, band) in SIZES.items():
                bridge("shot", w, h - band, os.path.join(RAW, f"{shot_id}_{kind}_{lang}.png"))
    finally:
        bridge("invoke", "KakDevMenu", "Freeze", "0")


def capture(ids, takes=1):
    """takes > 1: aynı sahneden aralıklı birkaç an çeker ({id}~1, ~2...); en iyisini `pick` ile seç."""
    shots = [s for s in SHOTS if not ids or s["id"] in ids or s["id"].split("_", 1)[1] in ids]
    try:
        for s in shots:
            # per_lang: panellerin dinamik metinleri (oyun sonu, günlük ödül) dil değişiminde yenilenmiyor →
            # sahne her dil için o dilde baştan kurulur. Diğerleri: aynı an, dil dondurulmuş karede değişir.
            runs = [(l,) for l in LANGS] if s.get("per_lang") else [LANGS]
            for langs in runs:
                print("●", s["id"], "/".join(langs))
                run_steps(BASE + [["invoke", "KakDevMenu", "SetLanguage", langs[0]]])
                run_steps(s["setup"])
                if takes == 1:
                    grab(s["id"], langs)
                else:
                    for k in range(1, takes + 1):
                        grab(f"{s['id']}~{k}", langs)
                        time.sleep(s.get("take_gap", 2.0))
                bridge("stop")
    finally:
        # Dil yalnızca Play'deki geçici kayda yazıldı; burada edit modunda SetLanguage ÇAĞIRMA (gerçek kayda yazar)
        bridge("stop")


# ---------------------------------------------------------------------------
# Çizim yardımcıları
# ---------------------------------------------------------------------------
def font(name, size):
    return ImageFont.truetype(os.path.join(FONTS, name), size)


def vgrad(w, h, top, bottom):
    t = np.linspace(0, 1, h)[:, None, None]
    a = np.array(top, float)[None, None, :] * (1 - t) + np.array(bottom, float)[None, None, :] * t
    return Image.fromarray(np.repeat(a, w, axis=1).astype(np.uint8), "RGB")


def radial_glow(w, h, cx, cy, rx, ry, color, strength):
    y, x = np.mgrid[0:h, 0:w]
    d = ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2
    a = np.clip(1 - d, 0, 1) ** 2 * strength
    layer = np.zeros((h, w, 4), np.uint8)
    layer[..., :3] = color
    layer[..., 3] = (a * 255).astype(np.uint8)
    return Image.fromarray(layer, "RGBA")


def fit_font(fname, text, max_w, size, min_size=24):
    while size > min_size:
        f = font(fname, size)
        if f.getbbox(text)[2] - f.getbbox(text)[0] <= max_w:
            return f
        size -= 2
    return font(fname, min_size)


def text_layer(size, text, f, fill_top, fill_bottom, strokes, shadow=None):
    """Metni gradyan dolgu + katmanlı kontur (dıştan içe) + gölgeyle çizer. strokes: [(renk, kalınlık)]"""
    w, h = size
    out = Image.new("RGBA", size, (0, 0, 0, 0))
    bb = f.getbbox(text)
    x = (w - (bb[2] - bb[0])) // 2 - bb[0]
    y = (h - (bb[3] - bb[1])) // 2 - bb[1]
    if shadow:
        col, off, blur = shadow
        sh = Image.new("RGBA", size, (0, 0, 0, 0))
        ImageDraw.Draw(sh).text((x, y + off), text, font=f, fill=col + (255,),
                                stroke_width=strokes[0][1] if strokes else 0, stroke_fill=col + (255,))
        out.alpha_composite(sh.filter(ImageFilter.GaussianBlur(blur)))
    for col, sw in strokes:
        ImageDraw.Draw(out).text((x, y), text, font=f, fill=col + (255,), stroke_width=sw, stroke_fill=col + (255,))
    mask = Image.new("L", size, 0)
    ImageDraw.Draw(mask).text((x, y), text, font=f, fill=255)
    grad = vgrad(w, h, fill_top, fill_bottom).convert("RGBA")
    grad.putalpha(mask)
    out.alpha_composite(grad)
    return out


def logo(w, h, size, text="KAÇ ATA KAÇ"):
    """Menüdeki logonun benzeri: Cinzel Decorative, altın dolgu, açık iç kontur, yeşil dış kontur, koyu gölge."""
    f = fit_font("CinzelDecorative-Black.ttf", text, int(w * 0.92), size)
    s = max(2, f.size // 22)
    return text_layer((w, h), text, f, GOLD_LIGHT, LOGO_FILL,
                      [(INK, s * 3), (GREEN, s * 2), ((0xE8, 0xF5, 0xE0), max(1, s // 2))],
                      shadow=((0x0B, 0x0A, 0x12), s * 2, s * 2))


def caption_band(w, h, top_color, head, sub, use_logo, scale):
    band = vgrad(w, h, INK, top_color).convert("RGBA")
    band.alpha_composite(radial_glow(w, h, w / 2, h * 0.45, w * 0.55, h * 0.55, GOLD, 0.10))
    head_h = int(h * (0.50 if use_logo else 0.40))
    if use_logo:
        hl = logo(w, head_h, int(150 * scale))
    else:
        f = fit_font("Nunito-ExtraBold.ttf", head, w - int(110 * scale), int(100 * scale))
        s = max(3, f.size // 13)
        hl = text_layer((w, head_h), head, f, GOLD_LIGHT, GOLD, [(INK, s)], shadow=((0x0B, 0x0A, 0x12), s, s))
    fs = fit_font("Nunito-ExtraBold.ttf", sub, w - int(120 * scale), int(46 * scale))
    sl = text_layer((w, int(fs.size * 1.8)), sub, fs, (0xFF, 0xFF, 0xFF), MIST, [(INK, max(2, fs.size // 12))])
    gap = int(8 * scale)
    total = head_h + gap + sl.height
    y0 = (h - total) // 2 - int(6 * scale)
    band.alpha_composite(hl, (0, y0))
    band.alpha_composite(sl, (0, y0 + head_h + gap))
    return band


def compose():
    made = []
    for kind, (w, h, bh) in SIZES.items():
        scale = w / 1080
        os.makedirs(os.path.join(OUT, "play" if kind == "play" else "appstore"), exist_ok=True)
        for s in SHOTS:
            for li, lang in enumerate(LANGS):
                src = os.path.join(RAW, f"{s['id']}_{kind}_{lang}.png")
                if not os.path.exists(src):
                    print("  eksik ham görüntü:", os.path.relpath(src, ROOT))
                    continue
                game = Image.open(src).convert("RGB")
                assert game.size == (w, h - bh), (src, game.size)
                top = tuple(int(c) for c in np.asarray(game)[:12].reshape(-1, 3).mean(0))
                head, sub = s["cap"][li * 2], s["cap"][li * 2 + 1]
                canvas = Image.new("RGBA", (w, h))
                canvas.alpha_composite(caption_band(w, bh, top, head, sub, s.get("logo"), scale), (0, 0))
                canvas.paste(game, (0, bh))
                # bant ile oyun arasında ince ayraç: koyu çizgi + altın parıltı
                d = ImageDraw.Draw(canvas)
                lw = max(3, int(4 * scale))
                d.rectangle((0, bh - lw, w, bh - 1), fill=INK)
                glow = radial_glow(w, lw * 2, w / 2, lw, w * 0.45, lw * 1.2, GOLD, 0.9)
                canvas.alpha_composite(glow, (0, bh - lw * 2 + lw // 2))
                folder = "play" if kind == "play" else "appstore"
                name = f"{lang.lower()}_{s['id']}_{w}x{h}.png"
                path = os.path.join(OUT, folder, name)
                canvas.convert("RGB").save(path, optimize=True)
                made.append(path)
    print(f"{len(made)} görüntü yazıldı")
    return made


# ---------------------------------------------------------------------------
# Öne çıkan grafik (Google Play, 1024×500, şeffaflık yok)
# ---------------------------------------------------------------------------
def load_sprite(rel, scale):
    im = Image.open(os.path.join(ROOT, rel)).convert("RGBA")
    return im.resize((im.width * scale, im.height * scale), Image.NEAREST)


def drop_shadow(canvas, sprite, pos, off=(0, 0), alpha=110, blur=0, ellipse=None):
    if ellipse:
        cx, cy, rx, ry = ellipse
        sh = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
        ImageDraw.Draw(sh).ellipse((cx - rx, cy - ry, cx + rx, cy + ry), fill=(0x0B, 0x0A, 0x12, alpha))
        canvas.alpha_composite(sh.filter(ImageFilter.GaussianBlur(max(1, ry // 3))))
    canvas.alpha_composite(sprite, pos)


def trail(canvas, sprite, pos, step, copies=3, alpha=0.28):
    """Uçan taşın arkasında soluk kopyalar (hareket izi). step: kopyalar arası kayma (px)."""
    for k in range(copies, 0, -1):
        ghost = sprite.copy()
        a = np.asarray(ghost).copy()
        a[..., 3] = (a[..., 3] * alpha * (1 - (k - 1) / copies)).astype(np.uint8)
        canvas.alpha_composite(Image.fromarray(a, "RGBA"), (pos[0] - step[0] * k, pos[1] - step[1] * k))
    canvas.alpha_composite(sprite, pos)


def feature():
    W, H = 1024, 500
    # Arka plan: taşsız arena görseli (zemin + duvarlar + meşaleler), yatay bant
    arena = Image.open(os.path.join(ROOT, "Assets", "Sprites", "Arena", "Dungeon_arena_01.png")).convert("RGB")
    band_h = int(arena.width * H / W)
    y0 = int(arena.height * 0.50 - band_h / 2)
    bg = arena.crop((0, y0, arena.width, y0 + band_h)).resize((W, H), Image.LANCZOS).convert("RGBA")
    shade = np.zeros((H, W, 4), np.uint8)
    xs = np.linspace(0, 1, W)[None, :]
    ys = np.linspace(0, 1, H)[:, None]
    shade[..., :3] = INK
    shade[..., 3] = (np.clip(0.86 - xs * 0.62 + (ys - 0.5) ** 2 * 0.6, 0.22, 0.9) * 255).astype(np.uint8)
    bg.alpha_composite(Image.fromarray(shade, "RGBA"))
    bg.alpha_composite(radial_glow(W, H, W * 0.76, H * 0.62, 300, 230, (0xF7, 0x76, 0x22), 0.20))
    bg.alpha_composite(radial_glow(W, H, W * 0.27, H * 0.45, 400, 160, GOLD, 0.08))

    rock = lambda sc: load_sprite("Assets/Art/Projectiles/rock.png", sc)
    made = []
    for lang, sub in (("tr", "Taş yağmurundan kaç, rekorunu kır!"), ("en", "Dodge the endless rock storm!")):
        c = bg.copy()
        foot = 468
        # kahramanlar sağa doğru kaçıyor (48 px × 6, tam sayı ölçek); görünür piksel sınırına göre yerleşim
        ada = load_sprite("Assets/Sprites/Characters/Ada/South_East/South_East_Run_2.png", 6)
        ata = load_sprite("Assets/Sprites/Player/South_East/South_East_Run_4.gif", 6)
        placed = []
        for spr, left in ((ada, 604), (ata, 790)):
            bx0, by0, bx1, by1 = spr.getbbox()
            x, y = left - bx0, foot - by1
            drop_shadow(c, spr, (x, y), ellipse=(x + (bx0 + bx1) // 2, foot - 2, 70, 11), alpha=150)
            placed.append((x + bx0, y + by0, x + bx1, y + by1))
        assert placed[1][2] <= W - 24, "Ata kenara taştı"
        drop_shadow(c, load_sprite("Assets/Art/Pets/firefly_0.png", 5), (placed[0][0] + 40, placed[0][1] - 96))
        # arkadan (sol üst) gelen taşlar, izleriyle; yüzlerden uzak
        for sc, pos, st in ((4, (596, 22), (26, 14)), (3, (772, 104), (22, 12)), (5, (880, 12), (30, 16))):
            trail(c, rock(sc), pos, st)
        for rel, sc, x, y in (("Assets/Art/Pickups/coin_0.png", 4, 540, 410), ("Assets/Art/Pickups/coin_1.png", 4, 742, 422)):
            sp = load_sprite(rel, sc)
            drop_shadow(c, sp, (x, y), ellipse=(x + sp.width // 2, y + sp.height + 8, sp.width // 3, 4), alpha=90)
        # logo + alt başlık (sol)
        c.alpha_composite(logo(560, 170, 104), (10, 124))
        fs = fit_font("Nunito-ExtraBold.ttf", sub, 500, 34)
        c.alpha_composite(text_layer((560, 70), sub, fs, (0xFF, 0xFF, 0xFF), MIST, [(INK, 4)]), (10, 288))
        path = os.path.join(OUT, "play", f"feature_{lang}_1024x500.png")
        c.convert("RGB").save(path, optimize=True)
        made.append(path)
    print("öne çıkan grafik:", ", ".join(os.path.relpath(p, ROOT) for p in made))


def icon():
    src = Image.open(os.path.join(ROOT, "Assets", "Art", "Icon", "app_icon.png")).convert("RGB")
    assert src.size == (1024, 1024), src.size
    src.save(os.path.join(OUT, "ikon_1024.png"), optimize=True)
    # Play: 32 bit PNG (alfa kanallı, opak); App Store: alfasız
    src.resize((512, 512), Image.LANCZOS).convert("RGBA").save(os.path.join(OUT, "ikon_512.png"), optimize=True)
    print("ikon: Docs/Magaza/ikon_1024.png (App Store), ikon_512.png (Google Play)")


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else "compose"
    if cmd == "capture":
        args = sys.argv[2:]
        n = 1
        if "--takes" in args:
            i = args.index("--takes"); n = int(args[i + 1]); del args[i:i + 2]
        capture(args, n)
    elif cmd == "pick":  # pick 1_oyun 3 → 1_oyun~3_* dosyalarını 1_oyun_* yapar
        sid, k = sys.argv[2], sys.argv[3]
        for f in os.listdir(RAW):
            if f.startswith(f"{sid}~{k}_"):
                os.replace(os.path.join(RAW, f), os.path.join(RAW, f.replace(f"{sid}~{k}_", f"{sid}_")))
        print("seçildi:", sid, k)
    elif cmd == "compose":
        compose()
    elif cmd == "feature":
        feature()
    elif cmd == "icon":
        icon()
    elif cmd == "all":
        capture([])
        compose()
        feature()
        icon()
    else:
        print(__doc__)
