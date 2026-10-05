using UnityEditor;
using UnityEngine;

// [3/10] Kịch bản v3.0 (claude/kich-ban-loc-v3.md, mục 15):
//  · két sắt ĐỨNG ở góc trái phía cửa sổ phòng bố mẹ (thay két nhỏ dưới gầm tủ) — ngăn trên giấy tờ nhà đất + chìa hầm, ngăn dưới cuốn sổ của bố đè lên giấy gói năm 1996
//  · hũ cốt sành dưới gầm bàn thờ hầm, miệng bịt giấy dầu + lạt + tờ niêm son
//  · quyển sách gia lễ trong lòng tủ thờ tầng 3 (gọi từ SapBanThoOngBa)
//  · sổ thu chi 1997–2002 trong tủ hồ sơ phòng làm việc (LocDoTrongTu.DienHoSo, ngăn 4)
public static partial class LocHouseBuilder
{
    const string SON = "#B0302A";   // một mã màu son duy nhất cho chữ của thầy (giấy gói, niêm hũ) — kịch bản mục 11

    // nút gom đồ nhỏ: tên n, bên trong có nút DoTu_TrongTu để LocChiTiet bỏ qua (giữ màu phẳng)
    static Transform NutV3(string n, LocProp.Kieu k = LocProp.Kieu.CoDinh)
    {
        var p = new GameObject(n); p.transform.SetParent(cur, false); Mark(p, k);
        var d = new GameObject(LocDoTrongTu.Nut).transform; d.SetParent(p.transform, false); return d;
    }
    static GameObject Pv(Transform par, PrimitiveType t, string n, Vector3 pos, Vector3 size, string hex, float yaw = 0, float roll = 0)
    {
        var g = GameObject.CreatePrimitive(t); Object.DestroyImmediate(g.GetComponent<Collider>());
        g.name = n; g.transform.SetParent(par, false);
        g.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, roll));
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = M("M_V3_" + hex.TrimStart('#'), hex);
        return g;
    }

    // ───── két sắt đứng: góc trái phía cửa sổ, lưng áp tường cửa sổ (z = 0), cạnh trái sát tường trái, cửa quay vào phòng (+Z)
    //       bản lề phía tường trái → cánh mở nằm dọc tường, không chắn lối; mép tự do + tay nắm phía bàn viết
    static void KetSatDung(float y)
    {
        const float x0 = 0.05f, x1 = 0.55f, z0 = 0.02f, z1 = 0.47f, H = 0.75f;
        var sat = M("M_V3_KetSat", "#4F5B57"); var trong = M("M_V3_KetSatTrong", "#2C3230");
        var g = new GameObject("KetSat_Dung"); g.transform.SetParent(cur, false);
        var old = cur; cur = g.transform;
        Box("KetSat_De", x0 + 0.02f, x1 - 0.02f, y, y + 0.04f, z0 + 0.02f, z1 - 0.02f, M("M_V3_DeDen", "#232523"));
        Box("KetSat_Day", x0, x1, y + 0.04f, y + 0.08f, z0, z1, sat);
        Box("KetSat_Noc", x0, x1, y + H - 0.04f, y + H, z0, z1, sat);
        Box("KetSat_Lung", x0, x1, y + 0.08f, y + H - 0.04f, z0, z0 + 0.04f, trong);
        Box("KetSat_HongA", x0, x0 + 0.04f, y + 0.08f, y + H - 0.04f, z0, z1, sat);
        Box("KetSat_HongB", x1 - 0.04f, x1, y + 0.08f, y + H - 0.04f, z0, z1, sat);
        Box("KetSat_Ke", x0 + 0.04f, x1 - 0.04f, y + 0.40f, y + 0.42f, z0 + 0.04f, z1 - 0.01f, trong, false);
        // cánh: đóng nằm trong mặt z = 0,49 (dày 0,04), từ bản lề x0 chạy về +X → yaw 0; mở → yaw −90 (cánh nằm dọc tường trái)
        float w = x1 - x0, hc = H - 0.12f;
        var canh = Leaf("KetSat_Canh", x0, z1 + 0.02f, y + 0.08f, w, hc, 0, sat);
        Transform C(string n, PrimitiveType t, Vector3 lp, Vector3 s, string hex)
        {
            var c = GameObject.CreatePrimitive(t); Object.DestroyImmediate(c.GetComponent<Collider>());
            c.name = n; c.transform.SetParent(canh, false); c.transform.localPosition = lp; c.transform.localScale = s;
            c.GetComponent<Renderer>().sharedMaterial = M("M_V3_" + hex.TrimStart('#'), hex); return c.transform;
        }
        C("BanPhim", PrimitiveType.Cube, new Vector3(w * 0.62f, hc * 0.68f, 0.026f), new Vector3(0.085f, 0.115f, 0.012f), "#1E1F1E");
        for (int r = 0; r < 4; r++) for (int k = 0; k < 3; k++)
            C($"Phim_{r}{k}", PrimitiveType.Cube, new Vector3(w * 0.62f - 0.024f + k * 0.024f, hc * 0.68f + 0.036f - r * 0.024f, 0.034f), new Vector3(0.017f, 0.017f, 0.006f), "#BFC3C4");
        C("TayNam", PrimitiveType.Cube, new Vector3(w - 0.07f, hc * 0.45f, 0.04f), new Vector3(0.03f, 0.14f, 0.03f), "#8C8F8A");
        C("BanLe_Tren", PrimitiveType.Cylinder, new Vector3(0f, hc * 0.85f, 0.02f), new Vector3(0.025f, 0.04f, 0.025f), "#3A3E3C");
        C("BanLe_Duoi", PrimitiveType.Cylinder, new Vector3(0f, hc * 0.15f, 0.02f), new Vector3(0.025f, 0.04f, 0.025f), "#3A3E3C");
        Mo(canh, -90f, false, "cửa két sắt");

        var d = new GameObject(LocDoTrongTu.Nut).transform; d.SetParent(g.transform, false);
        float yt = y + 0.42f, yd = y + 0.08f;
        // ngăn trên: xấp giấy tờ nhà đất kẹp bìa xanh + chùm chìa khoá hầm + khoá treo to
        Pv(d, PrimitiveType.Cube, "GiayToNhaDat", new Vector3(0.25f, yt + 0.025f, 0.24f), new Vector3(0.30f, 0.05f, 0.21f), "#E8E0C8", 3);
        Pv(d, PrimitiveType.Cube, "BiaXanh", new Vector3(0.25f, yt + 0.053f, 0.24f), new Vector3(0.305f, 0.006f, 0.215f), "#3E5A7A", 3);
        Pv(d, PrimitiveType.Cylinder, "ChumChia_Vong", new Vector3(0.45f, yt + 0.003f, 0.38f), new Vector3(0.05f, 0.003f, 0.05f), "#A8A08A");
        for (int i = 0; i < 3; i++)
            Pv(d, PrimitiveType.Cube, "ChiaKhoa_" + i, new Vector3(0.45f, yt + 0.004f + i * 0.003f, 0.38f + 0.03f), new Vector3(0.016f, 0.003f, 0.07f), i == 0 ? "#B8964A" : "#A9ABA6", -25 + i * 22);
        Pv(d, PrimitiveType.Cube, "KhoaTreo_Than", new Vector3(0.45f, yt + 0.035f, 0.15f), new Vector3(0.06f, 0.07f, 0.035f), "#9C7A3A");
        // ngăn dưới: giấy gói năm 1996 gấp vuông, còn buộc lạt, mép trước lộ chữ son — cuốn sổ của bố đè lên, lệch vào trong
        Pv(d, PrimitiveType.Cube, "GiayGoi_1996", new Vector3(0.30f, yd + 0.0175f, 0.28f), new Vector3(0.22f, 0.035f, 0.17f), "#8A6A45", -4);
        Pv(d, PrimitiveType.Cube, "GiayGoi_Lat_A", new Vector3(0.30f, yd + 0.037f, 0.28f), new Vector3(0.226f, 0.004f, 0.012f), "#C9B27A", -4);
        Pv(d, PrimitiveType.Cube, "GiayGoi_Lat_B", new Vector3(0.30f, yd + 0.037f, 0.28f), new Vector3(0.012f, 0.004f, 0.175f), "#C9B27A", -4);
        Pv(d, PrimitiveType.Cube, "GiayGoi_ChuSon", new Vector3(0.29f, yd + 0.0355f, 0.355f), new Vector3(0.10f, 0.002f, 0.012f), SON, -4);
        var so = A("NhatKy_Dong", 0.28f, 0.22f, yd + 0.039f, 0, k: LocProp.Kieu.CoDinh);   // cuốn sổ của bố
        if (so) so.name = "SoCuaBo_TrongKet";
        cur = old;
        Mark(g, LocProp.Kieu.CoDinh);
    }

    // ───── hũ cốt dưới gầm bàn thờ hầm: khuất một nửa dưới mép trước bàn thờ
    static void HuCot(GameObject banTho, float y)
    {
        float x = -2.25f, z = 11.35f;
        if (banTho)
        {
            var rs = banTho.GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            x = b.center.x - b.extents.x * 0.45f; z = b.max.z - 0.05f;
        }
        // [4/10] model hũ sành chi tiết (LOC_Nguon/HuCot/build_hu.py): thân men da lươn, giấy dầu, 2 vòng lạt, tờ niêm son, trong có lọn tóc buộc chỉ đỏ + mảnh xương
        var d = NutV3("HuCot");
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LOC_House/Models/HuCot/HuCot.fbx");
        if (!fbx) { Debug.LogWarning("THIẾU Models/HuCot/HuCot.fbx"); return; }
        var m = Object.Instantiate(fbx, d); m.name = "HuCot_Model";
        m.transform.position = new Vector3(x, y, z);
        Transform Tim(string n) { foreach (var t in m.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t; return null; }
        var goc = Tim("Hu_GiayDau_Goc");
        if (goc) { var h = goc.position - m.transform.position; h.y = 0; m.transform.rotation = Quaternion.FromToRotation(h.normalized, Vector3.forward) * m.transform.rotation; }   // góc lật quay ra ngoài gầm (+Z)
        Material Tex(string ten, string tex, float bong, bool haiMat = false)
        {
            var path = $"{MatFolder}/M_HuCot_{ten}.mat"; var mt = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mt) { mt = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mt, path); }
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexFolder}/HuCot/{tex}");
            mt.SetTexture("_BaseMap", t); mt.mainTexture = t; mt.SetColor("_BaseColor", Color.white); mt.SetFloat("_Smoothness", bong);
            if (haiMat) mt.SetFloat("_Cull", 0);
            EditorUtility.SetDirty(mt); return mt;
        }
        Material Lit(string ten, string hex, float bong)   // màu trơn nhưng có độ bóng (tóc ánh lên dưới đèn)
        {
            var path = $"{MatFolder}/M_HuCot_{ten}.mat"; var mt = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mt || mt.shader.name != "Universal Render Pipeline/Lit") { if (mt) AssetDatabase.DeleteAsset(path); mt = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mt, path); }
            mt.SetColor("_BaseColor", Hex(hex)); mt.SetFloat("_Smoothness", bong); EditorUtility.SetDirty(mt); return mt;
        }
        var bang = new System.Collections.Generic.Dictionary<string, Material>
        {
            ["Hu_Men"] = Tex("Men", "Hu_Men.png", 0.38f), ["Hu_DatNung"] = Tex("DatNung", "Hu_DatNung.png", 0.12f),
            ["Hu_Long"] = M("M_HuCot_Long", "#120C09"), ["Hu_GiayDau"] = Tex("GiayDau", "Hu_GiayDau.png", 0.35f, true),
            ["Hu_Niem"] = Tex("Niem", "Hu_Niem.png", 0.05f, true), ["Hu_Lat"] = Tex("Lat", "Hu_Lat.png", 0.2f),
            ["Hu_Toc"] = Lit("Toc", "#2B2522", 0.6f), ["Hu_ChiDo"] = Lit("ChiDo", "#C42A1E", 0.3f), ["Hu_Cot"] = Lit("Xuong", "#9A8D72", 0.15f),
        };
        foreach (var r in m.GetComponentsInChildren<Renderer>(true))
        {
            var ms = r.sharedMaterials;
            for (int i = 0; i < ms.Length; i++) if (ms[i]) foreach (var kv in bang) if (ms[i].name.StartsWith(kv.Key)) { ms[i] = kv.Value; break; }
            r.sharedMaterials = ms;
        }
        var vc = new GameObject("HuCot_VaCham"); vc.transform.SetParent(m.transform, false);
        vc.transform.SetPositionAndRotation(m.transform.position, Quaternion.identity);
        var col = vc.AddComponent<BoxCollider>();   // tương tác E (LocHuCot) — theo hũ khi bị kéo ra
        col.center = new Vector3(0, 0.155f, 0); col.size = new Vector3(0.24f, 0.31f, 0.24f);
        var hc = d.parent.gameObject.AddComponent<LocHuCot>();
        hc.than = m.transform; hc.goc = goc;
        hc.keo = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LOC_House/Audio/Hu_KeoHu.wav");
        hc.xe = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LOC_House/Audio/Hu_XeGiay.wav");
        hc.sot = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LOC_House/Audio/Hu_SotSoat.wav");
    }

    // ───── quyển sách gia lễ của mẹ trong lòng tủ thờ: dưới mấy thếp vàng mã và một bó nhang chưa bóc, sát cánh trái
    static void SachGiaLe(Vector3 p, float yDay, float yaw)
    {
        var d = NutV3("SachGiaLe_CuaMe");
        Pv(d, PrimitiveType.Cube, "Sach_Bia", new Vector3(p.x, yDay + 0.009f, p.z), new Vector3(0.145f, 0.018f, 0.205f), "#D8CBA0", yaw + 4);
        Pv(d, PrimitiveType.Cube, "Sach_TenDo", new Vector3(p.x, yDay + 0.0185f, p.z), new Vector3(0.10f, 0.001f, 0.03f), "#8E3B34", yaw + 4);
        Pv(d, PrimitiveType.Cube, "Sach_GiayKep", new Vector3(p.x, yDay + 0.009f, p.z), new Vector3(0.03f, 0.012f, 0.215f), "#F2F0E8", yaw + 10);
        Pv(d, PrimitiveType.Cube, "VangMa_1", new Vector3(p.x, yDay + 0.027f, p.z), new Vector3(0.12f, 0.016f, 0.19f), "#D9B84A", yaw - 6);
        Pv(d, PrimitiveType.Cube, "VangMa_2", new Vector3(p.x + 0.01f, yDay + 0.043f, p.z), new Vector3(0.12f, 0.016f, 0.19f), "#C9A23E", yaw + 9);
        Pv(d, PrimitiveType.Cylinder, "BoNhang", new Vector3(p.x, yDay + 0.062f, p.z), new Vector3(0.03f, 0.12f, 0.03f), "#B8453A", yaw).transform.rotation = Quaternion.Euler(90, yaw + 20, 0);
    }
}
