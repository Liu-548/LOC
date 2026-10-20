# Bước 6: 11 clip (30 fps) + xuất FBX cho Unity.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, numpy as np, json, rig, poses, geo
from mathutils import Vector, Matrix
from poses import merge, curl, NAM, CURL_R, G_NAM
OUT = "/home/claude/nhim/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b5.blend")
arm = bpy.data.objects["NhimRig"]; P = rig.Poser(arm)
HL_NAM = Vector(json.load(open(f"{OUT}/place.json"))["HL_NAM"])
sc = bpy.context.scene; sc.render.fps = 30
for b in arm.pose.bones: b.rotation_mode = "QUATERNION"

def posed_min(names, filt=None):
    dg = bpy.context.evaluated_depsgraph_get(); m = 9
    for n in names:
        o = bpy.data.objects[n]; e = o.evaluated_get(dg)
        W = geo.vweights(o) if filt else None
        for v in e.data.vertices:
            if filt and not filt(o, v.index, W): continue
            m = min(m, (o.matrix_world @ v.co).z)
    return m

def place_floor(spec, G, contact=0.0, names=("Body", "Quan", "Ao", "Bot"), filt=None, xy=(0, 0)):
    """đặt hông sao cho điểm thấp nhất (lọc) chạm mặt z=contact, hông ở (x, y)"""
    P.apply(spec, G)
    mz = posed_min(names, filt); h = arm.pose.bones["Hips"].matrix.translation.copy()
    w = Vector((xy[0], xy[1], h.z + contact - mz))
    return G.inverted() @ w

ACT = {}
def clip(name, keys, G, hl, loop=False):
    """keys: [(frame, spec)]"""
    act = bpy.data.actions.new(name); act.use_fake_user = True
    arm.animation_data_create(); arm.animation_data.action = act
    for f, spec in keys:
        P.apply(spec, G, hips_loc=hl(spec) if callable(hl) else hl)
        P.key(f)
    ACT[name] = (keys[-1][0], loop)
    print("clip", name, keys[-1][0], "khung")

N0 = merge(NAM, curl(CURL_R))
def tweak(base, **kw):
    out = {k: dict(v) for k, v in base.items()}
    for k, v in kw.items(): out[k] = {**out.get(k, {}), "d": v}
    return out

# ── nằm
hit_vao = tweak(N0, Chest=(0, -0.16, 1), Spine=(0, -0.15, 1), Shoulder_L=(1, -0.2, 0.07), Head=(-0.28, -0.19, 1))
clip("Nam_Ngu", [(0, N0), (38, hit_vao), (90, N0)], G_NAM, HL_NAM, loop=True)
nong = tweak(N0, Chest=(0, -0.19, 1), Shoulder_L=(1, -0.2, 0.035))
tay_dong = merge(nong, curl({"Index1_R": 10, "Index2_R": 15, "Middle1_R": 25}, 1))
clip("Nam_Thuc", [(0, N0), (30, nong), (60, N0), (75, tay_dong), (90, nong), (120, N0)], G_NAM, HL_NAM, loop=True)
tro = tweak(N0, Chest=(0, -0.05, 1), Spine=(0.05, -0.1, 1), Head=(-0.15, -0.30, 1), Neck=(-0.12, -0.3, 1),
            UpperLeg_L=(0.08, -0.92, -0.38), LowerLeg_L=(0, 0.5, -0.86), UpperLeg_R=(-0.04, -0.7, -0.7))
clip("Nam_TroMinh", [(0, N0), (28, tro), (55, tro), (90, N0)], G_NAM, HL_NAM)
ngoc = tweak(N0, Spine=(0.35, -0.12, 0.93), Chest=(0.62, -0.1, 0.78), Neck=(0.55, -0.15, 0.82), Head=(0.40, -0.25, 0.88),
             UpperArm_R=(-0.78, -0.5, -0.35), LowerArm_R=(0.05, -0.95, 0.3), Hand_R=(0.05, -0.95, 0.3), UpperArm_L=(0.30, -0.35, -0.89),
             Shoulder_R=(-1, -0.2, 0.25))
ngoc = merge(ngoc, curl(CURL_R, 0.1))
clip("Nam_NgocDau", [(0, N0), (24, tweak(ngoc, Chest=(0.45, -0.12, 0.88))), (45, ngoc), (60, ngoc)], G_NAM, HL_NAM)
nghe = tweak(N0, Neck=(-0.06, -0.3, 1), Head=(0.02, -0.34, 1))
clip("Nam_NgheDau", [(0, N0), (36, nghe), (50, nghe)], G_NAM, HL_NAM)
tha = tweak(N0, Chest=(0, -0.28, 1), Spine=(0, -0.22, 1), Head=(-0.33, -0.2, 1), Shoulder_L=(1, -0.2, -0.04))
clip("Nam_ThayKhan", [(0, N0), (25, tha), (60, N0)], G_NAM, HL_NAM)
so = tweak(N0, Spine=(0, -0.3, 1), Chest=(0, -0.33, 1), Neck=(-0.2, -0.42, 1), Head=(-0.25, -0.48, 1),
           LowerArm_R=(0.05, 0.3, 0.95),
           UpperLeg_L=(0.05, -0.92, -0.38), LowerLeg_L=(0, 0.6, -0.8), UpperLeg_R=(-0.03, -0.85, -0.5), LowerLeg_R=(0, 0.62, -0.78))
