# LỘC — Việc còn lại về gameplay (cập nhật 27/10/2026)

File này gom mọi phần gameplay **chưa làm**, **làm dở** và **chưa test kỹ**, kèm hướng dẫn cơ bản để làm tiếp. Danh sách lỗi sau lượt chơi thử nằm ở `GHI_CHU_VAN_DE_27-10.md`. Kịch bản gốc: `kich-ban-loc-v3.3.md` và `kich-ban-loc-v3.4-thay-doi.md` (trong Project Claude của nhóm).

> Quy ước nhóm: **mọi nội dung đều được sửa, nhưng phải bàn với cả nhóm trước.** Mục có dấu 🗣 là phần kịch bản chưa chốt, cần bàn trước khi làm.

---

## Cập nhật 28/10 — nhánh `claude/gameplay-con-lai` (trên `claude/sua-loi-test-1`)

Đã làm thành **cơ chế chơi được**, không cần dựng lại nhà: đồ mới dựng lúc vào scene bằng khối đơn giản, đặt theo vật có sẵn. Code ở `Scripts/LocCotTruyen_V34.cs` (phần mới của `LocCotTruyen`, lớp `partial`). **Mọi câu thoại / nội dung giấy dưới đây là bản tạm** 🗣, sửa thẳng trong file đó. Chỉ biên dịch thử ngoài Unity, **chưa chơi trong Unity**.

| Mục | Đã làm | Ghi chú |
|---|---|---|
| 1.1 Cuộc gọi thầy Đồng | Sau cuộc gọi không lời ~35 s, điện thoại reo lại, [E] Nhấc máy → thoại thầy Đồng (cờ `goi_thay`). Không nhấc thì ~55 s sau reo lại | Phải đứng trong 16 m quanh điện thoại |
| 1.1 Tờ giấy chép lời thầy | Vào túi sau cuộc gọi, đọc được (`giay_loi_thay`) | |
| 1.1 Cháo, lọ keo, chén số 3, dán chén | Lọ keo trên quầy cửa hàng (`lo_keo`), chén số ba (chén anh Phong) trên bàn thờ ông bà tầng 3 (`chen_3`), bát cháo trên bàn ăn (`bat_chao`). Dán chén giờ cần keo + chén số ba. Bưng bát cháo xuống bàn thờ hầm (`cung_chao`) | Đêm 2 chỉ "thắp nén mới" được khi đã có `goi_thay`, `dan_chen`, `cung_chao` (món nào không dựng được thì bỏ điều kiện đó) |
| 1.1 Cảnh nửa tỉnh cuối đêm | Nối vào `HetDem()` của Đêm 2 | |
| 1.2 Kiện hàng + văn khấn | Gói giấy dầu trong cửa chính (Đêm 3). Mở ra: bài khấn (`van_khan`), hai tờ giấy gói chữ son (`giay_goi`), tờ niêm (`giay_niem`) | |
| 1.2 Lời nhắn của mẹ | Trên bàn trang điểm phòng bố mẹ, Đêm 3 (`loi_nhan_me`, đọc tại chỗ) | Nội dung tạm: chỉ sang quyển sách trong tủ thờ |
| 1.2 Cắt cảnh đưa tang | `LocBanNgay.DuaTang`: hai khung tĩnh (linh cữu giữa nhà, cổng mở nhìn ra đường) + kèn đám + phụ đề "Sáng 8/8. Đưa tang." rồi mới tới sân vắng | |
| 1.3 Vòng đọc văn khấn | Sau bày mâm, đứng trong hầm, mở bài khấn trong túi và lật tới trang cuối → `nl_doc_van`. Giữ nhang chỉ mở sau đó | |
| 1.3 Chỗ rẽ cuối | Giữ nhang xong: hũ cốt hiện **[E] Niêm hũ / [F] Xé giấy niêm**. Xé → Good. Niêm (cần tờ niêm) + đã đọc sách → Evil. Niêm + chưa đọc → Bad (c) | |
| 1.3 Cảnh cổng sáng mùng một | Ra ngoài cổng, trời sáng, ba dòng độc thoại (Bad c / Evil) | |
| 1.4 Good | Xé niêm, bốn lọn tóc, bùa trên tường hầm rơi, "Cảm ơn" → cảnh sáng hôm sau (đã có) → chữ cuối, điện thoại reo không ai nhấc | |
| 1.4 Bad (a/b/c) | Bad (b): nhang tắt khi giữ nhang là hết, không châm lại. Mọi Bad → cảnh hầm, chén số năm vỡ → "Bốn năm sau." → chữ cuối | Chưa có model chén vỡ: chỉ tiếng vỡ + khung hình |
| 1.4 Evil | Cổng sáng → chân cầu thang, tiếng gọi "Anh Khôi ơi…" (đã có) → điện thoại: "Cháu khá hơn bố cháu." → "Năm năm sau." → chữ cuối. Ẩn Nhím khỏi phòng | |
| 1.4 Death | Khung hình tĩnh bàn thờ hầm (năm chỗ chén) trước chữ Death | Chưa có model chén số 4 vỡ |
| 2.7 | Đổi đêm giữa lúc thăm Nhím: dừng thoại cũ | |
| Gợi ý H | Thêm bậc gợi ý cho cuộc gọi, dán chén, bát cháo, gói hàng, bài khấn, chỗ rẽ | |

