# Bước 3: tóc tém ngang cằm, mái bằng (lưới tham số quanh đầu) + chỏm tóc ngắn lởm chởm sau gáy.
# Cột theo góc φ (0 = trước mặt −Y, tăng sang trái +X). Mỗi cột: từ xoáy đỉnh đầu bám theo da đầu (cách 1–2 cm), rồi rủ thẳng xuống tới mép cắt.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, numpy as np, math, geo, json
from mathutils import Vector
from mathutils.bvhtree import BVHTree
OUT = "/home/claude/nhim/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b2b.blend")
body = bpy.data.objects["Body"]; arm = bpy.data.objects["NhimRig"]
bvh = BVHTree.FromPolygons([v.co for v in body.data.vertices], [p.vertices for p in body.data.polygons])
hz = [v.co for v in body.data.vertices if v.co.z > 1.08]
YC = (min(c.y for c in hz) + max(c.y for c in hz)) / 2
ZC = 1.10; C = Vector((0, YC, ZC)); TOP = max(c.z for c in hz)
print("tâm đầu y", YC, "đỉnh", TOP)

BROW, CHIN = 1.112, 1.002
def zbot(phi):
    a = math.degrees(phi) % 360; a = min(a, 360 - a)
    if a < 48: return BROW + 0.004 * math.cos(math.radians(a) * 3)          # mái bằng, hơi cong
    if a < 78:
        t = (a - 48) / 30; t = t * t * (3 - 2 * t); return BROW + (CHIN + 0.012 - BROW) * t
    z = CHIN + 0.008 + 0.010 * (a - 78) / 102                                  # sau gáy ngắn hơn chút
    if a > 170: z += 0.022 * (1 - (180 - a) / 10) ** 0.5                       # chỏm bị cắt "lễ giữ mạng"
    return z
def thick(z, phi):
    a = math.degrees(phi) % 360; a = min(a, 360 - a)
    base = 0.007 + 0.011 * np.clip((1.17 - z) / 0.13, 0, 1)    # phồng dần xuống dưới
    return base * (0.6 if a < 40 else 1.0)

def ray(o, d):
    hit = bvh.ray_cast(o, d, 1.0)
    return hit[0] if hit[0] is not None else None

NC, NR = 44, 20
cols = []
for c in range(NC + 1):                        # cột cuối trùng cột đầu (đường nối UV ở sau gáy)
    phi = math.pi + 2 * math.pi * c / NC       # bắt đầu từ sau gáy
    dirh = Vector((math.sin(phi), -math.cos(phi), 0))
    pts = []
    # phần chỏm: tia xuyên tâm từ C theo độ cao 88° → 15°
    for el in np.linspace(88, 12, 16):
        e = math.radians(el); d = Vector((dirh.x * math.cos(e), dirh.y * math.cos(e), math.sin(e)))
        h = ray(C + d * 0.3, -d)
        if h is None: continue
        pts.append(h + d * thick(h.z, phi))
    # phần rủ: tia ngang từ trục đầu, lấy max bán kính từ trên xuống
    rmax = Vector((pts[-1].x, pts[-1].y - YC, 0)).length
    zb = zbot(phi)
    for z in np.linspace(pts[-1].z - 0.012, zb, 14):
        o = Vector((0, YC, z)) + dirh * 0.3
        h = ray(o, -dirh)
        r = (min(Vector((h.x, h.y - YC, 0)).length, 0.074 if 1.06 < z < 1.13 else 9) + thick(z, phi)) if h is not None else rmax
        a_ = math.degrees(phi) % 360; a_ = min(a_, 360 - a_)
        rmax = max(rmax * ((0.972 if a_ < 95 else 0.986) if z < 1.125 else 0.995), r)     # rủ thẳng, không bám vào cổ
        pts.append(Vector((0, YC, z)) + dirh * rmax)
    # cụp vào ở mép dưới (tóc tém cụp)
    for k in range(1, 3): pts[-k] = pts[-k] - dirh * 0.004 * (3 - k)
    # đều theo chiều dài
    L = np.cumsum([0] + [(pts[i + 1] - pts[i]).length for i in range(len(pts) - 1)])
    rs = []
    for s in np.linspace(0, L[-1], NR):
        i = min(np.searchsorted(L, s, "right") - 1, len(pts) - 2); t = (s - L[i]) / max(L[i + 1] - L[i], 1e-9)
        rs.append(pts[i].lerp(pts[i + 1], t))
    cols.append((phi, rs, L[-1]))

# xoáy (đỉnh chung), hơi lùi sau đỉnh đầu
crown = Vector((0, YC + 0.022, TOP + 0.012))
bm = bmesh.new(); uvl = bm.loops.layers.uv.new("UV")
cv = bm.verts.new(crown)
grid = [[bm.verts.new(p) for p in rs] for _, rs, _ in cols]
LMAX = max(l for _, _, l in cols)
vb = {}
for c in range(NC):
    phi, rs, L = cols[c]; u0, u1 = c / NC, (c + 1) / NC
    f = bm.faces.new((cv, grid[c + 1][0], grid[c][0]))
    for lp, uv in zip(f.loops, ((0.5 * (u0 + u1), 1), (u1, 1 - 0.02), (u0, 1 - 0.02))): lp[uvl].uv = uv
    for r in range(NR - 1):
        def V(cc, rr): return 1 - 0.02 - (cols[cc][2] * rr / (NR - 1)) / LMAX * 0.94
        f = bm.faces.new((grid[c][r], grid[c + 1][r], grid[c + 1][r + 1], grid[c][r + 1]))
        for lp, uv in zip(f.loops, ((u0, V(c, r)), (u1, V(c + 1, r)), (u1, V(c + 1, r + 1)), (u0, V(c, r + 1)))): lp[uvl].uv = uv
    vb[c] = 1 - 0.02 - cols[c][2] / LMAX * 0.94
bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
me = bpy.data.meshes.new("Toc"); bm.to_mesh(me); bm.free()
for p in me.polygons: p.use_smooth = True
toc = bpy.data.objects.new("Toc", me); bpy.context.scene.collection.objects.link(toc)
toc.parent = arm; g = toc.vertex_groups.new(name="Head"); g.add(list(range(len(me.vertices))), 1.0, "REPLACE")
m = toc.modifiers.new("Armature", "ARMATURE"); m.object = arm
json.dump({"v_bottom": [vb[c] for c in range(NC)], "nc": NC}, open(f"{OUT}/toc_uv.json", "w"))

# da đầu dưới tóc: giảm mạnh thêm (khuất hẳn)
print("tóc", geo.tris(toc), "tam giác")
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b3.blend")
