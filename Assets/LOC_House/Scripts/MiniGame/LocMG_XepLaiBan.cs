// Mini game 12 — XẾP LẠI BÀN HỌC của Khôi ("đóng băng hai năm"). Nhìn kỹ bố cục mấy giây rồi đồ biến mất, đặt về đúng chỗ cũ.
public class LocMG_XepLaiBan : LocMG_XepVao
{
    protected override string TenMacDinh() { return "Bàn học của Khôi"; }
    protected override string GoiYMacDinh() { return "Hai năm rồi mọi thứ vẫn nằm đúng chỗ. Nhớ lấy."; }
    protected override string LuatMacDinh() { return "Chọn một món bên dưới rồi bấm vào ô trên mặt bàn."; }
    protected override void CauHinh()
    {
        gioDapAn = 8f;
        cacMon = new[] { "Thời khoá biểu", "Ảnh mờ", "Mẩu giấy", "Góc đèn", "Cuốn nhật ký" };
        cacO = new[] { "Trên - trái", "Trên - giữa", "Trên - phải", "Dưới - trái", "Dưới - phải" };
        toaDoO = new[]
        {
            new UnityEngine.Vector2(0.17f, 0.2f), new UnityEngine.Vector2(0.5f, 0.2f), new UnityEngine.Vector2(0.83f, 0.2f),
            new UnityEngine.Vector2(0.3f, 0.8f), new UnityEngine.Vector2(0.7f, 0.8f)
        };
        dapAn = new[] { 0, 1, 3, 2, 4 };   // trên: TKB, ảnh mờ, góc đèn · dưới: mẩu giấy, nhật ký
        cauXong = "Mọi thứ về đúng chỗ. Có thứ gì đó rơi ra từ giữa các trang nhật ký.";
    }
}
