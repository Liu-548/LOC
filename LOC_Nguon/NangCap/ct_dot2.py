# ĐỢT 2 (29/10 tối) · dựng tay các món ngoài sân/đám + sập gỗ T3 còn thô sau đợt nâng tự động:
#   ghế nhựa tựa lưng (đỏ/xanh) + chồng 4/8 · bàn nhựa tròn chân inox (trần / phủ khăn) · vòng hoa cúc tươi/héo · sập gỗ.
# Giữ bbox món gốc (Unity đặt theo gốc toạ độ). Băng chữ vòng hoa dùng vật liệu "Bang_Chu_Vong_Hoa" — LocCotTruyen thay ảnh chữ lúc chạy.
from lib import *
from chung import *
import random

# ───────── ghế nhựa tựa lưng kiểu Duy Tân/Song Long cuối 199x
def _rrect(w, d, r, n=4):
    """Điểm chu vi hình chữ nhật bo góc (ngược chiều kim), tâm 0."""
    pts = []
    for cx, cy, a0 in ((w / 2 - r, -d / 2 + r, -90), (w / 2 - r, d / 2 - r, 0), (-w / 2 + r, d / 2 - r, 90), (-w / 2 + r, -d / 2 + r, 180)):
        for i in range(n + 1):
            a = math.radians(a0 + 90 * i / n); pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts

def _lang_tru(pts, z0, z1, M, name="tru", top_fn=None):
    bm = bmesh.new()
    d = [bm.verts.new((x, y, z0)) for x, y in pts]
    t = [bm.verts.new((x, y, z1 if not top_fn else top_fn(x, y))) for x, y in pts]
    n = len(pts)
    for i in range(n): bm.faces.new((d[i], d[(i + 1) % n], t[(i + 1) % n], t[i]))
    bm.faces.new(d[::-1]); bm.faces.new(t)
    return mesh_bm(bm, name, M)

def _vay(pts, z0, z1, M, t=0.003, name="vay"):
    """Diềm mỏng chạy theo chu vi (thành đứng)."""
    bm = bmesh.new(); n = len(pts)
    d = [bm.verts.new((x, y, z0)) for x, y in pts]; u = [bm.verts.new((x, y, z1)) for x, y in pts]
    for i in range(n): bm.faces.new((d[i], d[(i + 1) % n], u[(i + 1) % n], u[i]))
    o = mesh_bm(bm, name, M); solid(o, t, 1); smooth(o, 50); return o

def _chan_u(p0, p1, a0, a1, M):
    """Chân ghế nhựa: tiết diện chữ U thóp dần (mặt hở quay vào trong để chồng ghế)."""
    p0, p1 = Vector(p0), Vector(p1); ax = (p1 - p0)
    bm = bmesh.new()
    ring = []
    for t, (a, b) in ((0, (a0, a0 * 0.8)), (1, (a1, a1 * 0.8))):
        c = p0 + ax * t
        # chữ U: 8 điểm (ngoài 4 + lõm trong 4) trong mặt phẳng ngang
        ring.append([bm.verts.new(c + Vector(v)) for v in
                     ((-a / 2, -b / 2, 0), (a / 2, -b / 2, 0), (a / 2, b / 2, 0), (a * 0.3, b / 2, 0), (a * 0.3, -b * 0.15, 0),
                      (-a * 0.3, -b * 0.15, 0), (-a * 0.3, b / 2, 0), (-a / 2, b / 2, 0))])
    d, u = ring; n = 8
    for i in range(n): bm.faces.new((d[i], d[(i + 1) % n], u[(i + 1) % n], u[i]))
    bm.faces.new(d[::-1]); bm.faces.new(u)
    o = mesh_bm(bm, "chan_u", M); bevel(o, 0.0025, 1, 50); return o

def _thanh_cong(w, h, t, zc, y0, cong, M, nx=18, name="nan"):
    """Nan tựa lưng cong theo bề ngang (võng ra sau giữa)."""
    g = grid(w, h, nx, 2, (0, 0, 0), M, name=name)
    deform(g, lambda v: Vector((v.x, y0 + cong * (1 - (2 * v.x / w) ** 2), zc + v.y)))
    solid(g, t, 0); bevel(g, min(0.004, t * 0.4), 2, 30); smooth(g, 40); return g

