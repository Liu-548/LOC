# LỘC — nâng cấp asset (29/10): thư viện dựng mô hình bán thực tế bằng bpy.
# Quy ước: đơn vị mét, Blender Z lên, MẶT TRƯỚC = −Y (khớp file .glb gốc sau khi glTF importer đổi trục).
# Vật liệu đặt tên "<lớp>__<mô tả>" — Unity (LocChiTiet) đọc tiền tố lớp để gắn bộ texture PBR dùng chung (albedo chi tiết + normal + mask).
# UV0 = toạ độ mét chiếu hộp (1 đơn vị UV = 1 m) để vân gỗ/vải đúng tỉ lệ thật; Unity nhân _BaseMap_ST theo lớp.
import bpy, bmesh, math, os, json, random
from mathutils import Vector, Matrix, Euler

LOP = {"go", "goson", "son", "nhua", "vai", "carton", "dan", "chieu", "xm", "thung", "tulanh", "kl", "inox", "dong", "gom",
       "giay", "da", "caosu", "kinh", "anh", "den", "sat", "gach", "nen", "thuc"}

def hexrgb(h):
    h = h.lstrip('#'); return tuple(int(h[i:i + 2], 16) / 255 for i in (0, 2, 4))

def lin(c):  # sRGB → linear (Principled nhận linear)
    return tuple(x / 12.92 if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4 for x in c)

_mats = {}
def mat(lop, hexc, r=0.6, m=0.0, ten=None, a=1.0, emis=None, img=None, hai_mat=False, cat_alpha=False):
    """lop: lớp texture Unity · hexc: màu nền · r: roughness · m: metallic · a<1: trong suốt · emis: hex phát sáng · img: ảnh (đường dẫn) cho món có hình in."""
    assert lop in LOP, lop
    key = ten[1:] if (ten and ten.startswith("=")) else f"{lop}__{ten or hexc.strip('#')}"
    if key in _mats: return _mats[key]
    M = bpy.data.materials.new(key); M.use_nodes = True
    nt = M.node_tree; b = nt.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*lin(hexrgb(hexc)), 1)
    b.inputs["Roughness"].default_value = r; b.inputs["Metallic"].default_value = m
    M.diffuse_color = (*lin(hexrgb(hexc)), a); M.roughness = r; M.metallic = m
    if a < 1:
        b.inputs["Alpha"].default_value = a
        try: M.surface_render_method = 'BLENDED'
        except Exception: pass
        M.blend_method = 'BLEND' if hasattr(M, 'blend_method') else None
    if emis:
        b.inputs["Emission Color"].default_value = (*lin(hexrgb(emis)), 1); b.inputs["Emission Strength"].default_value = 1.0
    if img:
        t = nt.nodes.new("ShaderNodeTexImage"); t.image = bpy.data.images.load(img, check_existing=True)
        t.interpolation = 'Linear'
        nt.links.new(t.outputs["Color"], b.inputs["Base Color"])
        if cat_alpha:   # khoét lỗ theo alpha ảnh (ren, lưới) → glTF alphaMode MASK
            mth = nt.nodes.new("ShaderNodeMath"); mth.operation = 'GREATER_THAN'; mth.inputs[1].default_value = 0.5
            nt.links.new(t.outputs["Alpha"], mth.inputs[0]); nt.links.new(mth.outputs[0], b.inputs["Alpha"])
    M.use_backface_culling = not hai_mat
    _mats[key] = M
    return M

# ───────────────────────── tạo khối
_parts = []          # mọi object con của món đang dựng (sẽ gộp theo node)
def _link(o):
    bpy.context.scene.collection.objects.link(o); _parts.append(o); return o

def _bm_obj(bm, name, M):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me)
    if M: me.materials.append(M)
    return _link(o)

def _xf(o, loc, rot):
    o.location = Vector(loc)
    if rot: o.rotation_euler = Euler([math.radians(v) for v in rot])
    return o

