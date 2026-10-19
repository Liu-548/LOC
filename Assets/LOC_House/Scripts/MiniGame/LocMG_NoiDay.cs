using UnityEngine;

// Mini game 4 — NỐI DÂY ĐIỆN ở bảng điện chính. Nối bốn đầu dây bên trái với đầu cùng màu bên phải (bên phải bị xáo), bấm Cấp điện.
// Nối sai thì chập: màn hình đỏ chớp một cái. LocTuongTac ở bảng điện mở khoá cửa phòng thờ khi xong.
public class LocMG_NoiDay : LocMiniGame
{
    public string cauXong = "Điện về. Có tiếng chốt cửa trên cao bật ra.";
    static readonly Color[] mau =
    {
        new Color(0.85f, 0.2f, 0.18f), new Color(0.25f, 0.7f, 0.3f), new Color(0.25f, 0.45f, 0.9f), new Color(0.92f, 0.78f, 0.2f)
    };
    static readonly string[] tenMau = { "Đỏ", "Xanh lá", "Xanh dương", "Vàng" };

    int[] phai = new int[4];    // phai[j] = màu của đầu phải thứ j
    int[] noi = new int[4];     // noi[i] = đầu phải đang nối với đầu trái i (-1 = chưa)
    int chon = -1;
    float chap;

    protected override string TenMacDinh() { return "Bảng điện chính"; }
    protected override string GoiYMacDinh() { return "Dây bị gỡ lung tung. Nối đúng màu rồi cấp điện, nối sai sẽ chập."; }

    protected override void VaoGame()
    {
        int thu = 0;
        do
        {
            for (int i = 0; i < 4; i++) phai[i] = i;
            for (int i = 3; i > 0; i--) { int j = Random.Range(0, i + 1); int t = phai[i]; phai[i] = phai[j]; phai[j] = t; }
        } while (Dung(phai) && ++thu < 20);
        if (Dung(phai)) { int t = phai[0]; phai[0] = phai[1]; phai[1] = t; }
        for (int i = 0; i < 4; i++) noi[i] = -1;
        chon = -1; chap = 0f;
    }

    static bool Dung(int[] p) { for (int i = 0; i < p.Length; i++) if (p[i] != i) return false; return true; }

    protected override void MoiKhung(float dt) { if (chap > 0f) chap = Mathf.Max(0f, chap - dt); }

    Vector2 DauTrai(Rect r, int i) { return new Vector2(r.x + 70f, r.y + 36f + i * 74f); }
    Vector2 DauPhai(Rect r, int j) { return new Vector2(r.xMax - 70f, r.y + 36f + j * 74f); }

    protected override void Ve(Rect r)
    {
        // dây đã nối
        for (int i = 0; i < 4; i++)
            if (noi[i] >= 0) VeDay(DauTrai(r, i) + new Vector2(26f, 0), DauPhai(r, noi[i]) - new Vector2(26f, 0), 6f, mau[i]);
        // đầu dây
        for (int i = 0; i < 4; i++)
        {
            var c = DauTrai(r, i); var o = new Rect(c.x - 26f, c.y - 18f, 52f, 36f);
            Hop(o, mau[i]); Khung(o, i == chon ? Color.white : new Color(0, 0, 0, 0.6f), i == chon ? 3f : 1f);
            GUI.Label(new Rect(c.x - 66f, c.y + 12f, 120f, 18f), tenMau[i], KieuChu(11, TextAnchor.MiddleLeft, FontStyle.Normal, new Color(0.8f, 0.78f, 0.72f), false));
            if (GUI.Button(o, GUIContent.none, GUIStyle.none)) chon = chon == i ? -1 : i;
        }
        for (int j = 0; j < 4; j++)
        {
            var c = DauPhai(r, j); var o = new Rect(c.x - 26f, c.y - 18f, 52f, 36f);
            Hop(o, mau[phai[j]]); Khung(o, new Color(0, 0, 0, 0.6f), 1f);
            GUI.Label(new Rect(c.x - 54f, c.y + 12f, 120f, 18f), tenMau[phai[j]], KieuChu(11, TextAnchor.MiddleRight, FontStyle.Normal, new Color(0.8f, 0.78f, 0.72f), false));
            if (GUI.Button(o, GUIContent.none, GUIStyle.none) && chon >= 0)
            {
                for (int k = 0; k < 4; k++) if (noi[k] == j) noi[k] = -1;
                noi[chon] = j; chon = -1;
            }
        }
        if (GUI.Button(new Rect(r.x + r.width / 2f - 62f, r.y + 300f, 124f, 30f), "Cấp điện")) CapDien();
        if (chap > 0f) Hop(new Rect(r.x - 20f, r.y - 108f, 640f, 540f), new Color(1f, 0.1f, 0.05f, 0.45f * (chap / 0.45f)));
    }

    void CapDien()
    {
        int dung = 0, ho = 0;
        for (int i = 0; i < 4; i++) { if (noi[i] < 0) ho++; else if (phai[noi[i]] == i) dung++; }
        if (ho > 0) { Sai("Còn " + ho + " đầu dây hở."); return; }
        if (dung == 4) { Thang(cauXong); return; }
        chap = 0.45f;
        Sai("Chập! Đúng " + dung + "/4 dây." + (soLanSai >= saiThiGoiY ? " Màu nào nối màu đó." : ""));
    }
}
