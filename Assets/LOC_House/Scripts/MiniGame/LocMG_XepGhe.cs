// Mini game 7 — XẾP CHỖ NGỒI quanh bàn ăn. Đặt bốn cái chén vào bốn ghế, đọc dấu vết từng ghế mà suy ra.
// Dấu vết lấy từ chính bốn ghế đã dựng trong bếp: ghế Khôi sạch nhất (hai năm không ai ngồi), ghế Nhím có gối kê.
public class LocMG_XepGhe : LocMG_XepVao
{
    protected override string TenMacDinh() { return "Chỗ ngồi quanh bàn ăn"; }
    protected override string GoiYMacDinh() { return "Mỗi người một ghế, từ trước đến nay chưa ai đổi chỗ."; }
    protected override string LuatMacDinh()
    {
        return "Nhớ lại:\n• Bố ngồi đầu bàn, mặt quay vào trong nhà.\n• Mẹ ngồi cạnh phía thang.\n• Khôi ngồi cái ghế hai năm nay chưa ai động tới.\n• Nhím còn nhỏ, phải kê gối.";
    }
    protected override void CauHinh()
    {
        cacMon = new[] { "Chén của bố", "Chén của mẹ", "Chén của Khôi", "Chén của Nhím" };
        cacO = new[] { "Đầu bàn, mặt vào trong", "Ghế phía thang", "Ghế sạch bóng", "Ghế có gối kê" };
        toaDoO = new[]
        {
            new UnityEngine.Vector2(0.5f, 0.12f), new UnityEngine.Vector2(0.14f, 0.5f), new UnityEngine.Vector2(0.86f, 0.5f), new UnityEngine.Vector2(0.5f, 0.88f)
        };
        dapAn = new[] { 0, 1, 2, 3 };
        cauXong = "Bốn cái chén đã về đúng chỗ.";
    }
}
