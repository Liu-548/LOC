# LỘC — bàn tay low-poly kiểu PS1 (Khôi: hai tay · thầy Đồng: một tay phải). Dựng thuần bằng code, có xương.
# Chạy: python3 tay.py <ra_thu_muc>   (cần gói bpy)
# Quy ước: tay PHẢI dựng trước, ngón chỉ +Y, mu bàn tay hướng +Z, ngón cái phía -X. Tay trái = soi gương qua X.
# Mỗi ngón 3 xương (<Ngon>1..3), ngón cái 3 xương; Hand = cổ tay; Arm = cẳng tay (gốc ở khuỷu).
import bpy, bmesh, math, sys, os
from mathutils import Vector, Matrix

def hexc(h, a=1.0):
    c = [int(h[i:i + 2], 16) / 255 for i in (1, 3, 5)]
    return tuple(x ** 2.2 for x in c) + (a,)

# ───────── thông số hai kiểu tay
KHOI = dict(
    ten="Khoi", da="#C49274", ao="#4C5560", mong="#D9AE9A",
    canTay=(0.29, 0.046, 0.040, 0.030, 0.021),   # dài cẳng tay · bán kính ngang/dày ở khuỷu · ngang/dày ở cổ tay
    lon=1.0, khop=1.0, gay=1.0,
    ong=dict(dau=-0.29, cuoi=-0.115, r=1.32, xan=True),   # ống tay áo xắn tới giữa cẳng tay
    nhan=False, mongDai=0.0,
)
THAY = dict(
    ten="Thay", da="#9A6A4E", ao="#7A6A4A", mong="#C9A486",
    canTay=(0.29, 0.044, 0.038, 0.029, 0.020),
    lon=1.04, khop=1.18, gay=0.86,        # ngón gầy hơn, đốt khớp to (tay người ~60 tuổi)
    ong=dict(dau=-0.29, cuoi=-0.025, r=1.30, xan=False),   # áo khoác ka-ki dài tay, cổ tay áo sát cổ tay
    nhan=True, mongDai=0.014,             # nhẫn mặt đá xanh ngón áp út · móng út để dài
)

# ngón (tên, gốc x, gốc y, 3 đốt dài, bán kính gốc→ngọn)
NGON = [
    ("Index", -0.025, 0.090, (0.037, 0.022, 0.019), (0.0104, 0.0086)),
    ("Middle", -0.007, 0.093, (0.041, 0.025, 0.020), (0.0106, 0.0088)),
    ("Ring", 0.011, 0.090, (0.038, 0.024, 0.019), (0.0100, 0.0083)),
    ("Pinky", 0.027, 0.082, (0.030, 0.018, 0.017), (0.0090, 0.0074)),
]
NGON_CAI = ("Thumb", Vector((-0.022, 0.022, -0.007)), Vector((-0.40, 0.88, -0.22)).normalized(), (0.040, 0.029, 0.024), (0.0122, 0.0094))


def ring(bm, c, ax, side, rx, rz, n, rot=0.0):
    """vòng n đỉnh quanh tâm c, mặt phẳng vuông góc trục ax; side = hướng 'ngang', rx/rz bán kính ngang/dày"""
    up = ax.cross(side).normalized(); side = up.cross(ax).normalized()
    vs = []
    for i in range(n):
        a = rot + 2 * math.pi * i / n
        vs.append(bm.verts.new(c + side * math.cos(a) * rx + up * math.sin(a) * rz))
    return vs


def loft(bm, a, b):
    n = len(a)
    for i in range(n):
        bm.faces.new((a[i], a[(i + 1) % n], b[(i + 1) % n], b[i]))


def cap(bm, r, tip=None):
    if tip is None: bm.faces.new(list(reversed(r))); return
    t = bm.verts.new(tip); n = len(r)
    for i in range(n): bm.faces.new((r[i], r[(i + 1) % n], t))


def tube(bm, pts, radii, side, n, wt, flat=1.0, tip=None, open0=False):
    """ống qua các điểm pts; radii[i]=(ngang,dày); wt[i]=[(xương,trọng)]: gán nhóm đỉnh"""
    rs = []
    for i, p in enumerate(pts):
        ax = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
        r = ring(bm, p, ax, side, radii[i][0], radii[i][1] * flat, n, math.pi / n)
        for v in r: VG[v] = wt[i]
        rs.append(r)
    for i in range(len(rs) - 1): loft(bm, rs[i], rs[i + 1])
    if not open0: cap(bm, rs[0])
    if tip is not None:
        t = bm.verts.new(tip); VG[t] = wt[-1]
        n0 = len(rs[-1])
        for i in range(n0): bm.faces.new((rs[-1][i], rs[-1][(i + 1) % n0], t))
    else: bm.faces.new(rs[-1])
    return rs


