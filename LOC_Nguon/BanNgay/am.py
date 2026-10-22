# Âm thanh tạm cho cảnh ban ngày giữa hai đêm (tổng hợp bằng numpy) — chờ thu thật. 44,1 kHz mono 16-bit.
# BanNgay_TungKinh (loa nhỏ tụng kinh + mõ) · BanNgay_NguoiVieng (người viếng rì rầm) · BanNgay_KenDam (kèn đám xa + trống)
# BanNgay_XeMay (xe máy chạy qua) · Tra_Rot (rót trà vào chén)
import numpy as np, wave, os, sys
SR = 44100; OUT = sys.argv[1] if len(sys.argv) > 1 else "out"; os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(22)


def band(x, lo, hi):
    X = np.fft.rfft(x); f = np.fft.rfftfreq(len(x), 1 / SR)
    X *= np.exp(-0.5 * (np.maximum(lo - f, 0) / (lo * 0.3 + 1)) ** 2) * np.exp(-0.5 * (np.maximum(f - hi, 0) / (hi * 0.3 + 1)) ** 2)
    return np.fft.irfft(X, len(x))


def formant(x, fs, bws, gains):
    X = np.fft.rfft(x); f = np.fft.rfftfreq(len(x), 1 / SR); H = np.zeros_like(f)
    for fc, bw, g in zip(fs, bws, gains): H += g / (1 + ((f - fc) / bw) ** 2)
    return np.fft.irfft(X * H, len(x))


def env(n, a, d, shape=2.0):
    t = np.linspace(0, 1, n); return np.minimum(t / a, 1) ** shape * np.minimum((1 - t) / d, 1) ** shape


def loop_fade(x, sec=0.25):   # nối vòng lặp: trộn đuôi vào đầu
    n = int(sec * SR); w = np.linspace(0, 1, n)
    x[:n] = x[:n] * w + x[-n:] * (1 - w); return x[:-n]


