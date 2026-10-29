# NHÓM 2 · Tầng 2: giường đôi phòng bố mẹ (đệm, chiếu trúc, gối, chăn gấp, màn tuyn buộc), góc học của Nhím, đèn
from lib import *
from chung import *
import hoavan

def giuong_doi(c):
    mn, mx = c.mn, c.mx
    go = mat("go", "#5C3A24", r=0.4, ten="go_giuong_venni")
    sang = mat("go", "#7A4E32", r=0.35, ten="go_giuong_bat_sang")
    x0, x1, y0, y1 = mn.x, mx.x, mn.y, mx.y
    W = x1 - x0; cx = (x0 + x1) / 2
    a = 0.07
    # đầu giường (y0) cao 1,05, chân giường (y1) thấp 0,62
    for yy, hh in ((y0 + a / 2, mx.z), (y1 - a / 2, 0.62)):
        for x in (x0 + a / 2, x1 - a / 2):
            box((a, a, hh - 0.03), (x, yy, (hh - 0.03) / 2), go, b=0.008, seg=2)
            lathe([(0, 0), (a * 0.55, 0), (a * 0.6, 0.015), (a * 0.35, 0.04), (0, 0.05)], (x, yy, hh - 0.03), sang, n=20)
        zt = hh - 0.08
        box((W - a, 0.035, 0.08), (cx, yy, zt), go, b=0.008, seg=2)                       # đố trên cong nhẹ
        box((W - a, 0.03, 0.06), (cx, yy, 0.33), go, b=0.006)                              # đố dưới
        # pano: hai ô, gờ nổi chỉ soi (không chạm rồng phượng)
        npan = 2
        pw = (W - a - 0.04) / npan
        for i in range(npan):
            px = x0 + a / 2 + 0.02 + pw * (i + 0.5); pz = (0.36 + zt - 0.04) / 2; ph = zt - 0.04 - 0.36
            box((pw - 0.02, 0.02, ph), (px, yy, pz), go, b=0.004)
            box((pw - 0.1, 0.012, ph - 0.1), (px, yy - (0.014 if yy < cx else -0.014) * (1 if yy < (y0 + y1) / 2 else -1), pz), sang, b=0.02, seg=3)
    # thành giường hai bên + đà đỡ + vạt giường
    for x in (x0 + a / 2, x1 - a / 2):
        box((0.035, y1 - y0 - 2 * a, 0.16), (x, (y0 + y1) / 2, 0.33), go, b=0.006)
        box((0.03, y1 - y0 - 2 * a, 0.03), (x - math.copysign(0.03, x - cx), (y0 + y1) / 2, 0.27), go, b=0.003)
    for i in range(11):
        yy = y0 + a + 0.06 + i * (y1 - y0 - 2 * a - 0.12) / 10
        box((W - a - 0.06, 0.07, 0.016), (cx, yy, 0.296), mat("go", "#8C6A48", r=0.7, ten="vat_giuong_moc"), b=0.002)
    box((0.06, y1 - y0 - 2 * a, 0.05), (cx, (y0 + y1) / 2, 0.25), go, b=0.004)
    return None

def dem_mong(c):
    mn, mx = c.mn, c.mx
    v = mat("vai", "#C9B9A0", r=0.95, ten="vai_dem_soc", img=hoavan.vai_hoa("vai_dem_soc", nen="#CFC2A8", hoa=("#B9A88A",), la="#A89C80", so=14))
    o = cushion((mx.x - mn.x - 0.01, mx.y - mn.y - 0.01, mx.z - mn.z), ((mn.x + mx.x) / 2, (mn.y + mx.y) / 2, (mn.z + mx.z) / 2), v, r=0.02, phong=0.004)
    o["uvs"] = 2.0
    # đường may viền
    hw, hd = (mx.x - mn.x) / 2 - 0.012, (mx.y - mn.y) / 2 - 0.012; cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    for zz in (mn.z + 0.004, mx.z - 0.004):
        tube([(cx - hw, cy - hd, zz), (cx + hw, cy - hd, zz), (cx + hw, cy + hd, zz), (cx - hw, cy + hd, zz), (cx - hw, cy - hd, zz)], 0.003, mat("vai", "#8E7A5A", r=0.9, ten="vien_dem"), n=5)
    return None

