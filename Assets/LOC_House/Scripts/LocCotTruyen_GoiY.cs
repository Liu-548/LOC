using System.Collections.Generic;
using UnityEngine;

// [14/11] Gợi ý theo bảng dữ liệu — KHÔNG CÒN PHÍM H: Khôi tự nói ra khi người chơi mắc lâu (xem GoiYChuDong).
//  · Mỗi mục: id, đêm, thứ tự trong đêm (số nhỏ = việc chặn sớm hơn), bắt buộc hay tuỳ chọn, mo() = đã mở khoá, xong() = đã làm,
//    bac[] = 3 câu Khôi tự nhủ: mơ hồ → thu hẹp → gần như chỉ chỗ. Không câu nào nói mã số, tên phím hay "bấm E".
//  · Chọn mục bắt buộc đang mở có thứ tự nhỏ nhất (tuỳ chọn chỉ khi hết bắt buộc).
//  · Chỗ rẽ cuối chỉ đặt câu hỏi, không nghiêng về Niêm hay Xé. Quyển sách của mẹ không có trong gợi ý (đọc hay không đổi kết Niêm).
//  · Nội dung bản nháp: tài liệu "Nâng cấp gợi ý phím H — LỘC" (14/11) — câu mới / sửa đều chờ nhóm duyệt.
public partial class LocCotTruyen
{
    [Header("Gợi ý tự động (lời Khôi)")]
    public bool goiYChuDong = true;                          // tắt = không gợi ý gì cả
    // [14/11 chơi thử: "Đêm 1 không biết làm gì"] việc mới mở là Khôi nói ngay bậc 1 (mục tiêu), rồi rõ dần nhanh hơn
    public float[] giayMac = { 5f, 45f, 100f };              // giây (chỉ tính lúc tự do đi lại) kể từ khi việc mở → nói bậc 1 / 2 / 3
    public float giayNhacLai = 90f;                          // sau bậc 3: bao lâu nhắc lại một lần
    public int soNhacLai = 3;
    public bool hienMucTieu = true;                          // góc trên trái: dòng mờ ghi câu Khôi vừa tự nhủ cho việc đang làm

    class MucGoiY
    {
        public string id; public int dem, thuTu; public bool batBuoc = true;
        public System.Func<bool> mo, xong;
        public string[] bac;
    }
    List<MucGoiY> bangGoiY;
    string mucDau; bool daNhacGio11;

    LocDoor cuaHamD;
    bool DenHamBat => ctHam && ctHam.bat;
    // [14/11] người chơi tự thấy cửa sắt giếng trời: đứng gần ~2,5 m và nhìn về phía nó (hoặc đã có cờ cũ)
    bool ThayCuaHam => LocTui.Co("thay_cua_ham") || LocTui.Co("d1_cay_ham") || CuaHamMo;
    void KiemKhamPha()
    {
        if (LocMa.dem != 1 || !cuaHamT || LocTui.Co("thay_cua_ham")) return;
        var c = Camera.main; if (!c) return;
        var v = cuaHamT.position - c.transform.position; v.y = 0;
        if (v.magnitude > 2.6f) return;
        var f = c.transform.forward; f.y = 0;
        if (Vector3.Dot(f.normalized, v.normalized) < 0.55f) return;
        LocTui.Dat("thay_cua_ham");
        if (!CoChia()) LocTui.Noi("Cửa sắt... khóa treo bên ngoài.", "Dưới này là chỗ nào mà bố phải khóa kỹ thế.", "...Chìa gì quan trọng bố cũng khóa vào két.");
    }
    bool CuaHamMo => LocTui.Co("d1_xuong_ham") || (cuaHamD && (!cuaHamD.khoa || cuaHamD.DangMo));
    bool DuDoMam => canMam.Count == 0 || !canMam.Exists(x => !LocTui.CoMon(x));

