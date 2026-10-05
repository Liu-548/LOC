using UnityEngine;

// [5/10] Chậu nước cạnh giường Nhím: [E] Thay khăn — chỉ hiện khi lần thăm Đêm 1–2 tới bước thay khăn (LocNhim).
public class LocNhimChau : MonoBehaviour
{
    public string GoiY => LocNhim.I && LocNhim.I.ChoThay ? "[E] Thay khăn" : "";
    public void Bam() { if (LocNhim.I) LocNhim.I.ThayKhan(); }
}
