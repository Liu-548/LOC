# Bảng chữ cho xe buýt mở đầu + bảng "CẤM TẮM" ở bến (PIL, font DejaVu Sans — có đủ dấu tiếng Việt). Cũ, bạc, ố.
# python chu.py <thư mục ra>  → T_Chu_*.png (đặt vào Assets/LOC_House/KhuPho/Textures/)
import sys, os, random
from PIL import Image, ImageDraw, ImageFont, ImageFilter
OUT = sys.argv[1] if len(sys.argv) > 1 else "out"; os.makedirs(OUT, exist_ok=True)
F = "/usr/share/fonts/truetype/dejavu/"
rnd = random.Random(2002)

def font(name, size): return ImageFont.truetype(F + name, size)

def cu(img, ban=0.10, o=24):   # bụi, ố, xước
    px = img.load(); w, h = img.size
    for y in range(h):
        for x in range(w):
            r, g, b = px[x, y][:3]
            n = rnd.uniform(-o, o) * ban
            px[x, y] = (max(0, min(255, int(r + n))), max(0, min(255, int(g + n * 0.9))), max(0, min(255, int(b + n * 0.7))))
    lop = Image.new("L", img.size, 0); d = ImageDraw.Draw(lop)
    for _ in range(max(3, int(w * h / 6000))):   # mảng ố vàng nâu, mờ
        x, y = rnd.randrange(w), rnd.randrange(h); s = rnd.randint(max(4, h // 6), max(8, h // 2))
        d.ellipse([x - s * 2, y - s, x + s * 2, y + s], fill=rnd.randint(40, 90))
    lop = lop.filter(ImageFilter.GaussianBlur(max(3, h // 8)))
    img = Image.composite(Image.new("RGB", img.size, (128, 100, 62)), img, lop)
    return img.filter(ImageFilter.GaussianBlur(0.6))

def ghi(img, text, f, mau, cx=None, cy=None, rong=0.88):
    d = ImageDraw.Draw(img); w, h = img.size
    while True:   # co chữ cho vừa bề ngang
        bb = d.textbbox((0, 0), text, font=f); tw, th = bb[2] - bb[0], bb[3] - bb[1]
        if tw <= w * rong or f.size <= 8: break
        f = ImageFont.truetype(f.path, f.size - 2)
    d.text(((w - tw) / 2 - bb[0] if cx is None else cx, (h - th) / 2 - bb[1] if cy is None else cy), text, font=f, fill=mau)

# biển tuyến trước mũi: nền đen, chữ hổ phách (hư cấu: tuyến 09, Bến xe – Cầu Sắt)
im = Image.new("RGB", (512, 64), (14, 12, 10)); ghi(im, "09   BẾN XE – CẦU SẮT", font("DejaVuSansCondensed-Bold.ttf", 38), (236, 150, 46))
im.save(f"{OUT}/T_Chu_BienTuyen.png")

# bảng nhường ghế trên kính lái, phía trong
im = Image.new("RGB", (1024, 72), (226, 218, 196)); ghi(im, "XIN NHƯỜNG GHẾ CHO NGƯỜI GIÀ, TRẺ EM, PHỤ NỮ CÓ THAI", font("DejaVuSansCondensed-Bold.ttf", 40), (110, 34, 26))
cu(im, 0.25).save(f"{OUT}/T_Chu_NhuongGhe.png")

# cấm hút thuốc, cạnh cửa trước
im = Image.new("RGB", (256, 64), (232, 226, 210)); d = ImageDraw.Draw(im); d.rectangle([3, 3, 252, 60], outline=(150, 30, 24), width=4)
ghi(im, "CẤM HÚT THUỐC", font("DejaVuSansCondensed-Bold.ttf", 28), (150, 30, 24)); cu(im, 0.2).save(f"{OUT}/T_Chu_CamHutThuoc.png")

# bảng ở bến: tôn sơn trắng đã ngả, viền đỏ, chữ đỏ, rỉ chảy từ hai lỗ đinh
im = Image.new("RGB", (512, 256), (198, 192, 176)); d = ImageDraw.Draw(im)
d.rectangle([8, 8, 503, 247], outline=(146, 34, 26), width=12)
ghi(im, "CẤM TẮM", font("DejaVuSans-Bold.ttf", 104), (150, 32, 24), rong=0.8)
for x0 in (40, 472):
    d.ellipse([x0 - 7, 26, x0 + 7, 40], fill=(70, 50, 40))
    for k in range(12): d.line([x0 + rnd.randint(-3, 3), 40, x0 + rnd.randint(-5, 5), 40 + rnd.randint(20, 110)], fill=(122 + rnd.randint(-20, 10), 74, 40), width=1)
cu(im, 0.55, 40).save(f"{OUT}/T_Chu_CamTam.png")
print(sorted(os.listdir(OUT)))
