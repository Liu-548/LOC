#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// [21/10] Dựng các mini game hợp với kịch bản v3.2 vào scene (LOC → Dựng nhà LỘC gọi DungVaoScene; menu riêng để dựng lại nhanh).
// Mỗi điểm là một cục đánh dấu nhỏ (TD_…) đặt trên mặt vật thật, không gắn thẳng lên vật — để không đè LocVatPham/LocDoor/LocHuCot sẵn có.
//  Đêm 1–3  bàn học Khôi        Xếp lại bàn học (tuỳ chọn)
//  Đêm 2–3  bàn ăn giữa bếp     Chỗ ngồi quanh bàn ăn (tuỳ chọn)
//  Đêm 2    chén trên bàn thờ hầm   Dán chén 1 + 2 (kịch bản: dán chén dưới hầm)        → cờ dan_chen
//  Đêm 3    bàn thờ hầm         Nghi lễ vòng 1 bày mâm → vòng 3 giữ ngọn nhang          → cờ nl_bay_mam, nl_giu_nhang
// Không dùng (lệch kịch bản): khoá số tủ thờ (trùng két), đồng hồ, ghép ảnh, nối dây, guitar, radio (đài là dấu hiệu của thầy),
// giải chữ gương, nín thở trốn (game không có kẻ truy sát). Script của chúng vẫn giữ trong MiniGame/.
public static class LocMiniGameBuilder
{
    const string ROOT = "LOC_MiniGame";
    static Transform root, games;
    static StringBuilder log;
    static int dem;

    [MenuItem("LOC/Mini game/Dựng hệ mini game vào scene")]
    public static void Dung()
    {
        int n = DungVaoScene();
        EditorUtility.DisplayDialog("LOC", "Đã dựng " + n + " điểm mini game. Xem Assets/LOC_House/BaoCao_MiniGame.txt.", "OK");
    }

    public static int DungVaoScene()
    {
        log = new StringBuilder("[LocMiniGameBuilder] " + System.DateTime.Now + "\n"); dem = 0;
        var cu = GameObject.Find(ROOT);
        if (cu) Undo.DestroyObjectImmediate(cu);
        foreach (var t in Object.FindObjectsByType<LocTuongTacMG>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t) Undo.DestroyObjectImmediate(t);

        root = new GameObject(ROOT).transform; Undo.RegisterCreatedObjectUndo(root.gameObject, "Dựng mini game");
        games = new GameObject("TroChoi").transform; games.SetParent(root, false);

        var ban = Diem("BanHoc_Khoi_Bo", "bàn học", Game<LocMG_XepLaiBan>("XepLaiBan_Khoi"), 1, 3, 0f);
        if (ban) ban.id = "mg_ban_hoc";

        var ghe = Diem("BanAn_Bo", "bàn ăn", Game<LocMG_XepGhe>("XepGhe"), 2, 3, 0f);
        if (ghe) ghe.id = "mg_ban_an";

        var chen = Diem("Bo_Chen_Tren_De", "Dán chén", Game<LocMG_GhepChen>("DanChen"), 2, 2, 0f);
        if (chen) chen.id = "dan_chen";

        var mam = Diem("BanThoHam", "Bày mâm", Game<LocMG_MamCung>("BayMam"), 3, 3, 0.15f);
        var lua = Game<LocMG_GiuLua>("GiuNhang");
        lua.tieuDe = "Giữ ngọn nhang"; lua.goiY = "Gió lạnh thổi từng đợt. Giữ SPACE (hoặc chuột phải) để che, đừng che quá lâu kẻo mỏi tay.";
        lua.cauXong = "Ngọn nhang vẫn cháy. Gió yếu dần.";
        var nhang = Diem("BanThoHam", "Giữ nhang", lua, 3, 3, 0.15f);
        if (mam && nhang)
        {
            mam.id = "nl_bay_mam"; nhang.id = "nl_giu_nhang";
            nhang.gameObject.SetActive(false); mam.hien = new[] { nhang.gameObject };   // vòng 3 chỉ mở sau khi bày mâm
        }

        SapXep();   // [22/10] ba mini game sắp xếp: làm xong thì nhà đổi theo + một chi tiết hé lộ

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        log.AppendLine("Xong: " + dem + " điểm. Bỏ qua mini game: nút [Bỏ qua] hoặc phím B (LocMiniGame.choBoQua).");
        Debug.Log(log.ToString());
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_MiniGame.txt", log.ToString());
        return dem;
    }

