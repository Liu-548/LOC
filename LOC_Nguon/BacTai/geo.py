# Tiện ích hình học: vùng theo xương, tách vỏ (quần áo/bột), viền mép, xoá mặt, decimate, chuyển trọng số/shape key.
import bpy, bmesh, numpy as np
from mathutils import Vector, Matrix
from mathutils.bvhtree import BVHTree
from mathutils.geometry import barycentric_transform

def vweights(obj):
    """mảng (N, B) trọng số + danh sách tên nhóm"""
    names = [g.name for g in obj.vertex_groups]
    W = np.zeros((len(obj.data.vertices), len(names)))
    for v in obj.data.vertices:
        for g in v.groups: W[v.index, g.group] = g.weight
    return W, names

def dominant(obj):
    W, names = vweights(obj)
    return [names[i] for i in W.argmax(1)], W, names

def bone_param(arm, bone, co):
    b = arm.data.bones[bone]; h = arm.matrix_world @ b.head_local; t = arm.matrix_world @ b.tail_local
    d = t - h; return (Vector(co) - h).dot(d) / d.length_squared

def dup(obj, name):
    o = obj.copy(); o.data = obj.data.copy(); o.name = name; o.data.name = name
    bpy.context.scene.collection.objects.link(o); return o

def keep_faces(obj, keep):
    """keep(poly) -> bool. Xoá mặt còn lại + đỉnh lẻ."""
    bm = bmesh.new(); bm.from_mesh(obj.data); bm.faces.ensure_lookup_table()
    dead = [f for f in bm.faces if not keep(f)]
    bmesh.ops.delete(bm, geom=dead, context="FACES")
    loose = [v for v in bm.verts if not v.link_faces]; bmesh.ops.delete(bm, geom=loose, context="VERTS")
    bm.to_mesh(obj.data); bm.free(); obj.data.update()

def inflate(obj, off, smooth=0, rim=0.0, off_fn=None, smooth_fac=0.5):
    """đẩy đỉnh theo pháp tuyến (off hoặc off_fn(co, normal)), làm mượt, thêm viền dày rim (m) quặt vào trong ở mép hở"""
    bm = bmesh.new(); bm.from_mesh(obj.data); bm.normal_update()
    for v in bm.verts:
        d = off_fn(v.co, v.normal) if off_fn else off
        v.co += v.normal * d
    for _ in range(smooth):
        inner = [v for v in bm.verts if not v.is_boundary]
        bmesh.ops.smooth_vert(bm, verts=inner, factor=smooth_fac, use_axis_x=True, use_axis_y=True, use_axis_z=True)
    if rim > 0:
        bm.normal_update()
        bnd = [e for e in bm.edges if e.is_boundary]
        nrm = {v: v.normal.copy() for e in bnd for v in e.verts}
        res = bmesh.ops.extrude_edge_only(bm, edges=bnd)
        newv = [g for g in res["geom"] if isinstance(g, bmesh.types.BMVert)]
        # đỉnh mới trùng đỉnh cũ: tìm pháp tuyến theo cạnh nối
        for v in newv:
            src = next((e.other_vert(v) for e in v.link_edges if e.other_vert(v) in nrm), None)
            if src: v.co = src.co - nrm[src] * rim
    bm.to_mesh(obj.data); bm.free(); obj.data.update()

def decimate(obj, ratio, sym=True):
    m = obj.modifiers.new("Dec", "DECIMATE"); m.ratio = ratio; m.use_symmetry = sym; m.symmetry_axis = "X"
    # đưa lên đầu ngăn xếp (trước Armature)
    with bpy.context.temp_override(object=obj):
        while obj.modifiers[0].name != "Dec": bpy.ops.object.modifier_move_up(modifier="Dec")
        bpy.ops.object.modifier_apply(modifier="Dec")

def tris(obj): return sum(len(p.vertices) - 2 for p in obj.data.polygons)

