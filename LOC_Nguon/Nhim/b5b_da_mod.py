import numpy as np, math
from PIL import Image, ImageFilter

def noise(w, h, s, oct=4, seed=None):
    r = np.random.default_rng(seed) if seed is not None else rng
    a = np.zeros((h, w))
    for o in range(oct):
        f = 2 ** o; sw, sh = max(2, int(w / s * f)), max(2, int(h / s * f))
        a += np.asarray(Image.fromarray((r.random((sh, sw)) * 255).astype(np.uint8)).resize((w, h), Image.BICUBIC), float) / 255 / f
    return a / sum(1 / 2 ** o for o in range(oct))

def save(a, name, flip=True):
    """a: (H,W,3|4) 0..1, hàng 0 = đáy (theo bake) → lật cho ảnh PNG"""
    a = np.clip(a, 0, 1)
    if flip: a = a[::-1]
    Image.fromarray((a * 255 + 0.5).astype(np.uint8)).save(f"{TX}/{name}.png")

def mix(base, col, m):
    m = np.clip(m, 0, 1)[..., None]; return base * (1 - m) + np.asarray(col) * m

def gauss(p, c, s):
    d = (p - np.asarray(c)) / np.asarray(s); return np.exp(-0.5 * (d ** 2).sum(-1))

N1 = noise(S, S, 60, seed=1); N2 = noise(S, S, 8, seed=2); N3 = noise(S, S, 3, 2, seed=3)
EYE = [(0.0278, -0.0914, 1.0989), (-0.0278, -0.0914, 1.0989)]
MZ = 1.043   # đường miệng
front = (y < -0.06) & (z > 0.98)

