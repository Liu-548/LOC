# [22/10] Model đồ cho 3 mini game sắp xếp LỘC (bpy). Blender: Z lên, mét. Mặt "người đọc" = −Y (chữ trên mặt trên đọc từ phía −Y).
# Gốc: đồ đặt = tâm đáy · đồ phơi (MG_*Phoi) = điểm treo trên dây.
# Mọi model dùng 1 vật liệu "MG_Atlas" (T_MG_Atlas.png, atlas.py) — UV trỏ vào ô màu hoặc vùng texture.
# Chạy: python3 build_mg.py <thư mục ra>  → MG_*.fbx + XemTruoc_MG.png
import bpy, bmesh, math, random, os, sys, json
from mathutils import Vector, Matrix
OUT = sys.argv[1] if len(sys.argv) > 1 else "out"
HERE = os.path.dirname(os.path.abspath(__file__))
AT = json.load(open(os.path.join(HERE, "atlas.json"))); VUNG, PALXY = AT["VUNG"], AT["PALXY"]
random.seed(7)
bpy.ops.wm.read_factory_settings(use_empty=True)
MAT = bpy.data.materials.new("MG_Atlas")

def uv_pal(k):
    x, y = PALXY[k]; return ((x + 8) / 512, 1 - (y + 8) / 512)
def uv_tex(k, a, b):   # a: 0 trái → 1 phải · b: 0 dưới → 1 trên của vùng ảnh
    x0, y0, x1, y1 = VUNG[k]; a = min(max(a, 0), 1); b = min(max(b, 0), 1)
    return ((x0 + 0.5 + a * (x1 - x0 - 1)) / 512, 1 - (y0 + 0.5 + (1 - b) * (y1 - y0 - 1)) / 512)