def ghe_nhua(z0=0.0, y0=0.0, M=None, mn=None, mx=None):
    W = mx.x - mn.x; H = 0.802
    yF, yB = mn.y + 0.012, mn.y + 0.40      # chân trước/sau (chân sau chạm sàn ở y≈0,213)
    zS = 0.43                                # mặt ngồi
    o = []
    # mặt ngồi: tấm bo góc, hơi lõm giữa, vạt trước cong xuống + diềm dưới
    sw, sd = W - 0.012, 0.37; scy = mn.y + 0.012 + sd / 2
    pts = [(x, y + scy) for x, y in _rrect(sw, sd, 0.05)]
    seat = _lang_tru(pts, zS + z0 - 0.006, zS + z0, M, "mat_ngoi",
                     top_fn=lambda x, y: zS + z0 - 0.008 * (1 - min(1, (2 * x / sw) ** 2)) * (1 - min(1, (2 * (y - scy) / sd) ** 2)))
    bevel(seat, 0.003, 2, 30); o.append(seat)
    o.append(_vay(pts, zS + z0 - 0.04, zS + z0 - 0.004, M, 0.003, "dien_mat"))
    # gân dưới mặt ngồi (chữ thập + vành trong)
    o.append(box((sw - 0.06, 0.008, 0.025), (0, scy, zS + z0 - 0.018), M, b=0.002))
    o.append(box((0.008, sd - 0.06, 0.025), (0, scy, zS + z0 - 0.018), M, b=0.002))
    o.append(_vay([(x * 0.7, (y - scy) * 0.7 + scy) for x, y in pts], zS + z0 - 0.03, zS + z0 - 0.006, M, 0.003, "vanh_trong"))
    # dấu logo dập nổi giữa mặt ngồi + 4 lỗ thoát nước
    cyl(0.03, 0.0015, (0, scy, zS + z0 - 0.007), M, n=24, b=0)
    for sx in (-1, 1):
        for sy in (-1, 1):
            cyl(0.006, 0.002, (sx * 0.08, scy + sy * 0.08, zS + z0 - 0.0075), mat("nhua", "#1E1A18", r=0.8, ten="lo_toi"), n=8, b=0)
    # bốn chân U, xoè ra ngoài
    hx = W / 2 - 0.016
    for sx in (-1, 1):
        o.append(_chan_u((sx * (sw / 2 - 0.035), scy - sd / 2 + 0.04, zS + z0 - 0.02), (sx * hx, yF + 0.012, z0 + 0.0), 0.042, 0.03, M))
        o.append(_chan_u((sx * (sw / 2 - 0.035), scy + sd / 2 - 0.04, zS + z0 - 0.02), (sx * hx, mn.y + 0.40, z0 + 0.0), 0.042, 0.03, M))
        # đế chân (miếng nhựa mòn sẫm hơn)
        Mx = mat("nhua", "#5A4A42", r=0.7, ten="de_chan_mon")
        cyl(0.018, 0.006, (sx * hx, yF + 0.012, z0), Mx, n=10, b=0.001)
        cyl(0.018, 0.006, (sx * hx, mn.y + 0.40, z0), Mx, n=10, b=0.001)
    # cột tựa: từ mép sau mặt ngồi, ngả ra sau tới y max ở đỉnh
    ytop = mx.y - 0.02 + y0
    yb = scy + sd / 2 - 0.02
    for sx in (-1, 1):
        x = sx * (sw / 2 - 0.022)
        o.append(tube([(x, yb, zS + z0 - 0.01), (x, yb + 0.02, zS + z0 + 0.12), (x, ytop - 0.01, z0 + H - 0.05)], 0.014, M, n=10))
    # tựa lưng: thanh trên to bo tròn + 3 nan ngang cong, khe hở giữa
    def yz(z): return yb + (ytop - yb) * (z - zS - z0) / (H - zS)
    for zc, hh, tt in ((H - 0.035, 0.065, 0.018), (H - 0.13, 0.04, 0.012), (H - 0.205, 0.04, 0.012), (H - 0.28, 0.04, 0.012)):
        if zc + z0 - zS - z0 < 0.08: continue
        o.append(_thanh_cong(sw - 0.01, hh, tt, zc + z0, yz(zc + z0), 0.02, M))
    return o

