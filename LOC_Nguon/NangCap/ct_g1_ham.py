# NHÓM 1 · Hầm (bàn thờ mười ba năm không ai lau) + đồ điện dùng chung + cổng
from lib import *
from chung import *

def ban_tho_ham(c):
    """Bàn thờ gỗ mộc, không khăn, bụi phủ — đối cực với bàn thờ vong."""
    mn, mx = c.mn, c.mx
    go = mat("go", "#5E4A36", r=0.85, ten="go_moc_bui")
    bui = mat("xm", "#8E8676", r=1.0, ten="lop_bui", a=1.0)
    nhen = mat("vai", "#CFCBC0", r=1.0, ten="mang_nhen", a=0.45, hai_mat=True)
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z; cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    for sx in (-1, 1):
        for sy in (-1, 1):
            chan_thop(cx + sx * (W / 2 - 0.04), cy + sy * (D / 2 - 0.035), 0, H - 0.035, 0.045, 0.04, go, b=0.002)
    # mặt hai tấm ván, một tấm hơi cong vênh
    for k, s in enumerate((-1, 1)):
        o = box((W, D / 2 - 0.003, 0.032), (cx, cy + s * D / 4, H - 0.016), go, b=0.002)
        if k: o.rotation_euler.x = math.radians(0.6)
    for sy in (-1, 1): box((W - 0.1, 0.02, 0.07), (cx, cy + sy * (D / 2 - 0.035), H - 0.07), go, b=0.002)
    for sx in (-1, 1): box((0.02, D - 0.08, 0.07), (cx + sx * (W / 2 - 0.04), cy, H - 0.07), go, b=0.002)
    box((W - 0.08, 0.02, 0.025), (cx, cy, 0.12), go, b=0.002)
    # bụi đọng mép sau mặt bàn, chân bàn
    box((W - 0.02, 0.06, 0.002), (cx, mx.y - 0.035, H + 0.001), bui, b=0)
    # mạng nhện ở góc chân sau + dưới mặt
    for sx in (-1, 1):
        g = grid(0.18, 0.18, 6, 6, (0, 0, 0), nhen)
        x0 = cx + sx * (W / 2 - 0.04); y0 = mx.y - 0.035
        deform(g, lambda v, x0=x0, y0=y0, sx=sx: Vector((x0 - sx * (v.x + 0.09) * 0.8, y0 - (v.y + 0.09) * 0.25, H - 0.075 - (v.y + 0.09) * 0.8 + 0.02 * math.sin(v.x * 30))))
    return None

def bat_nhang_ham(c):
    mn, mx = c.mn, c.mx
    gom = mat("gom", "#6E5440", r=0.7, ten="bat_nhang_nau_cu")
    tro = mat("xm", "#9E978A", r=1.0, ten="tro_nen_chat")
    chan = mat("go", "#6E2E26", r=0.8, ten="chan_huong_cu_do")
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2; z0 = mn.z
    r = 0.09; h = 0.11
    lathe([(0, 0), (r * 0.7, 0), (r * 0.72, 0.008), (r * 1.0, h * 0.45), (r * 1.04, h * 0.6), (r * 1.0, h * 0.9), (r * 1.04, h), (r * 0.97, h), (r * 0.93, h * 0.9), (r * 0.9, h * 0.6), (0, h * 0.6)], (cx, cy, z0), gom, n=40)
    g = grid(r * 1.8, r * 1.8, 12, 12, (0, 0, 0), tro)
    rnd = random.Random(4)
    deform(g, lambda v: Vector((cx + v.x * min(1, (r * 0.92) / max(1e-4, (v.x ** 2 + v.y ** 2) ** 0.5)), cy + v.y * min(1, (r * 0.92) / max(1e-4, (v.x ** 2 + v.y ** 2) ** 0.5)), z0 + h - 0.02 + 0.004 * rnd.random())))
    # chân hương cũ: nhiều, dày đặc, gãy, bạc màu — 13 năm
    for i in range(22):
        a = rnd.uniform(0, 6.28); rr = rnd.uniform(0, r * 0.8)
        x, y = cx + rr * math.cos(a), cy + rr * math.sin(a); L = rnd.uniform(0.02, mx.z - (z0 + h) + 0.01)
        tx, ty = rnd.uniform(-0.012, 0.012), rnd.uniform(-0.012, 0.012)
        cyl(0.0015, L, (x, y, z0 + h - 0.02), chan, n=5, b=0).rotation_euler = (tx * 8, ty * 8, 0)
    return None