class M:   # một model = một bmesh, gom nhiều phần; mỗi mặt có luật màu
    def __init__(s): s.bm = bmesh.new(); s.uv = s.bm.loops.layers.uv.new("UVMap")
    def to(s, faces, rule, bounds=None):
        """rule(face) → ('pal', key) | ('tex', key, 'xy'|'xz')  — chiếu phẳng theo bounds (mặc định: bao các mặt này)"""
        faces = [f for f in faces if f.is_valid]
        vs = [v.co for f in faces for v in f.verts]
        if not vs: return
        bx = bounds or (min(v.x for v in vs), max(v.x for v in vs), min(v.y for v in vs), max(v.y for v in vs), min(v.z for v in vs), max(v.z for v in vs))
        for f in faces:
            r = rule(f)
            for l in f.loops:
                if r[0] == 'pal': l[s.uv].uv = uv_pal(r[1]); continue
                c = l.vert.co; ax = r[2]
                a = (c.x - bx[0]) / max(bx[1] - bx[0], 1e-6)
                b = ((c.y - bx[2]) / max(bx[3] - bx[2], 1e-6)) if ax == 'xy' else ((c.z - bx[4]) / max(bx[5] - bx[4], 1e-6))
                l[s.uv].uv = uv_tex(r[1], a, b)
    def new_faces(s, before): return [f for f in s.bm.faces if f.index == -2 or f not in before]

    def box(s, size, at=(0, 0, 0), rot=0.0, rule=lambda f: ('pal', 'trang'), bevel=0.0, tilt=0.0, bounds=None):
        old = set(s.bm.faces)
        r = bmesh.ops.create_cube(s.bm, size=1.0)["verts"]
        bmesh.ops.scale(s.bm, vec=size, verts=r)
        if bevel: bmesh.ops.bevel(s.bm, geom=list({e for v in r for e in v.link_edges}), offset=bevel, segments=1, affect='EDGES')
        nv = [v for f in s.bm.faces if f not in old for v in f.verts]; nv = list(set(nv))
        bmesh.ops.translate(s.bm, vec=(0, 0, size[2] / 2), verts=nv)   # đáy ở z = 0
        if tilt: bmesh.ops.rotate(s.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(tilt, 3, 'X'), verts=nv)
        bmesh.ops.rotate(s.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(rot, 3, 'Z'), verts=nv)
        bmesh.ops.translate(s.bm, vec=at, verts=nv)
        nf = [f for f in s.bm.faces if f not in old]; s.bm.normal_update()
        if bounds == 'local':   # chiếu texture theo khung của chính hộp (trước khi xoay) — dùng cho giấy, bìa xoay lệch
            s._map_local(nf, size, at, rot, tilt, rule)
        else: s.to(nf, rule, bounds)
        return nf
    def _map_local(s, nf, size, at, rot, tilt, rule):
        inv = Matrix.Rotation(-rot, 3, 'Z'); invt = Matrix.Rotation(-tilt, 3, 'X')
        for f in nf:
            r = rule(f)
            for l in f.loops:
                if r[0] == 'pal': l[s.uv].uv = uv_pal(r[1]); continue
                c = invt @ (inv @ (l.vert.co - Vector(at)))
                l[s.uv].uv = uv_tex(r[1], c.x / size[0] + 0.5, c.y / size[1] + 0.5)

    def lathe(s, prof, segs, col, at=(0, 0, 0), rep=4):
        """prof [(r, z)] · col(k) → ('pal', key) | ('tex', key) cho đoạn k; texture quấn quanh, lặp rep lần"""
        old = set(s.bm.faces)
        rings = [[s.bm.verts.new((at[0] + max(r, 1e-4) * math.cos(2 * math.pi * i / segs), at[1] + max(r, 1e-4) * math.sin(2 * math.pi * i / segs), at[2] + z)) for i in range(segs)] for r, z in prof]
        tz = [z for k, (r, z) in enumerate(prof[:-1]) if col(k)[0] == 'tex'] + [prof[k + 1][1] for k in range(len(prof) - 1) if col(k)[0] == 'tex']
        z0, z1 = (min(tz), max(tz)) if tz else (0, 1)
        for k in range(len(prof) - 1):
            c = col(k)
            for i in range(segs):
                j = (i + 1) % segs
                f = s.bm.faces.new((rings[k][i], rings[k][j], rings[k + 1][j], rings[k + 1][i]))
                if c[0] == 'pal':
                    for l in f.loops: l[s.uv].uv = uv_pal(c[1])
                else:
                    u0, u1 = (i % rep) / rep, (i % rep + 1) / rep
                    for l, (u, zz) in zip(f.loops, [(u0, prof[k][1]), (u1, prof[k][1]), (u1, prof[k + 1][1]), (u0, prof[k + 1][1])]):
                        l[s.uv].uv = uv_tex(c[1], u, (zz - z0) / max(z1 - z0, 1e-6))
        return [f for f in s.bm.faces if f not in old]

    def ong(s, pts, r0, r1, key, sides=6):
        old = set(s.bm.faces); prev = None; n = len(pts)
        for i, p in enumerate(pts):
            p = Vector(p); d = (Vector(pts[min(i + 1, n - 1)]) - Vector(pts[max(i - 1, 0)])).normalized()
            n1 = d.orthogonal().normalized(); n2 = d.cross(n1); r = r0 + (r1 - r0) * i / max(n - 1, 1)
            ring = [s.bm.verts.new(p + (n1 * math.cos(a) + n2 * math.sin(a)) * r) for a in [2 * math.pi * k / sides for k in range(sides)]]
            if prev:
                for m in range(sides): s.bm.faces.new((prev[m], ring[m], ring[(m + 1) % sides], prev[(m + 1) % sides]))
            prev = ring
        nf = [f for f in s.bm.faces if f not in old]; s.to(nf, lambda f: ('pal', key))

    def tam(s, outline, mat_tex, edge_key, plane='xz', thick=0.005, at=(0, 0, 0)):
        """tấm phẳng (vải, giấy) từ đường bao 2D (lõm được), dày thick. Tự chia tam giác + tự định hướng mặt:
        mặt trước nhìn về −Y (plane 'xz') hoặc +Z (plane 'xy'); mặt sau ngược lại; mép quay ra ngoài."""
        from mathutils.geometry import tessellate_polygon
        old = set(s.bm.faces)
        if plane == 'xz': P3 = lambda a, b, d: Vector((at[0] + a, at[1] + d, at[2] + b)); nTruoc = Vector((0, -1, 0)); dv = thick
        else: P3 = lambda a, b, d: Vector((at[0] + a, at[1] + b, at[2] + d)); nTruoc = Vector((0, 0, 1)); dv = -thick
        A = [s.bm.verts.new(P3(a, b, 0)) for a, b in outline]; B = [s.bm.verts.new(P3(a, b, dv)) for a, b in outline]
        def mat(vs, n):   # tạo mặt, lật nếu pháp tuyến ngược hướng mong muốn
            f = s.bm.faces.new(vs); f.normal_update()
            if f.normal.dot(n) < 0: f.normal_flip()
            return f
        for t in tessellate_polygon([[Vector((a, b, 0)) for a, b in outline]]):
            mat([A[i] for i in t], nTruoc); mat([B[i] for i in t], -nTruoc)
        c = sum((v.co for v in A), Vector()) / len(A)
        for i in range(len(A)):
            j = (i + 1) % len(A); m = (A[i].co + A[j].co) / 2
            mat([A[i], A[j], B[j], B[i]], (m - c))
        nf = [x for x in s.bm.faces if x not in old]
        ax = 1 if plane == 'xz' else 2
        def rule(fc):
            if abs(fc.normal[ax]) > 0.7: return mat_tex if mat_tex[0] == 'pal' else ('tex', mat_tex[1], plane)
            return ('pal', edge_key)
        s.to(nf, rule); return nf

    def done(s, name):
        me = bpy.data.meshes.new(name); s.bm.to_mesh(me); s.bm.free()
        me.materials.append(MAT); o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
        for p in me.polygons: p.use_smooth = False
        return o

