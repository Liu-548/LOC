// [22/10] Tuỳ chọn, Đêm 1 (sau khi cô về) – Đêm 3, bàn nước phòng khách. DỌN BÀN NƯỚC sau khi khách về.
// Hé lộ: cái cốc men nâu úp trên khay là cốc của bố (thiết kế nội thất GĐ5, Luật 3: "cốc chè của bố úp" — đồ buổi sáng 5/8 không ai động).
// Khôi không cất nó. Gieo nhẹ ý "mỗi người trong nhà một cái" trước khi thầy dặn "đủ năm chén, mỗi người một cái" ở Đêm 2 — không nhắc chén nước, không nhắc hầm.
public class LocMG_DonBanNuoc : LocMG_PhanLoai
{
    protected override string TenMacDinh() { return "Bàn nước"; }
    protected override string GoiYMacDinh() { return "Khách về hết rồi. Dọn tạm cái bàn."; }
    protected override void CauHinh()
    {
        cacNgan = new[] { "Bê xuống bếp rửa", "Sọt rác", "Hộp bánh kẹo", "Để nguyên chỗ cũ" };
        cacMon = new[] { "Mấy cái chén trà của khách", "Ấm trà nguội", "Đĩa vỏ hạt dưa", "Giấy kẹo vương vãi", "Bánh in, kẹo lạc còn thừa", "Gói hạt dưa chưa bóc", "Cốc sứ men nâu úp trên đĩa lót" };
        dungNgan = new[] { 0, 0, 1, 1, 2, 2, 3 };
        tuChoi = new[] { "", "", "", "", "", "", "Khoan. Cốc của bố." };
        ghiChu = new[] { "Chén nào cũng còn một lớp nước chè đặc.", "", "", "", "", "", "" };
        cauXong = "";
    }
}