    // [14/11 chơi thử] BẢNG NHIỆM VỤ: Khôi nói xong câu đầu → hiện nhiệm vụ rõ ràng ở góc trên trái (không đánh đố)
    static readonly Dictionary<string, string> NV = new()
    {
        // Đêm 1
        { "d1_thap_huong",  "Thắp hương cho bố mẹ ở bàn thờ phòng khách" },
        { "d1_tham_nhim",   "Lên tầng 2, lại gần giường xem tình trạng của Nhím" },
        { "d1_kham_pha",    "Có tiếng đập cửa sắt dưới nhà — xuống tầng 1 tìm xem phát ra từ đâu" },
        { "d1_cua_ham",     "Cửa sắt ở giếng trời bị khoá. Chìa có lẽ bố cất trong két sắt phòng bố mẹ" },
        { "tim_giay_me",    "Két sắt bị khoá. Bố hay quên mã — mẹ chắc có ghi lại. Tìm trong đồ của mẹ" },
        { "tim_manh",       "Mã két là năm anh Phong ra đời. Tìm 4 con số mẹ giấu ({n}/4)" },
        { "ghep_ma",        "Nhập mã vào két sắt: ghép 4 số theo thứ tự" },
        { "d1_tim_cua",     "Chùm chìa này mở cửa nào? Tìm chỗ có khoá treo" },
        { "d1_lay_chia",    "Lấy đồ trong két sắt" },
        { "d1_mo_cua_ham",  "Thử chùm chìa vào cửa sắt ở giếng trời" },
        { "d1_bat_den",     "Tìm công tắc đèn ở đầu cầu thang hầm" },
        { "d1_xuong_ham",   "Xuống hầm" },
        { "d1_giu_huong",   "Thắp nén hương mới ở bàn thờ — đừng để hương tắt" },
        { "d1_di_ngu",      "Về phòng mình đi ngủ" },
        // Đêm 2
        { "d2_tham_nhim",   "Lên tầng 2, lại gần giường xem Nhím thế nào" },
        { "d2_goi_khong_loi", "Nghe điện thoại bàn ở phòng khách" },
        { "d2_doc_so",      "Đọc hết cuốn sổ của bố (lấy trong két)" },
        { "goi_thay",       "Gọi cho ông thầy — số ghi ở bìa sau cuốn sổ" },
        { "dan_chen",       "Ông thầy dặn: dán lại hai cái chén vỡ dưới hầm" },
        { "cung_chao",      "Ông thầy dặn: cúng một bát cháo dưới hầm" },
        { "d2_giu_huong",   "Giữ hương ở bàn thờ không tắt" },
        { "di_ngu",         "Về phòng mình đi ngủ" },
        // Đêm 3
        { "mo_kien",        "Có gói đồ lạ ai để ở cửa chính — mở ra xem" },
        { "loi_nhan",       "Ai gửi gói đồ này? Mẹ có biết chuyện gì không — xem lại đồ của mẹ" },
        { "d3_bat_den",     "Bật đèn hầm ở đầu cầu thang" },
        { "he_hu",          "Xuống hầm xem cái hũ dưới gầm bàn thờ" },
        { "do_mam",         "Gom đủ đồ cúng cho mâm dưới hầm" },
        { "bay_mam",        "Bày mâm cúng dưới hầm theo lời thầy" },
        { "nl_doc_van",     "Đọc bài khấn trước bàn thờ dưới hầm" },
        { "giu_nhang",      "Thắp nhang và giữ cho nhang cháy hết" },
        { "chon_hu",        "Quyết định số phận cái hũ: niêm lại, hay xé niêm?" },
    };
    float tNVMoi; string nvTruoc, gNVChu, gNghiChu; GUIContent gNV, gNghi;

    void Muc(string id, int dem, int thuTu, System.Func<bool> mo, System.Func<bool> xong, params string[] bac)
        => bangGoiY.Add(new MucGoiY { id = id, dem = dem, thuTu = thuTu, mo = mo, xong = xong, bac = bac });

