# Đồ vật v3.5 (29/10, bpy): SoDo_BaoNo · CocNuoc_Nhim · ViThuoc_HaSot · LoiNhanMe_Giay → mỗi món một .glb (Y-up, gốc ở tâm đáy, mét).
# Chạy: python3 build_dovat.py <thư mục texture> <thư mục xuất>   (texture từ tex.py)
import bpy, bmesh, math, os, sys
from mathutils import Vector, Matrix
TEX, OUT = sys.argv[-2], sys.argv[-1]; os.makedirs(OUT, exist_ok=True)

def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)

def mat(name, rgb=(1, 1, 1), tex=None, alpha=1.0, clip=False, rough=0.7, metal=0.0):
    m = bpy.data.materials.new(name); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*rgb, 1); b.inputs["Roughness"].default_value = rough; b.inputs["Metallic"].default_value = metal
    if tex:
        t = m.node_tree.nodes.new("ShaderNodeTexImage"); t.image = bpy.data.images.load(f"{TEX}/{tex}")
        m.node_tree.links.new(t.outputs["Color"], b.inputs["Base Color"])
        if clip:   # Math Round trước Alpha → exporter glTF xuất alphaMode MASK
            r = m.node_tree.nodes.new("ShaderNodeMath"); r.operation = "ROUND"
            m.node_tree.links.new(t.outputs["Alpha"], r.inputs[0]); m.node_tree.links.new(r.outputs[0], b.inputs["Alpha"])
    if alpha < 1: b.inputs["Alpha"].default_value = alpha
    if alpha < 1 or clip:
        if hasattr(m, "surface_render_method"): m.surface_render_method = "DITHERED" if clip else "BLENDED"
        if hasattr(m, "blend_method"):
            try: m.blend_method = "CLIP" if clip else "BLEND"
            except Exception: pass
    return m

def obj(name, bm, mats, parent=None):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    for m in mats: me.materials.append(m)
    o = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(o)
    if parent: o.parent = parent
    return o

def root(name):
    e = bpy.data.objects.new(name, None); bpy.context.scene.collection.objects.link(e); return e

def box(bm, x0, x1, y0, y1, z0, z1, mi_top=0, mi_side=0, mi_bot=0, uv=None):
    """hộp; uv: lớp UV, mặt trên/dưới trải đủ ảnh 0–1 theo x,y"""
    v = [bm.verts.new((x, y, z)) for z in (z0, z1) for y in (y0, y1) for x in (x0, x1)]
    F = [((0, 2, 3, 1), mi_bot), ((4, 5, 7, 6), mi_top), ((0, 1, 5, 4), mi_side), ((1, 3, 7, 5), mi_side), ((3, 2, 6, 7), mi_side), ((2, 0, 4, 6), mi_side)]
    out = []
    for idx, mi in F:
        f = bm.faces.new([v[i] for i in idx]); f.material_index = mi; out.append(f)
        if uv:
            for l in f.loops:
                co = l.vert.co; l[uv].uv = ((co.x - x0) / (x1 - x0), (co.y - y0) / (y1 - y0))
    return out

