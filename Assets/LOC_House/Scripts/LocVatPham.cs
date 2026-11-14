using UnityEngine;

// [6/10] Món đồ trong nhà (kịch bản v3.1 mục 6.1, 8): nhặt vào túi (Tab), đọc tại chỗ, hoặc chỉ xem — một câu độc thoại.
public class LocVatPham : LocTuongTac
{
    public enum Kieu { Nhat, Doc, Xem }   // Nhat: vào túi · Doc: đọc tại chỗ, không lấy · Xem: chỉ độc thoại
    public string id, ten;
    public Kieu kieu = Kieu.Nhat;
    [TextArea] public string ghiChu;          // dòng mô tả trong túi
    [TextArea(3, 12)] public string[] trang;  // nội dung đọc được (rỗng = không đọc)
    public string[] khiNhat;                  // độc thoại khi nhặt / đọc / xem
    public int tuDem = 1;                     // trước đêm này chưa động vào được
    public string chuaToi = "";               // câu nói khi chưa tới đêm ("" = không hiện gợi ý)
    public int docTuDem;                      // mở đọc trong túi trước đêm này → chuaDoc
    public string chuaDoc = "Mai hẵng đọc.";
    public bool docKhiNhat;                   // [14/11] nhặt xong mở ra đọc ngay (mảnh giấy câu đố mã két)
    public string canCo, chuaCo;              // [v3.3] cần cờ này mới lấy được (vd thắp hương ông bà trước khi mở tủ thờ)
    bool ThieuCo => !string.IsNullOrEmpty(canCo) && !LocTui.Co(canCo);

    // [14/11] sổ thu chi 2002 (nướng sẵn trong scene) — sửa khi chạy:
    //  · bỏ dòng chú thích lộ liễu "(dòng cuối cùng… nét của bố)"
    //  · dòng tháng 8 thành "Để riêng cho thằng Phong" (nét mẹ) — khoản mẹ năm nào cũng để cho đứa con đã mất
    //  · thêm trang đầu: mảnh giấy kẹp bìa = SỐ THỨ NHẤT của mã két (câu đố 4 số — LocCotTruyen_ManhMa.cs)
    void Awake()
    {
        if (id != "so_thu_chi_2002" || trang == null) return;
        for (int i = 0; i < trang.Length; i++)
        {
            var t = trang[i]; if (t == null || !t.Contains("Ứng cho thằng Phong")) continue;
            int c = t.IndexOf("\n\n<i>(dòng cuối"); if (c >= 0) t = t.Substring(0, c);
            t = t.Replace("<size=23><i>4/8      Ứng cho thằng Phong ....... 1.980.000</i></size>", "4/8      Để riêng cho thằng Phong ....... 2.000.000");
            trang[i] = t;
        }
        if (trang.Length > 0 && trang[0] != null && !trang[0].Contains("Số thứ nhất"))
        {
            var l = new System.Collections.Generic.List<string>(trang);
            l.Insert(0, "<i>Kẹp trong bìa sổ, một mảnh giấy xé. Chữ mẹ.</i>\n\n\n<size=30><b>Số thứ nhất:  1</b></size>\n\n\nNăm nào em cũng để riêng cho thằng Phong một khoản,\nghi vào sổ như nó vẫn còn.\n\nÔng bảo vô lý.\nEm vẫn ghi.");
            trang = l.ToArray();
        }
    }

    public bool Toi => LocMa.dem >= tuDem;
    public bool DocDuoc => trang != null && trang.Length > 0 && LocMa.dem >= docTuDem;

    public override string GoiY =>
        !Toi ? (chuaToi.Length > 0 ? "[E] " + ten : "")
        : ThieuCo ? "[E] " + ten
        : kieu == Kieu.Nhat ? "[E] Lấy " + ten : kieu == Kieu.Doc ? "[E] Đọc " + ten : "[E] " + ten;

    public override void Bam()
    {
        if (!Toi) { LocTui.Noi(chuaToi); return; }
        if (ThieuCo) { LocTui.Noi(chuaCo); return; }
        if (kieu == Kieu.Nhat) { LocTui.Them(this); gameObject.SetActive(false); if (docKhiNhat && DocDuoc) LocTui.Doc(this); }
        else if (kieu == Kieu.Doc) LocTui.Doc(this);
        LocTui.Noi(khiNhat);
    }
}