    void DungBangGoiY()
    {
        bangGoiY = new List<MucGoiY>();
        if (cuaHamT) { cuaHamD = cuaHamT.GetComponentInChildren<LocDoor>(true); if (!cuaHamD) cuaHamD = cuaHamT.GetComponentInParent<LocDoor>(); }

        // ── Đêm 1
        Muc("d1_thap_huong", 1, 10, () => true, () => LocTui.Co("thap_huong"),
            "Về tới nhà rồi mà chưa thắp cho bố mẹ nén nào.", "Bàn thờ dựng ngoài phòng khách, trước linh cữu.", "Đứng cho ngay ngắn trước bàn thờ. Đứng lệch thì thắp gì.");
        Muc("d1_tham_nhim", 1, 20, () => LocTui.Co("thap_huong"), () => LocNhim.daTham[1],
            "Con bé ốm từ hôm qua. Chưa lên xem nó.", "Phòng nó trên tầng hai.", "Khăn trên trán chắc khô rồi. Chậu nước để cạnh giường.");
        // [14/11] KHÁM PHÁ TRƯỚC, NHIỆM VỤ SAU: thăm em xong chỉ có "đi một vòng nhà"; tự thấy cửa sắt (ThayCuaHam) / tự thử két mới mở nhiệm vụ
        Muc("d1_kham_pha", 1, 25, () => LocNhim.daTham[1] && LocTui.Co("go_ham") && !ThayCuaHam && !LocTui.Co("thu_ket") && !CoChia(), () => ThayCuaHam || LocTui.Co("thu_ket") || CoChia(),
            "Tiếng sắt. Như ai đập cửa.", "Hình như từ phía sau bếp.", "Giếng trời sau bếp... Hồi bé bố cấm ra đấy chơi.");
        // (tiếng đập vọng từ hầm khi xuống tầng 1 gần giếng trời — LocCotTruyen_ManhMa.Dem1ManhMa)
        Muc("d1_cua_ham", 1, 30, () => ThayCuaHam && !LocTui.Co("thu_ket") && !CoChia(), () => CuaHamMo || LocTui.Co("thu_ket") || CoChia(),
            "Chìa gì quan trọng bố cũng khóa vào két.", "Cái két sắt trong phòng bố mẹ.", "Phòng bố mẹ trên tầng hai. Két đứng ở góc cửa sổ.");
        // [14/11] mã két = năm sinh anh Phong, mẹ giấu 4 số ở 4 chỗ (LocCotTruyen_ManhMa.cs)
        Muc("tim_giay_me", 1, 30, () => LocTui.Co("thu_ket") && !LocTui.Co("doc:giay_ket") && SoManh() < 4 && !LocTui.Co("ket_mo"), () => LocTui.Co("doc:giay_ket") || SoManh() >= 4 || LocTui.Co("ket_mo"),
            "Mẹ cẩn thận lắm. Cái gì cũng ghi.", "Đồ của mẹ... ngay trong phòng này.", "Bàn trang điểm của mẹ.");
        Muc("tim_manh", 1, 31, () => LocTui.Co("doc:giay_ket") && SoManh() < 4 && !LocTui.Co("ket_mo"), () => SoManh() >= 4 || LocTui.Co("ket_mo"),
            "Bốn chỗ còn giữ hơi anh Phong.", "Sổ thu chi của mẹ. Bàn học của mình. Bàn nước. Chỗ con Nhím ngủ.", "Sổ thu chi trong tủ hồ sơ phòng bố. Hộc bàn học phòng mình. Khay trà phòng khách. Đầu giường con bé.");
        Muc("ghep_ma", 1, 32, () => SoManh() >= 4 && !LocTui.Co("ket_mo"), () => LocTui.Co("ket_mo"),
            "Đủ bốn số rồi.", "Một, chín, tám, không.", "Năm anh Phong ra đời. Gõ vào két.");
        Muc("d1_lay_chia", 1, 33, () => LocTui.Co("ket_mo"), () => CoChia() || CuaHamMo,
            "Két mở rồi.", "Trong ấy có chùm chìa lạ.", "Chùm chìa với cái khóa treo to. Chắc là của cái cửa kia.");
        Muc("d1_tim_cua", 1, 34, () => CoChia() && !ThayCuaHam, () => ThayCuaHam || CuaHamMo,
            "Chùm chìa lạ với cái khóa treo to. Khóa cái gì đây.", "Trong nhà có chỗ nào khóa treo đâu nhỉ.", "Giếng trời sau bếp... hình như có cái cửa sắt.");
        Muc("d1_mo_cua_ham", 1, 35, () => CoChia() && ThayCuaHam, () => CuaHamMo,
            "Có chìa rồi. Thử cái cửa sắt xem.", "Cửa ở giếng trời, sau bếp.", "Bức tường bên trái giếng trời.");
        Muc("d1_bat_den", 1, 36, () => CuaHamMo && !LocTui.Co("d1_xuong_ham"), () => DenHamBat || LocTui.Co("d1_xuong_ham"),
            "Dưới đấy tối om.", "Đi mò thế này thì thấy gì. Phải có cái công tắc.", "Đầu cầu thang, chỗ có chữ viết trên tường.");
        Muc("d1_xuong_ham", 1, 37, () => CuaHamMo && DenHamBat, () => LocTui.Co("d1_xuong_ham"),
            "Đèn sáng rồi. Xuống xem có gì.", "Xuống hẳn dưới hầm.", "Xuống tận cuối thang.");
        Muc("d1_giu_huong", 1, 40, () => ChoGiuHuong(1), () => LocTui.Co("giu_huong_1"),
            "Hương trên linh cữu chắc tàn rồi.", "Bác dặn đêm nay đừng để tắt hương.", "Thắp nén mới ở bàn thờ dưới nhà. Đứng cho ngay.");
        Muc("d1_di_ngu", 1, 50, () => LocTui.Co("giu_huong_1"), () => false,
            "Muộn rồi. Đi nằm thôi.", "Về phòng mình.", "Phòng mình cạnh phòng con bé.");

        // ── Đêm 2
        Muc("d2_tham_nhim", 2, 10, () => true, () => LocNhim.daTham[2],
            "Lên xem con bé thế nào.", "Nó vẫn nằm trên phòng.", "Thay cái khăn cho nó.");
        Muc("d2_goi_khong_loi", 2, 20, () => LocNhim.daTham[2] && CoGoiThay, () => LocTui.Co("goi_2"),
            "Lúc nãy hình như có tiếng chuông.", "Dưới phòng khách.", "Cái điện thoại bàn.");
        Muc("d2_doc_so", 2, 30, () => LocTui.Co("goi_2") || !CoGoiThay, () => LocNhim.daHoangLoan,
            "Bố hay ghi chép. Chắc phải có cuốn sổ nào đấy.", "Cuốn sổ lấy trong két hôm qua.", "Đọc cho hết. Tới tận bìa sau.");
        Muc("goi_thay", 2, 40, () => CoGoiThay && LocNhim.daHoangLoan, () => LocTui.Co("goi_thay"),
            "Bố có ghi số ông thầy.", "Bìa sau cuốn sổ.", "Điện thoại dưới phòng khách.");
        Muc("dan_chen", 2, 50, () => LocTui.Co("goi_thay") && mgDanChen && vpKeo && vpChen3, () => LocTui.Co("dan_chen"),
            "Ông ấy bảo dán hai cái chén dưới hầm.", "Keo thì cửa hàng nhà mình có bán, trong quầy.", "Còn cái chén trên phòng thờ tầng ba.");
        Muc("cung_chao", 2, 51, () => LocTui.Co("goi_thay") && vpChao && choDatChao, () => LocTui.Co("cung_chao"),
            "Còn bát cháo.", "Bác để cháo trên bàn ăn dưới bếp.", "Bưng xuống bàn thờ dưới hầm.");
        Muc("d2_giu_huong", 2, 60, () => ChoGiuHuong(2), () => LocTui.Co("giu_huong_2"),
            "Hương sắp tàn.", "Đêm nay vẫn phải giữ hương.", "Bàn thờ dưới nhà. Đứng cho ngay.");
        Muc("di_ngu", 2, 70, () => LocTui.Co("nua_tinh_2"), () => LocTui.Co("mo_2_xong"),
            "Muộn rồi. Về phòng mình ngủ.", "Phòng mình ngay cạnh phòng em.", "Nằm xuống giường đi.");

        // ── Đêm 3
        Muc("mo_kien", 3, 10, () => kienHang && LocTui.Co("kien_den"), () => LocTui.Co("mo_kien"),
            "Có cái gói ai để ở cửa.", "Giấy dầu, buộc dây... trông quen mắt.", "Cạnh cửa chính.");
        Muc("loi_nhan", 3, 20, () => KhoaHeHu && (LocTui.Co("mo_kien") || !kienHang), () => !KhoaHeHu,
            "Mẹ có để lại gì không.", "Phòng bố mẹ.", "Bàn trang điểm của mẹ.");
        Muc("d3_bat_den", 3, 29, () => !KhoaHeHu && !LocTui.Co("nl_giu_nhang") && ctHam, () => DenHamBat,
            "Dưới hầm tối lắm.", "Bật đèn lên rồi hẵng xuống.", "Công tắc ở đầu cầu thang.");
        Muc("he_hu", 3, 30, () => !KhoaHeHu, () => LocMa.huDaHe,
            "Cái hũ dưới hầm.", "Mẹ dặn đừng động vào. Nhưng phải xem trong ấy có gì.", "Xuống hầm, ngồi xuống trước gầm bàn thờ.");
        Muc("do_mam", 3, 40, () => mgBayMam && LocMa.huDaHe, () => DuDoMam || LocTui.Co("nl_bay_mam"),
            "Mâm cúng còn thiếu đồ.", "Gạo với muối để chỗ linh cữu.", "Mía người ta mua cho đám, để dưới bếp.");
        Muc("bay_mam", 3, 41, () => mgBayMam && LocMa.huDaHe && DuDoMam, () => LocTui.Co("nl_bay_mam"),
            "Đồ cúng đủ rồi.", "Ông ấy dặn bày mâm dưới hầm.", "Bày theo lời thầy. Tờ giấy chép vẫn còn trong túi.");
        Muc("nl_doc_van", 3, 50, () => LocTui.Co("nl_bay_mam") && vpVanKhan, () => LocTui.Co("nl_doc_van"),
            "Còn bài khấn.", "Đứng trước bàn thờ dưới hầm mà đọc.", "Mở trong túi, lật tới trang cuối.");
        Muc("giu_nhang", 3, 60, () => LocTui.Co("nl_doc_van") && mgGiuNhang, () => LocTui.Co("nl_giu_nhang"),
            "Còn nén nhang.", "Thắp lên rồi giữ cho nó cháy hết.", "Gió lùa thì che lại. Đừng buông tay.");
        Muc("chon_hu", 3, 70, () => LocHuCot.ChoChon, () => LocTui.Co("da_chon_hu"),
            "Còn cái hũ.", "Ông thầy bảo niêm lại. Sách của mẹ thì nói khác.", "Nhốt lại cho yên nhà, hay để người ta đi. Ai đúng?");
    }
    bool CoChia() => LocTui.Co("chia_ham") || LocTui.CoMon("chia_ham");

