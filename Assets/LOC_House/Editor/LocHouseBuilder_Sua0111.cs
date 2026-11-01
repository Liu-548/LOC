using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// [1/11] Sửa theo bản kiểm thử Major.docx + ghi chú chơi tay 01/11. Gọi trong Build() sau rà soát, trước rà nháy (LocNhayCua.Sua).
//  Ghi kết quả: Assets/LOC_House/BaoCao_Sua0111.txt
public static partial class LocHouseBuilder
{
    static void Sua0111()
    {
        var log = new StringBuilder("SỬA 01/11 — " + System.DateTime.Now.ToString("dd/MM HH:mm") + "\n");
        void Buoc(string ten, System.Action a) { try { a(); } catch (System.Exception e) { log.AppendLine("LỖI " + ten + ": " + e.Message); Debug.LogException(e); } }
        Buoc("băng vòng hoa", () => SuaBangVongHoa(log));
        Buoc("rạp", () => SuaRap(log));
        Buoc("đèn hầm", () => SuaDenHam(log));
        Buoc("bóng thang hầm", () => BongThangHam(log));
        Buoc("gói hàng", () => SuaGoiHang(log));
        Buoc("gấu bông", () => SuaGauBong(log));
        Buoc("va chạm", () => ThemVaCham(log));
        Buoc("tủ chén", () => LongTuChen(log));
        Buoc("chén hầm", () => ChenHamThat(log));
        Buoc("lưới thép kho", () => LuoiThepKho(log));
        Buoc("gối Nhím", () => GoiNhim(log));
        Buoc("cổng", () => CongKhit(log));
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_Sua0111.txt", log.ToString());
    }

    static IEnumerable<Transform> TatCa(string ten) => root.GetComponentsInChildren<Transform>(true).Where(t => t.name == ten);

