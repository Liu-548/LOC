# [03/11 tối] Tường / sàn chất nhà Việt cho bản "Thực tế" (thay texture lúc chạy trong LocThucTe.ThemTuongSan, tắt Thực tế là trả lại).
#  GachBong_HQ.png (+ _N) : sàn gạch bông xi măng 20 × 20 cm, 4 × 4 viên = 0,8 m (cùng lưới ron với T_GachBong_T1 → khít ron thật)
#                           4 viên ghép thành bông hoa 8 cánh ở góc chung; đỏ gạch · vàng nghệ · xanh rêu · than; màu bạc, sứt góc,
#                           xước, ron xám ngả nâu bẩn. Normal: ron lõm, mặt gạch hơi gợn.
#  TuongVoi_HQ.png        : tường quét vôi vàng 2,0 × 3,3 m (cùng tỉ lệ T_TuongVoi_O, đáy ảnh = chân tường): vệt chổi quét, lớp vôi cũ
#                           lộ ra, chân tường ẩm mốc có ngấn nước, trần ám khói nhang, vết nứt chân chim. Lặp liền hai chiều.
# Chạy: python3 tuong_san_tex.py <Assets/LOC_House/Resources/LOC_ThucTe>
import sys, os, numpy as np
from PIL import Image
OUT = sys.argv[1]; rng = np.random.default_rng(1945)

def nhieu(h, w, beta, seed, keo=(1.0, 1.0)):
    r = np.random.default_rng(seed); fy = np.fft.fftfreq(h)[:, None] * keo[0]; fx = np.fft.fftfreq(w)[None, :] * keo[1]
    f = np.sqrt(fx * fx + fy * fy); f[0, 0] = 1
    s = (1 / f ** (beta / 2)) * np.exp(1j * r.uniform(0, 2 * np.pi, (h, w))); s[0, 0] = 0
    x = np.real(np.fft.ifft2(s)); x -= x.min(); return x / (x.max() + 1e-9)

def lam_mo(x, sig):
    h, w = x.shape; fy = np.fft.fftfreq(h)[:, None]; fx = np.fft.fftfreq(w)[None, :]
    return np.real(np.fft.ifft2(np.fft.fft2(x) * np.exp(-2 * (np.pi * sig) ** 2 * (fx * fx + fy * fy))))

def ss(a, b, x): t = np.clip((x - a) / (b - a), 0, 1); return t * t * (3 - 2 * t)

def phap(hm, manh):
    gx = (np.roll(hm, -1, 1) - np.roll(hm, 1, 1)) * 0.5 * manh; gy = (np.roll(hm, -1, 0) - np.roll(hm, 1, 0)) * 0.5 * manh
    n = np.stack([-gx, gy, np.ones_like(hm)], -1); n /= np.linalg.norm(n, axis=-1, keepdims=True); return n * 0.5 + 0.5

def hex_(h): h = h.lstrip('#'); return np.array([int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)])

def luu(ten, arr):
    Image.fromarray((np.clip(arr, 0, 1) * 255 + 0.5).astype(np.uint8), "RGB").save(os.path.join(OUT, ten), optimize=True)
    print("ghi", ten, arr.shape)

def vet_nut(h, w, so, seed, dai=(40, 160), CONG=0.35):
    r = np.random.default_rng(seed); m = np.zeros((h, w))
    for _ in range(so):
        y, x = r.uniform(0, h), r.uniform(0, w); g = r.uniform(0, 2 * np.pi); L = r.integers(*dai)
        for _t in range(L):
            g += r.normal(0, CONG); y += np.sin(g); x += np.cos(g)
            m[int(y) % h, int(x) % w] = 1
            if r.random() < 0.02:   # rẽ nhánh ngắn
                yy, xx, gg = y, x, g + r.choice([-1, 1]) * 0.9
                for _k in range(r.integers(6, 30)):
                    gg += r.normal(0, 0.3); yy += np.sin(gg); xx += np.cos(gg); m[int(yy) % h, int(xx) % w] = 0.7
    return np.clip(lam_mo(m, 0.6) * 2.5, 0, 1)

