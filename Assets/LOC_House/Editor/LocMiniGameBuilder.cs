#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: LOC ▸ Mini game ▸ Dựng hệ mini game vào scene.
// Gắn LocTuongTacMG + mini game vào các vật trong nhà theo LOC_BanVe_BoCuc_NhaLoc.md. Vật có sẵn trong scene được tìm theo tên;
// không thấy thì tạo cục đánh dấu (BoxCollider nhỏ) tại đúng toạ độ bản vẽ. Chạy lại được: xoá root "LOC_MiniGame" cũ rồi dựng lại
// (LocTuongTacMG gắn thẳng lên vật có sẵn cũng được gỡ trước khi dựng lại). Cuối cùng in báo cáo vào Console.
public static class LocMiniGameBuilder
{
    const string ROOT = "LOC_MiniGame";
    static Transform root, games;
    static StringBuilder log;
    static List<LocTuongTacMG> daGan;

    [MenuItem("LOC/Mini game/Dựng hệ mini game vào scene")]
    public static void Dung()
    {
        int n = DungVaoScene();
        EditorUtility.DisplayDialog("LOC", "Đã dựng " + n + " điểm tương tác. Xem Console để biết vật nào tìm thấy, vật nào phải dùng cục đánh dấu.", "OK");
    }

    // [21/10 gộp] LOC ▸ Dựng nhà LỘC gọi thẳng hàm này (không hộp thoại); báo cáo ghi thêm vào BaoCao_MiniGame.txt
    public static int DungVaoScene()
    {
        log = new StringBuilder("[LocMiniGameBuilder]\n");
        daGan = new List<LocTuongTacMG>();
        var cu = GameObject.Find(ROOT);
        if (cu) Undo.DestroyObjectImmediate(cu);
        foreach (var t in Object.FindObjectsByType<LocTuongTacMG>(FindObjectsSortMode.None))
            if (t) Undo.DestroyObjectImmediate(t);

        var r = new GameObject(ROOT); Undo.RegisterCreatedObjectUndo(r, "Dựng mini game");
        root = r.transform;
        games = new GameObject("TroChoi").transform; games.SetParent(root, false);

        var dongHo = Object.FindAnyObjectByType<LocDongHo>();
        if (!dongHo) log.AppendLine("! Không thấy LocDongHo trong scene: mini game đồng hồ vẫn chơi được nhưng kim 3D không nhảy.");

        // ───────── ĐÊM 1 — làm quen: 3 mảnh chén
        var tuTho = Tim("TuDoTho_Fix");
        var khoaSo = Game<LocMG_KhoaSo>("KhoaSo_TuDoTho");
        khoaSo.ma = "0508";
        var tTu = Gan(tuTho, "TuDoTho_Fix", new Vector3(5.20f, 7.0f, 6.40f), "tủ đồ thờ", LocTuongTacMG.Loai.MoMiniGame);
        tTu.miniGame = khoaSo; tTu.demTu = 1; tTu.demDen = 3; tTu.thuongId = LocId.ManhChen(1);
        tTu.cauThieu = "Tủ khoá số.";
        tTu.moKhoa = Cua(tuTho); tTu.khoaLucDau = true; tTu.moLuon = true;

        var mam = Game<LocMG_MamCung>("MamCung_BanThoVong");
        var tMam = Gan(Tim("BanThoVong_Dem1"), "BanThoVong", new Vector3(0.9f, 1.1f, 3.70f), "bàn thờ vong (bày mâm lễ)", LocTuongTacMG.Loai.MoMiniGame);
        tMam.miniGame = mam; tMam.demTu = 1; tMam.demDen = 1; tMam.thuongId = LocId.ManhChen(2);

        var ban = Game<LocMG_XepLaiBan>("XepLaiBan_Khoi");
        var tBan = Gan(Tim("BanHoc_Khoi_Bo"), "BanHoc_Khoi", new Vector3(5.70f, 4.15f, 16.10f), "bàn học của Khôi", LocTuongTacMG.Loai.MoMiniGame);
        tBan.miniGame = ban; tBan.demTu = 1; tBan.demDen = 1; tBan.thuongId = LocId.ManhChen(3);

        // ───────── ĐÊM 2 — trộn kiểu: 4 mảnh chén (4–7) + chuỗi đồ
        Nhat("MocChiaKhoa", new Vector3(6.29f, 1.50f, 9.40f), LocId.ChiaLenDay, "chìa lên dây đồng hồ", 1);

        var gDongHo = Game<LocMG_DongHo>("DongHo");
        gDongHo.dongHo = dongHo;
        var tDH = Gan(Tim("DongHoQuaLac"), "DongHoQuaLac", new Vector3(5.50f, 2.0f, 0.15f), "đồng hồ quả lắc", LocTuongTacMG.Loai.MoMiniGame);
        tDH.miniGame = gDongHo; tDH.demTu = 2; tDH.canId = LocId.ChiaLenDay; tDH.tieuHao = false;
        tDH.cauThieu = "Đồng hồ ngừng. Cần thứ gì đó để lên dây."; tDH.thuongId = LocId.ManhChen(4);

        Nhat("KhayAmChen", new Vector3(6.28f, 0.6f, 1.88f), LocId.ManhAnh1, "mảnh ảnh", 2);
        Nhat("GiaSach_Bo", new Vector3(7.50f, 4.8f, 15.40f), LocId.ManhAnh2, "mảnh ảnh", 2);
        Nhat("BanTrangDiem_Bo", new Vector3(0.21f, 4.2f, 2.45f), LocId.ManhAnh3, "mảnh ảnh", 2);
        Nhat("TranhNho_HanhLang", new Vector3(3.10f, 5.0f, 11.20f), LocId.ManhAnh4, "mảnh ảnh", 2);
        var ghepAnh = Game<LocMG_GhepAnh>("GhepAnh");
        var tAnh = Gan(Tim("AnhGiaDinh_Tex"), "AnhGiaDinh", new Vector3(7.55f, 1.8f, 1.95f), "ảnh gia đình", LocTuongTacMG.Loai.MoMiniGame);
        tAnh.miniGame = ghepAnh; tAnh.demTu = 2; tAnh.canCo = LocId.ManhAnh; tAnh.cauThieu = "Tấm ảnh thiếu mảnh";
        tAnh.thuongId = LocId.ManhChen(5);

        var noiDay = Game<LocMG_NoiDay>("NoiDay");
        var tDien = Gan(Tim("BangDienChinh"), "BangDienChinh", new Vector3(6.26f, 1.8f, 8.75f), "bảng điện chính", LocTuongTacMG.Loai.MoMiniGame);
        tDien.miniGame = noiDay; tDien.demTu = 2; tDien.thuongId = LocId.ManhChen(6);

        var xepGhe = Game<LocMG_XepGhe>("XepGhe");
        var tGhe = Gan(Tim("BanAn_Bo"), "BanAn_Bo", new Vector3(4.25f, 0.95f, 14.60f), "bàn ăn (xếp chén theo ghế)", LocTuongTacMG.Loai.MoMiniGame);
        tGhe.miniGame = xepGhe; tGhe.demTu = 2; tGhe.thuongId = LocId.DayDan; tGhe.thuongTen = "Sợi dây đàn";

        var lenDay = Game<LocMG_LenDay>("LenDay");
        var tDan = Gan(Tim("Guitar"), "Guitar", new Vector3(7.45f, 3.85f, 16.20f), "đàn guitar cũ", LocTuongTacMG.Loai.MoMiniGame);
        tDan.miniGame = lenDay; tDan.demTu = 2; tDan.canId = LocId.DayDan; tDan.cauThieu = "Đàn thiếu một dây."; tDan.thuongId = LocId.BangThuAm;

        var radio = Game<LocMG_Radio>("Radio");
        var tRadio = Gan(Tim("RadioCassette_2Loa"), "RadioCassette", new Vector3(7.30f, 2.15f, 4.20f), "radio cassette", LocTuongTacMG.Loai.MoMiniGame);
        tRadio.miniGame = radio; tRadio.demTu = 2; tRadio.canId = LocId.BangThuAm; tRadio.cauThieu = "Radio chưa có băng."; tRadio.thuongId = LocId.ManhChen(7);

        // ───────── ĐÊM 3 — áp lực: 3 mảnh chén (8–10)
        Nhat("GuongTreo_Sanh", new Vector3(0.05f, 4.9f, 6.22f), LocId.ManhGuong, "mảnh gương", 3);

        var giuLua = Game<LocMG_GiuLua>("GiuLua");
        var tLua = Gan(Tim("CuaSatHam"), "CuaSatHam", new Vector3(6.35f, 1.0f, 6.80f), "đèn dầu (xuống hầm)", LocTuongTacMG.Loai.MoMiniGame);
        tLua.miniGame = giuLua; tLua.demTu = 3; tLua.thuongId = LocId.ManhChen(8);

        var guong = Game<LocMG_GiaiChuGuong>("GiaiChuGuong");
        var tBang = Gan(Tim("BanHoc_Nhim"), "BanHoc_Nhim", new Vector3(5.00f, 3.65f, 12.22f), "bảng đen của Nhím", LocTuongTacMG.Loai.MoMiniGame);
        tBang.miniGame = guong; tBang.demTu = 3; tBang.thuongId = LocId.ManhChen(9);

        var nin = Game<LocMG_NinTho>("NinTho");
        var tAo = Gan(Tim("TuAo_Khoi"), "TuAo_Khoi", new Vector3(3.68f, 4.3f, 12.88f), "tủ áo (núp vào)", LocTuongTacMG.Loai.MoMiniGame);
        tAo.miniGame = nin; tAo.demTu = 3; tAo.demDen = 3; tAo.thuongId = LocId.ManhChen(10);

        // ───────── NGHI THỨC — ráp chén ở hầm, mỗi đêm một lần
        var ghep = Game<LocMG_GhepChen>("GhepChen");
        var tChen = Gan(Tim("Bo_Chen_Tren_De"), "ChenVo_Ham", new Vector3(6.40f, -1.6f, 13.10f), "chén vỡ trên bàn thờ hầm", LocTuongTacMG.Loai.MoMiniGame);
        tChen.miniGame = ghep; tChen.demTu = 1; tChen.demDen = 3; tChen.lapLai = true; tChen.tieuHao = false; tChen.tieuHaoCo = false;

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        log.AppendLine("Xong: " + daGan.Count + " điểm tương tác. Lưu scene (Ctrl+S). Thử: F1/F2/F3 đổi đêm, F7 cho đủ đồ, F8 bỏ qua mini game đang mở.");
        Debug.Log(log.ToString());
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_MiniGame.txt", log.ToString());
        return daGan.Count;
    }

