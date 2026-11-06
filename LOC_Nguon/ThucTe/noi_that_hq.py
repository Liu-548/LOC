# [03/11] Đồ đạc phòng khách dựng lại chi tiết (Blender bpy 5.2) — chỉ dùng khi bật "Thực tế" (LocDoMoi.NoiThat thay lúc chạy).
#  SalonGheDai_HQ  : ghế salon gỗ dài — giữ đúng khung của bản gốc (vạt cạnh, vân gỗ thật), hai ô tựa lưng chạm nổi hoa cúc + dây lá
#  TuTVDung_HQ     : tủ TV đứng — giữ đúng mặt kệ (TV, đầu VCD, tủ kính vẫn đặt khít), vân gỗ, hai cánh dưới có ô pano chạm hoa
#  GheDauGo_HQ     : ghế đẩu gỗ — vạt cạnh, mặt ngồi mòn sáng
#  QuatTranThan_HQ : thân quạt trần sơn kem cũ (chụp trần, ty, bầu động cơ có gờ + khe gió, nắp đáy) — dựng mới, khít hộp bao bản gốc
#  QuatTranCanh_HQ : cánh quạt tôn sơn kem, tay đỡ sắt, cánh hơi xoắn — dựng mới, cùng gốc toạ độ (trục quạt) như bản gốc
#  CuaChinh_HQ     : cánh cửa chính gỗ pano: ô trên song mắt cáo + mặt trời chạm giữa, hai ô dưới pano nổi sao 8 cánh (theo hoa văn T_CuaChinh_HoaVan)
#                    hai mặt như nhau; KHÔNG có tay nắm / bản lề (giữ núm đồng + bản lề của bản gốc)
# Chạy: python3 noi_that_hq.py -- <Assets/LOC_House/Models/NoiThat> <ra.glb>
import bpy, bmesh, sys, math, random
import numpy as np
from mathutils import Vector

a = sys.argv[sys.argv.index('--') + 1:]; GOC, OUT = a[0], a[1]
rng = random.Random(2002); nrng = np.random.default_rng(2002)
bpy.ops.wm.read_factory_settings(use_empty=True)
sc = bpy.context.scene

