# LỘC — Kế hoạch tính năng GIẤC MƠ (02/11/2026) · CHƯA LÀM, chờ duyệt

Dựa trên bản dựng `main` (commit `f5e8a1e`, 1/11) và kịch bản v3.3 + v3.4.

---

## 0. Đặt vào đâu trong mạch game

| | Hiện tại | Đề xuất |
|---|---|---|
| Cuối Đêm 1 | Thắp nén mới → ra khỏi hầm/phòng Nhím → cắt đen → cảnh ban ngày (bàn nước) → Đêm 2 | Thắp nén mới → *"Ngủ thôi."* → về phòng Khôi **[E] Đi ngủ** ở `GiuongDon_Khoi` → **GIẤC MƠ 1** → bật dậy trên giường → cảnh ban ngày → Đêm 2 |
| Cuối Đêm 2 | Giữ hương → cảnh nửa tỉnh ở cửa phòng Nhím → cắt đen → đưa tang → Đêm 3 | Giữ hương → nửa tỉnh (*"Còn. Ngủ đi."*) → sang phòng Khôi (sát bên) **[E] Đi ngủ** → **GIẤC MƠ 2** → bật dậy → đưa tang → Đêm 3 |

Chèn vào `LocCotTruyen.HetDem()` ngay trước `LocBanNgay.Chay()`. Đêm 3 không có giấc mơ (kết thúc bằng ending).

## 1. Nguyên tắc chung (đề xuất)

1. **Không lộ mắt đỏ trước Đêm 3.** Kịch bản giữ "gặp ma mắt đỏ lần đầu" cho Đêm 3 sau khi hé hũ. Trong mơ, ma chỉ là **bóng đen cao gầy ~1,9 m, không thấy mắt** (khớp câu Nhím Đêm 3: *"Đen sì. Cao lắm."*).
2. **Mơ không có thua thật.** Thoát hay bị bắt thì đều tỉnh dậy, đêm sau vẫn diễn ra như cũ. Chỉ lưu cờ `mo2_thoat` (sau này dùng hay không thì tùy).
3. **Hai giấc mơ dùng chung một hành lang.** Đêm 2 người chơi nhận ra chỗ quen → sợ trước khi chuyện gì xảy ra. Làm giấc mơ 1 trước cũng là dựng xong nửa kỹ thuật của giấc mơ 2.
4. **Cửa cuối hành lang = cửa sắt hầm** (`CuaSatHam`, có ổ khóa treo trên cánh). Mơ 1 cửa **khóa**, mơ 2 cửa **mở được**. Ẩn ý: thứ duy nhất cứu Khôi là cánh cửa nhà cấm mở.
5. Giữ **Space 0,8 s** để bỏ qua (giống xe buýt / đoạn bác), tắt trước khi nộp như nút B.

---

## 2. GIẤC MƠ 1 — tư vấn: làm đơn giản hơn

### 3 phương án

| | Nội dung | Công sức | Nhận xét |
|---|---|---|---|
| **A (đề xuất)** | **"Hành lang — lần đầu"**: chỉ đi bộ, không chạy được, không thua, ~60 s | Thấp (dùng lại hành lang mơ 2 + `LocFirstPerson` có sẵn) | Gieo mọi thứ của mơ 2: hành lang, đồ vật **tự động đậy**, bóng đen, cánh cửa hầm |
| B | Ký ức bến Phà Cũ (sổ trang 11: *"Thằng Khôi suýt nữa cũng đi"*): đứng dưới nước đêm, có thứ kéo chân | Trung bình–cao (nước, bờ sông, cảnh mới) | Gắn lai lịch, nhưng đụng phần "đứa trẻ chết đuối" nhóm đã bỏ; Khôi chưa đọc sổ ở Đêm 1 |
| C | Cắt cảnh 15–20 s không điều khiển: vài khung hành lang chớp nháy + tiếng thét → tỉnh | Rất thấp | Rẻ nhất nhưng mỏng, không dạy gì cho mơ 2 |

### Diễn biến phương án A (~60 s)

