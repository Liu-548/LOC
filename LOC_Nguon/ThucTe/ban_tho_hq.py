# [03/11] Bộ bàn thờ vong dựng lại chi tiết (Blender bpy 5.2) — giữ đúng đồ vật & hoa văn Việt có sẵn, chỉ làm thật hơn:
#  BanThoVong_HQ : bàn phủ khăn trắng ngà, vạt khăn rủ có nếp, bậc kê ảnh phủ khăn
#  BatHuong_HQ   : bát hương sứ men ngà vẽ vàng (hoa văn cánh sen + hồi văn của bản gốc), tro + chân nhang đỏ cũ
#  LoHoaCuc_HQ   : lọ sứ men ngà cắm cúc trắng / vàng có lá
#  BatCom_HQ     : bát cơm đầy, quả trứng luộc, đôi đũa cắm dựng (bát hoa văn xanh của bản gốc)
#  DiaTrauCau_HQ : đĩa sứ, lá trầu têm, quả cau bổ
# Chạy: python3 ban_tho_hq.py -- <thư mục ảnh gốc (Image_1/Image_3.png)> <ra.glb>
import bpy, bmesh, sys, math, random, os
import numpy as np
from mathutils import Vector, Matrix

a = sys.argv[sys.argv.index('--') + 1:]; ANH, OUT = a[0], a[1]
rng = random.Random(1975); nrng = np.random.default_rng(1975)
bpy.ops.wm.read_factory_settings(use_empty=True)
sc = bpy.context.scene

