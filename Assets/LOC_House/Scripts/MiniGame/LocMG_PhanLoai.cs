using UnityEngine;

// [22/10] Mini game tổng quát "phân loại vào ngăn": nhiều món, mỗi ngăn chứa được nhiều món. Bấm món ở khay dưới → bấm ngăn để bỏ vào;
// bấm tên món trong ngăn để lấy ra. Đặt hết thì tự kiểm: món sai chỗ bật về khay. Không có thua.
//  cacNgan[i] tên ngăn · cacMon[j] tên món · dungNgan[j] ngăn đúng của món j
//  ghiChu[j]  (tuỳ chọn) Khôi để ý thấy gì khi cầm món j lên — hiện chữ nghiêng
//  tuChoi[j]  (tuỳ chọn) bỏ món j vào sai ngăn thì Khôi khựng lại, nói câu này, và món tự về đúng ngăn ("ma sát tốt" kiểu Unpacking)
// Lớp con chỉ điền dữ liệu: LocMG_DonBanNuoc, LocMG_RutQuanAo, LocMG_XepHoSo.
public class LocMG_PhanLoai : LocMiniGame
{
    public string[] cacNgan;
    public string[] cacMon;
    public int[] dungNgan;
    public string[] ghiChu;
    public string[] tuChoi;
    public string cauXong = "";

    protected int[] o = new int[0];   // o[j] = ngăn đang chứa món j (-1 = còn trên khay)
    int chon = -1;

    protected virtual void CauHinh() { }

    protected override void VaoGame()
    {
        bool hopLe = cacNgan != null && cacNgan.Length > 0 && cacMon != null && cacMon.Length > 0 && dungNgan != null && dungNgan.Length == cacMon.Length;
        if (!hopLe) CauHinh();
        o = new int[cacMon.Length];
        for (int j = 0; j < o.Length; j++) o[j] = -1;
        chon = -1;
    }

    static string Lay(string[] a, int j) { return a != null && j >= 0 && j < a.Length ? a[j] : ""; }

    void BoVao(int j, int ngan)
    {
        string tc = Lay(tuChoi, j);
        if (!string.IsNullOrEmpty(tc) && ngan != dungNgan[j]) { thongBao = tc; o[j] = dungNgan[j]; }
        else o[j] = ngan;
        chon = -1;
        for (int k = 0; k < o.Length; k++) if (o[k] < 0) return;
        KiemTra();
    }

    void KiemTra()
    {
        var sai = new System.Collections.Generic.List<string>();
        for (int k = 0; k < o.Length; k++) if (o[k] != dungNgan[k]) { sai.Add(cacMon[k]); o[k] = -1; }
        if (sai.Count == 0) { Thang(cauXong); return; }
        Sai(soLanSai + 1 >= saiThiGoiY ? "Chưa đúng chỗ: " + string.Join(", ", sai) + "." : "Có " + sai.Count + " món để chưa đúng chỗ.");
    }

    protected override void Ve(Rect r)
    {
        if (o == null || cacMon == null || o.Length != cacMon.Length) return;
        int n = cacNgan.Length;
        float gap = 8f, w = (r.width - gap * (n - 1)) / n, h = 168f;
        var chuMon = KieuChu(12, TextAnchor.MiddleLeft, FontStyle.Normal, Color.white, true);
        for (int i = 0; i < n; i++)
        {
            var b = new Rect(r.x + i * (w + gap), r.y, w, h);
            Hop(b, new Color(0.07f, 0.06f, 0.05f, 1f));
            Khung(b, chon >= 0 ? new Color(1f, 0.82f, 0.4f, 0.9f) : new Color(0.5f, 0.45f, 0.4f, 0.7f), 1f);
            GUI.Label(new Rect(b.x + 4, b.y + 3, b.width - 8, 34), cacNgan[i], KieuChu(12, TextAnchor.UpperCenter, FontStyle.Bold, new Color(0.9f, 0.86f, 0.76f), true));
            if (chon >= 0 && GUI.Button(new Rect(b.x, b.y, b.width, 36), GUIContent.none, GUIStyle.none)) { BoVao(chon, i); return; }
            float y = b.y + 40;
            int soMon = 0; for (int j = 0; j < o.Length; j++) if (o[j] == i) soMon++;
            float buoc = Mathf.Min(27f, (h - 44f) / Mathf.Max(1, soMon));   // ngăn đầy thì dồn dòng lại, không tràn ra ngoài
            for (int j = 0; j < o.Length; j++)
            {
                if (o[j] != i) continue;
                var dong = new Rect(b.x + 6, y, b.width - 12, Mathf.Min(24f, buoc - 2f));
                Hop(dong, new Color(0.28f, 0.22f, 0.14f, 1f));
                GUI.Label(new Rect(dong.x + 4, dong.y, dong.width - 8, dong.height), cacMon[j], chuMon);
                if (GUI.Button(dong, GUIContent.none, GUIStyle.none)) { o[j] = -1; return; }   // lấy ra
                y += buoc;
            }
            // ô trống còn lại của ngăn cũng nhận bấm
            if (chon >= 0 && y < b.yMax - 4 && GUI.Button(new Rect(b.x, y, b.width, b.yMax - y), GUIContent.none, GUIStyle.none)) { BoVao(chon, i); return; }
        }

        // khay: món chưa xếp
        GUI.Label(new Rect(r.x, r.y + h + 6, r.width, 18), "Bấm một món rồi bấm ngăn để bỏ vào · bấm món trong ngăn để lấy ra", KieuChu(11, TextAnchor.MiddleLeft, FontStyle.Italic, new Color(0.7f, 0.68f, 0.62f), false));
        int c = 0;
        for (int j = 0; j < o.Length; j++)
        {
            if (o[j] >= 0) continue;
            var b = new Rect(r.x + (c % 3) * 202f, r.y + h + 28 + (c / 3) * 34f, 196f, 30f);
            var c0 = GUI.color;
            GUI.color = j == chon ? new Color(1f, 0.85f, 0.4f) : Color.white;
            if (GUI.Button(b, cacMon[j]))
            {
                chon = chon == j ? -1 : j;
                string gc = Lay(ghiChu, j);
                if (chon == j && !string.IsNullOrEmpty(gc)) thongBao = gc;
            }
            GUI.color = c0;
            c++;
        }
    }
}
