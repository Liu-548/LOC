using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// [22/10] Rà mọi LocDoor: (1) phần dính "tĩnh" (BatchingStatic → bị gộp lúc Play, không quay theo cánh)
//  (2) món nằm trên mặt cánh nhưng KHÔNG là con của cánh (mở cửa thì món đứng yên lơ lửng)
//  (3) bản lề không nằm ở mép cánh  (4) cánh quét xuyên tường/đồ khi mở-đóng.  → Assets/LOC_House/BaoCao_CuaGan.txt
// Kèm "Kê đồ đạc theo phòng" → BaoCao_KeDo.txt (mỗi món LocProp: phòng, kiểu, vị trí, cỡ) để rà bố trí.
public static class LocRaCua
{
    [MenuItem("LOC/Rà cửa + đồ gắn cửa")]
    public static void Run()
    {
        // rà lần lượt từng đêm (bật nhóm DemN, tắt hai đêm kia) vì bàn ghế/đồ mỗi đêm khác nhau
        var nhom = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(x => x.name is "Dem1" or "Dem2" or "Dem3").ToArray();
        var cu = nhom.Select(x => x.gameObject.activeSelf).ToArray();
        var sb = new StringBuilder();
        for (int dem = 1; dem <= 3; dem++)
        {
            foreach (var x in nhom) x.gameObject.SetActive(x.name == "Dem" + dem);
            sb.AppendLine($"═════════ ĐÊM {dem}\n" + Ra(true));
        }
        for (int k = 0; k < nhom.Length; k++) nhom[k].gameObject.SetActive(cu[k]);
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_CuaGan.txt", sb.ToString());
    }

