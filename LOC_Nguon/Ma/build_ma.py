# Dựng 6 tư thế con ma LỘC bằng Skin modifier (bpy), xuất FBX cho Unity.
# Toạ độ Blender: Z lên, mặt trước = -Y, bên trái của ma = +X. Đơn vị mét, gốc ở sàn.
# Mỗi file: thân "Ma_<Ten>" (một mesh) + con "Dau" (gốc ở đỉnh cổ) + cháu "Mat_L", "Mat_R".
import bpy, bmesh, math, sys, os
from mathutils import Vector, Matrix

OUT = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else "/home/claude/ma/out"
os.makedirs(OUT, exist_ok=True)

V = Vector

def finger_chain(palm, fd, cv, spread, n=4, seg=(0.09, 0.08), r=(0.011, 0.008, 0.006), gap=0.022):
    """4 ngón dài: mỗi ngón gốc -> khớp (theo fd) -> đầu ngón (theo cv)."""
    chains = []
    for i in range(n):
        o = (i - (n - 1) / 2) * gap
        base = palm + spread * o + fd * 0.02
        mid = base + fd * seg[0]
        tip = mid + cv * seg[1]
        chains.append([(base, r[0]), (mid, r[1]), (tip, r[2])])
    return chains


def body(P):
    """P: dict khớp -> trả (verts[(co, r)], edges[(i,j)])"""
    verts, edges = [], []
    def add(co, r):
        verts.append((V(co), r)); return len(verts) - 1
    def chain(pts, start=None):
        prev = start
        for co, r in pts:
            i = add(co, r)
            if prev is not None: edges.append((prev, i))
            prev = i
        return prev
    pel = add(P["pelvis"], 0.13)
    bung = add((V(P["pelvis"]) + V(P["spine"])) / 2, 0.105); edges.append((pel, bung))
    spine = chain([(P["spine"], 0.1), (P["chest"], 0.12)], bung)
    chest = spine
    chain([(P["nb"], 0.066), (P["nt"], 0.052)], chest)   # cổ dày hơn, nối liền vào đầu
    for s in ("L", "R"):
        if ("sh" + s) not in P: continue
        wr = chain([(P["sh" + s], 0.05), (P["el" + s], 0.034), (P["wr" + s], 0.026), (P["palm" + s], 0.03)], chest)
        for fc in finger_chain(V(P["palm" + s]), V(P["fd" + s]).normalized(), V(P["cv" + s]).normalized(), V(P.get("sp" + s, (1, 0, 0))).normalized()):
            chain(fc, wr)
        hip = V(P["hip" + s]); hip = V(P["pelvis"]) + (hip - V(P["pelvis"])) * 0.8   # khớp hông kéo sát vào chậu
        chain([(hip, 0.088), (hip.lerp(V(P["knee" + s]), 0.5), 0.07), (P["knee" + s], 0.056), (P["ank" + s], 0.036), (P["toe" + s], 0.025)], pel)
    return verts, edges


def make_skin(name, P):
    verts, edges = body(P)
    me = bpy.data.meshes.new(name)
    me.from_pydata([v[0] for v in verts], edges, [])
    me.update()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob)
    bpy.context.view_layer.objects.active = ob
    ob.select_set(True)
    sk = ob.modifiers.new("Skin", "SKIN")
    sv = me.skin_vertices[0].data
    for i, (_, r) in enumerate(verts):
        sv[i].radius = (r, r)
    sv[0].use_root = True
    sub = ob.modifiers.new("Sub", "SUBSURF"); sub.levels = 1; sub.render_levels = 1
    bpy.ops.object.convert(target="MESH")
    bm = bmesh.new(); bm.from_mesh(ob.data); bmesh.ops.recalc_face_normals(bm, faces=bm.faces); bm.to_mesh(ob.data); bm.free()
    for p in ob.data.polygons: p.use_smooth = False
    ob.select_set(False)
    return ob