so = merge(so, curl(CURL_R, 1.5))
clip("Nam_So", [(0, N0), (20, so), (45, so)], G_NAM, HL_NAM)
nua = tweak(N0, Neck=(-0.12, -0.35, 1), Head=(-0.05, -0.42, 1), Chest=(0.08, -0.2, 1))
clip("Nam_NuaTinh", [(0, N0), (30, nua), (80, nua), (120, N0)], G_NAM, HL_NAM)

# ── ngồi ghế ăn (Good): gốc = sàn dưới tâm mặt ghế, mặt quay −Y; mông chạm mặt ghế 0,50
SIT = {"Spine": {"d": (0, 0.03, 1)}, "Chest": {"d": (0, 0.02, 1)}, "Neck": {"d": (0, -0.08, 1)}, "Head": {"d": (0, -0.06, 1)},
       "UpperLeg_L": {"d": (0.10, -1, -0.06)}, "UpperLeg_R": {"d": (-0.10, -1, -0.06)},
       "LowerLeg_L": {"d": (0.02, 0.28, -1)}, "LowerLeg_R": {"d": (-0.02, 0.22, -1)},
       "Foot_L": {"d": (0, -0.55, -0.85)}, "Foot_R": {"d": (0, -0.55, -0.85)},
       # [6/10] bỏ vẫy tay: tay phải buông, cẳng tay đặt dọc đùi phải
       "UpperArm_R": {"d": (-0.10, -0.28, -0.95)}, "LowerArm_R": {"d": (0.10, -0.93, -0.35)}, "Hand_R": {"d": (0.08, -0.9, -0.42)}}
THO = merge(SIT, {"Chest": {"d": (0, 0.0, 1)}, "Neck": {"d": (0, -0.09, 1)}})   # hít vào nhẹ
def buttock(o, i, W):
    w, names = W
    j = names.index("Hips") if "Hips" in names else None
    return j is not None and w[i, j] > 0.5
seat = place_floor(SIT, Matrix(), contact=0.50, names=("Quan",), filt=buttock, xy=(0, 0.02))
clip("Ngoi_NoiChuyen", [(0, SIT), (60, THO), (120, SIT)], Matrix(), seat, loop=True)

# ── Bad: ngồi quỳ trên giường mặt vào đầu giường, đầu gập nghiêng; rồi đứng ở góc, mặt vào tường
GAP = {"Spine": {"d": (0, 0.05, 1)}, "Chest": {"d": (0, 0.08, 1)}, "Neck": {"d": (0.42, 0.06, 0.9)}, "Head": {"d": (0.86, 0.12, 0.5)},
       "UpperArm_R": {"d": (-0.1, 0.05, -1)}, "LowerArm_R": {"d": (-0.04, 0.0, -1)}, "Hand_R": {"d": (-0.04, 0.0, -1)},
       "UpperArm_L": {"d": (0.10, 0.06, -1)}}
QUY = merge(GAP, {"UpperLeg_L": {"d": (0.06, -1, -0.10)}, "UpperLeg_R": {"d": (-0.06, -1, -0.10)},
                  "LowerLeg_L": {"d": (0.02, 1, -0.12)}, "LowerLeg_R": {"d": (-0.02, 1, -0.12)},
                  "Foot_L": {"d": (0, 0.6, 0.8)}, "Foot_R": {"d": (0, 0.6, 0.8)}})
G_QUY = poses.G_yaw(90)
quy = place_floor(QUY, G_QUY, contact=0.004, xy=(0.10, 0.08))
clip("Ngoi_DauGap", [(0, QUY), (30, QUY)], G_QUY, quy)
DUNG = merge(GAP, {"UpperLeg_L": {"d": (0.03, 0.0, -1)}, "UpperLeg_R": {"d": (-0.03, 0.0, -1)},
                   "LowerLeg_L": {"d": (0.0, 0.03, -1)}, "LowerLeg_R": {"d": (0.0, 0.03, -1)}})
dung = place_floor(DUNG, Matrix(), contact=0.0, xy=(0, 0))
clip("Dung_DauGap", [(0, DUNG), (30, DUNG)], Matrix(), dung)

json.dump({k: {"frames": v[0], "loop": v[1]} for k, v in ACT.items()}, open(f"{OUT}/clips.json", "w"), indent=1)
arm.animation_data.action = bpy.data.actions["Nam_Ngu"]
P.reset()
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b6.blend")

# ── xuất FBX
for o in bpy.data.objects: o.select_set(o.type in ("ARMATURE", "MESH"))
bpy.ops.export_scene.fbx(filepath=f"{OUT}/Nhim.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                         axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", add_leaf_bones=False,
                         bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                         bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0,
                         use_armature_deform_only=True, armature_nodetype="NULL", path_mode="STRIP", embed_textures=False,
                         use_mesh_modifiers=False, primary_bone_axis="Y", secondary_bone_axis="X")
print("FBX", os.path.getsize(f"{OUT}/Nhim.fbx") // 1024, "KB")
