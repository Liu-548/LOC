# [22/10] Atlas 512×512 cho đồ mini game sắp xếp (một vật liệu cho mọi model MG_*). Lọc Point trong Unity.
# Vùng (x0, y0, x1, y1) theo pixel, gốc trên-trái — build_mg.py dùng lại bảng VUNG/PAL này để gán UV.
import os, random, json, sys
from PIL import Image, ImageDraw, ImageFont
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.dirname(os.path.abspath(__file__))
G = "/usr/share/fonts/truetype"
IN = ImageFont.truetype(G + "/dejavu/DejaVuSans.ttf", 15)
INB = ImageFont.truetype(G + "/dejavu/DejaVuSans-Bold.ttf", 17)
def tay(co): return ImageFont.truetype(G + "/google-fonts/Lora-Italic-Variable.ttf", co)
rd = random.Random(22)

VUNG = {
    "bia": (0, 0, 256, 180),        # bìa hồ sơ chữ KHÔI
    "cuong": (256, 0, 512, 160),    # cuống phiếu chuyển tiền
    "hoadon": (0, 192, 128, 372),   # hoá đơn
    "aohoa": (128, 192, 256, 320),  # vải hoa nhỏ của Nhím
    "hatdua": (256, 192, 384, 320), # mặt túi hạt dưa
    "hoalam": (384, 192, 512, 320), # men trắng vẽ lam (chén, ấm)
    "keo": (0, 384 - 12, 64, 384 - 12 + 0),  # (không dùng)
    "keo_o": (128, 320, 192, 384),  # vải kẻ ô xanh (quần đùi)
    "so": (192, 320, 256, 384),     # vải sô trắng thô
    "khanmat": (256, 320, 320, 384),
    "khanlau": (320, 320, 384, 384),
    "biaso": (384, 320, 448, 384),  # bìa sổ giả da + nhãn gáy
    "menau": (448, 320, 512, 384),  # men nâu loang (cốc của bố)
}
PAL = {   # ô màu 16×16 ở dải y 384–512
    "trang": "#EEEAE0", "nuocche": "#4A2E12", "lam": "#2C4A8A", "kem": "#E6DCC2", "do": "#B8302A", "vang": "#D9A53A",
    "den": "#231815", "vohd_do": "#8E2A22", "vohd_den": "#2A1814", "keo_do": "#C9372C", "keo_vang": "#E3B23C", "keo_lam": "#3F7CAC",
    "keo_luc": "#5FA35A", "keo_hong": "#D96FA0", "banhin": "#F1E4DA", "banhin_hong": "#E7B9B9", "keolac": "#B8864A", "giaybong": "#E8E4D6",
    "menau": "#5E3418", "menau_sang": "#8A5530", "datnung": "#B98A5E", "hong": "#E8AAB6", "chitrang": "#FFFFFF", "quanlam": "#5B7FA8",
    "tat": "#F0F0EA", "khanmat": "#7FB6C9", "khanlau": "#E9D7A8", "so": "#F4F1E8", "biacung": "#C4A066", "biacung_trong": "#D8BC86",
    "giay": "#ECE6D2", "giay_canh": "#D9D0B6", "so_nau": "#7A4E2E", "so_vang": "#8C6E3E", "trangsach": "#EFE8D4", "gay": "#3A2A1E",
}
PALXY = {}
for i, k in enumerate(PAL): PALXY[k] = ((i % 32) * 16, 384 + (i // 32) * 16)

def chu_tay(im, xy, s, co, mau, nghieng=0.32, xoay=-3):
    f = tay(co); w, h = f.getbbox(s)[2] + 40, int(co * 1.6)
    l = Image.new("RGBA", (w, h), (0, 0, 0, 0)); d = ImageDraw.Draw(l); x = 10
    for ch in s:
        d.text((x, 4 + rd.randint(-2, 2)), ch, font=f, fill=mau); x += f.getlength(ch) * 1.02
    l = l.transform(l.size, Image.AFFINE, (1, nghieng, -nghieng * h / 2, 0, 1, 0), Image.BICUBIC).rotate(xoay, resample=Image.BICUBIC, expand=True)
    im.alpha_composite(l, (int(xy[0]), int(xy[1])))

def giay(w, h, nen, bui=8):
    im = Image.new("RGBA", (w, h), nen); px = im.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]; n = rd.randint(-bui, bui)
            px[x, y] = (max(0, min(255, r + n)), max(0, min(255, g + n)), max(0, min(255, b + n - 2)), a)
    return im

BUT_BI = (32, 52, 128, 255)
A = Image.new("RGBA", (512, 512), (128, 128, 128, 255))
def dan(ten, im):
    x0, y0, x1, y1 = VUNG[ten]; A.paste(im.resize((x1 - x0, y1 - y0), Image.LANCZOS), (x0, y0))

# bìa hồ sơ: giấy bìa vàng nâu, chữ bố bút dạ
im = giay(512, 360, (196, 160, 102, 255)); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 511, 359], outline=(150, 115, 68, 255), width=8); d.line([(0, 60), (512, 60)], fill=(170, 134, 84, 255), width=3)
chu_tay(im, (110, 110), "KHÔI", 130, (28, 34, 70, 255), 0.38, -4); dan("bia", im)

