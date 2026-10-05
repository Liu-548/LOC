using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [2/10 tối] ĐỒ TRONG TỦ — dựng bằng khối primitive (placeholder chi tiết, không cần file .glb), lắp vào lòng tủ / ngăn kéo do LocTuMo tạo.
// Mọi vật nằm trong 1 nút "DoTu_TrongTu" (LocChiTiet bỏ qua nút này để giữ màu phẳng). Khung toạ độ mỗi ngăn: x ngang (giữa = 0), y lên từ đáy ngăn, z từ lưng ra phía trước (0 → d).
// Cánh cửa tủ đóng thì đồ nằm khuất sau cánh; ngăn kéo thì đồ là con của cánh nên trượt theo.
public static class LocDoTrongTu
{
    public const string Nut = "DoTu_TrongTu";

    // ───── khung toạ độ + khối cơ bản
    class F
    {
        public Transform box; public Vector3 o; public Quaternion q; public float w, d, h; public System.Random r;
        public Vector3 P(float x, float y, float z) => o + q * new Vector3(x, y, z);
        public float Rf(float a, float b) => a + (float)r.NextDouble() * (b - a);
    }

    static readonly Dictionary<string, Material> mats = new();
    static Material M(string hex)
    {
        if (mats.TryGetValue(hex, out var m) && m) return m;
        ColorUtility.TryParseHtmlString(hex, out var c);
        m = new Material(Shader.Find("Universal Render Pipeline/Simple Lit")) { name = "M_DoTu_" + hex.TrimStart('#') };
        m.SetColor("_BaseColor", c); m.color = c; m.SetColor("_SpecColor", new Color(0.06f, 0.06f, 0.06f)); m.SetFloat("_Smoothness", 0.15f);
        return mats[hex] = m;
    }

    static GameObject Prim(F f, PrimitiveType t, Vector3 centre, Vector3 size, string hex, Quaternion rot, string ten)
    {
        var g = GameObject.CreatePrimitive(t); Object.DestroyImmediate(g.GetComponent<Collider>());
        g.name = ten; g.transform.SetParent(f.box, false);
        g.transform.localPosition = f.P(centre.x, centre.y, centre.z); g.transform.localRotation = f.q * rot; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = M(hex);
        return g;
    }
    // hộp: (x, y = ĐÁY, z) tâm đáy; yaw quanh trục đứng, roll quanh trục z (nghiêng)
    static float B(F f, float x, float y, float z, float w, float h, float d, string hex, float yaw = 0, float roll = 0, string ten = "hop")
    { Prim(f, PrimitiveType.Cube, new Vector3(x, y + h / 2, z), new Vector3(w, h, d), hex, Quaternion.Euler(0, yaw, roll), ten); return y + h; }
    // trụ đứng (đáy y)
    static float C(F f, float x, float y, float z, float dia, float h, string hex, string ten = "tru")
    { Prim(f, PrimitiveType.Cylinder, new Vector3(x, y + h / 2, z), new Vector3(dia, h / 2, dia), hex, Quaternion.identity, ten); return y + h; }
    // trụ nằm ngang theo trục x (dài len) hoặc trục z
    static void CX(F f, float x, float y, float z, float dia, float len, string hex, string ten = "tru") =>
        Prim(f, PrimitiveType.Cylinder, new Vector3(x, y + dia / 2, z), new Vector3(dia, len / 2, dia), hex, Quaternion.Euler(0, 0, 90), ten);
    static void CZ(F f, float x, float y, float z, float dia, float len, string hex, string ten = "tru") =>
        Prim(f, PrimitiveType.Cylinder, new Vector3(x, y + dia / 2, z), new Vector3(dia, len / 2, dia), hex, Quaternion.Euler(90, 0, 0), ten);
    static void S(F f, float x, float y, float z, float dia, string hex, float sy = 1f, string ten = "cau") =>
        Prim(f, PrimitiveType.Sphere, new Vector3(x, y + dia * sy / 2, z), new Vector3(dia, dia * sy, dia), hex, Quaternion.identity, ten);

