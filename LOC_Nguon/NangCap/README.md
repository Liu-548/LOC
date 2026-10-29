# Nâng cấp asset LỘC (29/10/2026)

Dựng lại toàn bộ model đồ vật bằng Blender (bpy 5.2, chạy như module Python), xuất đè đúng file `.glb` cũ (giữ GUID, gốc toạ độ, tên node mà code tham chiếu).

- `lib.py` — thư viện dựng (hộp vát, tiện tròn, ống, đệm, khăn rủ…), UV theo mét, gộp node, xuất glb, ảnh xem trước; `nang_tu_goc()` = nâng cấp tự động (vát/bo mịn + vật liệu PBR theo lớp) cho món chưa có công thức tay.
- `ct_g1_*.py`, `ct_g2_*.py` — công thức dựng tay theo mô tả trong prompt GĐ3–GĐ5; `ct_tudong.py` — mọi món còn lại.
- `texgen.py` — bộ texture PBR dùng chung `Assets/LOC_House/Textures/PBR/D2_<lớp>_{A,N,M}.png`; Unity gắn qua `LocChiTiet.ApD2` theo tiền tố tên vật liệu `<lớp>__…`.
- `nhom_theo_tang.json` — danh sách món theo 3 nhóm (1: T1 + hầm + sân + tiệm · 2: T2 · 3: T3).
- Chạy: `python3 run.py -- <thư mục ra> <module> [tên món…]` (cần `goc_paths.json` trỏ tới file gốc; bản gốc trước nâng cấp: `_models_goc.tgz`).