def srgb(h):
    h = h.lstrip('#'); c = [int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    return tuple(x / 12.92 if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4 for x in c)

def anh_np(ten, arr, mau=True):
    """mảng HxWx3 (0..1 sRGB, gốc trên-trái) → ảnh Blender (đóng gói vào glb)"""
    h, w = arr.shape[:2]
    im = bpy.data.images.new(ten, w, h, alpha=False)
    if not mau: im.colorspace_settings.name = 'Non-Color'
    px = np.concatenate([arr[::-1], np.ones((h, w, 1))], -1).astype(np.float32).ravel()
    im.pixels.foreach_set(px); im.pack(); return im

def vat_lieu(ten, mau=(1, 1, 1), anh=None, phap=None, nham=0.5, kl=0.0, do_phap=1.0):
    m = bpy.data.materials.new(ten); m.use_nodes = True
    nt = m.node_tree; b = nt.nodes["Principled BSDF"]
    b.inputs["Roughness"].default_value = nham; b.inputs["Metallic"].default_value = kl
    b.inputs["Base Color"].default_value = (*mau, 1)
    if anh is not None:
        t = nt.nodes.new("ShaderNodeTexImage"); t.image = anh
        if mau != (1, 1, 1):   # glTF: baseColorFactor × texture
            mx = nt.nodes.new("ShaderNodeMix"); mx.data_type = 'RGBA'; mx.blend_type = 'MULTIPLY'
            mx.inputs[0].default_value = 1.0
            nt.links.new(t.outputs["Color"], mx.inputs[6]); mx.inputs[7].default_value = (*mau, 1)
            nt.links.new(mx.outputs[2], b.inputs["Base Color"])
        else: nt.links.new(t.outputs["Color"], b.inputs["Base Color"])
    if phap is not None:
        t = nt.nodes.new("ShaderNodeTexImage"); t.image = phap
        nm = nt.nodes.new("ShaderNodeNormalMap"); nm.inputs["Strength"].default_value = do_phap
        nt.links.new(t.outputs["Color"], nm.inputs["Color"]); nt.links.new(nm.outputs["Normal"], b.inputs["Normal"])
    return m

# ───────────────────────── nhiễu / texture
def nhieu(h, w, beta, seed, keo_x=1.0):
    """nhiễu 1/f^beta lặp liền; keo_x > 1 kéo dài vệt theo trục ngang (thớ gỗ)"""
    r = np.random.default_rng(seed); fy = np.fft.fftfreq(h)[:, None]; fx = np.fft.fftfreq(w)[None, :]
    f = np.sqrt((fx * keo_x) ** 2 + fy ** 2); f[0, 0] = 1
    s = (1 / f ** (beta / 2)) * np.exp(1j * r.uniform(0, 2 * np.pi, (h, w))); s[0, 0] = 0
    x = np.real(np.fft.ifft2(s)); x -= x.min(); return x / (x.max() + 1e-9)

def lam_mo(x, sig):
    h, w = x.shape; fy = np.fft.fftfreq(h)[:, None]; fx = np.fft.fftfreq(w)[None, :]
    return np.real(np.fft.ifft2(np.fft.fft2(x) * np.exp(-2 * (np.pi * sig) ** 2 * (fx * fx + fy * fy))))

def phap_tu_cao(hm, manh):
    """bản đồ độ cao → normal map OpenGL (xanh lá = lên trên ảnh)"""
    gx = (np.roll(hm, -1, 1) - np.roll(hm, 1, 1)) * 0.5 * manh
    gy = (np.roll(hm, -1, 0) - np.roll(hm, 1, 0)) * 0.5 * manh
    n = np.stack([-gx, gy, np.ones_like(hm)], -1); n /= np.linalg.norm(n, axis=-1, keepdims=True)
    return n * 0.5 + 0.5

def ss(e0, e1, x):
    t = np.clip((x - e0) / (e1 - e0), 0, 1); return t * t * (3 - 2 * t)

def van_go(n=1024, seed=11):
    """vân gỗ xám ấm (nhân với màu vật liệu): thớ chạy theo trục ngang ảnh = trục U. Trả (albedo sRGB, độ cao)"""
    v = np.linspace(0, 1, n, endpoint=False)[:, None] * np.ones((1, n))
    cong = nhieu(n, n, 3.2, seed, 10)          # thớ uốn lượn chậm
    pha = v * 9 + (cong - 0.5) * 2.6
    vong = 0.5 + 0.5 * np.sin(2 * np.pi * pha)
    vong = vong ** 2.2                            # vân sớm/muộn: vệt tối mảnh
    soi = nhieu(n, n, 1.4, seed + 1, 24)          # sợi gỗ mảnh
    lo = np.clip((nhieu(n, n, 0.6, seed + 2, 30) - 0.72) * 6, 0, 1)   # lỗ mạch
    loang = nhieu(n, n, 2.6, seed + 3, 2)         # loang màu sơn vecni
    g = 0.80 - 0.20 * vong + 0.10 * (soi - 0.5) - 0.12 * lo + 0.10 * (loang - 0.5)
    alb = np.stack([g * 1.0, g * 0.93, g * 0.86], -1)
    cao = 0.55 * (1 - vong) + 0.35 * soi - 0.6 * lo
    return np.clip(alb, 0, 1), cao

def son_kem(n=512, seed=31):
    """sơn kem cũ: ố vàng loang, bụi, vài chỗ tróc lộ sắt"""
    loang = nhieu(n, n, 2.4, seed); bui = nhieu(n, n, 1.0, seed + 1); troc = nhieu(n, n, 2.0, seed + 2)
    m = np.clip((troc - 0.80) * 9, 0, 1)
    c = np.array([0.93, 0.89, 0.78])[None, None] * (0.92 + 0.08 * bui[..., None]) - np.array([0, 0.03, 0.09])[None, None] * loang[..., None]
    c = c * (1 - m[..., None]) + np.array([0.33, 0.27, 0.22])[None, None] * m[..., None]
    return np.clip(c, 0, 1), (1 - m) * 0.6 + bui * 0.2

# ───────────────────────── hoa văn chạm nổi (độ cao 0..1)
def luoi(w_m, h_m, ppm):
    W, H = int(w_m * ppm), int(h_m * ppm)
    x = (np.arange(W) + 0.5) / ppm - w_m / 2; y = h_m / 2 - (np.arange(H) + 0.5) / ppm
    return np.meshgrid(x, y)

def bong_hoa(x, y, R, canh, nhon=2.0):
    r = np.sqrt(x * x + y * y); t = np.arctan2(y, x)
    bien = R * (0.62 + 0.38 * np.abs(np.cos(canh * t / 2)) ** nhon)
    h = np.clip(1 - r / bien, 0, 1) ** 0.5
    gan = 1 - 0.25 * np.abs(np.sin(canh * t / 2)) ** 8            # gân giữa cánh
    nhuy = np.clip(1 - r / (R * 0.22), 0, 1) ** 0.4
    return np.maximum(h * gan * 0.85, nhuy)

def soi_chi(d, rong):    # gờ tròn có tâm d = 0
    return np.clip(1 - (d / rong) ** 2, 0, 1) ** 0.5

def la(x, y, x0, y0, goc, dai, rong):
    c, s = math.cos(goc), math.sin(goc)
    u = (x - x0) * c + (y - y0) * s; v = -(x - x0) * s + (y - y0) * c
    t = np.clip(u / dai, 0, 1); w = rong * np.sin(np.pi * t) ** 0.8
    h = np.clip(1 - (np.abs(v) / (w + 1e-6)) ** 2, 0, 1) * ((u > 0) & (u < dai))
    return h * (1 - 0.35 * soi_chi(v, rong * 0.12))              # gân lá chìm

def vien_chu_nhat(x, y, w_m, h_m, lui, rong):
    d = np.minimum(np.abs(np.abs(x) - (w_m / 2 - lui)) * (np.abs(y) <= h_m / 2 - lui + rong),
                   np.abs(np.abs(y) - (h_m / 2 - lui)) * (np.abs(x) <= w_m / 2 - lui + rong))
    return soi_chi(d, rong)

def hv_tua_ghe(w_m, h_m, ppm=900):
    """ô tựa ghế salon: khung hạt chỉ, giữa bông cúc trong vòng tròn, hai bên dây lá cuộn đối xứng"""
    x, y = luoi(w_m, h_m, ppm); R = h_m * 0.30
    h = vien_chu_nhat(x, y, w_m, h_m, 0.012, 0.004) * 0.55
    r = np.sqrt(x * x + y * y)
    h = np.maximum(h, soi_chi(r - R * 1.28, 0.006) * 0.7)
    h = np.maximum(h, bong_hoa(x, y, R, 12) * 1.0)
    ax = np.abs(x); x0 = R * 1.45; x1 = w_m / 2 - 0.03; P = (x1 - x0) / 1.5
    yc = 0.30 * h_m * np.sin(np.pi * (ax - x0) / P) * (ax > x0) * (ax < x1)
    than = soi_chi(np.abs(y - yc), 0.0045) * (ax > x0) * (ax < x1)
    h = np.maximum(h, than * 0.6)
    for k in range(5):
        xx = x0 + (k + 0.5) * (x1 - x0) / 5; yy = 0.30 * h_m * math.sin(math.pi * (xx - x0) / P)
        phia = 1 if k % 2 == 0 else -1
        for sx in (-1, 1):
            g = math.atan2(phia, 1.4 * sx) if sx > 0 else math.atan2(phia, -1.4)
            h = np.maximum(h, la(x, y, sx * xx, yy, g, h_m * 0.26, h_m * 0.07) * 0.75)
    return lam_mo(h, 1.2)

def hv_cua_tren(w_m, h_m, ppm=420):
    """ô trên cánh cửa: song mắt cáo chéo, mặt trời 16 tia trong vòng tròn ở giữa, 4 vòng nhỏ ở góc"""
    x, y = luoi(w_m, h_m, ppm); p = 0.105; rong = 0.009
    def song(t): return soi_chi(np.abs(((t / p + 0.5) % 1) - 0.5) * p, rong)
    lat = np.maximum(song(x + y), song(x - y)) * 0.7
    r = np.sqrt(x * x + y * y); R = 0.23
    lat *= ss(R * 1.05, R * 1.15, r)
    vong = soi_chi(r - R, 0.012)
    hoa = bong_hoa(x, y, R * 0.78, 16, 3.0)
    h = np.maximum(lat, vong)
    h = np.maximum(h, hoa * 0.9)
    vien = vien_chu_nhat(x, y, w_m, h_m, 0.012, 0.008) * 0.8
    h = np.maximum(h, vien)
    vang = np.maximum(vong, (hoa > 0.05) * 0.8)
    for sx in (-1, 1):
        for sy in (-1, 1):
            rr = np.sqrt((x - sx * (w_m / 2 - 0.07)) ** 2 + (y - sy * (h_m / 2 - 0.07)) ** 2)
            vg = soi_chi(rr - 0.025, 0.006)
            h = np.maximum(h, vg * 0.9); vang = np.maximum(vang, vg)
    return lam_mo(h, 1.0), vang

def hv_pano(w_m, h_m, ppm=420, sao=True, R=None):
    """ô pano nổi: mặt pano vát 4 cạnh + vòng tròn và sao 8 cánh ở giữa"""
    x, y = luoi(w_m, h_m, ppm)
    vat = min(w_m, h_m) * 0.10
    d = np.minimum(w_m / 2 - np.abs(x), h_m / 2 - np.abs(y))
    h = np.clip(d / vat, 0, 1) * 0.45
    R = R or min(w_m, h_m) * 0.36; r = np.sqrt(x * x + y * y)
    h = np.maximum(h, 0.45 + soi_chi(r - R, 0.008) * 0.45)
    if sao:
        t = np.arctan2(y, x)
        bien = R * 0.86 * (0.30 + 0.70 * np.abs(np.cos(4 * t)) ** 6)
        h = np.maximum(h, 0.45 + np.clip(1 - r / bien, 0, 1) ** 0.6 * 0.55)
    else:
        h = np.maximum(h, 0.45 + bong_hoa(x, y, R * 0.8, 4, 1.5) * 0.5)
    return lam_mo(h, 1.0)

# ───────────────────────── tiện ích lưới
def tam_noi(ten, w_m, h_m, cao, sau, tam, y0, mat_huong, vl, n_x, n_z, nghieng=0.0):
    """tấm chạm nổi: lưới n_x × n_z, mặt nhìn về mat_huong (−1 = −Y, +1 = +Y), lồi tối đa 'sau' m từ mặt y0 (y0 tại tâm; nghieng = dy/dz của mặt đỡ)"""
    H, W = cao.shape
    bm = bmesh.new(); uvl = bm.loops.layers.uv.new()
    vs = []
    for j in range(n_z + 1):
        row = []
        for i in range(n_x + 1):
            u, v = i / n_x, j / n_z
            hv = cao[min(H - 1, int((1 - v) * (H - 1))), min(W - 1, int(u * (W - 1)))]
            xx = (u - 0.5) * w_m; xx = xx if mat_huong < 0 else -xx
            row.append(bm.verts.new((tam[0] + xx, y0 + nghieng * (v - 0.5) * h_m + mat_huong * sau * hv, tam[1] + (v - 0.5) * h_m)))
        vs.append(row)
    for j in range(n_z):
        for i in range(n_x):
            q = [vs[j][i], vs[j][i + 1], vs[j + 1][i + 1], vs[j + 1][i]]
            f = bm.faces.new(q)
            for l, (di, dj) in zip(f.loops, [(0, 0), (1, 0), (1, 1), (0, 1)]): l[uvl].uv = ((i + di) / n_x, (j + dj) / n_z)
    me = bpy.data.meshes.new(ten); bm.to_mesh(me); bm.free()
    for p in me.polygons: p.use_smooth = True
    me.materials.append(vl)
    o = bpy.data.objects.new(ten, me); sc.collection.objects.link(o); return o

def hop(ten, x0, x1, y0, y1, z0, z1, vl, vat=0.0035):
    bm = bmesh.new(); bmesh.ops.create_cube(bm, size=1)
    for v in bm.verts: v.co = Vector(((x0 + x1) / 2 + v.co.x * (x1 - x0), (y0 + y1) / 2 + v.co.y * (y1 - y0), (z0 + z1) / 2 + v.co.z * (z1 - z0)))
    if vat > 0: bmesh.ops.bevel(bm, geom=list(bm.edges), offset=vat, segments=2, affect='EDGES', profile=0.5)
    me = bpy.data.meshes.new(ten); bm.to_mesh(me); bm.free(); me.materials.append(vl)
    o = bpy.data.objects.new(ten, me); sc.collection.objects.link(o); return o

def tien(ten, profile, vl, n=40, z_huong=1):
    """khối tiện tròn quanh trục Z từ profile [(r, z), ...]"""
    bm = bmesh.new(); vong = []
    for (r, z) in profile:
        vong.append([bm.verts.new((r * math.cos(2 * math.pi * k / n), r * math.sin(2 * math.pi * k / n), z)) for k in range(n)])
    for a_, b_ in zip(vong[:-1], vong[1:]):
        for k in range(n): bm.faces.new([a_[k], a_[(k + 1) % n], b_[(k + 1) % n], b_[k]])
    for vv in (vong[0], vong[-1]):
        if len(set(round(v.co.x, 6) for v in vv)) > 1: bm.faces.new(vv)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    me = bpy.data.meshes.new(ten); bm.to_mesh(me); bm.free(); me.materials.append(vl)
    for p in me.polygons: p.use_smooth = True
    o = bpy.data.objects.new(ten, me); sc.collection.objects.link(o); return o

def uv_hop_theo_tho(o, o_m=0.8):
    """UV hộp theo từng mảnh rời: U = trục dài nhất của mảnh (thớ gỗ chạy dọc thanh), lệch ngẫu nhiên từng mảnh"""
    bm = bmesh.new(); bm.from_mesh(o.data)
    uvl = bm.loops.layers.uv.verify(); bm.faces.ensure_lookup_table()
    con = set(bm.faces)
    while con:
        f0 = con.pop(); dao = [f0]; dao_set = {f0}; k = 0
        while k < len(dao):
            for e in dao[k].edges:
                for g in e.link_faces:
                    if g in con: con.discard(g); dao.append(g); dao_set.add(g)
            k += 1
        pts = [v.co for f in dao for v in f.verts]
        mn = [min(p[i] for p in pts) for i in range(3)]; mx = [max(p[i] for p in pts) for i in range(3)]
        dai = max(range(3), key=lambda i: mx[i] - mn[i])
        du, dv = rng.random(), rng.random()
        for f in dao:
            A = max(range(3), key=lambda i: abs(f.normal[i]))
            ac = [i for i in range(3) if i != A]
            U = dai if dai != A else max(ac, key=lambda i: mx[i] - mn[i])
            V = [i for i in ac if i != U][0]
            for l in f.loops: l[uvl].uv = (l.vert.co[U] / o_m + du, l.vert.co[V] / o_m + dv)
    bm.to_mesh(o.data); bm.free()

def nhap_goc(file, ten, doi_vl, vat=0.0025):
    """nhập .glb gốc, gộp một lưới, hàn đỉnh, vạt cạnh sắc, UV theo thớ, đổi vật liệu theo tên"""
    truoc = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=file)
    moi = [o for o in bpy.data.objects if o not in truoc]
    ms = [o for o in moi if o.type == 'MESH']
    for o in ms: o.data = o.data.copy(); o.data.transform(o.matrix_world); o.matrix_world = o.matrix_world.__class__.Identity(4)
    bpy.ops.object.select_all(action='DESELECT')
    for o in ms: o.select_set(True)
    bpy.context.view_layer.objects.active = ms[0]
    if len(ms) > 1: bpy.ops.object.join()
    o = bpy.context.view_layer.objects.active
    for x in [x for x in bpy.data.objects if x != o and x.type != 'MESH' and x not in truoc]: bpy.data.objects.remove(x)
    o.parent = None; o.name = ten; o.data.name = ten
    bm = bmesh.new(); bm.from_mesh(o.data)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.0004)
    bm.to_mesh(o.data); bm.free()
    if vat > 0:
        md = o.modifiers.new("vat", 'BEVEL'); md.width = vat; md.segments = 2; md.limit_method = 'ANGLE'
        md.angle_limit = math.radians(35); md.harden_normals = False; md.use_clamp_overlap = True
        bpy.context.view_layer.objects.active = o; bpy.ops.object.modifier_apply(modifier="vat")
    bm = bmesh.new(); bm.from_mesh(o.data); bmesh.ops.triangulate(bm, faces=bm.faces); bm.to_mesh(o.data); bm.free()
    for p in o.data.polygons: p.use_smooth = True
    o.data.set_sharp_from_angle(angle=math.radians(40))
    uv_hop_theo_tho(o)
    for i, m in enumerate(o.data.materials):
        for k, v in doi_vl.items():
            if m and k in m.name: o.data.materials[i] = v; break
    return o

