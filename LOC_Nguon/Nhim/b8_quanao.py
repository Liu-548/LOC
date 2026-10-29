# NHÍM — Bước 8 (29/10, kịch bản v3.3): ba bộ quần áo cho ba đêm. Giữ nguyên Ao/Quan của b7 làm bộ Đêm 1 (hoa nhí, tay áo trái xẻ viền),
# thêm lưới mới (cùng xương, cùng trọng số, cùng UV lặp):
#   Ao_D2   áo ba lỗ (bỏ hai ống tay của Ao)        · Quan_D2  quần đùi kẻ ô (giữ dáng quần — luôn nằm dưới chăn)
#   Ao_D3   áo cộc tay hồng nhạt (như Ao)            · Quan_D3  quần lửng · InTho  hình con thỏ in trước ngực (dán theo mặt áo, xương Chest)
# Unity bật/tắt theo đêm (LocNhim.DatBoDo). Xuất lại Nhim.fbx (11 clip giữ nguyên).
# Chạy: python3 b8_quanao.py <b7.blend> <thư mục xuất>
import bpy, bmesh, os, sys
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree
SRC, OUT = sys.argv[-2], sys.argv[-1]; os.makedirs(OUT, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=SRC)
O = bpy.data.objects; rig = O["NhimRig"]

def chep(ten_goc, ten_moi, ten_mat):
    g = O[ten_goc]; o = g.copy(); o.data = g.data.copy(); o.name = o.data.name = ten_moi
    bpy.context.scene.collection.objects.link(o)
    m = bpy.data.materials.new(ten_mat); o.data.materials.clear(); o.data.materials.append(m)
    return o

# ── Đêm 2: ba lỗ = bỏ mọi mặt có cả bốn đỉnh thuộc xương cánh tay (giữ vòng nách ở thân)
a2 = chep("Ao", "Ao_D2", "M_Nhim_Ao_D2")
ten = [g.name for g in a2.vertex_groups]
def troi(v): gs = sorted(v.groups, key=lambda g: -g.weight); return ten[gs[0].group] if gs else ""
bm = bmesh.new(); bm.from_mesh(a2.data)
tay = {v.index for v in a2.data.vertices if troi(v) in ("UpperArm_L", "UpperArm_R")}
# mặt nào chạm đỉnh cánh tay mà tâm đã ra ngoài bề ngang thân (|x| > 9,5 cm) cũng bỏ: nách gọn, không sót mẩu tay áo
xoa = [f for f in bm.faces if all(v.index in tay for v in f.verts) or (any(v.index in tay for v in f.verts) and abs(f.calc_center_median().x) > 0.095)]
mat_xoa = {f.index for f in xoa}
bmesh.ops.delete(bm, geom=xoa, context="FACES"); bm.to_mesh(a2.data); bm.free()
print("Ao_D2: bỏ", len(xoa), "mặt ống tay")

# ── da vai + bắp tay cho áo ba lỗ: Body của b7 bị khoét chỗ áo che (vai, nửa trên bắp tay) → dựng lại từ chính các mặt ống tay vừa bỏ,
#    co về sát trục xương cánh tay (bán kính 2,8 cm — nhỏ hơn tay Body 3,0 cm: chỗ nào Body có da thì Body nằm ngoài, che mảnh này).
#    Gần nách/vai (t < 0) giữ nguyên vị trí vải để khít mép nách áo. UV dồn về một vùng da bắp tay trên T_Nhim_Da → cùng màu da theo đêm.
from mathutils.geometry import intersect_point_line
dv = chep("Ao", "DaVai_D2", "M_Nhim_DaVai_D2")
bm = bmesh.new(); bm.from_mesh(dv.data)
bmesh.ops.delete(bm, geom=[f for f in bm.faces if f.index not in mat_xoa], context="FACES")
xuong = {s: (rig.data.bones["UpperArm_" + s].head_local, rig.data.bones["UpperArm_" + s].tail_local) for s in "LR"}
def tham(co): s = "L" if co.x > 0 else "R"; return intersect_point_line(co, *xuong[s])[1]
bmesh.ops.delete(bm, geom=[f for f in bm.faces if tham(f.calc_center_median()) > 0.65], context="FACES")   # đoạn tay áo xẻ chạy dọc bột (tay trái): nằm trong bột, bỏ
for v in bm.verts:
    s = "L" if v.co.x > 0 else "R"; H, T = xuong[s]
    p, t = intersect_point_line(v.co, H, T); r = v.co - p
    k = min(1.0, max(0.0, (t + 0.05) / 0.2))   # 0 sát nách → 1 từ một phần năm cánh tay trở xuống
    if r.length > 1e-6: v.co = p + r.normalized() * (r.length * (1 - k) + 0.028 * k)
