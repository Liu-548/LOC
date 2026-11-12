# xem trước: python3 prev.py file.blend out.png [che_do]  (che_do: dung | mat | ngoi)
import bpy, sys, math
from mathutils import Vector
f, out = sys.argv[1], sys.argv[2]; mode = sys.argv[3] if len(sys.argv) > 3 else "dung"
bpy.ops.wm.open_mainfile(filepath=f)
sc = bpy.context.scene; sc.render.engine = "BLENDER_WORKBENCH"
sh = sc.display.shading; sh.light = "STUDIO"; sh.color_type = "TEXTURE"
sc.render.resolution_x, sc.render.resolution_y = (900, 900)
for o in bpy.data.objects:
    if o.type == "MESH" and o.hide_get(): o.hide_render = True
cam = bpy.data.cameras.new("C"); co = bpy.data.objects.new("C", cam); sc.collection.objects.link(co); sc.camera = co
views = {"dung": [((0, -4.2, 0.9), (0, 0, 0.85), 30), ((3.0, -3.0, 0.95), (0, 0, 0.85), 30)],
         "mat": [((0, -0.9, 1.5), (0, 0, 1.47), 20), ((0.6, -0.6, 1.5), (0, 0, 1.47), 20)],
         "ngoi": [((0, -2.6, 0.5), (0, -0.2, 0.2), 35), ((2.4, -0.6, 0.35), (0, -0.2, 0.15), 35), ((0.9, 1.9, 0.9), (0, -0.2, 0.35), 35), ((0.35, -0.95, 0.75), (0, 0, 0.62), 32)]}
import os
for i, (p, t, fov) in enumerate(views[mode]):
    co.location = p; d = Vector(t) - Vector(p); co.rotation_euler = d.to_track_quat("-Z", "Y").to_euler(); cam.angle = math.radians(fov)
    sc.render.filepath = out.replace(".png", f"_{i}.png"); bpy.ops.render.render(write_still=True)