    // ───── [22/10] sắp xếp → nhà đổi theo (LocTrangThaiMG) → camera dừng ở một món, Khôi nghĩ vài dòng (LocTuongTacMG.cauPhatHien)
    //  Đêm 1–3  bàn nước phòng khách (sau khi cô về)  Dọn bàn nước   → cốc của bố, không ai cất
    //  Đêm 2–3  dây phơi sân phơi T3                 Rút quần áo    → áo Nhím cắt tay cho vừa bột, mũi khâu của mẹ
    //  Đêm 2–3  tủ hồ sơ phòng làm việc của bố       Xếp lại giấy tờ → cuống phiếu chuyển tiền gửi Khôi, tên mẹ, chữ bố
    static LocTrangThaiMG tt;
    static void SapXep()
    {
        tt = root.gameObject.AddComponent<LocTrangThaiMG>();
        var muc = new List<LocTrangThaiMG.Muc>();
        void Muc(GameObject g, int tu, string hien, string an) { if (g) muc.Add(new LocTrangThaiMG.Muc { vat = g, demTu = tu, demDen = 3, hienKhiCo = hien, anKhiCo = an }); }

        // ── bàn nước: chén khách, ấm nguội, vỏ hạt dưa, giấy kẹo → dọn đi; cốc của bố ở lại
        var ban = Tim("Salon_BanNuoc");
        if (ban)
        {
            var b = Bao(ban); float y = b.max.y; var c = b.center; float ex = b.extents.x * 0.7f, ez = b.extents.z * 0.7f;
            var rd = new System.Random(5);
            float R(float a) => (float)(rd.NextDouble() * 2 - 1) * a;
            var truoc = Nhom("TruocDon_BanNuoc");
            for (int i = 0; i < 5; i++) { var p = new Vector3(c.x + R(ex), y, c.z + R(ez)); Tru(truoc, "ChenTra", p, 0.055f, 0.04f, "#E8E2CF"); Tru(truoc, "NuocCheDong", p + Vector3.up * 0.032f, 0.044f, 0.004f, "#6B4A1E"); }
            Tru(truoc, "AmTra_Nguoi", new Vector3(c.x + ex * 0.3f, y, c.z - ez * 0.5f), 0.13f, 0.1f, "#CDBE98");
            Hop(truoc, "VoiAm", new Vector3(c.x + ex * 0.3f + 0.08f, y + 0.06f, c.z - ez * 0.5f), new Vector3(0.07f, 0.018f, 0.018f), "#CDBE98", 20);
            var dia = new Vector3(c.x - ex * 0.4f, y, c.z + ez * 0.3f);
            Tru(truoc, "DiaVoHatDua", dia, 0.17f, 0.012f, "#F2EEE6");
            for (int i = 0; i < 26; i++) Hop(truoc, "VoHatDua", dia + new Vector3(R(0.06f), 0.014f + i * 0.0006f, R(0.06f)), new Vector3(0.012f, 0.003f, 0.007f), i % 3 == 0 ? "#2A1814" : "#8E2A22", R(180));
            string[] mauKeo = { "#C9372C", "#E3B23C", "#3F7CAC", "#5FA35A", "#D96FA0" };
            for (int i = 0; i < 9; i++) Hop(truoc, "GiayKeo", new Vector3(c.x + R(ex), y + 0.002f, c.z + R(ez)), new Vector3(0.035f, 0.002f, 0.025f), mauKeo[i % 5], R(180));
            for (int i = 0; i < 4; i++) Hop(truoc, "GiayKeo_DuoiSan", new Vector3(c.x + R(ex * 1.6f), 0.004f, c.z + R(ez * 1.6f)), new Vector3(0.035f, 0.002f, 0.025f), mauKeo[(i + 2) % 5], R(180));
            Hop(truoc, "BanhIn_KeoLac", new Vector3(c.x - ex * 0.1f, y, c.z - ez * 0.1f), new Vector3(0.09f, 0.025f, 0.05f), "#D8C9A4", 12);
            Hop(truoc, "GoiHatDua", new Vector3(c.x + ex * 0.6f, y, c.z + ez * 0.6f), new Vector3(0.12f, 0.03f, 0.18f), "#B8302A", -25);
            var sau = Nhom("SauDon_BanNuoc");
            Tru(sau, "HopBanhKeo_Nap", new Vector3(c.x - ex * 0.2f, y, c.z + ez * 0.2f), 0.24f, 0.07f, "#A8291F");
            Tru(sau, "HopBanhKeo_VienVang", new Vector3(c.x - ex * 0.2f, y + 0.07f, c.z + ez * 0.2f), 0.2f, 0.003f, "#D9A53A");
            // cốc của bố — có ở cả hai trạng thái, không thuộc nhóm nào
            var cocP = new Vector3(c.x + ex * 0.55f, y, c.z - ez * 0.1f);
            var coc = Nhom("CocCuaBo_Up"); coc.transform.position = cocP;
            Tru(coc, "DiaLot", cocP, 0.11f, 0.008f, "#E8E2CF");
            Tru(coc, "Coc", cocP + Vector3.up * 0.008f, 0.075f, 0.09f, "#6B3E22");
            Hop(coc, "QuaiCoc", cocP + new Vector3(0.05f, 0.05f, 0), new Vector3(0.012f, 0.05f, 0.02f), "#6B3E22");
            Muc(truoc, 1, null, "mg_ban_nuoc"); Muc(sau, 1, "mg_ban_nuoc", null);

            var d = Diem("Salon_BanNuoc", "dọn bàn nước", Game<LocMG_DonBanNuoc>("DonBanNuoc"), 1, 3, 0f);
            if (d)
            {
                d.id = "mg_ban_nuoc"; d.sauCo = "ho_hang_ve"; d.nhinVao = coc.transform;
                var vc = d.GetComponent<BoxCollider>();   // bàn thấp (0,46 m): tia nhìn chéo xuống trúng mặt bàn trước cục 0,35 m → phủ kín mặt bàn
                vc.size = new Vector3(b.size.x, 0.3f, b.size.z); vc.center = new Vector3(c.x - d.transform.position.x, y + 0.12f - d.transform.position.y, c.z - d.transform.position.z);
                d.cauPhatHien = new[] { "Bố uống xong lúc nào cũng úp cốc như thế.", "Để đấy." };
            }
        }

        // ── sân phơi: đồ tang + đồ Nhím trên dây → gấp, xếp lên nắp bể nước
        var day = Tim("DayPhoi");
        var be = Tim("BeNuoc_Nap");
        if (day && be)
        {
            var bd = Bao(day); float yd = bd.center.y; float zd = bd.center.z + 0.03f;
            var truoc = Nhom("TruocRut_DayPhoi");
            float[] xs = { 3.09f, 3.57f, 4.05f, 4.53f, 5.05f };   // khe giữa năm áo tang (x 2,85 + 0,48·i)
            var ao = HopTex(truoc, "AoHoa_Nhim_Phoi", new Vector3(xs[0], yd - 0.17f, zd), new Vector3(0.30f, 0.32f, 0.008f), "T_MG_AoHoa_Nhim.png");
            Hop(truoc, "QuanDui_Nhim", new Vector3(xs[1], yd - 0.13f, zd), new Vector3(0.22f, 0.24f, 0.008f), "#5B7FA8");
            Hop(truoc, "TatTreCon_1", new Vector3(xs[2] - 0.04f, yd - 0.08f, zd), new Vector3(0.05f, 0.14f, 0.006f), "#F0F0EA");
            Hop(truoc, "TatTreCon_2", new Vector3(xs[2] + 0.04f, yd - 0.08f, zd), new Vector3(0.05f, 0.14f, 0.006f), "#F0F0EA");
            Hop(truoc, "KhanMat", new Vector3(xs[3], yd - 0.16f, zd), new Vector3(0.25f, 0.32f, 0.006f), "#7FB6C9");
            Hop(truoc, "KhanLauTay", new Vector3(xs[4], yd - 0.14f, zd), new Vector3(0.22f, 0.26f, 0.006f), "#E9D7A8");
            Muc(truoc, 2, null, "mg_quan_ao");
            for (int i = 1; i <= 5; i++) { var a = Tim("AoTang_Phoi_0" + i); if (a) Muc(a.gameObject, 1, null, "mg_quan_ao"); }

            var bb = Bao(be); float yb = bb.max.y; var cb = bb.center;
            var sau = Nhom("SauRut_DoGap");
            float t = yb;
            for (int i = 0; i < 4; i++) t = HopDay(sau, "DoTang_Gap", new Vector3(cb.x - 0.18f, t, cb.z), new Vector3(0.30f, 0.025f, 0.24f), i % 2 == 0 ? "#F4F1E8" : "#E9E5DA", i * 3 - 4);
            t = HopDay(sau, "QuanDui_Gap", new Vector3(cb.x + 0.2f, yb, cb.z), new Vector3(0.18f, 0.02f, 0.15f), "#5B7FA8", 6);
            var aoGap = HopTex(sau, "AoHoa_Nhim_Gap", new Vector3(cb.x + 0.2f, t + 0.009f, cb.z), new Vector3(0.20f, 0.018f, 0.17f), "T_MG_AoHoa_Nhim.png", -3);
            Hop(sau, "DuongCatTay_Vien", new Vector3(cb.x + 0.2f - 0.09f, t + 0.019f, cb.z), new Vector3(0.006f, 0.002f, 0.16f), "#FFFFFF", -3);   // mép cắt viền chỉ trắng
            HopDay(sau, "Khan_Gap", new Vector3(cb.x + 0.02f, yb, cb.z + 0.2f), new Vector3(0.16f, 0.03f, 0.12f), "#7FB6C9", 10);
            Muc(sau, 2, "mg_quan_ao", null);

            var d = Diem("DayPhoi", "rút quần áo", Game<LocMG_RutQuanAo>("RutQuanAo"), 2, 3, 0f);
            if (d)
            {
                d.id = "mg_quan_ao"; d.nhinVao = aoGap.transform;
                d.cauPhatHien = new[] { "Tay áo bị cắt cho vừa cái bột.", "Mũi khâu nhỏ, đều tăm tắp.", "Chỉ mẹ mới khâu kiểu này." };
            }
        }

        // ── phòng làm việc của bố: giấy tờ bị lục ra từ Đêm 1 (bàn + sàn) → xếp vào tủ; bìa "KHÔI" để lại trên bàn, mở sẵn
        var banGiay = Tim("BanGiay_Go");
        if (banGiay && Tim("TuHoSo_Sat"))
        {
            var b = Bao(banGiay); float y = b.max.y; var c = b.center;
            var rd = new System.Random(9);
            float R(float a) => (float)(rd.NextDouble() * 2 - 1) * a;
            var truoc = Nhom("TruocXep_HoSo");
            for (int i = 0; i < 6; i++) HopTex(truoc, "HoaDon_Roi", new Vector3(c.x + R(0.35f), y + 0.002f + i * 0.0015f, c.z + R(0.12f)), new Vector3(0.15f, 0.0015f, 0.21f), "T_MG_HoaDon.png", R(40));
            HopDay(truoc, "SoHang_2000", new Vector3(c.x - 0.3f, y, c.z + 0.05f), new Vector3(0.21f, 0.02f, 0.30f), "#7A4E2E", 8);
            HopDay(truoc, "SoThuChi_1999", new Vector3(c.x + 0.28f, y, c.z - 0.02f), new Vector3(0.21f, 0.018f, 0.30f), "#8C6E3E", -14);
            var tu = Bao(Tim("TuHoSo_Sat"));
            for (int i = 0; i < 4; i++) HopTex(truoc, "HoaDon_DuoiSan", new Vector3(tu.min.x - 0.35f + R(0.15f), b.min.y + 0.003f + i * 0.0015f, tu.center.z + R(0.3f)), new Vector3(0.15f, 0.0015f, 0.21f), "T_MG_HoaDon.png", R(60));
            HopDay(truoc, "BiaHoSo_DuoiSan", new Vector3(tu.min.x - 0.45f, b.min.y, tu.center.z + 0.15f), new Vector3(0.24f, 0.012f, 0.32f), "#C4A066", 30);
            Muc(truoc, 2, null, "mg_ho_so");

            var sau = Nhom("SauXep_BiaKhoi");
            var p0 = new Vector3(c.x - 0.05f, y, b.min.z + 0.2f);   // mép bàn phía ghế — chỗ trống, không đè chồng sổ cũ / bàn tính
            HopTex(sau, "BiaKhoi_TrangTrai", p0 + new Vector3(-0.13f, 0, 0), new Vector3(0.25f, 0.004f, 0.33f), "T_MG_BiaHoSo_Khoi.png", 180);
            float t = y;
            for (int i = 0; i < 7; i++) t = HopDay(sau, "CuongPhieu_Xap", new Vector3(p0.x + 0.13f, t, p0.z), new Vector3(0.21f, 0.0012f, 0.15f), "#E8E0C6", (i % 3 - 1) * 2.5f);
            var cuong = HopTex(sau, "CuongPhieu_TrenCung", new Vector3(p0.x + 0.13f, t, p0.z), new Vector3(0.21f, 0.0015f, 0.15f), "T_MG_CuongPhieu.png", 182);
            Muc(sau, 2, "mg_ho_so", null);

            var d = Diem("TuHoSo_Sat", "xếp lại giấy tờ", Game<LocMG_XepHoSo>("XepHoSo"), 2, 3, 0f);
            if (d)
            {
                d.id = "mg_ho_so"; d.nhinVao = cuong.transform;
                d.cauPhatHien = new[] { "Phiếu chuyển tiền. Tháng nào cũng có, từ tháng chín năm 2000.", "Người gửi: Nguyễn Thị Liên.", "...Chữ bố." };
            }
        }
        tt.muc = muc.ToArray();
        log.AppendLine("  trạng thái trước/sau: " + muc.Count + " mục");
    }

