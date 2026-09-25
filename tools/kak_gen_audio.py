#!/usr/bin/env python3
"""
Prosedürel 8-bit ses seti (Faz 10): efektler + iki müzik döngüsü. Dış kaynak/indirme yok.
Çıktı: Assets/Audio/Sfx/*.wav, Assets/Audio/Music/*.wav  (44.1 kHz, mono, 16-bit)
Tekrar üretilebilir (sabit tohum). Ses karakteri: yumuşak kare/üçgen dalga, kısa zarflar, kulak yormayan seviye.
"""
import os
import wave

import numpy as np

SR = 44100
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SFX = os.path.join(ROOT, "Assets", "Audio", "Sfx")
MUS = os.path.join(ROOT, "Assets", "Audio", "Music")
rng = np.random.default_rng(7)


def t_(dur):
    return np.arange(int(SR * dur)) / SR


def env(n, a=0.005, d=0.1, s=0.0, r=0.05, total=None):
    """ADSR benzeri zarf (saniye)."""
    total = n / SR if total is None else total
    t = np.arange(n) / SR
    e = np.ones(n)
    e = np.where(t < a, t / max(a, 1e-6), e)
    dec_end = a + d
    e = np.where((t >= a) & (t < dec_end), 1 - (1 - s) * (t - a) / max(d, 1e-6), e)
    e = np.where(t >= dec_end, s, e) if s > 0 else np.where(t >= dec_end, np.maximum(0, 1 - (t - dec_end) / max(r, 1e-6)) * 0, e)
    if s > 0:
        rel_start = total - r
        e = np.where(t >= rel_start, s * np.maximum(0, 1 - (t - rel_start) / max(r, 1e-6)), e)
    return np.clip(e, 0, 1)


def osc(freq, dur, kind="square", duty=0.5):
    t = t_(dur)
    f = np.broadcast_to(freq, t.shape) if np.ndim(freq) else np.full(t.shape, freq)
    phase = np.cumsum(f) / SR
    frac = phase % 1.0
    if kind == "square":
        return np.where(frac < duty, 1.0, -1.0)
    if kind == "tri":
        return 4 * np.abs(frac - 0.5) - 1
    if kind == "saw":
        return 2 * frac - 1
    return np.sin(2 * np.pi * phase)


def noise(dur):
    return rng.uniform(-1, 1, int(SR * dur))


def lowpass(x, k=0.15):
    y = np.empty_like(x)
    acc = 0.0
    for i, v in enumerate(x):
        acc += k * (v - acc)
        y[i] = acc
    return y


def sweep(f0, f1, dur, curve=1.0):
    t = t_(dur) / dur
    return f0 + (f1 - f0) * t ** curve


def fade(x, ms=4):
    n = int(SR * ms / 1000)
    if n > 0 and len(x) > 2 * n:
        x[:n] *= np.linspace(0, 1, n)
        x[-n:] *= np.linspace(1, 0, n)
    return x


def write(path, x, vol=0.8):
    x = fade(np.asarray(x, dtype=np.float64))
    peak = np.max(np.abs(x)) or 1.0
    x = x / peak * vol
    data = (np.clip(x, -1, 1) * 32767).astype(np.int16)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())
    print("  %-28s %.2f sn" % (os.path.relpath(path, ROOT), len(data) / SR))


def note(n):  # MIDI → Hz
    return 440.0 * 2 ** ((n - 69) / 12)


