// Mini game 5 — BÀY MÂM LỄ lên bàn thờ vong. Đặt sáu món vào sáu ô theo lời dặn (luật "đông bình tây quả" của nhà).
public class LocMG_MamCung : LocMG_XepVao
{
    protected override string TenMacDinh() { return "Bày mâm lễ"; }
    protected override string GoiYMacDinh() { return "Nhìn từ chỗ người đứng vái. Trái, phải theo người đứng vái."; }
    protected override string LuatMacDinh()
    {
        return "Lời dặn:\n• Đông bình tây quả: bình hoa bên phải, đĩa quả bên trái, hàng sau.\n• Bát hương ở giữa hàng sau.\n• Chén nước ngay trước bát hương.\n• Bát cơm quả trứng bên phải chén nước.\n• Đèn dầu bên trái chén nước.";
    }
    protected override void CauHinh()
    {
        cacMon = new[] { "Bình hoa", "Đĩa quả", "Bát hương", "Chén nước", "Bát cơm quả trứng", "Đèn dầu" };
        cacO = new[] { "Sau - trái", "Sau - giữa", "Sau - phải", "Trước - trái", "Trước - giữa", "Trước - phải" };
        toaDoO = new[]
        {
            new UnityEngine.Vector2(0.17f, 0.18f), new UnityEngine.Vector2(0.5f, 0.18f), new UnityEngine.Vector2(0.83f, 0.18f),
            new UnityEngine.Vector2(0.17f, 0.82f), new UnityEngine.Vector2(0.5f, 0.82f), new UnityEngine.Vector2(0.83f, 0.82f)
        };
        dapAn = new[] { 1, 2, 0, 5, 3, 4 };   // sau-trái: đĩa quả · sau-giữa: bát hương · sau-phải: bình hoa · trước: đèn dầu / chén nước / bát cơm
        cauXong = "Mâm lễ đã bày đúng chỗ. Hương cháy đều.";
    }
}
