# LỘC — bản phác giao diện bắt đầu + các menu (22/10). Mỗi màn 1920×1080.
import numpy as np, os
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageEnhance

D = "/home/claude/menu"; OUT = f"{D}/out"; os.makedirs(OUT, exist_ok=True)
W, H = 1920, 1080
NGA = (232, 226, 210); MO = (140, 133, 122); TOI = (14, 12, 10); SON = (176, 48, 42); GIAY = (216, 204, 176); MUC = (44, 36, 28)

def F(size, w=400, it=False, tay=False):
    if tay: return ImageFont.truetype(f"{D}/fonts/PatrickHand-Regular.ttf", size)
    f = ImageFont.truetype(f"{D}/fonts/NotoSerif{'-Italic' if it else ''}[wdth,wght].ttf", size)
    f.set_variation_by_axes([w, 100]); return f

def nen(ten, toi=0.55, mo=0, xam=0.0, xoaPhuDe=True):
    im = Image.open(f"{D}/bg/{ten}.png").convert("RGB")
    if im.width == 2560:
        if xoaPhuDe:   # xoá dòng phụ đề đáy màn hình của ảnh chụp kiểm tra
            a = im.crop((900, 1180, 1660, 1320)).filter(ImageFilter.GaussianBlur(30)); im.paste(a, (900, 1180))
        im = im.crop((71, 80, 2489, 1440))
    im = im.resize((W, H), Image.LANCZOS)
    if mo: im = im.filter(ImageFilter.GaussianBlur(mo))
    if xam: im = Image.blend(im, im.convert("L").convert("RGB"), xam)
    im = ImageEnhance.Brightness(im).enhance(1 - toi)
    return im

def phim(im, manh=1.0, seed=3):
    """lớp phim cũ: hạt, vạch quét, viền tối, lệch màu nhẹ"""
    a = np.asarray(im, float)
    r = np.random.default_rng(seed)
    a += r.normal(0, 7 * manh, a.shape[:2])[..., None]
    a[::3] *= 1 - 0.06 * manh
    yy, xx = np.mgrid[0:H, 0:W]; v = ((xx - W / 2) / (W / 2)) ** 2 + ((yy - H / 2) / (H / 2)) ** 2
    a *= (1 - 0.35 * manh * np.clip(v - 0.3, 0, 1))[..., None]
    a[..., 0] = np.roll(a[..., 0], 1, 1); a[..., 2] = np.roll(a[..., 2], -1, 1)
    band = np.exp(-((yy - H * 0.62) / 18) ** 2) * 10 * manh                      # dải sáng VHS trôi chậm
    a += band[..., None]
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8))

def chu(d, xy, s, f, col=NGA, anchor="la", bong=True):
    if bong: d.text((xy[0] + 2, xy[1] + 2), s, font=f, fill=(0, 0, 0), anchor=anchor)
    d.text(xy, s, font=f, fill=col, anchor=anchor)

def tieu_de(d, x, y, co=170):
    f = F(co, 600)
    for dx, col in ((-3, (120, 40, 40)), (3, (40, 70, 90))):      # lệch màu như băng cũ
        d.text((x + dx, y), "LỘC", font=f, fill=col)
    d.text((x, y), "LỘC", font=f, fill=NGA)
    bb = d.textbbox((x, y), "LỘC", font=f)

def muc_menu(d, x, y, ds, chon, co=40, gian=64, khoa=()):
    for i, (s, phu) in enumerate(ds):
        on = i == chon; k = s in khoa
        col = NGA if on else ((90, 85, 78) if k else MO)
        if on:
            d.rectangle([x - 34, y + i * gian + co * 0.48, x - 14, y + i * gian + co * 0.48 + 4], fill=SON)   # vệt mực son
        chu(d, (x, y + i * gian), s, F(co, 600 if on else 400), col)
        if phu:
            w = d.textlength(s, font=F(co, 600 if on else 400))
            chu(d, (x + w + 22, y + i * gian + co * 0.32), phu, F(int(co * 0.55), 400, it=True), (150, 142, 128) if on else (100, 95, 88))

def goi_y_phim(d, ds, y=H - 62, x=None):
    s = "     ".join(ds); f = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", 19)
    w = d.textlength(s, font=f); chu(d, ((W - w) / 2 if x is None else x, y), s, f, (120, 114, 104), bong=False)

