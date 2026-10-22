# Ảnh xem trước cho bản vẽ duyệt. python3 render.py <ten_anh> <canh>
import bpy, sys, math, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from mathutils import Vector, Euler
import tay

OUT = "/home/claude/tay/out"
bpy.ops.wm.open_mainfile(filepath=OUT + "/tay.blend")
sc = bpy.context.scene
sc.render.engine = "BLENDER_EEVEE" if "BLENDER_EEVEE" in [e.identifier for e in bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items] else "BLENDER_WORKBENCH"
sc.render.engine = "BLENDER_WORKBENCH"
sh = sc.display.shading; sh.light = "STUDIO"; sh.color_type = "MATERIAL"; sh.show_shadows = False
sc.display.shading.studio_light = "Default"
sc.render.film_transparent = False
sc.world = bpy.data.worlds.new("w"); sc.world.color = (0.18, 0.18, 0.18)
for o in bpy.data.objects:
    if o.type == "MESH":
        o.data.polygons.foreach_set("use_smooth", [False] * len(o.data.polygons))

R = {n: bpy.data.objects[n] for n in ("TayKhoi_R", "TayKhoi_L", "TayThay_R")}


def show(*names):
    for n, r in R.items():
        vis = n in names
        r.hide_render = not vis
        for c in r.children: c.hide_render = not vis


def orient(n, wrist, fwd, back):
    """đặt tay theo cổ tay, hướng ngón (fwd) và hướng mu bàn tay (back)"""
    from mathutils import Matrix
    y = Vector(fwd).normalized(); z = Vector(back); z = (z - y * z.dot(y)).normalized(); x = y.cross(z)
    M = Matrix((x, y, z)).transposed().to_4x4(); M.translation = Vector(wrist)
    R[n].matrix_world = M


def place(n, loc, rot):
    R[n].location = Vector(loc); R[n].rotation_euler = Euler([math.radians(a) for a in rot])


def cam(loc, look, lens=35, w=900, h=900):
    c = bpy.data.objects.get("Cam")
    if not c:
        c = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam")); sc.collection.objects.link(c)
    c.location = Vector(loc); c.data.lens = lens
    d = Vector(look) - c.location; c.rotation_euler = d.to_track_quat("-Z", "Y").to_euler()
    sc.camera = c; sc.render.resolution_x = w; sc.render.resolution_y = h


def render(name):
    sc.render.filepath = os.path.join(OUT, name + ".png"); bpy.ops.render.render(write_still=True)


def P(n, k):
    p = tay.POSES[k]; tay.pose(R[n], p["curl"], p.get("cai", (0, 0, 0)), p.get("wrist", (0, 0, 0)), p.get("splay", 0), p.get("doi", 0))


if __name__ == "__main__":
    which = sys.argv[-1]
    if which == "thu":
        show("TayKhoi_R"); place("TayKhoi_R", (0, 0, 0), (0, 0, 0)); P("TayKhoi_R", sys.argv[-2] if len(sys.argv) > 2 and sys.argv[-2] in tay.POSES else "Tha")
        for nm, lc in (("thu_tren", (0, -0.03, 0.5)), ("thu_ben", (-0.5, -0.03, 0.0)), ("thu_long", (0, -0.03, -0.5))):
            cam(lc, (0, -0.03, 0), lens=60, w=700, h=700); bpy.data.objects["Cam"].data.type = "ORTHO"; bpy.data.objects["Cam"].data.ortho_scale = 0.36; render(nm)


def prop(name, kind, loc, size, col, rot=(0, 0, 0)):
    if kind == "cyl": bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=size[0], depth=size[1], location=loc)
    else: bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = bpy.context.object; o.name = name
    if kind != "cyl": o.scale = size
    o.rotation_euler = Euler([math.radians(a) for a in rot])
    m = bpy.data.materials.new(name); m.diffuse_color = tay.hexc(col); o.data.materials.append(m)
    o.data.polygons.foreach_set("use_smooth", [False] * len(o.data.polygons))
    return o