def gop(ten, ds):
    bpy.ops.object.select_all(action='DESELECT')
    for o in ds: o.select_set(True)
    bpy.context.view_layer.objects.active = ds[0]; bpy.ops.object.join()
    o = bpy.context.view_layer.objects.active; o.name = ten; o.data.name = ten
    bm = bmesh.new(); bm.from_mesh(o.data); bmesh.ops.triangulate(bm, faces=bm.faces); bm.to_mesh(o.data); bm.free()
    return o

def hb(o):
    p = [Vector(c) for c in o.bound_box]
    return tuple(round(min(v[i] for v in p), 3) for i in range(3)), tuple(round(max(v[i] for v in p), 3) for i in range(3))

# ───────────────────────── texture dùng chung
ga, gc = van_go(1024, 11)
T_GO = anh_np("Go_VanXam", ga); T_GO_N = anh_np("Go_VanXam_N", phap_tu_cao(lam_mo(gc, 0.6), 2.2), False)
def go(ten, hex_, nham, sang=1.55):   # màu gốc (tuyến tính) chia cho độ sáng TB của vân (~0.62) để giữ tông như bản cũ
    c = srgb(hex_) if isinstance(hex_, str) else hex_
    return vat_lieu(ten, tuple(min(1, x * sang) for x in c), T_GO, T_GO_N, nham, 0, 0.6)

