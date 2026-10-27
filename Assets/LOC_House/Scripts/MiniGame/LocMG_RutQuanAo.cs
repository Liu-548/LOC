// [22/10] Tuỳ chọn, Đêm 2–3, sân phơi tầng 3. RÚT QUẦN ÁO, gấp, chia theo người.
// Lý do Khôi làm: kiêng phơi đồ qua đêm, nhất là sát tháng cô hồn (tục dân gian có thật) — mẹ vẫn dặn thế. Nhắc hạn mùng một mà không giải thích gì.
// Hé lộ: áo của Nhím bị cắt tay trái cho vừa bột (em ngã 1/8, mẹ mất 5/8) — đường khâu nhỏ, đều, "như chữ mẹ" (nối với nét chữ mẹ trong sổ thu chi Đêm 1).
public class LocMG_RutQuanAo : LocMG_PhanLoai
{
    protected override string TenMacDinh() { return "Dây phơi"; }
    protected override string GoiYMacDinh() { return "Hồi bé mẹ hay dặn: tháng bảy không để quần áo qua đêm."; }
    protected override void CauHinh()
    {
        cacNgan = new[] { "Đồ tang — gấp, để lên sạp", "Đồ của Nhím", "Khăn — vắt lên móc" };
        cacMon = new[] { "Ba cái áo sô trắng", "Mấy dải khăn tang", "Áo cộc hoa nhỏ", "Quần đùi kẻ ô, cỡ bé", "Đôi tất trẻ con", "Khăn mặt", "Khăn lau tay" };
        dungNgan = new[] { 0, 0, 1, 1, 1, 2, 2 };
        ghiChu = new[] { "Vải sô thô, còn ẩm.", "", "Tay trái bị cắt dọc lên tới nách. Mép viền lại bằng chỉ trắng.", "", "", "", "" };
        cauXong = "";
    }
}
