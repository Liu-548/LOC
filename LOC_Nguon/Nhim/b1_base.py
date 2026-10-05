# Bước 1: lưới MakeHuman bé gái 7 tuổi (châu Á), cao 1,20 m, gốc ở sàn giữa hai bàn chân, mặt trước −Y.
# Đối tượng: Body (da), Tights (lớp bó sát → may quần áo), Eye_L/R, MouthIn (răng + lưỡi), NhimRig (xương rút gọn + trọng số).
# Kết quả: out/b1.blend + out/b1_shapes.npz (độ lệch biểu cảm theo chỉ số base mesh, đã scale)
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import numpy as np, bpy, bmesh, mh, rig
from mathutils import Vector
OUT = "/home/claude/nhim/out"; os.makedirs(OUT, exist_ok=True)

V, VT, F = mh.load_obj()
W = mh.macro(gender=0.0, age_years=7, race={"asian": 1.0}, muscle=0.45, weight=0.40)
W["head/head-age-decr"] = 0.35      # đầu trẻ hơn một chút
W["cheek/l-cheek-inner-incr"] = W["cheek/r-cheek-inner-incr"] = 0.25   # má bầu
Vm = mh.morph(V, W)
P = mh.to_blender(Vm)
body_idx = sorted({i for f in F if f[0] == "body" for i in f[1]})
zmin = P[body_idx, 2].min(); S = 1.20 / (P[body_idx, 2].max() - zmin)
xc = P[body_idx, 0].mean()
def T(A): return (np.asarray(A) - [0, 0, zmin]) * S
P = T(P)

# biểu cảm (độ lệch, đã scale) — gộp vài unit thành các shape key dùng trong game
def delta(ws):
    return mh.to_blender(mh.morph(Vm, ws) - Vm + 0) * S * 1.0
E = "expression/units/asian/"
SHAPES = {
    "MatNham":  {E + "eye-left-closure": 1, E + "eye-right-closure": 1},
    "MatHe":    {E + "eye-left-slit": 0.8, E + "eye-right-slit": 0.8, E + "eye-left-closure": 0.35, E + "eye-right-closure": 0.35},
    "MiengMo":  {E + "mouth-open": 0.55, E + "mouth-parling": 0.4},
    "Cuoi":     {E + "mouth-corner-puller": 0.75, E + "mouth-elevation": 0.25, E + "eye-left-slit": 0.25, E + "eye-right-slit": 0.25},
    "MayNhau":  {E + "eyebrows-left-inner-up": 0.8, E + "eyebrows-right-inner-up": 0.8, E + "mouth-depression": 0.25},
    "NhamChat": {E + "eye-left-closure": 1, E + "eye-right-closure": 1, E + "eyebrows-left-down": 0.7, E + "eyebrows-right-down": 0.7,
                 E + "nose-compression": 0.5, E + "mouth-compression": 0.5},
}
D = {}
for k, ws in SHAPES.items():
    d = mh.morph(Vm, ws) - Vm
    D[k] = np.stack([d[:, 0], -d[:, 2], d[:, 1]], -1) * 0.1 * S
np.savez(f"{OUT}/b1_shapes.npz", **D)

bpy.ops.wm.read_factory_settings(use_empty=True)
sk = mh.load_skel(); Jt = sk["joints"]
def J(n): return Vector(P[Jt[n]].mean(0))

def mk(name, groups):
    fs = [f for f in F if f[0] in groups]
    used = sorted({i for f in fs for i in f[1]}); rm = {o: i for i, o in enumerate(used)}
    me = bpy.data.meshes.new(name)
    me.from_pydata([tuple(P[i]) for i in used], [], [[rm[i] for i in f[1]] for f in fs]); me.update()
    uv = me.uv_layers.new(name="UV")
    li = 0
    for f in fs:
        for t in f[2]:
            uv.data[li].uv = VT[t] if t >= 0 else (0, 0); li += 1
    for p in me.polygons: p.use_smooth = True
    o = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(o)
    o["base_idx"] = used   # chỉ số base mesh của từng đỉnh
    return o, rm

objs = {n: mk(n, g) for n, g in [("Body", ["body"]), ("Tights", ["helper-tights"]), ("Eye_L", ["helper-l-eye"]), ("Eye_R", ["helper-r-eye"]),
                                 ("MouthIn", ["helper-upper-teeth", "helper-lower-teeth", "helper-tongue"])]}

# pháp tuyến mu bàn tay (hướng ra khỏi lòng bàn tay)
palm = {}
for s, S_ in (("_L", ".L"), ("_R", ".R")):
    a = J(f"finger2-1{S_}____head"); b = J(f"finger5-1{S_}____head"); w = J(f"wrist{S_}____head"); m = J(f"finger3-1{S_}____head")
    n = (b - a).cross(m - w).normalized()
    # mu bàn tay ở tư thế A hướng ra ngoài–lên; chọn dấu sao cho cùng phía với trục ra ngoài cơ thể (+x bên trái)
    if n.dot(Vector((1 if s == "_L" else -1, 0, 0.6))) < 0: n = -n
    palm[s] = n
arm = rig.make_armature(rig.bone_defs(J), palm)

mhw = mh.load_weights()
for n, (o, rm) in objs.items():
    w = rig.mh_weights(mhw, rm, len(o.data.vertices))
    if n.startswith("Eye"): w = {"Eye" + n[-2:]: (np.arange(len(o.data.vertices)), np.ones(len(o.data.vertices)))}
    if n == "MouthIn": w = {"Head": (np.arange(len(o.data.vertices)), np.ones(len(o.data.vertices)))}
    if not w:
        print("KHÔNG có trọng số cho", n)
    rig.skin(o, arm, w)
    print(n, len(o.data.vertices), "đỉnh", len(o.data.polygons), "mặt", "xương:", len(w))

bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b1.blend")
print("chiều cao", max(v.co.z for v in objs["Body"][0].data.vertices))
