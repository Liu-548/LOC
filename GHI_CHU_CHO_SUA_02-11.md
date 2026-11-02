# LỘC — Ghi chú lỗi để sửa sau (02/11/2026)

Hai chỗ phát hiện lúc chơi thử nhánh `claude/giac-mo-dem2`. **Tạm dừng, chưa sửa** — khi nào cần thì làm theo ghi chú dưới đây.

---

## 1. Phòng Nhím trần nhà có hai màu (cam một nửa, xanh một nửa)

**Nguyên nhân (không phải lỗi vật liệu):**
- Phòng Nhím chỉ có **một đèn**: `Den_DenNgu_Nhim` (đèn ngủ, màu cam `#FFB36B`, tầm 4,5 m, cường độ 0,9, đổ bóng) — `LocHouseBuilder.cs` dòng ~995.
- Ánh sáng nền của cả nhà (scene `LOC_NhaLoc`, RenderSettings) đang là **Flat màu xanh đêm** `(0,11 ; 0,13 ; 0,19)`, sương cũng xanh `(0,10 ; 0,13 ; 0,19)`.
- Phần trần gần đèn ngủ ăn màu cam, phần xa đèn chỉ còn ánh nền xanh → chuyển từ cam sang xanh.

**Cách sửa (chọn một):**
| | Cách | Ảnh hưởng |
|---|---|---|
| a | Tăng tầm `Den_DenNgu_Nhim` lên ~6 m (cường độ 0,7–0,8) để phủ hết phòng | Chỉ phòng Nhím; ánh cam đều hơn |
| b | Thêm một đèn điểm cam rất yếu, không đổ bóng, giữa trần phòng Nhím (cường độ ~0,15, tầm 4 m) | Chỉ phòng Nhím; giữ được góc tối |
| c | Giảm xanh ánh nền toàn nhà (vd `(0,11 ; 0,12 ; 0,15)`) | Cả nhà bớt xanh, đổi không khí đêm |
| d | Giữ nguyên (cố ý: góc giường em ấm, phần còn lại lạnh) | — |

Lưu ý: đèn ngủ Nhím do cốt truyện + `LocMa` điều khiển (mất điện, "Bật đèn cho em") — thêm đèn phụ (b) thì phải gắn vào cùng công tắc `CongTac_PhongNhim` (`LocCongTac.NOI["PhongNhim"]`) để mất điện cũng tắt theo.

---

## 2. Tủ đứng ở hành lang tầng 2, trước cửa phòng Khôi — cửa 2 cánh nhưng chỉ mở như 1 cánh

**Vật:** `TuDung_RuongChanMan` đặt tại `(0,33 ; T2 ; 13,25)`, xoay 90°, dài 2,32 m dọc tường trái hành lang T2 (`LocHouseBuilder.cs` dòng ~920).

**Nguyên nhân:** bảng tủ `LocTuMo.Bang` (Editor/LocTuMo.cs) khai **một cánh duy nhất** cho tủ này:
```
Q(-1.13f, -0.29f, 0.045f, 1.755f, 0.29f, 0.31f, -1.125f, 0.301f, "cửa tủ đứng")
```
→ cả mặt trước rộng 0,84 m quay như một tấm, bản lề ở mép trái.

**Đo từ file `Models/NoiThat/BoSung/mesh/TuDung_RuongChanMan.glb`** (đã kiểm 02/11):
- Mặt cánh (vật liệu `vai__material_1_A1703D`) là **hai tấm rời**: `x −1,125 … −0,725` và `x −0,695 … −0,295` (y 0,05 … 1,75; z 0,295 … 0,307).
- **Hai tay nắm** (vật liệu `vai__material_2_B98700`) ở giữa: `x −0,77 … −0,75` và `x −0,67 … −0,65` (y 0,83 … 0,97; z 0,304 … 0,324). Miếng thứ ba `x 0,40 … 0,48, y 0,45 … 0,55` là núm của phần rương bên phải, không phải cánh.

**Cách sửa:** trong `LocTuMo.Bang`, thay một cánh bằng hai cánh (z1 nới tới 0,33 để lấy cả tay nắm):
```
Q(-1.13f, -0.71f, 0.045f, 1.755f, 0.29f, 0.33f, -1.125f, 0.301f, "cửa tủ đứng"),   // cánh trái, bản lề mép trái
Q(-0.71f, -0.29f, 0.045f, 1.755f, 0.29f, 0.33f, -0.295f, 0.301f, "cửa tủ đứng")    // cánh phải, bản lề mép phải
```
rồi chạy lại **LOC → Dựng nhà LỘC** (hoặc bước tách cánh tủ của `LocTuMo`). Kiểm bằng menu **LOC → Kiểm tra cửa tủ** (cánh không quét vào tường / đồ bên cạnh), và `hoc` (hộc trong tủ) đang khai `x −1,125 … −0,295` vẫn dùng được.

Hai cánh **cùng tên "cửa tủ đứng", cùng cha, cách nhau < 3,3 m** thì `LocDoor` tự coi là cửa đôi: bấm E vào cánh nào cũng **mở / đóng cả hai cùng lúc** (đúng yêu cầu "cửa 2 cánh phải mở/đóng cả 2 cánh"). Muốn mỗi cánh mở riêng thì đặt tên hai cánh khác nhau.