def make_head(parent, nt, look, tilt_deg=0.0, scale=(0.095, 0.11, 0.135)):
    """Đầu hình trứng dài, gốc tại đỉnh cổ nt, nhìn theo hướng look (mặc định -Y), nghiêng tilt quanh hướng nhìn."""
    bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=8, radius=1.0)
    h = bpy.context.active_object; h.name = "Dau"
    # dựng đầu ở hệ cục bộ: gốc = đỉnh cổ, mặt nhìn -Y
    for v in h.data.vertices:
        v.co = V((v.co.x * scale[0], v.co.y * scale[1] - 0.015, v.co.z * scale[2] + 0.12))
        if v.co.z < 0.07: v.co.y += 0.02   # cằm hơi lùi, hàm dài
    # cổ thuộc khối đầu: trụ vát từ trong cổ thân (z −0.08) lên lọt vào đáy sọ (z +0.10) → xoay đầu không hở khớp
    bm = bmesh.new(); bm.from_mesh(h.data)
    bmesh.ops.create_cone(bm, cap_ends=True, segments=10, radius1=0.054, radius2=0.062, depth=0.18,
                          matrix=Matrix.Translation(V((0, 0.0, 0.01))))
    # gáy + hàm: khối trứng nhỏ đắp vào chỗ cổ chạm sọ cho liền mạch
    bmesh.ops.create_uvsphere(bm, u_segments=10, v_segments=6, radius=1.0,
                              matrix=Matrix.Translation(V((0, 0.01, 0.075))) @ Matrix.Diagonal(V((0.075, 0.08, 0.065, 1))))
    bm.to_mesh(h.data); bm.free()
    for p in h.data.polygons: p.use_smooth = False
    eyes = []
    for s, nm in ((1, "Mat_L"), (-1, "Mat_R")):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=8, ring_count=5, radius=1.0)
        e = bpy.context.active_object; e.name = nm
        for v in e.data.vertices:
            v.co = V((v.co.x * 0.021, v.co.y * 0.008, v.co.z * 0.011))
        e.location = V((s * 0.037, -0.112, 0.145))
        e.rotation_euler = (0, s * math.radians(-12), 0)   # mắt xếch xuống phía ngoài
        e.parent = h
        eyes.append(e)
    # xoay toàn khối đầu tới hướng look + nghiêng
    look = V(look).normalized()
    rot = (-V((0, 1, 0))).rotation_difference(look).to_matrix().to_4x4()
    tilt = Matrix.Rotation(math.radians(tilt_deg), 4, look)
    h.matrix_world = Matrix.Translation(V(nt)) @ tilt @ rot
    h.parent = parent
    h.matrix_parent_inverse = parent.matrix_world.inverted()
    return h


def mirror(P):
    """Sinh khớp bên phải từ bên trái (đối xứng X) cho khớp còn thiếu."""
    out = dict(P)
    for k, v in P.items():
        if k.endswith("L"):
            kr = k[:-1] + "R"
            if kr not in out:
                out[kr] = (-v[0], v[1], v[2]) if k[:-1] not in ("fd", "cv", "sp") else (-v[0], v[1], v[2])
    return out

LEGS = dict(hipL=(0.09, 0, 0.95), kneeL=(0.10, -0.02, 0.52), ankL=(0.10, 0.02, 0.08), toeL=(0.10, -0.13, 0.02))

POSES = {}