VL_VENNI = go("go_salon_venni_HQ", (0.107, 0.042, 0.018), 0.34)
VL_TAYMON = go("go_tay_mon_HQ", (0.20, 0.09, 0.04), 0.42)
VL_LONGTU = go("go_long_tu_toi_HQ", (0.042, 0.018, 0.008), 0.6)
VL_CHI = go("go_chi_pano_HQ", (0.076, 0.028, 0.012), 0.4)
VL_DONG = vat_lieu("dong_tay_nam_HQ", (0.55, 0.36, 0.12), nham=0.32, kl=1.0)
VL_KINH = vat_lieu("kinh_dot_HQ", (0.597, 0.672, 0.658), nham=0.05)
VL_DAU1 = go("go_ghe_dau_mat_HQ", (0.30, 0.12, 0.04), 0.7)
VL_DAU2 = go("go_ghe_dau_chan_HQ", (0.195, 0.067, 0.015), 0.75)
VL_CUA = go("go_cua_chinh_HQ", srgb("#5A3418"), 0.45, 1.5)
VL_CUA_TOI = go("go_cua_long_HQ", srgb("#2A160B"), 0.55, 1.5)

def vl_cham(ten, cao, mau_go, mau_sau=None, vang=None, manh=6.0, nham=0.4):
    """vật liệu tấm chạm: vân gỗ (lấy mẫu theo kích thước ảnh) + kẽ chạm tối (AO giả từ độ cao) + (tuỳ) phần thếp vàng"""
    H, W = cao.shape
    yy = (np.arange(H)[:, None] * 2) % ga.shape[0]; xx = (np.arange(W)[None, :] * 2) % ga.shape[1]
    nen = ga[yy, xx]
    ao = np.clip(0.55 + 0.45 * (cao - lam_mo(cao, 4.0)) * 6 + 0.25 * cao, 0.35, 1.0)
    c_go = np.array(mau_go)[None, None]
    c_sau = np.array(mau_sau if mau_sau else mau_go)[None, None]
    mix = cao[..., None]
    alb = nen * (c_sau * (1 - mix) + c_go * mix) * ao[..., None]
    if vang is not None:
        v = np.clip(vang, 0, 1)[..., None]; alb = alb * (1 - v) + np.array([0.78, 0.60, 0.26])[None, None] * (0.7 + 0.3 * nen) * ao[..., None] * v
    im = anh_np(ten, np.clip(alb, 0, 1)); imn = anh_np(ten + "_N", phap_tu_cao(cao, manh), False)
    return vat_lieu(ten, (1, 1, 1), im, imn, nham, 0, 1.0)

