# LỘC — Việc còn lại về gameplay (cập nhật 27/10/2026)

File này gom mọi phần gameplay **chưa làm**, **làm dở** và **chưa test kỹ**, kèm hướng dẫn cơ bản để làm tiếp. Danh sách lỗi sau lượt chơi thử nằm ở `GHI_CHU_VAN_DE_27-10.md`. Kịch bản gốc: `kich-ban-loc-v3.3.md` và `kich-ban-loc-v3.4-thay-doi.md` (trong Project Claude của nhóm).

> Quy ước nhóm: **mọi nội dung đều được sửa, nhưng phải bàn với cả nhóm trước.** Mục có dấu 🗣 là phần kịch bản chưa chốt, cần bàn trước khi làm.

---

## 0. Bản đồ code: muốn sửa gì thì mở file nào

| Muốn làm | File chính | Ghi chú |
|---|---|---|
| Mạch truyện từng đêm (Đêm 1/2/3, cuộc gọi, đồng hồ, hết đêm, điểm lưu, kết) | `Assets/LOC_House/Scripts/LocCotTruyen.cs` | Mỗi đêm có một hàm `Dem1()` / `Dem2()` / `Dem3()` gọi mỗi khung hình. Thêm bước mới = thêm điều kiện cờ + `StartCoroutine(...)` |
| Nhím, các cảnh trong phòng Nhím, cảnh kết Good / Bad / Evil, hoảng loạn, nửa tỉnh | `Scripts/LocNhim.cs` | Cảnh kết gọi `SauGood` / `SauBad` / `SauEvil`. Ba hàm này được nối trong `LocCotTruyen.Start` (hiện chỉ tới `KetThuc(chữ)`) |
| Con ma: lấp ló, mất điện, Death | `Scripts/LocMa.cs` | Đêm hiện tại: `LocMa.dem`. `LocMa.tamDung = true` khi có cảnh/menu |
| Túi đồ, cờ cốt truyện, trang đọc, độc thoại | `Scripts/LocTui.cs` | `LocTui.Dat("co")`, `LocTui.Co("co")`, `LocTui.Nhan(id)`, `LocTui.Noi("câu…")` |
| Đồ nhặt / đọc / xem trong nhà | `Editor/LocHouseBuilder_GamePlay.cs` → hàm `Mon(...)` | Đặt `id`, tên, kiểu (`Nhat`/`Doc`/`Xem`), trang đọc, `tuDem`, `canCo` |
| Mini game và vật cần món để dùng | `Scripts/MiniGame/LocTuongTacMG.cs`, `Editor/LocMiniGameBuilder.cs` | `Loai.Nhat` / `Loai.Dung` / `Loai.MoMiniGame`. Thắng thì đặt cờ = `id` |
| Menu, chơi lại, lưu đầu đêm, mục Ending | `Scripts/LocMenu.cs` | `LocMenu.DatKet("good")` ghi kết đã đạt |
| Thoại có người nói | `LocCotTruyen.Thoai(("BÁC","…"), ("KHÔI","…"))` | Độc thoại nội tâm dùng `LocTui.Noi(...)` |

**Quy trình mỗi lần sửa:**
1. Sửa script. Nếu đổi vật trong nhà thì sửa file `Editor/LocHouseBuilder_*.cs`, **đừng** kéo thả tay trong scene, vì lần dựng sau sẽ bị ghi đè.
2. Chạy **LOC → Dựng nhà LỘC (tạo scene)**.
3. Kiểm bằng **LOC → Chơi thử một mạch (Play)**. Log và ảnh nằm ở `LOC_KiemTra/ChoiThu/`. Những gì cần đi bộ thật thì chơi tay.
4. Phím thử khi Play: `F1–F3` nhảy đêm, `F8` cảnh nửa tỉnh, `F9` Good, `F10` Bad, `9` hết đêm ngay, `B` bỏ qua mini game, `Shift+1…5` các kiểu dọa của ma.

---

## 1. Chưa làm (chưa có trong bản dựng)

