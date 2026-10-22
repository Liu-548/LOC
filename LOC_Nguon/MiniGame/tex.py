# [22/10] Texture cho 3 mini game sắp xếp (LocMiniGameBuilder): bìa hồ sơ "KHÔI", cuống phiếu chuyển tiền, áo hoa của Nhím, giấy hoá đơn, bìa sổ.
# Chạy: python3 tex.py  → ghi vào Assets/LOC_House/Textures/MiniGame/. Ảnh nhỏ, lọc Point trong Unity (kiểu PSX).
import os, random
from PIL import Image, ImageDraw, ImageFont, ImageFilter
OUT = os.path.join(os.path.dirname(__file__), "../../Assets/LOC_House/Textures/MiniGame")
os.makedirs(OUT, exist_ok=True)
G = "/usr/share/fonts/truetype"
IN = ImageFont.truetype(G + "/dejavu/DejaVuSans.ttf", 15)
INB = ImageFont.truetype(G + "/dejavu/DejaVuSans-Bold.ttf", 17)
def tay(co): return ImageFont.truetype(G + "/google-fonts/Lora-Italic-Variable.ttf", co)

def chu_tay(im, xy, s, co, mau, nghieng=0.32, xoay=-3, rd=None):
    """chữ viết tay: font nghiêng + xô thêm + xoay nhẹ + run nét"""
    f = tay(co)
    w, h = f.getbbox(s)[2] + 40, int(co * 1.6)
    l = Image.new("RGBA", (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(l)
    x = 10
    for ch in s:   # mỗi chữ lệch lên xuống một chút
        d.text((x, 4 + (rd.randint(-2, 2) if rd else 0)), ch, font=f, fill=mau)
        x += f.getlength(ch) * (1.02 if rd else 1)
    l = l.transform(l.size, Image.AFFINE, (1, nghieng, -nghieng * h / 2, 0, 1, 0), Image.BICUBIC)
    l = l.rotate(xoay, resample=Image.BICUBIC, expand=True)
    im.alpha_composite(l, (int(xy[0]), int(xy[1])))

def giay(w, h, nen, rd, bui=10):
    im = Image.new("RGBA", (w, h), nen)
    px = im.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]; n = rd.randint(-bui, bui)
            px[x, y] = (max(0, min(255, r + n)), max(0, min(255, g + n)), max(0, min(255, b + n - 2)), a)
    return im

BUT_BI = (32, 52, 128, 255)       # mực bút bi xanh
rd = random.Random(22)

# ── bìa hồ sơ: giấy bìa cứng vàng nâu, chữ bố bằng bút dạ: KHÔI
im = giay(512, 360, (196, 160, 102, 255), rd, 8)
d = ImageDraw.Draw(im)
d.rectangle([0, 0, 511, 359], outline=(150, 115, 68, 255), width=6)
d.line([(0, 60), (512, 60)], fill=(170, 134, 84, 255), width=2)        # nếp gấp mép bìa
chu_tay(im, (120, 120), "KHÔI", 120, (28, 34, 70, 255), 0.38, -4, rd)
im.save(os.path.join(OUT, "T_MG_BiaHoSo_Khoi.png"))

# ── cuống phiếu chuyển tiền bưu điện (in sẵn) + chữ bố viết tay (to, nghiêng) — người gửi ghi TÊN MẸ
im = giay(512, 320, (236, 228, 204, 255), rd, 7)
d = ImageDraw.Draw(im)
d.rectangle([0, 0, 511, 319], outline=(170, 150, 120, 255), width=3)
d.rectangle([0, 0, 511, 38], fill=(206, 196, 168, 255))
d.text((14, 9), "BƯU ĐIỆN  ·  PHIẾU CHUYỂN TIỀN  —  CUỐNG", font=INB, fill=(90, 70, 52, 255))
nhan = ["Số tiền:", "Người nhận:", "Người gửi:", "Ngày gửi:"]
for i, t in enumerate(nhan):
    y = 66 + i * 62
    d.text((16, y), t, font=IN, fill=(96, 82, 66, 255))
    d.line([(130, y + 30), (496, y + 30)], fill=(150, 136, 112, 255), width=1)
chu_tay(im, (140, 42), "600.000 đ", 40, BUT_BI, 0.34, -2, rd)
chu_tay(im, (140, 104), "Trần Hoài Khôi", 40, BUT_BI, 0.34, -2, rd)
chu_tay(im, (140, 166), "Nguyễn Thị Liên", 40, BUT_BI, 0.34, -3, rd)
chu_tay(im, (140, 228), "5 / 9 / 2000", 40, BUT_BI, 0.34, -2, rd)
d.ellipse([400, 250, 490, 310], outline=(150, 60, 60, 160), width=3)    # dấu bưu điện mờ (màu dấu, không phải mực son)
d.text((414, 270), "BĐ", font=INB, fill=(150, 60, 60, 160))
im.save(os.path.join(OUT, "T_MG_CuongPhieu.png"))

# ── áo hoa nhỏ của Nhím: nền hồng nhạt, hoa li ti
im = Image.new("RGBA", (128, 128), (232, 170, 182, 255)); d = ImageDraw.Draw(im)
for _ in range(70):
    x, y = rd.randint(0, 127), rd.randint(0, 127)
    d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=(250, 236, 238, 255)); d.point((x, y), fill=(210, 120, 60, 255))
im.save(os.path.join(OUT, "T_MG_AoHoa_Nhim.png"))

# ── hoá đơn / giấy tờ rời: giấy trắng ngà, dòng kẻ, chữ in mờ
im = giay(256, 360, (238, 234, 222, 255), rd, 6); d = ImageDraw.Draw(im)
d.text((14, 12), "HOÁ ĐƠN BÁN HÀNG", font=INB, fill=(80, 80, 96, 255))
for i in range(12):
    y = 52 + i * 24
    d.line([(12, y), (244, y)], fill=(190, 186, 176, 255), width=1)
    d.line([(16, y - 9), (16 + rd.randint(60, 200), y - 9)], fill=(110, 110, 130, 200), width=3)
im.save(os.path.join(OUT, "T_MG_HoaDon.png"))
print("xong")
