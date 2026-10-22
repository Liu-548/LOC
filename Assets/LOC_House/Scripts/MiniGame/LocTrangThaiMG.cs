using UnityEngine;

// [22/10] Nhà đổi theo việc người chơi đã làm: mỗi mục bật/tắt một vật theo đêm + cờ trong LocTui.
//  · "trước" (bàn bừa, quần áo trên dây, giấy tờ bày ra): anKhiCo = cờ mini game → làm xong thì biến mất
//  · "sau"  (bàn gọn, đồ đã gấp, bìa hồ sơ mở trên bàn): hienKhiCo = cờ mini game → làm xong thì hiện, giữ qua các đêm sau
// Cờ nằm trong LocTui nên chết → chơi lại đầu đêm cũng tự về đúng trạng thái. Nằm trên LOC_MiniGame (luôn bật), không nằm trong nhóm Dem1–3.
public class LocTrangThaiMG : MonoBehaviour
{
    [System.Serializable]
    public class Muc
    {
        public GameObject vat;
        public int demTu = 1, demDen = 3;
        public string hienKhiCo, anKhiCo;
        public bool Bat => LocMa.dem >= demTu && LocMa.dem <= demDen
                           && (string.IsNullOrEmpty(hienKhiCo) || LocTui.Co(hienKhiCo))
                           && (string.IsNullOrEmpty(anKhiCo) || !LocTui.Co(anKhiCo));
    }
    public Muc[] muc = new Muc[0];

    void Update()
    {
        foreach (var m in muc)
        {
            if (m == null || !m.vat) continue;
            bool b = m.Bat;
            if (m.vat.activeSelf != b) m.vat.SetActive(b);
        }
    }
}