1. Màn đen → tiếng thở. Khôi đứng giữa hành lang tối, đồ đạc chất hai bên. **Không có Shift chạy** (*bước chân nặng*), tốc độ đi chậm hơn bình thường ~20%.
2. Hành lang **thẳng ~50 m**, chỉ có vài ngã rẽ giả bên hông (lối tối, chặn bằng đồ chất cao).
3. Giữa đường, sau lưng có tiếng **kéo lê**. Nếu ngoái lại: một cái ghế/chậu **đang lơ lửng giữa không trung**, rồi rơi xuống đánh "rầm". Không trúng ai — chỉ để người chơi biết *đồ vật ở đây biết bay*.
4. Cuối hành lang: **cửa sắt hầm, ổ khóa treo trên cánh**. [E] Mở cửa → *"Khóa."* Từ bên trong có **ba tiếng gõ**.
5. Camera bị kéo quay ra sau (khóa điều khiển): **bóng đen đứng giữa hành lang, cách ~5 m**, không động đậy. Bóng đèn cuối cùng tắt.
6. Cắt → **bật dậy trên giường** (mục 4).

---

## 3. GIẤC MƠ 2 — "Chạy" (theo yêu cầu, ~1 phút 30 – 2 phút)

### 3.1. Hành lang
- Rộng **3,6 m lối đi** (+ đồ chất sát hai tường), cao ~2,6 m, trần thấp, bóng đèn tròn treo lưa thưa, vài cái chập chờn. Sương mù đen phía xa.
- Chia **3 làn (trái · giữa · phải), mỗi làn 1,2 m**.
- Dài **~450 m** (chạy 4,5 m/s × ~100 s), ghép từ các **đoạn mẫu 15 m**: thẳng · rẽ trái 90° · rẽ phải 90° · ngã ba giả · đoạn cuối có cửa. Khoảng **5–6 khúc rẽ thật**.
- **"Nhiều ngã rẽ"** — đề xuất: khúc rẽ chính **tự rẽ theo đường** (không bắt người chơi chọn), còn các ngã rẽ hai bên là **lối giả**: tối om, chất đồ chặn, đôi khi thoáng thấy bóng đen ở trong. Lý do: chọn đường + né 3 làn cùng lúc rất khó đọc ở góc nhìn thứ nhất và dễ chết oan.
- Đồ đạc dọc hành lang **lấy từ chính đồ trong nhà** (ghế nhựa, bao xi măng, chậu, thùng, quạt, ghế gỗ…) → nhà trong mơ là nhà bị kéo dài ra.

### 3.2. Điều khiển (đề xuất)
- **Tự chạy về phía trước** (ma đuổi nên không cho dừng). **A/D chuyển làn** (trượt 0,25 s). Chuột chỉ nhìn được ±40° ngang, ±25° dọc.
- Không ngồi, không Tab, không H.

### 3.3. Con ma đuổi
- Bóng đen (model `Ma_VanNguoi` + vật liệu ma nữ, **mắt tắt**), lướt, chân tan khói — không cần animation chạy.
- Luôn cách sau lưng **~8 m**. Mỗi lần bị ném trúng thì **áp sát thêm ~2 m**.
- Mở đầu: Khôi tỉnh trong hành lang, nghe tiếng thở sau lưng, camera tự quay lại 1,5 s thấy bóng đen ở cuối hành lang → quay về trước, chữ nhỏ ***"Chạy."*** → bắt đầu.

### 3.4. Ném đồ
- Ma không cần tay ném: **đồ vật Khôi vừa chạy qua tự nhấc lên rồi bay tới** (giống hồn ma quăng đồ). Người chơi không nhìn ra sau, nên báo bằng:
  - **Dấu "!" đỏ** ở **đáy màn hình, đúng vị trí làn** sắp bị ném (3 ô trái/giữa/phải), hiện **1,5 s** trước khi đồ tới. Kèm tiếng kéo lê → tiếng gió rít cao dần.
  - Đồ bay theo đường vòng cung, tới vị trí người chơi đúng lúc hết 1,5 s.
- **Làn bị nhắm** = làn người chơi đang đứng lúc "!" hiện (70%) hoặc làn bên cạnh (30%) → đứng yên cũng không an toàn.
- **Tính trúng** ở thời điểm chạm: Khôi đang ở trong làn đó (cách tâm làn < 0,6 m) → trúng. Đang trượt giữa hai làn thì tính theo vị trí thật.
- Không trúng thì đồ **rơi xuống phía trước, lăn đi** → người chơi thấy rõ vừa né được cái gì.
- **Độ khó theo thời gian:**