TOP = lambda key, side: (lambda f: ('tex', key, 'xy') if f.normal.z > 0.7 else ('pal', side))
P = lambda key: (lambda f: ('pal', key))
objs = []

# ═══ BÀN NƯỚC ═══
# chén hạt mít men lam, đáy còn lớp nước chè đặc
m = M()
CHEN = [(0, 0), (0.015, 0), (0.016, 0.006), (0.019, 0.008), (0.025, 0.02), (0.03, 0.036), (0.031, 0.041), (0.028, 0.041), (0.026, 0.03), (0.02, 0.016), (0.012, 0.011), (0, 0.010)]
m.lathe(CHEN, 12, lambda k: ('pal', 'kem') if k <= 1 else ('tex', 'hoalam') if k <= 5 else ('pal', 'lam') if k == 6 else ('pal', 'trang'))
m.lathe([(0, 0.0165), (0.0205, 0.0165)], 12, lambda k: ('pal', 'nuocche'))
objs.append(m.done("MG_ChenTra"))

# ấm tích men lam: thân, nắp + núm, vòi, quai
m = M()
m.lathe([(0, 0), (0.045, 0), (0.05, 0.005), (0.062, 0.03), (0.066, 0.05), (0.06, 0.075), (0.045, 0.09), (0.036, 0.093), (0.036, 0.098), (0, 0.098)], 12,
        lambda k: ('pal', 'kem') if k == 0 else ('tex', 'hoalam') if k <= 5 else ('pal', 'trang'))
m.lathe([(0, 0.098), (0.031, 0.098), (0.022, 0.104), (0.008, 0.106), (0.011, 0.114), (0, 0.118)], 12, lambda k: ('pal', 'lam') if k >= 3 else ('pal', 'trang'))
m.ong([(0.055, 0, 0.035), (0.075, 0, 0.05), (0.09, 0, 0.07), (0.1, 0, 0.088)], 0.010, 0.005, 'trang')
m.ong([(-0.055, 0, 0.075), (-0.08, 0, 0.07), (-0.088, 0, 0.05), (-0.078, 0, 0.032), (-0.056, 0, 0.028)], 0.006, 0.006, 'trang')
objs.append(m.done("MG_AmTra"))

# đĩa sứ + đống vỏ hạt dưa
m = M()
m.lathe([(0, 0), (0.05, 0), (0.055, 0.004), (0.08, 0.012), (0.085, 0.015), (0.082, 0.016), (0.07, 0.01), (0, 0.006)], 16, lambda k: ('pal', 'lam') if k in (3, 4) else ('pal', 'trang'))
for i in range(55):
    a = random.uniform(0, 6.283); r = 0.055 * math.sqrt(random.random()); h = 0.012 * (1 - (r / 0.055) ** 2)
    m.box((0.012, 0.007, 0.0022), at=(r * math.cos(a), r * math.sin(a), 0.006 + h * random.uniform(0.4, 1)), rot=random.uniform(0, 3.14), tilt=random.uniform(-0.5, 0.5),
          rule=P('vohd_den' if i % 3 == 0 else 'vohd_do'))
