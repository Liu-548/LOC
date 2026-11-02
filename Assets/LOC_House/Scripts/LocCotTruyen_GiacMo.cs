using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// [02/11] Nối GIẤC MƠ cuối Đêm 2 vào mạch (ke-hoach-giac-mo-02-11.md, đã chốt 02/11 — chỉ một giấc mơ, ở Đêm 2):
//  giữ hương (giu_huong_2) → cảnh nửa tỉnh ở cửa phòng Nhím ("Còn. Ngủ đi.") → sáng lại, "Ngủ thôi." (nua_tinh_2)
//  → về phòng Khôi, [E] Đi ngủ ở GiuongDon_Khoi → GIẤC MƠ (LocGiacMo) → bật dậy trên giường → hết đêm (cảnh ban ngày → Đêm 3).
//  Cờ: mo_2_xong + mo2_thoat | mo2_bi_bat (không đổi gì về sau — GM-10).
//  Phím thử (Editor): 8 ở Đêm 2 = đúng mạch thật (mơ → tỉnh → "Sáng hôm sau" → Đêm 3) · Shift + 8 = chỉ thử giấc mơ, ở lại đêm hiện tại.
public partial class LocCotTruyen
{
    LocVatTam choNgu;
    Transform giuongKhoi;
    float tNuaTinh = -1f; bool daNhacNgu;
    public LocVatTam ChoNgu => choNgu;   // tự kiểm đọc
    // true: bị bắt trong mơ = chết thật (màn Death, chơi lại từ chỗ lưu). Mặc định false: bị bắt → bật dậy, đêm vẫn tiếp (kế hoạch GM-10)
    public static bool biBatLaChet = false;

    void KhoiTaoGiacMo()
    {
        giuongKhoi = TimV("GiuongDon_Khoi");
        if (!giuongKhoi) { Debug.LogWarning("[LOC] Không thấy GiuongDon_Khoi — cuối Đêm 2 vào mơ thẳng sau cảnh nửa tỉnh"); return; }
        var b = BaoV(giuongKhoi);
        var g = new GameObject("CHO_DiNgu"); g.transform.SetParent(transform, false);
        float cao = Mathf.Clamp(b.size.y, 0.45f, 1.1f);
        g.transform.position = new Vector3(b.center.x, b.min.y + cao / 2f, b.center.z);
        var bc = g.AddComponent<BoxCollider>(); bc.size = new Vector3(b.size.x + 0.06f, cao + 0.04f, b.size.z + 0.06f);   // phủ ngoài hộp va chạm của giường để tia nhìn trúng
        choNgu = g.AddComponent<LocVatTam>();
        choNgu.goiY = () => LocMa.dem == 2 && LocTui.Co("nua_tinh_2") && !LocTui.Co("mo_2_xong") && !dangCanh ? "[E] Đi ngủ" : "";
        choNgu.bam = () => StartCoroutine(DiNgu(false));
    }

    // gọi từ Dem2(): giữ hương xong → cảnh nửa tỉnh → trả điều khiển, đi về giường
    IEnumerator SauGiuHuong2()
    {
        dangCanh = true;
        yield return new WaitForSeconds(1.2f);   // kịp thấy đèn hầm tắt sau lưng
        if (LocNhim.I)
        {
            LocNhim.I.CanhNuaTinh(true);
            yield return null;
            while (LocNhim.dangCanh) yield return null;   // cảnh tự cắt đen, màn vẫn đen; người chơi đứng ở cửa phòng Nhím
        }
        LocTui.Dat("nua_tinh_2");
        LocFirstPerson.khoa = true; LocMa.tamDung = true; LocFirstPerson.den = 1;
        if (!choNgu) { dangCanh = false; yield return DiNgu(false); yield break; }
        yield return new WaitForSeconds(0.8f);
        yield return Den(1, 0, 1.2f);
        LocFirstPerson.khoa = false; dangCanh = false;
        LocFirstPerson.PhuDe("Ngủ thôi.", 2.4f);
        tNuaTinh = Time.time; daNhacNgu = false;
    }