    // mục bắt buộc đang mở, thứ tự nhỏ nhất trước (bỏ qua 'tru')
    MucGoiY MucDangMo(string tru = null)
    {
        if (bangGoiY == null) DungBangGoiY();
        MucGoiY tot = null; int d = LocMa.dem;
        foreach (var m in bangGoiY)
        {
            if (m.dem != d || m.id == tru) continue;
            bool mo, xong;
            try { mo = m.mo(); xong = m.xong(); } catch { continue; }   // vật chưa dựng ở scene này → bỏ mục
            if (!mo || xong) { if (xong) bacGoiY.Remove(m.id); continue; }
            if (tot == null || (m.batBuoc && !tot.batBuoc) || (m.batBuoc == tot.batBuoc && m.thuTu < tot.thuTu)) tot = m;
        }
        return tot;
    }

    // [14/11] BỎ PHÍM H — gợi ý tự động bằng lời Khôi tự nhủ (phụ đề nghiêng như mọi câu độc thoại).
    //  · Đồng hồ "mắc" chỉ chạy khi người chơi đang tự do đi lại (không menu, không túi, không cảnh, không bị khoá) và
    //    đặt lại mỗi khi việc đứng đầu đổi → làm xong một việc là khỏi bị nhắc việc cũ.
    //  · giayMac[0] sau khi việc mở → bậc 1 (nêu việc) · giayMac[1] → bậc 2 (thu hẹp) · giayMac[2] → bậc 3 (gần như chỉ chỗ).
    //    Sau bậc 3: cứ giayNhacLai nhắc lại bậc 3 (hoặc việc song song cùng chặng), tối đa soNhacLai lần.
    //  · Câu vừa nói được giữ mờ ở góc trên trái (hienMucTieu) cho người lỡ không đọc kịp phụ đề.
    //  · Không chen ngang khi Khôi đang nói câu khác; chờ câu kia xong.
    float tMac; int lapLai;
    string mucTieuId, mucTieuChu;
    bool daNhacMatDien;

