#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// [27/10] CHƠI THỬ MỘT MẠCH (menu LOC → Chơi thử một mạch): công cụ kiểm thử, KHÔNG phải gameplay.
// Đi lần lượt Đêm 1 → Đêm 3 bằng code (dịch chuyển + bấm như người chơi), ghi mọi phụ đề, cờ cốt truyện, chụp ảnh các mốc,
// rồi lần lượt các kết cục (Death thật; Bad/Good/Evil gọi cảnh có sẵn vì đường vào chưa dựng). Chỉ ghi nhận, không sửa gì → LOC_KiemTra/ChoiThu/
public class LocChoiThu : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bat()
    {
        if (!UnityEditor.EditorPrefs.GetBool("LOC_ChoiThu", false)) return;
        UnityEditor.EditorPrefs.SetBool("LOC_ChoiThu", false);
        var g = new GameObject("_ChoiThu"); DontDestroyOnLoad(g); g.AddComponent<LocChoiThu>();
    }
    [UnityEditor.MenuItem("LOC/Chơi thử một mạch (Play)")]
    static void Menu() { UnityEditor.EditorPrefs.SetBool("LOC_ChoiThu", true); UnityEditor.EditorApplication.isPlaying = true; }

    const string DIR = "LOC_KiemTra/ChoiThu/";
    readonly System.Text.StringBuilder sb = new();
    float t0; int anh; string phuCu = "", chuCu = "", coCu = "", goiYCu = "";
    static readonly BindingFlags F = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
    static object Lay(System.Type t, object o, string ten) => t.GetField(ten, F)?.GetValue(o);
    static void Dat(System.Type t, object o, string ten, object v) => t.GetField(ten, F)?.SetValue(o, v);
    static object Goi(System.Type t, object o, string ten, params object[] a) => t.GetMethod(ten, F)?.Invoke(o, a);

    void G(string s) { var d = $"{Time.time - t0,7:F1}s  Đ{LocMa.dem}  {s}"; sb.AppendLine(d); System.IO.File.WriteAllText(DIR + "log.txt", sb.ToString()); }
    void Muc(string s) => G("════ " + s);
    void Loi(string s) => G("!! " + s);
    IEnumerator Chup(string ten) { ScreenCapture.CaptureScreenshot($"{DIR}{anh++:000}_{ten}.png"); G("ảnh " + ten); yield return null; yield return null; }
    IEnumerator Cho(float s) { yield return new WaitForSeconds(s); }
    IEnumerator ChoDen(System.Func<bool> dk, float toiDa, string ten)
    {
        float a = Time.time; while (!dk() && Time.time - a < toiDa) yield return null;
        if (!dk()) Loi($"hết {toiDa:0}s mà chưa: {ten}"); else G($"ok ({Time.time - a:0.0}s): {ten}");
    }

    LocFirstPerson fp => FindAnyObjectByType<LocFirstPerson>();
    void Toi(Vector3 p, Vector3 nhin) { var f = fp; if (f) f.DatTuThe(p, nhin); }
    static T Tim<T>(System.Func<T, bool> dk) where T : Object { foreach (var x in FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (dk(x)) return x; return null; }
    static LocVatPham V(string id) => Tim<LocVatPham>(v => v.id == id);
    static LocViec Viec(string id) => Tim<LocViec>(v => v.id == id);
    static LocTuongTacMG MG(string id) => Tim<LocTuongTacMG>(v => v.id == id);
    static Transform T(string n) { var g = Tim<Transform>(x => x.name == n); return g; }
    static Vector3 Tam(Transform t) { var rs = t.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) return t.position; var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b.center; }

    void Update()
    {
        if (t0 == 0) return;
        var p = (string)Lay(typeof(LocFirstPerson), null, "phuDe") ?? "";
        float het = (float)(Lay(typeof(LocFirstPerson), null, "phuDeHet") ?? 0f);
        if (Time.time > het) p = "";
        if (p != phuCu) { if (p.Length > 0) G((((bool)Lay(typeof(LocFirstPerson), null, "phuDeNghieng")) ? "  « " : "  » ") + p); phuCu = p; }
        var c = LocFirstPerson.chuGiua ?? ""; if (c != chuCu) { if (c.Length > 0) G("  [CHỮ GIỮA] " + c); chuCu = c; }
        var co = LocTui.ChuoiCo; if (co != coCu)
        {
            var cu = new HashSet<string>(coCu.Split('|'));
            foreach (var x in co.Split('|')) if (x.Length > 0 && !cu.Contains(x)) G("  + cờ " + x);
            coCu = co;
        }
        var gy = fp ? fp.GoiYDangHien ?? "" : ""; if (gy != goiYCu) { if (gy.Length > 0) G("  [gợi ý] " + gy); goiYCu = gy; }
    }

    IEnumerator Start()
    {
        System.IO.Directory.CreateDirectory(DIR);
        foreach (var f in System.IO.Directory.GetFiles(DIR, "*.png")) try { System.IO.File.Delete(f); } catch { }
        yield return new WaitForSeconds(1f);
        t0 = Time.time; G("CHƠI THỬ MỘT MẠCH " + System.DateTime.Now.ToString("dd/MM HH:mm"));
        yield return Chup("menu_canh_bao");
        Goi(typeof(LocMenu), null, "Vao", 1, null, null);   // = Bắt đầu (chơi mới)
        yield return new WaitForSeconds(2f);
        yield return Dem1(); yield return Dem2(); yield return Dem3(); yield return CacKet();
        G("════ HẾT"); UnityEditor.EditorApplication.isPlaying = false;
    }

    // ───────────────────────── ĐÊM 1
    IEnumerator Dem1()
    {
        Muc("MỞ ĐẦU — XE BUÝT");
        yield return ChoDen(() => LocXeBuyt.dangChay, 10, "xe buýt chạy");
        yield return Cho(20); yield return Chup("xe_20s");
        yield return ChoDen(() => !LocXeBuyt.dangChay, 90, "xuống xe");
        yield return Cho(1); yield return Chup("truoc_cong");
        yield return ChoDen(() => LocTui.Co("d1_mo_dau"), 15, "độc thoại trước cổng");
        yield return Cho(7);

        Muc("ĐÊM 1 — PHÒNG KHÁCH");
        var ct = LocCotTruyen.I; var bt = T("BanThoVong_Dem1");
        Toi(new Vector3(3.8f, 0.05f, -1.6f), bt.position + Vector3.up);
        yield return Cho(1); yield return Chup("vao_phong_khach");
        yield return ChoDen(() => LocTui.Co("d1_liem"), 4, "\"Về tới nơi thì liệm rồi\"");
        yield return Cho(5);
        var di = LocDi.I;
        G($"bác: {(di ? (di.gameObject.activeInHierarchy ? "có trong cảnh @ " + di.transform.position.ToString("F2") : "đang ẩn") : "KHÔNG CÓ LocDi")}");
        if (di) { Toi(new Vector3(3.8f, 0.05f, 0.4f), di.transform.position + Vector3.up * 1.3f); yield return Cho(0.5f); yield return Chup("bac_truoc_khi_thap"); }
        Toi(ct.demSauDung, ct.demSauNhin); yield return Cho(0.5f);
        var v = Viec("thap_huong"); G("gợi ý bàn thờ vong: " + v.GoiY);
        v.Bam(); yield return Cho(2); yield return Chup("thap_huong");
        yield return ChoDen(() => !LocFirstPerson.khoa, 10, "trả điều khiển sau thắp hương");
        if (di) { Toi(fp.transform.position, di.transform.position + Vector3.up * 1.3f); yield return Cho(3); yield return Chup("bac_dang_noi"); }
        yield return ChoDen(() => LocTui.Co("ho_hang_ve"), 120, "thoại bác chạy hết");
        yield return Cho(4); if (di) G($"bác sau thoại: daVe={di.daVe} active={di.gameObject.activeInHierarchy}");
        yield return Cho(15); if (di) G($"bác 15 s sau: daVe={di.daVe} active={di.gameObject.activeInHierarchy} @ {di.transform.position:F2}");
        var lich = V("lich_t8"); if (lich) { G("lịch: " + lich.GoiY); lich.Bam(); yield return Cho(5); } else Loi("không có tờ lịch tương tác");
        var bn = MG("mg_ban_nuoc"); G($"mini game dọn bàn nước: {(bn ? (bn.HienDuoc ? "mở" : "khoá") : "không có")}");
        var hs = MG("mg_ho_so"); G($"mini game xếp hồ sơ trước khi mở két: {(hs ? (hs.HienDuoc ? "MỞ (sai)" : "khoá") : "không có")}");

        Muc("ĐÊM 1 — PHÒNG BỐ MẸ / LÀM VIỆC");
        var ks = FindAnyObjectByType<LocKetSat>(); var ket = Tam(ks.transform); G("két ở " + ket.ToString("F2"));
        Toi(new Vector3(ket.x + 0.5f, ket.y - 0.35f, ket.z + 1.1f), ket); yield return Cho(0.5f);
        yield return Chup("ket_sat"); G("gợi ý két: " + ks.GoiY);
        ks.Bam(); yield return Cho(3);
        ks.GoMa("1959"); yield return Cho(2.5f); ks.GoMa("1234"); yield return Cho(1.5f);
        G("két còn khoá sau mã sai: " + (ks.canh && ks.canh.khoa));
        var stc = V("so_thu_chi_2002");
        if (stc) { var ph = stc.transform.position; Toi(ph + new Vector3(-0.9f, 0, 0), ph); yield return Cho(0.4f); G("sổ thu chi: " + stc.GoiY); stc.Bam(); yield return Cho(0.5f);
            for (int i = 0; i < stc.trang.Length; i++) { Dat(typeof(LocTui), LocTui.I, "trangI", i); yield return Cho(0.4f); yield return Chup("so_thu_chi_tr" + i); }
            LocTui.I.DatChe(0); }
        Toi(new Vector3(ket.x + 0.5f, ket.y - 0.35f, ket.z + 1.1f), ket); yield return Cho(0.3f);
        ks.Bam(); yield return Cho(0.5f); ks.GoMa("1980"); yield return Cho(1.5f);
        G("két mở: " + LocTui.Co("ket_mo")); yield return Chup("ket_mo");
        foreach (var id in new[] { "chia_ham", "so_cua_bo" }) { var m = V(id); if (m) { m.Bam(); yield return Cho(5); } else Loi("không có " + id); }
        G("trong két có sổ đỏ / giấy báo nợ: " + (T("SoDo") || T("SoDo_BaoNo") ? "có" : "KHÔNG"));
        G($"mini game xếp hồ sơ sau khi mở két: {(hs ? (hs.HienDuoc ? "mở" : "KHOÁ (sai)") : "không có")}");
        LocTui.I.DatChe(1); Dat(typeof(LocTui), LocTui.I, "chon", 1); yield return Cho(0.5f); yield return Chup("tui_so_bo_dem1"); LocTui.I.DatChe(0);

        Muc("ĐÊM 1 — NHÍM");
        var n = LocNhim.I;
        Toi(n.choCuaNgoai, n.choCuaTrong + Vector3.up * 1.4f); yield return Cho(0.5f);
        var cuaN = n.cua; G("cửa phòng Nhím đang " + (cuaN && cuaN.DangMo ? "mở" : "đóng") + (cuaN && cuaN.khoa ? " (khoá)" : ""));
        if (cuaN && !cuaN.DangMo) cuaN.Mo(); yield return Cho(1);
        Toi(n.choCuaTrong + Vector3.right * 0.6f, n.head.position); yield return Cho(1.5f); yield return Chup("nhim_d1_vao");
        yield return ChoDen(() => n.ChoThay, 40, "tới bước thay khăn");
        n.ThayKhan(); yield return Cho(3); yield return Chup("thay_khan_d1");
        yield return ChoDen(() => LocNhim.daTham[1], 30, "thăm Đêm 1 xong");
        yield return Cho(2);
        Toi(n.choCuaNgoai + Vector3.left * 0.8f, n.choCuaNgoai + Vector3.left * 3 + Vector3.up * 1.4f);
        yield return ChoDen(() => LocTui.Co("khoi_vo"), 6, "Khôi vỡ ở hành lang"); yield return Cho(2); yield return Chup("khoi_vo"); yield return Cho(4);
        Toi(n.choCuaTrong + Vector3.right * 0.6f, n.head.position); yield return Cho(5); yield return Chup("nhim_quay_lai_d1");

        Muc("ĐÊM 1 — HẦM");
        var cuaHam = T("CuaSatHam (mở)").GetComponent<LocDoor>();
        Toi(new Vector3(1.1f, 0.05f, 18.5f), cuaHam.transform.position + Vector3.up * 1.2f); yield return Cho(4);
        yield return Chup("truoc_cua_ham"); G($"cửa hầm: khoá={cuaHam.khoa} cần {cuaHam.canVatPham}");
        cuaHam.Bam(); yield return Cho(1.5f); G("cửa hầm mở: " + cuaHam.DangMo);
        var ctHam = T("CongTac_Ham").GetComponent<LocCongTac>();
        Toi(new Vector3(-0.6f, 0.05f, 18.1f), ctHam.transform.position); yield return Cho(0.6f); yield return Chup("hop_thang_bat_dien");
        ctHam.Dat(true); yield return Cho(0.3f);
        Toi(new Vector3(-2.0f, -2.25f, 13.8f), new Vector3(-2.0f, -1.6f, 11.0f)); yield return Cho(2); yield return Chup("ham_lan_dau");
        yield return Cho(14);
        var hu = FindAnyObjectByType<LocHuCot>(); G("hũ: " + hu.GoiY); hu.Bam(); yield return Cho(2.5f);
        Toi(new Vector3(-0.6f, 0.05f, 18.1f), new Vector3(3f, 1.5f, 18.3f)); yield return Cho(0.2f);
        Toi(new Vector3(1.0f, 0.02f, 18.3f), new Vector3(3f, 1.5f, 18.3f)); yield return Cho(0.4f);
        G("đèn hầm sau khi ra: " + (ctHam.bat ? "VẪN BẬT" : "tắt")); yield return Cho(1); yield return Chup("giat_minh_den_ham");
        yield return Cho(3);
        yield return ChoDen(() => LocTui.Co("goi_1"), 5, "cuộc gọi thứ nhất bắt đầu");
        Toi(new Vector3(1.0f, 0.05f, 3f), T("LOC_DienThoai_TronBo").position); yield return Cho(4); yield return Chup("dien_thoai_d1");

        Muc("ĐÊM 1 — GIỮ HƯƠNG");
        Toi(ct.demSauDung, ct.demSauNhin); yield return Cho(0.5f);
        G("gợi ý bàn thờ vong cuối đêm: " + v.GoiY);
        v.Bam(); yield return Cho(1);
        yield return ChoDen(() => LocMa.dem == 2, 40, "sang Đêm 2");
        yield return Cho(2); yield return Chup("ban_ngay_hoac_dau_dem2");
        yield return ChoDen(() => !LocFirstPerson.khoa && LocFirstPerson.den == 0, 25, "trả điều khiển đầu Đêm 2");
    }

    // ───────────────────────── ĐÊM 2
    IEnumerator Dem2()
    {
        Muc("ĐÊM 2");
        yield return Cho(6); yield return Chup("dau_dem2");
        G("linh cữu Đêm 2: " + (GameObject.Find("QuanTai_LOC") ? "có" : "KHÔNG"));
        var n = LocNhim.I;
        Toi(n.choCuaNgoai + Vector3.left * 0.4f, n.choCuaNgoai + Vector3.right * 2f + Vector3.up * 1.4f);
        int lo0 = LocMa.soLanLapLo; yield return Cho(25); G($"25 s ở hành lang T2 trước hoảng loạn: lấp ló {LocMa.soLanLapLo - lo0} lần (phải 0)");
        if (n.cua && !n.cua.DangMo) n.cua.Mo(); yield return Cho(1);
        Toi(n.choCuaTrong + Vector3.right * 0.6f, n.head.position); yield return Cho(1.5f); yield return Chup("nhim_d2");
        yield return ChoDen(() => n.ChoThay, 30, "Đêm 2 tới bước thay khăn"); n.ThayKhan();
        yield return ChoDen(() => LocNhim.daTham[2], 40, "thăm Đêm 2 xong");
        G("mini game rút quần áo trước hoảng loạn: " + (MG("mg_quan_ao")?.HienDuoc == true ? "mở" : "KHOÁ/không có"));

        Muc("ĐÊM 2 — CUỘC GỌI KHÔNG LỜI");
        var dt = T("LOC_DienThoai_TronBo");
        Toi(new Vector3(1.4f, 0.05f, 3.0f), dt.position); 
        yield return ChoDen(() => LocTui.Co("goi_2"), 5, "điện thoại Đêm 2 reo");
        yield return Cho(2); G("gợi ý nhìn điện thoại: " + fp.GoiYDangHien); yield return Chup("dien_thoai_d2_reo");
        var vdt = Viec("dien_thoai"); if (vdt) { G("LocViec điện thoại: " + vdt.GoiY); vdt.Bam(); } else Loi("điện thoại không có LocViec");
        yield return Cho(12);

        Muc("ĐÊM 2 — ĐỌC SỔ BỐ → HOẢNG LOẠN");
        Toi(new Vector3(5.2f, 3.65f, 15.4f), new Vector3(5.7f, 4.4f, 16.2f)); yield return Cho(0.5f); yield return Chup("phong_khoi_ban_hoc");
        var so = V("so_cua_bo"); LocTui.Doc(so); yield return Cho(0.5f);
        G($"sổ của bố: {so.trang.Length} trang, đọc được: {so.DocDuoc}");
        foreach (var i in new[] { 1, 7, 10, 11, 12, 18, 19, 22, 26 }) { Dat(typeof(LocTui), LocTui.I, "trangI", i); yield return Cho(i == 11 ? 3.5f : 0.6f); yield return Chup("so_bo_tr" + i); }
        Dat(typeof(LocTui), LocTui.I, "trangI", so.trang.Length - 1); yield return Cho(0.5f); yield return Chup("so_bo_bia_sau");
        LocTui.I.DatChe(0);
        yield return ChoDen(() => LocNhim.dangCanh, 3, "đoạn hoảng loạn bắt đầu");
        yield return Cho(6); yield return Chup("hoang_loan_1"); yield return Cho(8); yield return Chup("hoang_loan_2");
        yield return ChoDen(() => LocNhim.daHoangLoan && !LocNhim.dangCanh, 60, "hoảng loạn xong");
        G("mini game sau hoảng loạn: bàn nước " + MG("mg_ban_nuoc")?.HienDuoc + " · quần áo " + MG("mg_quan_ao")?.HienDuoc + " · hồ sơ " + MG("mg_ho_so")?.HienDuoc + " (phải False hết)");
        int lo1 = LocMa.soLanLapLo; float a = Time.time;
        Toi(new Vector3(3.0f, 0.05f, 9.0f), new Vector3(1.0f, 1.4f, 16f));   // sảnh sau tầng 1, nhìn về phía giếng trời / kho / WC (người chơi thường đi xuống sau hoảng loạn)
        while (LocMa.soLanLapLo == lo1 && Time.time - a < 70) yield return null;
        G($"lấp ló đầu tiên sau hoảng loạn: {(LocMa.soLanLapLo > lo1 ? $"sau {Time.time - a:0} s" : "KHÔNG có trong 70 s")}");
        if (LocMa.soLanLapLo > lo1) { yield return Cho(0.3f); yield return Chup("lap_lo_1"); }
        for (int k = 0; k < 3; k++) { var ok = (bool)Goi(typeof(LocMa), LocMa.I, "ThuLo", -1); G($"ép lấp ló thêm: {(ok ? "có" : "không có chỗ hợp lệ")} · tổng {LocMa.soLanLapLo}"); yield return Cho(0.4f); if (ok) yield return Chup("lap_lo_ep_" + k); yield return Cho(4); }

        Muc("ĐÊM 2 — ĐỒ CÚNG");
        foreach (var id in new[] { "bat_gao", "bat_muoi", "bo_mia" }) { var m = V(id); if (m) { G(id + ": " + m.GoiY); m.Bam(); yield return Cho(2); } }
        G("cháo / lọ keo / chén số 3 / thầy Đồng gọi điện có trong luồng: " + (V("lo_keo") || V("chen_3") ? "có" : "CHƯA (placeholder)"));
        var dc = MG("dan_chen"); G("dán chén: " + (dc ? (dc.HienDuoc ? "mở" : "khoá") : "không có"));

        // [27/10] các việc mới của Đêm 2 (cuộc gọi thầy Đồng, dán chén, bát cháo) — chơi thử tự động bật cờ cho qua
        G("cuộc gọi thầy / dán chén / bát cháo: " + (LocTui.Co("goi_thay") ? "đã gọi" : "chưa gọi") + " · " + (LocTui.Co("dan_chen") ? "đã dán" : "chưa dán") + " → bật cờ cho qua");
        LocTui.Dat("goi_thay"); LocTui.Dat("dan_chen"); LocTui.Dat("cung_chao");
        Muc("ĐÊM 2 — GIỮ HƯƠNG");
        var ct = LocCotTruyen.I; Toi(ct.demSauDung, ct.demSauNhin); yield return Cho(0.5f);
        var v = Viec("thap_huong"); G("gợi ý: " + v.GoiY); v.Bam();
        yield return ChoDen(() => LocMa.dem == 3, 75, "sang Đêm 3");
        yield return Cho(2); yield return Chup("ban_ngay_dem2_3");
        yield return ChoDen(() => !LocFirstPerson.khoa && LocFirstPerson.den == 0, 25, "trả điều khiển đầu Đêm 3");
    }

    // ───────────────────────── ĐÊM 3
    IEnumerator Dem3()
    {
        Muc("ĐÊM 3");
        yield return Cho(3); yield return Chup("dau_dem3");
        G("linh cữu Đêm 3: " + (GameObject.Find("QuanTai_LOC") ? "VẪN CÒN" : "không còn"));
        G("kiện hàng / lời nhắn mẹ / hai tờ giấy gói: " + (V("kien_hang") || V("loi_nhan_me") ? "có" : "CHƯA (placeholder)"));
        var n = LocNhim.I;
        Toi(n.choCuaNgoai, n.choCuaTrong + Vector3.up * 1.4f); if (n.cua && !n.cua.DangMo) n.cua.Mo(); yield return Cho(1);
        Toi(n.choCuaTrong + Vector3.right * 0.6f, n.head.position); yield return Cho(1.5f); yield return Chup("nhim_d3");
        yield return ChoDen(() => LocNhim.daTham[3], 60, "thăm Đêm 3 xong");

        Muc("ĐÊM 3 — HÉ HŨ → PHÒNG NHÍM");
        var ctHam = T("CongTac_Ham").GetComponent<LocCongTac>();
        Toi(new Vector3(-0.6f, 0.05f, 18.1f), ctHam.transform.position); yield return Cho(0.3f); ctHam.Dat(true);
        Toi(new Vector3(-2.0f, -2.25f, 13.8f), new Vector3(-2.0f, -1.6f, 11.0f)); yield return Cho(1.5f); yield return Chup("ham_dem3");
        var hu = FindAnyObjectByType<LocHuCot>(); G("hũ: " + hu.GoiY); hu.Bam();
        yield return Cho(6); yield return Chup("he_hu_toc");
        yield return ChoDen(() => LocTui.Co("diem_luu_1"), 60, "xong cảnh phòng Nhím + điểm lưu 1");
        yield return Cho(1); yield return Chup("sau_canh_phong_nhim");

        Muc("ĐÊM 3 — LẦN MẤT ĐIỆN ĐẦU");
        float a = Time.time; while (!(bool)Lay(typeof(LocMa), LocMa.I, "dangToi") && Time.time - a < 90) yield return null;
        G($"mất điện đầu tiên sau {Time.time - a:0} s");
        yield return Cho(1.3f); yield return Chup("mat_dien_dau");
        var chan = (Vector3)Lay(typeof(LocMa), LocMa.I, "chanMa"); var pp = fp.transform.position;
        G($"ma cách {Vector3.Distance(new Vector3(chan.x, 0, chan.z), new Vector3(pp.x, 0, pp.z)):0.0} m (lần đầu phải 5–6 m)");
        Toi(Vector3.MoveTowards(pp, chan, Vector3.Distance(pp, chan) - 2.6f), chan + Vector3.up * 1.6f);
        yield return Cho(1.5f); G("bước tới gần lần đầu: " + (LocMenu.DangChe || LocFirstPerson.chuGiua != null ? "CHẾT (sai)" : "sống"));
        yield return ChoDen(() => !(bool)Lay(typeof(LocMa), LocMa.I, "dangToi"), 10, "đèn sáng lại");

        Muc("ĐÊM 3 — THẮP HƯƠNG ÔNG BÀ + SÁCH");
        var anhOB = T("KhungAnhOngBa");
        Toi(new Vector3(3.8f, 7.05f, 4.9f), anhOB.position); yield return Cho(0.6f); yield return Chup("ban_tho_ong_ba");
        var sach = V("sach_gia_le"); G("sách trước khi thắp: " + sach.GoiY); sach.Bam(); yield return Cho(2);
        var vt3 = Viec("thap_huong_t3"); G("gợi ý thắp hương T3: " + vt3.GoiY + " · màn hình: " + fp.GoiYDangHien);
        vt3.Bam(); yield return Cho(4);
        sach.Bam(); yield return Cho(2); G("đã lấy sách: " + LocTui.CoMon("sach_gia_le"));
        LocTui.Doc(sach); for (int i = 0; i < sach.trang.Length; i++) { Dat(typeof(LocTui), LocTui.I, "trangI", i); yield return Cho(0.6f); yield return Chup("sach_tr" + i); }
        LocTui.I.DatChe(0); yield return ChoDen(() => LocTui.Co("diem_luu_2"), 3, "điểm lưu 2");

        Muc("ĐÊM 3 — NGHI LỄ (mini game) + CHỖ RẼ CUỐI");
        foreach (var id in new[] { "nl_bay_mam", "nl_giu_nhang" })
        {
            if (id == "nl_giu_nhang") LocTui.Dat("nl_doc_van");   // [27/10] vòng 2 đọc bài khấn
            var m = MG(id); if (!m) { Loi("không có " + id); continue; }
            yield return null; Toi(m.transform.position + Vector3.forward * 1.1f + Vector3.down * 0.6f, m.transform.position);
            G($"{id}: {(m.HienDuoc ? "mở" : "khoá")} · {m.GoiY}"); m.Bam(); yield return Cho(1); yield return Chup(id); LocMiniGame.BoQua(); yield return Cho(3);
        }
        G("chỗ rẽ cuối [Niêm hũ]/[Xé giấy niêm], cảnh cổng sáng mùng một: CHƯA DỰNG (placeholder → gọi cảnh kết có sẵn)");
    }

    // ───────────────────────── CÁC KẾT
    IEnumerator CacKet()
    {
        Muc("DEATH — bước vào trong 2 m lúc mất điện (lần thứ hai)");
        for (int k = 0; k < 4; k++)   // mất điện mà không đặt được ma (phòng hẹp) thì thử lại
        {
            LocMa.I.StartCoroutine((IEnumerator)Goi(typeof(LocMa), LocMa.I, "TatDen", -1));
            yield return Cho(1.4f);
            if (Lay(typeof(LocMa), LocMa.I, "dangHien") != null) break;
            G("mất điện nhưng không đặt được ma — thử lại"); yield return ChoDen(() => !(bool)Lay(typeof(LocMa), LocMa.I, "dangToi"), 10, "đèn sáng lại");
            Toi(new Vector3(3.8f, 0.05f, 2.0f), new Vector3(3.8f, 1.5f, 6f)); yield return Cho(0.5f);
        }
        var chan = (Vector3)Lay(typeof(LocMa), LocMa.I, "chanMa"); var pp = fp.transform.position;
        Toi(Vector3.MoveTowards(pp, chan, Vector3.Distance(pp, chan) - 1.2f), chan + Vector3.up * 1.6f);
        yield return Cho(0.5f); yield return Chup("death_mat_do");
        yield return ChoDen(() => LocFirstPerson.chuGiua != null, 12, "chữ Death Ending");
        yield return Chup("death_chu"); yield return Cho(3); yield return Chup("death_menu");
        Muc("CHƠI LẠI SAU KHI CHẾT → ĐIỂM LƯU");
        LocMa.ChoiLai(); yield return Cho(4);
        var p = fp.transform.position; G($"sau chơi lại: vị trí {p:F2} · hũ đã hé={LocMa.huDaHe} · ma giận={LocMa.maTucGian} · có sách={LocTui.CoMon("sach_gia_le")} · đêm {LocMa.dem}");
        yield return Chup("sau_choi_lai");

        Muc("BAD (a) — đồng hồ gõ đủ 12 tiếng (rút thời gian mỗi tiếng xuống 3 s để thử)");
        LocCotTruyen.I.phutMoiTieng = 0.05f;
        yield return ChoDen(() => LocNhim.dangCanh, 60, "cảnh Bad bắt đầu");
        yield return Cho(3); yield return Chup("bad_a"); yield return Cho(6); yield return Chup("bad_b");
        yield return ChoDen(() => !LocNhim.dangCanh, 40, "cảnh Bad xong"); yield return Cho(2); yield return Chup("bad_sau");

        yield return ChoDen(() => LocFirstPerson.chuGiua != null, 20, "Bad: màn chữ cuối"); yield return Chup("bad_chu_cuoi");
        yield return Cho(8); yield return Chup("bad_ve_menu");
        Goi(typeof(LocMenu), null, "Vao", 3, null, null); yield return Cho(4);
        Muc("GOOD (placeholder: gọi thẳng cảnh mép giường)");
        LocNhim.I.CanhGood(); yield return Cho(3); yield return Chup("good_1"); yield return Cho(5); yield return Chup("good_2");
        yield return ChoDen(() => !LocNhim.dangCanh, 30, "cảnh Good xong"); yield return Cho(2); yield return Chup("good_sau");

        yield return ChoDen(() => LocFirstPerson.chuGiua != null, 20, "Good: màn chữ cuối"); yield return Chup("good_chu_cuoi");
        yield return Cho(8); yield return Chup("good_ve_menu");
        Goi(typeof(LocMenu), null, "Vao", 3, null, null); yield return Cho(4);
        Muc("EVIL (placeholder: gọi thẳng tiếng gọi từ trên gác)");
        LocNhim.I.GoiTuTrenGac(); yield return Cho(4); yield return Chup("evil_1");
        yield return ChoDen(() => LocFirstPerson.chuGiua != null, 20, "Evil: màn chữ cuối"); yield return Chup("evil_chu_cuoi");
        yield return Cho(8); yield return Chup("evil_ve_menu");

        Muc("MENU — Ending");
        foreach (var k in new[] { "good", "bad", "evil", "death" }) G($"ending {k}: đã ghi {PlayerPrefs.GetInt("LOC_Ket_" + k, 0)} lần");
        yield return Chup("ve_menu");
    }
}
#endif
