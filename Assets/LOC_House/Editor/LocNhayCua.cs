using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// [1/10] nháy (z-fighting) = hai mặt đồng phẳng, cùng hướng, chồng lên nhau → GPU không biết vẽ mặt nào.
// Sua(): quét CẢ NHÀ (và mỗi cửa ở tư thế còn lại), đẩy bên nhỏ hơn ra trước 0,8 mm — vật nhỏ (chi tiết, cánh, bậu, nẹp) thắng mặt lớn.
//   khác renderer → dời transform của renderer nhỏ hơn; cùng 1 mesh → dời đỉnh tam giác nhỏ hơn (mesh sao riêng), trùng đỉnh hẳn → xoá tam giác trùng.
// Run() (menu): báo cáo quanh từng cửa + cầu thang và chụp mỗi cửa 12 góc (6 góc × đóng/mở) ghép 1 ảnh trong LOC_KiemTra/Cua.
public static class LocNhayCua
{
    const string Out = "LOC_KiemTra/Cua";
    const int TW = 480, TH = 300, COLS = 4, ROWS = 3;
    const float LechMax = 0.0004f, ChongMin = 0.002f, Day = 0.0008f;

    struct Tri { public Vector3 a, b, c, n; public float d, area; public int r, sub, ti; }
    struct Cap { public Tri A, B; }

    // ───── quét
    static List<Cap> Find(Bounds? region, IList<MeshRenderer> rends, out List<MeshRenderer> rs)
    {
        var tris = new List<Tri>(); rs = new List<MeshRenderer>(); var dem = new List<string>();
        foreach (var r in rends)
        {
            if (!r || (region.HasValue && !r.bounds.Intersects(region.Value))) continue;
            var mf = r.GetComponent<MeshFilter>(); var m = mf ? mf.sharedMesh : null; if (!m) continue;
            Vector3[] v; try { v = m.vertices; } catch { continue; }
            if (v.Length == 0) continue;
            var M = r.transform.localToWorldMatrix; int ri = rs.Count; rs.Add(r); dem.Add(Dem(r.transform));
            var w = new Vector3[v.Length]; for (int i = 0; i < v.Length; i++) w[i] = M.MultiplyPoint3x4(v[i]);
            for (int s = 0; s < m.subMeshCount; s++)
            {
                if (m.GetTopology(s) != MeshTopology.Triangles) continue;
                var ix = m.GetTriangles(s);
                for (int i = 0; i + 2 < ix.Length; i += 3)
                {
                    Vector3 a = w[ix[i]], b = w[ix[i + 1]], c = w[ix[i + 2]];
                    var cr = Vector3.Cross(b - a, c - a); float ar = cr.magnitude / 2;
                    if (ar < 2e-6f) continue;
                    if (region.HasValue) { var tb = new Bounds(a, Vector3.zero); tb.Encapsulate(b); tb.Encapsulate(c); if (!tb.Intersects(region.Value)) continue; }
                    var n = cr / (2 * ar);
                    tris.Add(new Tri { a = a, b = b, c = c, n = n, d = Vector3.Dot(n, a), area = ar, r = ri, sub = s, ti = i });
                }
            }
        }
        var grid = new Dictionary<(int, int, int, int), List<int>>();
        for (int i = 0; i < tris.Count; i++)
        {
            var n = tris[i].n; var k = (Mathf.RoundToInt(n.x * 40), Mathf.RoundToInt(n.y * 40), Mathf.RoundToInt(n.z * 40), Mathf.RoundToInt(tris[i].d / 0.002f));
            if (!grid.TryGetValue(k, out var l)) grid[k] = l = new List<int>(); l.Add(i);
        }
        var caps = new List<Cap>();
        foreach (var kv in grid)
            for (int dd = 0; dd <= 1; dd++)
            {
                if (!grid.TryGetValue((kv.Key.Item1, kv.Key.Item2, kv.Key.Item3, kv.Key.Item4 + dd), out var l2)) continue;
                var l1 = kv.Value;
                for (int ii = 0; ii < l1.Count; ii++)
                for (int jj = dd == 0 ? ii + 1 : 0; jj < l2.Count; jj++)
                {
                    Tri A = tris[l1[ii]], B = tris[l2[jj]];
                    if (Vector3.Dot(A.n, B.n) < 0.9998f || Mathf.Abs(A.d - B.d) > LechMax) continue;
                    if (dem[A.r] != null && dem[B.r] != null && dem[A.r] != dem[B.r]) continue;   // bản Đêm 1/2/3 chồng chỗ nhau nhưng chỉ bật 1 bản
                    if (Mathf.Abs(Vector3.Dot(A.n, B.b) - A.d) > LechMax || Mathf.Abs(Vector3.Dot(A.n, B.c) - A.d) > LechMax) continue;
                    if (Overlap(A, B)) caps.Add(new Cap { A = A, B = B });
                }
            }
        return caps;
    }

