# Bác tài xe buýt (13/11)

Dựng theo cùng quy trình với người dì (`LOC_Nguon/Di`): lưới gốc MakeHuman (base mesh + targets, CC0 từ
github.com/makehumancommunity/makehuman, `makehuman/data`) → đàn ông châu Á ~54 tuổi, cao 1,65 m.

- `b1_base.py` lưới + xương rút gọn · `b2_geo.py` sơ mi cộc tay + quần âu, xoá da bị che
- `b3_build.py` tóc ngắn muối tiêu, dép, giảm lưới (~1,8k tam giác), UV, texture vẽ bằng numpy (mặt, râu lún phún, nếp nhăn)
- `b4_ngoi.py` tư thế ngồi lái: IK hai đoạn đặt hai tay lên vô lăng (toạ độ vô lăng trong `LocKhuPho_TuyenXe`), chân lên bàn đạp;
  áp tư thế vào lưới, đặt mốc Goc / Truoc / Tren, xuất `BacTai.fbx` tĩnh (không xương).

Cần: `pip install bpy` (Blender 5.3) và thư mục `mh/` (base.obj, default.mhskel, default_weights.mhw, makehuman/data/targets.npz).
Trong game: `LocXeBuyt.DungBacTai()` xoá khối thô cũ khỏi lưới thân xe và đặt model này vào ghế lái.