| Thời gian | Nhịp ném | Ghi chú |
|---|---|---|
| 0–20 s | 4 s/lần, 1 món | Làm quen |
| 20–60 s | 3 s/lần | |
| 60–95 s | 2,2 s/lần, thỉnh thoảng **2 làn cùng lúc** | Luôn chừa 1 làn trống, luôn đủ thời gian để chuyển làn |
| 6 s cuối | Ngừng ném | Ma tăng tốc, người chơi chạy thẳng vào cửa |

### 3.5. Bị trúng
- Mỗi lần trúng: rung màn hình, chớp tối, khựng chân 0,5 s, tiếng thở gấp.
- Báo số lần trúng **không dùng thanh máu**: viền màn hình tối dần và nứt (lần 1 nhẹ, lần 2 rõ), nhịp tim to lên. *(Muốn có 3 chấm rõ ràng ở góc thì nói.)*
- **Trúng lần 3 → bị bắt** (mục 3.7).

### 3.6. Thoát — cánh cửa (~95–110 s)
- Hành lang mở ra đoạn thẳng cuối, **cửa sắt hầm** ở cuối, hé sẵn một khe.
- Chạy tới là cảnh tự chạy: tay Khôi (`LocTay`) đẩy cửa → lao vào → sập cửa → ổ khóa va lách cách → **ma đập cửa hai cái** → im.
- Tối hẳn. Ba giây. Rồi **sau lưng Khôi, trong bóng tối phía bên này cửa**: *"Anh Khôi ơi."* (giọng Nhím). *(Gợi ý tùy chọn — nhắc trang 19 sổ của bố: "Nó gọi được. Giọng y hệt con Nhím." mà Khôi vừa đọc trong Đêm 2. Không muốn thì bỏ.)*
- → Bật dậy trên giường.

### 3.7. Bị bắt — cutscene (~5 s)
1. Khôi ngã sấp, camera đập xuống sàn.
2. Camera bị lật ngửa lên: bóng đen cúi xuống sát.
3. **Bàn tay** (model `Ma_Tay` có sẵn) túm cổ áo, giật camera lên sát mặt nó (~0,4 m).
4. **Hét** (`Ma_TiengThet`): rung mạnh, mặt lao sát vào ống kính, nhiễu hình → **cắt đen ngay ở đỉnh tiếng hét**.
5. → Bật dậy trên giường (bản hoảng hơn).

> Model ma đang là tư thế đứng tĩnh, **không có miệng mở được**. Đề xuất bán cảnh hét bằng âm thanh + rung + mặt lao vào ống kính. Muốn thấy miệng há thật thì phải sửa model (thêm blendshape miệng) — làm thêm được, nhưng tốn công hơn.

---

## 4. Tỉnh dậy (chung cho cả 2 giấc mơ)
- Camera bắt đầu ở tư thế **nằm ngửa trên `GiuongDon_Khoi`** → **bật ngồi dậy trong 0,4 s**, thở dốc (`Khoi_Tho`), nhìn quanh phòng tối (đèn bàn đã tắt) → 2–3 giây → mờ đen → cảnh ban ngày / đưa tang như hiện tại.
- Độc thoại (chữ nghiêng, nháp):
  - Mơ 1: *"...Mơ."*
  - Mơ 2 thoát: *"Lại cái hành lang đấy."*
  - Mơ 2 bị bắt: *"...Mơ. Mơ thôi."* (tay run, thở lâu hơn)

---

## 5. Kỹ thuật (dựa trên code hiện có)

