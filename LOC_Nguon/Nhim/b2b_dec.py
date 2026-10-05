# Bước 2b: giảm lưới da theo vùng (mặt giữ nhiều nhất), chuyển 6 shape key biểu cảm từ lưới gốc sang lưới đã giảm.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, numpy as np, geo
from mathutils import Vector
OUT = "/home/claude/nhim/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b2a.blend")
body = bpy.data.objects["Body"]; full = bpy.data.objects["Body_Full"]

def region(d, co):
    if d.startswith(("Toes", "Foot")): return "chan"
    if d.startswith(("Thumb", "Index", "Middle", "Ring", "Pinky", "Hand")): return "tay"
    if d == "Head": return "mat" if co.y < -0.03 else "gay"
    return "than"
TARGET = {"chan": 0.13, "tay": 0.25, "gay": 0.22, "than": 0.55, "mat": 0.40}

def dec_region(name, ratio):
    dom, _, _ = geo.dominant(body)
    body.data.calc_loop_triangles()
    reg = [region(dom[v.index], v.co) for v in body.data.vertices]
    n_reg = sum(1 for t in body.data.loop_triangles if reg[t.vertices[0]] == name)
    tot = len(body.data.loop_triangles)
    vg = body.vertex_groups.get("_dec") or body.vertex_groups.new(name="_dec")
    for v in body.data.vertices: vg.add([v.index], 1.0 if reg[v.index] == name else 0.0, "REPLACE")
    m = body.modifiers.new("Dec", "DECIMATE"); m.ratio = (tot - n_reg * (1 - ratio)) / tot
    m.vertex_group = "_dec"; m.vertex_group_factor = 50; m.use_symmetry = True; m.symmetry_axis = "X"
    with bpy.context.temp_override(object=body, active_object=body):
        while body.modifiers[0].name != "Dec": bpy.ops.object.modifier_move_up(modifier="Dec")
        bpy.ops.object.modifier_apply(modifier="Dec")
    print(f"{name}: {n_reg} → mục tiêu {int(n_reg * ratio)} · tổng {tot} → {geo.tris(body)}")
for k, r in TARGET.items(): dec_region(k, r)
body.vertex_groups.remove(body.vertex_groups["_dec"])

# răng (bỏ lưỡi), mắt giữ nguyên
mi = bpy.data.objects["MouthIn"]
bm = bmesh.new(); bm.from_mesh(mi.data); bm.normal_update()
# lưỡi = phần thấp nhất phía sau; giữ hai dải răng (đỉnh phía trước)
# ponytail: lọc theo y — răng nằm trước (y nhỏ), lưỡi lùi sau
cut = np.percentile([v.co.y for v in bm.verts], 45)
bmesh.ops.delete(bm, geom=[f for f in bm.faces if f.calc_center_median().y > cut], context="FACES")
bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context="VERTS")
bm.to_mesh(mi.data); bm.free()

# ── shape key: lưới gốc (Body_Full, cùng thứ tự đỉnh base_idx) + độ lệch biểu cảm
D = np.load(f"{OUT}/b1_shapes.npz")
base_idx = list(full["base_idx"])
FV = np.array([v.co[:] for v in full.data.vertices])
S = geo.Surf(FV, geo.tri_list(full.data))
body.shape_key_add(name="Basis")
binfo = [S.bary(v.co) for v in body.data.vertices]
for k in D.files:
    d = D[k][base_idx]                     # độ lệch theo đỉnh của Body_Full
    sk = body.shape_key_add(name=k)
    for v, ((a, b, c), w, dist) in zip(body.data.vertices, binfo):
        sk.data[v.index].co = v.co + Vector(d[a] * w[0] + d[b] * w[1] + d[c] * w[2])
    print("shape", k, "max", np.abs(d).max())
# mắt khép: lông mi/mí đã nằm trên da; nhãn cầu không đổi
for kb in body.data.shape_keys.key_blocks: kb.value = 0
bpy.data.objects.remove(full)
print("da", geo.tris(body), "· răng", geo.tris(mi))
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b2b.blend")