    static Transform Tim(string ten)
    {
        foreach (var t0 in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t0.name != ten) continue;
            var t = t0; while (t.parent && t.parent.name == ten) t = t.parent;   // GLB: nút gốc và lưới con trùng tên → lấy nút gốc
            return t;
        }
        log.AppendLine("! không thấy " + ten); return null;
    }
    static Bounds Bao(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>(true);
        var b = rs.Length > 0 ? rs[0].bounds : new Bounds(t.position, Vector3.zero);
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
    static GameObject Nhom(string ten) { var g = new GameObject(ten); g.transform.SetParent(root, false); return g; }

    static readonly Dictionary<string, Material> mats = new();
    static Material Mau(string hex, string tex = null)
    {
        string k = hex + tex;
        if (mats.TryGetValue(k, out var m) && m) return m;
        ColorUtility.TryParseHtmlString(hex, out var c);
        m = new Material(Shader.Find("Universal Render Pipeline/Simple Lit")) { name = "M_MG_" + (tex ?? hex.TrimStart('#')) };
        m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", 0.1f);
        if (tex != null)
        {
            string p = "Assets/LOC_House/Textures/MiniGame/" + tex;
            if (AssetImporter.GetAtPath(p) is TextureImporter ti && ti.filterMode != FilterMode.Point) { ti.filterMode = FilterMode.Point; ti.SaveAndReimport(); }
            var tx = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (tx) m.SetTexture("_BaseMap", tx); else log.AppendLine("! thiếu texture " + p);
        }
        return mats[k] = m;
    }
    // hộp tâm tại p (p.y = đáy); không va chạm — đồ trang trí, không chặn tia nhìn
    static GameObject Hop(GameObject cha, string ten, Vector3 p, Vector3 kt, string hex, float yaw = 0, string tex = null)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(g.GetComponent<Collider>());
        g.name = ten; g.transform.SetParent(cha.transform, true);
        g.transform.SetPositionAndRotation(p + Vector3.up * kt.y / 2f, Quaternion.Euler(0, yaw, 0)); g.transform.localScale = kt;
        g.GetComponent<Renderer>().sharedMaterial = Mau(hex, tex);
        return g;
    }
    static GameObject HopTex(GameObject cha, string ten, Vector3 p, Vector3 kt, string tex, float yaw = 0) => Hop(cha, ten, p, kt, "#FFFFFF", yaw, tex);
    static float HopDay(GameObject cha, string ten, Vector3 p, Vector3 kt, string hex, float yaw = 0) { Hop(cha, ten, p, kt, hex, yaw); return p.y + kt.y; }
    static void Tru(GameObject cha, string ten, Vector3 p, float dk, float cao, string hex)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(g.GetComponent<Collider>());
        g.name = ten; g.transform.SetParent(cha.transform, true);
        g.transform.position = p + Vector3.up * cao / 2f; g.transform.localScale = new Vector3(dk, cao / 2f, dk);
        g.GetComponent<Renderer>().sharedMaterial = Mau(hex);
    }

    static T Game<T>(string ten) where T : LocMiniGame
    {
        var g = new GameObject(ten); g.transform.SetParent(games, false);
        return g.AddComponent<T>();
    }

    // cục đánh dấu 0,35 m đặt giữa mặt trên vật (dịch `lui` m theo +Z — phía người đứng ở hầm), mở mini game trong khoảng đêm
    static LocTuongTacMG Diem(string tenVat, string hienTen, LocMiniGame mg, int tu, int den, float lui)
    {
        Transform vat = null;
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == tenVat) { vat = t; break; }
        if (!vat) { log.AppendLine("! không thấy " + tenVat + " — bỏ điểm " + hienTen); return null; }
        var rs = vat.GetComponentsInChildren<Renderer>(true);
        var b = rs.Length > 0 ? rs[0].bounds : new Bounds(vat.position, Vector3.zero);
        foreach (var r in rs) b.Encapsulate(r.bounds);

        var g = new GameObject("TD_" + mg.name); g.transform.SetParent(root, false);
        g.transform.position = new Vector3(b.center.x, b.max.y + 0.12f, b.center.z + lui);
        g.AddComponent<BoxCollider>().size = Vector3.one * 0.35f;
        var tt = g.AddComponent<LocTuongTacMG>();
        tt.loai = LocTuongTacMG.Loai.MoMiniGame; tt.ten = hienTen; tt.miniGame = mg; tt.demTu = tu; tt.demDen = den;
        dem++;
        log.AppendLine($"  {hienTen,-12} Đêm {tu}–{den}  trên {tenVat}  tại {g.transform.position}");
        return tt;
    }
}
#endif