    void NoiGoiY(MucGoiY m, int i)
    {
        i = Mathf.Clamp(i, 0, m.bac.Length - 1);
        LocTui.Noi(m.bac[i]);
        mucTieuId = m.id; mucTieuChu = m.bac[i];
        bacGoiY[m.id] = Mathf.Max(bacGoiY.TryGetValue(m.id, out int c) ? c : 0, i + 1);
    }

    // gọi mỗi khung hình từ Update (sau khi đã loại cảnh)
    void GoiYChuDong()
    {
        if (LocFirstPerson.khoa || LocTui.Mo || LocMenu.DangChe || LocNhim.dangCanh || dangKet) return;
        if (LocMa.dem == 3 && gio >= 11 && gio < gioHet && !daNhacGio11 && !LocTui.DangNoi) { daNhacGio11 = true; LocTui.Noi("Mười một giờ rồi. Qua nửa đêm là sang mùng một."); }

        // Đêm 3 mất điện: nhắc một lần mỗi đợt, rồi im để người chơi tự xoay xở
        if (LocMa.dem == 3 && LocMa.MatDien)
        {
            if (!daNhacMatDien && !LocTui.DangNoi) { daNhacMatDien = true; LocTui.Noi("Đứng yên. Đừng đi về phía nó."); }
            return;
        }
        daNhacMatDien = false;
        KiemKhamPha();
        if (!goiYChuDong) return;

        tMac += Time.deltaTime;
        if (Time.frameCount % 15 != 0) return;
        var m = MucDangMo(); var id = m != null ? m.id : null;
        if (id != mucDau) { mucDau = id; tMac = 0f; lapLai = 0; mucTieuChu = null; return; }   // việc đổi → xoá dòng mục tiêu cũ
        if (m == null || LocTui.DangNoi) return;

        bacGoiY.TryGetValue(id, out int daNoi);   // số bậc đã nói cho việc này
        if (daNoi < m.bac.Length)
        {
            if (tMac < giayMac[Mathf.Min(daNoi, giayMac.Length - 1)]) return;
            NoiGoiY(m, daNoi);
            return;
        }
        // đã nói hết các bậc
        if (lapLai >= soNhacLai || tMac < giayMac[giayMac.Length - 1] + giayNhacLai * (lapLai + 1)) return;
        lapLai++;
        var khac = MucDangMo(id);
        if (khac != null && khac.batBuoc && khac.thuTu / 10 == m.thuTu / 10 && (lapLai % 2 == 1))
        {
            bacGoiY.TryGetValue(khac.id, out int k2);
            NoiGoiY(khac, k2 < khac.bac.Length ? k2 : khac.bac.Length - 1);
        }
        else NoiGoiY(m, m.bac.Length - 1);
    }