### 1.1 Đêm 2
| Việc | Hướng làm |
|---|---|
| **Cuộc gọi của thầy Đồng** (cuộc gọi chính, có lời) | Trong `Dem2()`, sau cuộc gọi không lời (`goi_2`) và trước khi đọc hết sổ: dùng lại coroutine `Reo(hoi, choNhac: true)` (tách ra một bản có thoại), rồi `Thoai(("THẦY ĐỒNG","…"))`. Xong thì đặt cờ `goi_thay`. Thoại lấy theo v3.3 mục C5 Đêm 2 🗣 |
| Tờ giấy chép lời thầy | Một `Mon(...)` kiểu `Nhat`, `tuDem: 2`, `canCo: "goi_thay"`, có `trang` để đọc |
| Gom đồ: cháo, lọ keo ở quầy, chén số 3 (tầng ba) | Mỗi món là một `LocTuongTacMG` `Loai.Nhat` (hoặc `Mon` kiểu `Nhat`). Bước **dán chén** = `Loai.MoMiniGame` dùng `LocMG_GhepChen`, `canId` = chén + keo. Thắng thì đặt cờ nối vào `Dem2()` |
| Cảnh nửa tỉnh cuối đêm | Cảnh đã có (`LocNhim.CanhNuaTinh()`, thử bằng F8). Chỉ cần gọi nó trong `HetDem()` của Đêm 2, trước cắt cảnh ban ngày |

### 1.2 Đêm 3
| Việc | Hướng làm |
|---|---|
| Kiện hàng + tờ văn khấn | Kiện hàng là `LocTuongTacMG` (mở ra → `LocTui.Nhan("van_khan")`). Tờ văn khấn cần có `trang` để đọc. Prompt asset đã có ở `prompt-gui-claude-design-gd6/02-…` |
| Lời nhắn của mẹ, hai tờ giấy gói | `Mon(...)` kiểu `Doc` hoặc `Nhat`, `tuDem: 3`. Nội dung theo v3.3 🗣 |
| Cắt cảnh đưa tang | Thay cảnh ban ngày cũ giữa Đêm 2 → 3 trong `HetDem()` / `LocBanNgay`. Có thể làm đơn giản: vài khung tĩnh + phụ đề |

### 1.3 Nghi lễ và chỗ rẽ kết (**phần quan trọng nhất**)
Hiện tại **người chơi chưa tự vào được Good và Evil**. Hai kết này chỉ chạy bằng phím thử hoặc công cụ chơi thử.

| Việc | Hướng làm |
|---|---|
| Vòng đọc văn khấn | Thêm vòng 2 giữa `nl_bay_mam` và `nl_giu_nhang` (`LocMiniGameBuilder.cs`, đoạn `mam.hien = …`). Dạng đơn giản: mở tờ văn khấn trong túi, lật hết trang thì đặt cờ `nl_doc_van` |
| **Chỗ rẽ cuối: Niêm hũ / Xé giấy niêm** | Sau `nl_giu_nhang`, cho hũ cốt (`LocHuCot.cs`) có hai lựa chọn `[E] Niêm hũ` / `[F] Xé giấy niêm`. Điều kiện kết (theo v3.3): xé niêm → **Good**; niêm hũ khi **đã đọc sách** (`doc:sach_gia_le`) → **Evil**; niêm hũ khi chưa đọc → Bad (c) 🗣 |
| Cảnh cổng sáng mùng một (Bad c / Evil) | Một cảnh mới trong `LocCotTruyen`: dịch người chơi ra cổng, trời sáng (đổi đèn), độc thoại, rồi `KetThuc(...)` |

### 1.4 Phần thân các kết (hiện chỉ là màn đen + chữ cuối → về menu)
Thay dần `KetThuc(chu)` trong `LocCotTruyen.Start` bằng coroutine riêng cho từng kết. Giữ dòng `LocMenu.DatKet(...)`.
- **Good:** xé niêm, bốn lọn tóc, Nhím nói "Cảm ơn", bùa rơi, chữ cuối, điện thoại reo.
- **Bad (a/b):** cảnh hầm chén số 5 vỡ, "bốn năm sau". **Bad (b)** (nhang tắt khi giữ nhang) chưa nối vào kết: khi `LocMG_GiuLua` thua thì gọi `LocNhim.I.CanhBad()`.
- **Evil:** Khôi ở chân cầu thang, tiếng gọi "Anh Khôi ơi…", cuộc gọi "Cháu khá hơn bố cháu", cảnh năm năm sau. Hiện Nhím vẫn nằm trong phòng, sai kịch bản.
- **Death:** khung hình tĩnh bàn thờ với chén số 4 vỡ, trước chữ Death.

### 1.5 Đồ vật / model / âm thanh còn thiếu
- Sổ đỏ + giấy báo nợ trong két: thêm `Mon(...)` kiểu `Xem`, đặt trong két, hiện sau `ket_mo`.
- Ba bộ quần áo Nhím (mỗi đêm một bộ), cốc nước + vỉ thuốc ở phòng Nhím.
- Vòng khoanh ngày 9 trên texture tờ lịch.
- Tay Khôi (góc nhìn thứ nhất), clip bác nắm tay.
- Âm thanh: chuông điện thoại, tiếng rè, tiếng tút, đồng hồ gõ đều là âm tổng hợp tạm (`Resources/LOC_V33/*.wav`). Thay bằng file thật cùng tên là xong, không cần sửa code.

