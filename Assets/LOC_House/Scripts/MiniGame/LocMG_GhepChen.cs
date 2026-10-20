using UnityEngine;

// [21/10] Đêm 2 — DÁN CHÉN dưới hầm (kịch bản v3.2 B3.9, C5 Đêm 2): ghép lại chén 1 (bố) rồi chén 2 (mẹ), mỗi chén 3×2 mảnh.
// Bấm hai mảnh để đổi chỗ cho tới khi khít. Xong cả hai thì bật cờ "dan_chen".
// shortcut: chưa đòi lọ keo (cửa tiệm) và chén số 3 (tầng 3) — thêm canId khi hai món đó được dựng
public class LocMG_GhepChen : LocMG_HoanVi
{
    int daDan;

    protected override string TenMacDinh() { return daDan == 0 ? "Dán chén của bố" : "Dán chén của mẹ"; }
    protected override string GoiYMacDinh() { return "Ráp từng mảnh cho khít. Chén chỉ cần chứa được nước."; }

    protected override void DatLuoi() { cot = 3; hang = 2; daDan = 0; cauXong = "Hai cái chén dán xong. Đường keo còn ướt."; }

    protected override void KhiXepXong()
    {
        if (++daDan < 2) { thongBao = "Xong chén của bố. Còn chén của mẹ."; Xao(); return; }
        Thang(cauXong);
    }

    protected override void VeAnh(Color32[] px, int n)
    {
        var nen = new Color32(30, 26, 24, 255);
        for (int i = 0; i < px.Length; i++) px[i] = nen;
        float s = n / 256f;
        // đĩa lót, chân, thân chén (nửa dưới elip), miệng
        TuHinhElip(px, n, 128 * s, 44 * s, 92 * s, 12 * s, 0, n, new Color32(170, 160, 146, 255));
        TuHinhChuNhat(px, n, (int)(102 * s), (int)(44 * s), (int)(154 * s), (int)(68 * s), new Color32(190, 182, 166, 255));
        TuHinhElip(px, n, 128 * s, 160 * s, 78 * s, 96 * s, 0, 160 * s, new Color32(214, 206, 190, 255));
        TuHinhElip(px, n, 128 * s, 160 * s, 78 * s, 14 * s, 0, n, new Color32(90, 80, 70, 255));
        // dải xanh trang trí quanh thân
        for (int y = (int)(112 * s); y < (int)(124 * s); y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x - 128 * s) / (78 * s), dy = (y - 160 * s) / (96 * s);
                if (dx * dx + dy * dy <= 0.86f) px[y * n + x] = new Color32(60, 90, 140, 255);
            }
        // vết nứt
        var nut = new Color32(22, 20, 18, 255);
        TuDuong(px, n, 140 * s, 172 * s, 118 * s, 120 * s, 3f * s, nut);
        TuDuong(px, n, 118 * s, 120 * s, 134 * s, 78 * s, 3f * s, nut);
        TuDuong(px, n, 90 * s, 170 * s, 128 * s, 128 * s, 3f * s, nut);
        TuDuong(px, n, 168 * s, 150 * s, 140 * s, 122 * s, 3f * s, nut);
        TuNhieu(px, n, 5, 11);
    }
}
