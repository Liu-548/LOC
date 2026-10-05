# Bước 4: chăn (mô phỏng vải phủ lên tư thế nằm), chăn vo (cảnh Bad), khăn trán khô/ướt, dây đeo tay (cảnh Good).
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, numpy as np, rig, poses, geo, json, math
from mathutils import Vector, Matrix
from mathutils.bvhtree import BVHTree
OUT = "/home/claude/nhim/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b3.blend")
arm = bpy.data.objects["NhimRig"]; P = rig.Poser(arm); body = bpy.data.objects["Body"]
PARTS = ["Body", "Ao", "Quan", "Bot", "Toc"]

def posed_points(names):
    dg = bpy.context.evaluated_depsgraph_get(); pts = []
    for n in names:
        o = bpy.data.objects[n]; e = o.evaluated_get(dg)
        pts += [tuple(o.matrix_world @ v.co) for v in e.data.vertices]
    return np.array(pts)

# ── vị trí hông cho tư thế nằm: đỉnh đầu x=0,87 · lưng y=−0,30 · đáy z=0 (mặt đệm)
NAM = poses.merge(poses.NAM, poses.curl(poses.CURL_R))
P.apply(NAM, poses.G_NAM)
a = posed_points(PARTS); hips = arm.pose.bones["Hips"].matrix.translation.copy()
off = Vector((0.87 - a[:, 0].max(), -0.30 - a[:, 1].min(), -a[:, 2].min() + 0.004))
HL_NAM = poses.G_NAM.inverted() @ (hips + off)
P.apply(NAM, poses.G_NAM, hips_loc=HL_NAM)
json.dump({"HL_NAM": list(HL_NAM)}, open(f"{OUT}/place.json", "w"))
print("hông nằm (khung thân)", HL_NAM)

def collider_from(names, name="_col"):
    dg = bpy.context.evaluated_depsgraph_get(); bm = bmesh.new()
    for n in names:
        o = bpy.data.objects[n]; e = o.evaluated_get(dg); me = e.to_mesh()
        me.transform(o.matrix_world); bm.from_mesh(me); e.to_mesh_clear()
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(ob)
    ob.modifiers.new("Col", "COLLISION"); ob.collision.thickness_outer = 0.006; return ob

def box(name, lo, hi):
    bpy.ops.mesh.primitive_cube_add(size=1, location=[(l + h) / 2 for l, h in zip(lo, hi)])
    o = bpy.context.object; o.name = name; o.scale = [h - l for l, h in zip(lo, hi)]
    bpy.ops.object.transform_apply(scale=True)
    o.modifiers.new("Col", "COLLISION"); o.collision.thickness_outer = 0.004; return o

def grid(name, x0, x1, y0, y1, z, nx, ny):
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=nx, y_subdivisions=ny, size=1, location=((x0 + x1) / 2, (y0 + y1) / 2, z))
    o = bpy.context.object; o.name = name; o.scale = (x1 - x0, y1 - y0, 1); bpy.ops.object.transform_apply(scale=True)
    o.data.uv_layers[0].name = "UV"; return o

def simulate(o, frames, pin=None, mass=0.15, stiff=8):
    m = o.modifiers.new("Cloth", "CLOTH"); s = m.settings
    s.quality = 8; s.mass = mass; s.tension_stiffness = stiff; s.compression_stiffness = stiff; s.bending_stiffness = 0.15
    s.air_damping = 2.0
    m.collision_settings.use_collision = True; m.collision_settings.distance_min = 0.005
    m.collision_settings.use_self_collision = True; m.collision_settings.self_distance_min = 0.004
    m.point_cache.frame_start = 1; m.point_cache.frame_end = frames
    sc = bpy.context.scene; sc.frame_start = 1; sc.frame_end = frames
    for f in range(1, frames + 1): sc.frame_set(f)
    with bpy.context.temp_override(object=o, active_object=o):
        bpy.ops.object.modifier_apply(modifier="Cloth")
    sc.frame_set(1)