def nhan(d, so, ten):   # nhãn góc của bản phác (không có trong game)
    d.rectangle([W - 330, 18, W - 18, 58], fill=(255, 255, 255))
    d.text((W - 318, 26), f"{so}  {ten}", font=F(20, 600), fill=(20, 20, 20))

# ───────── 1. Cảnh báo nội dung
def man1():
    im = Image.new("RGB", (W, H), (8, 7, 6)); d = ImageDraw.Draw(im)
    chu(d, (W / 2, 330), "CẢNH BÁO", F(30, 600), SON, "mm", False)
    d.line([W / 2 - 40, 368, W / 2 + 40, 368], fill=SON, width=3)
    chu(d, (W / 2, 440), "Game có chứa các nội dung về mê tín dị đoan, hù dọa, ám chỉ cái chết.", F(32), NGA, "mm", False)
    hu = ["Tác phẩm hư cấu. Mọi nghi lễ, sách vở, thầy cúng, địa danh trong game đều do hư cấu.",
          "Game phê phán kẻ lợi dụng nỗi sợ của người khác, không phê phán tín ngưỡng."]
    for i, s in enumerate(hu): chu(d, (W / 2, 600 + i * 40), s, F(25, it=True), MO, "mm", False)
    chu(d, (W / 2, 930), "Nhấn phím bất kỳ để tiếp tục", F(22), (110, 104, 96), "mm", False)
    d.rectangle([W / 2 - 120, 962, W / 2 + 120, 964], fill=(40, 36, 32)); d.rectangle([W / 2 - 120, 962, W / 2 + 40, 964], fill=(110, 104, 96))
    chu(d, (W / 2, 990), "(dòng này hiện sau 5 giây · thanh mảnh là đồng hồ đếm)", F(16, it=True), (70, 66, 60), "mm", False)
    im = phim(im, 0.6); nhan(ImageDraw.Draw(im), "01", "Cảnh báo nội dung"); return im

# ───────── 2. Menu chính
def man2():
    im = nen("05_dau_dem1_ngoai_cong", 0.45)
    a = np.asarray(im, float); xx = np.linspace(1, 0, W)[None, :, None]
    a *= 1 - 0.75 * np.clip(xx * 1.5 - 0.35, 0, 1)                                  # tối dần sang trái cho chữ
    im = Image.fromarray(a.astype(np.uint8)); d = ImageDraw.Draw(im)
    tieu_de(d, 150, 150)
    muc_menu(d, 160, 560, [("Tiếp tục", "Đêm 2 · 7/8/2002"), ("Bắt đầu", ""), ("Chọn đêm", "mở sau khi chơi hết một lần"),
                           ("Ending", "mở sau khi hoàn thành game"), ("Cài đặt", ""), ("Thoát", "")], 0, khoa=("Chọn đêm", "Ending"))
    chu(d, (W - 110, H - 140), "Nhà tôi nuôi nó mười ba năm.", F(26, it=True), (190, 182, 166), "ra")
    chu(d, (W - 110, H - 100), "Người mang nó tới thì chưa bao giờ đi.", F(26, it=True), (190, 182, 166), "ra")
    goi_y_phim(d, ["↑↓ / chuột  chọn", "Enter / nhấp  vào"], x=160)
    im = phim(im, 1.0); nhan(ImageDraw.Draw(im), "02", "Menu chính"); return im

# ───────── 3. Xác nhận bắt đầu lại
def hop_xac_nhan(im, tieu, dong, nut, chon=1):
    im = ImageEnhance.Brightness(im).enhance(0.45); d = ImageDraw.Draw(im)
    x0, y0, x1, y1 = W / 2 - 400, H / 2 - 150, W / 2 + 400, H / 2 + 150
    d.rectangle([x0, y0, x1, y1], fill=(18, 16, 14), outline=(70, 64, 56), width=2)
    d.rectangle([x0, y0, x0 + 6, y1], fill=SON)
    chu(d, (x0 + 50, y0 + 45), tieu, F(34, 600), NGA, bong=False)
    for i, s in enumerate(dong): chu(d, (x0 + 50, y0 + 110 + i * 36), s, F(24), MO, bong=False)
    for i, s in enumerate(nut):
        xx = x1 - 50 - (len(nut) - i) * 220 + 20; on = i == chon
        if on: d.rectangle([xx - 16, y1 - 70 + 18, xx - 4, y1 - 70 + 22], fill=SON)
        chu(d, (xx, y1 - 70), s, F(28, 600 if on else 400), NGA if on else MO, bong=False)
    return im

