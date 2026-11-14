using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [27/10] Phần gameplay v3.3 còn thiếu (VIEC_CON_LAI_GAMEPLAY.md mục 1.1–1.4), làm thành cơ chế chạy được ngay, KHÔNG cần dựng lại nhà:
//  [29/10] đồ có hình (lọ keo, chén số ba, gói hàng, lời nhắn của mẹ, các trạng thái chén hầm) nay là model thật do LocHouseBuilder_V35 đặt
//  khi dựng nhà; ở đây chỉ tìm theo tên rồi gắn tương tác. Giấy tờ chỉ nằm trong túi vẫn là vật ẩn (MonAn).
//
//  ĐÊM 2  cuộc gọi không lời (goi_2) → ~35 s sau điện thoại reo lại, lần này có lời: thầy Đồng (goi_thay) → Khôi chép lời ra giấy
//         → gom lọ keo (cửa hàng) + chén số ba (bàn thờ ông bà tầng ba) → dán chén dưới hầm (dan_chen, mini game sẵn có)
//         → bưng bát cháo (bếp) xuống bàn thờ hầm (cung_chao) → [đoạn hoảng loạn, đã có] → giữ hương → cảnh nửa tỉnh → hết đêm.
//  ĐÊM 3  gói hàng giấy dầu trong cửa (mo_kien): bài khấn + hai tờ giấy gói chữ son + tờ niêm · lời nhắn của mẹ (phòng bố mẹ)
//         nghi lễ: vòng 1 bày mâm (đã có) → vòng 2 đọc hết bài khấn trước bàn thờ hầm (nl_doc_van) → vòng 3 giữ nhang (đã có; nhang tắt = Bad b)
//         → chỗ rẽ ở hũ cốt: [E] Niêm hũ / [F] Xé giấy niêm.
//  KẾT    Xé niêm → bốn lọn tóc, bùa rơi, "Cảm ơn" → sáng hôm sau (cảnh Good đã có) → chữ cuối, điện thoại reo.
//         Niêm hũ + đã đọc sách của mẹ → cổng sáng mùng một → chân cầu thang, tiếng gọi (Evil đã có) → "Cháu khá hơn bố cháu." → năm năm sau.
//         Niêm hũ + chưa đọc sách → cổng sáng mùng một → Bad (cảnh đã có). Bad (a/b/c) → hầm: chén số năm vỡ → bốn năm sau.
//  Chữ thoại / nội dung giấy ở đây là BẢN TẠM (kịch bản v3.3 chưa chốt 🗣) — sửa thẳng trong file này.
public partial class LocCotTruyen
{
    const string THAY = "THẦY ĐỒNG";
    LocVatPham vpKeo, vpChen3, vpChao, vpLoiNhanMe, vpGiayThay, vpVanKhan, vpGiayGoi, vpGiayNiem;
    LocTuongTacMG mgDanChen, mgBayMam, mgGiuNhang;
    // [02/11] walkthrough #8: mâm cúng Đêm 3 đòi đủ gạo, muối, mía (trước nhặt được mà không dùng vào đâu)
    static readonly (string id, string ten)[] DO_MAM = { ("bat_gao", "gạo"), ("bat_muoi", "muối"), ("bo_mia", "mía") };
    string[] canCoMam0; readonly List<string> canMam = new(); string thieuCu;
    LocVatTam kienHang, choDatChao;
    GameObject chaoTrenBan;
    float tGoi2 = -1f, tReoThay;
    bool daNhacKhan, daNhacDocHam, daNhacBayMam, dangKet, daDanChen;
    public bool DangKet => dangKet;   // LocNhim.TatCanh: đang trong phần kết thì giữ màn đen, giữ khoá
    // [02/11] v3.3: chưa đọc lời nhắn của mẹ thì chưa hé hũ được (chỉ khoá khi lời nhắn có thật trong cảnh — không có thì không chặn)
    public static bool KhoaHeHu => I && I.vpLoiNhanMe && !LocTui.Co("doc:loi_nhan_me");