uvl = bm.loops.layers.uv.active
for f in bm.faces:
    for l in f.loops: l[uvl].uv = (0.149 + (l.vert.co.y + 0.05) * 0.06, 0.818 + (l.vert.co.z - 0.9) * 0.06)
bm.to_mesh(dv.data); bm.free()
print("DaVai_D2:", len(dv.data.polygons), "mặt")
chep("Quan", "Quan_D2", "M_Nhim_Quan_D2")
a3 = chep("Ao", "Ao_D3", "M_Nhim_Ao_D3")
chep("Quan", "Quan_D3", "M_Nhim_Quan_D3")

# ── hình thỏ in trước ngực Ao_D3: lưới 8×8 rộng 7,5 cm, chiếu lên mặt trước áo (−Y), nổi 1,5 mm
me = a3.data; bvh = BVHTree.FromPolygons([v.co for v in me.vertices], [p.vertices for p in me.polygons])
S, N, cx, cz = 0.075, 8, 0.0, 0.875
bm = bmesh.new(); uv = bm.loops.layers.uv.new("UV"); V = {}
for i in range(N + 1):
    for j in range(N + 1):
        x, z = cx - S / 2 + S * i / N, cz - S / 2 + S * j / N
        hit = bvh.ray_cast(Vector((x, -0.5, z)), Vector((0, 1, 0)))
        assert hit[0] is not None, f"không chạm áo tại {x:.3f},{z:.3f}"
        V[i, j] = bm.verts.new(hit[0] + hit[1].normalized() * 0.0015 * (1 if hit[1].y < 0 else -1))
for i in range(N):
    for j in range(N):
        f = bm.faces.new((V[i, j], V[i + 1, j], V[i + 1, j + 1], V[i, j + 1]))
        for l in f.loops:
            k = next(key for key, vv in V.items() if vv == l.vert); l[uv].uv = (k[0] / N, k[1] / N)
bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
if bm.faces[0].normal.y > 0: bmesh.ops.reverse_faces(bm, faces=bm.faces)   # mặt hướng ra trước (−Y)
mt = bpy.data.meshes.new("InTho"); bm.to_mesh(mt); bm.free()
tho = bpy.data.objects.new("InTho", mt); bpy.context.scene.collection.objects.link(tho)
tho.parent = rig; mt.materials.append(bpy.data.materials.new("M_Nhim_InTho"))
vg = tho.vertex_groups.new(name="Chest"); vg.add(list(range(len(mt.vertices))), 1.0, "REPLACE")
md = tho.modifiers.new("Armature", "ARMATURE"); md.object = rig
for p in mt.polygons: p.use_smooth = True

bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b8.blend")
for o in bpy.data.objects: o.select_set(o.type in ("ARMATURE", "MESH"))
bpy.ops.export_scene.fbx(filepath=f"{OUT}/Nhim.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                         axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", add_leaf_bones=False,
                         bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                         bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0,
                         use_armature_deform_only=True, armature_nodetype="NULL", path_mode="STRIP", embed_textures=False,
                         use_mesh_modifiers=False, primary_bone_axis="Y", secondary_bone_axis="X")
print("FBX", os.path.getsize(f"{OUT}/Nhim.fbx") // 1024, "KB · mesh:", sorted(o.name for o in bpy.data.objects if o.type == "MESH"))
