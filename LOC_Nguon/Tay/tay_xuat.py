# Xuất tay ra Unity (29/10, bản vẽ ThietKe_BanTay_v1 đã duyệt: áo Khôi xắn · tay thầy giữ nhẫn + móng út dài).
# Mỗi lần chạy dựng MỘT tay rồi xuất một .fbx; mỗi tư thế co ngón (POSES trong tay.py) thành một action một khung
# → Unity (LocHouseBuilder_Tay) lấy mẫu từng tư thế, LocTay chuyển giữa các tư thế bằng code.
# Chạy: python3 tay_xuat.py Khoi_R|Khoi_L|Thay_R <thư mục xuất>
import sys, os, bpy
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tay
loai, OUT = sys.argv[-2], sys.argv[-1]; os.makedirs(OUT, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
S = tay.KHOI if loai.startswith("Khoi") else tay.THAY
arm, objs = tay.build(S, OUT, mirror=loai.endswith("_L"))
bpy.context.view_layer.objects.active = arm
for ten, kw in tay.POSES.items():
    act = bpy.data.actions.new(ten); arm.animation_data_create(); arm.animation_data.action = act
    tay.pose(arm, **kw)
    for b in arm.pose.bones:
        for f in (0, 1): b.keyframe_insert("rotation_euler", frame=f)
    act.use_fake_user = True
tay.pose(arm, **tay.POSES["Tha"]); arm.animation_data.action = None
for o in bpy.data.objects: o.select_set(o == arm or o in objs)
ten_file = "Tay" + loai
bpy.ops.export_scene.fbx(filepath=f"{OUT}/{ten_file}.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                         axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", add_leaf_bones=False,
                         bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                         bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0,
                         use_armature_deform_only=True, armature_nodetype="NULL", path_mode="STRIP", embed_textures=False,
                         use_mesh_modifiers=False, primary_bone_axis="Y", secondary_bone_axis="X")
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/{ten_file}.blend")
tri = sum(sum(len(p.vertices) - 2 for p in o.data.polygons) for o in objs)
print(ten_file, "tam giác", tri, "· tư thế", list(tay.POSES), "·", os.path.getsize(f"{OUT}/{ten_file}.fbx") // 1024, "KB")