    // ───── vật dụng ghép
    // chồng quần áo/khăn gấp: n lớp, mỗi lớp lệch nhẹ, màu theo danh sách; trả về y đỉnh chồng
    static float Gap(F f, float x, float z, float y, float w, float d, float hMoi, string[] hex, int n, float lech = 0.006f)
    {
        for (int i = 0; i < n; i++)
        {
            float dh = hMoi * f.Rf(0.85f, 1.15f);
            y = B(f, x + f.Rf(-lech, lech), y, z + f.Rf(-lech, lech), w * f.Rf(0.96f, 1.0f), dh, d * f.Rf(0.96f, 1.0f), hex[i % hex.Length], f.Rf(-3, 3), 0, "gap");
        }
        return y;
    }
    // hàng sách / bìa hồ sơ đứng từ x0 → x1
    static void Sach(F f, float x0, float x1, float y, float z, float sau, string[] hex, float hMin, float hMax, float wMin = 0.018f, float wMax = 0.04f, bool nghieng = true)
    {
        float x = x0; int i = 0;
        while (x < x1 - wMin)
        {
            float w = Mathf.Min(f.Rf(wMin, wMax), x1 - x); float h = f.Rf(hMin, hMax);
            bool last = nghieng && x + w * 1.5f >= x1 - wMin;
            B(f, x + w / 2, y, z, w, h, sau * f.Rf(0.88f, 1f), hex[i++ % hex.Length], 0, last ? -8 : 0, "sach");
            x += w + 0.001f;
        }
    }
    // bát (ngửa hoặc úp), chồng n cái
    static float Bat(F f, float x, float y, float z, float dia, float h, string hex, int n = 1, bool up = false)
    {
        for (int i = 0; i < n; i++)
        {
            C(f, x, y, z, dia, h * 0.9f, hex, "bat");
            if (up) C(f, x, y - 0.0f, z, dia * 0.45f, 0.012f, hex, "chan_bat");
            else C(f, x, y + h * 0.9f - 0.004f, z, dia * 0.8f, 0.006f, "#CFC8B4", "long_bat");
            y += h * 0.5f;
        }
        return y + h * 0.5f;
    }
    static void Dia(F f, float x, float y, float z, float dia, string hex, int n = 1, bool dung = false)
    {
        for (int i = 0; i < n; i++)
        {
            if (dung) CZ(f, x + i * 0.014f, y, z, 0.012f, dia, hex, "dia_dung");
            else C(f, x, y + i * 0.012f, z, dia, 0.01f, hex, "dia");
        }
    }
    static void Chai(F f, float x, float y, float z, float dia, float h, string hex, string cap)
    {
        C(f, x, y, z, dia, h * 0.68f, hex, "chai"); C(f, x, y + h * 0.68f, z, dia * 0.4f, h * 0.26f, hex, "co_chai"); C(f, x, y + h * 0.94f, z, dia * 0.46f, h * 0.06f, cap, "nap_chai");
    }
    static void Hu(F f, float x, float y, float z, float dia, float h, string hex, string lid)
    {
        C(f, x, y, z, dia, h * 0.86f, hex, "hu"); C(f, x, y + h * 0.86f, z, dia * 1.04f, h * 0.14f, lid, "nap_hu");
    }
    static void Hop(F f, float x, float y, float z, float w, float h, float d, string hex, string lid, float yaw = 0)
    {
        B(f, x, y, z, w, h * 0.82f, d, hex, yaw, 0, "hop"); B(f, x, y + h * 0.82f, z, w * 1.02f, h * 0.18f, d * 1.02f, lid, yaw, 0, "nap_hop");
    }
    static void Noi(F f, float x, float y, float z, float dia, float h, string hex)
    {
        C(f, x, y, z, dia, h * 0.9f, hex, "noi"); C(f, x, y + h * 0.9f, z, dia * 1.03f, h * 0.1f, hex, "nap_noi");
        S(f, x, y + h, z, dia * 0.12f, "#2A2A2A", 0.8f, "nut_noi");
        B(f, x - dia * 0.58f, y + h * 0.55f, z, 0.05f, 0.015f, 0.025f, "#2A2A2A", 0, 0, "quai_noi"); B(f, x + dia * 0.58f, y + h * 0.55f, z, 0.05f, 0.015f, 0.025f, "#2A2A2A", 0, 0, "quai_noi");
    }
    // chùm chìa khoá đồng: vòng 10 mảnh + 4 chìa xoè không đều
    static void ChumChia(F f, float x, float y, float z)
    {
        for (int i = 0; i < 10; i++)
        {
            float a = i * Mathf.PI * 2 / 10; B(f, x + Mathf.Cos(a) * 0.016f, y, z + Mathf.Sin(a) * 0.016f, 0.012f, 0.004f, 0.008f, "#A8A8A2", -a * Mathf.Rad2Deg, 0, "vong_chia");
        }
        float[] ang = { 20, 65, 120, 200 }; float[] len = { 0.06f, 0.055f, 0.06f, 0.05f };
        for (int i = 0; i < 4; i++)
        {
            float a = ang[i] * Mathf.Deg2Rad; var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
            var c = new Vector3(x, y, z) + dir * (0.016f + len[i] / 2);
            Prim(f, PrimitiveType.Cube, new Vector3(c.x, y + 0.002f + (i % 2) * 0.003f, c.z), new Vector3(len[i], 0.003f, 0.011f), i % 2 == 0 ? "#9C7F44" : "#7E6732", Quaternion.Euler(0, -ang[i], 0), "chia");
            Prim(f, PrimitiveType.Cube, new Vector3(c.x + dir.x * len[i] * 0.42f, y + 0.003f, c.z + dir.z * len[i] * 0.42f), new Vector3(0.008f, 0.004f, 0.014f), i % 2 == 0 ? "#9C7F44" : "#7E6732", Quaternion.Euler(0, -ang[i], 0), "rang_chia");
        }
    }
    // [4/10] áo treo VUÔNG GÓC thanh treo như tủ thật (mặt áo nằm theo chiều sâu z, mỏng theo x) — bản cũ treo áo song song mặt tủ, áo 0,36 m xếp cách 0,12 m nên chồng nhau và lòi qua hông tủ.
    // kieu: 0 sơ mi · 1 áo khoác · 2 quần vắt móc · 3 áo dài · 4 áo cộc tay. Bề ngang lớn nhất (kể cả tay áo) ≤ 0,46 m → vừa lòng tủ sâu ≥ 0,48.
    static Transform Nhom(F f, Vector3 p, Quaternion r, string ten)
    {
        var t = new GameObject(ten).transform; t.SetParent(f.box, false);
        t.localPosition = f.P(p.x, p.y, p.z); t.localRotation = f.q * r; return t;
    }
    static GameObject PL(Transform t, PrimitiveType ty, Vector3 c, Vector3 s, string hex, Quaternion r, string ten)
    {
        var g = GameObject.CreatePrimitive(ty); Object.DestroyImmediate(g.GetComponent<Collider>());
        g.name = ten; g.transform.SetParent(t, false); g.transform.localPosition = c; g.transform.localRotation = r; g.transform.localScale = s;
        g.GetComponent<Renderer>().sharedMaterial = M(hex); return g;
    }
    static void AoTreo(F f, float x, float zc, float yRay, string hex, int kieu)
    {
        var I = Quaternion.identity;
        var t = Nhom(f, new Vector3(x, yRay, zc), Quaternion.Euler(f.Rf(-1.2f, 1.2f), f.Rf(-4, 4), 0), "ao_treo");
        // móc sắt vắt qua thanh treo + vai móc gỗ hai bên
        PL(t, PrimitiveType.Cylinder, new Vector3(0, -0.006f, 0), new Vector3(0.005f, 0.018f, 0.005f), "#A8A8A2", I, "moc_sat");
        PL(t, PrimitiveType.Cube, new Vector3(0, 0.016f, 0), new Vector3(0.005f, 0.005f, 0.034f), "#A8A8A2", I, "moc_cong");
        foreach (var s in new[] { -1f, 1f })
            PL(t, PrimitiveType.Cube, new Vector3(0, -0.045f, s * 0.093f), new Vector3(0.012f, 0.014f, 0.2f), "#8A6A3E", Quaternion.Euler(s * 14f, 0, 0), "vai_moc");
        if (kieu == 2)
        { // quần vắt qua thanh dưới của móc: hai nửa gập + nếp gấp tròn
            PL(t, PrimitiveType.Cube, new Vector3(0, -0.085f, 0), new Vector3(0.01f, 0.01f, 0.36f), "#8A6A3E", I, "thanh_moc");
            PL(t, PrimitiveType.Cylinder, new Vector3(0, -0.085f, 0), new Vector3(0.034f, 0.15f, 0.034f), hex, Quaternion.Euler(90, 0, 0), "nep_quan");
            foreach (var s in new[] { -1f, 1f })
            {
                PL(t, PrimitiveType.Cube, new Vector3(s * 0.009f, -0.085f - 0.25f, 0), new Vector3(0.015f, 0.5f, 0.30f), hex, I, "ong_quan");
                PL(t, PrimitiveType.Cube, new Vector3(s * 0.017f, -0.085f - 0.3f, 0), new Vector3(0.002f, 0.4f, 0.004f), "#2A2A2A", I, "duong_ly");
            }
            return;
        }
        float th = kieu == 1 ? 0.05f : 0.026f, w = kieu == 1 ? 0.36f : kieu == 3 ? 0.30f : 0.34f;
        float hT = kieu == 1 ? 0.74f : kieu == 3 ? 0.36f : 0.64f, tay = kieu == 1 ? 0.56f : kieu == 3 ? 0.56f : kieu == 4 ? 0.17f : 0.44f, wTay = kieu == 3 ? 0.045f : 0.055f;
        PL(t, PrimitiveType.Cube, new Vector3(0, -0.06f - hT / 2, 0), new Vector3(th, hT, w), hex, I, "than_ao");
        PL(t, PrimitiveType.Cube, new Vector3(0, -0.055f, 0), new Vector3(th + 0.008f, kieu == 3 ? 0.045f : 0.032f, kieu == 3 ? 0.07f : 0.11f), hex, I, "co_ao");
        foreach (var s in new[] { -1f, 1f })
            PL(t, PrimitiveType.Cube, new Vector3(0, -0.07f - tay / 2, s * (w / 2 + wTay / 2 - 0.004f)), new Vector3(th * 0.8f, tay, wTay), hex, Quaternion.Euler(-s * 2.5f, 0, 0), "tay_ao");
        if (kieu == 0 || kieu == 1)   // nẹp khuy / khoá kéo chạy giữa mép trước (mép z+)
            PL(t, PrimitiveType.Cube, new Vector3(0, -0.06f - hT / 2, w / 2 - 0.004f), new Vector3(th + 0.004f, hT * 0.96f, 0.012f), kieu == 1 ? "#B8B0A0" : "#F2F0E8", I, "nep_khuy");
        if (kieu == 3)   // áo dài: hai tà trước / sau rủ từ eo, hơi tách nhau
            foreach (var s in new[] { -1f, 1f })
                PL(t, PrimitiveType.Cube, new Vector3(s * 0.007f, -0.06f - hT - 0.25f, 0), new Vector3(0.012f, 0.5f, 0.27f), hex, Quaternion.Euler(0, 0, s * 1.5f), "ta_ao");
    }
    static void Giay(F f, float x, float y, float z, string hex, float yaw)
    {
        foreach (var s in new[] { -1f, 1f })
        {
            var rot = Quaternion.Euler(0, yaw, 0); var off = rot * new Vector3(s * 0.065f, 0, 0);
            Prim(f, PrimitiveType.Cube, new Vector3(x + off.x, y + 0.01f, z + off.z), new Vector3(0.09f, 0.02f, 0.26f), "#2A2A2A", rot, "de_giay");
            var up = rot * new Vector3(0, 0, -0.04f); Prim(f, PrimitiveType.Cube, new Vector3(x + off.x + up.x, y + 0.055f, z + off.z + up.z), new Vector3(0.085f, 0.07f, 0.17f), hex, rot, "than_giay");
            var tx = rot * new Vector3(0, 0, 0.07f); Prim(f, PrimitiveType.Cube, new Vector3(x + off.x + tx.x, y + 0.04f, z + off.z + tx.z), new Vector3(0.08f, 0.04f, 0.1f), hex, rot, "mui_giay");
        }
    }
    // xấp giấy (cream) + dây chun
    static float XapGiay(F f, float x, float y, float z, float w, float d, float h, string hex = "#E8E0C8", float yaw = 0, bool chun = false)
    {
        float t = B(f, x, y, z, w, h, d, hex, yaw, 0, "xap_giay");
        if (chun) B(f, x, y, z, w * 1.01f, h * 1.01f, 0.012f, "#B8453A", yaw, 0, "day_chun");
        return t;
    }

