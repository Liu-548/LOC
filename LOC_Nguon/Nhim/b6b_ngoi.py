# [6/10] Good: bỏ vẫy tay — thay clip Ngoi_VayTay bằng Ngoi_NoiChuyen (ngồi mép giường, tay phải đặt trên đùi, thở nhẹ).
# Mở b6.blend (đã có đủ 11 clip), chỉ thay 1 clip rồi xuất lại Nhim.fbx.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, json, rig, geo
from mathutils import Vector, Matrix
from poses import merge
OUT = sys.argv[sys.argv.index("--out") + 1] if "--out" in sys.argv else "/home/claude/nhim/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b6.blend")
arm = bpy.data.objects["NhimRig"]; P = rig.Poser(arm)
for b in arm.pose.bones: b.rotation_mode = "QUATERNION"

def posed_min(names, filt):
    dg = bpy.context.evaluated_depsgraph_get(); m = 9
    for n in names:
        o = bpy.data.objects[n]; e = o.evaluated_get(dg); W = geo.vweights(o)
        for v in e.data.vertices:
            if filt(o, v.index, W): m = min(m, (o.matrix_world @ v.co).z)
    return m
def buttock(o, i, W):
    w, names = W
    j = names.index("Hips") if "Hips" in names else None
    return j is not None and w[i, j] > 0.5

SIT = {"Spine": {"d": (0, 0.03, 1)}, "Chest": {"d": (0, 0.02, 1)}, "Neck": {"d": (0, -0.08, 1)}, "Head": {"d": (0, -0.06, 1)},
       "UpperLeg_L": {"d": (0.10, -1, -0.06)}, "UpperLeg_R": {"d": (-0.10, -1, -0.06)},
       "LowerLeg_L": {"d": (0.02, 0.28, -1)}, "LowerLeg_R": {"d": (-0.02, 0.22, -1)},
       "Foot_L": {"d": (0, -0.55, -0.85)}, "Foot_R": {"d": (0, -0.55, -0.85)},
       # tay phải buông, cẳng tay đặt dọc đùi phải
       "UpperArm_R": {"d": (-0.10, -0.28, -0.95)}, "LowerArm_R": {"d": (0.10, -0.93, -0.35)}, "Hand_R": {"d": (0.08, -0.9, -0.42)}}
THO = merge(SIT, {"Chest": {"d": (0, 0.0, 1)}, "Neck": {"d": (0, -0.09, 1)}})   # hít vào nhẹ

P.apply(SIT)
h = arm.pose.bones["Hips"].matrix.translation.copy()
seat = Vector((0, 0.02, h.z + 0.50 - posed_min(("Quan",), buttock)))

old = bpy.data.actions.get("Ngoi_VayTay")
if old: bpy.data.actions.remove(old)
act = bpy.data.actions.new("Ngoi_NoiChuyen"); act.use_fake_user = True
arm.animation_data_create(); arm.animation_data.action = act
for f, spec in ((0, SIT), (60, THO), (120, SIT)):
    P.apply(spec, hips_loc=seat); P.key(f)

arm.animation_data.action = bpy.data.actions["Nam_Ngu"]; P.reset()
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b6.blend")
for o in bpy.data.objects: o.select_set(o.type in ("ARMATURE", "MESH"))
bpy.ops.export_scene.fbx(filepath=f"{OUT}/Nhim.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                         axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", add_leaf_bones=False,
                         bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                         bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0,
                         use_armature_deform_only=True, armature_nodetype="NULL", path_mode="STRIP", embed_textures=False,
                         use_mesh_modifiers=False, primary_bone_axis="Y", secondary_bone_axis="X")
print("FBX", os.path.getsize(f"{OUT}/Nhim.fbx") // 1024, "KB", "· actions:", [a.name for a in bpy.data.actions])