    // BẢNG NHIỆM VỤ góc trên trái: tiêu đề · nhiệm vụ (chữ thẳng, rõ) · câu Khôi vừa tự nhủ (nghiêng, mờ)
    void OnGUI()
    {
        if (!hienMucTieu || string.IsNullOrEmpty(mucTieuChu) || mucDau == null) return;
        if (LocFirstPerson.khoa || LocTui.Mo || LocMenu.DangChe || dangCanh || dangKet || LocNhim.dangCanh) return;
        if (LocMa.dem == 3 && LocMa.MatDien) return;
        NV.TryGetValue(mucTieuId ?? "", out string nv);
        if (nv != null && nv.Contains("{n}")) nv = nv.Replace("{n}", SoManh().ToString());
        // trong phòng em: đang chờ thay khăn → nói rõ bước tiếp theo
        if ((mucTieuId == "d1_tham_nhim" || mucTieuId == "d2_tham_nhim") && LocNhim.I && LocNhim.I.ChoThay) nv = "Thay khăn cho Nhím — tới chậu nước cạnh giường";
        // thăm em xong mà chưa nghe tiếng đập → chưa có gì để tìm
        if (mucTieuId == "d1_kham_pha" && !LocTui.Co("go_ham")) nv = null;
        if (nv != nvTruoc) { nvTruoc = nv; tNVMoi = Time.unscaledTime; }
        float moi = Mathf.Clamp01((Time.unscaledTime - tNVMoi) / 0.5f);            // hiện dần 0,5 s
        float nhay = Mathf.Clamp01(1f - (Time.unscaledTime - tNVMoi) / 3f);        // 3 s đầu: viền sáng "nhiệm vụ mới"

        float k = Screen.height / 1080f, w = Mathf.Min(560 * k, Screen.width * 0.42f), x = 30 * k, y = 28 * k, pad = 14 * k;
        var fT = LocMenu.FontChu(2); var fN = LocMenu.FontChu(0); var fI = LocMenu.FontChu(1);
        var sNV = LocKieuChu.Lay("nv.chu", fN, Mathf.RoundToInt(23 * k), FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.95f, 0.93f, 0.88f)); sNV.wordWrap = true;
        var sNghi = LocKieuChu.Lay("nv.nghi", fI, Mathf.RoundToInt(18 * k), fI ? FontStyle.Normal : FontStyle.Italic, TextAnchor.UpperLeft, new Color(0.75f, 0.73f, 0.68f)); sNghi.wordWrap = true;