    // ── 1. băng chữ vòng hoa: .glb chỉ có chữ giữ chỗ (ô "8888"), UV một mặt nên có bản đọc ngược.
    //   Gán sẵn vật liệu chữ thật cho mọi bộ vòng hoa (cả Đêm 2/3 — trước đây chỉ thay lúc chạy, mà nhóm Đêm 2/3 đang tắt nên bỏ sót),
    //   dựng lại UV theo từng mảnh băng: chữ chạy dọc chiều dài mảnh, đọc xuôi từ phía người nhìn; mảnh một lớp thì thêm lớp mặt sau.
    static void SuaBangVongHoa(StringBuilder log)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LOC_House/Resources/LOC_V33/T_BangChuVongHoa.png");
        Material matMoi = null; int n = 0;
        foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            var ms = mr.sharedMaterials; int sub = System.Array.FindIndex(ms, m => m && m.name.StartsWith("Bang_Chu_Vong_Hoa"));
            var mf = mr.GetComponent<MeshFilter>(); if (sub < 0 || !mf || !mf.sharedMesh) continue;
            if (!matMoi)
            {
                // vật liệu gốc dùng shader glTF (texture tên baseColorTexture) → vật liệu URP Lit riêng, chắc chắn hiện chữ
                var p = $"{MatFolder}/M_BangChuVongHoa.mat";
                AssetDatabase.DeleteAsset(p);
                matMoi = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "M_BangChuVongHoa" };
                matMoi.SetTexture("_BaseMap", tex); matMoi.SetColor("_BaseColor", Color.white); matMoi.SetFloat("_Smoothness", 0.25f); matMoi.SetFloat("_Cull", 2);
                AssetDatabase.CreateAsset(matMoi, p);
            }
            mf.sharedMesh = UVBang(mf.sharedMesh, sub, mr.transform, out int dao);
            ms[sub] = matMoi; mr.sharedMaterials = ms; n++;
            log.AppendLine($"  băng {PathOf(mr.transform)}: {dao} mảnh");
        }
        log.AppendLine($"băng vòng hoa: {n} lưới");
    }

    static Mesh UVBang(Mesh src, int sub, Transform tr, out int soManh)
    {
        var m = Object.Instantiate(src); m.name = src.name + "_Bang";
        var V = new List<Vector3>(); m.GetVertices(V); var N = new List<Vector3>(); m.GetNormals(N);
        var U = new List<Vector2>(); m.GetUVs(0, U); var T = new List<Vector4>(); m.GetTangents(T); var C = new List<Color>(); m.GetColors(C);
        var U1 = new List<Vector2>(); m.GetUVs(1, U1);
        int nv = V.Count; if (U.Count != nv) { U = Enumerable.Repeat(Vector2.zero, nv).ToList(); }
        var tri = m.GetTriangles(sub);
        // hàn đỉnh theo vị trí (glb tách đỉnh theo tam giác)
        var khoa = new Dictionary<Vector3Int, int>(); var han = new int[nv];
        for (int i = 0; i < nv; i++) { var k = Vector3Int.RoundToInt(V[i] * 10000f); if (!khoa.TryGetValue(k, out var h)) khoa[k] = h = i; han[i] = h; }
        int nf = tri.Length / 3;
        var nW = new Vector3[nf]; var dt = new float[nf];
        for (int f = 0; f < nf; f++)
        {
            Vector3 a = tr.TransformPoint(V[tri[3 * f]]), b = tr.TransformPoint(V[tri[3 * f + 1]]), c = tr.TransformPoint(V[tri[3 * f + 2]]);
            var cr = Vector3.Cross(b - a, c - a); dt[f] = cr.magnitude; nW[f] = dt[f] > 1e-12f ? cr / dt[f] : Vector3.up;
        }
        // Unity (cuốn thuận kim đồng hồ là mặt trước): Cross(b−a, c−a) hướng về phía người xem mặt trước
        var canh = new Dictionary<(int, int), List<int>>();
        for (int f = 0; f < nf; f++) for (int e = 0; e < 3; e++)
            {
                int x = han[tri[3 * f + e]], y = han[tri[3 * f + (e + 1) % 3]]; var k = x < y ? (x, y) : (y, x);
                if (!canh.TryGetValue(k, out var l)) canh[k] = l = new List<int>(); l.Add(f);
            }
        var dao = new int[nf]; for (int i = 0; i < nf; i++) dao[i] = -1;
        var manh = new List<List<int>>();
        for (int f = 0; f < nf; f++)
        {
            if (dao[f] >= 0) continue;
            var l = new List<int>(); var st = new Stack<int>(); st.Push(f); dao[f] = manh.Count;
            while (st.Count > 0)
            {
                int g = st.Pop(); l.Add(g);
                for (int e = 0; e < 3; e++)
                {
                    int x = han[tri[3 * g + e]], y = han[tri[3 * g + (e + 1) % 3]];
                    foreach (var h in canh[x < y ? (x, y) : (y, x)]) if (dao[h] < 0 && Vector3.Dot(nW[h], nW[f]) > 0.7f) { dao[h] = manh.Count; st.Push(h); }
                }
            }
            manh.Add(l);
        }
        soManh = manh.Count;
        var nM = manh.Select(l => l.Aggregate(Vector3.zero, (s, f) => s + nW[f] * dt[f]).normalized).ToList();
        var tam = manh.Select(l => l.Aggregate(Vector3.zero, (s, f) => s + tr.TransformPoint(V[tri[3 * f]])) / l.Count).ToList();
        var moi = new List<int>();
        void Them(int i0, Vector3 nw, Vector2 uv, Vector3 dich)
        {
            V.Add(V[i0] + dich); N.Add(tr.InverseTransformDirection(nw).normalized); U.Add(uv);
            if (T.Count > 0) T.Add(T[i0]); if (C.Count > 0) C.Add(C[i0]); if (U1.Count > 0) U1.Add(U1[i0]);
            moi.Add(V.Count - 1);
        }
        for (int k = 0; k < manh.Count; k++)
        {
            var l = manh[k]; var n0 = nM[k];
            bool sinh = Enumerable.Range(0, manh.Count).Any(j => j != k && Vector3.Dot(nM[j], n0) < -0.9f && (tam[j] - tam[k]).magnitude < 0.01f);   // đã có lớp mặt sau
            foreach (var mat in sinh ? new[] { 1 } : new[] { 1, -1 })
            {
                var n = n0 * mat;
                var e1 = Vector3.Cross(Vector3.up, -n); if (e1.sqrMagnitude < 1e-4f) e1 = Vector3.Cross(Vector3.forward, -n); e1.Normalize();
                var e2 = Vector3.Cross(-n, e1).normalized;
                var ps = l.SelectMany(f => new[] { tri[3 * f], tri[3 * f + 1], tri[3 * f + 2] }).Select(i => tr.TransformPoint(V[i])).Select(p => new Vector2(Vector3.Dot(p, e1), Vector3.Dot(p, e2))).ToList();
                var tb = ps.Aggregate(Vector2.zero, (s, p) => s + p) / ps.Count;
                float cxx = 0, cyy = 0, cxy = 0; foreach (var p in ps) { var d = p - tb; cxx += d.x * d.x; cyy += d.y * d.y; cxy += d.x * d.y; }
                float th = 0.5f * Mathf.Atan2(2 * cxy, cxx - cyy);
                var L = new Vector2(Mathf.Cos(th), Mathf.Sin(th));
                if (Mathf.Abs(L.x) >= 0.35f) { if (L.x < 0) L = -L; } else if (L.y > 0) L = -L;   // ngang: trái → phải · dọc: trên → xuống
                var S = new Vector2(-L.y, L.x);
                float l0 = ps.Min(p => Vector2.Dot(p, L)), l1 = ps.Max(p => Vector2.Dot(p, L)), s0 = ps.Min(p => Vector2.Dot(p, S)), s1 = ps.Max(p => Vector2.Dot(p, S));
                var dichLocal = mat < 0 ? tr.InverseTransformVector(-n0 * 0.001f) : Vector3.zero;
                foreach (var f in l)
                {
                    var ids = mat > 0 ? new[] { tri[3 * f], tri[3 * f + 1], tri[3 * f + 2] } : new[] { tri[3 * f], tri[3 * f + 2], tri[3 * f + 1] };
                    foreach (var i in ids)
                    {
                        var w = tr.TransformPoint(V[i]); var q = new Vector2(Vector3.Dot(w, e1), Vector3.Dot(w, e2));
                        Them(i, n, new Vector2((Vector2.Dot(q, L) - l0) / Mathf.Max(1e-5f, l1 - l0), (Vector2.Dot(q, S) - s0) / Mathf.Max(1e-5f, s1 - s0)), dichLocal);
                    }
                }
            }
        }
        m.SetVertices(V); m.SetNormals(N); m.SetUVs(0, U); if (T.Count > 0) m.SetTangents(T); if (C.Count > 0) m.SetColors(C); if (U1.Count > 0) m.SetUVs(1, U1);
        m.SetTriangles(moi, sub); m.RecalculateBounds(); if (T.Count > 0) m.RecalculateTangents();
        return m;
    }

    // ── 2. rạp: vải lưng rạp cắm vào hai cánh cửa chính đang mở ra sân + che lối sang cửa hông tiệm → thu sâu rạp 0,86, lùi mép sau ra z −1,46
    static void SuaRap(StringBuilder log)
    {
        var rap = TatCa("SM_FuneralTent_300").FirstOrDefault(t => t.parent && t.parent.name == "SanTruoc"); if (!rap) { log.AppendLine("rạp: không thấy"); return; }
        var b0 = LocSceneAudit.WB(rap.gameObject);
        var s = rap.localScale; s.z *= 0.86f; rap.localScale = s;
        var b1 = LocSceneAudit.WB(rap.gameObject);
        rap.position += new Vector3(0, 0, -1.46f - b1.max.z);
        var b2 = LocSceneAudit.WB(rap.gameObject);
        log.AppendLine($"rạp: z {b0.min.z:0.00}…{b0.max.z:0.00} → {b2.min.z:0.00}…{b2.max.z:0.00}");
    }

    // ── 3. đèn hầm: xích kéo (dải đốt lơ lửng dưới bóng) còn nằm trong lưới thân đèn → bỏ phần lưới thấp hơn đáy bóng; nguồn sáng dời đúng chỗ bóng
    static void SuaDenHam(StringBuilder log)
    {
        var den = TatCa("DenHam_FullAssembly").FirstOrDefault(); if (!den) return;
        var bong = den.GetComponentsInChildren<Renderer>(true).FirstOrDefault(r => r.name == "BongDayToc");
        var than = den.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(r => r.name == "BongDayToc_than");
        if (!bong || !than) { log.AppendLine("đèn hầm: thiếu bóng/thân"); return; }
        float day = bong.bounds.min.y + 0.005f;
        var m = Object.Instantiate(than.sharedMesh); m.name = than.sharedMesh.name + "_KhongXich";
        var v = m.vertices; int bo = 0;
        for (int s = 0; s < m.subMeshCount; s++)
        {
            var t = m.GetTriangles(s); var giu = new List<int>();
            for (int i = 0; i + 2 < t.Length; i += 3)
            {
                float y = Mathf.Max(than.transform.TransformPoint(v[t[i]]).y, Mathf.Max(than.transform.TransformPoint(v[t[i + 1]]).y, than.transform.TransformPoint(v[t[i + 2]]).y));
                if (y < day) { bo++; continue; }
                giu.Add(t[i]); giu.Add(t[i + 1]); giu.Add(t[i + 2]);
            }
            m.SetTriangles(giu, s);
        }
        m.RecalculateBounds(); than.sharedMesh = m;
        var l = root.GetComponentsInChildren<Light>(true).FirstOrDefault(x => x.name == "Den_BongDayToc_Ham");
        if (l) l.transform.position = bong.bounds.center + Vector3.down * 0.06f;
        log.AppendLine($"đèn hầm: bỏ {bo} tam giác xích dưới bóng (y < {day:0.00}); nguồn sáng → {bong.bounds.center:F2}");
    }

    // ── 4. hai đèn trong hộp thang hầm chỉ có ánh sáng, không có bóng → bóng tròn trần trụi + đui + dây thả từ trần (hầm được phép bóng trần)
    static void BongThangHam(StringBuilder log)
    {
        Physics.SyncTransforms();
        foreach (var ten in new[] { "BongChieu_Ham" })   // [2/11] BongThang_Ham đã bỏ
        {
            var l = root.GetComponentsInChildren<Light>(true).FirstOrDefault(x => x.name == "Den_" + ten); if (!l) continue;
            var p = l.transform.position;
            float tran = Physics.Raycast(p, Vector3.up, out var h, 3f, ~0, QueryTriggerInteraction.Ignore) ? h.point.y : p.y + 3f;
            var g = new GameObject("BongTran_" + ten).transform; g.SetParent(l.transform.parent, false);
            float yb = Mathf.Min(p.y + 0.12f, tran - 0.35f);
            g.position = new Vector3(p.x, yb, p.z);
            // trần xa quá (bóng dưới chân thang: phía trên là ô thang hở lên kho) → treo trên cần sắt ngắn bắt vào tường gần nhất
            Vector3 tuong = Vector3.zero; float gan = 1.4f;
            if (tran - yb > 1.2f)
                foreach (var hg in new[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back })
                    if (Physics.Raycast(g.position, hg, out var hw, gan, ~0, QueryTriggerInteraction.Ignore)) { gan = hw.distance; tuong = hg; }
            void K(string n, PrimitiveType pt, Vector3 tam, Vector3 co, Material mat)
            {
                var k = GameObject.CreatePrimitive(pt); Object.DestroyImmediate(k.GetComponent<Collider>());
                k.name = n; k.transform.SetParent(g, false); k.transform.localPosition = tam; k.transform.localScale = co;
                var r = k.GetComponent<Renderer>(); r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var sang = M("M_BongTranHam_Sang", "#FFE2B0"); sang.EnableKeyword("_EMISSION"); sang.SetColor("_EmissionColor", new Color(1f, 0.6f, 0.24f) * 3f); EditorUtility.SetDirty(sang);
            float dai = tran - yb - 0.05f;
            if (tuong != Vector3.zero)
            {
                K("Can", PrimitiveType.Cube, tuong * (gan / 2) + Vector3.up * 0.12f, new Vector3(Mathf.Abs(tuong.x) * gan + 0.012f, 0.012f, Mathf.Abs(tuong.z) * gan + 0.012f), M("M_DuiDen_Den", "#2A2826"));
                K("Day", PrimitiveType.Cylinder, new Vector3(0, 0.09f, 0), new Vector3(0.006f, 0.04f, 0.006f), M("M_DayDien_Den", "#1E1C1A"));
            }
            else K("Day", PrimitiveType.Cylinder, new Vector3(0, 0.05f + dai / 2, 0), new Vector3(0.006f, dai / 2, 0.006f), M("M_DayDien_Den", "#1E1C1A"));
            K("Dui", PrimitiveType.Cylinder, new Vector3(0, 0.03f, 0), new Vector3(0.032f, 0.025f, 0.032f), M("M_DuiDen_Den", "#2A2826"));
            K("Bong", PrimitiveType.Sphere, new Vector3(0, -0.025f, 0), new Vector3(0.06f, 0.075f, 0.06f), sang);
            l.transform.position = g.position + Vector3.down * 0.1f;
            log.AppendLine($"bóng {ten}: y {yb:0.00}, trần {tran:0.00}");
        }
    }

    // ── 5. gói hàng Đêm 3: model gd6 bị ẩn từng phần còn lại mảnh rời rạc (dây, giấy, hộp lơ lửng) → một gói giấy dầu buộc lạt chữ thập nằm trên nền
    static void SuaGoiHang(StringBuilder log)
    {
        var g = TatCa("VT_GoiHang").FirstOrDefault(); if (!g) return;
        foreach (Transform c in g.Cast<Transform>().ToArray()) Object.DestroyImmediate(c.gameObject);
        foreach (var c in g.GetComponents<Collider>()) Object.DestroyImmediate(c);
        g.SetPositionAndRotation(new Vector3(5.0f, 0f, 0.45f), Quaternion.Euler(0, 15f, 0));
        void K(string n, Vector3 tam, Vector3 co, string hex)
        {
            var k = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(k.GetComponent<Collider>());
            k.name = n; k.transform.SetParent(g, false); k.transform.localPosition = tam; k.transform.localScale = co; k.GetComponent<Renderer>().sharedMaterial = M("M_Goi_" + n, hex);
        }
        K("GiayDau", new Vector3(0, 0.055f, 0), new Vector3(0.34f, 0.11f, 0.25f), "#7A5A34");
        K("Lat_Doc", new Vector3(0.06f, 0.0555f, 0), new Vector3(0.022f, 0.113f, 0.254f), "#C9B27A");
        K("Lat_Ngang", new Vector3(0, 0.0555f, 0.02f), new Vector3(0.344f, 0.113f, 0.022f), "#C9B27A");
        K("Nut", new Vector3(0.06f, 0.114f, 0.02f), new Vector3(0.04f, 0.008f, 0.03f), "#B9A06A");
        K("NhanSon", new Vector3(-0.09f, 0.1115f, -0.06f), new Vector3(0.09f, 0.002f, 0.06f), "#B0302A");
        var bc = g.gameObject.AddComponent<BoxCollider>(); bc.center = new Vector3(0, 0.06f, 0); bc.size = new Vector3(0.4f, 0.14f, 0.32f);
        log.AppendLine("gói hàng: dựng lại gói giấy dầu 34×25×11 cm tại (5,00 · 0 · 0,45)");
    }

    // ── 6. gấu bông: tay/tai là mảnh lưới rời nằm cách thân → kéo mỗi mảnh nhỏ áp vào thân (lún 3 mm)
    static void SuaGauBong(StringBuilder log)
    {
        foreach (var mf in TatCa("GauBong_Cu").SelectMany(t => t.GetComponents<MeshFilter>()).Where(x => x.sharedMesh).Distinct())
        {
            var m = Object.Instantiate(mf.sharedMesh); m.name = mf.sharedMesh.name + "_DinhTay";
            var v = m.vertices; int nv = v.Length;
            var cha = Enumerable.Range(0, nv).ToArray();
            int Goc(int i) { while (cha[i] != i) i = cha[i] = cha[cha[i]]; return i; }
            var khoa = new Dictionary<Vector3Int, int>();
            for (int i = 0; i < nv; i++) { var k = Vector3Int.RoundToInt(v[i] * 10000f); if (khoa.TryGetValue(k, out var h)) cha[Goc(i)] = Goc(h); else khoa[k] = i; }
            for (int s = 0; s < m.subMeshCount; s++) { var t = m.GetTriangles(s); for (int i = 0; i + 2 < t.Length; i += 3) { cha[Goc(t[i + 1])] = Goc(t[i]); cha[Goc(t[i + 2])] = Goc(t[i]); } }
            var nhom = Enumerable.Range(0, nv).GroupBy(Goc).Select(x => x.ToList()).OrderByDescending(x => x.Count).ToList();
            if (nhom.Count < 2) { log.AppendLine("gấu: một mảnh liền, không có tay rời"); continue; }
            // gom dần: mỗi lượt lấy mảnh gần khối đã gom nhất; hở 3 mm–10 cm thì kéo áp vào (lún 3 mm) rồi gom luôn
            var gom = new List<int>(nhom[0]); var con = nhom.Skip(1).ToList(); int doi = 0;
            float tl = mf.transform.lossyScale.x;
            while (con.Count > 0)
            {
                int chon = -1; float best = float.MaxValue; Vector3 a = default, b = default;
                for (int k = 0; k < con.Count; k++)
                    foreach (var i in con[k]) foreach (var j in gom) { float d = (v[i] - v[j]).sqrMagnitude; if (d < best) { best = d; a = v[i]; b = v[j]; chon = k; } }
                var mn = con[chon]; con.RemoveAt(chon);
                float kc = Mathf.Sqrt(best) * tl;
                log.AppendLine($"  gấu: mảnh {mn.Count} đỉnh, tâm {mf.transform.TransformPoint(mn.Aggregate(Vector3.zero, (s2, i) => s2 + v[i]) / mn.Count):F3}, cách {kc * 100:0.0} cm");
                if (kc > 0.003f && kc < 0.10f)
                {
                    var dich = (b - a) * (1f + 0.003f / kc);
                    foreach (var i in mn) v[i] += dich;
                    doi++;
                }
                gom.AddRange(mn);
            }
            m.vertices = v; m.RecalculateBounds(); mf.sharedMesh = m;
            var mc = mf.GetComponent<MeshCollider>(); if (mc) mc.sharedMesh = m;
            log.AppendLine($"gấu bông: {nhom.Count} mảnh, kéo {doi}");
        }
    }

    // ── 7. va chạm: tủ đứng hành lang T2 chỉ có va chạm ở cánh (đi xuyên thân tủ từ góc trái); vòng hoa không có va chạm (camera lọt vào cánh hoa)
    static void ThemVaCham(StringBuilder log)
    {
        foreach (var t in TatCa("TuDung_RuongChanMan"))
        {
            var mf = t.GetComponent<MeshFilter>(); if (!mf || t.GetComponent<Collider>()) continue;
            t.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh; log.AppendLine("va chạm thân tủ đứng hành lang T2");
        }
        foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("VongHoa") && t.GetComponent<LocProp>()))
        {
            if (t.GetComponentInChildren<Collider>(true) || t.GetComponentInParent<Collider>()) continue;
            var lb = LocSceneAudit.LocalBounds(t, t); if (lb.size.sqrMagnitude < 0.01f) continue;
            var bc = t.gameObject.AddComponent<BoxCollider>(); bc.center = lb.center; bc.size = lb.size + new Vector3(0.06f, 0, 0.06f);
            log.AppendLine("va chạm vòng hoa " + PathOf(t));
        }
    }

    // ── 8. tủ chén: ngăn dưới mở ra chỉ thấy hộc trống tối như mảng tường → lót lưng gỗ + bát đĩa, nồi trong hai ngăn
    static void LongTuChen(StringBuilder log)
    {
        var tu = TatCa("TuBuffet_ChenBat").FirstOrDefault(t => t.parent && t.parent.name == "Bep"); if (!tu) return;
        var hoc = tu.GetComponentsInChildren<Renderer>(true).Where(r => r.name == "HocTu").OrderBy(r => r.bounds.min.y).FirstOrDefault(); if (!hoc) return;
        var b = hoc.bounds;
        // mặt trước liền của thân tủ (nằm ngay sau hai cánh dưới) chưa được khoét → mở cánh chỉ thấy tấm phẳng tối như tường → bỏ tam giác mặt đó
        foreach (var mf in tu.GetComponentsInChildren<MeshFilter>(true).Where(x => x.sharedMesh && x.name == "TuBuffet_ChenBat"))
        {
            var m = Object.Instantiate(mf.sharedMesh); m.name = mf.sharedMesh.name + "_KhoetDuoi"; var v = m.vertices; int bo = 0; var T = mf.transform;
            var vung = new Bounds(); vung.SetMinMax(new Vector3(b.min.x - 0.06f, b.min.y - 0.04f, b.min.z - 0.04f), new Vector3(b.min.x + 0.05f, b.max.y + 0.04f, b.max.z + 0.04f));
            var mats = mf.GetComponent<MeshRenderer>().sharedMaterials;
            for (int s = 0; s < m.subMeshCount; s++)
            {
                var t = m.GetTriangles(s); var giu = new List<int>();
                for (int i = 0; i + 2 < t.Length; i += 3)
                {
                    Vector3 a = T.TransformPoint(v[t[i]]), c1 = T.TransformPoint(v[t[i + 1]]), c2 = T.TransformPoint(v[t[i + 2]]);
                    var n = Vector3.Cross(c1 - a, c2 - a).normalized;
                    float dt = Vector3.Cross(c1 - a, c2 - a).magnitude / 2;   // tấm liền che kín ô cánh (tam giác to) — giữ nẹp khung (tam giác mảnh)
                    if (dt > 0.06f && vung.Contains((a + c1 + c2) / 3) && Mathf.Abs(n.x) > 0.9f) { bo++; continue; }
                    giu.Add(t[i]); giu.Add(t[i + 1]); giu.Add(t[i + 2]);
                }
                m.SetTriangles(giu, s);
            }
            mf.sharedMesh = m; var mc = mf.GetComponent<MeshCollider>(); if (mc) mc.sharedMesh = m;
            log.AppendLine($"tủ chén: khoét mặt trước ngăn dưới, bỏ {bo} tam giác");
        }
        log.AppendLine($"tủ chén: hộc dưới {b.min:F2} → {b.max:F2} (đồ trong tủ đã có sẵn từ LocTuMo)");
    }

    // ── 9. cuộn lưới thép trong kho: vật liệu trong suốt pha → nhìn như bóng ma khung dây → đổi sang cắt alpha (dây đặc, lỗ hở)
    static void LuoiThepKho(StringBuilder log)
    {
        Material cat = null; int n = 0;
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            var ms = r.sharedMaterials; bool doi = false;
            for (int i = 0; i < ms.Length; i++)
            {
                if (!ms[i] || !ms[i].name.StartsWith("M_LuoiThep")) continue;
                if (!cat)
                {
                    // vật liệu gốc là shader glTF (không có cờ cắt alpha) → vật liệu URP Lit mới, lấy texture + màu gốc, cắt alpha, hai mặt
                    var src = ms[i]; var p = $"{MatFolder}/M_LuoiThep_Cat.mat";
                    AssetDatabase.DeleteAsset(p);
                    cat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "M_LuoiThep_Cat" };
                    Texture tx = null; foreach (var pr in new[] { "baseColorTexture", "_BaseMap", "_MainTex" }) if (!tx && src.HasProperty(pr)) tx = src.GetTexture(pr);
                    var mau = src.HasProperty("baseColorFactor") ? src.GetColor("baseColorFactor") : src.HasProperty("_BaseColor") ? src.GetColor("_BaseColor") : Color.gray; mau.a = 1;
                    cat.SetTexture("_BaseMap", tx); cat.SetColor("_BaseColor", mau);
                    cat.SetFloat("_AlphaClip", 1); cat.SetFloat("_Cutoff", 0.45f); cat.SetFloat("_Cull", 0); cat.SetFloat("_Smoothness", 0.35f); cat.SetFloat("_Metallic", 0.6f);
                    cat.EnableKeyword("_ALPHATEST_ON"); cat.SetOverrideTag("RenderType", "TransparentCutout"); cat.renderQueue = 2450;
                    AssetDatabase.CreateAsset(cat, p);
                    log.AppendLine($"lưới thép: texture {(tx ? tx.name : "KHÔNG CÓ")} · màu {mau}");
                }
                ms[i] = cat; doi = true;
            }
            if (doi) { r.sharedMaterials = ms; n++; }
        }
        log.AppendLine($"lưới thép kho: {n} renderer sang cắt alpha");
    }

    // ── 10. Nhím nằm không có gối → gối vỏ hoa nhỏ dưới đầu (đầu nằm lún vào gối)
    static void GoiNhim(StringBuilder log)
    {
        var giuong = TatCa("GiuongNhim_ChieuChan").FirstOrDefault(t => t.GetComponent<LocProp>()); if (!giuong) return;
        var bg = LocSceneAudit.WB(giuong.gameObject);
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(g.GetComponent<Collider>());
        g.name = "Goi_Nhim"; g.transform.SetParent(giuong.parent, false);
        g.transform.position = new Vector3(bg.max.x - 0.27f, bg.min.y + 0.334f + 0.035f, bg.center.z - 0.12f);
        g.transform.localScale = new Vector3(0.30f, 0.09f, 0.46f);
        g.GetComponent<Renderer>().sharedMaterial = M("M_Goi_Nhim", "#F2E6E0");
        log.AppendLine($"gối Nhím tại {g.transform.position:F2}");
    }

    // ── 11. cổng: hai cánh hở nhau 4 mm đúng giữa cổng → tia nhìn thẳng giữa lọt khe, không hiện [E] Mở cổng → nới hộp va chạm mỗi cánh 3 cm về mép tự do
    static void CongKhit(StringBuilder log)
    {
        foreach (var d in root.GetComponentsInChildren<LocDoor>(true).Where(d => d.ten == "cổng"))
        {
            var bc = d.GetComponent<BoxCollider>(); if (!bc) continue;
            bc.size += new Vector3(0.03f, 0, 0.02f); bc.center += new Vector3(0.015f, 0, 0);
            log.AppendLine("cổng: nới va chạm " + d.name);
        }
    }
}

