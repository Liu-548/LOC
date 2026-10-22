# Ghép bản vẽ duyệt (PNG 2700×1700) từ các ảnh render.py sheet
from PIL import Image, ImageDraw, ImageFont
O = "/home/claude/tay/out/"
F = "/usr/share/fonts/truetype/dejavu/DejaVuSans"
f = lambda s, b=False: ImageFont.truetype(F + ("-Bold" if b else "") + ".ttf", s)
BG, INK, MUTE, RED = (232, 226, 218), (30, 26, 22), (95, 88, 80), (150, 40, 32)
W, H = 2700, 1510
im = Image.new("RGB", (W, H), BG); d = ImageDraw.Draw(im)
d.text((60, 40), "LỘC — HAI BÀN TAY KHÔI · BÀN TAY THẦY ĐỒNG · CẢNH BAN NGÀY · bản phác low-poly", font=f(46, True), fill=INK)
d.text((60, 108), "Màu phẳng, chưa texture (PS1: sẽ thêm 128 px vẽ nếp khớp, gân, móng). Ảnh dựng thẳng từ model thật, có xương — duyệt dáng, tỉ lệ, màu, chi tiết.", font=f(24), fill=MUTE)


def put(name, x, y, w, h, cap=None, crop=None):
    a = Image.open(O + name + ".png").convert("RGB")
    if crop: a = a.crop(crop)
    a = a.resize((w, h)); im.paste(a, (x, y))
    if cap: d.text((x + w // 2, y + h + 10), cap, font=f(24, True), fill=INK, anchor="ma")


# ── hàng 1: Khôi
d.text((60, 160), "KHÔI — hai tay (người chơi chỉ thấy hai tay)", font=f(30, True), fill=RED)
put("K_tren", 60, 205, 380, 380, "Mu bàn tay")
put("K_long", 450, 205, 380, 380, "Lòng bàn tay")
put("K_ben", 840, 205, 380, 380, "Bên")
put("K_fp_huong", 1230, 205, 380, 380, "Góc người chơi: thắp hương")
x = 60
for k, t in (("Tha", "Thả"), ("Cam", "Cầm"), ("Nhon", "Nhón"), ("Nam", "Nắm"), ("Xin", "Ngửa")):
    put("K_p_" + k, x, 640, 296, 296, t); x += 310
# ── hàng 2: thầy
d.text((60, 990), "THẦY ĐỒNG — một bàn tay phải (chỉ ở Bad đường c, qua song sắt cổng)", font=f(30, True), fill=RED)
put("T_tren", 60, 1035, 400, 400, "Mu bàn tay")
put("T_34", 470, 1035, 400, 400, "3/4: nhẫn, móng út")
put("T_cong", 880, 1035, 400, 400, "Ngửa tay qua song cổng")

# ── cột phải
X = 1680; y = 160


def head(t):
    global y; d.text((X, y), t, font=f(30, True), fill=INK); y += 46


def line(t, s=22, c=INK, b=False):
    global y; d.text((X, y), t, font=f(s, b), fill=c); y += s + 12


head("THÔNG SỐ — KHÔI")
for t in ("• Tay nam 20 tuổi, dài bàn tay ~18 cm, khớp nhỏ, móng cắt ngắn",
          "• ~800 tam giác / tay (cả ống tay áo) · shade phẳng",
          "• Xương: Arm · Hand · 5 ngón × 3 đốt (16 xương) — co ngón bằng code",
          "• Áo dài tay xám chì, xắn tới giữa cẳng tay",
          "• Chỉ hiện khi có việc tay: thắp hương, thay khăn, hé hũ,",
          "   mở bọc vải, dán chén, xé niêm, rót trà (cảnh ban ngày)"):
    line(t)
y += 10; head("THÔNG SỐ — THẦY ĐỒNG")
for t in ("• Tay đàn ông ~60 tuổi: da sạm hơn, ngón gầy, đốt khớp to",
          "• Móng út để dài (~1,5 cm) · nhẫn vàng mặt đá xanh ngón áp út",
          "• Tay áo khoác ka-ki màu cứt ngựa, dài tới cổ tay",
          "• Không mặt, không người: chỉ bàn tay + cổ tay áo (kịch bản B4)"):
    line(t)
y += 10; head("BẢNG MÀU")
sw = [("Da Khôi", "#C49274"), ("Áo Khôi", "#4C5560"), ("Móng", "#D9AE9A"),
      ("Da thầy", "#9A6A4E"), ("Áo thầy", "#7A6A4A"), ("Đá nhẫn", "#2E7A55")]
for i, (t, h) in enumerate(sw):
    cx, cy = X + (i % 3) * 330, y + (i // 3) * 70
    d.rectangle((cx, cy, cx + 50, cy + 50), fill=tuple(int(h[k:k + 2], 16) for k in (1, 3, 5)), outline=(120, 110, 100))
    d.text((cx + 64, cy + 2), t, font=f(22, True), fill=INK); d.text((cx + 64, cy + 28), h, font=f(18), fill=MUTE)
y += 150
head("CẢNH BAN NGÀY GIỮA HAI ĐÊM (thay màn đen 2,5 s)")
for t in ("Sau Đêm 1 (~7 s): đen → nghe tiếng loa tụng kinh, tiếng người viếng",
          "  → sáng lên: phòng khách ban ngày, nắng trắng lọt cửa sắt,",
          "  góc người chơi cúi nhìn bàn nước: tay Khôi rót trà vào chén khách",
          "  (ấm tích + chén men lam của mini game) · tiếng kèn đám xa",
          "  → mờ dần về đen → Đêm 2.",
          "Sau Đêm 2 (~4 s, ngắn hơn): phòng khách vắng, ghế nhựa đã xếp",
          "  chồng, chỉ một tiếng xe máy ngoài đường → đen → Đêm 3.",
          "Không thoại, không chữ. Không có ma, không lấp ló.",
          "Âm thanh: âm tổng hợp tạm như các tiếng khác, chờ thu thật."):
    line(t, 21)
y += 10; head("LẮC CAMERA KHI ĐI — đã làm")
line("• Nhún ~1,4 cm, lắc ngang ~0,6 cm theo bước; ngồi lắc một nửa", 21)
line("• Cài đặt › Điều khiển: Tắt / Nhẹ (mặc định) / Vừa", 21)
y += 16
d.text((X, y), "Cần duyệt: dáng/tỉ lệ tay · màu da, màu áo · áo xắn tay hay buông", font=f(23, True), fill=RED); y += 34
d.text((X, y), "· nhẫn + móng út của thầy (giữ / bỏ) · nội dung cảnh ban ngày", font=f(23, True), fill=RED)
im.save(O + "ThietKe_BanTay_v1.png")
print(im.size)