# ═════════════════════════ 1. ghế salon dài
ghe = nhap_goc(f"{GOC}/PhongKhach/mesh/Salon_GheDai.glb", "SalonGheDai_HQ", {"venni": VL_VENNI, "tay_mon": VL_TAYMON})
# ô tựa (dò tia trên bản gốc): x 0,05–0,73 mỗi bên, z 0,56–0,77; lưng ngả: mặt ô y = 0,261 + 0,06·(z − 0,56)
w_o, h_o = 0.66, 0.20
cao = hv_tua_ghe(w_o, h_o)
vl = vl_cham("ChamTuaGhe", cao, (0.62, 0.40, 0.26), (0.40, 0.24, 0.15), nham=0.38, manh=9)
tam = [tam_noi(f"tua{i}", w_o, h_o, cao, 0.006, (sx * 0.39, 0.665), 0.2665, -1, vl, 90, 28, 0.06) for i, sx in enumerate((-1, 1))]
ghe = gop("SalonGheDai_HQ", [ghe] + tam)
print("ghe", hb(ghe), len(ghe.data.polygons))

# ═════════════════════════ 2. tủ TV đứng
tu = nhap_goc(f"{GOC}/PhongKhach/mesh/TuTV_Dung.glb", "TuTVDung_HQ",
              {"venni": VL_VENNI, "long_tu": VL_LONGTU, "kinh": VL_KINH, "chi_pano": VL_CHI, "dong": VL_DONG})