| Việc | Cách làm | Dùng lại |
|---|---|---|
| Hành lang | Script Editor mới `LocHouseBuilder_Mo.cs`, chạy trong **LOC → Dựng nhà LỘC**: dựng **5 đoạn mẫu** (ẩn), đặt ở rất xa nhà (x ≈ +300) | Model đồ vật trong `Models/`, cửa `CuaSatHam` |
| Ghép đường | Lúc vào mơ, `LocGiacMo.cs` (runtime) nhân bản các đoạn mẫu theo một chuỗi cố định → xong mơ thì xóa. **Scene không nặng thêm** (đang sát 100 MB) | |
| Mơ 1 | Đặt người chơi vào hành lang, dùng chính `LocFirstPerson` (khóa Shift, giảm tốc) | `LocFirstPerson`, `LocDoor` |
| Mơ 2 | `LocFirstPerson.khoa = true`, `LocGiacMo` tự điều khiển camera chạy theo đường, A/D làn, ném, "!" vẽ bằng OnGUI như chữ phụ đề | `LocAm`, `LocTay`, model ma, vật liệu `MaNu` |
| Ánh sáng | Tắt ánh sáng nhà, bật sương + 2 đèn đi theo người chơi + bóng đèn phát sáng trong hành lang; xong thì trả lại (giống `LocBanNgay.BatNgay/TraDem`) | |
| Ma nhà | `LocMa.tamDung = true` suốt giấc mơ (không lấp ló, không mất điện) | |
| Chọn đêm | "Chọn đêm 2/3" từ menu vẫn bắt đầu **sau** giấc mơ | `LocMenu` |
| Phím thử | `7` = vào mơ 1, `8` = vào mơ 2 (đang trống) + menu **LOC → Tự kiểm giấc mơ**: bot tự chạy, tự né/không né, chụp ảnh, ghi log | `LocTuKiemGamePlay` |

**Âm thanh cần thêm:** gió rít khi đồ bay · đồ rơi/vỡ (3–4 loại: nhựa, gỗ, sắt, sành) · chân chạy · nhịp tim · tiếng ma rít/thở · đập cửa sắt · gõ cửa · thở dốc khi tỉnh. Có sẵn: `Ma_TiengThet`, `Khoi_Tho`, `Dien_Chap`.

---

## 6. Thứ tự làm và kiểm thử

1. [E] Đi ngủ + cảnh tỉnh dậy + nối vào `HetDem` (chưa có mơ, chỉ cắt đen) — chơi thử Đêm 1 → 2 → 3 vẫn đi được.
2. Dựng hành lang + đoạn mẫu, chụp ảnh duyệt.
3. **Giấc mơ 1** hoàn chỉnh.
4. **Giấc mơ 2**: chạy, làn, ném, "!", trúng, độ khó.
5. Cutscene thoát / bị bắt.
6. Âm thanh.
7. Tự kiểm: chạy đủ cả 2 kết của mơ 2, đo thời lượng (phải ra 90–120 s), đếm số lần ném, kiểm "luôn có làn né được", rồi chơi qua Đêm 1 → 3 để chắc không phá các ending. Ghi báo cáo `claude/bao-cao-giac-mo-<ngày>.md`.

---

## 7. Cần chốt trước khi làm

| # | Câu hỏi | Đề xuất |
|---|---|---|
| GM-1 | Hai giấc mơ ở **cuối Đêm 1 và cuối Đêm 2**? | Có |
| GM-2 | Vào mơ bằng **[E] Đi ngủ ở giường Khôi**, hay tự động cắt khi hết đêm? | [E] Đi ngủ (để lúc tỉnh dậy trên giường là hợp lý) |
| GM-3 | Giấc mơ 1 theo phương án nào? | **A** — hành lang đi bộ, cửa khóa |
| GM-4 | Mơ 2: **tự chạy + A/D đổi làn**, hay người chơi giữ W tự chạy? | Tự chạy |
| GM-5 | Ngã rẽ: **rẽ tự động + lối giả hai bên**, hay cho chọn đường thật (rẽ sai là ngõ cụt)? | Rẽ tự động + lối giả |
| GM-6 | Ma trong mơ là **bóng đen không mắt** (giữ mắt đỏ cho Đêm 3)? | Có |
| GM-7 | Báo số lần trúng bằng **viền màn hình nứt/tối**, hay 3 chấm rõ ràng? | Viền màn hình |
| GM-8 | Cảnh hét: **âm thanh + rung + mặt lao vào** (model giữ nguyên), hay sửa model cho miệng há? | Giữ model |
| GM-9 | Câu *"Anh Khôi ơi."* sau cánh cửa khi thoát (mục 3.6)? | Có |
| GM-10 | Bị bắt / thoát có ảnh hưởng gì về sau không? | Không (chỉ lưu cờ) |
