using UnityEngine;

// Mini game tổng quát "xếp món vào ô": chọn một món ở hàng dưới, bấm một ô để đặt; bấm ô đã đặt để lấy ra; Xác nhận.
// Dùng cho mâm cúng, xếp chỗ ngồi, xếp lại bàn học, ... Lớp con (LocMG_MamCung, LocMG_XepGhe, LocMG_XepLaiBan) chỉ điền dữ liệu.
//  cacMon[j]  tên món j · cacO[i] tên ô i · dapAn[i] = chỉ số món đúng của ô i · toaDoO[i] vị trí ô (0..1 trong bảng; bỏ trống = tự chia lưới)
//  gioDapAn > 0: lúc mở cho XEM đáp án ngần ấy giây rồi xoá hết (trò nhớ chỗ cũ)
public class LocMG_XepVao : LocMiniGame
{
    public string[] cacMon;
    public string[] cacO;
    public int[] dapAn;
    public Vector2[] toaDoO;
    public float gioDapAn = 0f;
    [TextArea] public string luat = "";
    public string cauXong = "Đúng rồi.";

    protected int[] gan = new int[0];   // gan[i] = món đang ở ô i (-1 = trống)
    int chon = -1;
    float conLai;

    protected virtual void CauHinh() { }
    protected virtual string LuatMacDinh() { return ""; }

    protected override void VaoGame()
    {
        bool hopLe = cacMon != null && cacMon.Length > 0 && cacO != null && cacO.Length > 0 && dapAn != null && dapAn.Length == cacO.Length;
        if (!hopLe) CauHinh();
        gan = new int[cacO.Length];
        for (int i = 0; i < gan.Length; i++) gan[i] = -1;
        chon = -1; conLai = gioDapAn;
        if (conLai > 0f) for (int i = 0; i < gan.Length; i++) gan[i] = dapAn[i];
    }

    protected override void MoiKhung(float dt)
    {
        if (conLai <= 0f) return;
        conLai -= dt;
        if (conLai > 0f) return;
        conLai = 0f;
        for (int i = 0; i < gan.Length; i++) gan[i] = -1;
        thongBao = "Giờ đặt lại về đúng chỗ cũ.";
    }

    bool DaDat(int mon) { for (int i = 0; i < gan.Length; i++) if (gan[i] == mon) return true; return false; }

    protected override void Ve(Rect r)
    {
        if (gan == null || gan.Length != cacO.Length) return;
        bool xem = conLai > 0f;
        float bw = 400f, bh = 196f;
        string lt = string.IsNullOrEmpty(luat) ? LuatMacDinh() : luat;
        if (!string.IsNullOrEmpty(lt))
            GUI.Label(new Rect(r.x + bw + 12, r.y, r.width - bw - 12, bh), lt, KieuChu(13, TextAnchor.UpperLeft, FontStyle.Normal, new Color(0.85f, 0.82f, 0.74f), true));
        if (xem) GUI.Label(new Rect(r.x, r.y - 4, bw, 20), "Nhìn kỹ — còn " + Mathf.CeilToInt(conLai) + " giây", KieuChu(14, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(1f, 0.82f, 0.55f), false));

        // các ô
        int n = cacO.Length;
        bool coToaDo = toaDoO != null && toaDoO.Length == n;
        int cot = n <= 4 ? 2 : 3, hangSo = (n + cot - 1) / cot;
        GUI.enabled = !xem;
        for (int i = 0; i < n; i++)
        {
            float cx, cy;
            if (coToaDo) { cx = r.x + toaDoO[i].x * bw; cy = r.y + 20f + toaDoO[i].y * (bh - 40f); }
            else { cx = r.x + (i % cot + 0.5f) / cot * bw; cy = r.y + 20f + ((i / cot) + 0.5f) / hangSo * (bh - 40f); }
            var o = new Rect(cx - 62f, cy - 25f, 124f, 50f);
            Hop(o, gan[i] >= 0 ? new Color(0.28f, 0.22f, 0.14f, 1f) : new Color(0.06f, 0.05f, 0.05f, 1f));
            Khung(o, chon >= 0 ? new Color(1f, 0.82f, 0.4f, 0.9f) : new Color(0.5f, 0.45f, 0.4f, 0.7f), 1f);
            GUI.Label(new Rect(o.x, o.y + 1, o.width, 16), cacO[i], KieuChu(11, TextAnchor.MiddleCenter, FontStyle.Italic, new Color(0.7f, 0.68f, 0.62f), false));
            if (gan[i] >= 0) GUI.Label(new Rect(o.x, o.y + 16, o.width, 32), cacMon[gan[i]], KieuChu(13, TextAnchor.MiddleCenter, FontStyle.Bold, Color.white, true));
            if (GUI.Button(o, GUIContent.none, GUIStyle.none))
            {
                if (chon >= 0)
                {
                    for (int k = 0; k < n; k++) if (gan[k] == chon) gan[k] = -1;
                    gan[i] = chon; chon = -1;
                }
                else gan[i] = -1;
            }
        }

        // hàng món
        int m = cacMon.Length, moiHang = 4;
        for (int j = 0; j < m; j++)
        {
            var b = new Rect(r.x + (j % moiHang) * 138f, r.y + 208f + (j / moiHang) * 36f, 130f, 30f);
            bool da = DaDat(j);
            var c0 = GUI.color;
            GUI.color = j == chon ? new Color(1f, 0.85f, 0.4f) : (da ? new Color(1f, 1f, 1f, 0.45f) : Color.white);
            if (GUI.Button(b, cacMon[j] + (da ? " •" : ""))) { if (!da) chon = chon == j ? -1 : j; }
            GUI.color = c0;
        }
        GUI.enabled = true;

        GUI.enabled = !xem;
        if (GUI.Button(new Rect(r.xMax - 124f, r.y + 290f, 124f, 34f), "Xác nhận")) KiemTra();
        GUI.enabled = true;
    }

    void KiemTra()
    {
        int dung = 0, trong = 0;
        for (int i = 0; i < gan.Length; i++) { if (gan[i] < 0) trong++; else if (gan[i] == dapAn[i]) dung++; }
        if (trong > 0) { Sai("Còn " + trong + " ô trống."); return; }
        if (dung == gan.Length) { Thang(cauXong); return; }
        Sai(soLanSai + 1 >= saiThiGoiY ? "Đúng " + dung + "/" + gan.Length + " ô. Đọc lại lời dặn." : "Chưa đúng. Thử lại.");
    }
}
