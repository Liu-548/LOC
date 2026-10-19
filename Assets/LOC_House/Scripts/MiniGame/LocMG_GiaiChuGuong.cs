using UnityEngine;

// Mini game 8 — GIẢI CHỮ GƯƠNG ở bảng đen phòng Nhím. Chữ trên bảng bị lật ngược; cầm mảnh gương soi vào mới đọc được.
// Gõ lại dòng đọc được. Cần có ManhGuong trong túi (DuDieuKien).
public class LocMG_GiaiChuGuong : LocMiniGame
{
    public string dong = "ANH Ở DƯỚI HẦM";       // (giả định) lời nhắn ẩn trên bảng — sửa theo kịch bản
    public string cauXong = "Mẩu phấn cuối bảng khẽ rơi xuống.";
    string go = "";
    bool soi;

    protected override string TenMacDinh() { return "Bảng đen của Nhím"; }
    protected override string GoiYMacDinh() { return "Chữ trên bảng bị lật ngược. Giữ mảnh gương lên để đọc, rồi gõ lại dòng chữ."; }

    public override bool DuDieuKien(out string cau)
    {
        cau = "";
        if (LocTui.Co(LocId.ManhGuong)) return true;
        cau = "Chữ viết ngược. Cần thứ gì đó để soi.";
        return false;
    }

    protected override void VaoGame() { go = ""; soi = false; }

    protected override void Ve(Rect r)
    {
        var bang = new Rect(r.x + 20f, r.y + 10f, 560f, 120f);
        Hop(bang, new Color(0.07f, 0.1f, 0.08f, 1f)); Khung(bang, new Color(0.35f, 0.25f, 0.18f), 4f);
        var m = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(-1f, 1f), bang.center);
        GUI.Label(bang, dong, KieuChu(34, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(0.85f, 0.85f, 0.8f, 0.9f), false));
        GUI.matrix = m;

        var g = new Rect(r.x + 20f, r.y + 146f, 560f, 70f);
        if (soi)
        {
            Hop(g, new Color(0.5f, 0.6f, 0.65f, 0.35f)); Khung(g, new Color(0.8f, 0.85f, 0.9f), 2f);
            GUI.Label(g, dong, KieuChu(30, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(0.9f, 0.95f, 1f), false));
        }
        else Khung(g, new Color(0.4f, 0.4f, 0.4f, 0.5f), 1f);
        if (GUI.Button(new Rect(r.x + 200f, r.y + 224f, 200f, 28f), soi ? "Hạ gương xuống" : "Giơ gương lên")) soi = !soi;

        GUI.Label(new Rect(r.x + 20f, r.y + 266f, 100f, 28f), "Dòng chữ:", KieuChu(15, TextAnchor.MiddleLeft, FontStyle.Normal, Color.white, false));
        go = GUI.TextField(new Rect(r.x + 120f, r.y + 266f, 340f, 28f), go, 40);
        if (GUI.Button(new Rect(r.x + 470f, r.y + 266f, 110f, 28f), "Xác nhận"))
        {
            if (BoDau(go) == BoDau(dong)) Thang(cauXong);
            else Sai("Không đúng." + (soLanSai >= saiThiGoiY ? " Viết đủ cả dòng, không cần dấu cũng được." : ""));
        }
    }
}
