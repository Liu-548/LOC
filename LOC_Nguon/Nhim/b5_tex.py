# Bước 5: UV cho quần áo/bột/mắt, nướng vị trí, vẽ texture (numpy), gán vật liệu, giảm lưới chăn/quần áo.
import sys, os; sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy, bmesh, numpy as np, math, json, geo, bake
from mathutils import Vector
from PIL import Image, ImageDraw, ImageFont, ImageFilter
OUT = "/home/claude/nhim/out"; TX = f"{OUT}/tex"; os.makedirs(TX, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=f"{OUT}/b4.blend")
O = bpy.data.objects
rng = np.random.default_rng(11)

def noise(w, h, s, oct=4, seed=None):
    r = np.random.default_rng(seed) if seed is not None else rng
    a = np.zeros((h, w))
    for o in range(oct):
        f = 2 ** o; sw, sh = max(2, int(w / s * f)), max(2, int(h / s * f))
        a += np.asarray(Image.fromarray((r.random((sh, sw)) * 255).astype(np.uint8)).resize((w, h), Image.BICUBIC), float) / 255 / f
    return a / sum(1 / 2 ** o for o in range(oct))

def save(a, name, flip=True):
    """a: (H,W,3|4) 0..1, hàng 0 = đáy (theo bake) → lật cho ảnh PNG"""
    a = np.clip(a, 0, 1)
    if flip: a = a[::-1]
    Image.fromarray((a * 255 + 0.5).astype(np.uint8)).save(f"{TX}/{name}.png")

def mix(base, col, m):
    m = np.clip(m, 0, 1)[..., None]; return base * (1 - m) + np.asarray(col) * m

def gauss(p, c, s):
    d = (p - np.asarray(c)) / np.asarray(s); return np.exp(-0.5 * (d ** 2).sum(-1))

# ───────── 0. giảm lưới chăn, chăn vo, quần áo
for n, r in (("Chan", 0.42), ("ChanVo", 0.4), ("Ao", 0.5), ("Quan", 0.55)):
    geo.decimate(O[n], r, sym=(n in ("Ao", "Quan")))
    print("giảm", n, geo.tris(O[n]))

# ───────── 1. UV: quần áo + bột + dây (smart project, mật độ đều → in lặp), mắt (chiếu phẳng)
def smart(objs, unit):
    for o in bpy.context.view_layer.objects: o.select_set(o in objs)
    bpy.context.view_layer.objects.active = objs[0]
    for o in objs:
        if not o.data.uv_layers: o.data.uv_layers.new(name="UV")
        o.data.uv_layers.active = o.data.uv_layers[0]; o.data.uv_layers[0].name = "UV"
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=math.radians(55), island_margin=0.01, scale_to_bounds=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    # đổi tỉ lệ: 1 đơn vị UV = unit mét
    A3 = sum(p.area for o in objs for p in o.data.polygons)
    AU = 0
    for o in objs:
        uv = o.data.uv_layers[0].data
        for p in o.data.polygons:
            pts = [uv[i].uv for i in p.loop_indices]
            AU += abs(sum(pts[i].x * pts[i - 1].y - pts[i - 1].x * pts[i].y for i in range(len(pts)))) / 2
    k = math.sqrt(A3 / AU) / unit
    for o in objs:
        for d in o.data.uv_layers[0].data: d.uv = d.uv * k
    return k
smart([O["Ao"], O["Quan"]], 0.16)
smart([O["Bot"]], 1.0)       # bột: một ảnh riêng, không lặp (nướng vị trí để vẽ)
for o in (O["Bot"],):
    uv = o.data.uv_layers[0].data   # nén về 0..1
    us = np.array([d.uv[:] for d in uv]); mn, mx = us.min(0), us.max(0); s = (mx - mn).max()
    for d in uv: d.uv = (Vector(d.uv) - Vector(mn)) / s
