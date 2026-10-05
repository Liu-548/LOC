# Bước 2: tư thế nghỉ mới (tay trái gập 90° trước bụng — tay bó bột), bột, áo bộ cộc tay, quần lửng, xoá da bị che, giảm lưới.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, numpy as np, rig, geo
from mathutils import Vector, Matrix
OUT = "/home/claude/nhim/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b1.blend")
arm = bpy.data.objects["NhimRig"]; body = bpy.data.objects["Body"]; tights = bpy.data.objects["Tights"]
meshes = [o for o in bpy.data.objects if o.type == "MESH"]

# ── 1. tư thế nghỉ: tay buông tự nhiên, tay trái gập 90° (cẳng tay ngang trước bụng, ngón cái lên)
P = rig.Poser(arm)
P.apply({
    "Shoulder_L": {"d": (1, 0.05, -0.12)}, "Shoulder_R": {"d": (-1, 0.05, -0.12)},
    "UpperArm_L": {"d": (0.22, 0.02, -1)}, "LowerArm_L": {"d": (-0.55, -0.83, 0.06)}, "Hand_L": {"d": (-0.62, -0.78, 0.04)},
    "UpperArm_R": {"d": (-0.25, 0.03, -1)}, "LowerArm_R": {"d": (-0.1, -0.12, -1)}, "Hand_R": {"d": (-0.08, -0.15, -1)},
})
# ngón tay trái hơi co (thò ra khỏi bột), ngón cái chỉ lên
for f in ("Index", "Middle", "Ring", "Pinky"):
    arm.pose.bones[f + "1_L"].matrix_basis = Matrix.Rotation(np.radians(-18), 4, "X")
    arm.pose.bones[f + "2_L"].matrix_basis = Matrix.Rotation(np.radians(-22), 4, "X")
bpy.context.view_layer.update()
for o in meshes:
    with bpy.context.temp_override(object=o, active_object=o):
        bpy.ops.object.modifier_apply(modifier="Armature")
bpy.context.view_layer.objects.active = arm; arm.select_set(True)
bpy.ops.object.mode_set(mode="POSE"); bpy.ops.pose.select_all(action="SELECT"); bpy.ops.pose.armature_apply(selected=False)
bpy.ops.object.mode_set(mode="OBJECT")
for o in meshes:
    m = o.modifiers.new("Armature", "ARMATURE"); m.object = arm

# lưu bản da full để chuyển shape key sau này
full = geo.dup(body, "Body_Full"); full.hide_render = True; full.hide_set(True)

dom, W, names = geo.dominant(body)
def par(b, co): return geo.bone_param(arm, b, co)

# ── 2. bột tay trái (vỏ dày 1,4 cm quanh cánh tay dưới khuỷu…đốt ngón, vạt trên khuỷu)
def in_cast(v, d, margin=0.0):
    if d == "UpperArm_L": return par(d, v.co) > 0.50 + margin
    if d == "LowerArm_L": return True
    if d == "Hand_L": return par(d, v.co) < 0.86 - margin
    if d == "Thumb1_L": return par(d, v.co) < 0.35 - margin
    return False
cast = geo.dup(body, "Bot")
cv = {v.index for v in body.data.vertices if in_cast(v, dom[v.index])}
geo.keep_faces(cast, lambda f: all(v.index in cv for v in f.verts))
geo.inflate(cast, 0.0, smooth=0)
geo.inflate(cast, 0.016, smooth=14, rim=0.012, smooth_fac=0.6)
geo.decimate(cast, 0.18, sym=False)
# bột cứng: trên khuỷu theo cánh tay, dưới khuỷu theo cẳng tay
elbow = arm.matrix_world @ arm.data.bones["LowerArm_L"].head_local
ua = (arm.data.bones["UpperArm_L"].tail_local - arm.data.bones["UpperArm_L"].head_local).normalized()
geo.set_rigid(cast, lambda co: "UpperArm_L" if (Vector(co) - elbow).dot(-ua) > 0.015 else "LowerArm_L")
print("bột", geo.tris(cast), "tam giác")