# mặt pano nổi của hai cánh dưới (dò tia): x 0,07–0,54 · 0,66–1,13, z 0,13–0,52, mặt y −0,0445
w_o, h_o = 0.44, 0.36
cao = hv_pano(w_o, h_o, 700, sao=False)
vl = vl_cham("PanoTu", cao, (0.62, 0.40, 0.26), (0.40, 0.24, 0.15), nham=0.36, manh=12)
tam = [tam_noi(f"pano{i}", w_o, h_o, cao, 0.006, (cx, 0.325), -0.0452, -1, vl, 36, 30) for i, cx in enumerate((0.305, 0.895))]
tu = gop("TuTVDung_HQ", [tu] + tam)
print("tu", hb(tu), len(tu.data.polygons))

# ═════════════════════════ 3. ghế đẩu
dau = nhap_goc(f"{GOC}/BoSung/mesh/GheDau_Go.glb", "GheDauGo_HQ", {"material_0": VL_DAU1, "material_1": VL_DAU2}, 0.002)
print("dau", hb(dau), len(dau.data.polygons))

# ═════════════════════════ 4. quạt trần
sa, sc_ = son_kem(512)
T_SON = anh_np("SonKem", sa); T_SON_N = anh_np("SonKem_N", phap_tu_cao(lam_mo(sc_, 1.0), 3.0), False)
VL_SON = vat_lieu("son_quat_kem_HQ", (1, 1, 1), T_SON, T_SON_N, 0.55)
VL_SAT = vat_lieu("kl_ty_quat_HQ", (0.32, 0.30, 0.28), nham=0.45, kl=0.9)
VL_KHE = vat_lieu("son_khe_gio_HQ", (0.05, 0.045, 0.04), nham=0.8)
R = 0.089
chup = tien("chup", [(0.0, 0.0), (0.060, 0.0), (0.062, -0.004), (0.058, -0.022), (0.040, -0.036), (0.018, -0.042), (0.0, -0.042)], VL_SON, 48)
ty = tien("ty", [(0.0, -0.040), (0.011, -0.040), (0.011, -0.118), (0.0, -0.118)], VL_SAT, 24)
bau_p = [(0.0, -0.112), (0.030, -0.112), (0.060, -0.120), (0.080, -0.132), (R, -0.146), (R, -0.152), (0.086, -0.154),
         (0.086, -0.160), (R, -0.162), (R, -0.170), (0.080, -0.186), (0.055, -0.198), (0.0, -0.200)]
