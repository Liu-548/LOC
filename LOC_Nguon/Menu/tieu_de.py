# 6 phương án cách điệu chữ "LỘC" cho màn tiêu đề (22/10). Mỗi ô 900×460, nền = góc trái menu chính.
import numpy as np, sys
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageChops
sys.path.insert(0, "/home/claude/menu")
from ve_menu import nen, phim, F, D, OUT, NGA, MO, SON, W, H   # dùng lại hàm của bản phác menu

FT = f"{D}/fonts"
PW, PH = 900, 460
def ft(ten, co, w=None):
    f = ImageFont.truetype(f"{FT}/{ten}", co)
    if w: f.set_variation_by_axes([w] if "Oswald" in ten or "Prata" in ten else [w])
    return f

def nen_o():
    im = nen("05_dau_dem1_ngoai_cong", 0.45)
    a = np.asarray(im, float); xx = np.linspace(1, 0, W)[None, :, None]; a *= 1 - 0.75 * np.clip(xx * 1.5 - 0.35, 0, 1)
    return Image.fromarray(a.astype(np.uint8)).crop((60, 90, 60 + PW, 90 + PH))

def mask_chu(text, font, pad=40, **kw):
    bb = ImageDraw.Draw(Image.new("L", (1, 1))).textbbox((0, 0), text, font=font, **kw)
    m = Image.new("L", (bb[2] - bb[0] + 2 * pad, bb[3] - bb[1] + 2 * pad), 0)
    ImageDraw.Draw(m).text((pad - bb[0], pad - bb[1]), text, font=font, fill=255, **kw)
    return m

def dan(im, m, mau, xy):
    im.paste(Image.new("RGB", m.size, mau), xy, m)

