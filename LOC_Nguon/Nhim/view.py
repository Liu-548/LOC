# tiện ích render nhanh (workbench) nhiều góc rồi ghép ảnh
import bpy, math
from mathutils import Vector
from PIL import Image
def shots(name, views, res=(600,800), color='MATERIAL', bg=(0.08,0.08,0.08), ortho=None):
    sc=bpy.context.scene; sc.render.engine='BLENDER_WORKBENCH'; sc.display.shading.color_type=color; sc.display.shading.light='STUDIO'
    sc.display.shading.show_backface_culling=False
    sc.render.resolution_x,sc.render.resolution_y=res
    if sc.world is None: sc.world=bpy.data.worlds.new("w")
    sc.display.shading.background_type='VIEWPORT'; sc.display.shading.background_color=bg
    cam=bpy.data.objects.get("CamV") or bpy.data.objects.new("CamV",bpy.data.cameras.new("CamV"))
    if cam.name not in sc.collection.objects: sc.collection.objects.link(cam)
    sc.camera=cam; files=[]
    for i,(p,t,*rest) in enumerate(views):
        cam.data.type='ORTHO' if ortho else 'PERSP'
        if ortho: cam.data.ortho_scale=ortho
        cam.data.lens=rest[0] if rest else 50
        cam.location=p; cam.rotation_euler=(Vector(t)-Vector(p)).to_track_quat('-Z','Y').to_euler()
        f=f"/home/claude/nhim/ref/_{name}_{i}.png"; sc.render.filepath=f; bpy.ops.render.render(write_still=True); files.append(f)
    ims=[Image.open(f) for f in files]; W=sum(i.width for i in ims); H=max(i.height for i in ims)
    c=Image.new('RGB',(W,H)); x=0
    for im in ims: c.paste(im,(x,0)); x+=im.width
    c.save(f"/home/claude/nhim/ref/{name}.png"); return f"/home/claude/nhim/ref/{name}.png"
