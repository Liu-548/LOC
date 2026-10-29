# NHÓM 1 · Phòng khách tầng 1 (GĐ5 prompt 05): salon gỗ tay trứng, tủ TV đứng, đồ điện tử, ấm chén, đồng hồ quả lắc, tranh ảnh
from lib import *
from chung import *
import hoavan
from ct_thu import tv_crt

GO = lambda: mat("go", "#5C3A24", r=0.38, ten="go_salon_venni")
GO_MON = lambda: mat("go", "#8A6446", r=0.30, ten="go_tay_mon")

def _ghe_salon(c, dem_cho=None):
    mn, mx = c.mn, c.mx
    W = mx.x - mn.x; D = mx.y - mn.y; H = mx.z - mn.z
    x0, x1, y0, y1 = mn.x, mx.x, mn.y, mx.y
    go, mon = GO(), GO_MON()
    a = 0.055                                   # tiết diện chân
    yF, yB = y0 + 0.07, y1 - 0.05               # tâm chân trước / sau
    xL, xR = x0 + 0.045, x1 - 0.045
    zArm = 0.62; zSeat = 0.42
    # chân trước: lên tới tay ghế · chân sau: liền trụ lưng, ngả nhẹ
    for x in (xL, xR):
        chan_thop(x, yF, 0, zArm - 0.03, a, a * 0.78, go)
        thanh((x, yB, 0), (x, yB + 0.04, H - 0.04), a, go)
    # chân giữa cho ghế dài
    if W > 1.0:
        chan_thop((xL + xR) / 2, yF + 0.02, 0, 0.30, a * 0.9, a * 0.7, go)
    # vai ghế (yếm) quanh mặt ngồi
    zr = 0.30
    box((xR - xL, 0.022, 0.075), ((xL + xR) / 2, yF - 0.0, zr + 0.0375), go, b=0.004)
    box((xR - xL, 0.022, 0.06), ((xL + xR) / 2, yB, zr + 0.03), go, b=0.004)
    for x in (xL, xR): box((0.022, yB - yF, 0.075), (x, (yF + yB) / 2, zr + 0.0375), go, b=0.004)
    # giằng chân dưới
    for x in (xL, xR): box((0.025, yB - yF, 0.03), (x, (yF + yB) / 2, 0.11), go, b=0.003)
    box((xR - xL, 0.025, 0.03), ((xL + xR) / 2, (yF + yB) / 2, 0.11), go, b=0.003)
    # mặt ngồi: nan gỗ ghép
    n = max(4, int((xR - xL) / 0.12))
    for i in range(n):
        w = (xR - xL - 0.05) / n
        box((w - 0.006, yB - yF - 0.03, 0.02), (xL + 0.025 + w * (i + 0.5), (yF + yB) / 2 + 0.005, zSeat - 0.01), go, b=0.002)
    # tay ghế + đầu "tay trứng" (mòn sáng)
    for s, x in ((-1, xL), (1, xR)):
        box((0.08, yB - yF + 0.02, 0.03), (x - s * 0.005, (yF + yB) / 2 - 0.005, zArm - 0.015), go, b=0.008, seg=3)
        sphere(0.055, (x - s * 0.005, yF - 0.015, zArm - 0.01), mon, scale=(0.85, 1.25, 0.72), n=24)
        # song tiện dưới tay (2 song)
        for k in (0.33, 0.66):
            yy = yF + (yB - yF) * k
            lathe([(0.0, zSeat + 0.0), (0.016, zSeat), (0.018, zSeat + 0.03), (0.011, zSeat + 0.06), (0.016, zSeat + 0.10), (0.011, zSeat + 0.14),
                   (0.017, zArm - 0.05), (0.016, zArm - 0.03), (0.0, zArm - 0.03)], (x, yy, 0), go, n=12)
    # lưng tựa: pano cong nhẹ ở đỉnh, ngả 8°
    lean = math.radians(8)
    def ly(z): return yB + 0.01 + (z - zSeat) * math.tan(lean) * 0.45
    zb0, zb1 = zSeat + 0.06, H - 0.07
    # thanh ngang dưới lưng
    box((xR - xL - 0.04, 0.03, 0.05), ((xL + xR) / 2, ly(zb0), zb0), go, b=0.004)
    # đỉnh lưng cong (vòm)
    bm = bmesh.new(); seg = 24; wtop = xR - xL + 0.02
    top = []; bot = []
    for i in range(seg + 1):
        t = i / seg; x = xL - 0.01 + wtop * t
        hump = 0.035 * math.sin(t * math.pi) ** 1.5
        top.append((x, H - 0.003 - (0.035 - hump)))
        bot.append((x, H - 0.085 - (0.035 - hump) * 0.6))
    vf = [bm.verts.new((x, ly(z) - 0.016, z)) for x, z in top] + [bm.verts.new((x, ly(z) - 0.016, z)) for x, z in bot]
    vb = [bm.verts.new((x, ly(z) + 0.016, z)) for x, z in top] + [bm.verts.new((x, ly(z) + 0.016, z)) for x, z in bot]
    m1 = seg + 1
    for i in range(seg):
        bm.faces.new((vf[m1 + i], vf[m1 + i + 1], vf[i + 1], vf[i]))
        bm.faces.new((vb[i], vb[i + 1], vb[m1 + i + 1], vb[m1 + i]))
        bm.faces.new((vf[i], vf[i + 1], vb[i + 1], vb[i]))
        bm.faces.new((vb[m1 + i], vb[m1 + i + 1], vf[m1 + i + 1], vf[m1 + i]))
    bm.faces.new((vf[0], vb[0], vb[m1], vf[m1])); bm.faces.new((vf[m1 + seg], vb[m1 + seg], vb[seg], vf[seg]))
    o = mesh_bm(bm, "dinh_lung", go); bevel(o, 0.006, 2)
    # pano lưng: tấm ván có gờ nổi + soi chỉ
    zp0, zp1 = zb0 + 0.03, H - 0.09
    npan = 2 if W > 1.0 else 1
    pw = (xR - xL - 0.06) / npan
    for i in range(npan):
        px = xL + 0.03 + pw * (i + 0.5); zc = (zp0 + zp1) / 2
        p = box((pw - 0.02, 0.018, zp1 - zp0), (px, ly(zc), zc), go, b=0.004); p.rotation_euler.x = -lean * 0.45
        q = box((pw - 0.09, 0.012, zp1 - zp0 - 0.08), (px, ly(zc) - 0.012, zc), mon if False else go, b=0.012, seg=3); q.rotation_euler.x = -lean * 0.45
    if npan == 2:
        zc = (zp0 + zp1) / 2; m = box((0.05, 0.03, zp1 - zp0 + 0.02), ((xL + xR) / 2, ly(zc), zc), go, b=0.004); m.rotation_euler.x = -lean * 0.45
    return None

