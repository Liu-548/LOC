# Xuất tài nguyên menu cho Unity (22/10): font Noto Serif tĩnh + chữ tiêu đề "Lộc" kiểu G1 (nền trong suốt).
# Chạy: python3 xuat_unity.py <thư mục fonts> <thư mục ra>   (ra = Assets/LOC_House/Resources/LOC_Menu)
import sys, numpy as np
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
from PIL import Image, ImageDraw, ImageFont, ImageFilter

FT, OUT = sys.argv[1], sys.argv[2]

# Noto Serif là font biến thiên — Unity đọc ổn định hơn với bản tĩnh
for src, ten, w in (("NotoSerif[wdth,wght].ttf", "NotoSerif-Regular", 400), ("NotoSerif[wdth,wght].ttf", "NotoSerif-SemiBold", 600),
                    ("NotoSerif-Italic[wdth,wght].ttf", "NotoSerif-Italic", 400)):
    f = instantiateVariableFont(TTFont(f"{FT}/{src}"), {"wght": w, "wdth": 100}, updateFontNames=True)
    f.save(f"{OUT}/{ten}.ttf")

# ── chữ "Lộc" G1: bút lông khô (Comforter Brush), sơn đỏ #BA2820, vệt chảy — cùng tham số tieu_de_ae.py (seed 3)
def mask_chu(text, font, pad=40):
    bb = ImageDraw.Draw(Image.new("L", (1, 1))).textbbox((0, 0), text, font=font)
    m = Image.new("L", (bb[2] - bb[0] + 2 * pad, bb[3] - bb[1] + 2 * pad), 0)
    ImageDraw.Draw(m).text((pad - bb[0], pad - bb[1]), text, font=font, fill=255)
    return m

def kho_but(m, seed, manh):
    a = np.asarray(m, float) / 255; r = np.random.default_rng(seed)
    soi = r.random((m.height // 3 + 1, 1)).repeat(3, 0)[:m.height] * 0.5 + r.random(m.size[::-1]) * 0.5
    soi = np.asarray(Image.fromarray((soi * 255).astype(np.uint8)).filter(ImageFilter.BoxBlur(1)).resize(m.size), float) / 255
    return Image.fromarray((a * np.clip((soi - (1 - manh) * 0.7) * 3, 0, 1) * 255).astype(np.uint8))

def chay(a, r, n, dai):
    cot = (a > 0.6).any(0); yy, xx = np.ogrid[:a.shape[0], :a.shape[1]]
    for x in r.choice(np.where(cot)[0], n, replace=False):
        y0 = int(np.where(a[:, x] > 0.6)[0].max()) - 2
        L = int(r.integers(*dai)); w = int(r.integers(2, 6))
        a[y0:y0 + L, x:x + w] = np.maximum(a[y0:y0 + L, x:x + w], 0.92)
        a[((yy - (y0 + L)) ** 2 + (xx - (x + w / 2)) ** 2) <= (w * 0.95) ** 2] = 0.92
    return a

r = np.random.default_rng(3)
m = mask_chu("Lộc", ImageFont.truetype(f"{FT}/ComforterBrush-Regular.ttf", 300))
m = Image.fromarray(np.pad(np.asarray(m), ((0, 140), (0, 0))))
a = np.asarray(kho_but(m, 3, 0.93), float) / 255
a = chay(a, r, 10, (20, 120))
a *= 0.78 + 0.22 * r.random(a.shape)
a = np.asarray(Image.fromarray((a * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(0.7)))
ys, xs = np.where(a > 4); a = a[max(0, ys.min() - 6):ys.max() + 7, max(0, xs.min() - 6):xs.max() + 7]
rgba = np.zeros(a.shape + (4,), np.uint8); rgba[..., :3] = (186, 40, 32); rgba[..., 3] = a
Image.fromarray(rgba, "RGBA").save(f"{OUT}/TieuDe_Loc.png")
print("ok", a.shape)
