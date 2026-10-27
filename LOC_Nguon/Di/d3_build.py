# DÌ — Bước 3 (low-poly, đã duyệt 21/10): tóc + búi, khăn tang + 2 đuôi, dép; giảm lưới ~1,6k tam giác, shade phẳng; UV; texture; vật liệu.
# Mắt vẽ trên texture da (không dùng nhãn cầu) — to hơn bản phác theo yêu cầu 21/10.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, math, numpy as np, geo as G, bake
from mathutils import Vector, Matrix
from PIL import Image, ImageFilter
OUT = "/home/claude/di/out"; TX = f"{OUT}/tex"; os.makedirs(TX, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b2a.blend")
O = bpy.data.objects; arm = O["DiRig"]; B = O["Body"]
for n in ("Body_Full", "MouthIn", "Eye_L", "Eye_R"): bpy.data.objects.remove(O[n])
dom, _, _ = G.dominant(B)
cy = -0.035

def hairline(co):   # chân tóc: trán 1,515 → thái dương → sau gáy 1,40
    a = abs(math.degrees(math.atan2(co.x, -(co.y - cy))))
    if a < 35: return 1.515
    if a < 80: return 1.515 - (a - 35) / 45 * 0.065
    return 1.45 - (a - 80) / 100 * 0.05
def cap(name, keep, off, snap=None):
    o = G.dup(B, name)
    G.keep_faces(o, lambda f: all(dom[v.index] in ("Head", "Neck") and keep(v.co) for v in f.verts))
    if snap: G.snap_boundary(o, snap)
    G.inflate(o, off, smooth=4, rim=0.004)
    for g in list(o.vertex_groups): o.vertex_groups.remove(g)
    o.vertex_groups.new(name="Head").add(list(range(len(o.data.vertices))), 1.0, "REPLACE")
    return o
cap("Toc", lambda co: co.z > hairline(co) - 0.004 and not (abs(co.x) > 0.066 and 1.41 < co.z < 1.48 and abs(co.y - cy) < 0.035), 0.013)
def zc(co): return 1.522 - 0.045 * max(0, (co.y - (cy - 0.09))) / 0.18
cap("KhanTang", lambda co: abs(co.z - zc(co)) < 0.021, 0.022,
    lambda co: Vector((co.x, co.y, zc(co) + (0.018 if co.z > zc(co) else -0.018))))

def rigid(o, bone, parent=True):
    o.vertex_groups.new(name=bone).add(list(range(len(o.data.vertices))), 1.0, "REPLACE")
    o.parent = arm; m = o.modifiers.new("Armature", "ARMATURE"); m.object = arm
def prim(fn, name, **kw):
    fn(**kw); o = bpy.context.object; o.name = o.data.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True); return o
def placed(fn, name, scale, rot=(0, 0, 0), **kw):
    fn(**kw); o = bpy.context.object; o.name = o.data.name = name; o.scale = scale; o.rotation_euler = rot
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True); return o
rigid(placed(bpy.ops.mesh.primitive_uv_sphere_add, "Bui", (1.2, 0.85, 0.9), radius=0.042, location=(0, cy + 0.105, 1.43), segments=8, ring_count=5), "Head")
for s in (-1, 1):
    rigid(placed(bpy.ops.mesh.primitive_cube_add, f"DuoiKhan{'_L' if s > 0 else '_R'}", (0.034, 0.005, 0.20), (math.radians(-6), math.radians(6 * s), 0),
                 size=1, location=(0.022 * s, cy + 0.118, 1.36)), "Neck")
    sd = "_L" if s > 0 else "_R"
    f = arm.data.bones["Foot" + sd]; c = (Vector(f.head_local) + Vector(arm.data.bones["Toes" + sd].tail_local)) / 2
    rigid(placed(bpy.ops.mesh.primitive_cube_add, "Dep" + sd, (0.09, 0.25, 0.012), size=1, location=(c.x, c.y - 0.01, 0.006)), "Foot" + sd)
# gộp đuôi khăn vào khăn, dép trái+phải thành một, búi vào tóc (ít object = ít draw call)
print("OBJ", [o.name for o in O])
def join(dst, srcs):
    bpy.ops.object.select_all(action="DESELECT")
    for s in srcs: O[s].select_set(True)
    O[dst].select_set(True); bpy.context.view_layer.objects.active = O[dst]
    with bpy.context.temp_override(active_object=O[dst], selected_editable_objects=[O[dst]] + [O[s] for s in srcs]): bpy.ops.object.join()
    print("JOIN", dst, [o.name for o in O])