def salon_ghe_dai(c): return _ghe_salon(c)
def salon_ghe_don(c): return _ghe_salon(c)

def _dem(c, lom=0.0, lom_dau=+1, day_tuot=False, lat=False):
    mn, mx = c.mn, c.mx
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    img = hoavan.vai_hoa()
    vai = mat("vai", "#FFFFFF", r=0.9, ten="vai_dem_hoa_dogach", img=img)
    vien = mat("vai", "#6E3A2C", r=0.85, ten="vien_dem_nau")
    o = cushion((W - 0.012, D - 0.012, H - 0.008), (cx, cy, mn.z + H / 2), vai, r=H * 0.45, phong=0.008)
    o["uvs"] = 4.0
    # lõm ở một đầu (chỗ bố hay ngồi)
    if lom > 0:
        def f(v):
            t = max(0.0, (lom_dau * (v.x) / (W / 2) - 0.15) / 0.85)
            dy = 1 - min(1, abs(v.y) / (D / 2)) ** 2
            if v.z > 0: v.z -= lom * (math.sin(min(1, t) * math.pi / 2)) * dy
            return v
        deform(o, f)
    # viền quanh (piping)
    hw, hd, zc = W / 2 - 0.008, D / 2 - 0.008, mn.z + H * 0.5
    pts = []
    for i in range(41):
        a = i / 40 * 2 * math.pi
        x = max(-hw, min(hw, math.cos(a) * hw * 1.4)); y = max(-hd, min(hd, math.sin(a) * hd * 1.4))
        pts.append((cx + x, cy + y, zc))
    tube(pts, 0.004, vien, n=6)
    # dây buộc ở hai góc sau (+Y)
    for s in (-1, 1):
        bx, by = cx + s * (hw - 0.03), cy + hd
        if day_tuot and s == 1:
            tube([(bx, by, zc), (bx + 0.01, by + 0.03, zc - 0.01), (bx + 0.02, by + 0.05, mn.z - 0.0), (bx + 0.03, by + 0.06, mn.z)], 0.003, vien, n=6)
        else:
            tube([(bx - 0.02, by, zc), (bx, by + 0.012, zc - 0.002), (bx + 0.02, by, zc)], 0.003, vien, n=6)
            tube([(bx, by + 0.012, zc - 0.002), (bx - 0.006, by + 0.04, zc - 0.025)], 0.0028, vien, n=6)
            tube([(bx, by + 0.012, zc - 0.002), (bx + 0.008, by + 0.038, zc - 0.03)], 0.0028, vien, n=6)
    return None

def dem_gan_tv(c):
    # tìm đầu lõm trong file gốc (đỉnh thấp nhất) để giữ đúng phía
    me = [o for o in c.goc.values() if o.type == 'MESH'][0]
    pts = [me.matrix_world @ v.co for v in me.data.vertices]
    top = max(p.z for p in pts); low = [p for p in pts if p.z > (c.mn.z + c.mx.z) / 2 and p.z < top - 0.006]
    dau = 1 if (sum(p.x for p in low) / max(1, len(low))) >= (c.mn.x + c.mx.x) / 2 else -1
    return _dem(c, lom=0.015, lom_dau=dau, day_tuot=True)
def dem_kia(c): return _dem(c)
def dem_don(c): return _dem(c)

def ban_nuoc(c):
    mn, mx = c.mn, c.mx
    go, mon = GO(), GO_MON()
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    kinh = mat("kinh", "#CBD6D4", r=0.05, a=0.22, ten="kinh_ban")
    ren = mat("vai", "#FFFFFF", r=0.9, ten="ren_trang_ngam", img=hoavan.ren(), cat_alpha=True, hai_mat=True)
    a = 0.05; zt = H - 0.008
    for sx in (-1, 1):
        for sy in (-1, 1):
            chan_thop(cx + sx * (W / 2 - 0.04), cy + sy * (D / 2 - 0.04), 0, zt - 0.025, a, a * 0.75, go)
    # mặt gỗ có gờ viền
    box((W, D, 0.025), (cx, cy, zt - 0.0125), go, b=0.006, seg=3)
    box((W - 0.06, D - 0.06, 0.004), (cx, cy, zt + 0.0005), mat("go", "#4E2F1C", r=0.4, ten="go_mat_ban_toi"), b=0.001)
    # yếm bàn
    for sy in (-1, 1): box((W - 0.08, 0.02, 0.07), (cx, cy + sy * (D / 2 - 0.04), zt - 0.06), go, b=0.003)
    for sx in (-1, 1): box((0.02, D - 0.08, 0.07), (cx + sx * (W / 2 - 0.04), cy, zt - 0.06), go, b=0.003)
    # tầng dưới để báo cũ
    box((W - 0.09, D - 0.09, 0.016), (cx, cy, 0.12), go, b=0.003)
    for k in range(3):
        b2 = box((0.32, 0.24, 0.003), (cx - 0.15 + k * 0.012, cy + 0.02 - k * 0.01, 0.13 + k * 0.0035), mat("giay", "#D9D2BE", r=0.9, ten="bao_cu"), b=0)
        b2.rotation_euler.z = math.radians(k * 7 - 6)
    # khăn ren kẹp dưới kính + tấm kính
    p = plane(W * 0.55, D * 0.6, (cx, cy, zt + 0.0025), ren, rot=(90, 0, 0)); p["uvs"] = 1
    box((W - 0.01, D - 0.01, 0.008), (cx, cy, zt + 0.004 + 0.002), kinh, b=0.0015)
    return None

