using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// [2/10] cửa tủ mở được: tách cánh (tam giác nằm gọn trong hộp chọn) ra khỏi mesh tủ → GameObject riêng có gốc tại bản lề + LocDoor + BoxCollider.
// Thân tủ đặc thì khoét mặt trước trong ô cửa (cắt tam giác, giữ phần ngoài ô) và lắp hộc trong (5 mặt quay vào trong, tối hơn thân) + đợt.
// Toạ độ trong bảng = toạ độ FILE .glb (m, mặt trước +z nếu truoc = +1). glTFast lật X → tự dò dấu X theo số tam giác bắt được.
public static class LocTuMo
{
    public enum Kieu { Quay, Truot, NganKeo }

    public class Canh
    {
        public string ten = "cửa tủ";
        public Vector3 a, b;              // hộp chọn tam giác (file)
        public float hx, hz;              // bản lề (file x, z) — Quay
        public Kieu kieu = Kieu.Quay;
        public Vector3 truot;             // Truot: dịch (file) để sang trạng thái còn lại
        public float sauNgan = 0.3f;      // NganKeo: sâu lòng ngăn (m), kéo ra 85%
        public bool dangMo;               // cánh đang hé sẵn (Quay) / đang lệch (Truot)
        public Vector3 huongDong;         // Quay + dangMo: hướng từ bản lề về phía cánh khi đóng (file)
        public bool lamDay;               // cánh chỉ là 1 mặt phẳng → đắp thêm bản dày 12 mm phía sau
        public string moHinh;             // khác null: lấy cả model con này làm cánh (cửa tủ lạnh)
        public float gocMax = 100;        // Quay: góc mở tối đa (độ), hạ xuống khi cánh quét vào tường / đồ
        public float mat = float.NaN;     // lamDay: z (file) mặt trước thật của cánh — khi nút/tay nắm nhô ra làm lệch bao
        public Rect banMoi;               // ≠ 0: model không có cánh riêng → đắp tấm cánh mới phủ ô (file x,y), dày 12 mm trước mặt thân
    }

    public class Hoc { public Color? mau; public float x0, x1, y0, y1, zMat, zDay, kz0 = -9, kz1 = -9; public float[] ke; public float sang = 0.45f; public string moHinh; }

    public class Tu { public Vector3? fMin, fMax; public string ten, moHinh; public int truoc = 1; public float chiaX = float.NaN; public Canh[] canh = new Canh[0]; public Hoc[] hoc = new Hoc[0]; }

    static Canh Q(float x0, float x1, float y0, float y1, float z0, float z1, float hx, float hz, string ten = "cửa tủ")
        => new Canh { a = new Vector3(x0, y0, z0), b = new Vector3(x1, y1, z1), hx = hx, hz = hz, ten = ten };
    static Canh NK(float x0, float x1, float y0, float y1, float z0, float z1, float sau)
        => new Canh { a = new Vector3(x0, y0, z0), b = new Vector3(x1, y1, z1), kieu = Kieu.NganKeo, sauNgan = sau, ten = "ngăn kéo" };
    static Canh Gm(Canh c, float g) { c.gocMax = g; return c; }
    static Hoc H(float x0, float x1, float y0, float y1, float zMat, float zDay, float kz0, float kz1, params float[] ke)
        => new Hoc { x0 = x0, x1 = x1, y0 = y0, y1 = y1, zMat = zMat, zDay = zDay, kz0 = kz0, kz1 = kz1, ke = ke };

