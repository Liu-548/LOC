# [03/11 chiều] Ảnh thờ cho chế độ THƯỜNG (không "Thực tế"): lấy đúng ảnh ông / bà của bản Blender (AnhTho_Atlas.png 1024)
#  thu về 512 px như atlas gốc, giảm nhẹ số mức xám → hợp phong cách PSX (lọc Point đặt lúc chạy trong LocSuaNha.AnhThoThuong).
# Chạy: python3 anh_tho_thuong.py <AnhTho_Atlas.png> <ra AnhTho_Atlas_Thuong.png>
import sys, numpy as np
from PIL import Image
a = np.asarray(Image.open(sys.argv[1]).convert("RGB")).astype(np.float32) / 255
a = a.reshape(512, 2, 512, 2, 3).mean((1, 3))          # 1024 → 512 (trung bình 2×2)
a = np.round(a * 47) / 47                               # 48 mức mỗi kênh: hơi "bậc" như ảnh pixel cũ, vẫn rõ mặt
Image.fromarray((a * 255 + 0.5).astype(np.uint8), "RGB").save(sys.argv[2], optimize=True)
print("xong", sys.argv[2])
