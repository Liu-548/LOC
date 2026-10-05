# Hũ cốt LỘC (bpy). Blender: Z lên, đơn vị mét, gốc ở tâm đáy. MẶT TRƯỚC (phía góc giấy được lật) = −Y.
# Con: Hu_Than (men ngoài / đất nung miệng / lòng tối), Hu_GiayDau (phần còn lại), Hu_GiayDau_Goc (góc lật, gốc ở bản lề),
#      Hu_Niem_Sau / Hu_Niem_Duoi (dán chặt) / Hu_Niem_Goc (phần niêm trên góc lật, rách theo), Hu_Lat, Hu_Toc, Hu_ChiDo, Hu_Cot (mảnh xương dưới tóc)
import bpy, bmesh, math, random, os, sys
from mathutils import Vector, Matrix, noise
random.seed(3)
OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "/home/claude/hu/out"
bpy.ops.wm.read_factory_settings(use_empty=True)
N = 64
def mat(n, rgb):
    m = bpy.data.materials.new(n); m.diffuse_color = (*rgb, 1); return m

# ── thân: tiện theo biên dạng (r, z). ngoài → mép miệng → lòng
OUTER = [(0.0, 0.0), (0.068, 0.0), (0.078, 0.004), (0.084, 0.014), (0.094, 0.035), (0.106, 0.07), (0.114, 0.11), (0.117, 0.15),
         (0.113, 0.185), (0.101, 0.215), (0.086, 0.238), (0.071, 0.254), (0.064, 0.264), (0.063, 0.276), (0.066, 0.283),
         (0.072, 0.287), (0.074, 0.293), (0.072, 0.299)]
RIM = [(0.066, 0.302), (0.059, 0.301)]
INNER = [(0.056, 0.292), (0.056, 0.275), (0.062, 0.262), (0.08, 0.24), (0.097, 0.21), (0.106, 0.16), (0.104, 0.10), (0.088, 0.05), (0.06, 0.025), (0.0, 0.02)]
prof = OUTER + RIM + INNER
mi = [0] * (len(OUTER) - 1) + [1] * (len(RIM) + 1) + [2] * (len(INNER) - 1)   # chỉ số vật liệu theo đoạn biên dạng
# chân đất nung: đoạn ngoài dưới z 0.03 → vật liệu 1
for k in range(len(OUTER) - 1):
    if OUTER[k + 1][1] <= 0.035: mi[k] = 1
# v theo chiều dài biên dạng ngoài (cho texture men: 0 đáy → 1 miệng)
acc = [0.0]
for a, b in zip(prof, prof[1:]): acc.append(acc[-1] + math.dist(a, b))
Lout = acc[len(OUTER) - 1]
bm = bmesh.new(); uvl = bm.loops.layers.uv.new()
rows = []
for k, (r, z) in enumerate(prof):
    row = []
    for i in range(N):
        a = 2 * math.pi * i / N
        rr = r * (1 + 0.012 * noise.noise(Vector((math.cos(a) * 3, math.sin(a) * 3, z * 20))))   # thân nặn tay hơi méo
        row.append(bm.verts.new((rr * math.cos(a), rr * math.sin(a), z)))
    rows.append(row)
for k in range(len(prof) - 1):
    for i in range(N):
        j = (i + 1) % N
        f = bm.faces.new((rows[k][i], rows[k][j], rows[k + 1][j], rows[k + 1][i]))
        f.material_index = mi[k]
        for lp, (ii, kk) in zip(f.loops, [(i, k), (i + 1, k), (i + 1, k + 1), (i, k + 1)]):
            lp[uvl].uv = (ii / N, min(acc[kk] / Lout, 1.0) if kk < len(OUTER) else acc[kk] * 4 % 1)
bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
me = bpy.data.meshes.new("Hu_Than"); bm.to_mesh(me); bm.free()
than = bpy.data.objects.new("Hu_Than", me); bpy.context.collection.objects.link(than)
for m in (mat("Hu_Men", (0.25, 0.14, 0.08)), mat("Hu_DatNung", (0.46, 0.3, 0.2)), mat("Hu_Long", (0.05, 0.035, 0.03))): me.materials.append(m)
for p in me.polygons: p.use_smooth = True

def obj(name, bm, m):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o); me.materials.append(m)
    for p in me.polygons: p.use_smooth = True
    return o

