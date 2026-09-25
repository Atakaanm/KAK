#!/usr/bin/env python3
"""
KaçAtaKaç — Unity Editor köprü istemcisi (Assets/Editor/ClaudeBridge/KakBridge.cs ile konuşur).

Kullanım örnekleri:
  python3 tools/kak_bridge.py ping
  python3 tools/kak_bridge.py hb                      # heartbeat (editör canlı mı, kaç sn önce)
  python3 tools/kak_bridge.py refresh                 # değişen asset/script'leri içe aktar, derlemeyi bekle
  python3 tools/kak_bridge.py compile                 # zorla yeniden derle
  python3 tools/kak_bridge.py play | stop
  python3 tools/kak_bridge.py playfor 20              # Play → 20 sn bekle → loglardaki hata/uyarı özeti → Stop
  python3 tools/kak_bridge.py shot 1080 1920 [yol]    # Play modunda ekran görüntüsü
  python3 tools/kak_bridge.py shots [önek]            # standart telefon oranlarında ekran görüntüleri
  python3 tools/kak_bridge.py tests PlayMode [grup]
  python3 tools/kak_bridge.py menu "KacAtaKac/Diagnose Scene"
  python3 tools/kak_bridge.py invoke Tip.AdI Metot [stringArg]
  python3 tools/kak_bridge.py scene Assets/Scenes/SampleScene.unity
  python3 tools/kak_bridge.py log [satır]             # console.log sonu
  python3 tools/kak_bridge.py errors                  # console.log içindeki hata/istisnalar
  python3 tools/kak_bridge.py clear                   # console.log temizle
  python3 tools/kak_bridge.py focus                   # Unity'yi öne getir (arka planda tick yoksa)

Çıkış kodu: 0 = başarılı, 1 = hata/zaman aşımı.
"""
import json
import os
import subprocess
import sys
import time
import uuid

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BRIDGE = os.path.join(ROOT, ".claude-bridge")
INBOX = os.path.join(BRIDGE, "inbox")
OUTBOX = os.path.join(BRIDGE, "outbox")
CONSOLE = os.path.join(BRIDGE, "console.log")
HEARTBEAT = os.path.join(BRIDGE, "heartbeat.json")
SHOTS = os.path.join(BRIDGE, "screenshots")
UNITY_APP = "/Applications/Unity/Hub/Editor/6000.3.8f1/Unity.app"

# Standart test oranları (dikey telefon + tablet)
PHONE_SIZES = [
    ("9x16", 1080, 1920),
    ("9x19.5", 1170, 2532),
    ("9x20", 1080, 2400),
    ("9x21", 1080, 2520),
    ("3x4", 1536, 2048),
]


def heartbeat_age():
    try:
        with open(HEARTBEAT, encoding="utf-8") as f:
            hb = json.load(f)
        return (time.time() * 1000 - hb["unixMs"]) / 1000.0, hb
    except Exception:
        return None, None


def focus_unity():
    subprocess.run(["open", "-a", UNITY_APP], check=False)


def send(cmd, arg="", arg2="", path="", width=0, height=0, seconds=0.0, timeout=240, quiet=False):
    os.makedirs(INBOX, exist_ok=True)
    os.makedirs(OUTBOX, exist_ok=True)
    cid = time.strftime("%Y%m%d-%H%M%S") + "-" + uuid.uuid4().hex[:6]
    payload = {"id": cid, "cmd": cmd, "arg": arg, "arg2": arg2, "path": path,
               "width": width, "height": height, "seconds": seconds}
    tmp = os.path.join(INBOX, cid + ".tmp")
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump(payload, f)
    os.replace(tmp, os.path.join(INBOX, cid + ".json"))

    out = os.path.join(OUTBOX, cid + ".json")
    start = time.time()
    focused_once = False
    while time.time() - start < timeout:
        if os.path.exists(out):
            time.sleep(0.05)
            with open(out, encoding="utf-8") as f:
                res = json.load(f)
            os.remove(out)
            if not quiet:
                print(("OK   " if res["ok"] else "HATA ") + res["cmd"] + ": " + res["message"])
                if res.get("data"):
                    print(res["data"])
            return res
        # Editör uyuyorsa (heartbeat eski) bir kez öne getir
        age, _ = heartbeat_age()
        if not focused_once and age is not None and age > 45 and time.time() - start > 45:
            if not quiet:
                print("… heartbeat %.0f sn eski, Unity öne getiriliyor" % age)
            focus_unity()
            focused_once = True
        time.sleep(0.25)
    print("HATA " + cmd + ": zaman aşımı (%d sn). Heartbeat yaşı: %s" % (timeout, heartbeat_age()[0]))
    return {"ok": False, "cmd": cmd, "message": "timeout", "data": ""}


def tail(n=60):
    if not os.path.exists(CONSOLE):
        print("(console.log yok)")
        return
    with open(CONSOLE, encoding="utf-8", errors="replace") as f:
        lines = f.readlines()
    print("".join(lines[-n:]), end="")


def errors(since_line=0):
    if not os.path.exists(CONSOLE):
        return []
    with open(CONSOLE, encoding="utf-8", errors="replace") as f:
        lines = f.readlines()[since_line:]
    out = []
    capture = 0
    for line in lines:
        if "[ERROR]" in line or "[EXCEPTION]" in line or "[ASSERT]" in line:
            out.append(line.rstrip())
            capture = 4
        elif capture > 0 and not line[:2].isdigit():
            out.append("    " + line.rstrip())
            capture -= 1
        else:
            capture = 0
    return out