# ── 3. áo và quần từ lớp bó sát (Tights)
tdom, _, _ = geo.dominant(tights)
import math
NECK_C = (arm.data.bones["Neck"].head_local.x, arm.data.bones["Neck"].head_local.y)
def neck_r(co): return math.hypot(co.x - NECK_C[0], (co.y - NECK_C[1]) * 1.15)
def neckline(co):   # cổ tròn: trước thấp 0,945, sau 0,968
    a = math.atan2(co.x, -(co.y - NECK_C[1])); return 0.945 + 0.023 * (1 - math.cos(a)) / 2

NECK_Z, HEM_Z, WAIST_Z = 0.957, 0.585, 0.695
def shirt_v(v, d):
    z = v.co.z
    if d in ("Neck", "Head") or (z > neckline(v.co) - 0.004 and neck_r(v.co) < 0.072): return False
    if d.startswith("UpperArm"): return par(d, v.co) < 0.50
    if d.startswith(("LowerArm", "Hand", "Thumb", "Index", "Middle", "Ring", "Pinky")): return False
    return z > HEM_Z
def shorts_v(v, d):
    z = v.co.z
    if d.startswith("LowerLeg"): return par(d, v.co) < 0.55
    if d.startswith(("Foot", "Toes")): return False
    return z < WAIST_Z and d in ("Hips", "Spine") or d.startswith("UpperLeg")
def plane_snap(bone, t):
    b = arm.data.bones[bone]; h = Vector(b.head_local); d = Vector(b.tail_local) - h
    p0 = h + d * t; n = d.normalized()
    return lambda co: co - n * (co - p0).dot(n)
def snap_ao(co):
    if co.z > 0.9 and neck_r(co) < 0.085: return Vector((co.x, co.y, neckline(co)))
    if co.z < HEM_Z + 0.05: return Vector((co.x, co.y, HEM_Z))
    for s in ("_L", "_R"):
        if (co.x > 0) == (s == "_L") and abs(co.x) > 0.1: return plane_snap("UpperArm" + s, 0.5)(co)
    return None
def snap_quan(co):
    if co.z > WAIST_Z - 0.06: return Vector((co.x, co.y, WAIST_Z))
    for s in ("_L", "_R"):
        if (co.x > 0) == (s == "_L") and co.z < 0.4: return plane_snap("LowerLeg" + s, 0.55)(co)
    return None
REMAP = {'Head': 'Neck', 'LowerArm_L': 'UpperArm_L', 'LowerArm_R': 'UpperArm_R', 'Hand_L': 'UpperArm_L', 'Hand_R': 'UpperArm_R', 'Foot_L': 'LowerLeg_L', 'Foot_R': 'LowerLeg_R'}
for name, fn, off, rim, snap in (("Ao", shirt_v, 0.017, 0.009, snap_ao), ("Quan", shorts_v, 0.008, 0.008, snap_quan)):
    o = geo.dup(tights, name)
    keep = {v.index for v in tights.data.vertices if fn(v, tdom[v.index])}
    geo.keep_faces(o, lambda f: all(v.index in keep for v in f.verts))
    geo.weld(o, center=0.004)
    geo.snap_boundary(o, snap)
    geo.transfer_weights(o, body, bones={'Hips','Spine','Chest','Neck','Shoulder_L','Shoulder_R','UpperArm_L','UpperArm_R'} if name == 'Ao' else {'Hips','Spine','UpperLeg_L','UpperLeg_R','LowerLeg_L','LowerLeg_R'}, remap=REMAP)
    geo.inflate(o, off, smooth=6, rim=rim, smooth_fac=0.5)
    print(name, geo.tris(o), "tam giác")
bpy.data.objects.remove(tights)

# ── 4. xoá da bị che (chừa 3 cm trong mép áo/quần/bột cho khỏi lộ khe)
def covered(v, d):
    z = v.co.z; co = v.co
    if in_cast(v, d, margin=0.05): return True
    if d.startswith("UpperArm"): return par(d, co) < 0.44
    if d in ("Spine", "Chest", "Hips") or d.startswith(("Shoulder", "UpperLeg")):
        return z < neckline(co) - 0.03 or neck_r(co) > 0.095
    if d.startswith("LowerLeg"): return par(d, co) < 0.48
    return False
cov = {v.index for v in body.data.vertices if covered(v, dom[v.index])}
geo.keep_faces(body, lambda f: not all(v.index in cov for v in f.verts))
print("da còn", geo.tris(body))

bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b2a.blend")