def clear_props():
    for o in list(bpy.data.objects):
        if o.name.startswith("P_"): bpy.data.objects.remove(o)


def ortho(lc, look, sc_=0.36, w=700, h=700):
    cam(lc, look, w=w, h=h); c = bpy.data.objects["Cam"].data; c.type = "ORTHO"; c.ortho_scale = sc_


def persp(lc, look, lens=24, w=900, h=700):
    cam(lc, look, lens=lens, w=w, h=h); bpy.data.objects["Cam"].data.type = "PERSP"


if __name__ == "__main__" and which == "sheet":
    for n in R: place(n, (0, 0, 0), (0, 0, 0))
    # ── Khôi: hai tay, nhìn trên / bên / lòng
    show("TayKhoi_R", "TayKhoi_L"); P("TayKhoi_R", "Tha"); P("TayKhoi_L", "Tha")
    place("TayKhoi_R", (0.07, 0, 0), (0, 0, 0)); place("TayKhoi_L", (-0.07, 0, 0), (0, 0, 0))
    ortho((0, -0.05, 0.6), (0, -0.05, 0), 0.42); render("K_tren")
    ortho((0, -0.05, -0.6), (0, -0.05, 0), 0.42); render("K_long")
    show("TayKhoi_R"); place("TayKhoi_R", (0, 0, 0), (0, 0, 0)); ortho((0.6, -0.05, 0), (0, -0.05, 0), 0.42); render("K_ben")
    # ── góc nhìn người chơi: mắt ở gốc toạ độ, nhìn +Y
    E = (0, 0, 0)
    # thắp hương: hai tay chắp trước ngực, kẹp ba que nhang, ngón chỉ lên
    show("TayKhoi_R", "TayKhoi_L"); P("TayKhoi_R", "Chap"); P("TayKhoi_L", "Chap")
    orient("TayKhoi_R", (0.016, 0.30, -0.40), (0, 0.22, 1), (1, 0, 0)); orient("TayKhoi_L", (-0.016, 0.30, -0.40), (0, 0.22, 1), (-1, 0, 0))
    for i, dx in enumerate((-0.007, 0, 0.007)):
        prop(f"P_nhang{i}", "cyl", (dx * 1.6, 0.33, -0.20), (0.0018, 0.30), "#8A3A2A", (-12, (i - 1) * 4, 0))
    persp(E, (0, 1, -0.55), lens=30, w=700, h=700); render("K_fp_huong"); clear_props()
    # dải tư thế: tay phải, góc 3/4
    show("TayKhoi_R"); place("TayKhoi_R", (0, 0, 0), (0, 0, 0))
    for k in ("Tha", "Cam", "Nhon", "Nam", "Xin"):
        P("TayKhoi_R", k); persp((-0.19, 0.25, 0.12), (0, 0.06, -0.01), lens=55, w=500, h=500); render("K_p_" + k)
    # ── thầy Đồng: một tay phải
    show("TayThay_R"); place("TayThay_R", (0, 0, 0), (0, 0, 0)); P("TayThay_R", "Tha")
    ortho((0, -0.05, 0.6), (0, -0.05, 0), 0.42); render("T_tren")
    persp((0.22, 0.20, 0.16), (0, 0.06, 0), lens=50, w=700, h=700); render("T_34")
    # qua song sắt cổng, ngửa tay nhận hũ (Khôi đứng trong sân nhìn ra)
    P("TayThay_R", "Xin")
    orient("TayThay_R", (0.03, 0.42, -0.34), (0.05, -1, 0.12), (0, 0.1, -1))
    for i in range(-3, 4): prop(f"P_song{i}", "cyl", (i * 0.12 + 0.09, 0.34, -0.4), (0.008, 1.4), "#2A2C2E")
    prop("P_thanh", "cube", (0, 0.34, -0.80), (0.9, 0.02, 0.03), "#2A2C2E")
    persp(E, (0, 1, -0.55), lens=30, w=700, h=700); render("T_cong"); clear_props()
