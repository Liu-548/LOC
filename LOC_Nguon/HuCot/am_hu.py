# âm thanh cảnh hé hũ: kéo hũ sành trên nền xi măng, xé giấy niêm, sột soạt giấy dầu
import numpy as np, wave
SR = 44100; rng = np.random.default_rng(5)
def luu(ten, x):
    x = x / (np.abs(x).max() + 1e-9) * 0.85
    with wave.open(f"out/{ten}.wav", "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes((x * 32767).astype(np.int16).tobytes())
def lp(x, a): 
    y = np.zeros_like(x); s = 0
    for i in range(len(x)): s += a * (x[i] - s); y[i] = s
    return y
def bp(x, lo, hi): return lp(x, hi) - lp(x, lo)
# kéo hũ: 3 nhịp giật cục, sạn lạo xạo (xung ngẫu nhiên) + rền trầm của sành
n = int(SR * 1.3); t = np.arange(n) / SR
env = np.zeros(n)
for a, b in [(0.0, 0.32), (0.45, 0.8), (0.95, 1.22)]:
    m = (t >= a) & (t < b); env[m] = np.sin(np.pi * (t[m] - a) / (b - a)) ** 0.6
grit = (rng.random(n) < 0.02) * rng.normal(0, 1, n)
keo = bp(rng.normal(0, 1, n), 0.01, 0.25) * 0.6 + bp(grit, 0.05, 0.6) * 1.4
keo += np.sin(2 * np.pi * 190 * t) * 0.08 * bp(rng.normal(0, 1, n), 0.001, 0.02) * 30
luu("Hu_KeoHu", keo * env)
# xé giấy: chuỗi tiếng rách ngắn dày dần rồi dứt
n = int(SR * 0.7); t = np.arange(n) / SR
ev = np.zeros(n)
for k in range(260):
    p = rng.beta(2, 1.3) * 0.62; i = int(p * SR); L = rng.integers(40, 260)
    ev[i:i + L] += rng.normal(0, 1, min(L, n - i)) * np.exp(-np.arange(min(L, n - i)) / (L / 4)) * rng.uniform(0.3, 1)
xe = bp(ev, 0.08, 0.9) + bp(rng.normal(0, 1, n), 0.2, 0.9) * 0.05 * (t < 0.62)
luu("Hu_XeGiay", xe)
# sột soạt giấy dầu lật lên
n = int(SR * 0.9); t = np.arange(n) / SR
cr = (rng.random(n) < 0.006) * rng.normal(0, 1, n)
so = bp(cr, 0.1, 0.8) * 1.5 + bp(rng.normal(0, 1, n), 0.05, 0.4) * 0.15
luu("Hu_SotSoat", so * np.sin(np.pi * t / 0.9) ** 0.5)
print("ok")