public static partial class LocHouseBuilder
{
    // [1/11] rà nháy giờ quét cả đồ nhỏ đã nâng cấp → nhiều lưới sao riêng nằm trong scene (scene 83 → 142 MB). Lưới ≥ 1500 đỉnh chưa là asset → ghi ra
    //  Assets/LOC_House/Meshes_Scene (xoá và ghi lại mỗi lần dựng), scene chỉ giữ tham chiếu.
    static void LuuLuoiRaFile()
    {
        const string D = "Assets/LOC_House/Meshes_Scene";
        if (AssetDatabase.IsValidFolder(D)) AssetDatabase.DeleteAsset(D);
        AssetDatabase.CreateFolder("Assets/LOC_House", "Meshes_Scene");
        var da = new HashSet<Mesh>(); int n = 0; long dinh = 0;
        void Ghi(Mesh m)
        {
            if (!m || da.Contains(m) || AssetDatabase.Contains(m) || m.vertexCount < 1500) return;
            da.Add(m); var ten = new string(m.name.Select(ch => char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_').ToArray());
            AssetDatabase.CreateAsset(m, $"{D}/{n++:0000}_{(ten.Length > 40 ? ten.Substring(0, 40) : ten)}.asset"); dinh += m.vertexCount;
        }
        foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Ghi(mf.sharedMesh);
        foreach (var mc in Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Ghi(mc.sharedMesh);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LOC] ghi {n} lưới ({dinh} đỉnh) ra {D}");
        System.IO.File.AppendAllText("Assets/LOC_House/BaoCao_Sua0111.txt", $"lưới ra file: {n} lưới, {dinh} đỉnh → {D}\n");
    }
}

public static partial class LocHouseBuilder
{
    // ── 12. chén hầm: Bo_Chen_Tren_De.glb là MỘT lưới, năm chén là năm vật liệu men_su_ChenNguyen_1…5 (không phải nút con) → bản V35 không tìm thấy
    //   "ChenNguyen_i" nên chưa từng thay chén: dưới hầm luôn năm chén lành, trong khi thoại "Chén thì vỡ gần hết". Bỏ năm chén liền, đặt model trạng thái.
    static void ChenHamThat(StringBuilder log)
    {
        var bo = TatCa("Bo_Chen_Tren_De").FirstOrDefault(t => t.GetComponent<LocProp>()); if (!bo) return;
        var mr = bo.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.sharedMaterials.Any(m => m && m.name.StartsWith("men_su_ChenNguyen_")));
        var nhom = TatCa("ChenHam_TrangThai").FirstOrDefault();
        if (!mr || !nhom) { log.AppendLine("chén hầm: thiếu lưới/nhóm"); return; }
        if (nhom.childCount > 0) { log.AppendLine("chén hầm: nhóm trạng thái đã có sẵn"); return; }
        var mf = mr.GetComponent<MeshFilter>(); var m = Object.Instantiate(mf.sharedMesh); m.name = mf.sharedMesh.name + "_KhongChen"; var v = m.vertices; var ms = mr.sharedMaterials;
        var cho = new (string ten, string model, bool bat)[][]
        {
            new[] { ("Chen1_Vo", "ChenA_Tach", true), ("Chen1_Dan", "ChenA_GanLai", false) },
            new[] { ("Chen2_Vo", "ChenB_Tach", true), ("Chen2_Dan", "ChenB_GanLai", false) },
            new[] { ("Chen3_Dan", "ChenC_GanLai", false) },
            new[] { ("Chen4", "ChenNut", true), ("Chen4_Vo", "ChenC_Tach", false) },
            new[] { ("Chen5", "ChenMe", true), ("Chen5_Vo", "ChenB_Tach", false) },
        };
        var old = cur; cur = nhom; int dat = 0;
        for (int i = 0; i < 5; i++)
        {
            int sub = System.Array.FindIndex(ms, x => x && x.name.StartsWith("men_su_ChenNguyen_" + (i + 1))); if (sub < 0 || sub >= m.subMeshCount) continue;
            var t = m.GetTriangles(sub); if (t.Length == 0) continue;
            var b = new Bounds(mr.transform.TransformPoint(v[t[0]]), Vector3.zero); foreach (var k in t) b.Encapsulate(mr.transform.TransformPoint(v[k]));
            m.SetTriangles(new int[0], sub);
            foreach (var (ten, model, bat) in cho[i])
            {
                var g = Inst(model); if (!g) { log.AppendLine("chén hầm: THIẾU model " + model); continue; }
                g.name = ten;
                g.transform.SetPositionAndRotation(new Vector3(b.center.x, b.min.y, b.center.z), Quaternion.Euler(0, 37f * i + 10f, 0));
                g.SetActive(bat); dat++;
            }
        }
        cur = old; mf.sharedMesh = m;
        log.AppendLine($"chén hầm: bỏ năm chén liền, đặt {dat} model trạng thái (vỡ/dán/nứt/mẻ)");
    }
}