# 1. LoDau — đứng nép sau mép tường, người nghiêng về bên trái (+X), đầu ló ra ở x ≈ 0.33
POSES["LoDau"] = dict(P=mirror(dict(
    pelvis=(0, 0, 0.98), spine=(0.03, 0, 1.20), chest=(0.08, 0, 1.42), nb=(0.15, -0.01, 1.58), nt=(0.22, -0.02, 1.64),
    shL=(0.22, 0.03, 1.46), elL=(0.20, 0.14, 1.15), wrL=(0.14, 0.12, 0.82), palmL=(0.13, 0.12, 0.75), fdL=(0, 0, -1), cvL=(0, 0.4, -1), spL=(0, 1, 0),
    shR=(-0.12, 0, 1.50), elR=(-0.17, 0.03, 1.13), wrR=(-0.19, 0.01, 0.77), palmR=(-0.19, 0, 0.70), fdR=(0, 0, -1), cvR=(0, 0.4, -1), spR=(0, 1, 0),
    **LEGS)), nt=(0.22, -0.02, 1.64), look=(0.25, -1, 0), tilt=-28)

# 2. Tay — đứng sau tường, tay trái vươn ra bám mép: đốt ngón vòng qua mép (x 0.33) rồi áp lên mặt tường phía người chơi (y −0.17)
POSES["Tay"] = dict(P=mirror(dict(
    pelvis=(0, 0.05, 0.98), spine=(0, 0.05, 1.20), chest=(0.02, 0.04, 1.42), nb=(0.03, 0.03, 1.58), nt=(0.04, 0.02, 1.66),
    shL=(0.20, 0.03, 1.50), elL=(0.26, 0.10, 1.36), wrL=(0.33, 0.0, 1.47), palmL=(0.36, -0.07, 1.47), fdL=(0, -1, 0), cvL=(-1, 0, 0), spL=(0, 0, 1),
    shR=(-0.18, 0.04, 1.50), elR=(-0.22, 0.07, 1.13), wrR=(-0.24, 0.05, 0.77), palmR=(-0.24, 0.05, 0.70), fdR=(0, 0, -1), cvR=(0, 0.4, -1), spR=(0, 1, 0),
    **LEGS)), nt=(0.04, 0.02, 1.66), look=(0, -1, 0), tilt=0)

# 3. LanCan — đứng sau lan can (mép lan can ở y −0.18, cao 1.0), gập người qua, đầu thõng xuống nhìn người dưới, hai tay buông thõng
POSES["LanCan"] = dict(P=mirror(dict(
    pelvis=(0, 0.10, 0.98), spine=(0, -0.05, 1.12), chest=(0, -0.24, 1.16), nb=(0, -0.40, 1.10), nt=(0, -0.47, 1.02),
    shL=(0.17, -0.30, 1.15), elL=(0.22, -0.40, 0.86), wrL=(0.20, -0.38, 0.55), palmL=(0.20, -0.38, 0.48), fdL=(0, 0, -1), cvL=(0, 0.3, -1), spL=(0, 1, 0),
    hipL=(0.09, 0.10, 0.95), kneeL=(0.10, 0.08, 0.52), ankL=(0.10, 0.12, 0.08), toeL=(0.10, -0.02, 0.02))),
    nt=(0, -0.47, 1.02), look=(0, -0.6, -1), tilt=20)

# 4. BoTran — dựng như bò trên sàn (Unity lật ngược lên trần): thân ngang cao 0.42, khuỷu và gối nhô cao kiểu nhện
POSES["BoTran"] = dict(P=mirror(dict(
    pelvis=(0, 0.35, 0.42), spine=(0, 0.12, 0.44), chest=(0, -0.12, 0.46), nb=(0, -0.28, 0.50), nt=(0, -0.36, 0.52),
    shL=(0.18, -0.15, 0.48), elL=(0.52, -0.25, 0.78), wrL=(0.62, -0.55, 0.12), palmL=(0.62, -0.62, 0.04), fdL=(0.2, -1, 0), cvL=(0.2, -1, -0.6), spL=(1, 0, 0),
    hipL=(0.10, 0.36, 0.42), kneeL=(0.55, 0.40, 0.80), ankL=(0.62, 0.78, 0.10), toeL=(0.66, 0.90, 0.02))),
    nt=(0, -0.36, 0.52), look=(0, -1, 0.15), tilt=0)