def sfx():
    # Buton: kısa yükselen blip
    d = 0.06
    x = osc(sweep(900, 1350, d), d, "square", 0.25) * env(int(SR * d), 0.002, 0.05)
    write(os.path.join(SFX, "click.wav"), x, 0.45)

    # Taş fırlatma: alçalan hava sesi (sık çalar → yumuşak)
    d = 0.16
    x = lowpass(noise(d), 0.08) * env(int(SR * d), 0.01, 0.15) + 0.3 * osc(sweep(420, 180, d), d, "tri") * env(int(SR * d), 0.005, 0.12)
    write(os.path.join(SFX, "throw.wav"), x, 0.35)

    # Vuruş: tok darbe + çatırtı
    d = 0.24
    x = osc(sweep(190, 55, d, 0.6), d, "sine") * env(int(SR * d), 0.002, 0.22) + 0.6 * noise(d) * env(int(SR * d), 0.001, 0.07)
    write(os.path.join(SFX, "hit.wav"), x, 0.85)

    # Ölüm: alçalan arpej + gürültü
    parts = []
    for n in [64, 60, 57, 52]:
        dd = 0.11
        parts.append(osc(note(n), dd, "square", 0.5) * env(int(SR * dd), 0.003, 0.1))
    x = np.concatenate(parts)
    x = x + 0.5 * lowpass(noise(len(x) / SR), 0.2) * np.linspace(0.8, 0, len(x))
    write(os.path.join(SFX, "death.wav"), x, 0.8)

    # Powerup: yükselen parlak arpej
    parts = [osc(note(n), 0.055, "tri") * env(int(SR * 0.055), 0.002, 0.05) for n in [72, 76, 79, 84]]
    write(os.path.join(SFX, "powerup.wav"), np.concatenate(parts), 0.6)

    # Yakın geçiş: kısa yükselen fısıltı + ince blip
    d = 0.14
    x = lowpass(noise(d), 0.35) * env(int(SR * d), 0.02, 0.1) * 0.7 + 0.4 * osc(sweep(1400, 2100, d), d, "square", 0.2) * env(int(SR * d), 0.002, 0.06)
    write(os.path.join(SFX, "nearmiss.wav"), x, 0.45)

    # Dash: yükselen hava
    d = 0.18
    x = lowpass(noise(d), 0.25) * env(int(SR * d), 0.005, 0.17) + 0.4 * osc(sweep(250, 700, d), d, "tri") * env(int(SR * d), 0.003, 0.15)
    write(os.path.join(SFX, "dash.wav"), x, 0.55)

    # Göktaşı inişi: derin boom + gürültü
    d = 0.42
    x = osc(sweep(110, 38, d, 0.5), d, "sine") * env(int(SR * d), 0.002, 0.4) + 0.7 * lowpass(noise(d), 0.1) * env(int(SR * d), 0.001, 0.3)
    write(os.path.join(SFX, "meteor.wav"), x, 0.8)

    # Duvar kırılması (sık → çok kısa ve kısık)
    d = 0.07
    x = lowpass(noise(d), 0.3) * env(int(SR * d), 0.001, 0.06)
    write(os.path.join(SFX, "crumble.wav"), x, 0.25)

    # Kalkan: metalik çınlama
    d = 0.35
    t = t_(d)
    x = (np.sin(2 * np.pi * 1180 * t) + 0.5 * np.sin(2 * np.pi * 2360 * t) + 0.25 * np.sin(2 * np.pi * 3710 * t)) * np.exp(-t * 12)
    write(os.path.join(SFX, "shield.wav"), x, 0.55)

    # Kademe atlama: kısa fanfar
    parts = [osc(note(n), dd, "square", 0.5) * env(int(SR * dd), 0.003, dd * 0.9) for n, dd in [(67, 0.09), (72, 0.09), (76, 0.2)]]
    write(os.path.join(SFX, "stage.wav"), np.concatenate(parts), 0.55)

    # Olay alarmı: iki tonlu uyarı
    parts = []
    for i in range(4):
        dd = 0.12
        parts.append(osc(note(69 if i % 2 == 0 else 64), dd, "square", 0.5) * env(int(SR * dd), 0.003, 0.1))
    write(os.path.join(SFX, "event.wav"), np.concatenate(parts), 0.5)


def music(path, bpm, bars, prog, arp_pattern, lead=None, drums=True, vol=0.6):
    beat = 60.0 / bpm
    step = beat / 4  # 16'lık
    total = int(SR * step * 16 * bars)
    out = np.zeros(total)

    def add(sig, start_step):
        s = int(SR * step * start_step)
        e = min(total, s + len(sig))
        out[s:e] += sig[:e - s]

    for bar in range(bars):
        root = prog[bar % len(prog)]
        base = bar * 16
        # Bas: her vuruşta kök nota (üçgen)
        for b in range(4):
            dd = step * 3.5
            add(osc(note(root - 12), dd, "tri") * env(int(SR * dd), 0.004, dd * 0.9) * 0.55, base + b * 4)
        # Arpej: kök + minör akor
        chord = [root, root + 3, root + 7, root + 12]
        for i in range(16):
            n = chord[arp_pattern[i % len(arp_pattern)]]
            dd = step * 0.9
            add(osc(note(n + 12), dd, "square", 0.25) * env(int(SR * dd), 0.002, dd * 0.8) * 0.16, base + i)
        if lead:
            for (pos, n, length) in lead[bar % len(lead)]:
                dd = step * length
                add(osc(note(n), dd, "tri") * env(int(SR * dd), 0.01, dd * 0.9) * 0.28, base + pos)
        if drums:
            for b in range(4):
                kd = 0.12
                add(osc(sweep(120, 45, kd), kd, "sine") * env(int(SR * kd), 0.001, 0.11) * 0.6, base + b * 4)
            for h in range(8):
                hd = 0.03
                add(noise(hd) * env(int(SR * hd), 0.001, 0.028) * 0.12, base + h * 2 + 1)
            for sn in (4, 12):
                sd = 0.1
                add(lowpass(noise(sd), 0.5) * env(int(SR * sd), 0.001, 0.09) * 0.25, base + sn)
    # Döngü dikişi: son 20 ms yumuşak
    write(path, out, vol)


def main():
    print("Efektler:")
    sfx()
    print("Müzik:")
    # Menü: sakin, davulsuz, La minör – Fa – Do – Sol
    music(os.path.join(MUS, "menu_loop.wav"), 96, 8, [57, 53, 60, 55], [0, 1, 2, 3, 2, 1, 0, 2],
          lead=[[(0, 69, 6), (8, 72, 6)], [(0, 69, 4), (6, 67, 6)], [(0, 72, 6), (8, 76, 6)], [(0, 74, 12)]],
          drums=False, vol=0.5)
    # Oyun: tempolu, davullu
    music(os.path.join(MUS, "game_loop.wav"), 138, 8, [57, 57, 53, 55], [0, 2, 1, 3, 0, 2, 3, 2],
          lead=[[(0, 76, 3), (4, 74, 3), (8, 72, 3), (12, 74, 3)], [(0, 76, 6), (8, 79, 6)],
                [(0, 77, 3), (4, 76, 3), (8, 74, 6)], [(0, 71, 6), (8, 74, 6)]],
          drums=True, vol=0.55)


if __name__ == "__main__":
    main()
