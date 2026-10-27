// [22/10] Tuỳ chọn, [v3.3] Đêm 1 sau khi mở két → trước đoạn hoảng loạn Đêm 2, phòng làm việc của bố. XẾP LẠI GIẤY TỜ vào tủ hồ sơ tám ngăn (Đêm 1 đã lục tủ tìm mã két).
// Lý do Khôi làm: hôm qua lục tung; người họ hàng dặn "xem có giấy tờ gì cần cất riêng không".
// Hé lộ: bìa hồ sơ "KHÔI" chữ bố — cuống phiếu chuyển tiền gửi Khôi mỗi tháng từ 9/2000, người gửi ghi tên mẹ nhưng là nét chữ bố
// (người chơi đã học phân biệt chữ mẹ nhỏ đều / chữ bố to nghiêng ở trang sổ thu chi Đêm 1). Nối với sổ của bố trang 22: "Mình nói nặng."
public class LocMG_XepHoSo : LocMG_PhanLoai
{
    protected override string TenMacDinh() { return "Tủ hồ sơ của bố"; }
    protected override string GoiYMacDinh() { return "Lúc tìm mã két lục tung tủ của bố. Bác dặn giấy tờ gì cần thì cất riêng."; }
    protected override void CauHinh()
    {
        cacNgan = new[] { "Ngăn 1 · Hàng hoá", "Ngăn 2 · Nhà cửa", "Ngăn 4 · Thu chi", "Ngăn 6 · Các con" };
        cacMon = new[] { "Hoá đơn nhập xi măng 2001", "Sổ hàng năm 2000", "Hợp đồng xây nhà 1997", "Biên lai thuế đất", "Sổ thu chi 1999", "Sổ thu chi 2001", "Giấy nợ tiền xi măng", "Học bạ lớp 1 của Nhím", "Bìa cứng ghi KHÔI" };
        dungNgan = new[] { 0, 0, 1, 1, 2, 2, 0, 3, 3 };
        ghiChu = new[] { "", "", "", "", "Chữ mẹ. Nhỏ, đều.", "Chữ mẹ. Nhỏ, đều.", "Nợ tiền xi măng. Hạn cuối tháng chín. Bán cả nhà cũng không đủ. ...Còn con Nhím.", "", "Chữ bố, to, nghiêng. Trong có một xấp giấy mỏng." };
        cauXong = "";
    }
}