    public static string Ra(bool quet)
    {
        var sb = new StringBuilder($"RÀ CỬA + ĐỒ GẮN CỬA — {System.DateTime.Now:dd/MM HH:mm}\n");
        var doors = Object.FindObjectsByType<LocDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(d => P(d.transform)).ToArray();
        var rends = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(r => r.enabled && !r.GetComponent<TextMesh>()).ToArray();
        var added = new List<MeshCollider>();
        if (quet)
            foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (mf.sharedMesh && mf.sharedMesh.bounds.size.sqrMagnitude > 1e-6f && !mf.GetComponent<Collider>() && mf.GetComponent<Renderer>() && !mf.GetComponentInParent<LocDoor>()) try { added.Add(mf.gameObject.AddComponent<MeshCollider>()); } catch { }
        Physics.SyncTransforms();
        int nS = 0, nG = 0, nB = 0, nX = 0;
        try
        {
            foreach (var d in doors)
            {
                var t = d.transform; var ds = new StringBuilder();
                int st = t.GetComponentsInChildren<Transform>(true).Count(x => GameObjectUtility.AreStaticEditorFlagsSet(x.gameObject, StaticEditorFlags.BatchingStatic));
                if (st > 0) { ds.AppendLine($"   ✗ {st} nút BatchingStatic trong cánh → lúc Play bị gộp, không quay theo cánh"); nS++; }
                var lb = LocSceneAudit.LocalBounds(t, t);
                if (lb.size == Vector3.zero) { sb.AppendLine($"── {P(t)}  (không có mesh)"); continue; }
                // món ngoài cánh nằm trên / xuyên qua mặt cánh — xét cả tư thế dựng sẵn lẫn tư thế còn lại
                {
                    var p0 = t.position; var r0 = t.rotation; var s0 = t.localScale; var lp0 = t.localPosition; var hinge = t.TransformPoint(d.banLe);
                    for (int pose = 0; pose < 2; pose++)
                    {
                        if (pose == 1)
                        {
                            if (d.truot != Vector3.zero) t.localPosition = lp0 + d.truot;
                            else if (d.gapX > 0) t.localScale = new Vector3(s0.x * d.gapX, s0.y, s0.z);
                            else { var q = Quaternion.AngleAxis(d.goc, Vector3.up); t.SetPositionAndRotation(hinge + q * (p0 - hinge), q * r0); }
                        }
                        var lbP = LocSceneAudit.LocalBounds(t, t);
                        foreach (var r in rends)
                        {
                            if (r.transform.IsChildOf(t) || r.GetComponentInParent<LocDoor>()) continue;
                            var b = r.bounds; if (b.size.magnitude > 0.9f) continue;
                            var ib = RB(r, t); if (ib.size.z > 0.15f && ib.min.z < lbP.min.z - 0.15f) { /* món dày nằm sau cánh: chỉ xét nếu thò qua mặt */ }
                            float ox = Mathf.Min(ib.max.x, lbP.max.x) - Mathf.Max(ib.min.x, lbP.min.x), oy = Mathf.Min(ib.max.y, lbP.max.y) - Mathf.Max(ib.min.y, lbP.min.y);
                            if (ox < Mathf.Min(ib.size.x, 0.05f) * 0.6f || oy < Mathf.Min(ib.size.y, 0.05f) * 0.6f) continue;
                            float gap = Mathf.Max(ib.min.z - lbP.max.z, lbP.min.z - ib.max.z);   // < 0 = xuyên vào/qua cánh
                            if (gap > 0.012f) continue;
                            float sau = Mathf.Min(ib.max.z, lbP.max.z) - Mathf.Max(ib.min.z, lbP.min.z);
                            ds.AppendLine($"   ⚠ [{(pose == 0 ? "dựng" : "còn lại")}] {(gap < -0.002f ? $"XUYÊN {sau * 100:0.0} cm" : "sát mặt")}: {P(r.transform)}  cỡ {b.size.x:0.00}×{b.size.y:0.00}×{b.size.z:0.00}"); nG++;
                        }
                    }
                    t.position = p0; t.rotation = r0; t.localScale = s0;
                }
                bool quay = d.truot == Vector3.zero && d.gapX <= 0;
                if (quay)
                {
                    float dx = Mathf.Min(Mathf.Abs(d.banLe.x - lb.min.x), Mathf.Abs(d.banLe.x - lb.max.x));
                    if (dx > 0.06f) { ds.AppendLine($"   ✗ bản lề x={d.banLe.x:0.00} cách mép cánh {dx:0.00} m (cánh x {lb.min.x:0.00}…{lb.max.x:0.00})"); nB++; }
                }
                if (quet && quay && t.gameObject.activeInHierarchy)
                {
                    var bc = t.GetComponent<BoxCollider>();
                    var p0 = t.position; var r0 = t.rotation; var hinge = t.TransformPoint(d.banLe);
                    var bo = new HashSet<Collider>(t.GetComponentsInChildren<Collider>(true));
                    if (d.cungCua != null) foreach (var o in d.cungCua) if (o) foreach (var c in o.GetComponentsInChildren<Collider>(true)) bo.Add(c);
                    var vuong = new Dictionary<string, float>();
                    if (bc)
                        foreach (var f in new[] { 0.25f, 0.5f, 0.75f, 1f })
                        {
                            var q = Quaternion.AngleAxis(d.goc * f, Vector3.up); t.SetPositionAndRotation(hinge + q * (p0 - hinge), q * r0); Physics.SyncTransforms();
                            var half = Vector3.Scale(bc.size, t.lossyScale) / 2 - Vector3.one * 0.01f; half = Vector3.Max(half, Vector3.one * 0.005f);
                            foreach (var o in Physics.OverlapBox(t.TransformPoint(bc.center), half, t.rotation, ~0, QueryTriggerInteraction.Ignore))
                            {
                                if (bo.Contains(o) || o.GetComponentInParent<LocDoor>() || o.GetComponentInParent<CharacterController>() || o.name == "NguoiChoi") continue;
                                if (!Physics.ComputePenetration(bc, t.position, t.rotation, o, o.transform.position, o.transform.rotation, out _, out var dd) || dd < 0.02f) continue;
                                var k = P(o.transform); if (!vuong.TryGetValue(k, out var v) || v < dd) vuong[k] = dd;
                            }
                        }
                    t.SetPositionAndRotation(p0, r0); Physics.SyncTransforms();
                    foreach (var kv in vuong.OrderByDescending(x => x.Value)) { ds.AppendLine($"   ✗ quét xuyên {kv.Value * 100:0} cm: {kv.Key}"); nX++; }
                }
                sb.AppendLine($"── {P(t)}  «{d.ten}» goc {d.goc:0} {(d.dungSanLaMo ? "dựng mở" : "dựng đóng")}{(d.truot != Vector3.zero ? " trượt" : "")}{(d.gapX > 0 ? " gập" : "")}{(t.gameObject.activeInHierarchy ? "" : " [đang ẩn]")}");
                sb.Append(ds.Length > 0 ? ds.ToString() : "   OK\n");
            }
        }
        finally { foreach (var c in added) if (c) Object.DestroyImmediate(c); Physics.SyncTransforms(); }
        sb.Insert(0, $"TỔNG {doors.Length} cánh: {nS} cánh có phần tĩnh · {nG} món nằm trên cánh mà không gắn · {nB} bản lề lệch · {nX} chỗ quét xuyên\n");
        return sb.ToString();
    }