def tu_tv(c):
    # cánh dưới phải nằm trọn trong hộp chọn của LocTuMo (file z −0,003..0,045 → Blender y −0,045..0,003; x 0,017–0,6 / 0,6–1,183; cao 0,005–0,595)
    mn, mx = c.mn, c.mx
    go, mon = GO(), GO_MON()
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    cx = (mn.x + mx.x) / 2
    t = 0.02; yF = 0.0                      # mặt trước thân = y 0 (file z 0)
    toi = mat("go", "#3A2416", r=0.6, ten="go_long_tu_toi")
    dong = mat("dong", "#B08A4A", r=0.35, m=1.0, ten="tay_nam_dong")
    # hông, lưng
    for x in (mn.x + t / 2, mx.x - t / 2): box((t, mx.y - yF + 0.02, H - 0.07), (x, (yF - 0.02 + mx.y) / 2, 0.07 + (H - 0.07) / 2), go, b=0.003)
    box((W - 2 * t, 0.012, H - 0.08), (cx, mx.y - 0.006, 0.07 + (H - 0.08) / 2), toi, b=0)
    # bệ chân
    box((W, mx.y - yF + 0.02, 0.06), (cx, (yF - 0.02 + mx.y) / 2, 0.03), go, b=0.004)
    box((W - 0.01, 0.012, 0.012), (cx, yF - 0.026, 0.066), go, b=0.003)
    # gờ vương miện ba lớp trên nóc
    z = H - 0.055
    for dw, hh in ((0.0, 0.022), (0.025, 0.018), (0.04, 0.015)):
        box((W + dw - 0.01, mx.y - yF + 0.02 + dw / 2, hh), (cx, (yF - 0.02 + mx.y) / 2 - dw / 4, z + hh / 2), go, b=0.004); z += hh
    # đợt ngăn: trên cánh dưới, đáy hộc TV, ray kính
    for zz in (0.61, 1.11):
        box((W - 2 * t, mx.y - yF, 0.022), (cx, (yF + mx.y) / 2, zz), go, b=0.003)
        box((W - 2 * t, 0.02, 0.03), (cx, yF - 0.01, zz), go, b=0.003)
    box((W - 2 * t, 0.03, 0.012), (cx, yF - 0.012, H - 0.07), go, b=0.002)
    box((W - 2 * t - 0.01, mx.y - yF - 0.06, 0.006), (cx, (yF + mx.y) / 2 + 0.01, 1.48), mat("kinh", "#CBD6D4", r=0.05, a=0.25, ten="kinh_dot"), b=0.001)
    # nẹp đứng giữa tầng dưới (sau cánh)
    box((0.02, 0.02, 0.53), (cx, yF + 0.012, 0.07 + 0.265), go, b=0.002)
    # hai cánh pano (gọn trong hộp chọn)
    for x0, x1, tay in ((0.024, 0.594, 0.57), (0.606, 1.176, 0.63)):
        xc = (x0 + x1) / 2; w = x1 - x0
        box((w, 0.018, 0.5), (xc, -0.031, 0.075 + 0.25), go, b=0.004)
        box((w - 0.09, 0.006, 0.41), (xc, -0.0415, 0.075 + 0.25), go, b=0.012, seg=3)
        box((w - 0.05, 0.003, 0.45), (xc, -0.0405, 0.075 + 0.25), mat("go", "#4E2F1C", r=0.45, ten="go_chi_pano"), b=0.001)
        cyl(0.009, 0.004, (tay, -0.0405, 0.48), dong, n=16, rot=(90, 0, 0), b=0.001)
    return None

def canh_kinh(c):
    mn, mx = c.mn, c.mx
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    go = GO(); kinh = mat("kinh", "#CBD6D4", r=0.05, a=0.2, ten="kinh_canh_tu")
    v = 0.028
    box((W, 0.006 + 0.004, v), (mn.x + W / 2, (mn.y + mx.y) / 2, mn.z + v / 2), go, b=0.002)
    box((W, 0.01, v), (mn.x + W / 2, (mn.y + mx.y) / 2, mx.z - v / 2), go, b=0.002)
    box((v, 0.01, H - 2 * v), (mn.x + v / 2, (mn.y + mx.y) / 2, mn.z + H / 2), go, b=0.002)
    box((v, 0.01, H - 2 * v), (mx.x - v / 2, (mn.y + mx.y) / 2, mn.z + H / 2), go, b=0.002)
    box((W - 2 * v + 0.006, 0.004, H - 2 * v + 0.006), (mn.x + W / 2, (mn.y + mx.y) / 2, mn.z + H / 2), kinh, b=0)
    # khấc nắm tay (lõm tròn đồng)
    cyl(0.012, 0.003, (mx.x - 0.05, mn.y - 0.001, mn.z + H / 2), mat("dong", "#B08A4A", r=0.35, m=1, ten="tay_nam_dong"), n=16, rot=(90, 0, 0))
    return None

def dau_vcd(c):
    mn, mx = c.mn, c.mx
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    cx = (mn.x + mx.x) / 2; y0 = mn.y
    den = mat("nhua", "#2E2E2C", r=0.55, ten="vo_den_nham"); bong = mat("nhua", "#1E1E1E", r=0.25, ten="mat_den_bong")
    h = H - 0.004  # đĩa trên nóc ~ cao 2 mm
    box((W, D - 0.004, h - 0.004), (cx, mn.y + D / 2 + 0.002, h / 2 + 0.002), den, b=0.004)
    box((W - 0.01, 0.006, h - 0.012), (cx, y0 + 0.004, h / 2 + 0.002), bong, b=0.002)
    box((0.2, 0.004, 0.012), (cx - 0.05, y0, h * 0.62), mat("nhua", "#141414", r=0.4, ten="khay_dia"), b=0.001)
    box((0.07, 0.003, 0.016), (cx + 0.14, y0 - 0.001, h * 0.55), mat("kinh", "#20302A", r=0.1, ten="man_led_tat"), b=0.001)
    for i in range(4): cyl(0.004, 0.004, (cx + 0.09 + i * 0.016, y0 - 0.001, h * 0.28), mat("kl", "#9A9C9E", r=0.35, m=0.8, ten="nut_bac"), n=12, rot=(90, 0, 0))
    for s in (-1, 1):
        for sy in (0, 1): cyl(0.012, 0.004, (cx + s * (W / 2 - 0.04), mn.y + 0.04 + sy * (D - 0.09), 0), mat("caosu", "#1A1A1A", ten="chan_cao_su"), n=12, b=0)
    # đĩa VCD để quên trên nóc
    d = lathe([(0.0075, 0), (0.06, 0), (0.06, 0.0012), (0.0075, 0.0012)], (cx + 0.06, mn.y + D * 0.55, h + 0.0005), mat("kl", "#C9CCD2", r=0.15, m=0.9, ten="dia_vcd"), n=40)
    return None

