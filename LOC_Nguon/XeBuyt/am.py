# Âm thanh tạm cho đoạn mở đầu trên xe buýt (tổng hợp bằng numpy) — chờ thu thật. 44,1 kHz mono 16-bit.
# XeBuyt_DongCo (máy dầu nghe từ trong khoang + đường xóc + kính rung, lặp) · XeBuyt_CuaMo / XeBuyt_CuaDong (cửa hơi)
# XeBuyt_Phanh (xả hơi phanh) · Dem_CoTrung (dế + ếch ven sông, lặp) · Buoc_Bac (bước lên bậc thép bọc cao su)
# python am.py <thư mục ra>
import numpy as np, wave, os, sys
SR = 44100; OUT = sys.argv[1] if len(sys.argv) > 1 else "out"; os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(25)


def band(x, lo, hi):
    X = np.fft.rfft(x); f = np.fft.rfftfreq(len(x), 1 / SR)
    X *= np.exp(-0.5 * (np.maximum(lo - f, 0) / (lo * 0.3 + 1)) ** 2) * np.exp(-0.5 * (np.maximum(f - hi, 0) / (hi * 0.3 + 1)) ** 2)
    return np.fft.irfft(X, len(x))


def env(n, a, d, shape=2.0):
    t = np.linspace(0, 1, n); return np.minimum(t / a, 1) ** shape * np.minimum((1 - t) / d, 1) ** shape


def loop_fade(x, sec=0.3):
    n = int(sec * SR); w = np.linspace(0, 1, n)
    x[:n] = x[:n] * w + x[-n:] * (1 - w); return x[:-n]


def save(x, name, peak=0.5):
    x = x / (np.abs(x).max() + 1e-9) * peak
    with wave.open(f"{OUT}/{name}.wav", "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes((x * 32767).astype(np.int16).tobytes())


def N(sec): return rng.standard_normal(int(SR * sec))


def thump(n, f0, k): t = np.arange(n) / SR; return np.sin(2 * np.pi * f0 * t) * np.exp(-t * k)


# ── máy dầu trong khoang: nổ ~31 Hz (máy 6 xi lanh chạy chậm) + hoạ âm, ù đường, gió lùa cửa sổ hé, kính/tấm tôn rung lạch cạch
L = 6.3; n = int(L * SR); t = np.arange(n) / SR
f0 = 31 * (1 + 0.015 * np.sin(2 * np.pi * 0.21 * t))
ph = np.cumsum(f0) / SR
may = sum(np.sin(2 * np.pi * h * ph + rng.uniform(0, 6)) / h ** 0.8 for h in range(1, 9))
may *= 1 + 0.25 * np.sign(np.sin(2 * np.pi * 3 * ph))   # nhịp nổ
may = band(may, 25, 420)
duong = band(N(L), 30, 180) * 1.2
gio = band(N(L), 600, 2400) * 0.12 * (1 + 0.3 * np.sin(2 * np.pi * 0.17 * t))
lach = np.zeros(n)
for k in np.cumsum(rng.uniform(0.06, 0.5, 60)):
    if k > L - 0.05: break
    s = int(k * SR); m = int(0.03 * SR); tt = np.arange(m) / SR
    lach[s:s + m] += np.sin(2 * np.pi * rng.uniform(900, 2600) * tt) * np.exp(-tt * 140) * rng.uniform(0.2, 1)
x = may + duong + gio + 0.35 * band(lach, 700, 4000)
save(loop_fade(x), "XeBuyt_DongCo", 0.5)

# ── cửa hơi: cục "cạch" van + xì hơi + cánh gập va khung
def cua(mo):
    L = 1.4; n = int(L * SR); x = np.zeros(n)
    m = int(0.25 * SR); x[:m] += thump(m, 70, 22) * 0.9
    h0 = int(0.06 * SR); hn = int((0.75 if mo else 0.55) * SR)
    x[h0:h0 + hn] += band(rng.standard_normal(hn), 1800, 8000) * env(hn, 0.04, 0.7, 1.4) * 0.55
    c = int((1.05 if mo else 0.9) * SR); m = int(0.18 * SR)
    x[c:c + m] += (thump(m, 140, 35) + 0.4 * band(rng.standard_normal(m), 1500, 5000) * np.exp(-np.arange(m) / SR * 60))[:len(x[c:c + m])]
    return x
save(cua(True), "XeBuyt_CuaMo", 0.55)
save(cua(False), "XeBuyt_CuaDong", 0.55)

# ── xả hơi phanh "phì"
L = 1.3; n = int(L * SR); t = np.arange(n) / SR
x = band(N(L), 1400, 7500) * env(n, 0.015, 0.8, 1.2) + 0.08 * np.sin(2 * np.pi * 1850 * t) * np.exp(-t * 4)
save(x, "XeBuyt_Phanh", 0.5)

# ── dế + ếch ven sông, xa
L = 8.25; n = int(L * SR); t = np.arange(n) / SR; x = np.zeros(n)
for c in range(6):   # dế: chuỗi 3–4 xung ~4,3–5 kHz, lặp
    fc = rng.uniform(4200, 5200); per = rng.uniform(0.45, 0.9); k = rng.uniform(0, per); g = rng.uniform(0.25, 1)
    while k < L - 0.2:
        for p in range(rng.integers(3, 5)):
            s = int((k + p * 0.028) * SR); m = int(0.018 * SR); tt = np.arange(m) / SR
            x[s:s + m] += np.sin(2 * np.pi * fc * tt) * np.hanning(m) * g
        k += per * rng.uniform(0.9, 1.1)
for _ in range(9):   # ếch: "ộp" trầm, đôi khi hai tiếng
    k = rng.uniform(0, L - 0.6); f = rng.uniform(220, 380)
    for p in range(rng.integers(1, 3)):
        s = int((k + p * 0.22) * SR); m = int(0.16 * SR); tt = np.arange(m) / SR
        src = np.sign(np.sin(2 * np.pi * f * tt)) * np.sin(2 * np.pi * 28 * tt) ** 2
        x[s:s + m] += band(src, 150, 1400) * np.hanning(m) * 1.8
x = x + 0.05 * band(N(L), 200, 1200)
save(loop_fade(x), "Dem_CoTrung", 0.35)

# ── bước chân lên bậc thép bọc cao su
L = 0.4; n = int(L * SR)
x = thump(n, 95, 30) + 0.35 * band(rng.standard_normal(n), 900, 4500) * np.exp(-np.arange(n) / SR * 45)
save(x * env(n, 0.005, 0.2, 1), "Buoc_Bac", 0.6)
print(sorted(os.listdir(OUT)))