# ═════════════════════════ sàn gạch bông
N_O, PX = 4, 256; S = N_O * PX
KEM, DO, VANG, XANH, THAN = hex_("#E2D6BC"), hex_("#97412F"), hex_("#C2953E"), hex_("#46705F"), hex_("#33302B")
ROn = hex_("#7B746A")

def vien_gach(seed):
    """một viên (PX × PX), góc (0,0) là tâm bông lớn của 4 viên ghép. Trả (màu, vùng sứt). Vẽ ở 2× rồi thu nhỏ (khử răng cưa)"""
    c, sut = _vien(seed, PX * 2)
    c = c.reshape(PX, 2, PX, 2, 3).mean((1, 3)); sut = sut.reshape(PX, 2, PX, 2).mean((1, 3))
    return c, sut

def _vien(seed, PX):
    r = np.random.default_rng(seed)
    v, u = (np.mgrid[0:PX, 0:PX] + 0.5) / PX
    c = np.ones((PX, PX, 3)) * KEM
    rr = np.sqrt(u * u + v * v); t = np.arctan2(v, u)
    canh = rr < 0.43 * (0.52 + 0.48 * np.abs(np.cos(4 * t)) ** 1.6)
    c[canh] = DO
    c[(rr < 0.30 * (0.5 + 0.5 * np.abs(np.cos(4 * t + np.pi / 4)) ** 2)) & canh] = VANG
    c[rr < 0.10] = XANH
    c[np.abs(rr - 0.50) < 0.012] = THAN
    # góc đối diện: bông nhỏ xanh trong vòng than
    r2 = np.sqrt((1 - u) ** 2 + (1 - v) ** 2); t2 = np.arctan2(1 - v, 1 - u)
    c[r2 < 0.20 * (0.6 + 0.4 * np.abs(np.cos(2 * t2)))] = XANH
    c[r2 < 0.06] = VANG
    c[np.abs(r2 - 0.25) < 0.010] = THAN
    # nửa hình thoi lá ở giữa hai cạnh xa (ghép với viên bên cạnh thành lá trọn)
    for (cu, cv) in ((0.5, 1.0), (1.0, 0.5)):
        d = np.abs(u - cu) / (0.10 if cu == 0.5 else 0.20) + np.abs(v - cv) / (0.20 if cu == 0.5 else 0.10)
        c[d < 1] = VANG; c[d < 0.45] = DO
    # lá trên đường chéo, giữa hai bông
    du, dv = (u + v) / np.sqrt(2), (v - u) / np.sqrt(2)
    la_ = ((du - 0.86) / 0.14) ** 2 + (dv / 0.055) ** 2 < 1
    c[la_] = XANH
    # viền mảnh dọc mép viên (khung khi ghép)
    m = np.minimum(np.minimum(u, 1 - u), np.minimum(v, 1 - v))
    c[(m > 0.025) & (m < 0.035)] = THAN
    # bạc màu + loang theo từng viên
    nhat = nhieu(PX, PX, 2.4, seed + 1); lom = nhieu(PX, PX, 0.8, seed + 2)
    c = c * (1 - 0.28 * nhat[..., None]) + KEM * 0.28 * nhat[..., None]
    c *= (0.93 + 0.10 * lom)[..., None] * r.uniform(0.94, 1.05)
    # sứt góc / mẻ cạnh: lộ xi măng xám
    sut = np.zeros((PX, PX))
    for _ in range(r.integers(0, 3)):
        gu, gv = r.choice([0.0, 1.0]), r.choice([0.0, 1.0]); R = r.uniform(0.03, 0.08)
        sut = np.maximum(sut, ss(R, R * 0.6, np.sqrt((u - gu) ** 2 + (v - gv) ** 2) + (lom - 0.5) * 0.04))
    return c, sut