class Surf:
    """tra điểm gần nhất trên một lưới tham chiếu (toạ độ cục bộ) → (tam giác, toạ độ)"""
    def __init__(self, verts, tri):
        self.V = [Vector(v) for v in verts]; self.T = tri
        self.bvh = BVHTree.FromPolygons(self.V, tri)
    def map(self, p, other):
        """p trên lưới tham chiếu → điểm tương ứng trên lưới 'other' (cùng topo, toạ độ khác)"""
        loc, n, i, d = self.bvh.find_nearest(Vector(p))
        a, b, c = self.T[i]
        return barycentric_transform(loc, self.V[a], self.V[b], self.V[c], Vector(other[a]), Vector(other[b]), Vector(other[c])) + (Vector(p) - loc)
    def bary(self, p):
        loc, n, i, d = self.bvh.find_nearest(Vector(p))
        a, b, c = self.T[i]
        w = barycentric_transform(loc, self.V[a], self.V[b], self.V[c], Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1)))
        return (a, b, c), np.array(w), d

def tri_list(me):
    me.calc_loop_triangles()
    return [tuple(t.vertices) for t in me.loop_triangles]

def transfer_weights(dst, src, bones=None, maxdist=1.0, remap=None):
    """trọng số của dst = nội suy trọng số bề mặt gần nhất trên src"""
    W, names = vweights(src)
    S = Surf([v.co for v in src.data.vertices], tri_list(src.data))
    for g in list(dst.vertex_groups): dst.vertex_groups.remove(g)
    acc = np.zeros((len(dst.data.vertices), len(names)))
    for v in dst.data.vertices:
        (a, b, c), w, d = S.bary(v.co)
        acc[v.index] = W[a] * w[0] + W[b] * w[1] + W[c] * w[2]
    if remap:   # dồn trọng số xương không dùng sang xương gần nhất được phép
        for a_, b_ in remap.items():
            if a_ in names and b_ in names: i, j = names.index(a_), names.index(b_); acc[:, j] += acc[:, i]; acc[:, i] = 0
    if bones is not None:
        keep = [i for i, n in enumerate(names) if n in bones]; mask = np.zeros(len(names)); mask[keep] = 1; acc *= mask
    acc[acc < 0.02] = 0
    z = acc.sum(1) == 0
    if z.any():   # đỉnh mất hết trọng số: lấy xương được phép của đỉnh gần nhất có trọng số
        from mathutils.kdtree import KDTree
        good = np.nonzero(~z)[0]; kd = KDTree(len(good))
        for i in good: kd.insert(dst.data.vertices[int(i)].co, int(i))
        kd.balance()
        for i in np.nonzero(z)[0]: acc[i] = acc[kd.find(dst.data.vertices[int(i)].co)[1]]
    s = acc.sum(1, keepdims=True); s[s == 0] = 1; acc /= s
    for j, n in enumerate(names):
        idx = np.nonzero(acc[:, j])[0]
        if len(idx) == 0: continue
        g = dst.vertex_groups.new(name=n)
        for i in idx: g.add([int(i)], float(acc[i, j]), "REPLACE")

def set_rigid(obj, fn):
    """fn(co) -> tên xương: gán 100% một xương mỗi đỉnh"""
    for g in list(obj.vertex_groups): obj.vertex_groups.remove(g)
    for v in obj.data.vertices:
        n = fn(v.co); g = obj.vertex_groups.get(n) or obj.vertex_groups.new(name=n); g.add([v.index], 1.0, "REPLACE")

def snap_boundary(obj, fn):
    """fn(co: Vector) -> Vector|None: dời đỉnh mép hở về đường cắt gọn"""
    bm = bmesh.new(); bm.from_mesh(obj.data)
    for v in bm.verts:
        if v.is_boundary:
            r = fn(v.co.copy())
            if r is not None: v.co = r
    bm.to_mesh(obj.data); bm.free(); obj.data.update()

def weld(obj, dist=1e-4, center=0.0):
    bm = bmesh.new(); bm.from_mesh(obj.data)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=dist)
    if center:   # đường nối giữa thân trước/sau (x≈0) của lớp bó: hàn rộng hơn
        bmesh.ops.remove_doubles(bm, verts=[v for v in bm.verts if abs(v.co.x) < center], dist=center * 1.5)
    bm.to_mesh(obj.data); bm.free(); obj.data.update()