def chieu_truc(c):
    mn, mx = c.mn, c.mx
    tre = mat("chieu", "#C9B07A", r=0.6, ten="chieu_truc_tre")
    vien = mat("vai", "#5A3A28", r=0.9, ten="vien_chieu_vai")
    W, D = mx.x - mn.x, mx.y - mn.y; cx = (mn.x + mx.x) / 2
    n = int(W / 0.025)
    for i in range(n):
        x = mn.x + (i + 0.5) * W / n
        o = cyl(0.011, D - 0.04, (x, mn.y + 0.02, mn.z + 0.004), tre, n=8, rot=(-90, 0, 0), b=0)
        o.scale = (1, 0.35, 1)
    for yy in (mn.y + 0.01, mx.y - 0.01):
        box((W, 0.02, 0.006), (cx, yy, mn.z + 0.004), vien, b=0.002)
    return None

def goi(c):
    mn, mx = c.mn, c.mx
    v = mat("vai", "#E8E0CC", r=0.95, ten="ao_goi_kem", img=hoavan.vai_hoa("ao_goi_hoa", nen="#E8E0CC", hoa=("#D49A9A", "#C9A86A"), la="#9AAE8A", so=7))
    o = cushion((mx.x - mn.x - 0.01, mx.y - mn.y - 0.01, mx.z - mn.z - 0.005), ((mn.x + mx.x) / 2, (mn.y + mx.y) / 2, (mn.z + mx.z) / 2), v, r=(mx.z - mn.z) * 0.45, phong=0.012)
    o["uvs"] = 3.0
    # lõm giữa (vết đầu người nằm)
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    deform(o, lambda p: Vector((p.x, p.y, p.z - (0.012 * max(0, 1 - ((p.x / 0.16) ** 2 + (p.y / 0.1) ** 2)) if p.z > 0 else 0))))
    return None

def chan_gap(c):
    mn, mx = c.mn, c.mx
    v = mat("vai", "#7E4A5A", r=0.95, ten="chan_men_tim_than")
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z; cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    # chăn gấp ba lớp, mép gấp bo tròn, lệch nhẹ
    for k in range(3):
        o = cushion((W - 0.02 - k * 0.03, D - 0.02 - k * 0.02, H / 3 - 0.004), (cx + k * 0.008, cy - k * 0.006, mn.z + H / 6 + k * H / 3), v, r=H / 7, phong=0.005)
        o.rotation_euler.z = math.radians(k * 1.2 - 1)
    return None

def man_tuyn(c):
    """Màn tuyn trắng treo bốn góc, vén buộc túm ở hai cột đầu giường."""
    mn, mx = c.mn, c.mx
    tuyn = mat("vai", "#E6E6DF", r=0.9, ten="man_tuyn", img=_tuyn(), cat_alpha=True, hai_mat=True)
    day = mat("vai", "#D8D4C8", r=0.9, ten="day_man")
    x0, x1, y0, y1, zt = mn.x + 0.02, mx.x - 0.02, mn.y + 0.02, mx.y - 0.02, mx.z
    # nóc màn
    p = grid(x1 - x0, y1 - y0, 20, 24, (0, 0, 0), tuyn)
    deform(p, lambda v: Vector(((x0 + x1) / 2 + v.x, (y0 + y1) / 2 + v.y, zt - 0.03 * (1 - (2 * v.x / (x1 - x0)) ** 2) * (1 - (2 * v.y / (y1 - y0)) ** 2))))
    p["uvs"] = 6.0
    # bốn vách buông rủ, vén túm ở hai góc đầu
    for (xa, ya, xb, yb, tum) in ((x0, y0, x1, y0, True), (x0, y1, x1, y1, False), (x0, y0, x0, y1, True), (x1, y0, x1, y1, True)):
        L = math.hypot(xb - xa, yb - ya)
        g = grid(L, zt - 0.05, 30, 18, (0, 0, 0), tuyn)
        def f(v, xa=xa, ya=ya, xb=xb, yb=yb, L=L, tum=tum):
            t = min(1, max(0, (v.x + L / 2) / L)); h = min(1, max(0, (v.y + (zt - 0.05) / 2) / (zt - 0.05)))   # h: 0 đáy → 1 nóc
            x = xa + (xb - xa) * t; y = ya + (yb - ya) * t
            song = 0.03 * math.sin(t * L * 25) * (1 - h)
            nx, ny = -(yb - ya) / L, (xb - xa) / L
            z = 0.05 + h * (zt - 0.05)
            if tum:   # gom về hai đầu ở độ cao 1,1 m
                k = (1 - h) ** 1.5
                for e in (0.0, 1.0):
                    w = math.exp(-((t - e) * 6) ** 2)
                    x += (xa + (xb - xa) * e - x) * k * w * 0.9; y += (ya + (yb - ya) * e - y) * k * w * 0.9
                z = max(z, 0.9 * (1 - k) + 0.05)
            return Vector((x + nx * song, y + ny * song, z))
        deform(g, f); g["uvs"] = 6.0
    for (x, y) in ((x0, y0), (x1, y0), (x0, y1), (x1, y1)):
        tube([(x, y, zt), (x, y, zt + 0.08)], 0.002, day, n=4)
    return None

