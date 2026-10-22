# 22/10: vẽ lại vùng mắt (viền mi khớp mép mí, tròng 12,6 mm, lòng trắng ngà, bóng mí). Chạy: python3 b5b_mat.py <thư_mục_ra> moi (cần bake.npz, hid.npy, id.png sinh từ b6.blend — xem b7_giam.py)
# [22/10] Vẽ lại vùng mắt Nhím cho tự nhiên: viền mi khớp mép mí thật + nhãn cầu (tròng to/đậm hơn, bóng mí trên)
import sys; sys.path.insert(0, "/home/claude/nhimlp")
import numpy as np, os
from PIL import Image
TX = sys.argv[1]; os.makedirs(TX, exist_ok=True)
MOI = sys.argv[2] == "moi"
g = {"TX": TX, "MOI": MOI}
D = np.load("/home/claude/nhimlp/bake.npz"); p = D["pos"]; n = D["nrm"]; cov = D["cov"]
g.update(p=p, n=n, cov=cov, x=p[..., 0], y=p[..., 1], z=p[..., 2], S=1024, HID=np.load("/home/claude/nhimlp/hid.npy"))
# mép lỗ mắt đo từ ảnh chiếu thẳng (1 px = 0,1 mm)
a = np.asarray(Image.open("/home/claude/nhimlp/out/id.png").convert("L")) > 128
CX, CU, CL = [], [], []
for s in (1, -1):
    xs, us, ls = [], [], []
    for col in range(a.shape[1]):
        xm = (col - 600) * 1e-4 * s
        rows = np.nonzero(a[:, col])[0]
        if xm <= 0.005 or len(rows) == 0: continue
        xs.append(xm); us.append(1.099 + (300 - rows.min()) * 1e-4 + 0.00005); ls.append(1.099 + (300 - rows.max()) * 1e-4 - 0.00005)
    o = np.argsort(xs); CX.append(np.array(xs)[o]); CU.append(np.array(us)[o]); CL.append(np.array(ls)[o])
g.update(CX=CX, CU=CU, CL=CL)
exec(open("/home/claude/nhimlp/da_mod.py", encoding="utf-8").read(), g)
for lv, f in enumerate((0.05, 0.35, 0.6, 0.85)):
    g["save"](g["da"](f), f"T_Nhim_Da_{lv}")
# ─── nhãn cầu (256): uv = tâm mắt chiếu thẳng, bán kính 15,7 mm; hàng 0 = đáy
noise, mix, save = g["noise"], g["mix"], g["save"]
E = 256; yy, xx = (np.mgrid[0:E, 0:E] + 0.5) / E
for lv, f in enumerate((0.05, 0.35, 0.6, 0.85)):
    if not MOI:
        r = np.hypot(xx - 0.5, yy - 0.5) * 2; ang = np.arctan2(yy - 0.5, xx - 0.5)
        sc = np.broadcast_to(np.array([0.93, 0.91, 0.88]), (E, E, 3)).copy()
        vein = (np.abs(noise(E, E, 10, seed=5) - 0.5) < 0.012) * np.clip((r - 0.45) / 0.5, 0, 1)
        sc = mix(sc, (0.85, 0.45, 0.45), vein * (0.04 + 0.16 * f) + np.clip(r - 0.6, 0, 1) * 0.4 * f)
        iris = np.array([0.24, 0.15, 0.10]) * (0.75 + 0.5 * (0.5 + 0.5 * np.sin(ang * 23 + noise(E, E, 20, seed=6) * 6)))[..., None]
        iris = mix(iris, (0.12, 0.08, 0.06), np.clip((r - 0.30) / 0.05, 0, 1))
        c = mix(sc, iris, r < 0.35); c = mix(c, (0.02, 0.02, 0.02), r < 0.13)
        c = mix(c, (0.75, 0.68, 0.62), (r >= 0.35) * (r < 0.37) * 0.3)
    else:
        rs = np.hypot(xx - 0.5, yy - 0.5) * 2                      # theo tâm nhãn cầu
        ri = np.hypot(xx - 0.5, yy - 0.515) * 2                    # tròng nhích lên 0,25 mm để mí trên che mép trên
        ang = np.arctan2(yy - 0.515, xx - 0.5)
        sc = np.broadcast_to(np.array([0.90, 0.86, 0.82]), (E, E, 3)).copy()   # lòng trắng ngà, không trắng tinh
        sc = mix(sc, (0.74, 0.66, 0.62), np.clip((np.abs(xx - 0.5) * 2 - 0.30) / 0.45, 0, 1) * 0.55)   # tối dần về hai khoé
        vein = (np.abs(noise(E, E, 10, seed=5) - 0.5) < 0.012) * np.clip((rs - 0.45) / 0.5, 0, 1)
        sc = mix(sc, (0.85, 0.45, 0.45), vein * (0.04 + 0.16 * f) + np.clip(rs - 0.6, 0, 1) * 0.4 * f)
        IR = 0.40                                                   # tròng 12,6 mm: mí trên và mí dưới đều che một chút
        sw = 0.5 + 0.5 * np.sin(ang * 23 + noise(E, E, 20, seed=6) * 6)
        iris = np.array([0.20, 0.125, 0.08]) * (0.80 + 0.35 * sw)[..., None]
        iris = mix(iris, (0.30, 0.19, 0.11), np.clip(1 - (ri - 0.12) / 0.12, 0, 1) * 0.45)   # quầng sáng quanh đồng tử
        iris = mix(iris, (0.07, 0.05, 0.04), np.clip((ri - IR * 0.80) / (IR * 0.20), 0, 1))  # viền tròng sẫm
        am = np.clip((IR - ri) / 0.012, 0, 1)                        # mép tròng mềm
        c = mix(sc, iris, am); c = mix(c, (0.025, 0.02, 0.02), np.clip((0.115 - ri) / 0.01, 0, 1))
        # bóng mí trên đổ xuống nhãn cầu
        c = c * (1 - 0.45 * np.clip((yy - 0.54) / 0.10, 0, 1))[..., None]
        # đốm sáng nhỏ, mềm (trên-ngoài tròng)
        c = mix(c, (0.97, 0.96, 0.94), np.exp(-(((xx - 0.585) / 0.022) ** 2 + ((yy - 0.565) / 0.022) ** 2)) * 0.85)
    save(c, f"T_Nhim_Mat_{lv}")
print("xong")