    void GanThapHuongDem2()
    {
        Transform b1 = null, b2 = null;
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name == "BanThoVong_Dem1") b1 = t;
            else if (t.name == "BanThoVong_Dem2") b2 = t;
        }
        if (!b2) return;
        foreach (var v in b2.GetComponents<LocViec>()) if (v.id == "thap_huong") return;   // đã có (cảnh dựng lại bằng bản builder mới)
        BoxCollider goc = null;
        if (b1) foreach (var c in b1.GetComponents<BoxCollider>()) if (!goc || c.size.x * c.size.y * c.size.z > goc.size.x * goc.size.y * goc.size.z) goc = c;
        var bc = b2.gameObject.AddComponent<BoxCollider>();
        if (goc)
        {
            bc.center = b2.InverseTransformPoint(goc.transform.TransformPoint(goc.center));
            var s = b2.InverseTransformVector(goc.transform.TransformVector(goc.size));
            bc.size = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        }
        else bc.size = new Vector3(2.8f, 1.2f, 0.9f);   // không có bàn Đêm 1 để sao: hộp tạm cỡ bộ bàn thờ vong
        b2.gameObject.AddComponent<LocViec>().id = "thap_huong";
    }

    // ───── khởi tạo (gọi trong Start, trước ApDem)
    void KhoiTaoV34()
    {
        foreach (var t in FindObjectsByType<LocTuongTacMG>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.id == "dan_chen") mgDanChen = t;
            if (t.id == "nl_bay_mam") { mgBayMam = t; canCoMam0 = t.canCo ?? new string[0]; }
            if (t.id == "nl_giu_nhang") mgGiuNhang = t;
        }
        var cha = new GameObject("LOC_CotTruyen_V34").transform;

        // [3/11] SỬA: Đêm 2 không hiện "[E] Thắp nén mới" — LocViec "thap_huong" + hộp va chạm chỉ gắn trên BanThoVong_Dem1 (bị tắt ở Đêm 2),
        //  còn BanThoVong_Dem2 không có → gắn lại khi vào game (sao hộp va chạm của bàn Đêm 1: hai bàn cùng chỗ, cùng hướng).
        GanThapHuongDem2();

        // ── Đêm 2: lọ keo trên quầy cửa hàng · chén số ba trên bàn thờ ông bà · bát cháo trên bàn ăn
        var keo = TimV("LoKeo", "NganKeo");   // lọ keo nằm trong ngăn kéo quầy (bộ LOC_CanhQuayHang)
        if (keo)
            vpKeo = Mon(keo.gameObject, "lo_keo", "lọ keo", LocVatPham.Kieu.Nhat, "Lọ keo dán sành sứ, hàng của cửa hàng nhà mình. Còn hơn nửa.", 2,
                new[] { "Keo dán sứ. Cả phố toàn mua ở nhà mình." });
        var chen3 = TimV("VP_ChenSo3");
        if (chen3)
            vpChen3 = Mon(chen3.gameObject, "chen_3", "chén số ba", LocVatPham.Kieu.Nhat, "Chén của anh Phong. Vỡ một lần, bố dán lại. Đường keo cũ ngả vàng.", 2,
                new[] { "Chén của anh Phong.", "Bố dán lại. Vẫn còn thấy đường nứt." });
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == "BatChao_An" && t.parent && t.parent.name == "Dem2" && !t.GetComponent<LocVatPham>())
            {
                if (!t.GetComponentInChildren<Collider>(true)) HopTheoLuoi(t);   // nhóm Dem2 có thể đang ẩn: đo theo lưới, không theo Renderer.bounds
                vpChao = Mon(t.gameObject, "bat_chao", "bát cháo", LocVatPham.Kieu.Nhat, "Cháo trắng loãng bác nấu cho con bé. Nguội từ chiều.", 2,
                    new[] { "Nó có ăn được mấy đâu." }, dangKy: true);
                break;
            }
        var ham = GameObject.Find("BanThoHam");
        if (ham && vpChao)
        {
            var bh = BaoV(ham.transform);
            var g = new GameObject("CHO_DatChao"); g.transform.SetParent(cha, false); g.transform.position = new Vector3(bh.center.x + 0.3f, bh.max.y + 0.06f, bh.center.z);
            var bc = g.AddComponent<BoxCollider>(); bc.size = new Vector3(0.35f, 0.14f, 0.3f);
            choDatChao = g.AddComponent<LocVatTam>();
            choDatChao.goiY = () => LocMa.dem == 2 && LocTui.CoMon("bat_chao") && !LocTui.Co("cung_chao") ? "[E] Đặt bát cháo" : "";
            choDatChao.bam = () =>
            {
                LocTui.Bo("bat_chao"); LocTui.Dat("cung_chao"); DatChaoTrenBan();
                LocTui.Noi(LocTui.Co("goi_thay") ? "Ông ấy dặn thế." : "Để đây vậy.");
            };
            if (LocTui.Co("cung_chao")) DatChaoTrenBan();
        }
        if (mgDanChen && vpKeo && vpChen3)
        {
            mgDanChen.canId = "lo_keo"; mgDanChen.canCo = new[] { "chen_3" }; mgDanChen.tieuHao = true; mgDanChen.tieuHaoCo = true;
            mgDanChen.cauThieu = "Chưa dán được. Phải có keo, và cái chén thứ ba";
            if (string.IsNullOrEmpty(mgDanChen.cauXong)) mgDanChen.cauXong = "Đặt chén anh Phong vào chỗ thứ ba.";
        }

        // ── Đêm 2: tờ giấy chép lời thầy (đưa vào túi sau cuộc gọi)
        vpGiayThay = MonAn(cha, "giay_loi_thay", "tờ giấy chép lời thầy", "Chép vội lúc nghe điện thoại, nét chữ mình.", 2, new[]
        {
            "<b>Chép vội lời ông thầy</b>\n\n— Dán lại hai cái chén dưới hầm. Keo ở cửa hàng.\n— Chén trên phòng thờ đem xuống, đặt chỗ thứ ba.\n— Bát cháo bưng xuống bàn thờ dưới hầm.\n— Giữ hương linh cữu đến sáng.\n— Tối mai mùng một. Ông ấy gửi đồ sang. Làm đúng như trong giấy.\n— Không mở hũ.\n\n<i>Ông ấy biết cả bát cháo.</i>"
        });

        // ── Đêm 3: gói hàng giấy dầu ngay trong cửa chính
        var goi = TimV("VT_GoiHang");
        if (goi)
        {
            kienHang = goi.gameObject.AddComponent<LocVatTam>();
            kienHang.goiY = () => LocMa.dem == 3 && !LocTui.Co("mo_kien") ? "[E] Mở gói hàng" : "";
            kienHang.bam = () => StartCoroutine(MoKienHang());
        }
        vpVanKhan = MonAn(cha, "van_khan", "bài khấn", "Bài khấn chữ son trong gói hàng. Đọc trước bàn thờ dưới hầm.", 3, new[]
        {
            "<color=#B0302A><b>Bài khấn</b>\n\nHôm nay mùng một tháng bảy.\nGia chủ là con trai trưởng họ Trần, xin dâng lễ mọn: cháo, gạo, muối, mía, nước trong đủ năm chén.\n\nXin người khuất mặt ở nơi này nhận lễ,\nở yên trong nhà,\ngiữ cho người trong nhà được bình yên, làm ăn có lộc.</color>",
            "<color=#B0302A>Nhà này còn người, còn hương khói, không bỏ.\nCon xin giữ lễ như cha con đã giữ.\nNăm năm hương khói không dứt.\n\nCẩn cáo.</color>",
        });
        vpGiayGoi = MonAn(cha, "giay_goi", "hai tờ giấy gói", "Giấy dầu gói hàng, mặt trong viết chữ son.", 3, new[]
        {
            "<color=#B0302A><b>Mùng một tháng bảy</b>\n\nGiờ Tý xuống hầm.\nBày mâm trên bàn thờ dưới ấy: cháo, gạo, muối, mía, nước đủ năm chén.\nĐọc bài khấn trước bàn thờ, đọc cho hết.\nThắp nhang. Giữ cho nhang cháy hết, không để tắt.</color>",
            "<color=#B0302A>Xong thì dán tờ niêm lên miệng hũ, miết cho kín.\n\nKhông được mở.\nKhông được đốt.\nNiêm xong là xong.</color>",
        });
        vpGiayNiem = MonAn(cha, "giay_niem", "tờ niêm", "Tờ giấy vàng, chữ son, dán lên miệng hũ.", 3, null);

        // ── Đêm 3: lời nhắn của mẹ trên bàn trang điểm phòng bố mẹ
        var loiNhan = TimV("LoiNhanMe_Giay");
        if (loiNhan)
            vpLoiNhanMe = Mon(loiNhan.gameObject, "loi_nhan_me", "lời nhắn của mẹ", LocVatPham.Kieu.Doc, "Mảnh giấy xé từ tờ lịch, nét chữ của mẹ.", 3,
                    new[] { "Chữ mẹ." }, new[]
                    {
                        "<i>Mặt sau một tờ lịch, viết vội.</i>\n\nKhôi,\n\nCon về thì đừng xuống hầm một mình.\nCó gì cứ xem quyển sách của mẹ trong tủ thờ, mẹ kẹp giấy sẵn rồi.\nĐừng nghe ai hết.\n\nMẹ thương."
                    }, chuaToi: "");

        // ── nghi lễ: vòng 3 chỉ mở sau vòng 2 · nhang tắt = Bad (b)
        if (mgGiuNhang && vpVanKhan && kienHang) mgGiuNhang.sauCo = "nl_doc_van";
        if (mgGiuNhang && mgGiuNhang.miniGame is LocMG_GiuLua gl) gl.thuaLaKet = true;
        LocMG_GiuLua.KhiTatHan = () => { if (!dangKet) StartCoroutine(BadB()); };

        // ── các kết
        LocNhim.SauGood = () => StartCoroutine(KetGood());
        LocNhim.SauBad = () => StartCoroutine(KetBad());
        LocNhim.SauEvil = () => StartCoroutine(KetEvil());
    }

    void ApDemV34(int d)
    {
        ChenHam(LocTui.Co("dan_chen"), false, false);
        if (kienHang) kienHang.gameObject.SetActive(d == 3 && !LocTui.Co("mo_kien") && LocTui.Co("kien_den"));   // [02/11] gói chỉ có sau khi giao (GiaoKien)
        if (mgBayMam && d == 3)
        {
            // chỉ đòi món nào người chơi đã có hoặc còn nhặt được ở Đêm 3 (món đã mất theo nhóm đồ Đêm 2 thì thôi — không kẹt)
            canMam.Clear();
            foreach (var (id, _) in DO_MAM)
            {
                LocVatPham vp = null; foreach (var v in FindObjectsByType<LocVatPham>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (v.id == id) { vp = v; break; }
                if (LocTui.CoMon(id) || (vp && vp.gameObject.activeInHierarchy)) canMam.Add(id);
            }
            var tong = new List<string>(canCoMam0 ?? new string[0]); foreach (var id in canMam) if (!tong.Contains(id)) tong.Add(id);
            mgBayMam.canCo = tong.ToArray(); mgBayMam.tieuHaoCo = true;
        }
        if (chaoTrenBan) chaoTrenBan.SetActive(d == 2);
    }

    // Đêm 2 chỉ hết khi đã nghe thầy, dán chén, đặt bát cháo (món nào không dựng được thì bỏ qua điều kiện của món ấy)
    bool Dem2DuViec() =>
        (!CoGoiThay || LocTui.Co("goi_thay"))
        && (!(mgDanChen && vpKeo && vpChen3) || LocTui.Co("dan_chen"))
        && (!(vpChao && choDatChao) || LocTui.Co("cung_chao"));
    bool CoGoiThay => dienThoai && cReo && amReo;

    // ───── Đêm 2
    void Dem2V34()
    {
        if (!daDanChen && LocTui.Co("dan_chen")) { daDanChen = true; ChenHam(true, false, false); }
        if (LocTui.Co("goi_2") && tGoi2 < 0) tGoi2 = Time.time;
        // [02/11] v3.3: thầy KHÔNG tự gọi tới nữa — Khôi đọc sổ tới bìa sau (số thầy Đồng 826 315), qua đoạn hoảng loạn, rồi tự gọi (ChoGoiThay / GoiThay)
        // [03/11 tối] chơi thử: không ai biết phải tự gọi → thiếu "goi_thay" → Đêm 2 không hiện "Thắp nén mới", không đi ngủ được (kẹt cả đêm).
        //  Dự phòng giống bản trước 02/11: qua hoảng loạn 60 s mà chưa gọi thì thầy gọi tới (nhấc máy = cùng cuộc gọi). Tự gọi vẫn được như cũ.
        if (LocNhim.daHoangLoan && tHoangLoan < 0) tHoangLoan = Time.time;
        if (!CoGoiThay || !LocNhim.daHoangLoan || LocTui.Co("goi_thay") || dangReo || dangCanh || LocNhim.dangCanh || LocFirstPerson.khoa || LocTui.Mo) return;
        var p = fp.transform.position;
        if (hamPhong.Contains(p) || hopThang.Contains(p) || Time.time - tHoangLoan < 60f || Time.time < tReoThay || KC(dienThoai) > 16f) return;
        tReoThay = Time.time + 55f;   // không nhấc thì một lúc sau reo lại
        StartCoroutine(Reo(10, true, CuocGoiThay));
    }
    float tHoangLoan = -1f;

    // [02/11] [E] Gọi số ở bìa sổ: điện thoại phòng khách, Đêm 2, sau khi đã thấy số ở bìa sau sổ của bố và qua đoạn hoảng loạn
    bool ChoGoiThay() => LocMa.dem == 2 && CoGoiThay && !LocTui.Co("goi_thay") && LocTui.Co("so_bo_het") && LocNhim.daHoangLoan && !dangReo && !dangCanh;
    IEnumerator GoiThay()
    {
        if (!ChoGoiThay()) yield break;
        dangCanh = true; LocFirstPerson.khoa = true; LocMa.tamDung = true;
        LocFirstPerson.PhuDe("Tám. Hai. Sáu. Ba. Một. Năm.", 2.4f); yield return new WaitForSeconds(2.6f);   // quay số
        var hoi = HoiChuong();
        for (int i = 0; i < 3; i++) { amReo.PlayOneShot(hoi, 0.6f); yield return new WaitForSeconds(hoi.length + 0.4f); }   // chuông chờ đầu dây bên kia
        yield return CuocGoiThay();
        LocMa.tamDung = false; LocFirstPerson.khoa = false; dangCanh = false;
    }
    // tiếng "tuuu" chờ bắt máy (425 Hz, 1 s) — tổng hợp tạm; có file thì đặt Resources/LOC_V33/DienThoai_Cho
    static AudioClip hoiChuong;
    static AudioClip HoiChuong()
    {
        if (hoiChuong) return hoiChuong;
        var f = Resources.Load<AudioClip>("LOC_V33/DienThoai_Cho"); if (f) return hoiChuong = f;
        const int HZ = 22050; int n = HZ * 2; var d = new float[n];
        for (int i = 0; i < n; i++) { float t = i / (float)HZ; d[i] = t < 1f ? Mathf.Sin(2 * Mathf.PI * 425f * t) * 0.35f * Mathf.Clamp01(t * 40f) * Mathf.Clamp01((1f - t) * 40f) : 0f; }
        hoiChuong = AudioClip.Create("DienThoai_Cho", n, 1, HZ, false); hoiChuong.SetData(d, 0); hoiChuong.hideFlags = HideFlags.DontUnloadUnusedAsset;
        return hoiChuong;
    }

    // [02/11] thứ tự + mấy câu theo v3.3 (walkthrough Đêm 2 ⚠️): "Ông ấy đâu?" · im bảy giây · "Nhà còn ai lớn không?" · "Không mở hũ." tách riêng.
    //  Phần giữa vẫn là THOẠI TẠM của nhóm — sửa thẳng ở đây khi có toàn văn v3.3.
    IEnumerator CuocGoiThay()
    {
        if (cRe) { amReo.PlayOneShot(cRe, 0.5f); yield return LocFirstPerson.ChoThoai(1.2f); }
        yield return Thoai((THAY, "Ông ấy đâu?"));
        LocFirstPerson.PhuDe("KHÔI: ...", 7f, false); yield return LocFirstPerson.ChoThoai(7f);   // bảy giây im
        yield return Thoai((THAY, "Nhà còn ai lớn không?"),
            ("KHÔI", "...Cháu. Cháu là Khôi."),
            (THAY, "Nghe cho kỹ, tôi chỉ nói một lần."),
            (THAY, "Dưới hầm có hai cái chén vỡ. Dán lại cho lành. Keo ở cửa hàng nhà anh."),
            (THAY, "Cái chén trên phòng thờ, đem xuống đặt vào chỗ thứ ba."),
            (THAY, "Bát cháo bác anh nấu, bưng xuống để lên bàn thờ dưới ấy."),
            (THAY, "Giữ hương trên linh cữu đến sáng."),
            (THAY, "Tối mai mùng một. Tôi gửi đồ sang. Làm đúng như trong giấy."),
            (THAY, "Làm đúng thì con bé khỏi."),
            (THAY, "Không mở hũ."));
        if (cTut) { amReo.PlayOneShot(cTut, 0.8f); yield return LocFirstPerson.ChoThoai(cTut.length); }
        if (LocFirstPerson.boQua) amReo.Stop();
        LocTui.Dat("goi_thay");
        yield return new WaitForSeconds(0.6f);
        if (vpGiayThay) LocTui.Them(vpGiayThay);
        LocTui.Noi("Chép lại kẻo quên.", "...Sao ông ấy biết bác nấu cháo.");
    }

    void DatChaoTrenBan()
    {
        if (chaoTrenBan) { chaoTrenBan.SetActive(LocMa.dem == 2); return; }
        if (!choDatChao) return;
        if (!vpChao) return;
        chaoTrenBan = BanSaoHinh(vpChao.transform, "BatChao_TrenBanThoHam", choDatChao.transform.parent);   // bản sao hình bát cháo vừa bưng xuống
        chaoTrenBan.transform.SetPositionAndRotation(choDatChao.transform.position + Vector3.down * 0.06f, Quaternion.Euler(0, vpChao.transform.eulerAngles.y, 0));   // choDatChao cao 6 cm trên mặt bàn thờ; gốc bát ở đáy
        chaoTrenBan.SetActive(LocMa.dem == 2);
    }

    // ───── Đêm 3
    IEnumerator MoKienHang()
    {
        LocTui.Dat("mo_kien");
        dangCanh = true; LocFirstPerson.khoa = true; LocMa.tamDung = true;
        if (kienHang && fp) { var t0 = fp.cam.position + fp.cam.forward; var den = kienHang.transform.position; for (float t = 0; t < 1; t += Time.deltaTime / 0.7f) { fp.DatTuThe(fp.transform.position, Vector3.Lerp(t0, den, t * t * (3 - 2 * t))); yield return null; } }
        LocFirstPerson.PhuDe("Giấy dầu. Buộc lạt.", 2.2f); yield return new WaitForSeconds(2.4f);
        LocFirstPerson.PhuDe("Không ai gõ cửa. Không ai thấy ai mang tới.", 2.8f); yield return new WaitForSeconds(3.0f);
        if (vpVanKhan) LocTui.Them(vpVanKhan);
        if (vpGiayGoi) LocTui.Them(vpGiayGoi);
        if (vpGiayNiem) LocTui.Them(vpGiayNiem);
        if (kienHang) kienHang.gameObject.SetActive(false);
        LocFirstPerson.PhuDe("Nhặt được: bài khấn, hai tờ giấy gói, tờ niêm", 2.6f, false);
        LocFirstPerson.khoa = false; LocMa.tamDung = false; dangCanh = false;
    }

    void Dem3V34()
    {
        if (dangKet) return;
        // [02/11] mâm cúng: câu báo thiếu theo đúng món còn thiếu
        if (mgBayMam && !LocTui.Co("nl_bay_mam") && canMam.Count > 0)
        {
            var thieu = new List<string>(); foreach (var (id, ten) in DO_MAM) if (canMam.Contains(id) && !LocTui.CoMon(id)) thieu.Add(ten);
            var cau = thieu.Count == 0 ? mgBayMam.cauThieu : "Mâm còn thiếu " + string.Join(", ", thieu) + ".";
            if (cau != thieuCu) { thieuCu = cau; if (thieu.Count > 0) mgBayMam.cauThieu = cau; }
        }
        // [02/11] walkthrough Đêm 3: kiện hàng được GIAO (chuông cổng + xe máy + cái đài) chứ không nằm sẵn trong cửa
        if (kienHang && !LocTui.Co("kien_den") && !LocTui.Co("mo_kien") && !dangGiaoKien && !LocFirstPerson.khoa && !LocMenu.DangChe)
        { tChoKien += Time.deltaTime; if (tChoKien > 6f) StartCoroutine(GiaoKien()); }
        var p = fp.transform.position; bool trongHam = hamPhong.Contains(p);
        if (LocTui.Co("nl_bay_mam") && !daNhacBayMam && vpVanKhan) { daNhacBayMam = true; if (!LocTui.Co("nl_doc_van")) LocTui.Noi(LocTui.CoMon("van_khan") ? "Giờ đọc bài khấn." : "Còn bài khấn trong gói hàng."); }
        // vòng 2: lật tới trang cuối bài khấn, đứng trong hầm, sau khi bày mâm
        if (vpVanKhan && !LocTui.Co("nl_doc_van") && LocTui.DangDocId == "van_khan" && LocTui.DangTrangCuoi)
        {
            if (LocTui.Co("nl_bay_mam") && trongHam) { LocTui.Dat("nl_doc_van"); LocFirstPerson.PhuDe("Khấn xong.", 2f); }
            else if (!daNhacDocHam) { daNhacDocHam = true; LocFirstPerson.PhuDe(LocTui.Co("nl_bay_mam") ? "Phải đọc trước bàn thờ dưới hầm." : "Bày mâm xong đã rồi mới khấn.", 2.6f); }
        }
        if (LocTui.DangDocId != "van_khan") daNhacDocHam = false;
        if (LocTui.Co("nl_giu_nhang") && !LocTui.Co("da_chon_hu") && !daNhacKhan) { daNhacKhan = true; LocTui.Noi("Nhang cháy hết rồi.", "Còn cái hũ."); }
    }

    // ───── [02/11] giao kiện hàng đầu Đêm 3: xe máy chạy tới ngoài cổng (đài trên xe mở nhạc), bấm chuông hai lần, gói nằm trong cửa, xe chạy đi
    bool dangGiaoKien; float tChoKien;
    static readonly Vector3 NGOAI_CONG = new(3.8f, 1.1f, -6.4f);
    IEnumerator GiaoKien()
    {
        dangGiaoKien = true;
        var bn = GetComponent<LocBanNgay>();
        var xe = new GameObject("Kien_XeMay"); xe.transform.position = NGOAI_CONG + Vector3.left * 14f;
        AudioSource Nguon(AudioClip c, float vol, float xa)
        {
            var a = xe.AddComponent<AudioSource>(); a.clip = c; a.loop = true; a.volume = vol; a.spatialBlend = 1f; a.minDistance = 2.5f; a.maxDistance = xa; a.rolloffMode = AudioRolloffMode.Linear;
            if (c) a.Play(); return a;
        }
        var may = Nguon(bn ? bn.xeMay : null, 0.85f, 40f);
        var dai = Nguon(Resources.Load<AudioClip>("LOC_Radio_Nhac"), 0.45f, 32f);   // cái đài trên xe
        var lp = xe.AddComponent<AudioLowPassFilter>(); lp.cutoffFrequency = 2600f;
        var a0 = xe.transform.position;
        for (float t = 0; t < 1; t += Time.deltaTime / 3.2f) { xe.transform.position = Vector3.Lerp(a0, NGOAI_CONG, 1 - (1 - t) * (1 - t)); yield return null; }
        if (may) may.volume = 0.45f;   // tắt máy để nổ ga nhỏ
        yield return new WaitForSeconds(1.0f);
        var chuong = ChuongCong();
        for (int i = 0; i < 2; i++) { AudioSource.PlayClipAtPoint(chuong, NGOAI_CONG + Vector3.forward * 0.8f, 1f); yield return new WaitForSeconds(1.6f); }
        LocTui.Dat("kien_den");
        if (kienHang) kienHang.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        if (may) may.volume = 0.9f;
        var a1 = xe.transform.position; var a2 = NGOAI_CONG + Vector3.right * 30f;
        for (float t = 0; t < 1; t += Time.deltaTime / 5.5f) { xe.transform.position = Vector3.Lerp(a1, a2, t * t); if (may) may.volume = Mathf.Lerp(0.9f, 0.1f, t); if (dai) dai.volume = Mathf.Lerp(0.45f, 0.05f, t); yield return null; }
        Destroy(xe);
        dangGiaoKien = false;
    }
    // chuông điện cổng "kính coong" (chưa có file âm thanh — tổng hợp tạm; có file thì đặt Resources/LOC_V33/Chuong_Cong)
    static AudioClip chuongCong;
    static AudioClip ChuongCong()
    {
        if (chuongCong) return chuongCong;
        var f = Resources.Load<AudioClip>("LOC_V33/Chuong_Cong"); if (f) return chuongCong = f;
        const int HZ = 22050; int n = (int)(1.5f * HZ); var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)HZ, x = 0;
            foreach (var (bd, tan) in new[] { (0f, 659.3f), (0.55f, 523.3f) })
                if (t >= bd) { float u = t - bd; x += Mathf.Exp(-u * 3.2f) * (Mathf.Sin(2 * Mathf.PI * tan * u) + 0.35f * Mathf.Sin(2 * Mathf.PI * tan * 2.01f * u) + 0.12f * Mathf.Sin(2 * Mathf.PI * tan * 3.03f * u)); }
            d[i] = Mathf.Clamp(x * 0.45f, -1, 1);
        }
        chuongCong = AudioClip.Create("Chuong_Cong", n, 1, HZ, false); chuongCong.SetData(d, 0); chuongCong.hideFlags = HideFlags.DontUnloadUnusedAsset;
        return chuongCong;
    }

    // ───── chỗ rẽ cuối (LocHuCot gọi)
    public void NiemHu(LocHuCot hu)
    {
        if (dangCanh || dangKet) return;
        if (vpGiayNiem && !LocTui.CoMon("giay_niem")) { LocTui.Noi("Tờ niêm của ông ấy đâu rồi."); return; }
        StartCoroutine(Niem(hu));
    }
    public void XeNiem(LocHuCot hu) { if (!dangCanh && !dangKet) StartCoroutine(Xe(hu)); }

    void VaoKet() { dangKet = true; dangCanh = true; LocFirstPerson.khoa = true; LocMa.tamDung = true; LocMa.maTucGian = false; }

    IEnumerator Niem(LocHuCot hu)
    {
        VaoKet(); LocTui.Dat("da_chon_hu"); LocTui.Dat("chon_niem");
        yield return hu.CanhNiem(fp.cam);
        LocTui.Bo("giay_niem");
        bool docSach = LocTui.Co("doc:sach_gia_le");
        yield return new WaitForSeconds(0.8f);
        yield return CongSang(docSach);
        if (docSach)   // Evil: chân cầu thang, tiếng gọi từ trên gác
        {
            if (LocNhim.I) foreach (var r in LocNhim.I.GetComponentsInChildren<Renderer>(true)) r.enabled = false;   // v3.3: em không còn nằm trong phòng
            fp.DatTuThe(new Vector3(1.45f, 0.05f, 6.75f), new Vector3(1.45f, 3.4f, 9.6f));
            yield return Den(1, 0, 1.2f);
            yield return new WaitForSeconds(1.5f);
            yield return EvilCauThang();   // [02/11] v3.3: đi được tới chân cầu thang nhưng khựng ở bậc thứ nhất, tiếng chén vỡ
        }
        else { yield return ThayNhanHu(hu); TroiToi(); if (LocNhim.I) LocNhim.I.CanhBad(); else StartCoroutine(KetBad()); }
    }

    IEnumerator Xe(LocHuCot hu)
    {
        VaoKet(); LocTui.Dat("da_chon_hu"); LocTui.Dat("chon_xe");
        var camLP = fp.cam.localPosition; var camLR = fp.cam.localRotation;
        yield return hu.CanhXe(fp.cam);
        yield return Den(0, 1, 0.3f);   // đứng dậy, nhìn tường bùa
        fp.cam.localPosition = camLP; fp.cam.localRotation = camLR;
        fp.DatTuThe(fp.transform.position, new Vector3(hamPhong.center.x, hamPhong.min.y + 1.4f, hamPhong.min.z - 0.3f));
        yield return Den(1, 0, 0.4f);
        // [02/11] walkthrough: thiếu câu của Khôi trước khi bùa rơi (kịch bản v3.3)
        LocFirstPerson.PhuDe("KHÔI: Đi đi. Nhà tôi giữ lâu quá rồi.", 3f, false);
        yield return new WaitForSeconds(3.4f);
        yield return BuaRoi();
        yield return new WaitForSeconds(1.2f);
        LocFirstPerson.PhuDe("...: Cảm ơn.", 2.6f, false);   // [02/11] giọng con vong, KHÔNG tên (v3.3 ghi "…:") — trước ghi nhầm NHÍM
        yield return new WaitForSeconds(3.2f);
        yield return Den(LocFirstPerson.den, 1, 1.4f);
        fp.cam.localPosition = camLP; fp.cam.localRotation = camLR;   // CanhXe để camera ở chỗ hũ
        yield return new WaitForSeconds(1.0f);
        if (LocNhim.I) LocNhim.I.CanhGood(); else StartCoroutine(KetGood());
    }

    // bùa trên tường hầm bong ra, rơi xuống nền (sách gia lễ: hết giấy trấn thì vong không còn bị buộc)
    IEnumerator BuaRoi()
    {
        var ds = new List<Transform>();
        foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name.StartsWith("TuongBuaMau")) ds.Add(t);
        if (ds.Count == 0) yield break;
        float san = hamPhong.min.y + 0.22f;   // hamPhong: đáy hộp −2,5, nền hầm −2,3 → rơi sát nền
        var p0 = new Vector3[ds.Count]; var q0 = new Quaternion[ds.Count]; var tre = new float[ds.Count];
        for (int i = 0; i < ds.Count; i++) { p0[i] = ds[i].position; q0[i] = ds[i].rotation; tre[i] = Random.Range(0f, 1.4f); }
        for (float t = 0; t < 3.2f; t += Time.deltaTime)
        {
            for (int i = 0; i < ds.Count; i++)
            {
                if (!ds[i]) continue;
                float e = Mathf.Clamp01((t - tre[i]) / 1.6f); e = e * e;
                var vao = Vector3.ProjectOnPlane(hamPhong.center - p0[i], Vector3.up).normalized;
                ds[i].position = new Vector3(p0[i].x, Mathf.Lerp(p0[i].y, san, e), p0[i].z) + vao * 0.35f * e;
                ds[i].rotation = Quaternion.AngleAxis(80f * e, Vector3.Cross(Vector3.up, vao)) * q0[i];   // gốc tờ bùa ở mép dưới: mép trên đổ vào trong hầm
            }
            yield return null;
        }
    }

    // [29/10] Bad đường (c): sáng mùng một, trong sân sát cửa ngách cổng — một bàn tay thò vào ngửa ra, Khôi đưa cái hũ, tay khép lại rút đi.
    //  Thầy không bao giờ hiện hình: chỉ bàn tay (đặc tả tay mục 3). Cửa ngách mở hẳn (TL-H3 chưa chốt khe hé 25 cm).
    IEnumerator ThayNhanHu(LocHuCot hu)
    {
        if (!LocTay.I || !LocTay.I.tayThay) yield break;
        foreach (var d in FindObjectsByType<LocDoor>(FindObjectsSortMode.None)) if (d.name.StartsWith("CuaNgachCong") && !d.DangMo) d.Toggle();
        // lòng bàn tay thầy: trong ô cửa ngách (x 5,44–6,34 · z −5,25), thò vào sân ~20 cm, chếch từ sau trụ cổng phải (x 6,34–6,66) — khuỷu và người khuất sau trụ
        var cho = new Vector3(6.05f, 1.22f, -5.05f); var vao = new Vector3(-0.55f, 0f, 0.83f);
        fp.DatTuThe(new Vector3(5.72f, 0.05f, -4.62f), new Vector3(6.05f, 1.05f, -5.2f));
        var ban = hu && hu.than ? BanSaoHinh(hu.than, "HuCot_DuaThay", null) : null;
        if (ban) ban.SetActive(false);
        yield return Den(1, 0, 1.2f);
        yield return new WaitForSeconds(0.8f);
        yield return LocTay.TayThayNhanHu(cho, vao, ban);
        yield return new WaitForSeconds(1.0f);
        yield return Den(0, 1, 1.0f);
        if (ban) Destroy(ban);
    }

    // cổng sáng mùng một (Bad c / Evil): ra ngoài cổng, trời sáng
    IEnumerator CongSang(bool evil)
    {
        yield return Den(LocFirstPerson.den, 1, 1.2f);
        TroiSang();
        fp.DatTuThe(dem1Dung, dem1Nhin);
        yield return new WaitForSeconds(0.8f);
        yield return Den(1, 0, 1.6f);
        yield return new WaitForSeconds(1.0f);
        // [02/11] độc thoại cổng theo v3.3 (bản tạm cũ: "Sáng rồi." / "Không có chuyện gì cả."):
        //  Bad (c) — đã đọc lời nhắn của mẹ: "Mẹ bảo không được niêm." · chưa đọc: "Ông ấy bảo đem lên chùa."
        //  Evil — không đưa hũ (giữ lại): chỉ "Mùng một." + "Sáng rồi." (câu "Làm đúng như ông ấy dặn." cũ trái nghĩa: thầy muốn đem đi)
        LocFirstPerson.PhuDe("Mùng một.", 2.2f); yield return new WaitForSeconds(2.6f);
        if (evil) { LocFirstPerson.PhuDe("Sáng rồi.", 2.2f); yield return new WaitForSeconds(2.8f); }
        else { LocFirstPerson.PhuDe(LocTui.Co("doc:loi_nhan_me") ? "Mẹ bảo không được niêm." : "Ông ấy bảo đem lên chùa.", 2.8f); yield return new WaitForSeconds(3.4f); }
        yield return Den(0, 1, 1.2f);
    }
    Light nangSang; Color amb0, fog0; bool daSang;
    void TroiToi()
    {
        if (!daSang) return; daSang = false;
        RenderSettings.ambientLight = amb0; RenderSettings.fogColor = fog0;
        if (nangSang) Destroy(nangSang.gameObject);
    }
    void TroiSang()
    {
        if (!daSang) { amb0 = RenderSettings.ambientLight; fog0 = RenderSettings.fogColor; daSang = true; }
        RenderSettings.ambientLight = new Color(0.78f, 0.8f, 0.82f);
        RenderSettings.fogColor = new Color(0.78f, 0.8f, 0.8f);
        if (!nangSang)
        {
            nangSang = new GameObject("KetThuc_NangSang").AddComponent<Light>();
            nangSang.type = LightType.Directional; nangSang.color = new Color(1f, 0.95f, 0.86f); nangSang.intensity = 1.8f; nangSang.shadows = LightShadows.Soft;
            nangSang.transform.rotation = Quaternion.Euler(32f, 18f, 0f);
        }
    }

    // [02/11] Evil theo walkthrough (v3.3): tiếng gọi từ trên gác hai lần → người chơi đi được, bước lên bậc thứ nhất thì khựng lại,
    //  dưới hầm có tiếng chén vỡ → điện thoại. Không bước lên thì ~25 s sau cũng khựng (để cảnh không kẹt).
    bool thaKhoaKet;   // đang trong phần kết mà cho người chơi đi (LocCotTruyen.Update không ép khoá)
    IEnumerator EvilCauThang()
    {
        LocMenu.DatKet("evil");
        LocFirstPerson.PhuDe("NHÍM: Anh Khôi ơi...", 2.6f, false); yield return new WaitForSeconds(5f);
        LocFirstPerson.PhuDe("NHÍM: Anh Khôi ơi...", 2.6f, false); yield return new WaitForSeconds(2.6f);
        var p0 = fp.transform.position;
        thaKhoaKet = true; LocFirstPerson.khoa = false;
        for (float t = 0; t < 25f; t += Time.deltaTime)
        {
            var p = fp.transform.position;
            if (p.y > p0.y + 0.1f || p.z > p0.z + 0.45f) break;   // chân đã đặt lên bậc thứ nhất (thang đi lên theo +z)
            yield return null;
        }
        thaKhoaKet = false; LocFirstPerson.khoa = true;
        var cam = fp.cam; var lp = cam.localPosition;
        for (float t = 0; t < 0.25f; t += Time.deltaTime) { cam.localPosition = lp + Vector3.down * 0.05f * Mathf.Sin(t / 0.25f * Mathf.PI); yield return null; }   // khựng
        cam.localPosition = lp;
        var chen = GameObject.Find("Bo_Chen_Tren_De");
        if (LocMa.I && LocMa.I.chenVo) AudioSource.PlayClipAtPoint(LocMa.I.chenVo, chen ? chen.transform.position : fp.transform.position + Vector3.down * 2f, 1f);
        yield return new WaitForSeconds(2.2f);
        StartCoroutine(KetEvil());
    }

    // ───── thân các kết
    IEnumerator BadB()
    {
        VaoKet(); LocTui.Dat("nhang_tat");
        LocFirstPerson.PhuDe("...Tắt rồi.", 2.2f);
        yield return new WaitForSeconds(2.6f);
        if (LocNhim.I) LocNhim.I.CanhBad(); else StartCoroutine(KetBad());
    }

    IEnumerator KetGood()
    {
        dangKet = true;
        yield return new WaitForSeconds(0.5f);
        while (LocNhim.dangCanh) yield return null;   // cảnh sáng hôm sau tự cắt đen
        yield return Den(LocFirstPerson.den, 1, 0.8f);
        LocFirstPerson.chuGiua = "Mười ba năm, người mang nó tới không cho nó đi.";
        yield return new WaitForSeconds(3.5f);
        if (cReo && amReo) { amReo.spatialBlend = 0f; for (int i = 0; i < 3; i++) { amReo.PlayOneShot(cReo, 0.6f); yield return new WaitForSeconds(Mathf.Max(1.5f, cReo.length)); } }   // điện thoại reo… không ai nhấc
        else yield return new WaitForSeconds(3f);
        VeMenu();
    }
    void VeMenu() { LocFirstPerson.chuGiua = null; TroiToi(); dangKet = false; dangCanh = false; LocMenu.VeMenuChinh(); }

    IEnumerator KetBad()
    {
        dangKet = true;
        LocFirstPerson.khoa = true; LocMa.tamDung = true; LocFirstPerson.den = 1;
        yield return new WaitForSeconds(0.3f);
        while (LocNhim.dangCanh) yield return null;
        TroiToi();
        yield return new WaitForSeconds(0.9f);
        // hầm: năm chỗ chén, cái thứ năm vỡ
        var chen = GameObject.Find("Bo_Chen_Tren_De");
        if (chen && fp)
        {
            var b = BaoV(chen.transform);
            var dung = b.center + new Vector3(0, 0.3f, 0.8f);
            fp.DatTuThe(dung + Vector3.down * (fp.cam ? fp.cam.localPosition.y : 1.52f), b.center);
            var den = new GameObject("Ket_DenBanThoHam").AddComponent<Light>(); den.type = LightType.Point; den.range = 2.4f; den.intensity = 1.1f; den.color = new Color(1f, 0.6f, 0.3f);
            den.transform.position = b.center + Vector3.up * 0.9f;
            bool duongC = LocTui.Co("chon_niem") && !LocTui.Co("doc:sach_gia_le");   // Bad (c): cái hũ đã đưa qua cửa ngách
            var hu = duongC ? FindAnyObjectByType<LocHuCot>() : null;
            if (hu) foreach (var r in (hu.than ? hu.than : hu.transform).GetComponentsInChildren<Renderer>()) r.enabled = false;
            yield return Den(1, 0, 1.0f);
            yield return new WaitForSeconds(1.6f);
            ChenHam(LocTui.Co("dan_chen"), false, true);
            if (LocMa.I && LocMa.I.chenVo) AudioSource.PlayClipAtPoint(LocMa.I.chenVo, b.center, 1f);
            yield return new WaitForSeconds(2.4f);
            if (hu) yield return ChoHuTrong(hu, den);   // [02/11] walkthrough Bad (c): khung hầm, chỗ cái hũ trống, vòng bụi tròn
            yield return Den(0, 1, 0.8f);
            Destroy(den.gameObject);
        }
        LocFirstPerson.chuGiua = "Bốn năm sau.";
        yield return new WaitForSeconds(3f);
        LocFirstPerson.chuGiua = null;
        if (chen && fp) yield return BonNamSau(chen.transform);   // [02/11] cảnh thật: Khôi rót nước mỗi sáng, chén bốn nứt dài thêm
        LocFirstPerson.den = 1;
        LocFirstPerson.chuGiua = "Anh đã ở trong căn nhà này bốn năm.";
        yield return new WaitForSeconds(6f);
        VeMenu();
    }

    // [02/11] Evil "năm năm sau" (walkthrough ❌): ngoài cổng ban ngày, nhà buôn bán lại · dưới hầm năm cái chén mới lành lặn, hũ niêm mới ·
    //  đêm, cuối hành lang tầng một: nửa cái đầu lấp ló ở mép cửa (LocMa.ThuLo — không có chỗ hợp lệ thì bỏ khung này)
    IEnumerator NamNamSau()
    {
        TroiSang();
        fp.DatTuThe(dem1Dung, dem1Nhin);
        yield return Den(1, 0, 1.2f); yield return new WaitForSeconds(2.4f); yield return Den(0, 1, 0.8f);
        TroiToi();
        var bo = GameObject.Find("Bo_Chen_Tren_De");
        if (bo)
        {
            // năm chén mới: lưới Bo_Chen_Tren_De không còn chén liền (LocHouseBuilder_Sua0111 mục 12) → dựng năm chén lành đúng chỗ năm chén cũ
            var tt = TimV("ChenHam_TrangThai");
            bool coSan = false;
            foreach (var t in bo.GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith("ChenNguyen_") && t.GetComponent<Renderer>()) { t.gameObject.SetActive(true); coSan = true; }
            if (!coSan && tt) LocNuocChen.ChenMoi(tt);
            if (tt) tt.gameObject.SetActive(false);
            var b = BaoV(bo.transform);
            var dung = b.center + new Vector3(0, 0.3f, 0.8f);
            fp.DatTuThe(dung + Vector3.down * (fp.cam ? fp.cam.localPosition.y : 1.52f), b.center);
            var den = new GameObject("Ket_DenNamNam").AddComponent<Light>(); den.type = LightType.Point; den.range = 2.4f; den.intensity = 1.1f; den.color = new Color(1f, 0.62f, 0.32f);
            den.transform.position = b.center + Vector3.up * 0.9f;
            yield return Den(1, 0, 1.0f); yield return new WaitForSeconds(2.8f); yield return Den(0, 1, 0.8f);
            Destroy(den.gameObject);
        }
        if (LocMa.I)
        {
            fp.DatTuThe(new Vector3(3.0f, 0.05f, 9.0f), new Vector3(1.0f, 1.4f, 16f));   // sảnh sau tầng một nhìn về giếng trời / kho / WC
            yield return Den(1, 0, 1.0f);
            thaKhoaKet = true; LocFirstPerson.khoa = true; LocMa.tamDung = false;
            bool lo = false;
            try { var m = typeof(LocMa).GetMethod("ThuLo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); lo = m != null && (bool)m.Invoke(LocMa.I, new object[] { -1 }); }
            catch (System.Exception e) { Debug.LogWarning("[LOC] Evil năm năm sau: không gọi được lấp ló — " + e.Message); }
            yield return new WaitForSeconds(lo ? 3.6f : 1.6f);
            LocMa.tamDung = true; thaKhoaKet = false;
            yield return Den(0, 1, 0.8f);
        }
    }

    // khối hình đơn giản cho các cảnh kết (không va chạm, không đổ bóng, vật liệu Lit một màu lấy theo vật liệu sẵn có trong cảnh)
    static Material vlKhoiGoc;
    static GameObject Khoi(Transform cha, string ten, PrimitiveType kieu, Vector3 p, Vector3 co, Color mau)
    {
        var g = GameObject.CreatePrimitive(kieu); g.name = ten;
        var col = g.GetComponent<Collider>(); if (col) DestroyImmediate(col);
        g.transform.SetParent(cha, false); g.transform.position = p; g.transform.localScale = co;
        var r = g.GetComponent<Renderer>();
        if (!vlKhoiGoc) { var sh = Shader.Find("Universal Render Pipeline/Lit"); if (sh) vlKhoiGoc = new Material(sh); }
        if (vlKhoiGoc) { r.sharedMaterial = vlKhoiGoc; var mpb = new MaterialPropertyBlock(); mpb.SetColor("_BaseColor", mau); r.SetPropertyBlock(mpb); }
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return g;
    }

    // Bad (c): dưới gầm bàn thờ hầm, chỗ cái hũ để mười ba năm giờ trống — một vòng sạch giữa lớp bụi
    IEnumerator ChoHuTrong(LocHuCot hu, Light den)
    {
        var goc = hu.than ? hu.than : hu.transform;
        var b = BaoV(goc);
        float R = Mathf.Clamp(Mathf.Max(b.extents.x, b.extents.z), 0.08f, 0.3f), san = b.min.y;
        var cha = new GameObject("Ket_ChoHuTrong").transform;
        var bui = Khoi(cha, "Bui", PrimitiveType.Cylinder, new Vector3(b.center.x, san + 0.002f, b.center.z), new Vector3(R * 2f + 0.26f, 0.001f, R * 2f + 0.26f), new Color(0.42f, 0.39f, 0.34f));
        var sach = Khoi(cha, "VongSach", PrimitiveType.Cylinder, new Vector3(b.center.x, san + 0.004f, b.center.z), new Vector3(R * 2f, 0.001f, R * 2f), new Color(0.13f, 0.12f, 0.11f));
        var ra = Vector3.ProjectOnPlane(fp.transform.position - b.center, Vector3.up); ra = ra.sqrMagnitude > 1e-4f ? ra.normalized : Vector3.forward;
        var mat = b.center + ra * 0.75f + Vector3.up * 0.45f;
        yield return Den(0, 1, 0.5f);
        fp.cam.SetPositionAndRotation(mat, Quaternion.LookRotation(new Vector3(b.center.x, san, b.center.z) - mat, Vector3.up));
        if (den) den.transform.position = b.center + Vector3.up * 0.6f + ra * 0.3f;
        yield return Den(1, 0, 0.6f);
        yield return new WaitForSeconds(2.6f);
        yield return Den(0, 1, 0.6f);
        Destroy(cha.gameObject);
    }

    // "bốn năm sau": buổi sáng dưới hầm, tay Khôi rót nước vào cái chén thứ tư — vết nứt trên chén đã dài thêm
    IEnumerator BonNamSau(Transform boChen)
    {
        var tt = TimV("ChenHam_TrangThai");
        var c4 = tt ? tt.Find("Chen4") : null;
        var bc = c4 && c4.gameObject.activeInHierarchy ? BaoV(c4) : BaoV(boChen);
        var tam = bc.center;
        var ra = Vector3.ProjectOnPlane(fp.transform.position - tam, Vector3.up); ra = ra.sqrMagnitude > 1e-4f ? ra.normalized : Vector3.forward;
        var cha = new GameObject("Ket_BonNamSau").transform;
        if (c4 && c4.gameObject.activeInHierarchy)
        {
            // vết nứt dài thêm: đường gãy khúc tối trên thành chén phía người nhìn, từ miệng xuống gần đáy
            float r = Mathf.Max(bc.extents.x, bc.extents.z) * 0.98f, h = bc.size.y;
            var phai = Vector3.Cross(Vector3.up, ra);
            var diem = new[] { 0.97f, 0.74f, 0.52f, 0.28f, 0.1f };
            var lech = new[] { 0f, 0.012f, -0.006f, 0.01f, 0.002f };
            for (int i = 0; i + 1 < diem.Length; i++)
            {
                var a = tam + ra * r + phai * lech[i] + Vector3.up * (bc.min.y - tam.y + h * diem[i]);
                var bb = tam + ra * r + phai * lech[i + 1] + Vector3.up * (bc.min.y - tam.y + h * diem[i + 1]);
                var g = Khoi(cha, "VetNut", PrimitiveType.Cube, (a + bb) / 2f, new Vector3(0.0018f, Vector3.Distance(a, bb), 0.0018f), new Color(0.05f, 0.04f, 0.035f));
                g.transform.rotation = Quaternion.FromToRotation(Vector3.up, bb - a);
            }
        }
        var den = new GameObject("Ket_DenSang").AddComponent<Light>(); den.transform.SetParent(cha, false);
        den.type = LightType.Point; den.range = 2.2f; den.intensity = 0.9f; den.color = new Color(0.95f, 0.85f, 0.7f);
        den.transform.position = tam + Vector3.up * 0.9f + ra * 0.4f;
        var mat = tam + ra * 0.42f + Vector3.up * 0.3f;
        fp.cam.SetPositionAndRotation(mat, Quaternion.LookRotation(tam - mat, Vector3.up));
        yield return Den(1, 0, 1.0f);
        yield return new WaitForSeconds(0.6f);
        var bn = GetComponent<LocBanNgay>();
        if (bn && bn.rotTra) AudioSource.PlayClipAtPoint(bn.rotTra, tam, 0.6f);
        if (LocTay.I) yield return LocTay.RotTra(new Vector3(tam.x, bc.max.y, tam.z), 2.4f); else yield return new WaitForSeconds(2.4f);
        yield return new WaitForSeconds(1.2f);
        yield return Den(0, 1, 1.0f);
        Destroy(cha.gameObject);
    }

    IEnumerator KetEvil()
    {
        dangKet = true;
        LocFirstPerson.khoa = true; LocMa.tamDung = true;
        yield return new WaitForSeconds(1.0f);
        if (cReo && amReo) { amReo.PlayOneShot(cReo, 0.9f); yield return new WaitForSeconds(Mathf.Max(1.5f, cReo.length)); amReo.PlayOneShot(cReo, 0.9f); yield return new WaitForSeconds(Mathf.Max(1.5f, cReo.length)); }
        yield return Den(LocFirstPerson.den, 1, 0.8f);
        // [02/11] cuộc gọi đúng chữ v3.3 (trước: "Xong rồi chứ." — nghĩa ngược, như thể thầy tưởng mọi thứ theo ý mình)
        yield return Thoai((THAY, "Cháu giữ nó lại à."), ("KHÔI", "..."), (THAY, "Cháu khá hơn bố cháu."), ("KHÔI", "Dạ."));
        if (cTut && amReo) { amReo.PlayOneShot(cTut, 0.8f); yield return new WaitForSeconds(cTut.length); }
        LocFirstPerson.chuGiua = "Năm năm sau.";
        yield return new WaitForSeconds(3f);
        LocFirstPerson.chuGiua = null;
        yield return NamNamSau();   // [02/11] cảnh thật thay cho chỉ có chữ
        LocFirstPerson.den = 1;
        LocFirstPerson.chuGiua = "Anh không đếm lại bao giờ.";
        yield return new WaitForSeconds(6f);
        VeMenu();
    }

    // ───── tiện ích
    static IEnumerator Den(float tu, float toi, float giay)
    {
        for (float t = 0; t < 1; t += Time.deltaTime / giay) { LocFirstPerson.den = Mathf.Lerp(tu, toi, t); yield return null; }
        LocFirstPerson.den = toi;
    }

    // hộp va chạm theo lưới (dùng được cả khi vật đang ẩn), tối thiểu 12 × 6 × 12 cm
    static void HopTheoLuoi(Transform t)
    {
        var mf = t.GetComponentInChildren<MeshFilter>(true);
        var bc = t.gameObject.AddComponent<BoxCollider>();
        if (!mf || !mf.sharedMesh) { bc.size = new Vector3(0.12f, 0.08f, 0.12f); return; }
        var mb = mf.sharedMesh.bounds;
        var c = t.InverseTransformPoint(mf.transform.TransformPoint(mb.center));
        var s = t.InverseTransformVector(mf.transform.TransformVector(mb.size));
        var k = t.lossyScale;
        bc.center = c;
        bc.size = new Vector3(Mathf.Max(Mathf.Abs(s.x), 0.12f / Mathf.Max(1e-4f, Mathf.Abs(k.x))), Mathf.Max(Mathf.Abs(s.y), 0.06f / Mathf.Max(1e-4f, Mathf.Abs(k.y))), Mathf.Max(Mathf.Abs(s.z), 0.12f / Mathf.Max(1e-4f, Mathf.Abs(k.z))));
    }

    // hộp bao kể cả vật đang ẩn (nhóm Dem2/Dem3 có thể đang tắt lúc vào scene)
    static Bounds BaoV(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return new Bounds(t.position, Vector3.one * 0.1f);
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    // bản sao chỉ phần hình (lưới + vật liệu) của một vật: không nhân bản LocVatPham/va chạm (Awake sẽ đăng ký trùng id)
    static GameObject BanSaoHinh(Transform goc, string ten, Transform cha)
    {
        var g0 = new GameObject(ten); g0.transform.SetParent(cha, false); g0.transform.SetPositionAndRotation(goc.position, goc.rotation);
        foreach (var mf in goc.GetComponentsInChildren<MeshFilter>(true))
        {
            var g = new GameObject(mf.name); g.transform.SetParent(g0.transform, false);
            g.transform.localPosition = goc.InverseTransformPoint(mf.transform.position); g.transform.localRotation = Quaternion.Inverse(goc.rotation) * mf.transform.rotation; g.transform.localScale = mf.transform.lossyScale;
            g.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh; g.AddComponent<MeshRenderer>().sharedMaterials = mf.GetComponent<MeshRenderer>().sharedMaterials;
        }
        foreach (var sm in goc.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            var g = new GameObject(sm.name); g.transform.SetParent(g0.transform, false);
            g.transform.SetPositionAndRotation(sm.transform.position, sm.transform.rotation); g.transform.localScale = sm.transform.lossyScale;
            g.AddComponent<MeshFilter>().sharedMesh = sm.sharedMesh; g.AddComponent<MeshRenderer>().sharedMaterials = sm.sharedMaterials;
        }
        return g0;
    }

    // tìm cả vật đang ẩn (nhóm Dem2/Dem3 tắt lúc vào scene); cha: bắt buộc tên cha trực tiếp
    static Transform TimV(string ten, string cha = null)
    {
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == ten && (cha == null || (t.parent && t.parent.name == cha))) return t;
        Debug.LogWarning("[LOC] V34 không thấy " + ten + " — dựng lại nhà (LocHouseBuilder_V35)");
        return null;
    }

    // năm chỗ chén dưới hầm (LocHouseBuilder_V35): dan = đã dán chén Đêm 2 (hai cái đầu lành lại + chén số ba vào chỗ) · vo4 = Death · vo5 = Bad
    public static void ChenHam(bool dan, bool vo4, bool vo5)
    {
        var p = TimV("ChenHam_TrangThai"); if (!p) return;
        void S(string n, bool bat) { var t = p.Find(n); if (t) t.gameObject.SetActive(bat); }
        S("Chen1_Vo", !dan); S("Chen1_Dan", dan); S("Chen2_Vo", !dan); S("Chen2_Dan", dan); S("Chen3_Dan", dan);
        S("Chen4", !vo4); S("Chen4_Vo", vo4); S("Chen5", !vo5); S("Chen5_Vo", vo5);
    }

    // món đặt ngoài nhà: hộp va chạm theo vật + LocVatPham
    static LocVatPham Mon(GameObject g, string id, string ten, LocVatPham.Kieu kieu, string ghiChu, int tuDem, string[] khiNhat, string[] trang = null, string chuaToi = "", bool dangKy = true)
    {
        if (!g.GetComponentInChildren<Collider>(true))
        {
            var b = BaoV(g.transform); var bc = g.AddComponent<BoxCollider>();
            bc.center = g.transform.InverseTransformPoint(b.center);
            var s = g.transform.InverseTransformVector(b.size);
            bc.size = new Vector3(Mathf.Max(Mathf.Abs(s.x), 0.1f / Mathf.Max(0.001f, g.transform.lossyScale.x)), Mathf.Max(Mathf.Abs(s.y), 0.06f / Mathf.Max(0.001f, g.transform.lossyScale.y)), Mathf.Max(Mathf.Abs(s.z), 0.1f / Mathf.Max(0.001f, g.transform.lossyScale.z)));
        }
        var v = g.AddComponent<LocVatPham>();
        v.id = id; v.ten = ten; v.kieu = kieu; v.ghiChu = ghiChu; v.tuDem = tuDem; v.chuaToi = chuaToi;
        v.khiNhat = khiNhat ?? new string[0]; v.trang = trang ?? new string[0];
        if (dangKy) LocTui.DangKy(v);
        return v;
    }

    // món chỉ có trong túi (giấy tờ): vật ẩn mang LocVatPham để đọc được
    static LocVatPham MonAn(Transform cha, string id, string ten, string ghiChu, int docTuDem, string[] trang)
    {
        var g = new GameObject("TUI_" + id); g.transform.SetParent(cha, false); g.SetActive(false);
        var v = g.AddComponent<LocVatPham>();
        v.id = id; v.ten = ten; v.kieu = LocVatPham.Kieu.Nhat; v.ghiChu = ghiChu; v.tuDem = 1; v.docTuDem = docTuDem;
        v.khiNhat = new string[0]; v.trang = trang ?? new string[0];
        LocTui.DangKy(v);
        return v;
    }
}
