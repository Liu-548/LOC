# LỘC — Ghi chú vấn đề sau lượt chơi thử 27/10/2026

Bản này đi kèm commit đưa **kịch bản v3.3** vào Unity, cùng các sửa ngày 27/10.
Kiểm bằng **LOC → Chơi thử một mạch (Play)** (`Assets/LOC_House/Scripts/LocChoiThu.cs`, chỉ chạy trong Editor): công cụ tự đi menu → xe buýt → 3 đêm → Death → chơi lại ở điểm lưu → Bad (a) → Good → Evil → về menu. Lượt cuối dài 767 s, **0 mục lỗi**. Log và ảnh nằm ở `LOC_KiemTra/ChoiThu/` (không đưa lên git).

> **Các mục [CỐT LÕI] của v3.3 chưa được cả nhóm xác nhận.** Xem `kich-ban-loc-v3.3.md` Phần F trong Project, và `kich-ban-loc-v3.4-thay-doi.md` cho những gì đổi sau v3.3.

## 1. Đã sửa trong lần này
- Thoại / chữ theo v3.3: bác (phụ đề **BÁC**, giọng Bắc), hầm, két, sổ bố (trang 1, 3, 7, 11, 17–21), sổ thu chi có trang tháng 8, mini game, đoạn hoảng loạn.
- **Nhím biết và hiểu bố mẹ đã mất** (chốt 27/10). Bác: "Nó biết rồi…". Nhím Đêm 1: "Anh Khôi ơi. Em nhớ mẹ." **Bỏ hẳn cảnh vẽ lên bột.**
- Lấp ló 90–120 s, chỉ sau đoạn hoảng loạn, không lặp điểm cũ, phản ứng của Khôi nhạt dần qua 3 lần đầu. Từ Đêm 2 các cửa phòng để mở sẵn.
- Lần mất điện đầu luôn an toàn ("Đứng yên. Bố viết thế."). Hai điểm lưu ngầm ở Đêm 3. Nút chết đổi thành "Chơi lại".
- Giữ hương kết thúc Đêm 1–2. **Đêm 2 chơi thông được**: đọc hết sổ bố → hoảng loạn → giữ hương.
- Đồng hồ quả lắc Đêm 3 (đủ 12 tiếng → Bad a). Hai cuộc gọi không lời. Tờ lịch. Thắp hương ông bà rồi mới lấy sách. Đêm 3 không còn linh cữu.
- Bày mâm chỉ mở sau khi hé hũ.
- Good / Bad / Evil: **tạm** kết bằng màn đen + chữ cuối, rồi về menu.
- Băng vòng hoa "THÀNH KÍNH PHÂN ƯU" (trước là "8888…"). Tường hầm dán kín bùa 4 mặt. Tắt chữ debug.
- **Model bác:** mắt 3,0 × 1,5 cm, hai tay đặt hờ trước bụng, không lồng vào nhau. Script nguồn `LOC_Nguon/Di` đã cập nhật theo.

## 2. Lỗi / thiếu sót còn mở
| # | Vấn đề | Ghi chú |
|---|---|---|
| 1 | Nhím nằm **quay lưng ra cửa, đầu về tường sau** (kịch bản: đầu về tường phải, nhìn ra cửa) | Phải đổi tư thế nằm hoặc hướng giường của model |
| 2 | Tay bột của Nhím nằm dưới chăn, người chơi không thấy | Không còn ảnh hưởng vì đã bỏ vẽ bột. Nếu sau này cần thấy bột thì sửa tư thế |
| 3 | Lần mất điện đầu: ma cách ~4,5 m (kịch bản 5–6 m) | Nhiều chỗ không đủ khoảng trống phía trước |
| 4 | Tầng 2 rất ít chỗ lấp ló. Hành lang cạnh phòng Nhím gần như không có | Mép cửa phòng Khôi bị tủ áo che (TL-12) |
| 5 | Bật lớp phim cũ thì cả game xám đục, thiếu tương phản. Nhìn qua cửa sổ xe buýt gần như không thấy gì | Cần chỉnh mức "Đầy đủ" |
| 6 | Phòng khách có **hai** linh cữu, trong khi v3.3 ghi bản dựng có một | Chờ chốt TL-31 |
| 7 | Thoại bác dài ~70 s, người chơi đứng yên | Cân nhắc cắt bớt hoặc cho đi lại trong lúc nghe |
| 8 | Mất điện ở phòng hẹp đôi khi không đặt được ma (tối mà không có gì) | Vô hại, nhưng phí một lần dọa |
| 9 | Coroutine thăm Nhím không dừng khi đổi đêm giữa chừng (bằng phím thử) → thoại có thể chạy chồng | Chỉ gặp khi nhảy đêm bằng F1–F3 |
| 10 | Bài tự kiểm Nhím cũ còn chờ bước cũ | Cần cập nhật theo v3.3 |