    // gọi từ Dem2(): đã "Ngủ thôi." mà một phút sau vẫn lang thang → nhắc một lần (không có chữ gợi ý nào khác)
    void NhacDiNgu()
    {
        if (daNhacNgu || tNuaTinh < 0 || !LocTui.Co("nua_tinh_2") || LocTui.Co("mo_2_xong") || LocFirstPerson.khoa || LocTui.Mo) return;
        if (Time.time - tNuaTinh < 60f) return;
        daNhacNgu = true;
        LocFirstPerson.PhuDe("Khuya lắm rồi. Về phòng mà ngủ.", 2.6f);
    }

    // tắt đèn phòng Khôi trước khi nằm (bật dậy giữa đêm thì phòng tối — kế hoạch mục 4) · trả về bật/tắt cũ (phím thử)
    LocCongTac CongTacPhongKhoi() { var g = GameObject.Find("CongTac_PhongKhoi"); return g ? g.GetComponent<LocCongTac>() : null; }

    public void DiNguNgay() { if (!dangCanh && !LocGiacMo.dangMo) StartCoroutine(DiNgu(false)); }   // LocChoiThu
    IEnumerator DiNguThu() { yield return DiNgu(true); }

    // [E] Đi ngủ → tối dần → mơ → tỉnh dậy trên giường → (thật) hết đêm · (thử) trả điều khiển ở đêm hiện tại
    IEnumerator DiNgu(bool thu)
    {
        if (LocGiacMo.dangMo) yield break;
        dangCanh = true; LocFirstPerson.khoa = true; LocMa.tamDung = true;
        LocFirstPerson.PhuDe("", 0.01f);
        yield return Den(LocFirstPerson.den, 1, 0.9f);
        if (!thu) { var ni = TimV("TamNilon_PhuBui"); if (ni) ni.gameObject.SetActive(false); }   // Khôi đã kéo tấm nilon phủ bụi ra mới nằm được
        var ct = CongTacPhongKhoi(); bool denCu = ct && ct.bat;
        if (denCu) { ct.Dat(false); LocAm.Phat("CongTac_Tach", ct.transform.position, 0.8f); }
        var camT = fp.cam; var lp = camT.localPosition; var lr = camT.localRotation;
        yield return new WaitForSeconds(1.4f);
        bool thoat = false;
        var gm = LocGiacMo.Tao();
        yield return gm.Chay(fp, r => thoat = r);
        Debug.Log($"[LOC] Giấc mơ: {(thoat ? "THOÁT" : "BỊ BẮT")} · {gm.tgChay:0} s · ném {gm.soNem} (đôi {gm.soNemDoi}) · trúng {gm.soTrung} · cảnh báo {(gm.daCanhBao ? "có" : "không")}");
        camT.localPosition = lp; camT.localRotation = lr;
        if (!thu) { LocTui.Dat("mo_2_xong"); LocTui.Dat(thoat ? "mo2_thoat" : "mo2_bi_bat"); }
        if (!thoat && biBatLaChet && !thu)
        {
            LocFirstPerson.den = 1; LocFirstPerson.khoa = true;
            yield return new WaitForSeconds(1.5f);
            LocFirstPerson.chuGiua = "KHÔI KHÔNG TỈNH DẬY NỮA.";
            yield return new WaitForSeconds(2.5f);
            LocMenu.ManChet();
            yield break;
        }
        yield return TinhDay(thoat);
        camT.localPosition = lp; camT.localRotation = lr;
        if (thu)
        {
            if (denCu && ct) ct.Dat(true);
            yield return Den(1, 0, 0.8f);
            LocFirstPerson.PhuDe($"[thử] Giấc mơ xong — vẫn ở Đêm {LocMa.dem}. (Ở Đêm 2 bấm 8 không giữ Shift để đi đúng mạch sang Đêm 3.)", 3.5f, false);
            LocFirstPerson.khoa = false; LocMa.tamDung = false; dangCanh = false;
            yield break;
        }
        // [02/11] thông báo qua ngày mới: click hoặc chờ ~3 s
        yield return LocThongBao.NgayMoi("SÁNG HÔM SAU", "Đêm thứ hai đã qua.", LocMenu.NgayDai(3));
        dangCanh = false;
        yield return HetDem();   // nua_tinh_2 đã có → HetDem bỏ cảnh nửa tỉnh, màn đang đen → thẳng sang cảnh ban ngày
    }

