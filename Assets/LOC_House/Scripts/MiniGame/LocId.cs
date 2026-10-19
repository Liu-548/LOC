// Mã (id) dùng chung cho đồ trong túi và cờ nhiệm vụ của hệ mini game — gom một chỗ để khỏi gõ sai chữ.
//
// Đồ "mảnh chén": mỗi đêm có một chiếc chén vỡ, các mảnh nằm sau các mini game; gom đủ rồi xuống hầm ráp lại (LocMG_GhepChen).
//   Đêm 1: ManhChen1–3 · Đêm 2: ManhChen4–7 · Đêm 3: ManhChen8–10
public static class LocId
{
    public const string ChiaLenDay = "ChiaLenDay";   // nhặt ở móc chìa khoá, dùng chỉnh đồng hồ quả lắc
    public const string ChiaTuTho = "ChiaTuTho";     // thưởng của đồng hồ, mở tủ đồ thờ
    public const string DayDan = "DayDan";           // sợi dây thay cho dây đàn guitar
    public const string BangThuAm = "BangThuAm";     // thưởng của guitar, cho radio cassette
    public const string ManhGuong = "ManhGuong";     // nhặt ở gương sảnh tầng 2, soi bảng đen của Nhím
    public const string ManhAnh1 = "ManhAnh1";
    public const string ManhAnh2 = "ManhAnh2";
    public const string ManhAnh3 = "ManhAnh3";
    public const string ManhAnh4 = "ManhAnh4";
    public static readonly string[] ManhAnh = { ManhAnh1, ManhAnh2, ManhAnh3, ManhAnh4 };

    public const int SoManhChen = 10;
    public static string ManhChen(int i) { return "ManhChen" + i; }

    // các mảnh chén cần cho nghi thức của đêm dem (1–3)
    public static string[] ManhDem(int dem)
    {
        int tu = 1, den = 3;
        if (dem == 2) { tu = 4; den = 7; }
        else if (dem >= 3) { tu = 8; den = 10; }
        var r = new string[den - tu + 1];
        for (int i = 0; i < r.Length; i++) r[i] = ManhChen(tu + i);
        return r;
    }

    // mọi mã đồ có trong game (dùng cho phím thử F7)
    public static string[] TatCa()
    {
        var l = new System.Collections.Generic.List<string>();
        l.Add(ChiaLenDay); l.Add(ChiaTuTho); l.Add(DayDan); l.Add(BangThuAm); l.Add(ManhGuong);
        l.AddRange(ManhAnh);
        for (int i = 1; i <= SoManhChen; i++) l.Add(ManhChen(i));
        return l.ToArray();
    }

    public static string Ten(string id)
    {
        switch (id)
        {
            case ChiaLenDay: return "Chìa lên dây đồng hồ";
            case ChiaTuTho: return "Chìa nhỏ (tủ đồ thờ)";
            case DayDan: return "Sợi dây đàn";
            case BangThuAm: return "Cuộn băng cassette";
            case ManhGuong: return "Mảnh gương";
            case ManhAnh1: return "Mảnh ảnh (góc trên trái)";
            case ManhAnh2: return "Mảnh ảnh (góc trên phải)";
            case ManhAnh3: return "Mảnh ảnh (góc dưới trái)";
            case ManhAnh4: return "Mảnh ảnh (góc dưới phải)";
        }
        if (id != null && id.StartsWith("ManhChen")) return "Mảnh chén " + id.Substring(8);
        return id;
    }
}

// Nghi thức mỗi đêm. LocMG_GhepChen gọi DaXong(dem) khi người chơi ráp xong chén của đêm đó;
// script game (chuyển đêm, cảnh kết) đăng ký vào đây:  LocNghiThuc.DaXong += dem => { ... };
public static class LocNghiThuc
{
    public static System.Action<int> DaXong;
    public static bool Xong(int dem) { return LocTui.Flag("NghiThuc" + dem); }

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { DaXong = null; }
}
