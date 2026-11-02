# LỘC — Rà soát 03/11/2026 (nhánh `claude/ra-soat-03-11`)

Nhánh tách từ `claude/walkthrough-tiep`, đã gộp `thuc-te-giac-mo-dem2` (đồ hoạ "Thực tế" + tối ưu FPS + giấc mơ Đêm 2).
**Mọi nhánh trên GitHub đều đã nằm trong nhánh này.** GitHub không có issue hay PR nào đang mở.

> Code chỉ được kiểm biên dịch (cả bộ Editor lẫn bản build), **chưa chạy trong Unity**.
> Mở Unity → **LOC → Tự kiểm gameplay** có thêm phần "— sửa 03/11" để kiểm các mục dưới đây (kết quả ở `LOC_KiemTra/GamePlay/BaoCao.txt`).

## 1. Đèn / công tắc — "tắt rồi mà vẫn sáng" và ngược lại

| # | Lỗi | Nguyên nhân | Sửa |
|---|---|---|---|
| 1 | Tắt công tắc: nguồn sáng tắt nhưng **ống tuýp, bóng compact, bóng đèn chùm vẫn sáng rực** | Cả 21 bộ đèn trong nhà giữ vật liệu của file .glb ở ô phát sáng. glTFast phát sáng qua `emissiveFactor`, còn `LocCongTac` chỉ xét `_EmissionColor` | `LocCongTac.BongCua` xét cả hai (`LocDien.PhatSang`, `LocDien.BanTat`) |
| 2 | Sau **Chơi lại / Về menu**, tắt công tắc thì bóng vẫn sáng | `LocCongTac` giữ danh sách renderer trong biến tĩnh, chỉ xoá khi Unity nạp lại domain → trỏ vào vật của cảnh cũ đã huỷ | Sang cảnh mới thì làm lại bảng |
| 3 | Đèn ngủ Nhím nháy / tắt trong cảnh (Bad, hé hũ) mà **bóng vẫn sáng đều** | `LocNhim` bật tắt thẳng `Light.enabled`, bóng chỉ đổi khi bấm công tắc | `LocBongTheoDen`: bóng đi theo đèn, dù ai bật tắt |
| 4 | **Mất điện dập luôn lửa đèn dầu, nến sáp, đầu nhang** trên bàn thờ vong; có điện lại thì tự cháy lại | `LocMa` coi mọi vật liệu phát sáng là đồ điện | `LocDien` xác định cái gì ăn điện. `LocMa` bỏ qua lửa, nhang, bùa đang cháy và đèn dầu (`LocLuaNhapNhay`), và lấy thêm đồ điện trong các nhóm bật theo đêm |
| 5 | Công tắc phòng tắt cả nến điện bàn thờ ở gần | Gom mọi vật phát sáng trong 0,9 m quanh đèn | Nến điện cắm ổ riêng: mất điện thì tắt, công tắc phòng không điều khiển |
| 6 | **Đèn sảnh T2** (trước cửa phòng bố mẹ) không công tắc nào tắt được | Không có trong bảng `NOI` | Nối vào công tắc đầu thang T2 (`HanhLang_ChieuNghi`) |
| 7 | Tắt đèn rồi tắt "Hình ảnh thực tế" → **bóng sáng lại** dù đèn đang tắt | `LocThucTe` thay vật liệu rồi trả bản gốc, đè lên bản "tắt" của công tắc | `LocThucTe` không đụng vào vật liệu ăn điện / bản tắt |

Còn lại (không phải lỗi): góc làm việc T2, kho, giặt, tiệm, sân phơi không có công tắc nên đèn luôn sáng. Công tắc phòng bố mẹ có 2 phím nhưng chỉ nối đèn trần (không có đèn thứ hai để nối).

## 2. Cửa / tủ

- **Tủ đứng T2** (2 cánh dính thành 1): đã sửa ở `claude/walkthrough-tiep`.
- **Chạn bát**: 4 cánh cùng tên "cửa chạn" → `LocDoor` ghép cả 4, bấm cánh trên thì cánh dưới cũng bật ra. Nay hàng dưới đổi tên "cửa chạn dưới" lúc chạy (`LocSuaNha.TachHangCanh`). Bảng `LocTuMo` cũng đã sửa sẵn cho lần dựng nhà sau.
- Đã đối chiếu mọi tủ 2 cánh trong `LocTuMo.Bang` với hình học .glb: chỗ chia cánh đều trùng khe thật. Tam giác vắt qua hai cánh chỉ là đợt hoặc vách trong tủ (đúng là thuộc thân tủ). `CuaSatKinh` đo được 1 cánh 0,9 m, đúng như đang khai.
- Cửa đôi trong nhà (cổng, cửa chính, ban công, cửa sổ, góc làm việc, tủ áo Khôi) ghép đúng cặp. Hai cửa sổ cùng phòng (bố mẹ, phòng thờ) cách nhau trên 3,3 m nên không bị ghép lẫn.

## 3. Asset

- Cảnh nhà: không có script bị mất. Guid không nằm trong Assets chỉ là của gói URP (Volume, CameraData, shader Lit / SimpleLit / Unlit). Không vật liệu nào dùng shader ngoài URP, không thiếu texture.
- Thêm `.meta` (guid cố định) cho 4 script nhánh đồ hoạ đẩy lên mà thiếu meta.
- Xoá `Assets/LOC_Backup.meta` mồ côi.
- **Bỏ theo dõi `Assets/_Recovery/`**: hai bản sao cảnh nhà, mỗi file khoảng 45 MB, do Unity treo để lại. `.gitignore` đã ghi bỏ thư mục này nhưng hai file được thêm từ trước. Unity nạp chúng như cảnh thật, tốn bộ nhớ lúc mở dự án. Pull nhánh này thì hai file biến khỏi máy; cần lấy lại thì lấy từ lịch sử git.

## 4. Việc còn sót trong walkthrough

- ✅ Két: năm sinh người khác → "Không phải." (đã có từ trước, tài liệu ghi nhầm ❌).
- ✅ Chén hầm có nước; chén số 5 vơi nửa từ Đêm 3 (`LocNuocChen`).
- ✅ **Lỗi phát hiện thêm**: cảnh Evil "năm năm sau" chỉ thấy đế chén trống (bước dựng 01/11 đã bỏ năm chén liền) → dựng lại năm chén lành theo đúng số đo `ChenNguyen.glb`.
- 🗣 Cần toàn văn kịch bản v3.3 (không có trong repo / Project): giấy nợ tiền xi măng; hũ bọc vải trắng trong túi cạnh cửa (Good); hai tờ giấy gói + lời nhắn của mẹ đúng chữ; thoại thầy Đồng phần giữa; mini game đọc khấn theo vạch tàn nhang.
- 🗣 Chờ nhóm quyết (TL-29): lựa chọn ở cổng (Đưa hũ / Đặt hũ xuống).
- ❌ Việc lớn chưa làm: mini game nấu cháo; tìm gạo muối / mía có vật nhiễu; hoảng loạn chạy thật dọc hành lang.
