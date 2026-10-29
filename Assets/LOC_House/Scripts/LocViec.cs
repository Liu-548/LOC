using UnityEngine;

// [8/10] Việc trong cốt truyện (thắp hương…): nhìn vào rồi bấm E. Chữ gợi ý và việc xảy ra do LocCotTruyen quyết theo id.
public class LocViec : LocTuongTac
{
    public string id;
    public override string GoiY => LocCotTruyen.I ? LocCotTruyen.I.GoiY(id) : "";
    public override void Bam() { if (LocCotTruyen.I) LocCotTruyen.I.Bam(this); }

    // [27/10 chơi thử] "nhìn vào lẵng hoa bàn thờ vong vẫn chọn được": hộp va chạm bao cả bộ bàn thờ 2,8 m (cả lẵng hoa, vòng hoa hai bên).
    //  Chỉ nhận khi tia nhìn trúng dải giữa (quanh bát hương, di ảnh) — nuaRong m mỗi bên tâm, theo chiều dài của hộp. 0 = cả vật.
    public float nuaRong = -1f;
    BoxCollider hop;
    public override bool NhanTai(Vector3 diem)
    {
        if (nuaRong < 0f) nuaRong = id == "thap_huong" ? 0.55f : 0f;
        if (nuaRong <= 0f) return true;
        if (!hop) hop = GetComponent<BoxCollider>();
        if (!hop) return true;
        var p = hop.transform.InverseTransformPoint(diem) - hop.center;
        var s = Vector3.Scale(hop.size, hop.transform.lossyScale);
        return Mathf.Abs(s.x) >= Mathf.Abs(s.z) ? Mathf.Abs(p.x * hop.transform.lossyScale.x) <= nuaRong : Mathf.Abs(p.z * hop.transform.lossyScale.z) <= nuaRong;
    }
}