VG = {}   # đỉnh → [(xương, trọng)]


def build(S, outdir, mirror=False):
    global VG; VG = {}
    L = S["lon"]; K = S["khop"]; G = S["gay"]
    side = Vector((1, 0, 0))
    meshes = {}

    # ── da: cẳng tay + bàn tay + ngón (một mesh, nhiều ống chồng nhau — kiểu PS1, không hàn)
    bm = bmesh.new()
    dai, rxE, rzE, rxW, rzW = S["canTay"]
    tube(bm, [Vector((0, -dai, 0)), Vector((0, -dai * 0.5, -0.002)), Vector((0, -0.012, 0)), Vector((0, 0.004, 0))],
         [(rxE, rzE), ((rxE + rxW) / 2 * 1.04, (rzE + rzW) / 2), (rxW, rzW), (rxW * 1.02, rzW * 0.95)], side, 8,
         [[("Arm", 1)], [("Arm", 1)], [("Arm", .6), ("Hand", .4)], [("Hand", 1)]])
    # bàn tay: lòng/mu dẹt, rộng dần tới khớp ngón
    tube(bm, [Vector((0, 0.0, 0)), Vector((-0.003, 0.045, 0.002)), Vector((0.000, 0.088 * L, 0.0))],
         [(0.031 * L, 0.019), (0.043 * L, 0.0185), (0.045 * L, 0.0145)], side, 8,
         [[("Hand", 1)], [("Hand", 1)], [("Hand", 1)]])
    bones = {"Arm": (Vector((0, -dai, 0)), Vector((0, 0, 0))), "Hand": (Vector((0, 0, 0)), Vector((0, 0.06 * L, 0)))}
    nails = []
    for (ten, x, y, ds, (r0, r1)) in NGON:
        p = Vector((x * L, y * L, 0.002)); ax = Vector((0.025 * x / 0.03, 1, 0)).normalized()
        pts = [p - ax * 0.012]; rad = [(r0 * G * 1.05, r0 * 0.88)]; wt = [[("Hand", 1)]]
        q = p.copy()
        for k, d in enumerate(ds):
            d *= L; b0 = q.copy(); q = q + ax * d
            bones[f"{ten}{k + 1}"] = (b0, q.copy())
            rr = r0 + (r1 - r0) * (k + 1) / 3
            # khớp: phình nhẹ (tay thầy phình nhiều)
            pts += [b0 + ax * 0.002, b0 + ax * d * 0.5, q - ax * 0.0015]
            j = rr * G * (1.0 + 0.09 * (K - 1) * 4)
            rad += [(j, j * 0.86), (rr * G, rr * 0.82), (j * 0.98, j * 0.84)]
            prev = "Hand" if k == 0 else f"{ten}{k}"
            wt += [[(prev, .5), (f"{ten}{k + 1}", .5)], [(f"{ten}{k + 1}", 1)], [(f"{ten}{k + 1}", .8)] + ([(f"{ten}{k + 2}", .2)] if k < 2 else [])]
        tube(bm, pts, rad, side, 6, wt, tip=q + ax * 0.004 + Vector((0, 0, -0.001)))
        nails.append((ten, q, ax, rr * G))
    # ngón cái
    ten, p0, ax, ds, (r0, r1) = NGON_CAI
    p0 = Vector((p0.x * L, p0.y * L, p0.z)); q = p0.copy()
    pts = [p0 - ax * 0.006]; rad = [(r0 * 1.25, r0 * 1.05)]; wt = [[("Hand", 1)]]
    for k, d in enumerate(ds):
        d *= L; b0 = q.copy(); q = q + ax * d; bones[f"{ten}{k + 1}"] = (b0, q.copy())
        rr = r0 + (r1 - r0) * (k + 1) / 3
        prev = "Hand" if k == 0 else f"{ten}{k}"
        pts += [b0 + ax * 0.002, b0 + ax * d * 0.5, q - ax * 0.0015]
        j = rr * G * (1.0 + 0.09 * (K - 1) * 4) * (1.25 if k == 0 else 1)
        rad += [(j, j * 0.9), (rr * G * (1.3 if k == 0 else 1), rr * 0.86), (j * 0.98, j * 0.86)]
        wt += [[(prev, .5), (f"{ten}{k + 1}", .5)], [(f"{ten}{k + 1}", 1)], [(f"{ten}{k + 1}", .8)] + ([(f"{ten}{k + 2}", .2)] if k < 2 else [])]
    tube(bm, pts, rad, Vector((0.35, 0.2, 0.9)).normalized().cross(ax).normalized(), 6, wt, tip=q + ax * 0.004)
    nails.append((ten, q, ax, r1 * G))
    meshes["Da"] = bm

    # ── móng: tấm phẳng trên mu đốt cuối
    bm = bmesh.new()
    for (ten, q, ax, rr) in nails:
        up = Vector((0, 0, 1)) if ten != "Thumb" else Vector((0.45, 0.15, 0.88)).normalized()
        sd = ax.cross(up).normalized(); up = sd.cross(ax).normalized()
        ln = 0.011 + (S["mongDai"] if ten == "Pinky" else 0)
        base = q - ax * 0.010 + up * rr * 0.80
        vs = [bm.verts.new(base + sd * w * rr * 0.75 + ax * l + up * (0.0005 if 0 < l < ln else 0)) for (w, l) in ((-1, 0), (1, 0), (1, ln), (-1, ln))]
        for v in vs: VG[v] = [(f"{ten}3", 1)]
        bm.faces.new(vs)
    meshes["Mong"] = bm

    # ── ống tay áo
    bm = bmesh.new(); o = S["ong"]
    f = lambda y: (rxE + (rxW - rxE) * (y + dai) / dai, rzE + (rzW - rzE) * (y + dai) / dai)
    ys = [o["dau"] - 0.02, o["dau"] * 0.55 + o["cuoi"] * 0.45, o["cuoi"] - 0.02, o["cuoi"]]
    rads = [tuple(c * o["r"] for c in f(y)) for y in ys]
    if o["xan"]:   # xắn: một vòng gấp dày ở mép
        rads[-2] = tuple(c * 1.12 for c in rads[-2]); rads[-1] = tuple(c * 1.10 for c in rads[-1])
    tube(bm, [Vector((0, y, 0.001)) for y in ys], rads, side, 8, [[("Arm", 1)]] * 4)
    if o["xan"]:
        # mép xắn quay ngược vào trong cho thấy độ dày
        tube(bm, [Vector((0, o["cuoi"] - 0.001, 0.001)), Vector((0, o["cuoi"] - 0.03, 0.001))],
             [tuple(c * 0.985 for c in rads[-1]), tuple(c * 0.985 for c in rads[-2])], side, 8, [[("Arm", 1)]] * 2)
    meshes["Ao"] = bm

    if S["nhan"]:   # nhẫn ngón áp út, mặt đá xanh lục
        bm = bmesh.new(); a, b = bones["Ring1"]; ax = (b - a).normalized(); c = a + (b - a) * 0.62
        rr = 0.0128 * G
        r1 = ring(bm, c - ax * 0.0035, ax, side, rr, rr * 0.88, 8); r2 = ring(bm, c + ax * 0.0035, ax, side, rr, rr * 0.88, 8)
        for v in r1 + r2: VG[v] = [("Ring1", 1)]
        loft(bm, r1, r2)
        meshes["Nhan"] = bm
        bm = bmesh.new()
        st = bmesh.ops.create_cube(bm, size=0.0075)["verts"]
        for v in st: v.co = v.co * Vector((1, 1.15, 0.55)) + c + Vector((0, 0, rr * 0.9 + 0.001)); VG[v] = [("Ring1", 1)]
        meshes["Da_Nhan"] = bm

    # ── tạo object + armature
    col = bpy.data.collections.new("Tay_" + S["ten"]); bpy.context.scene.collection.children.link(col)
    side_name = "_L" if mirror else "_R"
    arm_d = bpy.data.armatures.new(f"Tay{S['ten']}{side_name}_Rig")
    arm = bpy.data.objects.new(f"Tay{S['ten']}{side_name}", arm_d); col.objects.link(arm)
    bpy.context.view_layer.objects.active = arm; bpy.ops.object.mode_set(mode="EDIT")
    M = Matrix.Scale(-1, 4, (1, 0, 0)) if mirror else Matrix.Identity(4)
    eb = {}
    for n, (h, t) in bones.items():
        b = arm_d.edit_bones.new(n); b.head = M @ h; b.tail = M @ t; b.roll = 0; eb[n] = b
    for n, b in eb.items():
        if n == "Hand": b.parent = eb["Arm"]
        elif n != "Arm":
            k = int(n[-1]); b.parent = eb["Hand"] if k == 1 else eb[n[:-1] + str(k - 1)]; b.use_connect = False
    bpy.ops.object.mode_set(mode="OBJECT")
    mats = {"Da": S["da"], "Mong": S["mong"], "Ao": S["ao"], "Nhan": "#C9A64A", "Da_Nhan": "#2E7A55"}
    objs = []
    for part, bm in meshes.items():
        me = bpy.data.meshes.new(f"{S['ten']}_{part}")
        # nhóm đỉnh lấy theo chỉ số trước khi ghi ra mesh
        bm.verts.index_update()
        weights = {v.index: VG.get(v, [("Hand", 1)]) for v in bm.verts}
        bmesh.ops.transform(bm, matrix=M, verts=bm.verts)
        if mirror: bmesh.ops.reverse_faces(bm, faces=bm.faces)
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        bm.to_mesh(me); bm.free()
        ob = bpy.data.objects.new(me.name, me); col.objects.link(ob); ob.parent = arm
        mname = "M_Tay" + S["ten"] + "_" + part.split("_")[-1]
        m = bpy.data.materials.get(mname) or bpy.data.materials.new(mname)
        m.diffuse_color = hexc(mats[part]); m.use_nodes = True
        m.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = hexc(mats[part])
        m.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = 0.35 if part in ("Nhan", "Da_Nhan", "Mong") else 0.75
        me.materials.append(m)
        for i, ws in weights.items():
            for bn, w in ws:
                g = ob.vertex_groups.get(bn) or ob.vertex_groups.new(name=bn); g.add([i], w, "ADD")
        mod = ob.modifiers.new("Arm", "ARMATURE"); mod.object = arm
        objs.append(ob)
    return arm, objs