def srgb(h):
    h = h.lstrip('#'); c = [int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    return tuple(x / 12.92 if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4 for x in c)

def anh_np(ten, arr):
    """mảng HxWx3 (0..1, gốc trên-trái) → ảnh Blender (đóng gói vào glb)"""
    h, w = arr.shape[:2]
    im = bpy.data.images.new(ten, w, h, alpha=False)
    px = np.concatenate([arr[::-1], np.ones((h, w, 1))], -1).astype(np.float32).ravel()
    im.pixels.foreach_set(px); im.pack(); return im

def vat_lieu(ten, mau=None, anh=None, nham=0.6, kl=0.0):
    m = bpy.data.materials.new(ten); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Roughness"].default_value = nham; b.inputs["Metallic"].default_value = kl
    if anh is not None:
        t = m.node_tree.nodes.new("ShaderNodeTexImage"); t.image = anh
        m.node_tree.links.new(t.outputs["Color"], b.inputs["Base Color"])
    else: b.inputs["Base Color"].default_value = (*mau, 1)
    return m

# ───── texture
def nhieu(h, w, beta, seed):
    r = np.random.default_rng(seed); fx = np.fft.fftfreq(h)[:, None]; fy = np.fft.fftfreq(w)[None, :]
    f = np.sqrt(fx * fx + fy * fy); f[0, 0] = 1
    x = np.real(np.fft.ifft2(np.exp(1j * r.uniform(0, 6.283, (h, w))) / f ** (beta / 2))); x -= x.min(); return x / (x.max() + 1e-9)

def tex_khan():
    n = 512; y, x = np.mgrid[0:n, 0:n]
    det = 0.03 * (np.sin(x * 2 * np.pi / 3) * np.sin(y * 2 * np.pi / 3))           # sợi dệt
    o = nhieu(n, n, 2.2, 3); bui = np.clip((o - 0.6) * 1.5, 0, 1) * 0.08            # ố nhẹ
    v = 0.86 + det - bui + (nhieu(n, n, 0.5, 4) - 0.5) * 0.03
    return np.stack([v * 1.0, v * 0.97, v * 0.9], -1)

def tex_su(goc, ten):
    """hoa văn bản gốc → phóng 4 lần, nét mềm như vẽ tay, men ngà loang + rạn mảnh"""
    from PIL import Image, ImageFilter
    im = Image.open(goc).convert("RGB"); w, h = im.size
    im = im.resize((w * 4, h * 4), Image.BICUBIC).filter(ImageFilter.GaussianBlur(1.6))
    A = np.asarray(im).astype(float) / 255; H, W = A.shape[:2]
    men = (nhieu(H, W, 2.0, 7)[..., None] - 0.5) * 0.05
    ran = np.zeros((H, W))
    for _ in range(60):   # rạn men
        px, py = nrng.uniform(0, W), nrng.uniform(0, H); g = nrng.uniform(0, 6.28)
        for _k in range(nrng.integers(20, 80)):
            g += nrng.normal(0, 0.4); px += math.cos(g) * 2; py += math.sin(g) * 2
            if 0 <= px < W and 0 <= py < H: ran[int(py), int(px)] = 1
    A = np.clip(A + men - ran[..., None] * 0.07, 0, 1)
    return A

def tex_go():
    n = 512; y, x = np.mgrid[0:n, 0:n] / n
    van = np.sin((x * 40 + nhieu(n, n, 2.5, 9) * 6) * np.pi) * 0.5 + 0.5
    v = 0.25 + 0.08 * van
    return np.stack([v * 1.3, v * 0.75, v * 0.5], -1)

KHAN = vat_lieu("Khan_Trang_Nga_HQ", anh=anh_np("T_KhanTrang", tex_khan()), nham=0.85)
SU_HUONG = vat_lieu("Su_Lu_Huong_HQ", anh=anh_np("T_SuLuHuong", tex_su(os.path.join(ANH, "Image_1.png"), "h")), nham=0.25)
SU_BAT = vat_lieu("Su_Bat_Com_HQ", anh=anh_np("T_SuBatCom", tex_su(os.path.join(ANH, "Image_3.png"), "b")), nham=0.25)
SU_NGA = vat_lieu("Su_Men_Nga_HQ", srgb("#E9E1CC"), nham=0.22)
TRO = vat_lieu("Tro_HQ", srgb("#8C8478"), nham=0.95)
NHANG = vat_lieu("Chan_Nhang_HQ", srgb("#9E2A22"), nham=0.7)
COM = vat_lieu("Com_HQ", srgb("#F1EDE2"), nham=0.8)
TRUNG = vat_lieu("Trung_HQ", srgb("#F4F0E6"), nham=0.35)
DUA = vat_lieu("Dua_HQ", srgb("#C8A472"), nham=0.6)
LA_TRAU = vat_lieu("La_Trau_HQ", srgb("#3E6B2C"), nham=0.45)
CAU = vat_lieu("Qua_Cau_HQ", srgb("#C99A3B"), nham=0.5)
CAU_TRONG = vat_lieu("Ruot_Cau_HQ", srgb("#D9C7A0"), nham=0.8)
CUC_T = vat_lieu("Cuc_Trang_HQ", srgb("#EFEBE1"), nham=0.7)
CUC_V = vat_lieu("Cuc_Vang_HQ", srgb("#E2C24E"), nham=0.7)
NHUY = vat_lieu("Nhuy_HQ", srgb("#B8A04A"), nham=0.8)
LA = vat_lieu("La_Cuc_HQ", srgb("#3D5434"), nham=0.6)
CANH = vat_lieu("Canh_Cuc_HQ", srgb("#4F6238"), nham=0.7)

class Luoi:
    def __init__(s): s.bm = bmesh.new(); s.uv = s.bm.loops.layers.uv.new("UVMap"); s.mats = []
    def mi(s, m):
        if m not in s.mats: s.mats.append(m)
        return s.mats.index(m)
    def mat(s, vs, m, uvs=None, mem=True):
        f = s.bm.faces.new([s.bm.verts.new(v) for v in vs]); f.material_index = s.mi(m); f.smooth = mem
        if uvs:
            for l, u in zip(f.loops, uvs): l[s.uv].uv = u
        return f
    def luoi(s, P, m, U=None, kin_u=False, mem=True):
        """lưới tứ giác từ mảng điểm P[i][j]"""
        n, k = len(P), len(P[0]); V = [[s.bm.verts.new(p) for p in r] for r in P]
        nn = n if kin_u else n - 1
        for i in range(nn):
            for j in range(k - 1):
                i2 = (i + 1) % n
                f = s.bm.faces.new((V[i][j], V[i2][j], V[i2][j + 1], V[i][j + 1])); f.material_index = s.mi(m); f.smooth = mem
                if U:
                    uu = [U[i][j], U[i2][j], U[i2][j + 1], U[i][j + 1]]
                    if kin_u and i2 == 0: uu[1] = (U[i][j][0] + (U[1][0][0] - U[0][0][0]), U[i2][j][1]); uu[2] = (uu[1][0], U[i2][j + 1][1])
                    for l, u in zip(f.loops, uu): l[s.uv].uv = u
    def tien(s, prof, m, so=40, u0=0.0, v_lap=1.0, x0=0, y0=0):
        """tiện tròn theo mặt cắt prof = [(r, z), ...] (dưới lên)"""
        P = [[Vector((x0 + r * math.cos(2 * math.pi * i / so), y0 + r * math.sin(2 * math.pi * i / so), z)) for (r, z) in prof] for i in range(so)]
        L = [0]; 
        for q in range(1, len(prof)): L.append(L[-1] + math.hypot(prof[q][0] - prof[q - 1][0], prof[q][1] - prof[q - 1][1]))
        U = [[(i / so, u0 + L[j] / max(L[-1], 1e-6) * v_lap) for j in range(len(prof))] for i in range(so)]
        s.luoi(P, m, U, kin_u=True)
    def xong(s, ten):
        bmesh.ops.remove_doubles(s.bm, verts=s.bm.verts, dist=1e-6)
        me = bpy.data.meshes.new(ten); s.bm.to_mesh(me); s.bm.free()
        for m in s.mats: me.materials.append(m)
        o = bpy.data.objects.new(ten, me); sc.collection.objects.link(o); return o

# ═════ 1. bàn thờ phủ khăn: mặt 1,2 × 0,6 cao 0,752; vạt rủ chạm đất có nếp; bậc kê ảnh
def ban_tho():
    L = Luoi(); W, D, H = 1.2, 0.6, 0.752; bo = 0.035
    # đường chu vi bo góc
    def chu_vi(w, d, r, n=480):
        pts = []; per = 2 * (w + d - 4 * r) + 2 * math.pi * r
        for i in range(n):
            t = per * i / n; segs = [(w - 2 * r, (r - w / 2, -d / 2), (1, 0)), (math.pi * r / 2, None, 0), (d - 2 * r, (w / 2, r - d / 2), (0, 1)), (math.pi * r / 2, None, 1),
                                     (w - 2 * r, (w / 2 - r, d / 2), (-1, 0)), (math.pi * r / 2, None, 2), (d - 2 * r, (-w / 2, d / 2 - r), (0, -1)), (math.pi * r / 2, None, 3)]
            for (ln, p0, dd) in segs:
                if t <= ln:
                    if p0 is not None: x, y = p0[0] + dd[0] * t, p0[1] + dd[1] * t; nx, ny = dd[1], -dd[0]
                    else:
                        c = [(w / 2 - r, -d / 2 + r), (w / 2 - r, d / 2 - r), (-w / 2 + r, d / 2 - r), (-w / 2 + r, -d / 2 + r)][dd]
                        g = -math.pi / 2 + dd * math.pi / 2 + t / r; x, y = c[0] + r * math.cos(g), c[1] + r * math.sin(g); nx, ny = math.cos(g), math.sin(g)
                    pts.append((x, y, nx, ny, i / n)); break
                t -= ln
        return pts
    cv = chu_vi(W + 2 * bo, D + 2 * bo, 0.04)
    # vạt khăn: nếp dọc (biên độ tăng dần xuống dưới), chân vạt hơi xoè, mép gấu lượn sóng
    rows = 34; P = []; U = []
    pha = [rng.uniform(0, 6.28) for _ in range(6)]
    for (x, y, nx, ny, t) in cv:
        col = []; ucol = []
        for j in range(rows + 1):
            v = j / rows; z = H + 0.004 - v * (H - 0.002)
            A = 0.003 + 0.028 * v ** 1.6
            nep = sum(math.sin(2 * math.pi * (t * f) + pha[k]) / (k + 1) for k, f in enumerate((38, 61, 97, 23)))
            d = A * nep * 0.55 + v ** 3 * 0.02
            gau = 0.006 * math.sin(2 * math.pi * t * 53 + pha[4]) * (v > 0.97)
            col.append(Vector((x + nx * d, y + ny * d, z + gau)))
            ucol.append((t * 14, (1 - v) * 2.2))
        P.append(col); U.append(ucol)
    L.luoi(P, KHAN, U, kin_u=True)
    # mặt bàn phủ khăn + mép uốn tròn xuống vạt
    top = [(x, y) for (x, y, nx, ny, t) in cv]   # cùng đỉnh với mép vạt → không hở đường tối
    c = L.mat([Vector((x * 0.0, y * 0.0, H + 0.004)) for (x, y) in top[:1]] and [Vector((x, y, H + 0.004)) for (x, y) in top], KHAN, [(x * 2 + 1, y * 2 + 1) for (x, y) in top])
    # bậc kê ảnh (phía sau, giữa): 0,6 × 0,21 × 0,13 phủ khăn, vạt trước có nếp nhỏ
    bx0, bx1, by0, by1, bz = -0.30, 0.30, 0.09, 0.30, H + 0.13
    nx_ = 40; P2 = []; U2 = []
    for i in range(nx_ + 1):
        x = bx0 + (bx1 - bx0) * i / nx_; col = []; ucol = []
        for j in range(9):
            v = j / 8; z = bz - v * 0.13; dy = -(0.002 + 0.008 * v) * (math.sin(i * 1.7 + 1.3) * 0.6 + math.sin(i * 0.63) * 0.4)
            col.append(Vector((x, by0 + dy - 0.004, z))); ucol.append((i / nx_ * 3, v))
        P2.append(col); U2.append(ucol)
    L.luoi(P2, KHAN, U2)
    L.mat([Vector((bx0, by0 - 0.004, bz)), Vector((bx1, by0 - 0.004, bz)), Vector((bx1, by1, bz)), Vector((bx0, by1, bz))], KHAN, [(0, 0), (3, 0), (3, 1), (0, 1)], mem=False)
    for x in (bx0, bx1):
        L.mat([Vector((x, by0 - 0.004, H)), Vector((x, by1, H)), Vector((x, by1, bz)), Vector((x, by0 - 0.004, bz))][::(1 if x > 0 else -1)], KHAN, [(0, 0), (1, 0), (1, 1), (0, 1)], mem=False)
    return L.xong("BanThoVong_HQ")

# ═════ 2. bát hương
def bat_huong():
    L = Luoi()
    prof = [(0.0, 0.0), (0.052, 0.0), (0.056, 0.004), (0.052, 0.012), (0.066, 0.016), (0.084, 0.04), (0.092, 0.07), (0.094, 0.095), (0.100, 0.104), (0.098, 0.110), (0.088, 0.108), (0.084, 0.095)]
    L.tien(prof, SU_HUONG, so=64)
    # tro: mặt nhấp nhô
    n = 24; P = []
    for i in range(n):
        g = 2 * math.pi * i / n; col = []
        for j in range(8):
            r = 0.084 * j / 7; z = 0.088 + 0.004 * math.sin(i * 1.3 + j * 2.1) * (j / 7) + 0.003 * (1 - j / 7)
            col.append(Vector((r * math.cos(g), r * math.sin(g), z)))
        P.append(col)
    L.luoi(P, TRO, kin_u=True)
    # chân nhang cũ: que đỏ cắm lệch, cao thấp khác nhau
    for k in range(34):
        r = math.sqrt(rng.random()) * 0.06; g = rng.uniform(0, 6.283); h = rng.uniform(0.015, 0.07)
        x, y = r * math.cos(g), r * math.sin(g); tx, ty = rng.uniform(-0.12, 0.12), rng.uniform(-0.12, 0.12)
        for (dx, dy) in ((0.0013, 0), (0, 0.0013), (-0.0013, 0), (0, -0.0013)):
            pass
        q = [Vector((x + dx, y + dy, 0.086)) for (dx, dy) in ((0.0013, 0), (0, 0.0013), (-0.0013, 0), (0, -0.0013))]
        t = Vector((tx * h, ty * h, h))
        for i in range(4):
            L.mat([q[i], q[(i + 1) % 4], q[(i + 1) % 4] + t, q[i] + t], NHANG)
    return L.xong("BatHuong_HQ")

# ═════ cúc (dùng lại cách dựng cánh của vòng hoa)
def he_truc(z):
    z = z.normalized(); x = Vector((0, 0, 1)).cross(z)
    if x.length < 1e-4: x = Vector((1, 0, 0))
    x.normalize(); y = z.cross(x); return Matrix((x, y, z)).transposed()

def bong_cuc(L, goc, truc, s, m):
    M = he_truc(truc) @ Matrix.Rotation(rng.uniform(0, 6.283), 3, 'Z')
    for k, (so, dai_c, ng, rong, cuon) in enumerate(((9, 0.40, 80, 0.34, 25), (12, 0.62, 62, 0.30, 10), (14, 0.84, 40, 0.27, -15), (16, 1.0, 16, 0.24, -30))):
        for i in range(so):
            a = 2 * math.pi * (i + 0.5 * k + rng.uniform(-0.25, 0.25)) / so
            Ln = s * dai_c * rng.uniform(0.85, 1.1); w = s * rong * rng.uniform(0.85, 1.15); el0 = math.radians(ng + rng.uniform(-8, 8))
            d = Vector((math.cos(a), math.sin(a), 0)); ng_ = Vector((-math.sin(a), math.cos(a), 0)); hang = []
            for j, t in enumerate((0.0, 0.5, 1.0)):
                el = el0 + t * math.radians(cuon); r = s * 0.12 + Ln * t * math.cos(el); hh = s * 0.12 + Ln * t * math.sin(el)
                ww = w * (0.45, 1.0, 0.55)[j]; c = d * r + Vector((0, 0, hh))
                ph = Vector((-math.sin(el) * math.cos(a), -math.sin(el) * math.sin(a), math.cos(el)))
                hang.append([goc + M @ (c + ng_ * ww / 2), goc + M @ (c - ph * ww * 0.35), goc + M @ (c - ng_ * ww / 2)])
            L.luoi([[hang[0][q], hang[1][q], hang[2][q]] for q in range(3)], m)

# ═════ 3. lọ hoa cúc
def lo_hoa():
    L = Luoi()
    prof = [(0.0, 0.0), (0.045, 0.0), (0.05, 0.006), (0.07, 0.05), (0.078, 0.11), (0.07, 0.17), (0.045, 0.22), (0.032, 0.26), (0.036, 0.29), (0.044, 0.30), (0.04, 0.302), (0.03, 0.29)]
    L.tien(prof, SU_NGA, so=48)
    for k in range(7):
        g = 2 * math.pi * k / 7 + rng.uniform(-0.3, 0.3); nghieng = rng.uniform(0.15, 0.45) if k else 0.05
        dau = Vector((0, 0, 0.29)); dinh = dau + Vector((math.cos(g) * nghieng * 0.25, math.sin(g) * nghieng * 0.25, rng.uniform(0.11, 0.17)))
        # cành
        for q in range(4):
            g2 = q * math.pi / 2; o = Vector((math.cos(g2), math.sin(g2), 0)) * 0.0025
            L.mat([dau + o, dau + o.cross(Vector((0, 0, 1))), dinh + o.cross(Vector((0, 0, 1))), dinh + o], CANH)
        # lá cúc (khía) ở lưng chừng cành
        p = dau.lerp(dinh, 0.55); hl = Vector((math.cos(g + 1.2), math.sin(g + 1.2), -0.3)).normalized()
        n_ = Vector((0, 0, 1)).cross(hl).normalized()
        L.mat([p, p + hl * 0.03 + n_ * 0.014, p + hl * 0.05 + n_ * 0.006, p + hl * 0.065, p + hl * 0.05 - n_ * 0.006, p + hl * 0.03 - n_ * 0.014], LA)
        bong_cuc(L, dinh, (dinh - dau).normalized() * 0.6 + Vector((0, -0.4, 0.3)), rng.uniform(0.038, 0.048), CUC_V if k % 3 == 0 else CUC_T)
    return L.xong("LoHoaCuc_HQ")

# ═════ 4. bát cơm quả trứng, đôi đũa cắm dựng
def bat_com():
    L = Luoi()
    prof = [(0.0, 0.0), (0.028, 0.0), (0.03, 0.008), (0.034, 0.01), (0.05, 0.022), (0.06, 0.042), (0.062, 0.058), (0.06, 0.06), (0.055, 0.045), (0.04, 0.02)]
    L.tien(prof, SU_BAT, so=48)
    # cơm vun ngọn (nhấp nhô hạt)
    n = 32; P = []
    for i in range(n):
        g = 2 * math.pi * i / n; col = []
        for j in range(10):
            t = j / 9; r = 0.058 * (1 - t) ** 0.8; z = 0.05 + 0.04 * (1 - (1 - t) ** 2) + 0.0025 * math.sin(i * 2.7 + j * 3.3)
            col.append(Vector((r * math.cos(g), r * math.sin(g), z)))
        P.append(col)
    L.luoi(P, COM, kin_u=True)
    # quả trứng luộc đặt trên ngọn
    n = 20; P = []
    for i in range(n):
        g = 2 * math.pi * i / n; col = []
        for j in range(11):
            b = -math.pi / 2 + math.pi * j / 10; rz = 0.021 if b < 0 else 0.026
            col.append(Vector((0.017 * math.cos(b) * math.cos(g), 0.017 * math.cos(b) * math.sin(g), 0.112 + rz * math.sin(b))))
        P.append(col)
    L.luoi(P, TRUNG, kin_u=True)
    # đôi đũa cắm thẳng đứng (đũa tre vuông thon đầu)
    for dx in (-0.006, 0.006):
        for q in range(4):
            g = q * math.pi / 2; o0 = Vector((math.cos(g), math.sin(g), 0)); o1 = Vector((math.cos(g + math.pi / 2), math.sin(g + math.pi / 2), 0))
            b0 = Vector((dx, 0.012, 0.03)); b1 = Vector((dx * 1.2, 0.014, 0.245))   # cắm sau quả trứng, giữa bát
            L.mat([b0 + o0 * 0.0022, b0 + o1 * 0.0022, b1 + o1 * 0.0032, b1 + o0 * 0.0032], DUA)
    return L.xong("BatCom_HQ")

# ═════ 5. đĩa trầu cau
def dia_trau():
    L = Luoi()
    prof = [(0.0, 0.0), (0.05, 0.0), (0.055, 0.004), (0.085, 0.01), (0.092, 0.016), (0.088, 0.017), (0.06, 0.008), (0.0, 0.006)]
    L.tien(prof, SU_NGA, so=56)
    for k in range(4):   # lá trầu hình tim, cong
        g = 2 * math.pi * k / 4 + 0.4; c = Vector((math.cos(g) * 0.03, math.sin(g) * 0.03, 0.009 + k * 0.002)); d = Vector((math.cos(g), math.sin(g), 0)); nn = Vector((-d.y, d.x, 0))
        pts = []
        for i in range(13):
            t = i / 12 * 2 * math.pi; x = 16 * math.sin(t) ** 3; y = 13 * math.cos(t) - 5 * math.cos(2 * t) - 2 * math.cos(3 * t) - math.cos(4 * t)
            p = c + nn * (x * 0.0022) + d * (y * 0.0022 + 0.02); p.z += 0.006 * (x / 16) ** 2
            pts.append(p)
        L.mat(pts[:-1], LA_TRAU)
    for k in range(3):   # quả cau bổ đôi
        g = 2 * math.pi * k / 3 + 1.0; c = Vector((math.cos(g) * 0.045, math.sin(g) * 0.045, 0.012))
        n = 14; P = []
        for i in range(n + 1):
            u = math.pi * i / n; col = []
            for j in range(9):
                b = -math.pi / 2 + math.pi * j / 8
                col.append(c + Vector((0.016 * math.cos(b) * math.cos(u), 0.016 * math.cos(b) * math.sin(u) * 0.1 + 0.0, 0.0)) + Vector((0, 0, 0.012 * (math.cos(b) * math.sin(u)))) + Vector((0, 0.02 * math.sin(b), 0)))
            P.append(col)
        L.luoi(P, CAU)
        L.mat([c + Vector((0.016 * math.cos(math.pi * i / 10) * 0.95, 0.02 * math.sin(math.pi * i / 10 - math.pi / 2) * 0.95, 0.0005)) for i in range(11)], CAU_TRONG)
    return L.xong("DiaTrauCau_HQ")

objs = [ban_tho(), bat_huong(), lo_hoa(), bat_com(), dia_trau()]
for i, o in enumerate(objs): o.location.x = i * 1.6   # tách nhau trong file (Unity khớp theo hộp bao, không theo vị trí)
for o in objs: print(o.name, "tris", sum(len(p.vertices) - 2 for p in o.data.polygons))
bpy.ops.object.select_all(action='DESELECT')
for o in objs: o.select_set(True)
bpy.ops.export_scene.gltf(filepath=OUT, use_selection=True, export_format='GLB', export_yup=True, export_apply=True)
print("xong")
