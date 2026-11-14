using System.Collections.Generic;
using UnityEngine;

// [14/11] Gợi ý phím H theo bảng dữ liệu (thay chuỗi if rải ở GoiYH / GoiYHV34 / GoiYGiacMo).
//  · Mỗi mục: id, đêm, thứ tự trong đêm (số nhỏ = việc chặn sớm hơn), bắt buộc hay tuỳ chọn, mo() = đã mở khoá, xong() = đã làm,
//    bac[] = 3 câu Khôi tự nhủ: mơ hồ → thu hẹp → gần như chỉ chỗ. Không câu nào nói mã số, tên phím hay "bấm E".
//  · Bấm H: chọn mục bắt buộc đang mở có thứ tự nhỏ nhất (tuỳ chọn chỉ khi hết bắt buộc). Đọc bậc kế tiếp.
//    Đã qua bậc 3 mà còn mục bắt buộc khác đang mở song song → chuyển sang mục đó. Bấm lại sau hơn giayResetGoiY giây → quay về bậc 1.
//  · Gợi ý chủ động (goiYChuDong): mục đứng đầu không đổi quá phutChuDong phút và chưa bấm H cho nó → Khôi lẩm bẩm bậc 1, mỗi mục một lần.
//  · Chỗ rẽ cuối chỉ đặt câu hỏi, không nghiêng về Niêm hay Xé. Quyển sách của mẹ không có trong gợi ý (đọc hay không đổi kết Niêm).
//  · Nội dung bản nháp: tài liệu "Nâng cấp gợi ý phím H — LỘC" (14/11) — câu mới / sửa đều chờ nhóm duyệt 🗣.
public partial class LocCotTruyen
{
    [Header("Gợi ý H")]
    public float giayResetGoiY = 90f;     // bấm H sau quãng này thì bắt đầu lại từ bậc nhẹ
    public bool goiYChuDong = true;       // Khôi tự lẩm bẩm khi mắc quá lâu
    public float phutChuDong = 4f;

    class MucGoiY
    {
        public string id; public int dem, thuTu; public bool batBuoc = true;
        public System.Func<bool> mo, xong;
        public string[] bac;
    }
    List<MucGoiY> bangGoiY;
    readonly Dictionary<string, float> lanBamGoiY = new();   // lần bấm H gần nhất cho từng mục
    readonly HashSet<string> daNhacChuDong = new();
    string mucDau; float tMucDau; bool daNhacGio11;

    LocDoor cuaHamD;
    bool DenHamBat => ctHam && ctHam.bat;
    bool CuaHamMo => LocTui.Co("d1_xuong_ham") || (cuaHamD && (!cuaHamD.khoa || cuaHamD.DangMo));
    bool DuDoMam => canMam.Count == 0 || !canMam.Exists(x => !LocTui.CoMon(x));

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
        Muc("d1_cua_ham", 1, 30, () => LocNhim.daTham[1] && !LocTui.Co("thu_ket") && !CoChia(), () => CuaHamMo,
            "Cái cửa sắt ngoài giếng trời. Bố khóa từ bao giờ.", "Khóa treo bên ngoài. Chìa thì bố cất kỹ lắm.", "Đồ quý bố mẹ toàn cho vào két trong phòng.");
        Muc("ma_ket", 1, 31, () => LocTui.Co("thu_ket"), () => LocTui.Co("ket_mo"),
            "Bố mẹ chắc gì nhớ được mã. Phải ghi ở đâu đó.", "Giấy tờ nhà mình toàn để bên phòng làm việc của bố.", "...Sao lại có dòng ứng tiền cho anh Phong.");
        Muc("d1_lay_chia", 1, 32, () => LocTui.Co("ket_mo"), () => CoChia() || CuaHamMo,
            "Két mở rồi.", "Trong ấy có chùm chìa lạ.", "Chùm chìa với cái khóa treo to. Chắc là của cái cửa kia.");
        Muc("d1_mo_cua_ham", 1, 33, () => CoChia(), () => CuaHamMo,
            "Có chìa rồi. Thử cái cửa sắt xem.", "Cửa ở giếng trời, sau bếp.", "Bức tường bên trái giếng trời.");
        Muc("d1_bat_den", 1, 34, () => CuaHamMo && !LocTui.Co("d1_xuong_ham"), () => DenHamBat || LocTui.Co("d1_xuong_ham"),
            "Dưới đấy tối om.", "Đi mò thế này thì thấy gì. Phải có cái công tắc.", "Đầu cầu thang, chỗ có chữ viết trên tường.");
        Muc("d1_xuong_ham", 1, 35, () => CuaHamMo && DenHamBat, () => LocTui.Co("d1_xuong_ham"),
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
        Muc("loi_nhan", 3, 20, () => KhoaHeHu, () => !KhoaHeHu,
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

    void GoiYH()
    {
        if (dangCanh || LocMenu.DangChe) return;
        if (LocMa.dem == 3 && LocMa.MatDien) { LocTui.Noi("Đứng yên. Đừng đi về phía nó."); return; }
        var m = MucDangMo();
        if (m == null) { LocTui.Noi("Chẳng còn gì phải làm. ...Hay mình quên cái gì."); return; }
        bacGoiY.TryGetValue(m.id, out int i);
        if (lanBamGoiY.TryGetValue(m.id, out float t0) && Time.time - t0 > giayResetGoiY) i = 0;   // lâu không hỏi → bắt đầu lại từ bậc nhẹ
        if (i >= m.bac.Length)
        {
            // đã qua bậc cuối: còn việc song song thì chuyển sang (từ bậc 1), không thì nhắc lại bậc cuối
            var khac = MucDangMo(m.id);
            if (khac != null && khac.batBuoc && khac.thuTu / 10 == m.thuTu / 10) { bacGoiY[m.id] = 0; m = khac; bacGoiY.TryGetValue(m.id, out i); if (i >= m.bac.Length) i = m.bac.Length - 1; }
            else i = m.bac.Length - 1;
        }
        LocTui.Noi(m.bac[i]);
        bacGoiY[m.id] = i + 1; lanBamGoiY[m.id] = Time.time;
        daNhacChuDong.Add(m.id);
    }

    // gọi mỗi khung hình từ Update (sau khi đã loại cảnh): lẩm bẩm khi mắc quá lâu + mốc giờ thứ 11 của Đêm 3
    void GoiYChuDong()
    {
        if (LocFirstPerson.khoa || LocTui.Mo || LocMenu.DangChe || LocNhim.dangCanh || dangKet) return;
        if (LocMa.dem == 3 && gio >= 11 && gio < gioHet && !daNhacGio11) { daNhacGio11 = true; LocTui.Noi("Mười một giờ rồi. Qua nửa đêm là sang mùng một."); }
        if (!goiYChuDong || (LocMa.dem == 3 && LocMa.MatDien)) return;
        if (Time.frameCount % 30 != 0) return;   // nửa giây một lần là đủ
        var m = MucDangMo(); var id = m != null ? m.id : null;
        if (id != mucDau) { mucDau = id; tMucDau = Time.time; return; }
        if (id == null || daNhacChuDong.Contains(id) || Time.time - tMucDau < phutChuDong * 60f) return;
        daNhacChuDong.Add(id);
        LocTui.Noi(m.bac[0]);
        bacGoiY[id] = Mathf.Max(bacGoiY.TryGetValue(id, out int i) ? i : 0, 1); lanBamGoiY[id] = Time.time;
    }
}