def ghe(c):
    M = mat("nhua", "#B33A2A" if "Red" in c.ten else "#2F7A5D", r=0.42, ten=("ghe_nhua_do" if "Red" in c.ten else "ghe_nhua_xanh"))
    ghe_nhua(0, 0, M, c.mn, c.mx)

def chong_ghe(c):
    n = 8 if c.ten.endswith("_8") else 4
    M = mat("nhua", "#B33A2A" if n == 8 else "#2F7A5D", r=0.42, ten=("ghe_nhua_do" if n == 8 else "ghe_nhua_xanh"))
    dz = (c.mx.z - 0.802) / (n - 1)
    mx1 = c.mx.copy(); mx1.y = c.mn.y + 0.424
    for i in range(n):
        dy = (c.mx.y - mx1.y) * i / max(1, n - 1)
        parts = ghe_nhua(i * dz, 0, M, c.mn, mx1)
        for p in parts: p.location.y += dy

# ───────── bàn nhựa tròn chân inox (bàn thuê đám)
def _ban(c, top_z=0.72, r=0.38):
    Mn = mat("nhua", "#E5DECA", r=0.45, ten="mat_ban_nhua_kem")
    Mi = mat("inox", "#A9ADB0", r=0.28, m=1.0, ten="chan_ban_inox")
    Mc = mat("caosu", "#2A2624", r=0.9, ten="nut_chan_cao_su")
    cx, cy = c.cx, c.cy
    # mặt bàn: tiện với mép cuộn tròn + vành gân dưới
    lathe([(0, top_z - 0.004), (r - 0.01, top_z - 0.004), (r, top_z - 0.008), (r + 0.004, top_z - 0.02), (r - 0.002, top_z - 0.032),
           (r - 0.012, top_z - 0.03), (r - 0.012, top_z - 0.012), (0, top_z - 0.012)], (cx, cy, 0), Mn, n=64)
    lathe([(0, top_z), (r - 0.012, top_z), (r - 0.004, top_z - 0.002), (r - 0.002, top_z - 0.005), (0, top_z - 0.005)], (cx, cy, 0), Mn, n=64)
    for k in range(8):
        a = k * math.pi / 4
        o = box((r * 0.85, 0.008, 0.022), (cx + math.cos(a) * r * 0.45, cy + math.sin(a) * r * 0.45, top_z - 0.022), Mn, b=0.002, rot=(0, 0, math.degrees(a)))
    cyl(0.07, 0.03, (cx, cy, top_z - 0.042), Mn, n=24, b=0.004)     # mâm đỡ
    # trụ inox + ống lồng
    cyl(0.026, top_z - 0.09, (cx, cy, 0.05), Mi, n=24, b=0.002)
    cyl(0.031, 0.05, (cx, cy, top_z - 0.09), Mi, n=24, b=0.003)
    cyl(0.045, 0.03, (cx, cy, 0.03), Mi, n=24, b=0.004)
    # chân chữ thập dẹt + nút cao su
    L = r * 0.92
    for k in range(4):
        a = k * math.pi / 2 + math.pi / 4
        ex, ey = cx + math.cos(a) * L, cy + math.sin(a) * L
        o = thanh((cx, cy, 0.04), (ex, ey, 0.018), 0.05, Mi, b=0.004, day=0.016)
        cyl(0.016, 0.018, (ex, ey, 0), Mc, n=12, b=0.002)

def ban_nhua(c):
    _ban(c, c.mx.z, (c.sz.x) / 2)

