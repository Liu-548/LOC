# Âm thanh tạm cho Nhím (tổng hợp bằng numpy) — chờ thu thật. 44,1 kHz mono 16-bit.
import numpy as np, wave, os
SR = 44100; OUT = "/home/claude/nhim/out/audio"; os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(4)

def band(x, lo, hi):
    X = np.fft.rfft(x); f = np.fft.rfftfreq(len(x), 1 / SR)
    X *= np.exp(-0.5 * (np.maximum(lo - f, 0) / (lo * 0.3 + 1)) ** 2) * np.exp(-0.5 * (np.maximum(f - hi, 0) / (hi * 0.3 + 1)) ** 2)
    return np.fft.irfft(X, len(x))
def env(n, a, d, shape=2.0):
    t = np.linspace(0, 1, n); return np.minimum(t / a, 1) ** shape * np.minimum((1 - t) / d, 1) ** shape
def save(x, name, peak=0.5):
    x = x / (np.abs(x).max() + 1e-9) * peak
    with wave.open(f"{OUT}/{name}.wav", "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes((x * 32767).astype(np.int16).tobytes())
def N(sec): return rng.standard_normal(int(SR * sec))

# thở khẽ (vòng lặp 3 s): hít 1,1 s — nghỉ — thở ra 1,3 s; có tiếng rít rất nhẹ (sốt, mũi nghẹt)
L = int(3.0 * SR); x = np.zeros(L)
hit = band(N(1.1), 500, 2200) * env(int(1.1 * SR), 0.45, 0.35)
hit += 0.25 * np.sin(2 * np.pi * 1150 * np.arange(len(hit)) / SR) * env(len(hit), 0.5, 0.4) * band(N(1.1), 20, 60) * 8
ra = band(N(1.3), 300, 1400) * env(int(1.3 * SR), 0.15, 0.6) * 0.8
x[int(0.1 * SR):int(0.1 * SR) + len(hit)] += hit; x[int(1.4 * SR):int(1.4 * SR) + len(ra)] += ra
save(x, "Nhim_Tho", 0.35)

# ho khan nhỏ (2 tiếng)
x = np.zeros(int(1.0 * SR))
for t0 in (0.05, 0.42):
    n = int(0.2 * SR); b = band(N(0.2), 300, 2600) * env(n, 0.03, 0.7, 1.5)
    b += 0.6 * np.sin(2 * np.pi * (210 - 60 * np.linspace(0, 1, n)) * np.arange(n) / SR) * env(n, 0.03, 0.6, 1.5)
    x[int(t0 * SR):int(t0 * SR) + n] += b * (1 if t0 < 0.1 else 0.7)
save(x, "Nhim_Ho", 0.45)

# chăn sột soạt
n = int(1.3 * SR); g = np.zeros(n)
for _ in range(60):
    s = rng.integers(0, n - 4000); ln = rng.integers(800, 4000); g[s:s + ln] += rng.random() * np.hanning(ln)
save(band(N(1.3), 1800, 7000) * g * env(n, 0.1, 0.3), "Chan_SotSoat", 0.3)

# vắt khăn: nước nhỏ giọt xuống chậu + róc rách
n = int(2.2 * SR); x = band(N(2.2), 600, 3000) * env(n, 0.05, 0.6) * 0.25
for k in range(26):
    t0 = 0.1 + k * 0.07 + rng.random() * 0.05; m = int(0.05 * SR); t = np.arange(m) / SR
    fr = 700 + 900 * t / 0.05 + rng.random() * 300
    s = int(t0 * SR); x[s:s + m] += np.sin(2 * np.pi * np.cumsum(fr) / SR) * np.exp(-t * 70) * (1 - k / 30)
save(x, "Khan_Vat", 0.45)

# điện chập (cảnh Bad): ù 100 Hz + lách tách
n = int(0.7 * SR); t = np.arange(n) / SR
x = sum(np.sin(2 * np.pi * 100 * h * t) / h for h in (1, 2, 3, 5, 7)) * 0.4
x *= (rng.random(n) > 0.0) * (0.5 + 0.5 * (np.sin(2 * np.pi * 9 * t) > 0))
for _ in range(14):
    s = rng.integers(0, n - 300); x[s:s + 300] += rng.standard_normal(300) * 2.5 * np.hanning(300)
save(x * env(n, 0.02, 0.2), "Dien_Chap", 0.5)
print(sorted(os.listdir(OUT)))

# Khôi thở gấp (đoạn hoảng loạn Đêm 2): 6 s, nhịp nhanh dần chậm lại
L = int(6.0 * SR); x = np.zeros(L); t = 0.0; k = 0
while t < 5.6:
    per = 0.55 + 0.45 * (t / 6.0)
    for d, lo, hi, a in ((per * 0.42, 600, 2600, 1.0), (per * 0.5, 300, 1500, 0.8)):
        n = int(d * SR); s = int(t * SR)
        if s + n < L: x[s:s + n] += band(N(d), lo, hi) * env(n, 0.3, 0.4) * a * (1.2 - 0.6 * t / 6)
        t += d
save(x, "Khoi_Tho", 0.55)
print("Khoi_Tho")
