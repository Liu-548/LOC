# Chạy: python3 run.py <thư mục ra> <module công thức> [tên món ...]
# Mỗi module công thức có dict CT = {tên_món: hàm(ctx)}; hàm dựng các phần bằng lib, trả:
#   None                → gộp mọi phần thành node chính (tên node mesh chính của file gốc)
#   {node: [phần...]}   → các node giữ riêng (pivot = pivot node gốc); phần còn lại vào node chính
import sys, os, json, importlib, math, traceback, time
sys.path.insert(0, os.path.dirname(__file__))
import bpy
from mathutils import Vector, Matrix
import lib

GOC = json.load(open(os.path.join(os.path.dirname(__file__), 'goc_paths.json')))
GIU = json.load(open(os.path.join(os.path.dirname(__file__), 'giu_node.json')))

class Ctx:
    pass

def chay(out, mod, ten):
    lib.reset()
    path = GOC[ten]
    goc = lib.nap_goc(path)
    meshes = [o for o in goc.values() if o.type == 'MESH']
    mn, mx = lib.bbox(meshes)
    c = Ctx(); c.mn, c.mx = mn, mx; c.sz = mx - mn; c.goc = goc; c.ten = ten
    c.cx, c.cy = (mn.x + mx.x) / 2, (mn.y + mx.y) / 2
    c.node = lambda n: goc.get(n)
    # ảnh texture của file gốc: tên vật liệu → đường dẫn png (để dùng lại ảnh in, nhãn, tranh)
    c.anh = {}
    for o in meshes:
        for sl in o.material_slots:
            m = sl.material
            if not m or not m.use_nodes: continue
            for nd in m.node_tree.nodes:
                if nd.type == 'TEX_IMAGE' and nd.image and m.name not in c.anh:
                    p = f"/tmp/locimg/{ten}__{len(c.anh)}.png"; os.makedirs('/tmp/locimg', exist_ok=True)
                    im = nd.image; im.filepath_raw = p; im.file_format = 'PNG'
                    try: im.save(); c.anh[m.name] = p
                    except Exception as e: print('anh loi', e)
    c.anh_list = list(c.anh.values())
    def _con(n):
        o = goc.get(n)
        if o is None: return []
        return [x for x in [o] + list(o.children_recursive) if x.type == 'MESH']
    c.con = _con
    c.bb = lambda n: lib.bbox(_con(n))
    def giu_mat(n, tien_to, ten="giu"):
        # chép nguyên các mặt dùng vật liệu tien_to (ảnh in, chữ…) của node gốc n → phần mới (giữ UV + vật liệu gốc)
        import bmesh
        out = []
        for o in _con(n):
            ids = [i for i, sl in enumerate(o.material_slots) if sl.material and sl.material.name.startswith(tien_to)]
            if not ids: continue
            bm = bmesh.new(); bm.from_mesh(o.data)
            bmesh.ops.delete(bm, geom=[f for f in bm.faces if f.material_index not in ids], context='FACES')
            bm.transform(o.matrix_world)
            p = lib.mesh_bm(bm, ten, None)
            for sl in o.material_slots: p.data.materials.append(sl.material)
            p["uv_giu"] = 1; out.append(p)
        return out
    c.giu_mat = giu_mat
    roots = [o for o in goc.values() if o.parent is None]
    main = max(meshes, key=lambda o: len(o.data.polygons)) if len(meshes) == 1 else None
    # tên node chính: mesh duy nhất, hoặc root
    if main is None:
        main_name = ten if ten in goc else roots[0].name
    else:
        main_name = main.name
    c.main_name = main_name
    res = mod.CT[ten](c) or {}
    # ghi lại thông tin node gốc trước khi xoá
    keepinfo = {}
    for n in list(res.keys()):
        o = goc.get(n)
        keepinfo[n] = (o.matrix_world.copy() if o else Matrix(), o.parent.name if (o and o.parent) else None)
    rest = [p for p in lib._parts if not any(p in v for v in res.values())]
    # xoá gốc
    lib.xoa(goc.values())
    for img in list(bpy.data.images):
        pass
    outs = []
    built = {}
    for n, parts in res.items():
        lib._parts[:] = [p for p in lib._parts if p not in parts]
        o = lib.gop(n, parts)
        mw = keepinfo[n][0]
        o.data.transform(mw.inverted()); o.matrix_world = mw
        built[n] = o
    lib._parts[:] = rest
    if rest:
        o = lib.gop(main_name if main_name not in built else main_name + "_than", rest)
        built[o.name] = o
    # dựng cây: root (nếu gốc có root riêng khác mesh chính và có node giữ) → con
    if len(built) > 1:
        rootname = ten
        r = bpy.data.objects.new(rootname if rootname not in built else rootname + "_goc", None); bpy.context.scene.collection.objects.link(r)
        for n, o in built.items():
            par = keepinfo.get(n, (None, None))[1]
            p = built.get(par) if par in built else r
            if p is o: p = r
            mw = o.matrix_world.copy(); o.parent = p; o.matrix_world = mw
        objs = [r] + list(built.values())
    else:
        objs = list(built.values())
    rel = os.path.relpath(path, '/home/claude/loc')
    dst = os.path.join(out, rel); os.makedirs(os.path.dirname(dst), exist_ok=True)
    # đặt tên vật liệu được code tham chiếu (giữ nếu công thức dùng lại)
    lib.xuat(dst, objs)
    nmn, nmx = lib.bbox([o for o in objs if o.type == 'MESH'])
    d = max(max(abs(nmn[i] - mn[i]), abs(nmx[i] - mx[i])) for i in range(3))
    t = lib.tris(objs)
    os.makedirs(os.path.join(out, '_xem'), exist_ok=True)
    lib.render(os.path.join(out, '_xem', ten), [o for o in objs if o.type == 'MESH'], size=420)
    return dict(ten=ten, tris=t, lech=round(d, 4), goc=[round(v, 3) for v in (mx - mn)], moi=[round(v, 3) for v in (nmx - nmn)], nodes=list(built.keys()))

if __name__ == '__main__':
    a = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else sys.argv[1:]
    out, modname = a[0], a[1]
    mod = importlib.import_module(modname)
    names = a[2:] or list(mod.CT.keys())
    log = []
    for n in names:
        t0 = time.time()
        try:
            r = chay(out, mod, n); r['s'] = round(time.time() - t0, 1); log.append(r); print('OK', json.dumps(r, ensure_ascii=False), flush=True)
        except Exception as e:
            traceback.print_exc(); log.append(dict(ten=n, loi=str(e))); print('LOI', n, e, flush=True)
    with open(os.path.join(out, f'_log_{modname}.json'), 'w') as f: json.dump(log, f, ensure_ascii=False, indent=0)