def ban_phu_khan(c):
    # bàn (node SM_PlasticTable giữ riêng) + khăn trải kẻ ca-rô đỏ trắng phủ rủ (node PlasticTable_set)
    lib_before = len(__import__('lib')._parts)
    top = c.mx.z - 0.004; r = 0.38
    _ban(c, top, r)
    import lib as L
    ban = L._parts[lib_before:]
    Mk = mat("vai", "#E8E2D6", r=0.9, ten="khan_ban_trang_nga", hai_mat=True)
    ru = 0.24; N = 44
    g = grid(2 * (r + ru), 2 * (r + ru), N, N, (0, 0, 0), Mk, name="khan")
    rng = random.Random(7)
    ph = [rng.uniform(0, 6.28) for _ in range(3)]
    def f(v):
        x, y = v.x, v.y; d = math.hypot(x, y); a = math.atan2(y, x)
        if d <= r: return Vector((c.cx + x, c.cy + y, top + 0.003))
        k = min(1.0, (d - r) / ru)
        # khăn vuông rủ: góc rủ dài hơn cạnh, nếp xoè theo góc
        dr = d - r
        song = 0.01 * math.sin(a * 9 + ph[0]) * k + 0.004 * math.sin(a * 17 + ph[1]) * k
        rr = r + 0.006 + 0.008 * math.sin(k * math.pi / 2) + song
        z = top + 0.003 - dr * 1.02
        return Vector((c.cx + math.cos(a) * rr, c.cy + math.sin(a) * rr, max(z, 0.2)))
    deform(g, f); smooth(g, 80); solid(g, 0.002, -1)
    khan = L._parts[-1:]
    return {"SM_PlasticTable": ban, "PlasticTable_set": khan}

# ───────── vòng hoa cúc (tươi / héo)
def _bong_cuc(r, M, Mn, rng, heo=False):
    """Bông cúc: đĩa cánh xếp tầng (lathe có răng) + nhuỵ; trục bông theo +Z cục bộ."""
    n = 10
    bm = bmesh.new()
    rings = []
    prof = [(0.0, 0.6), (0.45, 0.58), (1.0, 0.18), (0.9, 0.0)] if not heo else [(0.0, 0.38), (0.45, 0.34), (0.95, 0.02), (0.82, -0.2)]
    for j, (pr, pz) in enumerate(prof):
        ring = []
        for i in range(n):
            a = 2 * math.pi * i / n
            k = 1 + (0.18 if (i + j) % 2 else -0.1) * (j / len(prof))      # răng cánh
            if heo: k *= 1 + rng.uniform(-0.15, 0.08)
            ring.append(bm.verts.new((math.cos(a) * pr * r * k, math.sin(a) * pr * r * k, pz * r)))
        rings.append(ring)
    for j in range(len(rings) - 1):
        for i in range(n):
            bm.faces.new((rings[j][i], rings[j][(i + 1) % n], rings[j + 1][(i + 1) % n], rings[j + 1][i]))
    bm.faces.new(rings[0])
    o = mesh_bm(bm, "bong", M); smooth(o, 70)
    return o

