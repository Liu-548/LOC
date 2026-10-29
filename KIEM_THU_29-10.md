# LỘC — Danh sách kiểm thử tay (29/10/2026)

Những gì **chưa ai chơi tay** sau đợt 28–29/10: đồ vật mới, quần áo Nhím, tay Khôi, tay thầy, bác nắm tay, cùng các mục cũ còn treo. Công cụ chơi thử tự động đã chạy qua 3 đêm + 4 kết không lỗi, nhưng nó dịch chuyển và gọi thẳng cảnh, nên **mọi thứ cần đi bộ, bấm E thật, hay đi đúng đường rẽ** đều phải kiểm bằng tay.

Mỗi dòng có ô `[ ]` — đánh `[x]` khi đạt, `[!]` khi lỗi và ghi chú ngay dưới (ảnh: `LOC_KiemTra/`).

---

## Chuẩn bị

- Mở scene `LOC_NhaLoc` (dựng lại 29/10, 12:44 — có đèn tường mới). Bấm chuột vào **cửa sổ Game** trước khi dùng phím thử (không thì Unity nuốt phím F2).
- **Vào đêm bất kỳ:** Menu → *Chọn đêm* (đang mở sẵn để thử). Hoặc đang chơi bấm `F1` / `F2` / `F3`.
- **Phím thử:** `9` hết đêm ngay · `B` bỏ qua mini game · `F8` cảnh nửa tỉnh · `F9` cảnh Good · `F10` cảnh Bad · `Shift+1…5` các kiểu dọa · `H` gợi ý · `Tab` túi đồ.
- **Vị trí hay dùng:** phòng bố mẹ + phòng làm việc ở tầng 2 · phòng Nhím tầng 2 · phòng thờ ông bà tầng 3 · quầy cửa hàng vật liệu ở căn bên trái tầng 1 · hầm: từ bếp ra giếng trời, cửa sắt trên tường trái (mở bằng chìa trong két, mã két `1980`).
- Lưu ý: phím `F1–F3` là **nhảy đêm**, không đi qua các bước trước, nên vài mục dưới phải **chơi thông từ đầu đêm** — đã ghi rõ ở cột *Đường tới*.

---

## A. Đồ vật mới (29/10)

| # | Tình huống | Đường tới | Mong đợi | KQ |
|---|---|---|---|---|
| A1 | Sổ đỏ trong két | Đêm 1 → tầng 2, phòng bố mẹ, két góc cửa sổ → [E] nhập `1980` → nhìn ngăn trên | Sổ đỏ bìa đỏ nằm trên xấp giấy, mép giấy báo nợ ló ra bên phải. Không đè chùm chìa/khoá treo | [ ] |
| A2 | Tương tác sổ đỏ | Tiếp A1, nhìn sổ đỏ → [E] | Hiện "[E] sổ đỏ"; độc thoại *"Sổ đỏ. Kẹp giấy báo nợ." / "Nhà đang cắm ngân hàng."*. Sổ **không** vào túi | [ ] |
| A3 | Lấy chìa + sổ bố vẫn được | Tiếp A2, [E] chùm chìa, [E] sổ của bố | Hộp va chạm sổ đỏ không che chùm chìa/sổ bố; cả hai vào túi bình thường | [ ] |
| A4 | Cốc nước + vỉ thuốc | Đêm 1 → phòng Nhím → nhìn tủ đầu giường | Cốc thuỷ tinh có nước (sau ấm), vỉ thuốc + nửa viên phía trước. Không còn cốc trắng đục cũ. Đèn ngủ không đè ấm | [ ] |
| A5 | Chén hầm lúc đầu | Đêm 1 → xuống hầm lần đầu → nhìn bàn thờ | Chén 1–2 vỡ, chỗ 3 trống, chén 4 nứt, chén 5 mẻ. Khớp câu *"Chén thì vỡ gần hết."* | [ ] |
| A6 | Lọ keo | Đêm 2 → cửa hàng (căn trái tầng 1) → ngăn kéo quầy | Nhìn trúng lọ keo hiện [E], nhặt vào túi. **Không** còn lọ keo khối thứ hai trên mặt quầy | [ ] |
| A7 | Chén số ba | Đêm 2 → tầng 3, bàn thờ ông bà → chén nhỏ giữa nến trái và bát hương | Chén đứng trên khăn phủ, không lún, không chạm nến. Nhặt được | [ ] |
| A8 | Dán chén → chén hầm đổi | Đêm 2 **chơi thông**: có lọ keo + chén số ba → hầm → [E] Dán chén (thắng hoặc `B`) | Chén 1–2 lành (đường keo), chén số ba vào chỗ 3. Chén 4–5 giữ nguyên | [ ] |
| A9 | Trạng thái chén giữ qua đêm | Tiếp A8 → hết Đêm 2 (`9`) → Đêm 3 xuống hầm | Chén vẫn ở trạng thái đã dán | [ ] |
| A10 | Bát cháo dưới hầm | Đêm 2 → bếp, bàn ăn → nhặt bát cháo → hầm → [E] Đặt bát cháo | Bát cháo **thật** (không phải khối trụ) đặt bên phải bàn thờ, không đè đế chén | [ ] |
| A11 | Gói hàng | Đêm 3 → phòng khách, ngay trong cửa chính | Gói giấy dầu nằm dẹt trên sàn, có lạt. [E] Mở gói hàng → nhận bài khấn, hai tờ giấy gói, tờ niêm; gói biến mất | [ ] |
| A12 | Lời nhắn của mẹ | Đêm 3 → phòng bố mẹ → bàn trang điểm, nửa phía cửa sổ | Tờ lịch bóc, chữ bút bi xanh đọc được, nằm phẳng, không đè hộp đồ trang điểm. Đêm 1–2 **không** có | [ ] |
| A13 | Bad: chén 5 vỡ | Đêm 3 → để đồng hồ gõ đủ 12 tiếng (Bad a) **hoặc** `F10` | Khung bàn thờ hầm: chén 5 hiện mảnh vỡ đúng lúc tiếng vỡ | [ ] |
| A14 | Death: chén 4 vỡ | Đêm 3 sau hé hũ → đợi mất điện (lần thứ hai trở đi) → bước vào trong 2 m con ma | Khung tĩnh bàn thờ hầm có chén 4 vỡ, rồi chữ "NHÀ NÀY CÒN MỘT CÁI CHÉN." | [ ] |
| A15 | Chơi lại sau Death | Tiếp A14 → *Chơi lại* | Về điểm lưu, chén 4 **lành lại** (nứt), không còn mảnh vỡ | [ ] |