objs.append(m.done("MG_DiaVoHatDua"))

# giấy kẹo vặn hai đầu (3 màu)
for mau in ['keo_do', 'keo_vang', 'keo_lam']:
    m = M()
    m.box((0.018, 0.011, 0.005), rule=P(mau))
    for s_ in (-1, 1):
        m.tam([(0, -0.003), (0.012 * s_, -0.008), (0.014 * s_, 0.0), (0.012 * s_, 0.008), (0, 0.003)][::s_], ('pal', mau), mau, plane='xy', thick=0.001, at=(0.009 * s_, 0, 0.0015))
    objs.append(m.done("MG_GiayKeo_" + mau.split('_')[1]))

# bánh in + kẹo lạc còn thừa
m = M()
for i, (x, y, rt) in enumerate([(-0.03, 0.01, 0.2), (0.0, -0.015, -0.4), (0.03, 0.012, 0.9), (-0.005, 0.025, 1.4)]):
    if i < 2: m.box((0.055, 0.022, 0.012), at=(x, y, 0), rot=rt, rule=lambda f: ('pal', 'keolac') if f.normal.z < 0.7 else ('pal', 'giaybong'))
    else:
        m.lathe([(0, 0), (0.018, 0), (0.018, 0.012), (0.014, 0.015), (0, 0.015)], 8, lambda k: ('pal', 'banhin_hong') if k == 1 else ('pal', 'banhin'), at=(x, y, 0))
objs.append(m.done("MG_BanhKeoThua"))

# túi hạt dưa nylon đỏ: gối dẹt, hai đầu ép mỏng
m = M()
nf = m.box((0.12, 0.18, 0.03), rule=P('do'))
vs = list({v for f in nf for v in f.verts})
bmesh.ops.subdivide_edges(m.bm, edges=list({e for v in vs for e in v.link_edges}), cuts=3, use_grid_fill=True)
for v in m.bm.verts:
    t = min(1, abs(v.co.y) / 0.09); v.co.z = 0.015 + (v.co.z - 0.015) * (1 - 0.8 * t ** 3); v.co.x *= 1 - 0.08 * t ** 2
m.bm.normal_update(); m.to(list(m.bm.faces), TOP('hatdua', 'do'))
objs.append(m.done("MG_GoiHatDua"))

# hộp bánh kẹo tròn (hộp mứt thiếc): thân đỏ, đai và vành nắp vàng
m = M()
m.lathe([(0, 0), (0.118, 0), (0.12, 0.003), (0.12, 0.05), (0.123, 0.05), (0.123, 0.068), (0.118, 0.07), (0.07, 0.071), (0.06, 0.071), (0, 0.071)], 16,
        lambda k: ('pal', 'vang') if k in (3, 4, 5, 7) else ('pal', 'do'))
objs.append(m.done("MG_HopBanhKeo"))

# cốc của bố: men nâu loang, úp trên đĩa lót
m = M()
m.lathe([(0, 0), (0.035, 0), (0.04, 0.003), (0.055, 0.008), (0.057, 0.011), (0.052, 0.011), (0.04, 0.006), (0, 0.005)], 14, lambda k: ('pal', 'kem'))
m.lathe([(0.03, 0.005), (0.038, 0.005), (0.038, 0.009), (0.036, 0.08), (0.033, 0.09), (0.03, 0.095), (0.022, 0.095), (0.022, 0.093), (0, 0.093)], 14,
        lambda k: ('pal', 'menau_sang') if k <= 1 else ('tex', 'menau') if k == 2 else ('pal', 'datnung'))
m.ong([(0.036, 0, 0.022), (0.055, 0, 0.03), (0.06, 0, 0.05), (0.052, 0, 0.068), (0.035, 0, 0.072)], 0.006, 0.006, 'menau')
objs.append(m.done("MG_CocUp"))