for n in ("Eye_L", "Eye_R"):
    o = O[n]; me = o.data; c = Vector(np.array([v.co[:] for v in me.vertices]).mean(0)); r = 0.0157
    uv = me.uv_layers[0].data
    for p in me.polygons:
        for li in p.loop_indices:
            co = me.vertices[me.loops[li].vertex_index].co
            uv[li].uv = (0.5 + (co.x - c.x) / (2 * r), 0.5 + (co.z - c.z) / (2 * r)) if co.y < c.y - 0.004 else (0.03, 0.03)
for d in O["Chan"].data.uv_layers[0].data: d.uv = d.uv * 3
for d in O["ChanVo"].data.uv_layers[0].data: d.uv = d.uv * 3

# ───────── 2. DA (4 mức sốt: 0 khỏe · 1 Đêm 1 · 2 Đêm 2 · 3 Đêm 3)
body = O["Body"]
for kb in body.data.shape_keys.key_blocks: kb.value = 0
S = 1024
B = bake.bake_maps(body, S)
p, n, cov = B["pos"], B["nrm"], B["cov"]
x, y, z = p[..., 0], p[..., 1], p[..., 2]
N1 = noise(S, S, 60, seed=1); N2 = noise(S, S, 8, seed=2); N3 = noise(S, S, 3, 2, seed=3)
EYE = [(0.0278, -0.0914, 1.0989), (-0.0278, -0.0914, 1.0989)]
MZ = 1.043   # đường miệng
front = (y < -0.06) & (z > 0.98)