## B. Ba bộ quần áo Nhím

| # | Tình huống | Đường tới | Mong đợi | KQ |
|---|---|---|---|---|
| B1 | Đêm 1 | `F1` → phòng Nhím, lúc em nhổm dậy | Bộ hoa nhí trắng ngà như cũ | [ ] |
| B2 | Đêm 2 | `F2` → phòng Nhím, nhìn vai/cánh tay trên chăn | Áo ba lỗ sọc xanh nhạt. Vai và bắp tay là **da**, không thủng lỗ, không lòi mép vải; tay trái bột nối liền da | [ ] |
| B3 | Đêm 2 trở mình / giật mình | Tiếp B2, đứng chờ em trở mình, hoặc `Shift+số` cho dọa | Da vai không tách khỏi thân khi em cử động | [ ] |
| B4 | Đêm 3 | `F3` → phòng Nhím | Áo hồng in con thỏ trước ngực, quần lửng hồng | [ ] |
| B5 | Good | `F9` (hoặc đi đường Good, mục D5) | Nhím ngồi mép giường mặc bộ Đêm 3, hình thỏ dính theo ngực không trôi | [ ] |
| B6 | Bad | `F10` | Nhím đứng góc tường mặc bộ Đêm 3 | [ ] |
| B7 | Đổi đêm nhanh | `F1` → `F2` → `F3` → `F1` liên tiếp, mỗi lần xem phòng Nhím | Chỉ đúng một bộ hiện, không chồng hai bộ | [ ] |

## C. Tay Khôi