# ═══ SÂN PHƠI (gốc = điểm treo, z ≤ 0) ═══
# áo cộc hoa của Nhím, kẹp hai vai; tay trái (phía +x khi nhìn mặt trước) bị xẻ dọc tới nách, mép viền chỉ trắng
m = M()
AO = [(-0.06, 0), (-0.13, -0.02), (-0.2, -0.085), (-0.165, -0.125), (-0.125, -0.095), (-0.125, -0.32), (0.125, -0.32), (0.125, -0.1), (0.13, -0.02), (0.06, 0), (0.03, -0.028), (-0.03, -0.028)]
m.tam(AO, ('tex', 'aohoa'), 'hong')
m.tam([(0.13, -0.02), (0.235, -0.075), (0.215, -0.11), (0.14, -0.065)], ('tex', 'aohoa'), 'hong', at=(0, -0.002, 0))         # nửa trên tay xẻ, xoè ra
m.tam([(0.125, -0.1), (0.175, -0.115), (0.17, -0.215), (0.13, -0.17)], ('tex', 'aohoa'), 'hong', at=(0, 0.003, 0))             # nửa dưới rủ xuống sườn
m.ong([(0.14, -0.006, -0.065), (0.215, -0.006, -0.11)], 0.0018, 0.0018, 'chitrang', 4)                                            # đường viền chỉ trắng
m.ong([(0.175, -0.003, -0.115), (0.17, -0.003, -0.215)], 0.0018, 0.0018, 'chitrang', 4)
objs.append(m.done("MG_AoHoaPhoi"))

m = M()   # quần đùi kẻ ô, kẹp cạp
m.tam([(-0.11, 0), (0.11, 0), (0.125, -0.2), (0.02, -0.2), (0, -0.11), (-0.02, -0.2), (-0.125, -0.2)], ('tex', 'keo_o'), 'quanlam')
m.box((0.222, 0.008, 0.02), at=(0, 0.0025, -0.02), rule=P('quanlam'))
objs.append(m.done("MG_QuanDuiPhoi"))

m = M()   # đôi tất trẻ con, mũi chúc xuống
for sx in (-0.04, 0.04):
    m.tam([(-0.022, 0), (0.022, 0), (0.022, -0.1), (0.05, -0.12), (0.05, -0.145), (0.0, -0.145), (-0.022, -0.12)], ('pal', 'tat'), 'tat', thick=0.004, at=(sx, 0, 0))
objs.append(m.done("MG_TatPhoi"))

def khan_vat(name, key, w, h1, h2):   # khăn vắt qua dây: hai vạt trước/sau
    m = M()
    m.tam([(-w / 2, 0), (w / 2, 0), (w / 2, -h1), (-w / 2, -h1)], ('tex', key), key, thick=0.003, at=(0, -0.006, 0))
    m.tam([(-w / 2, 0), (w / 2, 0), (w / 2, -h2), (-w / 2, -h2)], ('tex', key), key, thick=0.003, at=(0, 0.004, 0))
    m.box((w, 0.013, 0.006), at=(0, 0, -0.002), rule=P(key))
    objs.append(m.done(name))
khan_vat("MG_KhanMatPhoi", 'khanmat', 0.25, 0.25, 0.19)
khan_vat("MG_KhanLauPhoi", 'khanlau', 0.22, 0.24, 0.2)

# ═══ ĐỒ ĐÃ GẤP (gốc = tâm đáy) ═══
m = M(); z = 0
for i in range(4):
    m.box((0.30, 0.24, 0.026), at=(random.uniform(-0.008, 0.008), random.uniform(-0.006, 0.006), z), rot=random.uniform(-0.05, 0.05), bevel=0.006, rule=TOP('so', 'so'), bounds='local'); z += 0.026
objs.append(m.done("MG_DoTangGap"))

m = M()   # áo hoa gấp; vạt tay áo xẻ gập lên trên, mép viền chỉ trắng
m.box((0.2, 0.17, 0.022), bevel=0.005, rule=lambda f: ('tex', 'aohoa', 'xy') if f.normal.z > 0.7 else ('pal', 'hong'), bounds='local')
m.box((0.085, 0.06, 0.004), at=(0.055, -0.045, 0.022), rot=0.35, rule=lambda f: ('tex', 'aohoa', 'xy') if f.normal.z > 0.7 else ('pal', 'hong'), bounds='local')
m.ong([(0.02, -0.062, 0.027), (0.098, -0.034, 0.027)], 0.0016, 0.0016, 'chitrang', 4)
objs.append(m.done("MG_AoHoaGap"))

