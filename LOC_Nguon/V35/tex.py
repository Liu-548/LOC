# Texture cho đồ vật v3.5 (29/10): sổ đỏ, giấy báo nợ, vỉ thuốc, lời nhắn của mẹ. Chạy: python3 tex.py OUT
import sys, os, random, math
from PIL import Image, ImageDraw, ImageFont, ImageFilter
OUT = sys.argv[1]; os.makedirs(OUT, exist_ok=True)
DJ = "/usr/share/fonts/truetype/dejavu/"
F = lambda n, s: ImageFont.truetype(DJ + n, s)
HAND = "/usr/share/fonts/truetype/google-fonts/Lora-Italic-Variable.ttf"
random.seed(29)

def bui(im, n=900, a=26):   # bụi, xước nhẹ
    d = ImageDraw.Draw(im, "RGBA"); W, H = im.size
    for _ in range(n):
        x, y = random.randrange(W), random.randrange(H); r = random.choice((1, 1, 2))
        d.ellipse((x, y, x + r, y + r), fill=(0, 0, 0, random.randrange(a // 2, a)))
    return im

def ctext(d, y, s, f, fill, W):
    w = d.textlength(s, font=f); d.text(((W - w) / 2, y), s, font=f, fill=fill)

# ── 1. bìa sổ đỏ (ảnh 512×728 ~ tỉ lệ 19×27 cm): đỏ son bạc màu, chữ nhũ vàng, quốc hiệu, vòng hoa văn chung chung
W, H = 512, 728
im = Image.new("RGB", (W, H), (168, 52, 44)); d = ImageDraw.Draw(im)
for y in range(H):   # vân giả da
    for x in range(0, W, 2):
        if random.random() < 0.18: d.point((x, y), fill=(150 + random.randrange(20), 44, 38))
d.rectangle((18, 18, W - 19, H - 19), outline=(206, 168, 92), width=3)
d.rectangle((28, 28, W - 29, H - 29), outline=(206, 168, 92), width=1)
G = (214, 178, 98)
ctext(d, 64, "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM", F("DejaVuSans-Bold.ttf", 19), G, W)
ctext(d, 92, "Độc lập - Tự do - Hạnh phúc", F("DejaVuSans.ttf", 17), G, W)
cx, cy, R = W // 2, 262, 84   # vòng hoa văn tròn tự bịa (không dùng quốc huy thật)
d.ellipse((cx - R, cy - R, cx + R, cy + R), outline=G, width=4)
d.ellipse((cx - R + 12, cy - R + 12, cx + R - 12, cy + R - 12), outline=G, width=2)
pts = [(cx + (R - 30 if i % 2 == 0 else R - 56) * math.sin(i * math.pi / 5), cy - (R - 30 if i % 2 == 0 else R - 56) * math.cos(i * math.pi / 5)) for i in range(10)]
d.polygon(pts, outline=G, fill=(190, 70, 50))
for i in range(16):
    a = i * math.pi / 8; d.line((cx + (R - 12) * math.cos(a), cy + (R - 12) * math.sin(a), cx + R * math.cos(a), cy + R * math.sin(a)), fill=G, width=2)
ctext(d, 400, "GIẤY CHỨNG NHẬN", F("DejaVuSerif-Bold.ttf", 33), G, W)
ctext(d, 454, "QUYỀN SỬ DỤNG ĐẤT", F("DejaVuSerif-Bold.ttf", 30), G, W)
ctext(d, 640, "Số: ......  /QSDĐ", F("DejaVuSans.ttf", 15), G, W)
im = bui(im, 1600, 30)
mask = Image.new("L", (W, H), 0); md = ImageDraw.Draw(mask)   # mép mòn sáng
for i in range(10): md.rectangle((i, i, W - 1 - i, H - 1 - i), outline=110 - i * 11)
im = Image.composite(Image.new("RGB", (W, H), (196, 120, 104)), im, mask.filter(ImageFilter.GaussianBlur(3)))
im.save(f"{OUT}/T_SoDo_Bia.png")

# ── 2. giấy báo nợ ngân hàng (ảnh 512×724, A5 dọc; phần ló khỏi sổ là mép phải trên)
W, H = 512, 724
im = Image.new("RGB", (W, H), (236, 232, 218)); d = ImageDraw.Draw(im)
k = (40, 44, 60); b = F("DejaVuSans-Bold.ttf", 18); r = F("DejaVuSans.ttf", 14)
d.text((30, 26), "NGÂN HÀNG", font=F("DejaVuSans-Bold.ttf", 15), fill=(30, 60, 120))
d.text((30, 46), "Chi nhánh thị xã", font=r, fill=(30, 60, 120))
ctext(d, 104, "GIẤY BÁO NỢ ĐẾN HẠN", F("DejaVuSans-Bold.ttf", 26), (150, 28, 28), W)
ctext(d, 140, "(Lần thứ hai)", r, k, W)
y = 190
for s in ["Kính gửi: Ông Trần Quang Vũ", "Địa chỉ: số 9, phố Chợ Cũ", "", "Hợp đồng vay số: 0417/HĐTD", "Tài sản thế chấp: Giấy CNQSDĐ số ........",
          "Số tiền gốc đến hạn: ........................ đ", "Lãi quá hạn: ........................ đ", "", "Hạn chót thanh toán: 30/9/2002",
          "Quá hạn trên, Ngân hàng sẽ tiến hành xử lý", "tài sản thế chấp theo quy định."]:
    d.text((30, y), s, font=r, fill=k); y += 30
d.ellipse((330, 560, 470, 700), outline=(170, 40, 40), width=4); d.ellipse((345, 575, 455, 685), outline=(170, 40, 40), width=2)
ctext(d, 620, " ", r, k, W)
d.text((362, 617), "ĐÃ KÝ", font=F("DejaVuSans-Bold.ttf", 18), fill=(170, 40, 40))
for x in range(0, W, 3):   # nếp gấp ba
    for yy in (241, 482): d.point((x, yy + random.randrange(-1, 2)), fill=(205, 200, 186))
bui(im, 500, 18).save(f"{OUT}/T_BaoNo.png")

# ── 3. vỉ thuốc: mặt nhôm in chữ (512×256)
W, H = 512, 256
im = Image.new("RGB", (W, H), (196, 198, 200)); d = ImageDraw.Draw(im)
for x in range(0, W, 4): d.line((x, 0, x, H), fill=(186 + random.randrange(14),) * 3)
for i in range(5):
    for j in range(2):
        d.text((14 + i * 100, 40 + j * 120), "PARACETAMOL", font=F("DejaVuSans-Bold.ttf", 11), fill=(30, 80, 150))
        d.text((24 + i * 100, 62 + j * 120), "500 mg", font=F("DejaVuSans.ttf", 12), fill=(30, 80, 150))
d.text((10, H - 22), "Lô SX 0102   HD 01/2004", font=F("DejaVuSans.ttf", 12), fill=(60, 60, 60))
im.save(f"{OUT}/T_ViThuoc.png")

# ── 4. lời nhắn của mẹ: mặt sau tờ lịch bóc (ảnh 400×600 ~ 10×15 cm). Mặt trước: chữ bút bi mẹ. Mặt sau: tờ lịch ngày 4.
W, H = 400, 600
im = Image.new("RGB", (W, H), (234, 228, 210)); d = ImageDraw.Draw(im)
pen = (36, 48, 104)
lines = ["Khôi,", "", "Con về thì đừng xuống", "hầm một mình.", "Có gì cứ xem quyển sách", "của mẹ trong tủ thờ,", "mẹ kẹp giấy sẵn rồi.",
         "Đừng nghe ai hết.", "", "            Mẹ thương."]
y = 48
for s in lines:
    f = ImageFont.truetype(HAND, 26 if s != "Khôi," else 30)
    x = 26 + random.randrange(-3, 4)
    for ch in s:   # lệch từng chữ một chút cho ra nét viết vội
        d.text((x, y + random.randrange(-2, 3)), ch, font=f, fill=pen); x += d.textlength(ch, font=f) + random.choice((0, 0, 1))
    y += 46
for _ in range(3):   # vệt mực
    x, yy = random.randrange(40, 360), random.randrange(80, 520); d.ellipse((x, yy, x + 3, yy + 2), fill=pen)
mep = Image.new("L", (W, H), 0); md = ImageDraw.Draw(mep)   # mép trên rách răng cưa (xé khỏi lốc lịch) → alpha
md.rectangle((0, 14, W, H), fill=255)
for x in range(0, W, 6): md.polygon([(x, 14), (x + 3, random.randrange(3, 13)), (x + 6, 14)], fill=255)
im = bui(im, 400, 16); im.putalpha(mep); im.save(f"{OUT}/T_LoiNhanMe.png")
im = Image.new("RGB", (W, H), (238, 234, 222)); d = ImageDraw.Draw(im)   # mặt sau: tờ lịch ngày 4/8/2002
ctext(d, 30, "THÁNG TÁM · 2002", F("DejaVuSans-Bold.ttf", 22), (40, 40, 40), W)
ctext(d, 110, "4", F("DejaVuSerif-Bold.ttf", 240), (40, 40, 40), W)
ctext(d, 400, "CHỦ NHẬT", F("DejaVuSans-Bold.ttf", 30), (40, 40, 40), W)
im = bui(im, 300, 14); im.putalpha(mep); im.save(f"{OUT}/T_LoiNhanMe_Sau.png")
print("tex ok")