| # | Tình huống | Đường tới | Mong đợi | KQ |
|---|---|---|---|---|
| C1 | Đi lại không thấy tay | Bất kỳ đêm, đi quanh nhà | Không bao giờ thấy tay khi không bấm gì | [ ] |
| C2 | Bấm E cửa | Đứng trước một cửa → [E] | Tay phải với tới chỗ bấm (~0,7 s) rồi rút ra mép dưới; cửa vẫn mở/đóng như cũ | [ ] |
| C3 | Bấm E công tắc / nhặt đồ | [E] một công tắc · [E] một món nhặt được | Như C2. Với món nhặt: tay vẫn với ra dù món biến mất ngay | [ ] |
| C4 | Bấm E liên tục | Đứng trước công tắc, bấm E dồn dập | Tay không chồng, không giật; công tắc vẫn đổi đủ số lần | [ ] |
| C5 | Không hiện khi mở UI | [E] két (nhập mã) · [E] một điểm mini game · Tab đọc sổ | Không có tay | [ ] |
| C6 | Thắp hương Đêm 1 | Đầu Đêm 1 → phòng khách, đứng chính diện bàn thờ vong → [E] Thắp hương | Hai tay chắp kẹp 3 nén hương (đầu than hồng), vái 3 lần, rút ra trước khi trả điều khiển | [ ] |
| C7 | Giữ hương linh cữu | Cuối Đêm 1 hoặc Đêm 2 → [E] giữ hương | Chắp tay vái ngắn (~2 s) | [ ] |
| C8 | Thắp hương ông bà T3 | Đêm 3 sau hé hũ → tầng 3, chính diện bàn thờ → [E] | Như C7 | [ ] |
| C9 | Thay khăn | Đêm 1 hoặc 2 → phòng Nhím → chậu nước [E] Thay khăn | Hai tay đưa tới trán em; khăn khô đổi thành khăn ướt lúc hai bàn tay che trán, **không** còn chớp đen ở giữa | [ ] |
| C10 | Hé hũ | Đêm 3 → hầm → [E] cái hũ | Hai tay ôm thân hũ lúc kéo ra; một tay nhón góc niêm lúc xé | [ ] |
| C11 | Niêm hũ | Đi tới chỗ rẽ (mục D) → [E] Niêm hũ | Hai lòng bàn tay miết tờ niêm lúc phụ đề *"Miết cho phẳng."* | [ ] |
| C12 | Xé niêm | Đi tới chỗ rẽ → [F] Xé giấy niêm | Một tay nhón góc niêm khi tiếng xé | [ ] |
| C13 | Rót trà ban ngày | Hết Đêm 1 (`9` hoặc chơi thông) → cảnh ban ngày ở bàn nước | Tay phải cầm ấm, nghiêng rót trên khay, có tiếng rót. Ấm không trôi khỏi tay | [ ] |
| C14 | Tay trong bóng tối | Đêm 3 mất điện → bấm E cửa | Tay tối như mọi thứ, không tự sáng | [ ] |
| C15 | Tay xuyên tường | Đứng sát tường/tủ → [E] cửa tủ, công tắc sát góc | Ghi lại chỗ nào tay đâm xuyên rõ (có thể chấp nhận, chỉ ghi) | [ ] |

## D. Đường rẽ cuối và các kết (đi thật, không dùng F9/F10)

Đường chung tới chỗ rẽ — **Đêm 3 chơi thông**: mở gói hàng (cửa chính) → xuống hầm hé hũ → (tuỳ đường) thắp hương ông bà T3 rồi lấy + đọc sách gia lễ trong tủ thờ → bày mâm dưới hầm (`B`) → Tab mở **bài khấn**, lật tới trang cuối **khi đang đứng trong hầm** → giữ nhang (`B`) → nhìn hũ: hiện [E] Niêm hũ / [F] Xé giấy niêm.

| # | Tình huống | Đường tới | Mong đợi | KQ |
|---|---|---|---|---|
| D1 | Vòng đọc khấn sai chỗ | Mở bài khấn ở ngoài hầm / trước khi bày mâm, lật tới trang cuối | Phụ đề nhắc ("Phải đọc trước bàn thờ dưới hầm." / "Bày mâm xong đã rồi mới khấn."), giữ nhang chưa mở | [ ] |
| D2 | Bad (b) | Tới giữ nhang, **không** bấm B, để nhang tắt | "...Tắt rồi." → cảnh Bad → chén 5 vỡ → "Bốn năm sau." | [ ] |
| D3 | **Bad (c) — tay thầy** | Đường chung, **không đọc sách** → [E] Niêm hũ | Cổng sáng mùng một → trong sân cạnh cửa ngách: một bàn tay (nhẫn, móng út dài) thò vào **từ sau trụ cổng phải**, ngửa ra; hai tay Khôi đưa cái hũ tới; tay thầy khép lại, rút đi cùng hũ → cảnh Bad | [ ] |
| D4 | D3 — chỗ dễ lỗi | Như D3 | Không thấy đầu cụt ống tay áo thầy; cửa ngách mở; hũ không rơi/khựng; tay Khôi không với hụt quá xa hũ | [ ] |
| D5 | Good | Đường chung (đọc sách hay không đều được) → [F] Xé giấy niêm | Tay nhón xé → bốn lọn tóc → bùa rơi → "Cảm ơn" → sáng hôm sau → chữ cuối, điện thoại reo | [ ] |
| D6 | Evil | Đường chung **có đọc sách** → [E] Niêm hũ | Cổng sáng (không có tay thầy) → chân cầu thang, tiếng gọi → cuộc gọi "Cháu khá hơn bố cháu." → "Năm năm sau." | [ ] |
| D7 | Niêm khi mất tờ niêm | Không mở gói hàng mà vẫn tới được hũ (nếu được) → [E] Niêm hũ | "Tờ niêm của ông ấy đâu rồi." — không vào kết | [ ] |
| D8 | Ending ghi nhận | Sau mỗi kết → menu → *Ending* | Kết vừa đạt hiện ở mục Ending | [ ] |