def den_ham(c):
    """Dây điện xoắn + dây kéo giữ bố cục gốc (vát mịn); bóng dây tóc + đui sứ dựng lại (node BongDayToc giữ riêng)."""
    out = nang_tu_goc(c, giu_node=["BongDayToc"], bo=("BongDayToc", "SoiToc", "DuiDen"))
    su = mat("gom", "#E8E2D2", r=0.4, ten="dui_su")
    kinh = mat("kinh", "#F4EAD0", r=0.05, a=0.55, ten="bong_day_toc")
    toc = mat("den", "#FFB060", emis="#FF9A40", ten="soi_toc")
    da, db = c.bb("DuiDen"); bb = c.bb("BongDayToc"); bc = (bb[0] + bb[1]) / 2
    lathe([(0, 0), (0.017, 0), (0.019, 0.01), (0.019, 0.04), (0.013, 0.05), (0.006, 0.058), (0, 0.06)], (bc.x, bc.y, da.z), su, n=20)
    zt = bb[1].z; hb = max(0.06, bb[1].z - bb[0].z)
    bong = lathe([(0, zt), (0.013, zt), (0.014, zt - hb * 0.15), (0.024, zt - hb * 0.35), (0.031, zt - hb * 0.6), (0.028, zt - hb * 0.82), (0.016, zt - hb * 0.96), (0, zt - hb)], (bc.x, bc.y, 0), kinh, n=28)
    s1 = tube([(bc.x - 0.006, bc.y, zt - hb * 0.4), (bc.x, bc.y, zt - hb * 0.55), (bc.x + 0.006, bc.y, zt - hb * 0.4)], 0.0007, toc, n=4)
    out["BongDayToc"] = [bong, s1]
    return out

def lib_bb(c, n):
    return c.bb(n)

def bien_so(c):
    mn, mx = c.mn, c.mx
    W, H = mx.x - mn.x, mx.z - mn.z; cx, zc = (mn.x + mx.x) / 2, (mn.z + mx.z) / 2
    nen = mat("son", "#1F4F8A", r=0.4, ten="bien_men_xanh")
    vien = mat("kl", "#C9CCCE", r=0.3, m=1, ten="vien_nhom")
    giu = c.giu_mat("BienSoNha", "Material_1") or []
    box((W, 0.004, H), (cx, mx.y - 0.002, zc), nen, b=0.003)
    for s in (-1, 1):
        box((W, 0.004, 0.006), (cx, mx.y - 0.005, zc + s * (H / 2 - 0.003)), vien, b=0.001)
        box((0.006, 0.004, H), (cx + s * (W / 2 - 0.003), mx.y - 0.005, zc), vien, b=0.001)
        cyl(0.004, 0.003, (cx + s * (W / 2 - 0.015), mx.y - 0.006, zc), vien, n=10, rot=(90, 0, 0))
    for p in giu: p.location.y -= 0.004
    return None

def tru_gach(c):
    """Trụ cổng gạch trát vữa sơn vàng ố, mũ trụ đổ bê tông có gờ, chân bẩn."""
    mn, mx = c.mn, c.mx
    vua = mat("xm", "#D5C39C", r=0.9, ten="vua_son_vang_o")
    be = mat("xm", "#B9AE96", r=0.9, ten="mu_tru_be_tong")
    ban = mat("xm", "#6E665A", r=1.0, ten="chan_tru_ban")
    W = mx.x - mn.x; H = mx.z - mn.z; cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    w = W - 0.04
    box((w, w, H - 0.14), (cx, cy, (H - 0.14) / 2), vua, b=0.006)
    box((w + 0.01, w + 0.01, 0.22), (cx, cy, 0.11), ban, b=0.006)
    # gờ chỉ ngang
    for z in (0.8, 1.5): box((w + 0.012, w + 0.012, 0.03), (cx, cy, z), vua, b=0.006)
    # mũ trụ: hai lớp + chóp vát
    box((W, W, 0.05), (cx, cy, H - 0.115), be, b=0.008)
    box((W - 0.03, W - 0.03, 0.04), (cx, cy, H - 0.07), be, b=0.008)
    bm = bmesh.new()
    s = (W - 0.05) / 2; z0 = H - 0.05
    vs = [bm.verts.new((cx + a * s, cy + b * s, z0)) for a, b in ((-1, -1), (1, -1), (1, 1), (-1, 1))]
    t = bm.verts.new((cx, cy, H))
    for i in range(4): bm.faces.new((vs[i], vs[(i + 1) % 4], t))
    bm.faces.new(vs[::-1]); o = mesh_bm(bm, "chop", be); bevel(o, 0.004, 1)
    return None

