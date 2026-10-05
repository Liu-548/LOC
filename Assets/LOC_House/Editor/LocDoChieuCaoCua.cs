using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// [2/10] Đo chiều cao thật của mọi cánh cửa/cổng (LocDoor, trừ cửa sổ) trong scene hiện tại và ghi Assets/LOC_House/BaoCao_ChieuCaoCua.txt.
// Menu: LOC → Đo chiều cao cửa. Cánh nào thấp hơn 1,80 m được đánh dấu THẤP.
public static class LocDoChieuCaoCua
{
    const float Min = 1.8f;

    [MenuItem("LOC/Đo chiều cao cửa")]
    public static void Do()
    {
        var kq = Object.FindObjectsByType<LocDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(d => d && !d.ten.Contains("sổ") && !d.ten.Contains("ngăn kéo") && !d.ten.Contains("tủ") && !d.ten.Contains("chạn") && !d.ten.Contains("két"))   // bỏ cánh đồ đạc
            .Select(d =>
            {
                var rs = d.GetComponentsInChildren<Renderer>(true);
                if (rs.Length == 0) return (d.ten, d.name, 0f);
                var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
                return (d.ten, d.name, b.size.y);
            })
            .OrderBy(t => t.Item3).ToArray();
        var sb = new StringBuilder();
        int thap = 0;
        foreach (var (ten, ten2, h) in kq)
        {
            bool t = h < Min - 0.001f; if (t) thap++;
            sb.AppendLine($"{h:0.00} m  {(t ? "THẤP  " : "")}{ten}  [{ten2}]");
        }
        sb.Insert(0, $"CHIỀU CAO CÁNH CỬA (ngưỡng {Min:0.00} m) — {kq.Length} cánh, {thap} cánh thấp hơn ngưỡng{System.Environment.NewLine}");
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_ChieuCaoCua.txt", sb.ToString());
        Debug.Log($"[LOC] Đo cửa: {kq.Length} cánh, thấp nhất {(kq.Length > 0 ? kq[0].Item3 : 0):0.00} m, {thap} cánh < {Min:0.00} m — Assets/LOC_House/BaoCao_ChieuCaoCua.txt");
    }
}
