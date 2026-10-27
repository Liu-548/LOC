# LỘC — Hướng dẫn chơi (mini game)

> Phần 1–3 là cách chơi, không lộ đáp án. **Phần 4 là lời giải đầy đủ** — chỉ mở khi kẹt.

## 1. Điều khiển
| Phím | Việc |
|---|---|
| W A S D, chuột | đi, nhìn |
| Shift | chạy |
| **E** | tương tác với vật đang nhìn (gợi ý `[E] ...` hiện trên màn hình) |
| **Tab** | mở / đóng túi đồ (lúc mở túi ma đứng yên) |
| **1 – 9** | chọn món đang cầm (khi mở Tab). Món cần thiết sẽ tự được cầm khi bạn dùng đúng chỗ |
| **Esc** | đóng mini game (bỏ dở, mở lại chơi tiếp được) |
| Chuột trái | bấm các nút trong mini game |
| Space | giữ: che gió (Giữ lửa) / nín thở (Nín thở) |

Phím thử (chỉ Editor / Development build): F1 F2 F3 đổi đêm · F7 cho đủ đồ · F8 bỏ qua mini game đang mở.

## 2. Mục tiêu mỗi đêm
Mỗi đêm có một **chiếc chén vỡ**. Các **mảnh chén** nằm sau các mini game. Gom đủ mảnh rồi xuống **hầm**, đến bàn thờ hầm, bấm E vào chén vỡ để ráp lại. Ráp xong là xong nghi thức của đêm.

| Đêm | Số mảnh cần | Mini game |
|---|---|---|
| 1 | 3 | Két khoá số · Bày mâm lễ · Xếp lại bàn học |
| 2 | 4 | Chỉnh đồng hồ · Ghép ảnh · Nối dây điện · Dò radio (+ Xếp chén theo ghế, Lên dây đàn để lấy đồ phụ) |
| 3 | 3 | Giữ lửa · Giải chữ gương · Nín thở |

Ráp chén: lưới 2×2 (đêm 1), 3×2 (đêm 2), 3×3 (đêm 3). Bấm hai mảnh để đổi chỗ cho đến khi khít.

## 2b. Chơi từ đầu đến cuối
- Đặt **Dem Bat Dau = 1** trên `LocMa`, bấm Play, không dùng phím thử.
- Ráp xong chén đêm 1 hoặc 2: màn đen, hiện "Hết đêm thứ …", sang đêm sau và bạn về chỗ xuất phát (túi đồ giữ nguyên).
- Ráp xong chén đêm 3: màn kết **HẾT**. Bấm **R** để chơi lại từ đầu (xoá túi đồ, về đêm 1).
- Chết (Death Ending) thì bấm R chơi lại từ đầu đêm đó.

## 3. Mẹo chung
- Nhìn kỹ đồ trong nhà: lịch, đồng hồ, bàn ăn, bàn học đều là manh mối của mini game khác.
- Vật nào chưa dùng được sẽ có câu báo thiếu gì. Xem túi (Tab) để biết mình đang có gì.
- Mini game sai nhiều lần sẽ hiện thêm gợi ý.
- Đêm 2 trở đi ma lấp ló; đêm 3 ma tức giận, tắt đèn từng đợt. Khi đang chơi mini game ma đứng yên, nhưng **Nín thở** thì chính nó là mối nguy.

## 4. Lời giải (spoiler)
**Đêm 1**
1. *Tủ đồ thờ (tầng 3)* — khoá số **0508**, lấy từ tờ lịch bloc dừng ở ngày 5/8 trong bếp. → Mảnh 1.
2. *Bàn thờ vong — bày mâm* — nhìn từ chỗ người đứng vái. Hàng sau: đĩa quả (trái), bát hương (giữa), bình hoa (phải). Hàng trước: đèn dầu (trái), chén nước (giữa), bát cơm quả trứng (phải). → Mảnh 2.
3. *Bàn học Khôi* — nhớ vị trí rồi xếp lại. Hàng trên: thời khoá biểu, ảnh mờ, góc đèn. Hàng dưới: mẩu giấy, cuốn nhật ký. → Mảnh 3.

**Đêm 2**
1. Nhặt *chìa lên dây* ở móc chìa khoá (sảnh sau), dùng ở *đồng hồ quả lắc*, chỉnh kim **2:40**. → Mảnh 4.
2. Nhặt 4 *mảnh ảnh* (khay ấm chén, giá sách Khôi, bàn trang điểm, tranh hành lang tầng 2), mang đến *ảnh gia đình* ở phòng khách và ghép. → Mảnh 5.
3. *Bảng điện chính* — nối dây theo màu, bấm Cấp điện. Nối sai bị chập nhưng không mất gì. → Mảnh 6.
4. *Bàn ăn* — Bố: đầu bàn mặt vào trong; Mẹ: ghế phía thang; Khôi: ghế sạch bóng; Nhím: ghế có gối kê. → Sợi dây đàn.
5. *Guitar (phòng Khôi)* — cần sợi dây đàn. Bấm Nghe nốt chuẩn rồi chỉnh sao cho khớp (thanh hiện "Khớp"). → Cuộn băng cassette.
6. *Radio cassette* — cần cuộn băng. Dò ba tần số **90,4 · 97,1 · 104,6 MHz**, mỗi nơi bấm Ghi lại. → Mảnh 7.

**Đêm 3**
1. *Cửa hầm — giữ lửa*: giữ Space khi hiện "GIÓ!", thả ra để tay hồi sức; chịu đủ ~20 giây. → Mảnh 8.
2. Nhặt *mảnh gương* ở gương treo sảnh tầng 2, mang đến *bàn học Nhím*: giơ gương lên đọc, gõ **ANH Ở DƯỚI HẦM** (không cần dấu). → Mảnh 9.
3. *Tủ áo Khôi — nín thở*: tiếng chân gần dần rồi xa dần trong ~14 giây. Giữ Space khi nó sát cánh tủ ("Nó đang ở sát cánh tủ"), thả ra để lấy lại hơi lúc nó còn xa. Bị nghe thấy quá lâu là Death Ending. → Mảnh 10.

> Một số mã, giờ và câu chữ là giả định của người thiết kế và có thể đã được đổi trong Inspector. Nếu lời giải ở đây không khớp bản đang chơi, xem `LOC_HuongDan_MiniGame.md`.
