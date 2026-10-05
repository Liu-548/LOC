import bpy, math, sys
from mathutils import Vector
OUT = "/home/claude/hu/out"
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/HuCot.blend")
tex = {"Hu_Men": "Hu_Men.png", "Hu_DatNung": "Hu_DatNung.png", "Hu_GiayDau": "Hu_GiayDau.png", "Hu_Niem": "Hu_Niem.png", "Hu_Lat": "Hu_Lat.png"}
for m in bpy.data.materials:
    m.use_nodes = True; nt = m.node_tree; b = nt.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = m.diffuse_color
    if m.name in tex:
        t = nt.nodes.new("ShaderNodeTexImage"); t.image = bpy.data.images.load(f"{OUT}/{tex[m.name]}")
        nt.links.new(t.outputs[0], b.inputs["Base Color"])
    b.inputs["Roughness"].default_value = 0.35 if m.name == "Hu_Men" else 0.8
    if m.name == "Hu_Toc": b.inputs["Roughness"].default_value = 0.45
sc = bpy.context.scene; sc.render.engine = "CYCLES"; sc.cycles.samples = 48; sc.cycles.device = "CPU"
sc.render.resolution_x, sc.render.resolution_y = 640, 640
w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True; w.node_tree.nodes["Background"].inputs[1].default_value = 0.25
L = bpy.data.lights.new("L", "AREA"); L.energy = 40; L.size = 0.5; lo = bpy.data.objects.new("L", L); sc.collection.objects.link(lo)
lo.location = (0.4, -0.6, 0.9); lo.rotation_euler = (math.radians(45), 0, math.radians(30))
cam = bpy.data.objects.new("C", bpy.data.cameras.new("C")); sc.collection.objects.link(cam); sc.camera = cam
cam.data.lens = 50
def look(p, t):
    cam.location = p; d = Vector(t) - Vector(p); cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
goc = bpy.data.objects["Hu_GiayDau_Goc"]
for name, ang, p, t in [("gan", -155, (0.0, -0.26, 0.6), (0, -0.03, 0.285)), ("dong", 0, (0, -0.75, 0.42), (0, 0, 0.16)), ("mo", -155, (0.15, -0.42, 0.6), (0, -0.01, 0.25)), ("mo_xa", -78, (0.25, -0.6, 0.5), (0, 0, 0.2)), ("rach", -8, (0.05, -0.38, 0.5), (0, 0, 0.27))]:
    goc.rotation_euler = (math.radians(ang), 0, 0)
    bpy.data.objects["Hu_Niem_Goc"].hide_render = (name == "rach") and False
    look(p, t); sc.render.filepath = f"/tmp/hu_{name}.png"; bpy.ops.render.render(write_still=True)
