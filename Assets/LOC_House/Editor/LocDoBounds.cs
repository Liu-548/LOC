using System.Linq;
using UnityEditor;
using UnityEngine;

// [4/10] đo bao thế giới: LOC_Do.txt mỗi dòng 1 tên (khớp đầu tên GameObject, "x<-0.2" = mọi renderer trong Tiem có max.x > −0,2) → LOC_Do_KetQua.txt
public static class LocDoBounds
{
    [MenuItem("LOC/Đo bao đồ vật")]
    public static void Run()
    {
        const string F = "LOC_Do.txt"; if (!System.IO.File.Exists(F)) return;
        var sb = new System.Text.StringBuilder();
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var l in System.IO.File.ReadAllLines(F).Select(s => s.Trim()).Where(s => s.Length > 0))
        {
            if (l.StartsWith("lan:"))   // lan:Tiem/Kho:-0.2 → renderer dưới nhánh có max.x > ngưỡng
            {
                var p = l.Split(':'); float lim = float.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture);
                var g = GameObject.Find("NhaLoc/" + p[1]); if (!g) { sb.AppendLine("không thấy " + p[1]); continue; }
                foreach (var r in g.GetComponentsInChildren<Renderer>()) if (r.bounds.max.x > lim) sb.AppendLine($"LẤN {r.name} [{r.transform.parent?.name}] max.x {r.bounds.max.x:0.000}  c ({r.bounds.center.x:0.00},{r.bounds.center.y:0.00},{r.bounds.center.z:0.00})");
                continue;
            }
            if (l.StartsWith("con:"))   // con:Ten → mọi renderer con của món đầu tiên trùng tên
            {
                var t0 = all.FirstOrDefault(t => t.name == l.Substring(4)); if (!t0) continue;
                foreach (var r in t0.GetComponentsInChildren<Renderer>()) sb.AppendLine($"  {r.name} min ({r.bounds.min.x:0.000},{r.bounds.min.y:0.000},{r.bounds.min.z:0.000}) max ({r.bounds.max.x:0.000},{r.bounds.max.y:0.000},{r.bounds.max.z:0.000})");
                continue;
            }
            foreach (var t in all.Where(t => t.name == l || t.name.StartsWith(l + " ")).Take(4))
            {
                var rs = t.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) continue;
                var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
                sb.AppendLine($"{t.name} [{t.parent?.name}] pos ({t.position.x:0.000},{t.position.y:0.000},{t.position.z:0.000}) yaw {t.eulerAngles.y:0}  min ({b.min.x:0.000},{b.min.y:0.000},{b.min.z:0.000}) max ({b.max.x:0.000},{b.max.y:0.000},{b.max.z:0.000}) size ({b.size.x:0.000},{b.size.y:0.000},{b.size.z:0.000})");
            }
        }
        System.IO.File.WriteAllText("LOC_Do_KetQua.txt", sb.ToString()); System.IO.File.Delete(F);
    }
}
