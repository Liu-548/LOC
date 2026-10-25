using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// [6/10] Tìm vật chắn lối đi cầu thang: với mỗi vế thang (…_Bac01, …_Bac02…) đặt "thân người chơi" (capsule bán kính 0,25 · cao 1,65 như NguoiChoi)
// lên từng bậc + chân/đỉnh thang, báo mọi collider khác (không phải bậc cùng vế) đang đè vào capsule.
// Menu: LOC → Kiểm tra lối đi cầu thang · kết quả: Console + Assets/LOC_House/BaoCao_ThangDi.txt
public static class LocKiemThang
{
    const float R = 0.25f, H = 1.65f;
    static readonly Regex Re = new Regex(@"^(.+)_Bac(\d+)$");

    [MenuItem("LOC/Kiểm tra lối đi cầu thang")]
    public static void Run()
    {
        Physics.SyncTransforms();
        var sb = new StringBuilder("KIỂM TRA LỐI ĐI CẦU THANG (capsule R=0,25 · cao 1,65)\n");
        var flights = new Dictionary<string, List<(int idx, Collider col)>>();
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            var m = Re.Match(c.gameObject.name);
            if (!m.Success) continue;
            string key = m.Groups[1].Value;
            if (!flights.TryGetValue(key, out var l)) flights[key] = l = new List<(int, Collider)>();
            l.Add((int.Parse(m.Groups[2].Value), c));
        }
        if (flights.Count == 0) { Debug.LogWarning("[LOC] Không thấy bậc thang nào (…_Bac01). Mở scene LOC_NhaLoc rồi chạy lại."); return; }

        var tong = new Dictionary<string, int>();
        foreach (var kv in flights.OrderBy(k => k.Key))
        {
            var steps = kv.Value.OrderBy(s => s.idx).ToList();
            var own = new HashSet<Collider>(steps.Select(s => s.col));
            sb.AppendLine($"\n== {kv.Key}  ({steps.Count} bậc)");
            var first = steps.First().col.bounds; var last = steps.Last().col.bounds;
            Vector3 dirv = new Vector3(last.center.x - first.center.x, 0, last.center.z - first.center.z);
            if (dirv.sqrMagnitude < 1e-4f) dirv = Vector3.forward; dirv.Normalize();
            var pts = new List<(string tag, Vector3 foot)>();
            pts.Add(("chân thang −0,9 m", new Vector3(first.center.x, GroundBelow(first.center - dirv * 0.9f, own, first.max.y), first.center.z) - dirv * 0.9f));
            pts.Add(("chân thang −0,4 m", new Vector3(first.center.x, GroundBelow(first.center - dirv * 0.4f, own, first.max.y), first.center.z) - dirv * 0.4f));
            foreach (var s in steps) pts.Add(($"bậc {s.idx:00}", new Vector3(s.col.bounds.center.x, s.col.bounds.max.y, s.col.bounds.center.z)));
            pts.Add(("đỉnh thang +0,4 m", new Vector3(last.center.x, last.max.y, last.center.z) + dirv * 0.5f));
            pts.Add(("đỉnh thang +0,9 m", new Vector3(last.center.x, last.max.y, last.center.z) + dirv * 1.0f));

            foreach (var (tag, foot) in pts)
            {
                var p0 = foot + Vector3.up * (R + 0.03f); var p1 = foot + Vector3.up * (H - R);
                var hits = Physics.OverlapCapsule(p0, p1, R * 0.98f, ~0, QueryTriggerInteraction.Ignore)
                    .Where(c => !own.Contains(c) && !Re.IsMatch(c.gameObject.name)).Distinct().ToList();
                if (hits.Count == 0) continue;
                foreach (var h in hits)
                {
                    string name = Path(h.transform);
                    sb.AppendLine($"  ✗ {tag} (x {foot.x:0.00} · y {foot.y:0.00} · z {foot.z:0.00}) bị chắn bởi: {name}  [{h.GetType().Name}]");
                    tong[name] = tong.TryGetValue(name, out var n) ? n + 1 : 1;
                }
            }
        }
        sb.AppendLine("\n== TỔNG HỢP vật chắn (tên · số điểm bị chắn)");
        if (tong.Count == 0) sb.AppendLine("  (không có vật nào đè vào lối đi — nguyên nhân khác: xem BaoCao_ThangDi và thông số CharacterController)");
        foreach (var kv in tong.OrderByDescending(k => k.Value)) sb.AppendLine($"  {kv.Value,3} × {kv.Key}");
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_ThangDi.txt", sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[LOC] Kiểm tra cầu thang xong: " + (tong.Count == 0 ? "không có vật chắn" : tong.Count + " vật chắn") + " — xem Assets/LOC_House/BaoCao_ThangDi.txt\n" + sb);
    }

    static float GroundBelow(Vector3 p, HashSet<Collider> own, float yMax)
    {
        if (Physics.Raycast(new Vector3(p.x, yMax + 0.5f, p.z), Vector3.down, out var hit, 6f, ~0, QueryTriggerInteraction.Ignore)) return hit.point.y;
        return yMax;
    }

    static string Path(Transform t)
    {
        var s = t.name; int n = 0;
        while (t.parent != null && n++ < 4) { t = t.parent; s = t.name + "/" + s; }
        return s;
    }
}