m = M(); m.box((0.18, 0.15, 0.02), bevel=0.005, rule=lambda f: ('tex', 'keo_o', 'xy') if f.normal.z > 0.7 else ('pal', 'quanlam'), bounds='local'); objs.append(m.done("MG_QuanDuiGap"))
m = M()
m.box((0.16, 0.12, 0.016), bevel=0.004, rule=TOP('khanmat', 'khanmat'), bounds='local')
m.box((0.15, 0.11, 0.014), at=(0.005, 0.003, 0.016), rot=0.08, bevel=0.004, rule=TOP('khanlau', 'khanlau'), bounds='local')
objs.append(m.done("MG_KhanGap"))

# ═══ GIẤY TỜ ═══
m = M()   # tờ hoá đơn hơi quăn hai đầu
nf = m.box((0.15, 0.21, 0.0008), rule=lambda f: ('tex', 'hoadon', 'xy') if f.normal.z > 0.7 else ('pal', 'giay_canh'))
bmesh.ops.subdivide_edges(m.bm, edges=list({e for f in nf for e in f.edges if abs(e.verts[0].co.y - e.verts[1].co.y) > 0.1}), cuts=4)
for v in m.bm.verts: v.co.z += 0.006 * (abs(v.co.y) / 0.105) ** 2
m.bm.normal_update(); objs.append(m.done("MG_HoaDon"))

def so(name, rule_top, mau):   # sổ bìa cứng: bìa trên/dưới + gáy cùng màu, ba mép còn lại là ruột giấy
    m = M()
    m.box((0.21, 0.30, 0.022), rule=lambda f: rule_top(f) if f.normal.z > 0.7 else ('pal', mau) if (f.normal.z < -0.7 or f.normal.x < -0.7) else ('pal', 'trangsach'), bounds='local')
    objs.append(m.done(name))
so("MG_SoHang", lambda f: ('tex', 'biaso', 'xy'), 'so_nau')
so("MG_SoThuChi", lambda f: ('pal', 'so_vang'), 'so_vang')

def bia(name, top_rule):   # bìa hồ sơ gập: lá dưới, xấp giấy, lá trên hơi lệch
    m = M()
    m.box((0.25, 0.33, 0.002), rule=P('biacung'))
    m.box((0.23, 0.31, 0.007), at=(0.003, 0.002, 0.002), rule=lambda f: ('pal', 'giay') if f.normal.z > 0.7 else ('pal', 'giay_canh'))
    m.box((0.25, 0.33, 0.002), at=(-0.002, -0.003, 0.009), rot=0.025, rule=lambda f: top_rule(f) if f.normal.z > 0.7 else ('pal', 'biacung_trong'), bounds='local')
    objs.append(m.done(name))
bia("MG_BiaHoSo", lambda f: ('pal', 'biacung'))
bia("MG_BiaKhoi", lambda f: ('tex', 'bia', 'xy'))

m = M(); z = 0   # xấp cuống phiếu xoè, tờ trên cùng thẳng
for i in range(7):
    m.box((0.21, 0.15, 0.0008), at=(0.004 * (6 - i), -0.003 * (6 - i), z), rot=(-0.22 + 0.035 * i) if i < 6 else 0.0,
          rule=lambda f: ('tex', 'cuong', 'xy') if f.normal.z > 0.7 else ('pal', 'giay_canh'), bounds='local'); z += 0.0011
objs.append(m.done("MG_CuongPhieuXap"))

# ── xuất: mỗi model một FBX (Apply Transform: Unity nhận mesh đã đổi trục, không có xoay ở gốc)
os.makedirs(OUT, exist_ok=True)
for o in objs:
    bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active = o
    bpy.ops.export_scene.fbx(filepath=f"{OUT}/{o.name}.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
                             mesh_smooth_type="FACE", bake_space_transform=True, add_leaf_bones=False, object_types={'MESH'})
    print(f"{o.name:18s} {len(o.data.polygons):5d} mặt  {tuple(round(d, 3) for d in o.dimensions)}")
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/MG_SapXep.blend")