# ── giấy dầu: đĩa phủ miệng + vạt rủ xuống cổ (mép dưới xơ, so le), nhăn. Lưới cực (vòng × góc)
HINGE_Y = -0.03   # bản lề: dây cung y = −3 cm (góc lật = phần y < bản lề, phía trước)
ZT = 0.3035
RINGS = [(0.0, ZT + 0.003)] + [(r, ZT + 0.003 - 0.04 * r * r / 0.07) for r in [0.006 * i for i in range(1, 12)]] + [(0.0745, ZT - 0.0012), (0.0775, 0.2985),
         (0.0785, 0.292), (0.0765, 0.286), (0.0705, 0.279), (0.0675, 0.272), (0.0672, 0.264)]
mGiay = mat("Hu_GiayDau", (0.5, 0.36, 0.2))
def giay(goc):
    bm = bmesh.new(); uvl = bm.loops.layers.uv.new(); V = {}
    def vert(k, i):
        if (k, i) in V: return V[k, i]
        r, z = RINGS[k]; a = 2 * math.pi * i / N
        nn = noise.noise(Vector((math.cos(a) * 6, math.sin(a) * 6, k * 0.7)))
        TOP = len(RINGS) - 6
        r2 = r + (0.0025 * nn if k >= TOP else 0)
        z2 = z + (0.0015 * nn if k < TOP else 0) + (0.0006 if goc else 0)
        if k == len(RINGS) - 1: z2 += 0.005 * noise.noise(Vector((a * 4, 1.3, 0)))   # mép dưới so le
        V[k, i] = bm.verts.new((r2 * math.cos(a), r2 * math.sin(a), z2)); return V[k, i]
    for k in range(len(RINGS) - 1):
        for i in range(N):
            j = (i + 1) % N
            cy = sum(RINGS[kk][0] * math.sin(2 * math.pi * ii / N) for kk, ii in [(k, i), (k, j), (k + 1, i), (k + 1, j)]) / 4
            if (goc and cy >= HINGE_Y + 0.003) or (not goc and cy < HINGE_Y - 0.003): continue   # chồng mép 6 mm để không hở

            vs = [vert(k, i), vert(k, j), vert(k + 1, j), vert(k + 1, i)]
            if k == 0: vs = [vert(0, 0), vert(1, i), vert(1, j)]   # tâm: tam giác
            try: f = bm.faces.new(vs)
            except ValueError: continue
            for lp in f.loops: lp[uvl].uv = (lp.vert.co.x * 5 + 0.5, lp.vert.co.y * 5 + 0.5)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bmesh.ops.solidify(bm, geom=bm.faces[:], thickness=0.0008)
    return obj("Hu_GiayDau_Goc" if goc else "Hu_GiayDau", bm, mGiay)
gd = giay(False); gg = giay(True)

# ── lạt tre: 2 vòng dẹt quanh cổ (z 0.268 · 0.274) + nút xoắn ở PHÍA SAU (+Y) và 2 đầu thừa
mLat = mat("Hu_Lat", (0.75, 0.66, 0.44))
bm = bmesh.new(); uvl = bm.loops.layers.uv.new()
def band(z0, r0, w=0.006, t=0.0016):
    prev = None
    for i in range(N + 1):
        a = 2 * math.pi * i / N; c, s = math.cos(a), math.sin(a)
        q = [bm.verts.new(((r0 + dr) * c, (r0 + dr) * s, z0 + dz)) for dr, dz in [(0, -w / 2), (t, -w / 2), (t, w / 2), (0, w / 2)]]
        if prev:
            for m in range(4):
                f = bm.faces.new((prev[m], q[m], q[(m + 1) % 4], prev[(m + 1) % 4]))
                for lp in f.loops: lp[uvl].uv = (i / N * 6, m / 4)
        prev = q
band(0.2675, 0.0682); band(0.2745, 0.0685)
# nút: 2 khối xoắn + 2 đầu lạt chĩa ra
for k, (dx, dz, ang, L) in enumerate([(0.0, 0.271, 0.4, 0.012), (0.004, 0.271, -0.5, 0.012), (-0.006, 0.266, 0.9, 0.03), (0.007, 0.262, -1.1, 0.024)]):
    r = bmesh.ops.create_cube(bm, size=1)["verts"]
    bmesh.ops.scale(bm, vec=(L if k > 1 else 0.012, 0.004, 0.006), verts=r)
    bmesh.ops.rotate(bm, cent=(0, 0, 0), matrix=Matrix.Rotation(ang, 3, 'Y'), verts=r)
    bmesh.ops.translate(bm, vec=(dx, 0.071, dz), verts=r)
bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
lat = obj("Hu_Lat", bm, mLat)

