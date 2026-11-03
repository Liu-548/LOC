# [03/11 tối] Texture cánh cúc cho vòng hoa / lọ hoa HQ (bản "Thực tế") — nhân với màu cánh (trắng / ngà / vàng / kem) trong LocDoMoi.
#  4 ô ngang (128 × 256 mỗi ô, u ngang cánh, v gốc dưới → mút trên):
#   0 tươi: gân dọc mờ, gốc hơi xanh vàng · 1 úa mép: viền nâu lấm tấm · 2 úa đầu: mút cánh khô nâu · 3 dập: úa mép + vết thâm
# Chạy: python3 canh_hoa_tex.py <ra.png>
import sys, numpy as np
from PIL import Image
rng = np.random.default_rng(1102)
W, H = 128, 256

def nhieu(h, w, beta, seed):
    r = np.random.default_rng(seed); fy = np.fft.fftfreq(h)[:, None]; fx = np.fft.fftfreq(w)[None, :]
    f = np.sqrt(fx * fx + fy * fy); f[0, 0] = 1
    s = (1 / f ** (beta / 2)) * np.exp(1j * r.uniform(0, 2 * np.pi, (h, w))); s[0, 0] = 0
    x = np.real(np.fft.ifft2(s)); x -= x.min(); return x / (x.max() + 1e-9)

def ss(a, b, x): t = np.clip((x - a) / (b - a), 0, 1); return t * t * (3 - 2 * t)

def o(kieu):
    v, u = np.mgrid[0:H, 0:W]; v = 1 - (v + 0.5) / H; u = (u + 0.5) / W   # v = 1 ở hàng trên cùng (mút cánh)
    n1 = nhieu(H, W, 2.2, 10 + kieu); n2 = nhieu(H, W, 1.0, 20 + kieu)
    c = np.ones((H, W, 3))
    # gân dọc: 7 gân hội tụ về gốc
    gan = np.zeros((H, W))
    for k in range(7):
        uc = 0.5 + (k - 3) / 7 * (0.25 + 0.75 * v)
        gan = np.maximum(gan, np.exp(-((u - uc) / 0.022) ** 2) * (0.6 + 0.4 * n2))
    c *= (1 - 0.045 * gan)[..., None]
    c *= (0.96 + 0.04 * n2)[..., None]
    # gốc cánh hơi xanh vàng
    goc = 1 - ss(0.0, 0.18, v)
    c *= (1 - goc[..., None] * (1 - np.array([0.90, 0.95, 0.78])))
    mep = np.minimum(u, 1 - u) * 2          # 0 ở mép, 1 ở sống cánh
    nau = np.array([0.66, 0.50, 0.31]); nau_dam = np.array([0.46, 0.33, 0.20])
    m = np.zeros((H, W))
    if kieu in (1, 3):
        m = np.maximum(m, ss(0.17, 0.0, mep + (n1 - 0.5) * 0.30) * ss(0.25, 0.7, v + (n2 - 0.5) * 0.3) * 0.9)
    if kieu == 2:
        m = np.maximum(m, ss(0.80, 0.98, v + (n1 - 0.5) * 0.22 + (1 - mep) * -0.06) * 0.95)
    if kieu == 3:
        d = np.sqrt(((u - 0.42) / 0.22) ** 2 + ((v - 0.58) / 0.12) ** 2) + (n1 - 0.5) * 0.6
        m = np.maximum(m, ss(1.0, 0.4, d) * (0.35 + 0.25 * n2))
    if kieu == 0:
        m = np.maximum(m, ss(0.08, 0.0, mep + (n1 - 0.5) * 0.15) * ss(0.6, 0.95, v) * 0.5)
    vien = m * ss(0.5, 0.95, m)              # vùng úa đậm dần ra mép
    c = c * (1 - m[..., None]) + nau[None, None] * m[..., None]
    c = c * (1 - vien[..., None] * 0.6) + nau_dam[None, None] * vien[..., None] * 0.6
    return np.clip(c, 0, 1)

img = np.concatenate([o(k) for k in range(4)], 1)
Image.fromarray((img * 255).astype(np.uint8), "RGB").save(sys.argv[1])
print("xong", sys.argv[1], img.shape)
