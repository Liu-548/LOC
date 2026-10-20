using UnityEngine;

// [8/10] Việc trong cốt truyện (thắp hương…): nhìn vào rồi bấm E. Chữ gợi ý và việc xảy ra do LocCotTruyen quyết theo id.
public class LocViec : LocTuongTac
{
    public string id;
    public override string GoiY => LocCotTruyen.I ? LocCotTruyen.I.GoiY(id) : "";
    public override void Bam() { if (LocCotTruyen.I) LocCotTruyen.I.Bam(this); }
}