def loa_thung(c):
    mn, mx = c.mn, c.mx
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    cx = (mn.x + mx.x) / 2; y0 = mn.y
    vo = mat("go", "#2E2E2C", r=0.6, ten="vo_loa_den")
    luoi = mat("vai", "#FFFFFF", r=0.9, ten="luoi_loa", img=hoavan.luoi())
    box((W, D, H), (cx, mn.y + D / 2, H / 2), vo, b=0.006, seg=3)
    p = plane(W - 0.024, H - 0.05, (cx, y0 - 0.001, H / 2 + 0.01), luoi); p["uvs"] = 1
    box((W - 0.016, 0.006, 0.012), (cx, y0 + 0.0, 0.016), vo, b=0.002)
    box((0.03, 0.002, 0.006), (cx, y0 - 0.002, 0.016), mat("kl", "#A8AAAC", r=0.3, m=0.9, ten="logo_tron"), b=0.001)
    return None

def dieu_khien(c):
    mn, mx = c.mn, c.mx
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    vo = mat("nhua", "#2B2B2B", r=0.5, ten="vo_dieu_khien")
    nilon = mat("nhua", "#E8EEF0", r=0.08, a=0.25, ten="nilon_boc")
    bang = mat("nhua", "#D9CFA8", r=0.6, a=0.85, ten="bang_dinh")
    box((W - 0.012, D - 0.012, H - 0.01), (cx, cy, mn.z + H / 2), vo, b=0.006, seg=3)
    for i in range(4):
        for j in range(3):
            box((0.008, 0.006, 0.003), (cx - 0.012 + j * 0.012, cy - 0.03 + i * 0.014, mx.z - 0.006), mat("caosu", "#5A5A5A", ten="phim_cao_su"), b=0.0015)
    box((0.01, 0.008, 0.003), (cx, cy + 0.055, mx.z - 0.006), mat("caosu", "#8E2E2A", ten="phim_nguon"), b=0.0015)
    box((W - 0.004, D - 0.004, H - 0.002), (cx, cy, mn.z + H / 2), nilon, b=0.006, seg=2)
    for s in (-1, 1): box((W, 0.02, H), (cx, cy + s * (D / 2 - 0.012), mn.z + H / 2), bang, b=0.004)
    return None

def khay_am_chen(c):
    mn, mx = c.mn, c.mx
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    khay = mat("nhua", "#A8322A", r=0.35, ten="khay_nhua_do")
    su = mat("gom", "#E6E4DA", r=0.15, ten="su_men_lam")
    lam = mat("gom", "#3E5E8A", r=0.15, ten="hoa_lam")
    # khay tròn Ø0,40 tâm (0,0)
    lathe([(0, 0), (0.19, 0), (0.2, 0.012), (0.205, 0.024), (0.196, 0.024), (0.188, 0.008), (0, 0.008)], (0, 0, mn.z), khay, n=48)
    # ấm
    zk = mn.z + 0.008
    lathe([(0, 0), (0.035, 0), (0.05, 0.02), (0.058, 0.05), (0.052, 0.08), (0.03, 0.095), (0.028, 0.1), (0, 0.1)], (0.0, 0.03, zk), su, n=32)
    lathe([(0, 0), (0.03, 0), (0.026, 0.008), (0.006, 0.016), (0.008, 0.026), (0, 0.028)], (0.0, 0.03, zk + 0.1), su, n=24)
    tube([(0.05, 0.03, zk + 0.035), (0.08, 0.03, zk + 0.06), (0.098, 0.03, zk + 0.09)], 0.006, su, n=8)
    tube([(-0.05, 0.03, zk + 0.07), (-0.078, 0.03, zk + 0.066), (-0.078, 0.03, zk + 0.035), (-0.054, 0.03, zk + 0.03)], 0.005, su, n=8)
    lathe([(0.05, 0.0), (0.0595, 0.0), (0.0595, 0.008), (0.05, 0.008)], (0.0, 0.03, zk + 0.045), lam, n=32)
    # 6 chén quanh ấm + 1 chén úp riêng ở mép (chén của bố)
    for i in range(6):
        a = math.radians(-30 + i * 60)
        x, y = 0.12 * math.cos(a), 0.03 + 0.12 * math.sin(a)
        bat(0.028, 0.035, (x, y, zk), su, n=24)
        lathe([(0.0262, 0.0), (0.0285, 0.0), (0.0285, 0.004), (0.0262, 0.004)], (x, y, zk + 0.027), lam, n=24)
    o = bat(0.028, 0.035, (0, 0, 0), su, n=24); o.rotation_euler.x = math.pi; o.location = (-0.15, -0.12, zk + 0.035)
    # phích vỏ sắt hoa + hộp chè sắt tây cạnh khay
    pv = mat("son", "#8E3B34", r=0.4, ten="phich_son_do"); vang = mat("son", "#D9B45E", r=0.35, ten="hoa_vang")
    px, py = mx.x - 0.06, mn.y + 0.1
    lathe([(0, 0), (0.055, 0), (0.057, 0.02), (0.057, 0.24), (0.045, 0.26), (0.028, 0.27), (0.028, 0.285), (0, 0.285)], (px, py, mn.z), pv, n=32)
    for z in (0.06, 0.18): lathe([(0.0, 0), (0.0585, 0), (0.0585, 0.012), (0.0, 0.012)], (px, py, mn.z + z), vang, n=32)
    cyl(0.026, 0.02, (px, py, mn.z + 0.28), mat("go", "#9C7A52", r=0.8, ten="nut_bac_phich"), n=16)
    tube([(px - 0.055, py, mn.z + 0.22), (px - 0.085, py, mn.z + 0.2), (px - 0.085, py, mn.z + 0.08), (px - 0.055, py, mn.z + 0.06)], 0.007, pv, n=8)
    hc = mat("son", "#2F5A4A", r=0.35, ten="hop_che_xanh")
    box((0.08, 0.08, 0.11), (mx.x - 0.06, mx.y - 0.06, mn.z + 0.055), hc, b=0.006, seg=2)
    box((0.084, 0.084, 0.02), (mx.x - 0.06, mx.y - 0.06, mn.z + 0.11), mat("son", "#B9963E", r=0.3, ten="nap_che_vang"), b=0.004)
    box((0.06, 0.002, 0.05), (mx.x - 0.06, mx.y - 0.101, mn.z + 0.055), vang, b=0.001)
    return None