def san():
    c = np.zeros((S, S, 3)); sut = np.zeros((S, S))
    for j in range(N_O):
        for i in range(N_O):
            vc, vs = vien_gach(100 + j * N_O + i)
            # lật gương cho 4 viên quanh một góc chung tạo thành bông trọn
            if i % 2 == 0: vc, vs = vc[:, ::-1], vs[:, ::-1]
            if j % 2 == 0: vc, vs = vc[::-1], vs[::-1]
            c[j * PX:(j + 1) * PX, i * PX:(i + 1) * PX] = vc; sut[j * PX:(j + 1) * PX, i * PX:(i + 1) * PX] = vs
    y, x = np.mgrid[0:S, 0:S]
    d = np.minimum(np.abs(((x + 0.5) % PX) - 0), PX - ((x + 0.5) % PX)); d = np.minimum(d, np.minimum(((y + 0.5) % PX), PX - ((y + 0.5) % PX)))
    ron = ss(3.2, 1.6, d)   # ron ~ 2,5 mm mỗi bên mép viên
    ban = nhieu(S, S, 2.0, 7); xuoc = vet_nut(S, S, 90, 8, (20, 90)) * 0.5
    c = c * (1 - sut[..., None]) + (hex_("#8E887C") * (0.9 + 0.2 * ban[..., None])) * sut[..., None]
    c = c * (1 - 0.10 * xuoc[..., None]) + 0.06 * xuoc[..., None]
    c *= (0.80 + 0.16 * ban)[..., None]                                  # bụi bẩn loang cả mặt sàn (gạch cũ ngả màu)
    c = c * (1 - ron[..., None]) + (ROn * (0.75 + 0.35 * ban[..., None])) * ron[..., None]
    cao = (1 - ron) * 0.9 - sut * 0.5 + (nhieu(S, S, 1.6, 9) - 0.5) * 0.08 - xuoc * 0.15
    return c, phap(lam_mo(cao, 0.8), 3.0)

# ═════════════════════════ tường vôi vàng
def tuong(W=1024, H=1690):
    v = 1 - (np.arange(H)[:, None] + 0.5) / H * np.ones((1, W))    # 0 = chân tường, 1 = sát trần (3,3 m)
    m_cao = v * 3.3
    voi = hex_("#E8DBB2")
    c = np.ones((H, W, 3)) * voi
    loang = nhieu(H, W, 2.6, 21); choi1 = nhieu(H, W, 1.6, 22, (6, 1)); choi2 = nhieu(H, W, 1.6, 23, (2.5, 1.5))
    c *= (0.94 + 0.08 * loang)[..., None]
    c *= (0.97 + 0.04 * (choi1 * 0.6 + choi2 * 0.4))[..., None]          # vệt chổi quét vôi
    # lớp vôi cũ lộ ra (trắng xám) với viền bong tróc
    cu = nhieu(H, W, 3.0, 24); mang = ss(0.80, 0.83, cu) * 0.7
    vien = ss(0.79, 0.805, cu) * ss(0.835, 0.81, cu)
    c = c * (1 - mang[..., None]) + hex_("#E2DCC6")[None, None] * (0.95 + 0.08 * choi1[..., None]) * mang[..., None]
    c *= (1 - 0.12 * vien)[..., None]
    # chân tường: ẩm, bẩn, ngấn nước, mốc
    sg = nhieu(H, W, 2.2, 25)
    ngan = 0.28 + (sg - 0.5) * 0.22                                       # độ cao ngấn (m) dao động
    am = ss(ngan + 0.02, ngan - 0.18, m_cao)
    c = c * (1 - 0.30 * am[..., None]) + hex_("#7D705A")[None, None] * 0.30 * am[..., None]
    vach = np.exp(-((m_cao - ngan) / 0.012) ** 2)
    c *= (1 - 0.16 * vach)[..., None]
    moc = ss(0.10, 0.0, m_cao + (nhieu(H, W, 1.2, 26) - 0.5) * 0.12) * ss(0.45, 0.75, nhieu(H, W, 1.0, 27))
    c = c * (1 - 0.55 * moc[..., None]) + hex_("#3C4234")[None, None] * 0.55 * moc[..., None]
    # sát trần ám khói (nhạt)
    khoi = ss(2.75, 3.3, m_cao + (loang - 0.5) * 0.3)
    c *= (1 - 0.10 * khoi)[..., None]
    # nứt chân chim
    nut = vet_nut(H, W, 7, 28, (160, 420), 0.10)
    c *= (1 - 0.22 * nut)[..., None]
    return c

os.makedirs(OUT, exist_ok=True)
a, n = san(); luu("GachBong_HQ.png", a); luu("GachBong_HQ_N.png", n)
luu("TuongVoi_HQ.png", tuong())
