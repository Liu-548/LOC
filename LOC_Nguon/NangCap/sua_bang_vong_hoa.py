# [29/10] Sửa chữ ngược trên băng vòng hoa VongHoaTang_1: đặt lại UV các mặt vật liệu Bang_Chu_Vong_Hoa theo từng mảnh băng,
# chữ đọc xuôi từ phía mặt đó nhìn vào (u trái→phải theo người xem, v dưới→trên); đuôi băng hẹp: chữ chạy dọc từ trên xuống.
import bpy, bmesh, sys
from mathutils import Vector
src, dst = sys.argv[sys.argv.index('--') + 1:][:2]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)
for o in [o for o in bpy.context.scene.objects if o.type == 'MESH']:
    ids = [i for i, s in enumerate(o.material_slots) if s.material and s.material.name.startswith('Bang_Chu_Vong_Hoa')]
    if not ids: continue
    bm = bmesh.new(); bm.from_mesh(o.data); uvl = bm.loops.layers.uv.active
    fs = [f for f in bm.faces if f.material_index in ids]
    bmesh.ops.remove_doubles(bm, verts=list({v for f in fs for v in f.verts}), dist=1e-4)   # glb tách đỉnh theo tam giác
    fs = [f for f in bm.faces if f.material_index in ids]
    # gom mảnh liền nhau cùng hướng mặt
    seen = set(); islands = []
    for f in fs:
        if f in seen: continue
        st = [f]; isl = []; seen.add(f)
        while st:
            g = st.pop(); isl.append(g)
            for e in g.edges:
                for h in e.link_faces:
                    if h in fs and h not in seen and h.normal.dot(f.normal) > 0.7: seen.add(h); st.append(h)
        islands.append(isl)
    # băng chỉ có một lớp mặt → nhìn phía kia chữ soi gương: nhân đôi mỗi mảnh, lật pháp tuyến, hai lớp cách nhau 1 mm
    for isl in list(islands):
        g = bmesh.ops.duplicate(bm, geom=isl)['geom']
        nf = [x for x in g if isinstance(x, bmesh.types.BMFace)]
        bmesh.ops.reverse_faces(bm, faces=nf)
        for v in [x for x in g if isinstance(x, bmesh.types.BMVert)]: v.co += isl[0].normal * -0.001
        islands.append(nf)
    bm.normal_update()
    for i in ids: o.material_slots[i].material.use_backface_culling = True
    mw = o.matrix_world
    for isl in islands:
        n = sum((mw.to_3x3() @ f.normal for f in isl), Vector()).normalized()
        pts = [mw @ v.co for f in isl for v in f.verts]
        s = -1 if n.y > 0 else 1                # nhìn từ −Y: phải = +x · nhìn từ +Y: phải = −x
        xs = [s * p.x for p in pts]; zs = [p.z for p in pts]
        w, h = max(xs) - min(xs), max(zs) - min(zs)
        for f in isl:
            for l in f.loops:
                p = mw @ l.vert.co; x, z = s * p.x - min(xs), p.z - min(zs)
                l[uvl].uv = (x / w, z / h) if w >= h else (1 - z / h, x / w)
    bm.to_mesh(o.data); bm.free()
bpy.ops.export_scene.gltf(filepath=dst, export_format='GLB', export_apply=True, export_tangents=True, export_yup=True, export_extras=False)