    static string Dem(Transform t)
    {
        for (var x = t; x; x = x.parent) if (x.name is "Dem1" or "Dem2" or "Dem3") return x.name;
        return null;
    }

    static bool Overlap(Tri A, Tri B)
    {
        var t1 = Vector3.Cross(A.n, Mathf.Abs(A.n.y) < 0.9f ? Vector3.up : Vector3.right).normalized; var t2 = Vector3.Cross(A.n, t1);
        var o = A.a;
        Vector2 P(Vector3 p) => new Vector2(Vector3.Dot(p - o, t1), Vector3.Dot(p - o, t2));
        var pa = new[] { P(A.a), P(A.b), P(A.c) }; var pb = new[] { P(B.a), P(B.b), P(B.c) };
        foreach (var poly in new[] { pa, pb })
            for (int i = 0; i < 3; i++)
            {
                var e = poly[(i + 1) % 3] - poly[i]; if (e.sqrMagnitude < 1e-10f) continue; var ax = new Vector2(-e.y, e.x).normalized;
                float a0 = float.MaxValue, a1 = float.MinValue, b0 = float.MaxValue, b1 = float.MinValue;
                foreach (var p in pa) { float s = Vector2.Dot(p, ax); a0 = Mathf.Min(a0, s); a1 = Mathf.Max(a1, s); }
                foreach (var p in pb) { float s = Vector2.Dot(p, ax); b0 = Mathf.Min(b0, s); b1 = Mathf.Max(b1, s); }
                if (Mathf.Min(a1, b1) - Mathf.Max(a0, b0) < ChongMin) return false;
            }
        return true;
    }

    static MeshRenderer[] AllRends() => Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
        .Where(r => r.enabled && r.GetComponent<MeshFilter>() && r.GetComponent<MeshFilter>().sharedMesh && !r.name.StartsWith("_")).ToArray();

    static readonly Dictionary<MeshRenderer, List<Vector3>> doi = new();   // hướng đã dời của từng renderer trong một lần Sua()
    static bool DaDoi(MeshRenderer r, Vector3 n, out Vector3 prev)
    {
        prev = Vector3.zero;
        if (!doi.TryGetValue(r, out var l)) return false;
        foreach (var v in l) if (Mathf.Abs(Vector3.Dot(v, n)) > 0.99f) { prev = v; return true; }
        return false;
    }

    static (int, int, int) Truc(Vector3 n)
    {
        if (n.x < -0.01f || (Mathf.Abs(n.x) <= 0.01f && (n.y < -0.01f || (Mathf.Abs(n.y) <= 0.01f && n.z < 0)))) n = -n;
        return (Mathf.RoundToInt(n.x * 20), Mathf.RoundToInt(n.y * 20), Mathf.RoundToInt(n.z * 20));
    }

    static float Vol(Renderer r) { var s = r.bounds.size; return Mathf.Max(s.x, 0.001f) * Mathf.Max(s.y, 0.001f) * Mathf.Max(s.z, 0.001f); }