## E. Bác nắm tay (Đêm 1)

| # | Tình huống | Đường tới | Mong đợi | KQ |
|---|---|---|---|---|
| E1 | Chuẩn | Vào Đêm 1 từ đầu → thắp hương chính diện → chờ | Camera quay về bác; bác bước tới (~0,6 m), tay trái Khôi đưa ra trước; tay phải bác nắm giữa cẳng tay Khôi qua 2 câu chào, rồi buông; Khôi đi lại được từ câu thứ ba | [ ] |
| E2 | Bác đứng chỗ khác / người chơi lệch | Thắp hương ở mép trái/phải vùng "chính diện" | Bác vẫn tới được, không đi xuyên linh cữu/bàn thờ, không kẹt quá 4 s | [ ] |
| E3 | Tay bác | Nhìn kỹ lúc nắm | Bàn tay bác đặt lên cẳng tay (không xuyên vào trong, không lơ lửng xa); khuỷu tay chĩa xuống tự nhiên | [ ] |
| E4 | Bác đi về | Sau thoại | Bác vẫn đi ra cổng, khép cổng, biến mất như trước | [ ] |
| E5 | Chơi lại đầu Đêm 1 | Đi tiếp rồi chết/chơi lại về đầu Đêm 1 (nếu điểm lưu có) | Không lặp nắm tay khi đã qua đoạn (`ho_hang_ve`) | [ ] |

## F. Gameplay 28/10 chưa ai chơi tay

| # | Tình huống | Đường tới | Mong đợi | KQ |
|---|---|---|---|---|
| F1 | Cuộc gọi thầy Đồng | Đêm 2: thăm Nhím → đứng gần điện thoại phòng khách tới khi có cuộc gọi không lời, nhấc máy → ở trong 16 m quanh điện thoại ~35 s | Reo lại, [E] Nhấc máy → thoại thầy Đồng → tờ giấy chép lời vào túi. Không nhấc thì ~55 s sau reo lại | [ ] |
| F2 | Đêm 2 chỉ hết khi đủ việc | Đêm 2, bỏ qua dán chén hoặc bát cháo → thử giữ hương | Chưa hết đêm tới khi đủ: nghe thầy, dán chén, đặt cháo | [ ] |
| F3 | Cảnh nửa tỉnh cuối Đêm 2 | Hết Đêm 2 bằng giữ hương | Có cảnh nửa tỉnh trước cảnh ban ngày | [ ] |
| F4 | Cắt cảnh đưa tang | Hết Đêm 2 | Hai khung tĩnh + kèn + "Sáng 8/8. Đưa tang." rồi sân vắng | [ ] |
| F5 | Gợi ý H các bước mới | Kẹt ở từng bước (cuộc gọi, dán chén, cháo, gói hàng, bài khấn, hũ) → `H` nhiều lần | Lùi bậc dần, không nói thẳng đáp án | [ ] |

## G. Mục cũ còn treo (27/10, chép lại cho đủ một chỗ)