**Chưa làm:** âm thanh thật (mục 1.5), mục 2.1–2.6, 2.8, 2.10. *(Model/đồ vật, tay Khôi, bác nắm tay: xong 29/10 — xem mục trên.)*

---

## Cập nhật 29/10 — model và đồ vật (mục 1.5), tay Khôi, bác nắm tay

Đã dựng lại nhà và chạy **LOC → Chơi thử một mạch**: qua hết 3 đêm + 4 kết, Console 0 lỗi mới. Đồ mới đều chụp kiểm tại chỗ (không lơ lửng, không đè đồ khác).

| Mục | Đã làm | File |
|---|---|---|
| Sổ đỏ + giấy báo nợ | Model thật trên xấp giấy tờ ngăn trên két, [E] "Sổ đỏ. Kẹp giấy báo nợ. / Nhà đang cắm ngân hàng." | `Models/V35/SoDo_BaoNo.glb`, `LocHouseBuilder_V3`, `_GamePlay` |
| Cốc nước + vỉ thuốc | Cốc thuỷ tinh có nước, vỉ paracetamol bóc 4 viên + nửa viên trên tủ đầu giường Nhím (bỏ cốc/vỉ thô gộp trong lưới tủ). Đèn ngủ dời trái vì chao đè lên ấm | `Models/V35/`, `LocHouseBuilder_V35` |
| Chén hầm | 5 chỗ đúng kịch bản (1–2 vỡ · 3 trống · 4 nứt · 5 mẻ). Dán chén xong: 1–2 lành + chén số ba vào chỗ 3. Bad: vỡ cái 5. Death: vỡ cái 4 | `LocHouseBuilder_V35`, `LocCotTruyen.ChenHam`, `LocMa` |
| Lọ keo, chén số ba, gói hàng, lời nhắn mẹ, bát cháo hầm | Thay khối tạm bằng model: lọ keo trong ngăn kéo quầy (có sẵn), chén dán keo trên bàn thờ ông bà, gói giấy dầu (bộ gd6), tờ lịch bóc chữ mẹ (Đêm 3), bản sao bát cháo thật | `LocHouseBuilder_V35`, `LocCotTruyen_V34` |
| Vòng khoanh ngày 9 | Texture lịch đã có sẵn, không phải làm | |
| Ba bộ quần áo Nhím | Đêm 1 hoa nhí (giữ nguyên) · Đêm 2 ba lỗ sọc xanh + quần kẻ ô (dựng lại da vai/bắp tay chỗ Body bị khoét) · Đêm 3 áo hồng in thỏ + quần lửng hồng. Kết giữ bộ Đêm 3. Bản FBX cũ ở `LOC_Backup/Nhim_b7_truoc_quanao_29-10.fbx` | `LOC_Nguon/Nhim/b8_quanao.py`, `tex_quanao.py`, `LocNhim.boDem1..3` |
| Tay Khôi (bản vẽ v1, áo xắn) | Chỉ hiện khi làm việc: bấm E → tay với tới chỗ bấm · thắp hương chắp tay kẹp 3 nén, vái 3 lần · thay khăn · kéo hũ, nhón góc niêm, miết niêm, xé niêm · rót trà cảnh ban ngày | `Models/Tay/`, `LocTay.cs`, `LocHouseBuilder_Tay` |
| Tay thầy Đồng (nhẫn + móng út) | Bad đường (c): sau cảnh cổng sáng, bàn tay thò qua cửa ngách từ sau trụ cổng, ngửa ra; Khôi đưa cái hũ; tay khép lại rút đi | `LocCotTruyen_V34.ThayNhanHu` |
| Bác nắm tay | Sau thắp hương: bác bước tới, tay phải nắm cẳng tay trái Khôi (IK hai khớp, không cần clip mới) qua hai câu chào rồi buông | `LocDi.BuocToi/NamTay`, `LocCotTruyen.ThapHuong` |

