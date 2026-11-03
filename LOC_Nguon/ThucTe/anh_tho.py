# [03/11] Ảnh thờ "ảnh cũ" thay ảnh pixel: chân dung đen trắng ngả nâu, mờ nét, hạt phim, xước, loá kính, băng tang đen góc trên.
#  Người KHÔNG có thật — vẽ thủ tục (hình khối + đổ bóng mềm), cố ý mờ như ảnh thờ phóng từ ảnh thẻ cũ.
#  Chạy: python3 anh_tho.py <ảnh atlas gốc 512 (Image_0 của Anh_Tho_Atlas)> <ra.png>   — giữ bố cục: trái = mẹ, phải = bố
import sys, numpy as np
from PIL import Image, ImageFilter, ImageDraw
rng = np.random.default_rng(1968)

def mo(a, r): return np.asarray(Image.fromarray((np.clip(a, 0, 1) * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(r))) / 255.0

def elip(h, w, cx, cy, rx, ry, mem=1.5):
    y, x = np.mgrid[0:h, 0:w]
    d = ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2
    return np.clip((1 - d) * rx / mem, 0, 1)

def chan_dung(h, w, nu):
    y, x = np.mgrid[0:h, 0:w] / np.array([h, w])[:, None, None]
    X, Y = x * w, y * h
    img = 0.42 + 0.16 * (1 - np.hypot(x - 0.42, y - 0.33) * 1.3)        # phông studio: sáng sau đầu, lệch trái
    cx = w * 0.5; hy = h * 0.37
    rx, ry = w * 0.205, h * 0.175                                          # đầu ~ 40% bề ngang ảnh (ảnh thẻ phóng)
    # vai + áo (vai rộng, bắt đầu ngay dưới cằm)
    vai = elip(h, w, cx, h * 0.93, w * 0.66, h * 0.33, 3)
    img = img * (1 - vai) + (0.20 if nu else 0.14) * vai
    if not nu:   # sơ mi trắng cổ bẻ + áo khoác tối
        co = np.clip(1 - np.abs(X - cx) / (w * 0.10) - (Y - h * 0.62) / (h * 0.16), 0, 1) * (Y > h * 0.60)
        img = img * (1 - co) + 0.80 * co
    else:        # áo bà ba / áo dài cổ tròn sáng hơn
        img += 0.06 * vai * np.clip(1 - np.abs(X - cx) / (w * 0.25), 0, 1)
    # cổ (ngắn)
    cd = np.clip(1 - np.abs(X - cx) / (w * 0.105), 0, 1) ** 0.5 * ((Y > hy + ry * 0.7) & (Y < h * 0.64))
    img = img * (1 - cd) + (0.58 - 0.12 * np.clip((X - cx) / (w * 0.1), 0, 1)) * cd
    # tai
    for s_ in (-1, 1): img = np.where(elip(h, w, cx + s_ * rx * 0.98, hy + ry * 0.05, rx * 0.12, ry * 0.2, 1) > 0.5, 0.55 - 0.1 * (s_ > 0), img)
    # mặt: trái xoan, cằm thon (bán kính ngang giảm dần xuống dưới)
    co_hep = 1 - 0.28 * np.clip((Y - hy) / ry, 0, 1) ** 1.5
    d = ((X - cx) / (rx * co_hep)) ** 2 + ((Y - hy) / ry) ** 2
    mat = np.clip((1 - d) * 6, 0, 1)
    sang = 0.82 - 0.30 * np.clip((X - (cx - rx)) / (2 * rx), 0, 1) ** 1.3 - 0.10 * np.clip((Y - hy) / ry, 0, 1)
    sang += 0.05 * elip(h, w, cx - rx * 0.45, hy + ry * 0.25, rx * 0.25, ry * 0.18, 1)        # gò má sáng
    img = img * (1 - mat) + sang * mat
    # tóc
    if nu:   # rẽ ngôi giữa, ôm sát, vén ra sau, phủ tới ngang tai
        toc = elip(h, w, cx, hy - ry * 0.32, rx * 1.12, ry * 0.85, 2) * (Y < hy + ry * 0.12)
        mai = np.clip(elip(h, w, cx, hy + ry * 0.25, rx * 0.86, ry * 0.95, 2) * 1.6, 0, 1)
        toc = np.clip(toc - mai * (Y > hy - ry * 0.55), 0, 1)
        toc = np.maximum(toc, elip(h, w, cx, hy - ry * 0.9, rx * 0.9, ry * 0.25, 2))
        img -= 0.05 * np.clip(1 - np.abs(X - cx) / 2.5, 0, 1) * (Y < hy - ry * 0.55) * (Y > hy - ry * 1.1)     # đường ngôi
    else:    # tóc ngắn rẽ ngôi lệch, chải ngược
        toc = elip(h, w, cx + rx * 0.05, hy - ry * 0.48, rx * 1.06, ry * 0.62, 2)
        toc = np.clip(toc - np.clip(elip(h, w, cx, hy + ry * 0.12, rx * 0.95, ry * 0.75, 2) * 1.6, 0, 1), 0, 1)
    img = img * (1 - toc) + (0.09 + 0.05 * np.clip((cx - X) / rx, 0, 1)) * toc
    # nét mặt mềm
    for s_ in (-1, 1):
        ex = cx + s_ * rx * 0.40; ey = hy - ry * 0.02
        img -= 0.30 * elip(h, w, ex, ey, rx * 0.16, ry * 0.07, 1)
        img -= 0.22 * elip(h, w, ex, ey - ry * 0.17, rx * 0.21, ry * 0.035, 1)
        img += 0.05 * elip(h, w, ex - rx * 0.03, ey - ry * 0.015, rx * 0.035, ry * 0.02, 1)
    img -= 0.12 * elip(h, w, cx + rx * 0.06, hy + ry * 0.28, rx * 0.06, ry * 0.22, 1)
    img -= 0.16 * elip(h, w, cx, hy + ry * 0.45, rx * 0.15, ry * 0.04, 1)
    img -= 0.20 * elip(h, w, cx, hy + ry * 0.68, rx * 0.24, ry * 0.035, 1)
    img -= 0.06 * elip(h, w, cx, hy + ry * 0.78, rx * 0.2, ry * 0.05, 1)            # bóng dưới môi
    img -= 0.10 * elip(h, w, cx + rx * 0.62, hy + ry * 0.25, rx * 0.25, ry * 0.5, 1) * mat
    img -= 0.12 * elip(h, w, cx, hy + ry * 1.12, rx * 0.6, ry * 0.12, 1)             # bóng cằm xuống cổ
    img = mo(img, max(1.6, w / 75))
    img += rng.normal(0, 0.03, img.shape)
    for _ in range(14):
        x0 = rng.integers(0, w); img[:, max(0, x0):x0 + 1] += rng.uniform(0.03, 0.08) * (rng.random((h, 1)) > 0.3)
    for _ in range(40):
        img = img + 0.15 * elip(h, w, rng.uniform(0, w), rng.uniform(0, h), rng.uniform(0.6, 1.8), rng.uniform(0.6, 1.8), 0.6)
    vig = 1 - 0.4 * np.clip(np.hypot(x - 0.5, y - 0.45) * 1.5, 0, 1) ** 2
    img = np.clip(img * vig, 0, 1)
    img = 0.08 + img * 0.82
    rgb = np.stack([img * 1.04 + 0.03, img * 0.96 + 0.015, img * 0.82], -1)
    lo = np.clip(1 - np.abs((x - y * 0.55) - 0.05) / 0.05, 0, 1) * 0.08 + np.clip(1 - np.abs((x - y * 0.55) - 0.16) / 0.02, 0, 1) * 0.05
    rgb += lo[..., None]
    bt = (np.abs((x + y) - 0.19) < 0.035)
    rgb[bt] = rgb[bt] * 0.08 + 0.02
    return np.clip(rgb, 0, 1)

if __name__ == "__main__":
    goc = Image.open(sys.argv[1]).convert("RGB").resize((1024, 1024), Image.BICUBIC)
    A = np.asarray(goc).astype(float) / 255
    # ô ảnh (toạ độ ảnh 512, gốc trên-trái) — trong PNG ảnh nằm NGƯỢC (UV mặt ảnh lật dọc) → vẽ xuôi rồi lật
    for (x0, x1, nu) in ((30, 228, True), (286, 484, False)):
        y0, y1 = 182, 486
        X0, X1, Y0, Y1 = x0 * 2, x1 * 2, y0 * 2, y1 * 2
        cd = chan_dung(Y1 - Y0, X1 - X0, nu)[::-1]
        A[Y0:Y1, X0:X1] = cd
    Image.fromarray((A * 255).astype(np.uint8)).save(sys.argv[2])
    print("xong")
