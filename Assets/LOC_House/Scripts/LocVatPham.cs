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

    public bool Toi => LocMa.dem >= tuDem;
    public bool DocDuoc => trang != null && trang.Length > 0 && LocMa.dem >= docTuDem;

    public override string GoiY =>
        !Toi ? (chuaToi.Length > 0 ? "[E] " + ten : "")
        : kieu == Kieu.Nhat ? "[E] Lấy " + ten : kieu == Kieu.Doc ? "[E] Đọc " + ten : "[E] " + ten;

    public override void Bam()
    {
        if (!Toi) { LocTui.Noi(chuaToi); return; }
        if (kieu == Kieu.Nhat) { LocTui.Them(this); gameObject.SetActive(false); }
        else if (kieu == Kieu.Doc) LocTui.Doc(this);
        LocTui.Noi(khiNhat);
    }
}
