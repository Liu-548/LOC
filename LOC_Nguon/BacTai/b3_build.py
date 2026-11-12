# DÌ — Bước 3 (low-poly, đã duyệt 21/10): tóc + búi, khăn tang + 2 đuôi, dép; giảm lưới ~1,6k tam giác, shade phẳng; UV; texture; vật liệu.
# Mắt vẽ trên texture da (không dùng nhãn cầu) — to hơn bản phác theo yêu cầu 21/10.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, math, numpy as np, geo as G, bake
from mathutils import Vector, Matrix
from PIL import Image, ImageFilter
OUT = "/home/claude/bactai/out"; TX = f"{OUT}/tex"; os.makedirs(TX, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b2.blend")
O = bpy.data.objects; arm = O["TaiRig"]; B = O["Body"]
for n in ("Body_Full", "MouthIn", "Eye_L", "Eye_R"): bpy.data.objects.remove(O[n])
dom, _, _ = G.dominant(B)
EYE = arm.data.bones["Eye_L"].head_local; HB = arm.data.bones["Head"]
cy = (HB.head_local.y + HB.tail_local.y) / 2 + 0.005
EX, EZ = abs(EYE.x), EYE.z
TOP = max(v.co.z for v in B.data.vertices)
print("mắt", EX, EZ, "đỉnh", TOP, "cy", cy)

def hairline(co):   # chân tóc đàn ông 54 tuổi: trán hơi hói vào hai bên, mai gọn, gáy cắt ngắn
    a = abs(math.degrees(math.atan2(co.x, -(co.y - cy))))
    f0 = TOP - 0.052
    if a < 22: return f0
    if a < 45: return f0 + 0.008 - (a - 22) / 23 * 0.004          # hai góc trán lùi vào
    if a < 85: return f0 + 0.004 - (a - 45) / 40 * 0.075            # thái dương / mai
    return f0 - 0.071 - (a - 85) / 95 * 0.03                        # sau gáy
def cap(name, keep, off, snap=None):
    o = G.dup(B, name)
    G.keep_faces(o, lambda f: all(dom[v.index] in ("Head", "Neck") and keep(v.co) for v in f.verts))
    if snap: G.snap_boundary(o, snap)
    G.inflate(o, off, smooth=4, rim=0.004)
    for g in list(o.vertex_groups): o.vertex_groups.remove(g)
    o.vertex_groups.new(name="Head").add(list(range(len(o.data.vertices))), 1.0, "REPLACE")
    return o
cap("Toc", lambda co: co.z > hairline(co) - 0.004 and not (abs(co.x) > 0.062 and EZ - 0.055 < co.z < EZ + 0.022 and abs(co.y - cy) < 0.04), 0.008)

def rigid(o, bone, parent=True):
    o.vertex_groups.new(name=bone).add(list(range(len(o.data.vertices))), 1.0, "REPLACE")
    o.parent = arm; m = o.modifiers.new("Armature", "ARMATURE"); m.object = arm
def prim(fn, name, **kw):
    fn(**kw); o = bpy.context.object; o.name = o.data.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True); return o
def placed(fn, name, scale, rot=(0, 0, 0), **kw):
    fn(**kw); o = bpy.context.object; o.name = o.data.name = name; o.scale = scale; o.rotation_euler = rot
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True); return o
for s in (-1, 1):
    sd = "_L" if s > 0 else "_R"
    f = arm.data.bones["Foot" + sd]; c = (Vector(f.head_local) + Vector(arm.data.bones["Toes" + sd].tail_local)) / 2
    rigid(placed(bpy.ops.mesh.primitive_cube_add, "Dep" + sd, (0.10, 0.27, 0.014), size=1, location=(c.x, c.y - 0.01, 0.007)), "Foot" + sd)
# gộp đuôi khăn vào khăn, dép trái+phải thành một, búi vào tóc (ít object = ít draw call)
print("OBJ", [o.name for o in O])
def join(dst, srcs):
    bpy.ops.object.select_all(action="DESELECT")
    for s in srcs: O[s].select_set(True)
    O[dst].select_set(True); bpy.context.view_layer.objects.active = O[dst]
    with bpy.context.temp_override(active_object=O[dst], selected_editable_objects=[O[dst]] + [O[s] for s in srcs]): bpy.ops.object.join()
    print("JOIN", dst, [o.name for o in O])
