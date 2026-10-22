# Ghép A (thư pháp bút lông) + E (sơn đỏ, vệt chảy) — 22/10
import numpy as np, sys
from PIL import Image, ImageDraw, ImageFont, ImageFilter
sys.path.insert(0, "/home/claude/menu")
from ve_menu import phim, F, D, OUT, W, H, MO
from tieu_de import nen_o, mask_chu, kho_but, ft, PW, PH

DO = (186, 40, 32)
def chay(a, r, n, dai):
    """vệt sơn chảy xuống từ đáy các nét, đầu vệt có giọt tròn"""
    cot = (a > 0.6).any(0); yy, xx = np.ogrid[:a.shape[0], :a.shape[1]]
    xs = np.where(cot)[0]
    for x in r.choice(xs, n, replace=False):
        y0 = int(np.where(a[:, x] > 0.6)[0].max()) - 2      # đáy nét tại cột đó
        L = int(r.integers(*dai)); w = int(r.integers(2, 6))
        a[y0:y0 + L, x:x + w] = np.maximum(a[y0:y0 + L, x:x + w], 0.92)
        a[((yy - (y0 + L)) ** 2 + (xx - (x + w / 2)) ** 2) <= (w * 0.95) ** 2] = 0.92
    return a

def bien_the(kho, n_chay, dai, seed, ten, mota):
    im = nen_o(); r = np.random.default_rng(seed)
    m = mask_chu("Lộc", ft("ComforterBrush-Regular.ttf", 300), pad=40)
    m = Image.fromarray(np.pad(np.asarray(m), ((0, 140), (0, 0))))           # chừa chỗ cho vệt chảy
    if kho: m = kho_but(m, seed, kho)
    a = np.asarray(m, float) / 255
    a = chay(a, r, n_chay, dai)
    a *= 0.78 + 0.22 * r.random(a.shape)                                     # sơn không đều
    a = np.asarray(Image.fromarray((a * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(0.7)), float)
    im.paste(Image.new("RGB", m.size, DO), (40, -10), Image.fromarray(a.astype(np.uint8)))
    return im, ten, mota

vs = [bien_the(0.93, 10, (20, 120), 3, "G1 · Bút khô + vệt chảy", "Nét bút lông khô, sơn đỏ, chảy nhiều."),
      bien_the(None, 6, (15, 90), 5, "G2 · Bút đẫm sơn", "Nét đầy, ít vệt chảy hơn, dễ đọc ở cỡ nhỏ."),
      bien_the(0.85, 4, (40, 170), 8, "G3 · Bút khô, ít vệt nhưng dài", "Nét xơ hơn, chỉ vài vệt chảy dài xuống.")]
o = []
for im, ten, mo in vs:
    big = Image.new("RGB", (W, H)); big.paste(im.resize((W, int(W * PH / PW))), (0, 0))
    o.append((phim(big, 0.9).crop((0, 0, W, int(W * PH / PW))).resize((PW, PH)), ten, mo))
gap, top = 30, 110
sheet = Image.new("RGB", (3 * PW + 4 * gap, PH + 90 + top + gap), (246, 243, 238)); d = ImageDraw.Draw(sheet)
d.text((gap, 34), "Chữ LỘC — ghép A + E (thư pháp, sơn đỏ, vệt chảy)", font=F(40, 600), fill=(30, 26, 24))
for k, (im, ten, mo) in enumerate(o):
    x = gap + k * (PW + gap); sheet.paste(im, (x, top))
    d.text((x, top + PH + 10), ten, font=F(26, 600), fill=(30, 26, 24)); d.text((x, top + PH + 48), mo, font=F(20, it=True), fill=(90, 84, 78))
sheet.save(f"{OUT}/15_TieuDe_AE.png")

# menu chính đầy đủ với G1
import ve_menu as V
def menu_g(idx, out):
    im = V.nen("05_dau_dem1_ngoai_cong", 0.45)
    a = np.asarray(im, float); xx = np.linspace(1, 0, W)[None, :, None]; a *= 1 - 0.75 * np.clip(xx * 1.5 - 0.35, 0, 1)
    im = Image.fromarray(a.astype(np.uint8))
    t, _, _ = [bien_the(0.93, 10, (20, 120), 3, "", ""), bien_the(None, 6, (15, 90), 5, "", ""), bien_the(0.85, 4, (40, 170), 8, "", "")][idx]
    base = nen_o(); diff = np.abs(np.asarray(t, int) - np.asarray(base, int)).sum(2) > 6     # lấy riêng phần chữ
    patch = Image.fromarray(np.asarray(t)); im.paste(patch, (60, 90), Image.fromarray((diff * 255).astype(np.uint8)))
    d = ImageDraw.Draw(im)
    V.muc_menu(d, 160, 560, [("Tiếp tục", "Đêm 2 · 7/8/2002"), ("Bắt đầu", ""), ("Chọn đêm", ""), ("Ending", "mở sau khi hoàn thành game"), ("Cài đặt", ""), ("Thoát", "")], 0, khoa=("Ending",))
    V.chu(d, (W - 110, H - 140), "Nhà tôi nuôi nó mười ba năm.", V.F(26, it=True), (190, 182, 166), "ra")
    V.chu(d, (W - 110, H - 100), "Người mang nó tới thì chưa bao giờ đi.", V.F(26, it=True), (190, 182, 166), "ra")
    V.goi_y_phim(d, ["↑↓ / chuột  chọn", "Enter / nhấp  vào"], x=160)
    im = V.phim(im, 1.0); V.nhan(ImageDraw.Draw(im), "02", "Menu chính"); im.save(out)
menu_g(0, f"{OUT}/02_G1.png")