join("KhanTang", ["DuoiKhan_L", "DuoiKhan_R"]); join("Toc", ["Bui"]); join("Dep_L", ["Dep_R"]); d_ = O["Dep_L"]; d_.name = d_.data.name = "Dep"

TRI = {"Body": 700, "Ao": 420, "Quan": 260, "Toc": 130, "KhanTang": 70}
for n, t in TRI.items():
    o = O[n]; G.decimate(o, min(1, t / max(G.tris(o), 1)), sym=True)
for o in O:
    if o.type == "MESH":
        for p in o.data.polygons: p.use_smooth = False
        o.data.uv_layers.active = o.data.uv_layers[0] if o.data.uv_layers else o.data.uv_layers.new(name="UV")
        o.data.uv_layers[0].name = "UV"
tot = sum(G.tris(o) for o in O if o.type == "MESH"); print("TỔNG tam giác", tot, {o.name: G.tris(o) for o in O if o.type == "MESH"})

# ── UV vải (Ao, Quan): smart project, 1 đơn vị UV = 0,25 m để texture lặp
def smart(o, unit):
    for x in bpy.context.view_layer.objects: x.select_set(x == o)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=math.radians(55), island_margin=0.01, scale_to_bounds=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    A3 = sum(p.area for p in o.data.polygons); uv = o.data.uv_layers[0].data; AU = 0
    for p in o.data.polygons:
        pts = [uv[i].uv for i in p.loop_indices]
        AU += abs(sum(pts[i].x * pts[i - 1].y - pts[i - 1].x * pts[i].y for i in range(len(pts)))) / 2
    k = math.sqrt(A3 / AU) / unit
    for d in uv: d.uv = d.uv * k
for n in ("Ao", "Quan", "Toc", "KhanTang", "Dep"): smart(O[n], 0.25)

rng = np.random.default_rng(3)
def noise(w, h, s, oct=3, seed=0):
    r = np.random.default_rng(seed); a = np.zeros((h, w))
    for o_ in range(oct):
        f = 2 ** o_; sw, sh = max(2, int(w / s * f)), max(2, int(h / s * f))
        a += np.asarray(Image.fromarray((r.random((sh, sw)) * 255).astype(np.uint8)).resize((w, h), Image.BICUBIC), float) / 255 / f
    return a / sum(1 / 2 ** o_ for o_ in range(oct))
def save(a, name, flip=True):
    a = np.clip(a, 0, 1); a = a[::-1] if flip else a
    Image.fromarray((a * 255 + 0.5).astype(np.uint8)).save(f"{TX}/{name}.png")
def mix(base, col, m): m = np.clip(m, 0, 1)[..., None]; return base * (1 - m) + np.asarray(col) * m
def gauss(p, c, s): d = (p - np.asarray(c)) / np.asarray(s); return np.exp(-0.5 * (d ** 2).sum(-1))
H = lambda h: np.array([int(h[i:i + 2], 16) / 255 for i in (1, 3, 5)])

# ── DA + MẶT (nướng vị trí theo UV MakeHuman, 512)
S = 512
Bk = bake.bake_maps(B, S); p, n_ = Bk["pos"], Bk["nrm"]; x, y, z = p[..., 0], p[..., 1], p[..., 2]
c = np.broadcast_to(H("#B5826A"), p.shape).copy() * (0.94 + 0.10 * noise(S, S, 40, seed=1))[..., None]
front = (y < -0.07) & (z > 1.36)
EX, EZ = 0.031, 1.459   # tâm mắt (xương Eye_L)
for s_ in (1, -1):
    u = (x - EX * s_) / 0.0150; v = (z - EZ) / 0.0075          # [27/10] mắt rộng 3,0 cm, cao 1,5 cm — chỉ to hơn bản phác một chút
    eye = (u ** 2 + v ** 2 < 1) & front
    c = mix(c, (0.93, 0.90, 0.85), eye)
    iris = ((x - EX * s_) ** 2 + ((z - EZ) * 1.0) ** 2 < 0.0072 ** 2) & eye
    c = mix(c, (0.20, 0.13, 0.09), iris)
    c = mix(c, (0.03, 0.02, 0.02), ((x - EX * s_) ** 2 + (z - EZ) ** 2 < 0.0045 ** 2) & eye)
    c = mix(c, (1, 1, 1), ((x - EX * s_ - 0.0035 * s_) ** 2 + (z - EZ - 0.004) ** 2 < 0.0018 ** 2) & eye)   # đốm sáng
    lid = (np.abs(u) < 1.12) & front & (np.abs(z - (EZ + 0.0075 * np.sqrt(np.clip(1 - u ** 2, 0, 1)))) < 0.0018)
    c = mix(c, (0.08, 0.06, 0.05), lid)                                                                          # viền mi trên đậm
    c = mix(c, (0.52, 0.36, 0.32), gauss(p, (EX * s_, -0.12, EZ - 0.016), (0.016, 0.03, 0.004)) * front * 0.45)  # quầng mắt (mệt, mấy hôm trông nhà)
    t = (s_ * x - 0.010) / 0.040                                                                                 # lông mày
    zc_ = EZ + 0.027 + 0.004 * np.sin(np.pi * np.clip(t, 0, 1)) - 0.002 * t
    c = mix(c, (0.13, 0.10, 0.08), (t > 0) & (t < 1) & (np.abs(z - zc_) < 0.0026 * (1 - 0.4 * np.clip(t, 0, 1))) & front)
