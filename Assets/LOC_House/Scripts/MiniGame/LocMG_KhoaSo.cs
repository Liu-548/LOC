using UnityEngine;

// Mini game 2 — KHOÁ SỐ (két sắt khoá số phòng bố mẹ). Chỉnh từng chữ số rồi bấm Mở.
// ma = mật mã (chỉ chữ số). Manh mối: tờ lịch bloc trong bếp dừng ở 5/8 → "0508" (ngày trước, tháng sau).
public class LocMG_KhoaSo : LocMiniGame
{
    public string ma = "0508";
    public string cauXong = "Cạch. Khoá mở rồi.";
    [TextArea] public string manhMoi = "Tờ lịch bloc trong bếp dừng ở một ngày. Ngày trước, tháng sau.";
    int[] so = new int[4];

    protected override string TenMacDinh() { return "Két sắt khoá số"; }
    protected override string GoiYMacDinh() { return "Bốn chữ số. Ai đó đã khoá lại trước khi đi."; }

    protected override void VaoGame() { so = new int[ma.Length]; }

    protected override void Ve(Rect r)
    {
        const float w = 80f;
        float x0 = r.x + r.width / 2f - ma.Length * w / 2f;
        var sd = KieuChu(44, TextAnchor.MiddleCenter, FontStyle.Bold, Color.white, false);
        for (int i = 0; i < ma.Length; i++)
        {
            float x = x0 + i * w;
            if (GUI.Button(new Rect(x + 8, r.y + 20, w - 16, 34), "+")) so[i] = (so[i] + 1) % 10;
            GUI.Label(new Rect(x, r.y + 60, w, 70), so[i].ToString(), sd);
            if (GUI.Button(new Rect(x + 8, r.y + 134, w - 16, 34), "-")) so[i] = (so[i] + 9) % 10;
        }
        if (GUI.Button(new Rect(r.x + r.width / 2f - 60, r.y + 200, 120, 36), "Mở"))
        {
            var s = "";
            for (int i = 0; i < so.Length; i++) s += so[i];
            if (s == ma) Thang(cauXong);
            else Sai("Không mở. Thử lại." + (soLanSai >= saiThiGoiY && !string.IsNullOrEmpty(manhMoi) ? "\nGợi ý: " + manhMoi : ""));
        }
    }
}
