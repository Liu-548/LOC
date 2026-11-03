# [03/11] Vòng hoa tang chi tiết cao (bản "Thực tế"), dựng bằng Blender (bpy 5.2).
#  Chạy: python3 vong_hoa_hq.py -- <VongHoaTang_1.glb gốc> <ra_HQ.glb> [ra_xem_truoc.glb]
#  · Giữ nguyên hệ toạ độ / kích thước của vòng gốc (tâm vòng (0, 0, 1.116), mặt trước nhìn về −Y) → đặt chồng khít lên vật cũ.
#  · Bản HQ KHÔNG có băng chữ: lúc chạy, LocThucTe giữ băng chữ của vật gốc (đã có chữ "THÀNH KÍNH PHÂN ƯU") và thay phần còn lại.
#  · Cúc trắng là chính, xen cúc vàng, cúc nhỏ màu kem; viền lá dương xỉ; lõi rêu xốp; giá tre ba chân có đốt.
import bpy, bmesh, sys, math, random
from mathutils import Vector, Matrix, Quaternion

args = sys.argv[sys.argv.index('--') + 1:]
# [03/11 tối] --day: vòng ĐẶC (lõi kín hoa) cho VongHoaTang_2 — bản gốc _2 có tấm lót (_VanLot) phủ kín giữa vòng, _1 là vòng rỗng
DAY = '--day' in args; args = [x for x in args if x != '--day']
SRC, OUT = args[0], args[1]; OUT_XEM = args[2] if len(args) > 2 else None
rng = random.Random(7105)
rng_o = random.Random(4242)   # [03/11 tối] chọn ô texture cánh (tươi / úa mép / úa đầu / dập) — bộ sinh riêng để hình dáng vòng không đổi
def o_canh():
    x = rng_o.random(); return 0 if x < 0.45 else 1 if x < 0.75 else 2 if x < 0.92 else 3
TAM = Vector((0.0, -0.005, 1.116)); R_LOI = 0.405

def mat(ten, mau, nham=0.75, sss=0.0):
    m = bpy.data.materials.get(ten) or bpy.data.materials.new(ten); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*mau, 1); b.inputs["Roughness"].default_value = nham
    if sss > 0:
        try: b.inputs["Subsurface Weight"].default_value = sss; b.inputs["Subsurface Radius"].default_value = (0.02, 0.02, 0.015)
        except Exception: pass
    return m