MZ = 1.392
lip = ((x / 0.022) ** 2 + ((z - MZ) / 0.0075) ** 2 < 1) & front
c = mix(c, (0.62, 0.36, 0.34), lip)
c = mix(c, (0.30, 0.16, 0.15), (np.abs(z - MZ) < 0.0012) & (np.abs(x) < 0.021) & front)
c = mix(c, (0.60, 0.42, 0.34), gauss(p, (0, -0.135, 1.418), (0.009, 0.03, 0.01)) * front * 0.3)                # bóng mũi

# da đầu dưới tóc → màu tóc (lấp khe thái dương/gáy sau khi giảm lưới)
ang = np.abs(np.degrees(np.arctan2(x, -(y - cy))))
hl = np.where(ang < 35, 1.515, np.where(ang < 80, 1.515 - (ang - 35) / 45 * 0.065, 1.45 - (ang - 80) / 100 * 0.05))
ear = (np.abs(x) > 0.064) & (z > 1.40) & (z < 1.475) & (np.abs(y - cy) < 0.035)
sc_ = np.asarray(Image.fromarray((((z > hl - 0.012) & ~ear & (z > 1.38)) * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(2)), float) / 255
c = mix(c, H("#1A1715"), sc_)
save(c, "T_Di_Da")

# ── vải áo (nâu đất, sợi thô), quần (lụa đen bóng mờ), tóc, khăn xô, dép — 128 px, lặp
T = 128; I, J = np.indices((T, T))
ao = np.broadcast_to(H("#5E4636"), (T, T, 3)) * (0.9 + 0.16 * noise(T, T, 20, seed=2))[..., None] - ((I + J) % 2 * 0.02)[..., None]
save(ao, "T_Di_Ao", False)
q = np.broadcast_to(H("#1F1C1B"), (T, T, 3)) * (0.85 + 0.3 * noise(T, T, 30, seed=3))[..., None]; save(q, "T_Di_Quan", False)
tc = np.broadcast_to(H("#1A1715"), (T, T, 3)) * (0.7 + 0.6 * np.repeat(rng.random((1, T)), T, 0))[..., None]
tc = mix(tc, (0.55, 0.53, 0.50), (rng.random((T, T)) > 0.985) * 0.6); save(tc, "T_Di_Toc", False)   # vài sợi bạc
kh = np.broadcast_to(H("#E8E2D2"), (T, T, 3)).copy(); kh -= (((I % 4) == 0) | ((J % 4) == 0))[..., None] * 0.07   # vải xô thưa
kh -= noise(T, T, 25, seed=4)[..., None] * 0.06; save(kh, "T_Di_Khan", False)
save(np.broadcast_to(H("#6A4A35"), (T, T, 3)) * (0.9 + 0.15 * noise(T, T, 16, seed=5))[..., None], "T_Di_Dep", False)

def matl(name, tex):
    m = bpy.data.materials.new(name); m.use_nodes = True; nt = m.node_tree; b = nt.nodes["Principled BSDF"]
    tn = nt.nodes.new("ShaderNodeTexImage"); tn.image = bpy.data.images.load(f"{TX}/{tex}.png"); tn.interpolation = "Closest"
    nt.links.new(tn.outputs[0], b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.85; return m
for o, (mn, tx) in {"Body": ("M_Di_Da", "T_Di_Da"), "Ao": ("M_Di_Ao", "T_Di_Ao"), "Quan": ("M_Di_Quan", "T_Di_Quan"),
                    "Toc": ("M_Di_Toc", "T_Di_Toc"), "KhanTang": ("M_Di_Khan", "T_Di_Khan"), "Dep": ("M_Di_Dep", "T_Di_Dep")}.items():
    O[o].data.materials.clear(); O[o].data.materials.append(matl(mn, tx))
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b3.blend")