        if (gNVChu != nv || gNghiChu != mucTieuChu)   // chỉ dựng lại khi chữ đổi (tránh rác bộ nhớ mỗi khung OnGUI)
        {
            gNVChu = nv; gNghiChu = mucTieuChu;
            gNV = new GUIContent(string.IsNullOrEmpty(nv) ? mucTieuChu : nv);
            gNghi = new GUIContent(string.IsNullOrEmpty(nv) ? "" : "Khôi nghĩ: “" + mucTieuChu + "”");   // ghi rõ ai nghĩ — khỏi tưởng nhân vật đứng trước mặt nói
        }
        float hT = 0f, hN = sNV.CalcHeight(gNV, w - 2 * pad), hG = gNghi.text.Length > 0 ? sNghi.CalcHeight(gNghi, w - 2 * pad) + 6 * k : 0;
        var hop = new Rect(x, y, w, pad + hT + hN + hG + pad);

        GUI.color = new Color(0, 0, 0, 0.55f * moi); GUI.DrawTexture(hop, Texture2D.whiteTexture);
        GUI.color = new Color(0.86f, 0.70f, 0.42f, (0.5f + 0.5f * nhay) * moi); GUI.DrawTexture(new Rect(x, y, 3 * k, hop.height), Texture2D.whiteTexture);   // vạch vàng bên trái
        GUI.color = new Color(1, 1, 1, moi);
        GUI.Label(new Rect(x + pad, y + pad * 0.8f + hT, w - 2 * pad, hN), gNV, sNV);
        if (hG > 0) GUI.Label(new Rect(x + pad, y + pad * 0.8f + hT + hN + 6 * k, w - 2 * pad, hG), gNghi, sNghi);
        GUI.color = Color.white;
    }
}