---

## 2. Làm dở / chưa hoàn thiện

| # | Vấn đề | Hướng sửa |
|---|---|---|
| 1 | Nhím nằm **quay lưng ra cửa**, đầu về tường sau (kịch bản: đầu về tường phải, nhìn ra cửa) | Xoay giường + điểm đặt Nhím trong `Editor/LocHouseBuilder_Nhim.cs`. Kiểm lại camera các cảnh trong phòng |
| 2 | Lần mất điện đầu: ma cách ~4,5 m (kịch bản 5–6 m) | `LocMa.DatMaTruocMat`: thử thêm hướng xiên ±45° trước khi lùi khoảng cách |
| 3 | Tầng 2 rất ít chỗ lấp ló, nhất là hành lang cạnh phòng Nhím | Thêm điểm mép cửa / lan can ở T2 (mép cửa phòng Khôi đang bị tủ áo che, xem TL-12) |
| 4 | Lớp phim cũ mức "Đầy đủ" làm cả game xám đục | Giảm độ mờ / tăng tương phản trong `Materials/LOC_HinhCu.asset` |
| 5 | Hai linh cữu trong phòng khách, v3.3 ghi một 🗣 | Chờ chốt TL-31 |
| 6 | Thoại bác dài ~70 s, người chơi đứng yên | Cắt bớt, hoặc cho đi lại trong lúc nghe (bỏ `khoa` trong đoạn thoại) 🗣 |
| 7 | Coroutine thăm Nhím không dừng khi nhảy đêm bằng phím thử → thoại chạy chồng | Lưu `Coroutine` trong `LocNhim.Tham` và `StopCoroutine` khi `LocMa.dem` đổi |
| 8 | Bài tự kiểm Nhím cũ còn chờ bước vẽ bột (đã bỏ) | Sửa `LocNhim.TuKiem()` theo v3.3 / v3.4 |
| 9 | Good / Bad / Evil kết bằng màn tạm | Xem 1.4 |
| 10 | Mini game: mọi mini game còn nút **Bỏ qua** (phím B) để dev cho nhanh | Trước khi nộp: đặt `LocMiniGame.choBoQua = false` |

---

## 3. Chưa test kỹ (cần người chơi tay)

Công cụ chơi thử dịch chuyển người chơi giữa các mốc, nên những gì cần **đi bộ thật** thì chưa kiểm được.

- [ ] **Thanh túi đồ mới (27/10)**: 6 ô góc phải dưới, phím `1–6` / lăn chuột chọn, `Tab` xem món đang chọn, `←→` lật trang. Kiểm: thanh ẩn ở menu / mini game / nhập mã két / xe buýt; mini game tự cầm món thì ô nhảy theo; có trên 6 món thì thanh cuộn; phím `1–6` không đụng phím thử `Shift+số`.
- [ ] Cuộc gọi thứ nhất Đêm 1: chỉ bật khi đi bộ qua hộp thang hầm ("Thôi. Mai tính.").
- [ ] Lấp ló khi đi bộ thật ở cầu thang và phòng khách. Phản ứng của Khôi nhạt dần qua lần 1 → 2 → 3, từ lần 4 thì thôi.
- [ ] Gợi ý H: bấm nhiều lần thì lùi bậc, không bao giờ nói đáp án.
- [ ] Va chạm khi đi (kẹt góc, xuyên tường, cầu thang), đoạn bê mâm xuống hầm.
- [ ] Chết → "Chơi lại" ở cả hai điểm lưu Đêm 3. "Tiếp tục" từ menu nạp đúng điểm lưu.
- [ ] Thời lượng một lượt chơi thật (mục tiêu 15–30 phút) và độ khó tìm đồ khi không có gợi ý.
- [ ] Bản build `.exe` (mới test trong Editor).

---

## 4. Thứ tự nên làm
1. **Chỗ rẽ Niêm hũ / Xé niêm** (1.3): không có thì không ai tự chơi tới Good / Evil.
2. **Cuộc gọi thầy Đồng + tờ giấy chép lời** (1.1): xương sống Đêm 2, phản diện chính.
3. Kiện hàng / văn khấn → vòng đọc văn khấn.
4. Phần thân các kết, mỗi kết một lần.
5. Đồ vật, model, âm thanh thật; sửa các mục ở phần 2.
6. Chơi tay toàn bộ mục 3 trước khi nộp.