def bevel(o, w=0.004, seg=2, ang=40, clamp=True):
    if w <= 0: return o
    md = o.modifiers.new("bev", 'BEVEL'); md.width = w; md.segments = seg; md.limit_method = 'ANGLE'
    md.angle_limit = math.radians(ang); md.use_clamp_overlap = clamp; md.harden_normals = False
    return o

def box(size, loc=(0, 0, 0), M=None, b=0.004, seg=2, rot=None, name="hop"):
    """Hộp size=(x,y,z), loc = TÂM."""
    bm = bmesh.new(); bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
    o = _bm_obj(bm, name, M); _xf(o, loc, rot)
    return bevel(o, min(b, min(size) * 0.45), seg)

def boxmin(size, mn, M=None, b=0.004, seg=2, name="hop"):
    """Hộp đặt theo góc nhỏ nhất mn."""
    return box(size, [mn[i] + size[i] / 2 for i in range(3)], M, b, seg, name=name)

def cyl(r, h, loc=(0, 0, 0), M=None, n=24, b=0.002, rot=None, r2=None, cap=True, name="tru"):
    """Trụ trục Z, loc = tâm ĐÁY. r2: bán kính đỉnh (côn)."""
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=cap, cap_tris=False, segments=n, radius1=r, radius2=(r if r2 is None else r2), depth=h)
    bmesh.ops.translate(bm, vec=Vector((0, 0, h / 2)), verts=bm.verts)
    o = _bm_obj(bm, name, M); _xf(o, loc, rot)
    if b > 0: bevel(o, min(b, h * 0.4, r * 0.4), 2, 30)
    smooth(o)
    return o

def lathe(prof, loc=(0, 0, 0), M=None, n=32, rot=None, name="tien", close_top=False, close_bot=False):
    """Tiện tròn: prof = [(r, z), ...] từ dưới lên. Đầu/cuối r=0 tự khép."""
    bm = bmesh.new()
    vs = [bm.verts.new((max(r, 0.0), 0, z)) for r, z in prof]
    es = [bm.edges.new((vs[i], vs[i + 1])) for i in range(len(vs) - 1)]
    bmesh.ops.spin(bm, geom=vs + es, cent=(0, 0, 0), axis=(0, 0, 1), steps=n, angle=math.radians(360), use_duplicate=False)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    if close_bot or close_top:
        bnd = [e for e in bm.edges if e.is_boundary]
        if bnd: bmesh.ops.holes_fill(bm, edges=bnd, sides=0)
    o = _bm_obj(bm, name, M); _xf(o, loc, rot); smooth(o, 50)
    return o