def save(x, name, peak=0.5):
    x = x / (np.abs(x).max() + 1e-9) * peak
    with wave.open(f"{OUT}/{name}.wav", "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes((x * 32767).astype(np.int16).tobytes())


def saw(freq):   # freq: mảng tần số theo mẫu
    ph = np.cumsum(freq) / SR; return 2 * (ph % 1) - 1


def N(sec): return rng.standard_normal(int(SR * sec))


# ── loa nhỏ tụng kinh: giọng đọc đều một cao độ, mỗi tiếng một nguyên âm; mõ gõ mỗi hai tiếng; loa rè
L = 8.25; n = int(L * SR); t = np.arange(n) / SR
f0 = 165 + 6 * np.sin(2 * np.pi * 0.13 * t)
voice = np.zeros(n); syl = 0.34
VOW = [(700, 1150, 2500), (500, 900, 2400), (350, 750, 2300), (450, 1700, 2500), (650, 1050, 2550)]
for k in range(int(L / syl)):
    s = int(k * syl * SR); m = int(syl * SR * 0.92)
    if s + m > n: break
    if k % 12 == 11: continue   # ngắt hơi
    seg = saw(f0[s:s + m]) * env(m, 0.12, 0.35, 1.5)
    fs = VOW[rng.integers(0, len(VOW))]
    voice[s:s + m] += formant(seg, fs, (90, 120, 200), (1.0, 0.6, 0.25))
mo = np.zeros(n)
for k in range(0, int(L / syl), 2):
    s = int(k * syl * SR); m = int(0.12 * SR)
    if s + m > n: break
    tt = np.arange(m) / SR; mo[s:s + m] += np.sin(2 * np.pi * 780 * tt) * np.exp(-tt * 55) + 0.5 * np.sin(2 * np.pi * 1290 * tt) * np.exp(-tt * 80)
x = band(voice, 350, 3200) + 0.35 * mo
x = np.tanh(x / (np.abs(x).max() + 1e-9) * 2.2)   # loa rè
x = band(x, 400, 3500) + 0.04 * band(N(L), 1500, 6000)
save(loop_fade(x), "BanNgay_TungKinh", 0.45)

# ── người viếng rì rầm: nhiều dòng tiếng nói không rõ chữ, lẫn tiếng ghế nhựa
L = 8.25; n = int(L * SR); x = np.zeros(n)
for v in range(7):
    f0 = rng.uniform(110, 240); src = saw(np.full(n, f0) * (1 + 0.04 * np.sin(2 * np.pi * rng.uniform(0.2, 0.6) * np.arange(n) / SR)))
    am = np.zeros(n); t0 = rng.uniform(0, 1)
    while t0 < L - 0.3:
        m = int(rng.uniform(0.12, 0.3) * SR); s = int(t0 * SR); am[s:s + m] += np.hanning(m)[:len(am[s:s + m])]; t0 += rng.uniform(0.15, 0.5) + (rng.uniform(0.6, 1.8) if rng.random() < 0.15 else 0)
    x += formant(src * am, (rng.uniform(450, 750), rng.uniform(1000, 1600)), (150, 250), (1, 0.5)) * rng.uniform(0.4, 1)
for _ in range(3):   # ghế nhựa cạ nền
    s = rng.integers(0, n - 9000); m = rng.integers(3000, 8000)
    x[s:s + m] += band(rng.standard_normal(m), 600, 2200) * np.hanning(m) * 3
x = band(x, 200, 2600)
save(loop_fade(x), "BanNgay_NguoiVieng", 0.35)

# ── kèn đám xa: kèn bóp giọng mũi, ngũ cung, luyến; trống đám thỉnh thoảng; vọng lại như từ đầu ngõ
L = 9.25; n = int(L * SR); t = np.arange(n) / SR
notes = [(587, 1.2), (698, 0.6), (784, 1.4), (698, 0.5), (587, 0.9), (523, 1.6), (587, 0.8), (440, 1.8)]
f = np.zeros(n); s = 0
for i, (hz, d) in enumerate(notes):
    m = int(d * SR); nxt = notes[(i + 1) % len(notes)][0]
    seg = np.full(m, float(hz)); k = int(min(0.15, d / 3) * SR)
    seg[-k:] = np.linspace(hz, nxt, k)   # luyến sang nốt sau
    f[s:s + m] = seg[:len(f[s:s + m])]; s += m
f[s:] = notes[0][0]
vib = 1 + 0.012 * np.sin(2 * np.pi * 5.6 * t) * np.minimum(t % 1.3 / 0.4, 1)
ken = saw(f * vib) + 0.5 * saw(f * vib * 2.002)
ken = formant(ken, (1300, 2600, 3500), (300, 400, 600), (1, 0.7, 0.4)) * (0.8 + 0.2 * np.sin(2 * np.pi * 0.3 * t))
trong = np.zeros(n)
for k in np.arange(0.4, L - 0.6, 2.3):
    s = int(k * SR); m = int(0.5 * SR); tt = np.arange(m) / SR
    trong[s:s + m] += np.sin(2 * np.pi * (95 - 30 * tt) * tt) * np.exp(-tt * 9)
x = band(ken, 300, 2300) + 0.8 * trong
d = int(0.083 * SR); y = x.copy(); y[d:] += 0.45 * x[:-d]; d2 = int(0.19 * SR); y[d2:] += 0.3 * x[:-d2]   # vọng
save(loop_fade(y), "BanNgay_KenDam", 0.4)

# ── xe máy chạy qua ngoài đường (trái → phải, Doppler)
L = 4.0; n = int(L * SR); t = np.arange(n) / SR
pos = (t - 1.8) * 9.0; dist = np.sqrt(pos ** 2 + 6 ** 2); dop = 1 - np.gradient(dist, 1 / SR) / 340
fe = 42 * dop * (1 + 0.03 * np.sin(2 * np.pi * 0.7 * t))
eng = sum(saw(fe * h) / h for h in (1, 2, 3)) + 0.4 * band(N(L), 200, 1200)
x = band(eng, 60, 1800) * (6 / dist) ** 1.4 + 0.02 * band(N(L), 300, 3000)
save(x * env(n, 0.05, 0.05, 1), "BanNgay_XeMay", 0.45)

# ── rót trà: tia nước rơi + tiếng róc rách cao dần khi chén đầy
L = 2.2; n = int(L * SR); t = np.arange(n) / SR
x = band(N(L), 1200, 5000) * 0.35
for k in range(70):
    s = int(rng.uniform(0.05, L - 0.15) * SR); m = int(0.035 * SR); tt = np.arange(m) / SR
    fr = (600 + 900 * s / n) * (1 + 2.5 * tt / 0.035)
    x[s:s + m] += np.sin(2 * np.pi * np.cumsum(fr) / SR) * np.exp(-tt * 90) * rng.uniform(0.3, 1)
save(x * env(n, 0.04, 0.12, 1), "Tra_Rot", 0.4)
print(sorted(os.listdir(OUT)))