# ── tờ niêm: dải rộng 5 cm vắt qua đỉnh theo trục Y, ôm sát biên dạng giấy + thân, xuống tới vai (z ≈ 0.235) cả 2 đầu
#    tách 3 phần: Sau (y > bản lề, dán chặt) · Goc (y < bản lề, trên mép miệng → lật theo góc giấy) · Duoi (vạt trước dưới mép miệng, ở lại; mép rách răng cưa)
mNiem = mat("Hu_Niem", (0.88, 0.8, 0.6))
path = [(r, z) for r, z in RINGS[:10]] + [(0.069, 0.262), (0.074, 0.256), (0.084, 0.246), (0.093, 0.236)]
OFF = 0.0012
# toạ độ dọc dải: s từ −đầu sau ... +đầu trước; điểm (y, z)
half = [(r + OFF * (1 if r > 0.06 else 0), z + OFF) for r, z in path]
pts = [(+r, z) for r, z in reversed(half[1:])] + [(-r, z) for r, z in half]     # y dương (sau) → y âm (trước)
# chia mịn
fine = []
for a, b in zip(pts, pts[1:]):
    for t in range(4): fine.append((a[0] + (b[0] - a[0]) * t / 4, a[1] + (b[1] - a[1]) * t / 4))
fine.append(pts[-1])
sacc = [0.0]
for a, b in zip(fine, fine[1:]): sacc.append(sacc[-1] + math.dist(a, b))
COLS = 8; WID = 0.05
tear = [0.2905 + 0.004 * random.uniform(-1, 1) for _ in range(COLS)]   # z rách mỗi cột (vạt trước)
parts = {"Sau": bmesh.new(), "Goc": bmesh.new(), "Duoi": bmesh.new()}
for bmx in parts.values(): bmx.loops.layers.uv.new()
VV = {}
def nv(bmx, part, k, c):
    key = (part, k, c)
    if key not in VV:
        y, z = fine[k]; x = -WID / 2 + WID * c / COLS
        VV[key] = bmx.verts.new((x, y, z + 0.0007 * math.sin(k * 0.55) * math.cos(c * 0.9)))
    return VV[key]
for k in range(len(fine) - 1):
    y0, z0 = fine[k]; y1, z1 = fine[k + 1]; ym, zm = (y0 + y1) / 2, (z0 + z1) / 2
    for c in range(COLS):
        if ym >= HINGE_Y: part = "Sau"
        elif zm >= tear[c] or (y0 > -0.06 and zm > 0.285): part = "Goc"
        else: part = "Duoi"
        bmx = parts[part]; uvl = bmx.loops.layers.uv.active
        vs = [nv(bmx, part, k, c), nv(bmx, part, k, c + 1), nv(bmx, part, k + 1, c + 1), nv(bmx, part, k + 1, c)]
        f = bmx.faces.new(vs)
        for lp, (u, v) in zip(f.loops, [(c / COLS, sacc[k]), ((c + 1) / COLS, sacc[k]), ((c + 1) / COLS, sacc[k + 1]), (c / COLS, sacc[k + 1])]):
            lp[uvl].uv = (u, 1 - v / sacc[-1])
niem = {}
for n, bmx in parts.items():
    bmesh.ops.remove_doubles(bmx, verts=bmx.verts, dist=1e-6)
    bmesh.ops.solidify(bmx, geom=bmx.faces[:], thickness=0.0006)
    bmesh.ops.recalc_face_normals(bmx, faces=bmx.faces)
    niem[n] = obj("Hu_Niem_" + n, bmx, mNiem)

# ── tóc: một lọn tóc đen gập đôi nằm ngay dưới miệng, chỗ buộc chỉ đỏ quay ra phía trước (−Y) — lộ ra khi lật góc
mToc = mat("Hu_Toc", (0.02, 0.018, 0.016)); mChi = mat("Hu_ChiDo", (0.6, 0.06, 0.05)); mCot = mat("Hu_Cot", (0.36, 0.32, 0.25))
bm = bmesh.new()
TIE = Vector((0.004, -0.044, 0.285))
for s in range(180):
    # sợi: từ chỗ buộc → vòng ra sau theo hình chữ U nằm trong lòng miệng, đầu sợi xoã
    side = random.choice((-1, 1)); sp = random.uniform(0.0, 0.006)
    pts = []
    for t in [i / 9 for i in range(10)]:
        a = math.pi * t
        p = TIE + Vector((side * (0.012 + 0.02 * math.sin(a)) + random.gauss(0, 0.0015), 0.075 * t + random.gauss(0, 0.002), -0.006 * math.sin(a) + random.gauss(0, 0.0012)))
        p.x += side * sp; pts.append(p)
    pts[0] = TIE + Vector((random.gauss(0, 0.0015), random.gauss(0, 0.001), random.gauss(0, 0.0012)))
    prev = None
    for i, p in enumerate(pts):
        d = (pts[min(i + 1, 9)] - pts[max(i - 1, 0)]).normalized(); n1 = d.orthogonal().normalized() * 0.0005; n2 = d.cross(n1).normalized() * 0.0005
        tri = [bm.verts.new(p + n1), bm.verts.new(p - n1 * 0.5 + n2), bm.verts.new(p - n1 * 0.5 - n2)]
        if prev:
            for m in range(3): bm.faces.new((prev[m], tri[m], tri[(m + 1) % 3], prev[(m + 1) % 3]))
        prev = tri