    // ───── sửa
    static int FixCaps(List<Cap> caps, List<MeshRenderer> rs, StringBuilder log)
    {
        var moved = new HashSet<(int, int, int, int)>();   // renderer + trục đã dời trong lượt này
        var triMove = new Dictionary<int, Dictionary<int, Vector3>>();   // renderer → đỉnh → dịch (world)
        var triDel = new Dictionary<int, HashSet<(int, int)>>();          // renderer → (sub, ti) xoá
        int n = 0;
        foreach (var c in caps)
        {
            var nk = (Mathf.RoundToInt(c.A.n.x * 20), Mathf.RoundToInt(c.A.n.y * 20), Mathf.RoundToInt(c.A.n.z * 20));
            if (c.A.r != c.B.r)
            {
                var ra = rs[c.A.r]; var rb = rs[c.B.r];
                var ax = Truc(c.A.n);
                bool ma = moved.Contains((c.A.r, ax.Item1, ax.Item2, ax.Item3)), mb = moved.Contains((c.B.r, ax.Item1, ax.Item2, ax.Item3));
                if (ma || mb) continue;   // một bên đã dời theo trục này trong lượt này (đã tách, hoặc cả hai cùng dời → để lượt sau)
                // bên nhỏ dời trước; renderer đã dời theo trục này (cùng hoặc ngược hướng) thì nhường bên kia — dời ngược lại sẽ triệt tiêu
                var cands = Vol(ra) <= Vol(rb) ? new[] { c.A.r, c.B.r } : new[] { c.B.r, c.A.r };
                int lose = -1; var dir = c.A.n;
                foreach (var ci in cands) if (!DaDoi(rs[ci], c.A.n, out _)) { lose = ci; break; }
                if (lose < 0) { lose = cands[0]; DaDoi(rs[lose], c.A.n, out var prev); dir = Vector3.Dot(prev, c.A.n) >= 0 ? c.A.n : -c.A.n; }   // cả hai đã dời: dời tiếp theo hướng cũ
                moved.Add((lose, ax.Item1, ax.Item2, ax.Item3));
                if (!doi.TryGetValue(rs[lose], out var ld)) doi[rs[lose]] = ld = new List<Vector3>();
                ld.Add(dir);
                rs[lose].transform.position += dir * Day;
                log.AppendLine($"  dời {Path(rs[lose].transform)}  {Day * 1000:0.0} mm ({dir.x:0.00},{dir.y:0.00},{dir.z:0.00})  ↔ {Path(rs[lose == c.A.r ? c.B.r : c.A.r].transform)}");
                n++;
            }
            else
            {
                var lose = c.A.area <= c.B.area ? c.A : c.B; var win = lose.ti == c.A.ti && lose.sub == c.A.sub ? c.B : c.A;
                var m = rs[lose.r].GetComponent<MeshFilter>().sharedMesh;
                var li = m.GetTriangles(lose.sub); var wi = m.GetTriangles(win.sub);
                var lv = new[] { li[lose.ti], li[lose.ti + 1], li[lose.ti + 2] }; var wv = new[] { wi[win.ti], wi[win.ti + 1], wi[win.ti + 2] };
                if (lv.Intersect(wv).Count() == 3)
                {
                    if (!triDel.TryGetValue(lose.r, out var del)) triDel[lose.r] = del = new HashSet<(int, int)>();
                    del.Add((lose.sub, lose.ti));
                }
                else
                {
                    if (!triMove.TryGetValue(lose.r, out var mv)) triMove[lose.r] = mv = new Dictionary<int, Vector3>();
                    foreach (var vi in lv) if (!wv.Contains(vi)) mv[vi] = c.A.n * Day;
                }
            }
        }
        foreach (var ri in triMove.Keys.Union(triDel.Keys).Distinct())
        {
            var r = rs[ri]; var mf = r.GetComponent<MeshFilter>();
            var m = Object.Instantiate(mf.sharedMesh); m.name = mf.sharedMesh.name.Replace("(Clone)", "") + "_nhay";
            if (triMove.TryGetValue(ri, out var mv))
            {
                var v = m.vertices;
                foreach (var kv in mv) v[kv.Key] += r.transform.InverseTransformVector(kv.Value);
                m.vertices = v;
            }
            if (triDel.TryGetValue(ri, out var del))
                for (int s = 0; s < m.subMeshCount; s++)
                {
                    var ix = m.GetTriangles(s); var keep = new List<int>(ix.Length);
                    for (int i = 0; i + 2 < ix.Length; i += 3) if (!del.Contains((s, i))) { keep.Add(ix[i]); keep.Add(ix[i + 1]); keep.Add(ix[i + 2]); }
                    m.SetTriangles(keep, s);
                }
            m.RecalculateBounds();
            mf.sharedMesh = m;
            log.AppendLine($"  sửa mesh {Path(r.transform)}: dời {(mv?.Count ?? 0)} đỉnh, xoá {(triDel.TryGetValue(ri, out var dd) ? dd.Count : 0)} tam giác trùng");
            n++;
        }
        return n;
    }