# cuống phiếu: in sẵn + chữ bố (to, nghiêng) — người gửi ghi TÊN MẸ
im = giay(512, 320, (236, 228, 204, 255), 7); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 511, 319], outline=(170, 150, 120, 255), width=3); d.rectangle([0, 0, 511, 38], fill=(206, 196, 168, 255))
d.text((14, 9), "BƯU ĐIỆN  ·  PHIẾU CHUYỂN TIỀN  —  CUỐNG", font=INB, fill=(90, 70, 52, 255))
for i, t in enumerate(["Số tiền:", "Người nhận:", "Người gửi:", "Ngày gửi:"]):
    y = 66 + i * 62; d.text((16, y), t, font=IN, fill=(96, 82, 66, 255)); d.line([(130, y + 30), (496, y + 30)], fill=(150, 136, 112, 255), width=1)
for i, s in enumerate(["600.000 đ", "Trần Hoài Khôi", "Nguyễn Thị Liên", "5 / 9 / 2000"]):
    chu_tay(im, (140, 42 + 62 * i), s, 42, BUT_BI, 0.34, -2)
d.ellipse([400, 250, 490, 310], outline=(150, 60, 60, 160), width=3); d.text((414, 270), "BĐ", font=INB, fill=(150, 60, 60, 160))
dan("cuong", im)

# hoá đơn
im = giay(256, 360, (238, 234, 222, 255), 6); d = ImageDraw.Draw(im)
d.text((14, 12), "HOÁ ĐƠN BÁN HÀNG", font=INB, fill=(80, 80, 96, 255))
for i in range(12):
    y = 52 + i * 24; d.line([(12, y), (244, y)], fill=(190, 186, 176, 255)); d.line([(16, y - 9), (16 + rd.randint(60, 200), y - 9)], fill=(110, 110, 130, 200), width=3)
dan("hoadon", im)

# vải hoa nhỏ (lặp được)
im = Image.new("RGBA", (128, 128), (232, 170, 182, 255)); d = ImageDraw.Draw(im)
for _ in range(60):
    x, y = rd.randint(4, 123), rd.randint(4, 123); d.ellipse([x - 3, y - 3, x + 3, y + 3], fill=(250, 236, 238, 255)); d.point((x, y), fill=(210, 120, 60, 255))
dan("aohoa", im)

# túi hạt dưa: nylon đỏ, chữ vàng
im = Image.new("RGBA", (128, 128), (184, 48, 42, 255)); d = ImageDraw.Draw(im)
d.rectangle([10, 34, 118, 94], fill=(226, 180, 60, 255)); d.text((20, 50), "HẠT DƯA", font=INB, fill=(150, 30, 26, 255))
d.text((26, 100), "loại 1", font=IN, fill=(240, 220, 160, 255)); dan("hatdua", im)

