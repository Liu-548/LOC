# BÁC TÀI — Bước 4: ngồi lái, hai tay nắm vô lăng (IK hai đoạn), chân đặt lên bàn đạp; áp tư thế vào lưới; đặt mốc; xuất FBX tĩnh.
#  Toạ độ trong xe (Unity, cục bộ xe buýt): khớp háng giữa = (−0,6; 1,29; 3,62) · tâm vô lăng (−0,6; 1,42; 4,05) bán kính 0,2,
#  pháp tuyến (0; 0,55; −0,83) · sàn 0,75. Ở Blender: mặt trước = −Y, Z lên → độ lệch (Unity Δx, Δy, Δz) = (Δx, −Δz, Δy).
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, math, numpy as np, rig
from mathutils import Vector, Matrix
OUT = "/home/claude/bactai/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b3.blend")
O = bpy.data.objects; arm = O["TaiRig"]; P = rig.Poser(arm); pb = arm.pose.bones
def U(dx, dy, dz): return Vector((dx, -dz, dy))   # độ lệch Unity → Blender

LEAN = float(os.environ.get("LEAN", "-0.22"))
THAN = {"Spine": {"d": (0, LEAN, 1)}, "Chest": {"d": (0, LEAN + 0.04, 1)}, "Neck": {"d": (0, -0.20, 1)}, "Head": {"d": (0, -0.04, 1)}}
P.apply(THAN)
def J(b, tail=False): x = pb[b]; return arm.matrix_world @ (x.tail if tail else x.head)
HJ = (J("UpperLeg_L") + J("UpperLeg_R")) / 2
print("LEAN", LEAN, "ngực - háng", tuple(round(v, 3) for v in (J("Neck") - HJ)), "hips rest", tuple(round(v,3) for v in (J("Spine") - HJ)))
L = lambda b: arm.data.bones[b].length

def ik(S, T, l1, l2, pole):
    d = (T - S); dl = min(d.length, l1 + l2 - 1e-3); dirn = d.normalized()
    a = (l1 * l1 - l2 * l2 + dl * dl) / (2 * dl); h = math.sqrt(max(l1 * l1 - a * a, 0))
    p = Vector(pole); p = (p - dirn * p.dot(dirn)).normalized()
    E = S + dirn * a + p * h
    return (E - S).normalized(), (S + dirn * dl - E).normalized()

# vô lăng
W = HJ + U(0, 0.13, 0.43); n = U(0, 0.55, -0.83).normalized()
upv = (Vector((0, 0, 1)) - n * n.dot(Vector((0, 0, 1)))).normalized(); rx = Vector((1, 0, 0))
spec = dict(THAN)
for s, sg in (("_L", 1), ("_R", -1)):
    grip = W + rx * (sg * 0.2 * math.cos(math.radians(28))) + upv * (0.2 * math.sin(math.radians(28)))
    S = J("UpperArm" + s); l1 = L("UpperArm" + s); l2 = L("LowerArm" + s)
    wrist = grip - (grip - S).normalized() * 0.075 - upv * 0.01
    du, dl = ik(S, wrist, l1, l2, (sg * 0.9, 0.25, -1.0))
    spec["UpperArm" + s] = {"d": tuple(du)}; spec["LowerArm" + s] = {"d": tuple(dl)}
    spec["Hand" + s] = {"d": tuple((grip - wrist).normalized())}
    # chân: mắt cá trên bàn đạp, đầu gối nhô lên phía trước
    Sl = J("UpperLeg" + s); A = HJ + U(sg * 0.12, -0.47, 0.50)
    du, dl = ik(Sl, A, L("UpperLeg" + s), L("LowerLeg" + s), (0, -0.4, 1))
    spec["UpperLeg" + s] = {"d": tuple(du)}; spec["LowerLeg" + s] = {"d": tuple(dl)}
    spec["Foot" + s] = {"d": tuple(U(sg * 0.05, -0.38, 0.92).normalized())}
P.apply(spec)
for s in ("_L", "_R"):   # nắm chặt vô lăng
    for f in ("Index", "Middle", "Ring", "Pinky"):
        pb[f + "1" + s].matrix_basis = pb[f + "1" + s].matrix_basis @ Matrix.Rotation(math.radians(-62), 4, "X")
        pb[f + "2" + s].matrix_basis = pb[f + "2" + s].matrix_basis @ Matrix.Rotation(math.radians(-70), 4, "X")
    pb["Thumb1" + s].matrix_basis = pb["Thumb1" + s].matrix_basis @ Matrix.Rotation(math.radians(-25), 4, "X")
    pb["Thumb2" + s].matrix_basis = pb["Thumb2" + s].matrix_basis @ Matrix.Rotation(math.radians(-35), 4, "X")
bpy.context.view_layer.update()
HJ = (J("UpperLeg_L") + J("UpperLeg_R")) / 2
print("HJ", tuple(round(v, 3) for v in HJ), "tay", [tuple(round(v, 3) for v in J("Hand" + s, True)) for s in ("_L", "_R")], "vô lăng", tuple(round(v, 3) for v in W))

meshes = [o for o in O if o.type == "MESH"]
for o in meshes:
    with bpy.context.temp_override(object=o, active_object=o):
        for m in list(o.modifiers):
            if m.type == "ARMATURE": bpy.ops.object.modifier_apply(modifier=m.name)
    o.parent = None
    o.matrix_world = o.matrix_world   # giữ chỗ
    for g in list(o.vertex_groups): o.vertex_groups.remove(g)
    for p in o.data.polygons: p.use_smooth = False
# gốc = khớp háng giữa; dời mọi lưới cho gốc về (0,0,0)
for o in meshes:
    o.data.transform(Matrix.Translation(-HJ) @ o.matrix_world); o.matrix_world = Matrix()
bpy.data.objects.remove(arm)
# vô lăng tham chiếu (chỉ để xem trước, không xuất)
bpy.ops.mesh.primitive_torus_add(major_radius=0.2, minor_radius=0.015, location=tuple(W - HJ))
vl = bpy.context.object; vl.name = "_VoLang"; vl.rotation_euler = n.to_track_quat("Z", "Y").to_euler()
for nm, p in (("Goc", Vector((0, 0, 0))), ("Truoc", Vector((0, -0.5, 0))), ("Tren", Vector((0, 0, 0.5)))):
    e = bpy.data.objects.new(nm, None); bpy.context.scene.collection.objects.link(e); e.location = p
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b4.blend")
for o in O: o.select_set(o.type == "MESH" and not o.name.startswith("_") or o.type == "EMPTY")
bpy.ops.export_scene.fbx(filepath=f"{OUT}/BacTai.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                         axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", add_leaf_bones=False, bake_anim=False,
                         path_mode="STRIP", embed_textures=False, use_mesh_modifiers=True)
print("FBX", os.path.getsize(f"{OUT}/BacTai.fbx") // 1024, "KB", sum(len(p.vertices) - 2 for o in O if o.type == "MESH" and not o.name.startswith("_") for p in o.data.polygons), "tam giác")