    // gọi từ LocHouseBuilder sau rà soát, trước khi lưu scene
    public static void Sua()
    {
        var log = new StringBuilder("SỬA NHÁY (đẩy bên nhỏ ra trước 0,8 mm)\n"); doi.Clear();
        int tong = 0;
        int truoc = int.MaxValue;
        for (int lan = 1; lan <= 8; lan++)
        {
            var caps = Find(null, AllRends(), out var rs);
            log.AppendLine($"── Lượt {lan} cả nhà: {caps.Count} cặp tam giác");
            if (caps.Count == 0 || caps.Count >= truoc) break;
            truoc = caps.Count;
            tong += FixCaps(caps, rs, log);
        }
        // mỗi cửa ở tư thế còn lại (đang đóng → mở thử, đang mở → đóng thử)
        foreach (var d in Object.FindObjectsByType<LocDoor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            var t = d.transform; var p0 = t.position; var r0 = t.rotation; var s0 = t.localScale; var hinge = t.TransformPoint(d.banLe);
            if (!TryBounds(t, out var b)) continue;
            PoseB(d, p0, r0, s0, hinge); TryBounds(t, out var b2); b.Encapsulate(b2); b.Expand(0.3f);
            for (int lan = 0; lan < 2; lan++)
            {
                var caps = Find(b, AllRends(), out var rs);
                if (caps.Count == 0) break;
                log.AppendLine($"── {t.name} (tư thế còn lại) lượt {lan + 1}: {caps.Count} cặp");
                tong += FixCaps(caps, rs, log);
            }
            t.SetPositionAndRotation(p0, r0); t.localScale = s0;
        }
        var con = Find(null, AllRends(), out _);
        log.AppendLine($"\nCÒN LẠI cả nhà: {con.Count} cặp tam giác");
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_SuaNhay.txt", log.ToString());
        Debug.Log($"[LOC] Sửa nháy: {tong} chỗ, còn {con.Count} cặp — Assets/LOC_House/BaoCao_SuaNhay.txt");
    }

    static void PoseB(LocDoor d, Vector3 p0, Quaternion r0, Vector3 s0, Vector3 hinge)
    {
        var t = d.transform;
        if (d.truot != Vector3.zero) { t.position = p0 + (t.parent ? t.parent.TransformVector(d.truot) : d.truot); return; }
        if (d.gapX > 0) { t.localScale = new Vector3(s0.x * d.gapX, s0.y, s0.z); return; }
        var q = Quaternion.AngleAxis(d.goc, Vector3.up); t.SetPositionAndRotation(hinge + q * (p0 - hinge), q * r0);
    }

