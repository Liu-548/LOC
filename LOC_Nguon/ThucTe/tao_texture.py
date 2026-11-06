# [03/11] Sinh texture thủ tục cho bản "Thực tế" (chạy: python3 tao_texture.py <thư mục Resources/LOC_ThucTe>)
#  ChiTiet_Grunge.png : texture chi tiết lặp liền (detail albedo ×2 của URP Lit) — xám trung tính ~128 có vết loang, sạn, xước
#                       → phủ lên MỌI vật liệu đã đổi sang Lit để mặt không còn phẳng lì "như nhựa".
#  VetBan_Atlas.png   : 4×2 ô 512 px, mỗi ô một loại vết bẩn (RGB = màu vết, A = độ đậm) cho decal nhân màu (multiply):
#                       0 bẩn chân tường · 1 mốc · 2 vệt nước chảy · 3 quầng ố nước · 4 vết tay/ố · 5 nứt · 6 bụi sàn · 7 gỉ/ố nâu
import sys, os, numpy as np
from PIL import Image
rng = np.random.default_rng(7105)

def nhieu_lap(n, beta=2.0, seed=0):
    """nhiễu 1/f^beta lặp liền (FFT trên lưới tuần hoàn), chuẩn hoá 0..1"""
    r = np.random.default_rng(seed)
    fx = np.fft.fftfreq(n)[:, None]; fy = np.fft.fftfreq(n)[None, :]
    f = np.sqrt(fx * fx + fy * fy); f[0, 0] = 1
    pha = r.uniform(0, 2 * np.pi, (n, n))
    spec = (1 / f ** (beta / 2)) * np.exp(1j * pha); spec[0, 0] = 0
    x = np.real(np.fft.ifft2(spec)); x -= x.min(); x /= x.max() + 1e-9
    return x

def grunge(n=512):
    a = nhieu_lap(n, 2.2, 1); b = nhieu_lap(n, 1.2, 2); c = nhieu_lap(n, 0.3, 3)
    loang = np.clip((a - 0.55) * 3.0, 0, 1)          # vết loang tối
    san = (c - 0.5) * 0.10                              # sạn mịn
    x = 0.5 + (b - 0.5) * 0.10 - loang * 0.16 + san
    # vài vết xước mảnh
    img = x.copy()
    for _ in range(40):
        x0, y0 = rng.integers(0, n, 2); dx, dy = rng.normal(0, 1, 2); L = rng.integers(20, 120)
        for t in range(L):
            xi = int(x0 + dx * t) % n; yi = int(y0 + dy * t) % n
            img[yi, xi] -= 0.05
    v = np.clip(img, 0, 1) * 255
    return Image.fromarray(np.stack([v, v, v], -1).astype(np.uint8), "RGB")

def mep(n, le=0.12):
    """mặt nạ mờ dần về 0 ở mép ô (để decal không có cạnh vuông)"""
    t = np.linspace(0, 1, n)
    m = np.minimum(np.clip(t / le, 0, 1), np.clip((1 - t) / le, 0, 1))
    return np.outer(m, m)

