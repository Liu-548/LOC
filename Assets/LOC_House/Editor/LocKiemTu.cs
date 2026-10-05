using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// [2/10 tối] quét cửa tủ: cánh quét qua tường/đồ khác khi mở, cánh bị chắn tia nhìn (E), tủ còn trống.
public static class LocKiemTu
{
    static string Ten(Transform t)
    {
        var s = t.name; if (t.parent) s = t.parent.name + "/" + s; return s;
    }

    // hướng ra trước của tủ (ngang): trục mỏng nhất của hộp va chạm cánh đang ở tư thế ĐÓNG, chiều hướng ra khỏi tâm tủ
    public static Vector3 Front(Transform tu)
    {
        var rs = tu.GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        var ds = tu.GetComponentsInChildren<LocDoor>(true); var vote = Vector3.zero;
        foreach (var d in ds.OrderBy(x => x.dungSanLaMo ? 1 : 0))
        {
            if (d.dungSanLaMo && vote != Vector3.zero) continue;
            var bc = d.GetComponent<BoxCollider>(); if (!bc) continue;
            var sc = Vector3.Scale(bc.size, d.transform.lossyScale); sc = new Vector3(Mathf.Abs(sc.x), Mathf.Abs(sc.y), Mathf.Abs(sc.z));
            int ax = sc.x <= sc.y && sc.x <= sc.z ? 0 : sc.y <= sc.z ? 1 : 2; if (ax == 1) continue;
            var dir = d.transform.TransformDirection(ax == 0 ? Vector3.right : Vector3.forward); dir.y = 0; dir.Normalize();
            var ctr = d.transform.TransformPoint(bc.center) - b.center; ctr.y = 0;
            vote += dir * (Vector3.Dot(dir, ctr) >= 0 ? 1 : -1);
        }
        if (vote == Vector3.zero) vote = tu.forward; vote.y = 0; return vote.normalized;
    }

