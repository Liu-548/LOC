using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// [1/11] chẩn đoán: LOC_Cay.txt mỗi dòng một tên → LOC_Cay_KetQua.txt: cây con (sâu ≤ 4) + bao thế giới, collider, vật liệu, bật/tắt
public static class LocCay
{
    [MenuItem("LOC/In cây con theo tên")]
    public static void Run()
    {
        if (!System.IO.File.Exists("LOC_Cay.txt")) return;
        var sb = new StringBuilder();
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var ten in System.IO.File.ReadAllLines("LOC_Cay.txt").Select(x => x.Trim()).Where(x => x.Length > 0))
            foreach (var t in all.Where(x => x.name == ten).Take(3)) In(t, 0, sb);
        System.IO.File.WriteAllText("LOC_Cay_KetQua.txt", sb.ToString());
        System.IO.File.Delete("LOC_Cay.txt");
    }
    // LOC_Hop.txt: "ten x0 y0 z0 x1 y1 z1" → mọi renderer có bao giao hộp đó (đường dẫn, bao, vật liệu, collider)
    [MenuItem("LOC/Kê vật trong hộp")]
    public static void Hop()
    {
        if (!System.IO.File.Exists("LOC_Hop.txt")) return;
        var sb = new StringBuilder(); var ci = System.Globalization.CultureInfo.InvariantCulture;
        var rs = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var line in System.IO.File.ReadAllLines("LOC_Hop.txt"))
        {
            var p = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries); if (p.Length < 7) continue;
            float f(int i) => float.Parse(p[i], ci);
            var hop = new Bounds(); hop.SetMinMax(new Vector3(f(1), f(2), f(3)), new Vector3(f(4), f(5), f(6)));
            sb.AppendLine("== " + p[0]);
            foreach (var r in rs.Where(r => r.bounds.Intersects(hop)).OrderBy(r => Duong(r.transform)))
            {
                var b = r.bounds; var c = r.GetComponentInParent<Collider>();
                sb.AppendLine($"  {Duong(r.transform)}  [{b.min.x:0.00},{b.min.y:0.00},{b.min.z:0.00} → {b.max.x:0.00},{b.max.y:0.00},{b.max.z:0.00}]  {string.Join(",", r.sharedMaterials.Select(m => m ? m.name + (m.HasProperty("_Surface") && m.GetFloat("_Surface") > 0.5f ? "(trong)" : "") : "null"))}{(c ? "  C" : "")}");
            }
        }
        // lối cửa chính: quét khối người chơi (r 0,25 · cao 1,65) từ sân vào phòng khách ở ba làn
        sb.AppendLine("== loi cua chinh (CapsuleCastAll z −1,6 → +1,2)");
        Physics.SyncTransforms();
        foreach (var x in new[] { 3.4f, 3.8f, 4.2f })
        {
            var a = new Vector3(x, -0.15f + 0.3f, -1.6f); var b2 = a + Vector3.up * 1.1f;
            foreach (var h in Physics.CapsuleCastAll(a, b2, 0.25f, Vector3.forward, 2.8f, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
                sb.AppendLine($"  x {x}: {Duong(h.collider.transform)}  ở z {h.point.z:0.00} y {h.point.y:0.00}");
        }
        System.IO.File.WriteAllText("LOC_Hop_KetQua.txt", sb.ToString());
        System.IO.File.Delete("LOC_Hop.txt");
    }
    // LOC_Vung.txt: "ten x0 y0 z0 x1 y1 z1" → với mọi lưới dưới nút tên đó: số tam giác có tâm trong hộp, theo vật liệu + hướng pháp tuyến
    [MenuItem("LOC/Kê tam giác trong vùng")]
    public static void Vung()
    {
        if (!System.IO.File.Exists("LOC_Vung.txt")) return;
        var sb = new StringBuilder(); var ci = System.Globalization.CultureInfo.InvariantCulture;
        foreach (var line in System.IO.File.ReadAllLines("LOC_Vung.txt"))
        {
            var p = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries); if (p.Length < 7) continue;
            float f(int i) => float.Parse(p[i], ci);
            var hop = new Bounds(); hop.SetMinMax(new Vector3(f(1), f(2), f(3)), new Vector3(f(4), f(5), f(6)));
            foreach (var g in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => t.name == p[0]))
                foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true))
                {
                    var m = mf.sharedMesh; if (!m) continue; var v = m.vertices; var T = mf.transform; var ms = mf.GetComponent<MeshRenderer>()?.sharedMaterials;
                    for (int s = 0; s < m.subMeshCount; s++)
                    {
                        var t = m.GetTriangles(s); var dem = new Dictionary<string, int>();
                        for (int i = 0; i + 2 < t.Length; i += 3)
                        {
                            Vector3 a = T.TransformPoint(v[t[i]]), b = T.TransformPoint(v[t[i + 1]]), c = T.TransformPoint(v[t[i + 2]]);
                            if (!hop.Contains((a + b + c) / 3)) continue;
                            var n = Vector3.Cross(b - a, c - a).normalized; var k = $"({Mathf.Round(n.x)},{Mathf.Round(n.y)},{Mathf.Round(n.z)})";
                            dem[k] = dem.TryGetValue(k, out var x) ? x + 1 : 1;
                        }
                        if (dem.Count > 0) sb.AppendLine($"{Duong(mf.transform)} sub{s} {(ms != null && s < ms.Length && ms[s] ? ms[s].name : "?")}: {string.Join(" ", dem.Select(kv => kv.Key + "×" + kv.Value))}");
                    }
                }
        }
        System.IO.File.WriteAllText("LOC_Vung_KetQua.txt", sb.ToString()); System.IO.File.Delete("LOC_Vung.txt");
    }
    static string Duong(Transform t) { var s = t.name; while (t.parent && t.parent.name != "NhaLoc") { t = t.parent; s = t.name + "/" + s; } return s; }

    static void In(Transform t, int d, StringBuilder sb)
    {
        var r = t.GetComponent<Renderer>(); var c = t.GetComponent<Collider>();
        string s = new string(' ', d * 2) + t.name + (t.gameObject.activeSelf ? "" : " [TẮT]");
        if (d == 0) { var p = t.parent; string path = ""; while (p) { path = p.name + "/" + path; p = p.parent; } s += $"  <{path}>  pos ({t.position.x:0.000},{t.position.y:0.000},{t.position.z:0.000}) rotY {t.eulerAngles.y:0.0} scale {t.lossyScale}"; }
        if (r) { var b = r.bounds; s += $"  R[{b.min.x:0.00},{b.min.y:0.00},{b.min.z:0.00} → {b.max.x:0.00},{b.max.y:0.00},{b.max.z:0.00}] {string.Join(",", r.sharedMaterials.Select(m => m ? m.name : "null"))}{(r.enabled ? "" : " (tắt R)")}"; }
        if (c) { var b = c.bounds; s += $"  C:{c.GetType().Name}[{b.min.x:0.00},{b.min.y:0.00},{b.min.z:0.00} → {b.max.x:0.00},{b.max.y:0.00},{b.max.z:0.00}]{(c.isTrigger ? " trig" : "")}"; }
        foreach (var mb in t.GetComponents<MonoBehaviour>()) if (mb) s += " +" + mb.GetType().Name;
        sb.AppendLine(s);
        if (d < 4) foreach (Transform ch in t) In(ch, d + 1, sb);
        else if (t.childCount > 0) sb.AppendLine(new string(' ', d * 2 + 2) + $"… {t.childCount} con");
    }
}
