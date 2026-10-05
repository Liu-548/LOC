// [1/10] Placeholder CHI TIẾT cho 7 món đồ tang mới (LOC_DoTang) — dựng bằng khối primitive khi chưa có file .glb.
// Có .glb trùng tên trong dự án thì AP() dùng .glb, bộ này tự bị bỏ qua. Gốc toạ độ cục bộ: tâm XZ, đáy y = 0, mặt trước = +Z.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static partial class LocHouseBuilder
{
    static GameObject AP(string name, float x, float z, float y, float yaw, bool solid, System.Action<Transform> build)
    {
        if (FindModel(name)) return A(name, x, z, y, yaw);
        placeholders.Add(name);
        var g = new GameObject("PH_" + name);
        g.transform.SetParent(cur, false);
        build(g.transform);
        if (solid)
        {
            var rs = g.GetComponentsInChildren<Renderer>();
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            var bc = g.AddComponent<BoxCollider>(); bc.center = g.transform.InverseTransformPoint(b.center); bc.size = b.size;
        }
        g.transform.rotation = Quaternion.Euler(0, yaw, 0);
        Align(g, x, y, z);
        return Mark(g, LocProp.Kieu.San);
    }

    // ── khối cơ bản (y = đáy khối)
    static GameObject TBx(Transform p, string n, float x, float y, float z, float w, float h, float d, Material m, float ry = 0)
    {
        var g = Cb(p, n, new Vector3(x, y + h / 2, z), new Vector3(w, h, d), m);
        if (ry != 0) g.transform.localRotation = Quaternion.Euler(0, ry, 0);
        return g;
    }
    static GameObject TCy(Transform p, string n, float x, float y, float z, float d, float h, Material m)
        => Cb(p, n, new Vector3(x, y + h / 2, z), new Vector3(d, h / 2, d), m, PrimitiveType.Cylinder);   // Cylinder Unity cao 2 → scale.y = h/2
    static GameObject TEl(Transform p, string n, float x, float yc, float z, float w, float h, float d, Material m)
        => Cb(p, n, new Vector3(x, yc, z), new Vector3(w, h, d), m, PrimitiveType.Sphere);

    // vật liệu phát sáng (lửa, đầu nhang) và vật liệu trong mờ (kính đèn)
    static Material Phat(string name, string hex, float k)
    {
        var m = M(name, hex);
        m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", Hex(hex) * k);
        EditorUtility.SetDirty(m); return m;
    }
    static Material KinhMo(string name, string hex, float alpha)
    {
        var m = M(name, hex);
        var c = Hex(hex); c.a = alpha;
        m.SetColor("_BaseColor", c); m.color = c;
        m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0); m.SetFloat("_SrcBlend", 5); m.SetFloat("_DstBlend", 10); m.SetFloat("_ZWrite", 0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.SetOverrideTag("RenderType", "Transparent"); m.renderQueue = 3000;
        EditorUtility.SetDirty(m); return m;
    }

    // ── chữ pixel 5×7 (đúng phong cách lowpoly/Point): chỉ gồm các chữ cần cho nhãn đồ tang
    static readonly Dictionary<char, string[]> Pix = new()
    {
        ['L'] = new[] { "X....", "X....", "X....", "X....", "X....", "X....", "XXXXX" },
        ['I'] = new[] { "XXXXX", "..X..", "..X..", "..X..", "..X..", "..X..", "XXXXX" },
        ['N'] = new[] { "X...X", "XX..X", "X.X.X", "X.X.X", "X..XX", "X...X", "X...X" },
        ['H'] = new[] { "X...X", "X...X", "X...X", "XXXXX", "X...X", "X...X", "X...X" },
        ['V'] = new[] { "X...X", "X...X", "X...X", "X...X", "X...X", ".X.X.", "..X.." },
        ['P'] = new[] { "XXXX.", "X...X", "X...X", "XXXX.", "X....", "X....", "X...." },
        ['U'] = new[] { "X...X", "X...X", "X...X", "X...X", "X...X", "X...X", ".XXX." },
        ['G'] = new[] { ".XXX.", "X...X", "X....", "X.XXX", "X...X", "X...X", ".XXXX" },
        ['D'] = new[] { "XXXX.", "X...X", "X...X", "X...X", "X...X", "X...X", "XXXX." },
        ['E'] = new[] { "XXXXX", "X....", "X....", "XXXX.", "X....", "X....", "XXXXX" },
        ['Đ'] = new[] { "XXX..", "X..X.", "X...X", "XXX.X", "X...X", "X..X.", "XXX.." },
    };

    static void Put(Color32[] px, int cw, int ch, int x, int y, Color32 c)
    {
        if (x >= 0 && x < cw && y >= 0 && y < ch) px[(ch - 1 - y) * cw + x] = c;   // y tính từ trên xuống
    }

    static void VeChu(Color32[] px, int cw, int ch, string line, int y0, Color32 fg)
    {
        int w = -1; foreach (var c in line) w += c == ' ' ? 3 : 6;
        int x = (cw - w) / 2;
        foreach (var c in line)
        {
            if (c == ' ') { x += 3; continue; }
            char b = c; bool acute = false, circ = false, dot = false;
            if (c == 'Ú') { b = 'U'; acute = true; }
            else if (c == 'Ế') { b = 'E'; acute = true; circ = true; }
            else if (c == 'Ị') { b = 'I'; dot = true; }
            var rows = Pix[b];
            for (int r = 0; r < 7; r++) for (int k = 0; k < 5; k++) if (rows[r][k] == 'X') Put(px, cw, ch, x + k, y0 + r, fg);
            if (acute && !circ) { Put(px, cw, ch, x + 3, y0 - 2, fg); Put(px, cw, ch, x + 2, y0 - 1, fg); }
            if (circ) { Put(px, cw, ch, x + 1, y0 - 1, fg); Put(px, cw, ch, x + 2, y0 - 2, fg); Put(px, cw, ch, x + 3, y0 - 1, fg); Put(px, cw, ch, x + 4, y0 - 3, fg); Put(px, cw, ch, x + 3, y0 - 3, fg); }
            if (dot) Put(px, cw, ch, x + 2, y0 + 8, fg);
            x += 6;
        }
    }

    // tấm giấy/nhãn có chữ: tạo PNG (một lần) trong Textures, lọc Point, rồi dán lên khối mỏng
    static void TNhan(Transform p, string name, Vector3 pos, float w, float h, int cw, int ch, string[] lines, int y0, int dy, string fg, string bg, bool vien)
    {
        var file = name + ".png";
        var rel = $"{TexFolder}/{file}";
        var full = System.IO.Path.GetFullPath(rel);
        if (!System.IO.File.Exists(full))
        {
            var px = new Color32[cw * ch];
            Color32 b = Hex(bg), f = Hex(fg);
            for (int i = 0; i < px.Length; i++) px[i] = b;
            if (vien) for (int x = 1; x < cw - 1; x++) for (int y = 1; y < ch - 1; y++) if (x == 1 || y == 1 || x == cw - 2 || y == ch - 2) Put(px, cw, ch, x, y, f);
            for (int i = 0; i < lines.Length; i++) VeChu(px, cw, ch, lines[i], y0 + i * dy, f);
            var t = new Texture2D(cw, ch, TextureFormat.RGBA32, false);
            t.SetPixels32(px);
            System.IO.File.WriteAllBytes(full, t.EncodeToPNG());
            Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(rel);
            var imp = (TextureImporter)AssetImporter.GetAtPath(rel);
            imp.filterMode = FilterMode.Point; imp.mipmapEnabled = false; imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }
        Cb(p, "Nhan_" + name, pos, new Vector3(w, h, 0.002f), MT("M_" + name, file, 1, 1));
    }

    // ───────── PK-70 thùng phúng điếu 0,41 × 0,31 × 0,88: thùng giấy trắng băng đen, khe bỏ phong bì, đặt trên bàn gỗ hai tầng
    static void XayThung(Transform p)
    {
        var go = MT("M_Tang_Go", "D_go.png", 1, 1, "#6A4A2E"); var goToi = MT("M_Tang_GoToi", "D_go.png", 1, 1, "#3E2A19");
        var giay = M("M_Tang_Giay", "#E9E5D6"); var nap = M("M_Tang_GiayCu", "#D3CEBD"); var den = M("M_Tang_Den", "#1A1A1A");
        foreach (var sx in new[] { -1f, 1f }) foreach (var sz in new[] { -1f, 1f }) TBx(p, "Chan", sx * 0.185f, 0, sz * 0.135f, 0.03f, 0.60f, 0.03f, goToi);
        TBx(p, "Gac_Duoi", 0, 0.20f, 0, 0.37f, 0.015f, 0.27f, go);
        TBx(p, "Mat_Ban", 0, 0.60f, 0, 0.41f, 0.02f, 0.31f, go);
        TBx(p, "Thung", 0, 0.62f, 0, 0.37f, 0.24f, 0.27f, giay);
        TBx(p, "Bang_Den", 0, 0.665f, 0, 0.376f, 0.03f, 0.276f, den);
        TBx(p, "Nap", 0, 0.86f, 0, 0.41f, 0.02f, 0.31f, nap);
        TBx(p, "Khe", 0, 0.879f, 0.05f, 0.14f, 0.003f, 0.014f, den);
        TNhan(p, "Chu_PhungDieu", new Vector3(0, 0.78f, 0.1395f), 0.28f, 0.07f, 64, 16, new[] { "PHÚNG ĐIẾU" }, 6, 0, "#1A1A1A", "#EFEBDD", false);
    }

    // ───────── PK-71 bàn con 0,42 × 0,30 × 0,63: khay sơn đỏ sẫm, bó nhang buộc dây đỏ, xấp phong bì
    static void XayKhay(Transform p)
    {
        var go = MT("M_Tang_Go", "D_go.png", 1, 1, "#6A4A2E"); var goToi = MT("M_Tang_GoToi", "D_go.png", 1, 1, "#3E2A19");
        var son = M("M_Tang_SonKhay", "#5A1A12"); var nhang = M("M_Tang_Nhang", "#A63A25"); var doM = M("M_Tang_Do", "#8E1B1B");
        var giay = M("M_Tang_Giay", "#E9E5D6"); var mep = M("M_Tang_GiayCu", "#D3CEBD");
        foreach (var sx in new[] { -1f, 1f }) foreach (var sz in new[] { -1f, 1f }) TBx(p, "Chan", sx * 0.18f, 0, sz * 0.12f, 0.03f, 0.58f, 0.03f, goToi);
        TBx(p, "Mat", 0, 0.58f, 0, 0.42f, 0.025f, 0.30f, go);
        TBx(p, "Khay_Day", 0, 0.605f, 0, 0.38f, 0.008f, 0.26f, son);
        foreach (var s in new[] { -1f, 1f })
        {
            TBx(p, "Khay_Got", 0, 0.613f, s * 0.13f, 0.38f, 0.016f, 0.008f, son);
            TBx(p, "Khay_Got", s * 0.19f, 0.613f, 0, 0.008f, 0.016f, 0.26f, son);
        }
        // bó nhang nằm dọc trục X: 8 cây quanh 1 cây giữa, buộc một vòng dây đỏ
        float bx = -0.08f, bz = -0.03f, cy = 0.613f + 0.015f;
        for (int i = 0; i < 9; i++)
        {
            float a = i * 45f * Mathf.Deg2Rad, r = i == 8 ? 0f : 0.011f;
            var n = Cb(p, "Nhang", new Vector3(bx, cy + r * Mathf.Sin(a), bz + r * Mathf.Cos(a)), new Vector3(0.007f, 0.11f, 0.007f), nhang, PrimitiveType.Cylinder);
            n.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        var day = Cb(p, "Day_Buoc", new Vector3(bx, cy, bz), new Vector3(0.034f, 0.01f, 0.034f), doM, PrimitiveType.Cylinder);
        day.transform.localRotation = Quaternion.Euler(0, 0, 90);
        // xấp phong bì trắng so le, phong bì trên cùng có nắp dán
        float[] ry = { 0, 9, -6, 4 };
        for (int i = 0; i < ry.Length; i++) TBx(p, "PhongBi", 0.08f, 0.613f + i * 0.003f, 0.0f, 0.11f, 0.003f, 0.07f, i % 2 == 0 ? giay : mep, ry[i]);
        TBx(p, "PhongBi_Nap", 0.08f, 0.613f + ry.Length * 0.003f, 0.012f, 0.1f, 0.0008f, 0.03f, mep, ry[ry.Length - 1]);
    }

    // ───────── PK-72 bát hương 0,16 × 0,15 × 0,31: bát sứ vành xanh, tro, ba cây nhang cháy dở
    static void XayBatHuong(Transform p)
    {
        var su = M("M_Tang_Su", "#D9D3C2"); var xanh = M("M_Tang_SuXanh", "#2F4F7F"); var tro = M("M_Tang_Tro", "#7C7870");
        var nhang = M("M_Tang_Nhang", "#A63A25"); var than = Phat("M_Tang_DauNhang", "#FF5A1A", 2f);
        TCy(p, "Chan", 0, 0, 0, 0.08f, 0.014f, su);
        TCy(p, "Than_Duoi", 0, 0.014f, 0, 0.115f, 0.04f, su);
        TCy(p, "Vien_Xanh", 0, 0.045f, 0, 0.118f, 0.006f, xanh);
        TCy(p, "Than_Tren", 0, 0.054f, 0, 0.15f, 0.04f, su);
        TCy(p, "Tro", 0, 0.094f, 0, 0.128f, 0.006f, tro);
        float[,] st = { { -0.02f, 0.01f, 6f, -4f }, { 0.015f, -0.015f, -3f, 5f }, { 0f, 0.025f, -5f, -2f } };
        for (int i = 0; i < 3; i++)
        {
            var n = Cb(p, "Nhang", new Vector3(st[i, 0], 0.205f, st[i, 1]), new Vector3(0.004f, 0.105f, 0.004f), nhang, PrimitiveType.Cylinder);
            n.transform.localRotation = Quaternion.Euler(st[i, 2], 0, st[i, 3]);
            Cb(p, "Dau_Chay", n.transform.localPosition + n.transform.localRotation * new Vector3(0, 0.105f, 0), Vector3.one * 0.007f, than, PrimitiveType.Sphere);
        }
        foreach (var (x, z) in new[] { (-0.04f, -0.02f), (0.045f, 0.02f) })   // chân nhang cũ đã cháy hết
            TCy(p, "Chan_Nhang", x, 0.1f, z, 0.004f, 0.035f, tro);
    }

    // ───────── PK-73 bài vị 0,17 × 0,10 × 0,34: tấm gỗ đứng trên đế, giấy trắng viền đen, chữ "LINH VỊ" quốc ngữ
    static void XayBaiVi(Transform p)
    {
        var go = MT("M_Tang_Go", "D_go.png", 1, 1, "#6A4A2E"); var goToi = MT("M_Tang_GoToi", "D_go.png", 1, 1, "#3E2A19");
        TBx(p, "De", 0, 0, 0, 0.17f, 0.03f, 0.10f, goToi);
        foreach (var s in new[] { -1f, 1f }) TBx(p, "Chan_Sau", s * 0.05f, 0.03f, -0.03f, 0.015f, 0.07f, 0.03f, goToi);
        TBx(p, "Tam_Go", 0, 0.03f, 0, 0.15f, 0.31f, 0.014f, go);
        TNhan(p, "Chu_BaiVi", new Vector3(0, 0.185f, 0.0081f), 0.12f, 0.27f, 32, 72, new[] { "LINH", "VỊ" }, 20, 16, "#1A1A1A", "#EFEBDD", true);
    }

    // ───────── PK-74 bát cơm quả trứng đôi đũa 0,13 × 0,13 × 0,25: cơm đầy vun, trứng luộc, đũa cắm thẳng
    static void XayBatCom(Transform p)
    {
        var su = M("M_Tang_Su", "#D9D3C2"); var xanh = M("M_Tang_SuXanh", "#2F4F7F");
        var com = M("M_Tang_Com", "#F1EEE4"); var trung = M("M_Tang_Trung", "#F4EFD8"); var dua = M("M_Tang_Dua", "#8B6A3E");
        TCy(p, "Chan", 0, 0, 0, 0.05f, 0.012f, su);
        TCy(p, "Bat", 0, 0.012f, 0, 0.12f, 0.055f, su);
        TCy(p, "Vien_Xanh", 0, 0.04f, 0, 0.122f, 0.005f, xanh);
        TEl(p, "Com", 0, 0.067f, 0, 0.115f, 0.07f, 0.115f, com);
        TEl(p, "Trung", 0, 0.118f, 0.015f, 0.04f, 0.052f, 0.04f, trung);
        foreach (var (x, t) in new[] { (-0.009f, 2f), (0.009f, -2f) })
        {
            var d = Cb(p, "Dua", new Vector3(x, 0.16f, -0.025f), new Vector3(0.005f, 0.18f, 0.005f), dua);
            d.transform.localRotation = Quaternion.Euler(3, 0, t);
        }
    }

    // ───────── PK-75 đèn dầu 0,12 × 0,12 × 0,27: đế + bình dầu đồng, bấc, ống khói thuỷ tinh trong mờ, ngọn lửa
    static void XayDenDau(Transform p)
    {
        var dong = M("M_Tang_Dong", "#9A6A2E"); var kinh = KinhMo("M_Tang_KinhDen", "#E8EEE8", 0.3f); var lua = Phat("M_Tang_Lua", "#FFB040", 2.5f);
        TCy(p, "De", 0, 0, 0, 0.12f, 0.012f, dong);
        TCy(p, "Binh_Dau", 0, 0.012f, 0, 0.105f, 0.05f, dong);
        TCy(p, "Co", 0, 0.062f, 0, 0.05f, 0.016f, dong);
        TCy(p, "Bac", 0, 0.078f, 0, 0.014f, 0.016f, dong);
        TCy(p, "Kinh_Duoi", 0, 0.078f, 0, 0.07f, 0.04f, kinh);
        TCy(p, "Kinh_Tren", 0, 0.118f, 0, 0.048f, 0.11f, kinh);
        TEl(p, "Lua", 0, 0.125f, 0, 0.02f, 0.05f, 0.02f, lua);
        TCy(p, "Mu", 0, 0.228f, 0, 0.07f, 0.014f, dong);
        TCy(p, "Nut", 0, 0.242f, 0, 0.02f, 0.028f, dong);
    }

    // ───────── PK-76 chồng khăn tang 0,29 × 0,14 × 0,05: năm lớp khăn trắng gấp, mép gấp sẫm hơn, một đuôi khăn thõng
    static void XayKhanTang(Transform p)
    {
        var vai = M("M_Tang_VaiTrang", "#EEEAE0"); var vai2 = M("M_Tang_VaiBong", "#DAD6CA"); var gap = M("M_Tang_VaiGap", "#CFCABB");
        float[] ry = { 0, 3, -2, 4, -3 }, ox = { 0, 0.004f, -0.003f, 0.002f, -0.002f };
        for (int i = 0; i < 5; i++)
        {
            var l = TBx(p, "Khan", ox[i], i * 0.0095f, 0, 0.28f, 0.0095f, 0.13f, i % 2 == 0 ? vai : vai2, ry[i]);
            var f = TBx(l.transform, "Mep_Gap", 0, 0, 0, 1, 1, 1, gap);   // mép gấp: tấm mỏng theo cạnh trước của lớp (toạ độ cục bộ của lớp)
            f.transform.localPosition = new Vector3(0, 0, 0.47f); f.transform.localScale = new Vector3(1, 1.04f, 0.06f);
        }
        TBx(p, "Duoi_Khan", -0.15f, 0.0455f, 0.02f, 0.06f, 0.004f, 0.07f, vai, 12);
    }
}
