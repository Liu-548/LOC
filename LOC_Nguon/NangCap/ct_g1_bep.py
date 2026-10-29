# NHÓM 1 · Bếp + chỗ ăn (GĐ5 prompt 06): bàn ăn + 4 ghế (mỗi ghế một dấu vết), khăn nilon kẻ ô, lồng bàn, tủ lạnh, bệ rửa, đồ điện nhỏ, lịch bloc
from lib import *
from chung import *
import hoavan

def GO_AN(): return mat("go", "#8A6440", r=0.5, ten="go_ban_an_venni_mo")
def GO_MON(): return mat("go", "#A5825A", r=0.35, ten="go_ban_an_mon")

def ban_an(c):
    mn, mx = c.mn, c.mx
    go = GO_AN()
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    a = 0.05
    for sx in (-1, 1):
        for sy in (-1, 1):
            chan_thop(cx + sx * (W / 2 - 0.05), cy + sy * (D / 2 - 0.05), 0, H - 0.03, a, a * 0.85, go)
    # mặt gỗ ghép 5 tấm
    n = 5
    for i in range(n):
        w = (D - 0.004) / n
        box((W, w - 0.002, 0.026), (cx, mn.y + 0.002 + w * (i + 0.5), H - 0.013), go, b=0.003)
    # yếm + giằng chân
    for sy in (-1, 1): box((W - 0.12, 0.022, 0.08), (cx, cy + sy * (D / 2 - 0.05), H - 0.07), go, b=0.003)
    for sx in (-1, 1):
        box((0.022, D - 0.12, 0.08), (cx + sx * (W / 2 - 0.05), cy, H - 0.07), go, b=0.003)
        box((0.025, D - 0.1, 0.03), (cx + sx * (W / 2 - 0.05), cy, 0.14), go, b=0.003)
    box((W - 0.1, 0.025, 0.03), (cx, cy, 0.14), go, b=0.003)
    return None

def _ghe_an(c, kieu):
    mn, mx = c.mn, c.mx
    go, mon = GO_AN(), GO_MON()
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z
    cx = (mn.x + mx.x) / 2
    yF, yB = mn.y + 0.035, mx.y - 0.025
    zs = 0.45; a = 0.035
    for sx in (-1, 1):
        chan_thop(cx + sx * (W / 2 - 0.025), yF, 0, zs - 0.02, a, a * 0.85, go)
        # chân sau liền trụ lưng, ngả nhẹ ra sau
        thanh((cx + sx * (W / 2 - 0.025), yB - 0.01, 0), (cx + sx * (W / 2 - 0.025), yB + 0.02, H - 0.005), a, go)
    # vai ghế + giằng
    for sx in (-1, 1): box((0.02, yB - yF, 0.05), (cx + sx * (W / 2 - 0.025), (yF + yB) / 2, zs - 0.045), go, b=0.003)
    box((W - 0.05, 0.02, 0.05), (cx, yF, zs - 0.045), go, b=0.003)
    for sx in (-1, 1): box((0.02, yB - yF, 0.022), (cx + sx * (W / 2 - 0.025), (yF + yB) / 2, 0.15), go, b=0.002)
    box((W - 0.05, 0.022, 0.022), (cx, yF, 0.2), go, b=0.002)
    # mặt ngồi gỗ phẳng
    box((W - 0.01, yB - yF + 0.05, 0.022), (cx, (yF + yB) / 2 - 0.005, zs - 0.011), go, b=0.004, seg=2)
    # lưng: thanh đỉnh cong + ba nan dọc
    def yb(z): return yB + 0.005 + (z - zs) * 0.065
    top = box((W - 0.02, 0.022, 0.06), (cx, yb(H - 0.04), H - 0.035), go, b=0.006, seg=2)
    box((W - 0.04, 0.02, 0.035), (cx, yb(zs + 0.1), zs + 0.1), go, b=0.003)
    for i, x in enumerate((-0.07, 0, 0.07)):
        M = mon if (kieu == "bo" and i == 1) else go     # ghế bố: nan giữa mòn bóng
        thanh((cx + x, yb(zs + 0.11), zs + 0.11), (cx + x, yb(H - 0.065), H - 0.065), 0.022, M, day=0.014)
    if kieu == "me":   # đệm mút mỏng bọc vải buộc trên mặt ngồi
        dem = mat("vai", "#7B5A6A", r=0.9, ten="dem_ghe_me")
        cushion((W - 0.04, yB - yF, 0.025), (cx, (yF + yB) / 2, zs + 0.012), dem, r=0.01, phong=0.004)
        for sx in (-1, 1):
            tube([(cx + sx * (W / 2 - 0.03), yB - 0.01, zs + 0.01), (cx + sx * (W / 2 - 0.022), yB + 0.005, zs - 0.01), (cx + sx * (W / 2 - 0.026), yB + 0.01, zs - 0.05)], 0.002, dem, n=5)
    if kieu == "nhim":  # gối nhỏ kê cho em với tới bàn + vết bút sáp ở chân
        goi = mat("vai", "#D9C9A0", r=0.9, ten="goi_ke_nhim")
        cushion((W - 0.08, yB - yF - 0.06, 0.07), (cx, (yF + yB) / 2 + 0.01, zs + 0.035), goi, r=0.025, phong=0.01)
        box((0.004, 0.0015, 0.03), (cx - W / 2 + 0.025, yF - 0.018, 0.25), mat("nhua", "#C9443A", r=0.8, ten="vet_but_sap"), b=0)
    if kieu == "bo":   # mặt ngồi xước
        for k in range(3):
            o = box((0.12, 0.0015, 0.0008), (cx - 0.04 + k * 0.03, (yF + yB) / 2 - 0.05 + k * 0.04, zs + 0.0004), mat("go", "#B08A60", r=0.6, ten="vet_xuoc_go"), b=0)
            o.rotation_euler.z = math.radians(20 + k * 9)
    return None