def man3():
    im = man2_tho()
    im = hop_xac_nhan(im, "Bắt đầu lại từ Đêm 1?", ["Tiến độ đã lưu (Đêm 2 · 7/8/2002) sẽ bị xoá.", "Chỉ có một chỗ lưu."], ["Bắt đầu lại", "Thôi"], 1)
    im = phim(im, 0.8); nhan(ImageDraw.Draw(im), "03", "Xác nhận bắt đầu lại"); return im

def man2_tho():
    im = nen("05_dau_dem1_ngoai_cong", 0.45); a = np.asarray(im, float); xx = np.linspace(1, 0, W)[None, :, None]
    a *= 1 - 0.75 * np.clip(xx * 1.5 - 0.35, 0, 1); im = Image.fromarray(a.astype(np.uint8)); d = ImageDraw.Draw(im)
    tieu_de(d, 150, 150)
    muc_menu(d, 160, 560, [("Tiếp tục", "Đêm 2 · 7/8/2002"), ("Bắt đầu", ""), ("Chọn đêm", ""), ("Ending", ""), ("Cài đặt", ""), ("Thoát", "")], 1)
    return im

# ───────── 4. Cài đặt — trang sổ, kẻ chấm như sổ thu chi
def trang_so(im, tab, hang, chon, ghichu=None):
    d = ImageDraw.Draw(im)
    x0, y0, x1, y1 = 360, 120, 1560, 960
    sh = Image.new("L", (W, H), 0); ImageDraw.Draw(sh).rectangle([x0 + 14, y0 + 18, x1 + 14, y1 + 18], fill=170)
    im.paste((0, 0, 0), (0, 0), sh.filter(ImageFilter.GaussianBlur(14)))
    giay = np.ones((y1 - y0, x1 - x0, 3)) * np.array(GIAY, float)
    r = np.random.default_rng(5); giay += r.normal(0, 4, giay.shape[:2])[..., None]
    gy = np.arange(y1 - y0)[:, None]; giay -= ((gy % 62) == 10)[..., None] * np.array([30, 22, 10])          # dòng kẻ ngang
    giay[:, 92:94] -= np.array([0, 50, 50])                                                        # lề đỏ
    im.paste(Image.fromarray(np.clip(giay, 0, 255).astype(np.uint8)), (x0, y0)); d = ImageDraw.Draw(im)
    chu(d, (x0 + 130, y0 + 40), "Cài đặt", F(44, 600), MUC, bong=False)
    tabs = ["Hình ảnh", "Âm thanh", "Điều khiển", "Phụ đề"]; tx = x0 + 130
    for t in tabs:
        on = t == tab; f = F(26, 600 if on else 400)
        d.text((tx, y0 + 122), t, font=f, fill=MUC if on else (130, 118, 100))
        w = d.textlength(t, font=f)
        if on: d.line([tx, y0 + 160, tx + w, y0 + 160], fill=SON, width=3)
        tx += w + 50
    for i, (ten, gt) in enumerate(hang):
        y = y0 + 210 + i * 62; on = i == chon
        f = F(28, 600 if on else 400); col = MUC if on else (70, 60, 48)
        d.text((x0 + 130, y), ten, font=f, fill=col)
        w = d.textlength(ten, font=f); gw = d.textlength(gt, font=F(28, 600))
        xr = x1 - 110
        dots = x0 + 130 + w + 16
        while dots < xr - gw - 70: d.text((dots, y), ".", font=F(28), fill=(150, 138, 118)); dots += 12   # chấm như sổ
        if on and not gt.endswith("›"):
            d.text((xr - gw - 46, y), "‹", font=F(30, 600), fill=SON); d.text((xr + 12, y), "›", font=F(30, 600), fill=SON)
        if on:
            d.rectangle([x0 + 100, y + 16, x0 + 114, y + 20], fill=SON)
        d.text((xr - gw, y), gt, font=F(28, 600), fill=col)
    if ghichu: d.text((x0 + 130, y1 - 75), ghichu, font=F(24, tay=True), fill=(80, 70, 60))
    goi_y_phim(d, ["↑↓ chọn", "←→ đổi", "Q / E  sang mục", "Esc  quay lại"], y=y1 + 40)