def console_len():
    if not os.path.exists(CONSOLE):
        return 0
    with open(CONSOLE, encoding="utf-8", errors="replace") as f:
        return len(f.readlines())


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return 1
    c = sys.argv[1]
    a = sys.argv[2:]

    if c == "hb":
        age, hb = heartbeat_age()
        if hb is None:
            print("heartbeat yok (köprü derlenmemiş olabilir)")
            return 1
        print("heartbeat %.1f sn önce | derleniyor=%s play=%s odak=%s sahne=%s bekleyen=%s" % (
            age, hb["isCompiling"], hb["isPlaying"], hb["appFocused"], hb["activeScene"], bool(hb["pending"])))
        return 0 if age < 10 else 1
    if c == "focus":
        focus_unity()
        return 0
    if c == "log":
        tail(int(a[0]) if a else 60)
        return 0
    if c == "errors":
        e = errors()
        print("\n".join(e) if e else "(hata yok)")
        return 1 if e else 0
    if c == "clear":
        r = send("clearConsole")
        return 0 if r["ok"] else 1
    if c == "view":
        r = send("invoke", arg="KakDevSetup", arg2={"game": "UseGameView", "sim": "UseSimulatorView"}.get(a[0] if a else "", "ViewInfo"))
        return 0 if r["ok"] else 1
    if c == "shot":
        w, h = int(a[0]), int(a[1])
        path = a[2] if len(a) > 2 else os.path.join(SHOTS, "shot_%dx%d.png" % (w, h))
        r = send("screenshot", path=os.path.abspath(path), width=w, height=h, timeout=60)
        return 0 if r["ok"] else 1
    if c == "shots":
        prefix = a[0] if a else time.strftime("%H%M%S")
        ok = True
        # Device Simulator açıksa oyun Screen boyutunu simüle cihazdan okur → Game view'a geç, sonra geri dön
        info = send("invoke", arg="KakDevSetup", arg2="ViewInfo", quiet=True)
        was_sim = "SimulatorView" in (info.get("data") or "")
        if was_sim:
            send("invoke", arg="KakDevSetup", arg2="UseGameView", quiet=True)
        send("invoke", arg="KakDevSetup", arg2="GodModeOn", quiet=True)
        for name, w, h in PHONE_SIZES:
            path = os.path.join(SHOTS, "%s_%s.png" % (prefix, name))
            r = send("screenshot", path=path, width=w, height=h, timeout=60)
            ok = ok and r["ok"]
        send("invoke", arg="KakDevSetup", arg2="GodModeOff", quiet=True)
        if was_sim:
            send("invoke", arg="KakDevSetup", arg2="UseSimulatorView", quiet=True)
        montage = os.path.join(SHOTS, prefix + "_montaj.png")
        subprocess.run([sys.executable, os.path.join(ROOT, "tools", "kak_montage.py"), montage] +
                       [os.path.join(SHOTS, "%s_%s.png" % (prefix, n)) for n, _, _ in PHONE_SIZES] + ["--h", "760"],
                       check=False, capture_output=True)
        print("montaj: " + montage)
        return 0 if ok else 1
    if c == "playfor":
        secs = float(a[0]) if a else 10
        start_line = console_len()
        r = send("play", timeout=120)
        if not r["ok"]:
            return 1
        time.sleep(secs)
        e = errors(start_line)
        r2 = send("stop", timeout=60)
        print("--- Play süresince hata/istisna: %d" % len([x for x in e if not x.startswith("    ")]))
        if e:
            print("\n".join(e[:80]))
        return 0 if (not e and r2["ok"]) else 1
    if c == "tests":
        mode = a[0] if a else "PlayMode"
        group = a[1] if len(a) > 1 else ""
        r = send("tests", arg=mode, arg2=group, seconds=600, timeout=660)
        return 0 if r["ok"] else 1
    if c == "scene":
        r = send("openScene", arg=a[0])
        return 0 if r["ok"] else 1
    if c == "menu":
        r = send("menu", arg=a[0])
        return 0 if r["ok"] else 1
    if c == "invoke":
        r = send("invoke", arg=a[0], arg2=a[1], path=a[2] if len(a) > 2 else "")
        return 0 if r["ok"] else 1
    if c in ("ping", "state", "refresh", "compile", "play", "stop", "saveScenes"):
        r = send(c, seconds=600 if c in ("refresh", "compile") else 0, timeout=660 if c in ("refresh", "compile") else 240)
        if c in ("refresh", "compile"):
            # Unity bazen komuttan önce kendisi derler: son derlemenin sonucunu her zaman göster
            try:
                with open(os.path.join(BRIDGE, "compile.json"), encoding="utf-8") as f:
                    ci = json.load(f)
                age = time.time() - ci["unixMs"] / 1000.0
                print("son derleme: %s (%.0f sn önce)%s" % ("BAŞARILI" if ci["success"] else "HATALI", age,
                      "" if ci["success"] else "\n" + "\n".join(ci["errors"][:20])))
                if not ci["success"]:
                    return 1
            except Exception:
                pass
        return 0 if r["ok"] else 1
    if c == "pause":
        r = send("pause", arg=a[0] if a else "on")
        return 0 if r["ok"] else 1
    print("bilinmeyen komut: " + c)
    return 1


if __name__ == "__main__":
    sys.exit(main())
