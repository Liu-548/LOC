using UnityEngine;

// [4/10 chiều] sắp xếp lại + tăng chi tiết:
//  · thùng sơn kho dựng từng thùng đúng cỡ (file ThungSon.glb là cụm 0,69 m, ThungSon_Cum2 là cụm 1,02 m — xếp theo bước 0,31 m nên chồng nhau và lòi qua tường nhà sang bếp tới 10 cm)
//  · tủ áo Khôi dựng lại bằng gỗ có chân tiện, gờ nóc, pano cánh, tay nắm đồng, lòng tủ có đồ (bản .glb cũ là hộp trơn, rỗng)
//  · móc khăn treo tường (bản .glb cũ chỉ là ba tấm phẳng) · phòng Nhím: rổ đồ chơi, kệ sách treo tường
public static partial class LocHouseBuilder
{
    static readonly System.Random rSon = new(404);
    static float RS(float a, float b) => a + (float)rSon.NextDouble() * (b - a);
    static readonly string[][] SonNhan = { new[] { "#3E7A4A", "#F2F0E6" }, new[] { "#2E5A9A", "#F2C94C" }, new[] { "#B8453A", "#F2F0E6" }, new[] { "#D9A23A", "#2A2A2A" } };
    static readonly string[] SonVet = { "#E8E2D0", "#C9A25E", "#6F8A5E", "#9FB7C9", "#D98E6A" };

