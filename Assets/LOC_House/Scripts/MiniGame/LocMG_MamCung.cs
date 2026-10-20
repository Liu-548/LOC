// [21/10] Đêm 3, nghi lễ vòng 1 — BÀY MÂM dưới hầm (kịch bản v3.2 C4.7). Đặt sáu món lễ cúng cô hồn vào sáu ô theo lời thầy dặn.
// shortcut: kịch bản muốn bảy món / chín ô (hai ô nhiễu) theo sơ đồ vẽ tay trên tờ giấy chép lời thầy — tờ giấy chưa dựng nên lời dặn hiện ngay bên cạnh
public class LocMG_MamCung : LocMG_XepVao
{
    protected override string TenMacDinh() { return "Bày mâm"; }
    protected override string GoiYMacDinh() { return "Bày theo lời thầy dặn. Trái, phải tính theo người đứng khấn."; }
    protected override string LuatMacDinh()
    {
        return "Tờ giấy chép lời thầy:\n• Bát nhang ở giữa, hàng sau.\n• Gạo bên trái bát nhang, muối bên phải.\n• Cháo loãng ngay trước bát nhang.\n• Mía bên trái cháo, khoai bên phải.";
    }
    protected override void CauHinh()
    {
        cacMon = new[] { "Bát nhang", "Bát gạo", "Bát muối", "Chén cháo loãng", "Mía chặt khúc", "Khoai luộc" };
        cacO = new[] { "Sau - trái", "Sau - giữa", "Sau - phải", "Trước - trái", "Trước - giữa", "Trước - phải" };
        toaDoO = new[]
        {
            new UnityEngine.Vector2(0.17f, 0.18f), new UnityEngine.Vector2(0.5f, 0.18f), new UnityEngine.Vector2(0.83f, 0.18f),
            new UnityEngine.Vector2(0.17f, 0.82f), new UnityEngine.Vector2(0.5f, 0.82f), new UnityEngine.Vector2(0.83f, 0.82f)
        };
        dapAn = new[] { 1, 0, 2, 4, 3, 5 };   // sau: gạo · nhang · muối — trước: mía · cháo · khoai
        cauXong = "Mâm bày xong. Thắp nhang.";
    }
}