def dong_ho_qua_lac(c):
    mn, mx = c.mn, c.mx
    W = mx.x - mn.x; cx = (mn.x + mx.x) / 2
    go = mat("go", "#6B4A32", r=0.3, ten="go_dong_ho_bong")
    dong = mat("dong", "#B08A4A", r=0.3, m=1, ten="dong_qua_lac")
    mat_so = mat("giay", "#FFFFFF", r=0.7, ten="mat_so_la_ma", img=_mat_so_img())
    kinh = mat("kinh", "#DDE6E4", r=0.03, a=0.15, ten="kinh_dong_ho")
    den = mat("kl", "#2B2B2B", r=0.4, m=0.6, ten="kim_den")
    y0 = mn.y; yb = mx.y
    zt, zb = mx.z, mn.z
    # vỏ: lưng, hai hông có trụ tiện, mái cong, chân đế
    box((W - 0.03, 0.012, zt - zb - 0.06), (cx, yb - 0.006, (zt + zb) / 2 - 0.01), go, b=0.002)
    for s in (-1, 1):
        box((0.025, yb - y0 - 0.03, zt - zb - 0.1), (cx + s * (W / 2 - 0.03), (y0 + yb) / 2 + 0.01, (zt + zb) / 2), go, b=0.004)
        lathe([(0, 0), (0.012, 0), (0.014, 0.02), (0.009, 0.06), (0.012, 0.3), (0.009, 0.55), (0.014, 0.62), (0.012, 0.66), (0, 0.66)], (cx + s * (W / 2 - 0.012), y0 + 0.02, zb + 0.06), go, n=12)
    # mái
    box((W, yb - y0, 0.03), (cx, (y0 + yb) / 2, zt - 0.06), go, b=0.006)
    bm_ = lathe([(0, 0), (0.06, 0), (0.05, 0.03), (0.02, 0.05), (0.008, 0.06), (0, 0.065)], (cx, (y0 + yb) / 2, zt - 0.045), go, n=24)
    bm_.scale = (2.2, 1.0, 0.8)
    sphere(0.012, (cx, (y0 + yb) / 2, zt - 0.006), dong, n=12)
    # đáy
    box((W - 0.02, yb - y0 - 0.01, 0.03), (cx, (y0 + yb) / 2, zb + 0.06), go, b=0.006)
    lathe([(0, 0), (0.02, 0), (0.03, 0.02), (0.0, 0.045)], (cx, (y0 + yb) / 2, zb + 0.015), go, n=16)
    # mặt số (tâm theo file gốc: tìm node MatSo)
    ms = c.node("MatSo"); mcen = (ms.matrix_world @ Vector((0, 0, 0))) if ms else Vector((cx, y0, zt - 0.2))
    a, b2 = lib_bbox_node(ms) if ms else (Vector((cx - 0.12, y0, zt - 0.32)), Vector((cx + 0.12, y0, zt - 0.08)))
    zc = (a.z + b2.z) / 2; rr = min(b2.x - a.x, b2.z - a.z) / 2
    cyl(rr + 0.012, 0.02, (cx, y0 + 0.022, zc), dong, n=40, rot=(90, 0, 0))
    o = cyl(rr, 0.003, (cx, y0 + 0.002, zc), mat_so, n=40, rot=(90, 0, 0), b=0)
    # UV phẳng cho mặt số
    _uv_tron(o, cx, zc, rr)
    # cửa kính mặt dưới (node KinhCua giữ riêng)
    kc = c.node("KinhCua"); ka, kb = lib_bbox_node(kc)
    kn = box((kb.x - ka.x, 0.004, kb.z - ka.z), ((ka.x + kb.x) / 2, (ka.y + kb.y) / 2, (ka.z + kb.z) / 2), kinh, b=0)
    kv = []
    for (sx, sz, ww, hh) in ((0, 1, kb.x - ka.x + 0.02, 0.018), (0, -1, kb.x - ka.x + 0.02, 0.018), (1, 0, 0.018, kb.z - ka.z), (-1, 0, 0.018, kb.z - ka.z)):
        kv.append(box((ww, 0.014, hh), ((ka.x + kb.x) / 2 + sx * ((kb.x - ka.x) / 2 + 0.005), (ka.y + kb.y) / 2 - 0.004, (ka.z + kb.z) / 2 + sz * ((kb.z - ka.z) / 2 + 0.005)), go, b=0.002))
    # quả lắc: cần + đĩa (pivot = điểm treo của node QuaLac)
    ql = c.node("QuaLac"); qa, qb = lib_bbox_children(ql)
    hp = ql.matrix_world.translation
    can = box((0.008, 0.003, hp.z - qa.z - 0.05), (hp.x, hp.y, (hp.z + qa.z + 0.05) / 2), dong, b=0.001)
    dia = cyl(0.045, 0.006, (hp.x, hp.y - 0.003, qa.z + 0.045), dong, n=32, rot=(90, 0, 0))
    # kim giờ/phút: pivot tâm mặt số (node KimGio/KimPhut) — giữ hướng như gốc (8:20)
    out = {"QuaLac": [can, dia], "KinhCua": [kn]}
    for nm, L, w in (("KimGio", rr * 0.55, 0.008), ("KimPhut", rr * 0.82, 0.005)):
        nd = c.node(nm); ch = [x for x in c.goc.values() if x.parent == nd and x.type == 'MESH']
        p0 = nd.matrix_world.translation
        a2, b3 = lib_bbox_children(nd)
        far = max([a2, b3, Vector((a2.x, a2.y, b3.z)), Vector((b3.x, a2.y, a2.z))], key=lambda v: (v - p0).length)
        d = (far - p0); d.y = 0; d = d.normalized() if d.length > 1e-4 else Vector((0, 0, 1))
        bm = bmesh.new()
        tip = p0 + d * L; side = Vector((d.z, 0, -d.x)) * w / 2
        vs = [bm.verts.new(p0 - d * 0.012 + side * 0.6), bm.verts.new(p0 + d * L * 0.7 + side), bm.verts.new(tip), bm.verts.new(p0 + d * L * 0.7 - side), bm.verts.new(p0 - d * 0.012 - side * 0.6)]
        for v in vs: v.co.y = p0.y - 0.003
        f = bm.faces.new(vs)
        o = mesh_bm(bm, nm, den); solid(o, 0.0015)
        out[nm] = [o]
    cyl(0.006, 0.006, (cx, y0 - 0.006, zc), dong, n=12, rot=(90, 0, 0))
    # lỗ lên dây (2) + ố vàng quanh lỗ: vẽ trong ảnh mặt số
    return out

def lib_bbox_node(o):
    if o is None: return Vector((0, 0, 0)), Vector((0, 0, 0))
    objs = [o] + [x for x in o.children_recursive if x.type == 'MESH']
    objs = [x for x in objs if x.type == 'MESH']
    return bbox(objs)

def lib_bbox_children(o):
    objs = [x for x in o.children_recursive if x.type == 'MESH']
    return bbox(objs)

