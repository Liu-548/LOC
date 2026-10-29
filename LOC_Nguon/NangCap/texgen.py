# Bộ texture PBR dùng chung theo lớp chất liệu (tileable, 1024²) cho LocChiTiet:
#   D2_<lop>_A.png  albedo chi tiết (nhân với màu gốc, trung bình ~0,9 để giữ màu)
#   D2_<lop>_N.png  normal map (tangent space, OpenGL +Y)
#   D2_<lop>_M.png  mask URP Lit: R metallic · G occlusion · A smoothness (nhân _Smoothness)
import numpy as np, os, sys
from PIL import Image
N = 1024
rng = np.random.default_rng(1997)
OUT = sys.argv[1] if len(sys.argv) > 1 else "tex"
os.makedirs(OUT, exist_ok=True)

def noise(scale, aniso=(1, 1), seed=None, n=N):
    """nhiễu tuần hoàn (tileable) lọc tần số; scale ~ số 'đốm' trên một cạnh."""
    r = np.random.default_rng(seed) if seed is not None else rng
    w = r.standard_normal((n, n))
    F = np.fft.fft2(w)
    fy = np.fft.fftfreq(n)[:, None] * n / aniso[1]; fx = np.fft.fftfreq(n)[None, :] * n / aniso[0]
    f = np.sqrt(fx ** 2 + fy ** 2)
    filt = np.exp(-(f / scale) ** 2)
    x = np.real(np.fft.ifft2(F * filt))
    x = (x - x.mean()) / (x.std() + 1e-9)
    return x

def fbm(oct, base, aniso=(1, 1), g=0.5):
    s = np.zeros((N, N)); a = 1; t = 0
    for i in range(oct):
        s += a * noise(base * 2 ** i, aniso); t += a; a *= g
    return s / t

def norm01(x):
    return (x - x.min()) / (x.max() - x.min() + 1e-9)

def nmap(h, k):
    gy, gx = np.gradient(h)
    # gradient tuần hoàn
    gx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) / 2; gy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) / 2
    nx, ny, nz = -gx * k, gy * k, np.ones_like(h)
    l = np.sqrt(nx ** 2 + ny ** 2 + nz ** 2)
    return np.stack([nx / l, ny / l, nz / l], -1) * 0.5 + 0.5

def save(lop, alb, h, k, metal, ao, smooth):
    a = np.clip(alb, 0, 1)
    if a.ndim == 2: a = np.stack([a] * 3, -1)
    Image.fromarray((a * 255).astype(np.uint8)).save(f"{OUT}/D2_{lop}_A.png")
    Image.fromarray((nmap(h, k) * 255).astype(np.uint8)).save(f"{OUT}/D2_{lop}_N.png")
    m = np.zeros((N, N, 4)); m[..., 0] = metal; m[..., 1] = np.clip(ao, 0, 1); m[..., 2] = 0; m[..., 3] = np.clip(smooth, 0, 1)
    Image.fromarray((m * 255).astype(np.uint8), "RGBA").save(f"{OUT}/D2_{lop}_M.png")
    print("xong", lop)

yy, xx = np.mgrid[0:N, 0:N] / N

def go(lop="go", varnish=True):
    # vân gỗ: vòng năm kéo dài theo U (dọc thớ), méo bằng nhiễu
    d = fbm(3, 2, aniso=(3, 1)) * 0.55 + noise(3, aniso=(2, 1)) * 0.3
    ring = np.sin((xx * 8 + d) * 2 * np.pi)
    ring = 0.5 + 0.5 * np.sign(ring) * np.abs(ring) ** 0.6
    pore = noise(300, aniso=(25, 1)); fleck = fbm(2, 30, aniso=(12, 1))
    alb = 0.82 + 0.13 * ring - 0.05 * np.clip(pore, 0, None) + 0.05 * fleck
    h = 0.4 * ring + 0.25 * pore * 0.3 + 0.1 * fleck
    wear = norm01(fbm(3, 4))
    sm = (0.95 if varnish else 0.7) - 0.25 * wear - 0.05 * np.clip(pore, 0, None)
    save(lop, alb, h, 1.6, 0, 1 - 0.08 * np.clip(pore, 0, None), sm)

def son(lop="son", chip=True):
    peel = noise(180) * 0.6 + noise(60) * 0.4
    dirt = norm01(fbm(4, 3))
    scr = np.clip(noise(400, aniso=(30, 1)) - 2.6, 0, None) * 2
    alb = 0.92 + 0.03 * peel - 0.10 * (dirt ** 3) - 0.08 * np.clip(scr, 0, 1)
    h = 0.15 * peel - 0.6 * np.clip(scr, 0, 1)
    sm = 0.9 - 0.3 * dirt ** 2 - 0.4 * np.clip(scr, 0, 1)
    save(lop, alb, h, 2.0, 0, 1 - 0.1 * dirt ** 3, sm)

def nhua(lop="nhua", grain=1.0):
    f = noise(300) * grain; d = norm01(fbm(3, 3))
    alb = 0.93 + 0.02 * f - 0.08 * d ** 3
    save(lop, alb, 0.25 * f, 1.0, 0, 1 - 0.05 * d ** 3, 0.85 - 0.25 * d ** 2)

def vai(lop="vai"):
    t = 160  # số sợi trên tile
    wx = np.sin(xx * t * 2 * np.pi); wy = np.sin(yy * t * 2 * np.pi)
    over = (np.sin(xx * t * np.pi) * np.sin(yy * t * np.pi) > 0)
    h = np.where(over, np.abs(wx), np.abs(wy)) * 0.6 + noise(200) * 0.15
    fz = fbm(3, 6)
    alb = 0.85 + 0.08 * h + 0.04 * fz
    save(lop, alb, h, 3.0, 0, 0.85 + 0.15 * h, 0.35 + 0.1 * h)

