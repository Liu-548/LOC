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
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("LOC", "Đang ở chế độ Play. Bấm Play lần nữa để thoát, rồi chạy lại menu này.", "OK");
            return;
        }
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
        if (mam) mam.sauCo = "he_hu";   // nghi lễ chỉ sau khi đã hé hũ (Đêm 3)
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
            for (int i = 0; i < 5; i++) Mo(truoc, "MG_ChenTra", new Vector3(c.x + R(ex), y, c.z + R(ez)), R(180));
            Mo(truoc, "MG_AmTra", new Vector3(c.x + ex * 0.3f, y, c.z - ez * 0.5f), 70);
            Mo(truoc, "MG_DiaVoHatDua", new Vector3(c.x - ex * 0.4f, y, c.z + ez * 0.3f), R(180));
            string[] keo = { "MG_GiayKeo_do", "MG_GiayKeo_vang", "MG_GiayKeo_lam" };
            for (int i = 0; i < 9; i++) Mo(truoc, keo[i % 3], new Vector3(c.x + R(ex), y, c.z + R(ez)), R(180));
            for (int i = 0; i < 4; i++) Mo(truoc, keo[(i + 1) % 3], new Vector3(c.x + R(ex * 1.6f), b.min.y, c.z + R(ez * 1.6f)), R(180));   // rơi dưới sàn
            Mo(truoc, "MG_BanhKeoThua", new Vector3(c.x - ex * 0.1f, y, c.z - ez * 0.1f), 12);
            Mo(truoc, "MG_GoiHatDua", new Vector3(c.x + ex * 0.6f, y, c.z + ez * 0.6f), -25);
            var sau = Nhom("SauDon_BanNuoc");
            Mo(sau, "MG_HopBanhKeo", new Vector3(c.x - ex * 0.2f, y, c.z + ez * 0.2f), 0);
            // cốc của bố — có ở cả hai trạng thái, không thuộc nhóm nào
            var cocP = new Vector3(c.x + ex * 0.55f, y, c.z - ez * 0.1f);
            var coc = Nhom("CocCuaBo_Up"); coc.transform.position = cocP;
            Mo(coc, "MG_CocUp", cocP, 200);
            Muc(truoc, 1, null, "mg_ban_nuoc"); Muc(sau, 1, "mg_ban_nuoc", null);

            var d = Diem("Salon_BanNuoc", "dọn bàn nước", Game<LocMG_DonBanNuoc>("DonBanNuoc"), 1, 3, 0f);
            if (d)
            {
                d.id = "mg_ban_nuoc"; d.sauCo = "ho_hang_ve"; d.truocHoangLoan = true; d.nhinVao = coc.transform;
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
            var bd = Bao(day); float yd = bd.center.y; float zd = bd.center.z + 0.02f;
            log.AppendLine($"  dây phơi x {bd.min.x:0.00}–{bd.max.x:0.00}");
            var truoc = Nhom("TruocRut_DayPhoi");
            float[] xs = { 3.09f, 3.57f, 4.05f, 4.53f, 5.05f };   // khe giữa năm áo tang (x 2,85 + 0,48·i)
            string[] treo = { "MG_AoHoaPhoi", "MG_QuanDuiPhoi", "MG_TatPhoi", "MG_KhanMatPhoi", "MG_KhanLauPhoi" };
            for (int i = 0; i < treo.Length; i++) Mo(truoc, treo[i], new Vector3(xs[i], yd, zd), 0);
            Muc(truoc, 2, null, "mg_quan_ao");
            for (int i = 1; i <= 5; i++) { var a = Tim("AoTang_Phoi_0" + i); if (a) Muc(a.gameObject, 1, null, "mg_quan_ao"); }

            var bb = Bao(be); float yb = bb.max.y; var cb = bb.center;
            var sau = Nhom("SauRut_DoGap");
            Mo(sau, "MG_DoTangGap", new Vector3(cb.x - 0.17f, yb, cb.z), 4);
            Mo(sau, "MG_QuanDuiGap", new Vector3(cb.x + 0.2f, yb, cb.z), 6);
            var aoGap = Mo(sau, "MG_AoHoaGap", new Vector3(cb.x + 0.2f, yb + 0.02f, cb.z), -3);
            Mo(sau, "MG_KhanGap", new Vector3(cb.x + 0.02f, yb, cb.z + 0.2f), 10);
            Muc(sau, 2, "mg_quan_ao", null);
            var huong = bd.center - cb; huong.y = 0; huong.Normalize();   // đứng giữa bể nước và dây phơi, cách bể 0,75 m
            var dung = Nhom("DungXem_DoGap"); dung.transform.position = new Vector3(cb.x, bb.min.y, cb.z) + huong * (Mathf.Max(bb.extents.x, bb.extents.z) + 0.75f);

            var d = Diem("DayPhoi", "rút quần áo", Game<LocMG_RutQuanAo>("RutQuanAo"), 2, 3, 0f);
            if (d && aoGap)
            {
                d.id = "mg_quan_ao"; d.truocHoangLoan = true; d.nhinVao = aoGap.transform; d.dungXem = dung.transform;
                d.cauPhatHien = new[] { "Tay áo bị cắt cho vừa cái bột.", "Mũi khâu nhỏ, đều tăm tắp.", "Chỉ mẹ mới khâu kiểu này." };
            }
        }

        // ── phòng làm việc của bố: giấy tờ bị lục ra từ Đêm 1 (bàn + sàn) → xếp vào tủ; bìa "KHÔI" để lại trên bàn, cuống phiếu xoè bên cạnh
        var banGiay = Tim("BanGiay_Go");
        var tuHs = Tim("TuHoSo_Sat");
        if (banGiay && tuHs)
        {
            var b = Bao(banGiay); float y = b.max.y; var c = b.center;
            var rd = new System.Random(9);
            float R(float a) => (float)(rd.NextDouble() * 2 - 1) * a;
            var truoc = Nhom("TruocXep_HoSo");
            for (int i = 0; i < 6; i++) Mo(truoc, "MG_HoaDon", new Vector3(c.x + R(0.35f), y + i * 0.0012f, c.z + R(0.12f)), R(40));
            Mo(truoc, "MG_SoHang", new Vector3(c.x - 0.3f, y + 0.008f, c.z + 0.05f), 8);
            Mo(truoc, "MG_SoThuChi", new Vector3(c.x + 0.28f, y + 0.008f, c.z - 0.02f), -14);
            var tu = Bao(tuHs);
            for (int i = 0; i < 4; i++) Mo(truoc, "MG_HoaDon", new Vector3(tu.min.x - 0.35f + R(0.15f), b.min.y + i * 0.0012f, tu.center.z + R(0.3f)), R(60));
            Mo(truoc, "MG_BiaHoSo", new Vector3(tu.min.x - 0.45f, b.min.y + 0.006f, tu.center.z + 0.15f), 30);
            Muc(truoc, 1, "ket_mo", "mg_ho_so");   // [v3.3] mở từ Đêm 1 sau khi mở két (TL-25)

            var sau = Nhom("SauXep_BiaKhoi");
            // bìa KHÔI + xấp cuống đặt chồng lên mép bìa: tìm chỗ trống trên mặt bàn (không đè bàn tính, hộp dấu, chồng sổ…), mặt chữ quay về phía người đứng
            var (p0, yaw) = ChoTrong(banGiay, new Vector2(0.30f, 0.36f), c.z > Bao(Tim("GheBanGiay") ?? banGiay).center.z ? 0f : 180f);
            Quaternion q = Quaternion.Euler(0, yaw, 0), qm = Quaternion.Euler(0, yaw + 180f, 0);   // FBX vào Unity: mặt chữ đọc từ phía +Z cục bộ → xoay 180° cho chữ quay về người đứng
            Mo(sau, "MG_BiaKhoi", p0 + qm * new Vector3(-0.03f, 0, 0.01f), yaw + 183);
            var cuong = Mo(sau, "MG_CuongPhieuXap", p0 + qm * new Vector3(0.04f, 0.0115f, -0.11f), yaw + 174);
            Muc(sau, 1, "mg_ho_so", null);
            var dung = Nhom("DungXem_HoSo"); dung.transform.position = new Vector3(p0.x, b.min.y, p0.z) + q * Vector3.back * 0.62f;

            var d = Diem("TuHoSo_Sat", "xếp lại giấy tờ", Game<LocMG_XepHoSo>("XepHoSo"), 1, 3, 0f);
            if (d && cuong)
            {
                d.id = "mg_ho_so"; d.sauCo = "ket_mo"; d.truocHoangLoan = true; d.nhinVao = cuong.transform; d.dungXem = dung.transform;
                d.cauPhatHien = new[] { "Phiếu chuyển tiền. Tháng nào cũng có, từ tháng chín năm 2000.", "Người gửi: Nguyễn Thị Liên.", "...Chữ bố.", "Hai năm. Không một chữ." };
            }
        }
        tt.muc = muc.ToArray();
        log.AppendLine("  trạng thái trước/sau: " + muc.Count + " mục");
    }

    // chỗ trống trên mặt bàn cho một món kt (x × z ở yaw 0): dò lưới 2 cm, né bao của mọi đồ đang đặt trên mặt bàn; thử yaw ưu tiên rồi xoay 90°.
    // Trả về tâm đáy + yaw. Không có chỗ thì đặt giữa bàn (ghi vào báo cáo).
    static (Vector3, float) ChoTrong(Transform ban, Vector2 kt, float yawUuTien)
    {
        var b = Bao(ban); float y = b.max.y;
        var vuong = new List<Bounds>();
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r.transform.IsChildOf(ban) || r.transform.IsChildOf(root)) continue;
            var rb = r.bounds;
            if (rb.max.y < y + 0.002f || rb.min.y > y + 0.6f || rb.max.x < b.min.x || rb.min.x > b.max.x || rb.max.z < b.min.z || rb.min.z > b.max.z) continue;
            vuong.Add(rb);
        }
        foreach (float yaw in new[] { yawUuTien, yawUuTien + 90f })
        {
            var s = Mathf.Abs(Mathf.Sin(yaw * Mathf.Deg2Rad)) > 0.5f ? new Vector2(kt.y, kt.x) : kt;
            var mepNguoi = Quaternion.Euler(0, yaw, 0) * Vector3.back;   // ưu tiên ô sát mép phía người đọc
            Vector3? tot = null; float diem = float.MaxValue;
            for (float x = b.min.x + s.x / 2 + 0.03f; x <= b.max.x - s.x / 2 - 0.03f; x += 0.02f)
                for (float z = b.min.z + s.y / 2 + 0.03f; z <= b.max.z - s.y / 2 - 0.03f; z += 0.02f)
                {
                    var o = new Bounds(new Vector3(x, y + 0.05f, z), new Vector3(s.x + 0.02f, 0.1f, s.y + 0.02f));
                    bool trung = false; foreach (var v in vuong) if (v.Intersects(o)) { trung = true; break; }
                    if (trung) continue;
                    float d = Vector3.Dot(new Vector3(x, 0, z) - new Vector3(b.center.x, 0, b.center.z), mepNguoi);
                    if (-d < diem) { diem = -d; tot = new Vector3(x, y, z); }
                }
            if (tot.HasValue) { log.AppendLine($"  bìa KHÔI: chỗ trống ({tot.Value.x:0.00}, {tot.Value.z:0.00}) yaw {yaw:0} — né {vuong.Count} món trên bàn"); return (tot.Value, yaw); }
        }
        log.AppendLine("! bàn giấy không còn chỗ trống cho bìa KHÔI — đặt giữa bàn");
        return (new Vector3(b.center.x, y, b.center.z), yawUuTien);
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

    // model đồ mini game (LOC_Nguon/MiniGame/build_mg.py → Models/MiniGame/*.fbx), một vật liệu atlas cho tất cả; gốc model = tâm đáy (đồ phơi: điểm treo)
    static Material atlas;
    static GameObject Mo(GameObject cha, string ten, Vector3 p, float yaw)
    {
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/LOC_House/Models/MiniGame/{ten}.fbx");
        if (!fbx) { log.AppendLine("! thiếu model " + ten); return null; }
        if (!atlas)
        {
            const string tp = "Assets/LOC_House/Textures/MiniGame/T_MG_Atlas.png", mp = "Assets/LOC_House/Materials/M_MG_Atlas.mat";
            if (AssetImporter.GetAtPath(tp) is TextureImporter ti && ti.filterMode != FilterMode.Point) { ti.filterMode = FilterMode.Point; ti.SaveAndReimport(); }
            atlas = AssetDatabase.LoadAssetAtPath<Material>(mp);
            if (!atlas) { atlas = new Material(Shader.Find("Universal Render Pipeline/Simple Lit")); AssetDatabase.CreateAsset(atlas, mp); }
            var tx = AssetDatabase.LoadAssetAtPath<Texture2D>(tp);
            atlas.SetTexture("_BaseMap", tx); atlas.mainTexture = tx; atlas.SetColor("_BaseColor", Color.white); atlas.SetFloat("_Smoothness", 0.15f);
            EditorUtility.SetDirty(atlas);
        }
        var g = Object.Instantiate(fbx, cha.transform); g.name = ten;
        g.transform.SetPositionAndRotation(p, Quaternion.Euler(0, yaw, 0) * fbx.transform.rotation);
        foreach (var r in g.GetComponentsInChildren<Renderer>(true)) r.sharedMaterial = atlas;
        foreach (var k in g.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(k);   // đồ trang trí, không chặn tia nhìn
        return g;
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