    // bật dậy trên giường: nằm ngửa → ngồi bật dậy 0,4 s → thở dốc, nhìn quanh phòng tối → độc thoại → tối dần
    IEnumerator TinhDay(bool thoat)
    {
        var camT = fp.cam;
        LocFirstPerson.den = 1; LocFirstPerson.khoa = true;
        Vector3 nam, ngoi, nhin, chan, veDau;
        if (giuongKhoi)
        {
            var b = BaoV(giuongKhoi);
            chan = CanhGiuong(b);
            var raNgoai = Vector3.ProjectOnPlane(chan - b.center, Vector3.up).normalized;
            var dai = b.size.x > b.size.z ? Vector3.right : Vector3.forward;
            float nuaDai = (b.size.x > b.size.z ? b.size.x : b.size.z) / 2f - 0.35f;
            var dau = TimGoi(b, dai, nuaDai);
            nam = new Vector3(dau.x, b.min.y + Mathf.Min(0.62f, b.size.y * 0.8f), dau.z);
            ngoi = nam + raNgoai * 0.25f + Vector3.up * 0.48f + (b.center - dau).normalized * 0.25f;
            nhin = ngoi + raNgoai * 2f + (b.center - dau).normalized * 0.8f + Vector3.down * 0.35f;
            veDau = Vector3.ProjectOnPlane(dau - b.center, Vector3.up).normalized;
        }
        else
        {
            chan = fp.transform.position; nam = chan + Vector3.up * 0.5f; ngoi = chan + Vector3.up * 1.0f; nhin = ngoi + fp.transform.forward * 2f; veDau = -fp.transform.forward;
        }
        fp.DatTuThe(chan, nhin);
        camT.position = nam; camT.rotation = Quaternion.LookRotation(Vector3.up, veDau.sqrMagnitude > 0.01f ? veDau : Vector3.forward);   // nằm ngửa: nhìn trần, đỉnh khung hình về phía đầu giường
        yield return new WaitForSeconds(0.5f);
        // mở mắt đột ngột
        if (LocNhim.I && LocNhim.I.khoiTho) AudioSource.PlayClipAtPoint(LocNhim.I.khoiTho, nam, thoat ? 0.8f : 1f);
        LocFirstPerson.den = 0;
        var q0 = camT.rotation; var q1 = Quaternion.LookRotation(nhin - ngoi, Vector3.up);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.4f)
        {
            float e = 1 - (1 - t) * (1 - t);
            camT.position = Vector3.Lerp(nam, ngoi, e) + (thoat ? Vector3.zero : Random.insideUnitSphere * 0.01f);
            camT.rotation = Quaternion.Slerp(q0, q1, e);
            yield return null;
        }
        // thở dốc + nhìn quanh (bị bắt: run lâu hơn)
        float giay = thoat ? 2.4f : 3.6f, rung = thoat ? 0.004f : 0.011f;
        for (float t = 0; t < giay; t += Time.deltaTime)
        {
            float tho = Mathf.Sin(t * (thoat ? 5.5f : 7f)) * 0.012f;
            float quay = Mathf.Sin(t / giay * Mathf.PI * 2f) * (thoat ? 25f : 18f);
            camT.position = ngoi + Vector3.up * tho + Random.insideUnitSphere * rung;
            camT.rotation = Quaternion.AngleAxis(quay, Vector3.up) * q1;
            yield return null;
        }
        LocFirstPerson.PhuDe(thoat ? "Lại cái hành lang đấy." : "...Mơ. Mơ thôi.", 2.4f);
        for (float t = 0; t < 2.6f; t += Time.deltaTime) { camT.position = ngoi + Random.insideUnitSphere * rung * 0.6f; yield return null; }
        yield return Den(0, 1, 0.9f);
        yield return new WaitForSeconds(0.4f);
        fp.DatTuThe(chan, nhin);
    }

    // chỗ đứng cạnh giường (sàn trống, gần cửa phòng nhất) — để lúc trả điều khiển không đứng lọt trong giường
    Vector3 CanhGiuong(Bounds b)
    {
        var cua = TimV("CuaKhoi (mở)"); var dich = cua ? cua.position : b.center + Vector3.left * 3f;
        var ung = new[] { new Vector3(b.min.x - 0.42f, b.min.y, b.center.z), new Vector3(b.max.x + 0.42f, b.min.y, b.center.z), new Vector3(b.center.x, b.min.y, b.min.z - 0.42f), new Vector3(b.center.x, b.min.y, b.max.z + 0.42f) };
        System.Array.Sort(ung, (p, q) => Vector3.Distance(p, dich).CompareTo(Vector3.Distance(q, dich)));
        foreach (var p in ung)
            if (!Physics.CheckCapsule(p + Vector3.up * 0.4f, p + Vector3.up * 1.6f, 0.28f, ~0, QueryTriggerInteraction.Ignore)) return p + Vector3.up * 0.03f;
        return ung[0] + Vector3.up * 0.03f;
    }
    // đầu giường = đầu có gối (tìm vật tên "Goi" trong giường), không có thì đầu xa cửa phòng
    Vector3 TimGoi(Bounds b, Vector3 dai, float nuaDai)
    {
        foreach (var t in giuongKhoi.GetComponentsInChildren<Transform>(true))
            if (t.name.ToLower().Contains("goi") && t != giuongKhoi) { var p = t.position; p.y = b.center.y; return b.center + dai * Mathf.Sign(Vector3.Dot(p - b.center, dai)) * nuaDai; }
        var cua = TimV("CuaKhoi (mở)"); var c = cua ? cua.position : b.center;
        return b.center - dai * Mathf.Sign(Vector3.Dot(c - b.center, dai) + 1e-4f) * nuaDai;
    }

    void GoiYGiacMo(ref string id, ref string[] bac)
    {
        if (LocMa.dem == 2 && LocTui.Co("nua_tinh_2") && !LocTui.Co("mo_2_xong"))
        { id = "di_ngu"; bac = new[] { "Muộn rồi. Về phòng mình ngủ.", "Phòng mình ngay cạnh phòng em." }; }
    }

    void PhimThuGiacMo(Keyboard k)
    {
#if UNITY_EDITOR
        // [02/11] phím 8 ở Đêm 2 = đúng mạch thật: mơ → tỉnh dậy → "Sáng hôm sau" → cảnh đưa tang → Đêm 3
        //         Shift + 8 (hoặc 8 ở đêm khác) = chỉ thử giấc mơ: tỉnh dậy rồi ở lại đêm đang chơi
        if (k != null && k.digit8Key.wasPressedThisFrame && !LocFirstPerson.khoa && !LocMenu.DangChe && !LocTui.Mo && !LocGiacMo.dangMo)
        {
            if (LocMa.dem == 2 && !k.shiftKey.isPressed) { LocTui.Dat("giu_huong_2"); LocTui.Dat("nua_tinh_2"); StartCoroutine(DiNgu(false)); }
            else StartCoroutine(DiNguThu());
        }
#endif
    }
}