def da(f):
    base = np.array([0.88, 0.71, 0.58])
    c = np.broadcast_to(base, p.shape).copy()
    c *= (0.94 + 0.10 * N1[..., None]) * (0.985 + 0.03 * N2[..., None])
    # xanh xao theo sốt (rút sắc, ngả xám vàng)
    g = c.mean(-1, keepdims=True); c = c * (1 - 0.18 * f) + g * 0.18 * f
    c = c * (1 - 0.05 * f) + np.array([0.80, 0.76, 0.62]) * 0.05 * f
    # lòng bàn tay/bàn chân nhạt hơn, gan bàn chân hơi bẩn
    c = mix(c, (0.80, 0.66, 0.56), (z < 0.025) * (n[..., 2] < -0.3) * 0.5)
    # má hồng trẻ con + đỏ vì sốt
    for s_ in (1, -1):
        c = mix(c, (0.93, 0.47, 0.44), gauss(p, (0.040 * s_, -0.095, 1.066), (0.016, 0.03, 0.013)) * (0.16 + 0.55 * f) * front)
        # quầng thâm dưới mắt
        c = mix(c, (0.56, 0.42, 0.46), gauss(p, (0.028 * s_, -0.098, 1.087), (0.012, 0.03, 0.0045)) * (0.10 + 0.40 * f) * front)
        # mí đỏ
        c = mix(c, (0.86, 0.50, 0.50), gauss(p, (0.028 * s_, -0.10, 1.099), (0.014, 0.03, 0.008)) * 0.30 * f * front)
        # tai hồng
        c = mix(c, (0.90, 0.55, 0.52), gauss(p, (0.074 * s_, -0.03, 1.095), (0.012, 0.02, 0.02)) * (0.2 + 0.3 * f))
    c = mix(c, (0.92, 0.50, 0.48), gauss(p, (0, -0.128, 1.068), (0.008, 0.02, 0.006)) * (0.10 + 0.35 * f) * front)
    if MOI:
        # [22/10] môi vẽ khớp lưới thật: khe miệng z 1,0415, khoé ±16,8 mm (đo từ lưới); viền mềm, không viền cứng
        MZ2, XC = 1.0415, 0.0170
        ax = np.clip(np.abs(x) / XC, 0, 1.5)
        hu = 0.0066 * np.clip(1 - ax ** 2, 0, 1) ** 0.6 - 0.0009 * np.exp(-(x / 0.0024) ** 2)   # môi trên + rãnh nhân trung
        hl = 0.0106 * np.clip(1 - ax ** 2.6, 0, 1) ** 0.42                                    # môi dưới
        du = z - MZ2
        dv = np.where(du >= 0, du / np.maximum(hu, 1e-5), -du / np.maximum(hl, 1e-5))          # 0 khe → 1 viền
        fr = (y < -0.100) & (np.abs(x) < XC * 1.1) & (z > 1.025) & (z < 1.052)
        lipm = np.clip((1.0 - dv) / 0.22, 0, 1) * fr                                            # mép mờ dần
        lipm = lipm * np.clip((1.04 - ax) / 0.10, 0, 1) ** 0.7                                    # nhạt dần về khoé
        lipc = np.array([0.78, 0.47, 0.46]) * (1 - f * 0.45) + np.array([0.80, 0.62, 0.58]) * f * 0.45
        lipc = np.broadcast_to(lipc, c.shape) * np.where(du >= 0, 0.93, 1.0)[..., None]       # môi trên sẫm hơn chút
        c = mix(c, lipc, lipm * (0.82 - 0.24 * f))
        c = mix(c, (0.90, 0.70, 0.66), np.exp(-((x / 0.006) ** 2 + ((du + 0.0050) / 0.0020) ** 2)) * fr * 0.18)   # bóng nhẹ môi dưới
        # khô nứt theo sốt: vài vạch dọc mảnh, không mảng trắng
        vach = (np.abs(np.sin(x / 0.0011 + N2 * 4)) > 0.97) * (du < -0.001) * (du > -0.0065)
        c = mix(c, (0.86, 0.74, 0.70), vach * lipm * 0.45 * f)
        # khe miệng: sẫm ở giữa, nhạt dần về khoé, khoé hơi lõm tối
        c = mix(c, (0.40, 0.22, 0.22), np.clip(1 - np.abs(du) / 0.0007, 0, 1) * np.clip((1 - ax) / 0.25, 0, 1) * fr * 0.7)
        c = mix(c, (0.62, 0.44, 0.40), np.exp(-(((np.abs(x) - XC) / 0.0012) ** 2 + (du / 0.0012) ** 2)) * (y < -0.09) * 0.35)
    else:
        # môi: nhợt và khô dần
        dx = x / 0.0205
        up = (z - MZ) / 0.0085; lo = (MZ - z) / 0.0105
        lip = ((dx ** 2 + np.where(z >= MZ, up, lo) ** 2) < 1) & (y < -0.104) & (z > 1.02) & (z < 1.06)
        lipm = np.asarray(Image.fromarray((lip * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(1.5)), float) / 255
        lipc = np.array([0.80, 0.50, 0.49]) * (1 - f * 0.4) + np.array([0.82, 0.64, 0.60]) * f * 0.4
        c = mix(c, lipc, lipm * (0.85 - 0.25 * f))
        c = mix(c, (0.93, 0.85, 0.80), lipm * (N3 > 0.62) * 0.35 * f)          # nứt nẻ
        c = mix(c, (0.42, 0.24, 0.24), (np.abs(z - MZ) < 0.0011) * (np.abs(x) < 0.0195) * (y < -0.10))
    # lông mày thưa, nâu đen
    for s_, (ex, ey, ez) in zip((1, -1), EYE):
        t = (s_ * x - 0.010) / 0.036
        zc = 1.1185 + 0.0055 * np.sin(np.pi * np.clip(t, 0, 1) * 0.85) - 0.0015 * t
        th = 0.0024 * (1 - 0.55 * np.clip(t, 0, 1))
        m = (t > 0) & (t < 1) & (np.abs(z - zc) < th) & (y < -0.08)
        m = m * (0.55 + 0.45 * (N3 > 0.45))
        c = mix(c, (0.24, 0.17, 0.13), m * 0.7)
        # [22/10] viền mi bám đúng mép lỗ mắt thật (đo từ lưới), mềm, dày dần về đuôi mắt
        if MOI:
            k = 0 if s_ > 0 else 1
            xm = x * s_
            zu_ = np.interp(xm, CX[k], CU[k], left=np.nan, right=np.nan)
            zl_ = np.interp(xm, CX[k], CL[k], left=np.nan, right=np.nan)
            t2 = np.clip((xm - CX[k][0]) / (CX[k][-1] - CX[k][0]), 0, 1)          # 0 khoé trong → 1 đuôi mắt
            sk = (y < -0.093) & ~HID & np.isfinite(zu_)
            w = 0.0010 + 0.0012 * t2 ** 1.5                                       # dày dần ra đuôi
            du = z - zu_
            m_up = np.clip(1 - np.abs(du - w * 0.45) / (w * 0.75), 0, 1) * np.clip(np.sin(np.pi * np.clip(t2 * 1.08, 0, 1)) * 3, 0, 1)
            c = mix(c, (0.07, 0.05, 0.05), np.nan_to_num(m_up * sk) * 0.97)
            # bóng mí trên (rất nhạt, ngay trên viền mi)
            c = mix(c, (0.62, 0.46, 0.42), np.nan_to_num(np.clip(1 - (du - w) / 0.0028, 0, 1) * (du > w) * sk) * 0.18)
            # đuôi mắt: vệt mi kéo ra 1,2 mm
            xe = CX[k][-1]; zt = np.interp(xe - 0.0005, CX[k], CU[k])
            m_t = (xm > xe - 0.0006) & (xm < xe + 0.0012) & (np.abs(z - (zt + (xm - xe) * 0.25)) < 0.00045) & (y < -0.09)
            c = mix(c, (0.09, 0.065, 0.06), m_t * 0.75)
            # mí dưới: một nét hồng nâu mờ, không viền đen
            dl = zl_ - z
            m_lo = np.clip(1 - np.abs(dl - 0.0004) / 0.0006, 0, 1) * np.clip(np.sin(np.pi * t2) * 2.5, 0, 1)
            c = mix(c, (0.62, 0.42, 0.40), np.nan_to_num(m_lo * sk) * 0.30)
            # khoé mắt trong hồng
            c = mix(c, (0.86, 0.55, 0.55), gauss(np.stack([xm, y * 0, z], -1), (CX[k][0] + 0.0006, 0, (CU[k][0] + CL[k][0]) / 2), (0.0009, 1, 0.0009)) * (y < -0.09) * 0.55)
        else:
            # viền mi trên (đậm) + mi dưới (nhạt)
            u = (x - ex) / 0.0128
            ok = (np.abs(u) < 1) & (y < -0.097) & (np.abs(z - ez) < 0.012)
            zu = ez + 0.0060 * np.sqrt(np.clip(1 - u ** 2, 0, 1)) + 0.0012 * u * s_
            zl = ez - 0.0046 * np.sqrt(np.clip(1 - u ** 2, 0, 1))
            c = mix(c, (0.10, 0.07, 0.07), ok * (z >= zu - 0.0004) * (z < zu + 0.0016) * 0.95)
            c = mix(c, (0.35, 0.25, 0.25), ok * (np.abs(z - zl) < 0.0007) * 0.35)
    # da đầu dưới tóc: nâu đen (đỡ lộ khe giữa các lọn)
    ang = np.degrees(np.arctan2(x, -(y + 0.03))); a = np.abs(ang)
    hl = np.where(a < 50, 1.165, np.where(a < 100, 1.125 - (a - 50) / 50 * 0.01, 1.07 - (a - 100) / 80 * 0.05))
    ear = (np.abs(x) > 0.066) & (z > 1.06) & (z < 1.125) & (np.abs(y + 0.03) < 0.03)
    scalp = (z > hl) & ~ear & (z > 1.0)
    sm = np.asarray(Image.fromarray((scalp * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(3)), float) / 255
    c = mix(c, (0.13, 0.10, 0.08), sm * 0.92)
    return c