def carton(lop="carton"):
    fib = noise(500, aniso=(4, 1)); blot = fbm(4, 4)
    alb = 0.88 + 0.04 * fib + 0.06 * blot
    save(lop, alb, 0.3 * fib, 1.2, 0, 1, 0.2 + 0.05 * fib)

def dan(lop="dan"):
    t = 24
    u = (xx * t) % 1; v = (yy * t) % 1
    cx = (np.floor(xx * t) + np.floor(yy * t)) % 2
    strip = np.where(cx == 0, np.sin(v * np.pi), np.sin(u * np.pi))
    fib = noise(300, aniso=(1, 8))
    alb = 0.75 + 0.2 * strip + 0.04 * fib
    save(lop, alb, strip * 0.8 + 0.1 * fib, 3.0, 0, 0.7 + 0.3 * strip, 0.4 * strip)

def chieu(lop="chieu"):
    t = 90
    u = (xx * t) % 1; cx = (np.floor(xx * t) + np.floor(yy * 12)) % 2
    strip = np.sin(u * np.pi) ** 0.5
    fib = noise(400, aniso=(1, 12))
    alb = 0.82 + 0.1 * strip + 0.05 * fib - 0.04 * cx
    save(lop, alb, strip * 0.6 + 0.1 * fib, 2.0, 0, 0.8 + 0.2 * strip, 0.45 * strip)

def xm(lop="xm"):
    a = fbm(5, 8); p = np.clip(noise(350) - 1.8, 0, None)
    stain = norm01(fbm(3, 2))
    alb = 0.88 + 0.06 * a - 0.15 * p - 0.10 * stain ** 2
    save(lop, alb, 0.5 * a - 0.8 * p, 2.5, 0, 1 - 0.3 * p, 0.25 - 0.1 * stain)

def kim(lop, brushed=True, rust=0.0):
    br = noise(500, aniso=(40, 1)) if brushed else noise(200)
    d = norm01(fbm(4, 3)); scr = np.clip(noise(400, aniso=(25, 1)) - 2.5, 0, None)
    alb = 0.92 + 0.03 * br - 0.1 * d ** 3
    r = np.clip(norm01(fbm(5, 5)) - (1 - rust), 0, None) / max(rust, 1e-3) if rust > 0 else 0
    if rust > 0:
        rc = np.stack([0.55 * np.ones_like(d), 0.30 * np.ones_like(d), 0.16 * np.ones_like(d)], -1)
        a3 = np.stack([alb] * 3, -1); rr = np.clip(r, 0, 1)[..., None]
        alb = a3 * (1 - rr) + rc * rr
        metal = 1 - np.clip(r, 0, 1)
    else:
        metal = np.ones_like(d)
    h = 0.15 * br - 0.5 * scr + (0.4 * np.clip(r, 0, 1) * noise(200) if rust > 0 else 0)
    save(lop, alb, h, 1.5, metal, 1 - 0.1 * d ** 3, 0.85 - 0.3 * d ** 2 - 0.3 * np.clip(scr, 0, 1) - (0.6 * np.clip(r, 0, 1) if rust > 0 else 0))

def gom(lop="gom"):
    cr = np.clip(np.abs(noise(90)) < 0.02, 0, 1) * 1.0
    d = norm01(fbm(3, 3))
    alb = 0.95 - 0.06 * cr - 0.05 * d ** 3
    save(lop, alb, 0.05 * noise(150) - 0.3 * cr, 1.0, 0, 1, 0.92 - 0.1 * d ** 2)

def giay(lop="giay"):
    fib = noise(600); b = fbm(3, 3)
    alb = 0.93 + 0.03 * fib + 0.03 * b
    save(lop, alb, 0.2 * fib, 1.0, 0, 1, 0.15)

def da(lop="da"):
    c = noise(220) ; cell = np.abs(noise(120)) ** 0.5
    alb = 0.88 + 0.06 * cell
    save(lop, alb, 0.6 * cell + 0.1 * c, 2.0, 0, 0.9 + 0.1 * cell, 0.45 - 0.15 * cell)

def caosu(lop="caosu"):
    f = noise(400)
    save(lop, 0.92 + 0.03 * f, 0.3 * f, 1.5, 0, 1, 0.3)

def gach(lop="gach"):
    t = 5; u = (xx * t) % 1; v = (yy * t) % 1
    joint = np.minimum(np.minimum(u, 1 - u), np.minimum(v, 1 - v)) < 0.015
    a = fbm(4, 10)
    alb = np.where(joint, 0.6, 0.9 + 0.05 * a)
    save(lop, alb, np.where(joint, -0.5, 0.1 * a), 2.0, 0, np.where(joint, 0.7, 1), np.where(joint, 0.2, 0.7))

def nen(lop="nen"):
    f = fbm(3, 6)
    save(lop, 0.92 + 0.05 * f, 0.1 * f, 1.0, 0, 1, 0.6)

def thuc(lop="thuc"):
    f = fbm(4, 12); s = np.clip(noise(500) - 2, 0, None)
    save(lop, 0.9 + 0.06 * f - 0.1 * s, 0.1 * f - 0.3 * s, 1.0, 0, 1, 0.55)

go("go", True); go("goson", True); son("son"); nhua("nhua"); nhua("thung", 1.6); vai("vai"); carton("carton"); dan("dan"); chieu("chieu")
xm("xm"); kim("kl"); kim("inox", True); kim("dong", False); kim("sat", False, rust=0.35); gom("gom"); giay("giay"); da("da"); caosu("caosu")
gach("gach"); nen("nen"); thuc("thuc")