def sphere(r, loc=(0, 0, 0), M=None, scale=(1, 1, 1), n=24, rot=None, name="cau"):
    bm = bmesh.new(); bmesh.ops.create_uvsphere(bm, u_segments=n, v_segments=max(8, n // 2), radius=r)
    bmesh.ops.scale(bm, vec=Vector(scale), verts=bm.verts)
    o = _bm_obj(bm, name, M); _xf(o, loc, rot); smooth(o, 80); return o

def tube(pts, r, M=None, n=10, name="ong", cap=True):
    """Ống tròn chạy theo polyline pts (nắn mềm bằng NURBS)."""
    cu = bpy.data.curves.new(name, 'CURVE'); cu.dimensions = '3D'; cu.bevel_depth = r; cu.bevel_resolution = max(1, n // 4)
    cu.use_fill_caps = cap
    sp = cu.splines.new('POLY'); sp.points.add(len(pts) - 1)
    for i, p in enumerate(pts): sp.points[i].co = (*p, 1)
    oc = bpy.data.objects.new(name, cu); bpy.context.scene.collection.objects.link(oc)
    dg = bpy.context.evaluated_depsgraph_get(); me = bpy.data.meshes.new_from_object(oc.evaluated_get(dg))
    bpy.data.objects.remove(oc); o = bpy.data.objects.new(name, me)
    if M: me.materials.clear(); me.materials.append(M)
    _link(o); smooth(o, 60); return o

def plane(w, h, loc=(0, 0, 0), M=None, rot=None, name="mat", uv_full=True):
    """Mặt phẳng XZ (đứng, nhìn từ −Y) w×h, loc = tâm. uv_full: UV 0..1 phủ kín (cho ảnh/nhãn)."""
    bm = bmesh.new()
    v = [bm.verts.new(p) for p in [(-w / 2, 0, -h / 2), (w / 2, 0, -h / 2), (w / 2, 0, h / 2), (-w / 2, 0, h / 2)]]
    f = bm.faces.new(v); uvl = bm.loops.layers.uv.new("UVMap")
    for l, uv in zip(f.loops, [(0, 0), (1, 0), (1, 1), (0, 1)]): l[uvl].uv = uv
    bm.normal_update()
    if f.normal.y > 0: f.normal_flip()
    o = _bm_obj(bm, name, M); _xf(o, loc, rot); o["uv_giu"] = 1; return o

def grid(w, d, nx, ny, loc=(0, 0, 0), M=None, name="luoi"):
    """Lưới nằm ngang (XY) để vò/khăn rủ; trả object để chỉnh đỉnh bằng hàm."""
    bm = bmesh.new(); bmesh.ops.create_grid(bm, x_segments=nx, y_segments=ny, size=0.5)
    bmesh.ops.scale(bm, vec=Vector((w, d, 1)), verts=bm.verts)
    o = _bm_obj(bm, name, M); _xf(o, loc, None); return o

def deform(o, fn):
    """fn(Vector cục bộ) → Vector mới."""
    for v in o.data.vertices: v.co = fn(v.co.copy())
    o.data.update(); return o

def solid(o, t, off=-1):
    md = o.modifiers.new("day", 'SOLIDIFY'); md.thickness = t; md.offset = off; return o

def subsurf(o, lv=1):
    md = o.modifiers.new("sub", 'SUBSURF'); md.levels = lv; md.render_levels = lv; return o

def smooth(o, ang=35):
    for p in o.data.polygons: p.use_smooth = True
    try:
        o.data.set_sharp_from_angle(angle=math.radians(ang))
    except Exception:
        pass
    return o

def cushion(size, loc, M, r=None, name="dem", phong=0.012):
    """Đệm mút: hộp bo tròn mạnh + phồng nhẹ ở giữa mặt trên/dưới."""
    r = r if r is not None else min(size) * 0.42
    o = box(size, loc, M, b=r, seg=3, name=name)
    o.modifiers["bev"].limit_method = 'NONE'
    bm = bmesh.new(); bm.from_mesh(o.data)
    bmesh.ops.subdivide_edges(bm, edges=[e for e in bm.edges if e.calc_length() > 0.04], cuts=2, use_grid_fill=True)
    sx, sy, sz = size
    for v in bm.verts:
        u = 1 - min(1, (2 * v.co.x / sx) ** 2); w = 1 - min(1, (2 * v.co.y / sy) ** 2)
        v.co.z += math.copysign(phong * u * w, v.co.z)
    bm.to_mesh(o.data); bm.free(); smooth(o, 60); return o

def mirror_x(objs):
    out = []
    for o in objs:
        c = o.copy(); c.data = o.data.copy(); c.location.x = -o.location.x; c.rotation_euler.z = -o.rotation_euler.z
        c.scale.x = -o.scale.x; _link(c); out.append(c)
    return out

def dup(o, loc=None, rot=None):
    c = o.copy(); c.data = o.data; _link(c)
    if loc is not None: c.location = Vector(loc)
    if rot is not None: c.rotation_euler = Euler([math.radians(v) for v in rot])
    return c

def array(o, n, step):
    out = [o]
    for i in range(1, n): out.append(dup(o, o.location + Vector(step) * i, None)); out[-1].rotation_euler = o.rotation_euler
    return out

# ───────────────────────── UV mét chiếu hộp + gộp node
def uv_met(o, xoay=False):
    """UV = toạ độ (mét) theo mặt chiếu trội; trục V (thớ gỗ, sợi xước) chạy theo chiều DÀI NHẤT của phần đó."""
    me = o.data
    if o.get("uv_giu") or o.get("uv_tron"): return
    if not me.uv_layers: me.uv_layers.new(name="UVMap")
    uvl = me.uv_layers.active.data
    mw = o.matrix_world
    ext = [0, 0, 0]
    if me.vertices:
        ws = [mw @ v.co for v in me.vertices]
        ext = [max(w[i] for w in ws) - min(w[i] for w in ws) for i in range(3)]
    for p in me.polygons:
        n = (mw.to_3x3() @ p.normal); a = (abs(n.x), abs(n.y), abs(n.z))
        if a[2] >= a[0] and a[2] >= a[1]: ax = (0, 1)
        elif a[0] >= a[1]: ax = (1, 2)
        else: ax = (0, 2)
        if ext[ax[0]] > ext[ax[1]]: ax = (ax[1], ax[0])     # V = trục dài hơn
        if xoay: ax = (ax[1], ax[0])
        k = o.get("uvs", 1.0)
        for li in p.loop_indices:
            co = mw @ me.vertices[me.loops[li].vertex_index].co
            uvl[li].uv = (co[ax[0]] * k, co[ax[1]] * k)

def gop(name, objs=None):
    """Áp modifier, gán UV mét, gộp mọi phần thành 1 object tên name (giữ nhiều vật liệu)."""
    objs = objs if objs is not None else list(_parts)
    dg = bpy.context.evaluated_depsgraph_get()
    bms = bmesh.new(); mats = []
    for o in objs:
        oe = o.evaluated_get(dg); me = bpy.data.meshes.new_from_object(oe)
        tmp = bpy.data.objects.new("tmp", me); tmp.matrix_world = o.matrix_world.copy()
        for kk in ("uv_giu", "uvs", "xoay", "uv_tron"):
            if kk in o: tmp[kk] = o[kk]
        bpy.context.scene.collection.objects.link(tmp)
        uv_met(tmp, tmp.get("xoay", False))
        me.transform(o.matrix_world); tmp.matrix_world = Matrix()
        # map vật liệu
        remap = {}
        for i, m in enumerate(me.materials):
            if m not in mats: mats.append(m)
            remap[i] = mats.index(m)
        for p in me.polygons: p.material_index = remap.get(p.material_index, 0)
        if not me.uv_layers: me.uv_layers.new(name="UVMap")
        me.uv_layers[0].name = "UVMap"
        bms.from_mesh(me)
        bpy.data.objects.remove(tmp); bpy.data.meshes.remove(me)
    for o in objs:
        if o.name in bpy.data.objects: bpy.data.objects.remove(o)
    _parts.clear()
    me = bpy.data.meshes.new(name); bms.to_mesh(me); bms.free()
    for m in mats: me.materials.append(m)
    o = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(o)
    return o

def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True); _mats.clear(); _parts.clear()

def bbox(objs):
    pts = [o.matrix_world @ Vector(c) for o in objs if o.type == 'MESH' for c in o.bound_box]
    mn = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
    mx = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
    return mn, mx

def nap_goc(path):
    """Nạp .glb gốc vào collection riêng (để lấy bbox, ảnh, vật liệu có texture); trả dict tên→object."""
    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=path)
    new = [o for o in bpy.data.objects if o not in before]
    return {o.name: o for o in new}

def anh_goc(goc, idx=0):
    """Lấy ảnh texture trong glb gốc → lưu png tạm, trả đường dẫn."""
    imgs = [i for i in bpy.data.images if i.size[0] > 0]
    if not imgs: return None
    im = imgs[min(idx, len(imgs) - 1)]
    p = f"/tmp/locimg_{abs(hash(im.name))}.png"; im.filepath_raw = p; im.file_format = 'PNG'; im.save()
    return p

def xoa(objs):
    for o in list(objs):
        if o.name in bpy.data.objects: bpy.data.objects.remove(o)

def khop(o, mn, mx, truc="xyz", giu_ti_le=False):
    """Co giãn + dịch object o cho bbox khớp đúng [mn, mx] (giữ vị trí/kích thước gốc để Unity đặt không lệch)."""
    a, b = bbox([o])
    s = [1, 1, 1]
    for i, k in enumerate("xyz"):
        if k in truc and (b - a)[i] > 1e-5: s[i] = (mx - mn)[i] / (b - a)[i]
    if giu_ti_le:
        f = min(s); s = [f, f, f]
    me = o.data
    me.transform(Matrix.Translation(-a)); me.transform(Matrix.Diagonal((*s, 1)))
    me.transform(Matrix.Translation(Vector([mn[i] if "xyz"[i] in truc else a[i] for i in range(3)])))
    return o

def xuat(path, objs):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.ops.export_scene.gltf(filepath=path, export_format='GLB', use_selection=True, export_apply=True, export_tangents=True,
                              export_yup=True, export_materials='EXPORT', export_image_format='AUTO', export_extras=False)

def tris(objs):
    return sum(sum(len(p.vertices) - 2 for p in o.data.polygons) for o in objs if o.type == 'MESH')

def render(path, objs, size=640, views=((-120, 18), (-60, 30), (60, 25))):
    """Ảnh xem trước workbench: nhiều góc ghép ngang."""
    sc = bpy.context.scene; sc.render.engine = 'BLENDER_WORKBENCH'
    sh = sc.display.shading; sh.light = 'STUDIO'; sh.color_type = 'MATERIAL'; sh.show_cavity = True; sh.show_shadows = True
    sc.render.resolution_x = sc.render.resolution_y = size
    mn, mx = bbox(objs); c = (mn + mx) / 2; r = max((mx - mn).length, 0.05)
    cam = bpy.data.objects.new('cam', bpy.data.cameras.new('cam')); sc.collection.objects.link(cam); sc.camera = cam
    cam.data.type = 'ORTHO'; cam.data.ortho_scale = r * 1.05
    files = []
    for i, (az, el) in enumerate(views):
        a = math.radians(az); e = math.radians(el)
        d = Vector((math.cos(a) * math.cos(e), math.sin(a) * math.cos(e), math.sin(e)))
        cam.location = c + d * r * 3; cam.rotation_euler = (-d).to_track_quat('-Z', 'Y').to_euler()
        f = f"{path}_{i}.png"; sc.render.filepath = f; bpy.ops.render.render(write_still=True); files.append(f)
    bpy.data.objects.remove(cam)
    return files

def mesh_bm(bm, name, M):
    """bmesh tự dựng → object (đã vào danh sách phần)."""
    return _bm_obj(bm, name, M)

def phan():
    return _parts

# ───────────────────────── NÂNG CẤP TỰ ĐỘNG (giữ nguyên bố cục món gốc)
import unicodedata, re as _re
def _khong_dau(s):
    s = unicodedata.normalize('NFD', s); s = ''.join(ch for ch in s if unicodedata.category(ch) != 'Mn')
    return s.replace('đ', 'd').replace('Đ', 'D').lower()

_LOP_TU = [
    (r"kinh|glass|thuy_?tinh|mat_?nuoc|nuoc_chen|water", "kinh"),
    (r"emiss|ngon_?lua|lua|flame|bong_?den|neon|led", "den"),
    (r"anh|atlas|photo|tex|nhan|label|chu|bang_?chu|banner|lich|poster|tranh", "anh"),
    (r"go|wood|lacquer|tre|bamboo|cay|mia|ke_|ban_|ghe|tu_|chan_|giuong|sap|dua|chopstick", "go"),
    (r"vai|cloth|khan|robe|tent|bat_?che|rem|man|dem|goi|chan_?men|ao|quan|non|hat|tang|cover|bong", "vai"),
    (r"su|porcelain|men|gom|ceramic|chen|bat|dia|plate|bowl|cup|binh_?hoa|lo_?hoa", "gom"),
    (r"nhua|plastic|nilon|chair|table|xo|thung|ro|ra|chau|coc|hop", "nhua"),
    (r"inox|steel|stainless", "inox"),
    (r"dong|brass|gold|vang_?kim|bronze|copper", "dong"),
    (r"sat|iron|pipe|ong_?thep|thep|rust|ri", "sat"),
    (r"kl|metal|nhom|alu|kim_?loai|chrome|bac", "kl"),
    (r"son|paint|enamel|tolE|ton", "son"),
    (r"giay|paper|vang_?ma|money|envelope|so_|book|sach|bao", "giay"),
    (r"carton|bia|box|card", "carton"),
    (r"chicken|pig|rice|xoi|gio|food|qua|fruit|com|trung|egg|chuoi|cam|tao|buoi|banh|keo|khoai|thit|rau|soup|thuc", "thuc"),
    (r"xi_?mang|cement|concrete|gach|brick|vua|tuong|tro|ash", "xm"),
    (r"cao_?su|rubber|lop|tyre", "caosu"),
    (r"da_|leather", "da"),
    (r"nen|wax|sap_?nen|candle", "nen"),
    (r"dan|wicker|may|rattan|cot", "dan"),
    (r"chieu|mat_?coi", "chieu"),
]

def doan_lop(ten, rgb):
    t = _khong_dau(ten)
    for re_, lop in _LOP_TU:
        if _re.search(re_, t): return lop
    r, g, b = rgb; mx_ = max(rgb); mn_ = min(rgb); v = mx_; s = 0 if mx_ == 0 else (mx_ - mn_) / mx_
    h = 0
    if mx_ != mn_:
        if mx_ == r: h = ((g - b) / (mx_ - mn_)) % 6
        elif mx_ == g: h = (b - r) / (mx_ - mn_) + 2
        else: h = (r - g) / (mx_ - mn_) + 4
        h /= 6
    if s > 0.25 and 0.02 < h < 0.13 and v < 0.7: return "go"
    if s < 0.15 and v > 0.55: return "son"
    return "nhua"

def _rgb_mat(m):
    if m and m.use_nodes:
        b = next((n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED'), None)
        if b:
            c = b.inputs["Base Color"].default_value
            tex = any(l.to_socket == b.inputs["Base Color"] for l in m.node_tree.links)
            a = b.inputs["Alpha"].default_value; alink = b.inputs["Alpha"].is_linked
            em = b.inputs["Emission Strength"].default_value > 0.01 and sum(b.inputs["Emission Color"].default_value[:3]) > 0.05
            return (c[0], c[1], c[2]), b.inputs["Roughness"].default_value, b.inputs["Metallic"].default_value, tex, (a < 0.98 or alink), em
    c = m.diffuse_color if m else (0.8, 0.8, 0.8, 1)
    return (c[0], c[1], c[2]), 0.6, 0.0, False, False, False

def _srgb(x): return 12.92 * x if x <= 0.0031308 else 1.055 * x ** (1 / 2.4) - 0.055

# luật phân loại theo TÊN MÓN, chép từ LocChiTiet.Rules (khớp đầu tiên thắng; None = bỏ qua, giữ vật liệu gốc; "mau" = theo màu)
_LUAT_MON = [
    (r"^(ChoiLau_Gop|Khan_Ru|Decal|Kinh|Guong$|TV_|Den|Bong|Ranh|Nhan|PH_|CuaChinh|Tranh|Tham_|Khung|KhungAnh|DongHoTreo_Vuong|MacAoDung|QuatCayDung)", None),
    (r"^TuLanh", "tulanh"), (r"Carton|ThungCarton|BocVai|GocKho|DongDoCung|BaoTai|BaoXi|Bui", "carton"),
    (r"XiMang|BeNuoc|ChauTrauBa|BonHoa|GachMau|ChauCay_|ChauCayKieng|ChauLuoiHo", "xm"),
    (r"ChieuMen|ChieuCoi|Chieu|NenSanPhoi|SapGo", "chieu"), (r"^Ro|RoRa|RauQua|Gio|Non|BoAm|ThamChui|ChoiLau|ChoiQuet|CayChoi", "dan"),
    (r"ThungNuoc|ThungRac|ThungGao|ThungSon|^Xo|Thau|ChauNhua|^Thung", "thung"), (r"Chong|GiuongXep|Tre$", "go"), (r"^TuAo|^TuQuanAo", "mau"),
    (r"Chan|Men|Goi|Rem|AoMua|AoTang|Ao_|^Ao|Khan|Tui|Balo|Gau|ManTuyn|DongPhuc|Dem|Salon_Dem|Walkman|DoTuDauGiuong_Me", "vai"),
    (r"TuiNilon|Nilon|Thung|Xo|Thau|Chau|Coc|DoChoi|Bup|Choi|Dep|Hop|GheDau_Nhua|GheNhua|TuNhua|SM_Plastic|BaoBot|Binh|ThungRac|BatDia_Trong|LongBan|Sach|Cap|DayPhoi|KepPhoi|Loa|Gat|Khay|Dia|Lon|Rao|Nut|Lo", "nhua"),
    (r"TuLanh|BeRua|BeGiat|XiBet|NoiCom|Bep|Noi|AmNhom|Am|KetSat|TuHoSo|BangDien|CauDao|Quat|CotPhoi|LanCan|GianPhoi|XeRua|XeDap|OngNuoc|ThanhSat|ThangNhom|CuonLuoi|BinhGas|Lavabo|Voi|MayBom|Ong|Thanh|Cuon|Phich|BinhThuy|TanNhiet|Antena|Anten|HopCong|DongHoNuoc|Chuong|HopThu|MocKhoa|Moc|Khoa|Song|Cong", "son"),
    (r"Ban|Ghe|Tu|Giuong|Ke|Go|Salon|Khung|Chan_|Buffet|Ruong|MacAo|DongHo|Tranh|Anh|Lich|Bang|Sap|Cay|Cua|Kho|ChoiLau|Choi", "go"),
]
_TEN_MON = [None]

def lop_theo_mon(ten):
    for re_, lop in _LUAT_MON:
        if _re.search(re_, ten): return lop, True
    return "mau", False

def mat_tu_goc(m, ten_giu=None):
    """Vật liệu gốc → vật liệu lớp D2 (giữ màu/độ nhám); giữ nguyên nếu có texture, trong suốt, phát sáng hoặc tên bị code tham chiếu."""
    if m is None: return mat("nhua", "#B0B0B0", ten="khong_ten")
    if ten_giu and any(m.name.startswith(t) for t in ten_giu): return m
    rgb, r, mt, tex, alpha, em = _rgb_mat(m)
    if tex or alpha or em: return m
    srgb = tuple(min(1, max(0, _srgb(x))) for x in rgb)
    lop = None
    t = _khong_dau(_re.sub(r"^(M_|Material_?)", "", m.name))
    for re_, l in _LOP_TU:
        if t and _re.search(re_, t): lop = l; break
    if lop is None and _TEN_MON[0]:
        lm, ok = lop_theo_mon(_TEN_MON[0])
        if ok and lm is None: return m          # LocChiTiet vốn bỏ qua món này
        if lm not in (None, "mau"): lop = lm
    if lop is None: lop = doan_lop("", srgb)
    if lop in ("kinh", "anh", "den"): return m
    if lop in ("dong", "kl", "inox", "sat") and mt < 0.5: mt = 0.9 if lop != "sat" else 0.6
    hx = "#%02X%02X%02X" % tuple(int(x * 255) for x in srgb)
    ten = _re.sub(r'[^A-Za-z0-9_]', '_', _khong_dau(m.name))[:40]
    return mat(lop, hx, r=max(0.08, min(1, r)), m=mt, ten=f"{ten}_{hx[1:]}")

def lam_min(o, kieu=None, ti_le=1.0):
    """o: object mesh (đã ở toạ độ thế giới). kieu 'cung' = vát cạnh · 'mem' = subdivision giữ cạnh sắc · None = tự đoán."""
    me = o.data
    bm = bmesh.new(); bm.from_mesh(me)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.0004)
    try: bmesh.ops.join_triangles(bm, faces=bm.faces, angle_face_threshold=math.radians(8), angle_shape_threshold=math.radians(25))
    except Exception: pass
    bm.normal_update()
    sharp = 0; n_e = 0; facet = 0
    for e in bm.edges:
        if len(e.link_faces) == 2:
            n_e += 1
            a = e.calc_face_angle(0)
            if a > math.radians(55): sharp += 1
            elif a > math.radians(14): facet += 1
    bm.to_mesh(me); bm.free()
    if not me.polygons: return o
    mn, mx = bbox([o]); dim = min(v for v in (mx - mn) if v > 1e-4) if any(v > 1e-4 for v in (mx - mn)) else 0.01
    if kieu is None:
        kieu = "mem" if (n_e > 0 and len(me.polygons) >= 10 and facet / max(1, n_e) > 0.18) else "cung"
    big = max(mx - mn)
    if kieu == "cung" or len(me.polygons) > 4000 or big < 0.03:
        w = min(0.006, max(0.0008, dim * 0.08)) * ti_le
        bevel(o, w, 2 if big > 0.15 else 1, 35)
        smooth(o, 32)
    else:
        # cạnh sắc (>55°) đánh crease để giữ dáng, phần còn lại bo mềm
        try:
            attr = me.attributes.get("crease_edge") or me.attributes.new("crease_edge", 'FLOAT', 'EDGE')
            bm = bmesh.new(); bm.from_mesh(me); bm.edges.ensure_lookup_table()
            vals = []
            for e in bm.edges:
                vals.append(1.0 if (len(e.link_faces) != 2 or e.calc_face_angle(0) > math.radians(55)) else 0.0)
            bm.free()
            attr.data.foreach_set("value", vals)
        except Exception as ex:
            print("crease loi", ex)
        subsurf(o, 1); smooth(o, 60)
    return o

def nang_tu_goc(c, giu_node=(), ten_vl_giu=(), kieu=None, bo=()):
    """Dựng lại mọi mesh gốc thành bản mịn (vát/bo) + vật liệu PBR theo lớp. Trả {node giữ: [phần]}."""
    out = {n: [] for n in giu_node}
    _TEN_MON[0] = c.ten
    def chu(o):
        p = o
        while p is not None:
            if p.name in out: return p.name
            p = p.parent
        return None
    for o in [x for x in c.goc.values() if x.type == 'MESH']:
        if any(o.name.startswith(b) for b in bo): continue
        me = o.data.copy(); me.transform(o.matrix_world)
        p = bpy.data.objects.new(o.name + "_m", me); bpy.context.scene.collection.objects.link(p); _parts.append(p)
        mats = [mat_tu_goc(sl.material, ten_vl_giu) for sl in o.material_slots] or [mat("nhua", "#B0B0B0", ten="khong_ten")]
        me.materials.clear()
        for m in mats: me.materials.append(m)
        if me.uv_layers and all(sl.material and (_rgb_mat(sl.material)[3]) for sl in o.material_slots): p["uv_giu"] = 1
        elif any(sl.material and _rgb_mat(sl.material)[3] for sl in o.material_slots): p["uv_tron"] = 1
        lam_min(p, kieu)
        k = chu(o)
        if k: out[k].append(p)
    return {k: v for k, v in out.items() if v}
