"""Faz 15 K7: karakter karelerinde el noktaları (meşale/kılıç/kalkan ele "cuk" otursun).
Her yönün duruş (idle) karesinde ten rengi kümelerinden kolları bulur (baş dışarıda); el = omuzdan en uzak piksel.
Koşu karelerinde eşyayı tutan kol sallanmaz (gerçekte de öyle): nokta = duruş noktası + gövde salınımı (baş tepesinin kayması);
böylece eşya eller arasında zıplamaz, taşınıyormuş gibi durur.
Sağ el kuralı (eşya hep sağ elde; dönünce el ve derinlik doğru):
  ekran yanı: S, SE, SW → sol · N, NE, NW → sağ · E → yakın kol · W → uzak kol
  derinlik (gövdenin önü/arkası): S, SE, E, NE → önde · N, NW, W, SW → arkada
Elle düzeltme: tools/hand_anchors_fix.json  {"Boy/East": [x, y], ...}  (duruş karesinde piksel, sol-üst 0,0)
Çıktı: Assets/Data/Anchors/hand_anchors.json (karakter → kare yolu → el pikseli, önde mi) — editör aracı CharacterAnchors SO'ya yazar.
Kullanım: python3 tools/kak_hand_anchors.py [--preview]
"""
import json, math, os, sys
from collections import Counter, deque
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kak_gen_ice import ROOT, SCRATCH  # noqa: E402

CHARS = {
    "Boy": "Assets/Sprites/Player",
    "Ada": "Assets/Sprites/Characters/Ada",
    "Swift": "Assets/Sprites/Characters/Swift",
    "Tank": "Assets/Sprites/Characters/Tank",
    "Lucky": "Assets/Sprites/Characters/Lucky",
}
DIRS = ["South", "South_East", "East", "North_East", "North", "North_West", "West", "South_West"]
SIDE = {"South": -1, "South_East": -1, "South_West": -1, "North": 1, "North_East": 1, "North_West": 1, "East": 0, "West": 0}
FRONT = {"South": True, "South_East": True, "East": True, "North_East": True, "North": False, "North_West": False, "West": False, "South_West": False}
FIX = os.path.join(ROOT, "tools", "hand_anchors_fix.json")
OUT = os.path.join(ROOT, "Assets", "Data", "Anchors", "hand_anchors.json")


def skin_colors(d):
    """Ten: güney idle yüzünün en sık rengi + ona yakın (gölge) tonlar."""
    f = [x for x in os.listdir(os.path.join(ROOT, d, "South")) if x.endswith((".png", ".gif")) and "Run" not in x][0]
    im = Image.open(os.path.join(ROOT, d, "South", f)).convert("RGBA")
    bb = im.getbbox(); w, h = bb[2] - bb[0], bb[3] - bb[1]
    cnt = Counter()
    for y in range(bb[1] + int(h * 0.18), bb[1] + int(h * 0.32)):
        for x in range(bb[0] + int(w * 0.3), bb[0] + int(w * 0.7)):
            p = im.getpixel((x, y))
            if p[3] > 200: cnt[p[:3]] += 1
    main = cnt.most_common(1)[0][0]
    return main


def is_skin(p, main):
    if p[3] < 200: return False
    r, g, b = p[:3]
    mr, mg, mb = main
    # ana ten ya da gölgesi (aynı ton ailesi, daha koyu/açık): kırmızı > yeşil > mavi ve oranlar yakın
    if not (r > g > b): return False
    d = abs(r - mr) + abs(g - mg) + abs(b - mb)
    if d < 30: return True
    ratio = (g / max(1, r), b / max(1, r)); mratio = (mg / mr, mb / mr)
    return abs(ratio[0] - mratio[0]) < 0.09 and abs(ratio[1] - mratio[1]) < 0.12 and 0.55 * mr <= r <= 1.15 * mr


def components(im, main, bb):
    w, h = im.size
    top_cut = bb[1] + (bb[3] - bb[1]) * 0.30   # baş/boyun üstü
    seen = set(); comps = []
    for y in range(h):
        for x in range(w):
            if (x, y) in seen or y < top_cut or not is_skin(im.getpixel((x, y)), main): continue
            q = deque([(x, y)]); seen.add((x, y)); c = []
            while q:
                cx, cy = q.popleft(); c.append((cx, cy))
                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if 0 <= nx < w and 0 <= ny < h and (nx, ny) not in seen and ny >= top_cut and is_skin(im.getpixel((nx, ny)), main):
                        seen.add((nx, ny)); q.append((nx, ny))
            comps.append(c)
    return comps


def hand_of(comp):
    """Omuz ≈ en üst piksel; el = omuzdan en uzak piksellerin ortası."""
    top = min(comp, key=lambda p: (p[1], p[0]))
    far = max(math.hypot(p[0] - top[0], p[1] - top[1]) for p in comp)
    tip = [p for p in comp if math.hypot(p[0] - top[0], p[1] - top[1]) >= far - 1.5]
    return (sum(p[0] for p in tip) / len(tip), sum(p[1] for p in tip) / len(tip))