def srgb(h):
    h = h.lstrip('#'); c = [int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    return tuple(x / 12.92 if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4 for x in c)

# vật liệu — tên khớp vật liệu cũ (dễ đối chiếu); Unity sẽ thay bằng vật liệu Lit của LocThucTe
M_TRANG = mat("nhua__hoa_cuc_trang_F2EEE5", srgb("#EFEBE1"), 0.7, 0.15)
M_TRANG2 = mat("hq__cuc_trang_nga", srgb("#E4DCC8"), 0.72, 0.15)
M_VANG = mat("nhua__hoa_cuc_vang_D8C77D", srgb("#E2C24E"), 0.7, 0.1)
M_NHO = mat("son__hoa_cuc_nho_E3D8C4", srgb("#E8DEC4"), 0.75, 0.1)
M_NHUY = mat("hq__nhuy_cuc", srgb("#B8A04A"), 0.8)
M_LA = mat("nhua__la_duong_xi_4E5F3E", srgb("#3D5434"), 0.6)
M_LOI = mat("Canh_Hoa", srgb("#2E3A26"), 0.95)
M_TRE = mat("go__gia_vong_hoa_tre_89714F", srgb("#9C8455"), 0.55)
MATS = [M_TRANG, M_TRANG2, M_VANG, M_NHO, M_NHUY, M_LA, M_LOI, M_TRE]
MI = {m.name: i for i, m in enumerate(MATS)}

bm = bmesh.new()
uv = bm.loops.layers.uv.new("UVMap")

def mat_phang(pts, mi, uvs=None):
    vs = [bm.verts.new(p) for p in pts]
    f = bm.faces.new(vs); f.material_index = mi; f.smooth = True
    if uvs:
        for l, u in zip(f.loops, uvs): l[uv].uv = u
    return f

def dai(pts_l, pts_r, mi):
    """dải tứ giác giữa hai đường (trái/phải)"""
    vl = [bm.verts.new(p) for p in pts_l]; vr = [bm.verts.new(p) for p in pts_r]; n = len(vl)
    for i in range(n - 1):
        f = bm.faces.new((vl[i], vr[i], vr[i + 1], vl[i + 1])); f.material_index = mi; f.smooth = True
        for l, (a, b) in zip(f.loops, ((0, i / (n - 1)), (1, i / (n - 1)), (1, (i + 1) / (n - 1)), (0, (i + 1) / (n - 1)))): l[uv].uv = (a, b)

def he_truc(z):
    """ma trận quay đưa trục +Z cục bộ về hướng z"""
    z = z.normalized(); x = Vector((0, 0, 1)).cross(z)
    if x.length < 1e-4: x = Vector((1, 0, 0))
    x.normalize(); y = z.cross(x)
    return Matrix((x, y, z)).transposed()

# ───── bông cúc: nhuỵ vòm + 3 tầng cánh hẹp cong (cúc đại đoá)
def bong_cuc(goc, truc, ban_kinh, mi, nho=False):
    M = he_truc(truc) @ Matrix.Rotation(rng.uniform(0, 6.283), 3, 'Z')
    s = ban_kinh
    # nhuỵ: vòm thấp
    n = 7 if nho else 9
    dinh = bm.verts.new(goc + M @ Vector((0, 0, s * 0.22)))
    vong = [bm.verts.new(goc + M @ Vector((math.cos(a) * s * 0.14, math.sin(a) * s * 0.14, s * 0.12))) for a in (2 * math.pi * i / n for i in range(n))]
    for i in range(n):
        f = bm.faces.new((dinh, vong[i], vong[(i + 1) % n])); f.material_index = MI["hq__nhuy_cuc"] if not nho else mi; f.smooth = True
    # cúc đại đoá: tầng trong cụp vào thành khối tròn che nhuỵ, tầng ngoài xoè và rủ đầu; cánh lòng máng (3 đỉnh ngang), đầu tù tròn
    tang = ((8, 0.40, 80, 0.34, +25), (11, 0.62, 62, 0.30, +10), (13, 0.84, 40, 0.27, -15), (14, 1.0, 16, 0.24, -30)) if not nho else ((7, 0.7, 62, 0.34, 0), (10, 1.0, 26, 0.28, -20))
    for k, (so, dai_c, nghieng, rong, cuon) in enumerate(tang):
        for i in range(so):
            a = 2 * math.pi * (i + 0.5 * k + rng.uniform(-0.25, 0.25)) / so
            L = s * dai_c * rng.uniform(0.85, 1.1); w = s * rong * rng.uniform(0.85, 1.15)
            el0 = math.radians(nghieng + rng.uniform(-8, 8))
            d = Vector((math.cos(a), math.sin(a), 0)); ngang = Vector((-math.sin(a), math.cos(a), 0))
            hang = []
            for j, t in enumerate((0.0, 0.5, 1.0)):
                el = el0 + t * math.radians(cuon)
                r = s * 0.12 + L * t * math.cos(el); h = s * 0.12 + L * t * math.sin(el)
                ww = w * (0.45, 1.0, 0.55)[j]
                c = d * r + Vector((0, 0, h))
                phap = Vector((-math.sin(el) * math.cos(a), -math.sin(el) * math.sin(a), math.cos(el)))   # pháp tuyến mặt cánh
                mang = phap * (-ww * 0.35)          # lòng máng: mép cao hơn sống cánh
                hang.append((goc + M @ (c + ngang * ww / 2), goc + M @ (c + mang), goc + M @ (c - ngang * ww / 2)))
            # đầu tù: một đỉnh ở mút
            tip = goc + M @ (d * (s * 0.12 + L * 1.06 * math.cos(el0 + math.radians(cuon))) + Vector((0, 0, s * 0.12 + L * 1.06 * math.sin(el0 + math.radians(cuon)))))
            V = [[bm.verts.new(p) for p in h_] for h_ in hang]; vt = bm.verts.new(tip)
            # UV cánh (CanhHoa.png 4 ô ngang): u ngang cánh trong ô, v gốc (0) → mút (1)
            o_ = o_canh(); U = lambda q: (o_ + 0.04 + 0.92 * q / 2) / 4
            for j in range(2):
                for q in range(2):
                    f = bm.faces.new((V[j][q], V[j + 1][q], V[j + 1][q + 1], V[j][q + 1])); f.material_index = mi; f.smooth = True
                    for l, (qq, jj) in zip(f.loops, ((q, j), (q, j + 1), (q + 1, j + 1), (q + 1, j))): l[uv].uv = (U(qq), jj * 0.47)
            for q in range(2):
                f = bm.faces.new((V[2][q], vt, V[2][q + 1])); f.material_index = mi; f.smooth = True
                for l, uu in zip(f.loops, ((U(q), 0.94), (U(1), 1.0), (U(q + 1), 0.94))): l[uv].uv = uu

# ───── lõi vòng: xuyến dẹt rêu/xốp (nhiễu bề mặt)
N, Mr, RT = 64, 12, 0.058
vv = []
for i in range(N):
    a = 2 * math.pi * i / N; ring = []
    for j in range(Mr):
        b = 2 * math.pi * j / Mr; nz = 1 + 0.12 * math.sin(7 * a + 3 * b) * math.cos(5 * b)
        rr = R_LOI + RT * math.cos(b) * nz; yy = RT * 0.7 * math.sin(b) * nz
        ring.append(bm.verts.new(TAM + Vector((rr * math.cos(a), yy, rr * math.sin(a)))))
    vv.append(ring)
for i in range(N):
    for j in range(Mr):
        f = bm.faces.new((vv[i][j], vv[(i + 1) % N][j], vv[(i + 1) % N][(j + 1) % Mr], vv[i][(j + 1) % Mr])); f.material_index = MI["Canh_Hoa"]; f.smooth = True

# ───── lá dương xỉ viền ngoài + viền trong
def la_duong_xi(goc, huong, ngang, L):
    """cuống + các lá chét so le, cong nhẹ về sau (+Y)"""
    sau = Vector((0, 1, 0)); n = 9
    tr = [goc + huong * (L * t) + sau * (0.03 * t * t) for t in (i / n for i in range(n + 1))]
    for i in range(1, n):
        t = i / n; lc = L * 0.32 * (1 - t) + 0.012
        for s in (-1, 1):
            p = tr[i]; tip = p + (ngang * s * lc + huong * lc * 0.45); mid = (p + tip) / 2 + sau * 0.004
            w = lc * 0.28
            mat_phang([p - huong * w * 0.5, mid + huong * w * 0.5 * 0 + (huong * w), tip, mid - huong * w * 0.3], MI["nhua__la_duong_xi_4E5F3E"])
    dai([q + ngang * 0.002 for q in tr], [q - ngang * 0.002 for q in tr], MI["nhua__la_duong_xi_4E5F3E"])

for i in range(64):
    a = 2 * math.pi * (i + rng.uniform(-0.35, 0.35)) / 64
    rad = Vector((math.cos(a), 0, math.sin(a))); tan = Vector((-math.sin(a), 0, math.cos(a)))
    huong = (rad + tan * rng.uniform(-0.5, 0.5) + Vector((0, 0.15, 0))).normalized()
    la_duong_xi(TAM + rad * (R_LOI + 0.04) + Vector((0, 0.025, 0)), huong, tan, rng.uniform(0.10, 0.17))
for i in range(28):
    a = 2 * math.pi * (i + rng.uniform(-0.3, 0.3)) / 28
    rad = Vector((math.cos(a), 0, math.sin(a))); tan = Vector((-math.sin(a), 0, math.cos(a)))
    la_duong_xi(TAM + rad * (R_LOI - 0.05) + Vector((0, 0.02, 0)), (-rad + Vector((0, 0.1, 0))).normalized(), tan, rng.uniform(0.05, 0.08))

# ───── hoa mặt trước (−Y): 3 vòng bông lớn + cúc nhỏ chen kẽ
def huong_bong(a, lech_r, mat_truoc=True):
    rad = Vector((math.cos(a), 0, math.sin(a)))
    v = Vector((0, -1 if mat_truoc else 1, 0)) + rad * lech_r + Vector((rng.uniform(-0.15, 0.15), 0, rng.uniform(-0.15, 0.15)))
    return v.normalized()

for (rv, so, rb, lech) in ((R_LOI - 0.045, 24, 0.052, -0.35), (R_LOI + 0.005, 27, 0.058, 0.0), (R_LOI + 0.057, 30, 0.052, 0.45)):
    for i in range(so):
        a = 2 * math.pi * (i + rng.uniform(-0.3, 0.3) + (0.5 if so == 30 else 0)) / so
        p = TAM + Vector((math.cos(a) * rv, -RT * 0.55 - rng.uniform(0, 0.015), math.sin(a) * rv))
        k = rng.random(); mi = MI["nhua__hoa_cuc_vang_D8C77D"] if k < 0.16 else (MI["hq__cuc_trang_nga"] if k < 0.42 else MI["nhua__hoa_cuc_trang_F2EEE5"])
        bong_cuc(p, huong_bong(a, lech), rb * rng.uniform(0.88, 1.12), mi)
for i in range(45):   # cúc nhỏ lấp khe
    a = rng.uniform(0, 6.283); rv = R_LOI + rng.uniform(-0.07, 0.08)
    p = TAM + Vector((math.cos(a) * rv, -RT * 0.75 - 0.01, math.sin(a) * rv))
    bong_cuc(p, huong_bong(a, rng.uniform(-0.4, 0.4)), rng.uniform(0.022, 0.03), MI["son__hoa_cuc_nho_E3D8C4"], nho=True)
for i in range(14):   # mặt sau thưa hơn
    a = 2 * math.pi * (i + rng.uniform(-0.3, 0.3)) / 14
    p = TAM + Vector((math.cos(a) * R_LOI, RT * 0.5, math.sin(a) * R_LOI))
    bong_cuc(p, huong_bong(a, 0.1, False), 0.045, MI["hq__cuc_trang_nga"])

# ───── [03/11 tối] vòng đặc: tấm lót rêu kín lòng vòng + các vòng cúc phủ kín giữa (giống VongHoaTang_2 gốc)
if DAY:
    n_l = 40
    for yl, xuoi in ((0.012, True), (0.016, False)):   # hai mặt (mặt sau lùi 4 mm, đỉnh riêng)
        tam_l = bm.verts.new(TAM + Vector((0, yl, 0)))
        vien_l = [bm.verts.new(TAM + Vector((math.cos(2 * math.pi * k / n_l) * (R_LOI + 0.01), yl, math.sin(2 * math.pi * k / n_l) * (R_LOI + 0.01)))) for k in range(n_l)]
        for k in range(n_l):
            q = (tam_l, vien_l[k], vien_l[(k + 1) % n_l]) if xuoi else (tam_l, vien_l[(k + 1) % n_l], vien_l[k])
            f = bm.faces.new(q); f.material_index = MI["Canh_Hoa"]; f.smooth = True
    bong_cuc(TAM + Vector((0, -0.03, 0)), Vector((0, -1, 0)), 0.06, MI["nhua__hoa_cuc_vang_D8C77D"])
    for (rv, so, rb) in ((0.10, 7, 0.050), (0.19, 13, 0.052), (0.28, 19, 0.054)):
        for i in range(so):
            a = 2 * math.pi * (i + rng.uniform(-0.3, 0.3)) / so
            p = TAM + Vector((math.cos(a) * rv, -0.022 - rng.uniform(0, 0.012), math.sin(a) * rv))
            k = rng.random(); mi = MI["nhua__hoa_cuc_vang_D8C77D"] if k < 0.14 else (MI["hq__cuc_trang_nga"] if k < 0.40 else MI["nhua__hoa_cuc_trang_F2EEE5"])
            bong_cuc(p, huong_bong(a, rv * 0.5), rb * rng.uniform(0.9, 1.1), mi)
    for i in range(26):   # cúc nhỏ lấp khe trong lòng
        a = rng.uniform(0, 6.283); rv = rng.uniform(0.05, R_LOI - 0.08)
        p = TAM + Vector((math.cos(a) * rv, -0.03, math.sin(a) * rv))
        bong_cuc(p, huong_bong(a, rng.uniform(-0.3, 0.3)), rng.uniform(0.022, 0.03), MI["son__hoa_cuc_nho_E3D8C4"], nho=True)

# ───── giá tre ba chân có đốt
def ong(a, b, r, mi, so=8, dot=0.32):
    truc = (b - a); L = truc.length; M = he_truc(truc)
    n_doan = max(2, int(L / 0.08)); vs = []
    for k in range(n_doan + 1):
        t = k / n_doan; pz = L * t
        phinh = 1.18 if (pz % dot) < 0.012 else 1.0      # đốt tre
        vs.append([bm.verts.new(a + M @ Vector((math.cos(2 * math.pi * j / so) * r * phinh, math.sin(2 * math.pi * j / so) * r * phinh, pz))) for j in range(so)])
    for k in range(n_doan):
        for j in range(so):
            f = bm.faces.new((vs[k][j], vs[k][(j + 1) % so], vs[k + 1][(j + 1) % so], vs[k + 1][j])); f.material_index = mi; f.smooth = True
            for l, (u_, v_) in zip(f.loops, ((j / so, k / n_doan), ((j + 1) / so, k / n_doan), ((j + 1) / so, (k + 1) / n_doan), (j / so, (k + 1) / n_doan))): l[uv].uv = (u_, v_ * L * 3)
mt = MI["go__gia_vong_hoa_tre_89714F"]
dinh = Vector((0, 0.0, 1.03))
ong(Vector((-0.30, -0.11, 0.0)), dinh + Vector((-0.04, 0, 0)), 0.013, mt)
ong(Vector((0.30, -0.11, 0.0)), dinh + Vector((0.04, 0, 0)), 0.013, mt)
ong(Vector((0.0, 0.27, 0.0)), dinh + Vector((0, 0.02, 0.02)), 0.012, mt)
ong(Vector((-0.24, -0.09, 0.30)), Vector((0.24, -0.09, 0.30)), 0.009, mt)
ong(Vector((-0.17, -0.07, 0.62)), Vector((0.17, -0.07, 0.62)), 0.009, mt)

bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-6)
me = bpy.data.meshes.new("VongHoaTang_1_HQ"); bm.to_mesh(me); bm.free()
for m in MATS: me.materials.append(m)
ob = bpy.data.objects.new("VongHoaTang_1_HQ", me)

