using System.Linq;
using UnityEditor;
using UnityEngine;

// [21/10] Phòng Khôi đóng cửa từ 9/2000 nên phải trông bỏ không lâu năm: màng bụi trên sàn và mặt đồ phẳng,
//  ố nước thấm từ mép trần xuống bốn tường, vết nước trên trần, thêm mạng nhện ở hai góc trần.
//  Chạy sau rà soát (đồ đã thả chạm mặt đỡ) để bụi nằm đúng mặt bàn/nóc tủ. Ảnh decal sinh bằng LOC_Nguon/PhongKhoi/bui.py.
public static partial class LocHouseBuilder
{
    static void PhongKhoiBoKhong()
    {
        var p = root.Find("Tang_2/PhongKhoi"); if (!p) return;
        var old = cur; cur = p;
        const float y = Y2, x0 = 3.2f, x1 = W, z0 = 12.5f, z1 = 16.4f;
        float xm = (x0 + x1) / 2, zm = (z0 + z1) / 2;
        foreach (var t in new[] { "Decal_Bui_San.png", "Decal_Bui_Mat.png", "Decal_O_Tuong.png" })
            AssetDatabase.ImportAsset($"{TexFolder}/{t}");   // ảnh mới kéo từ git: nhập trước, kẻo vật liệu decal được tạo với texture rỗng

        DF("Decal_Bui_San.png", "M_Decal_BuiSan", xm, y + 0.004f, zm, 0, x1 - x0, z1 - z0);

        // dải ố từ trần xuống tới y + 2,30 (trên đầu cửa phòng 2,10 và mép trên cửa sổ 2,30); lùi 2 mm, nằm sau decal mốc/mạng nhện (3–5 mm)
        float yb = y + 2.3f, h = C2 - 0.002f - yb;
        DW("Decal_O_Tuong.png", "M_Decal_OTuong", x0 + 0.002f, yb, zm, 90, z1 - z0, h);
        DW("Decal_O_Tuong.png", "M_Decal_OTuong", x1 - 0.002f, yb, zm, -90, z1 - z0, h);
        DW("Decal_O_Tuong.png", "M_Decal_OTuong", xm, yb, z0 + 0.002f, 0, x1 - x0, h);
        DW("Decal_O_Tuong.png", "M_Decal_OTuong", xm, yb, z1 - 0.002f, 180, x1 - x0, h);
        DC("Decal_VetNuocTran.png", "M_Decal_VetNuocTran", 6.7f, C2 - 0.003f, 15.7f, 1.3f, 1.3f, 35);
        DW("Decal_MangNhen.png", "M_Decal_MangNhen", x1 - 0.004f, y + 2.55f, 16.1f, -90, 0.6f, 0.6f);
        DW("Decal_MangNhen.png", "M_Decal_MangNhen", x1 - 0.3f, y + 2.55f, z0 + 0.004f, 0, 0.6f, 0.6f);

        // chỉ mặt phẳng: mặt kính bàn học, nóc tủ áo (ghế, đàn, túi, nilon trùm giường gồ ghề — bụi nổi lơ lửng nên bỏ)
        foreach (var n in new[] { "BanHoc_Khoi_Bo", "TuAo_Khoi_Go" })
        {
            var t = p.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == n);
            var rs = t ? t.GetComponentsInChildren<Renderer>().Where(r => !r.GetComponentInParent<LocDoor>()).ToArray() : new Renderer[0];   // bỏ cánh tủ/ngăn kéo đang hé
            if (rs.Length == 0) continue;
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            DF("Decal_Bui_Mat.png", "M_Decal_BuiMat", b.center.x, b.max.y + 0.003f, b.center.z, 0, b.size.x * 0.94f, b.size.z * 0.94f);
        }
        cur = old;
    }
}