def detect(path, main, direction):
    im = Image.open(path).convert("RGBA")
    bb = im.getbbox(); h = bb[3] - bb[1]
    leg_cut = bb[1] + h * 0.72  # bu çizginin altına inen kümeler bacak
    head_cut = bb[1] + h * 0.42  # ağırlık merkezi bunun üstündeyse baş/boyun/kulak
    arms = [c for c in components(im, main, bb)
            if len(c) >= 2 and min(p[1] for p in c) < leg_cut and sum(p[1] for p in c) / len(c) > head_cut]
    cx = (bb[0] + bb[2]) / 2
    if not arms:
        return None, im
    hands = [(hand_of(c), len(c)) for c in arms]
    side = SIDE[direction]
    if side < 0: pick = min(hands, key=lambda t: t[0][0])
    elif side > 0: pick = max(hands, key=lambda t: t[0][0])
    elif direction == "East": pick = max(hands, key=lambda t: (t[1], t[0][0]))   # yakın kol: en büyük
    else:
        pick = min(hands, key=lambda t: t[1]) if len(hands) > 1 else hands[0]   # Batı: uzak kol (küçük); yoksa görünenden
    hx, hy = pick[0]
    # Sağ el gövdenin arkasında gizliyse (KB, GB) yalnız sol el görünür: gövde ortasına göre aynala (gizli el öbür yanda)
    if side != 0:
        rows = [(x, y) for y in range(bb[1] + int(h * 0.40), bb[1] + int(h * 0.65)) for x in range(im.width) if im.getpixel((x, y))[3] > 200]
        torso = sum(x for x, _ in rows) / len(rows) if rows else cx
        if (side < 0 and hx > torso) or (side > 0 and hx < torso): hx = 2 * torso - hx
    return (hx, hy), im


def head_ref(im):
    """Gövde salınımı için referans: baş tepesi (en üst opak satır) ve o satırın ortası, kare merkezine göre."""
    bb = im.getbbox()
    top = bb[1]
    xs = [x for x in range(im.width) if im.getpixel((x, top))[3] > 200]
    cx = sum(xs) / len(xs) if xs else im.width / 2
    return cx - im.width / 2, top - im.height / 2


def main():
    fixes = json.load(open(FIX)) if os.path.exists(FIX) else {}
    result = {}
    previews = []
    for name, d in CHARS.items():
        skin = skin_colors(d)
        result[name] = {}
        for direction in DIRS:
            folder = os.path.join(ROOT, d, direction)
            if not os.path.isdir(folder): continue
            files = sorted(f for f in os.listdir(folder) if f.endswith((".png", ".gif")))
            idle = [f for f in files if "Run" not in f]
            runs = [f for f in files if "Run" in f]
            if not idle: continue
            # duruş karesi: el noktası (merkeze göre) + baş referansı
            key = f"{name}/{direction}"
            hand, im0 = detect(os.path.join(folder, idle[0]), skin, direction)
            if key in fixes: hand = tuple(fixes[key])
            if hand is None:
                bb = im0.getbbox(); hand = ((bb[0] + bb[2]) / 2, bb[1] + (bb[3] - bb[1]) * 0.6)
            rel_hand = (hand[0] + 0.5 - im0.width / 2, hand[1] + 0.5 - im0.height / 2)
            h0 = head_ref(im0)
            for f in idle + runs:
                im = Image.open(os.path.join(folder, f)).convert("RGBA")
                hr = head_ref(im)
                ax = rel_hand[0] + (hr[0] - h0[0])
                ay = rel_hand[1] + (hr[1] - h0[1])
                rel = os.path.relpath(os.path.join(folder, f), ROOT)
                # dx, dy: kare merkezinden piksel (sağ +, aşağı +)
                result[name][rel] = {"dx": round(ax, 2), "dy": round(ay, 2), "front": FRONT[direction], "mirror": "West" in direction}
                previews.append((name, direction, f, im, (ax + im.width / 2 - 0.5, ay + im.height / 2 - 0.5)))
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    # Unity JsonUtility sözlük okuyamaz: liste biçimi
    out = {"chars": [{"name": n, "frames": [dict(path=k, **v) for k, v in fr.items()]} for n, fr in result.items()]}
    json.dump(out, open(OUT, "w"), indent=1, ensure_ascii=False)
    print("el noktaları →", OUT, sum(len(v) for v in result.values()), "kare")
    if "--preview" in sys.argv:
        for name in CHARS:
            rows = [p for p in previews if p[0] == name]
            sc = 5
            sheet = Image.new("RGBA", (5 * 52 * sc + 40, 8 * 52 * sc + 40), (52, 74, 60, 255))
            dr = ImageDraw.Draw(sheet)
            for di, direction in enumerate(DIRS):
                frames = [p for p in rows if p[1] == direction]
                for fi, (_, _, f, im, hand) in enumerate(frames):
                    ox, oy = 20 + fi * 52 * sc, 20 + di * 52 * sc
                    sheet.alpha_composite(im.resize((im.width * sc, im.height * sc), Image.NEAREST), (ox, oy))
                    hx, hy = ox + (hand[0] + 0.5) * sc, oy + (hand[1] + 0.5) * sc
                    col = (255, 60, 60, 255) if FRONT[direction] else (60, 160, 255, 255)
                    dr.ellipse((hx - 6, hy - 6, hx + 6, hy + 6), outline=col, width=3)
                    # meşale sapı (hayali): elden yukarı
                    dr.line((hx, hy, hx + 2 * sc, hy - 9 * sc), fill=(255, 210, 90, 200), width=sc)
            out = os.path.join(SCRATCH, "audit", f"hands_{name}.png")
            sheet.save(out)
            print("önizleme:", out)


if __name__ == "__main__":
    main()
