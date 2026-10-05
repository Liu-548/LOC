# ảnh xem trước có texture (Cycles)
import sys; sys.path.insert(0,'/home/claude/nhim')
import bpy, math, json, rig, poses
from mathutils import Vector
from PIL import Image
def setup(res=(600,700), samples=24):
    sc=bpy.context.scene; sc.render.engine='CYCLES'; sc.cycles.samples=samples; sc.cycles.device='CPU'; sc.cycles.use_denoising=True
    sc.render.resolution_x,sc.render.resolution_y=res; sc.view_settings.view_transform='Standard'
    w=bpy.data.worlds.new("w"); sc.world=w; w.use_nodes=True; w.node_tree.nodes["Background"].inputs[0].default_value=(0.18,0.17,0.16,1); w.node_tree.nodes["Background"].inputs[1].default_value=0.35
    for nm,loc,en,sz in (("K",(1.2,-1.5,1.8),70,1.2),("F",(-1.5,-0.8,1.2),25,1.5),("R",(0.3,1.6,1.6),40,1.0)):
        L=bpy.data.lights.new(nm,"AREA"); L.energy=en; L.size=sz; lo=bpy.data.objects.new(nm,L); sc.collection.objects.link(lo); lo.location=loc
        lo.rotation_euler=(Vector((0,0,0.8))-Vector(loc)).to_track_quat('-Z','Y').to_euler()
    cam=bpy.data.objects.new("C",bpy.data.cameras.new("C")); sc.collection.objects.link(cam); sc.camera=cam
    return cam
def shot(cam,p,t,f,lens=50):
    cam.data.lens=lens; cam.location=p; cam.rotation_euler=(Vector(t)-Vector(p)).to_track_quat('-Z','Y').to_euler()
    bpy.context.scene.render.filepath=f; bpy.ops.render.render(write_still=True); return f
def strip(fs,out):
    ims=[Image.open(f) for f in fs]; W=sum(i.width for i in ims); H=max(i.height for i in ims); c=Image.new('RGB',(W,H)); x=0
    for im in ims: c.paste(im,(x,0)); x+=im.width
    c.save(out); return out
