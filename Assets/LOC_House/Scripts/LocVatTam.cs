using UnityEngine;

// [27/10] Vật tương tác dựng lúc chạy cho phần cốt truyện v3.3 (gói hàng, chỗ đặt bát cháo…): gợi ý và việc làm do LocCotTruyen gán.
//  Không có việc (gợi ý rỗng) thì tắt va chạm, để tia nhìn đi xuyên tới vật phía sau (vd mini game dán chén trên cùng bàn thờ).
public class LocVatTam : LocTuongTac
{
    public System.Func<string> goiY;
    public System.Action bam;
    public override string GoiY => goiY != null ? goiY() ?? "" : "";
    public override void Bam() { if (GoiY.StartsWith("[E]")) bam?.Invoke(); }
    Collider vc;
    void Start() { vc = GetComponent<Collider>(); }
    void Update() { if (vc) vc.enabled = GoiY.Length > 0; }
}
