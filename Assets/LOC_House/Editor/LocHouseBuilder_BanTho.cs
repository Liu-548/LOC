using System.Linq;
using UnityEngine;

public static partial class LocHouseBuilder
{
    // [2/10] bàn thờ ông bà theo lệ, tính theo hướng tủ thờ (người đứng nhìn vào):
    //  ảnh ông bên phải – bà bên trái (nam tả nữ hữu, tính từ bàn thờ nhìn ra); bát hương chính giữa, hai đèn nến kẹp hai bên;
    //  "đông bình tây quả": bình hoa bên phải, đĩa quả bên trái; chén nước ngay trước bát hương.
    //  Bình cúc (BinhHoaTho) thay lọ cúc nhỏ của bộ đồ thờ; hộp nhang nến dự trữ cất trong lòng tủ thờ, sát cánh phải.
    static void SapBanThoOngBa(GameObject set, float yaw)
    {
        if (!set) return;
        Transform F(string n) => set.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == n);
        Bounds BB(Transform t) { var rs = t.GetComponentsInChildren<Renderer>(true); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b; }
        var matTu = F("TuTho_MatTu"); var tu = F("BanThoGiaTien"); var khan = F("Khan_MatPhu"); var day = F("TuTho_ThanTu_DayTrong");
        if (!matTu || !tu) { Debug.LogWarning("[LOC] SapBanThoOngBa: thiếu TuTho_MatTu/BanThoGiaTien"); return; }
        var mb = BB(matTu);
        float top = khan ? BB(khan).max.y : mb.max.y;
        var front = tu.TransformDirection(Vector3.forward); front.y = 0; front.Normalize();
        var right = Vector3.Cross(Vector3.up, -front);
        var c = new Vector3(mb.center.x, top, mb.center.z);
        void Put(string n, float u, float? w)
        {
            var t = F(n); if (!t) return;
            var b = BB(t); var bc = new Vector3(b.center.x, 0, b.center.z); var cc = new Vector3(c.x, 0, c.z);
            float wNow = -Vector3.Dot(bc - cc, front);
            var dst = cc + right * u - front * (w ?? wNow);
            t.position += new Vector3(dst.x - bc.x, w.HasValue ? top - b.min.y : 0, dst.z - bc.z);
        }
        Put("BatHuong", 0f, 0.06f);
        Put("DenNen_L", -0.30f, 0.06f); Put("DenNen_R", 0.30f, 0.06f);
        Put("ChenNuocTrong", 0f, -0.19f);
        Put("DiaQua", -0.50f, -0.01f);
        Put("KhungAnh_Ong", 0.20f, null); Put("KhungAnh_Ba", -0.20f, null);   // giữ độ cao + sát tường, chỉ đổi bên
        var lo = F("LoHoaCuc"); if (lo) lo.gameObject.SetActive(false);
        var pb = c + right * 0.52f - front * 0.04f;
        A("BinhHoaTho", pb.x, pb.z, top, yaw);

        if (day)
        {
            var db = BB(day);
            float half = Mathf.Abs(front.x) * db.extents.x + Mathf.Abs(front.z) * db.extents.z;
            var p = new Vector3(db.center.x, 0, db.center.z) + right * 0.33f + front * (half - 0.085f);
            A("HuongNen_DuTru", p.x, p.z, db.max.y, yaw, k: LocProp.Kieu.CoDinh);   // trong lòng tủ thờ — rà soát không được nhấc ra
            var ps = new Vector3(db.center.x, 0, db.center.z) - right * 0.33f + front * (half - 0.13f);
            SachGiaLe(ps, db.max.y, yaw);   // [3/10 v3.0] quyển sách gia lễ của mẹ, sát cánh trái
        }
    }
}