join("Dep_L", ["Dep_R"]); d_ = O["Dep_L"]; d_.name = d_.data.name = "Dep"

TRI = {"Body": 950, "Ao": 460, "Quan": 280, "Toc": 120}
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
for n in ("Ao", "Quan", "Toc", "Dep"): smart(O[n], 0.25)

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

# ── DA + MẶT (nướng vị trí theo UV MakeHuman, 512) — đàn ông 54 tuổi, da rám nắng, râu lún phún, nếp nhăn trán
S = 512
Bk = bake.bake_maps(B, S); p, n_ = Bk["pos"], Bk["nrm"]; x, y, z = p[..., 0], p[..., 1], p[..., 2]
c = np.broadcast_to(H("#B07D60"), p.shape).copy() * (0.93 + 0.12 * noise(S, S, 40, seed=1))[..., None]
front = (y < cy - 0.04) & (z > EZ - 0.12)
for s_ in (1, -1):
    u = (x - EX * s_) / 0.0135; v = (z - EZ) / 0.0058
    eye = (u ** 2 + v ** 2 < 1) & front
    c = mix(c, (0.86, 0.82, 0.76), eye)
    iris = ((x - EX * s_) ** 2 + (z - EZ) ** 2 < 0.0060 ** 2) & eye
    c = mix(c, (0.16, 0.10, 0.07), iris)
    c = mix(c, (0.02, 0.02, 0.02), ((x - EX * s_) ** 2 + (z - EZ) ** 2 < 0.0036 ** 2) & eye)
    c = mix(c, (1, 1, 1), ((x - EX * s_ - 0.003 * s_) ** 2 + (z - EZ - 0.0028) ** 2 < 0.0014 ** 2) & eye)
    lid = (np.abs(u) < 1.12) & front & (np.abs(z - (EZ + 0.0058 * np.sqrt(np.clip(1 - u ** 2, 0, 1)))) < 0.0016)
    c = mix(c, (0.12, 0.08, 0.06), lid)
    c = mix(c, (0.50, 0.34, 0.28), gauss(p, (EX * s_, cy - 0.08, EZ - 0.014), (0.016, 0.03, 0.004)) * front * 0.5)   # bọng mắt, thức khuya chạy chuyến cuối
    c = mix(c, (0.48, 0.32, 0.25), ((np.abs(z - (EZ - 0.006 - 0.004 * np.abs(u))) < 0.0009) & (np.abs(u) > 0.9) & (np.abs(u) < 1.6) & front) * 0.8)   # vết chân chim
    t = (s_ * x - 0.008) / 0.044                                                                                # lông mày rậm, hơi xếch
    zc_ = EZ + 0.024 + 0.003 * np.sin(np.pi * np.clip(t, 0, 1)) + 0.002 * t
    c = mix(c, (0.09, 0.07, 0.06), (t > 0) & (t < 1) & (np.abs(z - zc_) < 0.0036 * (1 - 0.35 * np.clip(t, 0, 1))) & front)
MZ = EZ - 0.071
lip = ((x / 0.024) ** 2 + ((z - MZ) / 0.0060) ** 2 < 1) & front
c = mix(c, (0.52, 0.32, 0.29), lip)
c = mix(c, (0.24, 0.13, 0.12), (np.abs(z - MZ) < 0.0011) & (np.abs(x) < 0.023) & front)
c = mix(c, (0.56, 0.38, 0.30), gauss(p, (0, cy - 0.10, EZ - 0.04), (0.009, 0.03, 0.01)) * front * 0.3)        # bóng mũi
for s_ in (1, -1):   # rãnh mũi–má
    t = np.clip((EZ - 0.045 - z) / 0.035, 0, 1)
    c = mix(c, (0.46, 0.30, 0.23), ((np.abs(s_ * x - (0.022 + 0.012 * t)) < 0.0013) & (z < EZ - 0.045) & (z > EZ - 0.082) & front) * 0.75)
