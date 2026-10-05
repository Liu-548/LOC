using UnityEngine;
using UnityEditor;

// [2/10 tối] KHO TIỆM VẬT LIỆU dựng lại: lòng 5,80 × 17,20 (x −6,20…−0,20 · z 1,90…19,10), trần tôn 3,60.
// Lối đi chính x −3,40…−1,60 thẳng với ô cửa vách kho (x −2,80…−1,60); dãy kệ thép sát tường trái (x −6,20…−5,70),
// dải đảo giữa (x −4,70…−3,40), dải sát tường phải (x −1,55…−0,25). Góc đông-nam trong cùng (x −1,60…−0,20 · z 14,80…19,10) là hộp thang xuống hầm.
public static partial class LocHouseBuilder
{
    static Material KeThepMat => M("M_KeThep", "#2E5379");
    static Material PalletMat => M("M_PalletGo", "#9A7B52");
    static Material GachDoMat => M("M_GachDo", "#9E4A35");
    static Material HopGachMat => M("M_HopGach", "#B28C5B");
    static Material ThepRiMat => M("M_ThepRi", "#6E4631");
    static Material OngTrangMat => M("M_OngPVC_Trang", "#E4E1D5");
    static Material OngXanhMat => M("M_OngPVC_Xanh", "#2D6BA6");
    static Material VachVangMat => M("M_VachVang_San", "#C9A227");