def hop_cong_to(c):
    mn, mx = c.mn, c.mx
    W, D, H = mx.x - mn.x, mx.y - mn.y, mx.z - mn.z; cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    son = mat("son", "#8E9A8A", r=0.5, ten="hop_cong_to_xam_xanh")
    ri = mat("sat", "#7A5A3E", r=0.9, ten="ri_set")
    kinh = mat("kinh", "#BFD0CC", r=0.05, a=0.35, ten="o_kinh_cong_to")
    box((W, D * 0.8, H), (cx, cy + D * 0.1, H / 2), son, b=0.006)
    box((W - 0.02, 0.012, H - 0.02), (cx, mn.y + 0.006, H / 2), son, b=0.004)
    box((0.12, 0.004, 0.08), (cx, mn.y - 0.001, H * 0.62), kinh, b=0.002)
    box((0.11, 0.003, 0.07), (cx, mn.y + 0.003, H * 0.62), mat("anh", "#FFFFFF", r=0.8, ten="mat_cong_to", img=__import__('hoavan').chu("mat_cong_to", ["0 4 8 2 7", "kWh"], 256, 160, "#E8E4D6", "#222222", 40)), b=0)
    for i in range(6): box((0.06, 0.003, 0.004), (cx, mn.y - 0.001, 0.06 + i * 0.012), son, b=0.0005)
    box((0.02, 0.012, 0.03), (cx + W / 2 - 0.03, mn.y - 0.004, H / 2), mat("kl", "#5A5A58", r=0.5, m=0.9, ten="khoa_hop"), b=0.003)
    box((W - 0.03, 0.002, 0.03), (cx, mn.y - 0.0005, 0.02), ri, b=0)
    for k in range(3): tube([(cx - 0.06 + k * 0.05, cy, 0), (cx - 0.06 + k * 0.05, cy, -0.0)], 0.005, mat("nhua", "#2A2A2A", r=0.5, ten="day_den"), n=5)
    return None

def bang_dien(c):
    mn, mx = c.mn, c.mx
    W, H = mx.x - mn.x, mx.z - mn.z; cx = (mn.x + mx.x) / 2
    go = mat("go", "#B7A27E", r=0.7, ten="bang_go_dien")
    su = mat("gom", "#EDE8DA", r=0.4, ten="cau_chi_su")
    den = mat("nhua", "#2A2A2A", r=0.5, ten="nhua_den")
    yb = mx.y   # tường phía +Y, thiết bị nhô về −Y
    box((W, 0.018, H), (cx, yb - 0.009, mn.z + H / 2), go, b=0.004)
    # cầu dao + cầu chì sứ + aptomat
    box((0.09, 0.05, 0.14), (cx - 0.06, yb - (0.018 + 0.025), mn.z + H * 0.7), su, b=0.006)
    box((0.012, 0.04, 0.09), (cx - 0.06, yb - (0.06), mn.z + H * 0.72), den, b=0.003)
    for i in range(2):
        box((0.035, 0.03, 0.06), (cx + 0.05 + i * 0.045, yb - (0.018 + 0.015), mn.z + H * 0.7), su, b=0.004)
    box((0.05, 0.05, 0.08), (cx + 0.05, yb - (0.043), mn.z + H * 0.35), mat("nhua", "#E6E2D6", r=0.4, ten="aptomat"), b=0.004)
    box((0.012, 0.015, 0.02), (cx + 0.05, yb - (0.072), mn.z + H * 0.36), den, b=0.002)
    for k in range(4):
        tube([(cx - 0.09 + k * 0.05, yb - (0.03), mn.z + H), (cx - 0.09 + k * 0.05, yb - (0.03), mn.z + H * 0.82)], 0.003, den, n=5)
    return None