# ── 1. chăn mỏng phủ từ chân tới dưới nách (đầu giường +X, nệm 1,9 × 0,9, tường sau y = −0,45)
col = collider_from(["Body", "Ao", "Quan", "Bot"])
mat = box("_nem", (-0.95, -0.45, -0.25), (0.95, 0.45, 0.0))
wall = box("_tuong", (-1.2, -0.62, -0.3), (1.2, -0.46, 0.8))
chan = grid("Chan", -0.62, 0.60, -0.42, 0.80, 0.42, 36, 34)
# phủ bằng chiếu từ trên xuống (ổn định hơn mô phỏng rơi): độ cao = bề mặt người/nệm + căng phủ, mép phía phòng rủ xuống
dg = bpy.context.evaluated_depsgraph_get()
cme = col.evaluated_get(dg).data
cbvh = BVHTree.FromPolygons([v.co for v in cme.vertices], [p.vertices for p in cme.polygons])
EDGE = 0.45
H = {}
for v in chan.data.vertices:
    x, y = v.co.x, min(v.co.y, EDGE)
    hit = cbvh.ray_cast(Vector((x, y, 1.0)), Vector((0, 0, -1)), 2.0)
    H[v.index] = max(hit[0].z if hit[0] else 0.0, 0.0)
vs = list(chan.data.vertices)
pos = {v.index: Vector((v.co.x, min(v.co.y, EDGE), H[v.index])) for v in vs}
# căng phủ: z = max các điểm lân cận trừ độ võng theo khoảng cách (vải không lọt vào khe)
import itertools
pts = [(i, p) for i, p in pos.items()]
for i, p in pts:
    best = p.z
    for j, q in pts:
        d = (Vector((p.x, p.y)) - Vector((q.x, q.y))).length
        if d < 0.10: best = max(best, H[j] - d * 0.9)
    pos[i].z = best + 0.010
# mép phía phòng: phần vượt mép nệm rủ thẳng xuống
for v in vs:
    if v.co.y > EDGE:
        drop = v.co.y - EDGE
        pos[v.index] = Vector((v.co.x, EDGE + 0.012 + 0.02 * min(drop / 0.05, 1), pos[v.index].z - drop))
for v in vs: v.co = pos[v.index]
bm = bmesh.new(); bm.from_mesh(chan.data)
for _ in range(4):
    bmesh.ops.smooth_vert(bm, verts=[v for v in bm.verts if v.co.y < EDGE - 0.02], factor=0.5, use_axis_z=True)
bm.to_mesh(chan.data); bm.free()
for v in chan.data.vertices:   # không lún vào người
    if v.co.y < EDGE: v.co.z = max(v.co.z, H[v.index] + 0.008)
simulate(chan, 12, mass=0.3)   # rung nhẹ cho có nếp
for o in (col, mat, wall): bpy.data.objects.remove(o)
bpy.ops.object.shade_smooth()

# trọng số chăn: theo xương của bề mặt cơ thể gần nhất (mờ dần theo khoảng cách → Hips)
W, names = geo.vweights(body)
dg = bpy.context.evaluated_depsgraph_get(); be = body.evaluated_get(dg).data
BP = [body.matrix_world @ v.co for v in be.vertices]
bvh = BVHTree.FromPolygons(BP, [p.vertices for p in be.polygons])
OK = {"Hips", "Spine", "Chest", "UpperLeg_L", "UpperLeg_R", "LowerLeg_L", "LowerLeg_R", "UpperArm_L", "LowerArm_L", "Shoulder_L"}
for g in list(chan.vertex_groups): chan.vertex_groups.remove(g)
acc = np.zeros((len(chan.data.vertices), len(names)))
hi = names.index("Hips")
for v in chan.data.vertices:
    loc, n, fi, d = bvh.find_nearest(v.co)
    vs = be.polygons[fi].vertices
    w = np.mean([W[i] for i in vs], 0)
    w = np.array([ww if names[j] in OK else 0 for j, ww in enumerate(w)])
    if w.sum() == 0: w[hi] = 1
    k = math.exp(-max(d - 0.02, 0) / 0.06)
    acc[v.index] = w / w.sum() * k; acc[v.index, hi] += 1 - k
# mép trên gần tay phải đi theo tay một chút (cảnh "Em sợ" kéo chăn lên cằm)
hr = arm.pose.bones["Hand_R"].matrix.translation
ri = names.index("Hand_R")
for v in chan.data.vertices:
    d = (v.co - hr).length
    if d < 0.12: acc[v.index] *= 1 - 0.6 * (1 - d / 0.12); acc[v.index, ri] += 0.6 * (1 - d / 0.12)
acc /= acc.sum(1, keepdims=True)
for j, n in enumerate(names):
    idx = np.nonzero(acc[:, j] > 0.01)[0]
    if len(idx):
        g = chan.vertex_groups.new(name=n)
        for i in idx: g.add([int(i)], float(acc[i, j]), "REPLACE")