| # | Tình huống | Đường tới | Mong đợi | KQ |
|---|---|---|---|---|
| G1 | Thanh túi 6 ô | Nhặt nhiều món, `1–6` / lăn chuột, `Tab`, `←→` | Thanh ẩn ở menu, mini game, nhập mã két, xe buýt; trên 6 món thì cuộn | [ ] |
| G2 | Cuộc gọi đầu Đêm 1 | Đi bộ ra khỏi hầm qua hộp thang | Chuông reo, lại gần thì tắt, "Giờ này còn ai gọi." | [ ] |
| G3 | Lấp ló khi đi bộ | Đêm 2–3 sau hoảng loạn, đi cầu thang + phòng khách | Phản ứng Khôi nhạt dần lần 1→3, từ lần 4 thôi | [ ] |
| G4 | 15 lỗi sửa ngày 27/10 | Chấm giữa màn hình · Space nhảy · tự cúi khi xuống hầm/lên gác · công tắc bếp/hành lang T2 có đổi đèn · bật tắt đèn làm ma rụt · cửa chính không quét xuyên cột rạp · cáo phó cạnh trụ trái · linh vị giấy · đèn hầm khớp bóng | Như mô tả trong `GHI_CHU_VAN_DE_27-10.md` mục 5 | [ ] |
| G5 | Va chạm | Đi khắp nhà, nhất là cầu thang, cửa hầm, đoạn bê mâm | Không kẹt, không xuyên tường | [ ] |
| G6 | Thời lượng một lượt | Chơi thật từ đầu tới một kết, không phím thử | Ghi lại số phút (mục tiêu 15–30) và chỗ nào bí lâu | [ ] |
| G7 | Bản build | Build `.exe`, chơi tới hết Đêm 1 | Không lỗi biên dịch, tay + quần áo + đồ mới vẫn hiện | [ ] |

## H. Sửa "Lỗi và chỗ lệch kịch bản" (29/10 chiều)

Từng mục của bảng *2. Làm dở / chưa hoàn thiện* trong `VIEC_CON_LAI_GAMEPLAY.md`: đã sửa gì, tự kiểm đã xác nhận gì, và **người chơi tay cần xem gì**.

| # | Mục | Đã làm | Tự kiểm |
|---|---|---|---|
| 1 | Nhím nằm quay lưng ra cửa | Kiểm lại bản dựng hiện tại: em đã nằm mặt quay vào phòng, đầu về tường phải (+X) — mục cũ đã lỗi thời, **không sửa code** | Ảnh `LOC/Thử Nhím nằm: chụp` |
| 2 | Lần mất điện đầu: ma cách ~4,5 m | `LocMa.DatMaTruocMat` quét có thứ tự r = 6 → 5,5 → 5 m, góc 0, ±10 … ±60° trước khi rơi về dò ngẫu nhiên. `DatMaTrongToi` không tìm được chỗ thì dò lại vùng rộng 2,5–7 m (110 lần) | Tự kiểm con ma 38/38 không lấn tường; mục 4 "mất điện ngẫu nhiên": 24 lần đặt được / 8 lần không có chỗ |
| 3 | T2 ít chỗ lấp ló | Builder nhận thêm mép cửa ở **hành lang hẹp** (khe trống nhỏ hơn, đánh dấu `[hành lang hẹp]` trong `BaoCao_Ma.txt`), `LocMa` dùng ngưỡng nới cho các điểm này | Có thêm 1 điểm: mặt trong cửa phòng Nhím — nhưng tự kiểm **bỏ** điểm này vì thân ma đè công tắc + bàn học. **T2 vẫn chỉ 2 điểm dùng được** (cửa bố mẹ, cửa Nhím phía hành lang). Muốn thêm phải dời công tắc/bàn học, hoặc làm lan can đầu thang T2 |
| 4 | Lớp phim "Đầy đủ" xám đục | `LocHinhAnh.ApB` (đã áp vào `LOC_HinhCu.asset`): bão hoà −4, tương phản 24, hạt 0,14, viền tối 0,24, bloom 1,0/0,25. `LocMenu`: nhiễu VHS = phim × 0,35 | So ảnh `LOC_KiemTra/Tay/p_truoc_*.png` ↔ `p_sau_*.png` |
| 5 | Hai linh cữu (kịch bản ghi một) | **Chưa làm** — chờ chốt TL-31 🗣 | |
| 6 | Thoại bác dài ~70 s | Giữ **Space 0,8 s** để bỏ qua cả đoạn bác (như xe buýt). Bỏ qua xong: thả tay, mở khoá, bác đứng yên | |
| 7 | Thoại thăm Nhím chạy chồng khi nhảy đêm | Đã sửa từ 27/10 | |
| 8 | Bài tự kiểm Nhím treo | Bỏ bước vẽ bột; thêm: bài kiểm tháo `SauGood/SauBad/SauEvil` — trước đây phần kết của cốt truyện gọi `VeMenu` → nạp lại scene giữa bài kiểm (chết ở ảnh `128_bad`) | Chạy 29/10 12:48 **trước** bản sửa thứ hai: qua 3 đêm, hoảng loạn, nửa tỉnh, Good, Bad rồi treo. Bản sửa chưa chạy lại (Unity đang kẹt ở Play) |
| 9 | Đèn sáng mà không có bóng (chiếu nghỉ T1, T2, giếng trời, sân phơi) | `LocHouseBuilder_V35.DenGanTuong`: chiếu nghỉ và giếng trời có máng + tuýp 0,6 m gắn tường (ống phát sáng, công tắc tắt theo), cao 2,1 m trên mặt sàn ngay dưới. Chiếu nghỉ gắn ở tường cuối (+Z), không gắn lên đầu vách giữa hai vế thang. Sân phơi: nguồn sáng dời ngay dưới chao tôn, đổi sang đèn rọi 170° chúc xuống, không lọt qua tường | Ảnh `LOC_KiemTra/Tay/den_*.png` |
| 10 | Mini game còn nút Bỏ qua (B) | **Giữ nguyên** cho giai đoạn dev. Trước khi nộp: `LocMiniGame.choBoQua = false` | |