def _tuyn():
    import os
    from PIL import Image, ImageDraw
    p = os.path.join(hoavan.D, "tuyn.png")
    if os.path.exists(p): return p
    n = 128; im = Image.new("RGBA", (n, n), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    for i in range(0, n, 4):
        d.line((i, 0, i, n), fill=(235, 235, 228, 210)); d.line((0, i, n, i), fill=(235, 235, 228, 210))
    im.save(p); return p

def ban_hoc_nhim(c):
    mn, mx = c.mn, c.mx
    go = mat("go", "#B48A5C", r=0.5, ten="go_ban_hoc_nhim")
    son = mat("son", "#E6DCC6", r=0.4, ten="mat_ban_formica")
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z; cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    for sx in (-1, 1):
        for sy in (-1, 1): chan_thop(cx + sx * (W / 2 - 0.03), cy + sy * (D / 2 - 0.03), 0, H - 0.02, 0.035, 0.03, go)
    box((W, D, 0.022), (cx, cy, H - 0.011), go, b=0.004)
    box((W - 0.02, D - 0.02, 0.002), (cx, cy, H + 0.001), son, b=0.0006)
    box((W - 0.08, 0.018, 0.06), (cx, mn.y + 0.03, H - 0.05), go, b=0.003)
    box((W - 0.06, D - 0.06, 0.012), (cx, cy, 0.1), go, b=0.002)
    # vết bút sáp + nhãn dán hoạt hình đã bạc trên mặt bàn
    for k, col in enumerate(("#C9443A", "#3A6EC9", "#E0B23A")):
        o = box((0.05, 0.003, 0.0004), (cx - 0.12 + k * 0.03, cy + 0.06 - k * 0.02, H + 0.0022), mat("nhua", col, r=0.8, ten=f"sap_{col[1:]}"), b=0); o.rotation_euler.z = math.radians(15 + k * 30)
    return None

def ghe_nhua_nhim(c):
    mn, mx = c.mn, c.mx
    nh = mat("nhua", "#5B8FC7", r=0.4, ten="ghe_nhua_xanh_nhim")
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z; cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    o = box((W, D, 0.03), (cx, cy, H - 0.015), nh, b=0.012, seg=3)
    for i in range(3): box((W - 0.06, 0.012, 0.004), (cx, cy - 0.04 + i * 0.04, H + 0.001), nh, b=0.002)
    for sx in (-1, 1):
        for sy in (-1, 1):
            cyl(0.018, H - 0.02, (cx + sx * (W / 2 - 0.03), cy + sy * (D / 2 - 0.03), 0), nh, n=16, r2=0.014)
    return None

def den_ngu(c):
    mn, mx = c.mn, c.mx
    chao = mat("vai", "#E8D9B0", r=0.8, ten="chao_den_vai", a=0.85)
    de = mat("gom", "#C9A36A", r=0.3, ten="de_den_gom")
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2; H = mx.z - mn.z
    lathe([(0, 0), (0.05, 0), (0.055, 0.01), (0.04, 0.05), (0.045, 0.08), (0.02, 0.11), (0.012, 0.13), (0, 0.13)], (cx, cy, mn.z), de, n=32)
    cyl(0.006, 0.04, (cx, cy, mn.z + 0.13), mat("kl", "#B8A060", r=0.3, m=1, ten="ong_dong"), n=10)
    lathe([(0.085, 0.0), (0.087, 0.0), (0.05, 0.1), (0.048, 0.1)], (cx, cy, mn.z + H - 0.1), chao, n=40)
    sphere(0.022, (cx, cy, mn.z + H - 0.055), mat("den", "#FFF2D0", emis="#FFE2A0", ten="bong_ngu"), n=12)
    day_dien([(cx, cy + 0.04, mn.z + 0.005), (cx + 0.02, mx.y, mn.z + 0.003)], r=0.0025)
    return None

def den_ban_hoc(c):
    mn, mx = c.mn, c.mx
    xanh = mat("son", "#3F6E4F", r=0.4, ten="den_ban_hoc_xanh")
    kl = mat("kl", "#A8AAAC", r=0.35, m=1, ten="khop_den")
    cx, cy = 0.0, (mn.y + mx.y) / 2
    lathe([(0, 0), (0.075, 0), (0.075, 0.015), (0.05, 0.03), (0, 0.032)], (cx, cy, mn.z), xanh, n=32)
    tube([(cx, cy, mn.z + 0.03), (cx + 0.02, cy, mn.z + 0.18), (cx + 0.06, cy, mn.z + 0.27)], 0.008, xanh, n=10)
    sphere(0.012, (cx + 0.06, cy, mn.z + 0.27), kl, n=10)
    tube([(cx + 0.06, cy, mn.z + 0.27), (cx + 0.2, cy, mn.z + 0.29), (mx.x - 0.08, cy, mn.z + 0.26)], 0.007, xanh, n=10)
    lathe([(0.015, 0.0), (0.03, -0.02), (0.075, -0.09), (0.07, -0.092), (0.026, -0.022), (0.012, -0.002)], (mx.x - 0.075, cy, mn.z + 0.27), xanh, n=36).rotation_euler.y = math.radians(-20)
    return None

def bao_thuc(c):
    mn, mx = c.mn, c.mx
    do = mat("son", "#B8392E", r=0.35, ten="vo_bao_thuc_do"); kl = mat("kl", "#C9CCCE", r=0.3, m=1, ten="chuong_bao_thuc")
    mso = mat("anh", "#FFFFFF", r=0.5, ten="mat_so_bao_thuc", img=__import__('ct_g1_khach')._mat_so_img())
    cx = (mn.x + mx.x) / 2; y0 = mn.y; zc = mn.z + 0.07
    cyl(0.048, 0.04, (cx, y0 + 0.04, zc), do, n=40, rot=(90, 0, 0))
    o = cyl(0.04, 0.002, (cx, y0 + 0.0, zc), mso, n=40, rot=(90, 0, 0), b=0)
    __import__('ct_g1_khach')._uv_tron(o, cx, zc, 0.04)
    cyl(0.044, 0.004, (cx, y0 + 0.002, zc), mat("kinh", "#E6EEEC", r=0.03, a=0.15, ten="kinh_dong_ho"), n=40, rot=(90, 0, 0), b=0)
    for s in (-1, 1):
        sphere(0.022, (cx + s * 0.03, y0 + 0.02, zc + 0.05), kl, scale=(1, 1, 0.7), n=16)
        tube([(cx + s * 0.03, y0 + 0.03, zc - 0.035), (cx + s * 0.04, y0 + 0.035, mn.z)], 0.003, kl, n=5)
    tube([(cx - 0.015, y0 + 0.02, zc + 0.065), (cx, y0 + 0.02, zc + 0.075), (cx + 0.015, y0 + 0.02, zc + 0.065)], 0.002, kl, n=5)
    for nm, L, w, a in (("g", 0.022, 0.003, 120), ("p", 0.032, 0.002, 30)):
        o = box((w, 0.001, L), (cx, y0 - 0.0015, zc + L / 2), mat("kl", "#222222", r=0.4, m=0.5, ten="kim"), b=0); o.rotation_euler.y = math.radians(a)
        o.location = (cx + math.sin(math.radians(a)) * L / 2, y0 - 0.0015, zc + math.cos(math.radians(a)) * L / 2)
    return None

CT = {"GiuongDoi": giuong_doi, "DemMong": dem_mong, "ChieuTruc": chieu_truc, "Goi_BoMe_Bo": goi, "Goi_BoMe_Me": goi,
      "Chan_GapDo": chan_gap, "BanHoc_Nhim": ban_hoc_nhim, "GheNhua_Nhim": ghe_nhua_nhim,
      "DenNgu_Nhim": den_ngu, "DenBanHoc": den_ban_hoc, "DongHoDeBan_BaoThuc": bao_thuc}