    // ───── bảng tủ (số đo lấy từ file .glb, xem BaoCao_TuMo.txt)
    public static readonly Tu[] Bang =
    {
        new Tu { fMin = new Vector3(-0.75f, 0f, -1.8f), fMax = new Vector3(0.75f, 1.1f, -1.15f), ten = "BanThoGiaTien_Set", moHinh = "BanThoGiaTien", canh = new[] {
            Q(-0.665f, -0.002f, 0.228f, 0.94f, -1.222f, -1.17f, -0.66f, -1.205f, "cửa tủ thờ"),
            Q(0.002f, 0.665f, 0.228f, 0.94f, -1.222f, -1.17f, 0.66f, -1.205f, "cửa tủ thờ") } },
        new Tu { fMin = new Vector3(-0.36f, 0f, -0.176f), fMax = new Vector3(0.36f, 0.845f, 0.176f), ten = "TuDoTho_Fix", moHinh = "TuDoTho_Fix", canh = new[] {
            Q(-0.312f, -0.004f, 0.115f, 0.765f, 0.144f, 0.18f, -0.307f, 0.151f, "cửa tủ đồ thờ"),
            Q(-0.05f, 0.312f, 0.105f, 0.775f, 0.144f, 0.18f, 0.307f, 0.151f, "cửa tủ đồ thờ") },
            hoc = new[] { H(-0.307f, 0.307f, 0.12f, 0.76f, 0.144f, -0.15f, 0f, 0.1455f, 0.44f) } },
        new Tu { ten = "TuAo_Khoi", moHinh = "TuAo_Khoi", truoc = -1, canh = new[] {
            Gm(Q(-0.002f, 0.452f, -0.002f, 1.79f, -0.515f, -0.475f, 0.005f, -0.49f, "cửa tủ áo"), 70),
            new Canh { a = new Vector3(0.51f, -0.002f, -0.745f), b = new Vector3(0.92f, 1.79f, -0.47f), hx = 0.9f, hz = -0.49f,
                       dangMo = true, huongDong = new Vector3(-1, 0, 0), ten = "cửa tủ áo" } } },
        new Tu { ten = "TuQuanAo_GamHo", moHinh = "TuQuanAo_GamHo", chiaX = 0f, canh = new[] {
            Q(-0.575f, 0f, 0.275f, 1.835f, 0.25f, 0.30f, -0.57f, 0.27f, "cửa tủ quần áo"),
            Q(0f, 0.575f, 0.275f, 1.835f, 0.25f, 0.30f, 0.57f, 0.27f, "cửa tủ quần áo") },
            hoc = new[] { H(-0.57f, 0.57f, 0.28f, 1.83f, 0.275f, -0.255f, 0f, 0.283f, 1.55f) } },
        new Tu { ten = "TuNhua_Nhim", moHinh = "TuNhua_Nhim", canh = new[] {
            new Canh { a = new Vector3(0.015f, 0.015f, -0.012f), b = new Vector3(0.435f, 0.424f, 0.015f), hx = 0.02f, hz = 0.0f, lamDay = true, ten = "cửa tủ nhựa" } },
            hoc = new[] { H(0.02f, 0.43f, 0.02f, 0.421f, 0.0f, -0.37f, -0.01f, 0.0015f) } },
        new Tu { ten = "TuThap_Sanh", moHinh = "TuThap_Sanh", canh = new[] {
            new Canh { a = new Vector3(0.005f, 0.015f, -0.012f), b = new Vector3(0.40f, 0.535f, 0.015f), hx = 0.01f, hz = 0f, lamDay = true },
            new Canh { a = new Vector3(0.405f, 0.015f, -0.012f), b = new Vector3(0.80f, 0.535f, 0.015f), hx = 0.795f, hz = 0f, lamDay = true } },
            hoc = new[] { H(0.01f, 0.795f, 0.02f, 0.53f, 0.0f, -0.335f, -0.01f, 0.0008f, 0.28f) } },
        new Tu { fMin = new Vector3(-0.52f, 0f, -0.235f), fMax = new Vector3(0.52f, 1.78f, 0.235f), ten = "TuBuffet_ChenBat", moHinh = "TuBuffet_ChenBat", canh = new[] {
            Q(-0.475f, -0.004f, 0.05f, 0.96f, 0.205f, 0.24f, -0.47f, 0.213f, "cửa tủ chén"),
            Q(0.004f, 0.475f, 0.05f, 0.96f, 0.205f, 0.24f, 0.47f, 0.213f, "cửa tủ chén"),
            new Canh { a = new Vector3(-0.23f, 1.06f, 0.178f), b = new Vector3(0.23f, 1.75f, 0.192f), kieu = Kieu.Truot, truot = new Vector3(-0.24f, 0, 0), dangMo = true, ten = "kính tủ chén" } },
            hoc = new[] { H(-0.47f, 0.47f, 0.055f, 0.955f, 0.207f, -0.215f, 0f, 0.217f, 0.5f),
                          H(-0.475f, 0.475f, 1.02f, 1.75f, 0.165f, -0.2f, 0f, 0.17f) } },
        new Tu { fMin = new Vector3(-0.6f, 0f, -0.238f), fMax = new Vector3(0.6f, 1.704f, 0.238f), ten = "TuHoSo_Sat", moHinh = "TuHoSo_Sat", canh = new[] {
            NK(-0.574f, -0.026f, 0.061f, 0.419f, 0.204f, 0.245f, 0.36f), NK(0.026f, 0.574f, 0.061f, 0.419f, 0.204f, 0.245f, 0.36f),
            NK(-0.574f, -0.026f, 0.441f, 0.799f, 0.204f, 0.245f, 0.36f), NK(0.026f, 0.574f, 0.441f, 0.799f, 0.204f, 0.245f, 0.36f),
            NK(-0.574f, -0.026f, 0.821f, 1.179f, 0.204f, 0.245f, 0.36f), NK(0.026f, 0.574f, 0.821f, 1.179f, 0.204f, 0.245f, 0.36f),
            NK(-0.574f, -0.026f, 1.201f, 1.559f, 0.204f, 0.245f, 0.36f), NK(0.026f, 0.574f, 1.201f, 1.559f, 0.204f, 0.245f, 0.36f) },
            hoc = new[] { H(-0.57f, -0.03f, 0.065f, 0.415f, 0.212f, -0.2f, 0f, 0.2195f), H(0.03f, 0.57f, 0.065f, 0.415f, 0.212f, -0.2f, 0f, 0.2195f),
                          H(-0.57f, -0.03f, 0.445f, 0.795f, 0.212f, -0.2f, 0f, 0.2195f), H(0.03f, 0.57f, 0.445f, 0.795f, 0.212f, -0.2f, 0f, 0.2195f),
                          H(-0.57f, -0.03f, 0.825f, 1.175f, 0.212f, -0.2f, 0f, 0.2195f), H(0.03f, 0.57f, 0.825f, 1.175f, 0.212f, -0.2f, 0f, 0.2195f),
                          H(-0.57f, -0.03f, 1.205f, 1.555f, 0.212f, -0.2f, 0f, 0.2195f), H(0.03f, 0.57f, 1.205f, 1.555f, 0.212f, -0.2f, 0f, 0.2195f) } },
        new Tu { ten = "KetSat_KhoaSo", moHinh = "KetSat_KhoaSo", canh = new[] { Q(-0.19f, 0.19f, 0.01f, 0.23f, 0.168f, 0.205f, 0.185f, 0.178f, "cửa két sắt") },
            hoc = new[] { H(-0.185f, 0.185f, 0.015f, 0.225f, 0.17f, -0.165f, 0f, 0.1745f) } },
        new Tu { fMin = new Vector3(-1.16f, 0f, -0.324f), fMax = new Vector3(1.16f, 1.8f, 0.324f), ten = "TuDung_RuongChanMan", moHinh = "TuDung_RuongChanMan", canh = new[] { Q(-1.13f, -0.29f, 0.045f, 1.755f, 0.29f, 0.31f, -1.125f, 0.301f, "cửa tủ đứng") },
            hoc = new[] { H(-1.125f, -0.295f, 0.05f, 1.75f, 0.296f, -0.28f, 0f, 0.3045f, 1.2f) } },
        new Tu { fMin = new Vector3(-0.8f, 0f, -0.323f), fMax = new Vector3(0.8f, 1.2f, 0.323f), ten = "TuKinh", moHinh = "TuKinh", canh = new[] {
            new Canh { a = new Vector3(-0.002f, 0.035f, 0.309f), b = new Vector3(0.765f, 1.165f, 0.33f), kieu = Kieu.Truot, truot = new Vector3(-0.72f, 0, 0), ten = "kính tủ" },
            new Canh { a = new Vector3(-0.765f, 0.035f, 0.289f), b = new Vector3(0.002f, 1.165f, 0.2985f), kieu = Kieu.Truot, truot = new Vector3(0.72f, 0, 0), ten = "kính tủ" } } },
        new Tu { ten = "TuLanh_Bo", moHinh = "TuLanh", canh = new[] { new Canh { moHinh = "TuLanh_Canh", hx = 0.5f, hz = 0f, ten = "cửa tủ lạnh" } },
            hoc = new[] { new Hoc { x0 = 0.02f, x1 = 0.56f, y0 = 0.02f, y1 = 1.28f, zMat = 0f, zDay = -0.6f, kz0 = -0.01f, kz1 = 0.005f, ke = new[] { 0.45f, 0.85f }, mau = new Color(0.86f, 0.87f, 0.84f) } } },
        new Tu { ten = "TuTV_Dung_Bo", moHinh = "TuTV_Dung", canh = new[] {
            Q(0.017f, 0.6f, 0.005f, 0.595f, -0.003f, 0.045f, 0.022f, 0.01f, "cửa tủ TV"),
            Q(0.6f, 1.183f, 0.005f, 0.595f, -0.003f, 0.045f, 1.178f, 0.01f, "cửa tủ TV") },
            hoc = new[] { H(0.022f, 1.178f, 0.01f, 0.59f, 0f, -0.43f, -0.01f, 0.004f) } },
        new Tu { ten = "TuDauGiuong", moHinh = "TuDauGiuong", canh = new[] {
            new Canh { a = new Vector3(0.18f, 0.34f, -0.02f), b = new Vector3(0.22f, 0.39f, 0.02f), hx = 0.02f, hz = 0.006f, banMoi = new Rect(0.02f, 0.1f, 0.36f, 0.36f), ten = "cửa tủ đầu giường" } },
            hoc = new[] { H(0.02f, 0.38f, 0.1f, 0.46f, 0f, -0.40f, -0.005f, 0.005f) } },
        new Tu { ten = "BanTrangDiem_Bo", moHinh = "BanTrangDiem", canh = new[] {
            new Canh { a = new Vector3(0.345f, 0.545f, -0.015f), b = new Vector3(0.555f, 0.655f, 0.02f), kieu = Kieu.NganKeo, sauNgan = 0.3f, lamDay = true, ten = "ngăn kéo" } } },
        new Tu { ten = "Chan_BatDiaTrongChan", moHinh = "Chan_BatDiaTrongChan", canh = new[] {
            Q(-0.432f, 0.0005f, 0.795f, 1.578f, 0.205f, 0.25f, -0.428f, 0.217f, "cửa chạn"), Q(-0.0005f, 0.432f, 0.795f, 1.578f, 0.205f, 0.25f, 0.428f, 0.217f, "cửa chạn"),
            Q(-0.43f, 0.0005f, 0.145f, 0.775f, 0.205f, 0.25f, -0.425f, 0.217f, "cửa chạn"), Q(-0.0005f, 0.43f, 0.145f, 0.775f, 0.205f, 0.25f, 0.425f, 0.217f, "cửa chạn") },
            hoc = new[] { H(-0.428f, 0.428f, 0.799f, 1.573f, 0.209f, -0.21f, 0f, 0.2085f), H(-0.425f, 0.425f, 0.15f, 0.77f, 0.209f, -0.21f, 0f, 0.2085f) } },
        new Tu { ten = "BanCoHoc_Go", moHinh = "BanCoHoc_Go", canh = new[] {
            NK(-0.482f, -0.118f, 0.018f, 0.222f, 0.226f, 0.254f, 0.38f), NK(-0.482f, -0.118f, 0.238f, 0.442f, 0.226f, 0.254f, 0.38f), NK(-0.482f, -0.118f, 0.458f, 0.662f, 0.226f, 0.254f, 0.38f) },
            hoc = new[] { H(-0.475f, -0.125f, 0.025f, 0.215f, 0.22f, -0.2f, 0f, 0.2285f), H(-0.475f, -0.125f, 0.245f, 0.435f, 0.22f, -0.2f, 0f, 0.2285f), H(-0.475f, -0.125f, 0.465f, 0.655f, 0.22f, -0.2f, 0f, 0.2285f) } },
        new Tu { ten = "BanHoc_Khoi_Bo", moHinh = "BanHoc_Khoi", canh = new[] {
            new Canh { a = new Vector3(0.812f, 0.558f, -0.02f), b = new Vector3(1.058f, 0.652f, 0.02f), kieu = Kieu.NganKeo, sauNgan = 0.35f, lamDay = true, mat = 0.001f, ten = "ngăn kéo" } } },
        new Tu { ten = "BanGiay_Go", moHinh = "BanGiay_Go", canh = new[] {
            NK(0.078f, 0.642f, 0.428f, 0.702f, 0.326f, 0.364f, 0.3f) } },
    };