# ───────── tư thế: co ngón (độ) cho từng ngón, cổ tay
def pose(arm, curl, cai=(0, 0, 0), wrist=(0, 0, 0), splay=0, doi=0):
    """curl: dict ngón → (đốt1, đốt2, đốt3) độ; cai: ngón cái; wrist: (gập, nghiêng, xoay) độ"""
    pb = arm.pose.bones; sgn = -1 if arm.name.endswith("_L") else 1
    for b in pb: b.rotation_mode = "XYZ"; b.rotation_euler = (0, 0, 0)
    for ten, c in curl.items():
        for k in range(3): pb[f"{ten}{k + 1}"].rotation_euler = (-math.radians(c[k]), 0, 0)
        if splay:
            i = ["Index", "Middle", "Ring", "Pinky"].index(ten)
            pb[f"{ten}1"].rotation_euler.z = sgn * math.radians(splay * (1.5 - i) * 0.7)
    for k in range(3): pb[f"Thumb{k + 1}"].rotation_euler = (-math.radians(cai[k]), 0, 0)
    pb["Thumb1"].rotation_euler.z = -sgn * math.radians(doi)   # đối ngón: xoay ngón cái về phía ngón trỏ
    pb["Hand"].rotation_euler = (math.radians(wrist[0]), sgn * math.radians(wrist[2]), sgn * math.radians(wrist[1]))


