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
    LocVatTam kienHang, choDatChao;
    GameObject chaoTrenBan;
    float tGoi2 = -1f, tReoThay;
    bool daNhacKhan, daNhacDocHam, daNhacBayMam, dangKet, daDanChen;
    public bool DangKet => dangKet;   // LocNhim.TatCanh: đang trong phần kết thì giữ màn đen, giữ khoá

    // ───── khởi tạo (gọi trong Start, trước ApDem)
    void KhoiTaoV34()
    {
        foreach (var t in FindObjectsByType<LocTuongTacMG>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.id == "dan_chen") mgDanChen = t;
            if (t.id == "nl_bay_mam") mgBayMam = t;
            if (t.id == "nl_giu_nhang") mgGiuNhang = t;
        }
        var cha = new GameObject("LOC_CotTruyen_V34").transform;

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
        if (kienHang) kienHang.gameObject.SetActive(d == 3 && !LocTui.Co("mo_kien"));
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
        if (!CoGoiThay || !LocTui.Co("goi_2") || LocTui.Co("goi_thay") || dangReo || dangCanh || LocNhim.dangCanh || LocFirstPerson.khoa || LocTui.Mo) return;
        var p = fp.transform.position;
        if (hamPhong.Contains(p) || hopThang.Contains(p) || Time.time - tGoi2 < 35f || Time.time < tReoThay || KC(dienThoai) > 16f) return;
        tReoThay = Time.time + 55f;   // không nhấc thì một lúc sau reo lại
        StartCoroutine(Reo(10, true, CuocGoiThay));
    }

    IEnumerator CuocGoiThay()
    {
        yield return Thoai(("KHÔI", "A lô."));
        if (cRe) { amReo.PlayOneShot(cRe, 0.5f); yield return new WaitForSeconds(1.2f); }
        yield return Thoai((THAY, "Khôi đấy phải không."),
            ("KHÔI", "Vâng. Ai đấy ạ."),
            (THAY, "Người lo việc thờ cúng cho nhà anh. Bố anh chắc có nhắc."),
            (THAY, "Nghe cho kỹ, tôi chỉ nói một lần."),
            (THAY, "Dưới hầm có hai cái chén vỡ. Dán lại cho lành. Keo ở cửa hàng nhà anh."),
            (THAY, "Cái chén trên phòng thờ, đem xuống đặt vào chỗ thứ ba."),
            (THAY, "Bát cháo bác anh nấu, bưng xuống để lên bàn thờ dưới ấy."),
            (THAY, "Giữ hương trên linh cữu đến sáng."),
            (THAY, "Tối mai mùng một. Tôi gửi đồ sang. Làm đúng như trong giấy."),
            ("KHÔI", "Ông là thầy Đồng?"),
            (THAY, "Làm đúng thì con bé khỏi. Đừng có mở cái hũ."));
        if (cTut) { amReo.PlayOneShot(cTut, 0.8f); yield return new WaitForSeconds(cTut.length); }
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
            if (LocNhim.I) LocNhim.I.GoiTuTrenGac(); else StartCoroutine(KetEvil());
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
        yield return BuaRoi();
        yield return new WaitForSeconds(1.2f);
        LocFirstPerson.PhuDe("NHÍM: ...Cảm ơn.", 2.6f, false);
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
        LocFirstPerson.PhuDe("Mùng một.", 2.2f); yield return new WaitForSeconds(2.6f);
        LocFirstPerson.PhuDe("Sáng rồi.", 2.2f); yield return new WaitForSeconds(2.6f);
        LocFirstPerson.PhuDe(evil ? "Làm đúng như ông ấy dặn." : "Không có chuyện gì cả.", 2.6f); yield return new WaitForSeconds(3.2f);
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
            yield return Den(1, 0, 1.0f);
            yield return new WaitForSeconds(1.6f);
            ChenHam(LocTui.Co("dan_chen"), false, true);
            if (LocMa.I && LocMa.I.chenVo) AudioSource.PlayClipAtPoint(LocMa.I.chenVo, b.center, 1f);
            yield return new WaitForSeconds(2.4f);
            yield return Den(0, 1, 0.8f);
            Destroy(den.gameObject);
        }
        LocFirstPerson.chuGiua = "Bốn năm sau.";
        yield return new WaitForSeconds(3f);
        LocFirstPerson.chuGiua = "Anh đã ở trong căn nhà này bốn năm.";
        yield return new WaitForSeconds(6f);
        VeMenu();
    }

    IEnumerator KetEvil()
    {
        dangKet = true;
        LocFirstPerson.khoa = true; LocMa.tamDung = true;
        yield return new WaitForSeconds(1.0f);
        if (cReo && amReo) { amReo.PlayOneShot(cReo, 0.9f); yield return new WaitForSeconds(Mathf.Max(1.5f, cReo.length)); amReo.PlayOneShot(cReo, 0.9f); yield return new WaitForSeconds(Mathf.Max(1.5f, cReo.length)); }
        yield return Den(LocFirstPerson.den, 1, 0.8f);
        yield return Thoai(("KHÔI", "A lô."), (THAY, "Xong rồi chứ."), ("KHÔI", "Vâng."), (THAY, "Cháu khá hơn bố cháu."));
        if (cTut && amReo) { amReo.PlayOneShot(cTut, 0.8f); yield return new WaitForSeconds(cTut.length); }
        LocFirstPerson.chuGiua = "Năm năm sau.";
        yield return new WaitForSeconds(3f);
        LocFirstPerson.chuGiua = "Anh không đếm lại bao giờ.";
        yield return new WaitForSeconds(6f);
        VeMenu();
    }

    // ───── gợi ý H thêm cho phần mới
    void GoiYHV34(ref string id, ref string[] bac)
    {
        int d = LocMa.dem;
        if (d == 2)
        {
            if (CoGoiThay && LocTui.Co("goi_2") && !LocTui.Co("goi_thay")) { id = "goi_thay"; bac = new[] { "Lúc nãy điện thoại reo.", "Cứ ở gần điện thoại phòng khách.", "Reo thì nhấc máy." }; return; }
            if (LocTui.Co("goi_thay") && mgDanChen && vpKeo && vpChen3 && !LocTui.Co("dan_chen")) { id = "dan_chen"; bac = new[] { "Ông ấy bảo dán hai cái chén dưới hầm.", "Keo thì cửa hàng nhà mình có bán, trên quầy.", "Còn cái chén trên phòng thờ tầng ba." }; return; }
            if (LocTui.Co("goi_thay") && vpChao && choDatChao && !LocTui.Co("cung_chao")) { id = "cung_chao"; bac = new[] { "Còn bát cháo.", "Bác để cháo trên bàn ăn dưới bếp.", "Bưng xuống bàn thờ dưới hầm." }; return; }
        }
        if (d == 3)
        {
            if (kienHang && !LocTui.Co("mo_kien")) { id = "mo_kien"; bac = new[] { "Có cái gì để ở cửa.", "Cạnh cửa chính." }; return; }
            if (LocTui.Co("nl_bay_mam") && vpVanKhan && !LocTui.Co("nl_doc_van")) { id = "nl_doc_van"; bac = new[] { "Còn bài khấn.", "Đứng trước bàn thờ dưới hầm mà đọc.", "Mở trong túi, lật tới trang cuối." }; return; }
            if (LocHuCot.ChoChon) { id = "chon_hu"; bac = new[] { "Còn cái hũ.", "Ông ấy bảo niêm lại.", "Sách của mẹ thì nói khác." }; return; }
        }
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