## 3. Chưa dựng (đang thay tạm)
- **Đêm 2:** cháo, lọ keo ở quầy, chén số 3 trên tầng ba, **cuộc gọi của thầy Đồng**, tờ giấy chép lời thầy. Cảnh nửa tỉnh cuối đêm có sẵn nhưng chưa nối.
- **Đêm 3:** kiện hàng + tờ văn khấn, lời nhắn mẹ, hai tờ giấy gói, cắt cảnh đưa tang.
- **Nghi lễ:** vòng đọc văn khấn, **chỗ rẽ cuối** (Niêm hũ / Xé giấy niêm), **cảnh cổng sáng mùng một** (Bad c / Evil).
- **Phần thân các kết:**
  - Good: xé niêm, bốn lọn tóc, "Cảm ơn", bùa rơi, điện thoại reo.
  - Bad: chén 5 vỡ, cảnh bốn năm sau. Bad (b) nhang tắt chưa nối vào kết.
  - Evil: cuộc gọi "Cháu khá hơn bố cháu", cảnh năm năm sau.
  - Death: khung hình chén số 4 vỡ.
- **Đồ vật / model:** sổ đỏ + giấy báo nợ trong két, ba bộ quần áo Nhím, cốc nước + vỉ thuốc, vòng khoanh ngày 9 trên lịch, tay Khôi, clip bác nắm tay.
- **Âm thanh:** chuông điện thoại, tiếng rè, tiếng tút, tiếng đồng hồ gõ đều là âm tổng hợp tạm.

## 4. Chưa kiểm được bằng công cụ (cần người chơi tay)
- Cuộc gọi thứ nhất Đêm 1: chỉ bật khi đi bộ qua hộp thang hầm.
- Lấp ló khi đi bộ thật ở cầu thang và phòng khách. Gợi ý H, va chạm khi đi, đoạn bê mâm.

## 5. Sửa sau lượt chơi tay tối 27/10 (nhánh `claude/sua-loi-test-1`)
Theo danh sách 15 lỗi của lượt chơi tay. Kiểm bằng biên dịch toàn bộ script (bản Editor và bản build) với API Unity giả lập, **chưa chạy trong Unity**: cần chơi lại để xác nhận.

