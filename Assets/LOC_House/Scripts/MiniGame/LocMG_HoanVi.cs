using UnityEngine;

// Mini game tổng quát "ghép tranh hoán vị": ảnh cắt thành cot × hang ô, xáo trộn; bấm hai ô để đổi chỗ cho tới khi đúng.
// Ảnh vẽ bằng code (VeAnh) — không dùng ảnh người thật. Lớp con: LocMG_GhepAnh, LocMG_GhepChen.
public abstract class LocMG_HoanVi : LocMiniGame
{
    public int cot = 2, hang = 2;
    public int canhAnh = 192;
    public string cauXong = "Ghép xong.";

    int[] vt = new int[0];   // vt[ô trên bảng] = mảnh đang nằm ở đó
    int chon = -1;
    Texture2D anh;

    // y = 0 là HÀNG DƯỚI CÙNG của ảnh (như Texture2D)
    protected abstract void VeAnh(Color32[] px, int n);
    protected virtual void DatLuoi() { }   // lớp con đổi cot/hang (ví dụ theo đêm)
    protected virtual void KhiXepXong() { Thang(cauXong); }   // lớp con chơi nhiều lượt (dán hai chén)

    protected override void VaoGame()
    {
        DatLuoi();
        cot = Mathf.Max(1, cot); hang = Mathf.Max(1, hang);
        if (anh == null)
        {
            var px = new Color32[canhAnh * canhAnh];
            VeAnh(px, canhAnh);
            anh = TaoTexture(px, canhAnh);
        }
        Xao(); chon = -1;
    }

    protected override void RaKhoi() { }
    void OnDestroy() { if (anh) Destroy(anh); }

    bool DaXep() { for (int i = 0; i < vt.Length; i++) if (vt[i] != i) return false; return true; }

    protected void Xao()
    {
        int c = cot * hang;
        vt = new int[c];
        for (int i = 0; i < c; i++) vt[i] = i;
        if (c < 2) return;
        int thu = 0;
        do
        {
            for (int i = c - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int t = vt[i]; vt[i] = vt[j]; vt[j] = t;
            }
        } while (DaXep() && ++thu < 20);
        if (DaXep()) { int t = vt[0]; vt[0] = vt[1]; vt[1] = t; }
    }

    protected override void Ve(Rect r)
    {
        if (vt.Length != cot * hang) Xao();
        float canh = Mathf.Min(r.height - 6f, 300f);
        float bx = r.x + (r.width - canh) / 2f, by = r.y + 2f;
        float cw = canh / cot, ch = canh / hang;
        for (int i = 0; i < vt.Length; i++)
        {
            int manh = vt[i];
            var o = new Rect(bx + (i % cot) * cw + 1f, by + (i / cot) * ch + 1f, cw - 2f, ch - 2f);
            var uv = new Rect((manh % cot) / (float)cot, 1f - ((manh / cot) + 1f) / hang, 1f / cot, 1f / hang);
            GUI.DrawTextureWithTexCoords(o, anh, uv);
            if (i == chon) Khung(o, new Color(1f, 0.85f, 0.3f, 1f), 3f);
            if (GUI.Button(o, GUIContent.none, GUIStyle.none))
            {
                if (chon < 0) chon = i;
                else if (chon == i) chon = -1;
                else
                {
                    int t = vt[chon]; vt[chon] = vt[i]; vt[i] = t;
                    chon = -1;
                    if (DaXep()) { KhiXepXong(); return; }
                }
            }
        }
    }

    // ───── vẽ hình lên mảng điểm ảnh (y = 0 ở dưới cùng)
    protected static void TuHinhChuNhat(Color32[] p, int n, int x0, int y0, int x1, int y1, Color32 c)
    {
        x0 = Mathf.Clamp(x0, 0, n - 1); x1 = Mathf.Clamp(x1, 0, n - 1);
        y0 = Mathf.Clamp(y0, 0, n - 1); y1 = Mathf.Clamp(y1, 0, n - 1);
        for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) p[y * n + x] = c;
    }

    protected static void TuHinhTron(Color32[] p, int n, float cx, float cy, float rad, Color32 c)
    {
        int x0 = Mathf.Max(0, (int)(cx - rad)), x1 = Mathf.Min(n - 1, (int)(cx + rad));
        int y0 = Mathf.Max(0, (int)(cy - rad)), y1 = Mathf.Min(n - 1, (int)(cy + rad));
        float r2 = rad * rad;
        for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++)
            if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r2) p[y * n + x] = c;
    }

    // elip tâm (cx,cy) bán kính rx,ry; chỉ vẽ phần y trong [yMin,yMax]
    protected static void TuHinhElip(Color32[] p, int n, float cx, float cy, float rx, float ry, float yMin, float yMax, Color32 c)
    {
        int x0 = Mathf.Max(0, (int)(cx - rx)), x1 = Mathf.Min(n - 1, (int)(cx + rx));
        int y0 = Mathf.Max(0, (int)Mathf.Max(cy - ry, yMin)), y1 = Mathf.Min(n - 1, (int)Mathf.Min(cy + ry, yMax));
        for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++)
        {
            float dx = (x - cx) / rx, dy = (y - cy) / ry;
            if (dx * dx + dy * dy <= 1f) p[y * n + x] = c;
        }
    }

    protected static void TuDuong(Color32[] p, int n, float ax, float ay, float bx, float by, float day, Color32 c)
    {
        int buoc = Mathf.Max(2, (int)(Mathf.Max(Mathf.Abs(bx - ax), Mathf.Abs(by - ay)) * 1.5f));
        for (int i = 0; i <= buoc; i++)
        {
            float t = i / (float)buoc;
            TuHinhTron(p, n, Mathf.Lerp(ax, bx, t), Mathf.Lerp(ay, by, t), day / 2f, c);
        }
    }

    protected static void TuNhieu(Color32[] p, int n, int bienDo, int hat)
    {
        var rd = new System.Random(hat);
        for (int i = 0; i < p.Length; i++)
        {
            int d = rd.Next(-bienDo, bienDo + 1);
            var c = p[i];
            p[i] = new Color32((byte)Mathf.Clamp(c.r + d, 0, 255), (byte)Mathf.Clamp(c.g + d, 0, 255), (byte)Mathf.Clamp(c.b + d, 0, 255), 255);
        }
    }
}
