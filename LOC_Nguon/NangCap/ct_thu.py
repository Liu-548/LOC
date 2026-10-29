from lib import *

def tv_crt(c):
    mn, mx = c.mn, c.mx
    x0, x1 = mn.x + 0.04, mx.x - 0.04          # thân TV 0,56 (khăn ren thò 4 cm mỗi bên)
    y0, y1 = mn.y, mx.y                          # mặt trước = y0
    z0, z1 = mn.z, mx.z - 0.006                  # khăn ren dày ~6 mm trên nóc
    W = x1 - x0; cx = (x0 + x1) / 2
    vo = mat("nhua", "#8E9194", r=0.45, ten="vo_tv_xam_bac")
    vo2 = mat("nhua", "#6E7174", r=0.5, ten="vo_tv_lung")
    den = mat("nhua", "#2B2B2B", r=0.6, ten="nut_den")
    kinh = mat("kinh", "#2E3432", r=0.08, m=0.0, ten="man_hinh_tat")
    ren = mat("vai", "#E4DFD0", r=0.9, ten="khan_ren", hai_mat=True)
    # khung mặt trước bo góc
    fd = (y1 - y0) * 0.58
    box((W, fd, z1 - z0 - 0.006), (cx, y0 + fd / 2, (z0 + z1) / 2 + 0.003), vo, b=0.022, seg=4)
    # viền lõm quanh màn
    sw, sh = W * 0.80, (z1 - z0) * 0.70; sz = z0 + (z1 - z0) * 0.58
    box((sw + 0.03, 0.012, sh + 0.03), (cx, y0 + 0.002, sz), vo2, b=0.012, seg=3)
    # màn cong: lưới phồng ra trước
    s = grid(sw, sh, 16, 12, (0, 0, 0), kinh)
    def cong(v):
        u, w = v.x / (sw / 2), v.y / (sh / 2)
        return Vector((cx + v.x, y0 - 0.004 - 0.012 * (1 - 0.5 * u * u - 0.5 * w * w), sz + v.y))
    deform(s, cong); smooth(s, 80)
    # lưng thóp (bóng đèn hình)
    bd = y1 - (y0 + fd)
    bm = bmesh.new()
    prof = [(W * 0.47, (z1 - z0) * 0.46, -0.01), (W * 0.36, (z1 - z0) * 0.34, bd * 0.6), (W * 0.22, (z1 - z0) * 0.22, bd)]
    rings = []
    for hx, hz, yy in prof:
        ring = [bm.verts.new((cx + hx * math.copysign(abs(math.cos(t)) ** 0.5, math.cos(t)), y0 + fd + yy, (z0 + z1) / 2 + 0.01 + hz * math.copysign(abs(math.sin(t)) ** 0.5, math.sin(t)))) for t in [i * 2 * math.pi / 32 for i in range(32)]]
        rings.append(ring)
    for a, b2 in zip(rings, rings[1:]):
        for i in range(32): bm.faces.new((a[i], a[(i + 1) % 32], b2[(i + 1) % 32], b2[i]))
    bm.faces.new(rings[-1][::-1])
    o = mesh_bm(bm, "lung", vo2); smooth(o, 50)
    # khe tản nhiệt sau lưng
    hz = (z1 - z0) * 0.22; zc = (z0 + z1) / 2 + 0.01
    for i in range(7):
        box((W * 0.30, 0.004, 0.006), (cx, y1 + 0.001, zc - hz * 0.7 + i * hz * 0.23), den, b=0.001)
    # nút bấm + đèn nguồn + lưới loa hai bên dưới màn
    for i in range(5):
        cyl(0.007, 0.006, (cx - 0.07 + i * 0.022, y0 - 0.004, z0 + 0.045), den, n=16, rot=(90, 0, 0))
    cyl(0.003, 0.004, (cx + 0.08, y0 - 0.003, z0 + 0.045), mat("nhua", "#5A1E1E", r=0.3, ten="den_nguon"), n=10, rot=(90, 0, 0))
    for sgn in (-1, 1):
        for j in range(6):
            box((0.05, 0.004, 0.003), (cx + sgn * (W * 0.38), y0 - 0.001, z0 + 0.03 + j * 0.008), den, b=0.0008)
    # biển tên trơn (không hãng)
    box((0.06, 0.003, 0.012), (cx, y0 - 0.002, z0 + 0.075), mat("kl", "#B7B9BB", r=0.35, m=0.8, ten="bien_ten_tron"), b=0.001)
    # chân đế
    for sx in (-1, 1):
        for sy in (0, 1):
            box((0.05, 0.04, 0.006), (cx + sx * W * 0.38, y0 + 0.04 + sy * (fd - 0.09), z0 + 0.003), den, b=0.002)
    # khăn ren: phủ nóc, rủ hai bên 0,10, mép răng cưa
    top = z1
    k = grid(W + 0.08, fd * 0.86, 40, 24, (0, 0, 0), ren)
    def ru(v):
        x = v.x; ax = abs(x) - W / 2
        if ax > 0:
            zz = top - min(0.10, ax * 2.5) + 0.004 * math.sin(v.y * 90)
            xx = math.copysign(W / 2 + 0.006 + 0.01 * min(1, ax * 10), x)
        else:
            zz = top + 0.003 + 0.0015 * math.sin(v.x * 60) * math.sin(v.y * 50)
            xx = x
        return Vector((cx + xx, v.y + y0 + fd * 0.47, zz))
    deform(k, ru); smooth(k, 80)
    return None

CT = {"TV_CRT": tv_crt}