def man4():
    im = ImageEnhance.Brightness(nen("05_dau_dem1_ngoai_cong", 0.5, mo=8)).enhance(0.6)
    trang_so(im, "Hình ảnh", [("Chế độ màn hình", "Toàn màn hình"), ("Độ phân giải", "1920 × 1080"), ("Độ sáng", "Chỉnh  ›"),
                              ("Lớp phim cũ", "Đầy đủ"), ("Chất lượng bóng đổ", "Vừa"), ("Giới hạn khung hình", "60"), ("Đồng bộ dọc", "Bật")], 2,
             "Độ sáng: chỉnh tới khi hình bên phải vừa khuất — game cần lúc tối phải thật tối.")
    im = phim(im, 0.7); nhan(ImageDraw.Draw(im), "04", "Cài đặt — Hình ảnh"); return im

def man4b():
    im = ImageEnhance.Brightness(nen("05_dau_dem1_ngoai_cong", 0.5, mo=8)).enhance(0.6)
    trang_so(im, "Âm thanh", [("Âm lượng tổng", "80"), ("Hiệu ứng", "100"), ("Tiếng nền", "100"), ("Nhạc menu", "60"), ("Tiếp tục phát khi chuyển cửa sổ", "Tắt")], 0)
    im = phim(im, 0.7); nhan(ImageDraw.Draw(im), "05", "Cài đặt — Âm thanh"); return im

def man4c():
    im = ImageEnhance.Brightness(nen("05_dau_dem1_ngoai_cong", 0.5, mo=8)).enhance(0.6)
    trang_so(im, "Điều khiển", [("Độ nhạy chuột", "5,0"), ("Đảo trục dọc", "Tắt"), ("Lắc camera khi đi", "Bình thường"), ("Bảng phím", "Xem  ›")], 3,
             "Đi WASD · Nhìn chuột · Tương tác E · Túi đồ Tab · Gợi ý H · Ngồi C · Tạm dừng Esc")
    im = phim(im, 0.7); nhan(ImageDraw.Draw(im), "06", "Cài đặt — Điều khiển"); return im

def man4d():
    im = ImageEnhance.Brightness(nen("05_dau_dem1_ngoai_cong", 0.5, mo=8)).enhance(0.6)
    trang_so(im, "Phụ đề", [("Cỡ chữ", "Vừa"), ("Nền sau chữ", "Mờ"), ("Thời gian hiện chữ", "Bình thường")], 0)
    d = ImageDraw.Draw(im)   # xem trước hai kiểu phụ đề
    d.rectangle([490, 640, 1430, 840], fill=(20, 18, 16))
    d.text((960, 700), "NHÍM: Anh Khôi ơi! Anh về thật à.", font=F(30), fill=(240, 238, 232), anchor="mm")
    d.text((960, 770), "Hai năm rồi mới về.", font=F(30, it=True), fill=(170, 166, 158), anchor="mm")
    d.text((500, 650), "xem trước", font=F(18, it=True), fill=(110, 104, 96))
    im = phim(im, 0.7); nhan(ImageDraw.Draw(im), "07", "Cài đặt — Phụ đề"); return im

# ───────── 5. Chỉnh độ sáng
def man5():
    im = Image.new("RGB", (W, H), (0, 0, 0)); d = ImageDraw.Draw(im)
    chu(d, (W / 2, 200), "Độ sáng", F(40, 600), NGA, "mm", False)
    chu(d, (W / 2, 260), "Kéo tới khi hình bên trái vừa đủ thấy, hình bên phải vừa khuất.", F(26), MO, "mm", False)
    # hai đôi mắt như con vong — trái thấy được, phải gần như không
    for cx, k in ((700, 22), (1220, 7)):
        d.ellipse([cx - 140, 380, cx + 140, 700], fill=(k, k, k))
        for ex in (-45, 45): d.ellipse([cx + ex - 18, 500, cx + ex + 18, 520], fill=(k + 12, k + 12, k + 10))
    chu(d, (700, 740), "vừa đủ thấy", F(22, it=True), MO, "mm", False); chu(d, (1220, 740), "vừa khuất", F(22, it=True), MO, "mm", False)
    d.rectangle([660, 840, 1260, 846], fill=(60, 56, 50)); d.rectangle([660, 840, 930, 846], fill=NGA); d.ellipse([920, 832, 942, 854], fill=SON)
    goi_y_phim(d, ["←→ chỉnh", "Enter  xong", "Esc  huỷ"], y=930)
    im = phim(im, 0.5); nhan(ImageDraw.Draw(im), "08", "Chỉnh độ sáng"); return im