bpy.ops.wm.read_factory_settings(use_empty=True) if False else None
sc = bpy.context.scene
for o in list(sc.objects): bpy.data.objects.remove(o)
sc.collection.objects.link(ob)
print("tris", sum(len(p.vertices) - 2 for p in me.polygons))

def xuat(path, objs):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.ops.export_scene.gltf(filepath=path, use_selection=True, export_format='GLB', export_yup=True, export_apply=True)

# LOD1: rút gọn ~30% số tam giác (xa > vài mét; cũng dùng làm bóng đổ cho đỡ tốn)
lod = ob.copy(); lod.data = ob.data.copy(); lod.name = "VongHoaTang_1_HQ_LOD1"; lod.data.name = "VongHoaTang_1_HQ_LOD1"; sc.collection.objects.link(lod)
md = lod.modifiers.new("giam", 'DECIMATE'); md.ratio = 0.3
bpy.context.view_layer.objects.active = lod; bpy.ops.object.modifier_apply(modifier="giam")
print("tris LOD1", sum(len(p.vertices) - 2 for p in lod.data.polygons))
xuat(OUT, [ob, lod])
if OUT_XEM:
    # bản xem trước: thêm băng chữ của vòng gốc
    bpy.ops.import_scene.gltf(filepath=SRC)
    goc = [o for o in sc.objects if o.type == 'MESH' and o != ob][0]
    b2 = bmesh.new(); b2.from_mesh(goc.data)
    giu = [i for i, s in enumerate(goc.material_slots) if s.material and s.material.name.startswith("Bang_Chu")]
    bmesh.ops.delete(b2, geom=[f for f in b2.faces if f.material_index not in giu], context='FACES')
    b2.to_mesh(goc.data); b2.free()
    xuat(OUT_XEM, [ob, goc])
    sc.collection.objects.unlink(lod) if lod.name in sc.collection.objects else None
print("xong")