    // ───── khung theo ngăn
    static F Khung(Transform parent, Vector3 o, Quaternion q, float w, float d, float h, int seed)
    {
        var box = parent.Find(Nut);
        if (!box) { box = new GameObject(Nut).transform; box.SetParent(parent, false); }
        return new F { box = box, o = o, q = q, w = w, d = d, h = h, r = new System.Random(seed) };
    }

    static readonly Dictionary<string, int> thuTu = new();
    public static void Reset() { thuTu.Clear(); mats.Clear(); }
    static int Seed(string s) { int h = 17; foreach (var c in s) h = h * 31 + c; return h; }

    // lòng tủ có cánh: chia theo đợt `ke` thành các ngăn rồi bày đồ theo tên tủ
    public static int Hoc(string ten, int idx, int inst, Transform g, float xa, float xb, float y0, float y1, float zMat, float zDay, float[] ke, int truoc)
    {
        var ys = new List<float> { y0 + 0.01f }; if (ke != null) foreach (var k in ke.OrderBy(v => v)) { ys.Add(k + 0.01f); }
        var tops = new List<float>(); if (ke != null) foreach (var k in ke.OrderBy(v => v)) tops.Add(k - 0.01f); tops.Add(y1 - 0.004f);
        int n = 0;
        var q = Quaternion.LookRotation(new Vector3(0, 0, truoc), Vector3.up);
        for (int i = 0; i < ys.Count; i++)
        {
            float w = (xb - xa) - 0.03f, d = Mathf.Abs(zMat - zDay) - 0.045f, h = tops[i] - ys[i];
            var o = new Vector3((xa + xb) / 2, ys[i], zDay + truoc * 0.012f);
            var f = Khung(g, o, q, w, d, h, Seed(ten) + idx * 7 + i * 13 + inst * 101);
            n += Dien(ten, idx, i, inst, f);
        }
        return n;
    }

    // ngăn kéo: o = giữa đáy sát lưng ngăn (toạ độ cánh), fz = hướng ra trước (toạ độ cánh)
    public static int Ngan(string ten, int k, int inst, Transform leaf, Vector3 baseP, Vector3 fz, float w, float sau, float hgt)
    {
        var q = Quaternion.LookRotation(fz, Vector3.up);
        var o = baseP - fz * (sau / 2 - 0.005f) + Vector3.up * 0.006f;
        var f = Khung(leaf, o, q, w - 0.025f, sau - 0.025f, hgt - 0.006f, Seed(ten) + k * 31 + inst * 7);
        return Ngan(ten, k, inst, f);
    }

    // ───── tủ chỉ có đồ (không khoét): TuAo_Khoi — hộp x [xa,xb], y [y0,y1], z phía lưng zDay → phía cánh zMat
    public static int ChiDo(string ten, int inst, Transform g, float xa, float xb, float y0, float y1, float zMat, float zDay, int truoc)
    {
        var q = Quaternion.LookRotation(new Vector3(0, 0, truoc), Vector3.up);
        var f = Khung(g, new Vector3((xa + xb) / 2, y0, zDay + truoc * 0.01f), q, xb - xa - 0.03f, Mathf.Abs(zMat - zDay) - 0.04f, y1 - y0, Seed(ten) + inst * 101);
        return Dien(ten, 0, 0, inst, f);
    }