for k in range(3):   # nếp nhăn trán
    zz = EZ + 0.045 + k * 0.011
    c = mix(c, (0.50, 0.33, 0.26), ((np.abs(z - zz - 0.002 * np.cos(x * 40)) < 0.0008) & (np.abs(x) < 0.035) & front) * 0.6)
# râu lún phún (cằm, mép, hàm) — xám xanh
stub = (((z < MZ + 0.012) & (z > MZ - 0.075) & (np.abs(x) < 0.06)) | ((np.abs(z - (MZ + 0.009)) < 0.006) & (np.abs(x) < 0.028))) & (y < cy + 0.02)
stub &= ~lip
c = mix(c, (0.30, 0.27, 0.26), stub * (0.18 + 0.22 * noise(S, S, 6, seed=9)))
# da đầu dưới tóc → màu tóc
ang = np.abs(np.degrees(np.arctan2(x, -(y - cy))))
f0 = TOP - 0.052
hl = np.where(ang < 22, f0, np.where(ang < 45, f0 + 0.008 - (ang - 22) / 23 * 0.004, np.where(ang < 85, f0 + 0.004 - (ang - 45) / 40 * 0.075, f0 - 0.071 - (ang - 85) / 95 * 0.03)))
ear = (np.abs(x) > 0.062) & (z > EZ - 0.055) & (z < EZ + 0.022) & (np.abs(y - cy) < 0.04)
sc_ = np.asarray(Image.fromarray((((z > hl - 0.010) & ~ear & (z > EZ - 0.08)) * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(2)), float) / 255
c = mix(c, H("#2A2622"), sc_)
save(c, "T_Tai_Da")

# ── áo sơ mi đồng phục xanh xám (sợi kẻ mờ, sờn), quần âu sẫm, tóc muối tiêu, dép nhựa
T = 128; I, J = np.indices((T, T))
ao = np.broadcast_to(H("#8797A1"), (T, T, 3)) * (0.88 + 0.16 * noise(T, T, 18, seed=2))[..., None]
ao -= (((I % 8) == 0) * 0.025)[..., None]                       # đường dệt
ao = mix(ao, H("#6C7A83"), (noise(T, T, 40, seed=7) > 0.66) * 0.35)        # sờn nhẹ
save(ao, "T_Tai_Ao", False)
q = np.broadcast_to(H("#2B2A2C"), (T, T, 3)) * (0.85 + 0.25 * noise(T, T, 30, seed=3))[..., None]; save(q, "T_Tai_Quan", False)
tc = np.broadcast_to(H("#1E1B19"), (T, T, 3)) * (0.75 + 0.5 * np.repeat(rng.random((1, T)), T, 0))[..., None]
tc = mix(tc, (0.58, 0.56, 0.53), (rng.random((T, T)) > 0.93) * 0.8); save(tc, "T_Tai_Toc", False)   # muối tiêu
save(np.broadcast_to(H("#3B3631"), (T, T, 3)) * (0.9 + 0.15 * noise(T, T, 16, seed=5))[..., None], "T_Tai_Dep", False)

def matl(name, tex):
    m = bpy.data.materials.new(name); m.use_nodes = True; nt = m.node_tree; b = nt.nodes["Principled BSDF"]
    tn = nt.nodes.new("ShaderNodeTexImage"); tn.image = bpy.data.images.load(f"{TX}/{tex}.png"); tn.interpolation = "Closest"
    nt.links.new(tn.outputs[0], b.inputs["Base Color"]); b.inputs["Roughness"].default_value = 0.85; return m
for o, (mn, tx) in {"Body": ("M_Tai_Da", "T_Tai_Da"), "Ao": ("M_Tai_Ao", "T_Tai_Ao"), "Quan": ("M_Tai_Quan", "T_Tai_Quan"),
                    "Toc": ("M_Tai_Toc", "T_Tai_Toc"), "Dep": ("M_Tai_Dep", "T_Tai_Dep")}.items():
    O[o].data.materials.clear(); O[o].data.materials.append(matl(mn, tx))
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b3.blend")
