# DÌ — Bước 4: 3 clip (30 fps): Dung_Cho (đứng chờ, tay đan trước bụng), Dung_Noi (đứng nói, gật nhẹ), Di_Bo (đi tại chỗ — code dời người) + xuất FBX.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, json, rig
from mathutils import Vector, Matrix
from poses import merge
OUT = "/home/claude/di/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b3.blend")
arm = bpy.data.objects["DiRig"]; P = rig.Poser(arm)
bpy.context.scene.render.fps = 30
for b in arm.pose.bones: b.rotation_mode = "QUATERNION"
ACT = {}
def clip(name, keys, loop):
    act = bpy.data.actions.new(name); act.use_fake_user = True
    arm.animation_data_create(); arm.animation_data.action = act
    for f, spec, hl in keys:
        P.apply(spec, hips_loc=hl); P.key(f)
    ACT[name] = (keys[-1][0], loop)
REST_H = arm.data.bones["Hips"].head_local.copy()
def H(dz=0.0): return REST_H + Vector((0, 0, dz))

DAN = {   # tay đan trước bụng
    "UpperArm_L": {"d": (0.12, -0.30, -0.95)}, "LowerArm_L": {"d": (-0.72, -0.62, -0.30)}, "Hand_L": {"d": (-0.80, -0.55, -0.25)},
    "UpperArm_R": {"d": (-0.12, -0.30, -0.95)}, "LowerArm_R": {"d": (0.72, -0.62, -0.22)}, "Hand_R": {"d": (0.80, -0.55, -0.18)},
    "Spine": {"d": (0, 0.02, 1)}, "Chest": {"d": (0, 0.03, 1)}, "Neck": {"d": (0, -0.30, 1)}, "Head": {"d": (0, -0.12, 1)},
    "UpperLeg_L": {"d": (0.06, 0.0, -1)}, "UpperLeg_R": {"d": (-0.06, 0.0, -1)}, "LowerLeg_L": {"d": (0.05, 0.03, -1)}, "LowerLeg_R": {"d": (-0.05, 0.03, -1)},
}
HIT = merge(DAN, {"Chest": {"d": (0, 0.0, 1)}, "Head": {"d": (0, -0.10, 1)}})
clip("Dung_Cho", [(0, DAN, H()), (55, HIT, H(0.003)), (110, DAN, H())], True)
GAT = merge(DAN, {"Head": {"d": (0, -0.32, 1)}, "Neck": {"d": (0, -0.36, 1)}})
NGHIENG = merge(DAN, {"Head": {"d": (0.10, -0.18, 1)}})
clip("Dung_Noi", [(0, DAN, H()), (18, GAT, H(-0.003)), (34, DAN, H()), (60, NGHIENG, H()), (80, GAT, H(-0.003)), (96, DAN, H())], True)

def buoc(s):   # s=+1: chân trái trước
    L, R = ("_L", "_R") if s > 0 else ("_R", "_L")
    sx = 1 if L == "_L" else -1
    return {"Spine": {"d": (0, -0.04, 1)}, "Chest": {"d": (0, -0.02, 1)}, "Neck": {"d": (0, -0.25, 1)}, "Head": {"d": (0, -0.10, 1)},
            "UpperLeg" + L: {"d": (0.05 * sx, -0.42, -0.91)}, "LowerLeg" + L: {"d": (0.03 * sx, -0.10, -1)}, "Foot" + L: {"d": (0, -0.75, -0.05)},
            "UpperLeg" + R: {"d": (-0.05 * sx, 0.36, -0.93)}, "LowerLeg" + R: {"d": (-0.03 * sx, 0.45, -0.89)}, "Foot" + R: {"d": (0, -0.55, -0.7)},
            "UpperArm" + L: {"d": (0.12 * sx, 0.22, -0.97)}, "LowerArm" + L: {"d": (0.08 * sx, 0.05, -1)},
            "UpperArm" + R: {"d": (-0.12 * sx, -0.25, -0.96)}, "LowerArm" + R: {"d": (-0.08 * sx, -0.45, -0.88)}}
def qua(s):    # chân sau đang đưa qua
    L, R = ("_L", "_R") if s > 0 else ("_R", "_L")
    sx = 1 if L == "_L" else -1
    return {"Spine": {"d": (0, -0.04, 1)}, "Neck": {"d": (0, -0.25, 1)}, "Head": {"d": (0, -0.10, 1)},
            "UpperLeg" + L: {"d": (0.05 * sx, 0.0, -1)}, "LowerLeg" + L: {"d": (0.03 * sx, 0.06, -1)},
            "UpperLeg" + R: {"d": (-0.05 * sx, -0.25, -0.97)}, "LowerLeg" + R: {"d": (-0.03 * sx, 0.55, -0.83)}, "Foot" + R: {"d": (0, -0.8, -0.4)},
            "UpperArm_L": {"d": (0.12, 0.0, -1)}, "UpperArm_R": {"d": (-0.12, 0.0, -1)}}
clip("Di_Bo", [(0, buoc(1), H(-0.025)), (8, qua(1), H(0.008)), (16, buoc(-1), H(-0.025)), (24, qua(-1), H(0.008)), (32, buoc(1), H(-0.025))], True)
json.dump({k: {"frames": v[0], "loop": v[1]} for k, v in ACT.items()}, open(f"{OUT}/clips.json", "w"), indent=1)
arm.animation_data.action = bpy.data.actions["Dung_Cho"]; P.reset()
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b4.blend")
for o in bpy.data.objects: o.select_set(o.type in ("ARMATURE", "MESH"))
bpy.ops.export_scene.fbx(filepath=f"{OUT}/Di.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                         axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", add_leaf_bones=False,
                         bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                         bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0,
                         use_armature_deform_only=True, armature_nodetype="NULL", path_mode="STRIP", embed_textures=False,
                         use_mesh_modifiers=False, primary_bone_axis="Y", secondary_bone_axis="X")
print("FBX", os.path.getsize(f"{OUT}/Di.fbx") // 1024, "KB")