**Kiểm tay:**

| # | Tình huống | Đường tới | Mong đợi | KQ |
|---|---|---|---|---|
| H1 | Nhím nằm đúng hướng | Đêm 1 / 2 / 3 → lên phòng Nhím, đứng ở cửa | Mặt em quay vào phòng / ra phía cửa, đầu sát tường phải, không quay lưng | [ ] |
| H2 | Ma lần mất điện đầu | Đêm 1 chơi thông tới lần mất điện đầu, đứng giữa phòng khách nhìn thẳng | Ma hiện trước mặt **cách ~5–6 m**, lệch tối đa ~60° | [ ] |
| H3 | Mất điện trong phòng hẹp | Đứng trong WC / kho / cầu thang lúc mất điện (nhảy đêm 2–3 rồi chờ) | Ma vẫn hiện (gần hơn, 2,5–7 m), không đứng trong tường; nếu không có chỗ thì cảnh bỏ qua chứ không kẹt | [ ] |
| H4 | Lấp ló tầng 2 | Đêm 2–3, đi hành lang T2 từ đầu thang tới phòng Khôi vài lần | Ma ló ở cửa phòng bố mẹ / cửa phòng Nhím; không ló ra từ chỗ đè công tắc hay bàn học | [ ] |
| H5 | Lớp phim | Menu → Cài đặt → Hình ảnh → lớp phim **Đầy đủ**, đi phòng khách + hành lang T2 | Vẫn có hạt, viền tối, chữ, nhưng nhìn rõ đồ vật; không xám đục; nhiễu VHS nhẹ | [ ] |
| H6 | Bỏ qua đoạn bác | Đêm 1 từ đầu → thắp hương → khi bác bắt đầu nói, **giữ Space** | Dòng "Giữ Space để bỏ qua" hiện góc màn hình; giữ 0,8 s → bỏ hết đoạn; tay Khôi thả ra, đi lại được ngay, camera không khoá; cốt truyện đi tiếp bình thường (bác về, đêm chạy tiếp) | [ ] |
| H7 | Không giữ Space | Như H6, không bấm | Đoạn bác chạy đủ như E1–E4 | [ ] |
| H8 | Bỏ qua xe buýt | Chơi mới → đoạn ngồi xe buýt → giữ Space | Bỏ qua tới lúc xuống xe (đã có từ trước, kiểm lại cho chắc) | [ ] |
| H9 | Đèn chiếu nghỉ T1 / T2 | Đêm bất kỳ, bật công tắc hành lang (đầu thang T2), đi cầu thang T1→T2→T3 | Tuýp gắn tường cuối chiếu nghỉ, ngang trên đầu; không lơ lửng trên bậc; dây điện chạy xuống tường. Tắt công tắc → tuýp tối theo | [ ] |
| H10 | Đèn giếng trời | Ra giếng trời từ bếp | Tuýp trên tường trên cửa kho, sáng vùng giếng | [ ] |
| H11 | Đèn sân phơi | Lên sân phơi T3 | Sáng toả từ chao tôn trên dây phơi; phòng phía sau tường không có vệt sáng lạ trên trần | [ ] |
| H12 | Tự kiểm Nhím | Dừng Play → để Unity biên dịch → LOC → Tự kiểm Nhím (Play + chụp) | Chạy hết tới ảnh `clip_*`, ghi `LOC_KiemTra/Nhim/BaoCao_TuKiem.txt`, tự thoát Play (~5 phút) | [ ] |

Đã biết, chưa sửa: chữ trên băng tang vòng hoa bị ngược khi chạy (có từ trước).