def _uv_tron(o, cx, zc, r):
    me = o.data
    if not me.uv_layers: me.uv_layers.new(name="UVMap")
    uvl = me.uv_layers.active.data; mw = o.matrix_world
    for p in me.polygons:
        for li in p.loop_indices:
            co = mw @ me.vertices[me.loops[li].vertex_index].co
            uvl[li].uv = (0.5 + (co.x - cx) / (2 * r), 0.5 + (co.z - zc) / (2 * r))
    o["uv_giu"] = 1

def _mat_so_img():
    import os
    from PIL import Image, ImageDraw, ImageFont
    p = os.path.join(hoavan.D, "mat_so_la_ma.png")
    if os.path.exists(p): return p
    n = 512; im = Image.new("RGB", (n, n), (232, 226, 206)); d = ImageDraw.Draw(im)
    # ố vàng quanh lỗ lên dây
    for (x, y) in ((n * 0.36, n * 0.62), (n * 0.64, n * 0.62)):
        for r in range(60, 0, -4):
            d.ellipse((x - r, y - r, x + r, y + r), fill=(int(232 - (60 - r) * 0.35), int(226 - (60 - r) * 0.5), int(206 - (60 - r) * 0.9)))
        d.ellipse((x - 10, y - 10, x + 10, y + 10), fill=(60, 50, 35))
    d.ellipse((8, 8, n - 8, n - 8), outline=(43, 43, 43), width=4)
    d.ellipse((60, 60, n - 60, n - 60), outline=(43, 43, 43), width=2)
    f = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSerif.ttf", 44) if os.path.exists("/usr/share/fonts/truetype/dejavu/DejaVuSerif.ttf") else ImageFont.load_default()
    so = ["XII", "I", "II", "III", "IIII", "V", "VI", "VII", "VIII", "IX", "X", "XI"]
    for i, s in enumerate(so):
        a = math.radians(90 - i * 30); x = n / 2 + math.cos(a) * n * 0.36; y = n / 2 - math.sin(a) * n * 0.36
        tw = d.textlength(s, font=f); d.text((x - tw / 2, y - 26), s, font=f, fill=(43, 43, 43))
    for i in range(60):
        a = math.radians(i * 6); r1 = n * 0.46; r2 = n * (0.43 if i % 5 == 0 else 0.445)
        d.line((n / 2 + math.cos(a) * r1, n / 2 + math.sin(a) * r1, n / 2 + math.cos(a) * r2, n / 2 + math.sin(a) * r2), fill=(43, 43, 43), width=3 if i % 5 == 0 else 1)
    im = im.transpose(Image.FLIP_LEFT_RIGHT) if False else im
    im.save(p); return p

def _tranh(c, w_vien=0.05, day=0.03, mau_khung="#A88A4E", lop="go", kinh=True):
    mn, mx = c.mn, c.mx
    W, H = mx.x - mn.x, mx.z - mn.z
    cx = (mn.x + mx.x) / 2; zc = (mn.z + mx.z) / 2
    img = c.anh_list[0] if c.anh_list else None
    khung = mat(lop, mau_khung, r=0.35, m=0.0, ten=f"khung_{mau_khung.strip('#')}")
    anh = mat("anh", "#FFFFFF", r=0.8, ten=f"anh_{c.ten}", img=img) if img else mat("giay", "#DDD6C4", ten="giay_trang")
    kn = mat("kinh", "#E6EEEC", r=0.03, a=0.12, ten="kinh_khung") if kinh else None
    # mặt sau sát tường = mx.y (tường phía +Y), mặt trước −Y
    khung_anh(cx, mx.y, zc, W, H, w_vien, mx.y - mn.y, khung, anh, kn)
    # dây treo + đinh
    tube([(cx - W * 0.3, mx.y - 0.004, mx.z - 0.06), (cx, mx.y - 0.004, mx.z + 0.0), (cx + W * 0.3, mx.y - 0.004, mx.z - 0.06)], 0.0012, mat("kl", "#8A8A84", r=0.5, m=0.8, ten="day_thep"), n=4)
    return None

def tranh_theu(c): return _tranh(c, 0.06, 0.02, "#A88A4E", "son")
def anh_gia_dinh(c): return _tranh(c, 0.035, 0.02, "#5C3A24", "go")
def bang_gdvh(c): return _tranh(c, 0.025, 0.02, "#6B4A32", "go")

def khung_nho(c):
    """Khung ảnh nhỏ dựng đứng có chân chống (trong tủ kính)."""
    mn, mx = c.mn, c.mx
    W, H = mx.x - mn.x, mx.z - mn.z; cx = (mn.x + mx.x) / 2
    img = c.anh_list[0] if c.anh_list else None
    khung = mat("son", "#B79A5C", r=0.3, ten="khung_nho_ma_vang")
    anh = mat("anh", "#FFFFFF", r=0.8, ten=f"anh_{c.ten}", img=img) if img else mat("giay", "#DDD6C4", ten="giay_trang")
    nghieng = math.radians(10)
    v = 0.008
    parts = []
    parts += [box((W, 0.008, v), (cx, mn.y + 0.004, mn.z + H - v / 2 - 0.0), khung, b=0.0015),
              box((W, 0.008, v), (cx, mn.y + 0.004, mn.z + v / 2 + 0.006), khung, b=0.0015),
              box((v, 0.008, H - 0.006), (cx - W / 2 + v / 2, mn.y + 0.004, mn.z + 0.006 + (H - 0.006) / 2), khung, b=0.0015),
              box((v, 0.008, H - 0.006), (cx + W / 2 - v / 2, mn.y + 0.004, mn.z + 0.006 + (H - 0.006) / 2), khung, b=0.0015)]
    parts.append(plane(W - 2 * v + 0.002, H - 2 * v - 0.004, (cx, mn.y + 0.003, mn.z + 0.006 + (H - 0.006) / 2), anh))
    parts.append(box((W - 0.004, 0.003, H - 0.01), (cx, mn.y + 0.0075, mn.z + 0.006 + (H - 0.006) / 2), mat("carton", "#6B5A44", ten="lung_khung"), b=0))
    # chân chống
    o = box((0.012, 0.004, H * 0.7), (cx, mn.y + 0.014, mn.z + H * 0.35), mat("carton", "#6B5A44", ten="lung_khung"), b=0.001)
    o.rotation_euler.x = math.radians(-25)
    o.location.y = min(mx.y - 0.006, o.location.y)
    return None

