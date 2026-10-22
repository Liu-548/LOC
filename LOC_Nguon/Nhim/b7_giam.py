# NHÍM — Bước 7 (22/10, đã duyệt): giảm chi tiết 2 nấc (~10,7k → ~5,25k tam giác), giữ xương/trọng số/UV,
# chiếu lại 6 blendshape từ lưới cũ sang lưới mới, gãy cạnh > 40°, xuất lại Nhim.fbx (11 clip).
import bpy, bmesh, math, os, sys
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from mathutils.geometry import barycentric_transform
D = "/home/claude/nhimlp"; OUT = f"{D}/b7"; os.makedirs(OUT, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=f"{D}/b6.blend")
O = bpy.data.objects
TL = {"Body": 0.46, "Ao": 0.6, "Quan": 0.55, "Toc": 0.47, "Bot": 0.7, "Eye_L": 0.6, "Eye_R": 0.6, "MouthIn": 0.45}
GAY = ("Body", "Ao", "Quan", "Toc", "Bot")

def tri_cache(me, coords):
    me.calc_loop_triangles()
    tris = [tuple(t.vertices) for t in me.loop_triangles]
    return tris

for n, r in TL.items():
    o = O[n]; me = o.data
    keys = None
    if me.shape_keys:
        kb = me.shape_keys.key_blocks
        basis = [v.co.copy() for v in kb[0].data]
        keys = [(k.name, k.slider_min, k.slider_max, [v.co.copy() for v in k.data]) for k in kb[1:]]
        tris = tri_cache(me, basis)
        bvh = BVHTree.FromPolygons(basis, tris)
        o.shape_key_clear()
    bpy.context.view_layer.objects.active = o
    for x in bpy.context.view_layer.objects: x.select_set(x == o)
    d = o.modifiers.new("dec", "DECIMATE"); d.ratio = r; d.use_symmetry = True; d.symmetry_axis = "X"
    bpy.ops.object.modifier_move_to_index(modifier="dec", index=0)
    bpy.ops.object.modifier_apply(modifier="dec")
    if keys:
        o.shape_key_add(name="Basis", from_mix=False)
        new = [v.co.copy() for v in me.vertices]
        hit = []
        for co in new:
            loc, nor, fi, dist = bvh.find_nearest(co)
            hit.append((fi, dist))
        md = max(h[1] for h in hit); assert md < 0.003, f"{n}: đỉnh mới lệch xa mặt cũ {md}"
        for name, smin, smax, kco in keys:
            k = o.shape_key_add(name=name, from_mix=False); k.slider_min, k.slider_max = smin, smax
            for i, co in enumerate(new):
                a, b, c = tris[hit[i][0]]
                k.data[i].co = barycentric_transform(co, basis[a], basis[b], basis[c], kco[a], kco[b], kco[c])
            k.value = 0
    if n in GAY:
        for p in me.polygons: p.use_smooth = True
        me.set_sharp_from_angle(angle=math.radians(40))
        if n == "Body":   # [22/10] quanh miệng giữ mượt: gãy cạnh làm khoé môi thành mảng sáng phẳng
            sh = me.attributes.get("sharp_edge")
            if sh:
                V = me.vertices
                for e in me.edges:
                    if all(abs(V[i].co.x) < 0.026 and 1.020 < V[i].co.z < 1.058 and V[i].co.y < -0.085 for i in e.vertices):
                        sh.data[e.index].value = False
    print(n, sum(len(p.vertices) - 2 for p in me.polygons), "keys" if keys else "")
tot = sum(sum(len(p.vertices) - 2 for p in O[n].data.polygons) for n in TL); print("TONG", tot)
# trọng số: mỗi đỉnh vẫn phải có xương
for n in TL:
    o = O[n]; bad = sum(1 for v in o.data.vertices if not any(g.weight > 1e-4 for g in v.groups))
    assert bad == 0, f"{n}: {bad} đỉnh mất trọng số"
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b7.blend")
for o in bpy.data.objects: o.select_set(o.type in ("ARMATURE", "MESH"))
bpy.ops.export_scene.fbx(filepath=f"{OUT}/Nhim.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                         axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", add_leaf_bones=False,
                         bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                         bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0,
                         use_armature_deform_only=True, armature_nodetype="NULL", path_mode="STRIP", embed_textures=False,
                         use_mesh_modifiers=False, primary_bone_axis="Y", secondary_bone_axis="X")
print("FBX", os.path.getsize(f"{OUT}/Nhim.fbx") // 1024, "KB · actions:", [a.name for a in bpy.data.actions])