    [MenuItem("LOC/Kiểm tra cửa tủ")]
    public static void Run()
    {
        var sb = new StringBuilder($"KIỂM TRA CỬA TỦ — {System.DateTime.Now:dd/MM HH:mm}\n");
        var names = new HashSet<string>(LocTuMo.Bang.Select(t => t.ten)); names.Add("BanCanhGiuong_Thuoc"); names.Add("BanThoThanTai");
        var props = Object.FindObjectsByType<LocProp>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(p => names.Contains(p.name)).ToList();

        var added = new List<MeshCollider>();
        int real = 0;
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) real++;
        foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!mf.sharedMesh || mf.GetComponent<Collider>()) continue;
            var r = mf.GetComponent<Renderer>(); if (!r || !r.enabled) continue;
            added.Add(mf.gameObject.AddComponent<MeshCollider>());
        }
        var addedSet = new HashSet<Collider>(added);
        Physics.SyncTransforms();
        sb.AppendLine($"collider thật trong scene: {real}; collider tạm thêm để quét: {added.Count}\n");
        int nLoi = 0;
        try
        {
            foreach (var tu in props.OrderBy(p => p.name))
            {
                var rs = tu.GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
                var doors = tu.GetComponentsInChildren<LocDoor>(true);
                var hocs = tu.GetComponentsInChildren<Transform>(true).Where(x => x.name == "HocTu").ToList();
                sb.AppendLine($"== {tu.name}  tâm ({b.center.x:0.00},{b.center.y:0.00},{b.center.z:0.00}) cỡ ({b.size.x:0.00}×{b.size.y:0.00}×{b.size.z:0.00}) cánh={doors.Length} hộc={hocs.Count}");
                var own = new HashSet<Collider>(tu.GetComponentsInChildren<Collider>(true));
                foreach (var d in doors)
                {
                    var tr = d.transform; var bc = d.GetComponent<BoxCollider>();
                    var lp = tr.localPosition; var lr = tr.localRotation; var ls = tr.localScale;
                    var p0 = tr.position; var r0 = tr.rotation; var hinge = tr.TransformPoint(d.banLe);
                    var leafCols = new HashSet<Collider>(tr.GetComponentsInChildren<Collider>(true));
                    var loi = new List<string>();
                    // thân tủ (không phải cánh, hộc, đợt, lòng ngăn)
                    bool Than(Collider c) => own.Contains(c) && !leafCols.Contains(c) && !(c.name == "HocTu" || c.name == "DotTu" || c.name.StartsWith("Ngan_") || c.name == "BanCanh" || c.name.StartsWith("MatCanh"))
                                            && !(c.GetComponentInParent<LocDoor>() is LocDoor od && od != d && c.transform != tu.transform);
                    for (int s = 1; s <= 5; s++)
                    {
                        float t = s / 5f; float e = Mathf.SmoothStep(0, 1, t);
                        if (d.truot != Vector3.zero) tr.localPosition = lp + d.truot * e;
                        else { var q = Quaternion.AngleAxis(d.goc * e, Vector3.up); tr.SetPositionAndRotation(hinge + q * (p0 - hinge), q * r0); }
                        Physics.SyncTransforms();
                        if (!bc) continue;
                        var half = Vector3.Scale(bc.size, tr.lossyScale) / 2; var m = new Vector3(Mathf.Min(0.006f, half.x * 0.4f), Mathf.Min(0.006f, half.y * 0.4f), Mathf.Min(0.006f, half.z * 0.4f));
                        foreach (var o in Physics.OverlapBox(tr.TransformPoint(bc.center), half - m, tr.rotation, ~0, QueryTriggerInteraction.Ignore))
                        {
                            if (leafCols.Contains(o)) continue;
                            if (own.Contains(o) && !Than(o)) continue;
                            if (o.GetComponentInParent<CharacterController>()) continue;
                            string kind = own.Contains(o) ? "THÂN TỦ" : addedSet.Contains(o) ? "đồ" : "KẾT CẤU";
                            var s2 = $"{kind}:{Ten(o.transform)}";
                            if (!loi.Contains(s2)) loi.Add($"{s2}@{t:0.0}");
                        }
                    }
                    tr.localPosition = lp; tr.localRotation = lr; tr.localScale = ls; Physics.SyncTransforms();
                    // tia nhìn: đứng cách 0,9 m phía trước (theo trục mỏng của cánh), ngang tâm cánh, trạng thái dựng sẵn
                    string tia = "ok";
                    if (bc)
                    {
                        var dir = Front(tu.transform); var ctr = tr.TransformPoint(bc.center);
                        var hits = Physics.RaycastAll(ctr + dir * 0.9f, -dir, 1.3f, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance).ToList();
                        var first = hits.FirstOrDefault(h => !h.collider.GetComponentInParent<CharacterController>() && !addedSet.Contains(h.collider));
                        if (first.collider == null) tia = "KHÔNG trúng gì";
                        else if (!first.collider.GetComponentInParent<LocDoor>()) tia = $"BỊ CHẮN bởi collider THẬT {Ten(first.collider.transform)}";
                        else if (first.collider.GetComponentInParent<LocDoor>() != d) tia = $"trúng cánh khác {first.collider.GetComponentInParent<LocDoor>().name}";
                        var fd = hits.FirstOrDefault(h => addedSet.Contains(h.collider) && !h.collider.transform.IsChildOf(tu.transform));
                        if (fd.collider != null && fd.distance < 0.85f && tia == "ok") tia = $"ok (đồ {Ten(fd.collider.transform)} đặt sát trước, không có collider)";
                    }
                    // bản lề + hướng mở (Quay): bản lề phải nằm ở mép cánh VÀ ở mép ngoài tủ; mở xong cánh không được chui vào hộc
                    string banLe = "";
                    if (d.truot == Vector3.zero && d.gapX <= 0f && bc && !d.dungSanLaMo)
                    {
                        var rr = tr.GetComponentsInChildren<Renderer>(); var lb = rr[0].bounds; foreach (var r in rr) lb.Encapsulate(r.bounds);
                        var ed = Front(tu.transform); var ngang = Vector3.Cross(Vector3.up, ed);
                        float h0 = Vector3.Dot(hinge, ngang), cMin = Vector3.Dot(lb.center, ngang) - Mathf.Abs(Vector3.Dot(lb.extents, ngang)), cMax = cMin + 2 * Mathf.Abs(Vector3.Dot(lb.extents, ngang));
                        float tMin = float.MaxValue, tMax = float.MinValue; foreach (var r in tu.GetComponentsInChildren<Renderer>()) { float c0 = Vector3.Dot(r.bounds.center, ngang), e0 = Mathf.Abs(Vector3.Dot(r.bounds.extents, ngang)); tMin = Mathf.Min(tMin, c0 - e0); tMax = Mathf.Max(tMax, c0 + e0); }
                        float dMep = Mathf.Min(Mathf.Abs(h0 - cMin), Mathf.Abs(h0 - cMax)), dNgoai = Mathf.Min(Mathf.Abs(h0 - tMin), Mathf.Abs(h0 - tMax));
                        if (dMep > 0.05f) banLe += $" [BẢN LỀ lệch mép cánh {dMep:0.00} m]";
                        if (dNgoai > 0.12f) banLe += $" [BẢN LỀ không ở mép ngoài tủ ({dNgoai:0.00} m từ mép)]";
                        // trạng thái mở hoàn toàn: cánh có chui vào hộc / lệch vào trong thân không
                        var q2 = Quaternion.AngleAxis(d.goc, Vector3.up); tr.SetPositionAndRotation(hinge + q2 * (p0 - hinge), q2 * r0);
                        var ob = new Bounds(); bool f0 = true; foreach (var r in tr.GetComponentsInChildren<Renderer>()) { if (f0) { ob = r.bounds; f0 = false; } else ob.Encapsulate(r.bounds); }
                        var tb = new Bounds(); bool f1 = true; foreach (var r in tu.GetComponentsInChildren<Renderer>()) { if (r.transform.IsChildOf(tr)) continue; if (f1) { tb = r.bounds; f1 = false; } else tb.Encapsulate(r.bounds); }
                        float truocMat = Vector3.Dot(tb.center, ed) + Mathf.Abs(Vector3.Dot(tb.extents, ed));
                        float sauNhat = Vector3.Dot(ob.center, ed) - Mathf.Abs(Vector3.Dot(ob.extents, ed));
                        if (sauNhat < truocMat - 0.30f) banLe += $" [MỞ CHUI VÀO THÂN TỦ: cánh lọt sau mặt trước {truocMat - sauNhat:0.00} m]";
                        tr.localPosition = lp; tr.localRotation = lr; tr.localScale = ls; Physics.SyncTransforms();
                    }
                    if (banLe != "") tia += banLe;
                    bool bad = loi.Any(x => !x.StartsWith("THÂN TỦ")) || (tia != "ok" && !tia.StartsWith("ok ("));
                    if (bad) nLoi++;
                    sb.AppendLine($"   {(bad ? "✗" : "✓")} {d.name} \"{d.ten}\" goc={d.goc:0} truot={d.truot} dangMo={d.dungSanLaMo} tia: {tia}" + (loi.Count > 0 ? "\n        quét: " + string.Join(" | ", loi.Where(x => !x.StartsWith("THÂN TỦ")).Take(8)) + (loi.Any(x => x.StartsWith("THÂN TỦ")) ? " | (+chạm thân tủ/khung kính, bỏ qua)" : "") : ""));
                }
                // đồ nằm trong hộc
                foreach (var h in hocs)
                {
                    var hr = h.GetComponent<Renderer>(); if (!hr) continue; var hb = hr.bounds; hb.Expand(0.02f);
                    var inside = new List<string>();
                    foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                    {
                        if (r.transform.IsChildOf(tu.transform) && (r.name is "HocTu" or "DotTu" or "BanCanh" || r.name.StartsWith("Ngan_"))) continue;
                        if (r.transform.IsChildOf(tu.transform) && r.GetComponent<MeshFilter>() && r.transform.GetComponentInParent<LocDoor>() && !(r.transform.parent && r.transform.parent.name == LocDoTrongTu.Nut)) continue;   // đồ trong ngăn kéo (con của cánh) vẫn tính
                        if (r.transform.IsChildOf(tu.transform) && r.name.StartsWith(tu.name)) continue;
                        if (!hb.Contains(r.bounds.center)) continue;
                        inside.Add(Ten(r.transform));
                    }
                    sb.AppendLine($"   hộc ({h.parent?.name}) tâm ({hb.center.x:0.00},{hb.center.y:0.00},{hb.center.z:0.00}) cỡ {hb.size.x - 0.04f:0.00}×{hb.size.y - 0.04f:0.00}×{hb.size.z - 0.04f:0.00}: " + (inside.Count == 0 ? "TRỐNG" : $"{inside.Count} món ({string.Join(", ", inside.Distinct().Take(6))})"));
                }
            }
        }
        finally { foreach (var c in added) if (c) Object.DestroyImmediate(c); }
        sb.AppendLine($"\nTỔNG cánh có vấn đề: {nLoi}");
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_KiemTu.txt", sb.ToString());
        Debug.Log($"[LOC] Kiểm tra cửa tủ xong: {nLoi} cánh có vấn đề → BaoCao_KiemTu.txt");
    }
}