def chieu_men(c):
    """Chiếu + chăn màn gấp gọn cho khách ở lại (góc phòng khách)."""
    mn, mx = c.mn, c.mx
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    chieu = mat("chieu", "#C9B58A", r=0.8, ten="chieu_coi")
    men = mat("vai", "#6E2E3E", r=0.9, ten="chan_men_do_dam")
    men2 = mat("vai", "#D8CFB8", r=0.9, ten="goi_kem")
    # chiếu cuộn nằm dưới
    cy0 = cy
    o = cyl(0.07, W * 0.98, (mn.x + W * 0.01, cy - D * 0.2, mn.z + 0.07), chieu, n=24, rot=(0, 90, 0))
    o.scale = (1, 1, 1)
    # chăn gấp vuông, mép gấp bo
    cushion((W * 0.7, D * 0.75, 0.12), (cx - W * 0.1, cy + D * 0.08, mn.z + 0.06 + 0.12 / 2 + 0.02), men, r=0.035, phong=0.006)
    cushion((W * 0.62, D * 0.65, 0.06), (cx - W * 0.08, cy + D * 0.08, mn.z + 0.22 + 0.03), men2, r=0.025, phong=0.008)
    return None

def binh_hoa_lon(c):
    mn, mx = c.mn, c.mx
    gom = mat("gom", "#E8E4D8", r=0.2, ten="gom_trang_binh")
    lam = mat("gom", "#3E5E8A", r=0.2, ten="gom_vien_lam")
    hb = mx.z - mn.z
    hv = hb * 0.55
    lathe([(0, 0), (0.07, 0), (0.075, 0.01), (0.12, hv * 0.3), (0.135, hv * 0.55), (0.11, hv * 0.8), (0.065, hv * 0.95), (0.075, hv), (0.068, hv), (0.06, hv * 0.96), (0, hv * 0.96)], (0, 0, mn.z), gom, n=40)
    lathe([(0, 0), (0.1355, 0), (0.1355, 0.02), (0, 0.02)], (0, 0, mn.z + hv * 0.5), lam, n=40)
    # bó cúc trắng/vàng
    la = mat("thuc", "#4F6B3A", r=0.7, ten="la_cuc"); cuc_t = mat("thuc", "#EEE9DA", r=0.8, ten="cuc_trang"); cuc_v = mat("thuc", "#D9B13A", r=0.8, ten="cuc_vang")
    rnd = random.Random(3)
    for i in range(19):
        a = rnd.uniform(0, 6.28); r = rnd.uniform(0.0, 0.11) ; top = mn.z + hb - rnd.uniform(0.0, 0.18)
        bx, by = r * math.cos(a), r * math.sin(a)
        tube([(0, 0, mn.z + hv * 0.8), (bx * 0.5, by * 0.5, mn.z + hv + 0.08), (bx, by, top - 0.03)], 0.0025, la, n=5)
        hx = sphere(0.032, (bx, by, top - 0.02), cuc_t if i % 3 else cuc_v, scale=(1, 1, 0.55), n=16)
        for k in range(3):
            ll = sphere(0.022, (bx * 0.8 + rnd.uniform(-0.02, 0.02), by * 0.8 + rnd.uniform(-0.02, 0.02), top - 0.08 - k * 0.04), la, scale=(1.6, 0.6, 0.2), n=10)
            ll.rotation_euler = (rnd.uniform(-0.5, 0.5), rnd.uniform(-0.5, 0.5), rnd.uniform(0, 6))
    return None

def den_chum(c):
    mn, mx = c.mn, c.mx
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    dong = mat("dong", "#C9A64A", r=0.3, m=1, ten="dong_den_chum")
    thuy = mat("kinh", "#F2EBD8", r=0.05, a=0.6, ten="chao_thuy_tinh_mo")
    bong = mat("den", "#FFF6E0", r=0.3, emis="#FFE8B0", ten="bong_nen")
    H = mx.z - mn.z
    # đế ốp trần (mặt trên = mx.z)
    lathe([(0, H), (0.07, H), (0.068, H - 0.02), (0.03, H - 0.035), (0, H - 0.035)], (cx, cy, mn.z), dong, n=32)
    # thân tiện giữa
    lathe([(0, H * 0.25), (0.012, H * 0.25), (0.03, H * 0.33), (0.02, H * 0.42), (0.008, H * 0.5), (0.008, H * 0.85), (0.015, H * 0.9), (0.008, H * 0.95), (0, H * 0.95)], (cx, cy, mn.z), dong, n=20)
    lathe([(0, 0), (0.02, 0.02), (0.035, 0.06), (0.03, 0.1), (0.012, H * 0.25), (0, H * 0.25)], (cx, cy, mn.z), dong, n=20)
    R = min(mx.x - mn.x, mx.y - mn.y) / 2 - 0.04
    for i in range(5):
        a = i * 2 * math.pi / 5
        ex, ey = cx + R * math.cos(a), cy + R * math.sin(a)
        tube([(cx, cy, mn.z + H * 0.36), (cx + R * 0.45 * math.cos(a), cy + R * 0.45 * math.sin(a), mn.z + H * 0.26), (cx + R * 0.85 * math.cos(a), cy + R * 0.85 * math.sin(a), mn.z + H * 0.28), (ex, ey, mn.z + H * 0.42)], 0.006, dong, n=8)
        lathe([(0, 0), (0.02, 0), (0.025, 0.01), (0.0, 0.012)], (ex, ey, mn.z + H * 0.42), dong, n=16)
        lathe([(0.012, 0.0), (0.03, 0.01), (0.055, 0.07), (0.05, 0.075), (0.026, 0.015), (0.012, 0.004)], (ex, ey, mn.z + H * 0.43), thuy, n=24)
        lathe([(0, 0), (0.012, 0), (0.014, 0.02), (0.01, 0.045), (0, 0.055)], (ex, ey, mn.z + H * 0.44), bong, n=12)
    return None

def quat_tran_than(c):
    mn, mx = c.mn, c.mx
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    son = mat("son", "#E7E3D6", r=0.35, ten="quat_tran_kem")
    kl = mat("kl", "#9EA2A4", r=0.35, m=0.9, ten="ty_quat")
    z1, z0 = mx.z, mn.z
    lathe([(0, 0), (0.06, 0), (0.06, -0.008), (0.035, -0.03), (0, -0.032)], (cx, cy, z1), son, n=32)
    cyl(0.011, (z1 - z0) * 0.55, (cx, cy, z0 + (z1 - z0) * 0.3), kl, n=16)
    lathe([(0, 0), (0.06, 0.0), (0.085, 0.02), (0.088, 0.045), (0.07, 0.065), (0.02, 0.075), (0, 0.075)], (cx, cy, z0), son, n=40)
    lathe([(0.0, 0.0), (0.089, 0.0), (0.089, 0.006), (0.0, 0.006)], (cx, cy, z0 + 0.03), mat("son", "#B9AA7E", r=0.35, ten="vien_quat"), n=40)
    return None