def o_vet(kieu, n=512):
    y, x = np.mgrid[0:n, 0:n] / (n - 1)
    a = nhieu_lap(n, 2.0, 10 + kieu); b = nhieu_lap(n, 1.0, 20 + kieu); c = nhieu_lap(n, 0.4, 30 + kieu)
    mau = (0.42, 0.38, 0.32)
    if kieu == 0:   # bẩn chân tường: đậm ở dưới (y lớn = đáy ảnh), mép trên răng cưa
        cao = 0.35 + (a - 0.5) * 0.35
        alpha = np.clip((y - (1 - cao)) / 0.25, 0, 1) * (0.55 + 0.45 * b)
        mau = (0.40, 0.36, 0.30)
    elif kieu == 1: # mốc: chấm loang xanh đen
        d = np.sqrt((x - 0.5) ** 2 + (y - 0.6) ** 2)
        alpha = np.clip((0.45 - d) / 0.25 + (a - 0.5) * 1.2, 0, 1) * np.clip((c - 0.35) * 2.5, 0, 1)
        mau = (0.30, 0.34, 0.28)
    elif kieu == 2: # vệt nước chảy từ trên xuống
        cot = np.clip((nhieu_lap(n, 3.0, 40)[0:1, :] - 0.45) * 4, 0, 1)   # các dòng theo cột
        dai = np.clip(1 - y / (0.5 + a * 0.5), 0, 1)
        alpha = cot * dai * (0.5 + 0.5 * b)
        mau = (0.48, 0.42, 0.34)
    elif kieu == 3: # quầng ố nước (vòng có mép đậm)
        d = np.sqrt((x - 0.5) ** 2 + (y - 0.5) ** 2) + (a - 0.5) * 0.12
        vong = np.exp(-((d - 0.36) / 0.025) ** 2) * 0.9 + np.clip((0.36 - d) / 0.36, 0, 1) * 0.35
        alpha = np.clip(vong, 0, 1)
        mau = (0.55, 0.45, 0.32)
    elif kieu == 4: # vết tay / ố mờ
        d = np.sqrt((x - 0.5) ** 2 + (y - 0.5) ** 2)
        alpha = np.clip((0.42 - d) / 0.3, 0, 1) * (0.4 + 0.6 * a) * 0.7
        mau = (0.40, 0.36, 0.33)
    elif kieu == 5: # nứt: đường gấp khúc mảnh + nhánh
        img = np.zeros((n, n))
        def ve(x0, y0, goc, L, w):
            px, py = x0, y0
            for _ in range(L):
                goc += rng.normal(0, 0.35); px += np.cos(goc) * 3; py += np.sin(goc) * 3
                xi, yi = int(px), int(py)
                if 2 <= xi < n - 2 and 2 <= yi < n - 2: img[yi - w:yi + w + 1, xi - w:xi + w + 1] = 1
                if rng.random() < 0.03 and L > 20: ve(px, py, goc + rng.choice([-1, 1]) * 0.9, L // 3, max(0, w - 1))
        ve(n * 0.1, n * 0.5, 0.0, 140, 1)
        alpha = np.clip(img, 0, 1) * 0.9
        mau = (0.25, 0.23, 0.21)
    elif kieu == 6: # bụi bẩn sàn: mảng mờ to
        d = np.sqrt((x - 0.5) ** 2 + (y - 0.5) ** 2)
        alpha = np.clip((0.48 - d) / 0.35, 0, 1) * (0.3 + 0.7 * a) * 0.6
        mau = (0.55, 0.50, 0.44)
    else:           # gỉ / ố nâu
        d = np.sqrt((x - 0.5) ** 2 + (y - 0.45) ** 2)
        alpha = np.clip((0.4 - d) / 0.25 + (a - 0.5), 0, 1) * (0.5 + 0.5 * c) * 0.8
        mau = (0.50, 0.36, 0.24)
    alpha = np.clip(alpha, 0, 1) * mep(n)
    # sắc độ bên trong vết thay đổi nhẹ theo nhiễu
    k = 0.85 + 0.3 * (b[..., None] - 0.5)
    rgb = np.clip(np.array(mau)[None, None, :] * k, 0, 1)
    return np.concatenate([rgb, alpha[..., None]], -1)

def atlas():
    n = 512; A = np.zeros((2 * n, 4 * n, 4))
    for k in range(8):
        cx, cy = k % 4, k // 4
        A[cy * n:(cy + 1) * n, cx * n:(cx + 1) * n] = o_vet(k, n)
    # Unity: v = 0 ở đáy ảnh → hàng 0 (ô 0–3) đặt ở NỬA DƯỚI ảnh
    A = np.concatenate([A[n:], A[:n]], 0)
    return Image.fromarray((A * 255).astype(np.uint8), "RGBA")

if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else "."
    os.makedirs(out, exist_ok=True)
    grunge().save(os.path.join(out, "ChiTiet_Grunge.png"), optimize=True)
    atlas().save(os.path.join(out, "VetBan_Atlas.png"), optimize=True)
    print("xong")
