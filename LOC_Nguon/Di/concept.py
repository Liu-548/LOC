import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, math, numpy as np
from mathutils import Vector
bpy.ops.wm.open_mainfile(filepath="/home/claude/di/out/b2a.blend")
O = bpy.data.objects
def mat(n, c, r=0.7):
    m = bpy.data.materials.new(n); m.use_nodes = True; b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*[x ** 2.2 for x in c], 1); b.inputs["Roughness"].default_value = r; m.diffuse_color = (*[x ** 2.2 for x in c], 1); return m
H = lambda h: tuple(int(h[i:i + 2], 16) / 255 for i in (1, 3, 5))
C = {"da": H("#B5826A"), "ao": H("#5E4636"), "quan": H("#1F1C1B"), "khan": H("#E8E2D2"), "toc": H("#1A1715"), "dep": H("#6A4A35"), "mat": H("#2A1E18")}
for n, k in (("Body", "da"), ("Ao", "ao"), ("Quan", "quan"), ("Eye_L", "mat"), ("Eye_R", "mat"), ("MouthIn", "khan")):
    O[n].data.materials.clear(); O[n].data.materials.append(mat(n, C[k]))
for o in O:
    if o.name == "Body_Full": o.hide_render = True
# khối phác: tóc (chụp đầu + búi thấp), khăn tang (dải quấn trán + hai đuôi sau gáy), dép
arm = O["DiRig"]; hb = arm.data.bones["Head"]
hd = Vector(hb.head_local); top = 1.56
def add(fn, name, col, **kw):
    fn(**kw); o = bpy.context.object; o.name = name; o.data.materials.append(mat(name, col)); return o
cy = -0.035
import geo as G
B = O["Body"]; dom, _, _ = G.dominant(B)
def hairline(co):   # chân tóc: trán 1,512 → thái dương 1,49 → sau gáy 1,405
    a = math.degrees(math.atan2(co.x, -(co.y - cy))); a = abs(a)
    if a < 35: return 1.515
    if a < 80: return 1.515 - (a - 35) / 45 * 0.065
    return 1.45 - (a - 80) / 100 * 0.05
def cap(name, keep, off, col, snap=None):
    o = G.dup(B, name); o.modifiers.clear()
    if o.data.shape_keys: o.shape_key_clear()
    G.keep_faces(o, lambda f: all(dom[v.index] in ("Head", "Neck") and keep(v.co) for v in f.verts))
    if snap: G.snap_boundary(o, snap)
    G.inflate(o, off, smooth=4, rim=0.004); o.data.materials.clear(); o.data.materials.append(mat(name, col))
    for p in o.data.polygons: p.use_smooth = True
    return o
cap("Toc", lambda co: co.z > hairline(co) - 0.004 and not (abs(co.x) > 0.066 and 1.41 < co.z < 1.48 and abs(co.y - cy) < 0.035), 0.013, C["toc"])
def zc(co): return 1.522 - 0.045 * max(0, (co.y - (cy - 0.09))) / 0.18
def band(co): return abs(co.z - zc(co)) < 0.021   # khăn tang: dải ~4 cm quấn trán, nghiêng xuống sau gáy
def bsnap(co):
    z0 = zc(co); return Vector((co.x, co.y, z0 + (0.018 if co.z > z0 else -0.018)))
cap("KhanTang", band, 0.022, C["khan"], bsnap)
bm = add(bpy.ops.mesh.primitive_uv_sphere_add, "Bui", C["toc"], radius=0.042, location=(0, cy + 0.105, 1.43), segments=8, ring_count=5)
bm.scale = (1.2, 0.85, 0.9)
for s in (-1, 1):
    d = add(bpy.ops.mesh.primitive_cube_add, f"DuoiKhan{s}", C["khan"], size=1, location=(0.022 * s, cy + 0.118, 1.36))
    d.scale = (0.034, 0.005, 0.20); d.rotation_euler = (math.radians(-6), math.radians(6 * s), 0)
    f = arm.data.bones["Foot" + ("_L" if s > 0 else "_R")]
    ft = Vector(f.head_local); tt = Vector(arm.data.bones["Toes" + ("_L" if s > 0 else "_R")].tail_local)
    c = (ft + tt) / 2
    dp = add(bpy.ops.mesh.primitive_cube_add, f"Dep{s}", C["dep"], size=1, location=(c.x, c.y - 0.01, 0.006))
    dp.scale = (0.09, 0.25, 0.012)
TRI = {"Body": 700, "Ao": 420, "Quan": 260, "Toc": 120, "KhanTang": 70, "Eye_L": 8, "Eye_R": 8}
if "MouthIn" in O: bpy.data.objects.remove(O["MouthIn"])
tot = 0
for n, t in TRI.items():
    o = O[n]
    for m in list(o.modifiers): o.modifiers.remove(m)
    G.decimate(o, min(1, t / max(G.tris(o), 1)), sym=True)
    for p in o.data.polygons: p.use_smooth = False
    tot += G.tris(o); print("LP", n, G.tris(o))
for o in O:
    if o.type == "MESH" and not o.hide_render:
        for p in o.data.polygons: p.use_smooth = False
print("TONG", tot + 2 * 12 + 40)
sc = bpy.context.scene; sc.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in [e.identifier for e in bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items] else "BLENDER_EEVEE"
sc.render.resolution_x, sc.render.resolution_y = 520, 900; sc.view_settings.view_transform = "Standard"
w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True; w.node_tree.nodes["Background"].inputs[0].default_value = (0.42, 0.41, 0.40, 1); w.node_tree.nodes["Background"].inputs[1].default_value = 0.6
for nm, loc, en in (("K", (1.4, -2.0, 2.2), 160), ("F", (-1.8, -1.0, 1.4), 50), ("R", (0.3, 2.0, 2.0), 90)):
    L = bpy.data.lights.new(nm, "AREA"); L.energy = en; L.size = 2; lo = bpy.data.objects.new(nm, L); sc.collection.objects.link(lo); lo.location = loc
    lo.rotation_euler = (Vector((0, 0, 0.9)) - Vector(loc)).to_track_quat('-Z', 'Y').to_euler()
cam = bpy.data.objects.new("C", bpy.data.cameras.new("C")); sc.collection.objects.link(cam); sc.camera = cam
cam.data.type = "ORTHO"; cam.data.ortho_scale = 1.78
for i, a in enumerate((0, 35, 90, 180)):
    r = math.radians(a); p = Vector((4 * math.sin(r), -4 * math.cos(r), 0.80))
    cam.location = p; cam.rotation_euler = (Vector((0, 0, 0.80)) - p).to_track_quat('-Z', 'Y').to_euler()
    sc.render.filepath = f"/home/claude/di/ref/v{i}.png"; bpy.ops.render.render(write_still=True)
# cận mặt
cam.data.type = "PERSP"; cam.data.lens = 85; p = Vector((0.35, -1.25, 1.47)); cam.location = p
cam.rotation_euler = (Vector((0, -0.05, 1.45)) - p).to_track_quat('-Z', 'Y').to_euler()
sc.render.resolution_x, sc.render.resolution_y = 520, 520; sc.render.filepath = "/home/claude/di/ref/mat.png"; bpy.ops.render.render(write_still=True)