    // ───────── tiện ích
    static T Game<T>(string ten) where T : LocMiniGame
    {
        var g = new GameObject(ten); g.transform.SetParent(games, false);
        return g.AddComponent<T>();
    }

    static GameObject Tim(string ten)
    {
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == ten || t.name.StartsWith(ten)) return t.gameObject;
        return null;
    }

    static LocDoor[] Cua(GameObject g)
    {
        if (!g) return new LocDoor[0];
        return g.GetComponentsInChildren<LocDoor>(true);
    }

    static LocTuongTacMG Gan(GameObject vat, string ten, Vector3 toaDo, string hienTen, LocTuongTacMG.Loai loai)
    {
        GameObject g;
        if (vat && (vat.GetComponentInChildren<Collider>() != null))
        {
            g = vat; log.AppendLine("  gắn lên vật có sẵn: " + ten);
        }
        else
        {
            g = new GameObject("TD_" + ten); g.transform.SetParent(root, false);
            g.transform.position = vat ? vat.transform.position : toaDo;
            var bc = g.AddComponent<BoxCollider>(); bc.size = new Vector3(0.4f, 0.4f, 0.4f);
            log.AppendLine((vat ? "  vật không có collider, dùng cục đánh dấu: " : "! không thấy vật, dùng cục đánh dấu tại toạ độ bản vẽ: ") + ten);
        }
        var khac = g.GetComponent<LocTuongTac>();
        if (khac && !(khac is LocTuongTacMG)) log.AppendLine("! trùng: " + ten + " đã có " + khac.GetType().Name + " (hệ gameplay Đêm 1)");
        var t = g.GetComponent<LocTuongTacMG>();
        if (!t) t = Undo.AddComponent<LocTuongTacMG>(g);
        t.loai = loai; t.ten = hienTen;
        daGan.Add(t);
        return t;
    }

    static void Nhat(string tenVat, Vector3 toaDo, string id, string hienTen, int demTu)
    {
        var t = Gan(Tim(tenVat), tenVat, toaDo, hienTen, LocTuongTacMG.Loai.Nhat);
        t.id = id; t.demTu = demTu; t.demDen = 3; t.giuVat = true;
        if (t.gameObject.name.StartsWith("TD_")) t.giuVat = false;
    }
}
#endif