def quat_tran_canh(c):
    mn, mx = c.mn, c.mx
    son = mat("son", "#E7E3D6", r=0.35, ten="canh_quat_kem")
    kl = mat("kl", "#9EA2A4", r=0.35, m=0.9, ten="ty_quat")
    # cánh: bản mỏng hơi xoắn, bo đầu
    L = mx.x - mn.x; w = mx.y - mn.y
    bm = bmesh.new(); n = 12
    rows = []
    for i in range(n + 1):
        t = i / n; x = mn.x + 0.08 + (L - 0.08) * t
        hw = w / 2 * (0.75 + 0.25 * math.sin(t * math.pi * 0.9)) * (1 - 0.6 * max(0, t - 0.9) / 0.1)
        tw = math.radians(8) * (1 - t)
        rows.append([bm.verts.new((x, (mn.y + mx.y) / 2 + s * hw * math.cos(tw), (mn.z + mx.z) / 2 + s * hw * math.sin(tw))) for s in (-1, 1)])
    for a, b in zip(rows, rows[1:]): bm.faces.new((a[0], b[0], b[1], a[1]))
    o = mesh_bm(bm, "canh", son); solid(o, 0.004, 0); smooth(o, 40)
    box((0.12, 0.03, 0.006), (mn.x + 0.06, (mn.y + mx.y) / 2, (mn.z + mx.z) / 2), kl, b=0.002)
    for k in (0.02, 0.07):
        cyl(0.005, 0.004, (mn.x + 0.08 + k, (mn.y + mx.y) / 2, mx.z - 0.002), kl, n=8, b=0)
    return None

def hoa_tran(c):
    mn, mx = c.mn, c.mx
    vua = mat("xm", "#EEEBE2", r=0.8, ten="thach_cao_hoa_tran")
    R = (mx.x - mn.x) / 2; z = mx.z
    # bông hoa trần thạch cao: các vòng gờ + cánh hoa nổi
    lathe([(0, 0), (R, 0), (R, -0.004), (R * 0.9, -0.006), (R * 0.85, -0.004), (R * 0.6, -0.008), (R * 0.55, -0.004), (R * 0.3, -0.01), (R * 0.12, -0.012), (0.03, -0.006), (0, -0.006)], (0, 0, z), vua, n=64)
    for i in range(12):
        a = i * math.pi / 6
        p = sphere(R * 0.11, (R * 0.72 * math.cos(a), R * 0.72 * math.sin(a), z - 0.005), vua, scale=(1.8, 0.8, 0.35), n=12)
        p.rotation_euler.z = a
        q = sphere(R * 0.07, (R * 0.42 * math.cos(a + 0.26), R * 0.42 * math.sin(a + 0.26), z - 0.008), vua, scale=(1.6, 0.8, 0.35), n=10)
        q.rotation_euler.z = a + 0.26
    return None

def tran_giat_cap(c):
    """Khung trần giật cấp quanh phòng khách: hai bậc + gờ chỉ, hở giữa."""
    mn, mx = c.mn, c.mx
    vua = mat("xm", "#EEEBE2", r=0.85, ten="thach_cao_tran")
    W, D = mx.x - mn.x, mx.y - mn.y; z = mx.z
    # tính bề rộng dải từ file gốc: lỗ giữa ≈ ô rỗng
    b = 0.35
    for (w, d, h, k) in ((0, 0, 0.15, 0), (0.06, 0.06, 0.12, 1)):
        bb = b - k * 0.06
        box((W - w, bb, h), ((mn.x + mx.x) / 2, mn.y + w / 2 + bb / 2, z - h / 2), vua, b=0.004)
        box((W - w, bb, h), ((mn.x + mx.x) / 2, mx.y - w / 2 - bb / 2, z - h / 2), vua, b=0.004)
        box((bb, D - w - 2 * bb, h), (mn.x + w / 2 + bb / 2, (mn.y + mx.y) / 2, z - h / 2), vua, b=0.004)
        box((bb, D - w - 2 * bb, h), (mx.x - w / 2 - bb / 2, (mn.y + mx.y) / 2, z - h / 2), vua, b=0.004)
    # gờ chỉ mép lỗ
    for s in (-1, 1):
        box((W - 2 * b + 0.04, 0.025, 0.025), ((mn.x + mx.x) / 2, (mn.y + mx.y) / 2 + s * (D / 2 - b), z - 0.15 + 0.0125), vua, b=0.008, seg=3)
        box((0.025, D - 2 * b + 0.04, 0.025), ((mn.x + mx.x) / 2 + s * (W / 2 - b), (mn.y + mx.y) / 2, z - 0.15 + 0.0125), vua, b=0.008, seg=3)
    return None

CT = {
    "TV_CRT": tv_crt,
    "Salon_GheDai": salon_ghe_dai, "Salon_GheDon": salon_ghe_don,
    "Salon_Dem_NgoiDai_GanTV": dem_gan_tv, "Salon_Dem_NgoiDai_Kia": dem_kia, "Salon_Dem_NgoiDon": dem_don,
    "Salon_BanNuoc": ban_nuoc, "TuTV_Dung": tu_tv, "TuTV_CanhKinh_Trai": canh_kinh, "TuTV_CanhKinh_Phai": canh_kinh,
    "DauVCD": dau_vcd, "Loa_Thung": loa_thung, "DieuKhien": dieu_khien, "KhayAmChen": khay_am_chen,
    "DongHoQuaLac": dong_ho_qua_lac, "TranhTheu_Tex": tranh_theu, "AnhGiaDinh_Tex": anh_gia_dinh, "BangGDVH_Tex": bang_gdvh,
    "KhungNho_Khoi": khung_nho, "KhungNho_Nhim": khung_nho, "ChieuMen_KhachO": chieu_men, "BinhHoaLon_Gom": binh_hoa_lon,
    "DenChum_5Tay": den_chum, "QuatTran_Than": quat_tran_than, "QuatTran_Canh": quat_tran_canh, "HoaTran": hoa_tran,
    "TranGiatCap_Khung": tran_giat_cap,
}