    static int dem;
    static int Dien(string ten, int idx, int cp, int inst, F f)
    {
        int truoc = f.box.childCount; dem = 0;
        float W = f.w, D = f.d, H = f.h, hw = W / 2;
        switch (ten)
        {
            case "KetSat_KhoaSo":
                { // xấp giấy tờ nhà đất kẹp bìa xanh + chùm chìa (đúng mô tả két trong kịch bản)
                    float dd = Mathf.Min(D - 0.02f, 0.27f);
                    float t = XapGiay(f, -0.04f, 0f, D * 0.5f, 0.2f, dd, 0.05f, "#E8E0C8", 4);
                    t = B(f, -0.04f, t, D * 0.5f, 0.205f, 0.006f, dd + 0.005f, "#3E5A7A", 4, 0, "bia_xanh");
                    B(f, 0.1f, 0f, D * 0.45f, 0.1f, 0.003f, 0.2f, "#D6CDB0", -12, 0, "phong_bi");
                    ChumChia(f, 0.11f, 0.004f, D * 0.5f);
                    B(f, 0.12f, 0.007f, D * 0.76f, 0.07f, 0.02f, 0.045f, "#8E3B34", 18, 0, "so_nho");
                    break;
                }
            case "TuBuffet_ChenBat":
                if (cp == 0)
                { // đáy: nồi, thau, soong
                    Noi(f, -hw + 0.19f, 0, D * 0.55f, 0.30f, 0.17f, "#A9ABA6"); Noi(f, -hw + 0.52f, 0, D * 0.5f, 0.24f, 0.14f, "#8C8F8A");
                    C(f, hw - 0.34f, 0, D * 0.5f, 0.34f, 0.09f, "#E6E2D6", "thau_men"); C(f, hw - 0.34f, 0.085f, D * 0.5f, 0.31f, 0.006f, "#3E6088", "vanh_thau");
                    Noi(f, hw - 0.1f, 0, D * 0.4f, 0.16f, 0.12f, "#B9BCB6");
                    Chai(f, 0.05f, 0, D * 0.8f, 0.07f, 0.26f, "#D9B84A", "#B8453A");
                }
                else
                { // tầng giữa: bát chồng, đĩa dựng, ly, hũ
                    float y = 0.012f;
                    Bat(f, -hw + 0.12f, y, D * 0.55f, 0.15f, 0.07f, "#E9EDEE", 4); Bat(f, -hw + 0.30f, y, D * 0.55f, 0.15f, 0.07f, "#E9EDEE", 3); Bat(f, -hw + 0.47f, y, D * 0.5f, 0.17f, 0.08f, "#D8E2E8", 2, true);
                    for (int i = 0; i < 7; i++) CZ(f, -0.12f + i * 0.016f, y + 0.03f, D * 0.45f, 0.012f, 0.20f, i % 3 == 0 ? "#DDE6EA" : "#EEF0EE", "dia_dung");
                    for (int i = 0; i < 4; i++) C(f, 0.2f + (i % 2) * 0.07f, y, D * 0.35f + (i / 2) * 0.07f, 0.06f, 0.09f, "#CFE0E6", "ly");
                    Hu(f, hw - 0.18f, y, D * 0.55f, 0.20f, 0.14f, "#B8453A", "#D9B84A");
                    Chai(f, hw - 0.06f, y, D * 0.85f, 0.065f, 0.24f, "#7A4A2A", "#2A2A2A"); Hu(f, hw - 0.30f, y, D * 0.85f, 0.08f, 0.1f, "#E6E2D6", "#3E7A4A");
                }
                break;
            case "TuDauGiuong":
                if (inst % 2 == 0)
                { // bên bố: thuốc, dầu, đèn pin, bông băng, sổ
                    Hop(f, -0.1f, 0, D * 0.35f, 0.12f, 0.05f, 0.08f, "#D9D6CA", "#B8453A", 8);
                    B(f, 0.08f, 0, D * 0.3f, 0.06f, 0.004f, 0.1f, "#BFC3C4", 5, 0, "vi_thuoc"); B(f, 0.09f, 0.004f, D * 0.32f, 0.06f, 0.004f, 0.1f, "#C9CDCE", -4, 0, "vi_thuoc");
                    Chai(f, -0.02f, 0, D * 0.7f, 0.035f, 0.09f, "#B0462E", "#F0F0EA"); Chai(f, 0.03f, 0, D * 0.74f, 0.03f, 0.08f, "#3E7A4A", "#F0F0EA");
                    CX(f, 0.02f, 0, D * 0.55f, 0.04f, 0.16f, "#3A3A38", "den_pin"); C(f, 0.11f, 0, D * 0.55f, 0.05f, 0.02f, "#3A3A38", "dau_den_pin");
                    Hop(f, -0.1f, 0, D * 0.72f, 0.1f, 0.07f, 0.09f, "#F2F2EE", "#E8E8E2", -6); B(f, -0.1f, 0.075f, D * 0.72f, 0.03f, 0.001f, 0.03f, "#B8352A", -6, 0, "chu_thap");
                    B(f, -0.02f, 0.005f, D * 0.9f, 0.1f, 0.016f, 0.14f, "#5C3A24", 10, 0, "so_bo");
                }
                else
                { // bên mẹ: kim chỉ, khăn tay, lược, dầu cù là, sổ chi tiêu
                    Hu(f, -0.08f, 0, D * 0.4f, 0.14f, 0.06f, "#C79A3E", "#B8862E");
                    foreach (var (cx, hx) in new[] { (0.05f, "#EDE8D8"), (0.09f, "#B8453A"), (0.13f, "#3A3A38") }) C(f, cx, 0, D * 0.22f, 0.028f, 0.035f, hx, "cuon_chi");
                    Gap(f, 0.08f, D * 0.6f, 0, 0.1f, 0.1f, 0.012f, new[] { "#EDE8D8", "#E6D0D0" }, 2, 0.003f);
                    B(f, -0.12f, 0, D * 0.78f, 0.12f, 0.012f, 0.03f, "#7A5A38", 22, 0, "luoc_go");
                    Hu(f, 0.02f, 0, D * 0.82f, 0.045f, 0.03f, "#3E7A4A", "#2E5A38"); Chai(f, 0.1f, 0, D * 0.85f, 0.03f, 0.07f, "#C9A25E", "#8E3B34");
                    B(f, -0.04f, 0.005f, D * 0.95f, 0.1f, 0.014f, 0.14f, "#8A3B34", -8, 0, "so_chi_tieu");
                }
                break;
            case "TuDoTho_Fix":
                if (cp == 0)
                { // tầng dưới: giấy vàng mã, nhang, hộp nhang, dầu thắp
                    Gap(f, -hw + 0.12f, D * 0.5f, 0, 0.17f, 0.2f, 0.022f, new[] { "#D9B84A", "#CDAA3E" }, 5, 0.004f); Gap(f, -hw + 0.31f, D * 0.5f, 0, 0.17f, 0.2f, 0.022f, new[] { "#CDAA3E", "#D9B84A" }, 3, 0.004f);
                    foreach (var dx in new[] { 0f, 0.04f, 0.08f }) { CZ(f, 0.06f + dx, 0f, D * 0.5f, 0.032f, 0.24f, "#C7A86B", "bo_nhang"); CZ(f, 0.06f + dx, 0f, D * 0.5f + 0.12f, 0.034f, 0.025f, "#9B2D26", "dau_nhang"); }
                    Hop(f, hw - 0.14f, 0, D * 0.5f, 0.12f, 0.07f, 0.22f, "#8E3B34", "#7A2E28", 3);
                    Chai(f, hw - 0.05f, 0, D * 0.82f, 0.055f, 0.15f, "#CFE0D8", "#B8862E");
                }
                else
                { // tầng trên: khăn lau, ly thờ, nến, diêm
                    Gap(f, -hw + 0.12f, D * 0.5f, 0, 0.18f, 0.16f, 0.014f, new[] { "#EDE5D0", "#E2D8C0" }, 3, 0.004f);
                    for (int i = 0; i < 3; i++) C(f, -0.04f + i * 0.07f, 0, D * 0.35f, 0.045f, 0.05f, "#E7ECEF", "ly_tho");
                    for (int i = 0; i < 2; i++) { C(f, 0.2f + i * 0.07f, 0, D * 0.6f, 0.04f, 0.12f, "#E8E2CF", "nen"); C(f, 0.2f + i * 0.07f, 0, D * 0.6f, 0.06f, 0.012f, "#8A6A3E", "de_nen"); }
                    B(f, 0.12f, 0, D * 0.8f, 0.05f, 0.015f, 0.035f, "#B8453A", 25, 0, "hop_diem");
                }
                break;
            case "TuDung_RuongChanMan":
                if (cp == 0)
                { // tầng dưới: chăn bông gấp, gối, chiếu cuộn, màn
                    float t = Gap(f, -0.17f, D * 0.5f, 0, 0.58f, 0.44f, 0.095f, new[] { "#7A5A6A", "#4F6A7A", "#8A6A3E", "#6E7F5A", "#9A6A5A" }, 6, 0.01f);
                    B(f, -0.15f, t, D * 0.5f, 0.50f, 0.13f, 0.30f, "#EDE5D0", 6, 0, "goi"); B(f, -0.18f, t + 0.13f, D * 0.5f, 0.46f, 0.11f, 0.28f, "#E2D8C0", -5, 0, "goi");
                    C(f, hw - 0.1f, 0, D * 0.3f, 0.17f, 0.78f, "#C9B27A", "chieu_cuon"); C(f, hw - 0.1f, 0, D * 0.62f, 0.15f, 0.7f, "#BFA66A", "chieu_cuon");
                    B(f, hw - 0.14f, 0.78f, D * 0.3f, 0.2f, 0.01f, 0.2f, "#C9B27A", 0, 0, "nap_chieu");
                    Gap(f, hw - 0.28f, D * 0.75f, 0, 0.26f, 0.3f, 0.07f, new[] { "#ECECE6", "#E2E2DA" }, 4, 0.006f);
                }
                else
                { // tầng trên: hộp carton, khăn trải giường, túi vải
                    Hop(f, -hw + 0.22f, 0, D * 0.5f, 0.38f, 0.26f, 0.34f, "#B08A5A", "#A07A4A", 4); Hop(f, -hw + 0.62f, 0, D * 0.5f, 0.3f, 0.2f, 0.3f, "#B79463", "#A7835A", -3);
                    Gap(f, hw - 0.2f, D * 0.5f, 0, 0.34f, 0.36f, 0.05f, new[] { "#CFC9B4", "#B9C4B8", "#C9B8A8" }, 5, 0.008f);
                }
                break;
            case "TuLanh_Bo":
                if (cp == 0)
                { // ngăn rau củ
                    float y = 0.01f; hw = W / 2;
                    for (int i = 0; i < 3; i++) CZ(f, -hw + 0.09f + i * 0.045f, y, D * 0.35f, 0.04f, 0.2f, "#E08A2E", "ca_rot");
                    S(f, hw - 0.16f, y, D * 0.35f, 0.17f, "#5E8A4A", 0.85f, "bap_cai"); S(f, hw - 0.30f, y, D * 0.4f, 0.12f, "#7AAA5A", 0.9f, "xa_lach");
                    for (int i = 0; i < 3; i++) CZ(f, -0.12f + i * 0.03f, y, D * 0.78f, 0.03f, 0.26f, "#4E8A3E", "rau_muong");
                    for (int i = 0; i < 4; i++) S(f, hw - 0.34f + (i % 2) * 0.06f, y, D * 0.8f - (i / 2) * 0.06f, 0.06f, "#C43A2F", 0.9f, "ca_chua");
                    S(f, -hw + 0.1f, y, D * 0.72f, 0.06f, "#E6A03A", 1f, "cam"); S(f, -hw + 0.17f, y, D * 0.78f, 0.06f, "#E6A03A", 1f, "cam");
                }
                else if (cp == 1)
                { // hộp thức ăn có đậy, tô úp đĩa, chai nước
                    Hop(f, -hw + 0.12f, 0.01f, D * 0.35f, 0.18f, 0.09f, 0.14f, "#EAEAE3", "#4E8AD0", 3); Hop(f, -hw + 0.13f, 0.01f, D * 0.72f, 0.16f, 0.08f, 0.12f, "#EAEAE3", "#E68AB0", -4);
                    Bat(f, 0.04f, 0.01f, D * 0.4f, 0.17f, 0.075f, "#E9EDEE", 1); C(f, 0.04f, 0.085f, D * 0.4f, 0.19f, 0.008f, "#D8E2E8", "dia_dap");
                    Chai(f, hw - 0.1f, 0.01f, D * 0.3f, 0.075f, 0.30f, "#CFE0E6", "#4E8AD0"); Chai(f, hw - 0.1f, 0.01f, D * 0.7f, 0.075f, 0.28f, "#CFE0E6", "#B8453A");
                }
                else
                { // trứng, hũ tương, sữa
                    B(f, -hw + 0.12f, 0.01f, D * 0.45f, 0.2f, 0.04f, 0.12f, "#C9B28A", 4, 0, "vi_trung");
                    for (int i = 0; i < 6; i++) S(f, -hw + 0.06f + (i % 3) * 0.06f, 0.05f, D * 0.45f - 0.03f + (i / 3) * 0.06f, 0.045f, "#EAD9B8", 1.2f, "trung");
                    Hu(f, 0.05f, 0.01f, D * 0.35f, 0.08f, 0.1f, "#7A4A2A", "#E6E2D6"); Hu(f, 0.05f, 0.01f, D * 0.7f, 0.08f, 0.09f, "#C43A2F", "#E6E2D6");
                    B(f, hw - 0.14f, 0.01f, D * 0.4f, 0.08f, 0.14f, 0.06f, "#F0F0EC", 0, 0, "hop_sua"); B(f, hw - 0.14f, 0.15f, D * 0.4f, 0.082f, 0.05f, 0.062f, "#4E8AD0", 0, 0, "dinh_hop_sua");
                    B(f, hw - 0.1f, 0.01f, D * 0.8f, 0.1f, 0.05f, 0.06f, "#E8D890", 0, 0, "bo");
                }
                break;
            case "TuNhua_Nhim":
                Gap(f, -0.11f, D * 0.35f, 0, 0.14f, 0.16f, 0.025f, new[] { "#E6C45A", "#5E8A7E", "#D98E8E", "#9DB7C9" }, 5, 0.004f);
                Gap(f, 0.1f, D * 0.35f, 0, 0.14f, 0.16f, 0.03f, new[] { "#3E5A8A", "#5E7A9A" }, 3, 0.004f);
                for (int i = 0; i < 6; i++) B(f, -0.12f + (i % 3) * 0.045f, 0, D * 0.78f + (i / 3) * 0.045f, 0.04f, 0.04f, 0.04f, new[] { "#D9483A", "#F2C040", "#4E8AD0", "#5EAA5A", "#E68AB0", "#E6C45A" }[i], i * 17, 0, "khoi_go");
                B(f, 0.1f, 0, D * 0.75f, 0.1f, 0.022f, 0.13f, "#F2C040", 8, 0, "hop_sap");
                for (int i = 0; i < 5; i++) C(f, 0.07f + i * 0.015f, 0.022f, D * 0.75f, 0.011f, 0.03f, new[] { "#D9483A", "#F2C040", "#4E8AD0", "#5EAA5A", "#E68AB0" }[i], "sap_mau");
                B(f, 0.0f, 0, D * 0.55f, 0.14f, 0.012f, 0.19f, "#A8443A", -6, 0, "vo"); B(f, 0.01f, 0.012f, D * 0.55f, 0.14f, 0.012f, 0.19f, "#3E6088", 3, 0, "vo");
                break;
            case "TuQuanAo_GamHo":
                if (cp == 0)
                { // thanh treo + áo, quần, váy; dưới đáy hộp carton, giày
                    float yR = H - 0.07f;
                    CX(f, 0, yR, D * 0.5f, 0.025f, W - 0.04f, "#A8A8A2", "thanh_treo");
                    foreach (var s2 in new[] { -1f, 1f }) B(f, s2 * (W / 2 - 0.012f), yR - 0.02f, D * 0.5f, 0.02f, 0.06f, 0.04f, "#8C8F8A", 0, 0, "gia_thanh");
                    // [4/10] áo treo vuông góc thanh treo: sơ mi bố, áo khoác, quần tây, áo dài của mẹ — chừa 7 cm hai đầu thanh
                    string[] mau = { "#EDEAE0", "#4E6A8A", "#3A3A38", "#E6DCC8", "#8A6A4A", "#C98E8E", "#3E5A7A", "#D8CFB8", "#6E7F5A", "#B8B0A0", "#7A5A6A" };
                    int[] kd = { 0, 1, 2, 3, 0, 4, 2, 0, 1, 0, 4 };
                    float buoc = (W - 0.16f) / (mau.Length - 1);
                    for (int i = 0; i < mau.Length; i++) AoTreo(f, -hw + 0.08f + i * buoc + f.Rf(-0.012f, 0.012f), D * 0.5f + f.Rf(-0.004f, 0.004f), yR, mau[i], kd[i]);
                    Hop(f, -hw + 0.2f, 0, D * 0.55f, 0.34f, 0.2f, 0.32f, "#B08A5A", "#A07A4A", 4); Hop(f, hw - 0.2f, 0, D * 0.5f, 0.32f, 0.2f, 0.3f, "#B79463", "#A7835A", -4);
                    Giay(f, 0.0f, 0, D * 0.5f, "#6B4A2E", 4);
                }
                else
                {
                    Gap(f, -hw + 0.2f, D * 0.5f, 0, 0.36f, 0.4f, 0.07f, new[] { "#6E7F5A", "#8A6A3E", "#B9B4A0" }, 3, 0.008f);
                    Gap(f, 0f, D * 0.5f, 0, 0.3f, 0.34f, 0.05f, new[] { "#7A5A6A", "#4F6A7A" }, 2, 0.008f);
                    Hop(f, hw - 0.18f, 0, D * 0.5f, 0.28f, 0.15f, 0.24f, "#CFC9B4", "#BFB9A4", 6);
                }
                break;
            case "TuAo_Khoi_Treo":
                { // [4/10] ngăn treo tủ áo Khôi: sơ mi trắng đi học, áo khoác bò, quần tây, áo đá bóng — đáy để giày thể thao + túi thể thao
                    float yR = H - 0.07f;
                    CX(f, 0, yR, D * 0.5f, 0.022f, W - 0.02f, "#A8A8A2", "thanh_treo");
                    string[] mau = { "#EDEAE0", "#9DB7C9", "#3E5A7A", "#3A3A38", "#B8453A", "#6E7F5A" };
                    int[] kd = { 0, 0, 1, 2, 4, 1 };
                    float buoc = (W - 0.1f) / (mau.Length - 1);
                    for (int i = 0; i < mau.Length; i++) AoTreo(f, -hw + 0.05f + i * buoc, D * 0.5f + f.Rf(-0.004f, 0.004f), yR, mau[i], kd[i]);
                    Giay(f, -0.08f, 0, D * 0.73f, "#D8D6CC", 6);
                    CX(f, 0.04f, 0, D * 0.22f, 0.2f, 0.34f, "#2E4A7A", "tui_the_thao"); B(f, 0.04f, 0.19f, D * 0.22f, 0.3f, 0.012f, 0.03f, "#1E2A3A", 0, 0, "quai_tui");
                    break;
                }
            case "TuAo_Khoi_Ke":
                if (cp == 0) { Gap(f, -0.09f, D * 0.5f, 0, 0.3f, 0.34f, 0.045f, new[] { "#3E5A7A", "#4E6A8A", "#2E4A6A" }, 4, 0.006f); Hop(f, 0.12f, 0, D * 0.55f, 0.14f, 0.12f, 0.3f, "#B08A5A", "#A07A4A", 3); }
                else if (cp == 1) { Gap(f, -0.08f, D * 0.5f, 0, 0.3f, 0.32f, 0.03f, new[] { "#EDEAE0", "#D8D6CC", "#9DB7C9", "#EDEAE0" }, 6, 0.005f); Gap(f, 0.13f, D * 0.5f, 0, 0.12f, 0.2f, 0.02f, new[] { "#B8453A", "#2A2A2A" }, 4, 0.004f); }
                else if (cp == 2)
                {
                    Sach(f, -hw + 0.02f, 0.06f, 0, D * 0.35f, 0.17f, new[] { "#3E6088", "#A8443A", "#6F8A5E", "#D9B45E", "#5C3A24" }, 0.2f, 0.26f, 0.02f, 0.035f);
                    Hop(f, 0.13f, 0, D * 0.5f, 0.12f, 0.1f, 0.18f, "#8E6A3A", "#7A5A30", -6);   // hộp thiếc đựng thư
                    for (int i = 0; i < 4; i++) B(f, 0.13f, 0.1f + i * 0.016f, D * 0.5f, 0.07f, 0.014f, 0.11f, i % 2 == 0 ? "#2A2A2A" : "#CFC9B4", f.Rf(-8, 8), 0, "bang_cassette");
                }
                else
                {
                    Gap(f, -0.06f, D * 0.5f, 0, 0.34f, 0.4f, 0.07f, new[] { "#7A5A6A", "#C9B8A8" }, 2, 0.008f);
                    S(f, 0.14f, 0, D * 0.4f, 0.17f, "#2A3A5A", 0.45f, "mu_luoi_trai"); B(f, 0.14f, 0.0f, D * 0.4f + 0.1f, 0.13f, 0.01f, 0.08f, "#2A3A5A", 0, 0, "luoi_trai");
                }
                break;
            case "TuThap_Sanh":
                if (cp == 0)
                {
                    Giay(f, -hw + 0.15f, 0, D * 0.5f, "#3E5A8A", 3); Giay(f, 0.08f, 0, D * 0.5f, "#B8483A", -4);
                    B(f, hw - 0.17f, 0, D * 0.5f, 0.28f, 0.1f, 0.2f, "#B08A5A", 5, 0, "hop_giay");
                }
                else
                {
                    Hu(f, -hw + 0.12f, 0, D * 0.5f, 0.2f, 0.1f, "#B8453A", "#D9B84A");
                    B(f, -0.06f, 0, D * 0.5f, 0.22f, 0.04f, 0.28f, "#5C3A24", 6, 0, "album_anh"); B(f, -0.04f, 0.04f, D * 0.5f, 0.2f, 0.004f, 0.26f, "#CFC8B4", 6, 0, "bia_album");
                    CX(f, hw - 0.2f, 0, D * 0.4f, 0.04f, 0.16f, "#3A3A38", "den_pin"); Gap(f, hw - 0.12f, D * 0.75f, 0, 0.14f, 0.12f, 0.02f, new[] { "#EDE8D8" }, 3, 0.004f);
                }
                break;
            case "TuTV_Dung_Bo":
                { // băng cassette, băng video, đĩa VCD, ổn áp, điều khiển
                    var xc = new[] { "#CFC9B4", "#2A2A2A", "#B8B0A0", "#D8D2C0", "#3A3A38" };
                    Sach(f, -hw + 0.04f, -hw + 0.46f, 0, D * 0.3f, 0.1f, xc, 0.1f, 0.115f, 0.014f, 0.016f, false);
                    for (int i = 0; i < 9; i++) B(f, -hw + 0.52f + i * 0.034f, 0, D * 0.32f, 0.03f, 0.19f, 0.1f, i % 3 == 2 ? "#4A4A46" : "#2A2A2A", 0, 0, "bang_video");
                    float t = 0; for (int i = 0; i < 6; i++) t = B(f, hw - 0.28f, t, D * 0.3f, 0.14f, 0.011f, 0.14f, "#CFE0E6", f.Rf(-4, 4), 0, "hop_vcd");
                    B(f, 0.05f, 0, D * 0.6f, 0.22f, 0.09f, 0.16f, "#3A3A38", 3, 0, "on_ap"); B(f, 0.05f, 0.09f, D * 0.6f, 0.2f, 0.004f, 0.14f, "#BFC3C4", 3, 0, "mat_on_ap");
                    C(f, hw - 0.14f, 0, D * 0.65f, 0.17f, 0.045f, "#2A2A2A", "cuon_day_dien"); C(f, hw - 0.14f, 0.044f, D * 0.65f, 0.07f, 0.004f, "#3A3A38", "long_cuon_day");
                    B(f, hw - 0.4f, 0, D * 0.75f, 0.05f, 0.016f, 0.18f, "#2A2A2A", 12, 0, "dieu_khien");
                    B(f, -0.3f, 0, D * 0.78f, 0.34f, 0.025f, 0.2f, "#5C3A24", -3, 0, "sach_huong_dan");
                }
                break;
            case "Chan_BatDiaTrongChan":
                C(f, -hw + 0.18f, 0, D * 0.5f, 0.32f, 0.1f, "#BFC3C4", "thau_nhom"); Noi(f, -hw + 0.55f, 0, D * 0.5f, 0.22f, 0.16f, "#8C8F8A"); Noi(f, -hw + 0.55f, 0.16f, D * 0.5f, 0.16f, 0.1f, "#A9ABA6");
                Chai(f, hw - 0.2f, 0, D * 0.4f, 0.075f, 0.26f, "#D9B84A", "#B8453A"); Chai(f, hw - 0.08f, 0, D * 0.5f, 0.07f, 0.24f, "#7A4A2A", "#2A2A2A"); Chai(f, hw - 0.14f, 0, D * 0.75f, 0.07f, 0.24f, "#7A4A2A", "#2A2A2A");
                C(f, 0.05f, 0, D * 0.5f, 0.3f, 0.12f, "#5E8A7E", "ro_nhua"); Hu(f, 0.0f, 0, D * 0.85f, 0.12f, 0.12f, "#E6E2D6", "#3E7A4A");
                break;
            case "BanTrangDiem_Bo":
                Hu(f, -0.05f, 0, D * 0.5f, 0.09f, 0.04f, "#C79A3E", "#B8862E"); foreach (var (cx, hx) in new[] { (0.04f, "#EDE8D8"), (0.07f, "#B8453A") }) C(f, cx, 0, D * 0.3f, 0.026f, 0.03f, hx, "cuon_chi");
                B(f, 0.0f, 0, D * 0.8f, 0.12f, 0.01f, 0.03f, "#7A5A38", 15, 0, "luoc_go"); B(f, 0.05f, 0, D * 0.62f, 0.07f, 0.012f, 0.07f, "#EDE8D8", 8, 0, "khan_tay");
                break;
            case "BanCoHoc_Go": return DienBan(ten, cp, f);
            case "BanHoc_Khoi_Bo": return DienBan(ten, cp, f);
            case "BanGiay_Go": return DienBan(ten, cp, f);
            case "TuHoSo_Sat": return DienHoSo(cp, f);
        }
        return f.box.childCount - truoc;
    }