    [MenuItem("LOC/Kê đồ đạc theo phòng")]
    public static void KeDo()
    {
        var hidden = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => (t.name is "Dem2" or "Dem3") && !t.gameObject.activeSelf).ToList();
        foreach (var h in hidden) h.gameObject.SetActive(true);
        var sb = new StringBuilder("KÊ ĐỒ (LocProp): đường dẫn | kiểu | tâm x,z | đáy y | cỡ x×y×z | [cất]=đang ẩn\n");
        foreach (var p in Object.FindObjectsByType<LocProp>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(p => P(p.transform)))
        {
            if (p.GetComponentsInParent<LocProp>(true).Length > 1) continue;
            var b = LocSceneAudit.WB(p.gameObject);
            sb.AppendLine($"{P(p.transform)} | {p.kieu} | {b.center.x:0.00},{b.center.z:0.00} | {b.min.y:0.00} | {b.size.x:0.00}×{b.size.y:0.00}×{b.size.z:0.00}{(p.gameObject.activeSelf ? "" : " [cất]")}");
        }
        foreach (var h in hidden) h.gameObject.SetActive(false);
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_KeDo.txt", sb.ToString());
    }

    static Bounds RB(Renderer r, Transform frame)
    {
        var mf = r.GetComponent<MeshFilter>(); var mb = mf && mf.sharedMesh ? mf.sharedMesh.bounds : new Bounds(r.transform.InverseTransformPoint(r.bounds.center), Vector3.zero);
        var m = frame.worldToLocalMatrix * r.transform.localToWorldMatrix; var o = new Bounds(m.MultiplyPoint3x4(mb.center), Vector3.zero);
        for (int i = 0; i < 8; i++) o.Encapsulate(m.MultiplyPoint3x4(mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
        return o;
    }

    // LOC_In.txt: mỗi dòng một tên (hoặc đường dẫn con) → in toạ độ thế giới, góc, bao, cha của mọi nút trùng tên vào LOC_In_KetQua.txt
    [MenuItem("LOC/In toạ độ theo tên")]
    public static void InToaDo()
    {
        if (!System.IO.File.Exists("LOC_In.txt")) return;
        var sb = new StringBuilder();
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var ten in System.IO.File.ReadAllLines("LOC_In.txt").Select(x => x.Trim()).Where(x => x.Length > 0))
            foreach (var t in all.Where(x => x.name == ten || x.name.StartsWith(ten + "*")))
            {
                var b = LocSceneAudit.WB(t.gameObject);
                sb.AppendLine($"{P(t)}  pos ({t.position.x:0.000},{t.position.y:0.000},{t.position.z:0.000})  rot {t.eulerAngles.y:0.0}/{t.eulerAngles.x:0.0}/{t.eulerAngles.z:0.0}  bao tâm ({b.center.x:0.00},{b.center.y:0.00},{b.center.z:0.00}) cỡ ({b.size.x:0.00},{b.size.y:0.00},{b.size.z:0.00}){(t.GetComponent<LocDoor>() is LocDoor d ? $"  [LocDoor goc {d.goc} banLe {d.banLe} → {t.TransformPoint(d.banLe)}]" : "")}");
            }
        System.IO.File.WriteAllText("LOC_In_KetQua.txt", sb.ToString());
        System.IO.File.Delete("LOC_In.txt");
    }

    static string P(Transform t) { var s = t.name; while (t.parent && t.parent.name != "NhaLoc") { t = t.parent; s = t.name + "/" + s; } return s; }
}