    // đồ trong kho là CoDinh: rà soát tự động không dời (xếp khít nhau) → collider khối vô hình thay cho từng món
    static GameObject Ak(string name, float x, float z, float y, float yaw = 0) => A(name, x, z, y, yaw, k: LocProp.Kieu.CoDinh);
    static void Vol(float x0, float x1, float y0, float y1, float z0, float z1)
    {
        var g = new GameObject("Vol_Kho"); g.transform.SetParent(cur, false);
        g.transform.position = new Vector3((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2);
        var bc = g.AddComponent<BoxCollider>(); bc.size = new Vector3(x1 - x0, y1 - y0, z1 - z0);
    }

    // pallet gỗ 1,30 × 0,90 × 0,12
    static void Pallet(float xc, float zc, float w = 1.3f, float d = 0.9f)
    {
        Box("Pallet_Van", xc - w / 2, xc + w / 2, 0.09f, 0.12f, zc - d / 2, zc + d / 2, PalletMat);
        foreach (var dz in new[] { -d / 2, -0.05f, d / 2 - 0.1f }) Box("Pallet_Chan", xc - w / 2, xc + w / 2, 0f, 0.09f, zc + dz, zc + dz + 0.1f, PalletMat);
    }

    // chồng bao xi măng: 4 bao / lớp (2 × 2), lớp cuối thấp hơn một chút cho "chồng dở"
    static void ChongBao(float xc, float zc, int layers, string lopCuoi = "BaoXiMang_Chong")
    {
        Pallet(xc, zc);
        for (int k = 0; k < layers; k++)
            for (int i = 0; i < 2; i++) for (int j = 0; j < 2; j++)
                Ak(k == layers - 1 ? lopCuoi : "BaoXiMang_Chong", xc + (i - 0.5f) * 0.65f, zc + (j - 0.5f) * 0.45f, 0.12f + k * 0.19f, (k % 2) * 180);
        Vol(xc - 0.65f, xc + 0.65f, 0.12f, 0.12f + layers * 0.19f, zc - 0.45f, zc + 0.45f);
    }

    static void ChongGach(float xc, float zc, int rows)
    {
        Pallet(xc, zc);
        for (int k = 0; k < rows; k++)
        {
            float y0 = 0.12f + k * 0.07f, s = (k % 2 == 0) ? 0f : 0.015f;
            Box("Gach_Hang", xc - 0.6f + s, xc + 0.6f + s, y0, y0 + 0.065f, zc - 0.42f, zc + 0.42f, GachDoMat, false);
        }
        Vol(xc - 0.62f, xc + 0.62f, 0.12f, 0.12f + rows * 0.07f, zc - 0.42f, zc + 0.42f);
    }

    static void ChongHopGach(float xc, float zc, int layers)
    {
        Pallet(xc, zc);
        for (int k = 0; k < layers; k++)
            for (int i = 0; i < 2; i++)
            {
                float x0 = xc - 0.62f + i * 0.64f, y0 = 0.12f + k * 0.12f;
                Box("HopGach", x0, x0 + 0.60f, y0, y0 + 0.115f, zc - 0.43f, zc + 0.43f, HopGachMat, false);
            }
        Vol(xc - 0.62f, xc + 0.62f, 0.12f, 0.12f + layers * 0.12f, zc - 0.43f, zc + 0.43f);
    }

    // chồng thùng sơn: 4 × 3 thùng 18 L / lớp (bước 0,31 × 0,305, mỗi lớp 0,365), lớp trên cùng thiếu vài thùng — [4/10] dựng từng thùng (ThungSonDon) thay cho file cụm
    static void ChongSon(float xc, float zc, int layers, string asset)
    {
        Pallet(xc, zc);
        var p = NhomSon("ThungSon_Chong"); int nh = asset.EndsWith("2") ? 1 : 0;
        for (int k = 0; k < layers; k++)
            for (int i = 0; i < 4; i++) for (int j = 0; j < 3; j++)
            {
                if (k == layers - 1 && (i + j * 4) % 5 == 2) continue;
                ThungSonDon(p, xc + (i - 1.5f) * 0.31f + RS(-0.004f, 0.004f), zc + (j - 1) * 0.305f + RS(-0.004f, 0.004f), 0.12f + k * 0.362f, true, nh + (k == layers - 1 && i == 3 ? 2 : 0), k == layers - 1);
            }
        Vol(xc - 0.62f, xc + 0.62f, 0.12f, 0.12f + layers * 0.362f, zc - 0.45f, zc + 0.45f);
    }

    // kệ thép: khung trụ 5 × 5 cm, tầng ván gỗ; trục dọc theo Z
    static void KeThep(float x0, float x1, float z0, float z1, float h, params float[] lv)
    {
        foreach (var xx in new[] { x0, x1 - 0.05f })
            foreach (var zz in new[] { z0, (z0 + z1) / 2 - 0.025f, z1 - 0.05f })
                Box("Ke_Tru", xx, xx + 0.05f, 0, h, zz, zz + 0.05f, KeThepMat);
        foreach (var l in lv) Box("Ke_Tang", x0, x1, l, l + 0.03f, z0, z1, Go);
        Vol(x0, x1, 0, h, z0, z1);
    }

    // xếp đều một hàng đồ dọc theo Z trên một tầng kệ
    static void HangKe(string asset, float xc, float z0, float z1, float pitch, float y, float yaw = 0)
    {
        int n = (int)((z1 - z0) / pitch);
        float pad = ((z1 - z0) - n * pitch) / 2;
        for (int i = 0; i < n; i++) Ak(asset, xc, z0 + pad + pitch * (i + 0.5f), y, yaw);
    }

    // trụ tròn nằm dọc trục Z (thép cây, ống nhựa)
    static void Tru(string n, float x, float y, float z, float len, float d, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        g.name = n; g.transform.SetParent(cur, false);
        g.transform.position = new Vector3(x, y, z);
        g.transform.rotation = Quaternion.Euler(90, 0, 0);
        g.transform.localScale = new Vector3(d, len / 2, d);
        g.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        GameObjectUtility.SetStaticEditorFlags(g, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
    }

    // giá đỡ thép/ống: 3 gối đỡ + 2 trụ chặn hai bên
    static void GiaDoDai(float xc, float z0, float z1)
    {
        foreach (var zz in new[] { z0 + 0.3f, (z0 + z1) / 2, z1 - 0.3f })
        {
            Box("Gia_Goi", xc - 0.65f, xc + 0.65f, 0.1f, 0.18f, zz - 0.04f, zz + 0.04f, KeThepMat);
            Box("Gia_Chan", xc - 0.65f, xc - 0.60f, 0.18f, 0.80f, zz - 0.03f, zz + 0.03f, KeThepMat);
            Box("Gia_Chan", xc + 0.60f, xc + 0.65f, 0.18f, 0.80f, zz - 0.03f, zz + 0.03f, KeThepMat);
        }
        Vol(xc - 0.65f, xc + 0.65f, 0f, 0.7f, z0, z1);
    }

    static void KhoTiem()
    {
        cur = G("Tiem/Kho");
        const float z0 = 1.9f, z1 = 19.1f;

        // ── vạch vàng chia lối đi chính
        foreach (var vx in new[] { -3.40f, -1.60f }) Box("VachSan", vx - 0.03f, vx + 0.03f, 0.001f, 0.005f, z0 + 0.3f, z1 - 0.2f, VachVangMat, false);

        // ── dãy kệ thép sát tường trái: 5 ô × 2,40
        const float kx0 = -6.2f, kx1 = -5.7f, kc = -5.95f;
        float[] kz = { 2.4f, 5.0f, 7.6f, 10.2f, 12.8f };
        float[] l4 = { 0.08f, 0.63f, 1.18f, 1.73f };
        for (int i = 0; i < 5; i++) KeThep(kx0, kx1, kz[i], kz[i] + 2.4f, 2.1f, i == 4 ? new[] { 0.08f, 0.73f, 1.38f } : l4);
        { var ps = NhomSon("ThungSon_Ke"); for (int s = 0; s < 4; s++) for (int i = 0; i < 7; i++) ThungSonDon(ps, kc + RS(-0.01f, 0.01f), kz[0] + 0.18f + i * 0.34f, l4[s] + 0.03f, true, s + i / 4); }   // sơn: 7 thùng 18 L / tầng
        for (int s = 0; s < 4; s++) HangKe(s % 2 == 0 ? "CuonDayDien" : "CuonDayDien_2", kc, kz[1], kz[1] + 2.4f, 0.58f, l4[s] + 0.03f);   // cuộn dây điện
        for (int s = 0; s < 4; s++) HangKe("ThungGo", kc, kz[2], kz[2] + 2.4f, 0.58f, l4[s] + 0.03f, 90);                                  // thùng đồ nghề
        for (int s = 0; s < 4; s++) HangKe("ChauNhua", kc, kz[3], kz[3] + 2.4f, 0.58f, l4[s] + 0.03f);                                       // chậu, xô nhựa
        float[] l3 = { 0.08f, 0.73f, 1.38f };
        for (int s = 0; s < 3; s++) HangKe("ThungGao_Nhua", kc, kz[4], kz[4] + 2.4f, 0.48f, l3[s] + 0.03f);                                  // thùng nhựa lớn
        // góc trong cùng bên trái: thang nhôm dựa tường, chổi, ống nước dựng
        for (int i = 0; i < 3; i++) Ak("ThangNhom", -6.14f, 15.7f + i * 0.42f, 0, 90);
        for (int i = 0; i < 4; i++) Ak("CayChoi", -6.1f, 17.2f + i * 0.28f, 0, 0);
        for (int i = 0; i < 3; i++) { Ak("OngNuoc_Nam", -6.0f + i * 0.1f, 18.82f, 0, 0); Ak("OngNuoc_Bo", -6.0f + i * 0.1f, 18.7f, 0, 0); }

        // ── dải đảo giữa (x −4,05): theo thứ tự từ cửa vào trong
        const float ix = -4.05f;
        ChongBao(ix, 2.9f, 8);
        ChongBao(ix, 3.95f, 7, "BaoXiMang_Chong2");
        ChongGach(ix, 5.3f, 13);
        ChongGach(ix, 6.35f, 9);
        ChongHopGach(ix, 7.9f, 9);
        ChongHopGach(ix, 8.95f, 6);
        // giá thép cây (nằm dọc)
        GiaDoDai(ix, 10.3f, 12.6f);
        Tru("Thep_Bo", ix - 0.30f, 0.29f, 11.45f, 2.3f, 0.22f, ThepRiMat);
        Tru("Thep_Bo", ix + 0.30f, 0.29f, 11.45f, 2.3f, 0.22f, ThepRiMat);
        Tru("Thep_Bo", ix, 0.48f, 11.45f, 2.3f, 0.22f, ThepRiMat);
        // giá ống nhựa
        GiaDoDai(ix, 12.9f, 15.2f);
        for (int i = 0; i < 4; i++) Tru("Ong_PVC", ix + (i - 1.5f) * 0.12f, 0.235f, 14.05f, 2.3f, 0.11f, i % 2 == 0 ? OngTrangMat : OngXanhMat);
        for (int i = 0; i < 3; i++) Tru("Ong_PVC", ix + (i - 1) * 0.12f, 0.335f, 14.05f, 2.3f, 0.11f, i % 2 == 0 ? OngXanhMat : OngTrangMat);
        for (int i = 0; i < 2; i++) Tru("Ong_PVC", ix + (i - 0.5f) * 0.12f, 0.435f, 14.05f, 2.3f, 0.11f, OngTrangMat);
        // cuối dải: xe rùa đỗ thẳng hàng + cuộn lưới thép + bao dở
        Ak("XeRua", ix - 0.38f, 16.5f, 0, 0);
        Ak("XeRua", ix + 0.38f, 16.5f, 0, 0);
        for (int i = 0; i < 3; i++) Ak("CuonLuoiThep", ix - 0.42f + i * 0.42f, 17.75f, 0, 0);
        ChongBao(ix, 18.5f, 3, "BaoXiMang_Le");
        Vol(ix - 0.7f, ix + 0.7f, 0, 0.85f, 15.55f, 17.45f);                 // hai xe rùa
        Vol(ix - 0.65f, ix + 0.65f, 0, 0.95f, 17.55f, 17.95f);               // ba cuộn lưới thép

        // ── dải sát tường phải (x −0,90)
        const float ex = -0.9f;
        ChongBao(ex, 2.9f, 8);
        ChongBao(ex, 3.95f, 6, "BaoXiMang_Le");
        foreach (var kzc in new[] { 5.8f, 8.1f })
        {   // [4/10] file KeGoDai có sẵn hàng: giữ các hộp trắng ở tầng 1–2, cất thùng sơn + chồng gạch viền kiểu cũ ở tầng trên (thay bằng ThungSonDon)
            var ke = A("KeGoDai", -0.41f, kzc, 0, -90);
            if (ke) foreach (var t in ke.GetComponentsInChildren<Transform>(true))
                if (t.name is "than" or "goNap" or "nap" or "quai" || t.name.StartsWith("vien_")) t.gameObject.SetActive(false);
        }
        foreach (var kzc in new[] { 5.8f, 8.1f })
        {   // [4/10] kệ gỗ dài sát tường nhà (sâu x −0,62…−0,20): tầng trên 6 thùng 18 L, tầng giữa 2 hàng thùng 4 L, tầng dưới 1 hàng — cột kệ ở x > −0,25 nên thùng không vượt quá x −0,26
            var ps = NhomSon("ThungSon_Ke");
            for (int i = 0; i < 6; i++) ThungSonDon(ps, -0.43f, kzc - 0.9f + i * 0.36f, 0.905f, true, i / 2);
            for (int i = 0; i < 11; i++)   // gầm kệ (cao 0,285): một hàng thùng 4 L
            {
                float zz = kzc - 1.0f + i * 0.2f; if (Mathf.Abs(zz - kzc) < 0.11f) continue;
                ThungSonDon(ps, -0.40f, zz, 0f, false, i % 4, i % 3 == 0);
            }
        }
        ChongSon(ex, 9.9f, 3, "ThungSon");
        ChongSon(ex, 11.0f, 3, "ThungSon_Cum2");
        ChongBao(ex, 12.3f, 7);
        ChongBao(ex, 13.8f, 6, "BaoXiMang_Chong2");

        // ── ánh sáng: đèn tuýp dọc lối đi chính + đèn điểm
        foreach (var lz in new[] { 4.5f, 9.5f, 14.0f, 18.0f })
        {
            A("DenTuyp_120", -2.5f, lz, 3.6f, 90, top: true);
            L("Kho_" + lz, -2.5f, 3.3f, lz, "#FFE9C8", 5.5f, 0.55f);
        }
    }
}
