# [1/11] âm hiệu ứng tổng hợp cho LỘC: bước chân gạch, cửa gỗ kẹt, cổng sắt, công tắc, mở ổ khoá. numpy, 44,1 kHz mono 16-bit.
import numpy as np, wave
SR = 44100; rng = np.random.default_rng(11)
def ghi(ten, x):
    x = x / (np.max(np.abs(x)) + 1e-9) * 0.9
    with wave.open(ten + '.wav', 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes((x * 32767).astype('<i2').tobytes())
def t(s): return np.arange(int(SR * s)) / SR
def loc(x, a):  # lọc thông thấp một cực
    y = np.zeros_like(x); v = 0.0
    for i, s in enumerate(x): v += a * (s - v); y[i] = v
    return y
def env(n, tan): return np.exp(-np.arange(n) / (SR * tan))
# bước chân trên gạch: tiếng gót (bụp trầm) + đế dép quẹt (nhiễu lọc), 4 biến thể
for k in range(4):
    n = int(SR * 0.22); x = np.zeros(n)
    goc = rng.uniform(0.0, 0.012); i0 = int(goc * SR)
    tt = t(0.22)[: n - i0]
    bup = np.sin(2 * np.pi * rng.uniform(70, 95) * tt) * env(len(tt), 0.025)
    x[i0:] += bup * 0.9
    nz = loc(rng.standard_normal(n), 0.25) * env(n, 0.018)
    x += nz * 0.6
    j = int(SR * rng.uniform(0.05, 0.08)); m = n - j
    x[j:] += loc(rng.standard_normal(m), 0.12) * env(m, 0.03) * 0.35   # mũi chân chạm
    ghi(f'BuocChan_Gach_{k}', x)
# cửa gỗ kẹt: dao động ma sát 300–600 Hz, biên độ giật cục + đóng nhẹ
d = 0.9; tt = t(d); f = 340 + 120 * np.sin(2 * np.pi * 1.3 * tt) + 60 * rng.standard_normal(len(tt)).cumsum() / SR * 3
ph = 2 * np.pi * np.cumsum(f) / SR
gat = (np.sin(2 * np.pi * 23 * tt) > 0.2).astype(float) * 0.7 + 0.3
x = (np.sin(ph) + 0.4 * np.sin(2 * ph) + 0.2 * np.sin(3.1 * ph)) * gat * np.minimum(1, tt / 0.05) * np.exp(-tt * 1.2)
ghi('Cua_Ken', loc(x, 0.5))
# cửa đóng: bụp gỗ
tt = t(0.35); x = np.sin(2 * np.pi * 85 * tt) * env(len(tt), 0.05) + loc(rng.standard_normal(len(tt)), 0.2) * env(len(tt), 0.02) * 0.8
ghi('Cua_Dong', x)
# cổng sắt: rít kim loại (nhiều hài, lệch tần) + loảng xoảng chốt
tt = t(1.3); f0 = 620 + 90 * np.sin(2 * np.pi * 0.8 * tt)
ph = 2 * np.pi * np.cumsum(f0) / SR
x = sum(np.sin(ph * h + h) / h for h in (1, 2.03, 3.1, 4.7)) * np.minimum(1, tt / 0.08) * np.exp(-tt * 1.6) * 0.6
ck = np.zeros_like(tt); i = int(SR * 1.0); m = len(tt) - i
ck[i:] = sum(np.sin(2 * np.pi * fr * t(1.3)[:m]) for fr in (1180, 1730, 2410)) * env(m, 0.06)
ghi('Cong_Sat', x + ck * 0.8)
# công tắc: tách nhựa
tt = t(0.08); x = loc(rng.standard_normal(len(tt)), 0.6) * env(len(tt), 0.004) + np.sin(2 * np.pi * 2300 * tt) * env(len(tt), 0.006) * 0.4
x2 = np.zeros(int(SR * 0.12)); x2[:len(x)] = x; x2[int(SR * 0.012):int(SR * 0.012) + len(x)] += x * 0.5
ghi('CongTac_Tach', x2)
# mở ổ khoá treo: chìa lách cách → xoay → bật càng
n = int(SR * 1.1); x = np.zeros(n)
for s in (0.05, 0.12, 0.2, 0.26):
    i = int(SR * s); m = int(SR * 0.04); x[i:i + m] += loc(rng.standard_normal(m), 0.7) * env(m, 0.008) * 0.6
i = int(SR * 0.45); m = int(SR * 0.25); tt = t(0.25)
x[i:i + m] += loc(rng.standard_normal(m), 0.3) * 0.25 * np.sin(np.pi * tt / 0.25)
i = int(SR * 0.78); m = int(SR * 0.3); tt = t(0.3)
x[i:i + m] += sum(np.sin(2 * np.pi * fr * tt) for fr in (1650, 2900, 4100)) * env(m, 0.03) + loc(rng.standard_normal(m), 0.8) * env(m, 0.005)
ghi('OKhoa_Mo', x)
print('xong')