# phần chùm dày ngay tại chỗ buộc (đuôi tóc thò ra trước chỗ buộc ~1,5 cm)
for s in range(60):
    p0 = TIE + Vector((random.gauss(0, 0.002), 0, random.gauss(0, 0.0012)))
    p1 = p0 + Vector((random.gauss(0, 0.004), -0.016 - random.uniform(0, 0.006), random.gauss(0, 0.002)))
    d = (p1 - p0).normalized(); n1 = d.orthogonal().normalized() * 0.0005; n2 = d.cross(n1).normalized() * 0.0005
    a = [bm.verts.new(p0 + n1), bm.verts.new(p0 - n1 * 0.5 + n2), bm.verts.new(p0 - n1 * 0.5 - n2)]
    b = [bm.verts.new(p1)]
    for m in range(3): bm.faces.new((a[m], a[(m + 1) % 3], b[0]))
toc = obj("Hu_Toc", bm, mToc)
# chỉ đỏ: 4 vòng quấn quanh chỗ buộc + 2 đầu thừa buông
bm = bmesh.new()
def ong(points, r):
    prev = None
    for i, p in enumerate(points):
        d = (points[min(i + 1, len(points) - 1)] - points[max(i - 1, 0)]).normalized(); n1 = d.orthogonal().normalized(); n2 = d.cross(n1)
        ring = [bm.verts.new(p + (n1 * math.cos(a) + n2 * math.sin(a)) * r) for a in [k * 2 * math.pi / 6 for k in range(6)]]
        if prev:
            for m in range(6): bm.faces.new((prev[m], ring[m], ring[(m + 1) % 6], prev[(m + 1) % 6]))
        prev = ring
coil = [TIE + Vector((0.0045 * math.cos(t), -0.0018 + 0.0009 * t / (2 * math.pi), 0.0035 * math.sin(t))) for t in [i * 0.35 for i in range(72)]]
ong(coil, 0.0009)
ong([coil[-1] + Vector((0.003 * i, 0.002 * i, 0.0012 * i - 0.0002 * i * i)) for i in range(7)], 0.0006)   # 2 đầu chỉ thừa vắt lên tóc, quay vào trong
ong([coil[0] + Vector((-0.003 * i, 0.0015 * i, 0.001 * i - 0.00015 * i * i)) for i in range(7)], 0.0006)
chi = obj("Hu_ChiDo", bm, mChi)
# vài mảnh xương vàng ngà lẫn tro, nằm sâu hơn dưới tóc
bm = bmesh.new()
for k in range(6):
    r = bmesh.ops.create_icosphere(bm, subdivisions=1, radius=1)["verts"]
    bmesh.ops.scale(bm, vec=(random.uniform(0.006, 0.016), random.uniform(0.004, 0.008), random.uniform(0.003, 0.006)), verts=r)
    bmesh.ops.rotate(bm, cent=(0, 0, 0), matrix=Matrix.Rotation(random.uniform(0, 6.3), 3, 'Z'), verts=r)
    a = random.uniform(0, 6.3); rr = random.uniform(0, 0.04)
    bmesh.ops.translate(bm, vec=(rr * math.cos(a), rr * math.sin(a) - 0.01, random.uniform(0.255, 0.268)), verts=r)
cot = obj("Hu_Cot", bm, mCot)

# ── cha / con: góc giấy có gốc ở bản lề, niêm phần góc là con của góc giấy
root = bpy.data.objects.new("HuCot", None); bpy.context.collection.objects.link(root)
for o in [than, gd, lat, toc, chi, cot, niem["Sau"], niem["Duoi"]]: o.parent = root
piv = Vector((0, HINGE_Y, ZT + 0.004))
for o in [gg, niem["Goc"]]:
    o.data.transform(Matrix.Translation(-piv))
gg.location = piv; gg.parent = root
niem["Goc"].parent = gg
bpy.context.view_layer.update()
os.makedirs(OUT, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=f"{OUT}/HuCot.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                         axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", bake_space_transform=False, add_leaf_bones=False)
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/HuCot.blend")
for o in bpy.data.objects:
    if o.type == 'MESH': print(o.name, len(o.data.vertices), len(o.data.polygons))
