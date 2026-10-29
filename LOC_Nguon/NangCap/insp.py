# Xem nhanh glb gốc: cây node, bbox, ảnh workbench 2 góc
import bpy, sys, os, math, json
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:]
out=args[0]; files=args[1:]
os.makedirs(out,exist_ok=True)
info={}
def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)
for f in files:
    reset()
    bpy.ops.import_scene.gltf(filepath=f)
    objs=[o for o in bpy.context.scene.objects]
    ms=[o for o in objs if o.type=='MESH']
    if not ms: continue
    pts=[o.matrix_world@Vector(c) for o in ms for c in o.bound_box]
    mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts))); mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
    name=os.path.splitext(os.path.basename(f))[0]
    info[name]={'min':list(mn),'max':list(mx),'nodes':[(o.name,o.type,o.parent.name if o.parent else None, len(o.data.polygons) if o.type=='MESH' else 0) for o in objs],
                'mats':sorted({s.material.name for o in ms for s in o.material_slots if s.material})}
    sc=bpy.context.scene; sc.render.engine='BLENDER_WORKBENCH'; sc.display.shading.light='STUDIO'; sc.display.shading.color_type='MATERIAL'
    sc.display.shading.show_shadows=False; sc.display.shading.show_cavity=True
    sc.render.resolution_x=sc.render.resolution_y=360; sc.render.film_transparent=False
    sc.world=bpy.data.worlds.new('w') if not sc.world else sc.world
    c=(mn+mx)/2; r=max((mx-mn).length,0.05)
    cam=bpy.data.objects.new('cam',bpy.data.cameras.new('cam')); sc.collection.objects.link(cam); sc.camera=cam
    cam.data.type='ORTHO'; cam.data.ortho_scale=r*1.1
    for i,(az,el) in enumerate([(-90-30,20),(90-30,25)]):  # nhìn từ -Y (trước) lệch 30°, và từ sau
        a=math.radians(az); e=math.radians(el)
        d=Vector((math.cos(a)*math.cos(e), math.sin(a)*math.cos(e), math.sin(e)))
        cam.location=c+d*r*3; cam.rotation_euler=(-d).to_track_quat('-Z','Y').to_euler()
        sc.render.filepath=f"{out}/{name}_{i}.png"; bpy.ops.render.render(write_still=True)
json.dump(info,open(f"{out}/info_{os.getpid()}.json",'w'),indent=0)