# 5. VanNguoi — đứng ưỡn ngược ra sau, vai lệch, hai tay dài buông tới gối, đầu nghiêng 90°
POSES["VanNguoi"] = dict(P=mirror(dict(
    pelvis=(0, 0, 0.98), spine=(0, 0.10, 1.20), chest=(0.02, 0.20, 1.40), nb=(0.03, 0.14, 1.60), nt=(0.03, 0.06, 1.70),
    shL=(0.22, 0.20, 1.55), elL=(0.30, 0.16, 1.18), wrL=(0.32, 0.06, 0.78), palmL=(0.32, 0.04, 0.70), fdL=(0, -0.2, -1), cvL=(0, 0.5, -1), spL=(0, 1, 0),
    shR=(-0.19, 0.18, 1.45), elR=(-0.27, 0.15, 1.08), wrR=(-0.29, 0.05, 0.66), palmR=(-0.29, 0.03, 0.58), fdR=(0, -0.2, -1), cvR=(0, 0.5, -1), spR=(0, 1, 0),
    hipL=(0.10, 0, 0.95), kneeL=(0.12, -0.10, 0.52), ankL=(0.12, 0.02, 0.08), toeL=(0.12, -0.12, 0.02))),
    nt=(0.03, 0.06, 1.70), look=(0, -1, 0), tilt=90)

# 6. NgoiXom — ngồi xổm, gối dựng cao quá vai, tay chống sàn phía trước, đầu thấp chúi ra trước
POSES["NgoiXom"] = dict(P=mirror(dict(
    pelvis=(0, 0.15, 0.42), spine=(0, 0.02, 0.62), chest=(0, -0.10, 0.80), nb=(0, -0.22, 0.86), nt=(0, -0.30, 0.86),
    shL=(0.18, -0.12, 0.82), elL=(0.30, -0.30, 0.55), wrL=(0.22, -0.45, 0.08), palmL=(0.22, -0.50, 0.03), fdL=(0.1, -1, 0), cvL=(0.1, -1, -0.5), spL=(1, 0, 0),
    hipL=(0.10, 0.15, 0.42), kneeL=(0.24, -0.15, 0.95), ankL=(0.16, 0.05, 0.08), toeL=(0.17, -0.08, 0.02))),
    nt=(0, -0.30, 0.86), look=(0, -1, 0.1), tilt=-15)


def clear():
    bpy.ops.object.select_all(action="SELECT"); bpy.ops.object.delete()
    for m in list(bpy.data.meshes): bpy.data.meshes.remove(m)


mat_den = bpy.data.materials.new("M_Ma_Den"); mat_den.diffuse_color = (0, 0, 0, 1)
mat_mat = bpy.data.materials.new("M_Ma_Mat"); mat_mat.diffuse_color = (1, 1, 1, 1)

stats = []
for ten, d in POSES.items():
    clear()
    b = make_skin("Ma_" + ten, d["P"])
    b.data.materials.append(mat_den)
    h = make_head(b, d["nt"], d["look"], d["tilt"])
    h.data.materials.append(mat_den)
    for e in h.children: e.data.materials.append(mat_mat)
    tris = sum(len(p.vertices) - 2 for o in [b, h] + list(h.children) for p in o.data.polygons)
    zs = [ (b.matrix_world @ v.co).z for v in b.data.vertices ]
    stats.append(f"{ten}: {tris} tam giác, cao {max(zs):.2f} m (chưa kể đầu)")
    bpy.ops.object.select_all(action="DESELECT")
    for o in [b, h] + list(h.children): o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=f"{OUT}/Ma_{ten}.fbx", use_selection=True, apply_scale_options="FBX_SCALE_ALL",
                             axis_forward="-Z", axis_up="Y", mesh_smooth_type="FACE", bake_space_transform=False,
                             object_types={"MESH"}, add_leaf_bones=False)
    bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/Ma_{ten}.blend")
print("\n".join(stats))