    // ───── dữ liệu mesh làm việc (toạ độ cục bộ của gốc model g)
    class MD
    {
        public MeshFilter mf; public Matrix4x4 toG, toM;
        public List<Vector3> v = new(), n = new(); public List<Vector2> uv = new(); public List<Vector4> tg = new(); public List<Color> col = new();
        public List<List<int>> tri = new();   // theo submesh
        public bool sua;
    }

    public static void Apply(Transform root, System.Text.StringBuilder log)
    {
        LocDoTrongTu.Reset();
        foreach (var tu in Bang)
        {
            int inst = 0;
            foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(x => x.name == tu.ten && x.GetComponent<LocProp>()).ToList())
            {
                try { MotTu(t, tu, log, inst++); }
                catch (System.Exception e) { log.AppendLine($"!! {tu.ten}: {e.Message}"); Debug.LogException(e); }
            }
        }
    }

    [MenuItem("LOC/Debug tủ")]
    public static void DebugTu()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var tu in Bang)
            foreach (var t in Object.FindObjectsByType<LocProp>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(x => x.name == tu.ten))
            {
                var g = Goc(t.transform, tu.moHinh);
                sb.AppendLine($"== {tu.ten} g={(g ? g.name : "null")} parent={(g && g.parent ? g.parent.name : "-")} prefabRoot={(g ? PrefabUtility.IsAnyPrefabInstanceRoot(g.gameObject) : false)}");
                if (!g) continue;
                foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true).Take(6))
                {
                    var m = mf.sharedMesh; if (!m) { sb.AppendLine($"   {mf.name}: no mesh"); continue; }
                    var vs = new List<Vector3>(); m.GetVertices(vs);
                    var M = g.worldToLocalMatrix * mf.transform.localToWorldMatrix;
                    var b = new Bounds(M.MultiplyPoint3x4(m.bounds.min), Vector3.zero); b.Encapsulate(M.MultiplyPoint3x4(m.bounds.max));
                    sb.AppendLine($"   {mf.name}: readable={m.isReadable} vc={m.vertexCount} got={vs.Count} sub={m.subMeshCount} gLocal {b.min:F3}..{b.max:F3}");
                }
            }
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_DebugTu.txt", sb.ToString());
        Debug.Log("[LOC] debug tủ xong");
    }

    static Transform Goc(Transform t, string moHinh)
    {
        var cs = t.GetComponentsInChildren<Transform>(true).Where(x => x.name == moHinh).ToList();
        return cs.FirstOrDefault(x => PrefabUtility.IsAnyPrefabInstanceRoot(x.gameObject)) ?? cs.LastOrDefault();   // gốc prefab: các nút anh em (cánh) mới nằm dưới nó
    }

    static void MotTu(Transform tuT, Tu tu, System.Text.StringBuilder log, int inst)
    {
        var g = Goc(tuT, tu.moHinh);
        if (!g) { log.AppendLine($"!! {tu.ten}: không thấy model {tu.moHinh}"); return; }
        var mds = new List<MD>();
        foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!mf.sharedMesh || !mf.GetComponent<MeshRenderer>()) continue;
            var m = mf.sharedMesh; var md = new MD { mf = mf, toG = g.worldToLocalMatrix * mf.transform.localToWorldMatrix };
            md.toM = md.toG.inverse;
            m.GetVertices(md.v); m.GetNormals(md.n); m.GetUVs(0, md.uv); m.GetTangents(md.tg); m.GetColors(md.col);
            for (int i = 0; i < md.v.Count; i++) { md.v[i] = md.toG.MultiplyPoint3x4(md.v[i]); if (md.n.Count == md.v.Count) md.n[i] = md.toG.MultiplyVector(md.n[i]).normalized; }
            for (int s = 0; s < m.subMeshCount; s++) md.tri.Add(m.GetTopology(s) == MeshTopology.Triangles ? new List<int>(m.GetTriangles(s)) : new List<int>());
            mds.Add(md);
        }

        // dấu X: thử cả hai, chọn cách bắt được nhiều tam giác nhất
        float sx = -1; Vector3 lech = Vector3.zero;
        var gb = new Bounds(mds[0].v[0], Vector3.zero); foreach (var md0 in mds) foreach (var p0 in md0.v) gb.Encapsulate(p0);
        Vector3 Lech(float s) => tu.fMin.HasValue ? gb.center - new Vector3((tu.fMin.Value.x + tu.fMax.Value.x) / 2 * s, (tu.fMin.Value.y + tu.fMax.Value.y) / 2, (tu.fMin.Value.z + tu.fMax.Value.z) / 2) : Vector3.zero;
        if (tu.canh.Any(c => c.moHinh == null))
        {
            int Dem(float s) => tu.canh.Where(c => c.moHinh == null).Sum(c => mds.Sum(md => DemTrong(md, Box(c, s, Lech(s)))));
            int a = Dem(-1), b = Dem(1); sx = a >= b ? -1 : 1; lech = Lech(sx);
            log.AppendLine($"── {tu.ten}: lật X {(sx < 0 ? "có" : "không")} ({a}/{b} tam giác)");
        }
        else
        {
            // không có hộp cánh: chọn dấu X sao cho ô hộc nằm trong thân model
            float Phu(float s) => tu.hoc.Sum(h => Mathf.Max(0, Mathf.Min(gb.max.x, Mathf.Max(h.x0 * s, h.x1 * s)) - Mathf.Max(gb.min.x, Mathf.Min(h.x0 * s, h.x1 * s))));
            sx = Phu(-1) >= Phu(1) ? -1 : 1;
            log.AppendLine($"── {tu.ten}: lật X {(sx < 0 ? "có" : "không")} (theo hộc)");
        }
        if (lech != Vector3.zero) log.AppendLine($"   lệch gốc {lech:F3}");
        Vector3 L(Vector3 f) => new Vector3(f.x * sx, f.y, f.z) + lech;   // file → cục bộ g
        Vector3 Lv(Vector3 f) => new Vector3(f.x * sx, f.y, f.z);         // véc-tơ

        if (!float.IsNaN(tu.chiaX))
        {
            var ub = new Bounds(); bool first = true;
            foreach (var c in tu.canh.Where(c => c.moHinh == null)) { var bx = Box(c, sx, lech); if (first) { ub = bx; first = false; } else ub.Encapsulate(bx); }
            foreach (var md in mds) ChiaX(md, tu.chiaX * sx, ub);
        }

        // cánh
        int k = 0;
        foreach (var c in tu.canh)
        {
            k++;
            Transform leaf;
            if (c.moHinh != null)
            {
                var cg = Goc(tuT, c.moHinh); if (!cg) { log.AppendLine($"   !! không thấy {c.moHinh}"); continue; }
                var cb = new Bounds(); bool f0 = true;
                foreach (var r in cg.GetComponentsInChildren<MeshRenderer>()) { var lb0 = r.GetComponent<MeshFilter>().sharedMesh.bounds; var w0 = cg.InverseTransformPoint(r.transform.TransformPoint(lb0.min)); var w1 = cg.InverseTransformPoint(r.transform.TransformPoint(lb0.max)); if (f0) { cb = new Bounds(w0, Vector3.zero); f0 = false; } cb.Encapsulate(w0); cb.Encapsulate(w1); }
                float csx = Mathf.Abs(c.hx * -1 - Mathf.Clamp(c.hx * -1, cb.min.x, cb.max.x)) <= Mathf.Abs(c.hx - Mathf.Clamp(c.hx, cb.min.x, cb.max.x)) ? -1 : 1;
                var hingeW = cg.TransformPoint(new Vector3(c.hx * csx, 0, c.hz));
                leaf = new GameObject($"Canh_{k}").transform; leaf.SetParent(cg.parent, false);
                leaf.position = hingeW; leaf.rotation = cg.rotation;
                cg.SetParent(leaf, true);
                if (tu.ten == "TuLanh_Bo") DanHinhCanh(cg, "D_tulanh.png");   // thân đã khoét → hình mặt tủ lạnh chuyển sang cánh
            }
            else
            {
                var bx = Box(c, sx, lech);
                Vector3 hingeL = c.kieu == Kieu.Quay ? L(new Vector3(c.hx, c.a.y, c.hz)) : new Vector3(bx.center.x, bx.min.y, bx.center.z);
                leaf = TachCanh(g, mds, bx, hingeL, $"Canh_{k}");
                if (!leaf && c.banMoi.width > 0) { leaf = new GameObject($"Canh_{k}").transform; leaf.SetParent(g, false); leaf.localPosition = hingeL; leaf.localRotation = Quaternion.identity; }
                if (!leaf) { log.AppendLine($"   !! cánh {k}: không bắt được tam giác nào"); continue; }
                if (c.banMoi.width > 0)
                {
                    var r0 = c.banMoi; var ctr = L(new Vector3(r0.x + r0.width / 2, r0.y + r0.height / 2, tu.truoc * 0.006f));
                    var ban = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(ban.GetComponent<Collider>());
                    ban.name = "BanCanh"; ban.transform.SetParent(leaf, false);
                    ban.transform.localPosition = ctr - hingeL; ban.transform.localScale = new Vector3(r0.width, r0.height, 0.012f);
                    var src = g.GetComponentsInChildren<MeshRenderer>().OrderByDescending(x => x.bounds.size.sqrMagnitude).First();
                    ban.GetComponent<Renderer>().sharedMaterial = src.sharedMaterials.FirstOrDefault();
                }
                if (c.lamDay) DapDay(leaf, tu.truoc, float.IsNaN(c.mat) ? float.NaN : c.mat - bx.center.z + lech.z);
            }
            var d = leaf.gameObject.AddComponent<LocDoor>();
            d.ten = c.ten; d.banLe = Vector3.zero; d.dungSanLaMo = c.dangMo;
            var front = g.TransformDirection(new Vector3(0, 0, tu.truoc)).normalized;
            var lb = WorldBounds(leaf);
            if (c.kieu == Kieu.Quay)
            {
                var hW = leaf.position; var dir = lb.center - hW; dir.y = 0;
                if (c.dangMo)
                {
                    var dong = g.TransformDirection(Lv(c.huongDong)); dong.y = 0;
                    d.goc = Vector3.SignedAngle(dir, dong, Vector3.up);
                }
                else
                {
                    var q = Quaternion.AngleAxis(100, Vector3.up) * dir;
                    d.goc = Vector3.Dot(q, front) > 0 ? c.gocMax : -c.gocMax;
                }
            }
            else if (c.kieu == Kieu.Truot) d.truot = leaf.parent.InverseTransformVector(g.TransformVector(Lv(c.truot)));
            else
            {
                LapNgan(leaf, g, tu.truoc, c.sauNgan, MauThan(g), tu.ten, k, inst);
                d.truot = leaf.parent.InverseTransformVector(front * c.sauNgan * 0.85f);
                d.thoiGian = 0.45f;
            }
            var bc = leaf.gameObject.AddComponent<BoxCollider>();
            var lc = leaf.InverseTransformPoint(lb.center); var ls = leaf.InverseTransformVector(lb.size);
            bc.center = lc; bc.size = new Vector3(Mathf.Max(Mathf.Abs(ls.x), 0.03f), Mathf.Max(Mathf.Abs(ls.y), 0.03f), Mathf.Max(Mathf.Abs(ls.z), 0.03f));
            log.AppendLine($"   cánh {k} ({c.kieu}) {c.ten}: goc {d.goc:0}°, trượt {d.truot}");
        }

        // khoét + hộc
        int hi = 0;
        foreach (var h0 in tu.hoc)
        {
            var h = h0;
            var hg = h.moHinh != null ? Goc(tuT, h.moHinh) : g;
            float xa = Mathf.Min(h.x0 * sx, h.x1 * sx) + lech.x, xb = Mathf.Max(h.x0 * sx, h.x1 * sx) + lech.x;
            if (h.kz0 > -9) foreach (var md in mds) Khoet(md, xa, xb, h.y0 + lech.y, h.y1 + lech.y, Mathf.Min(h.kz0, h.kz1) + lech.z, Mathf.Max(h.kz0, h.kz1) + lech.z, tu.truoc);
            h = new Hoc { x0 = h.x0, x1 = h.x1, y0 = h.y0 + lech.y, y1 = h.y1 + lech.y, zMat = h.zMat + lech.z, zDay = h.zDay + lech.z, ke = h.ke?.Select(v => v + lech.y).ToArray(), sang = h.sang, mau = h.mau };
            LapHoc(g, xa, xb, h, tu.truoc, MauThan(g), log);
            if (!tu.canh.Any(c => c.kieu == Kieu.NganKeo)) LocDoTrongTu.Hoc(tu.ten, hi, inst, g, xa, xb, h.y0, h.y1, h.zMat, h.zDay, h.ke, tu.truoc);   // [2/10] đồ trong lòng tủ có cánh
            hi++;
        }

        // ghi lại mesh đã sửa
        foreach (var md in mds.Where(m => m.sua))
        {
            var m = Object.Instantiate(md.mf.sharedMesh); m.name = md.mf.sharedMesh.name.Replace("(Clone)", "") + "_tumo";
            m.Clear();
            var vs = md.v.Select(p => md.toM.MultiplyPoint3x4(p)).ToList();
            m.indexFormat = vs.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            m.SetVertices(vs);
            if (md.n.Count == md.v.Count) m.SetNormals(md.n.Select(x => md.toM.MultiplyVector(x).normalized).ToList());
            if (md.uv.Count == md.v.Count) m.SetUVs(0, md.uv);
            if (md.tg.Count == md.v.Count) m.SetTangents(md.tg);
            if (md.col.Count == md.v.Count) m.SetColors(md.col);
            m.subMeshCount = md.tri.Count;
            for (int s = 0; s < md.tri.Count; s++) m.SetTriangles(md.tri[s], s);
            if (md.n.Count != md.v.Count) m.RecalculateNormals();
            m.RecalculateBounds();
            md.mf.sharedMesh = m;
            if (md.tri.All(t => t.Count == 0)) md.mf.GetComponent<MeshRenderer>().enabled = false;   // nút đã chuyển hết sang cánh
        }
    }

    static Bounds Box(Canh c, float sx, Vector3 lech)
    {
        var p = new Vector3(c.a.x * sx, c.a.y, c.a.z) + lech; var q = new Vector3(c.b.x * sx, c.b.y, c.b.z) + lech;
        var b = new Bounds(p, Vector3.zero); b.Encapsulate(q); return b;
    }

    static bool Trong(Bounds b, Vector3 p) => p.x >= b.min.x && p.x <= b.max.x && p.y >= b.min.y && p.y <= b.max.y && p.z >= b.min.z && p.z <= b.max.z;

    static int DemTrong(MD md, Bounds b)
    {
        int n = 0;
        foreach (var t in md.tri) for (int i = 0; i + 2 < t.Count; i += 3) if (Trong(b, md.v[t[i]]) && Trong(b, md.v[t[i + 1]]) && Trong(b, md.v[t[i + 2]])) n++;
        return n;
    }

    // ───── tách cánh
    static Transform TachCanh(Transform g, List<MD> mds, Bounds bx, Vector3 hingeL, string ten)
    {
        var vs = new List<Vector3>(); var ns = new List<Vector3>(); var uvs = new List<Vector2>(); var subs = new List<List<int>>(); var mats = new List<Material>();
        foreach (var md in mds)
        {
            var ms = md.mf.GetComponent<MeshRenderer>().sharedMaterials;
            for (int s = 0; s < md.tri.Count; s++)
            {
                var t = md.tri[s]; var keep = new List<int>(); List<int> sub = null; var map = new Dictionary<int, int>();
                for (int i = 0; i + 2 < t.Count; i += 3)
                {
                    if (!(Trong(bx, md.v[t[i]]) && Trong(bx, md.v[t[i + 1]]) && Trong(bx, md.v[t[i + 2]]))) { keep.Add(t[i]); keep.Add(t[i + 1]); keep.Add(t[i + 2]); continue; }
                    if (sub == null) { sub = new List<int>(); subs.Add(sub); mats.Add(s < ms.Length ? ms[s] : ms.LastOrDefault()); }
                    for (int j = 0; j < 3; j++)
                    {
                        int vi = t[i + j];
                        if (!map.TryGetValue(vi, out var ni))
                        {
                            ni = vs.Count; map[vi] = ni; vs.Add(md.v[vi] - hingeL);
                            ns.Add(md.n.Count == md.v.Count ? md.n[vi] : Vector3.up); uvs.Add(md.uv.Count == md.v.Count ? md.uv[vi] : Vector2.zero);
                        }
                        sub.Add(ni);
                    }
                }
                if (sub != null) { md.tri[s] = keep; md.sua = true; }
            }
        }
        if (vs.Count == 0) return null;
        var m = new Mesh { name = ten };
        m.indexFormat = vs.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        m.SetVertices(vs); m.SetNormals(ns); m.SetUVs(0, uvs); m.subMeshCount = subs.Count;
        for (int s = 0; s < subs.Count; s++) m.SetTriangles(subs[s], s);
        m.RecalculateBounds();
        var leaf = new GameObject(ten);
        leaf.transform.SetParent(g, false); leaf.transform.localPosition = hingeL; leaf.transform.localRotation = Quaternion.identity;
        leaf.AddComponent<MeshFilter>().sharedMesh = m;
        leaf.AddComponent<MeshRenderer>().sharedMaterials = mats.ToArray();
        return leaf.transform;
    }

    // ───── cắt tam giác ngang mặt phẳng x = xs (chỉ những tam giác nằm trong vùng y,z của hộp cánh)
    static void ChiaX(MD md, float xs, Bounds ub)
    {
        for (int s = 0; s < md.tri.Count; s++)
        {
            var t = md.tri[s]; var outT = new List<int>(); bool doi = false;
            for (int i = 0; i + 2 < t.Count; i += 3)
            {
                var ids = new[] { t[i], t[i + 1], t[i + 2] };
                var ps = ids.Select(x => md.v[x]).ToArray();
                bool trongYZ = ps.All(p => p.y >= ub.min.y && p.y <= ub.max.y && p.z >= ub.min.z && p.z <= ub.max.z);
                float lo = ps.Min(p => p.x), hi = ps.Max(p => p.x);
                if (!trongYZ || lo >= xs - 1e-4f || hi <= xs + 1e-4f) { outT.AddRange(ids); continue; }
                doi = true;
                foreach (var poly in new[] { Clip(md, ids.ToList(), new Vector3(-1, 0, 0), -xs), Clip(md, ids.ToList(), new Vector3(1, 0, 0), xs) })
                    Fan(poly, outT);
            }
            if (doi) { md.tri[s] = outT; md.sua = true; }
        }
    }

    // ───── khoét: mặt hướng trước (cùng chiều truoc) nằm trong khoảng z [kz0,kz1] → bỏ phần trong ô [xa,xb]×[y0,y1]
    static void Khoet(MD md, float xa, float xb, float y0, float y1, float kz0, float kz1, int truoc)
    {
        for (int s = 0; s < md.tri.Count; s++)
        {
            var t = md.tri[s]; var outT = new List<int>(); bool doi = false;
            for (int i = 0; i + 2 < t.Count; i += 3)
            {
                var ids = new[] { t[i], t[i + 1], t[i + 2] };
                Vector3 a = md.v[ids[0]], b = md.v[ids[1]], c = md.v[ids[2]];
                var nn = Vector3.Cross(b - a, c - a); if (nn.sqrMagnitude < 1e-12f) { outT.AddRange(ids); continue; }
                nn.Normalize();
                bool mat = nn.z * truoc > 0.95f && new[] { a, b, c }.All(p => p.z >= kz0 && p.z <= kz1);
                float lx = Mathf.Min(a.x, b.x, c.x), hx = Mathf.Max(a.x, b.x, c.x), ly = Mathf.Min(a.y, b.y, c.y), hy = Mathf.Max(a.y, b.y, c.y);
                bool cham = hx > xa + 1e-4f && lx < xb - 1e-4f && hy > y0 + 1e-4f && ly < y1 - 1e-4f;
                if (!mat || !cham || Vector3.Cross(b - a, c - a).magnitude / 2 < 0.08f * (xb - xa) * (y1 - y0)) { outT.AddRange(ids); continue; }   // chỉ mặt lớn (mặt thân), không đụng mép đợt / đồ bày
                doi = true;
                var tam = ids.ToList();
                // 4 mảnh ngoài ô: trái, phải, dưới (giữa), trên (giữa)
                Fan(Clip(md, tam, new Vector3(1, 0, 0), xa), outT);
                Fan(Clip(md, tam, new Vector3(-1, 0, 0), -xb), outT);
                var giua = Clip(md, Clip(md, tam, new Vector3(-1, 0, 0), -xa), new Vector3(1, 0, 0), xb);
                Fan(Clip(md, giua, new Vector3(0, 1, 0), y0), outT);
                Fan(Clip(md, giua, new Vector3(0, -1, 0), -y1), outT);
            }
            if (doi) { md.tri[s] = outT; md.sua = true; }
        }
    }

    // giữ phần đa giác có dot(p, nPlane) <= d (Sutherland–Hodgman), đỉnh mới nội suy thuộc tính
    static List<int> Clip(MD md, List<int> poly, Vector3 nPlane, float d)
    {
        var res = new List<int>();
        if (poly.Count == 0) return res;
        for (int i = 0; i < poly.Count; i++)
        {
            int a = poly[i], b = poly[(i + 1) % poly.Count];
            float da = Vector3.Dot(md.v[a], nPlane) - d, db = Vector3.Dot(md.v[b], nPlane) - d;
            if (da <= 0) res.Add(a);
            if ((da < 0 && db > 0) || (da > 0 && db < 0)) res.Add(Lerp(md, a, b, da / (da - db)));
        }
        return res;
    }

    static int Lerp(MD md, int a, int b, float t)
    {
        md.v.Add(Vector3.Lerp(md.v[a], md.v[b], t));
        if (md.n.Count == md.v.Count - 1) md.n.Add(Vector3.Lerp(md.n[a], md.n[b], t).normalized);
        if (md.uv.Count == md.v.Count - 1) md.uv.Add(Vector2.Lerp(md.uv[a], md.uv[b], t));
        if (md.tg.Count == md.v.Count - 1) md.tg.Add(Vector4.Lerp(md.tg[a], md.tg[b], t));
        if (md.col.Count == md.v.Count - 1) md.col.Add(Color.Lerp(md.col[a], md.col[b], t));
        return md.v.Count - 1;
    }

    static void Fan(List<int> poly, List<int> outT)
    {
        for (int i = 1; i + 1 < poly.Count; i++) { outT.Add(poly[0]); outT.Add(poly[i]); outT.Add(poly[i + 1]); }
    }

    // ───── hộc trong: 5 mặt quay vào trong + đợt
    static Color MauThan(Transform g)
    {
        var r = g.GetComponentsInChildren<MeshRenderer>().OrderByDescending(x => x.bounds.size.sqrMagnitude).FirstOrDefault();
        var m = r ? r.sharedMaterials.FirstOrDefault(x => x) : null;
        if (!m) return new Color(0.4f, 0.3f, 0.2f);
        return m.HasProperty("baseColorFactor") ? m.GetColor("baseColorFactor") : m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
    }

    static readonly Dictionary<string, Material> matCache = new();
    static Material MatMau(Color c)
    {
        c.a = 1; var key = ColorUtility.ToHtmlStringRGB(c);
        if (matCache.TryGetValue(key, out var m) && m) return m;
        m = new Material(Shader.Find("Universal Render Pipeline/Simple Lit")) { name = "M_TrongTu_" + key };
        m.SetColor("_BaseColor", c); m.color = c;
        return matCache[key] = m;
    }

    static void LapHoc(Transform g, float xa, float xb, Hoc h, int truoc, Color than, System.Text.StringBuilder log)
    {
        const float e = 0.002f;
        float z0 = h.zMat, z1 = h.zDay;   // z0 phía trước, z1 phía sau
        var vs = new List<Vector3>(); var ns = new List<Vector3>(); var uv = new List<Vector2>(); var tr = new List<int>();
        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 n)
        {
            int i0 = vs.Count; vs.AddRange(new[] { a, b, c, d }); for (int i = 0; i < 4; i++) ns.Add(n);
            uv.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
            var cr = Vector3.Cross(b - a, c - a);
            // Unity: mặt trước có pháp tuyến = Cross(p1 − p0, p2 − p0)
            if (Vector3.Dot(cr, n) > 0) tr.AddRange(new[] { i0, i0 + 1, i0 + 2, i0, i0 + 2, i0 + 3 }); else tr.AddRange(new[] { i0, i0 + 2, i0 + 1, i0, i0 + 3, i0 + 2 });
        }
        float X0 = xa + e, X1 = xb - e, Y0 = h.y0 + e, Y1 = h.y1 - e, ZF = z0, ZB = z1 + truoc * e;
        var zf = truoc;   // pháp tuyến mặt đáy hộc quay ra trước
        Quad(new(X0, Y0, ZB), new(X1, Y0, ZB), new(X1, Y1, ZB), new(X0, Y1, ZB), new Vector3(0, 0, zf));      // lưng
        Quad(new(X0, Y0, ZF), new(X0, Y0, ZB), new(X0, Y1, ZB), new(X0, Y1, ZF), Vector3.right);              // hông trái (x nhỏ) nhìn sang +x
        Quad(new(X1, Y0, ZF), new(X1, Y1, ZF), new(X1, Y1, ZB), new(X1, Y0, ZB), Vector3.left);
        Quad(new(X0, Y0, ZF), new(X1, Y0, ZF), new(X1, Y0, ZB), new(X0, Y0, ZB), Vector3.up);                 // đáy
        Quad(new(X0, Y1, ZF), new(X0, Y1, ZB), new(X1, Y1, ZB), new(X1, Y1, ZF), Vector3.down);               // nóc
        var m = new Mesh { name = "HocTu" }; m.SetVertices(vs); m.SetNormals(ns); m.SetUVs(0, uv); m.SetTriangles(tr, 0); m.RecalculateBounds();
        var go = new GameObject("HocTu"); go.transform.SetParent(g, false);
        go.AddComponent<MeshFilter>().sharedMesh = m;
        var mHoc = h.mau ?? than * h.sang;
        go.AddComponent<MeshRenderer>().sharedMaterial = MatMau(mHoc);
        if (h.ke != null)
            foreach (var y in h.ke)
            {
                var k = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(k.GetComponent<Collider>());
                k.name = "DotTu"; k.transform.SetParent(g, false);
                k.transform.localPosition = new Vector3((X0 + X1) / 2, y, (ZF + ZB) / 2 - truoc * 0.01f);
                k.transform.localScale = new Vector3(X1 - X0 - 0.002f, 0.016f, Mathf.Abs(ZF - ZB) - 0.025f);
                k.GetComponent<Renderer>().sharedMaterial = MatMau(h.mau.HasValue ? h.mau.Value * 0.95f : than * Mathf.Min(1f, h.sang * 1.5f));
            }
    }

    // cánh chỉ là một mặt → đắp bản dày phía sau (lùi 0,5 mm, dày 12 mm)
    static void DapDay(Transform leaf, int truoc, float zThat = float.NaN)
    {
        var r = leaf.GetComponent<MeshRenderer>(); var b = leaf.GetComponent<MeshFilter>().sharedMesh.bounds;
        var k = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(k.GetComponent<Collider>());
        k.name = "BanCanh"; k.transform.SetParent(leaf, false);
        float zMat = !float.IsNaN(zThat) ? zThat : truoc > 0 ? b.max.z : b.min.z;
        k.transform.localPosition = new Vector3(b.center.x, b.center.y, zMat - truoc * (0.0005f + 0.006f));
        k.transform.localScale = new Vector3(b.size.x, b.size.y, 0.012f);
        k.GetComponent<Renderer>().sharedMaterial = r.sharedMaterials.FirstOrDefault();
    }

    // lòng ngăn kéo: đáy + 2 hông + lưng, nằm sau mặt ngăn
    static void LapNgan(Transform leaf, Transform g, int truoc, float sau, Color than, string tenTu, int k, int inst)
    {
        var lb = WorldBounds(leaf);
        var c = leaf.InverseTransformPoint(lb.center); var s = leaf.InverseTransformVector(lb.size); s = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        var fz = leaf.InverseTransformDirection(g.TransformDirection(new Vector3(0, 0, truoc))).normalized;   // hướng ra trước trong toạ độ cánh
        float w = s.x - 0.02f, hgt = Mathf.Max(0.04f, s.y * 0.7f), back = s.z / 2 + 0.001f;
        var mat = MatMau(than * 0.75f);
        void B(string n, Vector3 pos, Vector3 size)
        {
            var k = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(k.GetComponent<Collider>());
            k.name = n; k.transform.SetParent(leaf, false); k.transform.localPosition = pos; k.transform.localScale = size; k.GetComponent<Renderer>().sharedMaterial = mat;
        }
        var baseP = c - fz * (back + sau / 2) + Vector3.down * (s.y / 2 - 0.012f);
        B("Ngan_Day", baseP, new Vector3(w, 0.01f, sau));
        B("Ngan_HongT", baseP + new Vector3(-w / 2 + 0.005f, hgt / 2, 0), new Vector3(0.01f, hgt, sau));
        B("Ngan_HongP", baseP + new Vector3(w / 2 - 0.005f, hgt / 2, 0), new Vector3(0.01f, hgt, sau));
        B("Ngan_Lung", baseP - fz * (sau / 2 - 0.005f) + new Vector3(0, hgt / 2, 0), new Vector3(w, hgt, 0.01f));
        LocDoTrongTu.Ngan(tenTu, k, inst, leaf, baseP, fz, w, sau, hgt);   // [2/10] đồ trong ngăn kéo (con của cánh → trượt theo)
    }

    // cánh tủ lạnh trong file là hộp hở mặt trước (hình mặt tủ vốn dán trên thân) → đắp mặt trước mang cả tấm hình
    static void DanHinhCanh(Transform door, string tex)
    {
        var t = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LOC_House/Textures/" + tex); if (!t) return;
        var mf0 = door.GetComponentsInChildren<MeshFilter>().Where(x => x.sharedMesh).OrderByDescending(x => x.sharedMesh.bounds.size.x * x.sharedMesh.bounds.size.y).FirstOrDefault();
        if (!mf0) return;
        var b = mf0.sharedMesh.bounds;   // toạ độ cục bộ của mf0
        var mat = new Material(Shader.Find("Universal Render Pipeline/Simple Lit")) { name = "M_CanhHinh_" + tex };
        mat.SetTexture("_BaseMap", t); mat.mainTexture = t; mat.SetColor("_BaseColor", Color.white);
        mat.SetColor("_SpecColor", new Color(0.22f, 0.22f, 0.22f)); mat.SetFloat("_Smoothness", 0.4f);
        float zf = b.min.z + 0.0355f;   // mặt trước bản cánh (dày 36 mm), lùi 0,5 mm sau nhãn / nam châm dán trên cánh
        var m = new Mesh { name = "MatCanh_" + tex };
        m.vertices = new[] { new Vector3(b.min.x, b.min.y, zf), new Vector3(b.max.x, b.min.y, zf), new Vector3(b.max.x, b.max.y, zf), new Vector3(b.min.x, b.max.y, zf) };
        m.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        m.normals = Enumerable.Repeat(Vector3.forward, 4).ToArray();
        m.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 2, 1, 0, 3, 2 };   // hai mặt: chiều trục cục bộ của cánh sau khi glTFast lật X không chắc
        m.RecalculateBounds();
        var q = new GameObject("MatCanh"); q.transform.SetParent(mf0.transform, false);
        q.AddComponent<MeshFilter>().sharedMesh = m; q.AddComponent<MeshRenderer>().sharedMaterial = mat;
    }

    static Bounds WorldBounds(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b;
    }
}