def export(name, root_obj):
    for o in bpy.data.objects: o.select_set(False)
    for o in [root_obj] + list(root_obj.children_recursive): o.select_set(True)
    bpy.ops.export_scene.gltf(filepath=f"{OUT}/{name}.glb", export_format="GLB", use_selection=True, export_yup=True,
                              export_apply=True, export_materials="EXPORT", export_image_format="AUTO")
    tri = sum(sum(len(p.vertices) - 2 for p in o.data.polygons) for o in root_obj.children_recursive if o.type == "MESH")
    print(name, "tam giác", tri, "→", os.path.getsize(f"{OUT}/{name}.glb") // 1024, "KB")

# ───────────── 1. sổ đỏ kẹp giấy báo nợ (19 × 27 × 0,7 cm; giấy báo nợ gấp ba, ló 4,5 cm khỏi mép phải)
reset(); R = root("SoDo_BaoNo")
bia = mat("M_SoDo_Bia", tex="T_SoDo_Bia.png", rough=0.75); ruot = mat("M_SoDo_Ruot", (0.86, 0.82, 0.70), rough=0.9)
bm = bmesh.new(); uv = bm.loops.layers.uv.new("UV")
box(bm, -0.095, 0.095, -0.135, 0.135, 0.0, 0.0012, 0, 0, 0, uv)          # bìa dưới
box(bm, -0.093, 0.093, -0.133, 0.133, 0.0012, 0.0058, 1, 1, 1)            # ruột giấy
box(bm, -0.095, 0.095, -0.135, 0.135, 0.0058, 0.007, 0, 0, 0, uv)         # bìa trên
o = obj("SoDo", bm, [bia, ruot], R)
bn = mat("M_BaoNo", tex="T_BaoNo.png", rough=0.95)
bm = bmesh.new(); uv = bm.loops.layers.uv.new("UV")
box(bm, -0.02, 0.14, -0.06, 0.06, 0.0028, 0.0042, 0, 0, 0, uv)           # tờ báo nợ gấp ba (16 × 12 cm), nằm giữa ruột sổ
o2 = obj("BaoNo", bm, [bn], R); o2.rotation_euler = (0, 0, math.radians(7)); o2.location = (0.0, 0.025, 0)
export("SoDo_BaoNo", R)

# ───────────── 2. cốc nước thuỷ tinh trên tủ đầu giường Nhím (Ø 6,4 cm, cao 9 cm, nước 5,5 cm)
reset(); R = root("CocNuoc_Nhim")
tt = mat("M_CocThuyTinh", (0.83, 0.87, 0.85), alpha=0.28, rough=0.08); nuoc = mat("M_NuocCoc", (0.72, 0.8, 0.8), alpha=0.5, rough=0.05)
N = 16
prof = [(0.0, 0.0), (0.027, 0.0), (0.028, 0.004), (0.032, 0.09), (0.0305, 0.09), (0.0265, 0.006), (0.0, 0.006)]   # ngoài → mép → lòng
bm = bmesh.new(); rings = []
for r, z in prof:
    rings.append([bm.verts.new((r * math.cos(2 * math.pi * k / N), r * math.sin(2 * math.pi * k / N), z)) for k in range(N)])
for a, b in zip(rings, rings[1:]):
    for k in range(N):
        try: bm.faces.new((a[k], a[(k + 1) % N], b[(k + 1) % N], b[k]))
        except ValueError: pass
bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
obj("Coc", bm, [tt], R)
bm = bmesh.new()
bmesh.ops.create_cone(bm, cap_ends=True, segments=N, radius1=0.0262, radius2=0.0285, depth=0.049, matrix=Matrix.Translation((0, 0, 0.0065 + 0.0245)))
obj("MatNuoc", bm, [nuoc], R)
export("CocNuoc_Nhim", R)

# ───────────── 3. vỉ thuốc hạ sốt 10 viên (đã bóc 4) + nửa viên cắt sẵn ("sáng tối nửa viên")
reset(); R = root("ViThuoc_HaSot")
nhom = mat("M_ViNhom", tex="T_ViThuoc.png", rough=0.45, metal=0.6); nhua = mat("M_ViNhua", (0.9, 0.92, 0.93), alpha=0.45, rough=0.15)
vien = mat("M_VienThuoc", (0.95, 0.94, 0.9), rough=0.8); rach = mat("M_ViRach", (0.55, 0.56, 0.58), rough=0.6, metal=0.5)
bm = bmesh.new(); uv = bm.loops.layers.uv.new("UV")
box(bm, -0.05, 0.05, -0.0225, 0.0225, 0.0, 0.0008, 0, 0, 0, uv)
obj("Vi_Nen", bm, [nhom], R)
boc = {(0, 0), (1, 0), (0, 1), (2, 0)}   # bốn ô đã bóc
for i in range(5):
    for j in range(2):
        cx, cy = -0.04 + i * 0.02, -0.0105 + j * 0.021
        if (i, j) in boc:
            bm = bmesh.new()   # ô bóc: bọt nhựa bẹp + lỗ rách sẫm trên nền
            bmesh.ops.create_circle(bm, cap_ends=True, segments=10, radius=0.0058, matrix=Matrix.Translation((cx, cy, 0.00085)))
            o = obj(f"Vi_Rach_{i}{j}", bm, [rach], R)
            bm = bmesh.new()
            bmesh.ops.create_uvsphere(bm, u_segments=10, v_segments=4, radius=0.0068, matrix=Matrix.Translation((cx + 0.001, cy, 0.0008)) @ Matrix.Diagonal((1, 1, 0.18, 1)))
            obj(f"Vi_BotBep_{i}{j}", bm, [nhua], R)
            continue
        bm = bmesh.new()
        bmesh.ops.create_uvsphere(bm, u_segments=10, v_segments=5, radius=0.0068, matrix=Matrix.Translation((cx, cy, 0.0008)) @ Matrix.Diagonal((1, 1, 0.55, 1)))
        for v in [v for v in bm.verts if v.co.z < 0.0008]: v.co.z = 0.0008
        obj(f"Vi_Bot_{i}{j}", bm, [nhua], R)
        bm = bmesh.new()
        bmesh.ops.create_cone(bm, cap_ends=True, segments=12, radius1=0.0055, radius2=0.0052, depth=0.0028, matrix=Matrix.Translation((cx, cy, 0.0008 + 0.0014)))
        obj(f"Vien_{i}{j}", bm, [vien], R)
bm = bmesh.new()   # nửa viên nằm cạnh vỉ, mặt cắt quay ra ngoài
bmesh.ops.create_cone(bm, cap_ends=True, segments=12, radius1=0.0055, radius2=0.0052, depth=0.0028, matrix=Matrix.Translation((0, 0, 0.0014)))
bmesh.ops.bisect_plane(bm, geom=bm.verts[:] + bm.edges[:] + bm.faces[:], plane_co=(0, 0, 0), plane_no=(1, 0, 0), clear_inner=True)
bmesh.ops.contextual_create(bm, geom=[e for e in bm.edges if e.is_boundary])
o = obj("Vien_Nua", bm, [vien], R); o.location = (0.059, 0.006, 0); o.rotation_euler = (0, 0, math.radians(200))
export("ViThuoc_HaSot", R)

# ───────────── 4. lời nhắn của mẹ: mặt sau tờ lịch bóc (10 × 15 cm), hơi cong mép, chữ bút bi lên trên
reset(); R = root("LoiNhanMe_Giay")
truoc = mat("M_LoiNhanMe", tex="T_LoiNhanMe.png", clip=True, rough=0.95); sau = mat("M_LoiNhanMe_Sau", tex="T_LoiNhanMe_Sau.png", clip=True, rough=0.95)
def to(lop_mat, z, lat):
    bm = bmesh.new(); uv = bm.loops.layers.uv.new("UV")
    nx, ny = 6, 9; V = {}
    for i in range(nx + 1):
        for j in range(ny + 1):
            u, v = i / nx, j / ny; x, y = -0.05 + 0.1 * u, -0.075 + 0.15 * v
            cong = 0.004 * (2 * u - 1) ** 2 + 0.006 * max(0, v - 0.7) ** 2 / 0.09 * 0.5   # cong nhẹ hai mép dọc + mép trên (mép rách) vểnh
            V[i, j] = bm.verts.new((x, y, z + cong))
    for i in range(nx):
        for j in range(ny):
            q = [V[i, j], V[i + 1, j], V[i + 1, j + 1], V[i, j + 1]]
            if lat: q.reverse()
            f = bm.faces.new(q)
            for l in f.loops:
                co = l.vert.co; uu = (co.x + 0.05) / 0.1; l[uv].uv = (1 - uu if lat else uu, (co.y + 0.075) / 0.15)
    return obj("Giay_Truoc" if not lat else "Giay_Sau", bm, [lop_mat], R)
to(truoc, 0.0004, False); to(sau, 0.0001, True)
export("LoiNhanMe_Giay", R)