bau = tien("bau", bau_p, VL_SON, 64)
khe = tien("khe", [(0.0862, -0.1535), (0.0862, -0.1605)], VL_KHE, 64)
nap = tien("nap", [(0.0, -0.198), (0.030, -0.198), (0.026, -0.210), (0.014, -0.218), (0.0, -0.220)], VL_SAT, 32)
than = gop("QuatTranThan_HQ", [chup, ty, bau, khe, nap])
print("than", hb(than), len(than.data.polygons))

def canh_quat():
    bm = bmesh.new(); uvl = bm.loops.layers.uv.new()
    # tay đỡ sắt: thanh dẹt cong từ trục ra, ép dưới cánh
    tay = hop("tay", 0.0, 0.20, -0.018, 0.018, -0.006, 0.0, VL_SAT, 0.002)
    # cánh: lưới 24 × 8, mép bo tròn đầu, xoắn nhẹ quanh trục X, dày 3 mm (hai mặt)
    nx, ny = 28, 8; x0, x1 = 0.13, 0.55; y0, y1 = -0.08, 0.09
    def diem(u, v, mat):
        x = x0 + (x1 - x0) * u
        cy = (y0 + y1) / 2; hw = (y1 - y0) / 2
        tip = 1.0 if u < 0.85 else max(0.25, math.sqrt(max(0.0, 1 - ((u - 0.85) / 0.15) ** 2)))
        goc_ = 1.0 if u > 0.08 else 0.55 + 0.45 * (u / 0.08)
        y = cy + (v - 0.5) * 2 * hw * min(tip, goc_)
        xoan = math.radians(7 + 3 * u)
        z = (y - cy) * math.sin(xoan) * 0.9 + (0.0015 if mat else -0.0015)
        return (x, (y - cy) * math.cos(xoan) + cy, max(-0.0099, min(0.0099, z)))
    for mat in (1, 0):
        g = [[bm.verts.new(diem(i / nx, j / ny, mat)) for i in range(nx + 1)] for j in range(ny + 1)]
        for j in range(ny):
            for i in range(nx):
                q = [g[j][i], g[j][i + 1], g[j + 1][i + 1], g[j + 1][i]]
                if not mat: q = q[::-1]
                f = bm.faces.new(q)
                for l in f.loops: l[uvl].uv = (l.vert.co.x * 1.6, l.vert.co.y * 1.6)
    me = bpy.data.meshes.new("canh"); bm.to_mesh(me); bm.free(); me.materials.append(VL_SON)
    for p in me.polygons: p.use_smooth = True
    o = bpy.data.objects.new("canh", me); sc.collection.objects.link(o)
    return gop("QuatTranCanh_HQ", [o, tay])