    // ngăn kéo: tên tủ + số thứ tự cánh k (1..) → bày đồ
    static int Ngan(string ten, int k, int inst, F f)
    {
        int truoc = f.box.childCount;
        switch (ten)
        {
            case "TuHoSo_Sat": DienHoSo(k, f); break;
            case "BanTrangDiem_Bo": Dien(ten, 0, 0, inst, f); break;
            case "BanCoHoc_Go": case "BanHoc_Khoi_Bo": case "BanGiay_Go": DienBan(ten, k, f); break;
        }
        return f.box.childCount - truoc;
    }

    static int DienHoSo(int k, F f)
    {
        int truoc = f.box.childCount; float W = f.w, D = f.d, hw = W / 2; float H = Mathf.Min(f.h, 0.22f);
        var bia = new[] { "#3E6088", "#A8443A", "#6F8A5E", "#D9B45E", "#4A4A46" };
        var kem = new[] { "#E8E0C8", "#D6CDB0", "#EDE8D8" };
        switch (k)
        {
            case 1: Sach(f, -hw + 0.02f, -0.02f, 0, D * 0.5f, D * 0.8f, bia, H * 0.85f, H * 0.95f, 0.05f, 0.07f, false); XapGiay(f, 0.15f, 0, D * 0.5f, 0.22f, 0.28f, 0.06f, "#D6CDB0", 2); break;
            case 2: B(f, -0.1f, 0, D * 0.5f, 0.26f, 0.015f, 0.3f, "#C9B27A", 3, 0, "bia_ho_so"); Gap(f, -0.1f, D * 0.5f, 0.015f, 0.24f, 0.28f, 0.01f, kem, 8, 0.004f); C(f, 0.18f, 0, D * 0.4f, 0.14f, 0.03f, "#CFE0E6", "tui_nilon"); Gap(f, 0.18f, D * 0.7f, 0, 0.12f, 0.14f, 0.02f, new[] { "#D9D9D4" }, 3, 0.004f); break;
            case 3: XapGiay(f, -0.12f, 0, D * 0.5f, 0.2f, 0.27f, 0.07f, "#E8E0C8", 3, true); XapGiay(f, 0.12f, 0, D * 0.5f, 0.2f, 0.27f, 0.05f, "#D6CDB0", -4, true); break;
            case 4: { // [3/10 v3.0] chồng sổ thu chi gia đình 1997–2002 (mẹ ghi), cuốn 2002 trên cùng — chứa dòng "Ứng cho thằng Phong — 1.980.000" (mã két)
                    float t = 0; var mau = new[] { "#7A3A30", "#3E5A7A", "#5C3A24", "#6F8A5E", "#8E6A3A" };
                    for (int i = 0; i < 5; i++) t = B(f, -0.06f + f.Rf(-0.006f, 0.006f), t, D * 0.5f, 0.21f, 0.018f, 0.30f, mau[i], f.Rf(-4, 4), 0, "SoThuChi_" + (1997 + i));
                    B(f, -0.06f, t, D * 0.5f, 0.21f, 0.018f, 0.30f, "#5E86A8", 3, 0, "SoThuChi_2002");
                    B(f, 0.18f, 0, D * 0.5f, 0.16f, 0.03f, 0.22f, "#6F8A5E", 8, 0, "so_nho"); break; }
            case 5: Sach(f, -hw + 0.02f, hw - 0.02f, 0, D * 0.5f, D * 0.88f, new[] { "#E8E0C8", "#B9C4A0", "#E8E0C8", "#C9B27A" }, H * 0.8f, H * 0.95f, 0.006f, 0.012f); break;
            case 6: Hop(f, -0.12f, 0, D * 0.5f, 0.22f, 0.12f, 0.28f, "#B08A5A", "#A07A4A", 3); XapGiay(f, 0.15f, 0, D * 0.5f, 0.18f, 0.24f, 0.035f, "#EDE8D8", -6); break;
            case 7: C(f, -hw + 0.07f, 0, D * 0.3f, 0.07f, 0.1f, "#3A3A38", "hop_but"); for (int i = 0; i < 5; i++) CZ(f, -hw + 0.04f + i * 0.012f, 0.1f, D * 0.3f, 0.008f, 0.12f, new[] { "#3E6088", "#A8443A", "#2A2A2A", "#D9B45E", "#3E6088" }[i], "but");
                B(f, -0.12f, 0, D * 0.6f, 0.1f, 0.04f, 0.07f, "#BFC3C4", 0, 0, "hop_ghim"); C(f, 0.0f, 0, D * 0.35f, 0.075f, 0.05f, "#CFC4A0", "bang_keo"); C(f, 0.12f, 0, D * 0.35f, 0.05f, 0.07f, "#8E3B34", "con_dau"); B(f, 0.12f, 0, D * 0.62f, 0.08f, 0.012f, 0.06f, "#8E3B34", 0, 0, "de_con_dau");
                B(f, 0.18f, 0, D * 0.75f, 0.06f, 0.03f, 0.1f, "#3A3A38", 14, 0, "bam_kim"); break;
            default: B(f, -0.12f, 0, D * 0.5f, 0.12f, 0.02f, 0.18f, "#8A3B34", 10, 0, "so_nho"); XapGiay(f, 0.06f, 0, D * 0.4f, 0.16f, 0.2f, 0.01f, "#D6CDB0", -5); B(f, 0.2f, 0, D * 0.8f, 0.07f, 0.008f, 0.1f, "#EDE8D8", 8, 0, "tem"); break;
        }
        return f.box.childCount - truoc;
    }

