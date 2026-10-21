# DÌ — Bước 2: tư thế nghỉ (tay buông), áo bà ba dài tay (vạt ngang hông), quần lụa ống suông tới mắt cá, xoá da bị che.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, numpy as np, rig, geo, math
from mathutils import Vector, Matrix
OUT = "/home/claude/di/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b1.blend")
arm = bpy.data.objects["NhimRig"]; arm.name = arm.data.name = "DiRig"
body = bpy.data.objects["Body"]; tights = bpy.data.objects["Tights"]
meshes = [o for o in bpy.data.objects if o.type == "MESH"]

P = rig.Poser(arm)
P.apply({
    "Shoulder_L": {"d": (1, 0.05, -0.14)}, "Shoulder_R": {"d": (-1, 0.05, -0.14)},
    "UpperArm_L": {"d": (0.17, 0.03, -1)}, "LowerArm_L": {"d": (0.06, -0.14, -1)}, "Hand_L": {"d": (0.05, -0.12, -1)},
    "UpperArm_R": {"d": (-0.17, 0.03, -1)}, "LowerArm_R": {"d": (-0.06, -0.14, -1)}, "Hand_R": {"d": (-0.05, -0.12, -1)},
})
for s in ("_L", "_R"):
    for f in ("Index", "Middle", "Ring", "Pinky"):
        arm.pose.bones[f + "1" + s].matrix_basis = Matrix.Rotation(np.radians(-12), 4, "X")
        arm.pose.bones[f + "2" + s].matrix_basis = Matrix.Rotation(np.radians(-16), 4, "X")
bpy.context.view_layer.update()
for o in meshes:
    with bpy.context.temp_override(object=o, active_object=o):
        bpy.ops.object.modifier_apply(modifier="Armature")
bpy.context.view_layer.objects.active = arm; arm.select_set(True)
bpy.ops.object.mode_set(mode="POSE"); bpy.ops.pose.select_all(action="SELECT"); bpy.ops.pose.armature_apply(selected=False)
bpy.ops.object.mode_set(mode="OBJECT")
for o in meshes:
    m = o.modifiers.new("Armature", "ARMATURE"); m.object = arm

full = geo.dup(body, "Body_Full"); full.hide_render = True; full.hide_set(True)
dom, W, names = geo.dominant(body)
def par(b, co): return geo.bone_param(arm, b, co)

NB = arm.data.bones["Neck"].head_local
NECK_C = (NB.x, NB.y)
def neck_r(co): return math.hypot(co.x - NECK_C[0], (co.y - NECK_C[1]) * 1.15)
def neckline(co):   # cổ tròn áo bà ba: trước 1,300, sau 1,335
    a = math.atan2(co.x, -(co.y - NECK_C[1])); return 1.300 + 0.035 * (1 - math.cos(a)) / 2
HEM_Z, WAIST_Z, ANKLE = 0.815, 0.97, 0.115
SLEEVE = 0.90   # tay áo dài tới gần cổ tay
tdom, _, _ = geo.dominant(tights)
print("nhóm xương tights:", sorted(set(tdom)))

def shirt_v(v, d):
    z = v.co.z
    if d in ("Neck", "Head") or (z > neckline(v.co) - 0.005 and neck_r(v.co) < 0.09): return False
    if d.startswith("UpperArm"): return True
    if d.startswith("LowerArm"): return par(d, v.co) < SLEEVE
    if d.startswith(("Hand", "Thumb", "Index", "Middle", "Ring", "Pinky")): return False
    return z > HEM_Z
def pants_v(v, d):
    z = v.co.z
    if d.startswith(("Foot", "Toes")): return False
    if d.startswith("LowerLeg"): return z > ANKLE
    return (z < WAIST_Z and d in ("Hips", "Spine")) or d.startswith("UpperLeg")
def plane_snap(bone, t):
    b = arm.data.bones[bone]; h = Vector(b.head_local); d = Vector(b.tail_local) - h
    p0 = h + d * t; n = d.normalized()
    return lambda co: co - n * (co - p0).dot(n)
def snap_ao(co):
    if co.z > 1.2 and neck_r(co) < 0.11: return Vector((co.x, co.y, neckline(co)))
    if co.z < HEM_Z + 0.06: return Vector((co.x, co.y, HEM_Z))
    for s in ("_L", "_R"):
        if (co.x > 0) == (s == "_L") and abs(co.x) > 0.2: return plane_snap("LowerArm" + s, SLEEVE)(co)
    return None
def snap_quan(co):
    if co.z > WAIST_Z - 0.08: return Vector((co.x, co.y, WAIST_Z))
    if co.z < 0.3: return Vector((co.x, co.y, ANKLE))
    return None
REMAP = {'Head': 'Neck', 'Hand_L': 'LowerArm_L', 'Hand_R': 'LowerArm_R', 'Foot_L': 'LowerLeg_L', 'Foot_R': 'LowerLeg_R'}
def off_ao(co, n):   # áo rộng: thân 1,8 cm, tay áo 1,4 cm, vạt dưới loe thêm
    return 0.014 + 0.020 * np.clip((1.02 - co.z) / 0.2, 0, 1)
def off_quan(co, n):   # ống quần suông: loe dần xuống mắt cá
    return 0.004 if co.z > 0.80 else 0.008 + 0.024 * np.clip((0.75 - co.z) / 0.55, 0, 1)   # trên hông nằm gọn dưới áo
for name, fn, offf, rim, snap, bones in (
        ("Ao", shirt_v, off_ao, 0.010, snap_ao, {'Hips','Spine','Chest','Neck','Shoulder_L','Shoulder_R','UpperArm_L','UpperArm_R','LowerArm_L','LowerArm_R'}),
        ("Quan", pants_v, off_quan, 0.009, snap_quan, {'Hips','Spine','UpperLeg_L','UpperLeg_R','LowerLeg_L','LowerLeg_R'})):
    o = geo.dup(tights, name)
    keep = {v.index for v in tights.data.vertices if fn(v, tdom[v.index])}
    geo.keep_faces(o, lambda f: all(v.index in keep for v in f.verts))
    geo.weld(o, center=0.004)
    geo.snap_boundary(o, snap)
    geo.transfer_weights(o, body, bones=bones, remap=REMAP)
    geo.inflate(o, 0, smooth=6, rim=rim, off_fn=offf, smooth_fac=0.5)
    print(name, geo.tris(o), "tam giác")
bpy.data.objects.remove(tights)

def covered(v, d):
    z = v.co.z; co = v.co
    if d.startswith("UpperArm"): return True
    if d.startswith("LowerArm"): return par(d, co) < SLEEVE - 0.07
    if d in ("Spine", "Chest", "Hips") or d.startswith(("Shoulder", "UpperLeg")):
        return z < neckline(co) - 0.03 or neck_r(co) > 0.11
    if d.startswith("LowerLeg"): return z > ANKLE + 0.04
    return False
cov = {v.index for v in body.data.vertices if covered(v, dom[v.index])}
geo.keep_faces(body, lambda f: not all(v.index in cov for v in f.verts))
print("da còn", geo.tris(body))
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b2a.blend")