def unskin(obj):
    """toạ độ đang ở tư thế hiện tại → toạ độ nghỉ (nghịch đảo skinning tuyến tính, chính xác)"""
    Wo, no = geo.vweights(obj)
    Ms = {n: arm.pose.bones[n].matrix @ arm.data.bones[n].matrix_local.inverted() for n in no}
    for v in obj.data.vertices:
        M = Matrix(((0,) * 4,) * 4)
        for j, n in enumerate(no):
            if Wo[v.index, j] > 0: M = M + Ms[n] * Wo[v.index, j]
        v.co = M.inverted() @ v.co
    obj.data.update()

unskin(chan)
chan.parent = arm; m = chan.modifiers.new("Armature", "ARMATURE"); m.object = arm
print("chăn", geo.tris(chan))

# ── 2. khăn trán (dựng ở tư thế nghỉ: đầu thẳng) — dải gấp 16 × 5 cm, ôm theo trán
P.reset()
_t = bpy.data.objects["Toc"]; _nb = len(body.data.vertices)
bvh0 = BVHTree.FromPolygons([v.co for v in body.data.vertices] + [v.co for v in _t.data.vertices],
                            [tuple(p.vertices) for p in body.data.polygons] + [tuple(i + _nb for i in p.vertices) for p in _t.data.polygons])
