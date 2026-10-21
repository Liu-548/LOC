import bpy, math, sys
from mathutils import Vector
bpy.ops.wm.open_mainfile(filepath=sys.argv[-2])
sc = bpy.context.scene; sc.render.engine = "BLENDER_EEVEE"; sc.view_settings.view_transform = "Standard"
w = bpy.data.worlds.new("w"); sc.world = w; w.use_nodes = True; w.node_tree.nodes["Background"].inputs[0].default_value = (0.42, 0.41, 0.40, 1); w.node_tree.nodes["Background"].inputs[1].default_value = 0.7
for nm, loc, en in (("K", (1.4, -2.0, 2.2), 160), ("F", (-1.8, -1.0, 1.4), 60), ("R", (0.3, 2.0, 2.0), 90)):
    L = bpy.data.lights.new(nm, "AREA"); L.energy = en; L.size = 2; lo = bpy.data.objects.new(nm, L); sc.collection.objects.link(lo); lo.location = loc
    lo.rotation_euler = (Vector((0, 0, 0.9)) - Vector(loc)).to_track_quat('-Z', 'Y').to_euler()
cam = bpy.data.objects.new("C", bpy.data.cameras.new("C")); sc.collection.objects.link(cam); sc.camera = cam
fr = int(sys.argv[-1])
sc.frame_set(fr)
shots = [((0.0, -4, 0.8), (0, 0, 0.8), "ORTHO", 1.78, (520, 900)), ((2.8, -2.8, 0.8), (0, 0, 0.8), "ORTHO", 1.78, (520, 900)),
         ((2.8 * 1.414, 0, 0.8), (0, 0, 0.8), "ORTHO", 1.78, (520, 900)), ((0.30, -1.0, 1.46), (0, -0.05, 1.44), "PERSP", 85, (520, 900))]
for i, (p, t, ty, s, res) in enumerate(shots):
    sc.render.resolution_x, sc.render.resolution_y = res
    cam.data.type = ty
    if ty == "ORTHO": cam.data.ortho_scale = s
    else: cam.data.lens = s
    cam.location = p; cam.rotation_euler = (Vector(t) - Vector(p)).to_track_quat('-Z', 'Y').to_euler()
    sc.render.filepath = f"/home/claude/di/ref/p{i}.png"; bpy.ops.render.render(write_still=True)