def da(f):
    base = np.array([0.88, 0.71, 0.58])
    c = np.broadcast_to(base, p.shape).copy()
    c *= (0.94 + 0.10 * N1[..., None]) * (0.985 + 0.03 * N2[..., None])
    # xanh xao theo sốt (rút sắc, ngả xám vàng)
    g = c.mean(-1, keepdims=True); c = c * (1 - 0.18 * f) + g * 0.18 * f
    c = c * (1 - 0.05 * f) + np.array([0.80, 0.76, 0.62]) * 0.05 * f
    # lòng bàn tay/bàn chân nhạt hơn, gan bàn chân hơi bẩn
    c = mix(c, (0.80, 0.66, 0.56), (z < 0.025) * (n[..., 2] < -0.3) * 0.5)
    # má hồng trẻ con + đỏ vì sốt
    for s_ in (1, -1):
        c = mix(c, (0.93, 0.47, 0.44), gauss(p, (0.040 * s_, -0.095, 1.066), (0.016, 0.03, 0.013)) * (0.16 + 0.55 * f) * front)
        # quầng thâm dưới mắt
        c = mix(c, (0.56, 0.42, 0.46), gauss(p, (0.028 * s_, -0.098, 1.087), (0.012, 0.03, 0.0045)) * (0.10 + 0.40 * f) * front)
        # mí đỏ
        c = mix(c, (0.86, 0.50, 0.50), gauss(p, (0.028 * s_, -0.10, 1.099), (0.014, 0.03, 0.008)) * 0.30 * f * front)
        # tai hồng
        c = mix(c, (0.90, 0.55, 0.52), gauss(p, (0.074 * s_, -0.03, 1.095), (0.012, 0.02, 0.02)) * (0.2 + 0.3 * f))
    c = mix(c, (0.92, 0.50, 0.48), gauss(p, (0, -0.128, 1.068), (0.008, 0.02, 0.006)) * (0.10 + 0.35 * f) * front)
    # môi: nhợt và khô dần
    dx = x / 0.0205
    up = (z - MZ) / 0.0085; lo = (MZ - z) / 0.0105
    lip = ((dx ** 2 + np.where(z >= MZ, up, lo) ** 2) < 1) & (y < -0.104) & (z > 1.02) & (z < 1.06)
    lipm = np.asarray(Image.fromarray((lip * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(1.5)), float) / 255
    lipc = np.array([0.80, 0.50, 0.49]) * (1 - f * 0.4) + np.array([0.82, 0.64, 0.60]) * f * 0.4
    c = mix(c, lipc, lipm * (0.85 - 0.25 * f))
    c = mix(c, (0.93, 0.85, 0.80), lipm * (N3 > 0.62) * 0.35 * f)          # nứt nẻ
    c = mix(c, (0.42, 0.24, 0.24), (np.abs(z - MZ) < 0.0011) * (np.abs(x) < 0.0195) * (y < -0.10))
    # lông mày thưa, nâu đen
    for s_, (ex, ey, ez) in zip((1, -1), EYE):
        t = (s_ * x - 0.010) / 0.036
        zc = 1.1185 + 0.0055 * np.sin(np.pi * np.clip(t, 0, 1) * 0.85) - 0.0015 * t
        th = 0.0024 * (1 - 0.55 * np.clip(t, 0, 1))
        m = (t > 0) & (t < 1) & (np.abs(z - zc) < th) & (y < -0.08)
        m = m * (0.55 + 0.45 * (N3 > 0.45))
        c = mix(c, (0.24, 0.17, 0.13), m * 0.7)
        # viền mi trên (đậm) + mi dưới (nhạt)
        u = (x - ex) / 0.0128
        ok = (np.abs(u) < 1) & (y < -0.097) & (np.abs(z - ez) < 0.012)
        zu = ez + 0.0060 * np.sqrt(np.clip(1 - u ** 2, 0, 1)) + 0.0012 * u * s_
        zl = ez - 0.0046 * np.sqrt(np.clip(1 - u ** 2, 0, 1))
        c = mix(c, (0.10, 0.07, 0.07), ok * (z >= zu - 0.0004) * (z < zu + 0.0016) * 0.95)
        c = mix(c, (0.35, 0.25, 0.25), ok * (np.abs(z - zl) < 0.0007) * 0.35)
    # da đầu dưới tóc: nâu đen (đỡ lộ khe giữa các lọn)
    ang = np.degrees(np.arctan2(x, -(y + 0.03))); a = np.abs(ang)
    hl = np.where(a < 50, 1.165, np.where(a < 100, 1.125 - (a - 50) / 50 * 0.01, 1.07 - (a - 100) / 80 * 0.05))
    ear = (np.abs(x) > 0.066) & (z > 1.06) & (z < 1.125) & (np.abs(y + 0.03) < 0.03)
    scalp = (z > hl) & ~ear & (z > 1.0)
    sm = np.asarray(Image.fromarray((scalp * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(3)), float) / 255
    c = mix(c, (0.13, 0.10, 0.08), sm * 0.92)
    return c

for lv, f in enumerate((0.05, 0.35, 0.6, 0.85)):
    save(np.concatenate([da(f), np.ones_like(x)[..., None]], -1)[..., :3], f"T_Nhim_Da_{lv}")
print("da xong")

# ───────── 3. MẮT
E = 256; yy, xx = (np.mgrid[0:E, 0:E] + 0.5) / E
r = np.hypot(xx - 0.5, yy - 0.5) * 2   # 0 tâm → 1 mép đĩa
ang = np.arctan2(yy - 0.5, xx - 0.5)
for lv, f in enumerate((0.05, 0.35, 0.6, 0.85)):
    sc = np.broadcast_to(np.array([0.93, 0.91, 0.88]), (E, E, 3)).copy()
    vein = (np.abs(noise(E, E, 10, seed=5) - 0.5) < 0.012) * np.clip((r - 0.45) / 0.5, 0, 1)
    sc = mix(sc, (0.85, 0.45, 0.45), vein * (0.04 + 0.16 * f) + np.clip(r - 0.6, 0, 1) * 0.4 * f)
    iris = np.array([0.24, 0.15, 0.10]) * (0.75 + 0.5 * (0.5 + 0.5 * np.sin(ang * 23 + noise(E, E, 20, seed=6) * 6)))[..., None]
    iris = mix(iris, (0.12, 0.08, 0.06), np.clip((r - 0.30) / 0.05, 0, 1))
    c = mix(sc, iris, r < 0.35); c = mix(c, (0.02, 0.02, 0.02), r < 0.13)
    c = mix(c, (0.75, 0.68, 0.62), (r >= 0.35) * (r < 0.37) * 0.3)
    save(c, f"T_Nhim_Mat_{lv}")

# ───────── 4. TÓC (RGBA, cắt alpha ở ngọn)
tj = json.load(open(f"{OUT}/toc_uv.json")); vb = np.array(tj["v_bottom"]); NC = tj["nc"]
TW, TH = 512, 1024
uu = (np.arange(TW) + 0.5) / TW; vv = (np.arange(TH) + 0.5) / TH
U, Vv = np.meshgrid(uu, vv[::-1])               # hàng 0 = trên ảnh = v 1
cb = np.interp(uu * NC, np.arange(NC + 1), np.r_[vb, vb[0]])     # v ngọn theo cột
strand = np.repeat(rng.random(TW // 2), 2)
tip = cb + 0.004 + strand * 0.022
front = np.abs(uu - 0.5) < 0.14
clump = (np.sin(uu * TW / 9 * 2 * np.pi) > 0.55) & front           # mái bết thành từng túm
tip = np.where(clump, tip + 0.025, tip)
alpha = (Vv > tip[None, :]).astype(float)
alpha *= np.where(Vv < tip[None, :] + 0.02, 0.7 + 0.3 * (np.repeat(rng.random(TW // 2), 2) > 0.3)[None, :], 1)
st = noise(TW, TH, 3, 3, seed=7); sl = noise(TW, TH, 40, seed=8)
lines = np.repeat(rng.random(TW // 2), 2)[None, :] * np.ones((TH, 1))
col = np.array([0.105, 0.080, 0.065]) * (0.45 + 0.6 * lines[..., None] * (0.6 + 0.5 * st[..., None])) * (0.85 + 0.3 * sl[..., None])
col = col + np.array([0.10, 0.08, 0.07]) * np.exp(-((Vv - 0.86) / 0.05) ** 2)[..., None] * (st[..., None] > 0.5)
col = mix(col, (0.06, 0.045, 0.04), (front[None, :] & (Vv < 0.40)) * 0.5)            # mái bết mồ hôi
img = np.concatenate([col, alpha[..., None]], -1)
Image.fromarray((np.clip(img, 0, 1) * 255).astype(np.uint8)).save(f"{TX}/T_Nhim_Toc.png")

# ───────── 5. VẢI ÁO BỘ (ô lặp 512 = 16 cm): nền trắng ngà, hoa nhí xanh/hồng, đã bạc
T = 512
c = np.broadcast_to(np.array([0.90, 0.87, 0.78]), (T, T, 3)).copy()
im = Image.fromarray((c * 255).astype(np.uint8)); d = ImageDraw.Draw(im)
pts = [(i * 64 + 32 + rng.integers(-14, 15), j * 64 + 32 + rng.integers(-14, 15)) for i in range(8) for j in range(8)]
for k, (px, py) in enumerate(pts):
    colf = [(120, 150, 200), (220, 150, 160), (150, 175, 215)][k % 3]
    rr = 7 + rng.integers(0, 3); a0 = rng.random() * 6.28
    for q in range(5):
        a = a0 + q * 2 * np.pi / 5
        for ox in (-T, 0, T):
            for oy in (-T, 0, T):
                cx, cy = px + ox + math.cos(a) * rr * 0.9, py + oy + math.sin(a) * rr * 0.9
                d.ellipse([cx - rr * 0.6, cy - rr * 0.6, cx + rr * 0.6, cy + rr * 0.6], fill=colf)
                if q == 0:
                    d.ellipse([px + ox - 3, py + oy - 3, px + ox + 3, py + oy + 3], fill=(235, 205, 120))
                    lx, ly = px + ox + math.cos(a0 + 2.5) * rr * 1.9, py + oy + math.sin(a0 + 2.5) * rr * 1.9
                    d.ellipse([lx - 4, ly - 2.5, lx + 4, ly + 2.5], fill=(140, 175, 130))
c = np.asarray(im, float) / 255
wash = noise(T, T, 90, seed=9)
c = c * (0.93 + 0.08 * wash[..., None]); g = c.mean(-1, keepdims=True); c = c * 0.75 + g * 0.25   # bạc màu
weave = ((np.indices((T, T)).sum(0) % 2) * 0.02)[..., None]
save(c - weave, "T_Nhim_Ao", flip=False)

# ───────── 6. BỘT (nướng vị trí → vẽ theo toạ độ 3D)
bot = O["Bot"]; BS = 512
Bb = bake.bake_maps(bot, BS); bp = Bb["pos"]; bn = Bb["nrm"]
arm = O["NhimRig"]
el = arm.data.bones["LowerArm_L"].head_local; wr = arm.data.bones["LowerArm_L"].tail_local
ax = (wr - el).normalized()
t = ((bp - np.array(el)) @ np.array(ax)) / (wr - el).length      # 0 khuỷu → 1 cổ tay
c = np.broadcast_to(np.array([0.92, 0.90, 0.84]), (BS, BS, 3)).copy()
c *= (0.92 + 0.12 * noise(BS, BS, 12, seed=12)[..., None])
c = mix(c, (0.70, 0.68, 0.64), np.clip((t - 1.05) / 0.25, 0, 1) * 0.6)     # mép tay bẩn xám
c = mix(c, (0.80, 0.78, 0.72), (noise(BS, BS, 30, seed=13) > 0.7) * 0.35)
# hình vẽ bút sáp trên mặt trước cẳng tay (nhìn từ phía trước–trên)
canvas = Image.new("RGBA", (400, 160), (0, 0, 0, 0)); dc = ImageDraw.Draw(canvas)
for q in range(6):   # bông hoa đỏ
    a = q * np.pi / 3; dc.ellipse([60 + math.cos(a) * 22 - 14, 80 + math.sin(a) * 22 - 14, 60 + math.cos(a) * 22 + 14, 80 + math.sin(a) * 22 + 14], outline=(210, 50, 60, 255), width=5)
dc.ellipse([50, 70, 70, 90], fill=(240, 200, 60, 255))
dc.ellipse([130, 45, 200, 115], outline=(60, 110, 200, 255), width=5)                 # mặt cười xanh
dc.ellipse([148, 65, 156, 73], fill=(60, 110, 200, 255)); dc.ellipse([174, 65, 182, 73], fill=(60, 110, 200, 255))
dc.arc([145, 70, 185, 102], 20, 160, fill=(60, 110, 200, 255), width=5)
try: fnt = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", 46)
except Exception: fnt = None
dc.text((225, 50), "NHÍM", fill=(230, 110, 160, 255), font=fnt)
canvas = canvas.filter(ImageFilter.GaussianBlur(0.8))
cv = np.asarray(canvas, float) / 255
up = np.array([0, -0.6, 0.8]); up /= np.linalg.norm(up)       # mặt trước–trên của cẳng tay
side = np.cross(up, np.array(ax)); side /= np.linalg.norm(side)
mid = np.array(el) + np.array(ax) * (wr - el).length * 0.55
q = bp - mid
a_ = q @ np.array(ax); b_ = q @ side
facing = (bn @ up) > 0.35
cu = ((a_ / 0.11) * 0.5 + 0.5) * 400; cvv = ((-b_ / 0.044) * 0.5 + 0.5) * 160
okm = facing & (cu >= 0) & (cu < 400) & (cvv >= 0) & (cvv < 160) & (Bb["cov"] > 0)
iu = np.clip(cu.astype(int), 0, 399); iv = np.clip(cvv.astype(int), 0, 159)
pc = cv[iv, iu]
c = mix(c, pc[..., :3], pc[..., 3] * okm * (0.75 + 0.25 * (noise(BS, BS, 2, 2, seed=14) > 0.3)))
save(c, "T_Nhim_Bot")

# ───────── 7. CHĂN kẻ ô (ô lặp 512 = 40 cm), KHĂN, DÂY ĐEO
T = 512; I, J = np.indices((T, T))
c = np.broadcast_to(np.array([0.86, 0.84, 0.78]), (T, T, 3)).copy()
for k, (w, col) in enumerate([(70, (0.45, 0.55, 0.72)), (14, (0.72, 0.30, 0.32))]):
    off = 0 if k == 0 else 120
    bandx = ((J + off) % 256) < w; bandy = ((I + off) % 256) < w
    c = mix(c, col, bandx * 0.55); c = mix(c, col, bandy * 0.55)
c *= (0.92 + 0.1 * noise(T, T, 80, seed=15)[..., None]); c -= ((I + J) % 2 * 0.02)[..., None]
save(c, "T_Chan", flip=False)
K = 256; I, J = np.indices((K, K))
dry = np.broadcast_to(np.array([0.93, 0.92, 0.88]), (K // 2, K, 3)).copy()
dry -= (noise(K, K // 2, 2, 2, seed=16) * 0.06)[..., None]
dry = mix(dry, (0.45, 0.60, 0.80), ((J[:K // 2] % 128) < 8) * 0.8)
dry = mix(dry, (0.85, 0.80, 0.62), noise(K, K // 2, 30, seed=17) > 0.72)            # ố vàng
wet = dry * 0.78; wet = mix(wet, (0.70, 0.72, 0.74), 0.15)
save(np.concatenate([wet, dry], 0), "T_Khan", flip=False)    # nửa trên ảnh = v 0.5..1 = khăn ướt? (UV ướt ở nửa trên)
d = np.broadcast_to(np.array([0.90, 0.88, 0.80]), (K, K, 3)).copy()
d -= (noise(K, K, 2, 2, seed=18) * 0.05)[..., None]; d = mix(d, (0.75, 0.72, 0.65), (I % 6 == 0) * 0.2)
save(d, "T_DayDeo", flip=False)

# ───────── 8. vật liệu (tên khớp với script Unity)
def matl(name, tex=None, rough=0.75, alpha=False, col=(1, 1, 1)):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True; nt = m.node_tree; b = nt.nodes["Principled BSDF"]
    b.inputs["Roughness"].default_value = rough; b.inputs["Base Color"].default_value = (*col, 1)
    if tex:
        tn = nt.nodes.new("ShaderNodeTexImage"); tn.image = bpy.data.images.load(f"{TX}/{tex}.png", check_existing=True)
        nt.links.new(tn.outputs[0], b.inputs["Base Color"])
        if alpha: nt.links.new(tn.outputs[1], b.inputs["Alpha"]); m.blend_method = "CLIP" if hasattr(m, "blend_method") else None
    return m
MAT = {"Body": matl("M_Nhim_Da", "T_Nhim_Da_1", 0.55), "Eye_L": matl("M_Nhim_Mat", "T_Nhim_Mat_1", 0.1), "Eye_R": None,
       "MouthIn": matl("M_Nhim_Rang", None, 0.4, col=(0.85, 0.82, 0.75)), "Toc": matl("M_Nhim_Toc", "T_Nhim_Toc", 0.85, True),
       "Ao": matl("M_Nhim_Ao", "T_Nhim_Ao", 0.9), "Quan": None, "Bot": matl("M_Nhim_Bot", "T_Nhim_Bot", 0.85),
       "Chan": matl("M_Chan", "T_Chan", 0.9), "ChanVo": None, "Khan_Kho": matl("M_Khan", "T_Khan", 0.8), "Khan_Uot": None,
       "DayDeo": matl("M_DayDeo", "T_DayDeo", 0.9)}
last = None
for k, m in MAT.items():
    m = m or last; last = m
    O[k].data.materials.clear(); O[k].data.materials.append(m)
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/b5.blend")
tot = sum(geo.tris(O[n]) for n in ("Body", "Eye_L", "Eye_R", "MouthIn", "Toc", "Ao", "Quan", "Bot"))
print("TỔNG nhân vật:", tot, "· chăn", geo.tris(O["Chan"]), "· khăn", geo.tris(O["Khan_Kho"]), "· dây", geo.tris(O["DayDeo"]), "· chăn vo", geo.tris(O["ChanVo"]))
