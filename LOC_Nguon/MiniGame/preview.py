# Xem trước từng model MG_* (Cycles CPU), tự canh khung, ghép thành bảng XemTruoc_MG.png
import bpy, sys, math, os
from mathutils import Vector
OUT = sys.argv[1]; HERE = os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/MG_SapXep.blend")
sc = bpy.context.scene
mat = bpy.data.materials["MG_Atlas"]; mat.use_nodes = True
nt = mat.node_tree; bsdf = nt.nodes["Principled BSDF"]; bsdf.inputs["Roughness"].default_value = 0.7
tx = nt.nodes.new("ShaderNodeTexImage"); tx.image = bpy.data.images.load(f"{HERE}/T_MG_Atlas.png"); tx.interpolation = "Closest"
nt.links.new(tx.outputs["Color"], bsdf.inputs["Base Color"])
sc.render.engine = "CYCLES"; sc.cycles.samples = 16; sc.cycles.device = "CPU"; sc.render.resolution_x = sc.render.resolution_y = 360
w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True; w.node_tree.nodes["Background"].inputs[0].default_value = (0.3, 0.3, 0.33, 1)
ld = bpy.data.lights.new("sun", "SUN"); ld.energy = 3.0; lo = bpy.data.objects.new("sun", ld); sc.collection.objects.link(lo); lo.rotation_euler = (math.radians(45), 0, math.radians(-25))
cam = bpy.data.cameras.new("c"); co = bpy.data.objects.new("c", cam); sc.collection.objects.link(co); sc.camera = co; cam.lens = 50; cam.clip_start = 0.005
ms = [o for o in bpy.data.objects if o.type == "MESH"]
os.makedirs(f"{OUT}/xem", exist_ok=True)
for o in ms:
    for x in ms: x.hide_render = x is not o
    o.location = (0, 0, 0)
    bb = [o.matrix_world @ Vector(c) for c in o.bound_box]; c = sum(bb, Vector()) / 8; r = max((v - c).length for v in bb)
    huong = Vector((0, -1, -0.12)) if o.name.endswith("Phoi") else Vector((0.35, -0.75, 1.0))
    co.location = c + huong.normalized() * r * 3.0; co.rotation_euler = (c - co.location).to_track_quat("-Z", "Y").to_euler()
    sc.render.filepath = f"{OUT}/xem/{o.name}.png"; bpy.ops.render.render(write_still=True)