def den_tuyp(c):
    mn, mx = c.mn, c.mx
    L = mx.x - mn.x; cy = (mn.y + mx.y) / 2
    mang = mat("son", "#ECEAE2", r=0.4, ten="mang_den_trang")
    ong = mat("den", "#F2F6F4", r=0.2, emis="#DDEBFF", ten="ong_tuyp_sang")
    box((L - 0.04, 0.05, 0.03), (mn.x + L / 2, cy, mx.z - 0.015), mang, b=0.006)
    box((0.08, 0.04, 0.03), (mn.x + L * 0.45, cy, mx.z - 0.03), mang, b=0.004)   # chấn lưu
    for x in (mn.x + 0.012, mx.x - 0.012):
        box((0.024, 0.04, 0.045), (x, cy, mx.z - 0.03), mat("nhua", "#E2DED2", r=0.4, ten="dau_dui_tuyp"), b=0.004)
    cyl(0.013, L - 0.04, (mn.x + 0.02, cy, mx.z - 0.04), ong, n=16, rot=(0, 90, 0), b=0)
    for x in (mn.x + 0.025, mx.x - 0.025): cyl(0.0135, 0.02, (x - 0.01 if x > mn.x + 0.1 else x - 0.01, cy, mx.z - 0.04), mat("kl", "#AAAAAA", r=0.4, m=0.9, ten="dau_ong_kl"), n=16, rot=(0, 90, 0), b=0)
    # vệt đen hai đầu ống (ống cũ)
    return None

def bong_compact(c):
    mn, mx = c.mn, c.mx
    cx, cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    ong = mat("den", "#F4F4F0", r=0.2, emis="#FFF3DA", ten="ong_compact")
    lathe([(0, 0), (0.012, 0.0), (0.014, 0.025), (0.019, 0.04), (0.019, 0.055), (0, 0.055)], (cx, cy, mn.z), mat("nhua", "#EDEBE4", r=0.4, ten="de_compact"), n=20)
    cyl(0.011, 0.02, (cx, cy, mn.z - 0.0), mat("kl", "#B9B9B4", r=0.3, m=1, ten="duoi_xoay"), n=16)
    # ống xoắn
    pts = []
    for i in range(160):
        t = i / 159; a = t * 2 * math.pi * 4.5
        pts.append((cx + 0.012 * math.cos(a), cy + 0.012 * math.sin(a), mn.z + 0.058 + t * (mx.z - mn.z - 0.066)))
    tube(pts, 0.0045, ong, n=8)
    return None

def o_cam(c):
    mn, mx = c.mn, c.mx
    W, H = mx.y - mn.y, mx.z - mn.z; cy, zc = (mn.y + mx.y) / 2, (mn.z + mx.z) / 2
    m = mat("nhua", "#EDE9DC", r=0.4, ten="mat_o_cam_kem"); den = mat("nhua", "#2A2A2A", r=0.6, ten="lo_cam")
    box((mx.x - mn.x, W, H), ((mn.x + mx.x) / 2, cy, zc), m, b=0.004, seg=2)
    for s in (-1, 1):
        for k in (-1, 1):
            cyl(0.0025, 0.002, (mx.x + 0.0005, cy + s * 0.012 + k * 0.004, zc), den, n=8, rot=(0, 90, 0), b=0)
    cyl(0.003, 0.0015, (mx.x, cy, zc + H * 0.35), mat("kl", "#AAAAAA", r=0.4, m=0.9, ten="vit"), n=8, rot=(0, 90, 0), b=0)
    return None

CT = {"BanThoHam": ban_tho_ham, "BatNhang_FullAssembly": bat_nhang_ham, "DenHam_FullAssembly": den_ham,
      "BienSoNha": bien_so, "CongSat_TruGach": tru_gach, "HopCongTo": hop_cong_to, "BangDienChinh": bang_dien,
      "DenTuyp_120": den_tuyp, "DenTuyp_60": den_tuyp, "BongCompact": bong_compact, "OCam_Doi": o_cam}