| # | Lỗi | Đã sửa | Cần dựng lại nhà? |
|---|---|---|---|
| 1 | Cần chấm tròn giữa màn hình | `LocFirstPerson`: chấm tròn mờ giữa màn hình, to và sáng hơn khi nhìn trúng vật bấm E được. Tắt: `LocFirstPerson.chamGiua = false` | Không |
| 2 | Chân không chạm đất | Người chơi: skin của CharacterController 0,08 → 0,03 (hết lơ lửng 8 cm, bớt mắc mép bậc). Bác (`LocDi`): mỗi khung kéo bàn chân về sát sàn ngay dưới, lúc đi ra cổng chỉ đi theo mặt phẳng | Không |
| 3 | Công tắc bếp bật/tắt không thấy đèn đổi | Chỉ công tắc hầm từng được nối đèn. `LocCongTac` tự nối mọi công tắc với đèn của phòng theo tên (bảng `NOI`). Bếp và hành lang T2 là công tắc hai chiều. Đèn tắt thì bóng/tuýp gần đó thôi sáng. Công tắc phòng Nhím **không** nối (đèn ngủ do cốt truyện điều khiển) | Không |
| 4, 9 | Kẹt đầu khi xuống hầm, khi lên gác | `LocFirstPerson` tự cúi xuống 1,2 m khi phía trước vướng ở tầm đầu mà thân vẫn lọt, thoáng thì tự đứng lên | Không |
| 5, 6, 15 | Bật/tắt đèn: ma lấp ló không biến, bấm liên tục vẫn thấy ma, "reset ma" lúc được lúc không | Bấm công tắc bất kỳ: ma đang lấp ló rụt đi ngay, lần lấp ló sau lùi ít nhất nửa khoảng lấp ló (≥ 45 s). Đang mất điện (ma tức giận / dọa) thì bấm chỉ ghi nhớ, có điện lại mới áp; đèn đã tắt bằng công tắc thì có điện lại vẫn tắt | Không |
| 7 | Cửa chính quét xuyên cột rạp ngoài sân | Cửa chính mở 130° thay vì 180° (cột rạp nằm ở ~145°), sửa cả scene và `LocHouseBuilder`. Thêm `LocDoor.KiemGoc`: lần đầu dùng, mọi cánh cửa quay tự quét thử và dừng trước vật có va chạm đầu tiên trên đường quét (tắt: `LocDoor.TuChanGoc = false`) | Không |
| 8 | Cáo phó chặn trước cửa ngách cổng | Dời bảng sang trái cổng chính, sát trụ trái (x 2,69), sửa cả scene và builder | Không |
| 10 | Bài vị không phải linh vị | `XayBaiVi` dựng lại thành linh vị giấy: tờ giấy trắng viền đen chữ "LINH VỊ" trong khung mỏng màu đen, có chân chống, bỏ tấm gỗ và đế gỗ. Tên người mất để trống 🗣 | **Có** |
| 11 | Nhìn lẵng hoa bàn thờ vong vẫn chọn được | Việc "thắp hương" chỉ nhận khi tia nhìn trúng dải giữa bàn thờ (±0,55 m quanh bát hương), không nhận lẵng hoa / vòng hoa hai bên | Không |
| 12 | Không nhảy được | Space để nhảy (cao ~0,45 m). Không nhảy khi đang ngồi / đang tự cúi | Không |
| 13 | Đèn hầm có ánh sáng mà không có bóng, đèn một nơi ánh sáng một nơi | Đèn `Den_BongDayToc_Ham` lệch 0,7 m so với bóng của model: lúc chạy dời đèn về ngay dưới bóng và cho bóng phát sáng khi bật. Đèn chân thang hầm và đèn chiếu tới: thêm bóng dây tóc + dây điện thả từ trần (chỉ trong hầm, đúng luật dự án) | Không |
| 14 | F1–F3 không đổi đêm | Phím vẫn chạy, nhưng chữ báo bị tắt nên không thấy gì. Nay hiện phụ đề "[thử] Đêm n". **Lưu ý:** phải bấm chuột vào cửa sổ Game trước, nếu không Unity Editor nuốt phím F2 (đổi tên trong Hierarchy) | Không |

**Lỗi tương tự tìm thêm và đã sửa**
- Mọi công tắc trong nhà (trừ hầm) đều chỉ lật phím: đã nối như mục 3.
- Đèn có ánh sáng mà không có đèn: cuối phòng khách (dời sang x 5,2 cho tránh quạt trần), kho, phòng giặt, sảnh T2, góc làm việc, trần phòng Khôi. Đã thêm đèn trong `LocHouseBuilder` (**cần dựng lại nhà**). Đèn chiếu nghỉ T1/T2, giếng trời, sân phơi chưa thêm: không chắc trên đầu có trần để treo, hoặc là ánh sáng trời 🗣.
- Mọi cánh cửa quay có thể quét xuyên đồ: `LocDoor.KiemGoc` như mục 7.
- `LocMa.DoCua` nằm trong `#if UNITY_EDITOR` nhưng được gọi lúc chạy: bản build (không phải Editor) sẽ **không biên dịch được**. Đã dời ra ngoài.