def vong_hoa(c):
    heo = "Heo" in c.ten
    rng = random.Random(11 if heo else 5)
    if heo:
        Mt = [mat("vai", "#C9B88E", r=0.95, ten="cuc_heo_nau"), mat("vai", "#A88F62", r=0.95, ten="cuc_heo_sam"), mat("vai", "#D6CBB0", r=0.95, ten="cuc_heo_nhat")]
        Mla = mat("vai", "#5B5634", r=0.9, ten="la_heo_ua")
        Mnh = mat("vai", "#7A6236", r=0.9, ten="nhuy_heo")
    else:
        Mt = [mat("vai", "#F2EEE5", r=0.85, ten="cuc_trang"), mat("vai", "#F2EEE5", r=0.85, ten="cuc_trang"), mat("vai", "#E3C65A", r=0.85, ten="cuc_vang")]
        Mla = mat("vai", "#3F5A34", r=0.8, ten="la_duong_xi")
        Mnh = mat("vai", "#C9A23A", r=0.8, ten="nhuy_vang")
    Mx = mat("vai", "#3A4A2E", r=0.95, ten="xop_rom_buoc")
    Mt_tre = mat("go", "#B39A63", r=0.7, ten="tre_gia_vong")
    Md = mat("kl", "#6A6A64", r=0.5, m=0.8, ten="day_thep_buoc")
    cx = (c.mn.x + c.mx.x) / 2
    R, rt = 0.40, 0.068            # bán kính tâm ống vòng, nửa bề dày ống
    zc, yc = 0.80, -0.02
    # lõi vòng: rơm/xốp bó (xuyến dẹt)
    bm = bmesh.new(); N, Mr = 48, 10
    vs = []
    for i in range(N):
        a = 2 * math.pi * i / N; ring = []
        for j in range(Mr):
            b = 2 * math.pi * j / Mr
            rr = R + rt * math.cos(b) * 1.05; yy = rt * 0.75 * math.sin(b)
            ring.append(bm.verts.new((cx + rr * math.cos(a), yc + yy, zc + rr * math.sin(a))))
        vs.append(ring)
    for i in range(N):
        for j in range(Mr):
            bm.faces.new((vs[i][j], vs[(i + 1) % N][j], vs[(i + 1) % N][(j + 1) % Mr], vs[i][(j + 1) % Mr]))
    lo = mesh_bm(bm, "loi_vong", Mx); smooth(lo, 60)
    # lá dương xỉ viền ngoài (tấm lá nhọn chĩa ra)
    for i in range(36):
        a = 2 * math.pi * (i + rng.uniform(-0.3, 0.3)) / 36
        L = rng.uniform(0.06, 0.085) * (0.85 if heo else 1)
        bm = bmesh.new()
        p = [bm.verts.new(v) for v in ((-0.018, 0, 0), (0.018, 0, 0), (0.012, 0, L * 0.6), (0, 0, L), (-0.012, 0, L * 0.6))]
        bm.faces.new(p); la = mesh_bm(bm, "la", Mla); solid(la, 0.002, 0)
        rr = R + rt * 0.8
        la.location = (cx + rr * math.cos(a), yc + rng.uniform(-0.03, 0.03), zc + rr * math.sin(a))
        rx = (rng.uniform(-25, 25) + (35 if heo else 0))
        la.rotation_euler = (Euler((0, math.pi / 2 - a, 0)).to_matrix() @ Euler((math.radians(rx), 0, 0)).to_matrix()).to_euler()
    # bông cúc hai mặt: 3 hàng theo bán kính × mỗi mặt
    for mt in (-1, 1):
        rows = ((-0.04, 22, 0.03), (0.0, 26, 0.035), (0.045, 28, 0.032)) if mt < 0 else ((-0.03, 20, 0.033), (0.035, 24, 0.034))
        for row, (dr, nn, rb) in enumerate(rows):
            for i in range(nn):
                a = 2 * math.pi * (i + 0.5 * row + rng.uniform(-0.2, 0.2)) / nn
                k = rng.random()
                Mi = Mt[2] if (k < (0.12 if not heo else 0.35)) else Mt[0 if rng.random() < 0.6 else 1]
                rb2 = rb * rng.uniform(0.85, 1.15)
                b = _bong_cuc(rb2, Mi, Mnh, rng, heo)
                rr = R + dr
                # pháp tuyến bông: ra mặt (±Y) + nghiêng theo hàng
                n = Vector((math.cos(a) * dr * 6, mt * 1.0, math.sin(a) * dr * 6)).normalized()
                if heo: n = (n + Vector((0, 0, -0.5 - rng.random() * 0.4))).normalized()   # héo: gục xuống
                b.location = (cx + rr * math.cos(a), yc + mt * rt * 0.62, zc + rr * math.sin(a))
                b.rotation_euler = n.to_track_quat('Z', 'Y').to_euler()
                nh = sphere(rb2 * 0.28, (0, 0, 0), Mnh, scale=(1, 1, 0.5), n=6)
                nh.location = Vector(b.location) + n * rb2 * 0.5
    # băng chữ mặt trước: dải vải quấn ngang nửa dưới vòng (chữ chỉ mặt trước, mặt sau vải trơn) + hai đuôi rủ
    Mb = mat("vai", "#B5402F", r=0.85, ten="=Bang_Chu_Vong_Hoa", hai_mat=False)
    Mv = mat("vai", "#A8362A", r=0.85, ten="bang_vai_do", hai_mat=True)
    if heo: Mv = mat("vai", "#8E3A2E", r=0.9, ten="bang_vai_do_bac", hai_mat=True)
    bw, bh = 0.56, 0.14
    zb = zc - R * 0.62
    bm = bmesh.new(); uvl = bm.loops.layers.uv.new("UVMap")
    nx = 12; top = []; bot = []
    for i in range(nx + 1):
        u = i / nx; x = cx - bw / 2 + bw * u
        yy = yc - rt * 0.95 - 0.035 - 0.03 * (1 - (2 * u - 1) ** 2)          # phồng ra trước giữa
        bot.append(bm.verts.new((x, yy, zb - bh / 2 + (0.006 * math.sin(u * 9) if heo else 0))))
        top.append(bm.verts.new((x, yy, zb + bh / 2)))
    for i in range(nx):
        f = bm.faces.new((bot[i], bot[i + 1], top[i + 1], top[i]))
        for l, (uu, vv) in zip(f.loops, ((i / nx, 0), ((i + 1) / nx, 0), ((i + 1) / nx, 1), (i / nx, 1))): l[uvl].uv = (uu, vv)
    bm.normal_update()
    for f in bm.faces:
        if f.normal.y > 0: f.normal_flip()
    bang = mesh_bm(bm, "bang_chu", Mb); bang["uv_giu"] = 1
    # mặt sau của băng (vải trơn) + hai mép cuốn vào vòng
    tb = box((bw, 0.004, bh), (cx, yc - rt * 0.95 - 0.033, zb), Mv, b=0.001)
    for sx in (-1, 1):
        xx = cx + sx * bw / 2
        tube([(xx, yc - rt - 0.04, zb), (xx + sx * 0.01, yc, zb + 0.01), (xx, yc + rt + 0.01, zb)], 0.012, Mv, n=6)
        # đuôi băng rủ, xẻ đuôi cá
        bm = bmesh.new(); L = 0.42 if not heo else 0.4
        x0 = cx + sx * 0.06
        pts = [(x0 - 0.035, 0), (x0 + 0.035, 0), (x0 + 0.035 + sx * 0.03, -L), (x0 + sx * 0.03, -L + 0.04), (x0 - 0.035 + sx * 0.03, -L)]
        vv = [bm.verts.new((x, yc - rt - 0.045 - (0.01 * math.sin(-z * 8) if heo else 0), zb - bh / 2 + z)) for x, z in pts]
        bm.faces.new(vv); d = mesh_bm(bm, "duoi_bang", Mv); solid(d, 0.002, 0)
    # giá ba chân tre: hai chân trước sau lưng vòng + một chân chống sau, buộc dây thép ở đỉnh
    apex = Vector((cx, yc + rt + 0.03, zc + 0.34))
    feet = [Vector((cx - 0.27, yc + rt + 0.06, 0)), Vector((cx + 0.29, yc + rt + 0.05, 0)), Vector((cx - 0.03, c.mx.y, 0))]
    for k, f in enumerate(feet):
        top = apex + (apex - f).normalized() * 0.05
        tube([tuple(f), tuple(top)], 0.011 if k < 2 else 0.01, Mt_tre, n=8)
        # đốt tre
        for t in (0.25, 0.55, 0.8):
            p = f.lerp(top, t); cyl(0.0125, 0.008, (p.x, p.y, p.z - 0.004), Mt_tre, n=8, b=0.001)
    tube([(apex.x - 0.02, apex.y - 0.01, apex.z), (apex.x + 0.02, apex.y + 0.01, apex.z + 0.01), (apex.x - 0.015, apex.y + 0.012, apex.z - 0.015)], 0.003, Md, n=4)
    # thanh ngang đỡ vòng + dây buộc vòng vào chân
    tube([(cx - 0.22, yc + rt + 0.045, zc - 0.25), (cx + 0.23, yc + rt + 0.045, zc - 0.25)], 0.008, Mt_tre, n=6)
    for sx in (-1, 1):
        tube([(cx + sx * 0.2, yc + rt * 0.5, zc - 0.25 - 0.02), (cx + sx * 0.2, yc + rt + 0.05, zc - 0.25), (cx + sx * 0.2, yc + rt * 0.5, zc - 0.25 + 0.02)], 0.0025, Md, n=4)