def khan(name, dz=0.0, wet=False):
    bm = bmesh.new(); uv = bm.loops.layers.uv.new("UV")
    NX, NZ, T = 12, 3, 0.010
    rows = []
    for k in range(NZ + 1):
        z = 1.150 + dz + (k / NZ - 0.5) * 0.05
        row = []
        for i in range(NX + 1):
            x = (i / NX - 0.5) * 0.165
            h = bvh0.ray_cast(Vector((x, -0.4, z)), Vector((0, 1, 0)), 1.0)
            p = h[0] if h[0] else Vector((x, -0.1, z))
            row.append(p - Vector(h[1] if h[0] else (0, -1, 0)) * (-0.004))
        rows.append(row)
    def lay(off_sign):
        vs = [[bm.verts.new(p + Vector((0, -1, 0)) * (T if off_sign else 0)) for p in r] for r in rows]
        return vs
    A = lay(False); B = lay(True)
    for k in range(NZ):
        for i in range(NX):
            for L, flip in ((A, True), (B, False)):
                q = (L[k][i], L[k][i + 1], L[k + 1][i + 1], L[k + 1][i])
                f = bm.faces.new(q[::-1] if flip else q)
                for lp, (u, v) in zip(f.loops, [(i / NX, k / NZ), ((i + 1) / NX, k / NZ), ((i + 1) / NX, (k + 1) / NZ), (i / NX, (k + 1) / NZ)][::(-1 if flip else 1)]):
                    lp[uv].uv = (u, v * 0.5 + (0.5 if wet else 0))
    for k in range(NZ):   # mép hai đầu
        for i in (0, NX):
            bm.faces.new((A[k][i], A[k + 1][i], B[k + 1][i], B[k][i]) if i == 0 else (B[k][i], B[k + 1][i], A[k + 1][i], A[k][i]))
    for i in range(NX):
        bm.faces.new((A[0][i], A[0][i + 1], B[0][i + 1], B[0][i])[::-1]); bm.faces.new((A[NZ][i], A[NZ][i + 1], B[NZ][i + 1], B[NZ][i]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(o)
    o.parent = arm; g = o.vertex_groups.new(name="Head"); g.add(list(range(len(me.vertices))), 1, "REPLACE")
    o.modifiers.new("Armature", "ARMATURE").object = arm
    return o
khan("Khan_Kho"); khan("Khan_Uot", wet=True)

# ── 3. dây đeo tay (tam giác vải đỡ cẳng tay trái + quai vòng qua cổ) — tư thế nghỉ
def bpos(n, t): b = arm.data.bones[n]; return b.head_local.lerp(b.tail_local, t)
elbow = bpos("LowerArm_L", 0.0); wrist = bpos("LowerArm_L", 1.0)
fa = (wrist - elbow).normalized(); down = Vector((0, 0, -1))
side = fa.cross(down).normalized()
bm = bmesh.new(); uv = bm.loops.layers.uv.new("UV")
# máng đỡ: mặt cắt chữ U dưới cẳng tay (bán kính 4,6 cm), dài từ khuỷu tới cổ tay
NU, NL = 9, 8; ring = []
for i in range(NL + 1):
    c = elbow.lerp(wrist, i / NL) + fa * (0.01 if i == NL else 0)
    row = []
    for j in range(NU + 1):
        a = math.pi * (0.1 + 0.8 * j / NU)
        row.append(bm.verts.new(c + side * math.cos(a) * 0.046 + down * math.sin(a) * 0.05))
    ring.append(row)
for i in range(NL):
    for j in range(NU):
        f = bm.faces.new((ring[i][j], ring[i][j + 1], ring[i + 1][j + 1], ring[i + 1][j]))
        for lp, (u, v) in zip(f.loops, [(j / NU, i / NL), ((j + 1) / NU, i / NL), ((j + 1) / NU, (i + 1) / NL), (j / NU, (i + 1) / NL)]):
            lp[uv].uv = (u * 0.5, v)
# quai: từ hai đầu máng lên vòng qua sau gáy (dải rộng 4 cm), đẩy ra khỏi da 7 mm
neck = bpos("Neck", 0.3)
path = [wrist + Vector((0, 0, 0.02)), Vector((0.02, -0.09, 0.90)), Vector((-0.045, -0.06, 0.955)), Vector((-0.055, 0.0, 0.985)),
        Vector((-0.03, 0.05, 0.99)), Vector((0.03, 0.05, 0.99)), Vector((0.06, 0.0, 0.975)), Vector((0.085, -0.03, 0.93)), elbow + Vector((0, -0.01, 0.03))]
pts = []
for a_, b_ in zip(path, path[1:]):
    for t in np.linspace(0, 1, 5, endpoint=False): pts.append(a_.lerp(b_, t))
pts.append(path[-1])
prev = None
for k, p in enumerate(pts):
    h = bvh0.find_nearest(p)
    nrm = (p - h[0]).normalized() if h[0] and (p - h[0]).length > 1e-5 else Vector((0, -1, 0))
    p = h[0] + nrm * 0.008 if h[0] and (p - h[0]).length < 0.008 else p
    tan = (pts[min(k + 1, len(pts) - 1)] - pts[max(k - 1, 0)]).normalized()
    wv = tan.cross(nrm).normalized() * 0.02
    a_, b_ = bm.verts.new(p - wv), bm.verts.new(p + wv)
    if prev:
        f = bm.faces.new((prev[0], prev[1], b_, a_))
        for lp, (u, v) in zip(f.loops, [(0.55, k / len(pts)), (0.95, k / len(pts)), (0.95, (k + 1) / len(pts)), (0.55, (k + 1) / len(pts))]): lp[uv].uv = (u, v)
    prev = (a_, b_)
me = bpy.data.meshes.new("DayDeo"); bm.to_mesh(me); bm.free()
day = bpy.data.objects.new("DayDeo", me); bpy.context.scene.collection.objects.link(day)
day.parent = arm; day.modifiers.new("Armature", "ARMATURE").object = arm
geo.set_rigid(day, lambda co: "LowerArm_L" if co.z < 0.85 else ("Neck" if co.z > 0.96 else "Chest"))
print("dây đeo", geo.tris(day))

# ── 4. chăn vo (Bad A): chăn rơi chồng ở cuối giường — tĩnh, không xương
P.reset()
mat = box("_nem", (-0.95, -0.45, -0.25), (0.95, 0.45, 0.0)); wall = box("_tuong", (-1.2, -0.62, -0.3), (1.2, -0.46, 0.8))
vo = grid("ChanVo", -0.62, 0.50, -0.42, 0.78, 0.0, 30, 28)
for v in vo.data.vertices:   # gấp chữ Z rồi nâng lên — rơi xuống thành đống
    x = v.co.x; t = (x + 0.62) / 1.12
    seg = int(t * 3); f = t * 3 - seg
    xx = -0.85 + (f if seg % 2 == 0 else 1 - f) * 0.42
    v.co = Vector((xx, v.co.y * 0.85, 0.05 + seg * 0.05 + 0.03 * math.sin(v.co.y * 9)))
simulate(vo, 50, mass=0.2)
for o in (mat, wall): bpy.data.objects.remove(o)
vo.parent = arm
print("chăn vo", geo.tris(vo))

bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b4.blend")