# men trắng vẽ lam (u quanh thân, v theo chiều cao): viền lam trên/dưới + dây lá
im = Image.new("RGBA", (128, 128), (238, 234, 224, 255)); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 127, 9], fill=(44, 74, 138, 255)); d.rectangle([0, 118, 127, 127], fill=(44, 74, 138, 255))
import math as _m   # dây lá uốn lượn + chùm hoa cúc nhỏ (kiểu men lam Bát Tràng)
pts = [(x, 64 + 14 * _m.sin(x / 128 * 4 * _m.pi)) for x in range(0, 129, 2)]
d.line(pts, fill=(54, 84, 150, 255), width=2)
for k in range(8):
    x = 8 + k * 16; y = 64 + 14 * _m.sin(x / 128 * 4 * _m.pi); s = -1 if k % 2 else 1
    d.ellipse([x - 3, y + s * 6 - 2, x + 3, y + s * 6 + 2], fill=(70, 100, 160, 255))
for k in range(4):
    x = 16 + k * 32; y = 64 + 14 * _m.sin(x / 128 * 4 * _m.pi) - 18 * (1 if k % 2 == 0 else -1)
    for a in range(0, 360, 60): d.ellipse([x + 5 * _m.cos(_m.radians(a)) - 3, y + 5 * _m.sin(_m.radians(a)) - 3, x + 5 * _m.cos(_m.radians(a)) + 3, y + 5 * _m.sin(_m.radians(a)) + 3], fill=(54, 84, 150, 255))
    d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=(238, 234, 224, 255))
dan("hoalam", im)

def ke_o(c1, c2, n=8):
    im = Image.new("RGBA", (64, 64), c1); d = ImageDraw.Draw(im)
    for i in range(0, 64, n): d.line([(i, 0), (i, 63)], fill=c2, width=2); d.line([(0, i), (63, i)], fill=c2, width=2)
    return im
dan("keo_o", ke_o((91, 127, 168, 255), (230, 236, 244, 255)))
im = Image.new("RGBA", (64, 64), (244, 241, 232, 255)); d = ImageDraw.Draw(im)
for i in range(0, 64, 3): d.line([(i, 0), (i, 63)], fill=(226, 222, 210, 255)); d.line([(0, i + 1), (63, i + 1)], fill=(232, 228, 216, 255))
dan("so", im)
im = Image.new("RGBA", (64, 64), (127, 182, 201, 255)); d = ImageDraw.Draw(im); d.rectangle([0, 8, 63, 14], fill=(240, 240, 236, 255)); d.rectangle([0, 50, 63, 56], fill=(240, 240, 236, 255)); dan("khanmat", im)
im = Image.new("RGBA", (64, 64), (233, 215, 168, 255)); d = ImageDraw.Draw(im)
for i in range(4, 64, 12): d.line([(i, 0), (i, 63)], fill=(200, 120, 70, 255), width=2)
dan("khanlau", im)
im = giay(64, 64, (122, 78, 46, 255), 10); d = ImageDraw.Draw(im); d.rectangle([22, 0, 42, 63], fill=(70, 46, 30, 255)); d.rectangle([24, 20, 40, 40], fill=(226, 214, 180, 255)); dan("biaso", im)
im = Image.new("RGBA", (64, 64), (94, 52, 24, 255)); px = im.load()
for y in range(64):
    for x in range(64):
        t = y / 63; r, g, b = (138, 85, 48) if (y < 10 + 4 * ((x // 8) % 2)) else (int(94 - 20 * t), int(52 - 12 * t), int(24 - 6 * t))
        n = rd.randint(-6, 6); px[x, y] = (r + n, g + n, b + n, 255)
dan("menau", im)

d = ImageDraw.Draw(A)
for k, hx in PAL.items():
    x, y = PALXY[k]; d.rectangle([x, y, x + 15, y + 15], fill=hx)
A.save(os.path.join(OUT, "T_MG_Atlas.png"))
json.dump({"VUNG": VUNG, "PALXY": PALXY}, open(os.path.join(OUT, "atlas.json"), "w"))
print("atlas ok", len(PAL), "màu")
