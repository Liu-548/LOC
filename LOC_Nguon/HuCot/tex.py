# Texture hũ cốt: men sành da lươn, giấy dầu, tờ niêm son (chữ Hán), tóc
import numpy as np, random
from PIL import Image, ImageDraw, ImageFont, ImageFilter
rng = np.random.default_rng(7); random.seed(7)
O = "out/"

def noise(w, h, s, oct=4):
    a = np.zeros((h, w))
    for o in range(oct):
        f = 2 ** o; sw, sh = max(2, int(w / s * f)), max(2, int(h / s * f))
        n = Image.fromarray((rng.random((sh, sw)) * 255).astype(np.uint8)).resize((w, h), Image.BICUBIC)
        a += np.asarray(n, float) / 255 / f
    return a / sum(1 / 2 ** o for o in range(oct))

# 1. men sành: u = vòng quanh (lặp), v = 0 đáy → 1 miệng. Nâu đen ở vai, loang nâu đỏ ở bụng, chảy men dọc, đốm sạn
W, H = 1024, 1024
n1 = noise(W, H, 180); n2 = noise(W, H, 40)
v = np.linspace(1, 0, H)[:, None]  # hàng 0 = trên ảnh = v 1 (miệng)
base = np.array([62, 36, 22]) / 255; vai = np.array([34, 20, 13]) / 255; bung = np.array([96, 54, 30]) / 255
t = np.clip((v - 0.25) / 0.5, 0, 1)                      # bụng → vai
col = bung * (1 - t[..., None]) + vai * t[..., None]
col = col * (0.75 + 0.5 * n1[..., None]) + (n2[..., None] - 0.5) * 0.06
# vệt men chảy từ vai xuống
img = col.copy()
for k in range(70):
    x = rng.integers(0, W); L = rng.uniform(0.15, 0.5); w = rng.integers(3, 12)
    y0 = int((1 - rng.uniform(0.62, 0.8)) * H); y1 = min(H - 1, y0 + int(L * H))
    for y in range(y0, y1):
        fall = 1 - (y - y0) / (y1 - y0)
        xs = slice(max(0, x - w // 2), min(W, x + w // 2 + 1))
        img[y, xs] = img[y, xs] * (1 - 0.35 * fall) + vai * 0.35 * fall
# đốm sạn + rỗ men
for k in range(350):
    x, y = rng.integers(0, W), rng.integers(0, H); r = 1
    c = np.array([150, 110, 75]) / 255 if rng.random() < 0.5 else np.array([20, 12, 8]) / 255
    img[max(0, y - r):y + r, max(0, x - r):x + r] = img[max(0, y - r):y + r, max(0, x - r):x + r] * 0.4 + c * 0.6
# mép men đọng ở chân (v 0.08–0.12): viền dày sẫm, lượn sóng
for x in range(W):
    yb = int((1 - (0.10 + 0.02 * np.sin(x / W * 2 * np.pi * 7) + 0.01 * rng.random())) * H)
    img[yb - 6:yb, x] = vai * 0.8
    img[yb:, x] = np.array([128, 84, 56]) / 255 * (0.8 + 0.3 * n1[yb:, x][:, None])   # đất nung lộ ra dưới chân
Image.fromarray((np.clip(img, 0, 1) * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(0.6)).save(O + "Hu_Men.png")

# 2. đất nung (miệng hũ không men, lòng hũ)
d = np.array([118, 78, 52]) / 255 * (0.7 + 0.5 * noise(512, 512, 60)[..., None])
Image.fromarray((np.clip(d, 0, 1) * 255).astype(np.uint8)).save(O + "Hu_DatNung.png")

# 3. giấy dầu: nâu vàng ngả, thấm dầu loang, xơ giấy, nếp nhăn
W = H = 512
g = np.array([128, 92, 50]) / 255 * (0.78 + 0.4 * noise(W, H, 90)[..., None])
for k in range(40):  # vết dầu loang sẫm
    cx, cy, r = rng.integers(0, W), rng.integers(0, H), rng.integers(20, 90)
    yy, xx = np.mgrid[0:H, 0:W]; m = np.clip(1 - np.hypot(xx - cx, yy - cy) / r, 0, 1) ** 2
    g = g * (1 - 0.35 * m[..., None]) + np.array([60, 40, 20]) / 255 * 0.35 * m[..., None]
im = Image.fromarray((np.clip(g, 0, 1) * 255).astype(np.uint8)); dr = ImageDraw.Draw(im)
for k in range(400):
    x, y = rng.integers(0, W), rng.integers(0, H); a = rng.uniform(0, np.pi); L = rng.integers(4, 18)
    dr.line([x, y, x + L * np.cos(a), y + L * np.sin(a)], fill=(160, 122, 75), width=1)
for k in range(14):  # nếp nhăn sáng/tối
    x, y = rng.integers(0, W), rng.integers(0, H); a = rng.uniform(0, np.pi); L = rng.integers(60, 200)
    dr.line([x, y, x + L * np.cos(a), y + L * np.sin(a)], fill=(92, 64, 34), width=2)
    dr.line([x + 2, y + 1, x + 2 + L * np.cos(a), y + 1 + L * np.sin(a)], fill=(165, 128, 80), width=1)
im.filter(ImageFilter.GaussianBlur(0.5)).save(O + "Hu_GiayDau.png")

# 4. tờ niêm: giấy bản vàng cũ, chữ son viết dọc (sắc lệnh trấn yểm), một ấn vuông. Dải 256 × 1536, u = bề ngang, v = dọc dải
W, H = 256, 1536
p = np.array([226, 206, 152]) / 255 * (0.82 + 0.3 * noise(W, H, 70)[..., None])
yy = np.linspace(0, 1, H)[:, None]
p = p * (1 - 0.18 * np.clip(np.abs(yy - 0.5) * 2 - 0.6, 0, 1)[..., None] / 0.4)   # ố ở hai đầu
im = Image.fromarray((np.clip(p, 0, 1) * 255).astype(np.uint8)); dr = ImageDraw.Draw(im)
f = ImageFont.truetype("/usr/share/fonts/opentype/noto/NotoSerifCJK-Bold.ttc", 150, index=0)
fs = ImageFont.truetype("/usr/share/fonts/opentype/noto/NotoSerifCJK-Bold.ttc", 66, index=0)
SON = (176, 48, 42)
# đầu sau (v 0–0.3) chữ nhỏ; giữa (đỉnh miệng hũ) chữ 封 lớn; đầu trước (0.62–1) 敕令 鎮 + ấn
def chu(txt, font, x, y):
    tmp = Image.new("L", (W, 400), 0); ImageDraw.Draw(tmp).text((x, 0), txt, font=font, fill=255)
    tmp = tmp.filter(ImageFilter.GaussianBlur(1.2)).point(lambda a: 255 if a > 110 else int(a * 1.6))
    im.paste(Image.new("RGB", (W, 400), SON), (0, y), tmp)
for i, c in enumerate("急急如律令"): chu(c, fs, 95, 30 + i * 74)
chu("封", f, 53, 600)
for i, c in enumerate("敕令鎮"): chu(c, fs, 95, 1150 + i * 78)
# ấn vuông chữ trắng
im2 = Image.new("RGB", (96, 96), SON); d2 = ImageDraw.Draw(im2)
d2.rectangle([5, 5, 90, 90], outline=(226, 206, 152), width=3)
d2.text((14, 4), "印", font=ImageFont.truetype("/usr/share/fonts/opentype/noto/NotoSerifCJK-Bold.ttc", 66, index=0), fill=(226, 206, 152))
m = Image.fromarray((np.clip(noise(96, 96, 10) * 1.8 - 0.25, 0, 1) * 230).astype(np.uint8))
im.paste(im2, (80, 1400), m)
# xơ + đốm
dr = ImageDraw.Draw(im)
for k in range(300):
    x, y = rng.integers(0, W), rng.integers(0, H); a = rng.uniform(0, np.pi); L = rng.integers(3, 12)
    dr.line([x, y, x + L * np.cos(a), y + L * np.sin(a)], fill=(205, 182, 128), width=1)
for k in range(60):
    x, y, r = rng.integers(0, W), rng.integers(0, H), rng.integers(2, 9)
    dr.ellipse([x - r, y - r, x + r, y + r], fill=(196, 168, 110))
im.filter(ImageFilter.GaussianBlur(0.4)).save(O + "Hu_Niem.png")

# 5. lạt tre
L = np.array([196, 170, 112]) / 255 * (0.8 + 0.3 * noise(256, 32, 20)[..., None])
L[::4] *= 0.85
Image.fromarray((np.clip(L, 0, 1) * 255).astype(np.uint8)).save(O + "Hu_Lat.png")
print("ok")
