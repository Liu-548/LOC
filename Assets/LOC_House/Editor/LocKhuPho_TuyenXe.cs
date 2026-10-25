// [25/10] Tuyến xe buýt cho đoạn mở đầu Đêm 1 (LocXeBuyt) — nối vào khu phố quanh nhà LỘC, gọi từ LocKhuPho.Build().
//  · Phố nhà kéo dài về phía đông tới ngã ba x ≈ 150 với một đường ngang chạy theo trục Z.
//  · Đường ngang, từ bắc xuống nam: đoạn phố (z 420…211) → đoạn ven sông phía tây có bến, cây, kè, bờ bên kia (z 208…58) → dãy nhà → ngã ba.
//  · Xe: buýt thành phố cũ đầu những năm 2000 (sơn kem, sọc đỏ đô, ghế nhựa giả da xanh rêu, đèn tuýp, tay vịn inox), dựng bằng hộp như khu phố.
//    Không tên hãng, không logo, số tuyến và tên bến đều hư cấu.
//  · Người chơi trong game vẫn chỉ đi tới x 57 như trước (ChanDuong — va chạm không hình) vì dãy "DauPhai" bịt đầu phố đã bỏ.
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class LocKhuPho
{
    const float XL = 149.5f;            // tâm xe trên đường ngang (làn phải khi đi về phía nam)
    const float ZDau = 290f;            // xe bắt đầu ở đây (sau lưng: phố còn 130 m nữa, sương mù che)
    const float ZSong0 = 58f, ZSong1 = 208f, ZBen = 136f;
    static Mesh cau;

    static Mesh Cau()
    {
        if (cau) return cau;
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cau = g.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(g);
        return cau;
    }

    // thanh hộp nối hai điểm (dây, tay đòn, càng xe…)
    static void Thanh(MB b, Material m, Vector3 a, Vector3 c, float th, float th2 = -1)
    {
        var d = c - a; if (d.sqrMagnitude < 1e-6f) return;
        b.BoxM(m, Matrix4x4.TRS((a + c) / 2, Quaternion.LookRotation(d), Vector3.one), new Vector3(th, th2 < 0 ? th : th2, d.magnitude));
    }
    // hộp trong hệ toạ độ đã xoay (đồ đặt chéo: xe máy, ghế nhựa)
    static void HopR(MB b, Material m, Matrix4x4 goc, float x0, float x1, float y0, float y1, float z0, float z1)
        => b.BoxM(m, goc * Matrix4x4.Translate(new Vector3((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2)), new Vector3(x1 - x0, y1 - y0, z1 - z0));
    static void TruR(MB b, Material m, Matrix4x4 goc, Vector3 tam, float r, float cao, Quaternion xoay)
        => b.AddMesh(m, Cyl(), goc * Matrix4x4.TRS(tam, xoay, new Vector3(r * 2, cao / 2, r * 2)));

    // ═════════════════════════ đường ngang + đoạn ven sông
    static void TuyenXe()
    {
        var nhua = Mat("Nhua", "T_worn_asphalt.jpg", 3f, 3f, "#8C8C90", 0.25f, 0.55f);
        var gach = Mat("ViaHe", "T_patterned_concrete_pavers_02.jpg", 2f, 2f, "#B8AAA0", 0.05f, 0.2f);
        var be = Mat("BeTong", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#9A9890");
        var b = new MB(Matrix4x4.identity, new System.Random(25));
        b.Box(nhua, 144.7f, 155.3f, -0.65f, -0.30f, -5.4f, 420f, true);
        b.Box(gach, 144.7f, 146.5f, -0.65f, -0.15f, -7.2f, 420f, true);    // vỉa hè tây (đoạn sông: lối đi ven kè)
        b.Box(gach, 153.5f, 155.3f, -0.65f, -0.15f, -12.4f, 420f, true);   // vỉa hè đông
        b.Box(be, 146.5f, 146.7f, -0.30f, -0.15f, -7.2f, 420f);
        b.Box(be, 153.3f, 153.5f, -0.30f, -0.15f, -12.4f, 420f);
        b.Commit(grp, "DuongNgang", default, false);

        // hai dãy nhà hai bên đường ngang (mặt tiền quay ra đường)
        HangZ("Ngang_Tay_C", -5.4f, 55f, 144.7f, false, 12f, 3, 5, Kieu.Mat, 3.8f, 5.6f);
        HangZ("Ngang_Tay_A", 211f, 420f, 144.7f, false, 12f, 2, 5, Kieu.Mat, 3.6f, 5.4f);
        HangZ("Ngang_Dong_C", -14f, 62f, 155.3f, true, 12f, 2, 5, Kieu.Mat, 3.8f, 5.6f);
        HangZ("Ngang_Dong_B", 62f, 205f, 155.3f, true, 10f, 1, 2, Kieu.Mat, 4.2f, 6.5f);   // ven sông: nhà thấp, thưa
        HangZ("Ngang_Dong_A", 205f, 420f, 155.3f, true, 12f, 2, 5, Kieu.Mat, 3.6f, 5.4f);

        Song();
        CayVaDen();
        XeMayVaQuan();

        // chặn người chơi ở x 57 (chỗ dãy DauPhai cũ) — chỉ va chạm, không hình. Xe buýt chạy bằng transform nên đi xuyên qua được.
        var chan = new GameObject("ChanDuong_X57"); chan.transform.SetParent(grp, false);
        var bc = chan.AddComponent<BoxCollider>(); bc.center = new Vector3(57f, 2f, -9.7f); bc.size = new Vector3(1f, 5f, 8.6f);
    }

    static void Song()
    {
        var nuoc = Mat("NuocSong", null, 1, 1, "#0B1117", 0.55f, 0.93f);
        var ke = Mat("KeDa", "T_worn_mossy_plasterwall.jpg", 2f, 2f, "#6C6A5E", 0.05f, 0.15f);
        var dat = Mat("DatBo", null, 1, 1, "#1D211A", 0.02f, 0.05f);
        var sat = Mat("Sat", null, 1, 1, "#22201E");
        var go = Mat("GoThuyen", null, 1, 1, "#4E3E2C", 0.05f, 0.15f);
        var tau = Mat("ThuyenSon", null, 1, 1, "#34505A", 0.08f, 0.25f);
        var be = Mat("BeTong", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#9A9890");
        var b = new MB(Matrix4x4.identity, new System.Random(11));

        b.Box(nuoc, 20f, 144.6f, -1.62f, -1.45f, ZSong0, ZSong1);
        // kè đá bờ gần (hở chỗ bậc bến) + hai đầu đoạn sông + đất phủ hai đầu
        b.Box(ke, 144.45f, 144.75f, -1.7f, -0.15f, ZSong0, ZBen - 1.5f);
        b.Box(ke, 144.45f, 144.75f, -1.7f, -0.15f, ZBen + 1.5f, ZSong1);
        b.Box(ke, 20f, 144.75f, -1.7f, -0.15f, ZSong0 - 0.3f, ZSong0);
        b.Box(ke, 20f, 144.75f, -1.7f, -0.15f, ZSong1, ZSong1 + 0.3f);
        b.Box(dat, 20f, 144.7f, -0.65f, -0.15f, 50.8f, ZSong0 - 0.3f, true);
        b.Box(dat, 20f, 144.7f, -0.65f, -0.15f, ZSong1 + 0.3f, 212f, true);
        // bờ bên kia: bãi đất thấp, kè đất
        b.Box(dat, -40f, 32f, -0.4f, 0f, 40f, 230f, true);
        b.Box(ke, 31.7f, 32f, -1.7f, 0f, ZSong0, ZSong1);

        // lan can sắt thấp dọc kè (trừ chỗ xuống bến)
        for (float z = ZSong0 + 1f; z < ZSong1 - 0.5f; z += 2f)
        {
            if (Mathf.Abs(z - ZBen) < 1.9f) continue;
            b.Box(sat, 144.58f, 144.64f, -0.15f, 0.85f, z - 0.03f, z + 0.03f);
        }
        foreach (var (z0, z1) in new[] { (ZSong0 + 1f, ZBen - 1.5f), (ZBen + 1.5f, ZSong1 - 1f) })
        {
            b.Box(sat, 144.57f, 144.65f, 0.80f, 0.85f, z0, z1);
            b.Box(sat, 144.59f, 144.63f, 0.35f, 0.38f, z0, z1);
        }

        // bến: sáu bậc xi măng xuống nước, cọc buộc thuyền, một con thuyền gỗ
        for (int i = 0; i < 6; i++)
            b.Box(be, 144.7f - 0.5f * (i + 1), 144.7f - 0.5f * i, -1.7f, -0.15f - 0.22f * (i + 1), ZBen - 1.5f, ZBen + 1.5f);
        b.Box(ke, 141.6f, 144.75f, -1.7f, -0.15f, ZBen - 1.8f, ZBen - 1.5f);
        b.Box(ke, 141.6f, 144.75f, -1.7f, -0.15f, ZBen + 1.5f, ZBen + 1.8f);
        b.Box(go, 141.2f, 141.32f, -1.7f, -0.35f, ZBen - 2.6f, ZBen - 2.48f);
        var tt = Matrix4x4.TRS(new Vector3(139.9f, -1.5f, ZBen - 5.0f), Quaternion.Euler(0, 4, 2), Vector3.one);
        HopR(b, tau, tt, -0.55f, 0.55f, 0f, 0.38f, -2.0f, 2.0f);
        b.BoxM(tau, tt * Matrix4x4.TRS(new Vector3(0, 0.22f, 2.35f), Quaternion.Euler(-18, 0, 0), Vector3.one), new Vector3(0.7f, 0.36f, 0.9f));
        b.BoxM(tau, tt * Matrix4x4.TRS(new Vector3(0, 0.22f, -2.3f), Quaternion.Euler(14, 0, 0), Vector3.one), new Vector3(0.8f, 0.36f, 0.8f));
        HopR(b, go, tt, -0.48f, 0.48f, 0.25f, 0.30f, -1.9f, 1.9f);
        HopR(b, go, tt, -0.5f, 0.5f, 0.30f, 0.36f, -0.4f, -0.1f);
        Thanh(b, Mat("Day", null, 1, 1, "#0C0C0C", 0f, 0f), new Vector3(141.26f, -0.6f, ZBen - 2.54f), new Vector3(139.9f, -1.15f, ZBen - 3.1f), 0.015f);
        b.Commit(grp, "Song", default, false);

        // bảng "CẤM TẮM" + cột đèn bến — đèn vàng yếu, mặt nước hắt lại
        var bb = new MB(Matrix4x4.identity, new System.Random(12));
        var trang = Mat("BangTrang", null, 1, 1, "#BDB6A6", 0.05f, 0.2f);
        bb.Box(sat, 145.05f, 145.1f, -0.15f, 1.55f, ZBen + 2.6f, ZBen + 2.65f);
        bb.Box(sat, 145.05f, 145.1f, -0.15f, 1.55f, ZBen + 3.55f, ZBen + 3.6f);
        bb.Box(trang, 145.1f, 145.13f, 1.0f, 1.62f, ZBen + 2.5f, ZBen + 3.7f);
        var bt = Mat("BeTongCot", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#7C7A74");
        var den = Mat("DenPho", null, 1, 1, "#000000", 0f, 0f, false, new Color(1.0f, 0.62f, 0.28f) * 1.4f);
        bb.Box(bt, 145.2f, 145.36f, -0.15f, 4.2f, ZBen - 2.0f, ZBen - 1.84f);
        bb.Box(sat, 144.2f, 145.3f, 4.0f, 4.05f, ZBen - 1.94f, ZBen - 1.9f);
        bb.Box(den, 144.15f, 144.35f, 3.78f, 3.95f, ZBen - 2.0f, ZBen - 1.84f);
        bb.Commit(grp, "Ben_BangDen", default, false);
        DenPho(144.25f, ZBen - 1.92f, "#FFB060", 11f, 1.3f, 3.7f);
        BangChu("Bang_CamTam", "T_Chu_CamTam.png", new Vector3(145.135f, 1.31f, ZBen + 3.1f), Quaternion.Euler(0, -90, 0), 1.18f, 0.6f);

        // bờ bên kia: hàng nhà thấp ít đèn (sương mù gần như nuốt hết, chỉ còn vài ô cửa vàng)
        HangZ("BoKia", ZSong0 + 4f, ZSong1 - 4f, 26f, false, 9f, 1, 2, Kieu.Xa, 5f, 9f);
    }

    static void CayVaDen()
    {
        var than = Mat("ThanCay", null, 1, 1, "#3B342C", 0.03f, 0.1f);
        var la = Mat("LaCay", null, 1, 1, "#17231A", 0.03f, 0.1f);
        var la2 = Mat("LaCay2", null, 1, 1, "#1E2C1C", 0.03f, 0.1f);
        var r = new System.Random(31);
        var b = new MB(Matrix4x4.identity, r);
        // bạch đàn dọc kè (tây) và cây bàng thưa bên đông, đoạn ven sông
        for (float z = ZSong0 + 4f; z < ZSong1 - 3f; z += 8.5f + (float)r.NextDouble() * 3f)
            if (Mathf.Abs(z - ZBen) > 4f && Mathf.Abs(z - ZBen - 3f) > 2f && Mathf.Abs(Mathf.Repeat(z - ZSong0 - 14f + 20f, 40f) - 20f) > 2.2f) Cay(b, than, la, new Vector3(145.55f, -0.15f, z), 0, r);
        for (float z = ZSong0 + 9f; z < ZSong1 - 3f; z += 13f + (float)r.NextDouble() * 6f)
            Cay(b, than, la2, new Vector3(154.4f, -0.15f, z), 1, r);
        for (float z = ZSong0 + 6f; z < ZSong1; z += 7f + (float)r.NextDouble() * 5f)   // bờ bên kia
            Cay(b, than, r.NextDouble() < 0.5 ? la : la2, new Vector3(29f + (float)r.NextDouble() * 2f, 0f, z), r.Next(2), r);
        b.Commit(grp, "Cay_TuyenXe", default, false);

        // cột đèn dọc vỉa hè tây đường ngang, tay đèn vươn ra đường. Đoạn phố đèn khỏe, đoạn sông thưa và yếu.
        var bt = Mat("BeTongCot", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#7C7A74");
        var sat = Mat("Sat", null, 1, 1, "#22201E");
        var den = Mat("DenPho", null, 1, 1, "#000000", 0f, 0f, false, new Color(1.0f, 0.62f, 0.28f) * 1.4f);
        var b2 = new MB(Matrix4x4.identity, new System.Random(32));
        void Cot(float z, float cuong, float tam)
        {
            b2.Box(bt, 145.4f, 145.64f, -0.15f, 8.2f, z - 0.12f, z + 0.12f);
            b2.Box(sat, 145.5f, 147.2f, 7.3f, 7.36f, z - 0.03f, z + 0.03f);
            b2.Box(den, 146.9f, 147.4f, 7.12f, 7.28f, z - 0.18f, z + 0.18f);
            DenPho(147.15f, z, "#FFB060", tam, cuong, 7.0f);
        }
        for (float z = 4f; z < 50f; z += 24f) Cot(z, 1.6f, 12f);
        for (float z = ZSong0 + 14f; z < ZSong1; z += 40f) Cot(z, 0.9f, 10f);
        for (float z = 222f; z < 412f; z += 26f) Cot(z, 1.7f, 12f);
        b2.Commit(grp, "CotDen_DuongNgang", default, false);
    }

    // kieu 0: bạch đàn (cao, mảnh, tán thưa trên cao) · 1: cây bàng (tán xoè, thấp)
    static void Cay(MB b, Material than, Material la, Vector3 p, int kieu, System.Random r)
    {
        float h = kieu == 0 ? 8.5f + (float)r.NextDouble() * 3f : 5.5f + (float)r.NextDouble() * 1.5f;
        float rt = kieu == 0 ? 0.13f : 0.2f;
        b.AddMesh(than, Cyl(), Matrix4x4.TRS(p + Vector3.up * h * 0.45f, Quaternion.Euler((float)r.NextDouble() * 4 - 2, 0, (float)r.NextDouble() * 4 - 2), new Vector3(rt * 2, h * 0.45f, rt * 2)));
        int n = kieu == 0 ? 3 : 4;
        for (int i = 0; i < n; i++)
        {
            var o = new Vector3((float)r.NextDouble() * 1.6f - 0.8f, 0, (float)r.NextDouble() * 1.6f - 0.8f);
            var sc = kieu == 0 ? new Vector3(1.8f, 2.8f, 1.8f) * (0.8f + (float)r.NextDouble() * 0.4f) : new Vector3(3.6f, 1.5f, 3.6f) * (0.75f + (float)r.NextDouble() * 0.4f);
            float y = kieu == 0 ? h * (0.72f + 0.1f * i) : h * 0.8f + i * 0.25f;
            b.AddMesh(la, Cau(), Matrix4x4.TRS(p + o + Vector3.up * y, Quaternion.Euler(0, (float)r.NextDouble() * 360, 0), sc));
        }
    }

    // xe máy dựng trước cửa, vài bộ bàn ghế nhựa quán trà đá (một bóng tròn treo)
    static void XeMayVaQuan()
    {
        var r = new System.Random(41);
        var b = new MB(Matrix4x4.identity, r);
        var cho = new List<(Vector3 p, float yaw)>();
        for (float z = 216f; z < 300f; z += 6f + (float)r.NextDouble() * 9f) cho.Add((new Vector3(145.7f, -0.15f, z), 80f + (float)r.NextDouble() * 25f));
        for (float z = 212f; z < 300f; z += 7f + (float)r.NextDouble() * 10f) cho.Add((new Vector3(154.3f, -0.15f, z), -80f - (float)r.NextDouble() * 25f));
        for (float z = 2f; z < 50f; z += 9f + (float)r.NextDouble() * 9f) cho.Add((new Vector3(145.7f, -0.15f, z), 75f + (float)r.NextDouble() * 30f));
        for (float x = 62f; x < 140f; x += 10f + (float)r.NextDouble() * 14f) cho.Add((new Vector3(x, -0.15f, -6.0f), 170f + (float)r.NextDouble() * 20f));
        for (float x = 64f; x < 150f; x += 12f + (float)r.NextDouble() * 14f) cho.Add((new Vector3(x, -0.15f, -13.4f), -10f + (float)r.NextDouble() * 20f));
        foreach (var (p, yaw) in cho) XeMay(b, p, yaw, r);
        foreach (var (p, yaw) in new[] { (new Vector3(145.6f, -0.15f, 258f), 90f), (new Vector3(154.4f, -0.15f, 238f), -90f), (new Vector3(88f, -0.15f, -6.3f), 180f) })
        {
            QuanTraDa(b, p, yaw, r);
            var l = new GameObject("Den_QuanTraDa").AddComponent<Light>(); l.transform.SetParent(grp, false);
            l.transform.position = p + Quaternion.Euler(0, yaw, 0) * new Vector3(0, 2.4f, -0.3f);
            l.type = LightType.Point; l.color = Hex("#FFC27A"); l.range = 5.5f; l.intensity = 1.1f; l.shadows = LightShadows.None;
        }
        b.Commit(grp, "XeMay_Quan", default, false);
    }

    static void XeMay(MB b, Vector3 p, float yaw, System.Random r)
    {
        string[] son = { "#5E1C1A", "#2A3F5C", "#1C1C1C", "#6E6A60", "#3C4A2E" };
        int k = r.Next(son.Length); var vo = Mat("XeMay" + k, null, 1, 1, son[k], 0.2f, 0.45f);
        var den = Mat("XeMayDen", null, 1, 1, "#141414", 0.1f, 0.3f);
        var bac = Mat("XeMayBac", null, 1, 1, "#8C8E90", 0.4f, 0.6f);
        var g = Matrix4x4.TRS(p, Quaternion.Euler(0, yaw, 4f), Vector3.one);   // dựng chân chống, hơi nghiêng
        var ngang = Quaternion.Euler(0, 0, 90);
        TruR(b, den, g, new Vector3(0, 0.29f, 0.62f), 0.29f, 0.09f, ngang);
        TruR(b, den, g, new Vector3(0, 0.29f, -0.6f), 0.29f, 0.09f, ngang);
        TruR(b, bac, g, new Vector3(0, 0.29f, 0.62f), 0.1f, 0.1f, ngang);
        TruR(b, bac, g, new Vector3(0, 0.29f, -0.6f), 0.1f, 0.1f, ngang);
        HopR(b, vo, g, -0.17f, 0.17f, 0.35f, 0.92f, 0.22f, 0.42f);       // yếm chắn gió
        HopR(b, den, g, -0.14f, 0.14f, 0.30f, 0.36f, -0.2f, 0.25f);      // sàn để chân
        HopR(b, vo, g, -0.17f, 0.17f, 0.42f, 0.80f, -0.62f, -0.12f);     // thân sau
        HopR(b, den, g, -0.15f, 0.15f, 0.80f, 0.90f, -0.66f, 0.02f);     // yên
        HopR(b, bac, g, -0.36f, 0.36f, 1.0f, 1.03f, 0.42f, 0.45f);        // ghi đông
        HopR(b, vo, g, -0.1f, 0.1f, 0.9f, 1.04f, 0.40f, 0.54f);          // đầu đèn
        Thanh(b, bac, g.MultiplyPoint3x4(new Vector3(0, 0.95f, 0.5f)), g.MultiplyPoint3x4(new Vector3(0, 0.3f, 0.64f)), 0.04f);
        HopR(b, bac, g, -0.08f, 0.08f, 0.68f, 0.72f, -0.82f, -0.62f);    // baga
    }

    static void QuanTraDa(MB b, Vector3 p, float yaw, System.Random r)
    {
        var g = Matrix4x4.TRS(p, Quaternion.Euler(0, yaw, 0), Vector3.one);
        string[] mau = { "#8C2A22", "#2C4F7A", "#8C2A22", "#3B6B4A" };
        var ban = Mat("BanNhua", null, 1, 1, "#6A5A3E", 0.05f, 0.2f);
        HopR(b, ban, g, -0.35f, 0.35f, 0.40f, 0.44f, -0.25f, 0.25f);
        HopR(b, ban, g, -0.3f, 0.3f, 0f, 0.40f, -0.2f, 0.2f);
        var am = Mat("AmTra", null, 1, 1, "#9A8F7E", 0.3f, 0.5f);
        b.AddMesh(am, Cyl(), g * Matrix4x4.TRS(new Vector3(0.1f, 0.53f, 0.05f), Quaternion.identity, new Vector3(0.14f, 0.09f, 0.14f)));
        for (int i = 0; i < 4; i++)
        {
            var gh = Mat("GheNhua" + i, null, 1, 1, mau[i], 0.1f, 0.3f);
            float a = i * 90f + (float)r.NextDouble() * 25f;
            var gg = g * Matrix4x4.TRS(Quaternion.Euler(0, a, 0) * new Vector3(0, 0, 0.6f), Quaternion.Euler(0, a + 8, 0), Vector3.one);
            HopR(b, gh, gg, -0.15f, 0.15f, 0.22f, 0.26f, -0.15f, 0.15f);
            HopR(b, gh, gg, -0.13f, 0.13f, 0f, 0.22f, -0.13f, 0.13f);
        }
        var bong = Mat("BongTron", null, 1, 1, "#000000", 0f, 0f, false, new Color(1f, 0.8f, 0.5f) * 1.6f);
        b.AddMesh(bong, Cau(), g * Matrix4x4.TRS(new Vector3(0, 2.4f, -0.3f), Quaternion.identity, Vector3.one * 0.1f));
        Thanh(b, Mat("Day", null, 1, 1, "#0C0C0C", 0f, 0f), g.MultiplyPoint3x4(new Vector3(0, 2.45f, -0.3f)), g.MultiplyPoint3x4(new Vector3(0, 3.3f, -0.6f)), 0.012f);
    }

    // bảng chữ: một tấm Quad dán ảnh chữ (LOC_Nguon/XeBuyt/chu.py) — TextMesh trong project vẽ đè qua tường nên không dùng.
    // Quad đọc được khi nhìn theo +Z cục bộ của nó. phat > 0: chữ tự sáng (biển tuyến).
    static void BangChu(string ten, string tex, Vector3 pos, Quaternion rot, float w, float h, float phat = 0f, Transform cha = null)
    {
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad); q.name = ten;
        Object.DestroyImmediate(q.GetComponent<Collider>());
        q.transform.SetParent(cha ? cha : grp, false);
        q.transform.localPosition = pos; q.transform.localRotation = rot; q.transform.localScale = new Vector3(w, h, 1);
        var m = new Material(Shader.Find("Universal Render Pipeline/Simple Lit")) { name = "KP_" + ten };
        var t = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/Textures/{tex}");
        if (!t) Debug.LogWarning("[KhuPho] thiếu texture " + tex);
        m.SetTexture("_BaseMap", t); m.mainTexture = t; m.SetColor("_BaseColor", Color.white); m.color = Color.white;
        m.SetColor("_SpecColor", new Color(0.05f, 0.05f, 0.05f, 1)); m.SetFloat("_Smoothness", 0.2f);
        if (phat > 0) { m.EnableKeyword("_EMISSION"); m.SetTexture("_EmissionMap", t); m.SetColor("_EmissionColor", Color.white * phat); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; }
        Save(m, "KP_" + ten);
        q.GetComponent<MeshRenderer>().sharedMaterial = m;
    }

    // ═════════════════════════ đường chạy của xe
    static float ZLan(float x) => x > 60 ? -10.2f : x < 20 ? -8.65f : Mathf.Lerp(-8.65f, -10.2f, 0.5f - 0.5f * Mathf.Cos(Mathf.PI * (x - 20f) / 40f));

    // tâm xe trên mặt đường, ~1 m một điểm: đường ngang (đi nam) → cua phải bán kính 4 m → phố nhà (đi tây, từ giữa đường nhập dần về làn sát vỉa hè) → qua nhà LỘC tới x −40
    static Vector3[] TaoDuong(out int iDung, out float[] toc, out float[] moc)
    {
        const float y = -0.30f, R = 4f;
        var c = new Vector2(XL - R, -6.2f);
        var p = new List<Vector3>(); var v = new List<float>();
        for (float z = ZDau; z > c.y; z -= 1f)
        {
            p.Add(new Vector3(XL, y, z));
            v.Add(z > 205 ? 10f : z > 75 ? 8f : z > 25 ? 8.5f : Mathf.Lerp(3.2f, 8.5f, (z - c.y) / (25f - c.y)));
        }
        for (int i = 0; i <= 8; i++) { float a = -Mathf.PI / 2 * i / 8; p.Add(new Vector3(c.x + R * Mathf.Cos(a), y, c.y + R * Mathf.Sin(a))); v.Add(3.2f); }
        iDung = -1;
        for (float x = c.x - 1f; x > -40f; x -= 1f)
        {
            if (iDung < 0 && x < 3.8f) { iDung = p.Count; p.Add(new Vector3(3.8f, y, ZLan(3.8f))); v.Add(0f); }
            p.Add(new Vector3(x, y, ZLan(x)));
            v.Add(iDung >= 0 ? 7f : x >= 120f ? Mathf.Lerp(3.2f, 9f, (c.x - x) / (c.x - 120f)) : 9f);
        }
        toc = v.ToArray();
        // mốc thoại: quãng đường tới điểm đầu tiên thoả điều kiện
        var s = new float[p.Count];
        for (int i = 1; i < p.Count; i++) s[i] = s[i - 1] + Vector3.Distance(p[i - 1], p[i]);
        float Toi(System.Func<Vector3, bool> f) { for (int i = 0; i < p.Count; i++) if (f(p[i])) return s[i]; return s[^1]; }
        moc = new[]
        {
            Toi(q => q.z <= 280f), Toi(q => q.z <= 238f), Toi(q => q.z <= 214f), Toi(q => q.z <= ZBen + 24f),
            Toi(q => q.z <= ZBen - 18f), Toi(q => q.z <= 60f), Toi(q => q.z < -9f && q.x <= 110f), Toi(q => q.z < -8f && q.x <= 40f),
        };
        return p.ToArray();
    }

    // ═════════════════════════ xe buýt
    // Toạ độ cục bộ: +Z mũi xe, +X bên phải (phía cửa), Y = 0 mặt đường. Dài 9,0 · rộng 2,4 · nóc 2,9 · sàn trong 0,75.
    static void XeBuyt()
    {
        var duong = TaoDuong(out int iDung, out var toc, out var moc);
        var xe = new GameObject("XeBuyt_MoDau").transform; xe.SetParent(grp, false);

        var son = Mat("XB_SonKem", "T_painted_metal_shutter.jpg", 1.6f, 1.6f, "#CFC2A0", 0.15f, 0.35f);
        var soc = Mat("XB_SocDo", null, 1, 1, "#6B2622", 0.15f, 0.35f);
        var mai = Mat("XB_Mai", null, 1, 1, "#B4AE9E", 0.1f, 0.3f);
        var den = Mat("XB_Den", null, 1, 1, "#191919", 0.1f, 0.3f);
        var cot = Mat("XB_CotKinh", null, 1, 1, "#2B2B2A", 0.1f, 0.3f);
        var lot = Mat("XB_LotTrong", null, 1, 1, "#8E9488", 0.05f, 0.2f);
        var tran = Mat("XB_Tran", null, 1, 1, "#D6D0BC", 0.05f, 0.2f);
        var san = Mat("XB_San", null, 1, 1, "#3A342C", 0.05f, 0.2f);
        var caosu = Mat("XB_CaoSu", null, 1, 1, "#242322", 0.05f, 0.1f);
        var ghe = Mat("XB_Ghe", null, 1, 1, "#3E6560", 0.15f, 0.4f);
        var vo = Mat("XB_VoGhe", null, 1, 1, "#65686A", 0.1f, 0.3f);
        var inox = Mat("XB_Inox", null, 1, 1, "#A9ADB0", 0.6f, 0.7f);
        var quai = Mat("XB_QuaiNam", null, 1, 1, "#BDB29A", 0.1f, 0.3f);
        var vang = Mat("XB_VachVang", null, 1, 1, "#A88A2E", 0.1f, 0.3f);
        var lop = Mat("XB_Lop", null, 1, 1, "#141414", 0.05f, 0.15f);
        var tuyp = Mat("XB_DenTuyp", null, 1, 1, "#000000", 0f, 0f, false, new Color(0.86f, 0.92f, 0.82f) * 1.1f);
        var pha = Mat("XB_DenPha", null, 1, 1, "#000000", 0f, 0f, false, new Color(1f, 0.9f, 0.7f) * 2f);
        var hau = Mat("XB_DenHau", null, 1, 1, "#000000", 0f, 0f, false, new Color(0.9f, 0.08f, 0.05f) * 1.4f);
        var kinh = KinhXe();
        var b = new MB(Matrix4x4.identity, new System.Random(2002));

        // ── gầm, bánh
        b.Box(den, -1.1f, 1.1f, 0.30f, 0.62f, -4.3f, 4.3f);
        foreach (var z in new[] { 2.75f, -2.35f })
            foreach (var sx in new[] { -1f, 1f })
            {
                b.AddMesh(lop, Cyl(), Matrix4x4.TRS(new Vector3(sx * 1.03f, 0.48f, z), Quaternion.Euler(0, 0, 90), new Vector3(0.96f, 0.15f, 0.96f)));
                b.AddMesh(inox, Cyl(), Matrix4x4.TRS(new Vector3(sx * 1.16f, 0.48f, z), Quaternion.Euler(0, 0, 90), new Vector3(0.5f, 0.03f, 0.5f)));
            }

        // ── sàn (khoét giếng cửa giữa x 0,62…1,2 · z −0,6…0,6 và cửa trước z 3,4…4,3)
        b.Box(san, -1.16f, 0.62f, 0.62f, 0.75f, -4.46f, 4.46f);
        b.Box(san, 0.62f, 1.16f, 0.62f, 0.75f, -4.46f, -0.6f);
        b.Box(san, 0.62f, 1.16f, 0.62f, 0.75f, 0.6f, 3.4f);
        b.Box(san, 0.62f, 1.16f, 0.62f, 0.75f, 4.3f, 4.46f);
        b.Box(caosu, -0.33f, 0.33f, 0.75f, 0.756f, -3.7f, 3.1f);
        foreach (var (z0, z1) in new[] { (-0.6f, 0.6f), (3.4f, 4.3f) })   // hai bậc trong giếng cửa + mép vàng
        {
            b.Box(san, 0.62f, 0.92f, 0.30f, 0.52f, z0, z1);
            b.Box(san, 0.92f, 1.18f, 0.25f, 0.30f, z0, z1);
            b.Box(vang, 0.90f, 0.92f, 0.52f, 0.53f, z0, z1);
            b.Box(vang, 0.60f, 0.62f, 0.75f, 0.756f, z0, z1);
            b.Box(lot, 0.62f, 1.16f, 0.25f, 0.75f, z0 - 0.02f, z0);
            b.Box(lot, 0.62f, 1.16f, 0.25f, 0.75f, z1, z1 + 0.02f);
        }
        // hộp vè bánh trong khoang
        foreach (var z in new[] { 2.75f, -2.35f })
            foreach (var sx in new[] { -1f, 1f })
                b.Box(lot, sx > 0 ? 0.8f : -1.16f, sx > 0 ? 1.16f : -0.8f, 0.75f, 1.05f, z - 0.5f, z + 0.5f);

        // ── vỏ hai bên: dưới cửa sổ (vè bánh khoét), sọc đỏ đô, cột kính, kính, dải trên kính
        var phai = new[] { (-4.5f, -0.6f), (0.6f, 3.4f), (4.3f, 4.5f) };
        var trai = new[] { (-4.5f, 4.5f) };
        foreach (var (sx, doan) in new[] { (1f, phai), (-1f, trai) })
        {
            float xo0 = sx > 0 ? 1.16f : -1.2f, xo1 = sx > 0 ? 1.2f : -1.16f;
            float xs0 = sx > 0 ? 1.2f : -1.207f, xs1 = sx > 0 ? 1.207f : -1.2f;
            foreach (var (z0, z1) in doan)
            {
                b.Box(son, xo0, xo1, 0.98f, 1.55f, z0, z1);
                foreach (var (a0, a1) in Tru(z0, z1, new[] { (2.2f, 3.3f), (-2.9f, -1.8f) }))
                    b.Box(son, xo0, xo1, 0.30f, 0.98f, a0, a1);
                b.Box(soc, xs0, xs1, 1.30f, 1.42f, z0, z1);
                foreach (var (a0, a1) in Tru(z0, z1, new[] { (2.2f, 3.3f), (-2.9f, -1.8f) }))
                    b.Box(soc, xs0, xs1, 0.30f, 0.40f, a0, a1);
                b.Box(lot, sx > 0 ? 1.10f : -1.16f, sx > 0 ? 1.16f : -1.10f, 0.75f, 1.55f, z0, z1);
            }
            b.Box(son, xo0, xo1, 2.45f, 2.74f, -4.5f, 4.5f);
            foreach (var (z0, z1) in doan) b.Box(cot, xo0, xo1, 1.55f, 1.60f, z0, z1);
            for (float z = -4.45f; z < 4.5f; z += 1.1f)
                if (sx < 0 || !(z + 0.08f > -0.6f && z < 0.6f) && !(z + 0.08f > 3.4f && z < 4.3f)) b.Box(cot, xo0, xo1, 1.55f, 2.45f, z, z + 0.08f);
            if (sx < 0) b.Box(cot, xo0, xo1, 1.55f, 2.45f, 4.42f, 4.5f);
            else foreach (var z in new[] { -0.66f, 0.6f, 3.34f, 4.3f }) b.Box(cot, xo0, xo1, 0.30f, 2.45f, z, z + 0.06f);   // khung cửa
        }
        // kính hai bên (bên phải: ô cửa sổ của Khôi kéo mở nửa trước)
        b.Box(kinh, -1.19f, -1.18f, 1.60f, 2.45f, -4.4f, 4.42f);
        b.Box(kinh, 1.18f, 1.19f, 1.60f, 2.45f, -4.4f, -1.95f);   // [25/10] ô cạnh ghế Khôi (z −1,95…−0,66) để trống: nhìn ra ngoài không qua kính
        b.Box(kinh, 1.18f, 1.19f, 1.60f, 2.45f, 4.36f, 4.42f);
        b.Box(kinh, 1.172f, 1.182f, 1.60f, 2.45f, -2.6f, -1.95f);   // tấm kéo trượt đã đẩy lùi, chồng lên tấm sau
        b.Box(kinh, 1.18f, 1.19f, 1.60f, 2.45f, 0.66f, 3.34f);

        // ── mũi + đuôi
        b.Box(son, -1.2f, 1.2f, 0.30f, 1.15f, 4.46f, 4.5f);
        b.Box(soc, -1.2f, 1.2f, 0.95f, 1.07f, 4.5f, 4.507f);
        b.Box(den, -1.22f, 1.22f, 0.30f, 0.46f, 4.5f, 4.62f);
        b.Box(den, -0.5f, 0.5f, 0.55f, 0.85f, 4.5f, 4.52f);
        b.Box(pha, 0.72f, 1.0f, 0.62f, 0.80f, 4.5f, 4.53f);
        b.Box(pha, -1.0f, -0.72f, 0.62f, 0.80f, 4.5f, 4.53f);
        b.Box(kinh, -1.16f, 1.16f, 1.15f, 2.45f, 4.47f, 4.48f);
        b.Box(cot, -0.03f, 0.03f, 1.15f, 2.45f, 4.46f, 4.5f);
        b.Box(cot, -1.2f, -1.14f, 1.15f, 2.45f, 4.46f, 4.5f);
        b.Box(cot, 1.14f, 1.2f, 1.15f, 2.45f, 4.46f, 4.5f);
        b.Box(den, -1.2f, 1.2f, 2.45f, 2.74f, 4.46f, 4.5f);
        b.Box(son, -1.2f, 1.2f, 0.30f, 1.55f, -4.5f, -4.46f);
        b.Box(soc, -1.2f, 1.2f, 1.30f, 1.42f, -4.507f, -4.5f);
        b.Box(den, -1.22f, 1.22f, 0.30f, 0.46f, -4.62f, -4.5f);
        b.Box(hau, 0.82f, 1.05f, 0.58f, 0.86f, -4.53f, -4.5f);
        b.Box(hau, -1.05f, -0.82f, 0.58f, 0.86f, -4.53f, -4.5f);
        b.Box(kinh, -1.1f, 1.1f, 1.6f, 2.35f, -4.48f, -4.47f);
        b.Box(son, -1.2f, 1.2f, 1.55f, 1.6f, -4.5f, -4.46f);
        b.Box(son, -1.2f, -1.1f, 1.6f, 2.74f, -4.5f, -4.46f);
        b.Box(son, 1.1f, 1.2f, 1.6f, 2.74f, -4.5f, -4.46f);
        b.Box(son, -1.2f, 1.2f, 2.35f, 2.74f, -4.5f, -4.46f);
        // gương chiếu hậu ngoài
        foreach (var sx in new[] { -1f, 1f })
        {
            Thanh(b, den, new Vector3(sx * 1.2f, 2.2f, 4.4f), new Vector3(sx * 1.45f, 2.0f, 4.7f), 0.03f);
            b.Box(den, sx > 0 ? 1.38f : -1.52f, sx > 0 ? 1.52f : -1.38f, 1.55f, 2.0f, 4.66f, 4.72f);
        }

        // ── nóc + trần trong
        b.Box(mai, -1.2f, 1.2f, 2.74f, 2.84f, -4.5f, 4.5f);
        b.Box(mai, -1.0f, 1.0f, 2.84f, 2.9f, -4.4f, 4.4f);
        b.Box(mai, -0.45f, 0.45f, 2.9f, 3.0f, 0.6f, 1.5f);
        b.Box(tran, -1.16f, 1.16f, 2.62f, 2.66f, -4.46f, 4.46f);
        b.Box(tuyp, 0.48f, 0.6f, 2.59f, 2.62f, -3.9f, 3.0f);
        b.Box(tuyp, -0.6f, -0.48f, 2.59f, 2.62f, -3.9f, 3.0f);

        // ── tay vịn: hai thanh dọc trần + quai nắm, hai cột đứng hai bên cửa giữa và cửa trước
        foreach (var sx in new[] { -1f, 1f })
        {
            Thanh(b, inox, new Vector3(sx * 0.42f, 2.42f, -4.0f), new Vector3(sx * 0.42f, 2.42f, 3.0f), 0.035f);
            for (float z = -3.7f; z < 3.0f; z += 0.62f)
            {
                b.Box(quai, sx * 0.42f - 0.015f, sx * 0.42f + 0.015f, 2.2f, 2.42f, z - 0.008f, z + 0.008f);
                b.Box(quai, sx * 0.42f - 0.015f, sx * 0.42f + 0.015f, 2.12f, 2.2f, z - 0.05f, z + 0.05f);
            }
        }
        foreach (var z in new[] { -0.64f, 0.64f, 3.36f })
            b.AddMesh(inox, Cyl(), Matrix4x4.TRS(new Vector3(0.6f, 1.68f, z), Quaternion.identity, new Vector3(0.04f, 0.94f, 0.04f)));

        // ── ghế: hàng phải (sau cửa giữa có ghế Khôi z −1,4) và hàng trái, ghế băng cuối
        foreach (var z in new[] { -1.4f, -2.2f, -3.0f, 1.3f, 2.1f }) Ghe(b, ghe, vo, inox, den, 0.33f, 1.15f, z);
        foreach (var z in new[] { -3.0f, -2.2f, -1.4f, -0.6f, 0.2f, 1.0f, 1.8f, 2.6f }) Ghe(b, ghe, vo, inox, den, -1.15f, -0.33f, z);
        b.Box(ghe, -1.15f, 1.15f, 1.08f, 1.18f, -4.3f, -3.85f);
        b.Box(ghe, -1.15f, 1.15f, 1.18f, 1.78f, -4.42f, -4.3f);
        b.Box(vo, -1.15f, 1.15f, 0.75f, 1.08f, -4.3f, -3.9f);
        // một chỗ rách trên nệm ghế trước mặt Khôi
        b.Box(Mat("XB_GheRach", null, 1, 1, "#C9B98E", 0.05f, 0.2f), 0.55f, 0.72f, 1.181f, 1.183f, -0.8f, -0.68f);

        // ── chỗ bác tài: vách ngăn, ghế, bảng đồng hồ, vô lăng, gương trong + bùa đỏ treo
        b.Box(lot, -1.16f, -0.2f, 0.75f, 1.55f, 3.15f, 3.18f);
        b.Box(inox, -1.16f, -0.2f, 1.55f, 1.58f, 3.14f, 3.19f);
        b.Box(den, -0.85f, -0.35f, 1.08f, 1.2f, 3.5f, 3.95f);
        b.BoxM(den, Matrix4x4.TRS(new Vector3(-0.6f, 1.52f, 3.42f), Quaternion.Euler(-10, 0, 0), Vector3.one), new Vector3(0.5f, 0.66f, 0.08f));
        b.Box(den, -1.16f, 1.16f, 0.75f, 1.18f, 4.2f, 4.46f);
        b.Box(Mat("XB_DongHo", null, 1, 1, "#000000", 0f, 0f, false, new Color(0.3f, 0.6f, 0.35f) * 0.5f), -0.75f, -0.45f, 1.18f, 1.2f, 4.25f, 4.32f);
        var tamVL = new Vector3(-0.6f, 1.42f, 4.05f); var n = new Vector3(0, 0.55f, -0.83f).normalized;
        var u = Vector3.right; var w2 = Vector3.Cross(n, u).normalized;
        for (int i = 0; i < 14; i++)
        {
            float a0 = i / 14f * Mathf.PI * 2, a1 = (i + 1) / 14f * Mathf.PI * 2;
            Thanh(b, den, tamVL + 0.2f * (Mathf.Cos(a0) * u + Mathf.Sin(a0) * w2), tamVL + 0.2f * (Mathf.Cos(a1) * u + Mathf.Sin(a1) * w2), 0.03f);
        }
        Thanh(b, den, tamVL, new Vector3(-0.6f, 1.05f, 4.3f), 0.05f);
        b.Box(den, -0.15f, 0.15f, 2.3f, 2.38f, 4.36f, 4.42f);
        var bua = Mat("XB_BuaDo", null, 1, 1, "#8A1C16", 0.1f, 0.3f);
        Thanh(b, bua, new Vector3(0, 2.3f, 4.39f), new Vector3(0, 2.12f, 4.39f), 0.008f);
        b.Box(bua, -0.025f, 0.025f, 2.04f, 2.12f, 4.375f, 4.405f);
        b.Box(Mat("XB_BienBao", null, 1, 1, "#E6DFCB", 0.05f, 0.2f), -0.95f, 0.95f, 2.47f, 2.6f, 4.43f, 4.45f);   // bảng "xin nhường ghế" trên kính lái
        BacTai(b, den);
        b.Commit(xe, "Than", default, false);

        // ── cửa giữa: hai cánh, bản lề ở mép trước / sau, mở xoay vào trong
        Transform Canh(string ten, float zBanLe, float zXa)
        {
            var p = new Vector3(1.17f, 0.30f, zBanLe);
            var g = new GameObject(ten).transform; g.SetParent(xe, false); g.localPosition = p;
            var bc = new MB(Matrix4x4.Translate(-p), new System.Random(3));
            float z0 = Mathf.Min(zBanLe, zXa), z1 = Mathf.Max(zBanLe, zXa);
            bc.Box(cot, 1.155f, 1.185f, 0.30f, 1.35f, z0, z1);
            bc.Box(soc, 1.185f, 1.19f, 1.0f, 1.12f, z0, z1);
            bc.Box(cot, 1.155f, 1.185f, 2.36f, 2.45f, z0, z1);
            bc.Box(cot, 1.155f, 1.185f, 1.35f, 2.36f, z0, z0 + 0.06f);
            bc.Box(cot, 1.155f, 1.185f, 1.35f, 2.36f, z1 - 0.06f, z1);
            bc.Box(kinh, 1.165f, 1.175f, 1.35f, 2.36f, z0 + 0.06f, z1 - 0.06f);
            bc.Commit(g, "La", default, false);
            return g;
        }
        var canhTruoc = Canh("CanhCua_Truoc", 0.6f, 0.0f);
        var canhSau = Canh("CanhCua_Sau", -0.6f, 0.0f);
        // cửa trước (đóng, không mở)
        var bf = new MB(Matrix4x4.identity, new System.Random(4));
        bf.Box(cot, 1.155f, 1.185f, 0.30f, 1.35f, 3.4f, 4.3f);
        bf.Box(soc, 1.185f, 1.19f, 1.0f, 1.12f, 3.4f, 4.3f);
        bf.Box(cot, 1.155f, 1.185f, 1.35f, 2.45f, 3.83f, 3.87f);
        bf.Box(kinh, 1.165f, 1.175f, 1.35f, 2.45f, 3.4f, 4.3f);
        bf.Commit(xe, "CuaTruoc", default, false);

        // ── chữ: biển tuyến (hư cấu), bảng nhường ghế, số tuyến bên hông
        BangChu("XB_BienTuyen", "T_Chu_BienTuyen.png", new Vector3(0, 2.6f, 4.505f), Quaternion.Euler(0, 180, 0), 1.6f, 0.2f, 1.3f, xe);
        BangChu("XB_BangNhuongGhe", "T_Chu_NhuongGhe.png", new Vector3(0, 2.535f, 4.425f), Quaternion.identity, 1.86f, 0.13f, 0f, xe);
        BangChu("XB_CamHutThuoc", "T_Chu_CamHutThuoc.png", new Vector3(1.15f, 2.55f, 3.85f), Quaternion.Euler(0, 90, 0), 0.36f, 0.09f, 0f, xe);

        // ── đèn: ba bóng trong khoang (bóng giữa hay chập chờn), pha trước, hậu
        var dens = new List<Light>();
        foreach (var z in new[] { -2.6f, 0.2f, 2.4f })
        {
            var l = new GameObject("Den_Trong").AddComponent<Light>(); l.transform.SetParent(xe, false);
            l.transform.localPosition = new Vector3(0, 2.38f, z);
            l.type = LightType.Point; l.color = Hex("#D6E4CC"); l.range = 4.2f; l.intensity = 0.85f; l.shadows = LightShadows.None;
            dens.Add(l);
        }
        var sp = new GameObject("Den_Pha").AddComponent<Light>(); sp.transform.SetParent(xe, false);
        sp.transform.localPosition = new Vector3(0, 0.8f, 4.7f); sp.transform.localRotation = Quaternion.Euler(8, 0, 0);
        sp.type = LightType.Spot; sp.color = Hex("#FFE2B0"); sp.range = 28f; sp.spotAngle = 70f; sp.intensity = 3f; sp.shadows = LightShadows.None;
        var lh = new GameObject("Den_Hau").AddComponent<Light>(); lh.transform.SetParent(xe, false);
        lh.transform.localPosition = new Vector3(0, 0.75f, -4.8f);
        lh.type = LightType.Point; lh.color = Hex("#FF2A1A"); lh.range = 3f; lh.intensity = 0.7f; lh.shadows = LightShadows.None;

        var ghe0 = new GameObject("Ghe_Khoi").transform; ghe0.SetParent(xe, false); ghe0.localPosition = new Vector3(0.93f, 0.75f, -1.35f);

        // ── cảnh mở đầu
        var lx = xe.gameObject.AddComponent<LocXeBuyt>();
        lx.duong = duong; lx.toc = toc; lx.moc = moc; lx.iDung = iDung;
        lx.ghe = ghe0; lx.canhTruoc = canhTruoc; lx.canhSau = canhSau; lx.denTrong = dens.ToArray();
        AudioClip A(string ten) => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LOC_House/Audio/" + ten + ".wav");
        lx.dongCo = A("XeBuyt_DongCo"); lx.cuaMo = A("XeBuyt_CuaMo"); lx.cuaDong = A("XeBuyt_CuaDong"); lx.phanh = A("XeBuyt_Phanh");
        lx.coTrung = A("Dem_CoTrung"); lx.buoc = A("Buoc_Bac");
        xe.SetPositionAndRotation(duong[0], Quaternion.LookRotation(duong[1] - duong[0]));
        foreach (var t in xe.GetComponentsInChildren<Transform>(true)) GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);   // xe chạy → không static
        Debug.Log($"[LOC xe buýt] đường {duong.Length} điểm, dừng ở điểm {iDung}, mốc thoại: {string.Join(", ", System.Array.ConvertAll(moc, m => m.ToString("0")))} m");
    }

    // phần còn lại của [a,b] sau khi bỏ các khoảng cat
    static List<(float, float)> Tru(float a, float b, (float, float)[] cat)
    {
        var kq = new List<(float, float)> { (a, b) };
        foreach (var (c0, c1) in cat)
        {
            var moi = new List<(float, float)>();
            foreach (var (x0, x1) in kq)
            {
                if (c1 <= x0 || c0 >= x1) { moi.Add((x0, x1)); continue; }
                if (c0 > x0) moi.Add((x0, c0));
                if (c1 < x1) moi.Add((c1, x1));
            }
            kq = moi;
        }
        return kq;
    }

    static void Ghe(MB b, Material nem, Material vo, Material inox, Material sat, float x0, float x1, float zc)
    {
        b.Box(nem, x0, x1, 1.08f, 1.18f, zc - 0.21f, zc + 0.21f);
        b.Box(vo, x0 + 0.02f, x1 - 0.02f, 1.02f, 1.08f, zc - 0.2f, zc + 0.2f);
        b.BoxM(nem, Matrix4x4.TRS(new Vector3((x0 + x1) / 2, 1.48f, zc - 0.25f), Quaternion.Euler(-8, 0, 0), Vector3.one), new Vector3(x1 - x0, 0.62f, 0.07f));
        b.BoxM(vo, Matrix4x4.TRS(new Vector3((x0 + x1) / 2, 1.48f, zc - 0.295f), Quaternion.Euler(-8, 0, 0), Vector3.one), new Vector3(x1 - x0 - 0.02f, 0.6f, 0.025f));
        Thanh(b, inox, new Vector3(x0 + 0.03f, 1.82f, zc - 0.34f), new Vector3(x1 - 0.03f, 1.82f, zc - 0.34f), 0.03f);
        b.Box(sat, x0 + 0.05f, x0 + 0.08f, 0.75f, 1.02f, zc - 0.15f, zc - 0.12f);
        b.Box(sat, x1 - 0.08f, x1 - 0.05f, 0.75f, 1.02f, zc - 0.15f, zc - 0.12f);
    }

    // bác tài ngồi, hai tay trên vô lăng — khối thô kiểu PS1, quay lưng về khách
    static void BacTai(MB b, Material ghe)
    {
        var ao = Mat("XB_AoBacTai", null, 1, 1, "#6E7C86", 0.05f, 0.2f);
        var quan = Mat("XB_QuanBacTai", null, 1, 1, "#2E2C2A", 0.05f, 0.2f);
        var da = Mat("XB_Da", null, 1, 1, "#7A5A44", 0.05f, 0.25f);
        var toc = Mat("XB_Toc", null, 1, 1, "#141211", 0.05f, 0.2f);
        b.Box(quan, -0.8f, -0.4f, 1.2f, 1.36f, 3.55f, 3.98f);
        Thanh(b, quan, new Vector3(-0.7f, 1.27f, 3.96f), new Vector3(-0.7f, 0.8f, 4.15f), 0.13f);
        Thanh(b, quan, new Vector3(-0.5f, 1.27f, 3.96f), new Vector3(-0.5f, 0.8f, 4.15f), 0.13f);
        b.BoxM(ao, Matrix4x4.TRS(new Vector3(-0.6f, 1.62f, 3.66f), Quaternion.Euler(-6, 0, 0), Vector3.one), new Vector3(0.44f, 0.6f, 0.24f));
        b.AddMesh(da, Cyl(), Matrix4x4.TRS(new Vector3(-0.6f, 1.96f, 3.68f), Quaternion.identity, new Vector3(0.1f, 0.05f, 0.1f)));
        b.AddMesh(da, Cau(), Matrix4x4.TRS(new Vector3(-0.6f, 2.1f, 3.69f), Quaternion.identity, new Vector3(0.2f, 0.25f, 0.22f)));
        b.AddMesh(toc, Cau(), Matrix4x4.TRS(new Vector3(-0.6f, 2.15f, 3.67f), Quaternion.identity, new Vector3(0.215f, 0.2f, 0.225f)));
        foreach (var sx in new[] { -1f, 1f })
        {
            var vai = new Vector3(-0.6f + sx * 0.22f, 1.86f, 3.68f);
            var khuyu = new Vector3(-0.6f + sx * 0.27f, 1.56f, 3.86f);
            var tay = new Vector3(-0.6f + sx * 0.17f, 1.45f, 4.02f);
            Thanh(b, ao, vai, khuyu, 0.1f);
            Thanh(b, da, khuyu, tay, 0.075f);
        }
    }

    static Material KinhXe()
    {
        if (mats.TryGetValue("XB_Kinh", out var c) && c) return c;
        var m = new Material(Shader.Find("Universal Render Pipeline/Simple Lit")) { name = "KP_XB_Kinh" };
        var col = new Color(0.30f, 0.38f, 0.36f, 0.08f);   // [25/10] kính trong hơn: đèn trong xe hắt lên kính làm mờ hết cảnh ngoài
        m.SetColor("_BaseColor", col); m.color = col;
        m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0); m.SetFloat("_Cull", 0);
        m.SetOverrideTag("RenderType", "Transparent"); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        m.SetColor("_SpecColor", new Color(0.12f, 0.12f, 0.12f, 1)); m.SetFloat("_Smoothness", 0.5f);
        Save(m, "KP_XB_Kinh");
        tile[m] = (1f, 1f);
        return mats["XB_Kinh"] = m;
    }

    // ═════════════════════════ thử trong Play: chạy cảnh mở đầu, chụp ảnh vào LOC_KiemTra/XeBuyt/, tự thoát
    [MenuItem("LOC/Thử xe buýt mở đầu (Play)")]
    static void ThuXeBuyt()
    {
        if (EditorApplication.isPlaying) return;
        if (System.IO.Directory.Exists("LOC_KiemTra/XeBuyt"))
            foreach (var f in System.IO.Directory.GetFiles("LOC_KiemTra/XeBuyt")) System.IO.File.Delete(f);
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/LOC_NhaLoc.unity") EditorSceneManager.OpenScene("Assets/Scenes/LOC_NhaLoc.unity");
        EditorPrefs.SetBool("LOC_ThuXeBuyt", true);
        EditorApplication.isPlaying = true;
    }
}
