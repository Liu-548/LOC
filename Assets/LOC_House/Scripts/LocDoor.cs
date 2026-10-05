using UnityEngine;

// Cánh cửa / cổng / cửa sổ mở được (phục vụ tìm kiếm & giải đố).
// Tư thế dựng sẵn = trạng thái đầu. goc = số độ quay quanh trục đứng đi qua bản lề để sang trạng thái còn lại (dấu như Quaternion.Euler(0, goc, 0)).
// Gọi Toggle() / Mo() / Dong() từ script game, hoặc nhìn vào cánh và nhấn E (LocFirstPerson).
public class LocDoor : MonoBehaviour
{
    public string ten = "cửa";
    public Vector3 banLe;            // vị trí bản lề, toạ độ cục bộ của cánh
    public float goc = 90f;
    public bool dungSanLaMo = true;  // tư thế dựng sẵn là MỞ?
    public float thoiGian = 0.7f;    // giây
    public bool khoa;                // script game đặt true để khoá (Toggle không làm gì)
    public float gapX;               // > 0: cửa xếp/kéo — thay vì quay, thu chiều ngang (scale X) về tỉ lệ này quanh gốc
    public Vector3 truot;            // ≠ 0: ngăn kéo / kính lùa — tịnh tiến (toạ độ cục bộ của cha) thay vì quay
    public LocDoor[] cungCua;        // các cánh cùng ô cửa (cửa sổ 2 cánh)
    public GameObject[] anKhiMo;     // vật chắn (ô kính trà…) chỉ hiện khi tất cả cánh đều đóng

    Vector3 p0, hinge; Quaternion r0; float t, dich;

    Vector3 s0, lp0;
    void Awake() { lp0 = transform.localPosition; p0 = transform.position; r0 = transform.rotation; s0 = transform.localScale; hinge = transform.TransformPoint(banLe); CapNhatChan(); }

    public bool DangMo => (dich < 0.5f) == dungSanLaMo;
    public void Toggle() { if (khoa) return; dich = dich < 0.5f ? 1f : 0f; CapNhatChan(); }
    void CapNhatChan()
    {
        if (anKhiMo == null || anKhiMo.Length == 0) return;
        bool mo = DangMo;
        if (cungCua != null) foreach (var c in cungCua) if (c && c.DangMo) mo = true;
        foreach (var g in anKhiMo) if (g) g.SetActive(!mo);
    }
    public void Mo() { if (!DangMo) Toggle(); }
    public void Dong() { if (DangMo) Toggle(); }

    void Update()
    {
        if (Mathf.Approximately(t, dich)) return;
        t = Mathf.MoveTowards(t, dich, Time.deltaTime / Mathf.Max(0.05f, thoiGian));
        float e = Mathf.SmoothStep(0f, 1f, t);
        if (truot != Vector3.zero) { transform.localPosition = lp0 + truot * e; return; }
        if (gapX > 0f) { transform.localScale = new Vector3(Mathf.Lerp(s0.x, s0.x * gapX, e), s0.y, s0.z); return; }
        var q = Quaternion.AngleAxis(goc * e, Vector3.up);
        transform.SetPositionAndRotation(hinge + q * (p0 - hinge), q * r0);
    }
}