# ───────── 6. Tạm dừng
def man6():
    im = nen("08_dau_dem2", 0.55, mo=6, xam=0.6); d = ImageDraw.Draw(im)
    a = np.asarray(im, float); xx = np.linspace(1, 0, W)[None, :, None]; a *= 1 - 0.7 * np.clip(xx * 1.6 - 0.5, 0, 1)
    im = Image.fromarray(a.astype(np.uint8)); d = ImageDraw.Draw(im)
    chu(d, (160, 300), "ĐÊM 2", F(30, 600), MO); chu(d, (160, 345), "7 tháng 8 năm 2002", F(24, it=True), (110, 104, 96))
    muc_menu(d, 160, 450, [("Tiếp tục", ""), ("Cài đặt", ""), ("Chơi lại từ đầu đêm", ""), ("Về menu chính", ""), ("Thoát game", "")], 0)
    goi_y_phim(d, ["E  tương tác", "Tab  túi đồ", "H  gợi ý", "C  ngồi", "Esc  tiếp tục"], x=160)
    im = phim(im, 0.9); nhan(ImageDraw.Draw(im), "09", "Tạm dừng (Esc)"); return im

def man7():
    im = nen("08_dau_dem2", 0.55, mo=6, xam=0.6)
    im = hop_xac_nhan(im, "Về menu chính?", ["Game tự lưu ở đầu mỗi đêm.", "Những gì làm từ đầu Đêm 2 tới giờ sẽ mất."], ["Về menu", "Ở lại"], 1)
    im = phim(im, 0.8); nhan(ImageDraw.Draw(im), "10", "Xác nhận rời game"); return im

# ───────── 7. Thẻ đêm (giữa hai đêm, kèm tự lưu)
def man8():
    im = Image.new("RGB", (W, H), (6, 5, 4)); d = ImageDraw.Draw(im)
    chu(d, (W / 2, 470), "ĐÊM 2", F(72, 600), NGA, "mm", False)
    chu(d, (W / 2, 560), "Bố viết hết vào đây", F(30, it=True), MO, "mm", False)
    chu(d, (W / 2, 620), "7 tháng 8 năm 2002", F(22), (100, 95, 88), "mm", False)
    d.ellipse([W - 150, H - 96, W - 138, H - 84], fill=SON); chu(d, (W - 125, H - 100), "đã lưu", F(20, it=True), (110, 104, 96), bong=False)
    im = phim(im, 0.6); nhan(ImageDraw.Draw(im), "11", "Thẻ đêm + tự lưu"); return im

# ───────── 8. Death Ending
def man9():
    im = nen("07_ham_dem1", 0.25, xoaPhuDe=True); d = ImageDraw.Draw(im)
    im = Image.blend(im, Image.new("RGB", (W, H), (0, 0, 0)), 0.35); d = ImageDraw.Draw(im)
    chu(d, (W / 2, 820), "NHÀ NÀY CÒN MỘT CÁI CHÉN.", F(46, 500), NGA, "mm")
    for i, (s, on) in enumerate((("Chơi lại từ đầu đêm", True), ("Về menu chính", False))):
        x = W / 2 - 260 + i * 400
        if on: d.rectangle([x - 34, 935, x - 14, 939], fill=SON)
        chu(d, (x, 915), s, F(30, 600 if on else 400), NGA if on else MO)
    chu(d, (W / 2, 1000), "(hai lựa chọn hiện sau khi dòng chữ đứng yên ~3 giây)", F(16, it=True), (90, 85, 78), "mm", False)
    im = phim(im, 1.0); nhan(ImageDraw.Draw(im), "12", "Death Ending"); return im