    static int DienBan(string ten, int k, F f)
    {
        int truoc = f.box.childCount; float W = f.w, D = f.d, hw = W / 2;
        switch (ten)
        {
            case "BanCoHoc_Go":
                if (k == 1)
                { // dưới cùng: hồ sơ, album ảnh
                    XapGiay(f, -0.07f, 0, D * 0.5f, 0.16f, 0.24f, 0.045f, "#E8E0C8", 3); B(f, -0.07f, 0.045f, D * 0.5f, 0.165f, 0.006f, 0.245f, "#3E5A7A", 3, 0, "bia_xanh");
                    B(f, 0.09f, 0, D * 0.5f, 0.12f, 0.04f, 0.26f, "#5C3A24", -4, 0, "album_anh");
                }
                else if (k == 2)
                { // giữa: sổ hộ khẩu, phong bì, hoá đơn
                    B(f, -0.1f, 0, D * 0.35f, 0.09f, 0.012f, 0.13f, "#8E3B34", 8, 0, "so_ho_khau"); B(f, -0.08f, 0.012f, D * 0.38f, 0.085f, 0.012f, 0.12f, "#4E6A3E", -5, 0, "so_xanh");
                    Gap(f, 0.07f, D * 0.5f, 0, 0.12f, 0.2f, 0.005f, new[] { "#D6CDB0", "#EDE8D8" }, 6, 0.004f); XapGiay(f, 0.0f, 0, D * 0.82f, 0.2f, 0.07f, 0.02f, "#EDE8D8", 2, true);
                }
                else
                { // trên cùng: giấy lót ố, bút, kéo, hoá đơn gấp — chừa 0,15 × 0,22 m bên phải cho cuốn sổ
                    B(f, -0.04f, 0, D * 0.5f, 0.26f, 0.002f, 0.32f, "#D6C9A6", 4, 0, "giay_lot");
                    CX(f, -0.1f, 0.002f, D * 0.2f, 0.011f, 0.14f, "#3E6088", "but_bi"); CX(f, -0.09f, 0.002f, D * 0.28f, 0.011f, 0.14f, "#A8443A", "but_bi");
                    B(f, -0.12f, 0.002f, D * 0.55f, 0.09f, 0.004f, 0.02f, "#BFC3C4", 25, 0, "keo_thanh"); B(f, -0.14f, 0.002f, D * 0.6f, 0.03f, 0.006f, 0.03f, "#B8453A", 25, 0, "keo_cam");
                    for (int i = 0; i < 3; i++) B(f, -0.1f + i * 0.04f, 0.002f, D * 0.8f, 0.06f, 0.006f + i * 0.002f, 0.09f, "#EDE8D8", i * 14, 0, "hoa_don_gap");
                    B(f, -0.02f, 0.002f, D * 0.28f, 0.05f, 0.03f, 0.04f, "#BFC3C4", 0, 0, "hop_ghim");
                    B(f, 0.09f, 0.002f, D * 0.5f, 0.15f, 0.002f, 0.21f, "#F2F0E8", -6, 0, "giay_kham_Nhim");   // [3/10 v3.0] chỗ cuốn sổ cũ — giờ là giấy khám của Nhím
                }
                break;
            case "BanHoc_Khoi_Bo":
                for (int i = 0; i < 4; i++) CX(f, -0.04f, 0.008f * i, D * 0.2f + 0.012f * i, 0.008f, 0.17f, new[] { "#F2C040", "#3E6088", "#A8443A", "#5EAA5A" }[i], "but_chi");
                B(f, 0.0f, 0, D * 0.62f, 0.14f, 0.011f, 0.19f, "#3E6088", 4, 0, "vo"); B(f, -0.01f, 0.011f, D * 0.62f, 0.14f, 0.011f, 0.19f, "#A8443A", -5, 0, "vo");
                B(f, 0.1f, 0, D * 0.3f, 0.04f, 0.016f, 0.022f, "#EDE8D8", 0, 0, "tay"); B(f, 0.06f, 0, D * 0.82f, 0.2f, 0.004f, 0.03f, "#D9B45E", 0, 0, "thuoc_ke");
                B(f, -0.1f, 0, D * 0.82f, 0.05f, 0.012f, 0.05f, "#BFC3C4", 20, 0, "compa");
                break;
            case "BanGiay_Go":
                { float t = B(f, -0.12f, 0, D * 0.5f, 0.24f, 0.04f, Mathf.Min(0.3f, D - 0.02f), "#5C3A24", 2, 0, "so_cai"); t = B(f, -0.11f, t, D * 0.5f, 0.23f, 0.035f, Mathf.Min(0.3f, D - 0.02f), "#3E5A7A", -2, 0, "so_cai"); B(f, -0.12f, t, D * 0.5f, 0.24f, 0.04f, 0.3f, "#7A3A30", 4, 0, "so_cai");
                  Hu(f, 0.12f, 0, D * 0.3f, 0.07f, 0.05f, "#8E3B34", "#7A2E28"); XapGiay(f, 0.15f, 0, D * 0.68f, 0.14f, 0.18f, 0.025f, "#EDE8D8", -8, true); CX(f, 0.12f, 0, D * 0.15f, 0.011f, 0.14f, "#3E6088", "but_bi"); }
                break;
        }
        return f.box.childCount - truoc;
    }
}
