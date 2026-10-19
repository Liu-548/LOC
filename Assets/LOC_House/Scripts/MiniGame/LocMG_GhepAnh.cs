using UnityEngine;

// Mini game 3 — GHÉP ẢNH GIA ĐÌNH. Bốn mảnh ảnh nhặt rải khắp nhà (LocId.ManhAnh1–4), ghép lại thành tấm ảnh bốn người.
// Ảnh chỉ là bóng người trừu tượng (không mặt, không ảnh thật) với đồ vật khác nhau ở mỗi góc để nhận ra mảnh nào ở đâu.
public class LocMG_GhepAnh : LocMG_HoanVi
{
    protected override string TenMacDinh() { return "Ảnh gia đình"; }
    protected override string GoiYMacDinh() { return "Bốn mảnh vỡ của cùng một tấm ảnh. Bấm hai mảnh để đổi chỗ."; }
    protected override void DatLuoi() { cot = 2; hang = 2; }

    protected override void VeAnh(Color32[] px, int n)
    {
        var nen = new Color32(52, 40, 32, 255);
        for (int i = 0; i < px.Length; i++) px[i] = nen;
        float s = n / 256f;
        // đồ vật nền, mỗi góc một thứ
        TuHinhTron(px, n, 52 * s, 200 * s, 24 * s, new Color32(150, 130, 90, 255));      // đồng hồ tròn — trên trái
        TuHinhTron(px, n, 52 * s, 200 * s, 19 * s, nen);
        TuHinhChuNhat(px, n, (int)(160 * s), (int)(168 * s), (int)(236 * s), (int)(236 * s), new Color32(96, 110, 120, 255));   // cửa sổ — trên phải
        TuHinhChuNhat(px, n, (int)(196 * s), (int)(168 * s), (int)(200 * s), (int)(236 * s), nen);
        TuHinhChuNhat(px, n, (int)(160 * s), (int)(198 * s), (int)(236 * s), (int)(202 * s), nen);
        TuHinhChuNhat(px, n, (int)(4 * s), (int)(12 * s), (int)(76 * s), (int)(76 * s), new Color32(92, 52, 44, 255));         // tay ghế — dưới trái
        TuHinhChuNhat(px, n, (int)(205 * s), (int)(12 * s), (int)(244 * s), (int)(56 * s), new Color32(70, 60, 40, 255));      // chậu cây — dưới phải
        TuHinhTron(px, n, 224 * s, 78 * s, 18 * s, new Color32(50, 90, 50, 255));
        // bốn bóng người: bố, mẹ, Khôi, Nhím (chỉ khối và đầu tròn, không nét mặt)
        Nguoi(px, n, 78 * s, 30 * s, 28 * s, 96 * s, 15 * s, s);
        Nguoi(px, n, 118 * s, 30 * s, 26 * s, 88 * s, 14 * s, s);
        Nguoi(px, n, 158 * s, 30 * s, 23 * s, 76 * s, 13 * s, s);
        Nguoi(px, n, 194 * s, 30 * s, 18 * s, 50 * s, 11 * s, s);
        // viền ảnh
        var vien = new Color32(130, 110, 80, 255);
        int d = Mathf.Max(2, (int)(4 * s));
        TuHinhChuNhat(px, n, 0, 0, n - 1, d, vien); TuHinhChuNhat(px, n, 0, n - 1 - d, n - 1, n - 1, vien);
        TuHinhChuNhat(px, n, 0, 0, d, n - 1, vien); TuHinhChuNhat(px, n, n - 1 - d, 0, n - 1, n - 1, vien);
        TuNhieu(px, n, 6, 7);
    }

    static void Nguoi(Color32[] p, int n, float cx, float chan, float rong, float cao, float dau, float s)
    {
        TuHinhChuNhat(p, n, (int)(cx - rong / 2f), (int)chan, (int)(cx + rong / 2f), (int)(chan + cao), new Color32(118, 98, 84, 255));
        TuHinhTron(p, n, cx, chan + cao + dau * 0.8f, dau, new Color32(150, 128, 108, 255));
    }
}