Công cụ mới: **LOC → Thử tay: chụp** (`LOC_ThuTay.txt` → `LOC_KiemTra/Tay/`) đặt tay đứng yên để chụp, không cần Play.

**Còn lại của 1.5:** âm thanh thật (chuông, rè, tút, đồng hồ). **Chưa chơi tay để xem:** tay khi bấm E thật, đường Bad (c) có tay thầy (công cụ chơi thử gọi thẳng cảnh kết nên không đi qua), bác nắm tay lúc đang chạy.

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
| 1 | ~~Nhím nằm quay lưng ra cửa~~ | **Xong (29/10):** bản dựng hiện tại đã đúng hướng, không cần sửa. Kiểm tay H1 |
| 2 | ~~Lần mất điện đầu: ma cách ~4,5 m~~ | **Xong (29/10):** `DatMaTruocMat` quét 6 → 5 m, ±60° trước; `DatMaTrongToi` có vòng dò rộng. Kiểm tay H2–H3 |
| 3 | Tầng 2 rất ít chỗ lấp ló | **Làm một phần (29/10):** builder nhận điểm hành lang hẹp, nhưng điểm mới (mặt trong cửa Nhím) bị tự kiểm loại vì đè công tắc + bàn học → vẫn 2 điểm. Còn: dời công tắc/bàn, hoặc thêm lan can đầu thang T2 |
| 4 | ~~Lớp phim "Đầy đủ" xám đục~~ | **Xong (29/10):** `LocHinhAnh.ApB` + VHS × 0,35. Kiểm tay H5 |
| 5 | Hai linh cữu trong phòng khách, v3.3 ghi một 🗣 | Chờ chốt TL-31 |
| 6 | ~~Thoại bác dài ~70 s~~ | **Xong (29/10):** giữ Space 0,8 s để bỏ qua (`LocCotTruyen.BoQuaDuoc`). Kiểm tay H6–H7 |
| 7 | ~~Coroutine thăm Nhím chạy chồng~~ | Đã sửa 27/10 |
| 8 | Bài tự kiểm Nhím | **Đã sửa (29/10):** bỏ bước vẽ bột + tháo các hàm kết khi tự kiểm. Chưa chạy lại bản cuối (H12) |
| 9 | Good / Bad / Evil kết bằng màn tạm | Xem 1.4 |
| 11 | ~~Đèn sáng không có bóng (chiếu nghỉ T1/T2, giếng trời, sân phơi)~~ | **Xong (29/10):** tuýp gắn tường + chao sân phơi (`LocHouseBuilder_V35.DenGanTuong`). Kiểm tay H9–H11 |
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