def ghe_bo(c): return _ghe_an(c, "bo")
def ghe_me(c): return _ghe_an(c, "me")
def ghe_khoi(c): return _ghe_an(c, "khoi")
def ghe_nhim(c): return _ghe_an(c, "nhim")

def khan_nilon(c):
    mn, mx = c.mn, c.mx
    img = hoavan.ke_o()
    M = mat("nhua", "#FFFFFF", r=0.35, ten="nilon_ke_o_do_trang", img=img, hai_mat=True)
    chay = mat("nhua", "#5A3A22", r=0.7, ten="vet_chay_xem", hai_mat=True)
    W, D = 1.10, 0.70
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    top = mn.z + 0.045
    g = khan_phu(W, D, top, cx, cy, 0.05, M, nx=60, ny=40, song=0.008)
    g.rotation_euler.z = math.radians(2.0); g["uvs"] = 4.0
    # góc cháy xém + vết mờ tròn chỗ đặt nồi
    o = cyl(0.03, 0.0008, (cx + W / 2 - 0.05, cy - D / 2 + 0.06, top + 0.0025), chay, n=12, b=0); o.scale = (1.3, 0.8, 1)
    cyl(0.11, 0.0005, (cx - 0.15, cy + 0.05, top + 0.0024), mat("nhua", "#D8CFC0", r=0.5, a=0.5, ten="vet_mo_noi"), n=32, b=0)
    return None

def long_ban(c):
    mn, mx = c.mn, c.mx
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    R = min(mx.x - mn.x, mx.y - mn.y) / 2 - 0.005; H = mx.z - mn.z - 0.03
    luoi = mat("nhua", "#D98E8E", r=0.5, ten="luoi_long_ban", img=_luoi_long(), cat_alpha=True, hai_mat=True)
    hong = mat("nhua", "#D98E8E", r=0.45, ten="nhua_hong")
    # vòm lưới
    pr = [(R * math.sin(math.radians(a)) if a < 90 else R, H * (math.cos(math.radians(a))) if a < 90 else 0) for a in range(0, 91, 6)]
    pr = [(r, z) for r, z in pr][::-1]
    v = lathe([(R, 0)] + [(R * math.cos(math.radians(t)), H * math.sin(math.radians(t))) for t in range(0, 91, 6)], (cx, cy, mn.z), luoi, n=48)
    v["uvs"] = 12.0
    # vành chân + gân
    lathe([(R - 0.004, 0), (R + 0.004, 0), (R + 0.004, 0.012), (R - 0.004, 0.012)], (cx, cy, mn.z), hong, n=48)
    for i in range(8):
        a = i * math.pi / 4
        tube([(cx + (R * math.cos(math.radians(t))) * math.cos(a), cy + (R * math.cos(math.radians(t))) * math.sin(a), mn.z + H * math.sin(math.radians(t))) for t in range(0, 91, 10)], 0.003, hong, n=6)
    # quai cầm ở đỉnh
    tube([(cx - 0.03, cy, mn.z + H), (cx - 0.025, cy, mn.z + H + 0.025), (cx + 0.025, cy, mn.z + H + 0.025), (cx + 0.03, cy, mn.z + H)], 0.006, hong, n=8)
    return None

