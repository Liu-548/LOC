using UnityEngine;

// [03/11] Cái gì "ăn điện" — dùng chung cho LocMa (mất điện), LocCongTac (công tắc) và LocThucTe (đổi vật liệu).
//  Trước đây mọi vật liệu phát sáng đều bị coi là đồ điện: mất điện thì đầu nhang trên bàn thờ vong thôi đỏ, ngọn lửa đèn dầu / nến sáp tắt,
//  bùa đang cháy thôi cháy… rồi có điện lại thì tự cháy lại. Lửa và nhang không dính gì tới điện.
public static class LocDien
{
    // tên vật liệu (viết thường) của thứ phát sáng không phải đồ điện: lửa (đèn dầu, nến sáp, chậu hoá vàng), đầu nhang, bùa đang cháy,
    // và vài vật liệu glTF bị đánh dấu phát sáng cho nổi (nước trong chén thờ, muối) — không phải đèn
    static readonly string[] KHONG_DIEN = { "nhang", "lua", "flame", "labua", "nuoc_chen", "muoi" };
    // nến điện cắm ổ riêng trên bàn thờ: mất điện thì tắt, nhưng công tắc đèn phòng không điều khiển
    static readonly string[] O_RIENG = { "dennen", "nendien" };

    static bool Co(string ten, string[] ds) { ten = ten.ToLowerInvariant(); foreach (var s in ds) if (ten.Contains(s)) return true; return false; }

    public static bool PhatSang(Material m) =>
        m && ((m.HasProperty("_EmissionColor") && m.GetColor("_EmissionColor").maxColorComponent > 0.02f) ||
              (m.HasProperty("emissiveFactor") && m.GetColor("emissiveFactor").maxColorComponent > 0.02f));

    public static bool KhongAnDien(Material m) => m && Co(m.name, KHONG_DIEN);
    public static bool CamORieng(Material m) => m && Co(m.name, O_RIENG);
    public static bool DoDien(Material m) => PhatSang(m) && !KhongAnDien(m);   // phát sáng nhờ điện

    // bản tắt của vật liệu phát sáng: tắt cả hai kiểu phát sáng (URP: _EmissionColor + từ khoá _EMISSION · glTFast: emissiveFactor)
    public static Material BanTat(Material m, string hauTo)
    {
        var t = new Material(m) { name = m.name + hauTo };
        if (t.HasProperty("_EmissionColor")) t.SetColor("_EmissionColor", Color.black);
        if (t.HasProperty("emissiveFactor")) t.SetColor("emissiveFactor", Color.black);
        t.DisableKeyword("_EMISSION");
        return t;
    }

    // đèn mô phỏng lửa (đèn dầu dưới gầm linh cữu…): không tắt khi mất điện
    public static bool LaLua(Light l) => l && l.GetComponent<LocLuaNhapNhay>();

    // bản "tắt" do LocMa / LocCongTac tạo (đang gắn trên renderer khi đèn tắt)
    public static bool LaBanTat(Material m) => m && (m.name.EndsWith("_Tat") || m.name.EndsWith("_CongTacTat") || m.name.StartsWith("M_BongHam_Tat") || m.name.StartsWith("M_BongDayToc_Tat"));
}
