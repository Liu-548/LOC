"""Sinh 3 decal cho phòng Khôi bỏ không 2 năm: bụi sàn, bụi mặt đồ, ố vàng tường. python3 bui.py <thư mục Textures>"""
import sys
import numpy as np
from PIL import Image
from scipy.ndimage import gaussian_filter

out = sys.argv[1]
rng = np.random.default_rng(2000)


def noise(n, sigmas):
    a = np.zeros((n, n))
    for s, w in sigmas:
        a += gaussian_filter(rng.standard_normal((n, n)), s, mode='wrap') * w
    a -= a.min(); return a / a.max()


def save(name, rgb, alpha):
    a = np.clip(alpha, 0, 1)
    img = np.dstack([np.broadcast_to(np.array(rgb, float), a.shape + (3,)) if np.ndim(rgb) == 1 else rgb, a * 255]).astype(np.uint8)
    Image.fromarray(img, 'RGBA').save(f'{out}/{name}')


n = 1024
yy, xx = np.mgrid[0:n, 0:n] / (n - 1)
edge = np.minimum.reduce([xx, 1 - xx, yy, 1 - yy])            # 0 ở mép, 0,5 ở giữa

# 1) bụi sàn: màng xám nâu loang, dày lên sát chân tường, vài hạt bụi/xơ vải
f = noise(n, [(60, 1.0), (14, 0.5), (3, 0.25)])
a = 0.08 + 0.26 * f ** 1.5 + 0.30 * np.exp(-edge / 0.04)
speck = gaussian_filter((rng.random((n, n)) > 0.9996).astype(float), 1.2) * 6
a = a + np.clip(speck, 0, 0.35)
a *= np.clip(edge / 0.004, 0, 1)                              # mép ngoài cùng về 0 (không lem khi lặp)
save('Decal_Bui_San.png', (128, 118, 102), a * 0.85)

# 2) bụi mặt đồ: lớp mịn xám sáng, mờ dần ra mép
m = 512
y2, x2 = np.mgrid[0:m, 0:m] / (m - 1)
e2 = np.minimum.reduce([x2, 1 - x2, y2, 1 - y2])
g = noise(m, [(30, 1.0), (6, 0.4), (1.5, 0.2)])
save('Decal_Bui_Mat.png', (168, 160, 146), (0.30 + 0.25 * g) * np.clip(e2 / 0.08, 0, 1))

# 3) ố vàng tường: thấm từ mép trần xuống, có viền nước đậm và vệt chảy
h, w = 512, 1024
yv, xv = np.mgrid[0:h, 0:w] / np.array([h - 1, w - 1])[:, None, None]
lo = gaussian_filter(rng.standard_normal(w), 40, mode='wrap'); lo = (lo - lo.min()) / (lo.max() - lo.min())
depth = 0.25 + 0.45 * lo                                         # độ sâu vết thấm theo chiều ngang
for _ in range(9):                                               # vệt chảy dài
    c = rng.integers(40, w - 40); depth += rng.uniform(0.08, 0.2) * np.exp(-((np.arange(w) - c) / rng.uniform(14, 30)) ** 2)
depth = np.minimum(depth, 0.97)
d = yv - depth[None, :] + 0.03 * (noise(max(h, w), [(6, 1.0)])[:h, :w] - 0.5)                                          # < 0 là vùng đã thấm
tex = noise(max(h, w), [(25, 1.0), (5, 0.4)])[:h, :w]
a = np.where(d < 0, 0.10 + 0.30 * tex ** 1.3 + 0.10 * np.clip(-d / 0.3, 0, 1), 0) + 0.32 * np.exp(-(d / 0.01) ** 2)   # viền nước đậm ở mép vết
a *= np.clip(np.minimum(xv, 1 - xv) / 0.02, 0, 1)
col = np.dstack([np.full((h, w), 148), np.full((h, w), 118), np.full((h, w), 68)]).astype(float)
col *= (0.85 + 0.15 * tex)[..., None]
save('Decal_O_Tuong.png', col, a * 0.8)

for fn in ('Decal_Bui_San.png', 'Decal_Bui_Mat.png', 'Decal_O_Tuong.png'):
    im = np.array(Image.open(f'{out}/{fn}'))
    assert im.shape[2] == 4 and 0 < im[..., 3].mean() < 200, fn   # có alpha, không trống, không đặc
print('ok')
