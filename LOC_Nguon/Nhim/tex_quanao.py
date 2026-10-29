# NHÍM — texture ba bộ quần áo (29/10). Ô lặp 256 px như T_Nhim_Ao (Repeat), nền vải cotton đã bạc, loang nhẹ.
# Chạy: python3 tex_quanao.py <thư mục xuất>
import sys, os, random
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
OUT = sys.argv[1]; os.makedirs(OUT, exist_ok=True); random.seed(8); np.random.seed(8)
W = 256

def nen(rgb, bien=7):
    """nền vải: màu + loang mềm lặp liền mép (nhiễu tuần hoàn)"""
    n = np.random.rand(16, 16); n = np.tile(n, (3, 3))
    im = Image.fromarray((n * 255).astype(np.uint8)).resize((W * 3, W * 3), Image.BICUBIC).crop((W, W, 2 * W, 2 * W)).filter(ImageFilter.GaussianBlur(6))
    a = (np.asarray(im, dtype=float) / 255 - 0.5) * 2 * bien
    base = np.array(rgb, dtype=float)[None, None, :] + a[..., None]
    return np.clip(base, 0, 255)

def luu(arr, ten):
    Image.fromarray(np.clip(arr, 0, 255).astype(np.uint8)).save(f"{OUT}/{ten}.png")

# Đêm 2 · áo ba lỗ trắng ngà kẻ sọc xanh nhạt (8 sọc / ô)
a = nen((232, 231, 222))
for k in range(8):
    y0 = k * 32 + 4
    a[y0:y0 + 9, :] = a[y0:y0 + 9, :] * 0.25 + np.array([150, 186, 214]) * 0.75
    a[y0 + 12:y0 + 14, :] = a[y0 + 12:y0 + 14, :] * 0.5 + np.array([170, 200, 222]) * 0.5
luu(a, "T_Nhim_Ao_D2")

# Đêm 2 · quần đùi kẻ ô: nền xanh lơ, dải ngang/dọc xanh đậm + trắng đan nhau (chỗ chéo đậm hơn)
q = nen((150, 176, 200))
x = np.arange(W)
dai = ((x % 64) < 14).astype(float) * 0.55 + ((x % 64 > 36) & (x % 64 < 40)).astype(float) * -0.6   # + đậm · − sáng (sợi trắng)
for ax in (0, 1):
    d = dai[:, None] if ax == 0 else dai[None, :]
    dam = np.clip(d, 0, 1)[..., None]; sang = np.clip(-d, 0, 1)[..., None]
    q = q * (1 - dam * 0.6) + np.array([52, 70, 110]) * dam * 0.6
    q = q * (1 - sang) + np.array([236, 238, 236]) * sang
luu(q, "T_Nhim_Quan_D2")

# Đêm 3 · áo hồng nhạt trơn (đã giặt nhiều, hơi xỉn) · quần lửng hồng đậm hơn
luu(nen((236, 196, 200), 9), "T_Nhim_Ao_D3")
luu(nen((206, 150, 162), 9), "T_Nhim_Quan_D3")

# Đêm 3 · hình in con thỏ hoạt hình chung chung (không phải nhân vật có thật), mực in bong tróc sau nhiều lần giặt
S = 256; im = Image.new("RGBA", (S, S), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
vien, trang, hong = (92, 84, 96, 255), (250, 248, 242, 255), (238, 150, 168, 255)
for ex in (-1, 1):   # hai tai dài, tai phải gập xuống
    if ex < 0:
        d.ellipse((78, 18, 116, 128), fill=trang, outline=vien, width=5); d.ellipse((88, 34, 106, 114), fill=hong)
    else:
        d.polygon([(142, 110), (150, 50), (196, 30), (206, 46), (170, 70), (166, 116)], fill=trang, outline=vien)
        d.line([(142, 110), (150, 50), (196, 30), (206, 46), (170, 70), (166, 116)], fill=vien, width=5)
d.ellipse((52, 92, 204, 226), fill=trang, outline=vien, width=6)                  # đầu
d.ellipse((92, 140, 104, 156), fill=vien); d.ellipse((152, 140, 164, 156), fill=vien)   # mắt
d.ellipse((96, 143, 100, 147), fill=trang); d.ellipse((156, 143, 160, 147), fill=trang)
d.polygon([(120, 168), (136, 168), (128, 178)], fill=hong)                         # mũi
d.arc((112, 170, 128, 190), 0, 180, fill=vien, width=4); d.arc((128, 170, 144, 190), 0, 180, fill=vien, width=4)
d.ellipse((66, 166, 90, 182), fill=(244, 170, 186, 200)); d.ellipse((166, 166, 190, 182), fill=(244, 170, 186, 200))   # má hồng
arr = np.array(im).astype(float)
nut = (np.random.rand(S // 4, S // 4) > 0.82).astype(np.uint8) * 255   # bong mực: mảng nhỏ trong suốt bớt
nut = np.asarray(Image.fromarray(nut).resize((S, S), Image.NEAREST).filter(ImageFilter.GaussianBlur(1.2)), dtype=float) / 255
arr[..., 3] *= 1 - 0.55 * nut
arr[..., :3] = arr[..., :3] * 0.92 + 255 * 0.08 * (arr[..., 3:] > 0)              # mực hơi phai
Image.fromarray(arr.astype(np.uint8)).save(f"{OUT}/T_Nhim_InTho.png")
print("ok")
