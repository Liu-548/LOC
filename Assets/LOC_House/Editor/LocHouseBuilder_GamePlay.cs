using UnityEngine;

// [6/10] Gameplay cơ bản (kịch bản v3.1 mục 6.1, 8, Đêm 1): túi đồ (Tab) trên người chơi, món nhặt/đọc được, két khoá số 1980, cửa hầm khoá treo mở bằng chìa trong két.
//  Chuỗi Đêm 1: sổ thu chi 2002 (đọc) → két sắt (mã) → chùm chìa khoá hầm (nhặt) + sổ của bố (nhặt, Đêm 2 mới đọc) → cửa hầm.
public static partial class LocHouseBuilder
{
    static Transform Tim(string n)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t;
        Debug.LogWarning("[LOC gameplay] không thấy " + n); return null;
    }

    // hộp va chạm bao cả món (để tia nhìn trúng), cạnh tối thiểu `min` cho món nhỏ dễ nhắm
    static void VaChamBao(GameObject g, float min = 0.12f)
    {
        var rs = g.GetComponentsInChildren<Renderer>(true); if (rs.Length == 0) return;
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        var bc = g.AddComponent<BoxCollider>();
        bc.center = g.transform.InverseTransformPoint(b.center);
        var s = g.transform.InverseTransformVector(b.size);
        bc.size = new Vector3(Mathf.Max(Mathf.Abs(s.x), min), Mathf.Max(Mathf.Abs(s.y), min * 0.5f), Mathf.Max(Mathf.Abs(s.z), min));
    }

    static LocVatPham Mon(Transform t, string id, string ten, LocVatPham.Kieu kieu, string ghiChu, string[] khiNhat = null, string[] trang = null, int tuDem = 1, string chuaToi = "", int docTuDem = 0)
    {
        if (!t) return null;
        VaChamBao(t.gameObject);
        var v = t.gameObject.AddComponent<LocVatPham>();
        v.id = id; v.ten = ten; v.kieu = kieu; v.ghiChu = ghiChu; v.khiNhat = khiNhat ?? new string[0]; v.trang = trang ?? new string[0];
        v.tuDem = tuDem; v.chuaToi = chuaToi; v.docTuDem = docTuDem;
        return v;
    }

    static void GamePlay()
    {
        var nc = Tim("NguoiChoi"); if (nc) { nc.gameObject.AddComponent<LocTui>(); nc.gameObject.AddComponent<LocCotTruyen>(); nc.gameObject.AddComponent<LocMenu>(); }   // [22/10] menu   // [8/10] hệ thống đêm
        if (nc)   // [22/10] cắt cảnh ban ngày giữa hai đêm + âm thanh tạm (LOC_Nguon/BanNgay/am.py)
        {
            var bn = nc.gameObject.AddComponent<LocBanNgay>();
            AudioClip A(string n) => UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LOC_House/Audio/" + n + ".wav");
            bn.tungKinh = A("BanNgay_TungKinh"); bn.nguoiVieng = A("BanNgay_NguoiVieng"); bn.kenDam = A("BanNgay_KenDam"); bn.xeMay = A("BanNgay_XeMay"); bn.rotTra = A("Tra_Rot");
        }
        var ma = root.GetComponentInChildren<LocMa>(true); if (ma) ma.demBatDau = 1;   // [8/10] vào game là Đêm 1 (F1–F3 vẫn đổi để thử)
        var banTho = Tim("BanThoVong_Dem1"); if (banTho) { VaChamBao(banTho.gameObject); banTho.gameObject.AddComponent<LocViec>().id = "thap_huong"; }
        // [v3.3] điện thoại bàn (hai cuộc gọi không lời) · khung ảnh ông bà tầng 3 (thắp hương trước khi mở tủ thờ, Đêm 3)
        var dt = Tim("LOC_DienThoai_TronBo"); if (dt) { VaChamBao(dt.gameObject); dt.gameObject.AddComponent<LocViec>().id = "dien_thoai"; }
        var anhOngBa = Tim("KhungAnhOngBa"); if (anhOngBa) { VaChamBao(anhOngBa.gameObject); anhOngBa.gameObject.AddComponent<LocViec>().id = "thap_huong_t3"; }

        // ── sổ thu chi 2002 — đọc tại chỗ trong tủ hồ sơ phòng làm việc (mã két)
        Mon(Tim("SoThuChi_2002"), "so_thu_chi_2002", "sổ thu chi 2002", LocVatPham.Kieu.Doc, "Sổ thu chi của nhà, nét chữ của mẹ.", trang: new[]
        {
            "<b>THU CHI — 2002</b>\n\n<i>Nét chữ của mẹ, nhỏ, đều.</i>\n\nTháng 6\n\n2/6      Tiền học con Nhím ............... 120.000\n5/6      Chợ ....................................... 45.000\n11/6    Tiền nước ................................ 38.000\n20/6    Thuốc con Nhím ...................... 60.000\n28/6    Cúng rằm ................................. 150.000",
            "Tháng 7\n\n3/7      Chợ ....................................... 52.000\n8/7      Thuốc con Nhím ...................... 80.000\n12/7    Tiền điện tháng 6 .................. 186.000\n14/7    Thuốc con Nhím ...................... 95.000\n15/7    Chợ ....................................... 40.000\n26/7    Tiền điện tháng 7 .................. 192.000",
            "Tháng 8\n\n1/8      Thuốc con Nhím ...................... 95.000\n2/8      Bác sĩ sang bó bột ................. 150.000\n3/8      Chợ ....................................... 40.000\n<size=23><i>4/8      Ứng cho thằng Phong ....... 1.980.000</i></size>\n\n<i>(dòng cuối cùng của cả cuốn sổ: nét chữ khác — to, nghiêng, nét của bố)</i>",
        });

        // ── két sắt đứng phòng bố mẹ: khoá số 1980
        var ket = Tim("KetSat_Dung");
        if (ket)
        {
            var ks = ket.gameObject.AddComponent<LocKetSat>();
            var canh = ket.GetComponentInChildren<LocDoor>(true);
            if (canh) { canh.khoa = true; canh.khiKhoa = ks; ks.canh = canh; }

            // chùm chìa khoá hầm + khoá treo: gom các khối rời vào một nhóm nhặt được
            var vong = Tim("ChumChia_Vong");
            if (vong)
            {
                var chum = new GameObject("ChumChiaKhoaHam").transform; chum.SetParent(vong.parent, false);
                foreach (var n in new[] { "ChumChia_Vong", "ChiaKhoa_0", "ChiaKhoa_1", "ChiaKhoa_2", "KhoaTreo_Than" }) { var t = Tim(n); if (t) t.SetParent(chum, true); }
                Mon(chum, "chia_ham", "chùm chìa khóa", LocVatPham.Kieu.Nhat, "Một chùm chìa khóa lạ và cái khóa treo to. Lấy trong két sắt phòng bố mẹ.",
                    new[] { "Chìa khóa hầm à?", "Bố khóa cái này vào két làm gì.", "Bố mẹ có bao giờ cho xuống đâu." });
            }
            Mon(Tim("SoCuaBo_TrongKet"), "so_cua_bo", "sổ của bố", LocVatPham.Kieu.Nhat, "Cuốn sổ bìa cứng của bố, lấy trong két.",
                new[] { "Sổ của bố.", "Thôi cất đã. Mai xem." }, SoCuaBo(), docTuDem: 2);
        }

        // ── cửa sắt xuống hầm: khoá treo phía ngoài, mở bằng chìa trong két
        var cuaHam = Tim("CuaSatHam (mở)")?.GetComponent<LocDoor>();
        if (cuaHam) { cuaHam.khoa = true; cuaHam.canVatPham = "chia_ham"; cuaHam.loiKhoa = "Khóa treo bên ngoài."; }

        // ── quyển sách gia lễ của mẹ trong tủ thờ — chỉ Đêm 3 mới dám lấy; đọc xong bật cờ doc:sach_gia_le (= da_doc_sach, điều kiện Evil)
        Mon(Tim("SachGiaLe_CuaMe"), "sach_gia_le", "quyển sách", LocVatPham.Kieu.Nhat, "GIA LỄ PHỔ THÔNG — in lần thứ ba, 1998. Của mẹ, kẹp giấy hai chỗ.",
            new[] { "Con xin phép ông bà." }, new[]
            {
                "<b>CÚNG CÔ HỒN</b>\n\nCô hồn là vong không người thờ cúng. Lễ vật nên đơn sơ: cháo trắng nấu thật loãng, vì cô hồn cổ nhỏ, đồ đặc khó nuốt; gạo, muối; mía để cả vỏ chặt khúc; khoai lang luộc; bánh kẹo; nước sạch. Bày lễ nơi vong ở. Thắp nhang, khấn mời. Lễ chưa xong không được dời nhang, không để nhang tắt.",
                "<b>CẦU SIÊU CHO VONG KHÔNG NƠI NƯƠNG TỰA</b>\n\n1. Bày lễ như lễ cúng cô hồn, đặt nơi vong trú.\n2. Bày nước cho người trong nhà, đủ người, kể cả người đã khuất.\n3. Thắp nhang, khấn xin vong nhận lễ, bỏ oán, đi đường mình.\n4. <u>Hóa giấy trấn: những giấy niêm, giấy yểm giữ vong ở lại phải đem đốt cả, để vong không còn bị buộc.</u>\n      <i><color=#5A5048>giấy niêm trên hũ?</color></i>\n5. <u>Mở nơi vong trú — cửa, khám, hũ, tiểu — để vong đi. Không đóng lại, không niêm lại.</u>\n      <i><color=#5A5048>Ông ấy bảo không bao giờ được mở hũ. Không mở thì tiễn bằng cách nào?</color></i>\n6. Rắc gạo muối theo đường ra.",
            }, tuDem: 3, chuaToi: "Đồ thờ của ông bà. Đừng lục.");
        var sach = Tim("SachGiaLe_CuaMe")?.GetComponent<LocVatPham>(); if (sach) { sach.canCo = "thap_huong_t3"; sach.chuaCo = "Đồ thờ của ông bà. Đừng lục."; }

        // ── [v3.3] tờ lịch tháng 8 phòng khách (mọi đêm): mẹ khoanh ngày 9 — giỗ anh Phong
        Mon(Tim("LichThang8_2002"), "lich_t8", "tờ lịch", LocVatPham.Kieu.Xem, "", new[] { "Mẹ khoanh ngày chín.", "Giỗ anh Phong." });

        // ── đồ cúng Đêm 2 (mục 8.2): bát gạo, bát muối ở phòng khách, bó mía ở bếp
        Mon(Tim("LOC_BatGao"), "bat_gao", "bát gạo", LocVatPham.Kieu.Nhat, "Một bát gạo, để dưới đầu linh cữu.", tuDem: 2);
        Mon(Tim("LOC_BatMuoi"), "bat_muoi", "bát muối", LocVatPham.Kieu.Nhat, "Một bát muối hột.", tuDem: 2);
        Mon(Tim("BoMia"), "bo_mia", "bó mía", LocVatPham.Kieu.Nhat, "Mía chặt khúc, để cả vỏ. Mua cho đám.", tuDem: 2);
    }

    // cuốn sổ của bố — kịch bản v3.3 C5 Đêm 2 (sửa trang 1, 3, 7, 11, 17–21), Đêm 2: bìa + 26 trang + trang trắng + bìa sau
    static string[] SoCuaBo()
    {
        string[] t =
        {
            "10/8/89. Gặp thầy ở cổng đền. Mình chưa nói gì mà thầy đã bảo: nhà anh đang nợ, nợ to. Thầy bảo thầy giúp được.",
            "Thầy mang tới một cái hũ. Bảo trong đó là một vong không ai thờ. Hỏi là ai thì thầy không nói. Nuôi nó tử tế thì nó cho lộc.",
            "19/8. Làm lễ. Thầy cắt ít tóc hai vợ chồng bỏ vào hũ, bảo để nó nhận chủ. Thầy ghi tên cả nhà vào sớ, thầy giữ. Năm cái chén nước, mỗi người một cái, cho nó biết nhà này có những ai.",
            "30/8. Trúng số. Chưa bao giờ nghĩ đời mình có ngày này.",
            "22/6/96. Hết nợ rồi. Nhà cửa, cửa hàng, có cả.",
            "Vợ bảo trả hũ cho thầy, đủ rồi. Mình thì tiếc. Đang được thế này.",
            "Thầy bảo muốn trả thì đợi qua tháng bảy, tháng này nó động. Thầy làm lễ giữ mạng cho thằng Phong, nó tuổi xung. Lấy ít tóc. Đã cấm nó ra sông.",
            "14/8. Thằng Phong.\n\n\n\n<i><color=#7A6A58>(một vệt loang)</color></i>",
            "Thầy bảo: nó biết nhà mình định bỏ nó. Thầy giữ không nổi.",
            "Mình không tin cái thứ ấy nữa. Mang ra bến Phà Cũ ném rồi. Không nói với ai, cả vợ.",
            "Ra bến thắp hương cho thằng Phong. Thằng Khôi trượt chân. Người ta vớt kịp.",
            "Sáng ra có một gói để trước cửa. Giấy dầu, buộc lạt. Không ai gửi, không ai thấy ai mang tới. Mở ra là nó.",
            "Thầy bảo nó tự về. Vứt nó đi là nó lấy hết. Thầy nói đúng.",
            "Chén thằng Phong vỡ hôm nó đi. Mình ngồi dán lại được. Cũng lâu.",
            "Chén thằng Phong mình đem lên phòng thờ trên tầng rồi. Cho nó ở với ông bà. Nằm dưới ấy với cái kia là đủ.",
            "97. Xây nhà mới. Thầy bảo phải đào một cái hầm cho nó ở riêng. Cả dãy này chẳng nhà nào có hầm, thợ hỏi mãi. Mình bảo để chứa hàng.",
            "Tháng chạp 97. Thầy xuống hầm dán bùa, ở dưới ấy cả buổi chiều. Lên bảo bùa không trị được nó, chỉ giữ nó dưới ấy. Năm nào thầy cũng sẽ đến dán thêm.",
            "98. Cả phố cúp điện ba hôm. Thầy gọi sang dặn: nó chỉ mạnh trong tối. Xuống thì bật điện. Mình sơn luôn lên tường.",
            "Thầy dặn thêm, mình chép lại cho khỏi quên: trong nhà mà tối thì đứng yên. Đừng đi về phía nó. Nó không đi được tới mình, mình đi tới nó thôi.",
            "Mùng một tháng bảy năm ấy đóng cửa hàng cả ngày. Thầy bảo cả nhà ở yên trong nhà. Bùa chỉ giữ được trong nhà, ra khỏi cổng là hết.",
            "Từ đấy năm nào mùng một tháng bảy cũng cúng to dưới hầm. Thầy bảo sang tháng ấy bùa yếu, phải cho nó ăn no.",
            "9/2000. Thằng Khôi đi. Cãi nhau một trận. Mình nói nặng.",
            "Mấy năm nay làm ăn đi xuống. Thầy bảo nó đói, cúng thêm.",
            "Chưa kể gì mà thầy đã biết. Thầy giỏi thật.",
            "Con Nhím dạo này sốt mãi không dứt. Chén nó mẻ rồi. Mẻ hôm nào mình không biết.",
            "Thầy bảo để thầy lo.",
        };
        var l = new System.Collections.Generic.List<string>
        {
            "<b>Bìa</b>\n\n\n        Trần Quang Vũ            1959\n        Nguyễn Thị Liên          1960\n        Trần Hoài Phong          1980  <color=#5B4A6A>— 14 / 8 / 1996</color>\n        Trần Hoài Khôi            1982\n        Trần Ngọc Hân             2 / 4 / 1995",
        };
        for (int i = 0; i < t.Length; i++) l.Add($"<size=15><color=#8A7A68>{i + 1}</color></size>\n\n<i>{t[i]}</i>");
        l.Add("\n\n\n<color=#8A7A68><i>(trang trắng)</i></color>");
        l.Add("\n\n\n<color=#8A7A68><i>(mười mấy trang trắng nữa)</i></color>");
        l.Add("<b>Bìa sau, mặt trong</b>\n\n\n<i>Thầy Đồng — 826 315</i>");
        return l.ToArray();
    }
}

public static class LocTuKiemGamePlayMenu
{
    [UnityEditor.MenuItem("LOC/Tự kiểm gameplay (Play)")]
    static void Chay()
    {
        UnityEditor.EditorPrefs.SetBool("LOC_TuKiemGamePlay", true);
        if (!UnityEditor.EditorPrefs.HasKey("LOC_RIB")) UnityEditor.EditorPrefs.SetBool("LOC_RIB", UnityEditor.PlayerSettings.runInBackground);
        UnityEditor.PlayerSettings.runInBackground = true;   // LocNhimImport.TraLaiRIB trả lại khi thoát Play
        UnityEditor.EditorApplication.isPlaying = true;
    }
}