    // ───── menu: báo cáo + ảnh từng cửa
    [MenuItem("LOC/Rà nháy + chụp từng cửa")]
    public static void Run()
    {
        System.IO.Directory.CreateDirectory(Out);
        foreach (var f in System.IO.Directory.GetFiles(Out, "*.png")) System.IO.File.Delete(f);
        var rends = AllRends();
        var doors = Object.FindObjectsByType<LocDoor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OrderBy(d => Path(d.transform)).ToArray();
        var sb = new StringBuilder($"RÀ NHÁY — mặt đồng phẳng cùng hướng (lệch ≤ {LechMax * 1000:0.0} mm) chồng nhau ≥ {ChongMin * 1000:0} mm\n{doors.Length} cửa\n\n");

        var go = new GameObject("_CamCua"); var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        cam.fieldOfView = 60; cam.nearClipPlane = 0.02f; cam.farClipPlane = 40;
        var den = go.AddComponent<Light>(); den.type = LightType.Point; den.range = 3; den.intensity = 0.45f; den.color = new Color(1f, 0.95f, 0.85f);
        var rt = new RenderTexture(TW * COLS, TH * ROWS, 24);
        int loi = 0, idx = 0;
        try
        {
            foreach (var d in doors)
            {
                idx++;
                var t = d.transform; var p0 = t.position; var r0 = t.rotation; var s0 = t.localScale;
                var hinge = t.TransformPoint(d.banLe);
                void PA() { t.SetPositionAndRotation(p0, r0); t.localScale = s0; }
                void PB() => PoseB(d, p0, r0, s0, hinge);
                if (!TryBounds(t, out var ba)) continue;
                PB(); TryBounds(t, out var bb); PA();
                var bc = d.dungSanLaMo ? bb : ba;   // tư thế đóng

                string ten = $"{idx:00}_{Safe(t.name)}";
                sb.AppendLine($"── {ten}  ({d.ten})  [{Path(t.parent)}]");
                var region = ba; region.Encapsulate(bb); region.Expand(0.25f);
                int n = Report(region, rends, sb, d.dungSanLaMo ? "mở" : "đóng");
                PB(); n += Report(region, rends, sb, d.dungSanLaMo ? "đóng" : "mở"); PA();
                if (n == 0) sb.AppendLine("   OK"); else loi++;

                var u = bc.center - hinge; u.y = 0; float half = u.magnitude; var un = half > 0.01f ? u / half : t.right;
                var nrm = Vector3.Cross(un, Vector3.up).normalized;
                float y0 = bc.min.y, y1 = bc.max.y, ym = (y0 + y1) / 2;
                Vector3 H(float y) => new Vector3(hinge.x, y, hinge.z);
                Vector3 Lk(float y) { var l = hinge + un * half * 2; return new Vector3(l.x, y, l.z); }
                var c = new Vector3(bc.center.x, ym, bc.center.z);
                var views = new (Vector3 eye, Vector3 at)[]
                {
                    (c + nrm * 1.5f + Vector3.up * 0.2f, c),
                    (c - nrm * 1.5f + Vector3.up * 0.2f, c),
                    (H(ym) + nrm * 0.5f + un * 0.3f, H(ym)),
                    (H(ym) - nrm * 0.5f + un * 0.3f, H(ym)),
                    (H(y1 - 0.2f) + nrm * 0.45f + un * 0.25f, H(y1)),
                    (Lk(ym) + nrm * 0.5f - un * 0.3f, Lk(ym)),
                };
                RenderTexture.active = rt; GL.Clear(true, true, Color.black); RenderTexture.active = null;
                cam.targetTexture = rt;
                for (int pose = 0; pose < 2; pose++)
                {
                    if ((pose == 0) == d.dungSanLaMo) PB(); else PA();   // hàng trên: đóng, hàng dưới: mở
                    for (int v = 0; v < 6; v++)
                    {
                        int k = pose * 6 + v, col = k % COLS, row = k / COLS;
                        cam.rect = new Rect((float)col / COLS, 1f - (row + 1f) / ROWS, 1f / COLS, 1f / ROWS);
                        cam.transform.position = views[v].eye; cam.transform.LookAt(views[v].at);
                        cam.Render();
                    }
                }
                PA();
                cam.targetTexture = null; cam.rect = new Rect(0, 0, 1, 1);
                Save(rt, $"{Out}/{ten}.png");
            }
            foreach (var (nm, b) in new[] {
                ("CauThang_T1_T3", new Bounds(new Vector3(0.95f, 3.3f, 9.2f), new Vector3(2.2f, 7.2f, 3.9f))),
                ("CauThang_Ham", new Bounds(new Vector3(-0.85f, -1.1f, 17.0f), new Vector3(1.4f, 2.8f, 4.4f))) })
            {
                sb.AppendLine($"── {nm}");
                int n = Report(b, rends, sb, "");
                if (n == 0) sb.AppendLine("   OK"); else loi++;
            }
            var all = Find(null, rends, out var rsAll);
            sb.AppendLine($"\n── CẢ NHÀ: {all.Count} cặp tam giác");
            foreach (var g in all.GroupBy(c => (c.A.r, c.B.r)).OrderByDescending(g => g.Sum(c => Mathf.Min(c.A.area, c.B.area))).Take(80))
                sb.AppendLine($"   {Path(rsAll[g.Key.Item1].transform)}  ↔  {(g.Key.Item1 == g.Key.Item2 ? "(trong cùng model)" : Path(rsAll[g.Key.Item2].transform))}   {g.Count()} cặp");
        }
        finally { Object.DestroyImmediate(go); Object.DestroyImmediate(rt); }
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_NhayCua.txt", sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log($"[LOC] Rà nháy: {loi} chỗ có mặt chồng · ảnh {Out} · Assets/LOC_House/BaoCao_NhayCua.txt");
    }

    static int Report(Bounds region, IList<MeshRenderer> rends, StringBuilder sb, string tag)
    {
        var caps = Find(region, rends, out var rs);
        var groups = caps.GroupBy(c => c.A.r <= c.B.r ? (c.A.r, c.B.r) : (c.B.r, c.A.r)).ToList();
        foreach (var g in groups.OrderByDescending(g => g.Sum(c => Mathf.Min(c.A.area, c.B.area))))
            sb.AppendLine($"   [{tag}] {Path(rs[g.Key.Item1].transform)}  ↔  {(g.Key.Item1 == g.Key.Item2 ? "(trong cùng model)" : Path(rs[g.Key.Item2].transform))}   {g.Count()} cặp tam giác");
        return groups.Count;
    }

    static bool TryBounds(Transform t, out Bounds b)
    {
        b = default; bool any = false;
        foreach (var r in t.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any;
    }

    static void Save(RenderTexture rt, string path)
    {
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tex.Apply();
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        RenderTexture.active = null; Object.DestroyImmediate(tex);
    }

    static string Safe(string s) => new string(s.Select(ch => char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_').ToArray());

    static string Path(Transform t)
    {
        if (!t) return "";
        var parts = new List<string>();
        for (var x = t; x && parts.Count < 4; x = x.parent) parts.Insert(0, x.name);
        return string.Join("/", parts);
    }
}
