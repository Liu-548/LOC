// Khu phố quanh nhà LỘC + bầu trời đêm. Gọi từ LocHouseBuilder.MoiTruong().
// Bầu trời: HDRI "Kloppenheim 07 (Pure Sky)" — Poly Haven, CC0. Texture tường/ngói/tôn/đường: Poly Haven, CC0 (xem HMAsset/KhuPho/NGUON_CC0.md).
// Nhà phố (nhà ống 3–7 tầng, mái bằng/ngói/tôn, cửa cuốn, chuồng cọp, lan can sắt, bồn nước) dựng bằng code — mỗi căn gộp thành 1 mesh.
// Cố ý KHÔNG có máy lạnh, biển LED, kính phản quang: bối cảnh nhà ống cũ, không hiện đại.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class LocKhuPho
{
    const string Dir = "Assets/LOC_House/KhuPho";
    const string MatDir = "Assets/LOC_House/Materials";
    enum Kieu { Mat, Sau, Xa }   // mặt tiền đủ chi tiết · mặt sau (cửa sổ nhỏ, ống nước) · xa (đơn giản)

    static readonly Dictionary<string, Material> mats = new();
    static readonly Dictionary<Material, (float u, float v)> tile = new();
    static Transform grp;
    static Mesh cyl;

    // ═════════════════════════ vào từ builder
    public static void Troi()
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture>($"{Dir}/Sky/Sky_Kloppenheim07.hdr");
        if (AssetImporter.GetAtPath($"{Dir}/Sky/Sky_Kloppenheim07.hdr") is TextureImporter ti && (ti.wrapModeV != TextureWrapMode.Clamp || ti.mipmapEnabled))
        {
            ti.wrapModeU = TextureWrapMode.Repeat; ti.wrapModeV = TextureWrapMode.Clamp; ti.mipmapEnabled = false;
            ti.SaveAndReimport();
            tex = AssetDatabase.LoadAssetAtPath<Texture>($"{Dir}/Sky/Sky_Kloppenheim07.hdr");
        }
        var m = new Material(Shader.Find("Skybox/Panoramic")) { name = "KP_Troi" };
        m.SetTexture("_MainTex", tex);
        m.SetFloat("_Exposure", Exposure);
        m.SetFloat("_Rotation", 200f);
        m.SetColor("_Tint", new Color(0.62f, 0.68f, 0.85f, 1));
        Save(m, "KP_Troi");
        RenderSettings.skybox = m;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = FogDensity;
        RenderSettings.fogColor = Hex("#1A2030");
    }
    public static float Exposure = 0.22f, FogDensity = 0.011f;

    public static void Build(Transform parent)
    {
        mats.Clear(); tile.Clear();
        grp = new GameObject("KhuPho").transform; grp.SetParent(parent, false);

        DuongPho();

        // ── dãy nhà đối diện (mặt tiền quay về nhà LỘC, +Z) + dãy thứ hai cao hơn phía sau để không lộ khe
        Hang("DoiDien_1", -47f, 57f, -14.0f, 180, 13f, 3, 5, Kieu.Mat, 3.8f, 5.6f);
        Hang("DoiDien_2", -47f, 57f, -27.0f, 180, 13f, 5, 7, Kieu.Xa, 5f, 8f);
        // ── cùng bên với nhà LỘC: trái (tiệm ở giữa, đã có trong builder) và phải
        Nha("NhaTrai_Sat", new Vector3(-10.4f, 0, -5.4f), 0, 6.0f, 7.3f, 4, Kieu.Mat, 101);          // [2/10 tối] kho tiệm nới tới x −6,4 và dài tới z 19,3 → tách khối nhà bên trái
        Nha("NhaTrai_SatSau", new Vector3(-10.4f, 0, 1.9f), 0, 4.0f, 22.9f, 4, Kieu.Sau, 104);
        Hang("BenTrai", -47f, -10.4f, -5.4f, 0, 30.2f, 3, 5, Kieu.Mat, 3.8f, 5.6f);
        Nha("NhaSauTiem", new Vector3(-6.4f, 0, 19.3f), 0, 6.2f, 5.5f, 4, Kieu.Sau, 102);
        Nha("NhaPhai_Sat", new Vector3(8.0f, 0, -0.2f), 0, 6.0f, 25.0f, 4, Kieu.Mat, 103);
        SanNhaPhai();
        Hang("BenPhai", 14f, 57f, -5.4f, 0, 30.2f, 3, 5, Kieu.Mat, 3.8f, 5.6f);
        // ── phía sau nhà LỘC (lưng nhà quay về phía nhà LỘC)
        Hang("Sau_1", -47f, 57f, 24.8f, 0, 13f, 4, 6, Kieu.Sau, 4f, 6f);
        Hang("Sau_2", -47f, 57f, 37.8f, 0, 13f, 5, 7, Kieu.Xa, 5f, 8f);
        // ── bịt hai đầu phố
        HangZ("DauTrai", -40f, 51f, -47f, false, 12f, 5, 7, Kieu.Xa, 8f, 13f);
        HangZ("DauPhai", -40f, 51f, 57f, true, 12f, 5, 7, Kieu.Xa, 8f, 13f);

        CotDien();
    }

    // ═════════════════════════ đường + vỉa hè
    static void DuongPho()
    {
        var b = new MB(Matrix4x4.identity, new System.Random(5));
        var nhua = Mat("Nhua", "T_worn_asphalt.jpg", 3f, 3f, "#8C8C90", 0.25f, 0.55f);
        var gach = Mat("ViaHe", "T_patterned_concrete_pavers_02.jpg", 2f, 2f, "#B8AAA0", 0.05f, 0.2f);
        var be = Mat("BeTong", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#9A9890");
        b.Box(nhua, -50, 60, -0.65f, -0.30f, -14.0f, -5.4f, true);
        b.Box(gach, -50, 60, -0.65f, -0.15f, -7.2f, -5.4f, true);
        b.Box(gach, -50, 60, -0.65f, -0.15f, -14.0f, -12.4f, true);
        b.Box(be, -50, 60, -0.30f, -0.15f, -7.4f, -7.2f);
        b.Box(be, -50, 60, -0.30f, -0.15f, -12.6f, -12.4f);
        b.Commit(grp, "DuongPho", new Bounds(new Vector3(5, -0.4f, -9.7f), new Vector3(110, 0.5f, 8.6f)));
    }

    // sân trước của căn phải: nền xi măng, tường thấp, cổng sắt
    static void SanNhaPhai()
    {
        var b = new MB(Matrix4x4.identity, new System.Random(9));
        var be = Mat("BeTong", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#9A9890");
        var sat = Mat("Sat", null, 1, 1, "#22201E");
        var ray = Mat("LanCanSat", "T_LanCanSat.png", 0.16f, 1.0f, "#FFFFFF", 0.05f, 0.2f, true);
        b.Box(be, 8.0f, 14.0f, -0.35f, -0.15f, -5.4f, -0.2f, true);
        b.Box(be, 8.0f, 10.4f, -0.15f, 1.0f, -5.4f, -5.2f);
        b.Box(be, 12.4f, 14.0f, -0.15f, 1.0f, -5.4f, -5.2f);
        b.Box(be, 10.4f, 10.6f, -0.15f, 1.9f, -5.4f, -5.2f);
        b.Box(be, 12.2f, 12.4f, -0.15f, 1.9f, -5.4f, -5.2f);
        b.Box(ray, 10.6f, 12.2f, -0.15f, 1.55f, -5.32f, -5.28f);
        b.Box(sat, 10.6f, 12.2f, 1.55f, 1.6f, -5.34f, -5.26f);
        b.Commit(grp, "SanNhaPhai", new Bounds(new Vector3(11, 0.4f, -2.8f), new Vector3(6, 1.5f, 5.2f)));
    }

    // ═════════════════════════ hàng nhà
    static int Hash(string s) { int h = 17; foreach (var c in s) h = h * 31 + c; return h; }

    static void Hang(string name, float x0, float x1, float zf, float yaw, float depth, int fmin, int fmax, Kieu k, float wmin, float wmax)
    {
        var r = new System.Random(Hash(name));
        int i = 0;
        for (float x = x0; x < x1 - 0.5f; i++)
        {
            float w = wmin + (float)r.NextDouble() * (wmax - wmin);
            if (x1 - (x + w) < wmin * 0.6f) w = x1 - x;
            var o = yaw == 0 ? new Vector3(x, 0, zf) : new Vector3(x + w, 0, zf);
            Nha($"{name}_{i:00}", o, yaw, w, depth, r.Next(fmin, fmax + 1), k, r.Next());
            x += w;
        }
    }

    // hàng chạy dọc Z, mặt tiền quay về phía +X (xPos = false) hoặc −X (xPos = true)
    static void HangZ(string name, float z0, float z1, float xf, bool quayVeAm, float depth, int fmin, int fmax, Kieu k, float wmin, float wmax)
    {
        var r = new System.Random(Hash(name));
        int i = 0;
        for (float z = z0; z < z1 - 0.5f; i++)
        {
            float w = wmin + (float)r.NextDouble() * (wmax - wmin);
            if (z1 - (z + w) < wmin * 0.6f) w = z1 - z;
            var o = quayVeAm ? new Vector3(xf, 0, z + w) : new Vector3(xf, 0, z);
            Nha($"{name}_{i:00}", o, quayVeAm ? 90 : -90, w, depth, r.Next(fmin, fmax + 1), k, r.Next());
            z += w;
        }
    }

    // Toạ độ cục bộ: X dọc mặt tiền (nhìn từ ngoài: trái → phải), Y lên, Z đi vào trong nhà; mặt tiền ở z = 0 quay về −Z cục bộ.
    static void Nha(string name, Vector3 origin, float yaw, float w, float d, int floors, Kieu k, int seed)
    {
        var r = new System.Random(seed);
        var xf = Matrix4x4.TRS(origin, Quaternion.Euler(0, yaw, 0), Vector3.one);
        var b = new MB(xf, r);
        float H = 3.6f + 3.3f * (floors - 1);
        var tuong = WallMat(r);
        var be = Mat("BeTong", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#9A9890");
        b.Box(tuong, 0, w, 0, H, 0, d);

        if (k == Kieu.Mat) MatTien(b, r, w, floors, H);
        else if (k == Kieu.Sau) MatSau(b, r, w, floors, H);
        else MatXa(b, r, w, floors, H);
        CuaSoHong(b, r, w, d, floors);
        MaiNha(b, r, w, d, H, k, tuong);

        // hộp va chạm = thân nhà, quy ra toạ độ thế giới
        var p0 = xf.MultiplyPoint3x4(Vector3.zero); var p1 = xf.MultiplyPoint3x4(new Vector3(w, H, d));
        var bd = new Bounds(); bd.SetMinMax(Vector3.Min(p0, p1), Vector3.Max(p0, p1));
        b.Commit(grp, name, bd);
    }

    // ───── mặt tiền đầy đủ
    static void MatTien(MB b, System.Random r, float w, int floors, float H)
    {
        var be = Mat("BeTong", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#9A9890");
        var bang = Mat("BeTongSang", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#BDB9AE");
        // bậc thềm + gờ mái
        b.Box(be, 0, w, 0, 0.15f, -0.35f, 0);
        b.Box(bang, -0.05f, w + 0.05f, H - 0.4f, H, -0.25f, 0);
        if (r.NextDouble() < 0.35)   // hai trụ giả hai bên mặt tiền
        {
            b.Box(bang, 0, 0.28f, 0, H - 0.4f, -0.14f, 0);
            b.Box(bang, w - 0.28f, w, 0, H - 0.4f, -0.14f, 0);
        }
        for (int f = 1; f < floors; f++)
        {
            float y0 = 3.6f + 3.3f * (f - 1);
            b.Box(bang, -0.03f, w + 0.03f, y0 - 0.2f, y0 + 0.04f, -0.09f, 0);
        }

        // tầng trệt
        var sat = Mat("Sat", null, 1, 1, "#22201E");
        var cuon = Mat("CuaCuon", "T_painted_metal_shutter.jpg", 1.4f, 1.4f, "#9FA4A8", 0.2f, 0.35f);
        var go = Mat("GoCua", null, 1, 1, "#4A3220");
        var toi = Mat("KinhToi", null, 1, 1, "#0A0E13", 0.3f, 0.7f);
        var song = Mat("SongSatO", "T_SongSatO.png", 0.25f, 0.25f, "#FFFFFF", 0.05f, 0.2f, true);
        int g = r.Next(10);
        if (g < 5)   // cửa cuốn đóng
        {
            b.Box(cuon, 0.3f, w - 0.3f, 0.15f, 2.75f, -0.16f, -0.04f);
            b.Box(be, 0.25f, w - 0.25f, 2.75f, 3.1f, -0.26f, -0.02f);
            if (r.NextDouble() < 0.5) Bang(b, r, w);
            if (r.NextDouble() < 0.3) Mai(b, r, w);
        }
        else if (g < 8)   // cửa nhà: cửa gỗ + cổng sắt + ô cửa sổ song sắt
        {
            float dx = w * (0.3f + 0.4f * (float)r.NextDouble());
            b.Box(go, dx - 0.5f, dx + 0.5f, 0.15f, 2.3f, -0.1f, -0.02f);
            b.Box(song, dx - 0.55f, dx + 0.55f, 0.15f, 2.35f, -0.16f, -0.13f);
            float wx = dx < w / 2 ? (dx + w) / 2 + 0.1f : dx / 2 - 0.1f;
            b.Box(toi, wx - 0.5f, wx + 0.5f, 1.0f, 2.0f, -0.08f, -0.02f);
            b.Box(song, wx - 0.55f, wx + 0.55f, 0.95f, 2.05f, -0.2f, -0.17f);
        }
        else   // quán mở: ô lớn tối/ấm
        {
            var am = Mat("KinhAm", null, 1, 1, "#000000", 0.1f, 0.4f, false, new Color(0.55f, 0.30f, 0.12f));
            b.Box(r.NextDouble() < 0.5 ? am : toi, 0.4f, w - 0.4f, 0.4f, 2.6f, -0.08f, -0.02f);
            b.Box(bang, 0.3f, w - 0.3f, 0.4f, 0.6f, -0.2f, -0.02f);
            Bang(b, r, w);
            Mai(b, r, w);
        }

        // tầng lầu
        for (int f = 1; f < floors; f++)
        {
            float y0 = 3.6f + 3.3f * (f - 1);
            bool banCong = r.NextDouble() < 0.45;
            int nb = w >= 4.4f ? 2 : 1;
            if (banCong)
            {
                var ray = Mat("LanCanSat", "T_LanCanSat.png", 0.16f, 1.0f, "#FFFFFF", 0.05f, 0.2f, true);
                b.Box(bang, 0.02f, w - 0.02f, y0 - 0.16f, y0, -1.1f, 0);
                b.Box(ray, 0.04f, w - 0.04f, y0, y0 + 1.0f, -1.1f, -1.07f);
                b.Box(ray, 0.02f, 0.05f, y0, y0 + 1.0f, -1.1f, 0);
                b.Box(ray, w - 0.05f, w - 0.02f, y0, y0 + 1.0f, -1.1f, 0);
                DoorGlass(b, r, w * 0.5f, y0, 1.0f, 2.2f);
                if (nb == 2) CuaSo(b, r, w * 0.2f, y0 + 0.9f, 1.0f, 1.4f, false, true);
            }
            else
            {
                for (int i = 0; i < nb; i++)
                {
                    float xc = w * (i + 0.5f) / nb;
                    bool cage = f >= 2 && r.NextDouble() < 0.32;
                    CuaSo(b, r, xc, y0 + 0.9f, nb == 1 ? 1.5f : 1.3f, 1.4f, cage, true);
                }
            }
        }
    }

    static void Bang(MB b, System.Random r, float w)   // bảng hiệu phai màu
    {
        string[] hex = { "#6E3B36", "#2F5560", "#7A6A34", "#4B4B58", "#8A7A62" };
        int i = r.Next(hex.Length);
        var m = Mat("Bang" + i, null, 1, 1, hex[i], 0.05f, 0.2f);
        b.Box(m, 0.25f, w - 0.25f, 3.15f, 3.55f, -0.34f, -0.24f);
    }

    static void Mai(MB b, System.Random r, float w)    // mái hiên tôn/bạt
    {
        string[] hex = { "#6E3B36", "#2F5560", "#7A6A34", "#5A5A50" };
        int i = r.Next(hex.Length);
        var m = Mat("Hien" + i, null, 1, 1, hex[i], 0.05f, 0.2f);
        b.BoxM(m, Matrix4x4.TRS(new Vector3(w / 2, 2.95f, -0.75f), Quaternion.Euler(14, 0, 0), Vector3.one), new Vector3(w - 0.2f, 0.05f, 1.55f));
    }

    static void DoorGlass(MB b, System.Random r, float xc, float y0, float ww, float hh)
    {
        var f = Mat("KhungSon", null, 1, 1, "#B9B4A6", 0.05f, 0.2f);
        b.Box(f, xc - ww / 2 - 0.07f, xc + ww / 2 + 0.07f, y0, y0 + hh + 0.07f, -0.06f, 0);
        b.Box(KinhMat(r), xc - ww / 2, xc + ww / 2, y0, y0 + hh, -0.075f, -0.06f);
    }

    static void CuaSo(MB b, System.Random r, float xc, float ys, float ww, float hh, bool cage, bool full)
    {
        var f = Mat("KhungSon", null, 1, 1, "#B9B4A6", 0.05f, 0.2f);
        var bang = Mat("BeTongSang", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#BDB9AE");
        b.Box(f, xc - ww / 2 - 0.07f, xc + ww / 2 + 0.07f, ys - 0.07f, ys + hh + 0.07f, -0.06f, 0);
        b.Box(KinhMat(r), xc - ww / 2, xc + ww / 2, ys, ys + hh, -0.075f, -0.06f);
        if (!full) return;
        b.Box(f, xc - 0.02f, xc + 0.02f, ys, ys + hh, -0.09f, -0.075f);
        b.Box(bang, xc - ww / 2 - 0.12f, xc + ww / 2 + 0.12f, ys - 0.1f, ys - 0.03f, -0.17f, 0);
        if (cage)   // chuồng cọp: lồng sắt nhô ra + nắp tôn
        {
            var ray = Mat("LanCanSat", "T_LanCanSat.png", 0.16f, 1.0f, "#FFFFFF", 0.05f, 0.2f, true);
            var ton = Mat("TonCu", "T_rusty_corrugated_iron.jpg", 1.0f, 1.0f, "#8A7A70", 0.1f, 0.25f);
            float x0 = xc - ww / 2 - 0.15f, x1 = xc + ww / 2 + 0.15f, y1 = ys + hh + 0.3f;
            b.Box(ray, x0, x1, ys - 0.1f, y1, -0.78f, -0.75f);
            b.Box(ray, x0, x0 + 0.03f, ys - 0.1f, y1, -0.78f, 0);
            b.Box(ray, x1 - 0.03f, x1, ys - 0.1f, y1, -0.78f, 0);
            b.Box(ton, x0 - 0.05f, x1 + 0.05f, y1, y1 + 0.04f, -0.9f, 0);
        }
    }

    static Material KinhMat(System.Random r)
    {
        double q = r.NextDouble();
        if (q < 0.10) return Mat("KinhSang", null, 1, 1, "#000000", 0.1f, 0.4f, false, new Color(0.85f, 0.50f, 0.22f));
        if (q < 0.14) return Mat("KinhTV", null, 1, 1, "#000000", 0.1f, 0.4f, false, new Color(0.18f, 0.30f, 0.42f));
        if (q < 0.24) return Mat("KinhRem", null, 1, 1, "#000000", 0.1f, 0.4f, false, new Color(0.20f, 0.07f, 0.05f));
        return Mat("KinhToi", null, 1, 1, "#0A0E13", 0.3f, 0.7f);
    }

    // ───── mặt sau: cửa sổ nhỏ, ống nước, lồng phơi đồ
    static void MatSau(MB b, System.Random r, float w, int floors, float H)
    {
        var be = Mat("BeTong", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#9A9890");
        var ong = Mat("Ong", null, 1, 1, "#6C6E6A", 0.1f, 0.3f);
        var ray = Mat("LanCanSat", "T_LanCanSat.png", 0.16f, 1.0f, "#FFFFFF", 0.05f, 0.2f, true);
        var cuon = Mat("CuaCuon", "T_painted_metal_shutter.jpg", 1.4f, 1.4f, "#9FA4A8", 0.2f, 0.35f);
        b.Box(cuon, w * 0.25f, w * 0.75f, 0, 2.3f, -0.1f, -0.02f);
        float px = r.NextDouble() < 0.5 ? 0.25f : w - 0.25f;
        b.AddMesh(ong, Cyl(), Matrix4x4.TRS(new Vector3(px, H / 2, -0.12f), Quaternion.identity, new Vector3(0.11f, H / 2, 0.11f)));
        for (int f = 1; f < floors; f++)
        {
            float y0 = 3.6f + 3.3f * (f - 1);
            int n = 1 + r.Next(2);
            for (int i = 0; i < n; i++)
            {
                float xc = w * (0.2f + 0.6f * (float)r.NextDouble());
                b.Box(KinhMat(r), xc - 0.35f, xc + 0.35f, y0 + 1.1f, y0 + 2.0f, -0.05f, 0);
                if (r.NextDouble() < 0.5) b.Box(ray, xc - 0.4f, xc + 0.4f, y0 + 1.05f, y0 + 2.05f, -0.09f, -0.06f);
            }
            if (r.NextDouble() < 0.28)   // lồng phơi đồ nhô ra
            {
                float xc = w * 0.5f;
                b.Box(be, xc - 0.9f, xc + 0.9f, y0 - 0.12f, y0, -0.9f, 0);
                b.Box(ray, xc - 0.9f, xc + 0.9f, y0, y0 + 1.0f, -0.9f, -0.87f);
                b.Box(ray, xc - 0.9f, xc - 0.87f, y0, y0 + 1.0f, -0.9f, 0);
                b.Box(ray, xc + 0.87f, xc + 0.9f, y0, y0 + 1.0f, -0.9f, 0);
            }
        }
    }

    // ───── nhà xa: chỉ ô kính
    static void MatXa(MB b, System.Random r, float w, int floors, float H)
    {
        var cuon = Mat("CuaCuon", "T_painted_metal_shutter.jpg", 1.4f, 1.4f, "#9FA4A8", 0.2f, 0.35f);
        b.Box(cuon, 0.4f, w - 0.4f, 0, 2.7f, -0.06f, 0);
        for (int f = 1; f < floors; f++)
        {
            float y0 = 3.6f + 3.3f * (f - 1);
            int nb = Mathf.Max(1, Mathf.RoundToInt(w / 2.6f));
            for (int i = 0; i < nb; i++)
            {
                float xc = w * (i + 0.5f) / nb;
                b.Box(KinhMat(r), xc - 0.7f, xc + 0.7f, y0 + 0.9f, y0 + 2.3f, -0.06f, 0);
            }
        }
    }

    // cửa sổ nhỏ ở vách hông (chỉ thấy phần nhà cao hơn nhà bên cạnh)
    static void CuaSoHong(MB b, System.Random r, float w, float d, int floors)
    {
        var kh = Mat("KhungSon", null, 1, 1, "#B9B4A6", 0.05f, 0.2f);
        for (int f = 2; f < floors; f++)
        {
            float y0 = 3.6f + 3.3f * (f - 1);
            if (r.NextDouble() < 0.6) { float z = d * (0.25f + 0.5f * (float)r.NextDouble()); b.Box(kh, -0.03f, 0, y0 + 0.93f, y0 + 2.07f, z - 0.47f, z + 0.47f); b.Box(KinhMat(r), -0.04f, -0.03f, y0 + 1.0f, y0 + 2.0f, z - 0.4f, z + 0.4f); }
            if (r.NextDouble() < 0.6) { float z = d * (0.25f + 0.5f * (float)r.NextDouble()); b.Box(kh, w, w + 0.03f, y0 + 0.93f, y0 + 2.07f, z - 0.47f, z + 0.47f); b.Box(KinhMat(r), w + 0.03f, w + 0.04f, y0 + 1.0f, y0 + 2.0f, z - 0.4f, z + 0.4f); }
        }
    }

    // ───── mái
    static void MaiNha(MB b, System.Random r, float w, float d, float H, Kieu k, Material tuong)
    {
        var be = Mat("BeTong", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#9A9890");
        var sat = Mat("Sat", null, 1, 1, "#22201E");
        double q = r.NextDouble();
        b.Box(be, 0, w, H, H + 0.05f, 0, d);
        if (k != Kieu.Mat) q = q < 0.3 ? 0.9 : 0.1;   // nhà sau/xa: chủ yếu mái bằng
        if (q < 0.55)   // mái bằng có lan can + bồn nước
        {
            b.Box(tuong, 0, w, H, H + 0.9f, 0, 0.15f);
            b.Box(tuong, 0, w, H, H + 0.9f, d - 0.15f, d);
            b.Box(tuong, 0, 0.15f, H, H + 0.9f, 0, d);
            b.Box(tuong, w - 0.15f, w, H, H + 0.9f, 0, d);
            float cx = w * (0.3f + 0.4f * (float)r.NextDouble()), cz = d * (0.15f + 0.4f * (float)r.NextDouble());
            if (r.NextDouble() < 0.7)   // bồn inox trên bệ sắt
            {
                var inox = Mat("Inox", "T_painted_metal_shutter.jpg", 1.5f, 1.5f, "#C4C8CC", 0.5f, 0.6f);
                b.Box(sat, cx - 0.45f, cx + 0.45f, H, H + 0.45f, cz - 0.45f, cz + 0.45f);
                b.AddMesh(inox, Cyl(), Matrix4x4.TRS(new Vector3(cx, H + 0.45f + 0.65f, cz), Quaternion.identity, new Vector3(1.15f, 0.65f, 1.15f)));
            }
            if (r.NextDouble() < 0.35)   // buồng cầu thang mái
            {
                float hx = w * 0.1f;
                b.Box(tuong, hx, hx + 1.8f, H, H + 2.4f, d * 0.55f, d * 0.55f + 2.0f);
                b.Box(be, hx - 0.1f, hx + 1.9f, H + 2.4f, H + 2.5f, d * 0.55f - 0.1f, d * 0.55f + 2.1f);
            }
            if (r.NextDouble() < 0.4)   // ăng-ten xương cá
            {
                float ax = w * (0.2f + 0.6f * (float)r.NextDouble()), az = d * (0.1f + 0.2f * (float)r.NextDouble());
                b.Box(sat, ax - 0.02f, ax + 0.02f, H, H + 4.0f, az - 0.02f, az + 0.02f);
                for (int i = 0; i < 4; i++) b.Box(sat, ax - 0.5f + i * 0.05f, ax + 0.5f - i * 0.05f, H + 2.6f + i * 0.35f, H + 2.63f + i * 0.35f, az - 0.01f, az + 0.01f);
            }
        }
        else if (q < 0.85)   // mái ngói dốc phía trước
        {
            var ngoi = Mat("Ngoi", "T_clay_roof_tiles_02.jpg", 1.2f, 1.2f, "#8C7468", 0.1f, 0.3f);
            b.Box(tuong, 0, w, H, H + 0.5f, d - 0.15f, d);
            b.BoxM(ngoi, Matrix4x4.TRS(new Vector3(w / 2, H + 0.85f, 1.6f), Quaternion.Euler(-23, 0, 0), Vector3.one), new Vector3(w + 0.5f, 0.14f, 4.7f));
            b.Box(tuong, 0, w, H, H + 1.55f, 3.4f, 3.6f);
            b.Box(be, 0, 0.12f, H, H + 0.9f, 0, 3.6f);
            b.Box(be, w - 0.12f, w, H, H + 0.9f, 0, 3.6f);
        }
        else   // mái tôn cũ
        {
            var ton = Mat("TonCu", "T_rusty_corrugated_iron.jpg", 1.0f, 1.0f, "#8A7A70", 0.1f, 0.25f);
            b.Box(tuong, 0, w, H, H + 0.9f, 0, 0.15f);
            b.BoxM(ton, Matrix4x4.TRS(new Vector3(w / 2, H + 1.3f, d / 2), Quaternion.Euler(6, 0, 0), Vector3.one), new Vector3(w + 0.4f, 0.05f, d + 0.6f));
            b.Box(sat, 0.05f, 0.1f, H, H + 1.3f, 0.05f, 0.1f);
            b.Box(sat, w - 0.1f, w - 0.05f, H, H + 1.3f, 0.05f, 0.1f);
        }
    }

    static Material WallMat(System.Random r)
    {
        string[] hex = { "#D8C8BE", "#BFA9A0", "#E3D5C8", "#D8CDB8", "#BDB39E", "#C8C8BE", "#A9ABA0", "#CFC9BB", "#B5AFA0", "#BCD3D0", "#9FB9B8", "#C9DAD2" };
        string[] tex = { "T_peeling_painted_wall.jpg", "T_peeling_painted_wall.jpg", "T_peeling_painted_wall.jpg", "T_preconcrete_wall_001.jpg", "T_preconcrete_wall_001.jpg", "T_worn_mossy_plasterwall.jpg", "T_worn_mossy_plasterwall.jpg", "T_worn_plaster_wall.jpg", "T_worn_plaster_wall.jpg", "T_blue_plaster_weathered.jpg", "T_blue_plaster_weathered.jpg", "T_blue_plaster_weathered.jpg" };
        if (r.NextDouble() < 0.3)
        {
            string[] flat = { "#E2D7A4", "#D9B8B0", "#B7CBB0", "#CFD6DA", "#C9B79A" };
            int j = r.Next(flat.Length);
            return Mat("TuongPhang" + j, "T_beige_wall_001.jpg", 2f, 2f, flat[j], 0.04f, 0.15f);
        }
        int i = r.Next(hex.Length);
        return Mat("Tuong" + i, tex[i], 2.5f, 2.5f, hex[i], 0.04f, 0.15f);
    }

    // ═════════════════════════ cột điện, dây, đèn đường
    static void CotDien()
    {
        var b = new MB(Matrix4x4.identity, new System.Random(3));
        var bt = Mat("BeTongCot", "T_preconcrete_wall_001.jpg", 2.5f, 2.5f, "#7C7A74");
        var sat = Mat("Sat", null, 1, 1, "#22201E");
        var day = Mat("Day", null, 1, 1, "#0C0C0C", 0f, 0f);
        var den = Mat("DenPho", null, 1, 1, "#000000", 0f, 0f, false, new Color(1.0f, 0.62f, 0.28f) * 1.4f);
        var bien = Mat("BienAp", null, 1, 1, "#6E7072", 0.1f, 0.3f);
        float[] xs = { -30f, -9f, 12f, 33f };
        const float z = -6.4f;
        foreach (var x in xs)
        {
            b.Box(bt, x - 0.14f, x + 0.14f, -0.15f, 9.2f, z - 0.14f, z + 0.14f);
            b.Box(sat, x - 0.05f, x + 0.05f, 8.5f, 8.6f, z - 0.9f, z + 0.9f);
            b.Box(sat, x - 0.05f, x + 0.05f, 7.45f, 7.51f, z - 1.6f, z);          // tay đèn hướng ra đường
            b.Box(den, x - 0.32f, x + 0.32f, 7.28f, 7.42f, z - 1.75f, z - 1.35f);
            b.Box(sat, x - 0.06f, x + 0.06f, 6.3f, 6.36f, z - 0.4f, z + 0.5f);     // xà cáp viễn thông
            if (x == xs[1] || x == xs[3]) b.AddMesh(bien, Cyl(), Matrix4x4.TRS(new Vector3(x + 0.35f, 7.9f, z), Quaternion.identity, new Vector3(0.5f, 0.4f, 0.5f)));
        }
        var rr = new System.Random(77);
        for (int i = 0; i + 1 < xs.Length; i++)
        {
            float a = xs[i], c = xs[i + 1];
            foreach (var (y, dz, sag) in new[] { (8.55f, -0.85f, 0.7f), (8.55f, 0.85f, 0.7f), (9.2f, 0f, 0.75f), (6.34f, 0.45f, 0.5f), (6.34f, -0.35f, 0.55f), (6.31f, 0.05f, 0.65f) })
                Day(b, day, new Vector3(a, y, z + dz), new Vector3(c, y, z + dz), sag);
        }
        // dây câu vào mặt tiền dãy đối diện và nhà bên này
        foreach (var x in xs)
            for (int i = 0; i < 3; i++)
            {
                float tx = x + (float)(rr.NextDouble() * 10 - 5), ty = 4.5f + (float)rr.NextDouble() * 6f;
                Day(b, day, new Vector3(x, 6.34f, z + 0.1f * i), new Vector3(tx, ty, -13.95f), 0.9f, 0.022f);
            }
        b.Commit(grp, "CotDien", new Bounds(new Vector3(2, 4, -6.4f), new Vector3(70, 9, 0.4f)));

        foreach (var x in new[] { xs[1], xs[2] })   // đèn phố chính
            DenPho(x, z - 1.55f, "#FFB060", 12f, 2.0f);
        foreach (var x in new[] { xs[0], xs[3] })   // đèn yếu ở hai cột còn lại
            DenPho(x, z - 1.55f, "#FFB868", 9f, 1.0f);
        // ba cột đèn riêng bên vỉa hè đối diện, đèn yếu rọi lên mặt tiền dãy đối diện
        var b2 = new MB(Matrix4x4.identity, new System.Random(4));
        foreach (var x in new[] { -34f, -22f, -8f, 4f, 15f, 26f, 38f })
        {
            const float zc = -13.2f;
            b2.Box(bt, x - 0.11f, x + 0.11f, -0.15f, 6.8f, zc - 0.11f, zc + 0.11f);
            b2.Box(sat, x - 0.04f, x + 0.04f, 6.5f, 6.56f, zc, zc + 1.3f);
            b2.Box(den, x - 0.28f, x + 0.28f, 6.36f, 6.5f, zc + 1.0f, zc + 1.35f);
            DenPho(x, zc + 1.2f, "#FFB868", 10f, 1.0f, 6.2f);
        }
        b2.Commit(grp, "CotDen_DoiDien", new Bounds(new Vector3(4, 3, -13.2f), new Vector3(60, 7, 0.4f)));
    }

    // ponytail: mỗi đèn là Light điểm không đổ bóng; đã thử 2 đèn mạnh + 5 đèn yếu, trong nhà không đổi (so pixel). Thêm nữa thì phải so lại.
    static void DenPho(float x, float z, string hex, float range, float intensity, float y = 7.0f)
    {
        var g = new GameObject("Den_Pho").transform; g.SetParent(grp, false);
        g.position = new Vector3(x, y, z);
        var l = g.gameObject.AddComponent<Light>();
        l.type = LightType.Point; l.color = Hex(hex); l.range = range; l.intensity = intensity; l.shadows = LightShadows.None;
    }

    static void Day(MB b, Material m, Vector3 a, Vector3 c, float sag, float th = 0.03f)
    {
        const int N = 14;
        Vector3 Pt(int i) { float t = i / (float)N; return Vector3.Lerp(a, c, t) + Vector3.down * sag * 4 * t * (1 - t); }
        for (int i = 0; i < N; i++)
        {
            Vector3 p = Pt(i), q = Pt(i + 1), dir = q - p;
            b.BoxM(m, Matrix4x4.TRS((p + q) / 2, Quaternion.LookRotation(dir), Vector3.one), new Vector3(th, th, dir.magnitude + 0.01f));
        }
    }

    // ═════════════════════════ vật liệu
    static Mesh Cyl()
    {
        if (cyl) return cyl;
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyl = g.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(g);
        return cyl;
    }

    static Color Hex(string h) => ColorUtility.TryParseHtmlString(h, out var c) ? c : Color.magenta;

    static void Save(Material m, string name)
    {
        System.IO.Directory.CreateDirectory(MatDir);
        var p = $"{MatDir}/{name}.mat";
        if (System.IO.File.Exists(p)) AssetDatabase.DeleteAsset(p);
        AssetDatabase.CreateAsset(m, p);
    }

    static Material Mat(string key, string tex, float tu, float tv, string tint = "#FFFFFF", float spec = 0.05f, float smooth = 0.15f, bool cut = false, Color? emis = null)
    {
        if (mats.TryGetValue(key, out var c) && c) return c;
        var m = new Material(Shader.Find("Universal Render Pipeline/Simple Lit")) { name = "KP_" + key };
        if (tex != null)
        {
            var p = $"{Dir}/Textures/{tex}";
            if (cut && AssetImporter.GetAtPath(p) is TextureImporter ti && !ti.alphaIsTransparency)
            {
                ti.alphaIsTransparency = true; ti.mipMapsPreserveCoverage = true; ti.alphaTestReferenceValue = 0.5f; ti.SaveAndReimport();
            }
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (t) { m.SetTexture("_BaseMap", t); m.mainTexture = t; } else Debug.LogWarning("[KhuPho] thiếu texture " + p);
        }
        var col = Hex(tint); m.SetColor("_BaseColor", col); m.color = col;
        m.SetColor("_SpecColor", new Color(spec, spec, spec, 1)); m.SetFloat("_Smoothness", smooth);
        if (cut)
        {
            m.SetFloat("_AlphaClip", 1); m.SetFloat("_Cutoff", 0.5f); m.EnableKeyword("_ALPHATEST_ON");
            m.SetOverrideTag("RenderType", "TransparentCutout"); m.renderQueue = 2450;
        }
        if (emis is Color e)
        {
            m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", e);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }
        Save(m, "KP_" + key);
        tile[m] = (tu, tv);
        return mats[key] = m;
    }

    // ═════════════════════════ gộp hộp thành một mesh
    class MB
    {
        class Sub { public List<Vector3> v = new(), n = new(); public List<Vector2> uv = new(); public List<int> t = new(); }
        readonly Dictionary<Material, Sub> subs = new();
        readonly Matrix4x4 xf;
        readonly Vector2 off;

        public MB(Matrix4x4 xf, System.Random r) { this.xf = xf; off = new Vector2(r.Next(100) * 0.37f, r.Next(100) * 0.53f); }

        Sub S(Material m) { if (!subs.TryGetValue(m, out var s)) subs[m] = s = new Sub(); return s; }

        public void Box(Material m, float x0, float x1, float y0, float y1, float z0, float z1, bool bottom = false)
            => BoxM(m, Matrix4x4.Translate(new Vector3((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2)), new Vector3(x1 - x0, y1 - y0, z1 - z0), bottom);

        public void BoxM(Material m, Matrix4x4 local, Vector3 size, bool bottom = false)
        {
            var s = S(m);
            var (tu, tv) = tile.TryGetValue(m, out var tl) ? tl : (1f, 1f);
            var M = xf * local;
            for (int ax = 0; ax < 3; ax++)
            for (int sg = -1; sg <= 1; sg += 2)
            {
                if (ax == 1 && sg < 0 && !bottom) continue;
                int a1 = ax == 0 ? 2 : 0, a2 = ax == 1 ? 2 : 1;
                var nl = Vector3.zero; nl[ax] = sg;
                var q = new Vector3[4]; var uv = new Vector2[4];
                for (int i = 0; i < 4; i++)
                {
                    var p = Vector3.zero; p[ax] = 0.5f * sg;
                    p[a1] = (i < 2 ? -0.5f : 0.5f); p[a2] = (i == 0 || i == 3 ? -0.5f : 0.5f);
                    var lp = local.MultiplyPoint3x4(Vector3.Scale(p, size));
                    q[i] = M.MultiplyPoint3x4(Vector3.Scale(p, size));
                    uv[i] = new Vector2(lp[a1] / tu, lp[a2] / tv) + off;
                }
                var nw = M.MultiplyVector(nl).normalized;
                int[] o = Vector3.Dot(Vector3.Cross(q[1] - q[0], q[2] - q[0]), nw) < 0 ? new[] { 0, 3, 2, 1 } : new[] { 0, 1, 2, 3 };
                int i0 = s.v.Count;
                foreach (var i in o) { s.v.Add(q[i]); s.n.Add(nw); s.uv.Add(uv[i]); }
                s.t.AddRange(new[] { i0, i0 + 1, i0 + 2, i0, i0 + 2, i0 + 3 });
            }
        }

        public void AddMesh(Material m, Mesh mesh, Matrix4x4 local)
        {
            var s = S(m); var M = xf * local;
            int i0 = s.v.Count;
            var vs = mesh.vertices; var ns = mesh.normals; var us = mesh.uv;
            for (int i = 0; i < vs.Length; i++) { s.v.Add(M.MultiplyPoint3x4(vs[i])); s.n.Add(M.MultiplyVector(ns[i]).normalized); s.uv.Add(us[i]); }
            foreach (var t in mesh.triangles) s.t.Add(i0 + t);
        }

        public void Commit(Transform parent, string name, Bounds col)
        {
            var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            var vs = new List<Vector3>(); var ns = new List<Vector3>(); var uvs = new List<Vector2>();
            var keys = new List<Material>(subs.Keys);
            foreach (var k in keys) { vs.AddRange(subs[k].v); ns.AddRange(subs[k].n); uvs.AddRange(subs[k].uv); }
            mesh.SetVertices(vs); mesh.SetNormals(ns); mesh.SetUVs(0, uvs);
            mesh.subMeshCount = keys.Count;
            int baseV = 0;
            for (int i = 0; i < keys.Count; i++)
            {
                var t = new List<int>(subs[keys[i]].t.Count);
                foreach (var x in subs[keys[i]].t) t.Add(x + baseV);
                mesh.SetTriangles(t, i);
                baseV += subs[keys[i]].v.Count;
            }
            mesh.RecalculateBounds();
            var g = new GameObject(name); g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            g.AddComponent<MeshRenderer>().sharedMaterials = keys.ToArray();
            var bc = g.AddComponent<BoxCollider>(); bc.center = col.center; bc.size = col.size;
            GameObjectUtility.SetStaticEditorFlags(g, StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        }
    }
}