POSES = {
    "Tha": dict(curl={"Index": (8, 12, 6), "Middle": (12, 16, 8), "Ring": (15, 18, 9), "Pinky": (18, 20, 10)}, cai=(0, 6, 4)),
    "Nam": dict(curl={n: (75, 95, 60) for n in ("Index", "Middle", "Ring", "Pinky")}, cai=(20, 35, 25), doi=30),
    "Cam": dict(curl={"Index": (40, 55, 35), "Middle": (45, 60, 40), "Ring": (50, 62, 40), "Pinky": (55, 65, 40)}, cai=(20, 25, 20), doi=35),
    "Nhon": dict(curl={"Index": (38, 42, 22), "Middle": (55, 75, 45), "Ring": (65, 82, 50), "Pinky": (72, 88, 55)}, cai=(15, 20, 15), doi=48),
    "Xoe": dict(curl={n: (0, 2, 0) for n in ("Index", "Middle", "Ring", "Pinky")}, cai=(0, 0, 0), splay=10),
    "Xin": dict(curl={"Index": (18, 22, 10), "Middle": (22, 26, 12), "Ring": (26, 30, 14), "Pinky": (30, 34, 16)}, cai=(-10, 10, 8), splay=4),
    "Chap": dict(curl={n: (5, 6, 3) for n in ("Index", "Middle", "Ring", "Pinky")}, cai=(10, 5, 0)),
}

if __name__ == "__main__":
    out = sys.argv[-1] if len(sys.argv) > 1 and not sys.argv[-1].endswith(".py") else "/home/claude/tay/out"
    os.makedirs(out, exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rigs = {}
    rigs["Khoi_R"] = build(KHOI, out)[0]
    rigs["Khoi_L"] = build(KHOI, out, mirror=True)[0]
    rigs["Thay_R"] = build(THAY, out)[0]
    tri = {}
    for o in bpy.data.objects:
        if o.type == "MESH":
            k = o.parent.name; tri[k] = tri.get(k, 0) + sum(len(p.vertices) - 2 for p in o.data.polygons)
    print("TRI", tri)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out, "tay.blend"))