def _luoi_long():
    import os
    from PIL import Image, ImageDraw
    p = os.path.join(hoavan.D, "luoi_long_ban.png")
    if os.path.exists(p): return p
    n = 256; im = Image.new("RGBA", (n, n), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    for i in range(0, n, 16):
        d.line((i, 0, i, n), fill=(255, 255, 255, 255), width=3); d.line((0, i, n, i), fill=(255, 255, 255, 255), width=3)
    d.rectangle((96, 112, 128, 128), fill=(0, 0, 0, 0))   # một mắt lưới rách
    d.line((96, 112, 110, 126), fill=(255, 255, 255, 255), width=2)
    im.save(p); return p

def tu_lanh(c):
    """Thân tủ lạnh một cánh 1,30 m (mặt trước y=0 để LocTuMo khoét hộc), khăn ren + lọ hoa nhựa + hộp bánh sắt tây trên nóc, dây điện thõng sau."""
    mn, mx = c.mn, c.mx
    vo = mat("son", "#E2DDCF", r=0.35, ten="vo_tu_lanh_nga")
    o_ = mat("son", "#CFC6AE", r=0.5, ten="vo_tu_lanh_o")
    den = mat("nhua", "#2A2A2A", r=0.6, ten="day_den")
    W = 0.58; Hf = 1.30; Dd = 0.60
    x0 = mn.x; cx = x0 + W / 2
    t = 0.03
    for x in (x0 + t / 2, x0 + W - t / 2): box((t, Dd, Hf - 0.05), (x, Dd / 2, 0.05 + (Hf - 0.05) / 2), vo, b=0.012, seg=3)
    box((W, Dd, t), (cx, Dd / 2, Hf - t / 2), vo, b=0.015, seg=3)
    box((W, Dd - 0.02, 0.05), (cx, Dd / 2 + 0.01, 0.025), mat("nhua", "#3A3A38", r=0.6, ten="chan_de_tu_lanh"), b=0.006)
    box((W - 2 * t, 0.02, Hf - 0.1), (cx, Dd - 0.01, 0.05 + (Hf - 0.1) / 2), vo, b=0.003)
    # lưới tản nhiệt + máy nén phía sau
    for i in range(14): box((W - 0.12, 0.006, 0.006), (cx, Dd + 0.025, 0.15 + i * 0.075), mat("kl", "#2F2F2F", r=0.5, m=0.8, ten="gian_nong"), b=0)
    for x in (cx - (W - 0.12) / 2, cx + (W - 0.12) / 2): box((0.008, 0.008, 1.0), (x, Dd + 0.025, 0.65), mat("kl", "#2F2F2F", r=0.5, m=0.8, ten="gian_nong"), b=0)
    sphere(0.09, (cx, Dd - 0.06, 0.15), mat("son", "#1E1E1E", r=0.5, ten="may_nen"), scale=(1.0, 0.8, 0.9), n=16)
    day_dien([(cx + 0.2, Dd + 0.03, 0.2), (cx + 0.23, mx.y - 0.01, 0.6), (cx + 0.25, mx.y - 0.005, 1.25)], r=0.004)
    # gioăng quanh miệng (ngoài ô hộc)
    for (sx, sz, w, h) in ((0, 1, W - 0.01, 0.012), (0, -1, W - 0.01, 0.012)):
        box((w, 0.008, h), (cx, 0.004, (0.02 if sz < 0 else Hf - 0.016)), vo, b=0.002)
    # ố vàng mép trên
    box((W - 0.02, 0.002, 0.04), (cx, -0.0005, Hf - 0.035), o_, b=0)
    # khăn ren + đồ trên nóc
    ren = mat("vai", "#E4DFD0", r=0.9, ten="ren_noc_tu_lanh", img=hoavan.ren(), cat_alpha=True, hai_mat=True)
    k = khan_phu(W * 0.82, Dd * 0.8, Hf, cx, Dd / 2, 0.06, ren, nx=30, ny=24, song=0.004); k["uvs"] = 3.0
    hop = mat("son", "#2D5F8A", r=0.35, ten="hop_banh_thiec_xanh")
    lathe([(0, 0), (0.1, 0), (0.1, 0.07), (0.103, 0.072), (0.103, 0.09), (0, 0.09)], (cx + 0.12, Dd * 0.55, Hf + 0.003), hop, n=40)
    lathe([(0.0, 0.0), (0.1005, 0.0), (0.1005, 0.03), (0.0, 0.03)], (cx + 0.12, Dd * 0.55, Hf + 0.03), mat("son", "#B8392E", r=0.35, ten="vien_hop_do"), n=40)
    lo = mat("nhua", "#E8E4D8", r=0.3, ten="lo_hoa_nhua")
    lathe([(0, 0), (0.035, 0), (0.045, 0.04), (0.03, 0.09), (0.02, 0.11), (0.025, 0.12), (0, 0.12)], (cx - 0.14, Dd * 0.45, Hf + 0.003), lo, n=24)
    hoa = mat("nhua", "#C9707A", r=0.5, ten="hoa_nhua_hong"); la = mat("nhua", "#5E7A4E", r=0.5, ten="la_nhua")
    rnd = random.Random(5)
    for i in range(5):
        a = i * 1.25; tx, ty = cx - 0.14 + 0.05 * math.cos(a), Dd * 0.45 + 0.05 * math.sin(a); tz = Hf + 0.13 + rnd.uniform(-0.015, 0.015)
        tube([(cx - 0.14, Dd * 0.45, Hf + 0.1), (tx, ty, tz - 0.02)], 0.002, la, n=4)
        sphere(0.022, (tx, ty, tz), hoa, scale=(1, 1, 0.6), n=12)
    return None

def tu_lanh_canh(c):
    mn, mx = c.mn, c.mx
    vo = mat("son", "#E2DDCF", r=0.35, ten="vo_tu_lanh_nga")
    o_ = mat("son", "#CFC6AE", r=0.5, ten="vo_tu_lanh_o")
    W = mx.x - mn.x; H = mx.z - mn.z; cx = (mn.x + mx.x) / 2
    yf = mn.y; d = 0.05
    box((W, d, H), (cx, yf + d / 2, mn.z + H / 2), vo, b=0.012, seg=3)
    # gân dập ngang + vạch ngăn đá trên
    box((W - 0.06, 0.004, 0.004), (cx, yf - 0.001, mn.z + H * 0.78), o_, b=0.001)
    # gioăng cao su phía sau
    box((W - 0.01, 0.012, H - 0.01), (cx, mx.y - 0.006, mn.z + H / 2), mat("caosu", "#C9C6BC", r=0.8, ten="gioang"), b=0.004)
    # tay nắm dọc (ố vàng quanh tay nắm) — phía xa bản lề
    hx = mn.x + 0.04
    box((0.03, 0.012, 0.35), (hx, yf - 0.004, mn.z + H * 0.55), o_, b=0.004)
    tay = box((0.022, 0.03, 0.30), (hx, yf - 0.018, mn.z + H * 0.55), mat("nhua", "#BDB6A2", r=0.4, ten="tay_nam_tu_lanh"), b=0.008, seg=3)
    # nam châm quả dứa / chuối / táo + tờ giấy ghi số
    dua = mat("nhua", "#D9A93A", r=0.5, ten="nc_dua"); chuoi = mat("nhua", "#E6CF4A", r=0.5, ten="nc_chuoi"); tao = mat("nhua", "#B8372E", r=0.45, ten="nc_tao"); la = mat("nhua", "#4E7A3A", r=0.5, ten="nc_la")
    sphere(0.018, (cx + 0.1, yf - 0.006, mn.z + H * 0.7), dua, scale=(0.8, 0.5, 1.2), n=12)
    sphere(0.01, (cx + 0.1, yf - 0.006, mn.z + H * 0.7 + 0.025), la, scale=(0.6, 0.4, 1.4), n=8)
    tube([(cx - 0.05, yf - 0.006, mn.z + H * 0.62), (cx - 0.03, yf - 0.007, mn.z + H * 0.6), (cx, yf - 0.006, mn.z + H * 0.61)], 0.007, chuoi, n=8)
    sphere(0.016, (cx + 0.15, yf - 0.006, mn.z + H * 0.5), tao, scale=(1, 0.5, 0.95), n=12)
    giay = mat("anh", "#FFFFFF", r=0.9, ten="giay_so_dien_thoai", img=hoavan.chu("giay_sdt", ["Bác Thu  ...  8 4 7 2 1 6", "Cô Hạnh  ...  8 2 9 . . .", "Gas  ...  8 6 3 3 0 5"], 256, 320, "#EDE7D3", "#2B3A6E", 22))
    p = plane(0.1, 0.125, (cx + 0.1, yf - 0.0035, mn.z + H * 0.64), giay); p.rotation_euler.y = math.radians(-4)
    return None

def be_rua(c):
    mn, mx = c.mn, c.mx
    gach = mat("gach", "#EEEDE6", r=0.25, ten="gach_men_trang_op")
    xm = mat("xm", "#8C877D", r=0.9, ten="xi_mang_gam")
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z; cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    # hai trụ xây hai bên, mặt bệ trên, gầm hở
    for sx in (-1, 1): box((0.1, D, H - 0.06), (cx + sx * (W / 2 - 0.05), cy, (H - 0.06) / 2), gach, b=0.004)
    box((W - 0.2, D - 0.06, 0.04), (cx, cy + 0.03, 0.12), xm, b=0.003)
    # mặt bệ có lỗ chậu (4 tấm quanh lỗ 0,52 × 0,42)
    hw, hd = 0.26, 0.21; zt = H - 0.03
    box((W, (D - 2 * hd) / 2 + 0.02, 0.06), (cx, mn.y + ((D - 2 * hd) / 2) / 2 - 0.01, zt), gach, b=0.006)
    box((W, (D - 2 * hd) / 2 - 0.02, 0.06), (cx, mx.y - ((D - 2 * hd) / 2) / 2 + 0.01, zt), gach, b=0.006)
    for sx in (-1, 1): box(((W - 2 * hw) / 2, 2 * hd, 0.06), (cx + sx * (hw + (W - 2 * hw) / 4), cy, zt), gach, b=0.006)
    # xô nhựa úp + chai nước rửa bát dưới gầm
    xo = mat("nhua", "#3E6E8E", r=0.5, ten="xo_xanh_up")
    o = lathe([(0, 0), (0.12, 0), (0.14, 0.26), (0.145, 0.27), (0, 0.27)], (0, 0, 0), xo, n=32); o.rotation_euler.x = math.pi; o.location = (cx - 0.12, cy + 0.05, 0.27 + 0.14)
    lathe([(0, 0), (0.032, 0), (0.035, 0.03), (0.035, 0.17), (0.02, 0.2), (0.012, 0.21), (0.012, 0.23), (0, 0.23)], (cx + 0.15, cy - 0.05, 0.14), mat("nhua", "#5DAE6B", r=0.3, ten="chai_rua_bat"), n=20)
    return None

def chau_inox(c):
    mn, mx = c.mn, c.mx
    ino = mat("inox", "#B4B7B2", r=0.3, m=1, ten="chau_inox")
    W, D = mx.x - mn.x, mx.y - mn.y; cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    z1 = mx.z; dep = 0.18
    # vành phẳng + lòng chậu hộp bo
    box((W, D, 0.004), (cx, cy, z1 - 0.002), ino, b=0.002)
    o = box((W - 0.06, D - 0.06, dep), (cx, cy, z1 - dep / 2 - 0.002), ino, b=0.03, seg=4)
    solid(o, 0.0012, 1)
    o.modifiers["bev"].limit_method = 'NONE'
    cyl(0.022, 0.006, (cx, cy, z1 - dep - 0.004), mat("kl", "#8E918C", r=0.4, m=1, ten="lo_xa"), n=24)
    # miếng rửa bát + khăn lau vắt ngang thành chậu (khô)
    box((0.07, 0.045, 0.02), (cx + W / 2 - 0.06, mn.y + 0.05, z1 + 0.01), mat("vai", "#E3C24A", r=0.95, ten="mieng_rua_bat"), b=0.006)
    k = grid(0.12, 0.25, 6, 12, (0, 0, 0), mat("vai", "#C9BFA8", r=0.95, ten="khan_lau_kho", hai_mat=True))
    deform(k, lambda v: Vector((cx - W / 2 + 0.03 + v.x * 0.2, cy + v.y, z1 + 0.003 - max(0, abs(v.x) - 0.0) * 0.6 + 0.004 * math.sin(v.y * 60))))
    solid(k, 0.003)
    return None

def voi_dong(c):
    mn, mx = c.mn, c.mx
    dong = mat("dong", "#9C7A3E", r=0.45, m=1, ten="voi_dong_xin")
    x, y = (mn.x + mx.x) / 2, mx.y - 0.006
    z0 = mn.z
    cyl(0.018, 0.012, (x, y, z0), dong, n=20)
    cyl(0.011, 0.11, (x, y, z0 + 0.01), dong, n=16)
    tube([(x, y, z0 + 0.11), (x, y - 0.01, z0 + 0.155), (x, y - 0.05, z0 + 0.165), (x, y - 0.09, z0 + 0.15), (x, y - 0.11, z0 + 0.12)], 0.0075, dong, n=10)
    # tay vặn chữ thập
    for a in (0, 90):
        o = box((0.05, 0.008, 0.008), (x, y, z0 + 0.13), dong, b=0.003); o.rotation_euler.z = math.radians(a)
    sphere(0.009, (x, y, z0 + 0.135), dong, n=12)
    return None

def noi_com(c):
    mn, mx = c.mn, c.mx
    cx, cy = (mn.x + mx.x) / 2 + 0.0, (mn.y + mx.y) / 2 + 0.02
    vo = mat("son", "#E6E0D0", r=0.35, ten="vo_noi_com_kem"); hoa = mat("son", "#6F8A5E", r=0.4, ten="hoa_noi_com")
    xam = mat("nhua", "#9E9A8E", r=0.5, ten="nhua_xam_noi")
    H = mx.z - mn.z; R = 0.14
    lathe([(0, 0), (R * 0.85, 0), (R * 0.95, 0.02), (R, 0.05), (R, H * 0.62), (R * 0.97, H * 0.68), (0, H * 0.68)], (cx, cy, mn.z), vo, n=40)
    lathe([(0.0, 0.0), (R + 0.0005, 0.0), (R + 0.0005, 0.035), (0.0, 0.035)], (cx, cy, mn.z + 0.07), hoa, n=40)
    # nắp vòm + núm
    lathe([(0, 0), (R * 0.97, 0), (R * 0.9, 0.03), (R * 0.6, 0.06), (R * 0.2, 0.072), (0, 0.074)], (cx, cy, mn.z + H * 0.68), vo, n=40)
    lathe([(0, 0), (0.025, 0), (0.02, 0.012), (0, 0.016)], (cx, cy, mn.z + H * 0.68 + 0.072), xam, n=16)
    # quai xách + bảng công tắc phía trước (đèn báo tắt)
    tube([(cx - R + 0.01, cy, mn.z + H * 0.64), (cx - R - 0.01, cy, mn.z + H * 0.75), (cx, cy, mn.z + H * 0.83), (cx + R + 0.01, cy, mn.z + H * 0.75), (cx + R - 0.01, cy, mn.z + H * 0.64)], 0.004, xam, n=8)
    box((0.07, 0.03, 0.06), (cx, cy - R + 0.005, mn.z + 0.08), xam, b=0.008)
    box((0.02, 0.012, 0.03), (cx, cy - R - 0.012, mn.z + 0.085), mat("nhua", "#3A3A38", r=0.4, ten="can_gat"), b=0.004)
    cyl(0.004, 0.004, (cx + 0.022, cy - R - 0.01, mn.z + 0.1), mat("nhua", "#5A2420", r=0.3, ten="den_bao_tat"), n=10, rot=(90, 0, 0))
    # dây cuộn gọn phía sau
    for k in range(3):
        tube([(cx + 0.03 * math.cos(t / 3), mx.y - 0.03 + 0.02 * math.sin(t / 3), mn.z + 0.01 + k * 0.006) for t in range(0, 20)], 0.003, mat("nhua", "#2A2A2A", r=0.5, ten="day_den"), n=5)
    return None

def phich_hoa(c):
    mn, mx = c.mn, c.mx
    cx, cy = mn.x + 0.06, (mn.y + mx.y) / 2
    pv = mat("son", "#8E3B34", r=0.4, ten="phich_son_do"); vang = mat("son", "#D9B45E", r=0.35, ten="hoa_vang")
    kl = mat("kl", "#B9BCB6", r=0.35, m=1, ten="nhom_phich")
    H = mx.z - mn.z
    lathe([(0, 0), (0.055, 0), (0.058, 0.015), (0.058, H * 0.08), (0.055, H * 0.09), (0.055, H * 0.78), (0.045, H * 0.84), (0.03, H * 0.88), (0.028, H * 0.92), (0, H * 0.92)], (cx, cy, mn.z), pv, n=36)
    lathe([(0, 0), (0.0585, 0), (0.0585, H * 0.08), (0, H * 0.08)], (cx, cy, mn.z), kl, n=36)
    # hoa in vàng (bông 5 cánh nổi rất mỏng)
    rnd = random.Random(2)
    for i in range(10):
        a = rnd.uniform(0, 6.28); z = mn.z + rnd.uniform(H * 0.15, H * 0.72)
        o = sphere(0.011, (cx + 0.0555 * math.cos(a), cy + 0.0555 * math.sin(a), z), vang, scale=(1, 1, 1), n=10); o.scale = (0.25, 1, 1); o.rotation_euler.z = a
    # quai
    tube([(cx + 0.055, cy, mn.z + H * 0.72), (cx + mx.x - cx - 0.008, cy, mn.z + H * 0.66), (mx.x - 0.008, cy, mn.z + H * 0.28), (cx + 0.055, cy, mn.z + H * 0.22)], 0.006, pv, n=8)
    # nút bấc
    lathe([(0, 0), (0.024, 0), (0.026, 0.03), (0.02, 0.04), (0, 0.042)], (cx, cy, mn.z + H * 0.9), mat("go", "#9C7A52", r=0.8, ten="nut_bac_phich"), n=16)
    return None

def am_nhom(c):
    mn, mx = c.mn, c.mx
    nh = mat("kl", "#B9BCB6", r=0.4, m=1, ten="am_nhom"); kh = mat("kl", "#3A3632", r=0.8, m=0.4, ten="day_am_khoi")
    q = mat("nhua", "#2A2A2A", r=0.5, ten="quai_am_den")
    cx, cy = 0.0, (mn.y + mx.y) / 2; H = mx.z - mn.z
    lathe([(0, 0), (0.075, 0), (0.085, 0.01), (0.088, 0.04), (0.08, 0.075), (0.06, 0.095), (0.04, 0.1), (0, 0.1)], (cx, cy, mn.z), nh, n=40)
    lathe([(0.0, 0.0), (0.0855, 0.0), (0.0885, 0.025), (0.0, 0.025)], (cx, cy, mn.z - 0.0005), kh, n=40)
    lathe([(0, 0), (0.042, 0), (0.038, 0.01), (0.012, 0.018), (0.014, 0.03), (0, 0.032)], (cx, cy, mn.z + 0.1), nh, n=24)
    tube([(cx + 0.075, cy, mn.z + 0.03), (cx + 0.12, cy, mn.z + 0.06), (cx + 0.16, cy, mn.z + 0.11), (mx.x - 0.01, cy, mn.z + 0.13)], 0.009, nh, n=10)
    tube([(cx - 0.06, cy, mn.z + 0.09), (cx - 0.07, cy, mn.z + 0.16), (cx + 0.02, cy, mn.z + 0.17), (cx + 0.04, cy, mn.z + 0.095)], 0.007, q, n=8)
    return None

def thung_rac(c):
    mn, mx = c.mn, c.mx
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    v = mat("nhua", "#4E7A5E", r=0.45, ten="thung_rac_xanh"); den = mat("nhua", "#2A2A2A", r=0.5, ten="ban_dap")
    H = mx.z - mn.z
    lathe([(0, 0), (0.12, 0), (0.135, H * 0.9), (0, H * 0.9)], (cx, cy, mn.z), v, n=36)
    lathe([(0, 0), (0.14, 0), (0.14, 0.015), (0.11, 0.03), (0, 0.035)], (cx, cy, mn.z + H * 0.9), v, n=36)
    box((0.1, 0.06, 0.012), (cx, mn.y + 0.03, mn.z + 0.012), den, b=0.004)
    box((0.02, 0.02, 0.22), (cx, mx.y - 0.01, mn.z + 0.15), den, b=0.004)
    return None

def lich_bloc(c):
    mn, mx = c.mn, c.mx
    cx = (mn.x + mx.x) / 2
    bia = mat("anh", "#FFFFFF", r=0.7, ten="bia_lich_phong_canh", img=_tranh_lich())
    to = mat("anh", "#FFFFFF", r=0.8, ten="to_lich_5_8", img=_to_lich())
    giay = mat("giay", "#EDE7D6", r=0.9, ten="giay_lich_nga")
    W, H = 0.20, 0.28; yb = mx.y
    box((W, 0.003, H), (cx, yb - 0.0015, mn.z + H / 2), mat("carton", "#BFB49C", ten="bia_cung"), b=0.0006)
    p = plane(W - 0.006, H * 0.48, (cx, yb - 0.0032, mn.z + H * 0.74), bia)
    # khối lịch 0,12 × 0,09 × 0,03 (nửa dưới) + tờ trên cùng tách riêng, mép trên hơi vểnh
    zc = mn.z + 0.075
    box((0.12, 0.028, 0.09), (cx, yb - 0.003 - 0.014, zc), giay, b=0.001)
    t = grid(0.12, 0.09, 6, 8, (0, 0, 0), to)
    deform(t, lambda v: Vector((cx + v.x, yb - 0.0315 - max(0, v.y - 0.02) ** 2 * 1.4, zc + v.y)))
    t["uv_giu"] = 1
    me = t.data; me.uv_layers.new(name="UVMap")
    for poly in me.polygons:
        for li in poly.loop_indices:
            co = me.vertices[me.loops[li].vertex_index].co
            me.uv_layers[0].data[li].uv = ((co.x - cx) / 0.12 + 0.5, (co.z - zc) / 0.09 + 0.5)
    # vụn giấy tờ đã xé ở mép trên
    rnd = random.Random(8)
    for i in range(7):
        o = box((rnd.uniform(0.006, 0.014), 0.0006, rnd.uniform(0.004, 0.008)), (cx - 0.055 + i * 0.018, yb - 0.031, zc + 0.046), giay, b=0); o.rotation_euler.y = rnd.uniform(-0.5, 0.5)
    # đinh treo
    cyl(0.003, 0.012, (cx, yb, mn.z + H - 0.01), mat("kl", "#6E6A64", r=0.5, m=0.9, ten="dinh"), n=8, rot=(90, 0, 0))
    return None

def _to_lich():
    import os
    from PIL import Image, ImageDraw, ImageFont
    p = os.path.join(hoavan.D, "to_lich_5_8_2002.png")
    if os.path.exists(p): return p
    w, h = 384, 288; im = Image.new("RGB", (w, h), (237, 231, 214)); d = ImageDraw.Draw(im)
    F = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"; F2 = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
    def t(s, y, sz, f=F, col=(43, 43, 43)):
        ft = ImageFont.truetype(f, sz); tw = d.textlength(s, font=ft); d.text(((w - tw) / 2, y), s, font=ft, fill=col)
    t("THÁNG 8 · 2002", 10, 22); t("5", 36, 150); t("THỨ HAI", 196, 26)
    d.line((30, 232, w - 30, 232), fill=(90, 90, 90), width=2)
    t("Âm lịch: 27 · tháng Sáu", 238, 18, F2); t("Nhâm Ngọ", 262, 16, F2)
    im.save(p); return p

def _tranh_lich():
    import os
    from PIL import Image, ImageDraw
    p = os.path.join(hoavan.D, "tranh_lich_song_nui.png")
    if os.path.exists(p): return p
    w, h = 384, 256; im = Image.new("RGB", (w, h)); d = ImageDraw.Draw(im)
    for y in range(h): d.line((0, y, w, y), fill=(int(170 + 60 * y / h), int(196 + 30 * y / h), int(220 - 40 * y / h)))
    d.polygon([(0, 150), (80, 70), (150, 140), (220, 60), (320, 150), (384, 110), (384, 256), (0, 256)], fill=(86, 118, 96))
    d.polygon([(0, 190), (120, 160), (260, 185), (384, 165), (384, 256), (0, 256)], fill=(70, 98, 120))
    d.rectangle((0, 210, w, h), fill=(110, 150, 170))
    im.save(p); return p

def bat_chao_an(c):
    mn, mx = c.mn, c.mx
    su = mat("gom", "#F0EEE6", r=0.15, ten="bat_su_trang"); vien = mat("gom", "#3E5E8A", r=0.15, ten="vien_xanh_bat")
    chao = mat("thuc", "#EDE6D2", r=0.6, ten="chao_dac")
    cx, cy = mn.x + 0.055, (mn.y + mx.y) / 2
    bat(0.055, 0.052, (cx, cy, mn.z), su, n=40)
    lathe([(0.051, 0.0), (0.0556, 0.0), (0.0556, 0.004), (0.051, 0.004)], (cx, cy, mn.z + 0.047), vien, n=40)
    g = grid(0.09, 0.09, 10, 10, (0, 0, 0), chao)
    deform(g, lambda v: Vector((cx + v.x * min(1, 0.046 / max(0.001, (v.x ** 2 + v.y ** 2) ** 0.5 + 0.0001)) if (v.x ** 2 + v.y ** 2) ** 0.5 > 0.046 else cx + v.x,
                               cy + v.y * min(1, 0.046 / max(0.001, (v.x ** 2 + v.y ** 2) ** 0.5 + 0.0001)) if (v.x ** 2 + v.y ** 2) ** 0.5 > 0.046 else cy + v.y,
                               mn.z + 0.04 + 0.004 * math.cos(v.x * 40) * math.cos(v.y * 30))))
    # thìa nhôm
    nh = mat("kl", "#C2C4C0", r=0.35, m=1, ten="thia_nhom")
    sphere(0.016, (cx + 0.01, cy - 0.01, mn.z + 0.043), nh, scale=(1.3, 0.8, 0.25), n=12)
    tube([(cx + 0.025, cy - 0.01, mn.z + 0.045), (cx + 0.06, cy - 0.01, mn.z + 0.05), (mx.x - 0.002, cy - 0.01, mn.z + 0.053)], 0.0028, nh, n=6)
    return None

CT = {
    "BanAn": ban_an, "GheAn_bo": ghe_bo, "GheAn_me": ghe_me, "GheAn_khoi": ghe_khoi, "GheAn_nhim": ghe_nhim,
    "KhanBan_Nilon": khan_nilon, "LongBan": long_ban, "TuLanh": tu_lanh, "TuLanh_Canh": tu_lanh_canh,
    "BeRua": be_rua, "ChauInox": chau_inox, "Voi": voi_dong, "NoiComDien": noi_com, "PhichHoa": phich_hoa, "AmNhom": am_nhom,
    "ThungRac": thung_rac, "LichBloc": lich_bloc, "BatChao_An": bat_chao_an,
}
