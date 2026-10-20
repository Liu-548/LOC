# LỘC — Hệ mini game (hướng dẫn dùng)

Cảm hứng: chuỗi *tìm đồ → ghép/dùng đúng chỗ → giải đố cận cảnh → thưởng* của Paper Bride. Xương sống: mỗi đêm có một **chiếc chén vỡ**; các mảnh nằm sau mini game; gom đủ rồi xuống hầm ráp lại ở bàn thờ hầm = **nghi thức của đêm** (`LocNghiThuc.DaXong`).

## Cách dựng vào scene
1. Mở scene `LOC_NhaLoc`, menu **LOC ▸ Mini game ▸ Dựng hệ mini game vào scene**.
2. Đọc Console: vật nào tìm thấy theo tên trong bản vẽ thì gắn thẳng; không thấy thì tạo cục đánh dấu (BoxCollider 0,4 m) đúng toạ độ bản vẽ (dòng bắt đầu bằng `!`). Nên dời cục đánh dấu vào đúng vật.
3. Lưu scene. Chạy lại menu được (xoá bản cũ rồi dựng lại).

## Phím
Tab: túi đồ · 1–9: cầm món · E: tương tác · Esc: đóng mini game · F1/F2/F3: đổi đêm (có sẵn) · **F7**: cho đủ đồ · **F8**: bỏ qua mini game đang mở (F7/F8 chỉ trong Editor / Development build).

## Danh sách (đêm → mini game → thưởng)
| Đêm | Chỗ | Mini game | Thưởng |
|---|---|---|---|
| 1 | tủ đồ thờ (T3) | Khoá số, mã 0508 (lịch bloc bếp dừng 5/8) | Mảnh 1 |
| 1 | bàn thờ vong | Bày mâm lễ (`LocMG_MamCung`) | Mảnh 2 |
| 1 | bàn học Khôi | Xếp lại đồ trên bàn (`LocMG_XepLaiBan`, có giai đoạn nhìn trước) | Mảnh 3 |
| 2 | móc chìa → đồng hồ quả lắc | Chỉnh đồng hồ tới 2:40, kim 3D nhảy theo | Mảnh 4 |
| 2 | 4 mảnh ảnh → ảnh gia đình | Ghép ảnh (hoán vị ô) | Mảnh 5 |
| 2 | bảng điện chính | Nối dây điện (sai thì chập) | Mảnh 6 |
| 2 | bàn ăn | Xếp chén theo ghế | Sợi dây đàn |
| 2 | guitar (cần dây) | Lên dây đàn (âm sinh bằng code) | Cuộn băng |
| 2 | radio (cần băng) | Dò sóng, ghi 3 đoạn | Mảnh 7 |
| 3 | gương sảnh → bàn học Nhím | Giải chữ gương, gõ lại dòng chữ | Mảnh 9 |
| 3 | cửa hầm | Giữ lửa đèn dầu (thời gian thực) | Mảnh 8 |
| 3 | tủ áo Khôi | Nín thở trốn; bị nghe thấy = Death Ending | Mảnh 10 |
| 1–3 | bàn thờ hầm | Ráp chén (lưới 2×2 / 3×2 / 3×3 theo đêm), cần đủ mảnh | Nghi thức xong |

## Code
Thư mục `Assets/LOC_House/Scripts/MiniGame/`: `LocId` (mã đồ) · `LocTui` (túi + cờ + HUD) · `LocTuongTac` (nhặt / dùng / mở mini game, theo đêm) · `LocMiniGame` (lớp nền IMGUI) · `LocMG_*` (các mini game). Thêm mini game mới: kế thừa `LocMiniGame`, viết `VaoGame()` và `Ve(Rect)`, gọi `Thang()`/`Sai()`.

Sửa file gốc: `LocFirstPerson` (nhận `LocTuongTac`, không khoá chuột khi `khoa`), `LocDoor` (`cauKhoa`), `LocMa` (`BatChet()`), `LocDongHo` (`DatGio`).

## Giả định cần bạn chốt
Mã 0508, giờ 2:40, lời nhắn "ANH Ở DƯỚI HẦM", 3 đoạn radio, luật bày mâm, thứ tự mảnh theo đêm, toạ độ cục đánh dấu. Tất cả sửa được trong Inspector.

## Chưa kiểm
Mình **không có Unity** trong môi trường này: toàn bộ script chỉ được biên dịch bằng `mcs` với bản stub API Unity (sạch lỗi), chưa chạy thử trong Play mode. Nếu Unity báo lỗi hay hành vi lệch, gửi mình log. Phần dò Collider/vị trí của builder cũng chưa thử trên scene thật.