cq = canh_quat()
print("canh", hb(cq), len(cq.data.polygons))

# ═════════════════════════ 5. cánh cửa chính (x ±0,70 · z 0..2,60 · y ±0,025)
W2, HC, D = 0.70, 2.60, 0.025
ST, RT, RB, RG0, RG1, MU = 0.11, 0.11, 0.15, 1.13, 1.29, 0.07
khung = [hop("dung_trai", -W2, -W2 + ST, -D, D, 0, HC, VL_CUA), hop("dung_phai", W2 - ST, W2, -D, D, 0, HC, VL_CUA),
         hop("ngang_tren", -W2 + ST, W2 - ST, -D, D, HC - RT, HC, VL_CUA), hop("ngang_duoi", -W2 + ST, W2 - ST, -D, D, 0, RB, VL_CUA),
         hop("ngang_giua", -W2 + ST, W2 - ST, -D, D, RG0, RG1, VL_CUA), hop("dung_giua", -MU / 2, MU / 2, -D, D, RB, RG0, VL_CUA)]
# gờ chỉ nổi trên thanh ngang giữa (hai mặt)
for s in (-1, 1):   # rãnh tối khoét trên thanh ngang giữa (hai mặt), nằm trong bề dày cánh
    khung.append(hop(f"ranh_giua{s}", -W2 + ST + 0.02, W2 - ST - 0.02, (D - 0.003) if s > 0 else -D - 0.0005, (D + 0.0005) if s > 0 else (-D + 0.003),
                     (RG0 + RG1) / 2 - 0.012, (RG0 + RG1) / 2 + 0.012, VL_CUA_TOI, 0.001))
wt, ht = 2 * (W2 - ST), HC - RT - RG1
cao_t, vang_t = hv_cua_tren(wt, ht)
vl_t = vl_cham("ChamCuaTren", cao_t, (0.50, 0.32, 0.18), (0.10, 0.06, 0.035), vang=vang_t * 0.9, nham=0.45, manh=4)
lot_t = hop("lot_tren", -W2 + ST - 0.01, W2 - ST + 0.01, -0.008, 0.008, RG1 - 0.01, HC - RT + 0.01, VL_CUA_TOI, 0)
tam_t = [tam_noi(f"tren{s}", wt, ht, cao_t, 0.0165, (0, RG1 + ht / 2), s * 0.008, s, vl_t, 72, 72) for s in (-1, 1)]
wd, hd = W2 - ST - MU / 2, RG0 - RB
cao_d = hv_pano(wd, hd, 420, sao=True)
vang_d = soi_chi(np.abs(np.sqrt(sum(g * g for g in luoi(wd, hd, 420))) - min(wd, hd) * 0.36), 0.008)
vl_d = vl_cham("PanoCuaDuoi", cao_d, (0.55, 0.34, 0.19), (0.36, 0.21, 0.12), vang=vang_d * 0.8, nham=0.42, manh=4)
lot_d = [hop(f"lot_duoi{i}", cx - wd / 2 - 0.01, cx + wd / 2 + 0.01, -0.010, 0.010, RB - 0.01, RG0 + 0.01, VL_CUA, 0) for i, cx in enumerate((-(MU / 2 + wd / 2), MU / 2 + wd / 2))]
tam_d = [tam_noi(f"duoi{i}{s}", wd, hd, cao_d, 0.0145, (cx, RB + hd / 2), s * 0.010, s, vl_d, 28, 48)
         for i, cx in enumerate((-(MU / 2 + wd / 2), MU / 2 + wd / 2)) for s in (-1, 1)]
for o in khung + [lot_t] + lot_d: uv_hop_theo_tho(o)
cua = gop("CuaChinh_HQ", khung + [lot_t] + lot_d + tam_t + tam_d)
for p in cua.data.polygons: p.use_smooth = True
cua.data.set_sharp_from_angle(angle=math.radians(40))
print("cua", hb(cua), len(cua.data.polygons))

# ───────────────────────── xuất
bpy.ops.object.select_all(action='DESELECT')
for o in (ghe, tu, dau, than, cq, cua): o.select_set(True)
bpy.ops.export_scene.gltf(filepath=OUT, export_format='GLB', use_selection=True, export_apply=True, export_tangents=True,
                          export_image_format='AUTO', export_yup=True)
print("XONG", OUT)
