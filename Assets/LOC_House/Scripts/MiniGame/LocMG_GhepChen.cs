using UnityEngine;

// Nghi thức mỗi đêm — RÁP LẠI CHÉN VỠ ở bàn thờ hầm. Cần đủ các mảnh chén của đêm đó (LocId.ManhDem), ráp xong là xong nghi thức.
// Đêm 1: 2×2 mảnh · Đêm 2: 3×2 · Đêm 3: 3×3. LocTuongTac đặt trên bàn thờ hầm dùng lapLai = true (mỗi đêm một lần).
public class LocMG_GhepChen : LocMG_HoanVi
{
    int dem = 1;

    protected override string TenMacDinh() { return "Ráp lại chén"; }
    protected override string GoiYMacDinh() { return "Ráp từng mảnh lại cho khít, đừng để vết nứt lệch."; }

    int DemHienTai() { return Mathf.Clamp(LocMa.dem, 1, 3); }

    public override bool DuDieuKien(out string cau)
    {
        int d = DemHienTai();
        if (LocNghiThuc.Xong(d)) { cau = "Nghi thức đêm nay đã xong."; return false; }
        var can = LocId.ManhDem(d);
        int co = LocTui.DemCo(can);
        if (co < can.Length) { cau = "Còn thiếu mảnh chén. Đã có " + co + "/" + can.Length + "."; return false; }
        cau = ""; return true;
    }

    protected override void DatLuoi()
    {
        dem = DemHienTai();
        cauXong = "Chiếc chén liền lại. Nghi thức đêm " + dem + " đã xong.";
        if (dem == 1) { cot = 2; hang = 2; }
        else if (dem == 2) { cot = 3; hang = 2; }
        else { cot = 3; hang = 3; }
    }

    protected override void KhiThang()
    {
        LocTui.BoTatCa(LocId.ManhDem(dem));
        LocTui.Dat("NghiThuc" + dem);
        if (LocNghiThuc.DaXong != null) LocNghiThuc.DaXong(dem);
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