# ───────── sập gỗ: mặt ghép 4 tấm dày, diềm chạm vòm, chân quỳ thấp
def sap_go(c):
    Mg = mat("go", "#7A4A2C", r=0.38, ten="sap_go_gu")
    Mm = mat("go", "#8C5A36", r=0.3, ten="sap_go_mat_mon_bong")
    x0, y0, x1, y1 = c.mn.x, c.mn.y, c.mx.x, c.mx.y
    W, D, H = x1 - x0, y1 - y0, c.mx.z
    cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
    t = 0.045
    # 4 tấm ván theo chiều dài, khe 3 mm, giữa hơi mòn bóng (vết ngồi)
    nv = 5; gap = 0.003; bw = (D - 0.02 - gap * (nv - 1)) / nv   # số lẻ: tâm sập là ván liền (tia rà soát đặt đồ lên sập không lọt khe)
    for i in range(nv):
        yy = y0 + 0.01 + bw / 2 + i * (bw + gap)
        o = box((W - 0.02, bw, t), (cx, yy, H - t / 2), Mm if i in (1, 2, 3) else Mg, b=0.004)
    # nẹp mép quanh mặt (gờ chỉ)
    for sy in (-1, 1):
        box((W, 0.022, t + 0.01), (cx, cy + sy * (D / 2 - 0.011), H - (t + 0.01) / 2), Mg, b=0.005)
    for sx in (-1, 1):
        box((0.022, D, t + 0.01), (cx + sx * (W / 2 - 0.011), cy, H - (t + 0.01) / 2), Mg, b=0.005)
    # diềm (yếm) dưới mặt, mép dưới cắt vòm
    hz = 0.085; zt = H - t - 0.008
    def diem(L, along_x, pos):
        bm = bmesh.new(); n = 40; top = []; bot = []
        for i in range(n + 1):
            u = i / n; s = -L / 2 + L * u
            arch = 0.035 * max(0, math.sin(u * math.pi)) ** 0.6 * (1 if 0.12 < u < 0.88 else 0)
            arch += 0.01 * abs(math.sin(u * math.pi * 7)) * (1 if 0.12 < u < 0.88 else 0)
            zb = zt - hz + arch
            p0 = (pos[0] + (s if along_x else 0), pos[1] + (0 if along_x else s))
            bot.append(bm.verts.new((p0[0], p0[1], zb))); top.append(bm.verts.new((p0[0], p0[1], zt)))
        for i in range(n): bm.faces.new((bot[i], bot[i + 1], top[i + 1], top[i]))
        o = mesh_bm(bm, "diem", Mg); solid(o, 0.022, 0); bevel(o, 0.003, 1, 50); return o
    ins = 0.04
    diem(W - 2 * ins, True, (cx, y0 + ins)); diem(W - 2 * ins, True, (cx, y1 - ins))
    diem(D - 2 * ins, False, (x0 + ins, cy)); diem(D - 2 * ins, False, (x1 - ins, cy))
    # chân quỳ: khối vuông dưới diềm, cong ra ngoài ở bàn chân (cuộn)
    for sx in (-1, 1):
        for sy in (-1, 1):
            px, py = cx + sx * (W / 2 - ins - 0.03), cy + sy * (D / 2 - ins - 0.03)   # bàn chân cuộn ra vẫn nằm trong mép mặt
            prof = [(0.0, 0.0), (0.38, 0.12), (0.6, 0.4), (0.62, 0.7), (0.5, 1.0)]
            bm = bmesh.new(); rings = []
            for k, (o_, zf) in enumerate(prof):
                z = zf * (zt - 0.0)
                a = 0.055 - 0.012 * math.sin(zf * math.pi)
                off = 0.035 * (1 - zf) ** 2            # bàn chân cuộn ra ngoài
                ccx, ccy = px + sx * off, py + sy * off
                rings.append([bm.verts.new((ccx + ex * a / 2, ccy + ey * a / 2, z)) for ex, ey in ((-1, -1), (1, -1), (1, 1), (-1, 1))])
            for k in range(len(rings) - 1):
                for i in range(4): bm.faces.new((rings[k][i], rings[k][(i + 1) % 4], rings[k + 1][(i + 1) % 4], rings[k + 1][i]))
            bm.faces.new(rings[0][::-1]); bm.faces.new(rings[-1])
            o = mesh_bm(bm, "chan_quy", Mg); bevel(o, 0.006, 2, 30); smooth(o, 40)
    # đố giằng dưới mặt (2 thanh ngang)
    for k in (-1, 1):
        box((0.04, D - 2 * ins, 0.03), (cx + k * W * 0.2, cy, zt - 0.03), Mg, b=0.003)

CT = {"SM_PlasticChair_Red": ghe, "SM_PlasticChair_Green": ghe,
      "SM_PlasticChairStack_4": chong_ghe, "SM_PlasticChairStack_8": chong_ghe,
      "SM_PlasticTable": ban_nhua, "SM_PlasticTable_TableCover": ban_phu_khan,
      "VongHoa_Tuoi": vong_hoa, "VongHoa_Heo": vong_hoa, "SapGo": sap_go}