def kho_but(m, seed=1, manh=0.55):
    """vệt bút khô: khoét lỗ theo sợi ngang"""
    a = np.asarray(m, float) / 255; r = np.random.default_rng(seed)
    soi = r.random((m.height // 3 + 1, 1)).repeat(3, 0)[:m.height] * 0.5 + r.random(m.size[::-1]) * 0.5
    soi = np.asarray(Image.fromarray((soi * 255).astype(np.uint8)).filter(ImageFilter.BoxBlur(1)).resize(m.size), float) / 255
    a = a * np.clip((soi - (1 - manh) * 0.7) * 3, 0, 1)
    return Image.fromarray((a * 255).astype(np.uint8))

# A. Thư pháp bút lông
def A():
    im = nen_o(); m = mask_chu("Lộc", ft("ComforterBrush-Regular.ttf", 300))
    m2 = kho_but(m, 2, 0.93); dan(im, m2.filter(ImageFilter.GaussianBlur(0.8)), NGA, (40, -10))
    d = ImageDraw.Draw(im); d.ellipse([520, 268, 538, 286], fill=SON)          # chấm son như con dấu nhỏ
    return im, "A · Thư pháp bút lông", "Nét bút khô, chữ thường. Chấm son nhỏ thay con dấu."

# B. Giấy niêm son, chữ dọc
def B():
    im = nen_o(); x0, y0, w, h = 110, 18, 150, 430
    r = np.random.default_rng(7)
    giay = np.ones((h, w, 3)) * np.array([214, 192, 140], float) + r.normal(0, 6, (h, w, 1))
    giay *= (0.85 + 0.15 * np.asarray(Image.fromarray((r.random((8, 4)) * 255).astype(np.uint8)).resize((w, h), Image.BICUBIC), float) / 255)[..., None]
    g = Image.fromarray(np.clip(giay, 0, 255).astype(np.uint8))
    m = Image.new("L", (w, h), 255); dm = ImageDraw.Draw(m)
    xs = [w - 60 + i * 6 for i in range(11)]; dm.polygon([(w, h - 70)] + [(x, h - 70 + (i * 13 + r.integers(-6, 6))) for i, x in enumerate(xs)] + [(w, h)], fill=0)   # góc rách
    g2 = ImageDraw.Draw(g)
    f = ft("Prata-Regular.ttf", 112)
    for i, ch in enumerate("LỘC"):
        g2.text((w / 2, 85 + i * 132), ch, font=f, fill=(176, 48, 42), anchor="mm")
    g2.rectangle([10, 8, w - 10, h - 8], outline=(176, 48, 42), width=2)
    sh = Image.new("L", im.size, 0); sh.paste(m.filter(ImageFilter.GaussianBlur(1)), (x0 + 10, y0 + 12)); im.paste((0, 0, 0), (0, 0), sh.filter(ImageFilter.GaussianBlur(16)).point(lambda v: v * 0.35))
    im.paste(g.rotate(-2, expand=False, fillcolor=0), (x0, y0), m.rotate(-2))
    return im, "B · Giấy niêm son", "Chữ dọc mực son trên tờ niêm vàng, góc rách — như tờ niêm trên hũ."

# C. Băng tang / cáo phó
def C():
    im = nen_o(); d = ImageDraw.Draw(im)
    f = ImageFont.truetype(f"{FT}/Oswald[wght].ttf", 190); f.set_variation_by_axes([500])
    s = "L   Ộ   C"
    d.line([60, 95, 760, 95], fill=NGA, width=2); d.line([60, 103, 760, 103], fill=NGA, width=1)
    d.text((410, 245), s, font=f, fill=NGA, anchor="mm")
    d.line([60, 385, 760, 385], fill=NGA, width=1); d.line([60, 393, 760, 393], fill=NGA, width=2)
    return im, "C · Băng tang", "Chữ đứng, giãn cách, kẻ đôi trên dưới — giống băng chữ đám tang ngoài rạp."

# D. Giọt nước thay dấu nặng
def D_():
    im = nen_o(); f = ft("YesevaOne-Regular.ttf", 210)
    m = mask_chu("LÔC", f, pad=30); xy = (40, 10)
    dan(im, m, NGA, xy)
    # dấu nặng = một giọt nước (đọc vẫn ra "LỘC")
    bb = ImageDraw.Draw(Image.new("L", (1, 1))).textbbox((0, 0), "LÔC", font=f)
    wL = ImageDraw.Draw(Image.new("L", (1, 1))).textlength("L", font=f); wO = ImageDraw.Draw(Image.new("L", (1, 1))).textlength("Ô", font=f)
    cx = xy[0] + 30 - bb[0] + wL + wO / 2; cy = xy[1] + 30 - bb[1] + (bb[3] - bb[1]) + 40
    g = Image.new("L", (60, 90), 0); dg = ImageDraw.Draw(g); dg.ellipse([12, 38, 48, 74], fill=255); dg.polygon([(30, 4), (13, 50), (47, 50)], fill=255)
    im.paste((150, 172, 182), (int(cx - 30), int(cy - 45)), g.filter(ImageFilter.GaussianBlur(0.8)))
    # bóng nước gợn bên dưới
    ref = m.transpose(Image.FLIP_TOP_BOTTOM).crop((0, 0, m.width, int(m.height * 0.45)))
    a = np.asarray(ref, float); yy = np.arange(a.shape[0])[:, None]
    a = np.stack([np.roll(a[i], int(6 * np.sin(i / 3.2))) for i in range(a.shape[0])]) * np.clip(1 - yy / a.shape[0] * 1.6, 0, 1) * 0.22
    im.paste(Image.new("RGB", ref.size, (150, 172, 182)), (xy[0], xy[1] + m.height - 40), Image.fromarray(a.astype(np.uint8)))
    return im, "D · Giọt nước", "Dấu nặng thay bằng một giọt nước; dưới chữ có bóng nước gợn (năm chén nước)."

# E. Sơn đỏ trên tường (như chữ BẬT ĐIỆN)
def E():
    im = nen_o(); f = ImageFont.truetype(f"{FT}/Oswald[wght].ttf", 230); f.set_variation_by_axes([700])
    m = mask_chu("LỘC", f, pad=30)
    r = np.random.default_rng(4); a = np.asarray(m, float) / 255
    edge = np.asarray(m.filter(ImageFilter.GaussianBlur(3)), float) / 255
    a = np.clip(a * 0.85 + (edge - 0.5) * 0.6 * (r.random(a.shape) > 0.5), 0, 1)
    yb = int(np.where((a > 0.6).sum(1) > a.shape[1] * 0.2)[0].max()) - 4; cols = np.where(a[yb] > 0.6)[0]
    for x in r.choice(cols, 12, replace=False):
        L = int(r.integers(15, 110)); w = int(r.integers(2, 6))
        a[yb:yb + L, x:x + w] = np.maximum(a[yb:yb + L, x:x + w], 0.95)
        yy, xx = np.ogrid[:a.shape[0], :a.shape[1]]
        a[((yy - (yb + L)) ** 2 + (xx - (x + w / 2)) ** 2) <= (w * 0.9) ** 2] = 0.95   # giọt tròn ở đầu vệt
    a *= 0.75 + 0.25 * r.random(a.shape)
    dan(im, Image.fromarray((a * 255).astype(np.uint8)), (150, 36, 30), (30, 0))
    return im, "E · Sơn đỏ trên tường", "Quét sơn đỏ bằng tay, có vệt chảy — cùng nét với chữ BẬT ĐIỆN cạnh cửa hầm."

# F. Băng VHS rách
def F_():
    im = nen_o(); f = ft("BodoniModa[opsz,wght].ttf", 230)
    m = mask_chu("LỘC", f, pad=30); a = np.asarray(m, float)
    r = np.random.default_rng(9); b = a.copy()
    for _ in range(7):
        y = r.integers(0, a.shape[0] - 20); hh = r.integers(4, 22); dx = r.integers(-28, 28)
        b[y:y + hh] = np.roll(a[y:y + hh], dx, 1)
    mm = Image.fromarray(b.astype(np.uint8)); xy = (40, 0)
    dan(im, mm, (170, 40, 40), (xy[0] - 6, xy[1])); dan(im, mm, (40, 110, 140), (xy[0] + 6, xy[1])); dan(im, mm, NGA, xy)
    d = ImageDraw.Draw(im)
    for y in range(0, PH, 3): d.line([0, y, PW, y], fill=(0, 0, 0), width=1) if r.random() < 0.15 else None
    return im, "F · Băng VHS hỏng", "Chữ bị xé ngang và lệch màu như băng từ chạy sai đầu từ."

ds = [A, B, C, D_, E, F_]
o = []
for fn in ds:
    im, ten, mo = fn()
    big = Image.new("RGB", (W, H)); big.paste(im.resize((W, int(W * PH / PW))), (0, 0))
    im = phim(big, 0.9).crop((0, 0, W, int(W * PH / PW))).resize((PW, PH))
    o.append((im, ten, mo))
cot, gap, top = 2, 30, 110
sheet = Image.new("RGB", (cot * PW + (cot + 1) * gap, 3 * (PH + 90) + top + gap), (246, 243, 238)); d = ImageDraw.Draw(sheet)
d.text((gap, 34), "Chữ LỘC — 6 phương án cách điệu (chọn 1, hoặc ghép ý)", font=F(40, 600), fill=(30, 26, 24))
for k, (im, ten, mo) in enumerate(o):
    x = gap + (k % cot) * (PW + gap); y = top + (k // cot) * (PH + 90)
    sheet.paste(im, (x, y)); d.text((x, y + PH + 10), ten, font=F(26, 600), fill=(30, 26, 24)); d.text((x, y + PH + 48), mo, font=F(20, it=True), fill=(90, 84, 78))
sheet.save(f"{OUT}/14_TieuDe_PhuongAn.png")
