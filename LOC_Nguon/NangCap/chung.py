# Mẫu dựng dùng lại nhiều lần (đồ gỗ, khung tranh, chai lọ, bát chén, vải rủ…)
from lib import *

def chan_thop(x, y, z0, h, a_tren, a_duoi, M, b=0.004):
    """Chân vuông thóp dần xuống: tâm (x,y), đáy z0, cao h, cạnh trên/dưới."""
    bm = bmesh.new()
    vs = []
    for zz, a in ((0, a_duoi), (h, a_tren)):
        vs.append([bm.verts.new((x + sx * a / 2, y + sy * a / 2, z0 + zz)) for sx, sy in ((-1, -1), (1, -1), (1, 1), (-1, 1))])
    d, t = vs
    for i in range(4): bm.faces.new((d[i], d[(i + 1) % 4], t[(i + 1) % 4], t[i]))
    bm.faces.new(d[::-1]); bm.faces.new(t)
    o = mesh_bm(bm, "chan", M); bevel(o, b, 2); return o

def thanh(p0, p1, a, M, b=0.003, day=None):
    """Thanh gỗ tiết diện vuông a (hoặc a×day) nối p0→p1."""
    p0, p1 = Vector(p0), Vector(p1); d = p1 - p0; L = d.length
    o = box((a, day or a, L), (0, 0, 0), M, b=b)
    q = d.to_track_quat('Z', 'Y'); o.rotation_euler = q.to_euler(); o.location = (p0 + p1) / 2
    return o

def ong(p0, p1, r, M, n=12):
    p0, p1 = Vector(p0), Vector(p1); d = p1 - p0
    o = cyl(r, d.length, (0, 0, 0), M, n=n, b=0)
    o.rotation_euler = d.to_track_quat('Z', 'Y').to_euler(); o.location = p0
    return o

def khung_anh(cx, y0, zc, w, h, vien, day, Mk, Manh=None, Mkinh=None, roi=0.012, nhin=-1):
    """Khung ảnh/tranh treo tường: mặt trước hướng −Y (nhin=-1). y0 = mặt sau sát tường."""
    s = nhin
    yk = y0 + s * day / 2
    # bốn thanh khung có gờ (hai lớp)
    for (w2, h2, dd, k) in ((w, h, day, 1.0), (w - vien * 0.9, h - vien * 0.9, day * 0.4, 0.5)):
        pass
    t = []
    t.append(box((w, day, vien), (cx, yk, zc + h / 2 - vien / 2), Mk, b=0.003))
    t.append(box((w, day, vien), (cx, yk, zc - h / 2 + vien / 2), Mk, b=0.003))
    t.append(box((vien, day, h - 2 * vien), (cx - w / 2 + vien / 2, yk, zc), Mk, b=0.003))
    t.append(box((vien, day, h - 2 * vien), (cx + w / 2 - vien / 2, yk, zc), Mk, b=0.003))
    # gờ trong
    g = vien * 0.25
    t.append(box((w - 2 * vien + 2 * g, day * 0.5, g), (cx, yk + s * day * 0.4, zc + h / 2 - vien), Mk, b=0.001))
    t.append(box((w - 2 * vien + 2 * g, day * 0.5, g), (cx, yk + s * day * 0.4, zc - h / 2 + vien), Mk, b=0.001))
    # nền sau
    box((w - 0.004, 0.004, h - 0.004), (cx, y0 + s * 0.002, zc), mat("carton", "#6B5A44", ten="lung_khung"), b=0)
    if Manh:
        plane(w - 2 * vien + 0.006, h - 2 * vien + 0.006, (cx, y0 + s * (day * 0.35), zc), Manh)
    if Mkinh:
        plane(w - 2 * vien + 0.01, h - 2 * vien + 0.01, (cx, y0 + s * (day * 0.6), zc), Mkinh, name="kinh")
    return t

def chai(prof_r, h, loc, M, n=24, nap=None, Mnap=None):
    """Chai tiện: prof_r = [(tỉ lệ cao 0..1, bán kính)…]."""
    pr = [(0, 0)] + [(r, t * h) for t, r in prof_r] + [(0, h)]
    o = lathe(pr, loc, M, n=n)
    if nap:
        cyl(nap[0], nap[1], (loc[0], loc[1], loc[2] + h - 0.002), Mnap or M, n=16, b=0.001)
    return o

def bat(r, h, loc, M, day=0.003, n=32, chan=0.35, mieng=1.0):
    """Bát/chén sứ: thành cong, có chân đế, miệng bo."""
    rc = r * chan
    pr = [(0, 0.0), (rc, 0.0), (rc, h * 0.08), (rc * 0.92, h * 0.10), (r * 0.62, h * 0.28), (r * 0.88, h * 0.62), (r * mieng, h),
          (r * mieng - day, h), (r * 0.86 - day, h * 0.62), (r * 0.6 - day, h * 0.30), (rc * 0.8, h * 0.14), (0, h * 0.14)]
    return lathe(pr, loc, M, n=n)

def dia(r, h, loc, M, n=32):
    pr = [(0, 0), (r * 0.55, 0), (r * 0.55, h * 0.3), (r * 0.62, h * 0.35), (r * 0.95, h * 0.85), (r, h), (r - 0.004, h), (r * 0.9, h * 0.8), (r * 0.6, h * 0.55), (0, h * 0.55)]
    return lathe(pr, loc, M, n=n)

def coc(r, h, loc, M, day=0.0025, n=24, thon=0.85):
    pr = [(0, 0), (r * thon, 0), (r, h), (r - day, h), (r * thon - day, day * 2), (0, day * 2)]
    return lathe(pr, loc, M, n=n)

def khan_phu(w, d, top_z, cx, cy, ru, M, nx=36, ny=26, song=0.006, ren_mep=True):
    """Khăn phủ mặt bàn/nóc tủ: phẳng trên mặt w×d, phần thừa rủ xuống `ru`, gợn sóng."""
    g = grid(w + 2 * ru, d + 2 * ru, nx, ny, (0, 0, 0), M)
    def f(v):
        x, y = v.x, v.y
        ox = max(0, abs(x) - w / 2); oy = max(0, abs(y) - d / 2); o = max(ox, oy)
        if o <= 0: return Vector((cx + x, cy + y, top_z + 0.002))
        k = min(1, o / ru)
        nx2 = x if ox <= 0 else math.copysign(w / 2 + 0.004 + 0.01 * k, x)
        ny2 = y if oy <= 0 else math.copysign(d / 2 + 0.004 + 0.01 * k, y)
        wave = song * math.sin((x + y) * 45) * k
        return Vector((cx + nx2 + (wave if oy > 0 else 0), cy + ny2 + (wave if ox > 0 else 0), top_z + 0.002 - o))
    deform(g, f); smooth(g, 80); return g

def nem_khan(o):
    return o

def day_dien(pts, M=None, r=0.003):
    return tube(pts, r, M or mat("nhua", "#2A2A2A", r=0.5, ten="day_dien_den"), n=6)

def oc_vit(x, y, z, M, r=0.004, nhin=(90, 0, 0)):
    return cyl(r, 0.002, (x, y, z), M, n=10, b=0, rot=nhin)

def mo_chuyen(lst, v):
    for o in lst: o.location += Vector(v)
    return lst