def man10():
    im = ImageEnhance.Brightness(nen("05_dau_dem1_ngoai_cong", 0.5, mo=10)).enhance(0.45); d = ImageDraw.Draw(im)
    chu(d, (160, 110), "Ending", F(52, 600), NGA); chu(d, (162, 185), "Đã đạt 2 / 4", F(24, it=True), MO)
    the = [("GOOD", "Tiễn", "85_good", "Đã đạt · 1 lần", None),
           ("DEATH", "Nhà này còn một cái chén", "07_ham_dem1", "Đã đạt · 3 lần", None),
           ("BAD", None, None, None, "Làm theo lời thầy đến bước cuối, khi chưa biết hết mọi chuyện. Hoặc để lỡ buổi lễ."),
           ("EVIL", None, None, None, "Biết hết sự thật — tóc trong hũ, lời mẹ, quyển sách — mà vẫn chọn lộc.")]
    cw, chh, gap, x0, y0 = 370, 640, 36, 160, 260
    for k, (nhom, ten, anh, dat, goi) in enumerate(the):
        x = x0 + k * (cw + gap); on = k == 2
        d.rectangle([x, y0, x + cw, y0 + chh], fill=(20, 18, 16), outline=(160, 150, 134) if on else (60, 56, 50), width=2)
        if anh:
            p = Image.open(f"{D}/bg/{anh}.png").convert("RGB"); p = p.crop((p.width * 0.2, p.height * 0.05, p.width * 0.8, p.height * 0.82))
            p = ImageEnhance.Color(p.resize((cw - 24, 300), Image.LANCZOS), ).enhance(0.6); im.paste(p, (x + 12, y0 + 12)); d = ImageDraw.Draw(im)
        else:
            d.rectangle([x + 12, y0 + 12, x + cw - 12, y0 + 312], fill=(10, 9, 8))
            chu(d, (x + cw / 2, y0 + 162), "?", F(90, 300), (60, 56, 50), "mm", False)
        chu(d, (x + 24, y0 + 340), nhom, F(20, 600), SON if ten else (110, 104, 96), bong=False)
        if ten:
            chu(d, (x + 24, y0 + 375), ten, F(30 if len(ten) < 14 else 25, 600), NGA, bong=False)
            chu(d, (x + 24, y0 + chh - 50), dat, F(20, it=True), MO, bong=False)
        else:
            chu(d, (x + 24, y0 + 375), "Chưa mở", F(28, 400), (120, 114, 104), bong=False)
            chu(d, (x + 24, y0 + 440), "Gợi ý", F(18, 600), (140, 133, 122), bong=False)
            wds = goi.split(); dong = ""; yy = y0 + 470
            for w_ in wds:
                if d.textlength(dong + " " + w_, font=F(22, it=True)) > cw - 48: chu(d, (x + 24, yy), dong.strip(), F(22, it=True), (190, 182, 166), bong=False); yy += 32; dong = ""
                dong += " " + w_
            chu(d, (x + 24, yy), dong.strip(), F(22, it=True), (190, 182, 166), bong=False)
    goi_y_phim(d, ["←→ chọn", "Enter  xem lại đoạn kết", "Esc  quay lại"])
    im = phim(im, 0.8); nhan(ImageDraw.Draw(im), "13", "Ending"); return im

if __name__ == "__main__":
    mans = [man1, man2, man3, man4, man4b, man4c, man4d, man5, man6, man7, man8, man9, man10]
    for i, m in enumerate(mans, 1):
        m().save(f"{OUT}/{i:02d}.png"); print(i)
    # tờ tổng: 4 × 3
    th = [Image.open(f"{OUT}/{i:02d}.png").resize((600, 338), Image.LANCZOS) for i in range(1, len(mans) + 1)]
    sheet = Image.new("RGB", (4 * 600 + 5 * 30, 4 * 338 + 5 * 30 + 120), (246, 243, 238)); ds = ImageDraw.Draw(sheet)
    ds.text((30, 30), "LỘC — giao diện bắt đầu và các menu (bản phác để duyệt, 22/10)", font=F(40, 600), fill=(30, 26, 24))
    for k, t in enumerate(th): sheet.paste(t, (30 + (k % 4) * 630, 120 + (k // 4) * 368))
    sheet.save(f"{OUT}/00_TongQuan.png")
