# Nướng bản đồ vị trí / pháp tuyến theo UV (Cycles) → vẽ texture bằng numpy theo toạ độ 3D.
import bpy, numpy as np

def bake_maps(obj, size=1024, uv="UV", margin=6):
    """trả về pos (H,W,3), nrm (H,W,3), cov (H,W) — hàng 0 = đáy ảnh (theo Blender)"""
    sc = bpy.context.scene; sc.render.engine = "CYCLES"; sc.cycles.samples = 1; sc.cycles.device = "CPU"
    out = {}
    old = list(obj.data.materials)
    obj.data.materials.clear()
    mat = bpy.data.materials.new("_bake"); mat.use_nodes = True; nt = mat.node_tree
    for n in list(nt.nodes): nt.nodes.remove(n)
    geo = nt.nodes.new("ShaderNodeNewGeometry"); em = nt.nodes.new("ShaderNodeEmission"); o = nt.nodes.new("ShaderNodeOutputMaterial")
    img_n = nt.nodes.new("ShaderNodeTexImage")
    nt.links.new(em.outputs[0], o.inputs[0])
    obj.data.materials.append(mat)
    if obj.data.uv_layers.get(uv): obj.data.uv_layers.active = obj.data.uv_layers[uv]
    for key, sock in (("pos", "Position"), ("nrm", "Normal")):
        img = bpy.data.images.new(f"_b_{key}", size, size, float_buffer=True, alpha=True)
        img.colorspace_settings.name = "Non-Color"
        img.generated_color = (0, 0, 0, 0)
        img_n.image = img; nt.nodes.active = img_n
        for l in list(nt.links):
            if l.to_node == em: nt.links.remove(l)
        nt.links.new(geo.outputs[sock], em.inputs[0])
        bpy.context.view_layer.objects.active = obj
        for x in bpy.context.view_layer.objects: x.select_set(x == obj)
        sc.render.bake.margin = margin
        sc.render.bake.use_clear = True
        bpy.ops.object.bake(type="EMIT", margin=margin)
        a = np.array(img.pixels[:]).reshape(size, size, 4)
        out[key] = a[..., :3]; out["cov"] = a[..., 3]
    obj.data.materials.clear()
    for m in old: obj.data.materials.append(m)
    # Position của Geometry là toạ độ thế giới → đổi về cục bộ
    M = np.array(obj.matrix_world.inverted())
    p = out["pos"].reshape(-1, 3) @ M[:3, :3].T + M[:3, 3]; out["pos"] = p.reshape(size, size, 3)
    return out