    // một thùng sơn kim loại: lớn 18 L (Ø 0,29 · cao 0,37 cả nắp) hoặc nhỏ 4 L (Ø 0,17 · 0,20). y = đáy. Trả về chiều cao.
    static float ThungSonDon(Transform p, float x, float z, float y, bool lon, int nhan, bool quai = true)
    {
        float d = lon ? 0.29f : 0.17f, h = lon ? 0.345f : 0.18f;
        var g = new GameObject("ThungSonDon").transform; g.SetParent(p, false);
        g.SetPositionAndRotation(new Vector3(x, y, z), Quaternion.Euler(0, RS(0, 360), 0));
        var than = M("M_Son_Than", "#CFD0CA"); var nap = M("M_Son_Nap", "#A9ABA6");
        var sc = SonNhan[((nhan % SonNhan.Length) + SonNhan.Length) % SonNhan.Length];
        Cb(g, "than", new Vector3(0, h / 2, 0), new Vector3(d, h / 2, d), than, PrimitiveType.Cylinder);
        Cb(g, "nhan", new Vector3(0, h * 0.47f, 0), new Vector3(d + 0.004f, h * 0.31f, d + 0.004f), M("M_Son_Nhan_" + sc[0].TrimStart('#'), sc[0]), PrimitiveType.Cylinder);
        Cb(g, "soc_nhan", new Vector3(0, h * 0.53f, 0), new Vector3(d + 0.006f, h * 0.045f, d + 0.006f), M("M_Son_Soc_" + sc[1].TrimStart('#'), sc[1]), PrimitiveType.Cylinder);
        Cb(g, "vanh_day", new Vector3(0, 0.006f, 0), new Vector3(d + 0.006f, 0.006f, d + 0.006f), nap, PrimitiveType.Cylinder);
        Cb(g, "vanh_mieng", new Vector3(0, h - 0.004f, 0), new Vector3(d + 0.008f, 0.006f, d + 0.008f), nap, PrimitiveType.Cylinder);
        Cb(g, "nap", new Vector3(0, h + 0.008f, 0), new Vector3(d + 0.004f, 0.008f, d + 0.004f), nap, PrimitiveType.Cylinder);
        Cb(g, "long_nap", new Vector3(0, h + 0.0165f, 0), new Vector3(d * 0.82f, 0.001f, d * 0.82f), than, PrimitiveType.Cylinder);
        if (quai)
        {
            var sat = M("M_Son_Quai", "#5E605C");
            foreach (var s in new[] { -1f, 1f }) Cb(g, "tai_quai", new Vector3(s * (d / 2 + 0.004f), h * 0.86f, 0), new Vector3(0.008f, 0.03f, 0.016f), sat);
            var q = Cb(g, "quai", new Vector3(0, h + 0.021f, 0.02f), new Vector3(d + 0.012f, 0.005f, 0.006f), sat);
            q.transform.localRotation = Quaternion.Euler(0, RS(-12, 12), 0);
        }
        if (rSon.NextDouble() < 0.45)   // vệt sơn chảy từ miệng thùng
        {
            int iv = rSon.Next(SonVet.Length); var v = M("M_Son_Vet_" + iv, SonVet[iv]);
            float a = RS(0, 360) * Mathf.Deg2Rad, dl = RS(0.05f, 0.11f);
            var dv = Cb(g, "vet_son", new Vector3(Mathf.Sin(a) * (d / 2 + 0.003f), h - dl / 2, Mathf.Cos(a) * (d / 2 + 0.003f)), new Vector3(0.025f, dl, 0.003f), v);
            dv.transform.localRotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);
        }
        return h + 0.017f;
    }

    static Transform NhomSon(string ten) { var t = new GameObject(ten).transform; t.SetParent(cur, false); Mark(t.gameObject, LocProp.Kieu.CoDinh); return t; }

    // ───────────── tủ áo Khôi (gỗ, hai cánh) — gốc = giữa mép lưng sát tường, ở sàn; +z cục bộ = mặt cánh
    static void TuAoKhoiMoi(float x, float z, float y, float yaw)
    {
        var g = new GameObject("TuAo_Khoi_Go").transform; g.SetParent(cur, false);
        g.SetPositionAndRotation(new Vector3(x, y, z), Quaternion.Euler(0, yaw, 0));
        Mark(g.gameObject, LocProp.Kieu.CoDinh);
        Material go = M("M_TuKhoi_Go", "#6B4A2E"), dam = M("M_TuKhoi_GoDam", "#4A3220"), trong = M("M_TuKhoi_Trong", "#3A2818");
        void K(string n, float x0, float x1, float y0, float y1, float z0, float z1, Material m) =>
            Cb(g, n, new Vector3((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2), new Vector3(x1 - x0, y1 - y0, z1 - z0), m);
        foreach (var lx in new[] { -0.41f, 0.41f })
            foreach (var lz in new[] { 0.05f, 0.49f })
            {
                Cb(g, "chan_tien", new Vector3(lx, 0.055f, lz), new Vector3(0.045f, 0.045f, 0.045f), dam, PrimitiveType.Cylinder);
                Cb(g, "chan_bau", new Vector3(lx, 0.06f, lz), new Vector3(0.055f, 0.02f, 0.055f), dam, PrimitiveType.Sphere);
            }
        K("khung_chan", -0.449f, 0.449f, 0.10f, 0.135f, 0.0f, 0.53f, go);
        K("diem_chan", -0.452f, 0.452f, 0.10f, 0.165f, 0.53f, 0.556f, dam);
        K("hong_trai", -0.45f, -0.432f, 0.10f, 1.90f, 0f, 0.53f, go);
        K("hong_phai", 0.432f, 0.45f, 0.10f, 1.90f, 0f, 0.53f, go);
        K("noc", -0.4495f, 0.4495f, 1.88f, 1.8995f, 0f, 0.53f, go);
        K("gioi_noc_1", -0.47f, 0.47f, 1.90f, 1.93f, 0f, 0.572f, dam);
        K("gioi_noc_2", -0.46f, 0.46f, 1.93f, 1.952f, 0f, 0.562f, go);
        K("gioi_noc_3", -0.482f, 0.482f, 1.952f, 1.975f, 0f, 0.584f, dam);
        K("nep_tren", -0.4495f, 0.4495f, 1.875f, 1.8995f, 0.53f, 0.554f, dam);
        K("lung", -0.432f, 0.432f, 0.135f, 1.88f, 0.0f, 0.008f, trong);
        K("day", -0.432f, 0.432f, 0.135f, 0.148f, 0.008f, 0.53f, trong);
        K("vach_giua", -0.008f, 0.008f, 0.148f, 1.88f, 0.008f, 0.53f, trong);
        float[] ke = { 0.55f, 1.0f, 1.42f };
        foreach (var k in ke) K("dot", 0.008f, 0.432f, k - 0.018f, k, 0.008f, 0.52f, trong);
        LocDoTrongTu.Hoc("TuAo_Khoi_Treo", 0, 0, g, -0.432f, -0.008f, 0.148f, 1.88f, 0.53f, 0.008f, null, 1);
        LocDoTrongTu.Hoc("TuAo_Khoi_Ke", 1, 0, g, 0.008f, 0.432f, 0.148f, 1.88f, 0.53f, 0.008f, ke, 1);
        CanhTuGo(g, -0.4485f, +1, 88f);    // cánh trái sát tường: mở tối đa 88° cho khỏi quét vào tường
        CanhTuGo(g, 0.4485f, -1, 95f);
        // đồ trên nóc: va li giả da + hộp giấy (đồ Khôi mang về từ ký túc xá)
        var vali = M("M_Vali_GiaDa", "#5C4630");
        Cb(g, "vali", new Vector3(-0.13f, 1.975f + 0.1f, 0.27f), new Vector3(0.52f, 0.2f, 0.36f), vali);
        Cb(g, "vali_vien", new Vector3(-0.13f, 1.975f + 0.1f, 0.27f), new Vector3(0.528f, 0.012f, 0.368f), M("M_Vali_Vien", "#3A2C1E"));
        Cb(g, "vali_quai", new Vector3(-0.13f, 1.975f + 0.1f, 0.455f), new Vector3(0.12f, 0.02f, 0.016f), M("M_Vali_Vien", "#3A2C1E"));
        foreach (var s in new[] { -1f, 1f }) Cb(g, "vali_khoa", new Vector3(-0.13f + s * 0.17f, 1.975f + 0.15f, 0.452f), new Vector3(0.03f, 0.02f, 0.006f), M("M_DongThau", "#B8862B"));
        Cb(g, "hop_giay", new Vector3(0.24f, 1.975f + 0.075f, 0.25f), new Vector3(0.3f, 0.15f, 0.32f), M("M_HopCarton_Nau", "#A98B5E"));
        Cb(g, "bang_dinh", new Vector3(0.24f, 1.975f + 0.151f, 0.25f), new Vector3(0.05f, 0.002f, 0.322f), M("M_BangDinh", "#C9B27A"));
        var bc = g.gameObject.AddComponent<BoxCollider>(); bc.center = new Vector3(0, 0.99f, 0.265f); bc.size = new Vector3(0.9f, 1.98f, 0.53f);
    }

    // cánh tủ gỗ: bản lề tại (hx) mép ngoài, dir = +1 cánh trải về +x; khung nổi + 2 pano + tay nắm đồng + bản lề
    static void CanhTuGo(Transform g, float hx, int dir, float gocMo)
    {
        Material go = M("M_TuKhoi_Go", "#6B4A2E"), dam = M("M_TuKhoi_GoDam", "#4A3220"), pano = M("M_TuKhoi_Pano", "#7A5636"), dong = M("M_DongThau", "#B8862B");
        var p = new GameObject(dir > 0 ? "CanhTuKhoi_Trai" : "CanhTuKhoi_Phai").transform; p.SetParent(g, false);
        p.localPosition = new Vector3(hx, 0.168f, 0.543f); p.localRotation = Quaternion.identity;
        float w = 0.446f, h = 1.705f, cx = dir * w / 2;
        Cb(p, "canh", new Vector3(cx, h / 2, 0), new Vector3(w, h, 0.02f), go);
        foreach (var ex in new[] { 0.03f, w - 0.03f }) Cb(p, "nep_doc", new Vector3(dir * ex, h / 2, 0.013f), new Vector3(0.05f, h, 0.006f), dam);
        foreach (var yy in new[] { 0.03f, 0.62f, h - 0.03f }) Cb(p, "nep_ngang", new Vector3(cx, yy, 0.0115f), new Vector3(w - 0.1f, 0.05f, 0.006f), dam);
        Cb(p, "pano_duoi", new Vector3(cx, 0.325f, 0.0105f), new Vector3(w - 0.17f, 0.47f, 0.006f), pano);
        Cb(p, "pano_tren", new Vector3(cx, 1.16f, 0.0105f), new Vector3(w - 0.17f, 0.97f, 0.006f), pano);
        foreach (var yy in new[] { 0.325f, 1.16f }) Cb(p, "hoa_van", new Vector3(cx, yy, 0.0142f), new Vector3(0.09f, 0.09f, 0.002f), dam);   // ô vuông chạm giữa pano
        float xm = dir * (w - 0.045f);
        Cb(p, "tay_nam", new Vector3(xm, 0.92f, 0.034f), new Vector3(0.012f, 0.06f, 0.012f), dong, PrimitiveType.Cylinder);
        foreach (var yy in new[] { 0.87f, 0.97f }) Cb(p, "chan_tay_nam", new Vector3(xm, yy, 0.024f), new Vector3(0.012f, 0.012f, 0.022f), dong);
        if (dir > 0)
        {
            Cb(p, "o_khoa", new Vector3(xm, 0.8f, 0.0145f), new Vector3(0.022f, 0.04f, 0.003f), dong);
            Cb(p, "lo_khoa", new Vector3(xm, 0.797f, 0.0165f), new Vector3(0.005f, 0.014f, 0.002f), M("M_LoKhoa", "#151515"));
        }
        foreach (var yy in new[] { 0.16f, h - 0.16f }) Cb(p, "ban_le", new Vector3(dir * 0.004f, yy, 0.0f), new Vector3(0.01f, 0.07f, 0.026f), dong);
        Mo(p, 0f, false, "cửa tủ áo", -dir * gocMo);
    }

    // ───────────── móc khăn treo tường: gốc = mặt tường ở giữa tấm gỗ, +z cục bộ hướng ra phòng
    // vat: mỗi chốt một món — "khan:#mau:#soc", "khanNho:#mau:#soc", "mu" (mũ cối), null = để trống
    static void MocKhanMoi(float x, float z, float y, float yaw, params string[] vat)
    {
        var rot = Quaternion.Euler(0, yaw, 0);
        var g = new GameObject("KeTreo_Go").transform; g.SetParent(cur, false); g.SetPositionAndRotation(new Vector3(x, y, z), rot);
        Mark(g.gameObject, LocProp.Kieu.Treo);
        var k = new GameObject("Khan_Treo").transform; k.SetParent(cur, false); k.SetPositionAndRotation(new Vector3(x, y, z), rot);
        Mark(k.gameObject, LocProp.Kieu.Treo);
        Material go = MT("M_MacAo_Go", "D_go.png", 1f, 1f, "#5A3B22"), dong = M("M_DongThau", "#B8862B"), oc = M("M_OcVit", "#3A3A38");
        int n = vat.Length; const float buoc = 0.27f; float L = n * buoc + 0.05f;   // khăn gấp dọc rộng 0,24 → chốt cách 0,27 cho khăn không chồng nhau
        Cb(g, "tam_go", new Vector3(0, 0, 0.011f), new Vector3(L, 0.085f, 0.022f), go);
        Cb(g, "vat_mep", new Vector3(0, 0.046f, 0.013f), new Vector3(L, 0.008f, 0.026f), go);
        foreach (var sx in new[] { -L / 2 + 0.03f, L / 2 - 0.03f }) { var o = Cb(g, "oc_vit", new Vector3(sx, 0, 0.0225f), new Vector3(0.012f, 0.0015f, 0.012f), oc, PrimitiveType.Cylinder); o.transform.localRotation = Quaternion.Euler(90, 0, 0); }
        for (int i = 0; i < n; i++)
        {
            float px = (i - (n - 1) / 2f) * buoc;
            var c = Cb(g, "chot", new Vector3(px, 0.004f, 0.052f), new Vector3(0.014f, 0.03f, 0.014f), dong, PrimitiveType.Cylinder);
            c.transform.localRotation = Quaternion.Euler(78, 0, 0);
            Cb(g, "nuom_chot", new Vector3(px, 0.011f, 0.083f), Vector3.one * 0.022f, dong, PrimitiveType.Sphere);
            if (vat[i] == null) continue;
            var a = vat[i].Split(':');
            if (a[0] == "mu")
            {   // mũ cối của bố: chỏm bán cầu + vành + quai, treo nghiêng trên chốt
                var mu = new GameObject("MuCoi").transform; mu.SetParent(k, false); mu.localPosition = new Vector3(px, -0.07f, 0.1f); mu.localRotation = Quaternion.Euler(-70, 0, 0);
                var xanh = M("M_MuCoi", "#6B6A3E");
                Cb(mu, "chom", new Vector3(0, 0.0f, 0), new Vector3(0.22f, 0.13f, 0.25f), xanh, PrimitiveType.Sphere);
                Cb(mu, "vanh", new Vector3(0, -0.02f, 0), new Vector3(0.3f, 0.006f, 0.33f), xanh, PrimitiveType.Cylinder);
                Cb(mu, "nut_dinh", new Vector3(0, 0.066f, 0), new Vector3(0.03f, 0.008f, 0.03f), M("M_MuCoi_Nut", "#4E4D2E"), PrimitiveType.Cylinder);

                Cb(k, "quai_mu", new Vector3(px, -0.035f, 0.074f), new Vector3(0.006f, 0.08f, 0.004f), M("M_MuCoi_Quai", "#5A4630"));   // quai da vắt qua chốt
                continue;
            }
            bool nho = a[0] == "khanNho"; float w = nho ? 0.18f : 0.24f, hT = nho ? 0.32f : 0.46f;
            Material m = M("M_Khan_" + a[1].TrimStart('#'), a[1]), soc = M("M_Khan_" + a[2].TrimStart('#'), a[2]);
            var nep = Cb(k, "nep_khan", new Vector3(px, 0.01f, 0.06f), new Vector3(0.026f, w / 2, 0.026f), m, PrimitiveType.Cylinder);
            nep.transform.localRotation = Quaternion.Euler(0, 0, 90);
            Cb(k, "khan_truoc", new Vector3(px + 0.006f, 0.01f - hT / 2, 0.072f), new Vector3(w, hT, 0.007f), m).transform.localRotation = Quaternion.Euler(-3, 0, 1.5f);
            Cb(k, "khan_sau", new Vector3(px, 0.01f - hT * 0.42f, 0.049f), new Vector3(w, hT * 0.84f, 0.007f), m);
            foreach (var dy in new[] { 0.06f, 0.085f }) Cb(k, "soc_khan", new Vector3(px + 0.006f, 0.01f - hT + dy, 0.0765f), new Vector3(w * 0.99f, 0.016f, 0.002f), soc).transform.localRotation = Quaternion.Euler(-3, 0, 1.5f);
            Cb(k, "tua_khan", new Vector3(px + 0.006f, 0.01f - hT - 0.008f, 0.0735f), new Vector3(w * 0.96f, 0.016f, 0.004f), soc).transform.localRotation = Quaternion.Euler(-3, 0, 1.5f);
        }
    }

    // ───────────── phòng Nhím: rổ nhựa đựng đồ chơi (0,42 × 0,30 · cao 0,24), tâm đáy tại (x, z)
    static void RoDoChoiNhim(float x, float z, float y)
    {
        var g = new GameObject("Hop_DoChoi_Nhim").transform; g.SetParent(cur, false); g.position = new Vector3(x, y, z);
        Mark(g.gameObject, LocProp.Kieu.CoDinh);
        Material ro = M("M_RoNhua_Hong", "#D9708A"), lo = M("M_RoNhua_Lo", "#7A2E40");
        float W2 = 0.21f, D2 = 0.15f, H = 0.24f, t = 0.012f;
        Cb(g, "day", new Vector3(0, 0.006f, 0), new Vector3(2 * W2, 0.012f, 2 * D2), ro);
        foreach (var s in new[] { -1f, 1f })
        {
            Cb(g, "thanh_dai", new Vector3(0, H / 2, s * (D2 - t / 2)), new Vector3(2 * W2, H, t), ro);
            Cb(g, "thanh_ngan", new Vector3(s * (W2 - t / 2), H / 2, 0), new Vector3(t, H, 2 * D2 - 2 * t), ro);
            Cb(g, "lo_quai", new Vector3(s * (W2 + 0.0005f), H - 0.05f, 0), new Vector3(t * 0.9f, 0.03f, 0.1f), lo);
            Cb(g, "vanh", new Vector3(0, H - 0.006f, s * (D2 + 0.003f)), new Vector3(2 * W2 + 0.01f, 0.014f, 0.008f), ro);
            for (int i = 0; i < 5; i++) Cb(g, "lo_thoang", new Vector3(-0.14f + i * 0.07f, 0.09f, s * (D2 + 0.0005f)), new Vector3(0.035f, 0.08f, t * 0.9f), lo);
        }
        // đồ chơi trong rổ: khối gỗ màu, bóng nhựa, ô tô đồ chơi, con lật đật
        string[] mau = { "#D9483A", "#F2C040", "#4E8AD0", "#5EAA5A", "#E68AB0", "#E6C45A", "#F28C38" };
        var rr = new System.Random(77);
        for (int i = 0; i < 9; i++)
        {
            var b = Cb(g, "khoi_go", new Vector3(-0.15f + (i % 3) * 0.05f + (float)rr.NextDouble() * 0.01f, 0.13f + (i / 3) * 0.035f, -0.08f + (i % 2) * 0.05f), Vector3.one * 0.04f, M("M_DoChoi_" + i % 7, mau[i % 7]));
            b.transform.localRotation = Quaternion.Euler(rr.Next(0, 40), rr.Next(0, 90), rr.Next(0, 40));
        }
        Cb(g, "bong", new Vector3(0.1f, 0.17f, -0.05f), Vector3.one * 0.12f, M("M_DoChoi_Bong", "#E8D24A"), PrimitiveType.Sphere);
        Cb(g, "soc_bong", new Vector3(0.1f, 0.17f, -0.05f), new Vector3(0.122f, 0.03f, 0.122f), M("M_DoChoi_0", mau[0]), PrimitiveType.Sphere);
        var oto = new GameObject("oto").transform; oto.SetParent(g, false); oto.localPosition = new Vector3(0.06f, 0.2f, 0.07f); oto.localRotation = Quaternion.Euler(0, 25, 12);
        Cb(oto, "than", new Vector3(0, 0.025f, 0), new Vector3(0.14f, 0.035f, 0.065f), M("M_DoChoi_2", mau[2]));
        Cb(oto, "ca_bin", new Vector3(-0.01f, 0.055f, 0), new Vector3(0.07f, 0.03f, 0.058f), M("M_DoChoi_Kinh", "#CFE0E6"));
        foreach (var wx in new[] { -0.045f, 0.045f }) foreach (var wz in new[] { -0.035f, 0.035f }) { var bx = Cb(oto, "banh", new Vector3(wx, 0.012f, wz), new Vector3(0.028f, 0.006f, 0.028f), M("M_LoKhoa", "#151515"), PrimitiveType.Cylinder); bx.transform.localRotation = Quaternion.Euler(90, 0, 0); }
        Cb(g, "lat_dat", new Vector3(-0.12f, 0.2f, 0.06f), new Vector3(0.07f, 0.08f, 0.07f), M("M_DoChoi_0", mau[0]), PrimitiveType.Sphere);
        Cb(g, "lat_dat_dau", new Vector3(-0.12f, 0.26f, 0.06f), Vector3.one * 0.05f, M("M_DoChoi_Da", "#F0D2B4"), PrimitiveType.Sphere);
        var bc = g.gameObject.AddComponent<BoxCollider>(); bc.center = new Vector3(0, H / 2, 0); bc.size = new Vector3(2 * W2, H, 2 * D2);
    }

    // kệ gỗ treo tường trên bàn học Nhím + sách vở, truyện tranh xếp gọn, heo đất. Gốc = mặt tường ở mép dưới tấm kệ, +z cục bộ ra phòng
    static void KeSachNhim(float x, float z, float y, float yaw)
    {
        var rot = Quaternion.Euler(0, yaw, 0);
        var g = new GameObject("KeGo_Nhim").transform; g.SetParent(cur, false); g.SetPositionAndRotation(new Vector3(x, y, z), rot);
        Mark(g.gameObject, LocProp.Kieu.Treo);
        Material go = M("M_KeNhim_Go", "#8A6440"), sat = M("M_KeNhim_Sat", "#3A3A38");
        Cb(g, "tam_ke", new Vector3(0, 0.009f, 0.1f), new Vector3(0.66f, 0.018f, 0.2f), go);
        Cb(g, "nep_ke", new Vector3(0, 0.0f, 0.197f), new Vector3(0.66f, 0.03f, 0.008f), go);
        foreach (var sx in new[] { -0.26f, 0.26f })
        {
            Cb(g, "eke_dung", new Vector3(sx, -0.07f, 0.006f), new Vector3(0.02f, 0.14f, 0.012f), sat);
            var e = Cb(g, "eke_xien", new Vector3(sx, -0.05f, 0.07f), new Vector3(0.012f, 0.012f, 0.17f), sat); e.transform.localRotation = Quaternion.Euler(-38, 0, 0);
        }
        var s = new GameObject("SachVo_Nhim").transform; s.SetParent(cur, false); s.SetPositionAndRotation(new Vector3(x, y, z), rot);
        Mark(s.gameObject, LocProp.Kieu.Treo);
        string[] bia = { "#3E7AC0", "#E6C45A", "#5EAA5A", "#D9483A", "#9B6BC0", "#F2F0E8", "#4E8AD0", "#E68AB0", "#F28C38" };
        var rr = new System.Random(99); float bx = -0.31f;
        for (int i = 0; i < 11; i++)   // sách giáo khoa + vở đứng, cuốn cuối nghiêng tựa
        {
            float w = 0.012f + (float)rr.NextDouble() * 0.012f, h = 0.2f + (float)rr.NextDouble() * 0.05f;
            var b = Cb(s, "sach_dung", new Vector3(bx + w / 2, 0.018f + h / 2, 0.1f), new Vector3(w, h, 0.15f + (float)rr.NextDouble() * 0.02f), M("M_BiaSach_" + i % 9, bia[i % 9]));
            Cb(s, "nhan_gay", new Vector3(bx + w / 2, 0.018f + h * 0.7f, 0.1f + 0.0805f), new Vector3(w * 0.8f, 0.03f, 0.001f), M("M_NhanSach", "#F2F0E8"));
            if (i == 10) b.transform.localRotation = Quaternion.Euler(0, 0, -12);
            bx += w + 0.002f;
        }
        float ty = 0.018f;
        for (int i = 0; i < 6; i++)   // chồng truyện tranh nằm (Doraemon, Thần đồng đất Việt… khổ nhỏ)
        {
            var b = Cb(s, "truyen_tranh", new Vector3(0.12f + (float)(rr.NextDouble() - 0.5) * 0.01f, ty + 0.0045f, 0.1f), new Vector3(0.115f, 0.009f, 0.175f), M("M_BiaSach_" + (i + 3) % 9, bia[(i + 3) % 9]));
            b.transform.localRotation = Quaternion.Euler(0, (float)(rr.NextDouble() - 0.5) * 8, 0); ty += 0.0095f;
        }
        // heo đất
        var heo = M("M_HeoDat", "#D9785A");
        Cb(s, "heo_than", new Vector3(0.26f, 0.018f + 0.05f, 0.1f), new Vector3(0.1f, 0.085f, 0.075f), heo, PrimitiveType.Sphere);
        Cb(s, "heo_mom", new Vector3(0.315f, 0.018f + 0.052f, 0.1f), new Vector3(0.016f, 0.012f, 0.025f), heo, PrimitiveType.Cylinder).transform.localRotation = Quaternion.Euler(0, 0, 90);
        foreach (var sz in new[] { -1f, 1f })
        {
            Cb(s, "heo_tai", new Vector3(0.29f, 0.018f + 0.09f, 0.1f + sz * 0.02f), new Vector3(0.012f, 0.025f, 0.02f), heo).transform.localRotation = Quaternion.Euler(sz * 20, 0, -20);
            Cb(s, "heo_mat", new Vector3(0.302f, 0.018f + 0.07f, 0.1f + sz * 0.016f), Vector3.one * 0.008f, M("M_LoKhoa", "#151515"), PrimitiveType.Sphere);
        }
        Cb(s, "heo_khe", new Vector3(0.26f, 0.018f + 0.092f, 0.1f), new Vector3(0.03f, 0.003f, 0.005f), M("M_LoKhoa", "#151515"));
    }
}
