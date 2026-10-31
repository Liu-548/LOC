# Dựng lại 4 âm thanh điện thoại + đồng hồ (Resources/LOC_V33) — 31/10/2026.
# Chuông + nhấc máy + đồng hồ cắt từ bản thu CC0 trên Freesound (xem NGUON.md); tiếng rè + tút tổng hợp (tút đúng chuẩn VN 425 Hz).
# Chạy: python3 LOC_Nguon/AmThanh/tao_am_v33.py   (cần ffmpeg + numpy)
import subprocess, wave, os, numpy as np
SR = 44100
GOC = os.path.dirname(os.path.abspath(__file__))
RA = os.path.join(GOC, "..", "..", "Assets", "LOC_House", "Resources", "LOC_V33")
rng = np.random.default_rng(31102026)

def doc(ten):
    raw = subprocess.run(["ffmpeg", "-v", "error", "-i", os.path.join(GOC, "nguon", ten), "-ac", "1", "-ar", str(SR), "-f", "s16le", "-"], capture_output=True, check=True).stdout
    return np.frombuffer(raw, np.int16) / 32768.0

def ghi(ten, x, dinh_db=-3.0):
    x = x * (10 ** (dinh_db / 20) / max(1e-9, np.abs(x).max()))
    with wave.open(os.path.join(RA, ten), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((np.clip(x, -1, 1) * 32767).astype(np.int16).tobytes())
    print(ten, f"{len(x)/SR:.2f}s")

def s(t): return int(round(t * SR))
def mo(n, a=0.0, b=0.0):  # fade vào a giây, ra b giây
    g = np.ones(n)
    if a: g[:s(a)] = np.linspace(0, 1, s(a))
    if b: g[-s(b):] *= np.linspace(1, 0, s(b)) ** 2
    return g
def dai(x, lo, hi):  # lọc dải bằng FFT (không cần scipy)
    X = np.fft.rfft(x); f = np.fft.rfftfreq(len(x), 1 / SR)
    X *= 1 / (1 + (lo / np.maximum(f, 1)) ** 4) / (1 + (f / hi) ** 4)
    return np.fft.irfft(X, len(x))
def dinh_dau(x, gan, cua=0.15):  # tìm điểm bắt đầu tiếng gõ gần `gan` (giây)
    a, b = s(gan - cua), s(gan + cua); e = np.abs(x[a:b])
    k = np.convolve(e, np.ones(64) / 64, "same"); i = int(np.argmax(np.diff(k)))
    return (a + i) / SR - 0.01

# 1) Chuông reo — chuông điện cơ hai chuông (WE 500). Nhịp VN: reo 1 s, nghỉ 4 s → clip 5 s, code lặp theo độ dài clip.
x = doc("456433_3902754-hq.mp3")
reo = x[s(7.25):s(8.30)].copy()
duoi = x[s(9.30):s(10.6)].copy()            # tiếng chuông tự ngân khi ngắt dòng
cf = s(0.03); reo[-cf:] *= np.linspace(1, 0, cf); duoi[:cf] *= np.linspace(0, 1, cf)
mot = np.concatenate([reo[:-cf], reo[-cf:] + duoi[:cf], duoi[cf:]]) * mo(len(reo) + len(duoi) - cf, 0.004, 0.4)
out = np.zeros(s(5.0)); out[:len(mot)] = mot
ghi("DienThoai_Reo.wav", out)

# 2) Tiếng rè — nhấc ống nghe (bản thu) rồi đường dây không ai nói: rè dải thoại 300–3400 Hz, ù 50 Hz, lách tách
nhac = doc("616840_97550-hq.mp3")[s(0.05):s(0.85)] * mo(s(0.8), 0, 0.15)
n = s(4.0); t = np.arange(n) / SR
re = dai(rng.standard_normal(n), 300, 3400) * 0.05 * (1 + 0.35 * np.sin(2 * np.pi * 0.7 * t + 1.3)) * (1 + 0.2 * dai(rng.standard_normal(n), 0.1, 3) * 40)
um = sum(a * np.sin(2 * np.pi * 50 * k * t) for k, a in [(1, 0.004), (3, 0.010), (5, 0.006), (7, 0.003)])
lach = np.zeros(n)
for i in rng.choice(n - 400, 28, replace=False):
    lach[i:i + 400] += rng.standard_normal(400) * np.exp(-np.arange(400) / 60) * rng.uniform(0.05, 0.25)
dong = (re + um + dai(lach, 400, 5000)) * mo(n, 0.25, 0.3)
out = np.zeros(n); out[:len(nhac)] += nhac * 0.9; out[s(0.35):] += dong[:n - s(0.35)] * 2.0
ghi("DienThoai_Re.wav", out, -6.0)

# 3) Tút — đầu kia gác máy: cạch, rồi tín hiệu bận VN 425 Hz, 0,5 s kêu / 0,5 s nghỉ ×3, qua loa ống nghe
n = s(3.0); t = np.arange(n) / SR
nhip = ((t % 1.0) < 0.5).astype(float); nhip = np.convolve(nhip, np.ones(s(0.008)) / s(0.008), "same")
tut = np.sin(2 * np.pi * 425 * t); tut = np.tanh(2.2 * tut) / np.tanh(2.2)      # méo nhẹ của loa ống nghe
dong = dai(tut * nhip * 0.5 + rng.standard_normal(n) * 0.006, 300, 3400)
cach = np.zeros(n); k = s(0.05); cach[:k] = rng.standard_normal(k) * np.exp(-np.arange(k) / 200) * 0.6
out = np.zeros(n); out[s(0.0):] += dai(cach, 200, 3000); out[s(0.25):] += dong[:n - s(0.25)]
ghi("DienThoai_Tut.wav", out * mo(n, 0, 0.05), -10.0)

# 4) Đồng hồ gõ — tiếng thứ 12 (tiếng cuối, không có tiếng sau đè) của đồng hồ quả lắc treo tường
x = doc("658589_11673893-hq.mp3")
a = dinh_dau(x, 26.62)
mot = x[s(a):s(a + 2.8)] * mo(s(2.8), 0.003, 0.8)
ghi("DongHo_Go.wav", mot)
